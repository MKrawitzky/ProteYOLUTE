-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/11/17"

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")

local P = {}

---Execute an extended wash function
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param execLeft IPalParticipant
---@param execAux IPalParticipant
---@param pp PalPlus
---@param pump Pump
---@param includeTrapColumn boolean
---@param isTransferCap boolean
---@param cycles number
---@param organic_volume number
---@param aqueous_volume number
---@param organic_flow number
---@param aqueous_flow number
---@param yield_function function
---@return ErrorCode
function P.extended_wash(installed, context, execLeft, execAux, pp, pump, includeTrapColumn, isTransferCap, cycles, organic_volume, aqueous_volume, organic_flow, aqueous_flow, yield_function)
	local baltic = require "baltic"
	local chrom  = require "chromatography"
	require "degas"
	---@type Zirconium
	local zr = require "zirconium"
	local pf = require "pump_functions"
	local pr = require "PreRunFunctions"

	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

    local settings = pump:GetSettings()
	local max_volume = pump.TotalPistonVolume - 0.001
	local aqueousUnityFlowInjectionPath = chrom.GetUnityFlow(installed, pressSettings.GradientPumpMaxTargetPressure, true, true, true, isTransferCap, includeTrapColumn, nil)
	local organic_pressure = organic_flow / aqueousUnityFlowInjectionPath
	local aqueous_pressure = aqueous_flow / aqueousUnityFlowInjectionPath
	local timeoutA = (aqueous_volume / aqueous_flow) * 60 * 10		-- [seconds]
	local timeoutB = (organic_volume / organic_flow) * 60 * 10		-- [seconds]
	local cnt = 0

	local error_code = {err = "", message = ""}

	local function warningmsg(msg)
	error_code.err = "warning"
	error_code.message = msg..error_code.message
	end
	local function errormsg(msg)
	error_code.err = "error"
	error_code.message = msg..error_code.message
	end

	local function sleep_100()
		context:Sleep(100)
	end

	---Prime the injection path with pump A
	---@param volume number
	---@return nil
	local function prime(volume, timeout)
		local piston = pump:GetPistonPosition(zr.A)
		volume = volume + piston
		local max_time_allowed = pf.now() + timeout
		while (piston < volume) do
			if ( pf.now() > max_time_allowed ) then
				warningmsg("Pump time out ")
				return
			end
			if (piston >= max_volume) then
				errormsg("Pump exceeded volume limit ")
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
				context:Report("Extended wash", Severity.Error, true, "Extended wash timed out after 5 minutes.\n\nThe expected pressure or flow condition was not reached in time.\n\nLikely causes:\n- Large air bubble in the system\n- Blocked column or waste line\n- Insufficient solvent\n\nThe system will decompress. Check connections and solvent levels, then retry.")
				pr.decompressSystem(context)
				context:Abort()
			end
			context:Sleep(1000)
			runTime =  runTime + 1
		until criteria()
	end

	---Cleaning the injector with aqueous or organic
	---@param organic boolean
	local function CleanInjector_with_organic(organic)
		--Clean trap column with organic solvent 2
		local pressure = organic_pressure
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)
		context:Sleep(500)
		if organic then
			pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Red, execLeft, baltic.InjectionValve.Inject)
			-- fill sample loop with solvent 2
			pp.CleanInjector(execLeft, execAux, pp.Organic, 25, true, false)
		else
			pressure = aqueous_pressure
			pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Blue, execLeft, baltic.InjectionValve.Inject)
			-- fill sample loop with solvent 1
			pp.CleanInjector(execLeft, execAux, pp.Aqueous, 500, true, false)
		end
		execLeft:Wait(pp.Quantity(1000, "ms"))
		pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
	-- wait until injector has been cleaned
		while not execLeft.IsIdle do
			yield_function()
		end
		waitForCriteria(function() local isPressureReached = pump:GetCurrentPressure(zr.A) >= (pressure*0.7) return isPressureReached end)
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Normal, execAux, baltic.InjectionValve.Inject)
		context:Sleep(250)
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)
		context:Sleep(250)
	end

	context:Log(baltic.devider)
	context:Log("--- Extended Wash ---")
	context:Log("Number of cycles: {0}", cycles)
	context:Log("Volume organic:   {0} \181L", organic_volume)
	context:Log("Flow organic:     {0} \181L/min", organic_flow)
	context:Log("Pressure organic: {0} bar", organic_pressure)
	context:Log("Timeout organic:  {0} sec", timeoutB)
	context:Log("Volume aqueous:   {0} \181L", aqueous_volume)
	context:Log("Flow aqueous:     {0} \181L/min", aqueous_flow)
	context:Log("Pressure aqueous: {0} bar", aqueous_pressure)
	context:Log("Timeout aqueous:  {0} sec", timeoutA)
	context:Log(baltic.devider)

	local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
	-- bail if a pump failed degassing..
	if not (a and b) then
		pr.decompressSystem(context)
		context:Abort()
	end

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)

	zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject, false)
	if includeTrapColumn then
		pr.SetValvePosition(execLeft, valveT, baltic.TrapValve.Trap)
		---@type IChromatographyColumnType
		local trp = context:GetArgumentValue("trap")
		local p = pf.getColumnMaxPressure(trp, pressSettings.GradientPumpMaxTargetPressure)
		if p < organic_pressure then organic_pressure = p end
		if p < aqueous_pressure then aqueous_pressure = p end
	else
		-- OneColumnSetup
		pr.SetValvePosition(execLeft, valveT, baltic.TrapValve.InjectWaste)
	end

	local actualMaxPressureLimit = pump:GetMaxPressureLimit (zr.A)

	local limitOrganic = pf.GetMaxPressureLimitWithDelta(installed.MaxPumpPressure, organic_pressure)
	local limitAqueous = pf.GetMaxPressureLimitWithDelta(installed.MaxPumpPressure, aqueous_pressure)
	local limit = math.max(limitOrganic, limitAqueous)

	if (limit >= actualMaxPressureLimit) then pump:SetMaxPressureLimit (zr.A, limit) end

	pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
	while (cnt < cycles) do
		cnt = cnt + 1
		-- clean organic
		context:Sleep(500)
		pf.Manualmode_Pump_constantPressure(zr.A, organic_pressure, pump, sleep_100)
		context:Log("--- Clean injector with organic")
		CleanInjector_with_organic(true)	-- clean with organic
		context:Log("--- Prime with organic volume: {0}uL", organic_volume)
		prime(organic_volume, timeoutB)

		-- clean aqueous
		context:Log("--- Clean injector with aqueous")
		pf.Manualmode_Pump_constantPressure(zr.A, aqueous_pressure, pump, sleep_100)
		CleanInjector_with_organic(false)	-- clean aqueous
		context:Log("--- Prime with aqueous volume: {0}uL", aqueous_volume)
		prime(aqueous_volume, timeoutA)
	end
	execLeft:LeaveObject()

	zr.setPumpSettings(context, pump, settings)
	local caller = "extended_wash"
	zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)
	zr.logInstrSettings(context, settings, caller)

	context:LogMeta(context.Name, "Extended wash", "", "N", "True")

	return error_code
