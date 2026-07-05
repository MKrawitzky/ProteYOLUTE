local Date = "2025/07/03"

local P = {}

-- dependencies as locals here
luanet.load_assembly("Bruker.Zirconium.Pump.Communication")
luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type ValveState
local ValveState = luanet.import_type("Bruker.Zirconium.Pump.Communication.ValveState")

---@type PistonState
local PistonState = luanet.import_type("Bruker.Zirconium.Pump.Communication.PistonState")
---@type Channel
local Channel = luanet.import_type("Bruker.Zirconium.Pump.Communication.Channel")

---@type PumpMethod
local ZrPumpMethod = luanet.import_type("Bruker.Zirconium.Pump.Communication.PumpData.PumpMethod")
---@type RTTLine
local RTTLine = luanet.import_type("Bruker.Zirconium.Pump.Communication.PumpData.PumpMethod+RTTLine")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")

local _error = error

---@type PumpSideState
P.PumpSideState = luanet.import_type("Bruker.Zirconium.Pump.Communication.PumpSideState")
---@type PumpSideSignal
P.PumpsideSignal = luanet.import_type("Bruker.Zirconium.Pump.Communication.PumpSideSignal")
---@type MethodState
P.MethodState = luanet.import_type("Bruker.Zirconium.Pump.Communication.MethodState")
---@type MethodSignal
P.MethodSignal = luanet.import_type("Bruker.Zirconium.Pump.Communication.MethodSignal")
---@type DigitalOutput
P.DigitalOutput = luanet.import_type("Bruker.Zirconium.Pump.Communication.DigitalOutput")

---@type Channel
P.A = Channel.A
---@type Channel
P.B = Channel.B

-- replace global env with package env; no more global access, everything 'global' will belong to P.
_ENV = P

local devider = "--------------------------------------------------------"

local valveShiftPosition = {cntA=0, posA=0, cntB=0, posB=0}
local pumpPosition = {aStart = 0, aEnd = 0, bStart = 0, bEnd = 0}

---Store the volume of the pumps
---@param pump Pump
---@param start boolean
function P.storePumpVolume(pump, start)
	if start == true then
		pumpPosition.aStart = pump:GetPistonPosition(P.A)
		pumpPosition.bStart = pump:GetPistonPosition(P.B)
	else
		pumpPosition.aEnd = pump:GetPistonPosition(P.A)
		pumpPosition.bEnd = pump:GetPistonPosition(P.B)
	end
end

---Stores and logs the pump volume
---@param context IProcedureExecutionContext
---@param pump Pump
function P.logPumpVolume(context, pump)
	P.storePumpVolume(pump, false)
	context:Log("  PumpA position @ start: {0:#0.00}", pumpPosition.aStart)
	context:Log("  PumpB position @ start: {0:#0.00}", pumpPosition.bStart)
	context:Log("  PumpA position @ end:   {0:#0.00}", pumpPosition.aEnd)
	context:Log("  PumpB position @ end:   {0:#0.00}", pumpPosition.bEnd)
	context:Log("  PumpA volume used:      {0:#0.00} uL", (pumpPosition.aEnd-pumpPosition.aStart))
	context:Log("  PumpB volume used:      {0:#0.00} uL", (pumpPosition.bEnd-pumpPosition.bStart))
end

---Store the valve shift counter of the desired channel
---@param channel Channel
---@param position number
---@param actualPos number
function P.saveValveShiftPosition(channel, position, actualPos)
	if channel == P.A then
		valveShiftPosition.posA  = position		-- store last valve position
		if actualPos ~= position then valveShiftPosition.cntA = valveShiftPosition.cntA + 1 end			-- add 1 to counter
	else
		valveShiftPosition.posB  = position		-- store last valve position
		if actualPos ~= position then valveShiftPosition.cntB = valveShiftPosition.cntB + 1 end			-- add 1 to counter
	end
end

function P.resetValveABShiftCounterPosition()
	valveShiftPosition.cntA = 0
	valveShiftPosition.cntB = 0
