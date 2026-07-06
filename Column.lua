-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

-- require('lldebugger').start()

local Date = "2025/07/23"

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")
---@type JournalEntry
local JournalEntry = luanet.import_type("Bruker.Lc.JournalEntry")
---@type LogTo
local LogTo = luanet.import_type("Bruker.Lc.Business.LogTo")

local baltic = require "baltic"
require "degas"

---@param context InitHelper
function Initialize (context)
	context.Name = "Column"
	context.Description = "Prepare or diagnose the columns"
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Diagnostics

	context:DeclareParameter("Detach separation column", true, nil, "check", "Pressure reduction to safely detach the column", "")
	context:DeclareParameter("Diagnose separation column", false, nil, "check", "Check for a leakage and blockage", "")
	context:DeclareParameter("Diagnose trap column", false, nil, "check", "Check for a leakage and blockage", "")
	context:DeclareParameter("Prepare separation column for storage", false, nil, "check", "Using solvent from item position 1", "ItemPositions.PNG")
--	context:DeclareParameter("Equilibrate separation column", false, nil, "check", "Separation column equilibration", "")
--	context:DeclareParameter("Equilibrate trap column", false, nil, "check", "Trap column equilibration", "")

	context:DeclareParameter("trap", nil, nil, "custom")
	context:DeclareParameter("separator", nil, nil, "custom")
end

---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (_, context)
-- This function is called when the method is uploaded (Upload method or Acquisition start)
-- All errors are surpressed at the moment
	local validation = require "validation"
	validation.verify_specified(context, "Prepare separation column for storage")
	validation.verify_specified(context, "Detach separation column")
