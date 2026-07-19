-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

-- require('lldebugger').start()

local Date = "2025/11/17"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
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

local baltic = require "baltic"
require "degas"

---@param context InitHelper
function Initialize (context)
	context.Name = "Preparation"
	context.Description = "Preparing the LC system for running samples"
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Preparation

-- !!!DON'T declare advanced settings parameters here!!!

	context:DeclareParameter("Include solvent replacement", false, nil, "check", false, "Several times refilling pumps and cleaning syringe and capillaries", "")
	context:DeclareParameter("Include column preparation", false, nil, "check", false, "Trap and Separation column equilibration", "")

	context:DeclareParameter("Separator0", "", nil, "separator", "", "")

	context:DeclareParameter("Extended Wash", false, nil, "check")
	context:DeclareParameter("Do Not Include Trap Column", false, nil, "check", false, "", "", 30)
	context:DeclareParameter("Flow Organic", 10.0, "\181L/min", "decimal", false, "", "", 30, "", 1)
	context:DeclareParameter("Volume Organic", 10.0, "\181L", "decimal", false, "", "", 30, "", 1)
	context:DeclareParameter("Flow Aqueous", 10.0, "\181L/min", "decimal", false, "", "", 30, "", 1)
	context:DeclareParameter("Volume Aqueous", 40.0, "\181L", "decimal", false, "", "", 30, "", 1)
	--> Service parameter
	context:DeclareParameter("Disable Injector Cleaning", false, "", "check", true, "", "", 30)
	--< Service parameter

	context:DeclareParameter("Separator1", "", nil, "separator", "", "")

	context:DeclareParameter("Inject mass calibrant", false, nil, "check", false, "To be done for optimizing the MS", "")
	context:DeclareParameter("Injection time", 2, "min", "decimal", false, "Time of calibrant signal", "", 30, "", 2)
	context:DeclareParameter("Flow rate", 0.3, "\181L/min", "decimal", false, "Calibrant flow rate", "", 30, "", 1)

	context:DeclareParameter("separator", nil, nil, "custom")
	context:DeclareParameter("trap", nil, nil, "custom")
end

---This function is never called?
---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (_, context)
	local validation = 	require "validation"

	-- only require column types to be specified, if column prep is true (otherwise auto-prep may fail).
	if (context:GetArgumentValue("Include column preparation") or context:GetArgumentValue("Inject mass calibrant")) then
		validation.verify_specified(context, "trap")
		validation.verify_specified(context, "separator")
	end
	validation.verify_specified(context, "Include solvent replacement")
	validation.verify_specified(context, "Include column preparation")

	validation.verify_specified(context, "Extended Wash")

	if (context:GetArgumentValue("Extended Wash") == true) then
		validation.verify_specified(context, "Flow Organic")
		validation.verify_range(context, "Flow Organic", 1, 100)

		validation.verify_specified(context, "Volume Organic")
		validation.verify_range(context, "Volume Organic", 0, 100)

		validation.verify_specified(context, "Flow Aqueous")
		validation.verify_range(context, "Flow Aqueous", 1, 100)

		validation.verify_specified(context, "Volume Aqueous")
		validation.verify_range(context, "Volume Aqueous", 0, 300)
	end

	validation.verify_specified(context, "Inject mass calibrant")

	if (context:GetArgumentValue("Inject mass calibrant") == true) then
		validation.verify_specified(context, "Injection time")
		validation.verify_range(context, "Injection time", 2.0, 34.0)

		validation.verify_specified(context, "Flow rate")
		validation.verify_range(context, "Flow rate", 0.01, baltic.maxFlow)
	end	
end