end

---Gets the actual valve positions, stores and logs them
---@param context IProcedureExecutionContext
---@param pump Pump
function P.logValveABShiftCounterPosition(context, pump)
	local posA = pump:GetSetManualValvePosition(P.A)			-- get actual valve position
	local posB = pump:GetSetManualValvePosition(P.B)
	P.saveValveShiftPosition(P.A, posA, posA)					-- save actual valve position
	P.saveValveShiftPosition(P.B, posB, posB)
	context:Log("ValveA counter: {0}; position: {1}", valveShiftPosition.cntA, valveShiftPosition.posA)
	context:Log("ValveB counter: {0}; position: {1}", valveShiftPosition.cntB, valveShiftPosition.posB)
end

---Switch the valve to the desired position clockwise or counterclockwise, depending on the strat and end position and on the pressure
---@param context IProcedureExecutionContext
---@param pump Pump
---@param channel Channel
---@param target number
---@param clockwise boolean | nil
---@return number
---@return number
function P.SetValvePosition(context, pump, channel, target, clockwise)
	---Converts a numeric value to boolean
	---@param numericValue number
	---@return boolean
	local function tobool(numericValue)
		return numericValue == 1
	end

	if (target < 0 or target >= 360) then
		_error("Valve position must be between 0 and 359 degrees (received "..tostring(target)..")", 2)
	end

	local cw = 0
	if clockwise == true then cw = 1 end
	if (nil == clockwise) then			-- 0=0; 1=60; 2=120; 3=180; 4=240; 5=300
		-- local aPumpValveSource[] 	= {0,0,0,0,0,0,1,1,1,1,1,1,2,2,2,2,2,2,3,3,3,3,3,3,4,4,4,4,4,4,5,5,5,5,5,5}
		-- local aPumpValveTarget[] 	= {0,1,2,3,4,5,0,1,2,3,4,5,0,1,2,3,4,5,0,1,2,3,4,5,0,1,2,3,4,5,0,1,2,3,4,5}
		local aPumpValveCWlowP			= {0,1,1,0,0,0,0,0,1,1,1,0,0,0,0,1,1,1,0,0,0,0,1,1,1,0,0,0,0,1,1,1,0,0,0,0}		-- 0: counterclockwise
		local aPumpValveCWhighP			= {0,0,0,0,0,0,0,0,1,1,1,0,1,1,0,1,1,1,1,0,0,0,1,1,1,0,0,0,0,1,1,0,0,0,0,0}		-- 1: clockwise
		local aPumpValveWaste			= {0,1,1,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,1,1,0,0,0}		-- 1: stop for depressurize via waste port
		--> these arrays are for block positions
		local aTargetPosBlockCWlowP		= {1,1,1,0,0,0,0,1,1,1,0,0,0,0,1,1,1,0,0,0,0,1,1,1,1,0,0,0,1,1,1,1,0,0,0,1}
		local aTargetPosBlockCWhighP	= {1,0,0,0,0,0,0,1,1,1,0,0,1,0,1,1,1,1,0,0,0,1,1,1,1,0,0,0,1,1,1,0,0,0,0,1}
		local aTargetPosBlockWaste		= {0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,1,0,0,0,0}
		local aSourcePosBlockCWlowP		= {0,1,1,1,0,0,0,0,1,1,1,0,0,0,0,1,1,1,1,0,0,0,1,1,1,1,0,0,0,1,1,1,1,0,0,0}
		local aSourcePosBlockCWhighP	= {0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,1,1,1,0,0,0,0,1,1,0,0,0,0,0}
		local aSourcePosBlockWaste		= {0,1,1,0,0,0,0,1,0,0,0,0,0,1,1,0,0,0,0,1,1,0,0,0,0,1,1,0,0,0,0,1,1,0,0,0}
		local aLowPress					= {aPumpValveCWlowP,aTargetPosBlockCWlowP,aSourcePosBlockCWlowP}
		local aHighPress				= {aPumpValveCWhighP,aTargetPosBlockCWhighP,aSourcePosBlockCWhighP}
		local aToWaste					= {aPumpValveWaste,aTargetPosBlockWaste,aSourcePosBlockWaste}

		local source = pump:GetSetManualValvePosition(channel)
		local arrayIdx = 1
		local addValue = 1

		local targetBlock = false
		local sourceBlock = false
		for i=0, 5 do
			if targetBlock == false then
				targetBlock = target == ((i * 60) + 30)
				if (targetBlock == true) then
					arrayIdx = 2
					addValue = 0.5
				end
			end
			if sourceBlock == false then
				sourceBlock = source == ((i * 60) + 30)
				if (sourceBlock == true) then
					arrayIdx = 3
					addValue = -2
				end
			end
		end

		local cwArray = aLowPress[arrayIdx]
		local idx = (source/10) + (target/60) + addValue
		cw = cwArray[idx]
		local currentPressure = pump:GetCurrentPressure(channel)
		if (currentPressure > 10) then
			cwArray = aHighPress[arrayIdx]		-- system is under pressure therefore use high pressure array for direction
			cw = cwArray[idx]
			if (aToWaste[arrayIdx][idx] == 1) then
				P.saveValveShiftPosition(channel, 180, source)
				pump:SetManualValvePosition(channel, 180, tobool(cw))
				local t = 0
				repeat
					context:Sleep(200)
					t=t+1
				until (pump:GetCurrentPressure(channel) <= 10 or t > 25)
				if t > 25 then
					local ch = "A"
					if channel == P.B then ch = "B" end
					local valve = "Valve "..ch
					context:Report(valve, Severity.Error, true, "Pressure reduction not possible. Please check the waste tubing for blockage.")
					context:Abort()
				end
				idx = 18 + (target/60) + addValue	-- idx = waste * 6 + target + 1
				cw = aLowPress[arrayIdx][idx]	-- system is without pressure therefore use low pressure array for direction
			end
		end
	end
	P.saveValveShiftPosition(channel, target, pump:GetSetManualValvePosition(channel))
	pump:SetManualValvePosition(channel, target, tobool(cw))
	-- wait for the valve to stop moving.
	repeat
		context:Sleep(200)
	until ValveState.Stopped == pump:GetValveState(channel)

	return target, cw
