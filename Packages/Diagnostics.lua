local Date = "2025/09/17"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type JournalEntry
local JournalEntry = luanet.import_type("Bruker.Lc.JournalEntry")
---@type LogTo
local LogTo = luanet.import_type("Bruker.Lc.Business.LogTo")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")

local P = {}

---Diagnose the instrument
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param pressure table
---@param advanced boolean
---@param self_Test SelfTest
function P.diagnostics(installed, context, pressure, advanced, self_Test)
	context:Log("Diagnose Lua date: {0}", Date)
	context:Log("Diagnose Lua name: {0}", "Diagnostics")

	require "degas"

	local baltic = require "baltic"
	local chrom = require "chromatography"
	local parallel = require "parallel"
	local pf = require "pump_functions"
	local pp = require "palplus"
	local pr = require "PreRunFunctions"
	local csv = require "csv_file_logging"
	---@type Zirconium
	local zr = require "zirconium"
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type IJournal
	local journal = context:GetProcedureParticipant(baltic.JournalRole)
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local valveI = pp.QueryValveDrive(execLeft, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execLeft, pp.Capabilities.ISelectorValve)
	-- set cutoff pressure to maximum
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local results = {}
	local testOK = true
	local finished = false
	local factory = false
	local OQ = false
	local basic = false
	local N = baltic.Naming

	local leakTest = "Leak Test"

	local selfTest = self_Test.isSelfTest
	local isService = self_Test.isService
	local showMsgFRTPumpAMixTee = false
	local frtPumpAMixTee = false
	local frtPumpBMixTee = false
	local frtInjectionSystem = false
	local ltPumpA = false
	local ltPumpB = false
	local ltMixTeeSystem = false
	local ltInjectionSystem = false
	local lpltPumpAB = false
	local continousLeakTestMeasurement = false
	local limitRoute1, tP1 = 0, 0
	local limitRoute2, tP2 = 0, 0
	local limitRoute3, tP3 = 0, 0
	local noLimit = false
	if context:GetArgumentValue("Disable limits for leakage detection") ~= nil then noLimit = context:GetArgumentValue("Disable limits for leakage detection") end
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Disable limits for leakage detection", noLimit))

	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow restriction test pump A mixTee", false)) -- will be overwritten with true when test is performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow restriction test pump B mixTee", false)) -- will be overwritten with true when test is performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow restriction test injection system", false)) -- will be overwritten with true when test is performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test mixTee system", false)) -- will be overwritten with true when test is performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test injection system", false)) -- will be overwritten with true when test is performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Low pressure leak test", false)) -- will be overwritten with true when test is performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test pump A", false)) -- will be overwritten with true when test is performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test pump B", false)) -- will be overwritten with true when test is performed

	local csvFileName = "Diagnostics_Results"
	if selfTest then
		if isService then
			csvFileName = "Service_Results"
		else
			csvFileName = "Self_Test_Results"
		end
	end

	if selfTest == false then
		basic = context:GetArgumentValue("Basic")
		if (advanced == true) then
			frtPumpAMixTee = context:GetArgumentValue("Flow restriction test pump A mixTee")
			frtPumpBMixTee = context:GetArgumentValue("Flow restriction test pump B mixTee")
			frtInjectionSystem = context:GetArgumentValue("Flow restriction test injection system")
			ltPumpA = context:GetArgumentValue("Leak test pump A")
			ltPumpB = context:GetArgumentValue("Leak test pump B")
			ltMixTeeSystem = context:GetArgumentValue("Leak test mixTee system")
			ltInjectionSystem = context:GetArgumentValue("Leak test injection system")
			if context.AppKey.Special == baltic.FactoryDiag then
				continousLeakTestMeasurement = context:GetArgumentValue("Continuous leak test pumps")
			end

			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Diagnostic type", "Advanced"))
		elseif (basic == true) then
			if context.AppKey.Special == baltic.OQDiag then
				frtPumpAMixTee = context:GetArgumentValue("Flow restriction test")
				frtPumpBMixTee = context:GetArgumentValue("Flow restriction test")
				frtInjectionSystem = context:GetArgumentValue("Flow restriction test")
				ltPumpA = context:GetArgumentValue("High-pressure leak test")
				ltPumpB = context:GetArgumentValue("High-pressure leak test")
				ltMixTeeSystem = context:GetArgumentValue("High-pressure leak test")
				ltInjectionSystem = context:GetArgumentValue("High-pressure leak test")
	--			lpltPumpAB = context:GetArgumentValue("Low pressure leak test pump A and B") -- PBNE-872
			else
				frtPumpAMixTee = context:GetArgumentValue("Flow restriction test")
				frtPumpBMixTee = context:GetArgumentValue("Flow restriction test")
				frtInjectionSystem = context:GetArgumentValue("Flow restriction test")
				ltPumpA = context:GetArgumentValue("High-pressure leak test")
				ltPumpB = context:GetArgumentValue("High-pressure leak test")
				ltMixTeeSystem = context:GetArgumentValue("High-pressure leak test")
				ltInjectionSystem = context:GetArgumentValue("High-pressure leak test")
				lpltPumpAB = context:GetArgumentValue("Low-pressure leak test")
			end

			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Diagnostic type", "Basic"))
		end

		if context.AppKey.Special == baltic.OQDiag then
			OQ = true
		else
			if context.AppKey.Special == baltic.FactoryDiag then
				factory = true
			end
		end
		context:Log("{0}-diagnose", context.AppKey.Special)
	else
		context:Log("Self-diagnose")
	end

	-- read the current column oven temperature
	local ovenTemp = pump:GetCurrentExternalTemperature()
	context:Log("--- Oven temperature: {0}", ovenTemp)
	if (ovenTemp < 1) then
		ovenTemp = 20
		context:Log("--- internal variable ovenTemp: {0}", ovenTemp)
	end

	local function nowTime()
		return os.clock()
	end

	local limit = {}
	if baltic.microElute then
		limit.lpfA = 0.04			-- Low pressure hold flow
		limit.lpfB = 0.04			-- Low pressure hold flow
		limit.lpbA = 15				-- Low pressure build volume
		limit.lpbB = 15				-- Low pressure build volume
		limit.bpfA = 1.0			-- Route 1 (1.87)
		limit.bpfB = 3.0			-- Route 2 (5.03)
		limit.bpsA = 3.5			-- Route 3 (4.59)
	--	limit.bpsB = 2.0			-- not used
		limit.ltsA = 0.3			-- Pump A, valve A, injection path, loop
		limit.ltsB = 0.3			-- Pump B, valve B
		limit.ltfA = -0.012			-- Flow sensor A
		limit.ltfB = 0.012			-- Valve B, trap, transfer line
		limit.ltfA2 = 0.012			-- negative flow from pump B through flow sensor A
		limit.ltPA1000 = 1.0		-- high pressure leak test pump A @ 1000 bar
		limit.ltPB1000 = 1.5		-- high pressure leak test pump B @ 1000 bar
	else
		if factory then		-- this limits are only valid with H2O in channel A and ACN in channel B
			limit.lpfA = 0.06			-- Low pressure hold flow		-- PBNE-872
			limit.lpfB = 0.075			-- Low pressure hold flow		-- PBNE-872
			limit.lpbA = 10				-- Low pressure build volume
			limit.lpbB = 20				-- Low pressure build volume	-- PBNE-872
			limit.bpfA = 0.63			-- Route 1
			limit.bpfB = 1.8			-- Route 2
			limit.bpsA = 4.0			-- Route 3
		--	limit.bpsB = 2.0			-- not used
			limit.ltsA = 0.3			-- Pump A, valve A, injection path, loop
			limit.ltsB = 0.3			-- Pump B, valve B
			limit.ltfA = -0.03			-- negative flow from pump B through flow sensor A
			limit.ltfB = 0.03			-- Valve B, trap, transfer line
			limit.ltfA2 = 0.03			-- Flow sensor A
			limit.ltfB3 = -0.3			-- valve B, flow sensor B: flow from pump A through flow sensor B
			limit.ltPA1000 = 1.0		-- high pressure leak test pump A @ 1000 bar
			limit.ltPB1000 = 1.5		-- high pressure leak test pump B @ 1000 bar
		else
			if OQ then		-- this limits are only valid with H2O in channel A and ACN in channel B
				limit.lpfA = 0.075			-- Low pressure hold flow
				limit.lpfB = 0.075			-- Low pressure hold flow
				limit.lpbA = 15				-- Low pressure build volume
				limit.lpbB = 20				-- Low pressure build volume
				limit.bpfA = 0.6			-- Route 1
				limit.bpfB = 1.5			-- Route 2
				limit.bpsA = 3.0			-- Route 3
			--	limit.bpsB = 2.0			-- not used
				limit.ltsA = 0.6			-- Pump A, Valve A, injection path, loop
				limit.ltsB = 0.6			-- Pump B, valve B
				limit.ltfA = -0.04			-- negative flow from pump B through flow sensor A
				limit.ltfB = 0.04			-- Valve B, trap, transfer line
				limit.ltfA2 = 0.04			-- Flow sensor A
				limit.ltfB3 = -0.3			-- valve B, flow sensor B: flow from pump A through flow sensor B
				limit.ltPA1000 = 1.0		-- high pressure leak test pump A @ 1000 bar
				limit.ltPB1000 = 1.5		-- high pressure leak test pump B @ 1000 bar
			else	-- standard limits -- this limits are only valid with H2O in channel A and ACN in channel B
				limit.lpfA = 0.1			-- Low pressure hold flow		-- PBNE-872
				limit.lpfB = 0.15			-- Low pressure hold flow		-- PBNE-872
				limit.lpbA = 20				-- Low pressure build volume
				limit.lpbB = 25				-- Low pressure build volume
				limit.bpfA = 0.5			-- Route 1
				limit.bpfB = 1.5			-- Route 2
				limit.bpsA = 3.0			-- Route 3
			--	limit.bpsB = 2.0			-- not used
				limit.ltsA = 1.5			-- Pump A, Valve A, injection path, loop
				limit.ltsB = 2.0			-- Pump B, valve B
				limit.ltfA = -0.05			-- negative flow from pump B through flow sensor A
				limit.ltfB = 0.05			-- Valve B, trap, transfer line
				limit.ltfA2 = 0.05			-- Flow sensor A
				limit.ltfB3 = -0.3			-- valve B, flow sensor B: negaive flow from pump A through flow sensor B
				limit.ltPA1000 = 1.0		-- high pressure leak test pump A @ 1000 bar
				limit.ltPB1000 = 1.5		-- high pressure leak test pump B @ 1000 bar
			end
		end
	end

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)

	--	Signalize_Reset
	pr.Signalize_Reset(context)
	context:ShowComposition(true)

	pr.iniFlowResistance(context, pump)

	-- init zirconium channels, abort if unsuccessful
	if not zr.InitChannel(context, pump, zr.A) then
		context:Report(N.PumpA, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode.

Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end
	if not zr.InitChannel(context, pump, zr.B) then
		context:Report(N.PumpB, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode.

Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end

	local settings = pump:GetSettings()
	zr.logInstrSettings(context, settings, "Diagnostics")

	local function sleep_200()
		context:Sleep(200)
	end
	local function sleep_100()
		context:Sleep(100)
	end

	pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false)		-- set max pump pressure

	local function clearResults()
		local i = 1
		while(results[i] ~= nil) do
			results[i] = nil
			i = i+1
		end
	end

	---Reporting the test results
	---@param test string
	---@param press number
	local function reportResults(test, press)
		local index = {}
		local unit = {}
		local function journalAdd(array, startIndex, endIndex)
			for i=startIndex, endIndex do
				if array[i] ~= nil then
					local decimalPlaces = pr.getMetaDataPrecision(array[i])
					local value = pf.noExp(array[i], 6)
					csv.logValueInCSVFile(context, csvFileName, test, index[i], value, unit[i], -1)
					journal:Add(JournalEntry.Set(LogTo.Both, context.Name, index[i], array[i], unit[i], decimalPlaces))
				end
				context:Sleep(250)
			end
		end
		local function journalDelete(startIndex, endIndex)
			for i=startIndex, endIndex do
				journal:Delete(index[i])
				context:Sleep(250)
			end
		end
		journal:Delete(test)
		if test == "Low pressure leak test" then
			local journalTest = "--- Low pressure leak test"
			journal:Delete(journalTest)
			journal:Add(JournalEntry.Set(LogTo.Journal,context.Name, journalTest, os.date("%c")))
			index[1] = "Hold Flow A @50bar"
			index[2] = "Hold Flow B @50bar"
			index[3] = "Compression volume A @50bar"
			index[4] = "Compression volume B @50bar"
			unit[1] = "nL/min"
			unit[2] = "nL/min"
			unit[3] = "\181L"
			unit[4] = "\181L"
			journalDelete(1, 4)
			journalAdd(results, 1, 4)
		end
		if test == "Flow restriction test" then
			local journalTest = "--- Flow restriction test"
			journal:Delete(journalTest)
			journal:Add(JournalEntry.Set(LogTo.Journal,context.Name, journalTest, os.date("%c")))
			index[5] = "FRT: Route 1"
			index[6] = "FRT: Route 1a"
			index[7] = "FRT: Route 1b"
			index[8] = "FRT: Route 1c"
			index[9] = "FRT: Route 1d"
			index[10] = "FRT: Route 2"
			index[11] = "FRT: Route 3 with loop"
			index[12] = "FRT: Route 3 w/o loop"
			unit[5] = "\181L/min"
			unit[6] = "\181L/min"
			unit[7] = "\181L/min"
			unit[8] = "\181L/min"
			unit[9] = "\181L/min"
			unit[10] = "\181L/min"
			unit[11] = "\181L/min"
			unit[12] = "\181L/min"

			journalDelete(5, 12)
			journalAdd(results, 5, 12)
		end
		if (test == "Leak test") then
			local journalTest = "--- Leak test"
			journal:Add(JournalEntry.Set(LogTo.Journal,context.Name, journalTest, os.date("%c")))
			index[15] = "LT: MixTee tubing A @1000bar"
			index[16] = "LT: MixTee tubing B @1000bar"
			index[22] = "LT: MixTee tubing A @1200bar"
			index[23] = "LT: MixTee tubing B @1200bar"
			index[17] = "LT: Base leakage mixTee tubing A @"..press.."bar"
			index[18] = "LT: Injection tubing @"..press.."bar"
			index[19] = "LT: Injection w/o loop @"..press.."bar"
			index[20] = "LT: Injection w/o injection tubing @"..press.."bar"
			unit[15] = "\181L/min"
			unit[16] = "\181L/min"
			unit[17] = "\181L/min"
			unit[18] = "\181L/min"
			unit[19] = "\181L/min"
			unit[20] = "\181L/min"
			unit[22] = "\181L/min"
			unit[23] = "\181L/min"

			journalDelete(17, 20)			-- PNS-513: delete only LT: injection system. mixTee system results are deleted in its procedure.
			journalAdd(results, 17, 20)		-- PNS-513: display only LT: injection system. mixTee system results are written in its procedure.
--			journalDelete(15, 23)
--			journalAdd(results, 15, 23)
		end
	end

	-- concurrently sets the position of (optionally) all 4 valves.
	---Set 1 to 4 valves
	---@param a_angle number|nil
	---@param b_angle number|nil
	---@param i_angle number|nil
	---@param t_angle number|nil
	local function set_valves(a_angle, b_angle, i_angle, t_angle)
		if (not execLeft.IsIdle) then
			error("cannot be used while not idle", 2)
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

	---Generate a string
	---@param prefix string
	---@param ... string
	---@return string
	local function gen_subject_string(prefix, ...)
		local subjects = table.pack(...)
		local subject = prefix
		for i = 1,subjects.n do
			subject = subject.." @"..subjects[i]
		end
		return subject
	end

	---Report a leakage
	---@param message string|nil
	---@param ... string
	local function report_leak(message, ...)
		pr.Signalize_Reset(context)
		local subject = gen_subject_string("Leak test", ...)
		context:Report(subject, Severity.Warn, true, message or "Leakage detected somewhere in the highlighted components.")
		context:WaitForSignal(subject)
		testOK = false
	end

	---Build pressure on both pumps
	---@param pA number
	---@param pB number
	---@param yield_function function
	local function build_pressureAB(pA, pB, yield_function)
		if zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) then return end
		local bPTime = nowTime() + 120
		context:Log("build_pressure AB")
		pf.Manualmode_Pump_constantPressure(zr.A,pA, pump, yield_function)
		pf.Manualmode_Pump_constantPressure(zr.B, pB, pump, yield_function)
		while not zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) do 
			yield_function()
			if ((math.abs(pump:GetCurrentPressure(zr.A) - pA) <= 1) and (math.abs(pump:GetCurrentPressure(zr.B) - pB) <= 1)) or (nowTime() > bPTime) then 
				break
			end
		end
	end

	---comment
	---@param channel Channel
	---@param p number
	---@param yield_func function
	local function build_pressure(channel, p, yield_func)
		if zr.IsEmpty(pump, channel) then return end
		local bPTime = nowTime() + 120
		context:Log("build_pressure {0}", channel)
		pf.Manualmode_Pump_constantPressure(channel, p, pump, yield_func)
		while not zr.IsEmpty(pump, channel) do
			yield_func()
			if (1 >= math.abs(pump:GetCurrentPressure(channel) - p)) or (nowTime() > bPTime) then
				break
			end
		end
	end

	-- If pressure std deviation is small, we have reached steady state.
	-- then if pressure avg is over a certain threshold, we are no longer compressing air -> done
	-- else continue compressing air.
	---Monitor a flow and speed channel
	---@param flow_channel Channel
	---@param speed_channel Channel
	---@param max_volume number
	---@param max_deviation number
	---@param press number
	---@param size number
	---@param yield_function function
	---@return number
	---@return number
	---@return boolean
	---@return boolean
	local function monitor_channel(flow_channel, speed_channel, max_volume, max_deviation, press, size, yield_function)
		-- use a ringbuffer holding position, flow, pressure, and time.
		local ringbuffer = require "ringbuffer"
		local timeOut = nowTime() + 300
		local buffer = ringbuffer()
		local flowAcc = 0
		local pressureAcc = 0
		local deviationAcc = 0
		-- max piston position
		local _max = math.min(pump:GetPistonPosition(speed_channel)+max_volume, pump.TotalPistonVolume-0.01)
		-- fill buffer with 'method.MeasuringTime' items

		repeat
			local item =
			{
				position = pump:GetPistonPosition(speed_channel),
				flow = pump:GetCurrentFlow(flow_channel),
				pressure = pump:GetCurrentPressure(speed_channel),
				clock = os.clock()
			}
			buffer:append(item)
			flowAcc = flowAcc + item.flow
			pressureAcc = pressureAcc + item.pressure
			item.presdiffsq = (item.pressure-pressureAcc/buffer:size())^2
			deviationAcc = deviationAcc + item.presdiffsq
			parallel.sleep(yield_function, 1000)
		until buffer:size() == size
		buffer:prev() -- so the next() below will point to the correct item
		local speedAvg = {}
		local n = 0
		local cnt = 0
		repeat
			-- update readings
			local pos = pump:GetPistonPosition(speed_channel)
			local flow = pump:GetCurrentFlow(flow_channel)
			local pressure = pump:GetCurrentPressure(speed_channel)
			local clock = os.clock()
			if (pos > _max) then
				-- max volume used or empty
				return 0, 0, false, false		-- return no flow and no speed, because flow is evaluated afterwords
			end
			-- update accumulators
			local item = buffer:next()
			-- subtract old values from accumulators
			flowAcc = flowAcc - item.flow
			pressureAcc = pressureAcc - item.pressure
			deviationAcc = deviationAcc - item.presdiffsq
			-- calc avg piston speed in ul/min
			n = n + 1
			if n>30 then
				table.remove(speedAvg,1)
			end
			table.insert(speedAvg, pos)
			-- update buffer
			item.position = pos
			item.flow = flow
			item.pressure = pressure
			item.clock = clock
			item.presdiffsq = (pressure-(pressureAcc+pressure)/size)^2
			-- add new values to accumulators
			flowAcc = flowAcc + item.flow
			pressureAcc = pressureAcc + item.pressure
			deviationAcc = deviationAcc + item.presdiffsq
			-- evaluate deviation