---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)

	local chrom 	= require "chromatography"
	local ci 		= require "column_information"
	local parallel 	= require "parallel"
	local pf 		= require "pump_functions"
	local pp 		= require "palplus"
	local pr 		= require "PreRunFunctions"
	local csv 		= require "csv_file_logging"
	---@type Zirconium
	local zr 		= require "zirconium"
	---@type Pump
	local pump 		= context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end

	-- ProteYOLUTE intelligent monitoring
	local core = require("proteyolute_core")
	local session = core.beginProcedure(context, installed, pump, zr)

	---@type IPalParticipant
	local execLeft 	= context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux 	= context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type IJournal
	local journal 	= context:GetProcedureParticipant(baltic.JournalRole)
	---@type IProcedureStatusParticipant
	local status 	= context:GetProcedureParticipant(baltic.LcStatusRole)
	local valveI 	= pp.QueryValveDrive(execLeft, pp.Capabilities.ILcInjectorValve)
	local valveT 	= pp.QueryValveDrive(execLeft, pp.Capabilities.ISelectorValve)
	---@type PumpSettings
	local settings  = pump:GetSettings()

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local solventReplacement = context:GetArgumentValue("Include solvent replacement")

	local N = baltic.Naming

	local csvFileName = "Preparation_Results"

	-- read the current column oven temperature
	local ovenTemp = pump:GetCurrentExternalTemperature()
	context:Log("--- Oven temperature: {0}", ovenTemp)
	if (ovenTemp < 1) then
		ovenTemp = 40
		context:Log("--- internal variable ovenTemp: {0}", ovenTemp)
	end

	---Log the item positions in the execution log file
	local function printItemPositionNames()
		local iP = pp.QueryModules(execAux, "ItemPositionDescription")
		context:Log("Item position order unsorted:")
		for i=1, 10 do
			if iP[i] == nil then break end
			context:Log("{0}.  {1}", i, iP[i].Name)
		end
	end

	local function sleep_50()
		context:Sleep(50)
	end

	local function sleep_100()
		context:Sleep(100)
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

		local runTime = 0

		pf.Manualmode_Pump_constantPressure(channel, pressure, pump, sleep_100)
		while true do
			if zr.IsEmpty(pump, channel) or (runTime >= 300) then
				pf.Manualmode_Pump_constantSpeed(channel, 0, pump, sleep_100)
				local msg = DotNetString.Format("Pump {0:}:", channel)
				context:Report(msg, Severity.Error, true, "Could not build pressure within the time limit.\n\nLikely causes:\n- Large air bubble in the pump\n- Empty solvent reservoir\n- Leaking fitting or connection\n- Pump valve not seated correctly\n\nThe system will decompress. Check solvent levels and connections, then retry.")
				pr.decompressSystem(context)
				context:Abort()
			end

			-- PNS-748
			-- the percentage alone does not give enough room for the low target pressures at solvent exchange.
			-- So also accept a 2 bar difference as well to make solvent exchange pass more consistently.
			if (pump:GetCurrentPressure(channel) > pressure*.99 or pump:GetCurrentPressure(channel) > pressure - 2) then break end
			yield_function()
			runTime = runTime + 1
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

	---Queueing the cleaning aktivities
	---@param context IProcedureExecutionContext
	---@param colRed ColorsRGB
	---@param colBlue ColorsRGB
	---@param yield_function function
	local function queue_autosampler_wash(context, colRed, colBlue, yield_function)
		--clean injector with loop and syringe
		pr.Signalize_Reset(context)
		pp.PrimeLCPToolLoop(execLeft, true, false)
		while not execLeft.IsIdle do
			yield_function()
		end
		if context:GetArgumentValue("Disable Injector Cleaning") == false then
			pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)
			while not execLeft.IsIdle do
				yield_function()
			end
			parallel.sleep(yield_function, 1000)
			pr.SignalizeValveIInjectionPath(context, colRed, execLeft, baltic.InjectionValve.Inject)
			pp.CleanInjector(execLeft, execAux, pp.Organic, 25, false, false)			-- IdexFix05
			while not execLeft.IsIdle do
				yield_function()
			end
			pr.SignalizeValveIInjectionPath(context, colBlue, execLeft, nil)
			pp.CleanInjector(execLeft, execAux, pp.Aqueous, 500, false, true)			-- IdexFix05
			execLeft:Wait(pp.Quantity(1000, "ms"))
			pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
			while not execLeft.IsIdle do
				yield_function()
			end
			pr.Signalize_Reset(context)
		end
	end
