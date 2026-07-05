local Date = "2025/07/23"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

local baltic = require "baltic"
require "degas"

-- don't change pumpOnly, it will be set by the application key --
local pumpOnly = false

---@param context InitHelper
function Initialize (context)
	context.Name = "MagicMix"
	context.Description = "Flushing the LC system with magic mix"
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Diagnostics

	if context.AppKey.Special == baltic.ProductionPumpOnly then pumpOnly = true end
	if pumpOnly == false then
		context:DeclareParameter("Include solvent replacement", false, nil, "boolean", "Several times refilling pumps and cleaning syringe and capillaries", "")
		context:DeclareParameter("Include column preparation", false, nil, "boolean", "Trap and Separation column equilibration", "")
	end
-- 	Special = {StdDiag, OQDiag, FactoryDiag, ServiceDiag, Production, ProductionPumpOnly}
	if context.AppKey.Special == baltic.Production or context.AppKey.Special == baltic.FactoryDiag or pumpOnly == true then 
		context:DeclareParameter("Cleaning pumps", false, nil, "boolean", "Procedure to clean the pumps", "")
		if pumpOnly == false then
			context:DeclareParameter("Cleaning autosampler", false, nil, "boolean", "Procedure to clean the autosampler", "")
		end
		context:DeclareParameter("Cleaning pump repetition",75, "x", "integer", "Number of pump cleaning repetitions", "")
	end
end

