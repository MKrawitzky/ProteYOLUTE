-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/06/05"

local flowRate = 0.30	-- uL/min
local solventB = 80		-- %
local defOvenTemp = 35  -- degC
local maxFlow = 3.0
local toggleON = false
local ovenTempSetPt = 20

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type JournalEntry
local JournalEntry = luanet.import_type("Bruker.Lc.JournalEntry")
---@type LogTo
local LogTo = luanet.import_type("Bruker.Lc.Business.LogTo")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---@param context InitHelper
function Initialize (context)
	context.Name = "Idle flow"
	context.Description = "Procedure for keeping emitter wet"
	context.Hidden = true
	context.DecompressOnExit = true
	context.OverwriteLogFiles = true
	context.NumberOfLogFilesToKeep = 5
	context.LedState = LedState.Idle

	context:DeclareParameter("idle_flow_rate", flowRate, "\181L/min", "decimal") 
	context:DeclareParameter("composition", solventB, "%", "decimal")		
	context:DeclareParameter("max_flow", maxFlow, "\181L/min", "decimal") 
	context:DeclareParameter("via_trap", false)
	context:DeclareParameter("default_oven_temperature", defOvenTemp, "degC", "decimal")
	-- PBNE-657: Column oven is diconnected but this is ignored by the software
	context:DeclareParameter("is_use_oven_temperature", true)
	context:DeclareParameter("oven_temperature", ovenTempSetPt, "deg", "decimal")
	context:DeclareParameter("trap", nil, nil, "custom")
	context:DeclareParameter("separator", "none", nil, "custom")
end

--- this function is called when the Preferences dialog is launched, the column selection has changed, 
--- and also called when Preferences.xml is not present and needs to be created. Therefore, if someone changes
--- the separation column, this function can modify the idle flow rate and any other context parameter
---@param experiment ExperimentInfo
---@param installed IInstalledHardwareContext
---@param context AdjustmentContext
function GenerateMethod (experiment, installed, context)
-- This function is called when the generate button is pressed in the method editor
	local pf = require "pump_functions"
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	-- The experiment.OvenTemperature value that is passed in is 25
	ovenTempSetPt = experiment.OvenTemperature
	context:SetArgumentValue("idle_flow_rate", flowRate)

	-- only for connection to instrument and is not used in Main function
	local defaultOvenTemp = defOvenTemp
	context:SetArgumentValue("default_oven_temperature", defaultOvenTemp)
	context:SetArgumentValue("oven_temperature", experiment.OvenTemperature)
	context:SetArgumentValue("separator", experiment.Separator)
	local flowMax = 10
	context:SetArgumentValue("trap", experiment.Trap)
	flowMax = pf.getSepColumnFlow(experiment.Trap, experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, maxFlow, solventB*0.01, ovenTempSetPt)
	context:SetArgumentValue("max_flow", flowMax)
end

--- this function is called to validate the flow rate (and other parameters) in dialog entry in the Preferences dialog using 
--- the separation column data in the "experiment" parameter
---@param experiment ExperimentInfo
---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function ValidateMethod (experiment, _, context)
	-- validate the Preferences dialog entered flow_rate based upon experiment.Separator data. The standard red error box would 
	-- be shown if the entered parameter is out of range, just like the method editor
	local val = require "validation"

	val.verify_specified(context, "idle_flow_rate")
	val.verify_specified(context, "composition")
	val.verify_specified(context, "default_oven_temperature")
	val.verify_specified(context, "oven_temperature")
	val.verify_range(context, "idle_flow_rate", 0, maxFlow) 
	val.verify_range(context, "composition", 0, 100)
	val.verify_range(context, "default_oven_temperature", 0, experiment.Separator.MaximumTemperature)
	val.verify_range(context, "oven_temperature", 0, experiment.Separator.MaximumTemperature)
end

---@param _ IInstalledHardwareContext
---@param __ IProcedureValidationContext
function Validate (_, __)
-- no validation needed as the initialize values can not be changed by the user
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
	require "degas"

	local baltic = require "baltic"
	local chrom = require "chromatography"
	local ci = require "column_information"
	local pf = require "pump_functions"
	local pp = require "palplus"
	local pr = require "PreRunFunctions"
	local ssf = require "strategySplineFit"
	---@type Zirconium
	local zr = require "zirconium"
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IJournal
	local journal = context:GetProcedureParticipant(baltic.JournalRole)
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	local isTrapUsed = context:GetArgumentValue("via_trap")

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)

	--	Signalize_Reset
	pr.Signalize_Reset(context)
	context:ShowComposition(true)

	context:Log("Idle function called: {0}", os.date())
	context:Log("Lua date:             {0}", Date)
	---@type IChromatographyColumnType
	local trap = context:GetArgumentValue("trap")
	ci.logColumnInformation(context, trap, context:GetArgumentValue("separator"))

	-- check if column oven is intended to use and connected and temperature is set
	if not pr.IsOvenAndTemperatureOK(context, pump) then
		context:Report("Oven", Severity.Error, true, "Missing column oven. Temperature is set but no oven connected.")
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

	local function sleep_100()
		context:Sleep(100)
	end
	local function sleep_1000()
		context:Sleep(1000)
	end

	flowRate = context:GetArgumentValue("idle_flow_rate")
	local composition = context:GetArgumentValue("composition")*0.01
	-- Don't reduce pressure here (PNS-87)
	-- otherwise Fast 1C will not start at the desired pressure
--	pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_100, false)

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

	context:Log("Waiting 5 minutes before starting idle flow")
	context:Sleep(5*60*1000)