--[[
	---Queueing the cleaning aktivities
	---@param context IProcedureExecutionContext
	---@param colRed string
	---@param colBlue string
	---@param yield_function function
	local function queue_autosampler_cleanInjector(context, colRed, colBlue, yield_function) -- only called in prepare_autosampler
		--clean injector only w/o loop
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)
		while not execLeft.IsIdle do
			yield_function()
		end
		parallel.sleep(yield_function, 1000)
		pr.SignalizeValveIInjectionPath(context, colRed, execLeft, baltic.InjectionValve.Load)
		pp.CleanInjector(execLeft, execAux, pp.Organic, 25, true, false)
		while not execLeft.IsIdle do
			yield_function()
		end
		pr.SignalizeValveIInjectionPath(context, colBlue, execLeft, nil)
		pp.CleanInjector(execLeft, execAux, pp.Aqueous, 500, false, true)
		execLeft:Wait(pp.Quantity(1000, "ms"))
		pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
		while not execLeft.IsIdle do
			yield_function()
		end
		pr.Signalize_Reset(context)
	end
--]]
	---Cleaning the syringe
	---@param yield_function function
	local function prepare_autosampler(yield_function) --this function is called in prepare
		local depth = pp.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
		local primeCycles = 1
		local dispenseVolume = 300
		local pumpSpeed = 30
		if solventReplacement then
			primeCycles = 3
			dispenseVolume = 5000
		end
		pp.EmptySyringe_And_LeaveObject(context, execLeft, execAux, depth, installed.SyringeZeroPosition)

        local wash_module = pp.QueryModule(execLeft, pp.Capabilities.ILcMsWashStation)

		---Bürkert Valve is opened in VolumeToBePumped and closed in PrimeInjectorWithVolumePump

		---move the LCMS tool to waste position 2 and dispense 300uL (5mL if solvent replacement) solv 2 at 30uL/s
		execLeft:MoveToObject(wash_module, pp.Organic)
		execLeft:PenetrateObject(wash_module, pp.Organic, depth, pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
		pp.VolumeToBePumped(execLeft, pp.Organic, dispenseVolume, pumpSpeed)
		execLeft:LeaveObject()

		---move the LCMS tool to waste position 1 and dispense 300uL (5mL if solvent replacement) solv 1 at 30uL/s
		execLeft:MoveToObject(wash_module, pp.Aqueous)
		execLeft:PenetrateObject(wash_module, pp.Aqueous, depth, pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
		pp.VolumeToBePumped(execLeft, pp.Aqueous, dispenseVolume, pumpSpeed)
		execLeft:LeaveObject()

		pp.PrimeSyringeWithVolumePump(context, execLeft, primeCycles, yield_function, true)		-- 3x (2x organic + 4x aqueous)
		pp.PrimeInjectorWithVolumePump(context, execLeft, yield_function, false)
	end

	---Extensively cleaning the autosampler
	---@param includeTrapColumn boolean
	---@param yield_function function
	---@param timeout number
	---@return ErrorCode
	local function extended_wash(includeTrapColumn, yield_function, timeout)
		local max_volume = pump.TotalPistonVolume - 0.001
		local organic_volume = context:GetArgumentValue("Volume Organic")
		local aqueous_volume = context:GetArgumentValue("Volume Aqueous")
		local unityFlowInjectionPath = chrom.GetUnityFlow(installed, pressSettings.GradientPumpMaxTargetPressure, true, true, true, false, includeTrapColumn, nil)
		local organic_pressure = context:GetArgumentValue("Flow Organic") / unityFlowInjectionPath
		local aqueous_pressure = context:GetArgumentValue("Flow Aqueous") / unityFlowInjectionPath

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Volume Organic", organic_volume, "\181L", "N1"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Volume Aqueous", aqueous_volume, "\181L", "N1"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow Organic", context:GetArgumentValue("Flow Organic"), "\181L/min", "N1"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow Aqueous", context:GetArgumentValue("Flow Aqueous"), "\181L/min", "N1"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Do Not Include Trap Column", context:GetArgumentValue("Do Not Include Trap Column")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Disable Injector Cleaning", context:GetArgumentValue("Disable Injector Cleaning")))

		---@type ErrorCode
		local error_code = {err = "", message = ""}

		local function warningmsg(msg)
			error_code.err = "warning"
			error_code.message = msg..error_code.message
		end

		---Priming with pump A
		---@param volume number
		---@return nil|number
		local function prime(volume)
			local piston = pump:GetPistonPosition(zr.A)
			volume = volume + piston
			local max_time_allowed = pf.now() + timeout
			while (piston < volume) do
				if ( pf.now() > max_time_allowed ) then
					warningmsg("Pump time out ")
					return timeout
				end
				if (piston >= max_volume) then
					warningmsg("Pump exceeded volume limit ")
					return
				end
				context:Sleep(1000)
				piston = pump:GetPistonPosition(zr.A)
			end
			pump:GetPistonPosition(zr.A)
			context:Log("Prime piston: {0}", piston)
		end

		local function waitForCriteria(criteria)
			local runTime = 0
			repeat
				if runTime >= 300 then
					context:Report("Extended Wash", Severity.Error, true, "Extended wash timed out after 5 minutes.\n\nThe system could not reach the required pressure or flow in time.\n\nCheck for air bubbles, leaks, or insufficient solvent. The system will decompress.")
					pr.decompressSystem(context)
					context:Abort()
				end
				context:Sleep(1000)
				runTime =  runTime + 1
			until criteria()
		end

		---Cleaning the injector with solvent A or B
		---@param organic boolean
		local function CleanInjector_with_solvent(organic)
			--Clean trap column with organic solvent 2
			local pressure = organic_pressure
			pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)
			while not execLeft.IsIdle do
				yield_function()
			end
			sleep_1000()
			if organic then
				pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Red, execLeft, baltic.InjectionValve.Inject)
				-- fill sample loop with solvent 2
				pp.CleanInjector(execLeft, execAux, pp.Organic, 25, true, false)
			else
				pressure = aqueous_pressure
				pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Blue, execLeft, nil)
				-- fill sample loop with solvent 1
				pp.CleanInjector(execLeft, execAux, pp.Aqueous, 500, true, false)
			end
				-- wait until injector has been cleaned
			while not execLeft.IsIdle do
				yield_function()
			end
			waitForCriteria(function() local isPressureReached = pump:GetCurrentPressure(zr.A) >= (pressure*0.7) return isPressureReached end)
			pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Normal, execAux, nil)
			context:Sleep(250)
			pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)
			context:Sleep(250)
		end

		local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
		-- bail if a pump failed degassing..
		if not (a and b) then
			pr.decompressSystem(context)
			context:Abort()
		end

		zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_6_1)

		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject, false)
		if context:GetArgumentValue("Do Not Include Trap Column") then
			pr.SetValvePosition(execLeft, valveT, baltic.TrapValve.InjectWaste)
		else
			pr.SetValvePosition(execLeft, valveT, baltic.TrapValve.Trap)
		end

		-- clean organic
		context:Sleep(500)
		pf.Manualmode_Pump_constantPressure(zr.A, organic_pressure, pump, sleep_100)
		if context:GetArgumentValue("Disable Injector Cleaning") == false then
			context:Log("--- Clean injector with organic")
			CleanInjector_with_solvent(true)	-- clean with organic
			execLeft:Wait(pp.Quantity(1000, "ms"))
			pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
			context:Log("--- Prime with organic volume: {0}uL", organic_volume)
		end
		if organic_volume ~= nil then prime(organic_volume) end

		-- clean aqueous
		pf.Manualmode_Pump_constantPressure(zr.A, aqueous_pressure, pump, sleep_100)
		if context:GetArgumentValue("Disable Injector Cleaning") == false then
			context:Log("--- Clean injector with aqueous")
			CleanInjector_with_solvent(false)	-- clean aqueous
			execLeft:Wait(pp.Quantity(1000, "ms"))
			pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
			context:Log("--- Prime with aqueous volume: {0}uL", aqueous_volume)
		end
		if aqueous_volume ~= nil then prime(aqueous_volume) end

		execLeft:LeaveObject()

		return error_code
	end

	---Calculating the trap pressure
	---@param column IChromatographyColumnType
	---@return number, number
	local function TrapEqLoad_pressure(column)
		local trapFlow = chrom.TrapTragetFlow(column)
		local eqSystemPres_10ul = chrom.GetEquilibrationSystemPressure(installed, trapFlow)
		local calculatedTrapPressure = math.min(chrom.column_pressure(column, pressSettings.GradientPumpMaxTargetPressure, trapFlow, chrom.viscosity_H2O_20C) + eqSystemPres_10ul, pf.getColumnMaxPressure(column, pressSettings.GradientPumpMaxTargetPressure))
		return calculatedTrapPressure, trapFlow
	end

	---Equilibrate the trap column
	local function equilibrate_trap_column()
		---@type IChromatographyColumnType
		local trap 					= context:GetArgumentValue("trap")
		local equiVolume			= 50 * chrom.column_volume(trap) + 30 -- combined solvent volume
		local sleep					= 5		-- seconds
		local pressure, flow = TrapEqLoad_pressure(trap)

		local limit = pf.GetMaxPressureLimitWithDelta(installed.MaxPumpPressure, pressure)
		pf.SetMaxPressureLimit(zr.A, limit, pump, sleep_100)
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_6_1)

		context:Log(baltic.devider)
		context:Log("--- Trap column preparation running ...")
		context:Log("--- Preparation volume [uL]:   			{0}", equiVolume)
		context:Log("--- Preparation target pressure [bar]: 	{0}", pressure)
		context:Log("--- Preparation flow [uL/min]: 			{0}", flow)

		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)
		while not execLeft.IsIdle do
			sleep_100()
		end
		sleep_1000()
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Red, execLeft, baltic.InjectionValve.Inject)

		-- fill loop using solvent 2
		pp.CleanInjector(execLeft, execAux, pp.Organic, 25, true, true)
		execLeft:Wait(pp.Quantity(1000, "ms"))
		while not execLeft.IsIdle do
			sleep_100()
		end
		set_valves(baltic.PumpValve.Inject, nil, baltic.InjectionValve.Load, baltic.TrapValve.Trap)
		context:Sleep(500)
		pr.Signalize_Reset(context)

		pp.PrimeLCPToolLoop(execLeft, true, true)

		buildPressure_A(pressure)

		local startPistonPos = pump:GetPistonPosition(zr.A)
		repeat
			context:Sleep(sleep*1000)
			local actPistPos = pump:GetPistonPosition(zr.A)
			local volume = actPistPos-startPistonPos