end

---Inits the channel
---@param context IProcedureExecutionContext
---@param pump Pump
---@param channel Channel
---@return boolean
function P.InitChannel(context, pump, channel)
	local state = pump:GetPumpSideState(channel)
	if (P.PumpSideState.ManualMode ~= state) then
		if (P.PumpSideState.Uninitialized == state) then
			-- PumpSideInit only allowed when uninitialized
			pump:PumpSideInit(channel, true)
		else
			if (P.PumpSideState.Initialized ~= state) then
				pump:SetPumpSideSignal(channel, P.PumpsideSignal.Stop)
				-- here we cannot wait for a state change because we could already be idle.
				context:Sleep(1000)
				-- we should be in an idle state and can transition into Aspirate.
				pump:SetPumpSideSignal(channel, P.PumpsideSignal.Aspirate)
				-- don't expect aspirating as we will transition directly to initialized if at home pos
				context:Sleep(1000)
				pump:SetPumpSideSignal(channel, P.PumpsideSignal.Stop)
				repeat
					context:Sleep(200)
					state = pump:GetPumpSideState(channel)
				until P.PumpSideState.Initialized == state
			end
			-- ManualMode signal only allowed when initialized
			pump:SetPumpSideSignal(channel, P.PumpsideSignal.ManualMode)
		end
		-- wait for manual mode
		repeat
			context:Sleep(200)
			state = pump:GetPumpSideState(channel)
		until P.PumpSideState.ManualMode == state
	end
	local isManualMode = P.PumpSideState.ManualMode == state
	return isManualMode
end

---Return true if the pump is empty
---@param pump Pump
---@param channel Channel
---@return boolean
function P.IsEmpty(pump, channel)
	local stopped = PistonState.AtEndPosition == pump:GetPistonState(channel)
	return stopped
