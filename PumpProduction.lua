-- require('lldebugger').start()

local Date = "2025/09/02"

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
	context.Name = "Pump Production"
	context.Description = "Procedures for cleaning the pumps"
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Diagnostics

	context:DeclareParameter("Pump serial number", "?", "num", "text", "Serial number of the pump", "")
	context:DeclareParameter("Run time", 66, "hours", "integer", "Minimum time until the last step ends", "")
	context:DeclareParameter("Cycle time", 6, "hours", "integer", "Time for one cycle inclusive waiting", "")
	context:DeclareParameter("Solvent replacements", 4, "x", "integer", "Number of pump cleaning repetitions per cycle", "")
	context:DeclareParameter("Aspiration speed", 300, "\181L/min", "integer", "Aspiration speed for solvent replacement", "")
	context:DeclareParameter("Dispense speed", 4000, "\181L/min", "integer", "Dispense speed for solvent replacement", "")
	context:DeclareParameter("Direct flow cycles", 2, "x", "integer", "Number of direct flow cycles", "")
	context:DeclareParameter("Direct flow max. pressure", 200, "bar", "integer", "Maximum pressure for Direct flow", "")
	context:DeclareParameter("Capillary cleaning flow", 50, "\181L/min", "integer", "Pump speed to flush the injection capillary", "")
	context:DeclareParameter("Capillary cleaning volume", 500, "\181L", "integer", "The injection capillary is rinsed with this volume", "")
	context:DeclareParameter("Capillary cleaning max. pressure", 200, "bar", "integer", "Maximum pressure for Capillary cleaning", "")
end