--			context:Log("Pump A: dispensed volume {0} ul; current flow {1} ul/min", volume, flow)
		until (volume >= equiVolume)
		pf.reducePressure(context, pump, zr, zr.A, nil, 50, 50, 20, sleep_100, baltic.smooth)

		-- wait for pump to stop completely
		pf.isPumpIdle(pump, sleep_100)
		context:Log("Trap column equilibration finished")
	end

	---Equilibrate the separation column
	local function equilibrate_separation_column()
		---@type IChromatographyColumnType
		local sep 			= context:GetArgumentValue("separator")
		local columnVolume	= chrom.column_volume(sep)
		local equiVolume	= columnVolume*5
		local sleep			= 5      -- seconds
		local minute		= 60     -- = 1 minute
		local pMax = math.min(pf.getColumnMaxPressure(sep, pressSettings.GradientPumpMaxTargetPressure), baltic.Settings.GradientPumpDefColumnEquilPressure)

		local limit = pf.GetMaxPressureLimitWithDelta(installed.MaxPumpPressure, pMax)
		pf.SetMaxPressureLimit(zr.A, limit, pump, sleep_100)
		pf.SetMaxPressureLimit(zr.B, limit, pump, sleep_100)

		zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)
		-- if the equilibration flow is lower than this, expect clogged column
		local column_flow = chrom.column_flow(sep, pressSettings.GradientPumpMaxTargetPressure, pMax, chrom.viscosity_mix(ovenTemp, 1))

		context:Log(baltic.devider)
		context:Log("Separation column equilibration running ...")
		context:Log("Preparation volume [uL]: {0}", equiVolume)
		context:Log("Preparation target pressure [bar]: {0}", pMax)
		context:Log("Preparation flow [uL/min]: {0}", column_flow)
		context:Log(baltic.devider)

		---Return the average unity flow of the desired channel
		---@param channel Channel
		---@return number
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

		pr.Signalize_Reset(context)
		set_valves(nil, baltic.PumpValve.MixTee, baltic.InjectionValve.Inject, baltic.TrapValve.GradientA)
		context:Sleep(250)

		buildPressure_B(pMax)
		context:Sleep(30*1000)

		accumulate(zr.B)
		pf.reducePressure(context, pump, zr, nil, zr.B, 50, 50, 20, sleep_100, baltic.smooth)
		zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste)
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee, false) -- specific direction here to keep pressure
		context:Sleep(250)
		buildPressure_A(pMax)
		context:Sleep(30*1000)
		local uF = accumulate(zr.A)
		context:Log("Separation Column system unity flow [uL/min/bar]: {0}", uF)
