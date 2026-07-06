-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/11/12"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")

local P = {}

local timer = 0						-- seconds for function 'isFlowInRange()'
local ringbuffer = require "ringbuffer"
local buffer = ringbuffer()

function P.now()	-- [s]
    return os.clock()
end

---Check if the pump is idle
---@param pump Pump
---@param yield_func function
---@return boolean
function P.isPumpIdle(pump, yield_func)
	local ok = true
	local timeOut = P.now() + 60
	while pump.IsIdle and timeOut < P.now() do
		yield_func()
	end
	if timeOut >= P.now() then ok = false end
	return ok
end

---This function prevents exponents like 1E-17 for zero values
---@param val number
---@param digits integer|nil
---@return number
function P.noExp(val, digits)
	if digits == nil then digits = 3 end
	local shift = 10 ^ digits
	local value = math.floor( val*shift ) / shift
	return value
end

---This function checks if the actVal differs less than +-1% to value
---@param value number
---@param actVal number
---@return boolean
function P.InRange(value, actVal)
	local diff = math.max(value*0.01, 0.01)
	local isInRange = math.abs(value-actVal)<=math.abs(diff)
	return isInRange
end

---Store the flow sensor offset
---@param channel Channel
---@param setPt number
---@param pump Pump
---@param yield_func function
function P.SetFlowCalibrationOffset(channel, setPt, pump, yield_func)
	local zr = require "zirconium"
	zr.SetFlowCalibrationOffset(pump, channel, setPt)
	-- check if the set point is set in the ZrHAL and retry if different
	local retryCnt = 0
	while not P.InRange(setPt, zr.GetFlowCalibrationOffset(pump, channel)) and retryCnt < 10 do
		yield_func()
		zr.SetFlowCalibrationOffset(pump, channel, setPt)
		retryCnt = retryCnt + 1
	end
end

---Store the flow sensor factor
---@param channel Channel
---@param setPt number
---@param pump Pump
---@param yield_func function
function P.SetFlowCalibrationFactor(channel, setPt, pump, yield_func)
	local zr = require "zirconium"
	zr.SetFlowCalibrationFactor(pump, channel, setPt)
	-- check if the set point is set in the ZrHAL and retry if different
	local retryCnt = 0
	while not P.InRange(setPt, pump:GetFlowCalibrationFactor(channel)) and retryCnt < 10 do
		yield_func()
		zr.SetFlowCalibrationFactor(pump, channel, setPt)
		retryCnt = retryCnt + 1
	end
end

----------------------------------------------
---Operate the pump module in constant binary flow mode
---@param context IProcedureExecutionContext
---@param setPtA number
---@param setPtB number
---@param pump Pump
---@param zr Zirconium
---@param yield_func function
function P.Manualmode_Pump_constantFlow_binary(context, setPtA, setPtB, pump, zr, yield_func)
	local function inRangeBinary()
		local inRangeA = P.InRange(setPtA, pump:GetSetFlow(zr.A))
		local inRangeB = P.InRange(setPtB, pump:GetSetFlow(zr.B))
		return (inRangeA and inRangeB)
	end
	context:Log("--- Set constant binary flow ---")
	pump:Manualmode_Pump_constantFlow_binary(P.noExp(setPtA, 2),P.noExp(setPtB, 2))
	-- check if the set point is set in the ZrHAL and retry if different
	local retryCnt = 0
	while not inRangeBinary() and retryCnt < 10 do
		yield_func()
		context:Log("--- Set constant binary flow {0} ---", retryCnt+1)
		pump:Manualmode_Pump_constantFlow_binary(P.noExp(setPtA),P.noExp(setPtB))
		retryCnt = retryCnt + 1
	end
end

---Operate the pump in constant flow mode
---@param channel Channel
---@param setPt number
---@param pump Pump
---@param yield_func function
function P.Manualmode_Pump_constantFlow(channel, setPt, pump, yield_func)
	pump:Manualmode_Pump_constantFlow(channel, P.noExp(setPt, 2))
	-- check if the set point is set in the ZrHAL and retry if different
	local retryCnt = 0
	while not P.InRange(setPt, pump:GetSetFlow(channel)) and retryCnt < 10 do
		yield_func()
		pump:Manualmode_Pump_constantFlow(channel, P.noExp(setPt, 2))
		retryCnt = retryCnt + 1
	end
end

---Operate the pump in constant speed mode or stop the pump
---@param channel Channel
---@param setPt number
---@param pump Pump
---@param yield_func function
function P.Manualmode_Pump_constantSpeed(channel, setPt, pump, yield_func)
	-- PNS-417: Reverted to use constant speed to stop the pump because
	-- stop signal causes issues that leave the pistons unresponsive for too long
	--if (setPt == 0) then
	--	---@type Zirconium
	--	local zr =     require "zirconium"
	--	pump:SetPumpSideSignal(channel, zr.PumpsideSignal.Stop)
	--else
		pump:Manualmode_Pump_constantSpeed(channel, P.noExp(setPt))
		-- check if the set point is set in the ZrHAL and retry if different
		local retryCnt = 0
		while not P.InRange(setPt, pump:GetSetPistonSpeed(channel)) and retryCnt < 10 do
			yield_func()
			pump:Manualmode_Pump_constantSpeed(channel, P.noExp(setPt))
			retryCnt = retryCnt + 1
		end
	--end
end

---Operate the pump in constant pressure mode
---@param channel Channel
---@param setPt number
---@param pump Pump
---@param yield_func function
function P.Manualmode_Pump_constantPressure(channel, setPt, pump, yield_func)
	pump:Manualmode_Pump_constantPressure(channel, P.noExp(setPt, 2))
	-- check if the set point is set in the ZrHAL and retry if different
	local retryCnt = 0
	while not P.InRange(setPt, pump:GetTargetPressure(channel)) and retryCnt < 10 do
		yield_func()
		pump:Manualmode_Pump_constantPressure(channel, P.noExp(setPt, 2))
		retryCnt = retryCnt + 1
	end
end

----------------------------------------------
function P.getRingBuffer()
	return ringbuffer()
end

