local Date = "2025/07/23"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---@param context InitHelper
function Initialize (context)
	context.Name = "Valve leakage"
	context.Description = "Procedure for testing the pump valves for a leakage"
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Diagnostics

	context:DeclareParameter("Test valve naming", "Valve 001", "", "text", "enter any text you want, this is used to identify the log file","")
	context:DeclareParameter("System leakage", "-1", "nL/min", "text", "if '-1' then determine the system leakage, otherwise enter the leakage from a previous run", "")
	context:DeclareParameter("Test pressure", 1000, "bar", "integer", "Test pressure for the leakage test", "")
	context:DeclareParameter("Wait time", 30, "sec", "integer", "Time before starting the leakage measurement", "")
	context:DeclareParameter("Measure time", 60, "sec", "integer", "Time for measuring the leakage", "")
	context:DeclareParameter("Repetition", 0, "num", "integer", "Number of repetitions", "")
	context:DeclareParameter("Start port", 1, "num", "integer", "Number of the port to be started", "")
	context:DeclareParameter("Test single port position", false, nil, "boolean", "Testing just one port", "")
	context:DeclareParameter("Single port rotor position", 0, "deg", "integer", "Turn the valve to this angle", "")
	context:DeclareParameter("Test all ports", true, nil, "boolean", "Testing all port", "")
	context:DeclareParameter("Write values into file", true, nil, "boolean", "Write results into a file", "")
end