--		journal:Add(JournalEntry("SystemUnityFlow sep column", uF, "\181L/min/bar"))

		pf.reducePressure(context, pump, zr, zr.A, nil, 50, 50, 20, sleep_100, baltic.smooth)
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste)
		context:Sleep(250)
		pr.Signalize_Reset(context)
		-- wait for pump to stop completely
		pf.isPumpIdle(pump, sleep_100)
		context:Log("Separation column equilibration finished")
	end

	---Flush the route 1, 2 and 3
	---@param bar number
	local function flushRoutes(bar)
		local p = pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false)	-- flow path is w/o columns; press limit is max column press + 20 bar
		p = math.min(bar, p)
		local function flushChannel(channel)
			local unUsedChannel = zr.A
			if channel == zr.A then
				unUsedChannel = zr.B
			end
			zr.SetValvePosition(context, pump, unUsedChannel, baltic.PumpValve.Waste, nil)
			zr.SetValvePosition(context, pump, channel, baltic.PumpValve.MixTee, nil)
			pr.SetValvePosition(execLeft, valveT, baltic.TrapValve.Waste)
			build_pressure(channel, p, sleep_1000)
		end

		flushChannel(zr.B)
		context:Sleep(1000*60)
		pf.reducePressure(context, pump, zr, nil, zr.B, 50, 50, 20, sleep_100, baltic.smooth)
		flushChannel(zr.A)
		context:Sleep(1000*60)
		pf.reducePressure(context, pump, zr, zr.A, nil, 50, 50, 20, sleep_100, baltic.smooth)