function P.printBuffer(context)
	local bS = buffer:size()
	for i=1, bS do
		local item = buffer:getItemAtPosition(i)
		context:Log("Buffer[{0}]: fA = {1}, fB = {2}, dA = {3}, dB = {4}, setFA = {5}, setFB = {6}", i, item.fA, item.fB, item.dA, item.dB, item.sFA, item.sFB)
	end
end

---Monitor the deviation of the flow
---@param setFlow FlowAB
---@param actFlow FlowAB
---@param intendedBufferSize number
---@return number
function P.monitor_deviation(setFlow, actFlow, intendedBufferSize)
	-- fill buffer with 'flow' item
	if setFlow.A == 0 then		-- prevent devision by zero
		if actFlow.A == 0 then
			setFlow.A = 0.0001
			actFlow.A = 0.0001
		else
			setFlow.A = actFlow.A
		end
	end
	if setFlow.B == 0 then		-- prevent devision by zero
		if actFlow.B == 0 then
			setFlow.B = 0.0001
			actFlow.B = 0.0001
		else
			setFlow.B = actFlow.B
		end
	end
	local item = {
		fA = actFlow.A,
		fB = actFlow.B,
		dA = (100/setFlow.A*actFlow.A)-100,
		dB = (100/setFlow.B*actFlow.B)-100,
		sFA = setFlow.A,
		sFB = setFlow.B
	}
	if math.abs(setFlow.A-actFlow.A) < 0.01 then item.dA = 0 end	-- ignore to small differences like noise
	if math.abs(setFlow.B-actFlow.B) < 0.01 then item.dB = 0 end	-- ignore to small differences like noise

	if buffer:size() >= intendedBufferSize then
		-- delete first entry
		buffer:removeAt(0)
	end
	-- update and append reading
	buffer:append(item)
	return buffer:getAvg()
end

---Check if the pressure is near the maximum or higher
---@param context IProcedureExecutionContext
---@param pumpCutoffPressure integer [bar]
---@param mP number [bar]
---@param pump Pump
---@param zr Zirconium
---@param code number
---@return number
function P.isPressureInRange(context, pumpCutoffPressure, mP, pump, zr, code)
	local maxPumpPressure = pumpCutoffPressure
	local maxP = math.min(mP, maxPumpPressure)
	local p = math.max(pump:GetCurrentPressure(zr.A), pump:GetCurrentPressure(zr.B))
	if (((p-maxP) > 1)) then		-- pressure higher max pressure
		code = 4											-- "error pressure" 
		context:Log("Pressure exceeded maximum ({0} bar)", maxP)
	else if (((maxP*0.95-p) < 0) and (code == 0)) then		-- pressure near max pressure
		code = 2											-- "error pressure" 
		context:Log("Pressure {0} bar is approaching maximum ({1} bar)", P.noExp(p,4), maxP)
		end
	end
	return code
end

function P.setTimer(seconds)
	timer = P.now() + seconds
end

---Initialize the timer for the flow observer
---@param seconds number
function P.iniFlowObserver(seconds)
	buffer:reset()
	P.setTimer(seconds)
end

---Check if the flow deviation is not too high
---@param context IProcedureExecutionContext
---@param percent number
---@param pump Pump
---@param zr Zirconium
---@param mode string
---@param code number
---@param intendedBufferSize number
---@return number
function P.isFlowInRange(context, percent, pump, zr, mode, code, intendedBufferSize)
	if P.now()>timer then
		local logFlow = false
		local setF = {A=pump:GetSetFlow(zr.A), B=pump:GetSetFlow(zr.B)}
		local actF = {A=P.noExp(pump:GetCurrentFlow(zr.A),4), B=P.noExp(pump:GetCurrentFlow(zr.B),4)}
		if intendedBufferSize == nil then intendedBufferSize = 12 end			-- if not specified use 6 values
		local avgDevA, avgDevB, avgFlowA, avgFlowB = P.monitor_deviation(setF, actF, intendedBufferSize)
		-- is deviation > percent or > 20nL and buffer is fully filled (over 2 minutes)
		local allowedFlowDeviationA = math.max(percent, P.noExp(100/setF.A*0.02,1))
		local allowedFlowDeviationB = math.max(percent, P.noExp(100/setF.B*0.02,1))
		if logFlow then
			context:Log("+++ act. flow A: {0}; avg. flow A: {1}; set flow A: {2}; avg deviation A {3}; dev allowed {4}; buffer size: {5} +++", actF.A, P.noExp(avgFlowA,4), P.noExp(setF.A,4), P.noExp(avgDevA,4), allowedFlowDeviationA, buffer:size())
			context:Log("+++ act. flow B: {0}; avg. flow B: {1}; set flow B: {2}; avg deviation B {3}; dev allowed {4}; buffer size: {5} +++", actF.B, P.noExp(avgFlowB,4), P.noExp(setF.B,4), P.noExp(avgDevB,4), allowedFlowDeviationB, buffer:size())
		end
		if ((math.abs(avgDevA) > allowedFlowDeviationA) or (math.abs(avgDevB) > allowedFlowDeviationB)) and (buffer:size() >= intendedBufferSize) then
			-- flow is not intended flow
