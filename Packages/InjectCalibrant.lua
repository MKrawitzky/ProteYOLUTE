-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/11/17"

local P = {}

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")

---Handle an error message
---@param context IProcedureExecutionContext
---@param error_code ErrorCode
local function handle_message(context, error_code)
  local pr = require "PreRunFunctions"
	local text = "injectCalibrant"
	if ( error_code.err == "warning" ) then
		context:Report(text, Severity.Warn, true, error_code.message)
		pr.decompressSystem(context)
		context:Abort()
	elseif (error_code.err == "error" ) then
		context:Report(text, Severity.Error, true, error_code.message)
		pr.decompressSystem(context)
		context:Abort()
	else
		context:Report(text, Severity.Info, true, error_code.message)
	end
end

---Calculations for calibrant injection
---@param installed IInstalledHardwareContext
---@param flow number
---@param unityFlowColumn number
---@param systemVolume number
---@param calVolume number
---@param maxColumnPressure number
---@param pumpMaxPressure integer [bar]
---@return number
---@return number
---@return number
---@return number
---@return number
function P.calibrantInjectionTime(installed, flow, unityFlowColumn, systemVolume, calVolume, maxColumnPressure, pumpMaxPressure)
	local chrom =  require "chromatography"

								-- loading capillary, loop, injection capillary, transfer capillary
	local systemUnityFlow = chrom.GetUnityFlow(installed, pumpMaxPressure, true, true, true, true, false, nil)
	local unityFlowSum = 1/(1/systemUnityFlow+1/unityFlowColumn)
	local targetPressA = flow/unityFlowSum
	if targetPressA > maxColumnPressure then
		targetPressA = maxColumnPressure
		flow = math.min(flow, targetPressA*unityFlowSum)
	end
	if targetPressA > pumpMaxPressure then
		targetPressA = pumpMaxPressure
		flow = math.min(flow, targetPressA*unityFlowSum)
	end
	-- PNS-1047: increased the preparing time to get a longer calibrant time
	-- The wish is to have more time after the calibrant injection. So I added 4 minutes to the previous 3.5 minutes
	local preparingTime = 7.5
	local calStartTime = systemVolume/flow
	local calTime = calVolume/flow
	-- PNS-1047: increased the volume to get a longer calibrant time
	-- The wish is to have more time after the calibrant injection. So I increased the volume to get additional time of 4 minutes after the calibrant.
	local volume = systemVolume + calVolume + (flow * 5.5)
	local timeToSignalEnd = preparingTime + calStartTime + calTime

	return timeToSignalEnd, unityFlowSum, targetPressA, flow, volume
end