--			local deviation = math.sqrt(deviationAcc/size)
--			context:Log("{5} avg flow={1:0.000}, {0} speed={2:0.000}, {0} avg pressure={4:0.0}, {0} dev. pressure={3:0.0}", speed_channel, flowAcc/buffer:size(), speed, deviation, pressureAcc/size, flow_channel)
			parallel.sleep(yield_function, 1000)
			cnt = cnt + 1
		until math.sqrt(deviationAcc/size) <= max_deviation and pressureAcc/size >= (press-10) and n >= 60 or (nowTime() > timeOut)
		-- we don't need to check explicitly if we reach target pressure - the leakage rate will reveal if not, and fail the test.
		local flowAvg = flowAcc/size
		local avgSpeed = 0
		local posStart = 0
		cnt = 0
		for i, v in pairs(speedAvg) do
			if i == 1 then posStart = v end
			cnt = cnt + 1
			avgSpeed = v
		end
--		context:Log("--- --- avgSpeed: {0}, posStart: {1}, cnt: {2}, ", avgSpeed, posStart, cnt)
		avgSpeed = (avgSpeed-posStart)/cnt*60
		context:Log("Average [uL/min]: {0} flow={1:0.000}, {3} speed={2:0.000}", flow_channel, flowAvg, avgSpeed, speed_channel)
		return flowAvg, avgSpeed, true, true
	end

	---Monitor a flow and speed channel
	---@param flow_channel Channel
	---@param speed_channel Channel
	---@param method table
	---@param testPressure number [bar]
	---@param yield_function function
	---@param ... string
	---@return number
	---@return number
	local function monitor_channel2(flow_channel, speed_channel, method, testPressure, yield_function, ...)
		local flow, speed, passedFlow, _ = monitor_channel(flow_channel, speed_channel, method.MaxVolume, method.MaxPressureDeviation, testPressure, method.MeasuringTime, yield_function)
		if (passedFlow == false) then
			-- out of solvent, leak, air
			local msg = DotNetString.Format("Pump {0} solvent consumption too large - this could be due to trapped air or excessive leakage.", speed_channel)
			report_leak(msg, ...)
			pr.decompressSystem(context)
			context:Abort()
		end
		return flow, speed
	end

	---Calculate a new limit depending onthe used pressure
	---@param p1 number
	---@param p2 number
	---@param lmt number
	---@return number
	local function getLimit(p1, p2, lmt)
		if p2 < p1 then
			context:Log("--- Testing with reduced pressure: {0} bar", p2)
			lmt = lmt*p2/p1
		end
		return lmt
	end

	---Low pressure leak test
	---@param yield_function function
	local function lowPressureTest(yield_function)
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Low pressure leak test", true))

		local test = "Low pressure leak test"
		local lpt = "Low pressure leak test. This takes about 9 minutes."
		if selfTest == true then
			test = "Self LT: Pump A+B @50bar"
			context:Log("Low pressure self test started")
			if (self_Test.showMessage == true) then status:SetStatus(test) end
		else
			if (self_Test.showMessage == true) then status:SetStatus(lpt) end
		end
		local degas_zr_A = true
		local degas_zr_B = true

		context:Log(baltic.devider)
		context:Log(test)
		context:Log(baltic.devider)
		local method = {}
		method.DegasPressure 		= 50				-- bar
		method.BuildPressureSpeed 	= 300				-- uL/min
		method.HoldTime 			= 300				-- seconds	
		method.PumpEmptySpeed 		= 2000				-- uL/min
		method.MaxBuildVolumeA 		= 50				-- uL
		method.MaxBuildVolumeB 		= 50				-- uL
		method.MaxIterations 		= 10
		method.MinIterations 		= 1

		---Refill a pump
		---@param channel Channel
		---@param yield_func function
		local function refill(channel, yield_func)
			context:Log("Refill {0}", channel)
			if zr.IsFull(pump, channel) then return end -- don't waste valve switch
			zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent, nil)
			pf.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpRefillSpeed, pump, yield_func)
			while not zr.IsFull(pump, channel) do yield_func() end
			parallel.sleep(yield_func, 2000)
			pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
		end

		---Degassing a channel
		---@param channel Channel
		---@param reportTest boolean
		---@param yield_func function
		---@return number
		---@return boolean
		local function degas_channel(channel, reportTest, yield_func)
			local posBefore, posStartPressurizing = 0, 0
			local testDegasOK = true
			refill(channel, yield_func)
			pf.Manualmode_Pump_constantSpeed(channel, 300, pump, yield_func)
			parallel.sleep(yield_func, 2000)
			pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_func)
            parallel.sleep(yield_func, 4000)
			posStartPressurizing = pump:GetPistonPosition(channel)
			zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Compress, nil)
			parallel.sleep(yield_func, 2000)
			pf.Manualmode_Pump_constantSpeed(channel, 3, pump, yield_func)
			parallel.sleep(yield_func, 1000)	-- 50nL infeed due hysteresis of the pump
			pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_func)
			local buildPresTime = nowTime()
			build_pressure(channel, method.DegasPressure, yield_func)
			parallel.sleep(yield_func, 2000)
			local p = "Pump A"
			if channel == zr.B then p = "Pump B" end
			if nowTime() > (buildPresTime+30) then
				pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_func)
				if selfTest == false then
					context:Report(p, Severity.Warn, true, "Pressure ({0} bar) not reached.", method.DegasPressure)
				end
				context:Log("{0} could not reach the target pressure of {1} bar.", p, method.DegasPressure)
				testDegasOK = false
				posBefore = pump:GetPistonPosition(channel) - posStartPressurizing
				parallel.sleep(yield_func, 2000)
			else
				parallel.sleep(yield_func, 180*1000)
				posBefore = pump:GetPistonPosition(channel) - posStartPressurizing
				parallel.sleep(yield_func, method.HoldTime*1000)
				local posAfter = pump:GetPistonPosition(channel) - posStartPressurizing
				pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_func)

				if reportTest then
					local holdFlow = posAfter-posBefore
					local holdFlowPerMin = holdFlow*60/method.HoldTime
					local Text = "Pump A"
					local lmt = limit.lpfA
					local lmtBP = limit.lpbA
					if channel == zr.B then
						lmt = limit.lpfB
						lmtBP = limit.lpbB
						Text = "Pump B"
						results[2] = (holdFlowPerMin*1000)
					else
						results[1] = (holdFlowPerMin*1000)
					end
					if holdFlowPerMin > lmt and selfTest == false then
						context:Report(Text, Severity.Warn, true, "The flow ({0:0.000} \181L/min) is too high at pressure ({1} bar).", holdFlowPerMin, method.DegasPressure)
						testDegasOK = false
					end
					if posBefore > lmtBP and selfTest == false then
						context:Report(Text, Severity.Warn, true, "The volume ({0:0.000} \181L) for building pressure ({1} bar) is too high.", posBefore, method.DegasPressure)
						testDegasOK = false
					end
					context:Log("Degas {0}: build pressure vol={1:0.000} \181L, hold pressure vol={2:0.00} nL", Text, posBefore, holdFlow*1000)
					context:Log("{0}: the leakage rate is: {1:0.000} \181L/min at a pressure of: {2:0.00} bar.", Text, holdFlowPerMin, method.DegasPressure)
				end
			end
			return posBefore, testDegasOK
		end

		-- start degassing here
		if degas_zr_A or degas_zr_B then
			settings = pump:GetSettings()
			zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)
			local digOut = pump:GetDigitalOutputs()
			local degasserOnTime = nowTime()
			if digOut < 32  then	-- 32 == DigitalOutput.PO2 is already true
				pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
				degasserOnTime = degasserOnTime + baltic.preOntimeDegasser
			end

			pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false)		-- set max pump pressure
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 300, sleep_200, baltic.smooth)

			-- wait until degasser is on for the time defined in baltic.preOntimeDegasser
			while (nowTime() < degasserOnTime) do sleep_200() end

			local reportTest = basic or selfTest	-- PBNE-773: the results of the low pressure leak test are always enabled in extended diagnose, not enabled in smart diagnose