--			code = 1										-- "warning flow" 
			if (math.abs(avgDevA) > allowedFlowDeviationA) then
				P.printBuffer(context)
				context:Log("+++ act. flow A: {0}; avg. flow A: {1}; set flow A: {2}; avg deviation A {3}; dev allowed {4}; buffer size: {5} +++", actF.A, P.noExp(avgFlowA,4), P.noExp(setF.A,4), P.noExp(avgDevA,4), allowedFlowDeviationA, buffer:size())
				local diffFlowMsg = "Flow deviation on Pump A: measured "..P.noExp(avgDevA,4).."% (allowed: "..P.noExp(allowedFlowDeviationA,4).."%)\n\nThe actual flow is drifting from the set flow. Check for air bubbles, leaks at fittings, or a partially blocked column."
				if code == 2 then
					diffFlowMsg = "Flow on Pump A is unstable because the system pressure has reached its limit.\n\nCheck for a blocked or restricted column, clogged frit, or incorrect pressure settings."
				else
					code = 1										-- "warning flow" 
				end
				context:Report(mode, Severity.Info, true, diffFlowMsg)
			end
			if (math.abs(avgDevB) > allowedFlowDeviationB) then
				P.printBuffer(context)
				context:Log("+++ act. flow B: {0}; avg. flow B: {1}; set flow B: {2}; avg deviation B {3}; dev allowed {4}; buffer size: {5} +++", actF.B, P.noExp(avgFlowB,4), P.noExp(setF.B,4), P.noExp(avgDevB,4), allowedFlowDeviationB, buffer:size())
				local diffFlowMsg = "Flow deviation on Pump B: measured "..P.noExp(avgDevB,4).."% (allowed: "..P.noExp(allowedFlowDeviationB,4).."%)\n\nThe actual flow is drifting from the set flow. Check for air bubbles, leaks at fittings, or a partially blocked column."
				if code == 2 then
					diffFlowMsg = "Flow on Pump B is unstable because the system pressure has reached its limit.\n\nCheck for a blocked or restricted column, clogged frit, or incorrect pressure settings."
				else
					code = 1										-- "warning flow" 
				end
				context:Report(mode, Severity.Info, true, diffFlowMsg)
			end
		end
		timer = P.now()+10			-- get next value in 10 seconds
	end
	return code
end

---Checking if the gradient is OK (flow and pressure)
---@param context IProcedureExecutionContext
---@param pumpCutoffPressure integer [bar]
---@param percent number
---@param mP number
---@param pump Pump
---@param zr Zirconium
---@param code number
---@param mode string
---@return number
function P.isGradientOK(context, pumpCutoffPressure, percent, mP, pump, zr, code, mode)
	code = P.isPressureInRange(context, pumpCutoffPressure, mP, pump, zr, code)
	if (code == 4) then				-- code == 4 == error
		-- The gradient is stopped but the runtime in HyStar continues
		-- After the runtime is left the state stays at Acquisition and no new run is started
		local msg = "Pressure exceeded "..pumpCutoffPressure.." bar (the maximum allowed pressure).\n\nLikely causes:\n- Blocked or clogged separation column\n- Blocked frit or inline filter\n- Kinked or crushed capillary\n- Incorrect column selected in the method editor\n\nThe gradient has been stopped. Check all connections and the column before restarting."
		context:Report(mode, Severity.Error, true, msg)
		zr.abortGradient(pump)
	end
	if (code < 3) then
		code = P.isFlowInRange(context, percent, pump, zr, mode, code, 12)
	end
	return code
end

local preCode = 0
---Report a gradient message
---@param context IProcedureExecutionContext
---@param code number
---@param mode string
function P.showGradientMassage(context, code, mode)
	if (preCode ~= code) then			-- show msg only if code has changed
		preCode = code
		if (code == 1) then
			-- The gradient continues, the acquisition is not stopped but no new run starts
			context:Report(mode, Severity.Error, true, "Unstable flow detected during the gradient.\n\nLikely causes:\n- Air bubble trapped in the pump or capillary\n- Loose fitting or leaking connection\n- Column partially blocked\n- Insufficient solvent in the reservoir\n\nThe run will continue but results may be affected. Check all connections and solvent levels.")
		else if (code == 2) then
			-- The gradient continues, the acquisition is not stopped, following acquisitions continues and the warning disappears
			context:Report(mode, Severity.Info, true, "Pressure is approaching the maximum limit (within 5%).\n\nThis is a warning — the system is still running but is near the safe pressure boundary.\n\nConsider:\n- Reducing the flow rate\n- Checking for a partially blocked column or frit\n- Verifying the correct column is selected in the method")
			end
		end
	end
end

----------------------------------------------
---Return the trap column pressure
---@param trap IChromatographyColumnType?
---@param pumpMaxPressure integer [bar]
---@param pressTarget number [bar]
---@return number
function P.getTrapColumnPressure(trap, pumpMaxPressure, pressTarget)
	local pressure = pumpMaxPressure
	if pressTarget ~= nil then pressure = pressTarget end
	if trap ~= nil then
		if trap.IsMaximumPressure then pressure = math.min(pressure, trap.MaximumPressure) end
	end
	return pressure
end

---return the trap column flow
---@param trap IChromatographyColumnType?
---@param flowTarget number
---@return number
function P.getTrapColumnFlow(trap, flowTarget)
	if trap ~= nil then
		if trap.IsMaximumFlow then flowTarget = math.min(flowTarget, trap.MaximumFlow) end
	end
	return flowTarget
end

---Return the separator column flow
---@param trap IChromatographyColumnType?
---@param sep IChromatographyColumnType
---@param pumpMaxPressure integer [bar]
---@param flowTarget number
---@param composition number
---@param ovenTemp number
---@return number
function P.getSepColumnFlow(trap, sep, pumpMaxPressure, flowTarget, composition, ovenTemp)
	local chrom =  require "chromatography"
	local viscoMix = chrom.viscosity_H2O_20C
	if composition ~= nil and ovenTemp ~= nil then viscoMix = chrom.viscosity_mix(ovenTemp, composition) end
	local colUnityFlow = chrom.column_flow(sep, pumpMaxPressure, 1, viscoMix)
	if sep.IsAdvancedSettings then colUnityFlow = sep.UnityFlow end				-- use advanced parameter no matter what the oven temperature and composition is
	local pressure = flowTarget/colUnityFlow
	if pressure > pumpMaxPressure then
		pressure = pumpMaxPressure
		flowTarget = math.min(flowTarget, pressure*colUnityFlow)
	end
	if sep.IsMaximumPressure then pressure = math.min(pressure, sep.MaximumPressure) end
	if trap ~= nil then
		local trapPress = P.getTrapColumnPressure(trap, pumpMaxPressure, pressure)
		pressure = math.min(pressure, trapPress)
	end
	flowTarget = math.min(flowTarget, pressure*colUnityFlow)
	if sep.IsMaximumFlow then flowTarget = math.min(flowTarget, sep.MaximumFlow) end
	return flowTarget