--	validation.verify_specified(context, "Equilibrate separation column")
--	validation.verify_specified(context, "Equilibrate trap column")
	validation.verify_specified(context, "Diagnose separation column")
	validation.verify_specified(context, "Diagnose trap column")
	validation.verify_specified(context, "trap")
	validation.verify_specified(context, "separator")
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
	local chrom = require "chromatography"
	---@type Zirconium
	local zr = require "zirconium"
	local pp = require "palplus"
	local pf = require "pump_functions"
	local parallel = require "parallel"
	local pr = require "PreRunFunctions"
	local ci = require "column_information"
	---@type IJournal
	local journal = context:GetProcedureParticipant(baltic.JournalRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local injector = pp.QueryModule(execLeft, pp.Capabilities.IInjector)
	local wash_module = pp.QueryModule(execLeft, pp.Capabilities.ILcMsWashStation)
	local valveI = pp.QueryValveDrive(execLeft, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execLeft, pp.Capabilities.ISelectorValve)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Detach separation column", false)) -- will be overwritten with true if actually executed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Diagnose separation column", false)) -- will be overwritten with true if actually executed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Diagnose trap column", false)) -- will be overwritten with true if actually executed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Prepare separation column for storage", false)) -- will be overwritten with true if actually executed

	---@type IChromatographyColumnType
	local trap = context:GetArgumentValue("trap")

	-- read the current column oven temperature
	local ovenTemp = pump:GetCurrentExternalTemperature()
	context:Log("--- Oven temperature: {0}", ovenTemp)
	if (ovenTemp < 1) then
		ovenTemp = 20
		context:Log("--- internal variable ovenTemp: {0}", ovenTemp)
	end

	local function sleep_100()
		context:Sleep(100)
	end

	local function sleep_250()
		context:Sleep(250)
	end

	local function sleep_1000()
		context:Sleep(1000)
	end

	---Function to set one or more valves
	---@param a_angle number|nil
	---@param b_angle number|nil
	---@param i_angle number|nil
	---@param t_angle number|nil
	local function set_valves(a_angle, b_angle, i_angle, t_angle) -- concurrently sets (optionally) all 4 valves.
		while not execLeft.IsIdle do
			sleep_100()
		end
		if (i_angle) then
			pr.SetValvePosition(execLeft, valveI, i_angle)
		end
		if (t_angle) then
			pr.SetValvePosition(execLeft, valveT, t_angle)
		end
		local tasks = {}
		if (a_angle) then
			tasks[#tasks+1] = { zr.SetValvePosition, context, pump, zr.A, a_angle, nil }
		end
		if (b_angle) then
			tasks[#tasks+1] = { zr.SetValvePosition, context, pump, zr.B, b_angle, nil }
		end
		parallel.run(sleep_100, table.unpack(tasks))
		while (not execLeft.IsIdle) do sleep_100() end
	end

	---Function to build the desired pressure on one channel
	---@param channel Channel
	---@param pressure number
	---@param yield_function function
	local function build_pressure(channel, pressure, yield_function)
		if zr.IsEmpty(pump, channel) then return end
		context:Log("build_pressure {0}", channel)

		pf.Manualmode_Pump_constantPressure(channel, pressure, pump, yield_function)
		local timeOut = 30 + pf.now()
		while true do
			if zr.IsEmpty(pump, channel) then
				-- TODO report leakage here?
				break
			end
			if (pf.now() > timeOut) then break end
			if (pump:GetCurrentPressure(channel) > pressure*.99) then break end
			yield_function()
		end
	end

	---Function to build the desired pressure on channel A
	---@param pressure number
	local function buildPressure_A(pressure)
		build_pressure(zr.A, pressure, sleep_1000)
	end

	---Function to build the desired pressure on channel B
	---@param pressure number
	local function buildPressure_B(pressure)
		build_pressure(zr.B, pressure, sleep_1000) --use 1000 as in buildPressure_B in 1097
	end

	---Function to build a constant flow
	---@param channel Channel
	---@param flow number
	---@param yield_function function
	local function buildConstantFlow(channel, flow, yield_function)
		if zr.IsEmpty(pump, channel) then return end
		context:Log("build_flow {0}", channel)

		pf.Manualmode_Pump_constantFlow(channel,flow, pump, yield_function)
		local timeOut = 30 + pf.now()
		while true do
			if zr.IsEmpty(pump, channel) then
				-- TODO report leakage here?
				break
			end
			if (pf.now() > timeOut) then break end
			if (pump:GetCurrentFlow(channel) > flow*.99) then break end
			yield_function()
		end
	end

	---commenFunction to build a constant flow on channel At
	---@param flow number
	local function buildConstantFlow_A(flow)
		buildConstantFlow(zr.A, flow, sleep_1000)
	end

	---Function to build a constant flow on channel B
	---@param flow number
	local function buildConstantFlow_B(flow)
		buildConstantFlow(zr.B, flow, sleep_1000)
	end

	---Queueing the cleaning aktivities
	local function queue_autosampler_wash()
		--clean injector with loop and syringe
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)
		context:Sleep(1000)
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Red, execLeft, baltic.InjectionValve.Inject)
		pp.CleanInjector(execLeft, execAux, pp.Organic, 25, true, false)		-- IdexFix05
		while not execLeft.IsIdle do sleep_1000() end
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Blue, execLeft, nil)
		pp.CleanInjector(execLeft, execAux, pp.Aqueous, 500, false, true)		-- IdexFix05
		while not execLeft.IsIdle do sleep_1000() end
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Normal, execLeft, nil)
		pp.PrimeLCPToolLoop(execLeft, true, true)
	end

	---Queueing the cleaning aktivities
	---@param yieldFunction function
	local function queue_autosampler_cleanInjector(yieldFunction) -- only called in prepare_autosampler
		--clean injector only w/o loop
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Red, execLeft, nil)
		pp.CleanInjector(execLeft, execAux, pp.Organic, 25, true, false)
		while not execLeft.IsIdle do yieldFunction() end
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Blue, execLeft, nil)
		pp.CleanInjector(execLeft, execAux, pp.Aqueous, 500, false, true)
		while not execLeft.IsIdle do yieldFunction() end
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Normal, execLeft, nil)
	end

	---Calculating the trap pressure
	---@param column IChromatographyColumnType
	---@return number
	---@return number
	local function TrapEqLoad_pressure(column)
		local flow = chrom.TrapTragetFlow(column)
		local colPressure = chrom.column_pressure(column, pressSettings.GradientPumpMaxTargetPressure, flow, chrom.viscosity_H2O_20C)
		local equiPressure = chrom.GetEquilibrationSystemPressure(installed, flow)
		local pressure = colPressure + equiPressure
		pressure = math.min(pressSettings.GradientPumpMaxTargetPressure, pressure)		-- PBNE-666
		if column.IsMaximumPressure then
			if pressure > column.MaximumPressure then
				pressure = math.min(pressure, column.MaximumPressure)
				flow = pressure * baltic.unityFlowEqLoadTrap
			end
		end
		return pressure, flow
	end

	---Prepare the separation column for storage
	---@param pumpMaxPressure integer [bar]
	local function prepForStorage(pumpMaxPressure)
		---@type IChromatographyColumnType
		local sep = context:GetArgumentValue("separator")
		local tool = pp.QueryModule(execAux, pp.Capabilities.IToolLc)
		local speed = pp.Quantity("5 uL/s")
		local delay = pp.Quantity("2000 ms")
		local detergent_depth = pp.Quantity(baltic.WashSolventLinerPenetrationDepth, "mm")
		local depth = pp.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
		local itemPosition = pp.QueryModules(execAux, "ItemPositionDescription")
		local prepSolvent = itemPosition[pr.GetItemPosIdx(1, execAux)]			-- get preparation solvent from item position 1 (near the instrument)
		local _, loopVolume = chrom.getCapillaryIDandLength(installed, baltic.Naming.Loop)		-- loopID is unused
		local pMax = math.min(pf.getColumnMaxPressure(sep, pumpMaxPressure), baltic.Settings.GradientPumpDefColumnEquilPressure)

		-- Max pressure is set at beginning of any column procedure