--			local reportTest = factory
--			if OQ then reportTest = OQ end

			context:Log("Degas channel A and B")
			local p_degas_a = { degas_channel, zr.A, reportTest, parallel.yield }
			local p_degas_b = { degas_channel, zr.B, reportTest, parallel.yield }
			local a, testA, b, testB = parallel.run(sleep_200, p_degas_a, p_degas_b)
			context:Log("Compression volume A {0} \181L:", a)
			context:Log("Compression volume B {0} \181L:", b)
			if not testA then
				context:Log("Degassing channel A failed")
				testOK = false
			end
			if not testB then
				context:Log("Degassing channel B failed")
				testOK = false
			end
			results[3] = a
			results[4] = b

			pump:SetSettings(settings)
			local caller = "Degas"
			zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)
			zr.logInstrSettings(context, settings, caller)
		end

		reportResults(test, 50)		-- pressure is not taken into account here
		-- switch degasser off if it is switched on outside of this function
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
		context:Sleep(1000)
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, yield_function)
		pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, yield_function)
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Self LP A: Compression volume", results[3], "\181L"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Self LP B: Compression volume", results[4], "\181L"))
		if selfTest == false then
			if (self_Test.showMessage == true) then status:RemoveStatus(lpt) end
		else
			if (self_Test.showMessage == true) then status:RemoveStatus(test) end
		end
		csv.logValueInCSVFile(context, csvFileName, test, "LP A: Compression volume", results[3], "µL", -1)
		csv.logValueInCSVFile(context, csvFileName, test, "LP B: Compression volume", results[4], "µL", -1)
	end

	local backpressuretest_subject = "Flow restriction test"

	---Report a clogage
	---@param message string?
	---@param ... string
	local function report_clog(message, ...)
		context:Sleep(1000)
		local subject = gen_subject_string(backpressuretest_subject, ...)
		context:Report(subject, Severity.Warn, true, message or "Blockage detected somewhere in the highlighted components.")
		context:WaitForSignal(subject)
		testOK = false
	end

	---Calculate the limit for route 1
	---@param press number
	---@param baseLimit number
	---@param withTrap boolean
	---@param withTransferLine boolean
	---@param withMixTeeCapillary boolean
	---@param withRestrictionCapillary boolean
	---@return number
	---@return number
	local function calcLimitRoute1(press, baseLimit, withTrap, withTransferLine, withMixTeeCapillary, withRestrictionCapillary)
		local pA = math.min(press, pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, withTrap, false, press))
		if withTrap then pA = pf.getTrapColumnPressure(context:GetArgumentValue("trap"), pressSettings.GradientPumpMaxTargetPressure, pA) end
		local lmtRoute1 = getLimit(press, pA, baseLimit)
		local expectedFlow = baltic.unityFlowRoute1_H2O * pA
		local pressureVAtoFS = chrom.capillary_pressure(installed, N.ValveAToFS, expectedFlow, chrom.viscosity_H2O_20C, true)
		local pressureTrap = 0
		if withTrap then pressureTrap = chrom.column_pressure(context:GetArgumentValue("trap"), pressSettings.GradientPumpMaxTargetPressure, expectedFlow, chrom.viscosity_H2O_20C) + chrom.capillary_pressure(installed, N.Trap, expectedFlow, chrom.viscosity_H2O_20C, true) end
		if (withTrap == false) then pressureTrap = 0 end
		local pressureTransfer = chrom.capillary_pressure(installed, N.TransferLine, expectedFlow, chrom.viscosity_H2O_20C, true)
		if (withTransferLine == false) then pressureTransfer = 0 end
		local pressureMTtoVT = chrom.capillary_pressure(installed, N.MixTeeToTrapValve, expectedFlow, chrom.viscosity_H2O_20C, true)
		if (withMixTeeCapillary == false) then pressureMTtoVT = 0 end
		local pressureFStoMT = chrom.capillary_pressure(installed, N.FSAToMixTee, expectedFlow, chrom.viscosity_H2O_20C, true)
		if (withRestrictionCapillary == false) then pressureFStoMT = 0 end
		local testPressureA = pressureTrap + pressureVAtoFS + pressureFStoMT + pressureMTtoVT + pressureTransfer
		lmtRoute1 = lmtRoute1 * (pA/testPressureA)
		if (lmtRoute1 > 2) then 			-- normalize pA to a limit of 2
			pA = pA * (2/lmtRoute1)
			lmtRoute1 = 2
		end
		lmtRoute1 = pf.noExp((lmtRoute1 + 0.005), 2)	-- round to 2 digits

		return lmtRoute1, pA
	end

	---Calculate the limit for route 2
	---@param press number
	---@return number
	---@return number
	local function calcLimitRoute2(press)
		local p = math.min(press, pf.setMaxPressureLimitB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false, nil))	-- flow path is w/o columns; press limit is max pump press + 20 bar
		local testLimit = getLimit(press, p, limit.bpfB)
		local expectedFlow = baltic.unityFlowRoute2_ACN * p
		local pressureVBtoFS = chrom.capillary_pressure(installed, N.ValveBToFS, expectedFlow, chrom.viscosity_ACN_20C, true)
		local pressureFStoMT = chrom.capillary_pressure(installed, N.FSBToMixTee, expectedFlow, chrom.viscosity_ACN_20C, true)
		local pressureMTtoVT = chrom.capillary_pressure(installed, N.MixTeeToTrapValve, expectedFlow, chrom.viscosity_ACN_20C, true)
		local TestPressureB = pressureVBtoFS + pressureFStoMT + pressureMTtoVT
		testLimit = (p/TestPressureB)*testLimit
		testLimit = pf.noExp((testLimit + 0.005), 2)	-- round to 2 digits
		return testLimit, p
	end

	---Calculate the limit for route 3
	---@param press number
	---@param baseLimit number
	---@param withLoop boolean
	---@return number
	---@return number
	local function calcLimitRoute3(press, baseLimit, withLoop)
		local pA = math.min(press, pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false, nil))	-- flow path is w/o columns; press limit is max pump press + 20 bar
		local testLimit = getLimit(press, pA, baseLimit)
		local expectedFlow = baltic.unityFlowRoute3_H2O * pA
		local pressureVAtoVI = chrom.capillary_pressure(installed, N.ValveAToInjectionValve, expectedFlow, chrom.viscosity_H2O_20C, false)
		local pressureLoop = 0
		if withLoop then pressureLoop = chrom.loop_pressure(installed, expectedFlow, chrom.viscosity_H2O_20C) end
		local pressureVItoVT = chrom.capillary_pressure(installed, N.InjectToTrap, expectedFlow, chrom.viscosity_H2O_20C, false)
		local TestPressureA = pressureVAtoVI + pressureLoop + pressureVItoVT
		testLimit = (pA/TestPressureA)*testLimit
		testLimit = pf.noExp((testLimit + 0.005), 2)	-- round to 2 digits
		return testLimit, pA
	end

	---Flow restriction test route 1 (pump A - mixTee)
	---@param method table
	---@param yield_func function
	---@return boolean
	local function flowRestrictionTest_PumpAMixTee(method, yield_func)
		status:SetStatus("Pump A MixTee")

		context:Log("-------------------------------------------------------")
		context:Log("------- Flow restriction test injection system --------")
		context:Log("-------------------------------------------------------")

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow restriction test pump A mixTee", true))

		if selfTest == false then
			if pf.getColumnMaxPressure(context:GetArgumentValue("trap"), pressSettings.GradientPumpMaxTargetPressure) < method.TestPressureA then
				context:Report("Flow restriction test pressure", Severity.Info, true, "The maximum pressure is below test pressure. Replace trap by a tubing and chose 'None' to test with nominal test pressure.")
				context:WaitForSignal("Flow restriction test pressure")
				testOK = false
				status:RemoveStatus("Pump A MixTee")
				return true
			end
		end

		---Monitor the flow channel A
		---@param press number
		---@return boolean|number
		---@return boolean|number
		local function monitor_channel_2(press)
			build_pressure(zr.A, pf.noExp(press), yield_func)
			local flow, speed = monitor_channel(zr.A, zr.A, method.MaxVolume, method.MaxPressureDeviation, press-2, method.MeasuringTime, yield_func)
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 20, yield_func, baltic.smooth)
			return flow, speed
		end

		---Flow restriction test Route 1
		---@param limit_1 number
		---@param tP1_1 number
		---@param flag number
		---@return number
		local function Route_1(limit_1, tP1_1, flag)
			context:Log("--- Testing channel A through flow sensor")
			context:ShowComposition(true)
			set_valves(baltic.PumpValve.MixTee, baltic.PumpValve.Compress, nil, baltic.TrapValve.GradientT)
			pr.Signalize_Reset(context)
			context:Signalize(baltic.ColorsRGB.LightGray, N.Separator, N.Separator)
			context:Log("Testpressure for Route_1: {0}bar",tP1_1)
			context:Log("--- Flow A limit is: >{0}", limit_1)
			local flow, _ = monitor_channel_2(tP1_1)
			if flow == false then return -2 end
			if pump:GetCurrentFlow(zr.B) < -50 then return -1 end
			context:Log("Testpressure for Route_1: {0}bar",tP1_1)
			context:Log("--- Flow A limit is: >{0}", limit_1)
			results[5] = flow
			if (flow < limit_1) then
				return 1		-- trap
			end
			return flag
		end

		---Flow restriction test Route 1a without the trap
		---@param limit_1a number
		---@param tP1_1a number
		---@param flag number
		---@return number
		local function Route_1a(limit_1a, tP1_1a, flag)
			context:Log("--- Testing channel A through flow sensor without trap")
			set_valves(nil, nil, nil, baltic.TrapValve.GradientA)
			context:Log("Testpressure for Route_1a: {0}bar",tP1_1a)
			context:Log("--- Flow A limit is: >{0}", limit_1a)
			local flow, _ = monitor_channel_2(tP1_1a)
			if flow == false then return -2 end
			context:Log("Testpressure for Route_1a: {0}bar",tP1_1a)
			context:Log("--- Flow A limit is: >{0}", limit_1a)
			results[6] = flow
			if (flow < limit_1a) then
				return 2		-- transfer line
			end
			return flag
		end

		---Flow restriction test Route 1b without the trap and without the transfer tubing
		---@param limit_1b number
		---@param tP1_1b number
		---@param flag number
		---@return number
		local function Route_1b(limit_1b, tP1_1b, flag)
			context:Log("--- Testing channel A through flow sensor without trap valve")
			if selfTest == false then
				set_valves(nil, nil, nil, baltic.TrapValve.Waste)
			else
				set_valves(baltic.PumpValve.MixTee, baltic.PumpValve.Compress, nil, baltic.TrapValve.Waste)
			end
			context:Log("Testpressure for Route_1B: {0}bar",tP1_1b)
			context:Log("--- Flow A limit is: >{0}", limit_1b)
			local flow, _ = monitor_channel_2(tP1_1b)
			if flow == false then return -2 end
			context:Log("Testpressure for Route_1B: {0}bar",tP1_1b)
			context:Log("--- Flow A limit is: >{0}", limit_1b)
			results[7] = flow
			if (flow < limit_1b) and selfTest == false then
				return 3		-- all path to trap valve
			end
			return flag
		end

		---Flow restriction test Route 1b without the trap and without the mixTee capillary
		---@param limit_1c number
		---@param tP1_1c number
		---@param flag number
		---@return number
		local function Route_1c(limit_1c, tP1_1c, flag)
			context:Log("--- Testing channel A through flow sensor without mixTee capillary")
			set_valves(nil, nil, nil, baltic.TrapValve.Waste)
			context:Log("Testpressure for Route_1C: {0}bar",tP1_1c)
			context:Log("--- Flow A limit is: >{0}", limit_1c)
			local flow, _ = monitor_channel_2(tP1_1c)
			if flow == false then return -2 end
			context:Log("Testpressure for Route_1C: {0}bar",tP1_1c)
			context:Log("--- Flow A limit is: >{0}", limit_1c)
			results[8] = flow
			if (flow < limit_1c) then
				return 4		-- all path to mixTee
			end
			return flag
		end

		---Flow restriction test Route 1b without the trap and without the restriction capillary
		---@param limit_1d number
		---@param tP1_1d number
		---@param flag number
		---@return number
		local function Route_1d(limit_1d, tP1_1d, flag)
			context:Log("--- Testing channel A through flow sensor without restriction capillary")
			context:Log("Testpressure for Route_1D: {0}bar",tP1_1d)
			context:Log("--- Flow A limit is: >{0}", limit_1d)
			local flow, _ = monitor_channel_2(tP1)
			if flow == false then return -2 end
			context:Log("Testpressure for Route_1D: {0}bar",tP1_1d)
			context:Log("--- Flow A limit is: >{0}", limit_1d)
			results[9] = flow
			if (flow < limit_1d) then
				return 5		-- all path to flow sensor
			end
			return flag
		end

		---Report the flow restriction test result
		---@param flag number
		local function report_BPT(flag)
			if flag == -2 then
				context:Report("Flow restriction test route 1", Severity.Warn, true, "Pump A is empty or the solvent consumption exceeded 400uL.")
				testOK = false
			else
				if flag == -1 then
					context:Signalize(baltic.ColorsRGB.Normal, N.ValveBToFS, N.FlowB, N.ValveB)
					report_clog(nil, N.ValveBToFS, N.FlowB, N.ValveB)
				else
					if flag == 1 then
						context:Signalize(baltic.ColorsRGB.Normal, N.Trap)
						report_clog(nil, N.Trap)
					else
						if flag == 2 then
							context:Signalize(baltic.ColorsRGB.Normal, N.TransferLine)
							report_clog(nil, N.TransferLine)
						else
							if flag == 3 then
								context:Signalize(baltic.ColorsRGB.Normal, N.MixTeeToTrapValve)
								report_clog(nil, N.MixTeeToTrapValve)
							else
								if flag == 4 then
									context:Signalize(baltic.ColorsRGB.Normal, N.FSAToMixTee)
									report_clog(nil, N.FSAToMixTee)
								else
									if flag == 5 then
										context:Signalize(baltic.ColorsRGB.Normal, N.ValveAToFS, N.FlowA)
										report_clog(nil, N.ValveAToFS, N.FlowA)
										if flag == 6 then
											context:Signalize(baltic.ColorsRGB.Normal, N.PumpAFrontToValve, N.ValveA)
											report_clog(nil, N.PumpAFrontToValve, N.ValveA)
										end
									end
								end
							end
						end
					end
				end
			end
		end

		zr.ChangePressurePID(context, pump, baltic.PPID.PID_25_1_1)

		if selfTest == false then
			context:WaitForSignal("Flow restriction test pumpA mixTee")
			-- subsystem A clogged?
			local flag = 0
	--		local limit = calcLimitRoute1(method.TestPressureA, limit.bpfA)			-- this is already calculated @ FRT start
			flag = Route_1(limitRoute1, tP1, flag)
			if flag > 0 then		-- without trap
				testOK = false
				limitRoute1, tP1 = calcLimitRoute1(method.TestPressureA, limit.bpfA, false, true, true, true)
				flag = Route_1a(limitRoute1, tP1, flag)							-- limit is higher due to no trap
				if flag > 1 then	-- without trap and transfer line
					limitRoute1, tP1 = calcLimitRoute1(method.TestPressureA, limit.bpfA, false, false, true, true)
					flag = Route_1b(limitRoute1, tP1, flag)	-- limit is higher due to no trap and no transfer line
					if flag > 2 then	-- without trap, transfer line and mixTee capillary
						context:Report("Flow restriction test route 1b", Severity.Info, true, "Restriction is too high. Remove the mixTee capillary and click 'continue'")
						context:Signalize(baltic.ColorsRGB.LightGray, N.ValveTShortGroove, N.TrapValveToWaste)
						context:Signalize(baltic.ColorsRGB.Gold, N.MixTeeToTrapValve)
						limitRoute1, tP1 = calcLimitRoute1(method.TestPressureA, limit.bpfA, false, false, false, true)
						context:WaitForSignal("Flow restriction test route 1b")
						context:Signalize(baltic.ColorsRGB.LightGray, N.MixTeeToTrapValve)
						flag = Route_1c(limitRoute1, tP1, flag)	-- limit is higher due to no trap, no transfer line and no mixTee capillary
						if flag > 3 then	-- without trap, transfer line, mixTee capillary and restriction capillary
							context:Report("Flow restriction test route 1c", Severity.Info, true, "Restriction is too high. Remove the restriction capillary and click 'continue'")
							context:Signalize(baltic.ColorsRGB.LightGray, N.MixTeeToTrapValve)
							context:Signalize(baltic.ColorsRGB.Gold, N.FSAToMixTee)
							limitRoute1, tP1 = calcLimitRoute1(method.TestPressureA, limit.bpfA, false, false, false, false)
							context:WaitForSignal("Flow restriction test route 1c")
							context:Signalize(baltic.ColorsRGB.LightGray, N.FSAToMixTee)
							flag = Route_1d(limitRoute1, tP1, flag)	-- limit is higher due to no trap, no transfer line, no mixTee capillary and no restriction capillary
							if flag > 4 then	-- check pump and front capillary
								set_valves(baltic.PumpValve.Waste, nil, nil, nil)
								context:Sleep(500)
								pf.Manualmode_Pump_constantPressure(zr.A, 10, pump, yield_func)
								context:Sleep(2500)
								local press = pump:GetCurrentPressure(zr.A)
								context:Sleep(100)
								pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, yield_func)
								local testLimit = 5
								context:Log("--- Pressure A limit is: <{0}", testLimit)
								if (press > testLimit) then
									flag = 6
								end
							end
						end
					end
				end
			end
			report_BPT(flag)
			parallel.sleep(yield_func, 1000)
			if (flag ~= 0) then
				status:RemoveStatus("Pump A MixTee")
				return true
			end
		else
			Route_1b(limitRoute1, tP1, 0)
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Self FRT: Pump A MixTee", results[7], "\181L/min"))
			csv.logValueInCSVFile(context, csvFileName, "Self FRT: Pump A MixTee", "FR: Route 1b", results[7], "µL/min", -1)

		end
		-- done
		pf.reducePressure(context, pump, zr, zr.A, nil, 50, 50, 300, yield_func, baltic.smooth)
		set_valves(baltic.PumpValve.Waste, nil, nil, nil)
		status:RemoveStatus("Pump A MixTee")
		return false
	end

	---Flow restriction test route 2 (pump B - mixTee)
	---@param method table
	---@param yield_func function
	---@return boolean
	local function flowRestrictionTest_PumpBMixTee(method, yield_func)
		status:SetStatus("Pump B MixTee")

		context:Log("-------------------------------------------------------")
		context:Log("--- Flow restriction test pump B and mixTee system ----")
		context:Log("-------------------------------------------------------")

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow restriction test pump B mixTee", true))

		---Monitor the flow and speed channel
		---@param channel Channel
		---@param press number
		---@return boolean|number
		---@return boolean|number
		local function monitor_channel_FR(channel, press)
			build_pressure(channel, pf.noExp(press), yield_func)
			local flow, speed = monitor_channel(channel, channel, method.MaxVolume, method.MaxPressureDeviation, press-2, method.MeasuringTime, yield_func)
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 20, yield_func, baltic.smooth)
			return flow, speed
		end
		-- subsystem B clogged?
		if selfTest == false then
			context:Log("--- Testing channel B through flow sensor")
		else
			context:Log("Self testing channel B through flow sensor")
		end
		context:ShowComposition(true)
		if pump:GetSetManualValvePosition(zr.A) == baltic.PumpValve.MixTee then
			set_valves(baltic.PumpValve.Waste, baltic.PumpValve.MixTee, nil, baltic.TrapValve.Waste)
		else
			set_valves(nil, baltic.PumpValve.MixTee, nil, baltic.TrapValve.Waste)
		end
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_18_1_1)
		context:Log("Testpressure for Route 2: {0}bar",tP2)
		context:Log("--- Flow B limit is: >{0}", limitRoute2)
		local flow, _ = monitor_channel_FR(zr.B, tP2)
		if flow == false then
			if selfTest == false then
				context:Report("Flow restriction test Pump B and MixTee system", Severity.Warn, true, "Pump B is empty or the solvent consumption exceeded 400\181L.")
			else
				context:Log("Pump B is empty or the solvent consumption exceeded 400\181L.")
			end
			testOK = false
			status:RemoveStatus("Pump B MixTee")
			return true
		end
		results[10] = flow
		if selfTest == false then
			context:Log("Testpressure for Route 2: {0}bar",tP2)
			context:Log("--- Flow B limit is: >{0}", limitRoute2)
			-- assess flow/speed
			if (flow < limitRoute2) then
				set_valves(nil, baltic.PumpValve.Waste, nil, nil)
				context:Sleep(500)
				pf.Manualmode_Pump_constantPressure(zr.B, 10, pump, yield_func)
				context:Sleep(2500)
				local press = pump:GetCurrentPressure(zr.B)
				context:Sleep(100)
				pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, yield_func)
				local testLimit = 5
				context:Log("--- Pressure B limit is: <{0}", testLimit)
				pf.reducePressure(context, pump, zr, nil, zr.B, 50, 50, 300, yield_func, baltic.smooth)
				if (press < testLimit) then
					report_clog(nil, N.ValveBToFS, N.ValveB, N.FlowB, N.FSBToMixTee, N.MixTee, N.MixTeeToTrapValve, N.ValveT, N.TrapValveToWaste)
					status:RemoveStatus("Pump B MixTee")
					return true
				else
					report_clog(nil, N.PumpBFrontToValve, N.ValveB)
					status:RemoveStatus("Pump B MixTee")
					return true
				end
			end
		else
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Self FRT: Pump B MixTee", results[10], "\181L/min"))
			csv.logValueInCSVFile(context, csvFileName, "Self FRT: Pump B MixTee", "FR: Route 2", results[10], "µL/min", -1)
		end
		-- done
		pf.reducePressure(context, pump, zr, nil, zr.B, 50, 50, 300, yield_func, baltic.smooth)
		set_valves(nil, baltic.PumpValve.Waste, nil, nil)
		status:RemoveStatus("Pump B MixTee")
		return false
	end

	---Flow restriction test route 3 (injection path)
	---@param method table
	---@param yield_func function
	---@return boolean
	local function flowRestrictionTest_Injection_System(method, yield_func)
		context:Log("-------------------------------------------------------")
		context:Log("------- Flow restriction test injection system --------")
		context:Log("-------------------------------------------------------")

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow restriction test injection system", true))

		if selfTest == false then
			if pf.getColumnMaxPressure(context:GetArgumentValue("trap"), pressSettings.GradientPumpMaxTargetPressure) < method.TestPressureA then
				context:Report("Flow restriction test pressure", Severity.Info, true, "The maximum pressure is below test pressure. Replace trap by a tubing and chose 'None' to test with nominal test pressure.")
				context:WaitForSignal("Flow restriction test pressure")
				testOK = false
				return true
			end
		end
		---Monitor the flow and speed channel
		---@param channel Channel
		---@param press number
		---@return boolean|number
		---@return boolean|number
		local function monitor_channel_FRI(channel, press)
			build_pressure(channel, pf.noExp(press), yield_func)
			local flow, speed = monitor_channel(channel, channel, method.MaxVolume, method.MaxPressureDeviation, press-2, method.MeasuringTime, yield_func)
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 20, yield_func, baltic.smooth)
			return flow, speed
		end

		-- injection subsystem clogged?
		status:SetStatus("Injection System")
		if selfTest == false then
			context:Log("--- Testing injection path")
		else
			context:Log("Self testing injection path")
		end
		-- If the limit is higher means a higher flow is expected.
		-- With a higher flow the pump has problems to reach the set pressure.
		-- The flow through the Idex loading capillary is expected 2.44 x higher than with the nanoViper capillary.
		-- Therefor the integral part must be recalculated depending on the unityflow.
		-- Experiments showed that the unityFlow x 1000 is a sufficent value for the I-part of the PID values.
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_25_12_1)				-- PBNE-827

		-- for new fw we are not finetuning I parameter
		-- local unityFlowInjectionPath = 1/(1/chrom.capillary_flow_byName(installed, N.ValveAToInjectionValve, 1, chrom.viscosity_H2O_20C)+1/chrom.capillary_flow_byName(installed, N.InjectToTrap, 1, chrom.viscosity_H2O_20C))
		-- zr.ChangePressurePID(context, pump, {I=unityFlowInjectionPath*1000})		-- I~400 for Idex (350/100); I~150 for (Idex (250/25); I~50 for nanoViper (350/20)

		set_valves(nil, nil, baltic.InjectionValve.Load, baltic.TrapValve.InjectWaste)
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject, true)		-- keeping pressure
		pr.Signalize_Reset(context)
		context:Log("Testpressure for Route 3: {0}bar",tP3)
		context:Log("--- Speed A limit is: >{0}",limitRoute3)
		local _, speed = monitor_channel_FRI(zr.A, tP3)
		if selfTest == false then
			if (speed == false) then
				context:Report("Flow restriction test route 3", Severity.Warn, true, "Pump A is empty or the solvent consumption exceeded 400uL.")
				testOK = false
				return true
			end
		end
		results[11] = speed
		if selfTest == false then
			context:Log("Testpressure for Route 3: {0}bar",tP3)
			context:Log("--- Speed A limit is: >{0}",limitRoute3)
			-- assess flow/speed
			if (speed < limitRoute3) then
				testOK = false
				status:SetStatus("w/o loop")
				-- Check if loop is blocked
				limitRoute3, tP3 = calcLimitRoute3(method.TestPressureA, limit.bpsA, false)
				set_valves(nil, nil, baltic.InjectionValve.Inject, baltic.TrapValve.InjectWaste)
				context:Log("Testpressure for Route 3 w/o loop: {0}bar",tP3)
				context:Log("--- Speed A limit is: >{0}",limitRoute3)
				_, speed = monitor_channel_FRI(zr.A, tP3)
				if speed == false then
					context:Report("Flow restriction test injection system w/o loop", Severity.Warn, true, "Pump A is empty or the solvent consumption exceeded 400uL.")
					testOK = false
					return true
				end
				results[12] = speed
				context:Log("Testpressure for Route 3 w/o loop: {0}bar",tP3)
				context:Log("--- Speed A limit is: >{0}",limitRoute3)
				status:RemoveStatus("w/o loop")

				if (speed < limitRoute3) then
					-- The loop is not the blockage
					report_clog(nil, N.PumpARearToValve, N.ValveA, N.ValveAToInjectionValve, N.ValveI,  N.InjectToTrap, N.ValveT)
				else
					report_clog(nil, N.Loop)
				end
				testOK = false
				return true
			end
		else
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Self FRT: Injection Path", results[11], "\181L/min"))
			csv.logValueInCSVFile(context, csvFileName, "Self FRT: Injection Path", "FR: Route 3 with loop", results[11], "µL/min", -1)
		end

		status:RemoveStatus("Injection System")

		-- done
		pf.reducePressure(context, pump, zr, nil, zr.B, 50, 50, 300, yield_func, baltic.smooth)
		set_valves(baltic.PumpValve.Waste, baltic.PumpValve.Waste, nil, nil)
		pr.Signalize_Reset(context)
		context:Sleep(500)
		return false
	end

	---Leak test of pump A or B
	---@param test string
	---@param testPressure table
	---@param channel Channel
	---@param ltPump boolean
	---@param yield_func function
	---@param MaxVolume number [µL]
	---@param MaxPressureDeviation number
	---@param MeasuringTime number [s]
	---@return number|boolean
	local function leakTest_Pump(test, testPressure, channel, ltPump, yield_func, MaxVolume, MaxPressureDeviation, MeasuringTime)
		local DotNetStr = luanet.import_type("System.String")
		local speed, volumePassed = nil, false
		---@type number|boolean
		local returnValue = false

		if (ltPump == true) then
			local txt = test
			if (channel == zr.A) then
				txt = txt.." pump A"
			else
				txt = txt.." pump B"
			end
			status:SetStatus(txt)
			zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Compress, nil)

			for n=1, #testPressure do
				build_pressure(channel, testPressure[n], yield_func)
				parallel.sleep(yield_func, 30000)
				if (continousLeakTestMeasurement == true) then		-- display continously the pump volume (== leakage)
					local counter = 0								-- if the value is stable: == no leak
					local volume = 0								-- if the volume decrease, pump is leaking
					local leakage = 0.0
					local msg = DotNetStr.Format("Pump {0} volume: {1} nL/min", channel, leakage)
					while (counter < 300) do		-- stop after 10 minutes
						status:SetStatus(msg)
						volume = pump:GetPistonPosition(channel)
						parallel.sleep(yield_func,2000)
						leakage = (pump:GetPistonPosition(channel)-volume)*30000
						leakage = pf.noExp(leakage,1)
						status:RemoveStatus(msg)
	--					parallel.sleep(yield_func,200)
						msg = DotNetStr.Format("Pump {0} leakage: {1} nL/min", channel, leakage)
						counter = counter + 1
					end
				else
					_, speed, _, volumePassed = monitor_channel(channel, channel, MaxVolume, MaxPressureDeviation, testPressure[n], MeasuringTime, yield_func)
				end
				if (speed ~= nil) and (volumePassed == true) then
					if (testPressure[n] == 1000) then returnValue = speed end
					local decimalPlaces = pr.getMetaDataPrecision(speed)
					if (channel == zr.A) then
						journal:Delete("LT: Pump A @"..testPressure[n].."bar")
						journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "LT: Pump A @"..testPressure[n].."bar", speed,"\181L/min", decimalPlaces))
						csv.logValueInCSVFile(context, csvFileName, "LT: Pump A @"..testPressure[n].."bar", "Average piston speed A", speed, "\181L/min", -1)
					else
						journal:Delete("LT: Pump B @"..testPressure[n].."bar")
						journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "LT: Pump B @"..testPressure[n].."bar", speed,"\181L/min", decimalPlaces))
						csv.logValueInCSVFile(context, csvFileName, "LT: Pump B @"..testPressure[n].."bar", "Average piston speed B", speed, "\181L/min", -1)
					end
				end
			end
			status:RemoveStatus(txt)
		end
		return returnValue
	end

	---Leak test mixTee system
	---@param method table
	---@param yield_func function
	---@return number
	local function leakTest_MixTeeSystem(method, yield_func)
		local flowB, flowA

		local function Signalize_MixT_TransferLine_wTrap()
			pr.Signalize_Reset(context)
			context:Sleep(500)
			context:Signalize(baltic.ColorsRGB.LightGray, N.Separator, N.Separator)
			context:Signalize(baltic.ColorsRGB.Red, N.FSAToMixTee, N.FlowA, N.ValveAToFS)
			context:SignalizeText(baltic.ColorsRGB.White, N.FlowA)
		end
		local function Signalize_MixT_woTransferLine_woTrap()
			pr.Signalize_Reset(context)
			context:Sleep(500)
			context:Signalize(baltic.ColorsRGB.Red, N.PumpBFrontToValve, N.ValveBGroove, N.ValveBToFS, N.FlowB, N.FSBToMixTee, N.MixTeeToTrapValve, N.FSAToMixTee, N.FlowA, N.ValveAToFS)
			context:Signalize(baltic.ColorsRGB.LightGray, N.PumpARearToValve, N.ValveAToInjectionValve, N.InjectToTrap, N.TrapValveToWaste, N.Trap, N.ValveAGroove, N.ValveIGroovesLoad, N.ValveTGrooves)
			context:Signalize(baltic.ColorsRGB.DarkGray, N.Loop, N.Loop)
			context:SignalizeText(baltic.ColorsRGB.White, N.FlowA, N.FlowB)
		end
		local function Signalize_MixT_ValveA()
			pr.Signalize_Reset(context)
			context:Sleep(500)
			context:Signalize(baltic.ColorsRGB.Red, N.PumpBFrontToValve, N.ValveBGroove, N.ValveBToFS, N.FlowB, N.FSBToMixTee, N.FSAToMixTee, N.FlowA, N.ValveAToFS)
			context:Signalize(baltic.ColorsRGB.LightGray, N.PumpARearToValve, N.ValveAToInjectionValve, N.InjectToTrap, N.TrapValveToWaste, N.Trap, N.ValveAGroove, N.ValveIGroovesLoad, N.ValveTGrooves)
			context:Signalize(baltic.ColorsRGB.DarkGray, N.Loop, N.Loop)
			context:SignalizeText(baltic.ColorsRGB.White, N.FlowA, N.FlowB)
		end
		local function Signalize_MixT_ValveB()
			pr.Signalize_Reset(context)
			context:Sleep(500)
			context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.ValveAGroove, N.ValveAToFS, N.FlowA, N.FSAToMixTee, N.FSBToMixTee, N.FlowB, N.ValveBToFS)
			context:Signalize(baltic.ColorsRGB.LightGray, N.ValveTGrooves)
			context:Signalize(baltic.ColorsRGB.DarkGray, N.Loop)
			context:SignalizeText(baltic.ColorsRGB.White, N.FlowA, N.FlowB)
		end

		local function checkWithOutTrap(press)
			status:SetStatus("w/o Trap")
			set_valves(nil, nil, nil, baltic.TrapValve.GradientA)
			build_pressure(zr.B, press, yield_func)			-- pbne-808: test with full pressure because the trap is out
			flowB, _ = monitor_channel2(zr.B, zr.B, method, press, yield_func, N.PumpB, N.PumpBFrontToValve, N.PumpBRearToValve, N.ValveB, N.ValveBToFS, N.FlowB, N.FSBToMixTee, N.FSAToMixTee, N.FlowA, N.ValveAToFS, N.ValveA, N.MixTeeToTrapValve, N.ValveT, N.TransferLine)
			status:RemoveStatus("w/o Trap")
			return flowB
		end
		local function checkWithOutTransferLine(press)
			status:SetStatus("w/o transfer line")
			set_valves(nil, nil, nil, baltic.TrapValve.GradientA+30)	-- trap valve to block position
			Signalize_MixT_woTransferLine_woTrap()
			flowB, _ = monitor_channel2(zr.B, zr.B, method, press, yield_func, N.PumpB, N.PumpBFrontToValve, N.PumpBRearToValve, N.ValveB, N.ValveBToFS, N.FlowB, N.FSBToMixTee, N.FSAToMixTee, N.FlowA, N.ValveAToFS, N.ValveA, N.MixTeeToTrapValve, N.ValveT)
			status:RemoveStatus("w/o transfer line")
			return flowB
		end
		local function checkMixTtoValveA(press)
			status:SetStatus("MixTee to valve A")