end

---Aktivate the maximum pressure limit for the pump
---@param channel Channel
---@param setPt number
---@param pump Pump
---@param yield_func function
function P.SetMaxPressureLimit(channel, setPt, pump, yield_func)
	pump:SetMaxPressureLimit(channel, setPt)
	-- check if the set point is set in the ZrHAL and retry if different
	local retryCnt = 0
	while not P.InRange(setPt, pump:GetMaxPressureLimit(channel)) and retryCnt < 10 do
		yield_func()
		pump:SetMaxPressureLimit(channel, setPt)
		retryCnt = retryCnt + 1
	end
end

---Return the maximum column pressure
---@param column IChromatographyColumnType?
---@param pumpMaxPressure integer
---@return number
function P.getColumnMaxPressure(column, pumpMaxPressure)
    local maxPress = pumpMaxPressure
	if column ~= nil then
		if column.IsMaximumPressure and (column.MaximumPressure > 1) then
			maxPress = math.min(column.MaximumPressure, maxPress)
		end
	end
    return maxPress
end

---Return the lowest maximum column pressure
---@param trp IChromatographyColumnType?
---@param sep IChromatographyColumnType
---@param pumpMaxPressure integer [bar]
---@return number
function P.getMaxColumnPressure(trp, sep, pumpMaxPressure)
	local maxPressSep = P.getColumnMaxPressure(sep, pumpMaxPressure) -- it is checked for 'nil' inside
	local maxPressTrp = P.getColumnMaxPressure(trp, pumpMaxPressure) -- it is checked for 'nil' inside
    return math.min(maxPressSep, maxPressTrp)
end

---Log the pressure sensor offset, factors and the maximum set pressure
---@param ps any
---@param context IProcedureExecutionContext
---@param pump Pump
---@param zr Zirconium
function P.logExternalOffsetFactorAndMaxPressure(ps, context, pump, zr)
	local pA = pump:GetMaxPressureLimit(zr.A)
	local pB = pump:GetMaxPressureLimit(zr.B)
	context:Log("--- max pressure: --------------------------------------")
	context:Log("--- max pressure A: {0}; max pressure B: {1}", P.noExp(pA, 4), P.noExp(pB, 4))
	context:Log("--- offset A/B: ----------------------------------------")
	context:Log("--- ExternalZeroPressureOffsetA: {0}; ExternalZeroPressureOffsetB: {1}", P.noExp(ps.ExternalZeroPressureOffsetA, 5), P.noExp(ps.ExternalZeroPressureOffsetB, 5))
	context:Log("--- factor A/B: ----------------------------------------")
	context:Log("--- ExternalPressureScalingFactorA: {0}; ExternalPressureScalingFactorB: {1}", P.noExp(ps.ExternalPressureScalingFactorA, 5), P.noExp(ps.ExternalPressureScalingFactorB, 5))
	context:Log("--------------------------------------------------------")
end

---Return the pump maximum and cutoff pressure.
---Cutoff pressure for pressure sensor 1600 bar = 1250 bar
---Cutoff pressure for pressure sensor 1000 bar = 1020 bar
---@param maxSensorPressure number
---@return PressureSensor
function P.getPressureSensorSettings(maxSensorPressure)
	local baltic = require "baltic"
	if (maxSensorPressure > 1250) then return baltic.Settings_Sensor_1600 end
	return baltic.Settings_Sensor_1000
end

---Returns the max pressure with an added delta depending on the pump max pressure
---@param pumpMaxPressure number
---@param maxPressure number
---@return number
function P.GetMaxPressureLimitWithDelta(pumpMaxPressure, maxPressure)
	local deltaPressure = 50
	if pumpMaxPressure < 1100 then deltaPressure = 20 end
	local p = maxPressure + deltaPressure
	return p
end

---Activate the maximum pressure for both pumps
---@param context IProcedureExecutionContext
---@param pump Pump
---@param pumpMaxPressure integer [bar]
---@param isTrapIn boolean
---@param isSepIn boolean
---@return number
function P.setMaxPressureLimitsAB(context, pump, pumpMaxPressure, isTrapIn, isSepIn)
	local function sleep_100()
		context:Sleep(100)
	end
	---@type Zirconium
	local zr =  require "zirconium"
	local sep = nil
	local trp = nil
	if isTrapIn then
		---@type IChromatographyColumnType
		trp = context:GetArgumentValue("trap")
		context:Log("--- Trap: {0} ---", trp)
	end
	if isSepIn then
		---@type IChromatographyColumnType
		sep = context:GetArgumentValue("separator")
		context:Log("--- Separator: {0} ---", sep)
	end
    local p = pumpMaxPressure
	if sep ~= nil then p = P.getMaxColumnPressure(trp, sep, pumpMaxPressure) end
	context:Log("--- MaxPressureLimitAB: {0} bar ---", p)
    P.reducePressure(context, pump, zr, zr.A, zr.B, p, p, 300, sleep_100, false)
	local pA = pump:GetCurrentPressure(zr.A)
	local pB = pump:GetCurrentPressure(zr.B)
	if (pA <= (p+5)) or (pB <= (p+5)) then
		local limit = P.GetMaxPressureLimitWithDelta(pumpMaxPressure, p)
		P.SetMaxPressureLimit(zr.A, limit, pump, sleep_100)
		P.SetMaxPressureLimit(zr.B, limit, pump, sleep_100)
		local settings = pump:GetSettings()
		P.logExternalOffsetFactorAndMaxPressure(settings, context, pump, zr)
	else
		context:Log("--- MAX PRESSURE {0} has not been set, act pressure is A: {1}, B: {2}", p, pA, pB)
	end
	return p
end