end

---Return true if the pump is full
---@param pump Pump
---@param channel Channel
---@return boolean
function P.IsFull(pump, channel)
	local isAtHomePosition = PistonState.AtHomePosition == pump:GetPistonState(channel)
	return isAtHomePosition
end

---Return true if the pump is at least halfe full
---@param pump Pump
---@param channel Channel
---@return boolean
function P.IsAtleastHalfFull(pump, channel)
	local volume = pump.TotalPistonVolume
	local isHalfeFull = pump:GetPistonPosition(channel) < volume / 2
	return isHalfeFull
end


---Create a gradient instance to which gradient segments may be added
---@param minPressure number
---@param maxPressure number
---@param flowControl boolean
---@param temperature number
---@return PumpMethod
function P.CreateGradient(minPressure, maxPressure, flowControl, temperature)
	-- create two-channel method
	---@type PumpMethod
	local method = ZrPumpMethod(2)
	method.ColumnTemperature = temperature
	method.HighPressureVersion = true
	method.FlowControl = flowControl or true
	method.MinPressureLimit = minPressure
	method.MaxPressureLimit = maxPressure/0.9 -- fw takes 90% of method.MaxPressureLimit. BEWARE!!! This changes the fw pressure limit "switch" for both channels
	if method.MaxPressureLimit > 1300 then method.MaxPressureLimit = 1300 end		-- ONLY for FW183
	return method
end

---Add a segment to the gradient
---@param gradient PumpMethod
---@param time number
---@param flow number
---@param percentB number
function P.AddGradientSegment(gradient, time, flow, percentB)
	local line = RTTLine()
	line.Time = time
	line.Flow = flow
	line.PercentB = percentB
	gradient:AddRTTLine(line)
end

--- Start a gradient trunk method.
--@param gradient The gradient instance to pull gradient info from.
-- @return nil if supplied gradient is validated and started, otherwise a string message.
--- Check whether the pump is currently running a gradient trunk method.
-- @return true if a gradient trunk method is running, false otherwise.
---comment Returns true if a gradient is running
---@param pump Pump
---@return boolean
function P.IsGradientRunning(pump)
	local isRunning = pump:GetMethodState(pump.TrunkMethodIndex) == P.MethodState.RunningTrunk
	return isRunning
end

---Returns true if gradient is loaded
---@param pump Pump
---@return boolean
function P.IsGradientLoaded(pump)
	local isLoaded = pump:GetMethodState(pump.TrunkMethodIndex) == P.MethodState.Loaded
	return isLoaded
end

---comment
---@param context IProcedureExecutionContext
---@param pump Pump
---@param gradient PumpMethod
---@return unknown | nil
function P.LoadGradient(context, pump, gradient)
	context:Log("--- LoadGradient() ---")
	local trunkIdx = pump.TrunkMethodIndex
	local i = 0
	local valid, message = gradient:Validate()
	if not valid then
		return message
	end

	-- PNS-690: It is important to directly load the new gradient without unloading the old one first.
	-- With unloading first we observed that fast 1c did not work properly anymore, assuming that the pump goes
	-- into a different state which affects flow/pressure control.
	context:Log("--- Loading gradient method index: {0}",trunkIdx)
	pump:LoadMethod(trunkIdx, gradient)
	context:Sleep(500)
	while (not P.IsGradientLoaded(pump)) do
		context:Sleep(1000)
		i=i+1
		pump:LoadMethod(trunkIdx, gradient)
		context:Log("Gradient load retry attempt {0}",i)
		if i>10 then
			local msg = DotNetString.Format("Gradient failed to load after {0:#0} attempts",i)
			context:Report(msg, Severity.Error, true, "The pump gradient method could not be loaded. Possible causes:\n- Communication issue between software and pump firmware\n- Pump is in an unexpected state\n\nTry: Restart the instrument and run 'Preparation'. If the problem persists, contact your service representative.")
			context:Log("Gradient load failed after {0} attempts",i)
			context:Abort()
		end
	end
	-- no return statement here is on purpose.