--			set_valves(nil, nil, nil, baltic.TrapValve.Trap)		-- is already set
			Signalize_MixT_ValveA()
			flowA, _ = monitor_channel2(zr.A, zr.B, method, press, yield_func, N.PumpB, N.PumpBFrontToValve, N.PumpBRearToValve, N.ValveB, N.ValveBToFS, N.FlowB, N.FSBToMixTee, N.FSAToMixTee, N.FlowA, N.ValveAToFS, N.ValveA, N.MixTeeToTrapValve, N.ValveT)
			status:RemoveStatus("MixTee to valve A")
			return flowA
		end
		local function checkMixTtoValveB(press)
			status:SetStatus("MixTee to valve B")
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee+30, true)		-- turn clockwise!!!
			set_valves(baltic.PumpValve.MixTee, nil, nil, baltic.TrapValve.Trap)
			Signalize_MixT_ValveB()
			build_pressure(zr.A, press, yield_func)		-- pbne-808: test with full pressure because the trap is out
			flowB, _ = monitor_channel2(zr.B, zr.A, method, press, yield_func, N.PumpA, N.PumpAFrontToValve, N.PumpARearToValve, N.ValveA, N.ValveAToFS, N.FlowA, N.FSAToMixTee, N.FSBToMixTee, N.FlowB, N.ValveBToFS, N.ValveB, N.MixTeeToTrapValve, N.ValveT)
			status:RemoveStatus("MixTee to valve B")
			return flowB
		end

		journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "--- Leak test", os.date("%c")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test mixTee system", true))
		status:SetStatus("MixTee system")

		if selfTest == false then
			-- wait for plug before building pressure
			context:WaitForSignal(leakTest)
		end

		zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_10_1)

		for n=1, #method.TestPressure do
			local press = method.TestPressure[n]
			-- PNS-633: ignore trap maximum pressure