--		pf.SetMaxPressureLimit(zr.A, pMax+20, pump, sleep_100)
--		pf.SetMaxPressureLimit(zr.B, pMax+20, pump, sleep_100)
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)

		pr.Signalize_Reset(context)

		execLeft:ChangeTool( tool )
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
		pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

		context:Report("Prepare for storage", Severity.Info, true, "Place a vial with storage liquid in position 1 (rear position) of the wash module")
		-- empty syringe into waste so there's room for new solvent
		pp.EmptySyringe_And_LeaveObject(context, execLeft, execAux, depth, installed.SyringeZeroPosition)

		context:Log("Cleaning LCP-Tool")
		pp.PrimeLCPToolLoop(execLeft, true, false)
		execLeft:LeaveObject()

		context:WaitForSignal("Prepare for storage")
		-- get Rear Airgap
		execLeft:AspirateSyringe( pp.Quantity(0.5,"uL"), speed, nil, delay)
		-- get solvent
		execLeft:MoveToObject( prepSolvent,1, true, true, true )
		execLeft:PenetrateWithBottomSense( prepSolvent, 1, pp.Quantity(1, "mm"), nil, nil)
		execLeft:AspirateSyringe( pp.Quantity(loopVolume+4.5, "uL"), speed, nil, delay)
		execLeft:LeaveObject()
		-- get Front Airgap
		execLeft:AspirateSyringe( pp.Quantity(0.5,"uL"), speed, nil, delay)
		-- dip needle exterior wash module Solvent A
		execLeft:MoveToObject( wash_module, pp.Aqueous, true, true, true )
		execLeft:PenetrateObject( wash_module, pp.Aqueous, detergent_depth, pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
		execLeft:Wait(pp.Quantity(5,"s"))
		execLeft:LeaveObject()

		-- Inject solvent
		execLeft:MoveToObject(injector)
		execLeft:PenetrateWithConstForce( injector )
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)
		while not execLeft.IsIdle do  -- wait for solvent injection to finish
			sleep_100()
		end
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Purple, execLeft, baltic.InjectionValve.Inject)
		execLeft:Wait(pp.Quantity("1000 ms")) --wait
		execLeft:DispenseSyringe( pp.Quantity(loopVolume, "uL"), speed) -- inject solvent into loop
		execLeft:Wait(pp.Quantity("1000 ms")) --wait
		while not execLeft.IsIdle do  -- wait for solvent injection to finish
			sleep_100()
		end
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Normal, execLeft, nil)
		-- Prepare to push out solvent 
		set_valves(baltic.PumpValve.Inject, nil, nil, baltic.TrapValve.Analytical)
		context:Sleep(250)
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)
		while not execLeft.IsIdle do  -- wait for solvent injection to finish
			sleep_100()
		end

		execLeft:EmptySyringe(pp.Quantity("5 uL/s"))
		local p_cleanInjector = { queue_autosampler_cleanInjector, parallel.yield }
		local p_buildPressure_A = { build_pressure, zr.A, pMax, parallel.yield }
		parallel.run(sleep_100, p_cleanInjector, p_buildPressure_A)
		while not execLeft.IsIdle do  -- wait for solvent injection to finish
			sleep_100()
		end

		local prepVolume = math.min(0.75*loopVolume, 10*chrom.column_volume(sep))
		local startPistonPos = pump:GetPistonPosition(zr.A)
		local prevPistPos = startPistonPos
		local cnt = 0
		context:Log("Preparation volume: {0} ul", prepVolume)

		repeat
			cnt = cnt + 1
			context:Sleep(10*1000)
			local actPistPos = pump:GetPistonPosition(zr.A)
			local flow = (actPistPos - prevPistPos)/(10/60)
			prevPistPos = actPistPos
			local volume = actPistPos-startPistonPos
			context:Log("Pump A: dispensed volume {0} ul; current flow {1} ul/min", volume, flow)
		until (volume >= prepVolume)

		pf.reducePressure(context, pump, zr, zr.A, nil, 10, 10, 30, sleep_100, baltic.smooth)
	end

	---Equilibrate the trap column
	---This is done via the injection path
	---The loop is filled with organic and then pushed thru the trap column to waste
	local function equilibrate_trap_column()
		local equiVolume			= 50 * chrom.column_volume(trap) + 30 -- combined solvent volume
		local sleep					= 5		-- seconds
		local pressure, flow = TrapEqLoad_pressure(trap)

		pf.SetMaxPressureLimit(zr.A, pressSettings.GradientPumpCutoffPressure, pump, sleep_100)
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_6_1)

		context:Log(baltic.devider)
		context:Log("--- Trap column preparation running ...")
		context:Log("--- Preparation volume [uL]:   			{0}", equiVolume)
		context:Log("--- Preparation target pressure [bar]: 	{0}", pressure)
		context:Log("--- Preparation flow [uL/min]: 			{0}", flow)

		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)
		context:Sleep(1000)
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Red, execLeft, baltic.InjectionValve.Inject)
		-- fill loop using solvent 2
		pp.CleanInjector(execLeft, execAux, pp.Organic, 25, true, true)
		while not execLeft.IsIdle do
			sleep_100()
		end
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Normal, execLeft, nil)
		set_valves(baltic.PumpValve.Inject, nil, baltic.InjectionValve.Load, baltic.TrapValve.Trap)
		context:Sleep(250)
		pp.PrimeLCPToolLoop(execLeft, true, true)

		buildPressure_A(pressure)

		local startPistonPos = pump:GetPistonPosition(zr.A)

		repeat
			context:Sleep(sleep*1000)
			local actPistPos = pump:GetPistonPosition(zr.A)
			local volume = actPistPos-startPistonPos
