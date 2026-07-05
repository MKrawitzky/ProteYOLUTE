-- require('lldebugger').start()

local Date = "2025/07/23"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type JournalEntry
local JournalEntry = luanet.import_type("Bruker.Lc.JournalEntry")
---@type LogTo
local LogTo = luanet.import_type("Bruker.Lc.Business.LogTo")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---@param context InitHelper
function Initialize (context)
	context.Name = "Calibration"
	context.Description = "Sensor calibration procedures"
	context.Hidden = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Calibration

	context:DeclareParameter("Calibrate compressibility factor", true, nil, "radio", false, "Evaluate the solvent B compressibility factor", "", 0, "A")

	--> Service parameter
	context:DeclareParameter("Set compressibility factor", false, nil, "radio", true, "Set the solvent B compressibility factor", "", 0, "A")
	context:DeclareParameter("Compressibility factor B", 1.70, nil, "decimal", true, "Compressibility factor B to be set", "", 30, "", 2)
	context:DeclareParameter("Calibrate pressure sensors offset", false, nil, "radio", true, "Calibrate the offset of the pressure sensors, starting at offsets = 0", "", 0, "A")
	context:DeclareParameter("Set pressure sensors offset", false, nil, "radio", true, "Set the offset of the pressure sensors", "", 0, "A")
	context:DeclareParameter("Pressure sensor offset A", 140, nil, "decimal", true, "Pressure sensor offset A to be set. Valid range (140, 350).", "", 30, "", 2)
	context:DeclareParameter("Pressure sensor offset B", 140, nil, "decimal", true, "Pressure sensor offset B to be set. Valid range (140, 350).", "", 30, "", 2)
	--< Service parameter

	context:DeclareParameter("Separator0", "", nil, "separator", "", "")

--	context:DeclareParameter("Calibrate flow sensors new", true)	-- PBNE-660 disabled due to future version

	context:DeclareParameter("Calibrate flow factor and offset", false, nil, "radio", false, "Sensor autocalibration of factor and offset for A and B side.", "", 0, "A")
	context:DeclareParameter("Factor and Offset A calibration", true, nil, "check", false, "Sensor A autocalibration of factor and offset", "FlowSensorA.PNG", 30)
	context:DeclareParameter("Factor and Offset B calibration", true, nil, "check", false, "Sensor B autocalibration of factor and offset", "FlowSensorB.PNG", 30)

	context:DeclareParameter("Calibrate flow offset", false, nil, "radio", false, "Sensor offset calibration", "", 0, "A")
	context:DeclareParameter("Offset A calibration", true, nil, "check", false, "Sensor A offset calibration", "FlowSensorA.PNG", 30)
	context:DeclareParameter("Offset B calibration", true, nil, "check", false, "Sensor B offset calibration", "FlowSensorB.PNG", 30)

	context:DeclareParameter("Set flow factor", false, nil, "radio", false, "Set factor and perform an offset calibration", "", 0, "A")
	context:DeclareParameter("Set factor A", true, nil, "check", false, "Set factor A and perform an offset calibration", "FlowSensorA.PNG", 30)
	context:DeclareParameter("Factor A value", 1.0, nil, "decimal", false, "Factor A to be set", "FlowSensorA.PNG", 30, "", 2)
	context:DeclareParameter("Set factor B", true, nil, "check", false, "Set factor B and perform an offset calibration", "FlowSensorB.PNG", 30)
	context:DeclareParameter("Factor B value", 2.15, nil, "decimal", false, "Factor B to be set", "FlowSensorB.PNG", 30, "", 2)
end