---Activate the maximum pressure for pump A and return the set pressure
---@param context IProcedureExecutionContext
---@param pump Pump
---@param pumpMaxPressure integer [bar]
---@param isTrapIn boolean
---@param isSepIn boolean
---@param pressTarget number|nil
---@return number
function P.setMaxPressureLimitA(context, pump, pumpMaxPressure, isTrapIn, isSepIn, pressTarget)
	local function sleep_100()
		context:Sleep(100)
	end
	---@type Zirconium
	local zr =     require "zirconium"
	local trp =    nil
	local sep =    nil
    if isTrapIn then
		---@type IChromatographyColumnType
		trp = context:GetArgumentValue("trap")
	end

    if isSepIn then
		---@type IChromatographyColumnType
		sep = context:GetArgumentValue("separator")
	end
	local pressureLimit = pumpMaxPressure
	if sep ~= nil then pressureLimit = P.getMaxColumnPressure(trp, sep, pumpMaxPressure) end

	if pressTarget == nil or pressTarget > pressureLimit then
		pressTarget = pressureLimit
	end

	context:Log("--- MaxPressureLimitA: {0} bar ---", pressureLimit)
    P.reducePressure(context, pump, zr, zr.A, nil, pressTarget, pressTarget, 300, sleep_100, false)
	local pA = pump:GetCurrentPressure(zr.A)
	if (pA <= (pressureLimit+5)) then
		local pressureLimitWithDelta = P.GetMaxPressureLimitWithDelta(pumpMaxPressure, pressureLimit)
		P.SetMaxPressureLimit(zr.A, pressureLimitWithDelta, pump, sleep_100)
		context:Sleep(100)
		local settings = pump:GetSettings()
		P.logExternalOffsetFactorAndMaxPressure(settings, context, pump, zr)
	else
		context:Log("--- MAX PRESSURE {0} has not been set, act pressure is A: {1}", pressureLimit, pA)
	end
	return pressureLimit
end

---Activate the maximum pressure for pump B and return the set pressure
---@param context IProcedureExecutionContext
---@param pump Pump
---@param pumpMaxPressure integer [bar]
---@param isTrapIn boolean
---@param isSepIn boolean
---@param pressTarget number|nil
---@return number
function P.setMaxPressureLimitB(context, pump, pumpMaxPressure, isTrapIn, isSepIn, pressTarget)
	local function sleep_100()
		context:Sleep(100)
	end
	---@type Zirconium
	local zr =     require "zirconium"
	local trp =    nil
	local sep =    nil
    if isTrapIn then
		---@type IChromatographyColumnType
		trp = context:GetArgumentValue("trap")
	end

    if isSepIn then
		---@type IChromatographyColumnType
		sep = context:GetArgumentValue("separator")
	end
	local pressureLimit = pumpMaxPressure
	if sep ~= nil then pressureLimit = P.getMaxColumnPressure(trp, sep, pumpMaxPressure) end

	if pressTarget == nil or pressTarget > pressureLimit then
		pressTarget = pressureLimit
	end
	context:Log("--- MaxPressureLimitB: {0} bar ---", pressureLimit)
    P.reducePressure(context, pump, zr, nil, zr.B, pressTarget, pressTarget, 300, sleep_100, false)
	local pB = pump:GetCurrentPressure(zr.B)
	if (pB <= (pressureLimit+5)) then
		local pressureLimitWithDelta = P.GetMaxPressureLimitWithDelta(pumpMaxPressure, pressureLimit)
		P.SetMaxPressureLimit(zr.B, pressureLimitWithDelta, pump, sleep_100)
		context:Sleep(100)
		local settings = pump:GetSettings()
		P.logExternalOffsetFactorAndMaxPressure(settings, context, pump, zr)
	else
		context:Log("--- MAX PRESSURE {0} has not been set, act pressure is B: {1}", pressureLimit, pB)
	end
	return pressureLimit
end

function P.deletePressABandCompositionFile()
	local fileName = "/BDALSystemData/HyStar/LogFiles/Bruker proteoElute pressLog.txt"
	os.remove(fileName)
end
function P.storePressABandComposition(pressA, pressB, composition)
	local fileName = "/BDALSystemData/HyStar/LogFiles/Bruker proteoElute pressLog.txt"
	local strg = tostring(pressA)..", "..tostring(pressB)..", "..tostring(composition).."\n"
	local function appendString()
		local file = io.open(fileName, "a")			-- Opens a file in append mode
		if file then
			io.output(file)
			io.write(strg)
			io.close()
		end
	end
	pcall(appendString)
end

---Return the maximum pressure of pump A or B and optionallythe corresponding flow and composition
---@param pump Pump
---@param zr Zirconium
---@return number
---@return number
---@return number
function P.getPressAndComp(pump, zr)
	local pA = pump:GetCurrentPressure(zr.A)
	local pB = pump:GetCurrentPressure(zr.B)
	local flowB = pump:GetSetFlow(zr.B)
	local comp = flowB/(pump:GetSetFlow(zr.A) + flowB)
	return P.noExp(pA, 3), P.noExp(pB, 3), P.noExp(comp, 3)
end

---Evaluate the gradient pressure
---@param context IProcedureExecutionContext
---@param sumPressA number
---@param sumPressB number
---@param numOfValues number
function P.evaluatePressure(context, sumPressA, sumPressB, numOfValues)
	if numOfValues == 1 then return end
	local function square(num)
		return num * num
	end
	local function file_exists(name)
	   local f = io.open(name, "r")
	   local fileExist = f ~= nil and io.close(f)
	   return fileExist
	end
	context:Log("---------------------------------------------------")
	context:Log("Evaluating pressure channel A and B")
	local fileName = "/BDALSystemData/HyStar/LogFiles/Bruker proteoElute pressLog.txt"

	local stat, exist = pcall(file_exists, fileName)
	if (stat == true) and (exist == true) then
		local averagePressA = sumPressA/numOfValues
		local averagePressB = sumPressB/numOfValues
		local maxPressA = 0.0
		local maxPressB = 0.0
		local varA = 0
		local varB = 0
		local startPosition = nil
		local endPosition = nil
		local msg = ""
		local number = nil

		for line in io.lines(fileName) do			-- read pressure A and B from file
			_, endPosition = string.find(line, ",")
			startPosition = 1
			msg = string.sub(line, startPosition, endPosition-1)
			if msg ~= nil then
				number = tonumber(msg)
				if number then
					varA = varA + square(number-averagePressA)
					maxPressA = math.max(number, maxPressA)
					startPosition = endPosition+1
					_, endPosition = string.find(line, ",", startPosition)
					msg = string.sub(line, startPosition, endPosition-1)
					number = tonumber(msg)
					if number then
						varB = varB + square(number-averagePressB)
						maxPressB = math.max(number, maxPressB)
					end
				end
			end
		end
		local stdevA = math.sqrt(varA/(numOfValues-1))
		local stdevB = math.sqrt(varB/(numOfValues-1))
		context:Log("Max pressure A: {0} bar", P.noExp(maxPressA,0))
		context:Log("Max pressure B: {0} bar", P.noExp(maxPressB,0))
		context:Log("Standard deviation pressure A: {0}", P.noExp(stdevA,3))
		context:Log("Standard deviation pressure B: {0}", P.noExp(stdevB,3))
		context:Log("---------------------------------------------------")
		return
	else
		context:Log("File: {0} not found.", fileName)
	end
