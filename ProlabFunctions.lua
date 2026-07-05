local Date = "2025/04/09"

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---@param context InitHelper
function Initialize (context)
	context.Name = "Prolab Functions"
	context.Description = ""
	context.Hidden = false
	context.LedState = LedState.Diagnostics

	context:DeclareParameter("Flush A-to-Trap and MixT", true)
	context:DeclareParameter("Flow sensor zero calibration", false)
	context:DeclareParameter("Flow sensor factor calibration", false)
	context:DeclareParameter("Set defaults only", false)
end

---@param _ IInstalledHardwareContext
---@param __ IProcedureValidationContext
function Validate (_, __)

end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
  context:Log("Lua date: {0}", Date)

	local baltic = require "baltic"
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end

	---@type Zirconium
	local zr = require "zirconium"
	local pf = require "pump_functions"
	local pp = require "palplus"

	local parallel = require "parallel"

--	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	local function sleep_100()
		context:Sleep(100)
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

	pump:SetMaxPressureLimit(zr.A, pressSettings.GradientPumpCutoffPressure)
	pump:SetMaxPressureLimit(zr.B, pressSettings.GradientPumpCutoffPressure)

	local method = {}

	method.TestPressure = 20			-- bar
	method.BuildPressureSpeed = 100			-- uL/min
	method.TestTime = 4*60					-- seconds (per test)
	method.LogInterval = 30					-- seconds	
	method.LogSize = 40
	method.CalibrationDuration = 300

	--context:Report("Calibrate flow sensors", Severity.Dialog, "You should only proceed if the solvent composition or a flow sensor was changed. Otherwise, please abort the procedure.")
	--	context:WaitForSignal("Calibrate flow sensors")

	-- Theoretical pump flow sensor calibration values

	if (context:GetArgumentValue("Set defaults only")) then
		pump:SetFlowCalibrationFactorA(1)
		pump:SetFlowCalibrationFactorB(2.55)
		pump:SetFlowCalibrationOffsetA(0)
		pump:SetFlowCalibrationOffsetB(0)
		return
	end

	if (context:GetArgumentValue("Flow sensor zero calibration")) then
		pump:SetFlowCalibrationOffsetA(0)
		pump:SetFlowCalibrationOffsetB(0)
	end
	if (context:GetArgumentValue("Flow sensor factor calibration")) then
		pump:SetFlowCalibrationFactorA(1)
		pump:SetFlowCalibrationFactorB(1)
	end
	local function sleeping_yield()
		context:Sleep(100)
	end

  local function refill(channel, yield_function)
	context:Log("Refill")
	if zr.IsFull(pump, channel) then return end

	zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent)
	pf.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpRefillSpeed, pump, sleep_100)
	while not zr.IsFull(pump, channel) do yield_function() end
	parallel.sleep(yield_function, 2000)
 end
  local function push_insert(array,size,value)

		for i=1,size-1 do
			array[i] = array[i+1]
		end

		array[size] = value

	end

	local function log_array(array,size)

		for i=1,size do
			context:Log("ar[{0}] = {1}",i,array[i])
		end
	end
	local function average(array,size) --return average 
		local sum=0
		for i=1,size do
			sum = sum + array[i]
		end
		return sum/size
	end
	local function stdev(array,size) --standard deviation
		local sum=0
		local avg=average(array,size)

		for i=1,size do
			local dev=avg-array[i]
			sum = sum + dev*dev
		end

		return math.sqrt(sum/size)

	end

	local function avg_delta(array,size) --return average change between array values
		local sum=0
		for i=1,size-1 do
			sum = sum + array[i+1]-array[i]
		end
		return sum/(size-1)
	end

	local function monitor_flow(description,channel,pistonspeed)
		context:Log("")
		context:Log("###############################################")
		context:Log("START: {0}",description)
		context:Log("###############################################")
		local flowlog = {}
		for i=1,method.LogSize do
			flowlog[i]=0
		end

		local n = method.CalibrationDuration
		for i = 1,n do
			local flow = pump:GetCurrentFlow(channel)
			push_insert(flowlog, method.LogSize, flow)
			context:Sleep(1000)
		end
		context:Log("###############################################")
		context:Log("END: {0}",description)
		context:Log("-----------------------------------------------")
		context:Log("FlowLog:")
		log_array(flowlog,method.LogSize)

		local avgd=avg_delta(flowlog,method.LogSize)
		local avg=average(flowlog,method.LogSize)
		local sd=stdev(flowlog,method.LogSize)
		context:Log("Average change in last logged flowrates: {0}",avgd)
		context:Log("Average value of last logged flowrates: {0}",avg)
		context:Log("Standard deviation of last logged flowrates: {0}",sd)
		if (pistonspeed > 0) then
			if (math.abs(avgd) < 0.001 and sd < 0.005) then 
				context:Log("Conclusion: use for flow sensor calibration: {0} vs {1}",pistonspeed,avg)
				--write calibration point to pump if accepted. OBH
				zr.SetFlowCalibrationFactor(pump, channel, 0.5/avg)

				--offset is factor dependent, recalculate
				zr.SetFlowCalibrationOffset(pump, channel, zr.GetFlowCalibrationOffset(pump, channel)*0.5/avg )

			else
				context:Log("Conclusion: not stable, repeat")
			end
		end
		context:Log("")
		context:Log("###############################################")

	end

	local function build_pressure(channel, pressure, dummyFunction, yield_function)
		if dummyFunction then return end
		if zr.IsEmpty(pump, channel) then return end
		context:Log("build_pressure {0}", channel)

		pf.Manualmode_Pump_constantSpeed(channel, method.BuildPressureSpeed, pump, sleep_100)
		while not zr.IsEmpty(pump, channel) do 
			if (pump:GetCurrentPressure(channel) > pressure*.9) then 
				pf.Manualmode_Pump_constantSpeed(channel, method.BuildPressureSpeed/5, pump, sleep_100)
			end
			if (pump:GetCurrentPressure(channel) > pressure*.99) then 
				pf.Manualmode_Pump_constantSpeed(channel, 0, pump, sleep_100)
				pump:Manualmode_Pump_constantPressure(channel,pressure)
				break
			end

			yield_function()
		end
		local pistonPosition = pump:GetPistonPosition(channel)
		return pistonPosition
	end

	local function fscal_composite(description,steps)
			local logarA = {}
			local logarB = {}

			local logFlowA = {}
			local logFlowB = {}

		--	zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Compress)
	--		zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Compress)		
			context:Sleep(1000)


			for i = 1,steps do
				context:Sleep(method.LogInterval*1000) 
				logarA[i] = pump:GetPistonPosition(zr.A)
				logarB[i] = pump:GetPistonPosition(zr.B)
				logFlowA[i] = pump:GetCurrentFlow(zr.A)
				logFlowB[i] = pump:GetCurrentFlow(zr.B)
			end
			local endPressureA=pump:GetCurrentPressure(zr.A)
			local endPressureB=pump:GetCurrentPressure(zr.B)
			--pump:Manualmode_Pump_constantSpeed(zr.A, 0)
			--pump:Manualmode_Pump_constantSpeed(zr.B, 0)
			--output diagnostics at the end

			context:Log("")
			context:Log("###############################################")
			context:Log("Description: {0}",description)
			context:Log("-----------------------------------------------")
			context:Log("EndPressureA : {0}", endPressureA)	
			context:Log("EndPressureB : {0}", endPressureB)
			context:Log("")
			local n=0
			local sum=0
			local lastVol=0

			for i = 1,steps do 
				context:Log("PumpPosA{0} : {1} uL, delta: {2} nL, flowSensorValue: {3} nL/min", i, logarA[i],1000*(logarA[i]-lastVol),1000*logFlowA[i])
				if (i>steps/2) then -- average based on the second half of logged piston positions
					sum = sum+(logarA[i]-lastVol)  -- += deltaVolume
					lastVol=logarA[i]
					n = n+1 
				end
				lastVol=logarA[i]
			end
			local leakA = 1000*60*sum/n/method.LogInterval
			context:Log("Pump A average flow: [nL/min]: {0} ",leakA)

			context:Log("")
			n=0
			sum=0
			lastVol=0
			local sumB = 0
			local sumA = 0
			for i = 1,steps do 
				context:Log("PumpPosB{0} : {1} uL, delta: {2} nL, flowSensorValue: {3} nL/min", i, logarB[i],1000*(logarB[i]-lastVol),1000*logFlowB[i])	
				if (i>steps/2) then 
					sum = sum+(logarB[i]-lastVol)
					lastVol=logarB[i]
					sumB = sumB + logFlowB[i]
					sumA = sumA + logFlowA[i]
					n = n+1
				end	
				lastVol=logarB[i]
			end
			local leakB = 1000*60*sum/n/method.LogInterval
			context:Log("Pump B average flow: [nL/min]:{0} ",leakB)
			context:Log("FSA average flow: [nL/min]:{0} ",(1000*sumA/n))
			context:Log("FSB average flow: [nL/min]:{0} ",(1000*sumB/n))
			context:Log("###############################################")

			-- Set FSA and FSB zero points. OBH
			pump:SetFlowCalibrationOffsetA(-sumA/n)
			pump:SetFlowCalibrationOffsetB(-sumB/n)
	end
	context:Log("BackPressure")

	local steps = method.TestTime / method.LogInterval
	if (steps > 4) then 	
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
		context:Sleep(10*1000)
		local p_refill_a = { refill, zr.A, parallel.yield }
		local p_refill_b = { refill, zr.B, parallel.yield }
		parallel.run(sleeping_yield, p_refill_a, p_refill_b)
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)

		--leak_test_dual("Pumps isolated",steps)

		if (context:GetArgumentValue("Flow sensor zero calibration")) then
			context:Report("Calibrate", Severity.Dialog, true, "Please disconnect the separation column and leave the transferline unblocked, then click 'Confirm' to continue. Two minutes to next action.")
			context:WaitForSignal("Calibrate")
		end

		--Block Trap and Inject valve (if not done manually with blanks) - not not for BackPressure - think about it!?
	--	execLeft:MoveValveDrive(valveI, pp.Quantity(baltic.InjectionValve.Inject-30, "deg"))
	--	execAux:MoveValveDrive(valveT, pp.Quantity(baltic.TrapValve.Trap, "deg")) -- Blocked from MixTee POW
	--	context:Sleep(1000)

	-- 	Prime flow sensor with appropriate solvents. OBH	
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee,false)
		context:Sleep(500)
		zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee,false)
		context:Sleep(500)
		execAux:MoveValveDrive(valveT, pp.Quantity(baltic.TrapValve.Waste, "deg"))
		build_pressure(zr.A, 25, false, sleeping_yield)
		build_pressure(zr.B, 25, false, sleeping_yield)
		pf.Manualmode_Pump_constantSpeed(zr.A, 1, pump, sleep_100)
		pf.Manualmode_Pump_constantSpeed(zr.B, 1, pump, sleep_100)
		context:Sleep(60000)
		if (context:GetArgumentValue("Flush A-to-Trap and MixT")) then
			pump:Manualmode_Pump_constantSpeed(zr.B, 0)
			zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste,false)
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject,true)
			execAux:MoveValveDrive(valveT, pp.Quantity(baltic.TrapValve.InjectWaste, "deg"))
			context:Sleep(60000)
		end
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
		pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste)
		context:Sleep(500)
		zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste)
		context:Sleep(500)

		context:Sleep(20000)

		execAux:MoveValveDrive(valveT, pp.Quantity(baltic.TrapValve.Trap, "deg")) -- MixTee Blocked
		context:Sleep(20000)

		if (context:GetArgumentValue("Flow sensor zero calibration")) then
			context:Report("Calibrate2", Severity.Dialog, true, "Please plug the open end of the transfer line, then click 'Confirm' to continue.")
			context:WaitForSignal("Calibrate2")


			fscal_composite("Test1: ZeroPoints. No pressure, flow sensors isolated",5)
			if (context:GetArgumentValue("Flow sensor factor calibration")) then
				context:Report("Calibrate3", Severity.Dialog, true, "Please remove the plug from the transfer line, then click 'Confirm' to continue.")
				context:WaitForSignal("Calibrate3")
				execAux:MoveValveDrive(valveT, pp.Quantity(baltic.TrapValve.Waste, "deg"))
				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee,false)
				build_pressure(zr.A, 25, false, sleeping_yield)	

				pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
				context:Sleep(20000)
				pf.Manualmode_Pump_constantSpeed(zr.A, 0.5, pump, sleep_100)

				context:Sleep(20*60*1000)

				monitor_flow("FlowMonitorA to Waste: 0.5 uL/min",zr.A,0.5)

				pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)

				zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Waste,false)
				context:Sleep(500)
				zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee,false)
				context:Sleep(500)
				build_pressure(zr.B, 25, false, sleeping_yield)
				pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
				context:Sleep(20000)
				pf.Manualmode_Pump_constantSpeed(zr.B, 0.5, pump, sleep_100)

				context:Sleep(20*60*1000)

				monitor_flow("FlowMonitorB to Waste: 0.5 uL/min",zr.B,0.5)
			end
		end

		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
		context:Sleep(10*1000)
		p_refill_a = { refill, zr.A, parallel.yield }
		p_refill_b = { refill, zr.B, parallel.yield }
		parallel.run(sleeping_yield, p_refill_a, p_refill_b)
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
	else
		context:Log("Insufficient number of steps")
	end

	-- stop pumps before ending
	pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
	pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

	dictator:Dispose()

	-- wait for pump to stop completely
	while not pump.IsIdle do
		sleeping_yield()
	end

end