---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (installed, context)
-- This function is called when the method is uploaded (Upload method or Acquisition start)
-- All errors are surpressed at the moment
	local pf = require "pump_functions"
	local v  = require "validation"

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	v.verify_range(context, "Aspiration speed", 1, 4000)
	v.verify_range(context, "Dispense speed", 1, 4000)
	v.verify_range(context, "Direct flow max. pressure", 1, pressSettings.GradientPumpMaxTargetPressure)
	v.verify_range(context, "Capillary cleaning volume", 1, 1300)
	v.verify_range(context, "Capillary cleaning max. pressure", 1, pressSettings.GradientPumpMaxTargetPressure)
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)

	local parallel 	= require "parallel"
	local pf 		= require "pump_functions"
	---@type Zirconium
	local zr 		= require "zirconium"
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local pumpNumber = context:GetArgumentValue("Pump serial number")
	local runTime = pf.now() + (context:GetArgumentValue("Run time")*3600)				-- [sec]
	local cycleTime = context:GetArgumentValue("Cycle time")*3600						-- [sec]
	local solventRepetitions = context:GetArgumentValue("Solvent replacements")
	local aspirationSpeed = -context:GetArgumentValue("Aspiration speed")
	local dispenseSpeed = context:GetArgumentValue("Dispense speed")
	local directFlowCycles = context:GetArgumentValue("Direct flow cycles")
	local directFlowPressure = context:GetArgumentValue("Direct flow max. pressure")
	local capCleaningVolume = context:GetArgumentValue("Capillary cleaning volume")		-- [µL]
	local capCleaningSpeed = context:GetArgumentValue("Capillary cleaning flow")		-- µL/min
	local capCleaningPressure = context:GetArgumentValue("Capillary cleaning max. pressure")
	local osDate = os.date("%Y%m%d")
	local osTime = os.date("%H%M%S")
	local fileDirectory = "PumpProductionTestLogs"
	local fileLocation = DotNetString.Format("{0}_{1}_{2}.csv", osDate, osTime, pumpNumber)


	local function sleep_100()
		context:Sleep(100)
	end
	local function sleep_1000()
		context:Sleep(1000)
	end

	context:Log(baltic.devider)
	context:Log("Lua date:             {0}", Date)
	context:Log(baltic.devider)
	context:Log("Production pump cleaning parameter:")
	context:Log("    Pump serial number:               {0}", pumpNumber)
	context:Log("    Run time:                         {0} hours", context:GetArgumentValue("Run time"))
	context:Log("    Solvent replacements / cycle:     {0} cycles", solventRepetitions)
	context:Log("        Aspiration speed:             {0} \181L/min", aspirationSpeed)
	context:Log("        Dispense speed:               {0} \181L/min", dispenseSpeed)
	context:Log("    Direct flow cycles:               {0} cycles", directFlowCycles)
	context:Log("    Direct flow max. pressure:        {0} bar", directFlowPressure)
	context:Log("    Capillary cleaning flow:          {0} \181L/min", capCleaningSpeed)
	context:Log("    Capillary cleaning volume:        {0} \181L", capCleaningVolume)
	context:Log("    Capillary cleaning max. pressure: {0} bar", capCleaningPressure)
	context:Log(baltic.devider)

	-- init zirconium channels, abort if unsuccessful
	if not zr.InitChannel(context, pump, zr.A) then
		context:Report(baltic.Naming.PumpA, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode.

Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end
	if not zr.InitChannel(context, pump, zr.B) then
		context:Report(baltic.Naming.PumpB, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode.

Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end

	---Print a string in a file
	---@param stringToBePrinted string
	local function printToFile(stringToBePrinted)
		stringToBePrinted = stringToBePrinted.."\n"
		local f = io.open(fileDirectory.."\\"..fileLocation, "a")
		context:Sleep(100)
		if not f then
			os.execute("mkdir " .. fileDirectory)
			context:Sleep(1000)
			f = io.open(fileDirectory.."\\"..fileLocation, "a")
		end
		context:Sleep(100)
		if f then
			io.output(f)
			context:Sleep(100)
			io.write(stringToBePrinted)
			context:Sleep(100)
		end
		io.close()
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

	-- calibrate pressure sensors first before starting the procedures
	calSensors()

	local caller = "PumpProduction"
	local settings = pump:GetSettings()
	zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

	-- if there is still pressure from e.g. Idle flow, it is released in setMaxPressureLimitsAB()
	pf.SetMaxPressureLimit(zr.A, 1020, pump, sleep_100)		-- set max pump A pressure
	pf.SetMaxPressureLimit(zr.B, 1020, pump, sleep_100)		-- set max pump B pressure

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

	---Refill pump A and B
	local function refillPumps()
		local function refillPump(channel, yield_func)
			context:Log("Refill {0}", channel)
			if zr.IsFull(pump, channel) then return end -- don't waste valve switch
			zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent, nil)
			yield_func()
			pf.Manualmode_Pump_constantSpeed(channel, aspirationSpeed, pump, yield_func)
			while not zr.IsFull(pump, channel) do yield_func() end
		end
		degasserON()
		local p_refill_a = { refillPump, zr.A, parallel.yield }
		local p_refill_b = { refillPump, zr.B, parallel.yield }
		parallel.run(sleep_100, p_refill_a, p_refill_b)
	end

	---Release the pressure and switch the pumps off
	---@param chA	boolean
	---@param chB	boolean
	local function releasePressure(chA, chB)
		local pressA = pump:GetCurrentPressure(zr.A)
		local pressB = pump:GetCurrentPressure(zr.B)
		sleep_1000()
		if ((pressA > 10) or (pressB > 10)) then
			if chA then pf.Manualmode_Pump_constantPressure(zr.A, 5, pump, sleep_100) end
			if chB then pf.Manualmode_Pump_constantPressure(zr.B, 5, pump, sleep_100) end
			local timeOut = pf.now() + 300			-- 5 minutes time out
			while ((pressA > 10) or (pressB > 10)) and (timeOut > pf.now()) do
				if chA then
					pressA = pump:GetCurrentPressure(zr.A)
				else
					pressA = 1
				end
				if chB then
					pressB = pump:GetCurrentPressure(zr.B)
				else
					pressB = 1
				end
				sleep_1000()
			end
			pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
		end
	end

	---Exchange the solvent
	local function solventExchange()
		local function emptyPumps()
			local function empty_pump(channel, yield_func)
				context:Log("Purge {0}", channel)
				if zr.IsEmpty(pump, channel) then return end -- don't waste valve switch
				zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Waste, nil)
				yield_func()
				pf.Manualmode_Pump_constantSpeed(channel, dispenseSpeed, pump, yield_func)
				while (zr.IsEmpty(pump, channel) == false) do yield_func() end
			end
			local p_empty_a = { empty_pump, zr.A, parallel.yield }
			local p_empty_b = { empty_pump, zr.B, parallel.yield }
			parallel.run(sleep_100, p_empty_a, p_empty_b)
		end

		local cnt=0
		status:SetStatus("Solvent replacement")
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)		-- degasser on without waiting
		while (cnt<solventRepetitions) do
			cnt = cnt+1
			local msg = DotNetString.Format("{0}/{1}", cnt,solventRepetitions)
			status:SetStatus(msg)
			emptyPumps()
			refillPumps()
			if (cnt<solventRepetitions) then context:Sleep(10*1000) end			-- wait 10 seconds
			status:RemoveStatus(msg)
		end
		status:RemoveStatus("Solvent replacement")
	end

	---Run direct flow n times
	---@param stopTime integer	-- timeout [sec]
	local function directFlow_n_times(stopTime)
		local function runDirectFlow(directFlowTimeOut)
			local setPressA = true
			local setPressB = true
			local pA, pB = 0, 0
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee, nil)
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee, nil)
			sleep_1000()
			pf.Manualmode_Pump_constantSpeed(zr.A, 200, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, 200, pump, sleep_100)
			local isPumpA_empty = zr.IsEmpty(pump, zr.A)
			local isPumpB_empty = zr.IsEmpty(pump, zr.B)
			sleep_1000()
			while ((isPumpA_empty == false) and (isPumpB_empty == false)) and (directFlowTimeOut >= pf.now()) do
				if ((pA > directFlowPressure) and setPressA) then
					pf.Manualmode_Pump_constantPressure(zr.A, directFlowPressure, pump, sleep_100)
					setPressA = false
				end
				if ((pB > directFlowPressure) and setPressB) then
					pf.Manualmode_Pump_constantPressure(zr.B,directFlowPressure, pump, sleep_100)
					setPressB = false
				end
				isPumpA_empty = zr.IsEmpty(pump, zr.A)
				isPumpB_empty = zr.IsEmpty(pump, zr.B)
				sleep_1000()
				if (setPressA) then pA = pump:GetCurrentPressure(zr.A) end
				if (setPressB) then pB = pump:GetCurrentPressure(zr.B) end
			end
		end

		zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)
		local msg1 =  "Direct flow @ "..directFlowPressure.." bar"
		status:SetStatus(msg1)
		stopTime = (stopTime / directFlowCycles) - 360		-- 360 == releasePressure + refillPumps
		local i = 0
		while (i < directFlowCycles) do
			i = i + 1
			local msg2 = tostring(i).."/"..tostring(directFlowCycles)
			status:SetStatus(msg2)
			local directFlowTimeOut = stopTime + pf.now()
			runDirectFlow(directFlowTimeOut)
			releasePressure(true, true)		-- 1 minute
			refillPumps()			-- 5 minutes
			status:RemoveStatus(msg2)
		end
		degasserOFF()
		pump:SetSettings(settings)
		status:RemoveStatus(msg1)
	end

	---Clean the injection capillary
	local function rinseInjectionCapillary_new()
		local setPressA = true
		local pA = 0
		local pumpVolume = pump:GetPistonPosition(zr.A)
		local remainingVolume = capCleaningVolume
		local msg3 = "remaining volume: "..pf.noExp(remainingVolume,1).." \181L"
		capCleaningVolume = capCleaningVolume + pumpVolume
		status:SetStatus("Rinsing injection capillary")
		-- do not log the status, only show it in LC-Control
		local statusDictator = LoggingDictator.Prevent(status)
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject, nil)
		pf.Manualmode_Pump_constantSpeed(zr.A, capCleaningSpeed, pump, sleep_100)
		while (pumpVolume < capCleaningVolume) do 
			if ((pA > capCleaningPressure) and setPressA) then
				pf.Manualmode_Pump_constantPressure(zr.A, capCleaningPressure, pump, sleep_100)
				setPressA = false
			end
			status:SetStatus(msg3)
			sleep_1000()
			pumpVolume = pump:GetPistonPosition(zr.A)
			status:RemoveStatus(msg3)
			remainingVolume = capCleaningVolume-pumpVolume
			msg3 = "remaining volume: "..pf.noExp(remainingVolume,1).." \181L"
			if (setPressA) then pA = pump:GetCurrentPressure(zr.A) end
		end
		statusDictator:Dispose()
		status:RemoveStatus("Rinsing injection capillary")
	end

	local msg = DotNetString.Format("Pump Serial Number: {0}, Minimum Runtime: {1} Hours, Solvent Replacement Cycles: {2}, Aspiration Speed: {3}, Dispense Speed: {4}, Direct Flow max. Pressure {5} bar", pumpNumber, context:GetArgumentValue("Run time"), solventRepetitions, aspirationSpeed, dispenseSpeed, directFlowPressure)
	pcall(printToFile, msg)