--			context:Log("Pump A: dispensed volume {0} ul; current flow {1} ul/min", volume, flow)
		until (volume >= equiVolume)

		while not execLeft.IsIdle do
			sleep_100()
		end

		-- wait for pump to stop completely
		pf.isPumpIdle(pump, sleep_100)
		context:Log("Trap column equilibration finished")
		context:Log(baltic.devider)
	end

	---Equilibrate the separation column
	---This is done via the mixTee
	---First pump B is pumping organic thru the separation caolumn
	---Second pump A is pumping acqueous thru the separation column
	local function equilibrate_separation_column()
		---@type IChromatographyColumnType
		local sep 			= context:GetArgumentValue("separator")
		local columnVolume	= chrom.column_volume(sep)
		local equiVolume	= columnVolume*5
		local sleep			= 5      -- seconds
		local minute		= 60     -- = 1 minute
		local defEquilPress = baltic.Settings.GradientPumpDefColumnEquilPressure
		local pMax = math.min(pf.getColumnMaxPressure(sep, pressSettings.GradientPumpCutoffPressure), defEquilPress)
		local column_flow = chrom.column_flow(sep, pressSettings.GradientPumpMaxTargetPressure, pMax, chrom.viscosity_mix(ovenTemp, 1))

		local limit = pf.GetMaxPressureLimitWithDelta(installed.MaxPumpPressure, pMax)
		pf.SetMaxPressureLimit(zr.A, limit, pump, sleep_100)
		pf.SetMaxPressureLimit(zr.B, limit, pump, sleep_100)
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)
		-- if the equilibration flow is lower than this, expect clogged column

		context:Log(baltic.devider)
		context:Log("Separation column equilibration running ...")
		context:Log("Equilibration volume [uL]: {0}", equiVolume)
		context:Log("Equilibration target pressure [bar]: {0}", pMax)
		context:Log("Equilibration flow [uL/min]: {0}", column_flow)
		context:Log(baltic.devider)

		local function accumulate(channel)
			local av_uF = 0			-- unityFlow
			local av_cP = 0			-- currentPressure
			local counter = 0
			local volume = 0
			local currentFlow = 0
			local timeout = pf.now() + 300		-- timeout = 5 minutes 
			while (volume < equiVolume) and (pf.now() < timeout) do
				context:Sleep(sleep*1000)
				currentFlow = pump:GetCurrentFlow(channel)
				counter = counter + 1
				av_uF = av_uF + currentFlow
				av_cP = av_cP + pump:GetCurrentPressure(channel)
				volume = volume + (currentFlow*sleep/minute)
