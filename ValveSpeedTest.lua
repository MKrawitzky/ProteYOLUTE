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
	context.Name = "Valve lifetime"
	context.Description = "Procedure for testing the lifetime of the pump valve seals"
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Diagnostics

	context:DeclareParameter("Test Pressure", 800, "bar", "integer", "Test pressure", "")
	context:DeclareParameter("Pump Number", "1", "num", "text", "Serial number of the pump", "")
	context:DeclareParameter("Time Between Shifts", 60, "sec", "integer", "Time between valve shifts", "")
	context:DeclareParameter("Shift Speed", 288, "steps", "integer", "Valve shift speed", "")
	context:DeclareParameter("Acceleration", 1660, "steps", "integer", "Acceleration speed", "")
	context:DeclareParameter("Refill Pumps", true, nil, "boolean", "Refill pumps at procedure start", "")
	context:DeclareParameter("Maximum Leakage", 2000, "nL/min", "integer", "Maximum allowable leakage before stopping the test with the leaking channel", "")
end

---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (installed, context)
	local pf = require "pump_functions"
	local validation = require "validation"

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	validation.verify_specified(context, "Test Pressure")
	validation.verify_range(context, "Test Pressure", 0, pressSettings.GradientPumpMaxTargetPressure)
	validation.verify_specified(context, "Time Between Shifts")
	validation.verify_range(context, "Time Between Shifts", 1, 600)
	validation.verify_specified(context, "Pump Number")
	validation.verify_specified(context, "Shift Speed")
	validation.verify_specified(context, "Acceleration")
	validation.verify_specified(context, "Maximum Leakage")
	validation.verify_range(context, "Maximum Leakage", 0, 100000)
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)

	require "degas"
	local baltic 	= require "baltic"
	local parallel  = require "parallel"
	local pf 		= require "pump_functions"
	---@type Zirconium
	local zr 		= require "zirconium"

	local valvePosition = {180,240,300}
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	
	local function sleep_100()
		context:Sleep(100)
	end

	---Function to build the desired pressure on channel A and B
	---@param p number
	local function build_pressureAB(p)
		if zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) then return end
		local bPTime = os.clock() + 120
		context:Log("build_pressure AB")
		pf.Manualmode_Pump_constantPressure(zr.A, p, pump, sleep_100)
		pf.Manualmode_Pump_constantPressure(zr.B, p, pump, sleep_100)
		while not zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) do 
			sleep_100()
			if (pump:GetCurrentPressure(zr.A) <= p-1) and (os.clock() > bPTime) then 
				context:Report("Channel A", Severity.Error, true, "Pump A could not reach the target pressure.\n\nCheck for leaks, air in the pump, or a blocked waste line.")
			else
				break 
			end
			if (pump:GetCurrentPressure(zr.B) <= p-1) and (os.clock() > bPTime) then 
				context:Report("Channel B", Severity.Error, true, "Pump B could not reach the target pressure.\n\nCheck for leaks, air in the pump, or a blocked waste line.")
			else
				break 
			end
		end
		context:Log("pressure AB reached")
	end

	---Switch the desired valve(s) to the position
	---@param angle number
	---@param clockwise boolean|nil
	---@param switchValveA boolean|nil
	---@param switchValveB boolean|nil
	local function switchValves(angle, clockwise, switchValveA, switchValveB)
		if switchValveA then zr.SetValvePosition(context, pump, zr.A, angle, clockwise) end
		if switchValveB then zr.SetValvePosition(context, pump, zr.B, angle, clockwise) end
	end

	local timeBetweenShifts = context:GetArgumentValue("Time Between Shifts")
	local shiftSpeed = context:GetArgumentValue("Shift Speed")
	local acceleration = context:GetArgumentValue("Acceleration")
	local pressure = context:GetArgumentValue("Test Pressure")
	local refillPumps = context:GetArgumentValue("Refill Pumps")
	local maxLeakage = context:GetArgumentValue("Maximum Leakage")
	
	local setting 	= {"Test Pressure: ", "Shift Speed: ", "Acceleration: ", "Waiting Time: ", "Maximum Leakage: "}
	local unit		= {"bar", "", "s", "nL/min"}
	local value		= {pressure, shiftSpeed, acceleration, timeBetweenShifts, maxLeakage}