--[[ disabled due to PBNE-585
		p = pf.setMaxPressureLimitA(context, pump, isTrap, false, nil)	-- flow path is with columns; press limit is max column press + 20 bar
		p = math.min(bar, p)
		set_valves(baltic.PumpValve.MixTee, baltic.PumpValve.Compress, nil, baltic.TrapValve.GradientT)
		context:Sleep(250)
		context:Signalize(baltic.ColorsRGB.Blue, N.MixTeeToTrapValve, N.ValveTShortGroove, N.ValveTLongGroove, N.Trap, N.TransferLine, N..Separator)
		context:Signalize(baltic.ColorsRGB.LightGray, N.TrapValveToWaste, N.TrapValveToWaste)
		build_pressure(zr.A, p, sleep_1000)
		context:Sleep(1000*180)
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
--]]
		p = pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false, nil)	-- flow path w/o columns; press limit is max column press + 20 bar
		local desiredFlow = 10		-- µL/min
		bar = chrom.GetEquilibrationSystemPressure(installed, desiredFlow)
		p = math.min(bar, p)
		set_valves(baltic.PumpValve.Inject, baltic.PumpValve.Waste, baltic.InjectionValve.Load, baltic.TrapValve.InjectWaste)
		context:Sleep(250)
		pr.Signalize_Reset(context)
		build_pressure(zr.A, p, sleep_1000)
		context:Sleep(1000*120)
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
		pr.Signalize_Reset(context)
	end

	local function calSensors()
		context:SetAbortEnabled(false)
		context:Sleep(2000)
		status:SetStatus("Pressure Sensor Calibration")
		context:Log(baltic.devider)
		context:Log("--- Pressure Sensor Offset Calibration:")

		local a, b, passed = pf.calibrate_press_sensors(installed, context, pump, zr, baltic.PumpValve.Waste)
		if not passed then
			context:SetAbortEnabled(true)
			context:Abort()
		end
		settings.ExternalZeroPressureOffsetA = a
		settings.ExternalZeroPressureOffsetB = b
		pump:SetSettings(settings)

		context:Log(baltic.devider)
		-- here just journal because in the twinscape file calibration values are always included
		status:RemoveStatus("Pressure Sensor Calibration")
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Pressure sensor calibration", true))
		journal:Add(JournalEntry.Set(LogTo.Journal,context.Name, "--- Pressure Sensor Calibration", os.date("%c")))
		journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Pressure sensor offset A", a, "bar", "N1"))
		journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Pressure sensor offset B", b, "bar", "N1"))
		csv.logValueInCSVFile(context, csvFileName, "Pressure sensor calibration", "Pressure sensor offset A", a, "bar", 1)
		csv.logValueInCSVFile(context, csvFileName, "Pressure sensor calibration", "Pressure sensor offset B", b, "bar", 1)
		context:SetAbortEnabled(true)
		return true
	end

	--- Pressure reduction and checking whether a low pressure is reached, even if the pressure sensor has an unfavorable offset.
	local function simpleReducePressure()
		local function simpleReduce(channel, yield_function)
			local actualPressure = pump:GetCurrentPressure(channel)
			if (actualPressure > 50) then
		        context:Log("--- Pressure channel {0}: {1}", channel, pump:GetCurrentPressure(channel))
				local timeOut = pf.now() + 60			-- 60 seconds timeout
				pf.Manualmode_Pump_constantPressure(channel, 40, pump, yield_function)
				parallel.sleep(yield_function, 500)
				while (actualPressure > 50) and (timeOut > pf.now()) do
					if ((actualPressure - pump:GetCurrentPressure(channel)) < 2) then break end		-- pressure reduction is low => low pressure is reached
					actualPressure = pump:GetCurrentPressure(channel)
					parallel.sleep(yield_function, 500)
				end
				pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_function)
				context:Log("--- Final pressure channel {0}: {1}", channel, pump:GetCurrentPressure(channel))
			end
		end

		local p_reduce_A = { simpleReduce, zr.A, parallel.yield }
		local p_reduce_B = { simpleReduce, zr.B, parallel.yield }
		parallel.run(sleep_50, p_reduce_A, p_reduce_B)
	end

	context:Log(baltic.devider)
	context:Log("Lua date:             {0}", Date)
	context:Log("No of item positions: {0}", installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions))

	context:Log(baltic.devider)
	local  FWVersion, versionOK = pr.isFWVersion_OK(execAux, baltic.minAutoSamplerFWVersion, baltic.maxAutoSamplerFWVersion)
	-->	00:00:00.2	Autosampler firmware version: 4.16.30549.0
	if not versionOK then
		local errorMsg = DotNetString.Format("This plug-in is not compatible with firmware version: {0}", FWVersion)
		context:Report("Firmware version", Severity.Error, true, errorMsg)
		context:Abort()
	end
	context:Log("Autosampler firmware version: {0}", FWVersion)
	context:Log(baltic.devider)

	csv.logValueInCSVFile(context, csvFileName, "Preparation started", "", "", "", -1)

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)

	context:Log(baltic.devider)
	printItemPositionNames()

	--	Signalize_Reset
	pr.Signalize_Reset(context)
	context:ShowComposition(false)

	local fR = require "PreRunFunctions"
	fR.iniFlowResistance(context, pump)

	-- init zirconium channels, abort if unsuccessful
	if not zr.InitChannel(context, pump, zr.A) then
		context:Report(N.PumpA, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end
	if not zr.InitChannel(context, pump, zr.B) then
		context:Report(N.PumpB, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end

	if (context:GetArgumentValue("separator") == nil) then
		local msg = "Please select a separation column"
		context:Report("Missing column", Severity.Warn, true, msg)
		context:Abort()
	else
		if not chrom.IsColumnParameterSet(context, context:GetArgumentValue("separator")) then
			context:Abort()
		end
	end
	---@type IChromatographyColumnType
	local trap = context:GetArgumentValue("trap")
	if (trap == nil) then
		local msg = "Please select a trap column"
		context:Report("Missing column", Severity.Warn, true, msg)
		context:Abort()
	elseif not chrom.IsColumnParameterSet(context, trap) then
		context:Abort()
	end

	ci.logColumnInformation(context, trap, context:GetArgumentValue("separator"))

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)

	simpleReducePressure()

	-- stop both pumps
	pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
	pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

	calSensors()
	settings = pump:GetSettings()		-- read updated settings again for later use

	local caller = "Prepare"
	zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)
	zr.logInstrSettings(context, settings, caller)

	-- if there is still pressure from e.g. Idle flow, it is released in setMaxPressureLimitsAB()
	pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false)		-- set max pump pressure

	-- log to twinscape the general procedure parameters
	-- results or subparameters are logged in the sub procedures
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Inject mass calibrant", context:GetArgumentValue("Inject mass calibrant")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Extended Wash", context:GetArgumentValue("Extended Wash")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Include solvent replacement", context:GetArgumentValue("Include solvent replacement")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Include column preparation", context:GetArgumentValue("Include column preparation")))

	if (context:GetArgumentValue("Inject mass calibrant") == true) and
		(context:GetArgumentValue("Extended Wash") == false) and
		(context:GetArgumentValue("Include solvent replacement") == false) and
		(context:GetArgumentValue("Include column preparation") == false) then
		----------------------------------------------------
		-- Calibrant Injection
		----------------------------------------------------
		local ic = require "InjectCalibrant"
		---@type IChromatographyColumnType
		local sep = context:GetArgumentValue("separator")
		local flowTarget = context:GetArgumentValue("Flow rate")

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Injection time", context:GetArgumentValue("Injection time"), "min", "N2"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow rate", flowTarget, "\181L/min", "N1"))

		if flowTarget == nil then flowTarget = 0 end
		local composition = 0
		local flow = pf.getSepColumnFlow(nil, sep, pressSettings.GradientPumpMaxTargetPressure, flowTarget, composition, ovenTemp)

		---@type CalibrantInjectionParameter
		local qpcaui_params = {
			sample_aspirate_speed 		= pp.Quantity("5 uL/s"),
			sample_postaspirate_delay 	= pp.Quantity("2000 ms"),
			presample_air_volume 		= pp.Quantity("0.5 uL"),
			calibrant_volume 			= math.min(flow*context:GetArgumentValue("Injection time"), 20.0),	-- 17=20-preair-postair
										-- don't make pp.Quantity due to calculations with this value
			postsample_air_volume 		= pp.Quantity("1 uL"),
			sample_inject_speed 		= pp.Quantity("1 uL/s"),
			flowPerBar 					= chrom.column_flow(context:GetArgumentValue("separator"), pressSettings.GradientPumpMaxTargetPressure, 1, chrom.viscosity_mix(ovenTemp, 0)),
			systemVolume 				= chrom.column_volume(context:GetArgumentValue("separator")) + baltic.SystemVolume		-- column volume + capillary volume
		}
		local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100) -- make sure that pumpheads are sufficiently filled
		-- bail if a pump failed degassing..
		if (not (a and b)) then
			pr.decompressSystem(context)
			context:Abort()
		end
		ic.injectCalibrant(installed, context, qpcaui_params, flow, sleep_100)
		context:Report("Calibrant Injection", Severity.Info, true, "finished")
	else
		local prepPassed = true

		context:Log(baltic.devider)
		context:Log("--- System Preparation:")
		context:Log(baltic.devider)