end

---Unloads the gradient
---@param context IProcedureExecutionContext
---@param pump Pump
function P.UnLoadGradient(context, pump)
	context:Log("--- UnLoadGradient() ---")
	local trunkIdx = pump.TrunkMethodIndex
	context:Log("--- Unloading gradient method index: {0}",trunkIdx)
	pump:SetMethodSignal(trunkIdx, P.MethodSignal.Unload)
	context:Sleep(200)
	-- no return statement here is on purpose.
end

---Starts the gradient
---@param context IProcedureExecutionContext
---@param pump Pump
function P.StartGradient(context, pump)
	context:Log("--- StartGradient() ---")
	local trunkIdx = pump.TrunkMethodIndex
	local i = 0

	context:Log("--- Starting gradient method index: {0}",trunkIdx)
	pump:SetMethodSignal(trunkIdx, P.MethodSignal.StartTrunk)
	context:Sleep(200)
	while (not P.IsGradientRunning(pump)) do
		context:Sleep(1000)
		i=i+1
		pump:SetMethodSignal(trunkIdx, P.MethodSignal.StartTrunk)
		context:Log("Gradient start retry attempt {0}",i)
		if i>10 then
			local msg = DotNetString.Format("Gradient failed to start after {0:#0} attempts",i)
			context:Report(msg, Severity.Error, true, "The pump gradient method could not be started. Possible causes:\n- Communication issue between software and pump firmware\n- Pump is in an unexpected state\n\nTry: Restart the instrument and run 'Preparation'. If the problem persists, contact your service representative.")
			context:Log("Gradient start failed after {0} attempts",i)
			context:Abort()
		end
	end
	-- no return statement here is on purpose.
end

---Aborts the gradient
---@param pump Pump
function P.abortGradient(pump)
	if (pump:GetMethodState(pump.TrunkMethodIndex) == P.MethodState.RunningTrunk) then
		pump:SetMethodSignal(pump.TrunkMethodIndex, P.MethodSignal.Abort)	-- MethodSignal.Unload
	end
--	pump:SetMethodSignal(pump.TrunkMethodIndex, MethodSignal.AcknowledgeStop)	-- this seems not to be neccessary
end

---transfers pid settings from a Lua table to a zirconium PID object. Unspecified values (nil) in the lua table are ignored.
---@param instrPID PID
---@param newPID PIDParams
local function update_pid(instrPID, newPID)
	if (newPID.P) then
		instrPID.P = newPID.P
	end
	if (newPID.I) then
		instrPID.I = newPID.I
	end
	if (newPID.D) then
		instrPID.D = newPID.D
	end

	if (newPID.ResultMin) then
		instrPID.ResultMin = newPID.ResultMin
	end
	if (newPID.ResultMax) then
		instrPID.ResultMax = newPID.ResultMax
	end
end

---Logs the PID values
---@param context IProcedureExecutionContext
---@param pressPID PID
---@param flowPID PID
---@param origin string
function P.logPIDs(context, pressPID, flowPID, origin)
	context:Log(devider)
	context:Log("--- Called from: {0}", origin)
	context:Log("--- Log pressure PIDs")
	context:Log("--- P: {0}; I: {1}; D: {2}", pressPID.P, pressPID.I, pressPID.D)
	context:Log("--- Result min: {0}; Result max: {1}", pressPID.ResultMin, pressPID.ResultMax)
	context:Log("--- Log flow PIDs")
	context:Log("--- P: {0}; I: {1}; D: {2}", flowPID.P, flowPID.I, flowPID.D)
	context:Log("--- Result min: {0}; Result max: {1}", flowPID.ResultMin, flowPID.ResultMax)
	context:Log(devider)
end