--	local header0 = DotNetString.Format("Test Pressure: {0:0}bar, Shift Speed: {1:0}, Acceleration: {2:0}, Waiting Time: {3:0}s, Maximum Leakage: {4:0}nL/min", pressure, shiftSpeed, acceleration, timeBetweenShifts, maxLeakage)
	local header1 = " , ,switch,angleA,volumeA,leakageA,pressureA, ,angleB,volumeB,leakageB,pressureB"
	local header2 = "date,time,#,[deg],[uL],[nL/min],[bar], ,[deg],[uL],[nL/min],[bar]"

	---Create a CSV file with a header and return the file
	---@param context IProcedureExecutionContext
	---@return string
	local function createCSVFile(context)
		local function writeHeader()
			for i=1, #setting do
				local item = DotNetString.Format("{0},{1},{2}",setting[i], value[i], unit[i])
				io.write(item,"\n")
			end
		end
		local dir = "TestLogs"
--		local dir = "/BDALSystemData/"
		local timeStamp = DotNetString.Format("{0}{1}{2}{3}{4}{5}", os.date("%Y"), os.date("%m"), os.date("%d"), os.date("%H"), os.date("%M"), os.date("%S")) 
		local fileName = "Pump" .. context:GetArgumentValue("Pump Number")
		local file = DotNetString.Format("{0}/{1}-{2}.csv", dir, timeStamp, fileName)
--		os.execute("mkdir "..dir)
		local f = io.open(file, "a")			-- Opens a file in append mode
		context:Sleep(100)
		if not f then
			os.execute("mkdir " .. dir)
			context:Sleep(100)
			f = io.open(dir.."\\"..dir, "a")
		end
		context:Sleep(100)
		if f then
			io.output(f)
			io.write(timeStamp, "-", fileName, ".csv", "\n")
			writeHeader()
			io.write("","\n")
			io.write(header1,"\n")
			io.write(header2)
		end
		io.close()
		return file
	end

	---Write values into the CSV file
	---@param logFile string
	---@param cycle number
	---@param preVA number
	---@param preVB number
	---@return number
	---@return number
	---@return number
	---@return number
	---@return string
	local function logValues(logFile, cycle, preVA, preVB)
		local function appendValues(newValues)
			local f = io.input(logFile)
			context:Sleep(100)
			if f then
				local content = io.read("*all")
				local newContent = DotNetString.Format("{0}{1}{2}", content, "\n", newValues)
				io.output(f)
				io.write(newContent)
			end
			io.close()
		end

		local d = os.date("%x")
		local t = os.date("%X")
		local c = cycle
		local aA = pump:GetSetManualValvePosition(zr.A)			-- angle A
		local vA = pump:GetPistonPosition(zr.A)					-- volume A
		local leakA = vA-preVA
		local pA = pump:GetCurrentPressure(zr.A)
		local aB = pump:GetSetManualValvePosition(zr.B)			-- angle B
		local vB = pump:GetPistonPosition(zr.B)					-- volume B
		local leakB = vB-preVB
		local pB = pump:GetCurrentPressure(zr.B)
		leakA = leakA*60/tonumber(timeBetweenShifts)*1000			-- calc leakage / minute
		leakB = leakB*60/tonumber(timeBetweenShifts)*1000			-- calc leakage / minute
		local values = DotNetString.Format("{0},{1},{2},{3},{4:0.000},{5:0},{6:0.0}, ,{7},{8:0.000},{9:0},{10:0.0}", d,t,c,aA,vA,leakA,pA,aB,vB,leakB,pB)
		local returnValues = DotNetString.Format("#: {0}| angleA: {1}| leakA: {2:0}| pA: {3:0.0}|| angleB: {4}| leakB: {5:0}| pB: {6:0.0}", c,aA,leakA,pA,aB,leakB,pB)

		pcall(appendValues, values)
		return vA, vB, leakA, leakB, returnValues
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

	local function sleep_200()
		context:Sleep(200)
	end

	---Refill the desired pump
	---@param channel Channel
	---@param sleeping_yield function
	local function RefillPump(channel, sleeping_yield)
		if zr.IsFull(pump, channel) then return end -- don't waste valve switch
      -- wait until degasser is on for the time defined in baltic.preOntimeDegasser
		if pump:GetCurrentPressure(channel) < 10 then
			zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent, nil)
			pf.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpRefillSpeed, pump, sleeping_yield)
			while not zr.IsFull(pump, channel) do sleeping_yield() end
			pf.Manualmode_Pump_constantSpeed(channel, 0, pump, sleeping_yield)
			context:Sleep(2000)
			pf.Manualmode_Pump_constantSpeed(channel, 300, pump, sleeping_yield)
			context:Sleep(2000)
			pf.Manualmode_Pump_constantSpeed(channel, 0, pump, sleeping_yield)
			context:Sleep(4000)
		end
	end


	pump:SetMaxPressureLimit(zr.A, 1020)
	pump:SetMaxPressureLimit(zr.B, 1020)