---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (_, context)
	local validation = 	require "validation"

	validation.verify_specified(context, "Factor A value")
	validation.verify_range(context, "Factor A value", 0.1, 10)

	validation.verify_specified(context, "Factor B value")
	validation.verify_range(context, "Factor B value", 0.1, 10)
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
	context:Log("Lua date: {0}", Date)

	require "degas"
	local baltic = require "baltic"
	local csv = require "csv_file_logging"
	---@type Zirconium
	local zr = require "zirconium"
	local pp = require "palplus"

	local parallel = require "parallel"
	local pf = require "pump_functions"
	local pr = require "PreRunFunctions"

	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end

	local procStartTime = pf.now()
	local calFlowSensors_New	= false	-- context:GetArgumentValue("Calibrate flow sensors new")	-- PBNE-660 false due to future version
	-- channel A
	---@type boolean
	local calFactorOffset_A = context:GetArgumentValue("Factor and Offset A calibration") and context:GetArgumentValue("Calibrate flow factor and offset")
	---@type boolean
	local setFactor_A =       context:GetArgumentValue("Set factor A") and context:GetArgumentValue("Set flow factor")
	---@type boolean
	local calOffset_A =       context:GetArgumentValue("Offset A calibration") and context:GetArgumentValue("Calibrate flow offset")
	-- channel B
	---@type boolean
	local calFactorOffset_B = context:GetArgumentValue("Factor and Offset B calibration") and context:GetArgumentValue("Calibrate flow factor and offset")
	---@type boolean
	local setFactor_B =       context:GetArgumentValue("Set factor B") and context:GetArgumentValue("Set flow factor")
	---@type boolean
	local calOffset_B =       context:GetArgumentValue("Offset B calibration") and context:GetArgumentValue("Calibrate flow offset")

	local calFlow = 0.5
	local endTime = 0

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	local N = baltic.Naming

	local csvFileName = "Calibration_Results"

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)

	zr.storePumpVolume(pump, true)
	--	Signalize_Reset
	pr.Signalize_Reset(context)
	context:Log("Signal reset")
	context:ShowComposition(true)

	pr.iniFlowResistance(context, pump)

	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)

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
	zr.logInstrSettings(context, settings, "Calibration")

	local function sleep_100()
		context:Sleep(100)
	end
	local function sleep_250()
		context:Sleep(250)
	end

	pf.SetMaxPressureLimit(zr.A, pressSettings.GradientPumpCutoffPressure, pump, sleep_100)
	pf.SetMaxPressureLimit(zr.B, pressSettings.GradientPumpCutoffPressure, pump, sleep_100)

	local method = {
		TestPressure = 20,				-- [bar]
		BuildPressureSpeed = 100,		-- [uL/min]
		TestTime = 4*60,				-- [seconds] (per test)
		LogInterval = 1,				-- [seconds]
		LogSize = 40,
		CalibrationDuration = 300,		-- used in 'monitor_flow' [int]
		FlowCalDuration = 80,			-- [min]
		FlowCalBuildPres_A = 60,			-- [bar] (pressure for 500nL/min)
		FlowCalBuildPres_B = 50			-- [bar] (pressure for 500nL/min)
	}

	---Insert a new value at position one
	---@param array table
	---@param size number
	---@param value number
	local function push_insert(array,size,value)
		for i=1,size-1 do
			array[i] = array[i+1]
		end

		array[size] = value
	end

	---Log the array content
	---@param array table
	---@param size number
	local function log_array(array,size)
		for i=1,size do
			context:Log("ar[{0}] = {1}",i,array[i])
		end
	end

	---Return the average of the array
	---@param array table
	---@param size number
	---@return number
	local function average(array,size) --return average 
		local sum=0
		for i=1,size do
			sum = sum + array[i]
		end
		return sum/size
	end

	---Return the standard deviation of the array
	---@param array table
	---@param size number
	---@return number
	local function stdev(array,size) --standard deviation
		local sum=0
		local avg=average(array,size)

		for i=1,size do
			local dev=avg-array[i]
			sum = sum + dev*dev
		end

		return math.sqrt(sum/size)
	end

	---Return the average of the differences of the values
	---@param array table
	---@param size number
	---@return number
	local function avg_delta(array,size) --return average change between array values
		local sum=0
		for i=1,size-1 do
			sum = sum + array[i+1]-array[i]
		end
		return sum/(size-1)
	end

	---Monitor the flow of the desired channel
	---@param description string
	---@param channel Channel
	---@param pistonspeed number
	local function monitor_flow(description, channel, pistonspeed)
		context:Log("")
		context:Log(baltic.devider)
		context:Log("START: {0}",description)
		local flowFactor = pump:GetFlowCalibrationFactor(channel)
		local flowlog = {}
		for i=1,method.LogSize do
			flowlog[i]=0
		end

		local n = method.CalibrationDuration
		for i = 1,n do
			local flow = pump:GetCurrentFlow(channel)
			push_insert(flowlog,method.LogSize,flow)
			context:Sleep(1000)
		end
		context:Log("END: {0}",description)
		context:Log(baltic.devider)
		context:Log("FlowLog:")
		log_array(flowlog,method.LogSize)

		local avgd=avg_delta(flowlog,method.LogSize)
		local avg=average(flowlog,method.LogSize)
		local sd=stdev(flowlog,method.LogSize)
		context:Log("Average change of last logged flowrates: {0}",avgd)
		context:Log("Average value of last logged flowrates: {0}",avg)
		context:Log("Standard deviation of last logged flowrates: {0}",sd)
		if (pistonspeed > 0) then
			if (math.abs(avgd) < 0.001 and sd < 0.005) then 
				context:Log("Conclusion: use for flow sensor calibration: {0} vs {1}",pistonspeed,avg)
				--write calibration point to pump if accepted. OBH
				if avg ~= 0 then	-- devide by zero prevention
					local newFactor = 0.5/avg*flowFactor
					if newFactor < 0.1 or newFactor > 10 then
						-- new factor is out of range
						local msg = DotNetString.Format("Factor {0} is out of range (0.1 - 10.0)! Flow sensor {1} has not been calibrated.", newFactor, channel)
						context:Report("Flow sensor", Severity.Warn, true, msg)
					else
						local flowCalibrationOffset = zr.GetFlowCalibrationOffset(pump, channel)
						local fOff = flowCalibrationOffset*0.5/avg
						if math.abs(fOff) > 0.1 then
							-- new offset is out of range
							local msg = "Offset ("..pf.noExp(fOff,4)..") is out of range (+-0.1). Flow sensor has not been calibrated."
							context:Report("Flow sensor", Severity.Warn, true, msg)
						else
							pf.SetFlowCalibrationFactor(channel,newFactor, pump, sleep_100)
							context:Sleep(1000)
							--offset is factor dependent, recalculate
							pf.SetFlowCalibrationOffset(channel, fOff, pump, sleep_100)
							if channel == zr.A then
								settings.FlowCalibrationFactorA = newFactor
								settings.FlowCalibrationOffsetA = fOff
								csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration factor A", newFactor, "", 2)
								csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration offset A", fOff, "", 5)
							else
								settings.FlowCalibrationFactorB = newFactor
								settings.FlowCalibrationOffsetB = fOff
								csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration factor B", newFactor, "", 2)
								csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration offset B", fOff, "", 5)
							end
						end
					end
				end
			else
				context:Log("Conclusion: not stable, repeat")
			end
		end
		context:Log("")
		context:Log(baltic.devider)
	end

	---Build pressure on the desired channel
	---@param channel Channel
	---@param pressure number
	---@param dummyFunction boolean
	---@param yield_function function
	local function build_pressure(channel, pressure, dummyFunction, yield_function)
		if dummyFunction then return end
		if zr.IsEmpty(pump, channel) then return end
		context:Log("build_pressure {0} to {1} bar", channel, pressure)

		pf.Manualmode_Pump_constantSpeed(channel, method.BuildPressureSpeed, pump, yield_function)
		while not zr.IsEmpty(pump, channel) do 
			if (pump:GetCurrentPressure(channel) > pressure*.9) then 
				pf.Manualmode_Pump_constantSpeed(channel, (method.BuildPressureSpeed/5), pump, yield_function)
			end
			if (pump:GetCurrentPressure(channel) > pressure*.99) then 
				pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_function)
				pf.Manualmode_Pump_constantPressure(channel, pressure, pump, yield_function)
				break
			end
			yield_function()
		end
	end

	---Build pressure on channel A and B
	---@param pressureA number
	---@param pressureB number
	---@param yield_function function
	local function build_pressure_AB(pressureA, pressureB, yield_function)
		local p_press_a = { build_pressure, zr.A,pressureA, false, parallel.yield }
		local p_press_b = { build_pressure, zr.B, pressureB, false, parallel.yield }
		parallel.run(yield_function, p_press_a, p_press_b)
	end

	---Determine the new offset values for the flow sensors
	---@param description string
	---@param stps number
	---@return number
	---@return number
	local function fscal_composite(description, stps)
		context:Log("Flow sensor calibration started")
		local logarA = {}
		local logarB = {}

		local logFlowA = {}
		local logFlowB = {}

		context:Sleep(1000)
		for i = 1,stps do
			context:Sleep(method.LogInterval*1000) 
			logarA[i] = pump:GetPistonPosition(zr.A)
			logarB[i] = pump:GetPistonPosition(zr.B)
			logFlowA[i] = pump:GetCurrentFlow(zr.A)
			logFlowB[i] = pump:GetCurrentFlow(zr.B)
		end
		local endPressureA=pump:GetCurrentPressure(zr.A)
		local endPressureB=pump:GetCurrentPressure(zr.B)

		context:Log("")
		context:Log(baltic.devider)
		context:Log("Description: {0}",description)
		context:Log("EndPressureA : {0}", endPressureA)
		context:Log("EndPressureB : {0}", endPressureB)
		context:Log("")
		local n=0
		local sum=0

		for i = 2,stps do 
			context:Log("PumpPosA{0} : {1} uL, delta: {2} nL, flowSensorValue: {3} nL/min", i, logarA[i],1000*(logarA[i]-logarA[i-1]),1000*logFlowA[i])
			if (i>stps/2) then -- average based on the second half of logged piston positions
				sum = sum+(logarA[i]-logarA[i-1])
				n = n+1 
			end
		end
		local leakA = 1000*60*sum/n/method.LogInterval
		context:Log("Pump A average flow: [nL/min]: {0} based on {1} logged values ",leakA,n)

		context:Log("")
		n=0
		sum=0
		local sumB = 0
		local sumA = 0
		for i = 2,stps do
			context:Log("PumpPosB{0} : {1} uL, delta: {2} nL, flowSensorValue: {3} nL/min", i, logarB[i],1000*(logarB[i]-logarB[i-1]),1000*logFlowB[i])
			if (i>stps/2) then
				sum = sum+(logarB[i]-logarB[i-1])
				sumB = sumB + logFlowB[i]
				sumA = sumA + logFlowA[i]
				n = n+1
			end
		end
		local leakB = 1000*60*sum/n/method.LogInterval
		local actOffsets = {pump:GetFlowCalibrationOffsetA(), pump:GetFlowCalibrationOffsetB()}
		context:Log("Pump B average flow: [nL/min]:{0} based on {1} logged values",leakB,n)
		context:Log("FSA average flow:    [nL/min]:{0} @ Offset: {1}",(1000*sumA/n),actOffsets[1])
		context:Log("FSB average flow:    [nL/min]:{0} @ Offset: {1}",(1000*sumB/n),actOffsets[2])
		context:Log("FSA new offset:       [\181L]:{0}",((sumA/n)-actOffsets[1]))
		context:Log("FSB new offset:       [\181L]:{0}",((sumB/n)-actOffsets[2]))

		context:Log("")
		context:Log(baltic.devider)
		-- Return FSA and FSB zero points. OBH
		return (-sumA/n+actOffsets[1]), (-sumB/n+actOffsets[2])
	end

	---Calculate the time for the calibration
	---@return number
	local function calcEndTime()
		local end_Time = 240
