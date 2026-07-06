-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

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

---@param context InitHelper
function Initialize (context)
	context.Name = "Capillary ID"
	context.Description = "Procedures determine the capillary ID"
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Diagnostics

	context:DeclareParameter("Capillary A length", "180", "mm", "text", "Nominal length", "")
	context:DeclareParameter("Capillary A ID", "10", "µm", "text", "Nominal ID", "")
	context:DeclareParameter("Capillary B length", "180", "mm", "text", "Nominal length", "")
	context:DeclareParameter("Capillary B ID", "10", "µm", "text", "Nominal ID", "")
	context:DeclareParameter("Solvent A", "H2O", "'H2O' or 'ACN'", "text", "Filling of solvent B", "")
	context:DeclareParameter("Solvent B", "H2O", "'H2O' or 'ACN'", "text", "Filling of solvent B", "")
	context:DeclareParameter("Test flow", 0.5, "\181L", "decimal", "Flow for evaluating the ID of the capillary", "")
end

---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (_, context)
-- This function is called when the method is uploaded (Upload method or Acquisition start)
-- All errors are surpressed at the moment
local value = context:GetArgumentValue("Solvent B")
	local msg = "Text must equal {0} or {1}"
	if (value ~= "") and (value ~= "H2O") and (value ~= "ACN") then
		context:Report("Solvent B", Severity.Error, true, msg, "H2O", "ACN")
	end
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
	local chrom     = require "chromatography"
	local pf 		= require "pump_functions"
	---@type Zirconium
	local zr 		= require "zirconium"
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local capALength = tonumber(context:GetArgumentValue("Capillary A length"))
	local capBLength = tonumber(context:GetArgumentValue("Capillary B length"))
	local testFlow = context:GetArgumentValue("Test flow")
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	local function sleep_100()
		context:Sleep(100)
	end
  
	if (context:GetArgumentValue("Solvent A") == "H2O") then
		pf.SetFlowCalibrationFactor(zr.B, 1.0, pump, sleep_100)		-- SolventB: ACN
	else
		pf.SetFlowCalibrationFactor(zr.B, 2.3, pump, sleep_100)		-- SolventB: H2O
	end
	if (context:GetArgumentValue("Solvent B") == "ACN") then
		pf.SetFlowCalibrationFactor(zr.B, 2.3, pump, sleep_100)		-- SolventB: ACN
	else
		pf.SetFlowCalibrationFactor(zr.B, 1.0, pump, sleep_100)		-- SolventB: H2O
	end

	---Calibrate the pressure sensors
	---@return boolean
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
	context:Log("Procedures determine the capillary ID:")
	context:Log("    Nominal capillary A length:   {0} mm", capALength)
	context:Log("    Nominal capillary A ID:       {0} µm", context:GetArgumentValue("Capillary A ID"))
	context:Log("    Nominal capillary B length:   {0} mm", capBLength)
	context:Log("    Nominal capillary B ID:       {0} µm", context:GetArgumentValue("Capillary B ID"))
	context:Log("    Solvent A:                    {0}", context:GetArgumentValue("Solvent A"))
	context:Log("    Solvent B:                    {0}", context:GetArgumentValue("Solvent B"))
	context:Log("    Test flow:                    {0} \181L/min", testFlow)
	context:Log(baltic.devider)

	-- init zirconium channels, abort if unsuccessful
	if not zr.InitChannel(context, pump, zr.A) then
		context:Report(baltic.Naming.PumpA, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end
	if not zr.InitChannel(context, pump, zr.B) then
		context:Report(baltic.Naming.PumpB, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end
--[[
	local function printToFile(stringToBePrinted)
		stringToBePrinted = stringToBePrinted.."\n"
		local f = assert(io.open(fileDirectory.."\\"..fileLocation, "a"))
		context:Sleep(100)
		if not f then
			os.execute("mkdir " .. fileDirectory)
			context:Sleep(100)
			f = assert(io.open(fileDirectory.."\\"..fileLocation, "a"))
		end
		context:Sleep(100)
		context:Sleep(100)
		io.output(f)
		context:Sleep(100)
		io.write(stringToBePrinted)
		context:Sleep(100)
		io.close()
	end
--]]

	local settings = pump:GetSettings()
	local offsetA = settings.ExternalZeroPressureOffsetA
	local offsetB = settings.ExternalZeroPressureOffsetB
	local sensorSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	if not pf.IsPressSensorInRange(offsetA, offsetB, sensorSettings.SensorLimitLow, sensorSettings.SensorLimitHigh) then
		calSensors()
		settings = pump:GetSettings()		-- read updated settings again for later use
	end

	local caller = "PumpProduction"
	zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)
	zr.logInstrSettings(context, settings, caller)

--	zr.ChangePressurePID(context, pump, baltic.PPID.pumpFunction2)
	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)

	-- if there is still pressure from e.g. Idle flow, it is released in setMaxPressureLimitsAB()
	pf.SetMaxPressureLimit(zr.A, pressSettings.GradientPumpCutoffPressure, pump, sleep_100)		-- set max pump A pressure
	pf.SetMaxPressureLimit(zr.B, pressSettings.GradientPumpCutoffPressure, pump, sleep_100)		-- set max pump B pressure

	---Switch degasser on
	local function degasserON()
		local digOut = pump:GetDigitalOutputs()
		if digOut < 32  then	-- 32 == DigitalOutput.PO2 is already true
			pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
			context:Sleep(30*1000)
		end
	end
	---Switch degasser off
	local function degasserOFF()
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
	end

	local function refillPumps()
		if ((pump:GetPistonPosition(zr.A) > baltic.pumpLevel)  or (pump:GetPistonPosition(zr.B) > baltic.pumpLevel))then
			context:Log("Refill pumps A / B")
			if (zr.IsFull(pump, zr.A) and zr.IsFull(pump, zr.B)) then return end -- don't waste valve switch
			degasserON()
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Solvent, nil)
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Solvent, nil)
			pf.Manualmode_Pump_constantSpeed(zr.A, baltic.Settings.GradientPumpRefillSpeed, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, baltic.Settings.GradientPumpRefillSpeed, pump, sleep_100)
			local dictator = LoggingDictator.Prevent(pump)
			while not (zr.IsFull(pump, zr.A) and zr.IsFull(pump, zr.B)) do sleep_100() end
			context:Sleep(2000)
			pf.Manualmode_Pump_constantSpeed(zr.A, 300, pump, sleep_100)
			context:Sleep(2000)
			pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
            context:Sleep(2000)

			dictator:Dispose()
			degasserOFF()
		end
	end

	local function releasePressure()
		local pressA = pump:GetCurrentPressure(zr.A)
		local pressB = pump:GetCurrentPressure(zr.B)
		if ((pressA or pressB) > 10) then
			pf.Manualmode_Pump_constantPressure(zr.A, 5, pump, sleep_100)
			pf.Manualmode_Pump_constantPressure(zr.B, 5, pump, sleep_100)
			local dictator = LoggingDictator.Prevent(pump)
			while ((pressA or pressB) > 10) do
				pressA = pump:GetCurrentPressure(zr.A)
				pressB = pump:GetCurrentPressure(zr.B)
				context:Sleep(1000)
			end
			dictator:Dispose()
			pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
		end
	end

	local function determineCapillaryIDs()
		local timeOut = pf.now() + 60
		local viscosityA = chrom.viscosity_H2O_20C
		if (context:GetArgumentValue("Solvent A") == "ACN") then viscosityA = chrom.viscosity_ACN_20C end
		local viscosityB = chrom.viscosity_H2O_20C
		if (context:GetArgumentValue("Solvent B") == "ACN") then viscosityB = chrom.viscosity_ACN_20C end

		local pressValveAToFlowA = chrom.capillary_pressure(installed, baltic.Naming.ValveAToFS, testFlow, viscosityA, false)
		local pressValveBToFlowB = chrom.capillary_pressure(installed, baltic.Naming.ValveBToFS, testFlow, viscosityB, false)

		local flowA = 0
		local flowB = 0
		local i = 0
		local n = 0
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee, nil)
		zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee, nil)
		context:Sleep(500)
		pf.Manualmode_Pump_constantFlow_binary(context, testFlow, testFlow, pump, zr, sleep_100)
		local dictator = LoggingDictator.Prevent(pump)
		while ((pump:GetSetFlow(zr.A) <= testFlow * 0.99) and (pump:GetSetFlow(zr.B) <= testFlow * 0.99) or (pf.now() > timeOut)) do sleep_100() end
		if (timeOut < pf.now()) then
			context:Report("Flow", Severity.Warn, true, "not reached. Check for a leakage or air in the pump")
			return
		end

		while (i < 600) do
			i = i + 1
			if (i > 300) then
				n = n + 1
				flowA = flowA + pump:GetSetFlow(zr.A)
				flowB = flowB + pump:GetSetFlow(zr.B)
			end	-- get piston position after 30 seconds
			sleep_100()
		end
		dictator:Dispose()
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
		flowA = flowA / n	-- average flow after 30 seconds == µL/min
		flowB = flowB / n	-- average flow after 30 seconds == µL/min
		local capPressureA = pump:GetCurrentPressure(zr.A) - pressValveAToFlowA
		local capPressureB = pump:GetCurrentPressure(zr.B) - pressValveBToFlowB