end
----------------------------------------------
---Reduce the pressure of the system
---@param context IProcedureExecutionContext
---@param pump Pump
---@param zr Zirconium
---@param channelA any
---@param channelB any
---@param pressureA number
---@param pressureB number
---@param timeout number
---@param yield_function function
---@param smoothTransition boolean
function P.reducePressure(context, pump, zr, channelA, channelB, pressureA, pressureB, timeout, yield_function, smoothTransition)
    local baltic = require "baltic"
    local parallel = require "parallel"

	local blockInjectionPath = false
	local ch = channelA
	local press = pressureA
	if channelA == nil then
		ch = channelB
		press = pressureB
	else
		blockInjectionPath = ((pump:GetSetManualValvePosition(channelA) == baltic.PumpValve.Inject) and (pressureA+50 < pump:GetCurrentPressure(channelA)))
	end

    local pressChA = pump:GetCurrentPressure(zr.A)
    local pressChB = pump:GetCurrentPressure(zr.B)
	local reducePressure = pressChA > pressureA or pressChB > pressureB
	local checkFlow = false
    local settings = pump:GetSettings()

	if (pump:GetSetManualValvePosition(channelA) == baltic.PumpValve.MixTee) and (pump:GetSetManualValvePosition(channelB) == baltic.PumpValve.MixTee) then 
		checkFlow = true
		timeout = timeout * 1.5
	end

    local function now()
        return os.clock()
    end

	---Reduce the pressure of the desired channel
	---@param channel Channel
	---@param pressTarget number
	---@param yield_func function
	local function reduce(channel, pressTarget, yield_func)
        if channel ~= nil then 
	        local t = now() + timeout
			if pressTarget < 5 then pressTarget = 5 end
			if (pump:GetSetManualValvePosition(channel) == baltic.PumpValve.MixTee) and checkFlow then		-- use this if pump.valve==MixTee AND NOT (trapValve==Analytical OR trapValve==Trap)
				if pump:GetCurrentPressure(channel) > (pressTarget-2) then
					P.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_func)
					P.Manualmode_Pump_constantFlow(channel, 0, pump, yield_func)
				end
			else
				if pump:GetCurrentPressure(channel) > (pressTarget-2) then P.Manualmode_Pump_constantPressure(channel, (pressTarget-5), pump, yield_func) end
			end
			while (pump:GetCurrentPressure(channel) > pressTarget) and (now() < t) do 
				yield_func()
			end
			P.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_func)
        end
	end

    context:Log(baltic.devider)
    context:Log("--- Reduce pressure ---")
    context:Log("--- Pressure channel A: {0}", pressChA)
    context:Log("--- Pressure channel B: {0}", pressChB)

    if reducePressure then
		if smoothTransition then
			timeout = 3600
			zr.ChangePressurePID(context, pump, baltic.PPID.PID_50_2_1)
		else
			if checkFlow then
				zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_2_1)
			else
				zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_2_1)
			end
		end
        yield_function()

		context:Log("--- Set pressure channel A:          {0}", pressureA)
		context:Log("--- Set pressure channel B:          {0}", pressureB)
		context:Log("--- Reduce pressure flow controlled: {0}", checkFlow)

		if blockInjectionPath then
			local pp		 = require "palplus"
			local pr 		 = require "PreRunFunctions"
			---@type IPalParticipant
			local execAux	 = context:GetProcedureParticipant(baltic.AuxExecutorRole)
			---@type IPalParticipant
			local execLeft	 = context:GetProcedureParticipant(baltic.LeftExecutorRole)
			local valveI 	 = pp.QueryValveDrive(execLeft, pp.Capabilities.ILcInjectorValve)
			local actPos 	 = pp.GetInjectorValvePosition(execAux)

			while not execLeft.IsIdle do			-- PBNE-1044
				yield_function()
			end
			pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Block)
			context:Sleep(250)
			if (channelA ~= nil) and (channelB ~= nil) then
				local p_reducePressure_a = { reduce, channelA, pressureA, parallel.yield }
				local p_reducePressure_b = { reduce, channelB, pressureB, parallel.yield }
				parallel.run(yield_function, p_reducePressure_a, p_reducePressure_b)
			else
				reduce(ch, press, yield_function)
			end
			if actPos ~= nil then pr.SetValvePosition(execAux, valveI, actPos) end
		else
			if (channelA ~= nil) and (channelB ~= nil) then
				local p_reducePressure_a = { reduce, channelA, pressureA, parallel.yield }
				local p_reducePressure_b = { reduce, channelB, pressureB, parallel.yield }
				parallel.run(yield_function, p_reducePressure_a, p_reducePressure_b)
			else
				reduce(ch, press, yield_function)
			end
		end
	end

	P.isPumpIdle(pump, yield_function)

	if reducePressure then
        zr.setPumpSettings(context, pump, settings)
        local caller = "reducePressure"
        zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)
        zr.logInstrSettings(context, settings, caller)
        context:Log("--- Final pressure channel A: {0}", pump:GetCurrentPressure(channelA))
        context:Log("--- Final pressure channel B: {0}", pump:GetCurrentPressure(channelB))
	end
    context:Log(baltic.devider)