---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (installed, context)
	local pf = require "pump_functions"
	local v  = require "validation"

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	v.verify_range(context, "Test pressure", 0, pressSettings.GradientPumpMaxTargetPressure)
	v.verify_range(context, "Wait time", 1, 3600)
	v.verify_range(context, "Measure time", 1, 600)
	v.verify_range(context, "Start port", 1, 6)
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
	context:Log("Lua date:			{0}", Date)
	context:Log("--- Experiment:    {0}", context.Description)

	require "degas"

	local baltic = require "baltic"
	local pf = require "pump_functions"
	local pp = require "palplus"
	---@type Zirconium
	local zr = require "zirconium"
	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)

	local testPressure = context:GetArgumentValue("Test pressure")
	local waitTime = context:GetArgumentValue("Wait time")
	local measureTime = context:GetArgumentValue("Measure time")
	local repetition = context:GetArgumentValue("Repetition")
	local startPort = context:GetArgumentValue("Start port")
	local postStartPort = startPort+1
	if postStartPort > 6 then postStartPort = postStartPort-6 end
	local systemLeakage = tonumber(context:GetArgumentValue("System leakage"))
	local valveLeakage = {0,0,0,0,0,0}
	local valveLeakageSingle = {0,0,0,0,0,0}
	local valveLeakagePortToPort = {0,0,0,0,0,0}
	local valveLeakageBlindPorts = {0,0,0,0,0,0}
	local averagePortLeakage = 0
	local averageValveLeakage = 0
	local valveT = pp.QueryValveDrive(execLeft, pp.Capabilities.ISelectorValve)
	local osDate = os.date("%Y%m%d")
	local osTime = os.date("%H%M%S")
	local valveName = context:GetArgumentValue("Test valve naming")
	local fileDirectory = "TestLogs"
	local fileLocation = DotNetString.Format("{0}_{1}_{2}.txt", osDate, osTime, valveName)
	local writeToFile = context:GetArgumentValue("Write values into file")
	local firstLogging = true
	local statusPressure = "pressurizing..."
	local statusWaiting = "waiting..."

	--	Signalize_Reset
	context:ShowComposition(true)

	local function sleep_100()
		context:Sleep(100)
	end
	local function sleep_250()
		context:Sleep(250)
	end

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

	local function nowTime()
		return os.clock()
	end

	---Write a string into a file
	---@param storageString string
	local function storeInFile(storageString)
		storageString = storageString.."\n"
		local f = io.open(fileDirectory.."\\"..fileLocation, "a")
		context:Sleep(100)
		if not f then
			os.execute("mkdir " .. fileDirectory)
			context:Sleep(100)
			f = io.open(fileDirectory.."\\"..fileLocation, "a")
		end
		context:Sleep(100)
		if f then
			io.output(f)
			context:Sleep(100)
			io.write(storageString)
			context:Sleep(100)
		end
		io.close()
	end

	---Build a pressure on channel A
	---@param p number
	---@return boolean
	local function build_pressure(p)
		if zr.IsEmpty(pump, zr.A) then return false end
		local succsess = true
		local bPTime = nowTime() + 120
		context:Log("build_pressure {0}", zr.A)
		status:SetStatus(statusPressure)
		pf.Manualmode_Pump_constantPressure(zr.A, p, pump, sleep_250)
		while not zr.IsEmpty(pump, zr.A) do 
			sleep_250()
			if (pump:GetCurrentPressure(zr.A) >= p-1) then break end
			if (nowTime() > bPTime) then
				context:Report("Valve leakage test", Severity.Info, true, "Could not reach the test pressure within the time limit.\n\nThis usually indicates a significant leak in the system. Check all fittings, connections, and valve seals before retrying." )
				succsess = false
				break
			end
		end
		status:RemoveStatus(statusPressure)
		return succsess
	end

	---Determine the average leakage rate
	---@return number|nil
	local function getAverageLeakage()
		local avLeakage = 0
		local n = 0
		local t = measureTime
		while t >= 0 do
			local msg = "Measure time: "..t.."s"
			status:SetStatus(msg)
			avLeakage = avLeakage + pump:GetCurrentFlow(zr.A)
			context:Sleep(1000)
			status:RemoveStatus(msg)
			t = t - 1
			n = n + 1
		end
		if n > 0 then return avLeakage/n end
		return nil
	end

	---Return the average system leakage
	---@return number
	local function getSystemLeakage()
	-- 1. determine leakage of pumpA - valveA - tubing - blockage
		context:Report("Determine system leakage", Severity.Info, true, "block the tubing from MixTee to test valve at the valve side.")
		context:WaitForSignal("Determine system leakage")
		local pr = require "PreRunFunctions"
		local msg = "Getting system leakage"
		status:SetStatus(msg)
		status:SetStatus(statusPressure)
		if not build_pressure(testPressure) then
			pr.decompressSystem(context)
			context:Abort()
		end
		status:SetStatus(statusWaiting)
		context:Sleep(waitTime*1000)
		status:RemoveStatus(statusWaiting)
		local sLeakage = getAverageLeakage()
		status:RemoveStatus(msg)
		if sLeakage == nil then
			context:Report("System leakage", Severity.Warn, true, "System leakage could not be determined")
			pr.decompressSystem(context)
			context:Abort()
		end
		return sLeakage*1000
	end

	---evaluate the valve leakage
	---@param port number
	---@return number
	local function getValveLeakage(port)
	-- 4. determine leakage of test valve @ port
		local toPort = port + 1
		if toPort > 6 then toPort = toPort - 6 end
		local msg = DotNetString.Format("Valve switched to ports {0}-{1}", port, toPort)
		status:SetStatus(msg)
		if not build_pressure(testPressure) then
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 300, sleep_250, false)
			status:RemoveStatus(msg)
			return 6000
		end
		status:SetStatus(statusWaiting)
		context:Sleep(waitTime*1000)
		status:RemoveStatus(statusWaiting)
		local sLeakage = getAverageLeakage()
		status:RemoveStatus(msg)
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 300, sleep_250, false)
		if sLeakage == nil then
			context:Report("Valve leakage", Severity.Warn, true, "Valve leakage could not be determined")
			context:Abort()
		end
		return sLeakage*1000
	end

	local function logTestResults(storeValues)
		-- This is executed if the repetition > 0 --
		local naming = DotNetString.Format("Average valve leakage @ {0} bar", testPressure)
		local n = 1
		local i = startPort
		local k = 0
		averageValveLeakage = 0
		if context:GetArgumentValue("Test all ports") == true then n = 6 end
		if storeValues == true then
			pcall(storeInFile, baltic.devider)
			pcall(storeInFile, naming)
		end
		context:Log(baltic.devider)
		context:Log(naming)
		while(n > 0) do
			local toPort = i + 1
			if toPort > 6 then toPort = toPort - 6 end
			local value = valveLeakage[i]
			local msg = DotNetString.Format("  Average port: {0:#0}, leakage: {1:#0.000} nL/min", i, value)
