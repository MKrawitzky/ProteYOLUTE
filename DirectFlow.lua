-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

﻿-- require('lldebugger').start()

local Date = "2025/09/15"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type GradientContainer
local GradientContainer = luanet.import_type("Bruker.Lc.Baltic.GradientContainer")
---@type JournalEntry
local JournalEntry = luanet.import_type("Bruker.Lc.JournalEntry")
---@type LogTo
local LogTo = luanet.import_type("Bruker.Lc.Business.LogTo")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---This function is called at plug-in start and if direct flow is called
---@param context InitHelper
function Initialize (context)
    local baltic = require "baltic"

	context.Name = "Direct flow"
	context.Description = "Manual flow control"
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.IsApplyActive = true
	context.LedState = LedState.DirectFlow

	local flow = 0.3
	if baltic.MicroElute then flow = 3.0 end

	context:DeclareParameter("Constant flow", true, "", "radio", false, "Direct flow, flow controlled", "DirectFlow.PNG", 0, "A")
	context:DeclareParameter("Composition", 5, "%B", "integer", false, "Solvent B portion", "DirectFlow.PNG", 30)
	context:DeclareParameter("Flow rate", flow, "\181L/min", "decimal", false, "Constant flow rate", "DirectFlow.PNG", 30, "", 1)

	context:DeclareParameter("Separator0", "", nil, "separator", "", "")

	context:DeclareParameter("Constant pressure", false, "", "radio", false, "Direct flow, pressure controlled", "DirectFlow.PNG", 0, "A")
	context:DeclareParameter("Pressure", 30, "bar", "integer", false, "Constant pressure value", "DirectFlow.PNG", 30)
	context:DeclareParameter("Channel A+B", true, "", "radio", false, "Direct flow pressure controlled channel A+B", "DirectPressureAB.PNG", 50, "B")
	context:DeclareParameter("Channel A", false, "", "radio", false, "Direct flow pressure controlled channel A", "DirectPressureA.PNG", 50, "B")
	context:DeclareParameter("Channel B", false, "", "radio", false, "Direct flow pressure controlled channel B", "DirectPressureB.PNG", 50, "B")

	context:DeclareParameter("Separator1", "", nil, "separator", "", "")

	context:DeclareParameter("via Trap Column", false, nil, "check", "Direct flow via the trap column", "")

	context:DeclareParameter("trap", nil, nil, "custom")
	context:DeclareParameter("separator", nil, nil, "custom")
--  	context:DeclareParameter("Column oven connected", false, nil, "check")
end

