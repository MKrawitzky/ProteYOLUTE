local Date = "2025/04/10"

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---@param context InitHelper
function Initialize (context)
	context.Name = "Valve stability"
	context.Description = "Procedure for testing the valve stability."
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Diagnostics

	local flow = 0.3
	local cycles = 500
	local switchTime = 20

	context:DeclareParameter("Flow rate", flow, "\181L/min", "decimal", "Target flow", "")
	context:DeclareParameter("Sum of valve switches", cycles, "num", "integer", "Target valve switches", "")
	context:DeclareParameter("Time between switches", switchTime, "sec", "integer", "Time between valve switches", "")
	context:DeclareParameter("All valves", true, nil, "boolean", "Switch all valves", "")
	context:DeclareParameter("Pump valves", false, nil, "boolean", "Switch pump valves only", "")
	context:DeclareParameter("Injector valve", false, nil, "boolean", "Switch injector valve only", "")
	context:DeclareParameter("Trap valve", false, nil, "boolean", "Switch trap valve only", "")
end

---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (_, context)
	local validation 	= require "validation"

	validation.verify_specified(context, "Flow rate")
	validation.verify_specified(context, "Sum of valve switches")
	validation.verify_range(context, "Flow rate", 0.1, 2)
	validation.verify_range(context, "Sum of valve switches", 0, 10000)
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)

	context:Log("Lua date: {0}", Date)

	require "degas"

	local baltic 	 = require "baltic"
	local parallel = require "parallel"
	local pf 		 = require "pump_functions"
	local pp 		 = require "palplus"
	local pr		 = require "PreRunFunctions"
	---@type Zirconium
	local zr 		 = require "zirconium"

	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local valveI = pp.QueryValveDrive(execLeft, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execLeft, pp.Capabilities.ISelectorValve)

	--	Signalize_Reset
	--  context:Signalize(baltic.ColorsRGB.Normal, baltic.SignalizeAll)

	context:Log("Lua date:		 {0}", Date)
	context:Log("--- Experiment:   {0}", context.Description)
	--    ci.logColumnInformation(context, trap, sep)
	context:Log("'{0}' is '{1}'", valveI.Name, pp.Capabilities.ILcInjectorValve)
	context:Log("'{0}' is '{1}'", valveT.Name, pp.Capabilities.ISelectorValve)

	pr.iniFlowResistance(context, pump)

	local function sleep_100()
		context:Sleep(100)
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

	-- read the current column oven temperature
	local ovenTemp = pump:GetCurrentExternalTemperature()
	context:Log("--- Oven temperature: {0}", ovenTemp)
	if (ovenTemp < 1) then
		ovenTemp = 20
		context:Log("--- internal variable ovenTemp: {0}", ovenTemp)
	end

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

    -- start Main here
	local all = context:GetArgumentValue("All valves")
	local AB = context:GetArgumentValue("Pump valves")
	local I  = context:GetArgumentValue("Injector valve")
	local T  = context:GetArgumentValue("Trap valve")
	local flow_rate = context:GetArgumentValue("Flow rate")

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

	---Switch desired valves as specified in the initialize function
	local function switchValves()
		if all then
			set_valves(baltic.PumpValve.MixTee, baltic.PumpValve.MixTee, baltic.InjectionValve.Load, baltic.TrapValve.GradientT)
			pf.Manualmode_Pump_constantSpeed(zr.A, flow_rate, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, flow_rate, pump, sleep_100)
		else
			if AB then
				set_valves(baltic.PumpValve.MixTee, baltic.PumpValve.MixTee, nil, baltic.TrapValve.Waste)
				pf.Manualmode_Pump_constantSpeed(zr.A, flow_rate, pump, sleep_100)
				pf.Manualmode_Pump_constantSpeed(zr.B, flow_rate, pump, sleep_100)
			else
				if I then
					set_valves(baltic.PumpValve.Inject, nil, baltic.InjectionValve.Load, baltic.TrapValve.InjectWaste)
					pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
					pf.Manualmode_Pump_constantSpeed(zr.A, flow_rate, pump, sleep_100)
				else
					if T then
						set_valves(nil, baltic.PumpValve.MixTee, nil, baltic.TrapValve.GradientT)
						pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
						pf.Manualmode_Pump_constantSpeed(zr.B, flow_rate, pump, sleep_100)
					end
				end
			end
		end
	end

	---Switch the desired valves back and set a flow rate
	local function switchValvesBack()
		if all then
			set_valves(baltic.PumpValve.Inject, baltic.PumpValve.Waste, baltic.InjectionValve.Inject, baltic.TrapValve.InjectWaste)
			pf.Manualmode_Pump_constantSpeed(zr.A, flow_rate, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, flow_rate, pump, sleep_100)
		else
			if AB then
				set_valves(baltic.PumpValve.Waste, baltic.PumpValve.Waste, nil, nil)
				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste, nil)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste, nil)
				pf.Manualmode_Pump_constantSpeed(zr.A, flow_rate, pump, sleep_100)
				pf.Manualmode_Pump_constantSpeed(zr.B, flow_rate, pump, sleep_100)
			else
				if I then
					set_valves(baltic.PumpValve.Inject, nil, baltic.InjectionValve.Inject, baltic.TrapValve.InjectWaste)
					pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
					pf.Manualmode_Pump_constantSpeed(zr.A, flow_rate, pump, sleep_100)
				else
					if T then
						set_valves(nil, baltic.PumpValve.MixTee, nil, baltic.TrapValve.GradientA)
						pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
						execAux:MoveValveDrive(valveT, pp.Quantity(baltic.TrapValve.GradientA, "deg"))
						zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee, nil)
						pf.Manualmode_Pump_constantSpeed(zr.B, flow_rate, pump, sleep_100)
					end
				end
			end
		end
	end

	local t = context:GetArgumentValue("Time between switches")*1000
	local msg = "Testing valves..."
    status:SetStatus(msg)
    local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
    -- bail if a pump failed degassing..
    if (not (a and b)) then
		pr.decompressSystem(context)
        context:Abort()
    end
	local cnt = 0
	local n = context:GetArgumentValue("Sum of valve switches")
    while not (zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B)) do
		switchValves()
		context:Sleep(t)
		switchValvesBack()
		context:Sleep(t)
		cnt = cnt + 2
		if cnt >= n then break end
    end
    status:RemoveStatus(msg)
	pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
	pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

	dictator:Dispose()

	zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Service, nil)
	zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Service, nil)
	pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Service)
	pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Service)

    -- wait for pump to stop
	pf.isPumpIdle(pump, sleep_100)
end