---Logs the instrument settings
---@param context IProcedureExecutionContext
---@param ps table
---@param origin string
function P.logInstrSettings(context, ps, origin)
	context:Log(devider)
	context:Log("--- Log instrument settings called from: {0}", origin)
	context:Log("--- HomeSpeed: {0}; PurgeSpeed: {1}", ps.HomeSpeed, ps.PurgeSpeed)
	context:Log("--- PressureTrackingExcessFactor: {0}", ps.PressureTrackingExcessFactor)
	context:Log("--- FlowResistanceA: {0}; FlowResistanceB: {1}; FlowResistanceOut: {2}", ps.FlowResistanceA, ps.FlowResistanceB, ps.FlowResistanceOut)
	context:Log("--- FC_FlowFilterDepth: {0}; FC_PressureFilterDepth: {1}", ps.FlowController_FlowFilterDepth, ps.FlowController_PressureFilterDepth)
	context:Log("--- PC_OutputFilterDepth: {0}", ps.PressureController_OutputFilterDepth)
	context:Log("--- CompressibilityFactorB: {0}; PressurePIDNegativeResultDivisor: {1}", ps.CompressibilityFactorB, ps.PressurePIDNegativeResultDivisor)
	context:Log("--- ExternalZeroPressureOffsetA: {0}; ExternalZeroPressureOffsetB: {1}", ps.ExternalZeroPressureOffsetA, ps.ExternalZeroPressureOffsetB)
	context:Log("--- ExternalPressureScalingFactorA: {0}; ExternalPressureScalingFactorB: {1}", ps.ExternalPressureScalingFactorA, ps.ExternalPressureScalingFactorB)
	context:Log(devider)
end

---Set pressure offsets and factors
---@param context IProcedureExecutionContext
---@param pump Pump
---@param ps table
function P.setPumpSettings(context, pump, ps)
	local SensorDefaultOffsetRange = {100,350}
	local SensorDefaultFactor = {0.01,0.04}

	if (ps.ExternalZeroPressureOffsetA < SensorDefaultOffsetRange[1])
		or (ps.ExternalZeroPressureOffsetA > SensorDefaultOffsetRange[2])
		or (ps.ExternalZeroPressureOffsetB < SensorDefaultOffsetRange[1])
		or (ps.ExternalZeroPressureOffsetB > SensorDefaultOffsetRange[2])
		or (ps.ExternalPressureScalingFactorA < SensorDefaultFactor[1])
		or (ps.ExternalPressureScalingFactorA > SensorDefaultFactor[2])
		or (ps.ExternalPressureScalingFactorB < SensorDefaultFactor[1])
		or (ps.ExternalPressureScalingFactorB > SensorDefaultFactor[2]) then
		context:Report("Pump Settings", Severity.Info, true, "Pressure sensor calibration values are out of the expected range.\n\nThis typically happens after a sensor replacement or firmware update.\n\nAction required: Run the 'Preparation' procedure to recalibrate the pressure sensors.")
	else
		pump:SetSettings(ps)
	end
end

---Changes the pressure PID values
---@param context IProcedureExecutionContext
---@param pump Pump
---@param newPID PIDParams
function P.ChangePressurePID(context, pump, newPID)
	local settings = pump:GetSettings()
	local instrPID = settings.PressurePID

	update_pid(instrPID, newPID)
	context:Log(devider)
	context:Log("PID settings changed:")
	context:Log("P: {0} : {1}", instrPID.P, newPID.P~=nil)
	context:Log("I: {0} : {1}", instrPID.I, newPID.I~=nil)
	context:Log("D: {0} : {1}", instrPID.D, newPID.D~=nil)
	context:Log("Result min {0} : {1}; Result max: {2} : {3}", instrPID.ResultMin, newPID.ResultMin~=nil, instrPID.ResultMax, newPID.ResultMax~=nil)

	settings.PressurePID = instrPID
	P.setPumpSettings(context, pump, settings)
	context:Log(devider)
end