--[[ disabled due to PBNE-585
			local Isr = "Solvent replacement"

			if solventReplacement then
				context:Report(Isr, Severity.Info, true, "Please disconnect the separation column, then click 'Confirm' to continue.")
			end
--]]

		local degasserOnTime = pf.now()
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)

		local prepText = "Preparation, this takes about 5 minutes"
		status:SetStatus(prepText)
		local min_iterations = 1;
		if solventReplacement then
			status:RemoveStatus(prepText)
			prepText = "Running solvent replacement"
			status:SetStatus(prepText)
			context:Log(baltic.devider)
			context:Log("--- Solvent Replacement:")
			context:Log(baltic.devider)
			min_iterations = 10;
		else
			context:Log(baltic.devider)
			context:Log("--- Standard Preparation ")
			context:Log(baltic.devider)
		end

		zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)
		local msg = "Refilling pumps"
		status:SetStatus(msg)

		local p_degas_A = { degas_Channel, context, zr, zr.A, min_iterations, true, -1, nil, nil, degasserOnTime, parallel.yield }
		local p_degas_B = { degas_Channel, context, zr, zr.B, min_iterations, true, -1, nil, nil, degasserOnTime, parallel.yield }
		local p_prepAutosampler = { prepare_autosampler, parallel.yield }
		local a, repA, passedA, b, repB, passedB = parallel.run(sleep_50, p_degas_A, p_degas_B, p_prepAutosampler)

		local namePrepA = "Preparation A"
		local namePrepB = "Preparation B"
		journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "--- Preparation", os.date("%c")))
		journal:Add(JournalEntry.Set(LogTo.Both, context.Name, namePrepA, a, "\181L", "N2"))
		journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Preparation repetitions A", repA, "", "N0"))
		journal:Add(JournalEntry.Set(LogTo.Both, context.Name, namePrepB, b, "\181L", "N2"))
		journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Preparation repetitions B", repB, "", "N0"))
		csv.logValueInCSVFile(context, csvFileName, "Preparation", namePrepA , a, "\181L", 2)
		csv.logValueInCSVFile(context, csvFileName, "Preparation", "Preparation repetitions A" , repA, "", 0)
		csv.logValueInCSVFile(context, csvFileName, "Preparation", namePrepB , b, "\181L", 2)
		csv.logValueInCSVFile(context, csvFileName, "Preparation", "Preparation repetitions B" , repB, "", 0)

		--[[ PNS-694: In general we want to have this, but it is postponed to future version because too little time for validation in 1.0
		if (a > 40) or (b > 40)then
			local msg40 = "Too much air in the pump detected. Please check the solvent filling and for a leakage."
			if (a > 40) then
				context:Report(namePrepA, Severity.Warn, true, msg40)
			end
			if (b > 40) then
				context:Report(namePrepB, Severity.Warn, true, msg40)
			end
			prepPassed = false
		end
		if (a < 5) or (a > 15) or (b < 10) or (b > 25) then
			local msg5_15 = "Preparation value out of range. Please run a preparation including solvent replacement, and check correct solvent assignment (100% water on channel A; 80 to 90% acetonitrile in water on channel B)."
			if (a < 5) or (a > 15) then
				context:Report(namePrepA, Severity.Info, true, msg5_15)
			end
			if (b < 10) or (b > 25) then
				context:Report(namePrepB, Severity.Info, true, msg5_15)
			end
			prepPassed = false
		end
		]]

		if passedA == false or passedB == false then prepPassed = false end

		zr.setPumpSettings(context, pump, settings)
		zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)
		zr.logInstrSettings(context, settings, caller)
		-- switch degasser off
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
		status:RemoveStatus(msg)

		if (not (a and b)) then
			context:Log("Degas returned invalid parameters")
			pr.decompressSystem(context)
			context:Abort()
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
				pr.decompressSystem(context)
				context:Abort()
			end
		end

		-- wait for AS done (we cannot do that in autosampler_purge/prepare_autosampler since they would block running degas concurrently)
		while not execLeft.IsIdle do
			sleep_100()
		end

		if solventReplacement then
			context:ShowComposition(true)
			flushRoutes(100)
			pf.isPumpIdle(pump, sleep_100)
		end

		status:RemoveStatus(prepText)

		if context:GetArgumentValue("Extended Wash") and prepPassed == true then
			context:Log(baltic.devider)
			context:Log("--- Extended Wash:")
			context:Log(baltic.devider)
			status:SetStatus("cleaning injection path")
			---@type ErrorCode
			local error_code = {err = "", message = ""}
			error_code = extended_wash(true, sleep_100, 900)
			if ( error_code.err ~= "" ) then
				error_code.message = "failed - "..error_code.message
				context:Report("Extended wash", Severity.Warn, true, error_code.message)
				prepPassed = false
			end
			execLeft:MoveToHome()
			status:RemoveStatus("cleaning injection path")
		end

		if (context:GetArgumentValue("Include column preparation"))  and prepPassed == true then
			status:SetStatus("Column preparation")
			context:Log(baltic.devider)
			context:Log("--- Column Preparation:")
			context:Log(baltic.devider)
			local Icp = "Column preparation"
			context:Report(Icp, Severity.Info, true, "If the separation column is disconnected please connect the separation column, then click 'Confirm' to continue.")
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 300, sleep_100, baltic.smooth)
			equilibrate_trap_column()
			queue_autosampler_wash(context, baltic.ColorsRGB.Red, baltic.ColorsRGB.Blue, sleep_1000)
			context:WaitForSignal(Icp)
			context:ShowComposition(true)
			equilibrate_separation_column()
			-- wait for queued autosampler wash to finish
			while not execLeft.IsIdle do
				sleep_100()
			end
			status:RemoveStatus("Column preparation")
		end
		if prepPassed then
			context:Report("Preparation", Severity.Info, true, "passed")
		else
			context:Report("Preparation", Severity.Info, true, "failed")
		end
	end
	pr.Signalize_Reset(context)
	status:SetStatus("Reducing pressure")
	pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 300, sleep_100, baltic.smooth)	-- PBNE-607
	status:RemoveStatus("Reducing pressure")
	-- pumps are stoped in 'reducePressure'
	pf.isPumpIdle(pump, sleep_100)
	context:ShowComposition(true)

	dictator:Dispose()

	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)

	while not execLeft.IsIdle do
		sleep_100()
	end

	session:finish("completed")
end