--			local reducedPressure = pf.setMaxPressureLimitB(context, pump, press, true, false, nil)	-- trap is in path mixTee; press limit is max column press + 20 bar
			if selfTest == false then
				context:Log("--- Monitor channel B")
				if (pump:GetSetManualValvePosition(zr.B) == (baltic.PumpValve.MixTee + 30)) then
					zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee, false)		-- turn counter clockwise; the zirconium function doesn't know the "block" position and will switch over "Waste"
					set_valves(baltic.PumpValve.Inject, nil, nil, baltic.TrapValve.GradientT)
				else
					set_valves(baltic.PumpValve.Inject, baltic.PumpValve.MixTee, nil, baltic.TrapValve.GradientT)
				end
				Signalize_MixT_TransferLine_wTrap()
			else
				context:Log("--- Monitor channel B")
				set_valves(baltic.PumpValve.Inject, baltic.PumpValve.MixTee, nil, baltic.TrapValve.GradientT+30)
				Signalize_MixT_woTransferLine_woTrap()
				pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false, press)
				pf.setMaxPressureLimitB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false, press)
			end

			build_pressure(zr.B, press, yield_func)
			if (pump:GetCurrentPressure(zr.B) < (press*0.9)) then
				context:Log("Pressure not reached, waiting 30 seconds before monitoring for leaks")
				parallel.sleep(yield_func, 30000)
			end

			flowB, _ = monitor_channel2(zr.B, zr.B, method, press, yield_func, N.PumpB, N.PumpBFrontToValve, N.PumpBRearToValve, N.ValveB, N.ValveBToFS, N.FlowB, N.FSBToMixTee, N.FSAToMixTee, N.FlowA, N.ValveAToFS, N.ValveA, N.MixTeeToTrapValve, N.ValveT, N.Trap, N.TransferLine)
			local decimalPlaces = pr.getMetaDataPrecision(flowB)
			local testName = "LT: MixTee tubing A @"..press.."bar"
			journal:Delete(testName)
			journal:Add(JournalEntry.Set(LogTo.Both, context.Name, testName, flowB,"\181L/min", decimalPlaces))
			csv.logValueInCSVFile(context, csvFileName, testName, "Flow B", flowB, "\181L/min", -1)

			if (selfTest == false) then
				if (flowB > limit.ltfB) and (noLimit == false) and (press < 1200) then
					testOK = false
					flowB = checkWithOutTrap(press)
					decimalPlaces = pr.getMetaDataPrecision(flowB)
					testName = "LT: MixTee tubing A w/o trap @"..press.."bar"
					journal:Delete(testName)
					-- PNS-513:
					journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, testName, flowB,"\181L/min", decimalPlaces))
					csv.logValueInCSVFile(context, csvFileName, testName, "Flow B", flowB, "\181L/min", -1)
					if (flowB > limit.ltfB) then
						flowB = checkWithOutTransferLine(press)
						decimalPlaces = pr.getMetaDataPrecision(flowB)
						testName = "LT: MixTee tubing A w/o transferline @"..press.."bar"
						journal:Delete(testName)
						-- PNS-513:
						journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, testName, flowB,"\181L/min", decimalPlaces))
						csv.logValueInCSVFile(context, csvFileName, testName, "Flow B", flowB, "\181L/min", -1)
						if (flowB > limit.ltfB) then
							flowA = checkMixTtoValveA(press)
							decimalPlaces = pr.getMetaDataPrecision(flowA)
							testName = "LT: MixTee to valve A tubing @"..press.."bar"
							journal:Delete(testName)
							-- PNS-513:
							journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, testName, flowA,"\181L/min", decimalPlaces))
							csv.logValueInCSVFile(context, csvFileName, testName, "Flow B", flowA, "\181L/min", -1)
							if (flowA < limit.ltfA) then
								report_leak(nil, N.ValveA, N.ValveAToFS, N.FlowA)
							else
								report_leak(nil, N.FlowB, N.FSBToMixTee, N.MixTeeToTrapValve, N.FlowA, N.FSAToMixTee)
							end
						else
							report_leak(nil, N.TransferLine)
						end
					else
						report_leak(nil, N.Trap)
					end
					break
				else
					build_pressure(zr.A, press, yield_func)		-- build pressure pump A to pressure pump B level
					if (pump:GetCurrentPressure(zr.A) < (press*0.9)) then
						context:Log("Pressure not reached, waiting 30 seconds before monitoring for leaks")
						parallel.sleep(yield_func, 30000)
					end
					local flowB_2 = checkMixTtoValveB(press)
					decimalPlaces = pr.getMetaDataPrecision(flowB_2)
					testName = "LT: MixTee tubing B @"..press.."bar"
					journal:Delete(testName)
					journal:Add(JournalEntry.Set(LogTo.Both, context.Name, testName, flowB_2,"\181L/min", decimalPlaces))
					csv.logValueInCSVFile(context, csvFileName, testName, "Flow B", flowB_2, "\181L/min", -1)
					if (flowB_2 < limit.ltfB3) and (noLimit == false) then		-- it is the same limit as for the pumps
						report_leak(nil, N.ValveBToPlug, N.ValveB, N.ValveBToFS, N.FlowB)
					end
				end
			else
				testName = "Self LT: MixTee system @"..press.."bar"
				journal:Delete(testName)
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, testName, flowB,"\181L/min", decimalPlaces))
				csv.logValueInCSVFile(context, csvFileName, testName, "LT: MixTee tubings A", flowB, "µL/min", -1)
			end
		end
		status:RemoveStatus("MixTee system")
		return flowB
	end

	-- Leak test of injection system MUST be done with max pressure of the trap
	-- because the trap is always under pressure.
	---Leak test of the injection path
	---@param base_Leakage number|nil
	---@param method table
	---@param reducePressureAtTheEnd boolean
	---@param yield_func function
	local function leakTest_Injection_System(base_Leakage, method, reducePressureAtTheEnd, yield_func)
		local function Signalize_MixT_ValveT_SelfTest(color)
			pr.Signalize_Reset(context)
			context:Signalize(color, N.PumpBFrontToValve, N.ValveBGroove, N.ValveBToFS, N.FSBToMixTee, N.FlowB, N.FSAToMixTee, N.FlowA, N.ValveAToFS, N.MixTeeToTrapValve)
			context:SignalizeText(baltic.ColorsRGB.White, N.FlowA, N.FlowB)
			context:Signalize(baltic.ColorsRGB.LightGray, N.Separator, N.Separator)
			context:Log("Signal mixTee to valveT")
		end
		local function Signalize_MixT_ValveT(color)
			pr.Signalize_Reset(context)
			context:Signalize(color, N.FlowB, N.FSAToMixTee, N.FlowA, N.ValveAToFS)
			context:SignalizeText(baltic.ColorsRGB.White, N.FlowA, N.FlowB)
			context:Signalize(baltic.ColorsRGB.LightGray, N.Separator, N.Separator)
			context:Log("Signal mixTee to valveT")
		end
		local function Signalize_InjectionSystem_Red()
			context:Signalize(baltic.ColorsRGB.Red, N.ValveAGroove, N.ValveAToInjectionValve, N.ValveIGroovesLoad, N.Loop, N.InjectToTrap)
			context:Signalize(baltic.ColorsRGB.LightGray, N.PumpAFrontToValve, N.PumpARearToValve)
			if (selfTest == true) then
				context:Signalize(baltic.ColorsRGB.Red, N.FSBToMixTee, N.ValveBToFS, N.ValveBGroove, N.PumpBFrontToValve )
			end
		end
		local function Signalize_InjectionSystem_LightGray()
			context:Signalize(baltic.ColorsRGB.LightGray, N.PumpAFrontToValve, N.PumpARearToValve, N.ValveAGroove, N.ValveAToInjectionValve, N.ValveIGroovesLoad, N.Loop, N.InjectToTrap)
		end

		---Determinig the base leakage of the mixTee system @ the test pressure
		---@return number
		local function getBaseLeakageMixTee()
			status:SetStatus("Getting base leakage")
			-- PNS-633: ignore trap maximum pressure
			local isTrapIn = false
			if selfTest == true then
				isTrapIn = false
				local valveTPos, isBlocked = pp.isTrapValveBlocked(execAux)
				if isBlocked == false then
					-- with trap valve blocked
					set_valves(baltic.PumpValve.Inject, baltic.PumpValve.MixTee, baltic.InjectionValve.Load, valveTPos+30)
				else
					set_valves(baltic.PumpValve.Inject, nil, nil, nil)
				end
				Signalize_MixT_ValveT_SelfTest(baltic.ColorsRGB.Red)
			else
				set_valves(baltic.PumpValve.Inject, baltic.PumpValve.MixTee, baltic.InjectionValve.Load, baltic.TrapValve.GradientT)
				Signalize_MixT_ValveT(baltic.ColorsRGB.Red)
			end

			Signalize_InjectionSystem_LightGray()
			-- recompress A in case it was stopped in B subsystem check
			local testPressure = math.min(method.TestPressure, pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, isTrapIn, false))	-- no columns in path; set max pump pressure
			build_pressureAB(pf.noExp(testPressure), pf.noExp(testPressure), sleep_100)
			local flow, _ = monitor_channel2(zr.B, zr.B, method, testPressure, yield_func, N.ValveA, N.ValveAToInjectionValve, N.ValveI, N.InjectToTrap, N.ValveT)
			status:RemoveStatus("Getting base leakage")
			return flow
		end
		local function flushMixTeeAndInjectionSystem()
			-- flush mixTee system to remove solvent B in the A channel
			set_valves(baltic.PumpValve.MixTee, baltic.PumpValve.Compress, baltic.InjectionValve.Inject, baltic.TrapValve.Waste)
			pf.Manualmode_Pump_constantFlow(zr.A, 0.5, pump, sleep_100)
			local pumpStartPosition = pump:GetPistonPosition(zr.A)
			local timeOut = 60
			while (((pumpStartPosition + 0.2) > pump:GetPistonPosition(zr.A)) or timeOut <= 0) do
				context:Sleep(1000)
				timeOut = timeOut - 1
			end
			pf.Manualmode_Pump_constantPressure(zr.A, 0, pump, sleep_100)
			timeOut = 60
			while true do 
				context:Sleep(1000)
				if ((pump:GetCurrentPressure(zr.A) <= 4) or (timeOut <= 0)) then break end
				timeOut = timeOut - 1
			end
			-- flush injection system to remove solvent B
			set_valves(baltic.PumpValve.Inject, nil, nil, baltic.TrapValve.InjectWaste)
			pf.Manualmode_Pump_constantPressure(zr.A, 100, pump, sleep_100)
			pumpStartPosition = pump:GetPistonPosition(zr.A)
			pp.CleanInjector(execLeft, execAux, pp.Aqueous, 40, true, true)
			execLeft:Wait(pp.Quantity(1000, "ms"))
			pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
			timeOut = 60
			while (((pumpStartPosition + 20) > pump:GetPistonPosition(zr.A)) or timeOut <= 0) do
				context:Sleep(1000)
				timeOut = timeOut - 1
			end
			pf.Manualmode_Pump_constantPressure(zr.A, 0, pump, sleep_100)
			timeOut = 60
			while true do 
				context:Sleep(1000)
				if ((pump:GetCurrentPressure(zr.A) <= 4) or (timeOut <= 0)) then break end
				timeOut = timeOut - 1
			end
		end
		---Testing the injection system for a leakage at the test pressure
		---@param press number [bar]
		---@return number
		local function checkInjectionSystem(press)
			set_valves(baltic.PumpValve.Compress, nil, nil, nil)
			Signalize_MixT_ValveT(baltic.ColorsRGB.Red)
			Signalize_InjectionSystem_Red()
			-- recompress A in case it was stopped in B subsystem check
			context:Sleep(2000)
			local flow, _ = monitor_channel2(zr.B, zr.B, method, press, yield_func, N.ValveA, N.ValveAToInjectionValve, N.ValveI, N.InjectToTrap, N.ValveT)
			return flow
		end
		---Testing the injection system without the loop for a leakage at the test pressure
		---@param press number [bar]
		---@return number
		local function checkWoLoop(press)
			status:SetStatus("w/o Loop")
			set_valves(nil, nil, baltic.InjectionValve.Inject, nil)
			context:Signalize(baltic.ColorsRGB.LightGray, N.ValveIGroovesLoad)
			context:Signalize(baltic.ColorsRGB.DarkGray, N.Loop)
			context:Signalize(baltic.ColorsRGB.Red, N.ValveIGrooveLoadwoLoop)
			context:Sleep(2000)
			local flow, _ = monitor_channel2(zr.B, zr.B, method, press, yield_func, N.ValveA, N.ValveAToInjectionValve, N.ValveI, N.InjectToTrap, N.ValveT)
			status:RemoveStatus("w/o Loop")
			return flow

		end
		---Testing the injection system without the injection line for a leakage at the test pressure
		---@param press number [bar]
		---@return number
		local function checkwoInjectionLine(press)
			status:SetStatus("w/o Injection line")
			set_valves(nil, nil, baltic.InjectionValve.Block, nil)
			context:Signalize(baltic.ColorsRGB.LightGray, N.InjectToTrap, N.ValveIGrooveInject, N.ValveIGroovesLoad)
			context:Sleep(2000)
			local flow, _ =monitor_channel2(zr.B, zr.B, method, press, yield_func, N.ValveA, N.ValveAToInjectionValve, N.ValveI, N.InjectToTrap, N.ValveT)
			status:RemoveStatus("w/o Injection line")
			return flow
		end

		---Reducing the pressure of channel A and B
		---@param pA number
		---@param pB number
		---@param leakThroughFSA boolean
		local function reducePressureAB(pA, pB, leakThroughFSA)
			if (leakThroughFSA == true) and (selfTest == false) then		-- a leak was detected
				context:Report("High leak in injection path", Severity.Info, true, "Running solvent exchange is recommended because solvent B has maybe entered into channel A.")
			end
			zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)
			set_valves(baltic.PumpValve.Inject, nil, nil, nil)
			local bPTime = nowTime() + 120
			context:Log("build_pressure AB")

			pf.Manualmode_Pump_constantPressure(zr.A, 0, pump, sleep_100)
			pf.Manualmode_Pump_constantPressure(zr.B, 0, pump, sleep_100)
			pr.Signalize_Reset(context)
			while true do 
				context:Sleep(100)
				if ((pump:GetCurrentPressure(zr.A) <= pA-1) and (pump:GetCurrentPressure(zr.B) <= pB-1)) or (nowTime() > bPTime) then 
					break 
				end
			end
			zr.ChangePressurePID(context, pump, baltic.PPID.PID_25_12_1)
			if (leakThroughFSA == true) then		-- wait for confirmation only in case of a leak
				status:SetStatus("cleaning to remove possible solvent B")
				flushMixTeeAndInjectionSystem()
				if (selfTest == false) then
					context:WaitForSignal("High leak in injection path")
				end
				status:RemoveStatus("cleaning to remove possible solvent B")
			end
		end

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test injection system", true))

		local leakThroughFSA = false
		local flowI, baseLeakageB
		local press = method.TestPressure
		noLimit = press ~= 1000			-- compare with the limit only for 1000 bar test

		-- wait for plug before building pressure
		if (selfTest == false) then
			context:WaitForSignal(leakTest)
			context:Signalize(baltic.ColorsRGB.LightGray, N.Separator, N.Separator)
		end
		-- test injection system
		if (self_Test.showMessage == true) then status:SetStatus("Injection system") end
		if (base_Leakage == nil) or (selfTest == true) then
			baseLeakageB, _ = getBaseLeakageMixTee()
			context:Log("baseLeakageB: {0}", baseLeakageB)
		else
			baseLeakageB = base_Leakage
			pump:SetManualValvePosition(zr.B, baltic.PumpValve.MixTee, false)		-- set valve B counterclockwise, because it is in block position from a previous procedure
			if (selfTest == false) then
				set_valves(baltic.PumpValve.Inject, nil, baltic.InjectionValve.Load, baltic.TrapValve.GradientT)
				-- PNS-633: ignore trap maximum pressure
				press = pf.setMaxPressureLimitsAB(context, pump, method.TestPressure, false, false)	-- no columns in path; set max pump pressure
			else
				set_valves(baltic.PumpValve.Inject, nil, baltic.InjectionValve.Load, baltic.TrapValve.GradientT+30)
			end
			method.TestPressure = press-2
			build_pressureAB(pf.noExp(press), pf.noExp(press), sleep_100)
		end

		-- report the base leakage of mixTee system
		results[17] = baseLeakageB

		if (baseLeakageB > limit.ltfB) and (selfTest == false) and (noLimit == false) then
			-- there seems to be a leakage in the mixTee system
			context:Report("Leak test", Severity.Info, true, "There is a leakage in the MixTee system detected. Please run the leak test of the mixTee system.")
			testOK = false
		else
			-- there seems to be a leakage between flowA and valve A
			flowI, _ = checkInjectionSystem(press)
			local i = 18
			results[i] = pf.noExp((flowI-baseLeakageB), 5)
			if (selfTest == false) and (noLimit == false) then
				if ((flowI-baseLeakageB) > -limit.ltfA) then -- bypass loop
					leakThroughFSA = true
					flowI = checkWoLoop(press)
					i=i+1
					results[i] = pf.noExp((flowI-baseLeakageB), 5)
					if ((flowI-baseLeakageB) > -limit.ltfA) then -- bypass loop still leaking, so loop likely not the problem
						flowI = checkwoInjectionLine(press)
						i=i+1
						results[i] = pf.noExp((flowI-baseLeakageB), 5)
						if ((baseLeakageB+flowI) > -limit.ltfA) then -- not inject line
							report_leak(nil, N.ValveAToInjectionValve, N.ValveA)
						else
							report_leak(nil, N.InjectToTrap)
						end
					else
						report_leak(nil, N.Loop)
					end
				end
			else			-- selfTest = true
				local valveTPos, isBlocked = pp.isTrapValveBlocked(execAux)
				if isBlocked == false then
					set_valves(nil, nil, nil, valveTPos-30)
				end
				local test = "Self LT: Injection system @ "..press.." bar"
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, test, results[17], "\181L/min"))
				csv.logValueInCSVFile(context, csvFileName, test, "LT: Injection tubing @1000bar", results[17], "µL/min", -1)
			end
		end
		if reducePressureAtTheEnd == true then
			reducePressureAB(45, 45, leakThroughFSA)
		end
		if (self_Test.showMessage == true) then status:RemoveStatus("Injection system") end
	end

	local function lowPressureLeakTestPumpAB()
		context:ShowComposition(false)
		lowPressureTest(sleep_200)
		context:ShowComposition(true)
	end

	---Pump pressure test @ 1000 bar or at different pressures
	local function pumpPressureTest()
