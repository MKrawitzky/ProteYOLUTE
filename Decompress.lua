-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/11/12"

luanet.load_assembly("Bruker.Lc")

---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---@param context InitHelper
function Initialize (context)
    -- This function is called to cleanup after other procedures after they are completed or aborted. 
	context.Name = "Decompress"
	context.Description = "Procedure for letting out pump pressure"
	context.Hidden = true
	context.DecompressOnExit = false
	context.OverwriteLogFiles = true
	context.NumberOfLogFilesToKeep = 5
	context.LedState = LedState.Decompress
end

---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (installed, context)
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)

	local baltic = require "baltic"
	local pf = require "pump_functions"
	local pr = require "PreRunFunctions"
	---@type Zirconium
	local zr = require "zirconium"
	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)

	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end

	local fR = require "PreRunFunctions"
	fR.iniFlowResistance(context, pump)

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

	context:Log("Decompress function called: {0}", os.date())
	context:Log("Lua date:             {0}", Date)

	-- reset the progress indication and show activity
	status:Standby()
	status:SetStatus("Decompressing...")

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)

	--	Signalize_Reset
	pr.Signalize_Reset(context)
	context:Log("Signal reset")
	context:ShowComposition(true)
	context:Sleep(250)

	local settings = pump:GetSettings()
	local offsetA = settings.ExternalZeroPressureOffsetA
	local offsetB = settings.ExternalZeroPressureOffsetB
	local sensorSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	if not pf.IsPressSensorInRange(offsetA, offsetB, sensorSettings.SensorLimitLow, sensorSettings.SensorLimitHigh) then
		local DotNetString = luanet.import_type("System.String")
		local msg1 = DotNetString.Format("Pressure sensor out of range ( {0:#0} - {1:#0})", sensorSettings.SensorLimitLow, sensorSettings.SensorLimitHigh)
		local msg2 = DotNetString.Format("Sensor A: {0:#0.0}, Sensor B: {1:#0.0}.\n\nThe pressure sensor calibration values are outside the expected range.\nRun 'Preparation' to recalibrate the sensors.", offsetA, offsetB)
		context:Report(msg1, Severity.Info, true, msg2)
		context:Sleep(1000)
	end

	-- *************** Standby code ***************
	context:Log("Starting Standby in Decompress.lua file")

	-- Standby code for pumps
	pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
	zr.abortGradient(pump)
	context:Log("--- Set method signal 'abort'")
	context:Sleep(200)
	local i=0
	local n=0
	while zr.IsGradientRunning(pump) do
		sleep_100()
		i=i+1
		if i>10 then
			i=0
			n=n+1
			zr.abortGradient(pump)
			context:Log("Abort gradient method - attempt {0}",n)
		end
		if n>10 then
			context:Log("Abort gradient method failed after {0} attempts",n-1)
			status:RemoveStatus("Decompressing...")
			context:Abort()
		end
	end

	-- channel A
	context:Log("--- Stopping channel A ---")
	local state = pump:GetPumpSideState(zr.A)

	if state == zr.PumpSideState.ManualMode then
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
	elseif (state == zr.PumpSideState.Aspirating or
			state == zr.PumpSideState.Compressing or
			state == zr.PumpSideState.PumpingBinaryFlow or
			state == zr.PumpSideState.PumpingBinaryPressure or
			state == zr.PumpSideState.PumpingIsocraticFlow or
			state == zr.PumpSideState.PumpingIsocraticPressure or
			state == zr.PumpSideState.Purging or
			state == zr.PumpSideState.Releasing) then
		pump:SetPumpSideSignal(zr.A, zr.PumpsideSignal.Stop)
	end

	-- channel B
	context:Log("--- Stopping channel B ---")
	state = pump:GetPumpSideState(zr.B)

	if state == zr.PumpSideState.ManualMode then
		pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
	elseif (state == zr.PumpSideState.Aspirating or
			state == zr.PumpSideState.Compressing or
			state == zr.PumpSideState.PumpingBinaryFlow or
			state == zr.PumpSideState.PumpingBinaryPressure or
			state == zr.PumpSideState.PumpingIsocraticFlow or
			state == zr.PumpSideState.PumpingIsocraticPressure or
			state == zr.PumpSideState.Purging or
			state == zr.PumpSideState.Releasing) then
		pump:SetPumpSideSignal(zr.B, zr.PumpsideSignal.Stop)
	end


	-- decompress the system to prvent backflow from one channel to the other
	-- only if both pump valves in mixTee position
	local timeout = 30
	local press = 20
	local t = pf.now() + timeout

	if ((pump:GetSetManualValvePosition(zr.A) == baltic.PumpValve.MixTee) or (pump:GetSetManualValvePosition(zr.A) == baltic.PumpValve.Compress)) and (pump:GetSetManualValvePosition(zr.B) == baltic.PumpValve.MixTee) then
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)
		if (pump:GetCurrentPressure(zr.A) > (press-2)) or (pump:GetCurrentPressure(zr.B) > (press-2)) then
			context:Log("Decompress channels")
			if (pump:GetSetManualValvePosition(zr.A) == baltic.PumpValve.MixTee) then
				pump:Manualmode_Pump_constantFlow_binary(0, 0)
			else
				if (pump:GetCurrentPressure(zr.A) > (press-2)) then
					pump:Manualmode_Pump_constantPressure(zr.A, press-10)
				end
				pump:Manualmode_Pump_constantPressure(zr.B, press-10)
			end
		end
		while ((pump:GetCurrentPressure(zr.A) > press) or (pump:GetCurrentPressure(zr.B) > press)) and (pf.now() <= t) do
			sleep_100()
		end
	end

	if (pf.now() > t) then
		context:Log("Channels not decompressed within {0} sec. Pump pistons moving back. There might enter solvent from one channel to the other. A degas procedure will follow this pressure reduction", timeout)
		pump:Manualmode_Pump_constantPressure(zr.A, press-10)
		pump:Manualmode_Pump_constantPressure(zr.B, press-10)
		t = pf.now() + timeout
		while ((pump:GetCurrentPressure(zr.A) > press) or (pump:GetCurrentPressure(zr.B) > press)) and (pf.now() <= t) do
			sleep_100()
		end
		if (pf.now() > t) then
			context:Log("Channels not decompressed within {0} sec. Pump valves switched to waste to relieve pressure.", timeout*2)
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste)
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste)
		end
		context:Report("Decompress", Severity.Info, true, "It is possible that solvent has leaked from one channel to the other. Please run 'Preparation' to refill the pumps with pure solvents.")
	else
		context:Log("Channels decompressed")
	end

	pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
	pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

	-- Don't leave the pump valves on compress120 (the solvent bottle content will flow thrue the valve to waste and empty it)
	context:Log("Set pump valves to 'Waste' if actual position is 'Compress120'")
	if pump:GetSetManualValvePosition(zr.A) == baltic.PumpValve.Compress120 then zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste) end
	if pump:GetSetManualValvePosition(zr.B) == baltic.PumpValve.Compress120 then zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste) end

	-- Standby code for autosampler
	execLeft:Standby()

	while not execLeft.IsIdle do
		sleep_100()
	end

	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)

	status:RemoveStatus("Decompressing...")

	dictator:Dispose()

	context:Log("Exit procedure")
end