--[[
(pressure*r_cm^4*math.pi)/(8*viscosity*length_mm/10)*60000000000 = usedVolumeA
r_cm^4 = usedVolumeA*(8*viscosity*length_mm/10)/60000000000/pressure/math.pi
ID_µm = SQRT(SQRT((usedVolume*8*viscosity*length_mm/10)/(60000000000*pressure*pi)))*2*10000
--]]
		local ID_A_cm = math.sqrt(math.sqrt((flowA*8*viscosityA*capALength/10)/(60000000000*capPressureA*math.pi)))*2
		local ID_B_cm = math.sqrt(math.sqrt((flowB*8*viscosityB*capBLength/10)/(60000000000*capPressureB*math.pi)))*2
		local ID_A = pf.noExp(ID_A_cm*10000,2)
		local ID_B = pf.noExp(ID_B_cm*10000,2)
		local msgAB = DotNetString.Format("A: {0} \181m;  B: {1} \181m", pf.noExp(ID_A,2), pf.noExp(ID_B,2))
		context:Report("Capillary IDs:", Severity.Info, true, msgAB)
	end

	refillPumps()
	determineCapillaryIDs()
	releasePressure()

	dictator:Dispose()

	-- pumps are stoped in 'releasePressure()' in 'directFlow()'
	pf.isPumpIdle(pump, sleep_100)
end