--		Ramp pressure test pump A and B at 50, 100, 250, 500, 800, 1000, 500, 50 bar

		local size = #pressure

		local measureTime = 60			-- [minutes]
		if size > 1 then
			measureTime = 1
		end

		local function sleep_Seconds(seconds)
			context:Sleep(seconds*1000)
		end
		local function evaluateLeakage(channel, delay, yield_function)
			local startPistonPosition = pump:GetPistonPosition(channel)
			parallel.sleep(yield_function, delay * 60 * 1000)		-- [ms]
			local leak = pump:GetPistonPosition(channel) - startPistonPosition
			parallel.sleep(yield_function, 100)		-- [100 ms]
			return leak
		end

		---Log the results to TwinScape
		---@param leakageA number
		---@param leakageB number
		---@param press number
		local function logResults(leakageA, leakageB, press)
			local ltA = pf.noExp((leakageA / measureTime), 5)		-- [µL/min]
			local ltB = pf.noExp((leakageB / measureTime), 5)		-- [µL/min]
			local msgA = "Self LT: Pump A @"..press.."bar"
			local msgB = "Self LT: Pump B @"..press.."bar"
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, msgA, ltA, "\181L/min"))
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, msgB, ltB , "\181L/min"))
			local testProcedure = "Self HPLC seals conditioning"
			msgA = "LT: Pump A @"..press.."bar"
			msgB = "LT: Pump B @"..press.."bar"
			csv.logValueInCSVFile(context, csvFileName, testProcedure, msgA, ltA, "µL/min", -1)
			csv.logValueInCSVFile(context, csvFileName, testProcedure, msgB, ltB , "µL/min", -1)
		end

		context:ShowComposition(false)
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_10_1)

		if size == 1 then
			local testPressure = pressure[1]

			local function positioningPiston(channel, yield_function)
				local pumpSpeed = 1000
				local desiredPosition = baltic.MaxPumpVolume - 500
				local actualPosition = pump:GetPistonPosition(channel)
				if actualPosition > desiredPosition then
					pumpSpeed = -pumpSpeed
					zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent, nil)
				else
					zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Waste, nil)
				end

				if math.abs(actualPosition - desiredPosition) > 10 then
					pf.Manualmode_Pump_constantSpeed(channel, pumpSpeed, pump,yield_function)

					while math.abs(actualPosition - desiredPosition) > 50 do
						yield_function()
						actualPosition = pump:GetPistonPosition(channel)
					end
				end
				pf.Manualmode_Pump_constantSpeed(channel, 0, pump,yield_function)
			end
			local function pressureTestPump(channel, yield_function)
				positioningPiston(channel, yield_function)
				zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Compress, nil)
				build_pressure(channel, testPressure, yield_function)
				parallel.sleep(yield_function, 30000)		-- 30 seconds delay
				local leak = evaluateLeakage(channel, measureTime, yield_function)
				return leak
			end
			if (self_Test.showMessage == true) then status:SetStatus("High pressure leak test pump A and B") end
			pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
			context:Sleep(10*1000)
			local p_pressureTest_PumpA = { pressureTestPump, zr.A, parallel.yield }
			local p_pressureTest_PumpB = { pressureTestPump, zr.B, parallel.yield }
			local p_logPressSensorOffsetA = { pf.flowSensorOffset, context, zr, pump, zr.A, true, 60, parallel.yield }
			local p_logPressSensorOffsetB = { pf.flowSensorOffset, context, zr, pump, zr.B, true, 60, parallel.yield }
			local leakA, leakB, offsetA, offsetB = parallel.run(sleep_200, p_pressureTest_PumpA, p_pressureTest_PumpB, p_logPressSensorOffsetA, p_logPressSensorOffsetB)
			logResults(leakA, leakB, testPressure)
			pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)

			if offsetA ~= nil then
--	Don't save the new offset. Only report it.				
--				pf.SetFlowCalibrationOffset(zr.A, offsetA, pump, sleep_200)
--				settings.FlowCalibrationOffsetA = offsetA
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Flow Sensor A Offset Leak Test Pump", offsetA, "\181L/min", "N5"))
				csv.logValueInCSVFile(context, csvFileName, "Flow Sensor A", "Offset Leak Test Pump", offsetA, "µL/min", 5)
			end
			if offsetB ~= nil then
--	Don't save the new offset. Only report it.				
--				pf.SetFlowCalibrationOffset(zr.B, offsetB, pump, sleep_200)
--				settings.FlowCalibrationOffsetB = offsetB
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Flow Sensor B Offset Leak Test Pump", offsetB, "\181L/min", "N5"))
				csv.logValueInCSVFile(context, csvFileName, "Flow Sensor B", "Offset Leak Test Pump", offsetB, "µL/min", 5)
			end
			if (self_Test.showMessage == true) then status:RemoveStatus("High pressure leak test pump A and B") end
		else
--			local pressure = {50, 100, 250, 500, 800, 1000, 500, 50}
			local actualPressureA = 0
			local actualPressureB = 0
			local a_LeakA, a_LeakB = {}, {}

			set_valves(baltic.PumpValve.Compress, baltic.PumpValve.Compress, nil, nil)
			pr.Signalize_Reset(context)
			context:Sleep(500)
			context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.PumpARearToValve)
			context:Signalize(baltic.ColorsRGB.Red, N.PumpBFrontToValve, N.PumpBRearToValve)

			local i = 1
			while (i <= size) do
				local msg = "Pressure ramp test @ " .. pressure[i] .. " bar"
				status:SetStatus(msg)
				actualPressureA = pump:GetCurrentPressure(zr.A)
				actualPressureB = pump:GetCurrentPressure(zr.B)
				build_pressureAB(pressure[i], pressure[i], sleep_200)
				if (actualPressureA > pressure[i]) or (actualPressureB > pressure[i]) then
					sleep_Seconds(300)
				else
					sleep_Seconds(120)
				end
				local p_evaluate_PumpA = { evaluateLeakage, zr.A, measureTime, parallel.yield }
				local p_evaluate_PumpB = { evaluateLeakage, zr.B, measureTime, parallel.yield }
				a_LeakA[i], a_LeakB[i] = parallel.run(sleep_200, p_evaluate_PumpA, p_evaluate_PumpB)
				logResults(a_LeakA[i], a_LeakB[i], pressure[i])
				i = i + 1
				status:RemoveStatus(msg)
			end
		end
		context:ShowComposition(true)
	end

	---Run a preparation with solvent exchange
	---@return number
	---@return boolean
	---@return number
	---@return boolean
	local function prepareSystem()
		---Cleaning the syringe
		---@param yield_function function
		local function prepare_autosampler(yield_function)
			local depth = pp.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
			local primeCycles = 1
			local dispenseVolume = 300
			local pumpSpeed = 30
			pp.EmptySyringe_And_LeaveObject(context, execLeft, execAux, depth, installed.SyringeZeroPosition)

			local wash_module = pp.QueryModule(execLeft, pp.Capabilities.ILcMsWashStation)

			---Bürkert Valve is opened in VolumeToBePumped and closed in PrimeInjectorWithVolumePump

			---move the LCMS tool to waste position 2 and dispense 300uL
			execLeft:MoveToObject(wash_module, pp.Organic)
			execLeft:PenetrateObject(wash_module, pp.Organic, depth, pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
			pp.VolumeToBePumped(execLeft, pp.Organic, dispenseVolume, pumpSpeed)
			execLeft:LeaveObject()

			---move the LCMS tool to waste position 1 and dispense 300uL
			execLeft:MoveToObject(wash_module, pp.Aqueous)
			execLeft:PenetrateObject(wash_module, pp.Aqueous, depth, pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
			pp.VolumeToBePumped(execLeft, pp.Aqueous, dispenseVolume, pumpSpeed)
			execLeft:LeaveObject()

			pp.PrimeSyringeWithVolumePump(context, execLeft, primeCycles, yield_function, true)		-- 3x (2x organic + 4x aqueous)
			pp.PrimeInjectorWithVolumePump(context, execLeft, yield_function, false)
		end

		---Log the results to the journal
		---@param valA number
		---@param repetitionA integer
		---@param passedA boolean
		---@param valB number
		---@param repetitionB integer
		---@param passedB boolean
		local function reportPreparationValues(valA, repetitionA, passedA, valB, repetitionB, passedB)
			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "--- Preparation", os.date("%c")))
			if passedA == true then
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Preparation A", valA, "\181L", "N2"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Repetitions A", repetitionA, "", "N0"))
				csv.logValueInCSVFile(context, csvFileName, "Preparation", "Preparation A", valA, "", 2)
				csv.logValueInCSVFile(context, csvFileName, "Preparation", "Preparation repetitions A", repetitionA, "", 0)
			end
			if passedB == true then
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Preparation B", valB, "\181L", "N2"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Repetitions B", repetitionB, "", "N0"))
				csv.logValueInCSVFile(context, csvFileName, "Preparation", "Preparation B", valB, "", 2)
				csv.logValueInCSVFile(context, csvFileName, "Preparation", "Preparation repetitions B", repetitionB, "", 0)

			end
		end

		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
		context:Sleep(10*1000)

		status:SetStatus("Preparing pump A, B and autosampler")
		local p_degas_A = { degas_Channel, context, zr, zr.A, 1, true, -1, 10, parallel.yield }
		local p_degas_B = { degas_Channel, context, zr, zr.B, 1, true, -1, 10, parallel.yield }
		local p_prepAutosampler = { prepare_autosampler, parallel.yield }
		local a, repA, passedA, b, repB, passedB = parallel.run(sleep_100, p_degas_A, p_degas_B, p_prepAutosampler)

		reportPreparationValues(a, repA, passedA, b, repB, passedB)
		status:RemoveStatus("Preparing pump A + B")
		return a, passedA, b, passedB
	end

	local function flushSystem()
		local function reducePumpAPressure(targetPressure)
			pf.Manualmode_Pump_constantPressure(zr.A, targetPressure, pump, sleep_100)
			local runTime = 30 + pf.now()		-- seconds
			while pump:GetCurrentPressure(zr.A) > (targetPressure+5) do
				if runTime < pf.now() then break end
				sleep_200()
			end
		end
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 10, 10, 30, sleep_100, baltic.smooth)

		status:SetStatus("Flushing the system")
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_10_1)

		-- run 30s 100%A via loading capillary 
		set_valves(baltic.PumpValve.Inject, baltic.PumpValve.Waste, baltic.InjectionValve.Load, baltic.TrapValve.InjectWaste)
		pf.Manualmode_Pump_constantPressure(zr.A, 100, pump, sleep_100)
		local runTime = 30 + pf.now()		-- seconds
		while runTime > pf.now() do
			sleep_200()
		end
		-- reduce pressure
		reducePumpAPressure(50)

		set_valves(baltic.PumpValve.MixTee, nil, nil, baltic.TrapValve.GradientT)

		-- flush the mixTee to separation column
		local sep = context:GetArgumentValue("separator")
		local trap = context:GetArgumentValue("trap")
		local flowTarget = chrom.sepMaxFlow
		local maxPressure = 800
		local initialTestPressure = maxPressure
		local isFlowDriven = true
		-- if the flow is restricted set the maximum flow
		local unityFlow = chrom.GetUnityFlowSystem(installed)
		unityFlow = 1/(1/unityFlow + 1/chrom.column_flow(sep, pressSettings.GradientPumpMaxTargetPressure, 1, chrom.viscosity_H2O_20C))
		-- if the pressure wwould be too high to rech the maximum flow set the maximum pressure
		if (unityFlow * maxPressure) < flowTarget then isFlowDriven = false end

		if isFlowDriven == true then
			pf.Manualmode_Pump_constantFlow(zr.A, flowTarget, pump, sleep_100)
		else
			-- run 30s 100%A (blocked B valve) at 1000 bar (or max column P) via mix tee with trap
			maxPressure = math.min(maxPressure, pf.getMaxColumnPressure(trap, sep, pressSettings.GradientPumpMaxTargetPressure)*0.8)
			pf.Manualmode_Pump_constantPressure(zr.A, maxPressure, pump, sleep_100)
		end

		runTime = 30 + pf.now()		-- seconds
		while runTime > pf.now() do
			sleep_200()
		end

		-- block the separation column
		set_valves(nil, nil, nil, baltic.TrapValve.GradientT-30)
		-- reduce pressure
		reducePumpAPressure(50)

		-- run 30s 100%A (blocked B valve) at 1000 bar (or max column P) via mix tee without trap
		set_valves(nil, nil, nil, baltic.TrapValve.GradientA)
		if initialTestPressure ~= maxPressure then
			maxPressure = math.min(initialTestPressure, pf.getMaxColumnPressure(nil, sep, pressSettings.GradientPumpMaxTargetPressure)*0.8)
		end
		if isFlowDriven == true then
			pf.Manualmode_Pump_constantFlow(zr.A, flowTarget, pump, sleep_100)
		else
			pf.Manualmode_Pump_constantPressure(zr.A, maxPressure, pump, sleep_100)
		end
		runTime = 30 + pf.now()		-- seconds
		while runTime > pf.now() do
			sleep_200()
		end

		-- reduce pressure with column not inline and leave the trap valve in block position until idle flow
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
		set_valves(nil, nil, nil, baltic.TrapValve.GradientA+30)
	end

	-- main function starts here

	-- if there is still pressure from e.g. Idle flow, release it
	pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 20, sleep_100, baltic.smooth)

	if (pump:GetPistonPosition(zr.A) > 50) or (pump:GetPistonPosition(zr.B) > 50) then
		local a, b, _, _ = degas(context, 1, -1, -1, pf.now(), sleep_100)
		-- bail if a pump failed degassing..
		if (not (a and b)) then
			pr.decompressSystem(context)
			context:Abort()
		end
	end
	settings = pump:GetSettings()

	if selfTest == false then
		csv.logValueInCSVFile(context, csvFileName, "Diagnostics started", "", "", "", -1)
	end

	if selfTest == true then
		if isService == false then
			csv.logValueInCSVFile(context, csvFileName, "Self-test started", "", "", "", -1)
		end
		journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Self test", os.date("%c")))

		-- self Leak Test High Pressure Injection System Ramp Test
		if self_Test.self_LT_HP_IS_RT == true then
			local test = "Self LT ramp: Injection system"
			if (self_Test.showMessage == true) then status:SetStatus(test) end
			pr.Signalize_Reset(context)
			local testMethod = {MaxVolume=60, MaxPressureDeviation=1.0, MeasuringTime=60, TestPressure=1000}

			for n=1, #pressure do
				local reducePressureAtTheEnd = pressure[n+1] == nil
				testMethod.TestPressure = pressure[n]
				leakTest_Injection_System(nil, testMethod, reducePressureAtTheEnd, sleep_200)
				reportResults(test, pressure[n])
			end

			if (self_Test.showMessage == true) then status:RemoveStatus(test) end
		end

		if self_Test.self_LT_HP == true then
			local test = "Self high pressure leak test"
			if (self_Test.showMessage == true) then status:SetStatus(test) end
			local testMethod = {MaxVolume=60, MaxPressureDeviation=1.0, MeasuringTime=60, TestPressure=pressure}
			---@type number?
			local flowB = leakTest_MixTeeSystem(testMethod, sleep_200)
			pr.Signalize_Reset(context)
			for n=1, #pressure do
				local reducePressureAtTheEnd = pressure[n+1] == nil		-- reduce pressure at the end and flush the system if a solvent B could enter the injection system
				if pressure[n] ~= 1000 then flowB = nil end
				testMethod.TestPressure = pressure[n]
				leakTest_Injection_System(flowB, testMethod, reducePressureAtTheEnd, sleep_200)
			end

			if (self_Test.showMessage == true) then status:RemoveStatus(test) end
		end

		if self_Test.self_LT_LP_AB == true then
			lowPressureLeakTestPumpAB()
		end

		if self_Test.self_LT_HP_AB == true then
			-- make copy of the pressure list if the list is needed in 'self_LT_P_RT'
			local pressure_List = pressure
			if isService == false then
				pressure = {1000} 