end

---Check if the pressure sensor factors are in a range
---@param offsetA number
---@param offsetB number
---@param SensorLimitLow number
---@param SensorLimitHigh number
---@return boolean
function P.IsPressSensorInRange(offsetA, offsetB, SensorLimitLow, SensorLimitHigh)
	local high = SensorLimitHigh
	local low = SensorLimitLow
	if (((offsetA > high) or (offsetB > high)) or ((offsetA < low) or (offsetB < low))) then return false end
	return true
end

---Check if the pressure sensor offsets and factors are OK
---@param context IProcedureExecutionContext
---@param zr Zirconium
---@param pump Pump
---@param defOffset number
---@param defFactor number
---@return boolean
function P.checkPressSensorSettings(context, zr, pump, defOffset, defFactor)
	local ps = pump:GetSettings()
	context:Log("--------------------------------------------------------")
	context:Log("--- ExternalZeroPressureOffsetA: {0:#0.0}; ExternalZeroPressureOffsetB: {1:#0.0}", ps.ExternalZeroPressureOffsetA, ps.ExternalZeroPressureOffsetB)
	context:Log("--- ExternalPressureScalingFactorA: {0:#0.00000}; ExternalPressureScalingFactorB: {1:#0.00000}", ps.ExternalPressureScalingFactorA, ps.ExternalPressureScalingFactorB)
	pump:GetMaxPressureLimit(zr.A)
	pump:GetMaxPressureLimit(zr.B)
	context:Log("--------------------------------------------------------")

	local msg1 = "Pressure Sensor Check"
	local details = ""
	local settingsOK = true
	if (math.abs(ps.ExternalZeroPressureOffsetA - defOffset) > (defOffset * 0.23))then
		details = DotNetString.Format("{0}\n- Sensor A offset: {1:#0.000} (expected near {2:#0.000})", details, ps.ExternalZeroPressureOffsetA, defOffset)
		settingsOK = false
	end
	if (math.abs(ps.ExternalZeroPressureOffsetB - defOffset) > (defOffset * 0.23))then
		details = DotNetString.Format("{0}\n- Sensor B offset: {1:#0.000} (expected near {2:#0.000})", details, ps.ExternalZeroPressureOffsetB, defOffset)
		settingsOK = false
	end
	if (math.abs(ps.ExternalPressureScalingFactorA - defFactor) > (defFactor * 0.02))then
		details = DotNetString.Format("{0}\n- Sensor A scaling factor: {1:#0.00000} (expected near {2:#0.00000})", details, ps.ExternalPressureScalingFactorA, defFactor)
		settingsOK = false
	end
	if (math.abs(ps.ExternalPressureScalingFactorB - defFactor) > (defFactor * 0.02))then
		details = DotNetString.Format("{0}\n- Sensor B scaling factor: {1:#0.00000} (expected near {2:#0.00000})", details, ps.ExternalPressureScalingFactorB, defFactor)
		settingsOK = false
	end
	if settingsOK == false then
		local msg = DotNetString.Format("Pressure sensor calibration values are out of range:{0}\n\nRun 'Preparation' to recalibrate. If this persists, the pressure sensor may need replacement.", details)
		context:Report(msg1, Severity.Error, true, msg)
	end
	return settingsOK
end

---Calibrate the pressure sensor offsets
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param pump Pump
---@param zr Zirconium
---@param valvePos number
---@return number
---@return number
---@return boolean
function P.calibrate_press_sensors(installed, context, pump, zr, valvePos)
	local function sleep_1000()
		context:Sleep(1000)
	end

	local sensorSettings = P.getPressureSensorSettings(installed.MaxPumpPressure)
	local passed = true
	local continue = true
	local testOffsetLo = 3
	local testOffsetHi = 6
	zr.SetValvePosition(context, pump, zr.A, valvePos)
	zr.SetValvePosition(context, pump, zr.B, valvePos)
	local settings = pump:GetSettings()

-->	Starting values for a new instrument / FW
--	context:Sleep(2000)
--	settings.ExternalPressureScalingFactorA = 0.01972
--	settings.ExternalPressureScalingFactorB = 0.01972
--	settings.ExternalPressureScalingFactorA = 0.03075
--	settings.ExternalPressureScalingFactorB = 0.03075
--	settings.ExternalZeroPressureOffsetB = -250
--	settings.ExternalZeroPressureOffsetA = -250
--	context:Sleep(2000)
--	pump:SetSettings(settings)
--<	Starting values for a new instrument / FW

	context:Sleep(2000)				-- waiting due to PBNE-604

	-- set offset so that a pressure of > 3 and < 6 is shown
	-- this prevents malfunction in case of an unexpected abort
	while continue do
		continue = false
		local pumpPressA = pump:GetCurrentPressure(zr.A)
		local pumpPressB = pump:GetCurrentPressure(zr.B)
		if pumpPressA < testOffsetLo then
			settings.ExternalZeroPressureOffsetA = settings.ExternalZeroPressureOffsetA - 50
			continue = true
		elseif pumpPressA > testOffsetHi then
			settings.ExternalZeroPressureOffsetA = settings.ExternalZeroPressureOffsetA + pumpPressA - testOffsetHi
			continue = true
		end
		if pumpPressB < testOffsetLo then
			settings.ExternalZeroPressureOffsetB = settings.ExternalZeroPressureOffsetB - 50
			continue = true
		elseif pumpPressB > testOffsetHi then
			settings.ExternalZeroPressureOffsetB = settings.ExternalZeroPressureOffsetB + pumpPressB - testOffsetHi
			continue = true
		end
		pump:SetSettings(settings)
		context:Sleep(250)
	end
	context:Sleep(5*1000)

	local n, offsetA, offsetB = 0, 0, 0
	for i=1, 5 do
		n=i
		offsetA = offsetA + pump:GetCurrentPressure(zr.A)
		offsetB = offsetB + pump:GetCurrentPressure(zr.B)
		context:Sleep(1000)
	end
	offsetA = offsetA/n + settings.ExternalZeroPressureOffsetA
	offsetB = offsetB/n + settings.ExternalZeroPressureOffsetB
	context:Log("offsetA: {0}, B: {1}", offsetA, offsetB)
	if not P.IsPressSensorInRange(offsetA, offsetB, sensorSettings.SensorLimitLow, sensorSettings.SensorLimitHigh) then
		local msg1 = DotNetString.Format("Pressure sensor offset out of range ( {0:#0} - {1:#0})", sensorSettings.SensorLimitLow, sensorSettings.SensorLimitHigh)
		local msg2 = DotNetString.Format("Sensor A: {0:#0.0}, sensor B: {1:#0.0}.", offsetA, offsetB)
		context:Report(msg1, Severity.Info, true, msg2.."\n\nThe pressure sensor offset is outside the normal calibration range. This may indicate sensor wear or drift.\n\nRecommended: Schedule a service appointment to inspect or replace the pressure sensor.")
		context:Sleep(1000)
	end
	settings.ExternalZeroPressureOffsetA = offsetA
	settings.ExternalZeroPressureOffsetB = offsetB
	zr.setPumpSettings(context, pump, settings)
	local caller = "calibrate_press_sensors"
	-- zr.logPIDs(context, settings.PressurePID, caller) no PID change here
	zr.logInstrSettings(context, settings, caller)

	P.isPumpIdle(pump, sleep_1000)
	return settings.ExternalZeroPressureOffsetA, settings.ExternalZeroPressureOffsetB, passed