function P.setFlowPID(context, pump, settings, FlowP, FlowI, FlowD)
	settings.FlowPID.P = FlowP
	settings.FlowPID.I = FlowI
	settings.FlowPID.D = FlowD
	pump:SetSettings(settings)
	context:Log("Flow PID settings changed:")
	context:Log("P: {0}; I: {1}; D: {2}", FlowP, FlowI, FlowD)
end

---transfers heater pid settings from a Lua table to a zirconium PID object. Unspecified values (nil) in the lua table are ignored.
---@param instrPID PID
---@param newPID PIDParams
local function update_heater_pid(instrPID, newPID)
	if (newPID.P) then instrPID.P = newPID.P end
	if (newPID.I) then instrPID.I = newPID.I end
	if (newPID.D) then instrPID.D = newPID.D end

	if (newPID.ResultMin) then instrPID.ResultMin = newPID.ResultMin end
	if (newPID.ResultMax) then instrPID.ResultMax = newPID.ResultMax end
end

---Changes the external heaters PID values
---@param context IProcedureExecutionContext
---@param pump Pump
---@param newPID PIDParams
function P.ChangeExternalHeaterPID(context, pump, newPID)
	local settings = pump:GetSettings()
	local instrPID = settings.ExternalHeatingPID
	update_heater_pid(instrPID, newPID)
	context:Log(devider)
	context:Log("PID settings changed:")
	context:Log("P: {0} : {1}; Pmin: {2} : {3}; Pmax: {4} : {5}", instrPID.P, newPID.P~=nil, instrPID.Pmin, newPID.Pmin~=nil, instrPID.Pmax, newPID.Pmax~=nil)
	context:Log("I: {0} : {1}; Imin: {2} : {3}; Imax: {4} : {5}; IWindow: {6} : {7}", instrPID.I, newPID.I~=nil, instrPID.Imin, newPID.Imin~=nil, instrPID.Imax, newPID.Imax~=nil, instrPID.IWindow, newPID.IWindow~=nil)
	context:Log("D: {0} : {1}; Dmin: {2} : {3}; Dmax: {4} : {5}", instrPID.D, newPID.D~=nil, instrPID.Dmin, newPID.Dmin~=nil, instrPID.Dmax, newPID.Dmax~=nil)
	context:Log("Result min: {0} : {1}; Result max: {2} : {3}", instrPID.ResultMin, newPID.ResultMin~=nil, instrPID.ResultMax, newPID.ResultMax~=nil)
	settings.ExternalHeatingPID = instrPID
	P.setPumpSettings(context, pump, settings)
	context:Log(devider)
end

---Gets the flow calibration offset for a given channel
---@param pump Pump
---@param channel Channel
---@return number
function P.GetFlowCalibrationOffset(pump, channel)
	local flowCalibrationOffset = 0
	if (channel == P.A) then
		flowCalibrationOffset = pump:GetFlowCalibrationOffsetA()
	else
		flowCalibrationOffset = pump:GetFlowCalibrationOffsetB()
	end

	return flowCalibrationOffset
end

---Sets the flow calibration offset for a given channel
---@param pump Pump
---@param channel Channel
---@param value number
function P.SetFlowCalibrationOffset(pump, channel, value)
	if (channel == P.A) then
		pump:SetFlowCalibrationOffsetA(value)
	else
		pump:SetFlowCalibrationOffsetB(value)
	end
end

---Gets the flow calibration factor for a given channel
---@param pump Pump
---@param channel Channel
---@return number
function P.GetFlowCalibrationFactor(pump, channel)
	local flowCalibrationFactor
	if (channel == P.A) then
		flowCalibrationFactor = pump:GetFlowCalibrationFactorA()
	else
		flowCalibrationFactor = pump:GetFlowCalibrationFactorB()
	end

	return flowCalibrationFactor
end

---Sets the flow calibration factor for a given channel
---@param pump Pump
---@param channel Channel
---@param value number
function P.SetFlowCalibrationFactor(pump, channel, value)
	if (channel == P.A) then
		pump:SetFlowCalibrationFactorA(value)
	else
		pump:SetFlowCalibrationFactorB(value)
	end
end

return P