--	context:Log("Waiting 10 seconds before starting idle flow")
--	context:Sleep(10*1000)
	context:Log("--- Standby idle flow started ---")
	pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_100, false)
	local maxPress = pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, isTrapUsed, true)
	if not chrom.IsColumnParameterSet(context, context:GetArgumentValue("separator")) then
		context:Abort()
	end

	local fR = require "PreRunFunctions"
	fR.iniFlowResistance(context, pump)

	journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "--- Idle Flow", os.date("%c")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Last Idle Flow", os.date("%c")))
	journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Idle: Flow rate", flowRate, "\181L/min", "N3"))
	journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Idle: Composition B", composition*100, "%", "N2"))
	if context:GetArgumentValue("is_use_oven_temperature") == true then
		local actualOvenTemperature = pump:GetCurrentExternalTemperature()
		journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Column Oven Temperature", actualOvenTemperature, "degree", "N2"))
	end
--	local toggleON = context:GetArgumentValue("is_toggle_idleflow")

	-- ************* added flow_Rate & composition parameters 
	---Function to establish the desired flow
	---@param flow number
	---@param comp number
	---@param flowA number
	---@param flowB number
	local function establishFlow(flow, comp, flowA, flowB)
		local GradientContainer = luanet.import_type("Bruker.Lc.Baltic.GradientContainer")
		local gradient = GradientContainer()
		gradient:AddSetPoint(GradientContainer.SetPoint(0, flow, comp*100))
		gradient:AddSetPoint(GradientContainer.SetPoint(60, flow, comp*100), true)

		--  spline(context, gradient, pp, execAux, pump, zr, flowA, flowB, splineTime, setNewPID, useTrap)
		ssf.spline_Gradient(context, gradient, pump, zr, pressSettings.GradientPumpMaxTargetPressure, pressSettings.GradientPumpCutoffPressure, flowA, flowB, nil, false, isTrapUsed)
	end

	---Function to toggle the flow between A and B composition
	---@param flow number
	---@param comp number
	local function toggleBinaryFlow(flow, comp)
		local flowA = math.max(pump:GetCurrentFlow(zr.A),0)
		local flowB = math.max(pump:GetCurrentFlow(zr.B),0)
		if flowA > flowB then
			establishFlow(flow, comp, flowA, flowB)
		else
			establishFlow(flow, (1-comp), flowA, flowB)
		end
	end

	context:Log("'{0}' is '{1}'", valveI.Name, pp.Capabilities.ILcInjectorValve)
	context:Log("'{0}' is '{1}'", valveT.Name, pp.Capabilities.ISelectorValve)

	local mode = "In idle"
	local code = 0
	while true do
		if ((pump:GetPistonPosition(zr.A) > baltic.pumpLevel) or (pump:GetPistonPosition(zr.B) > baltic.pumpLevel)) then
			-- stop binary flow before degassing and refilling pumps
			pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
			local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
			-- bail if a pump failed degassing..
			if (not (a and b)) then
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Idle Flow Failed", "True"))
				pr.decompressSystem(context)
				context:Abort()
			end
		end
		if ((pump:GetSetManualValvePosition(zr.A) ~= baltic.PumpValve.MixTee) or (pump:GetSetManualValvePosition(zr.B) ~= baltic.PumpValve.MixTee)) then 
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_1000, baltic.smooth)
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee)
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee)
			context:Sleep(2000)
		end
		-- switch injection valve to 'Load' position
		-- not to get dirt into the loop , the loop must not remain in 'Inject' position for a long time
		-- also to prevent material flow of the rotor seal into the stator holes in Block position the position must not remain in 'Block' position
		pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Load)		-- PBNE-692
		if isTrapUsed then
			pr.SetValvePosition(execAux, valveT, baltic.TrapValve.GradientT)

		else
			pr.SetValvePosition(execAux, valveT, baltic.TrapValve.GradientA)

		end
		context:Sleep(5000)
		local flowA = math.max(pump:GetCurrentFlow(zr.A),0)
		local flowB = math.max(pump:GetCurrentFlow(zr.B),0)
		establishFlow(flowRate, composition, flowA, flowB)

		pf.iniFlowObserver(180)
		local duration = 90 + pf.now()			-- [s]
	    while (zr.IsEmpty(pump, zr.A) == false) and (zr.IsEmpty(pump, zr.B) == false) do
			if (pf.now() > duration) and toggleON then
				toggleBinaryFlow(flowRate, composition)
				pf.iniFlowObserver(60)
				duration = duration + 90	-- [s]
			end
			sleep_1000()
							-- (context, percent, mP, pump, zr, code, mode, buffer, numOfAvgValues=12)
			code = pf.isGradientOK(context, pressSettings.GradientPumpCutoffPressure, 20, maxPress, pump, zr, code, mode)
			if code == 4 then
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Idle: Pressure Higher Maximum",  math.max(pump:GetCurrentPressure(zr.A), pump:GetCurrentPressure(zr.B)), "bar", "N1"))
				break
			end		-- kill inner loop
		end
		context:Log("PumpA piston position: {0}", pump:GetPistonPosition(zr.A))
		context:Log("PumpB piston position: {0}", pump:GetPistonPosition(zr.B))
		context:Log("--- Starting refill of both pumps ---")
		if code == 4 then break end			-- kill outer loop
		dictator:Dispose()
	end

	-- this is executed if code=4 (pressure exceeded maximum)
    pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_1000, baltic.smooth)

	dictator:Dispose()

	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)
end