--[[
OK 15x solvent replacement (aspiration: 100uL/min, dispense: 4000uL/min). 
OK 2x direct flow at 400 bar (limited to maximum 200uL/min). 
OK 1x direct flow through 2nd capillary (500uL total volume at 50uL/min). 
OK Then we wait until we reached 6h total time. So this new procedure consists of blocks of 6h each
--]]

	local cycleCounter = 0

	while (pf.now() < runTime) do
		cycleCounter = cycleCounter + 1
		local startTime = pf.now()

		local cycleMsg = "Cycle " .. tostring(cycleCounter)
		status:SetStatus(cycleMsg)

		context:Log("Starting cycle " .. tostring(cycleCounter))

		releasePressure(true, true)			-- 1 minute
		solventExchange()					-- solventRepetitions * (1350/"Aspiration speed" + 1350/"Dispense speed") minutes
--		releasePressure()					-- 1 minute
		local directFlowTimeOut = startTime + cycleTime - pf.now() - 960		-- 960 == (16 x 60sec) == rest of runtime of the cycle after direct flow
		directFlow_n_times(directFlowTimeOut)
		rinseInjectionCapillary_new()		-- 10 minutes
		releasePressure(true, false)		-- 1 minute
		refillPumps()						-- 5 minutes

		-- do not log the status, only show it in LC-Control
		context:Log("Finished cycle " .. tostring(cycleCounter) .. " excluding waiting time")
		context:Log("Waiting before cycle time passed until starting next cycle...")

		local statusDictator = LoggingDictator.Prevent(status)
		local cnt = cycleTime - (pf.now() - startTime)
		while (cnt > 0) do
			local hours = math.floor(cnt/3600)
			local minutes = math.floor((cnt-(hours*3600))/60)
			local seconds = math.floor(cnt-(hours*3600)-(minutes*60))
			msg = DotNetString.Format("Waiting {0:0}:{1:00}:{2:00} before starting next cycle", hours, minutes, seconds)
			status:SetStatus(msg)
			context:Sleep(1000)
			status:RemoveStatus(msg)
			cnt = cnt - 1
		end
		statusDictator:Dispose()

		status:RemoveStatus(cycleMsg)
		context:Log("Finished cycle " .. tostring(cycleCounter) .. " including waiting time")
	end

	dictator:Dispose()

	-- pumps are stoped in 'releasePressure()' in 'directFlow()'
	pf.isPumpIdle(pump, sleep_100)
end