--				context:Log("Volume pump {1}: {0} [uL]", volume, channel)
			end
			if counter > 0 then
				context:Log("Volume pump {1}: {0} [uL]", volume, channel)
				av_cP = av_cP/counter
				av_uF = av_uF/counter/av_cP
			end
			return av_uF
		end

		-- with valve A to compress: flow B goes thru flowsensor A
		set_valves(baltic.PumpValve.Waste, baltic.PumpValve.MixTee, nil, baltic.TrapValve.GradientA)
		if sep.IsMaximumFlow then
			local maxFlowSepB = pf.getSepColumnFlow(nil, sep, pressSettings.GradientPumpMaxTargetPressure, sep.MaximumFlow, 1, ovenTemp)
			context:Log("Preparation target flow B [\181L/min]: {0}", maxFlowSepB)
			buildConstantFlow_B(maxFlowSepB)
		else
			buildPressure_B(pMax)
		end

		accumulate(zr.B)
		pf.reducePressure(context, pump, zr, nil, zr.B, 5, 5, 60, sleep_100, baltic.smooth)
		set_valves(baltic.PumpValve.Waste, baltic.PumpValve.MixTee, nil, nil)
		if sep.IsMaximumFlow then
			local maxFlowSepA = pf.getSepColumnFlow(nil, sep, pressSettings.GradientPumpMaxTargetPressure, sep.MaximumFlow, 0, ovenTemp)
			context:Log("Preparation target flow A [\181L/min]: {0}", maxFlowSepA)
			buildConstantFlow_A(maxFlowSepA)
		else
			buildPressure_A(pMax)
		end

		local uF = accumulate(zr.A)
		context:Log("Separation Column system unity flow [uL/min/bar]: {0}", uF)

		pf.reducePressure(context, pump, zr, zr.A, nil, 5, 5, 60, sleep_100, baltic.smooth)
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste)

		-- wait for pump to stop completely
		pf.isPumpIdle(pump, sleep_100)
		context:Log("Separation column equilibration finished")
	end

	-- required
	context:Log("Column Lua date: {0}", Date)

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)

	--	Signalize_Reset
	pr.Signalize_Reset(context)
	context:ShowComposition(false)

	local fR = require "PreRunFunctions"
	fR.iniFlowResistance(context, pump)

	-- init zirconium channels, abort if unsuccessful
	if not zr.InitChannel(context, pump, zr.A) then
		context:Report(baltic.Naming.PumpA, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end
	if not zr.InitChannel(context, pump, zr.B) then
		context:Report(baltic.Naming.PumpB, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end

	if (trap == nil) or (context:GetArgumentValue("separator") == nil) then
		local msg = "Please select a column"
		context:Report("Missing column", Severity.Info, true, msg)
		context:Abort()
	else
		ci.logColumnInformation(context, trap, context:GetArgumentValue("separator"))

		local caller = "Column"
		local settings = pump:GetSettings()
		zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)
		zr.logInstrSettings(context, settings, caller)

		local dictator = LoggingDictator.Prevent(pump)
		if installed.IsExtendedLoggingEnabled then dictator:Dispose() end

		pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure,false, false)		-- set max pump pressure

		if (context:GetArgumentValue("Detach separation column") == false) then
			-- degassing should not be done if just the column shall be detached
			local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
			-- bail if a pump failed degassing..
			if (not (a and b)) then
				pr.decompressSystem(context)
				context:Abort()
			end
		end

		-- PBNE-596: reduce to 25 bar instead of to 5 bar
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 25, 25, 300, sleep_100, baltic.smooth)		

		if (context:GetArgumentValue("Prepare separation column for storage")) then
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Prepare separation column for storage", true))
			local msg = "Preparing column for storage"
			status:SetStatus(msg)
			context:Log(baltic.devider)
			context:Log("--- Preparing separation column for storage:")
			prepForStorage(pressSettings.GradientPumpMaxTargetPressure)
			context:Report("Separation column", Severity.Info, true, "Column is now ready for storage")
			context:Sleep(1000)
			queue_autosampler_wash()
			context:Log(baltic.devider)
			while not execLeft.IsIdle do
				sleep_100()
			end
			context:WaitForSignal("Separation column")
			status:RemoveStatus(msg)
		else
			if (context:GetArgumentValue("Detach separation column")) then
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Detach separation column", true))
				-- Only if pump valves are switched to MixTee
				if (pump:GetSetManualValvePosition(zr.A) == baltic.PumpValve.MixTee) and (pump:GetSetManualValvePosition(zr.B) == baltic.PumpValve.MixTee) then 
					pf.Manualmode_Pump_constantFlow_binary(context, 0, 0, pump, zr, sleep_100)							-- keep flow at zero to prevent back flow from B to A
				end
				context:Report("Separation column", Severity.Info, true, "Column can now be detached")
				context:WaitForSignal("Separation column")
				pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
				pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
			else
				local diagTrap = nil
				local diagSep = context:GetArgumentValue("Diagnose separation column")
				diagTrap = context:GetArgumentValue("Diagnose trap column")
				if (diagTrap or diagSep) then
					local testOK = true

					local function getFlow(sep, TestPressure)
						local avFlow = 0

						local function averageFlow()
							local av_F = 0			-- average flow
							local counter = 0
							while (counter < 60) do
								context:Sleep(1000)
								counter = counter + 1
								av_F = av_F + pump:GetCurrentFlow(zr.A)
							end
							if counter > 0 then
								av_F = av_F/counter
							end
							return av_F
						end

						if sep then
							set_valves(baltic.PumpValve.MixTee, baltic.PumpValve.Waste, nil, baltic.TrapValve.GradientA)
							context:Sleep(250)
						else
							context:Report("Trap Diagnose", Severity.Info, true, "Please disconnect the separation column, then click 'Confirm' to continue.")
							context:WaitForSignal("Trap Diagnose")
							set_valves(baltic.PumpValve.MixTee, baltic.PumpValve.Waste, nil, baltic.TrapValve.GradientT)
							context:Sleep(250)
							context:Signalize(baltic.ColorsRGB.LightGray, baltic.Naming.Separator, baltic.Naming.Separator)
						end
						build_pressure(zr.A, TestPressure, sleep_250)
						context:Sleep(60*1000)
						avFlow = averageFlow()
						return avFlow
					end

					if diagSep then
						journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Diagnose separation column", true))
						status:SetStatus("Diagnose separation column")
						context:Log(baltic.devider)
						context:Log("--- Diagnose separation column:")
						context:ShowComposition(false)

						zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)

						local N = baltic.Naming
						---@type IChromatographyColumnType
						local sep = context:GetArgumentValue("separator")
						local visco = chrom.viscosity_mix(ovenTemp, 0)
						local sepUnityFlow = chrom.column_flow(sep, pressSettings.GradientPumpMaxTargetPressure, 1, visco)
						local unityFlowVAtoFS = chrom.capillary_flow_byName(installed, N.ValveAToFS, 1, chrom.viscosity_H2O_20C)
						local unityFlowFStoMT = chrom.capillary_flow_byName(installed, N.FSAToMixTee, 1, chrom.viscosity_H2O_20C)
						local unityFlowMTtoVT = chrom.capillary_flow_byName(installed, N.MixTeeToTrapValve, 1, chrom.viscosity_H2O_20C)
						local unityFlowVTtoColumn = chrom.capillary_flow_byName(installed, N.TransferLine, 1, chrom.viscosity_H2O_20C)
						local unityFlow = 1/(1/unityFlowVAtoFS + 1/unityFlowFStoMT + 1/unityFlowMTtoVT + 1/unityFlowVTtoColumn + 1/sepUnityFlow)
						local testP = math.min(0.8*pf.getColumnMaxPressure(sep, pressSettings.GradientPumpMaxTargetPressure), 2/unityFlow)
						local expectedFlow = testP*unityFlow
						local f = getFlow(true, testP)
						context:Log("Test pressure: {0}, expected flow: {1}, actual flow {2}", testP, expectedFlow, f)
						context:Log("Separator unity flow calculated: {0}", sepUnityFlow)
						context:Log("Unity flow sum calculated: {0}", unityFlow)
						context:Log("Unity flow sum measured:   {0}", f/testP)
						if (f < expectedFlow*0.25) then		-- 25% of the expected flow
							local msg = "Flow is too low. Check column for blockage. Measured flow: "..pf.noExp(f,3).." expected flow: "..pf.noExp(expectedFlow,3)
							context:Report("Separation column Diagnose", Severity.Warn, true, msg)
							testOK = false
						elseif (f < expectedFlow*0.5) then		-- 50% of the expected flow
							local msg = "Flow is too low. Check column for blockage. Measured flow: "..pf.noExp(f,3).." expected flow: "..pf.noExp(expectedFlow,3)
							context:Report("Separation column Diagnose", Severity.Info, true, msg)
							testOK = false
						end
						if (f > expectedFlow*2) then	-- twice as high as the expected flow
							local msg = "Flow is too high. Check column connections for leakage. Measured flow: "..pf.noExp(f,3).." expected flow: "..pf.noExp(expectedFlow,3)
							context:Report("Separation column Diagnose", Severity.Warn, true, msg)
							testOK = false
						elseif (f > expectedFlow*1.5) then	-- 50% higher as the expected flow
							local msg = "Flow is too high. Check column connections for leakage. Measured flow: "..pf.noExp(f,3).." expected flow: "..pf.noExp(expectedFlow,3)
							context:Report("Separation column Diagnose", Severity.Info, true, msg)
							testOK = false
						end
						if testOK then context:Report("Separation column Diagnose", Severity.Info, true, "passed") end

						status:RemoveStatus("Diagnose separation column")
						context:Log(baltic.devider)
					end
					if diagTrap then
						journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Diagnose trap column", true))
						status:SetStatus("Diagnose trap column")
						context:Log(baltic.devider)
						context:Log("--- Diagnose trap column:")
						context:ShowComposition(false)

						zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_6_1)

						local N = baltic.Naming
						local flow = 2
						local pressureTrap = chrom.column_pressure(trap, pressSettings.GradientPumpMaxTargetPressure, flow, chrom.viscosity_H2O_20C) + chrom.capillary_pressure(installed, N.Trap, flow, chrom.viscosity_H2O_20C, false)
						local pressureVAtoFS = chrom.capillary_pressure(installed, N.ValveAToFS, flow, chrom.viscosity_H2O_20C, false)
						local pressureFStoMT = chrom.capillary_pressure(installed, N.FSAToMixTee, flow, chrom.viscosity_H2O_20C, true)
						local pressureMTtoVT = chrom.capillary_pressure(installed, N.MixTeeToTrapValve, flow, chrom.viscosity_H2O_20C, false)
						local pressureVTtoTrap = chrom.capillary_pressure(installed, N.Trap, flow, chrom.viscosity_H2O_20C, false)
						local pressureVTtoColumn = chrom.capillary_pressure(installed, N.TransferLine, flow, chrom.viscosity_H2O_20C, false)
						local TestPressureA = pressureTrap + pressureVAtoFS + pressureFStoMT + pressureMTtoVT + pressureVTtoTrap + pressureVTtoColumn
						local testP = math.min(0.8*pf.getColumnMaxPressure(trap, pressSettings.GradientPumpMaxTargetPressure), TestPressureA)
						local expectedFlow = testP/TestPressureA*flow
						local f = getFlow(false, testP)
						local unityFlowSum = chrom.GetUnityFlowTrap(installed)
						context:Log("Test pressure: {0}, expected flow: {1}, actual flow {2}", testP, expectedFlow, f)
						context:Log("Trap unity flow calculated: {0}", chrom.column_flow(trap, pressSettings.GradientPumpMaxTargetPressure, 1, chrom.viscosity_H2O_20C))
						context:Log("Unity flow sum calculated: {0}", unityFlowSum)
						context:Log("Unity flow sum measured:   {0}", f/testP)
						if (f < expectedFlow*0.25) then		-- 25% of the expected flow
							local msg = "Flow is too low. Check column for blockage. Measured flow: "..pf.noExp(f,3).." expected flow: "..pf.noExp(expectedFlow,3)
							context:Report("Trap column Diagnose", Severity.Warn, true, msg)
							testOK = false
						elseif (f < expectedFlow*0.5) then		-- 50% of the expected flow
							local msg = "Flow is too low. Check column for blockage. Measured flow: "..pf.noExp(f,3).." expected flow: "..pf.noExp(expectedFlow,3)
							context:Report("Trap column Diagnose", Severity.Warn, true, msg)
							testOK = false
						end
						if (f > expectedFlow*2) then	-- twice as high as the expected flow
							local msg = "Flow is too high. Check column connections for leakage. Measured flow: "..pf.noExp(f,3).." expected flow: "..pf.noExp(expectedFlow,3)
							context:Report("Trap column Diagnose", Severity.Warn, true, msg)
							testOK = false
						elseif (f > expectedFlow*1.5) then	-- 50% higher as the expected flow
							local msg = "Flow is too high. Check column connections for leakage. Measured flow: "..pf.noExp(f,3).." expected flow: "..pf.noExp(expectedFlow,3)
							context:Report("Separation column Diagnose", Severity.Info, true, msg)
							testOK = false
						end
						if testOK then
							context:Report("Trap column Diagnose ", Severity.Info, true, "passed")
							context:WaitForSignal("Trap column Diagnose ")
						end

						context:Report("Trap column Diagnose  ", Severity.Info, true, "Please reconnect the separation column to the open end of the transfer line.")		-- PBNE-642

						status:RemoveStatus("Diagnose trap column")
						context:Log(baltic.devider)
					end
				else
					-- old option where retrieving this value gives an error so make condition false by default
					if false and context:GetArgumentValue("Equilibrate separation column") then
						status:SetStatus("Equilibrate separation column")
						context:Log(baltic.devider)
						context:Log("--- Equilibrate separation column:")
						context:ShowComposition(true)
						equilibrate_separation_column()
						-- wait for queued autosampler wash to finish
						while not execLeft.IsIdle do
							sleep_100()
						end
						context:ShowComposition(false)
						status:RemoveStatus("Equilibrate separation column")
						context:Log(baltic.devider)
						context:Report("Separation column", Severity.Info, true, "Equilibration finished")

					end
					-- old option where retrieving this value gives an error so make condition false by default
					if false and context:GetArgumentValue("Equilibrate trap column") then
						status:SetStatus("Equilibrate trap column")
						context:Log(baltic.devider)
						context:Log("--- Equilibrate trap column:")
						equilibrate_trap_column()
						-- wait for queued autosampler wash to finish
						while not execLeft.IsIdle do
							sleep_100()
						end
						status:RemoveStatus("Equilibrate trap column")
						context:Log(baltic.devider)
						context:Report("Trap column", Severity.Info, true, "Equilibration finished")
					end
				end
			end
		end
		local i=0
		local n=0
		while not pump.IsIdle do
			sleep_100()
			i=i+1
			if i>10 then
				i=0
				n=n+1
				pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
				pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
				context:Log("Set pump speed to zero - attempt {0}",n)
			end
			if n>10 then
				context:Log("Set pump speed to zero failed after {0} attempts",n-1)
				break
	--			context:Abort()
			end
		end
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 30, sleep_100, baltic.smooth)

		context:ShowComposition(true)
		while not execLeft.IsIdle do
			sleep_100()
		end
		dictator:Dispose()

		zr.logValveABShiftCounterPosition(context, pump)
		zr.logPumpVolume(context, pump)
	end
end