---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (_, context)
	local validation = 	require "validation"

	-- only require column types to be specified, if column prep is true (otherwise auto-prep may fail).
	if pumpOnly == false then
		if (context:GetArgumentValue("Include column preparation") or context:GetArgumentValue("Inject mass calibrant")) then
			validation.verify_specified(context, "trap")
			validation.verify_specified(context, "separator")
		end
		validation.verify_specified(context, "Include solvent replacement")
		validation.verify_specified(context, "Include column preparation")
	end
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)

	local parallel 	= require "parallel"
	local pf 		= require "pump_functions"
	local pp 		= require "palplus"
	local pr 		= require "PreRunFunctions"
	---@type Zirconium
	local zr 		= require "zirconium"
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local valveI = pp.QueryValveDrive(execLeft, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execLeft, pp.Capabilities.ISelectorValve)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	local N = baltic.Naming

	-- read the current column oven temperature
	local ovenTemp = 20
	context:Log("--- Oven temperature: {0}", ovenTemp)

	local function sleep_100()
		context:Sleep(100)
	end

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

	local function calSensors()
		context:Sleep(2000)
		status:SetStatus("Pressure Sensor Calibration")
		context:Log(baltic.devider)
		context:Log("--- Pressure Sensor Offset Calibration:")

		local _, _, passed = pf.calibrate_press_sensors(installed, context, pump, zr, baltic.PumpValve.Waste)
		if not passed then
			context:Abort()
		end
		context:Log(baltic.devider)
		status:RemoveStatus("Pressure Sensor Calibration")
		return true
	end

	context:Log(baltic.devider)
	context:Log("Lua date:             {0}", Date)
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

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)
	context:Log(baltic.devider)

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

	local is_calibrated = false
	local settings = pump:GetSettings()
	local offsetA = settings.ExternalZeroPressureOffsetA
	local offsetB = settings.ExternalZeroPressureOffsetB
	local sensorSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	if not pf.IsPressSensorInRange(offsetA, offsetB, sensorSettings.SensorLimitLow, sensorSettings.SensorLimitHigh) then
		is_calibrated = calSensors()
		settings = pump:GetSettings()		-- read updated settings again for later use
	end

	local caller = "MagicMix"
	zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)
	zr.logInstrSettings(context, settings, caller)

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

	-- if there is still pressure from e.g. Idle flow, it is released in setMaxPressureLimitsAB()
	pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false)		-- set max pump pressure

	local cleaning = context:GetArgumentValue("Cleaning pumps")
	if pumpOnly == false then
		cleaning = cleaning or context:GetArgumentValue("Cleaning autosampler")
	end
	if not is_calibrated then
		calSensors()
	end
	if cleaning then
		if context:GetArgumentValue("Cleaning pumps") then
			-- cleaning procedure starts here
			local function cleaningProcedure()
				status:SetStatus("Cleaning pumps")
				local function degasserON()
					local digOut = pump:GetDigitalOutputs()
					if digOut < 32  then	-- 32 == DigitalOutput.PO2 is already true
						pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
						context:Sleep(30*1000)
					end
				end
				local function degasserOFF()
					pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
				end
				local function emptyPumps()
					local function empty_pump(channel, yield_func)
							context:Log("Purge {0}", channel)
							if zr.IsEmpty(pump, channel) then return end -- don't waste valve switch
							zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Waste, nil)
							pf.Manualmode_Pump_constantSpeed(channel, 2000, pump, yield_func)
							while not zr.IsEmpty(pump, channel) do yield_func() end
					end
					local p_empty_a = { empty_pump, zr.A, parallel.yield }
					local p_empty_b = { empty_pump, zr.B, parallel.yield }
					parallel.run(sleep_100, p_empty_a, p_empty_b)
				end
				local function refillPumps()
					local function refillPump(channel, yield_func)
						context:Log("Refill {0}", channel)
						if zr.IsFull(pump, channel) then return end -- don't waste valve switch
						zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent, nil)
						pf.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpRefillSpeed, pump, yield_func)
						while not zr.IsFull(pump, channel) do yield_func() end
						parallel.sleep(yield_func, 2000)
						pf.Manualmode_Pump_constantSpeed(channel, 300, pump, yield_func)
						parallel.sleep(yield_func, 2000)
						pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_func)
						parallel.sleep(yield_func, 4000)
					end
					local p_refill_a = { refillPump, zr.A, parallel.yield }
					local p_refill_b = { refillPump, zr.B, parallel.yield }
					parallel.run(sleep_100, p_refill_a, p_refill_b)
				end

				degasserON()
				local repetitions = context:GetArgumentValue("Cleaning pump repetition")
				local cnt=0
				while cnt<repetitions do
					local msg = DotNetString.Format("{0}/{1}", (cnt+1),repetitions)
					status:SetStatus(msg)
					for i=1, 2 do
						emptyPumps()
						refillPumps()
					end
					if (repetitions-cnt)>0 then context:Sleep(30*60*1000) end			-- wait 30 minutes
					cnt = cnt+1
					status:RemoveStatus(msg)
				end
				status:RemoveStatus("Cleaning pumps")
				degasserOFF()
			end

			cleaningProcedure()		-- cleaning pumps and pump valves
		end
		if pumpOnly == false then
			if context:GetArgumentValue("Cleaning autosampler") then
				local function valveI_cleaning()
					status:SetStatus("Cleaning injection valve")
					set_valves(baltic.PumpValve.Inject, baltic.PumpValve.Waste, baltic.InjectionValve.Load, baltic.TrapValve.InjectWaste)
					pf.Manualmode_Pump_constantSpeed(zr.A, 1.0, pump, sleep_100)
					local n = 25			-- == 50 switches
					for i=1,n do
						local msg = DotNetString.Format("{0}/{1}", i,n)
						status:SetStatus(msg)
						context:Sleep(60*1000)
						set_valves(nil, nil, baltic.InjectionValve.Inject, nil)
						context:Sleep(60*1000)
						set_valves(nil, nil, baltic.InjectionValve.Load, nil)
						status:RemoveStatus(msg)
					end
					pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
					status:RemoveStatus("Cleaning injection valve")
				end
				local function valveT_cleaning()
					status:SetStatus("Cleaning trap valve")
					set_valves(baltic.PumpValve.MixTee, baltic.PumpValve.MixTee, nil, baltic.TrapValve.Waste)
					pf.Manualmode_Pump_constantFlow_binary(context, 0.5, 0.5, pump, zr, sleep_100)
					local n = 25			-- == 50 switches
					for i=1,n do
						local msg = DotNetString.Format("{0}/{1}", i,n)
						status:SetStatus(msg)
						context:Sleep(60*1000)
						set_valves(nil, nil, nil, baltic.TrapValve.GradientA)
						context:Sleep(60*1000)
						set_valves(nil, nil, nil, baltic.TrapValve.Waste)
						status:RemoveStatus(msg)
					end
					pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
					pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
					status:RemoveStatus("Cleaning trap valve")
				end

				valveI_cleaning()		-- cleaning injection valve
				valveT_cleaning()		-- cleaning trap valve
			end
		end
	end
	pr.Signalize_Reset(context)
	pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 300, sleep_100, baltic.smooth)	-- PBNE-607
	-- pumps are stoped in 'reducePressure'
	pf.isPumpIdle(pump, sleep_100)
	context:ShowComposition(true)

	dictator:Dispose()

	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)

	while not execLeft.IsIdle do
		sleep_100()
	end
end