--		if context:GetArgumentValue("Calibrate flow sensors new") then endTime = endTime + 2110 end		-- PBNE-660 disabled due to future version
		if calFactorOffset_A then end_Time = end_Time + 5640 end
		if calFactorOffset_B then end_Time = end_Time + 5130 end
		if (setFactor_A or calOffset_A or setFactor_B or calOffset_B) then end_Time = end_Time + 280 end

		return end_Time
	end

	---Calculate the new offsets
	---@param endT number
	---@param stps number
	local function calOffsets(endT, stps)
		context:Log(baltic.devider)
		context:Log("--- starting offset calibration channel A / B")
		context:Log("--- Flow Sensor Calibration: set values for calibration procedure")
		context:Log("--- Flow Sensor A  Offset: {0}", pump:GetFlowCalibrationOffsetA())
		context:Log("---                Factor: {0}", pump:GetFlowCalibrationFactor(zr.A))
		context:Log("--- Flow Sensor B  Offset: {0}", pump:GetFlowCalibrationOffsetB())
		context:Log("---                Factor: {0}", pump:GetFlowCalibrationFactor(zr.B))
		context:Log(baltic.devider)
		local oc_msg = DotNetString.Format("Offset calibration, the procedure finishes at about {0:#0.0} minutes",endT)
		status:SetStatus(oc_msg)

		local a,b = fscal_composite("Test1: ZeroPoints. No pressure, flow sensors isolated", stps)
		if calOffset_A then
			if math.abs(a) <= 0.1 then
				pf.SetFlowCalibrationOffset(zr.A, a, pump, sleep_100)
				settings.FlowCalibrationOffsetA = a
				csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration offset A", a, "", 5)
			else
				local msg = "Offset ("..pf.noExp(a,4)..") is out of range (+-0.1)"
				context:Report("Flow sensor A", Severity.Warn, true, msg)
				calFactorOffset_A = false
			end
			context:Sleep(1000)
		end
		if calOffset_B then
			if math.abs(b) <= 0.1 then
				pf.SetFlowCalibrationOffset(zr.B, b, pump, sleep_100)
				settings.FlowCalibrationOffsetB = b
				csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration offset B", b, "", 5)
			else
				local msg = "Offset ("..pf.noExp(b,4)..") is out of range (+-0.1)"
				context:Report("Flow sensor B", Severity.Warn, true, msg)
				calFactorOffset_B = false
			end
		end

		context:Sleep(30000) -- display situation after offset calibration	

		status:RemoveStatus(oc_msg)
		context:Log(baltic.devider)
	end

	---Determine the leakage of channel A and B 
	---@param calFlw number
	---@return number
	---@return number
	local function getLeakageAB(calFlw)
		---Measure the actual offset
		---@return number
		---@return number
		local function getOffset()
			context:Log("getOffset()")
			-- flush flow sensors
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee)
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee)
			context:Sleep(250)
			pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Waste)
			context:Sleep(250)
			pf.Manualmode_Pump_constantFlow_binary(context, 1.0, 1.0, pump, zr, sleep_250)
			context:Sleep(20000)
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 60, sleep_250, baltic.smooth)

			-- get offsets
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste)
			context:Sleep(250)
			context:Signalize(baltic.ColorsRGB.Blue, N.FlowA, N.FlowA)
			context:Signalize(baltic.ColorsRGB.LightGray, N.FSAToMixTee, N.FSAToMixTee)
			context:SignalizeText(baltic.ColorsRGB.White, N.FlowA)
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste)
			context:Sleep(250)
			context:Signalize(baltic.ColorsRGB.Red, N.FlowB, N.FlowB)
			context:Signalize(baltic.ColorsRGB.LightGray, N.FSBToMixTee, N.FSBToMixTee)
			context:SignalizeText(baltic.ColorsRGB.White, N.FlowB)
			context:Sleep(2000)
			pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Trap)
			context:Sleep(20000)			-- wait 20s

			local offsetA = 0
			local offsetB = 0
			local i = 60
			while i>0 do
				offsetA = offsetA + pump:GetCurrentFlow(zr.A)
				offsetB = offsetB + pump:GetCurrentFlow(zr.B)
				context:Sleep(1000)
				i=i-1
			end
			context:Signalize(baltic.ColorsRGB.Normal, N.FlowA, N.FlowB)

			offsetA = offsetA / 60			-- [uL/min]
			offsetB = offsetB / 60			-- [uL/min]
			context:Log("  Flow sensor offset A: {0}", offsetA)
			context:Log("  Flow sensor offset B: {0}", offsetB)
			return offsetA, offsetB
		end
		---Return the pressure corresponding to the flow
		---@param testChannel Channel
		---@param blockChannel Channel
		---@param cF number
		---@return number
		local function getTestPressure(testChannel, blockChannel, cF)
			context:Log("getTestPressure({0}, {1}, {2})", testChannel, blockChannel, cF)
			zr.SetValvePosition(context, pump, testChannel, baltic.PumpValve.MixTee)
			context:Sleep(250)
			zr.SetValvePosition(context, pump, blockChannel, baltic.PumpValve.Waste)
			context:Sleep(250)
			pf.Manualmode_Pump_constantSpeed(blockChannel, 0, pump, sleep_250)
			pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Waste)
			context:Sleep(250)
			pf.Manualmode_Pump_constantFlow(testChannel, cF, pump, sleep_250)
			local flow = 0
			local flowLimit = cF*0.995
			local noTimeOut =  true
			local cnt = 0
			while ((flow < flowLimit) and noTimeOut) do
				context:Sleep(1000)
				flow = pump:GetCurrentFlow(testChannel)
				if cnt >= 120 then noTimeOut = false end
				cnt = cnt + 1
			end
			context:Sleep(60000)
			local actPressure = pump:GetCurrentPressure(testChannel)
			return actPressure
		end
		---Return the one second average piston position
		---@param ch Channel
		---@return number
		local function getPistonPosition(ch)
			context:Log("getPistonPosition({0})", ch)
			local piston = 0
			for i=1, 10 do
				piston = piston + pump:GetPistonPosition(ch)
				context:Log("Piston {0} position: {1}", ch, piston)
				context:Sleep(100)
			end
			return piston / 10
		end
		---Determine the leakage of the desired channel
		---@param testChannel Channel
		---@param blockChannel Channel
		---@param pressure number
		---@param offset number
		---@return number
		local function getLeakage(testChannel, blockChannel, pressure, offset)
			context:Log("getLeakage({0}, {1}, {2}, {3})", testChannel, blockChannel,  pressure, offset)
			zr.SetValvePosition(context, pump, testChannel, baltic.PumpValve.MixTee)
			context:Sleep(250)
			zr.SetValvePosition(context, pump, blockChannel, baltic.PumpValve.Waste)
			context:Sleep(250)
			pf.Manualmode_Pump_constantSpeed(blockChannel, 0, pump, sleep_250)
			pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Trap)
			if testChannel == zr.B then
				context:Signalize(baltic.ColorsRGB.Red, N.PumpBFrontToValve, N.ValveBGroove, N.ValveBToFS, N.FlowB, N.FSBToMixTee, N.MixTeeToTrapValve)
				context:SignalizeText(baltic.ColorsRGB.White, N.FlowB)
			end

			context:Sleep(250)
			local msg = DotNetString.Format("Determine pump {0} leakage", testChannel)
			status:SetStatus(msg)
			method.BuildPressureSpeed = 50
			build_pressure(testChannel, pressure, false, sleep_250)
			context:Sleep(120000)
			local piston1 = getPistonPosition(testChannel)
			local leakageBehindFS = 0
			local i = 150
			while i>0 do
				leakageBehindFS = leakageBehindFS + pump:GetCurrentFlow(testChannel) - offset
				context:Sleep(1000)
				i=i-1
			end
			leakageBehindFS = leakageBehindFS / 150			-- [uL/min]
			context:Sleep(150000)
			local piston2 = getPistonPosition(testChannel)
			local leakage = math.max(((piston2-piston1)/5-leakageBehindFS),0)
			pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Inject)

			context:Log("	piston1 					{0}: {1}", testChannel, piston1)
			context:Log("	piston2 					{0}: {1}", testChannel, piston2)
			context:Log("	leakage behind flow sensor 	{0}: {1} \181L/min", testChannel, leakageBehindFS)
			context:Log("	pump leakage 				{0}: {1} \181L/min", testChannel, leakage)
			context:Sleep(500)
			status:RemoveStatus(msg)
			return leakage
		end
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 60, sleep_250, baltic.smooth)

		local offsetA, offsetB = getOffset()

		-- get pressure A @ 500nL through flow sensor
		pr.Signalize_Reset(context)
		local pressureA = getTestPressure(zr.A, zr.B, calFlw)
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 60, sleep_250, baltic.smooth)
		context:Signalize(baltic.ColorsRGB.Normal, N.TrapValveToWaste, N.ValveTShortGroove)
		-- get leakage rate A:
		local leakageA = getLeakage(zr.A, zr.B, pressureA, offsetA)
		pr.Signalize_Reset(context)

		-- get pressure B @ 500nL through flow sensor
		local pressureB = getTestPressure(zr.B, zr.A, calFlw)
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 60, sleep_250, baltic.smooth)
		context:Signalize(baltic.ColorsRGB.Normal, N.TrapValveToWaste, N.ValveTShortGroove)
		-- get leakage rate B:
		local leakageB = getLeakage(zr.B, zr.A, pressureB, offsetB)
		pr.Signalize_Reset(context)

		return leakageA, leakageB
	end

	---Determine the flow factor of the flow sensor
	---@param ch Channel
	---@param cF number
	---@param leakage number
	---@param actFlowFactor number
	---@param newFlowFactor number
	---@return boolean
	---@return number
	local function calFlowSensor(ch, cF, leakage, actFlowFactor, newFlowFactor)
		local function getPistonPosition()
			local piston = 0
			for i=1, 10 do
				piston = piston + pump:GetPistonPosition()
				context:Log("Piston {0} position: {1}", ch, piston)
				context:Sleep(100)
			end
			return piston / 10
		end

		local preparingTime = 120000		-- 2 minutes
		local waitingTime 	= 300000		-- 5 minutes
		local channel = 1					-- 1: channel A; 2: channel B
		local continue = true
		if leakage > 0.05 then
			local errorMSG = DotNetString.Format("The leakage ({0:#0.0000}\181L/min) of pump {1} is too high (limit: 0.05 \181L/min).\nEliminate the leakage and run the calibration again.", leakage, ch)
			context:Report("Calibrate flow sensor failed", Severity.Warn, true, errorMSG)
			continue = false
		else
			local dictator3 = LoggingDictator.Prevent(pump)
			local msg = "Calibrate flow sensor "--..ch
			status:SetStatus(msg)
			if ch == zr.A then
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste)
			else
				channel = 2						-- channel B
				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste)
			end
			context:Sleep(250)
			pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Waste)
			context:Sleep(250)
			zr.SetValvePosition(context, pump, ch, baltic.PumpValve.MixTee)
			pr.Signalize_Reset(context)
			context:Sleep(250)
			pf.Manualmode_Pump_constantFlow(ch, cF, pump, sleep_250)
			context:Sleep(preparingTime)
			local piston1 = getPistonPosition()
			context:Sleep(waitingTime)
			local piston2 = getPistonPosition()
			context:Sleep(250)
			local flow = (piston2-piston1)/(waitingTime/60000)-leakage
			context:Sleep(250)
			newFlowFactor[channel] = flow/cF*newFlowFactor[channel]
			context:Log(baltic.devider)
			context:Log("Channel,         {0}", ch)
			context:Log("  leakage,       {0} \181L/min", leakage)
			context:Log("  piston1,       {0}", piston1)
			context:Log("  piston2,       {0}", piston2)
			context:Log("  flow,          {0} \181L/min", flow)
			context:Log("  setFlow,       {0} \181L/min", cF)
			context:Log("  actFlowFactor, {0}", actFlowFactor[channel])
			context:Log("  newFlowFactor, {0}", newFlowFactor[channel])
			context:Log(baltic.devider)
			context:Sleep(1000)
			status:RemoveStatus(msg)
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 60, sleep_250, baltic.smooth)
			local dic = LoggingDictator.Prevent(pump)
		end
		return continue, newFlowFactor[channel]
	end

	---Set the flow factors in the pump
	---@param factors FlowAB
	local function setFlowSensorFactors(factors)
		pf.SetFlowCalibrationFactor(zr.A, factors[1], pump, sleep_100)
		pf.SetFlowCalibrationFactor(zr.B, factors[2], pump, sleep_100)
		settings.FlowCalibrationFactorA = factors[1]
		settings.FlowCalibrationFactorB = factors[2]
		csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration factor A", factors[1], "", 2)
		csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration factor B", factors[2], "", 2)
	end

	---Store the new flow factors in the pump
	---@param actFlowFactor FlowAB
	---@param newFlowFactor FlowAB
	---@param end_Time number
	---@return boolean
	---@return number
	local function setFlowFactors(actFlowFactor, newFlowFactor, end_Time)
		context:Log("    actFlowFactor A: {0}", actFlowFactor[1])
		context:Log("    newFlowFactor A: {0}", newFlowFactor[1])
		if (newFlowFactor[1] > 0.5) and (newFlowFactor[1] < 10) then
			local deltaFactor = math.abs(newFlowFactor[1]-actFlowFactor[1])
			context:Log("    deltaFactor A: {0}", deltaFactor)
			if (deltaFactor > 0.2) then
				local now = pf.now()/60
				local msg1 = DotNetString.Format("New flow factor ({0:#0.0000}) is far away from previous factor ({1:#0.0000}).\nClick 'Abort' to skip saving the new factor.\nClick 'Confirm' to save the new factor.", newFlowFactor[1], actFlowFactor[1])
				context:Report("Sensor A", Severity.Warn, true, msg1)
				setFlowSensorFactors(actFlowFactor)				-- restore settings if user aborts the calibration
				context:WaitForSignal("Sensor A")
				setFlowSensorFactors(newFlowFactor)				-- user want the calibration to be continued therefore set the new factors
				end_Time = end_Time + pf.now()/60 - now
			end
			pf.SetFlowCalibrationFactor(zr.A, newFlowFactor[1], pump, sleep_100)
			context:Sleep(250)
			settings.FlowCalibrationFactorA = newFlowFactor[1]
			csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration factor A", newFlowFactor[1], "", 2)
		else
			local msg = DotNetString.Format("Flow factor ({0:#0.0000}) is out of range (0.1-10)", newFlowFactor[1])
			context:Report("Sensor A", Severity.Warn, true, msg)
			return false, end_Time
		end
		context:Log("    actFlowFactor B: {0}", actFlowFactor[2])
		context:Log("    newFlowFactor B: {0}", newFlowFactor[2])
		if (newFlowFactor[2] > 0.5) and (newFlowFactor[2] < 10) then
			local deltaFactor = math.abs(newFlowFactor[2]-actFlowFactor[2])
			context:Log("    deltaFactor B: {0}", deltaFactor)
			if (deltaFactor > 0.2) then
				local now = pf.now()/60
				local msg3 = DotNetString.Format("New flow factor ({0:#0.0000}) is far away from previous factor ({1:#0.0000}).\nClick 'Abort' to skip saving the new factor.\nClick 'Confirm' to continue and save the new factor.", newFlowFactor[2], actFlowFactor[2])
				context:Report("Sensor B", Severity.Warn, true, msg3)
				setFlowSensorFactors(actFlowFactor)				-- restore settings if user aborts the calibration
				context:WaitForSignal("Sensor B")
				setFlowSensorFactors(newFlowFactor)				-- user want the calibration to be continued therefor set the new factors
				end_Time = end_Time + pf.now()/60 - now
			end
			pf.SetFlowCalibrationFactor(zr.B, newFlowFactor[2], pump, sleep_100)
			context:Sleep(250)
			settings.FlowCalibrationFactorB = newFlowFactor[1]
			csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow calibration factor B", newFlowFactor[1], "", 2)
		else
			local msg2 = DotNetString.Format("Flow factor ({0:#0.0000}) is out of range (0.1-10)", newFlowFactor[2])
			context:Report("Sensor B", Severity.Warn, true, msg2)
			return false, end_Time
		end
		return true, end_Time
	end

	---Logs the options of this procedure to twinscape
	---The actual calibration values of the pump are logged always and don't need to be provided again here
	local function logParametersToTwinscape ()
		---@type IJournal
		local journal = context:GetProcedureParticipant(baltic.JournalRole)
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set compressibility factor", context:GetArgumentValue("Set compressibility factor")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Calibrate compressibility factor", context:GetArgumentValue("Calibrate compressibility factor")))

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Calibrate pressure sensors offset", context:GetArgumentValue("Calibrate pressure sensors offset")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set pressure sensors offset", context:GetArgumentValue("Set pressure sensors offset")))

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Calibrate flow factor and offset", context:GetArgumentValue("Calibrate flow factor and offset")))
		if (context:GetArgumentValue("Calibrate flow factor and offset")) then
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Factor and Offset A calibration", context:GetArgumentValue("Factor and Offset A calibration")))
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Factor and Offset B calibration", context:GetArgumentValue("Factor and Offset B calibration")))
		end

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Calibrate flow offset", context:GetArgumentValue("Calibrate flow offset")))
		if (context:GetArgumentValue("Calibrate flow offset")) then
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Offset A calibration", context:GetArgumentValue("Offset A calibration")))
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Offset B calibration", context:GetArgumentValue("Offset B calibration")))
		end

		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set flow factor", context:GetArgumentValue("Set flow factor")))
		if (context:GetArgumentValue("Set flow factor")) then
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set factor A", context:GetArgumentValue("Set factor A")))
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set factor B", context:GetArgumentValue("Set factor B")))
		end
	end