end

---Cleaning the injection path
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param execLeft IPalParticipant
---@param execAux IPalParticipant
---@param chrom any
---@param pp PalPlus
---@param pr any
---@param pump Pump
---@param duration number [sec]
function P.injection_path_wash(installed, context, execLeft, execAux, chrom, pp, pr, pump, duration)
	local baltic = require "baltic"
	local pf = require "pump_functions"
	---@type Zirconium
	local zr = require "zirconium"
	local desiredFlow = 30
	local pressure = chrom.GetEquilibrationSystemPressure(installed, desiredFlow)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	local function sleep_100()
		context:Sleep(100)
	end

	local settings = pump:GetSettings()
	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)

	pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false, nil)

	local _, solventVolume = chrom.getCapillaryIDandLength(installed, "loop")
	pp.cleanInjectionPath(execLeft, execAux, solventVolume, pr, sleep_100)
	while not execLeft.IsIdle do sleep_100() end
	pf.Manualmode_Pump_constantPressure(zr.A, pressure, pump, sleep_100)
	pp.PrimeLCPToolLoop(execLeft, true, true)
	while not execLeft.IsIdle do sleep_100() end
	local bPTime = pf.now() + 60							-- time out 60 seconds
	while not zr.IsEmpty(pump, zr.A) do 
		context:Sleep(250)
		if (pump:GetCurrentPressure(zr.A) >= pressure-10) or (pf.now() > bPTime) then
			break
		end
	end
	local waitTime = duration * 1000			-- waiting time [seconds]
	context:Sleep(waitTime)
	zr.setPumpSettings(context, pump, settings)
end

return P