--	pf.setMaxPressureLimitsAB(context, pump, 21, false, false)		-- set max pump pressure
	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)

	local block = "Block pump valves A and B"
	context:Report(block, Severity.Info, true, "Block positions: 2, 6 and 5")
	context:WaitForSignal(block)

	if refillPumps then
		local startTime = os.clock()
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 60, sleep_100, baltic.smooth)
		while (os.clock() < ( startTime + baltic.preOntimeDegasser)) do sleep_200() end
		local rA = { RefillPump, zr.A, parallel.yield }
		local rB = { RefillPump, zr.B, parallel.yield }
		parallel.run(sleep_200, rA, rB)
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
	end

	status:SetStatus("Preparing test...")
	switchValves(valvePosition[1], nil, true, true)
	build_pressureAB(pressure)
	context:Sleep(60000)		-- wait 60 seconds for stable pressure
	status:RemoveStatus("Preparing test...")

	local preVA = pump:GetPistonPosition(zr.A)
	local preVB = pump:GetPistonPosition(zr.B)
	local leakageVA, leakageVB = 0, 0
	local stat, logFile = pcall(createCSVFile, context)
	local waitTime = os.clock() + timeBetweenShifts
	local size = #valvePosition
	local switchValveA = true
	local switchValveB = true
	local stopPumpA = true
	local stopPumpB = true
	local msg = "Test started"
	local i = 2
	local n = 1
	status:SetStatus(msg)
	while true do
		context:Sleep(100)
		if os.clock() > waitTime then
			waitTime = os.clock() + timeBetweenShifts
			status:RemoveStatus(msg)
			if (stat == true) and (logFile ~= nil) then
				preVA, preVB, leakageVA, leakageVB, msg = logValues(logFile, n, preVA, preVB)
			else
				break
			end
			status:SetStatus(msg)
			if (zr.IsEmpty(pump, zr.A) or (switchValveA == false)) and stopPumpA == true then
				pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
				stopPumpA = false
			end
			if (zr.IsEmpty(pump, zr.B) or (switchValveB == false)) and stopPumpB == true then
				pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
				stopPumpB = false
			end
			if leakageVA > maxLeakage and stopPumpA == true then
				pf.Manualmode_Pump_constantPressure(zr.A, 5, pump, sleep_100)
				switchValveA = false
				context:Report("Valve A:", Severity.Error, true, "Leakage detected ({0})", leakageVA)
			end
			if leakageVB > maxLeakage and stopPumpB == true then
				pf.Manualmode_Pump_constantPressure(zr.B, 5, pump, sleep_100)
				switchValveB = false
				context:Report("Valve B:", Severity.Error, true, "Leakage detected ({0})", leakageVA)
			end
			if switchValveA == false and switchValveB == false then break end
			switchValves(valvePosition[i], i~=1, switchValveA, switchValveB)
			i = i + 1
			if i > size then i = 1 end
			n = n + 1
		end
	end

	dictator:Dispose()

end