--			local msg = DotNetString.Format("  Port {0:#0} to port {1:#0}: {2:#0.000} nL/min", i, toPort, value)
			if storeValues == true then pcall(storeInFile, msg) end
			context:Log(msg)
			averageValveLeakage = averageValveLeakage + value
			n = n-1
			i = i+1
			k = k+1
			if i > 6 then i = i - 6 end
		end
		averageValveLeakage = averageValveLeakage / k
		local msg = DotNetString.Format("Average valve leakage:            {0:#0.000} nL/min", averageValveLeakage)
		if storeValues == true then pcall(storeInFile, msg) end
		context:Log(msg)
		context:Log(baltic.devider)
	end

	local function logSingleResult(port, leakage, valveAngle, testCycle, storeValues)
		-- This is executed after each measurement --
		local val = math.ceil(leakage*1000)/1000		-- just 3 following digits
		if storeValues then
			if firstLogging then
				firstLogging = false
				pcall(storeInFile, baltic.devider.."\n".."Cycle "..testCycle)
			end
			pcall(storeInFile, "Port "..port..", leakage "..val.." nL/min, @valve angle "..valveAngle.." deg")
		end
		context:Log("Port "..port..", leakage "..val.." nL/min, @valve angle "..valveAngle.." deg")
	end

	local function logPortToPortResult(port, prePort, postPort, leakage, storeValues)
		-- This is executed after each measurement --
		local val = math.ceil(leakage*1000)/1000		-- just 3 following digits
		if storeValues then
			pcall(storeInFile, baltic.devider.."\n".."Port "..prePort.." - "..port.." - "..postPort..", average leakage "..val.." nL/min")
		end
		context:Log("Port "..prePort.." - "..port.." - "..postPort..", average leakage "..val.." nL/min")
	end

	local function logPortToBlindPortsResult(port, leakage, storeValues)
		-- This is executed after each measurement --
		local val = math.ceil(leakage*1000)/1000		-- just 3 following digits
		if storeValues then
			pcall(storeInFile, "Port "..port.." to blind ports average leakage "..val.." nL/min")
		end
		context:Log("Port "..port.." to blind ports average leakage "..val.." nL/min")
	end

	local function logAveragePortToPortResult(arPtP, storeValues)
		local valPtP = 0
		for i=1, 6 do
			valPtP = valPtP + arPtP[i]
		end
		local avPtPVal = math.ceil(valPtP*1000)/6000		-- just 3 following digits
		context:Sleep(250)
		if storeValues then
			pcall(storeInFile, baltic.devider.."\n".."Average over all (pre port) - (port) - (post port) leakage: "..avPtPVal.." nL/min")
		end
		context:Log("Average over all (pre port) - (port) - (post port) leakage: "..avPtPVal.." nL/min")
	end

	local function logAveragePortToBlindPortsResult(arPtBP, storeValues)
		local valPtBP = 0
		for i=1, 6 do
			valPtBP = valPtBP + arPtBP[i]
		end
		local avPtBPVal = math.ceil(valPtBP*1000)/6000		-- just 3 following digits
		context:Sleep(250)
		if storeValues then
			pcall(storeInFile, "Average over all blind ports leakage: "..avPtBPVal.." nL/min")
		end
		context:Log("Average over all blind ports leakage: "..avPtBPVal.." nL/min")
	end

	local function logAveragePortResult(port, avLeakage, storeValues)
		-- This is executed after each measurement --
		local val = math.ceil(avLeakage*1000)/1000		-- just 3 following digits
		if storeValues then
			pcall(storeInFile, "Port "..port..", average leakage over all positions: "..val.." nL/min")
		end
		context:Log("Port "..port..", average leakage over all positions: "..val.." nL/min")
	end
	
	local function logSinglePortResult(port, leakage, storeValues)
		-- This is executed after each measurement --
		local val = math.ceil(leakage*1000)/1000		-- just 3 following digits
		if storeValues then
			pcall(storeInFile, "Port "..port..", leakage w/o valve switching: "..val.." nL/min")
		end
		context:Log("Port "..port..", leakage w/o valve switching: "..val.." nL/min")
	end
	
	local function logAverageAllPortsResult(arPtP, arPtBP, storeValues)
		local valPtP = 0
		local valPtBP = 0
		for i=1, 6 do
			valPtP = valPtP + arPtP[i]
			valPtBP = valPtBP + arPtBP[i]
		end
		local avPVal = math.ceil((valPtP*2+valPtBP*4)*1000)/36000		-- just 3 following digits
		context:Sleep(250)
		if storeValues then
			pcall(storeInFile, "Average leakage over all ports: "..avPVal.." nL/min")
		end
		context:Log("Average leakage over all ports: "..avPVal.." nL/min")
	end

	local function getSinglePortLeakage(start_Port, testCycle)
		firstLogging = true
		local sP = start_Port
		local leakageSum = 0
		local n = 6
		if context:GetArgumentValue("Test single port position") == true then
			n = 1
		end
		local i = sP
		local prePort = i-1
		local postPort = i+1
		if postPort > 6 then postPort = postPort - 6 end
		if prePort < 1 then prePort = prePort + 6 end
		local msg2 = DotNetString.Format("Testing port {0} in cycle {1}", i, testCycle)
		local contTest = DotNetString.Format("Remove blockage and connect tubing to port {0} of test valve.\r\nBlock port {1} and port {2}, other ports can be left open.", i, prePort, postPort)
		context:Report(msg2, Severity.Info, true, contTest)
		context:Signalize(baltic.ColorsRGB.LightGray, baltic.Naming.ValveTLongGroove, baltic.Naming.ValveTShortGroove, baltic.Naming.Trap, baltic.Naming.TrapValveToWaste, baltic.Naming.TransferLine, baltic.Naming.Separator, baltic.Naming.MixTeeToTrapValve)
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 300, sleep_250, false)
		context:WaitForSignal(msg2)
		while(n > 0) do
			local valveAngle = 420-i*60
			if context:GetArgumentValue("Test single port position") == true then
				valveAngle = context:GetArgumentValue("Single port rotor position")
			end
			if valveAngle < 0 then valveAngle = valveAngle + 360 end
			if valveAngle >= 360 then valveAngle = valveAngle - 360 end
			execLeft:MoveValveDrive(valveT, pp.Quantity(valveAngle, "deg"))
			local msg1 = DotNetString.Format("Continue test port {0} @ cycle {1}", i, testCycle)
			context:Log(msg1)
			local val = getValveLeakage(i)
			context:Log("systemLeakage: {0} nL", systemLeakage)
			context:Log("ValveLeakage: {0} nL", val)
			val = val - systemLeakage
			logSingleResult(sP, val, valveAngle, testCycle, writeToFile)
			context:Log("ValveLeakage minus systemLeakage: {0} nL", val)
			valveLeakageSingle[i] = valveLeakageSingle[i] + val
			if testCycle == (repetition+1) then
				valveLeakage[i] = valveLeakageSingle[i] / (6*testCycle)
			end
			if (n == 1) or (n == 6) then
				valveLeakagePortToPort[sP] = valveLeakagePortToPort[sP] + val
			else
				valveLeakageBlindPorts[sP] = valveLeakageBlindPorts[sP] + val
			end
			leakageSum = leakageSum + val
			context:Log("----------------------------------------------")
			local msg = DotNetString.Format("Valve leakage port {0}: {1:#0.000} nL", i, val)
			context:Log(msg)
			n = n-1
			i = i+1
			if i>6 then i = i-6 end
		end
		if context:GetArgumentValue("Test single port position") == true then
			logSinglePortResult(sP, valveLeakageSingle[sP], writeToFile)
		else
			if testCycle == (repetition+1) then
				local preP = sP-1
				if preP < 1 then preP = preP + 6 end
				local postP = sP+1
				if postP > 6 then postP = postP - 6 end
				valveLeakagePortToPort[sP] = valveLeakagePortToPort[sP] / (2 * testCycle)
				valveLeakageBlindPorts[sP] = valveLeakageBlindPorts[sP] / (4 * testCycle)
				averagePortLeakage = (valveLeakagePortToPort[sP]*2 + valveLeakageBlindPorts[sP]*4) / 6
				logPortToPortResult(sP, preP, postP, valveLeakagePortToPort[sP], writeToFile)
				logPortToBlindPortsResult(sP, valveLeakageBlindPorts[sP], writeToFile)
				logAveragePortResult(sP, averagePortLeakage, writeToFile)
			end
		end
	end

	pf.SetMaxPressureLimit(zr.A, 1020, pump, sleep_100)		-- set max pump pressure
	pf.SetMaxPressureLimit(zr.B, 1020, pump, sleep_100)		-- set max pump pressure
	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)
	zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee)
	zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Compress)

	os.execute("mkdir "..fileDirectory)

	local storageMsg = DotNetString.Format("Valve name: {0}\nTest pressure: {1} bar\nWait time: {2} sec\nMeasure time: {3} sec\nRepetitions: {4}\nStart port: {5}\nTest single port position: {6}\nSingle port rotor position: {7} deg\nTest all ports: {6}\n", context:GetArgumentValue("Test valve naming"), testPressure, waitTime, measureTime, repetition, startPort, context:GetArgumentValue("Test single port position"), context:GetArgumentValue("Single port rotor position"), context:GetArgumentValue("Test all ports"))
	pcall(storeInFile, storageMsg)

	if systemLeakage == -1 then
		systemLeakage = getSystemLeakage()
		storageMsg = "System leakage: "..systemLeakage.."\n"
		pcall(storeInFile, storageMsg)
	end

	local msg0 = DotNetString.Format("Testing valve "..valveName)
	context:Report(msg0, Severity.Info, true, "Mount test valve as trap valve.")
	pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 300, sleep_250, false)
	context:WaitForSignal(msg0)
	local testCycle = repetition
	while (testCycle >= 0) do
		local n = 1
		local i = startPort
		if context:GetArgumentValue("Test all ports") == true and  context:GetArgumentValue("Test single port position") == false then n = 6 end
		while(n > 0) do
			getSinglePortLeakage(i, repetition-testCycle+1)
			n = n-1
			i = i+1
			if i > 6 then i = i - 6 end
		end
		testCycle = testCycle-1
	end
	if repetition > 0 then logTestResults(writeToFile) end

	if context:GetArgumentValue("Test all ports") == true and  context:GetArgumentValue("Test single port position") == false then
		logAveragePortToPortResult(valveLeakagePortToPort, writeToFile)
		logAveragePortToBlindPortsResult(valveLeakageBlindPorts, writeToFile)
		logAverageAllPortsResult(valveLeakagePortToPort, valveLeakageBlindPorts, writeToFile)
	end

	dictator:Dispose()

	pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 300, sleep_250, false)
	pf.SetMaxPressureLimit(zr.A, 1020, pump, sleep_100)
	pf.SetMaxPressureLimit(zr.B, 1020, pump, sleep_100)
	context:Sleep(2000)
end