-- Main starts here
	csv.logValueInCSVFile(context, csvFileName, "Calibration started", "", "" ,"", -1)

	if context:GetArgumentValue("Set compressibility factor") == true then
		settings.CompressibilityFactorB = context:GetArgumentValue("Compressibility factor B")
		csv.logValueInCSVFile(context, csvFileName, "Calibration", "Compressibility Factor B", context:GetArgumentValue("Compressibility factor B"), "", 2)
		context:Sleep(500)
		pump:SetSettings(settings)
	elseif context:GetArgumentValue("Calibrate compressibility factor") == true then
		local function evaluateResultBFactor()
			pf.Manualmode_Pump_constantPressure(zr.A, 1000, pump, sleep_100)
			pf.Manualmode_Pump_constantPressure(zr.B, 1000, pump, sleep_100)
			local timeOut = pf.now() + 120		-- 120 seconds timeout
			local startPressureA = pump:GetCurrentPressure(zr.A)
			local startPressureB = pump:GetCurrentPressure(zr.B)
			local endPressureA, endPressureB = 0, 0
			while  true do
				startPressureA = pump:GetCurrentPressure(zr.A)
				startPressureB = pump:GetCurrentPressure(zr.B)
				if (startPressureA > 100) or (startPressureB > 100) then break end
				if (pf.now() > timeOut) then return nil end
				sleep_100()
			end
			-- stop if there is too much air in one pump
			if (startPressureA < startPressureB*0.25) or (startPressureB < startPressureA*0.25) then return nil end
			timeOut = pf.now() + 120		-- 120 seconds timeout
			while  true do
				endPressureA = pump:GetCurrentPressure(zr.A)
				endPressureB = pump:GetCurrentPressure(zr.B)
				if (endPressureA > 500) or (endPressureB > 500) then break end
				if (pf.now() > timeOut) then return nil end
				sleep_100()
			end
			pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

			local pressureDifferenceA = endPressureA - startPressureA
			local pressureDifferenceB = endPressureB - startPressureB
			context:Log("pressureDifferenceA = {0}, pressureDifferenceB = {1}", pressureDifferenceA, pressureDifferenceB)

			return pf.noExp(pressureDifferenceA/pressureDifferenceB, 2)
		end

		local function setValves()
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 10, 10, 300, sleep_100, false)
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste, nil)
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste, nil)
			context:Sleep(1000)
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Compress, nil)
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Compress, nil)
		end

		zr.ChangePressurePID(context, pump, {P=100, I=100, D=100, ResultMin=-800000,  ResultMax=800000})
		settings = pump:GetSettings()

		local resultBFactor
		for _=1, 5, 1 do
			setValves()
			resultBFactor = evaluateResultBFactor()
			if (resultBFactor ~= nil) then
				-- set the new compressibility factor only if there was not too much air in the pumps
				context:Log("Compressibility factor B = {0}", settings.CompressibilityFactorB * resultBFactor)
				context:Log("resultBFactor = {0}", resultBFactor)
				settings.CompressibilityFactorB = settings.CompressibilityFactorB * resultBFactor
				csv.logValueInCSVFile(context, csvFileName, "Calibration", "Compressibility Factor B", settings.CompressibilityFactorB, "", 2)
				pump:SetSettings(settings)
			else
				break
			end
			if (resultBFactor > 0.95) and (resultBFactor < 1.05) then break end
		end
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)
		local functionName = "Calibrate compressibility factor"
		if (resultBFactor ~= nil) then
			context:Report(functionName, Severity.Info, true, "passed.")
		else
			context:Report(functionName, Severity.Warn, true, "evaluation failed.")
		end
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)
	elseif context:GetArgumentValue("Set pressure sensors offset") == true then
		local offsetA = context:GetArgumentValue("Pressure sensor offset A")
		local offsetB = context:GetArgumentValue("Pressure sensor offset B")
		if (math.abs(offsetA + settings.ExternalZeroPressureOffsetA) > 1000) or (math.abs(offsetB + settings.ExternalZeroPressureOffsetB) > 1000) then
			local minValue = pressSettings.SensorLimitLow
			local maxValue = pressSettings.SensorLimitHigh
			context:Report("Pressure sensor", Severity.Info, true, "This offset can lead to an over pressure. Please set a value in the range of (" ..minValue..", "..maxValue..")")
			context:Abort()
		end
		settings.ExternalZeroPressureOffsetA = offsetA
		settings.ExternalZeroPressureOffsetB = offsetB
		logParametersToTwinscape ()
		csv.logValueInCSVFile(context, csvFileName, "Calibration", "Pressure sensor offset A", context:GetArgumentValue("Pressure sensor offset A"), "", 2)
		csv.logValueInCSVFile(context, csvFileName, "Calibration", "Pressure sensor offset B", context:GetArgumentValue("Pressure sensor offset B"), "", 2)
		context:Sleep(500)
		pump:SetSettings(settings)
	elseif context:GetArgumentValue("Calibrate pressure sensors offset") == true then
		context:SetAbortEnabled(false)
		context:Sleep(2000)
		status:SetStatus("Pressure Sensor Calibration")
		context:Log(baltic.devider)
		context:Log("--- Pressure Sensor Offset Calibration:")

		local a, b, passed = pf.calibrate_press_sensors(installed, context, pump, zr, baltic.PumpValve.Waste)
		if not passed then
			context:SetAbortEnabled(true)
			context:Abort()
		end
		settings.ExternalZeroPressureOffsetA = a
		settings.ExternalZeroPressureOffsetB = b
		pump:SetSettings(settings)

		context:Log(baltic.devider)
		-- here just journal because in the twinscape file calibration values are always included
		status:RemoveStatus("Pressure Sensor Calibration")
		logParametersToTwinscape ()
		---@type IJournal
		local journal = context:GetProcedureParticipant(baltic.JournalRole)
		journal:Add(JournalEntry.Set(LogTo.Journal,context.Name, "--- Pressure Sensor Calibration", os.date("%c")))
		journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Pressure sensor offset A", a, "bar", "N1"))
		journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Pressure sensor offset B", b, "bar", "N1"))
		csv.logValueInCSVFile(context, csvFileName, "Pressure sensor calibration", "Pressure sensor offset A", a, "bar", 1)
		csv.logValueInCSVFile(context, csvFileName, "Pressure sensor calibration", "Pressure sensor offset B", b, "bar", 1)
		context:SetAbortEnabled(true)
	else
		endTime = 0
		local steps = method.TestTime / method.LogInterval

		endTime = calcEndTime()

		context:Log(baltic.devider)
		context:Log("--- Flow Sensor Calibration: previous values")
		context:Log("--- Flow Sensor A  Offset: {0}", pump:GetFlowCalibrationOffsetA())
		context:Log("---                Factor: {0}", pump:GetFlowCalibrationFactor(zr.A))
		context:Log("--- Flow Sensor B  Offset: {0}", pump:GetFlowCalibrationOffsetB())
		context:Log("---                Factor: {0}", pump:GetFlowCalibrationFactor(zr.B))
		context:Log(baltic.devider)

		context:Report("Calibrate flow sensors", Severity.Warn, true, "Perform only if solvent composition or a flow sensor was exchanged! Otherwise, please abort the procedure.")
		context:WaitForSignal("Calibrate flow sensors")

		-- channel A
		if not calFactorOffset_A then
			if setFactor_A then
				local fA = context:GetArgumentValue("Factor A value")
				if fA < 0.1 or fA > 10 then
					context:Report("Flow sensor A", Severity.Warn, true, "Factor A is out of range (0.1 - 10.0)! Click confirm to abort the procedure.")
					context:WaitForSignal("Flow sensor A")
					context:Abort()
				end
				if fA ~= nil then
					pf.SetFlowCalibrationFactor(zr.A, fA, pump, sleep_100)
					settings.FlowCalibrationFactorA = fA
					csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow Calibration Factor A", fA, "", 2)
				end
			end
		end
		context:Sleep(1000)
		-- channel B
		if not calFactorOffset_B then
			if setFactor_B then
				local fB = context:GetArgumentValue("Factor B value")
				if fB < 0.1 or fB > 10 then
					context:Report("Flow sensor B", Severity.Warn, true, "Factor B is out of range (0.1 - 10.0)! Click confirm to abort the procedure.")
					context:WaitForSignal("Flow sensor B")
					context:Abort()
				end
				if fB ~= nil then
					pf.SetFlowCalibrationFactor(zr.B, fB, pump, sleep_100)
					settings.FlowCalibrationFactorB = fB
					csv.logValueInCSVFile(context, csvFileName, "Calibration", "Flow Calibration Factor B", fB, "", 2)
				end
			end
		end

		if (steps > 4) then
			endTime = (pf.now()+calcEndTime()-procStartTime)/60
			local prepMsg = DotNetString.Format("Calibration, the procedure finishes at about {0:#0.0} minutes",endTime)
			status:SetStatus(prepMsg)
			local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
			-- bail if a pump failed degassing..
			if (not (a and b)) then
				pr.decompressSystem(context)
				context:Abort()
			end
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 60, sleep_100, baltic.smooth)
			local dictator4 = LoggingDictator.Prevent(pump)
			--leak_test_dual("Pumps isolated",steps)

			zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

			if calFlowSensors_New then
				-- Flow sensor calibration:
				-- 1. A+B set flow to calFlow (500nL/min)
				-- 2. A+B get pressure of pump
				-- 3. A+B get pump leakage at that pressure
				-- 4. A/B determine pump flow
				-- 5. A/B calc new factor: 
				--		factor = pump flow / calFlow * actualFactor
				---@type FlowAB
				local actFlowFactor = {A = pump:GetFlowCalibrationFactor(zr.A), B = pump:GetFlowCalibrationFactor(zr.B)}
				---@type FlowAB
				local actFlowOffset = {A = pump:GetFlowCalibrationOffsetA(), B = pump:GetFlowCalibrationOffsetB()}
				context:Log(baltic.devider)
				context:Log("--- Start flow sensor calibration ---")
				context:Log("--- actual factor A: {0}", actFlowFactor[1])
				context:Log("--- actual offset A: {0}", actFlowOffset[1])
				context:Log("--- actual factor B: {0}", actFlowFactor[2])
				context:Log("--- actual offset B: {0}", actFlowOffset[2])
				context:Log(baltic.devider)

				---@type FlowAB
				local newFlowFactor = {A = 1.0, B = 1.0}
				context:Log("--- setting flow start factor A: {0}, B: {1}", newFlowFactor[1], newFlowFactor[2])
				setFlowSensorFactors(newFlowFactor)
				local leackageA, leackageB = getLeakageAB(calFlow)
				pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 60, sleep_100, baltic.smooth)
				local dictator5 = LoggingDictator.Prevent(pump)
				local continue = true
				continue, newFlowFactor[1] = calFlowSensor(zr.A, calFlow, leackageA, actFlowFactor[1], newFlowFactor[1])
				if not continue then
					setFlowSensorFactors(actFlowFactor)			-- restore flow factor settings due to failed calibration
					context:Sleep(500)
					context:Abort()
				end
				context:Sleep(500)
				continue, newFlowFactor[2] = calFlowSensor(zr.B, calFlow, leackageB, actFlowFactor[2], newFlowFactor[2])
				if not continue then
					setFlowSensorFactors(actFlowFactor)			-- restore flow factor settings due to failed calibration
					context:Sleep(500)
					context:Abort()
				end
				context:Sleep(500)
				continue = false
				continue, endTime = setFlowFactors(actFlowFactor, newFlowFactor, endTime)
				if continue then
					calOffset_A = true
					calOffset_B = true
				end
				pr.Signalize_Reset(context)
				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste)
				context:Sleep(20000)
				pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Trap)
				context:Sleep(20000)
			else
				--Block Trap and Inject valve (if not done manually with blanks) - not for BackPressure - think about it!?
				--	execLeft:MoveValveDrive(valveI, pp.Quantity(baltic.InjectionValve.Inject-30, "deg"))
				--	execAux:MoveValveDrive(valveT, pp.Quantity(baltic.TrapValve.Trap, "deg")) -- Blocked from MixTee POW
				--	context:Sleep(1000)

				-- 	Prime flowsensor with appropriate solvents. OBH	
				-- parallelisieren?		
				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee,false)
				context:Sleep(500)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee,false)
				context:Sleep(500)
				pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Waste)
				build_pressure_AB(150, 130, sleep_100)
				pf.Manualmode_Pump_constantSpeed(zr.A, 1, pump, sleep_100)
				pf.Manualmode_Pump_constantSpeed(zr.B, 1, pump, sleep_100)
				context:Sleep(180000)
				pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
				pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

				-- paralellisieren?
				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste)
				context:Sleep(20000)
				pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Trap)
				context:Sleep(20000)

				-- always perform an Offset Calibration if any Calibration option for the respective channel is true
				if (calFactorOffset_A or setFactor_A) then
					calOffset_A = true	 -- always perform offset calibration
				end

				if (calFactorOffset_B or setFactor_B) then
					calOffset_B = true	 -- always perform offset calibration
				end
				context:Sleep(1000)

				status:RemoveStatus(prepMsg)
			end

			if calOffset_A or calOffset_B then
				calOffsets(endTime, steps)
			end

			if calFactorOffset_A then
				local actFlowFactor = pump:GetFlowCalibrationFactor(zr.A)
				local actFlowOffset = pump:GetFlowCalibrationOffsetA()
				context:Log(baltic.devider)
				context:Log("--- starting factor calibration channel A")
				context:Log("--- actual factor A: {0}", actFlowFactor)
				context:Log("--- actual offset A: {0}", actFlowOffset)
				context:Log(baltic.devider)

				local fcA_msg = DotNetString.Format("Factor calibration channel A, the procedure finishes at about {0:#0.0} minutes",endTime)
				status:SetStatus(fcA_msg)

				pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Waste)
				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee,false)
				context:Sleep(500)	
				build_pressure(zr.A, method.FlowCalBuildPres_A, false, sleep_100)-- expected back prssure

				context:Sleep(20000)
				pf.Manualmode_Pump_constantSpeed(zr.A, 0.5, pump, sleep_100)

				context:Sleep(method.FlowCalDuration*60*1000)

				local describtion = "FlowMonitorA to transfer capillary: 0.5 uL/min"
				describtion = "FlowMonitorA to Waste: 0.5 uL/min"
				monitor_flow(describtion,zr.A,0.5)

				pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)

				status:RemoveStatus(fcA_msg)
				context:Log(baltic.devider)
			end

			if calFactorOffset_B then
				local actFlowFactor = pump:GetFlowCalibrationFactor(zr.B)
				local actFlowOffset = pump:GetFlowCalibrationOffsetB()
				context:Log(baltic.devider)
				context:Log("--- starting factor calibration channel B")
				context:Log("--- actual factor B: {0}", actFlowFactor)
				context:Log("--- actual offset B: {0}", actFlowOffset)
				context:Log(baltic.devider)

				local fcB_msg = DotNetString.Format("Factor calibration channel B, the procedure finishes at about {0:#0.0} minutes",endTime)
				status:SetStatus(fcB_msg)

				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste,false)
				context:Sleep(500)
				pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Waste)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee,false)
				context:Sleep(500)	
				build_pressure(zr.B, method.FlowCalBuildPres_B, false, sleep_100)-- expected back prssure

				context:Sleep(20000)
				pf.Manualmode_Pump_constantSpeed(zr.B, 0.5, pump, sleep_100)

				context:Sleep(method.FlowCalDuration*60*1000)

				local describtion = "FlowMonitorB to transfer capillary: 0.5 uL/min"
				describtion = "FlowMonitorB to Waste: 0.5 uL/min"
				monitor_flow(describtion,zr.B,0.5)

				pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

				status:RemoveStatus(fcB_msg)
				context:Log(baltic.devider)
			end

			context:Log(baltic.devider)
			context:Log("--- Flow Sensor Calibration: values after calibration procedure")
			context:Log("--- Flow Sensor A  Offset: {0}", pump:GetFlowCalibrationOffsetA())
			context:Log("---                Factor: {0}", pump:GetFlowCalibrationFactor(zr.A))
			context:Log("--- Flow Sensor B  Offset: {0}", pump:GetFlowCalibrationOffsetB())
			context:Log("---                Factor: {0}", pump:GetFlowCalibrationFactor(zr.B))
			context:Log(baltic.devider)

			degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
	-- Do not abort here because the calibration ends anyway --
			-- bail if a pump failed degassing..
	--		if (not (a and b)) then
	--			context:Abort()
	--		end
			pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
		else
			context:Log("Insufficient number of steps")	
		end
	end
	pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Load)

	-- stop pumps before ending
	pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
	pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

	dictator:Dispose()

	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)

	pump:SetSettings(settings)

	logParametersToTwinscape()

	-- wait for pump to stop completely
	pf.isPumpIdle(pump, sleep_100)

	context:Report("Calibration", Severity.Info, true, "The calibration has been completed.")
end