end

---Leave the pump on until the criteria has been reached
---@param pump Pump
---@param channel Channel
---@param criteria function
---@param yield_function function
---@param timeout number
---@return number
---@return string
---@return string
function P.pump_until(pump, channel, criteria, yield_function, timeout)
	-- return current time in milliseconds.
	local function now()
		return os.clock()
	end

	local err = ""
	local msg = ""

	local piston = pump:GetPistonPosition(channel)
--	local max_piston = math.min(pump.TotalPistonVolume - 0.001, piston + max_volume)
	local max_time_allowed = now() + timeout
	repeat
		if ( now() > max_time_allowed ) then
			err = "error"
			msg = "Pump exceeded time limit"
			local volume = pump:GetPistonPosition(channel) - piston
			return volume, err, msg
		end
		yield_function()
	until criteria()
	local volume = pump:GetPistonPosition(channel) - piston
	return volume, err, msg
end

---Refill both pumps in parallel
---@param context IProcedureExecutionContext
---@param pump Pump
---@param zr Zirconium
---@param refillVolA number
---@param refillVolB number
function P.refillAB(context, pump, zr, refillVolA, refillVolB)
	local parallel = require "parallel"

	local function sleep_100()
		context:Sleep(100)
	end

	---Refill the pump
	---@param context IProcedureExecutionContext
	---@param pump Pump
	---@param zr Zirconium
	---@param channel Channel
	---@param refillVolume number
	---@param yield_function function
	local function refill(context, pump, zr, channel, refillVolume, yield_function)
		local baltic = require "baltic"

		--do not refill if pump has used less than 500 uL (can be increased with further testing)
		--push 50 uL to waste if pump is full
		local minimumVolume = 50
		local pistPos = pump:GetPistonPosition(channel)

		if pistPos > refillVolume then
			pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
			parallel.sleep(yield_function, 30000)
			zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent,nil)

			P.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpRefillSpeed, pump, yield_function)

			-- do not fill completely, thus allowing pistons to move backwards during initial run
			while (pump:GetPistonPosition(channel) > minimumVolume) do yield_function() end
			yield_function() 
		else
			if pistPos < minimumVolume then --empty 50 nL
				zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Waste,nil)
				P.Manualmode_Pump_constantSpeed(channel, 300, pump, yield_function)
				while (pump:GetPistonPosition(channel) < minimumVolume) do yield_function() end
			end
		end
		P.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_function)
		parallel.sleep(yield_function, 2000)
		P.Manualmode_Pump_constantSpeed(channel, 300, pump, yield_function)
		parallel.sleep(yield_function, 2000)
		P.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_function)
		parallel.sleep(yield_function, 4000)
	end

	local p_refill_a = { refill, context, pump, zr, zr.A, refillVolA, parallel.yield }
	local p_refill_b = { refill, context, pump, zr, zr.B, refillVolB, parallel.yield }

	parallel.run(sleep_100, p_refill_a, p_refill_b)
	context:Sleep(5*1000)

	pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
end

---Evaluate the flow sensor offset
---@param context IProcedureExecutionContext
---@param zr Zirconium
---@param pump Pump
---@param ch Channel
---@param runTest boolean
---@param waitBeforeStart number seconds
---@param yield_func function
---@return number?
function P.flowSensorOffset(context, zr, pump, ch, runTest, waitBeforeStart, yield_func)
	if runTest == true then
		local parallel = require "parallel"
		waitBeforeStart = waitBeforeStart * 1000
		local logName = "Flowsensor A offset drift"
		local offset = 0
		if ch == zr.B then
			logName = "Flowsensor B offset drift"
		end
		parallel.sleep(yield_func, waitBeforeStart)		-- wait 600 seconds
		context:Log("Start "..logName)
		local actFlowOffset = zr.GetFlowCalibrationOffset(pump, ch)
		parallel.sleep(yield_func, 100)
		-- get an average of 100 values
		for i=1, 100 do
			offset = offset + pump:GetCurrentFlow(ch)		-- [uL]
			parallel.sleep(yield_func, 100)
		end
		offset = offset/100			-- [uL]
		if math.abs(offset) <= 0.1 then
			return (-offset+actFlowOffset)
		else
			local channel = "A"
			if ch == zr.B then channel = "B" end
			local msg1 = "Flow sensor "..channel
			local msg2 = "Flow sensor offset is "..(math.floor(offset*100000) / 100000).." uL/min (acceptable range: -0.1 to +0.1).\n\nThe flow sensor may need recalibration or replacement. Run the 'Calibrate' procedure to attempt automatic correction."
			context:Report(msg1, Severity.Warn, true, msg2)
			return
		end
	end
end

return P