--				if (installed.MaxPumpPressure > 1200) then pressure = {1000, 1200} end			-- PNS-763
			end
			pumpPressureTest()
			pressure = pressure_List
		end

		if self_Test.self_LT_P_RT == true then
			pumpPressureTest()
		end

		if self_Test.self_FRT_B_MT == true then
			if (self_Test.showMessage == true) then status:SetStatus("Self flow restriction test") end
			limitRoute2, tP2 = calcLimitRoute2(100)
			flowRestrictionTest_PumpBMixTee({TestPressureB=tP2, MeasuringTime=60, MaxPressureDeviation=1.0, MinTestPressure=98, MaxVolume=400}, sleep_200)
			if (self_Test.showMessage == true) then status:RemoveStatus("Self flow restriction test") end
		end

		if self_Test.self_FRT_IS == true then
			if (self_Test.showMessage == true) then status:SetStatus("Self flow restriction test") end
			limitRoute3, tP3 = calcLimitRoute3(100, limit.bpsA, true)
			flowRestrictionTest_Injection_System({TestPressureA=tP3, MeasuringTime=60, MaxPressureDeviation=1.0, MinTestPressure=98, MaxVolume=400}, sleep_200)
			if (self_Test.showMessage == true) then status:RemoveStatus("Self flow restriction test") end
		end

		if self_Test.self_FRT_A_MT == true then
			if (self_Test.showMessage == true) then status:SetStatus("Self flow restriction test") end
			limitRoute1, tP1 = calcLimitRoute1(100, limit.bpfA, false, false, true, true)
			flowRestrictionTest_PumpAMixTee({TestPressureA=tP1, MeasuringTime=60, MaxPressureDeviation=1.0, MinTestPressure=98, MaxVolume=400}, sleep_200)
			if (self_Test.showMessage == true) then status:RemoveStatus("Self flow restriction test") end
		end

		if self_Test.self_Prepare_and_Flush == true then
			prepareSystem()
			flushSystem()
		end

		finished = true
	else
		local isLTMessageDisplayed = false
		if (frtPumpAMixTee == true) then
			context:Report("Flow restriction test pumpA mixTee", Severity.Info, true, "Please disconnect the separation column, then click 'Confirm' to continue.")
		else
			if (ltMixTeeSystem == true or ltInjectionSystem == true) then
				context:Report(leakTest, Severity.Info, true, "Please disconnect the separation column and plug the open end of the transfer line, then click 'Confirm' to continue.")
				isLTMessageDisplayed = true
			end
		end

	--> These procedures are only for production use --
		if context.AppKey.Special == baltic.Production then
			local flushCycles = context:GetArgumentValue("Flush cycles")
			local testPressure = context:GetArgumentValue("Test pressure")
			local T1 = context:GetArgumentValue("Wait time T1")
			local T2 = context:GetArgumentValue("Wait time T2")

			---Delay in a loop
			---@param delay number
			local function delayLoop(delay)
				local t = delay			-- seconds
				local T = DotNetString.Format(" {0:#0}", t)

				for i=1, t do
					T = DotNetString.Format("{0:#0}", t-i)
					status:SetStatus(T)
					context:Sleep(1000)
					status:RemoveStatus(T)
				end
			end

			---empty and refill the pumps
			---@param iterations number
			local function purgePumps(iterations)
				local msg1pP = "Flush pumps"
				local function refillPump(channel, yield_function)
					context:Log("Refill {0}", channel)
					if not zr.IsFull(pump, channel) then
						zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent, nil)
						pf.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpRefillSpeed, pump, yield_function)
						while not zr.IsFull(pump, channel) do yield_function() end
					end
				end
				local function refillPumps()
					local p_refillPump_a = { refillPump, zr.A, parallel.yield }
					local p_refillPump_b = { refillPump, zr.B, parallel.yield }
					parallel.run(sleep_100, p_refillPump_a, p_refillPump_b)
				end
				local function empty_pump(channel, yield_function)
					context:Log("Purge {0}", channel)
					if not zr.IsEmpty(pump, channel) then
						zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Waste, nil)
						pf.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpPurgeSpeed, pump, yield_function)
						while not zr.IsEmpty(pump, channel) do yield_function() end
					end
				end

				pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
				context:Sleep(10*1000)

				status:SetStatus(msg1pP)
				for i=1, iterations do
					local msgpP = DotNetString.Format(" {0} of {1}", i, iterations)
					status:SetStatus(msgpP)
					local p_empty_pump_a = { empty_pump, zr.A, parallel.yield }
					local p_empty_pump_b = { empty_pump, zr.B, parallel.yield }
					parallel.run(sleep_100, p_empty_pump_a, p_empty_pump_b)
					context:Sleep(5*1000)

					refillPumps()
					context:Sleep(5*1000)
					status:RemoveStatus(msgpP)
				end
				status:RemoveStatus(msg1pP)
				pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
			end
			local function flushMixT()
				local testFM = "Flow test"
				local msg1FM = "flush MixTee:"
				local msg2FM = "Open outlet of MixTee. Click 'Confirm' if solvent releases the MixTee."
				local msg3FM = "Flow through MixTee:"
				local msg4FM = "Click 'Confirm' if flow is stable."

				status:SetStatus(testFM)


				context:Report(msg1FM, Severity.Info, true, msg2FM)
				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee, nil)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee, nil)
				pf.Manualmode_Pump_constantPressure(zr.A, 100, pump, sleep_100)
				pf.Manualmode_Pump_constantPressure(zr.B, 100, pump, sleep_100)
				context:WaitForSignal(msg1FM)

				context:Report(msg3FM, Severity.Info, true, msg4FM)
				context:WaitForSignal(msg3FM)
				local fA2 = pump:GetCurrentFlow(zr.A)
				local fB2 = pump:GetCurrentFlow(zr.B)
				local failed = false

				if fA2 < 0.2 then
					local msg5FM = "Flow A"
					local msg6FM = DotNetString.Format("through MixTee ({0:#0.00}) is < 0.2 \181L/min.", fA2)
					context:Sleep(100)
					context:Report(msg5FM, Severity.Error, true, msg6FM)
					failed = true
				end
				if fB2 < 0.2 then
					local msg7FM = "Flow B"
					local msg8FM = DotNetString.Format("through MixTee ({0:#0.00}) is < 0.2 \181L/min.", fB2)
					context:Sleep(100)
					context:Report(msg7FM, Severity.Error, true, msg8FM)
					failed = true
				end

				journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, testFM, os.date("%c")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, testFM, true))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Pump A: flow through MixT", fA2, "\181L/min"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Pump B: flow through MixT", fB2 , "\181L/min"))
				csv.logValueInCSVFile(context, csvFileName, testFM, "Pump A: flow through MixT", fA2, "\181L/min", -1)
				csv.logValueInCSVFile(context, csvFileName, testFM, "Pump B: flow through MixT", fB2, "\181L/min", -1)

				status:RemoveStatus(testFM)

				if failed then
					pr.decompressSystem(context)
					context:Abort()
				end

				context:Report("Flow A and B", Severity.Info, true, "are OK, (> 0.2 \181L/min).")
			end
			local function high_pressure_test()
				local function Signalize_up_to_MixT()
					pr.Signalize_Reset(context)
					context:Sleep(500)
					context:Signalize(baltic.ColorsRGB.Red, N.PumpBFrontToValve, N.ValveBGroove, N.ValveBToFS, N.FlowB, N.FSBToMixTee)
					context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.ValveAGroove, N.ValveAToFS, N.FlowA, N.FSAToMixTee)
					context:Signalize(baltic.ColorsRGB.LightGray, N.MixTeeToTrapValve, N.TrapValveToWaste, N.Trap, N.ValveTGrooves, N.TransferLine, N.Separator)
				end
				local function getOneSecondPistonPositionAverage()
					local val1, val2 = 0, 0
					for i=1, 10 do
						val1 = val1 + pump:GetPistonPosition(zr.A)
						val2 = val2 + pump:GetPistonPosition(zr.B)
						sleep_100()
					end
					return val1/10, val2/10
				end
				local testHP = "High pressure leak test"
				local msgT1HP = "Wait time T1 [s] "
				local msgT2HP = "Wait time T2 [s] "
				local msg3HP = "Block outlet MixTee. Click 'Confirm' to start the procedure."

				status:SetStatus(testHP)

				context:Log("-------------------------------------------------------")
				context:Log("--- {0}", testHP)
				context:Log("-------------------------------------------------------")

				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee, true)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee, true)
				context:Sleep(1000)

				Signalize_up_to_MixT()

				context:Report(testHP, Severity.Info, true, msg3HP)
				context:WaitForSignal(testHP)

				local stopA = false
				local stopB = false

				local posA1, posB1 = getOneSecondPistonPositionAverage()

				context:Sleep(1000)
				pf.Manualmode_Pump_constantPressure(zr.A, testPressure, pump, sleep_100)
				pf.Manualmode_Pump_constantPressure(zr.B, testPressure, pump, sleep_100)
				while not (zr.IsEmpty(pump, zr.A) and zr.IsEmpty(pump, zr.B)) do
					if (pump:GetCurrentPressure(zr.A) >= testPressure-1) then
						stopA = true
					end
					if (pump:GetCurrentPressure(zr.B) >= testPressure-1) then
						stopB = true
					end
					if stopA and stopB then break end
					sleep_100()
				end
				if not (stopA and stopB) then
					local msg4HP = "Test procedure aborted"
					local msg5HP = "Pressure could not be established."
					context:Report(msg4HP, Severity.Info, true, msg5HP)
					pr.decompressSystem(context)
					context:Abort()
				end

				context:Sleep(5000)
				local posA2, posB2 = getOneSecondPistonPositionAverage()

				status:SetStatus(msgT1HP)
				delayLoop(T1)
				local volA1, volB1 = getOneSecondPistonPositionAverage()
				status:RemoveStatus(msgT1HP)

				status:SetStatus(msgT2HP)
				delayLoop(T2)
				local volA2, volB2 = getOneSecondPistonPositionAverage()
				status:RemoveStatus(msgT2HP)

				local leakVolA = (volA2-volA1)/T2
				local leakVolB = (volB2-volB1)/T2

				journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, testHP, os.date("%c")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test pump A", true))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test pump B", true))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "HP A: compression volume", posA2-posA1, "\181L"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "HP A: leak rate", leakVolA, "\181L/min"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "HP B: compression volume", posB2-posB1, "\181L"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "HP B: leak rate", leakVolB, "\181L/min"))
				csv.logValueInCSVFile(context, csvFileName, testHP, "HP A: compression volume", posA2-posA1, "\181L", -1)
				csv.logValueInCSVFile(context, csvFileName, testHP, "HP A: leak rate", leakVolA, "\181L/min", -1)
				csv.logValueInCSVFile(context, csvFileName, testHP, "HP B: compression volume", posB2-posB1, "\181L", -1)
				csv.logValueInCSVFile(context, csvFileName, testHP, "HP B: leak rate", leakVolB, "\181L/min", -1)

				status:RemoveStatus(testHP)
			end
			local function low_pressure_test()
				local testLP = "Low pressure leak test"
				local msgT1LP = "Wait time T1 [s] "
				local msgT2LP = "Wait time T2 [s] "
				local stopA = false
				local stopB = false

				status:SetStatus(testLP)

				context:Log("-------------------------------------------------------")
				context:Log("--- {0}", testLP)
				context:Log("-------------------------------------------------------")

				testPressure = 50

				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste, true)
				context:Sleep(100)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste, true)
				context:Sleep(1000)
				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Compress, true)
				context:Sleep(100)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Compress, true)
				context:Sleep(1000)

				local posA1 = pump:GetPistonPosition(zr.A)
				local posB1 = pump:GetPistonPosition(zr.B)
				context:Sleep(1000)
				pf.Manualmode_Pump_constantPressure(zr.A, testPressure, pump, sleep_100)
				pf.Manualmode_Pump_constantPressure(zr.B, testPressure, pump, sleep_100)
				while not (zr.IsEmpty(pump, zr.A) and zr.IsEmpty(pump, zr.B)) do
					if (pump:GetCurrentPressure(zr.A) >= testPressure-1) then
						stopA = true
					end
					if (pump:GetCurrentPressure(zr.B) >= testPressure-1) then
						stopB = true
					end
					if stopA and stopB then break end
					sleep_100() 
				end
				context:Sleep(5000)
				local posA2 = pump:GetPistonPosition(zr.A)
				local posB2 = pump:GetPistonPosition(zr.B)

				if not (stopA and stopB) then
					local msg3LP = "Test procedure aborted"
					local msg4LP = "Pressure could not be established."
					context:Report(msg3LP, Severity.Info, true, msg4LP)
					pr.decompressSystem(context)
					context:Abort()
				end

				status:SetStatus(msgT1LP)
				delayLoop(T1)
				local volA1 = pump:GetPistonPosition(zr.A)
				local volB1 = pump:GetPistonPosition(zr.B)
				status:RemoveStatus(msgT1LP)

				status:SetStatus(msgT2LP)
				delayLoop(T2)
				local volA2 = pump:GetPistonPosition(zr.A)
				local volB2 = pump:GetPistonPosition(zr.B)
				status:RemoveStatus(msgT2LP)

				local leakVolA = (volA2-volA1)/T2
				local leakVolB = (volB2-volB1)/T2

				journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, testLP, os.date("%c")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, testLP, true))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "LP A: compression volume", posA2-posA1, "\181L"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "LP A: leak rate", leakVolA, "\181L/min"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "LP B: compression volume", posB2-posB1, "\181L"))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "LP B: leak rate", leakVolB, "\181L/min"))
				csv.logValueInCSVFile(context, csvFileName, testLP, "LP A: compression volume", posA2-posA1, "\181L", -1)
				csv.logValueInCSVFile(context, csvFileName, testLP, "LP A: leak rate", leakVolA, "\181L/min", -1)
				csv.logValueInCSVFile(context, csvFileName, testLP, "LP B: compression volume", posB2-posB1, "\181L", -1)
				csv.logValueInCSVFile(context, csvFileName, testLP, "LP B: leak rate", leakVolB, "\181L/min", -1)
				status:RemoveStatus(testLP)
			end

			if context:GetArgumentValue("Flush system") then
				journal:Delete("Flow test")
				journal:Delete("Pump A: flow through MixT [\181L/min]")
				journal:Delete("Pump B: flow through MixT [\181L/min]")

				purgePumps(flushCycles)
				flushMixT()
			end
			if context:GetArgumentValue("High pressure test") then
				journal:Delete("High pressure leak test")
				journal:Delete("HP A: compression volume [\181L]")
				journal:Delete("HP A: leak rate [\181L/min]")
				journal:Delete("HP B: compression volume [\181L]")
				journal:Delete("HP B: leak rate [\181L/min]")
				high_pressure_test()
			end
			if context:GetArgumentValue("Low pressure test") then
				journal:Delete("Low pressure leak test")
				journal:Delete("LP A: compression volume [\181L]")
				journal:Delete("LP A: leck rate [\181L/min]")
				journal:Delete("LP B: compression volume [\181L]")
				journal:Delete("LP B: leak rate [\181L/min]")
				pf.reducePressure(context, pump, zr, zr.A, zr.B, 20, 20, 60, sleep_100, baltic.smooth)
				local sensorSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
				if not pf.checkPressSensorSettings(context, zr, pump, sensorSettings.SensorDefaultOffset, sensorSettings.SensorDefaultFactor) then
					pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
					pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
					context:Abort()
				end
				low_pressure_test()
			end
		end
	--< These procedures are only for production use --

		if (frtPumpAMixTee == true or frtPumpBMixTee == true or frtInjectionSystem == true) then
			status:SetStatus("Flow Restriction Test")
			limitRoute1, tP1 = calcLimitRoute1(100, limit.bpfA, true, true, true, true)
			limitRoute2, tP2 = calcLimitRoute2(100)
			limitRoute3, tP3 = calcLimitRoute3(100, limit.bpsA, true)
			context:Log("-- Flow restriction test limits --")
			context:Log("Route 1: {0} @ {1} bar",limitRoute1, tP1)
			context:Log("Route 2: {0} @ {1} bar",limitRoute2, tP2)
			context:Log("Route 3: {0} @ {1} bar",limitRoute3, tP3)
			context:Log("------------------------------------")

			pr.Signalize_Reset(context)
			context:Sleep(500)
		end
		if (frtPumpBMixTee == true) then
			flowRestrictionTest_PumpBMixTee({TestPressureB=tP2, MeasuringTime=60, MaxPressureDeviation=1.0, MinTestPressure=98, MaxVolume=400}, sleep_200)
		end
		if (frtInjectionSystem == true and testOK == true) then
			flowRestrictionTest_Injection_System({TestPressureA=tP3, MeasuringTime=60, MaxPressureDeviation=1.0, MinTestPressure=98, MaxVolume=400}, sleep_200)
		end
		if (frtPumpAMixTee == true and testOK == true) then
			flowRestrictionTest_PumpAMixTee({TestPressureA=tP1, MeasuringTime=60, MaxPressureDeviation=1.0, MinTestPressure=98, MaxVolume=400}, sleep_200)
			if (ltMixTeeSystem == false and ltInjectionSystem == false) then
				context:Report("Flow restriction test pump A mixTee", Severity.Info, true, "Please reconnect the separation column, then click 'Confirm' to finish the flow restriction test.")
				showMsgFRTPumpAMixTee = true
			end
		end
		if (frtPumpAMixTee == true or frtPumpBMixTee == true or frtInjectionSystem == true) then
			reportResults("Flow restriction test", 100)		-- pressure is not taken into account here
			status:RemoveStatus("Flow Restriction Test")
		end
		if (testOK == false) then
			ltPumpA = false
			ltPumpB = false
			ltMixTeeSystem = false
			ltInjectionSystem = false
			lpltPumpAB = false
		else
			clearResults()
		end

		if ((ltMixTeeSystem == true or ltInjectionSystem == true) and isLTMessageDisplayed == false) then
			-- this message is only dispayed if there was not a FRT executed
			context:Report(leakTest, Severity.Info, true, "Please disconnect the separation column and plug the open end of the transfer line, then click 'Confirm' to continue.")
		end

		if (ltPumpA == true or ltPumpB == true) then
			context:Log("-- Limits --")
			context:Log("Pump A, Valve A, injection path, loop: {0}",limit.lpsA)
			context:Log("Pump B, valve B:    					{0}",limit.lpsB)
			context:Log("------------------------------------")

			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test pump A", ltPumpA))
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leak test pump B", ltPumpB))

			zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_10_1)
			pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Waste)

			-- set the journal headline here, otherwise it will be overwritten due to several pressure tests.
			local journalTest = "--- Leak test"
			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, journalTest, os.date("%c")))

			local p_leakTest_PumpA = { leakTest_Pump, "Leak test", pressure, zr.A, ltPumpA, parallel.yield, 60, 1.0, 60 }
			local p_leakTest_PumpB = { leakTest_Pump, "Leak test", pressure, zr.B, ltPumpB, parallel.yield, 60, 1.0, 60 }
			local p_logPressSensorOffsetA = { pf.flowSensorOffset, context, zr, pump, zr.A, (ltPumpA and ltPumpB), 30, parallel.yield }
			local p_logPressSensorOffsetB = { pf.flowSensorOffset, context, zr, pump, zr.B, (ltPumpA and ltPumpB), 30, parallel.yield }
			local speedA, speedB, offsetA, offsetB = parallel.run(sleep_200, p_leakTest_PumpA, p_leakTest_PumpB, p_logPressSensorOffsetA, p_logPressSensorOffsetB)
			sleep_200()
			-- speedA and speedB is only returned for pressure == 1000 bar
			if speedA and (speedA > limit.ltPA1000) then
				local msg = "LT: Pump A @1000bar"
				local msg2 = DotNetString.Format("the leakage rate of ({0:#0.00}) is > {1:#0.00}\181L/min.", speedA, limit.ltPA1000)
				context:Sleep(100)
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, msg, speedA, "\181L/min", "N4"))
				context:Report(msg, Severity.Warn, true, msg2)
			end
			if speedB and (speedB > limit.ltPB1000) then
				local msg = "LT: Pump B @1000bar"
				local msg2 = DotNetString.Format("the leakage rate of ({0:#0.00}) is > {1:#0.00}\181L/min.", speedB, limit.ltPB1000)
				context:Sleep(100)
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, msg, speedB, "\181L/min", "N4"))
				context:Report(msg, Severity.Warn, true, msg2)
			end
			if offsetA ~= nil then