---This function is never called?
---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (installed, context)
    local baltic 		= require "baltic"
	local pf			= require "pump_functions"
	local validation 	= require "validation"
	local maxFlow = baltic.maxFlow
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	validation.verify_specified(context, "Constant flow")

	if (context:GetArgumentValue("Constant flow") == true) then
		validation.verify_specified(context, "Flow rate")
		validation.verify_range(context, "Flow rate", 0.01, maxFlow)

		validation.verify_specified(context, "Composition")
		validation.verify_range(context, "Composition", 0, 100)
	end	

	validation.verify_specified(context, "Constant pressure")

	if (context:GetArgumentValue("Constant pressure") == true) then
		validation.verify_specified(context, "Pressure")
		validation.verify_range(context, "Pressure", 0, pressSettings.GradientPumpMaxTargetPressure)
	end	

	validation.verify_specified(context, "trap")
	validation.verify_specified(context, "separator")
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
    require "degas"

	local baltic = require "baltic"
	local ci = require "column_information"
	local chrom = require "chromatography"
    local pf = require "pump_functions"
	local pr = require "PreRunFunctions"
	local ssf = require "strategySplineFit"

	---@type Zirconium
    local zr = require "zirconium"
    local pp = require "palplus"

	---@type IApplySettingsContext | nil
	local isApplied = nil
	local code = 0
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
    local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	---@type IProcedureStatusParticipant
    local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end

	-- ProteYOLUTE intelligent monitoring
	local core = require("proteyolute_core")
	local session = core.beginProcedure(context, installed, pump, zr)

	---@type IJournal
	local journal = context:GetProcedureParticipant(baltic.JournalRole)
	---@type IChromatographyColumnType
	local sep = context:GetArgumentValue("separator")
	---@type IChromatographyColumnType
    local trap = context:GetArgumentValue("trap")
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)

	--	Signalize_Reset
	pr.Signalize_Reset(context)
	context:ShowComposition(true)

	context:Log("Lua date:			{0}", Date)
	context:Log("--- Experiment:    {0}", context.Description)

	local FWVersion, fwOK = pr.isFWVersion_OK(execAux, baltic.minAutoSamplerFWVersion, baltic.maxAutoSamplerFWVersion)
	if fwOK == false then
		local errorMsg = DotNetString.Format("This plug-in is not compatible with firmware version: {0}", FWVersion)
		context:Report("Firmware version", Severity.Error, true, errorMsg)
		context:Abort()
	end
	context:Log("Autosampler firmware version: {0}", FWVersion)

    ci.logColumnInformation(context, trap, sep)
    context:Log("'{0}' is '{1}'", valveI.Name, pp.Capabilities.ILcInjectorValve)
    context:Log("'{0}' is '{1}'", valveT.Name, pp.Capabilities.ISelectorValve)

	if (trap == nil) or (sep == nil) then
		local msg = "Please select a column"
		context:Report("Missing column", Severity.Info, true, msg)
		context:Sleep(1000)
		context:Abort()
	else
		if not chrom.IsColumnParameterSet(context, sep) then
			context:Abort()
		end
		if not chrom.IsColumnParameterSet(context, trap) then
			context:Abort()
		end

		pr.iniFlowResistance(context, pump)

		local function sleep_100()
			context:Sleep(100)
		end
		local function sleep_1000()
			context:Sleep(1000)
		end

		local function journalDeleteEntries()
			journal:Delete("--- Direct Flow")
			journal:Delete("Flow rate")
			journal:Delete("Composition B")
			journal:Delete("--- Direct Flow A")
			journal:Delete("--- Direct Flow B")
			journal:Delete("--- Direct Flow A+B")
			journal:Delete("Constant pressure A")
			journal:Delete("Constant pressure B")
			journal:Delete("Constant pressure A+B")
		end

		---Set the valves and reduce the pressure if > 50bar
		---@param angleA number?
		---@param cwA boolean?
		---@param angleB number?
		---@param cwB boolean?
		---@param angleI number?
		---@param angleT number?
		local function setValves(angleA, cwA, angleB, cwB, angleI, angleT)
			local pressure = math.max(pump:GetCurrentPressure(zr.A), pump:GetCurrentPressure(zr.B))
			local actPosA = pump:GetSetManualValvePosition(zr.A)
			local actPosB = pump:GetSetManualValvePosition(zr.B)
			local actPosI = pp.GetInjectorValvePosition(execAux)
			local actPosT = pp.GetTrapValvePosition(execAux)
			if pressure > 50 then
				if (actPosA and angleA and actPosA ~= angleA) or
				   (actPosB and angleB and actPosB ~= angleB) or
				   (actPosI and angleI and actPosI ~= angleI) or
				   (actPosT and angleT and actPosT ~= angleT) then
					pf.reducePressure(context, pump, zr, zr.A, zr.B, 45, 45, 300, sleep_100, false)
				end
			end
			if (actPosA and angleA and actPosA ~= angleA) then zr.SetValvePosition(context, pump, zr.A, angleA, nil) end
			if (actPosB and angleB and actPosB ~= angleB) then zr.SetValvePosition(context, pump, zr.B, angleB, nil) end
			if (actPosI and angleI and actPosI ~= angleI) then
				pr.SetValvePosition(execAux, valveI, angleI)
			end
			if (actPosT and angleT and actPosT ~= angleT) then
				pr.SetValvePosition(execAux, valveT, angleT)
			end
		end

		local function constantFlow()
			local composition = context:GetArgumentValue("Composition")*0.01
			local flow_rate = context:GetArgumentValue("Flow rate")
			local isTrapUsed = context:GetArgumentValue("via Trap Column")
			context:SetApplyEnabled(false)

			journalDeleteEntries()

			---activate new parameters
			---@param setParameter IApplySettingsContext
			---@param gradient GradientContainer
			---@return GradientContainer
			local function setNewParameter(setParameter, gradient)
				composition = setParameter:GetArgumentValue("Composition")*0.01
				flow_rate = setParameter:GetArgumentValue("Flow rate")

				context:SetApplyEnabled(false)

				isTrapUsed = setParameter:GetArgumentValue("via Trap Column")
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Flow rate", flow_rate, "\181L/min", "N1"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Composition B", composition, "%", "N0"))

				gradient:Clear()
				if flow_rate ~= nil then
					gradient:AddSetPoint(GradientContainer.SetPoint(0, flow_rate, composition*100))
					gradient:AddSetPoint(GradientContainer.SetPoint(100, flow_rate, composition*100), true)
				end
				return gradient
			end

			--xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			-- create gradient for splineFit
			local gradient = GradientContainer()
			if flow_rate ~= nil then
				gradient:AddSetPoint(GradientContainer.SetPoint(0, flow_rate, composition*100))
				gradient:AddSetPoint(GradientContainer.SetPoint(100, flow_rate, composition*100), true)
			end
			--xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "--- Direct Flow", os.date("%c")))
			journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Flow rate", flow_rate, "\181L/min", "N1"))
			journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Composition B", composition, "%", "N0"))
			--	if context:GetArgumentValue("Column oven connected") == true then
			--		local actualOvenTemperature = pump:GetCurrentExternalTemperature()
			--		journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Oven Temperature", actualOvenTemperature, "degree", "N2"))
			--	end
			local maxPress = pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, isTrapUsed, true)		-- set max pump pressure

			local mode = "In Direct Flow"
			local chA = false -- context:GetArgumentValue("Flow Test Channel A")
			local chB = false -- context:GetArgumentValue("Flow Test Channel B")
			if (isApplied ~= nil) then
				gradient = setNewParameter(isApplied, gradient)
			end
			isApplied = nil
			while isApplied == nil do
				local msg = DotNetString.Format("Direct flow: {0:#0.00} \181L/min @ {1:P1}B solvent. Click 'Abort Direct flow' to stop.", flow_rate, composition)
				status:SetStatus(msg)
				local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
				-- bail if a pump failed degassing..
				if (not (a and b)) then
					journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Degassing Failed", "True", "", ""))
					pr.decompressSystem(context)
					context:Abort()
				end

				local flow_A = pump:GetCurrentFlow(zr.A)
				local flow_B = pump:GetCurrentFlow(zr.B)
				local valve_A = pump:GetSetManualValvePosition(zr.A)
				local valve_B = pump:GetSetManualValvePosition(zr.B)

				if (flow_A < -0.1 or flow_B < -0.1) or
				((valve_A ~= baltic.PumpValve.MixTee) or (valve_B ~= baltic.PumpValve.MixTee)) then
					pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_100, false)
				end
				if isTrapUsed then
					setValves(nil, nil, nil, nil, nil, baltic.TrapValve.GradientT)
				else
					setValves(nil, nil, nil, nil, nil, baltic.TrapValve.GradientA)
				end

				if chA == false and chB == false then
					setValves(baltic.PumpValve.MixTee,  nil, baltic.PumpValve.MixTee,  nil, nil, nil)
					context:Sleep(2500)
					local flowA = math.max(pump:GetCurrentFlow(zr.A),0)
					local flowB = math.max(pump:GetCurrentFlow(zr.B),0)

					ssf.spline_Gradient(context, gradient, pump, zr, pressSettings.GradientPumpMaxTargetPressure, pressSettings.GradientPumpCutoffPressure, flowA, flowB, nil, true, isTrapUsed)

					pf.iniFlowObserver(60)
					local showMsg1 = true
					local showMsg2 = true
					context:SetApplyEnabled(true)

					while (zr.IsEmpty(pump, zr.A) == false) and (zr.IsEmpty(pump, zr.B) == false) do
						context:Sleep(1000)
						code = pf.isGradientOK(context, pressSettings.GradientPumpCutoffPressure, 20, maxPress, pump, zr, code, mode)
						if code == 4 then break end
						if code == 1 and showMsg1 then
							context:LogMeta("DirectFlow", "Unstable Flow", "", "N", "True")
							showMsg1 = false
						end
						if code == 2 and showMsg2 then
							context:Report(mode, Severity.Info, true, "Pressure was within 5% to the maximum pressure. Check connections and column or reduce the flow.")
							context:LogMeta("DirectFlow", "Pressure within 5% Of Maximum", "bar", "N", math.max(pump:GetCurrentPressure(zr.A), pump:GetCurrentPressure(zr.B)))
							showMsg2 = false
						end
						isApplied = context:CheckForApply()
						if (isApplied ~= nil) then
							if (isApplied:GetArgumentValue("Constant flow") == true) then
								gradient = setNewParameter(isApplied, gradient)
							end
							break
						end
					end
					context:Log("PumpA piston position: {0}", pump:GetPistonPosition(zr.A))
					context:Log("PumpB piston position: {0}", pump:GetPistonPosition(zr.B))
					context:Log("--- Starting refill of both pumps ---")
					status:RemoveStatus(msg)
					if code == 4 then break end
				else
					if chA == true then
						setValves(baltic.PumpValve.MixTee, nil, nil, nil, nil, nil)
					else
						setValves(baltic.PumpValve.Waste, nil, nil, nil, nil, nil)
					end
					if chB == true then
						setValves(nil, nil, baltic.PumpValve.MixTee, nil, nil, nil)
					else
						setValves(nil, nil, baltic.PumpValve.Waste, nil, nil, nil)
					end
					context:Sleep(1000)
					if chA == true then pf.Manualmode_Pump_constantFlow(zr.A, (flow_rate * (1-composition)), pump, sleep_100) end
					if chB == true then pf.Manualmode_Pump_constantFlow(zr.B, (flow_rate * composition), pump, sleep_100) end
					local function isFlowReachedSetPoint(channel, setPoint)
						local isFlowReached = setPoint >= pump:GetCurrentFlow(channel)
						return isFlowReached
					end
					local flowNotReached = true
					local cnt = 0
					while (flowNotReached == true) do
						if chA == true then flowNotReached = isFlowReachedSetPoint(zr.A, pf.noExp(flow_rate * (1-composition))) end
						if chB == true then flowNotReached = isFlowReachedSetPoint(zr.B, pf.noExp(flow_rate * composition)) end
						if cnt >= 120 then
							flowNotReached = false
						end
						sleep_1000()
						cnt = cnt + 1
					end
					context:Sleep(15000)
					if chA == true then pf.Manualmode_Pump_constantSpeed(zr.A, (flow_rate * (1-composition)), pump, sleep_100) end
					if chB == true then pf.Manualmode_Pump_constantSpeed(zr.B, (flow_rate * composition), pump, sleep_100) end
					while true do
						sleep_100()
						isApplied = context:CheckForApply()
						if (isApplied ~= nil) then
							if (isApplied:GetArgumentValue("Constant flow") == true) then
								_ = setNewParameter(isApplied, gradient)
							end
							break
						end
					end
				end
			end

			if (code ~= 0) then
				-- this is executed if code=4 (pressure exceeded maximum)
				pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_100, baltic.Smooth)
			end
			pf.showGradientMassage(context, code, mode)
		end

		local function constantPressure()
			local N = baltic.Naming
			local pressure = tonumber(context:GetArgumentValue("Pressure"))
			local channel = "A+B"
			local isTrapUsed = context:GetArgumentValue("via Trap Column")

			context:SetApplyEnabled(false)
			zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)
			pf.setMaxPressureLimitsAB(context, pump, pf.noExp(pressSettings.GradientPumpMaxTargetPressure,0), isTrapUsed, true)

			journalDeleteEntries()

			if context:GetArgumentValue("Channel A") == true then channel = "A" end
			if context:GetArgumentValue("Channel B") == true then channel = "B" end

			---activate new parameters
			---@param setParameter IApplySettingsContext
			local function setNewParameter(setParameter)
				context:SetApplyEnabled(false)
				local currentChannel = channel
				pressure = tonumber(setParameter:GetArgumentValue("Pressure"))
				channel = "A+B"
				if setParameter:GetArgumentValue("Channel A") == true then channel = "A" end
				if setParameter:GetArgumentValue("Channel B") == true then channel = "B" end
				isTrapUsed = setParameter:GetArgumentValue("via Trap Column")
				if currentChannel ~= channel then
					pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
					pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
				end
				local constPressuretext = "Constant pressure "..channel
				journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, constPressuretext, pressure, "bar", "N1"))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Channel A", context:GetArgumentValue("Channel A") or context:GetArgumentValue("Channel A+B")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Channel B", context:GetArgumentValue("Channel B") or context:GetArgumentValue("Channel A+B")))
			end

			local directFlowText = "--- Direct flow "..channel
			local constPressuretext = "Constant pressure "..channel
			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, directFlowText, os.date("%c")))
			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, constPressuretext, pressure, "bar", "N1"))
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Channel A", context:GetArgumentValue("Channel A") or context:GetArgumentValue("Channel A+B")))
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Channel B", context:GetArgumentValue("Channel B") or context:GetArgumentValue("Channel A+B")))

			if (isApplied ~= nil) then
				setNewParameter(isApplied)
			else
				if pressure ~= nil then
					pf.setMaxPressureLimitsAB(context, pump, pf.noExp(pressure,0), isTrapUsed, true)
				else
					context:Abort()
				end
			end
			isApplied = nil
			while isApplied == nil do
				local msg = DotNetString.Format("Constant pressure: {0:#0} bar. Click 'Abort Direct flow' to stop.", pressure)
				status:SetStatus(msg)
				local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
				-- bail if a pump failed degassing..
				if (not (a and b)) then
					journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Degassing Failed", "True", "", ""))
					pr.decompressSystem(context)
					context:Abort()
				end

	--			pf.reducePressure(context, pump, zr, zr.A, zr.B, 10, 10, 60, sleep_100, false)
				context:Sleep(1000)
				if isTrapUsed then
					setValves(nil, nil, nil,  nil, nil, baltic.TrapValve.GradientT)
				else
					setValves(nil, nil, nil,  nil, nil, baltic.TrapValve.GradientA)
				end
				context:Sleep(500)
				pr.Signalize_Reset(context)
				-- switch valves accordingly
				if channel == "A+B" then
					setValves(baltic.PumpValve.MixTee, false, baltic.PumpValve.MixTee, false, nil, nil)
					context:Sleep(1000)
					if pressure ~= nil then
						pf.Manualmode_Pump_constantPressure(zr.B, pressure, pump, sleep_100)
						pf.Manualmode_Pump_constantPressure(zr.A, pressure, pump, sleep_100)
					end
				else
					if channel == "A" then
						local blockValve = pump:GetSetManualValvePosition(zr.A) + 30
						if blockValve >= 300 then blockValve = 270 end
						setValves(baltic.PumpValve.MixTee, false, blockValve, true, nil, nil)
						context:Sleep(250)
						if isTrapUsed then
							context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.ValveAGroove, N.ValveAToFS, N.FlowA, N.FSAToMixTee, N.MixTeeToTrapValve, N.ValveTLongGroove, N.ValveTShortGroove, N.Trap, N.TransferLine, N.Separator)
						else
							context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.ValveAGroove, N.ValveAToFS, N.FlowA, N.FSAToMixTee, N.MixTeeToTrapValve, N.ValveTLongGroove, N.TransferLine, N.Separator)
						end
						context:SignalizeText(baltic.ColorsRGB.White, N.FlowA)
						if pressure ~= nil then pf.Manualmode_Pump_constantPressure(zr.A, pressure, pump, sleep_100) end
					else
						if channel == "B" then
							local blockValve = pump:GetSetManualValvePosition(zr.A) + 30
							if blockValve >= 300 then blockValve = 270 end
							setValves(blockValve, true, baltic.PumpValve.MixTee, false, nil, nil)
							context:Sleep(250)
							if isTrapUsed then
								context:Signalize(baltic.ColorsRGB.Red, N.PumpBFrontToValve, N.ValveBGroove, N.ValveBToFS, N.FlowB, N.FSBToMixTee, N.MixTeeToTrapValve, N.ValveTLongGroove, N.ValveTShortGroove, N.Trap, N.TransferLine, N.Separator)
							else
								context:Signalize(baltic.ColorsRGB.Red, N.PumpBFrontToValve, N.ValveBGroove, N.ValveBToFS, N.FlowB, N.FSBToMixTee, N.MixTeeToTrapValve, N.ValveTLongGroove, N.TransferLine, N.Separator)
							end
							context:SignalizeText(baltic.ColorsRGB.White, N.FlowB)
							if pressure ~= nil then pf.Manualmode_Pump_constantPressure(zr.B, pressure, pump, sleep_100) end
						end
					end
				end

				context:SetApplyEnabled(true)
				while (zr.IsEmpty(pump, zr.A) == false) and (zr.IsEmpty(pump, zr.B) == false) do
					context:Sleep(1000)
					isApplied = context:CheckForApply()
					if (isApplied ~= nil) then
						if (isApplied:GetArgumentValue("Constant pressure") == true) then
							setNewParameter(isApplied)
						end
						break
					end
				end
				context:Log("PumpA piston position: {0}", pump:GetPistonPosition(zr.A))
				context:Log("PumpB piston position: {0}", pump:GetPistonPosition(zr.B))
				context:Log("--- Starting refill of both pumps ---")
				status:RemoveStatus(msg)
			end
		end

		-- init zirconium channels, abort if unsuccessful
		if not zr.InitChannel(context, pump, zr.A) then
			context:Report(baltic.Naming.PumpA, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
			context:Abort()
		end
		if not zr.InitChannel(context, pump, zr.B) then
			context:Report(baltic.Naming.PumpB, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
			context:Abort()
		end

		-- Read the set column oven temperature
		-- The actual temperature may still differ from the desired temperature
		-- if the set temperature has just been changed.
		local ovenTemp = pump:GetSetExternalTemperature()
		context:Log("--- Oven temperature: {0}", ovenTemp)
		if (ovenTemp < 1) then
			ovenTemp = 20
			context:Log("--- internal variable ovenTemp: {0}", ovenTemp)
		end

		local settings = pump:GetSettings()
		zr.logInstrSettings(context, settings, "Direct flow")
		local unityFlowSystem = 0.008768		-- unity flow of capillary 18cm/10um
		local unityFlow = unityFlowSystem

	-- no check for nil needed because sep is always initialized
		if (sep.Name ~= "none") then
			unityFlow = 1/(1/unityFlowSystem+1/chrom.column_flow(sep, pressSettings.GradientPumpMaxTargetPressure, 1, chrom.viscosity_mix(ovenTemp, 0.2)))
		end

		context:Log("--- viscosityH2o @ {0}C: {1}", ovenTemp, chrom.viscosity_mix(ovenTemp, 0))
		context:Log("--- Separator Unity-Flow @ ovenTemp [ul/bar]:   {0}", unityFlow)

		while code == 0 do
			if isApplied ~= nil then
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Constant flow", isApplied:GetArgumentValue("Constant flow")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Constant pressure", isApplied:GetArgumentValue("Constant pressure")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "via Trap Column", isApplied:GetArgumentValue("via Trap Column")))

				if isApplied:GetArgumentValue("Constant flow") then
					constantFlow()
				elseif isApplied:GetArgumentValue("Constant pressure") then
					constantPressure()
				end
			else
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Constant flow", context:GetArgumentValue("Constant flow")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Constant pressure", context:GetArgumentValue("Constant pressure")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "via Trap Column", context:GetArgumentValue("via Trap Column")))

				if context:GetArgumentValue("Constant flow") then
					constantFlow()
				elseif context:GetArgumentValue("Constant pressure") then
					constantPressure()
				end
			end
		end

		-- wait for pump to stop
		pf.isPumpIdle(pump, sleep_100)

		dictator:Dispose()

		zr.logValveABShiftCounterPosition(context, pump)
		zr.logPumpVolume(context, pump)

		session:finish("completed")
	end
end