---Inject a calibrant
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param qpcaui_params CalibrantInjectionParameter
---@param flow number
---@param sleep_function function
function P.injectCalibrant(installed, context, qpcaui_params, flow, sleep_function)
	local baltic = require "baltic"
	local cplae = require "const_pressure_load_and_equilibration"
	local pf = require "pump_functions"
	local pp = require "palplus"
	local pr = require "PreRunFunctions"
	local qaw = require "queue_autosampler_wash"
	---@type Zirconium
	local zr = require "zirconium"

	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	local wash_module = pp.QueryModule(execAux, pp.Capabilities.ILcMsWashStation)
	local injector = pp.QueryModule(execAux, pp.Capabilities.IInjector)
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local tool = pp.QueryModule(execAux, pp.Capabilities.IToolLc)
	local speed = qpcaui_params.sample_aspirate_speed
	local delay = qpcaui_params.sample_postaspirate_delay
	local detergent_depth = pp.Quantity(baltic.WashSolventLinerPenetrationDepth, "mm")
	local depth = pp.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
	local itemPosition = pp.QueryModules(execAux, "ItemPositionDescription")
	local calibrant_2 = itemPosition[pr.GetItemPosIdx(2, execAux)]
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local maxColumnPressure = pf.getColumnMaxPressure(context:GetArgumentValue("separator"), pressSettings.GradientPumpMaxTargetPressure)
	local p,v,f,t = 0,0,0,0
	---@type ErrorCode
	local error_code = {err = "", message = ""}

	local ovenTemp = pump:GetCurrentExternalTemperature()
	context:Log("--- Oven temperature: {0}", ovenTemp)
	if (ovenTemp < 1) then
		ovenTemp = 20
		context:Log("--- internal variable ovenTemp: {0}", ovenTemp)
	end

	local timeToSignalEnd, unityFlowSum, targetPressA, flowNew, volume = P.calibrantInjectionTime(installed, flow, qpcaui_params.flowPerBar, qpcaui_params.systemVolume, qpcaui_params.calibrant_volume, maxColumnPressure, pressSettings.GradientPumpMaxTargetPressure)

	pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, true, targetPressA)

	local msg = "calibrant picking up and injection, this finishes in about " ..pf.noExp(timeToSignalEnd, 2) .." minutes"
	status:SetStatus(msg)
	execLeft:LeaveObject()
	execLeft:ChangeTool( tool )

	-- empty syringe into waste so there's room for new calibrant
	pp.EmptySyringe_And_LeaveObject(context, execLeft, execAux, depth, installed.SyringeZeroPosition)

	context:Log("Cleaning tool and tool loop")
	pp.PrimeLCPToolLoop(execLeft, true, false)
	execLeft:LeaveObject()

	-- always use partial loop injection for calibrant injection: minimize air
	-- get Rear Airgap
	execLeft:AspirateSyringe( qpcaui_params.presample_air_volume, speed, nil, delay)
	-- get calibrant
	execLeft:MoveToObject( calibrant_2, 1, true, true, true )
	execLeft:PenetrateWithBottomSense( calibrant_2, 1, pp.Quantity(1, "mm"), nil, nil)
	execLeft:AspirateSyringe( pp.Quantity(qpcaui_params.calibrant_volume+3, "uL"), speed, nil, delay)
	execLeft:LeaveObject()
	-- get Front Airgap
	execLeft:AspirateSyringe( qpcaui_params.postsample_air_volume, speed, nil, delay)
	-- dip needle exterior wash module Solvent A
	execLeft:MoveToObject( wash_module, pp.Aqueous, true, true, true )
	execLeft:PenetrateObject( wash_module, pp.Aqueous, detergent_depth, pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
	execLeft:Wait(pp.Quantity(5,"s"))
	execLeft:LeaveObject()

	-- Inject calibrant (similar to partial loop sample injection)
	execLeft:MoveToObject(injector)
	execLeft:PenetrateWithConstForce( injector )
	while not execLeft.IsIdle do  -- wait for calibrant injection to finish
		sleep_function()
	end
	-- reduce pressure to not to switch valves under high pressure during calibrant injection or extended wash
	-- !!! make sure that: pump.valve==MixTee AND NOT (trapValve==Analytical OR trapValve==Trap) !!!
	pf.reducePressure(context, pump, zr, zr.A, zr.B, 10, 5, 30, sleep_function, baltic.smooth)
	pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load) -- prepare to flush injector with calibrant into waste
	context:Sleep(500)
	pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Blue, execAux, baltic.InjectionValve.Load)
	execLeft:Wait(pp.Quantity("1000 ms")) --wait
	execLeft:DispenseSyringe( qpcaui_params.postsample_air_volume+1.5, qpcaui_params.sample_inject_speed) --flush injector with calibrant into waste
	execLeft:Wait(pp.Quantity("1000 ms")) --wait
	while not execLeft.IsIdle do  -- wait for calibrant injection to finish
		sleep_function()
	end
	pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)
	context:Sleep(500)
	pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Blue, execAux, baltic.InjectionValve.Inject)
	execLeft:Wait(pp.Quantity("1000 ms")) --wait
	execLeft:DispenseSyringe( pp.Quantity(qpcaui_params.calibrant_volume, "uL"), qpcaui_params.sample_inject_speed) -- inject calibrant into loop
	execLeft:Wait(pp.Quantity("1000 ms")) --wait
	while not execLeft.IsIdle do  -- wait for calibrant injection to finish
		sleep_function()
	end

	-- Prepare to push out calibrant 
	zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject)                               -- solvent A to loop valve
	pr.SetValvePosition(execLeft, valveT, baltic.TrapValve.Analytical)
	while not execLeft.IsIdle do
		sleep_function()
	end
	pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Normal, execAux, nil)	
	pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)
	context:Sleep(1000)

	status:RemoveStatus(msg)
	msg = "calibrant injection and wash, this finishes in about " ..pf.noExp(timeToSignalEnd, 2) .." minutes"
	status:SetStatus(msg)

	-- Start AS wash
	execLeft:EmptySyringe()
	context:Log("PrimeLCPToolLoop")
	pp.PrimeLCPToolLoop(execLeft, false, false)
	if context:GetArgumentValue("Disable Injector Cleaning") == false then
		context:Log("queue_clean_injector")
		qaw.queue_clean_injector(execLeft, execAux, pp, false, false)
	end
	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)

	context:Log(baltic.devider)
	context:Log("--- Calibrant Injection:")
	context:Log("--- Separator Unity-Flow @ ovenTemp: {0}ul/min/bar", qpcaui_params.flowPerBar)
	context:Log("--- System Unity-Flow (105cm/20um):  {0}ul/min/bar", unityFlowSum)
	context:Log("--- System Volume:                   {0}uL", qpcaui_params.systemVolume)
	context:Log("--- Injection Pressure:              {0}bar", targetPressA)
	context:Log("--- Calibrant volume:                {0}uL", qpcaui_params.calibrant_volume)
	context:Log("--- Volume:                          {0}uL", volume)
	context:Log("--- Flow:                            {0}uL/min", flowNew)
	context:Log("--- TimeToSignalEnd:                 {0}min", timeToSignalEnd)
	context:Log(baltic.devider)
	p,v,f,t,error_code.err, error_code.message = cplae.const_pressure_load_and_equilibration(pump, zr.A, targetPressA, volume, flowNew, sleep_function, timeToSignalEnd*600, false)
	if ( error_code.err ~= "" ) then
		context:Log("--- Calibrant injection failed")
		error_code.message = "Calibrant injection - "..error_code.message
		handle_message(context, error_code)
	end
	dictator:Dispose()
	context:Log("--- Finished: Calibrant Injection: (const_pressure_load_and_equilibration)")
	context:Log("--- act. pressure [bar]:                 {0}",p)
	context:Log("--- used volume [uL]:                    {0}",v)
	context:Log("--- average flow (incl. leaks) [uL/min]: {0}",f)
	context:Log("--- act. calibrant injection time [min]: {0}",t/60)
	context:Log(baltic.devider)
	context:LogMeta(context.Name, "Calibrant injection volume", "\181L", "N1", qpcaui_params.calibrant_volume)

	while not execLeft.IsIdle do
		sleep_function()
	end
	status:RemoveStatus(msg)

end

return P