--	Don't save the new offset. Only report it.				
--				pf.SetFlowCalibrationOffset(zr.A, offsetA, pump, sleep_200)
--				settings.FlowCalibrationOffsetA = offsetA
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow Sensor A Offset Leak Test Pump", offsetA, "\181L/min", "N4"))
				csv.logValueInCSVFile(context, csvFileName, "Leak Test", "Flow Sensor A Offset Leak Test Pump", offsetA, "\181L/min", 4)
			end
			if offsetB ~= nil then
--	Don't save the new offset. Only report it.				
--				pf.SetFlowCalibrationOffset(zr.B, offsetB, pump, sleep_200)
--				settings.FlowCalibrationOffsetB = offsetB
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow Sensor B Offset Leak Test Pump", offsetB, "\181L/min", "N4"))
				csv.logValueInCSVFile(context, csvFileName, "Leak Test", "Flow Sensor B Offset Leak Test Pump", offsetB, "\181L/min", 4)
			end
			if ltPumpA == false then speedB = speedA end		-- if ltPumpA == false then leak test pump B returns on speedA!!!
			if (continousLeakTestMeasurement == false) then
				if ltPumpA == true then
					if (speedA == false) then
						ltPumpA = false
						testOK = false
						context:Log("Leak test pump A failed due to a large leak")
						report_leak(nil, N.PumpA, N.PumpARearToValve, N.PumpAFrontToValve)
					else
						if (speedA > limit.ltsA)  and (noLimit == false)then
							-- the limit is only valid for 1000 bar test
							context:Log("Leak test pump A failed @1000bar (speedA: {0})", speedA)
							testOK = false
							report_leak(nil, N.PumpA, N.PumpARearToValve, N.PumpAFrontToValve)
						end
					end
				end
				if ltPumpB == true then
					if (speedB == false) then
						ltPumpB = false
						testOK = false
						context:Log("Leak test pump B failed due to a large leak")
						report_leak(nil, N.PumpB, N.PumpBRearToValve, N.PumpBFrontToValve)
					else
						if (speedB > limit.ltsB) and (noLimit == false) then
							-- the limit is only valid for 1000 bar test
							context:Log("Leak test pump B failed @1000bar (speedB: {0})", speedB)
							testOK = false
							report_leak(nil, N.PumpB, N.PumpBRearToValve, N.PumpBFrontToValve)
						end
					end
				end
			end
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 45, 45, 300, sleep_200, baltic.smooth)
		end

		if ((ltMixTeeSystem == true or ltInjectionSystem == true) and testOK == true) then
			status:SetStatus(leakTest)
			-- this message is only dispayed if there was not a FRT executed
			context:Log("-- Limits as a base for limit calculations --")
			context:Log("Pump A, Valve A, injection path, loop:           {0}",limit.ltsA)
			context:Log("Pump B, valve B:                                 {0}",limit.ltsB)
			context:Log("Flow sensor A:                                   {0}",limit.ltfA)
			context:Log("Valve B, trap, transfer line:                    {0}",limit.ltfB)
			context:Log("Negative flow from pump B through flow sensor A: {0}",limit.ltfA2)
			context:Log("------------------------------------")

			zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_10_1)
			---@type number?
			local flowB = nil
			if (ltMixTeeSystem == true) then
				flowB = leakTest_MixTeeSystem({TestPressure=pressure, MaxVolume=60, MaxPressureDeviation=1.0, MeasuringTime=60}, sleep_200)
				context:Signalize(baltic.ColorsRGB.LightGray, N.ValveBToPlug)
				pr.Signalize_Reset(context)
			end
			if (ltInjectionSystem == true and testOK == true) then
				local testMethod = {MaxVolume=60, MaxPressureDeviation=1.0, MeasuringTime=60, TestPressure = 1000 }
				for n=1, #pressure do
					local reducePressureAtTheEnd = pressure[n+1] == nil
					testMethod.TestPressure = pressure[n]
					if pressure[n] ~= 1000 then flowB = nil end		-- detemine the base leakage
					leakTest_Injection_System(flowB, testMethod, reducePressureAtTheEnd, sleep_200)
				end
			end

			pr.Signalize_Reset(context)
			status:RemoveStatus(leakTest)
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 45, 45, 300, sleep_200, baltic.smooth)
		end

		if ((continousLeakTestMeasurement == false) or (ltMixTeeSystem == true or ltInjectionSystem == true and ltPumpA == false and ltPumpB == false)) then
			reportResults("Leak test", 1000)		-- pressure is not taken into account here
		end
		if (ltMixTeeSystem == true or ltInjectionSystem == true) then
			context:Report("Leak test finished", Severity.Info, true, "Please remove the plug and reconnect the separation column, then click 'Confirm' to finish the leak test.")
		end

		if (lpltPumpAB == true and testOK == true) then
			lowPressureLeakTestPumpAB()
		end

		if (ltMixTeeSystem == true or ltInjectionSystem == true) then
			context:WaitForSignal("Leak test finished")
		else
			if (showMsgFRTPumpAMixTee == true) then
				context:WaitForSignal("Flow restriction test pump A mixTee")
				context:Sleep(250)
			end
		end

		if context:GetArgumentValue("Test volumetric wash pump") == true then
			local detergent = 1
			if context:GetArgumentValue("Pump 2") == true then
				detergent = 2
			end

			context:Report("Wash pump test", Severity.Info, true, "Please empty completely the vial in item position 5. 5mL of solvent will be dispensed into it")
			context:WaitForSignal("Wash pump test")

			context:Log("Running wash pump test of channel {0}", detergent)

			local wash_module = pp.QueryModule(execAux, pp.Capabilities.ILcMsWashStation)
			local itemPosition = pp.QueryModules(execAux, "ItemPositionDescription")
			local itemPosition5 = itemPosition[pr.GetItemPosIdx(5, execAux)]

			execLeft:LeaveObject()
			execLeft:MoveToObject( wash_module, pp.Waste, true, true, true )
			execLeft:PenetrateObject( wash_module, pp.Waste, pp.Quantity(15, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
			pp.VolumeToBePumped(execLeft, detergent, 3000, 50)		-- this takes 60 seconds
			execLeft:Wait(pp.Quantity(1, "s"))		-- [sec]
			execLeft:LeaveObject()
			execLeft:MoveToObject( itemPosition5, 1, true, true, true )
			execLeft:PenetrateObject( itemPosition5, 1, pp.Quantity(15, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
			pp.VolumeToBePumped(execLeft, detergent, 5000, 50)		-- this takes 100 seconds
			execLeft:Wait(pp.Quantity(1, "s"))		-- [sec]
			execLeft:LeaveObject()
			if detergent == 2 then
				-- clean the LCP tool with aqueous
				execLeft:MoveToObject( wash_module, pp.Waste, true, true, true )
				execLeft:PenetrateObject( wash_module, pp.Waste, pp.Quantity(15, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
				pp.VolumeToBePumped(execLeft, pp.Aqueous, 1000, 50)		-- this takes 20 seconds
				execLeft:Wait(pp.Quantity(1, "s"))		-- [sec]
			end
			pp.SetLCToolValve(execLeft, pp.LcToolValveClose)

			while not execLeft.IsIdle do
				sleep_200()
			end
			context:Report("Wash pump test:", Severity.Info, true, "The vial filling must be 5mL.")
			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "--- Wash Pump Test", os.date("%c")))
			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Test volumetric wash pump", true))
		end
	end
	pump:SetSettings(settings)

	if (finished == true) and (self_Test.showMessage == true) then
		context:Report("Self diagnostic", Severity.Info, true, "finished")
	elseif (testOK == true) and (self_Test.showMessage == true) then
		context:Report("Diagnostic", Severity.Info, true, "passed")
	end
	while not execLeft.IsIdle do
		sleep_100()
	end

	pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 30, sleep_100, baltic.smooth)

	dictator:Dispose()

	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)
end

return P
