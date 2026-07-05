local Date = "2025/09/18"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")

local baltic = require "baltic"
local parallel = require "parallel"
local pf = require "pump_functions"

---Degassing a pump
---@param context IProcedureExecutionContext
---@param zr Zirconium
---@param channel Channel
---@param pressure number
---@param holdTime number
---@param checkForAir boolean
---@param yield_function function
---@return number
---@return number
function degas_pump(context, zr, channel, pressure, holdTime, checkForAir, yield_function)
	---@type Pump
    local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)

	local oneSecondGone = pf.now()+1
    local function build_pressure(yield_func)
        if zr.IsEmpty(pump, channel) then return false end
        context:Log("build_pressure {0}", channel)
		local runTime = 0
		local compressTime = pf.now()
		local actualPressure = pump:GetCurrentPressure(channel)
		local reduceSpeed = true

		yield_func()
        pf.Manualmode_Pump_constantPressure(channel, (pressure*2), pump, yield_func)
        while not zr.IsEmpty(pump, channel) do 
			if (checkForAir == true) then
				yield_func()
				if (pump:GetCurrentPressure(channel) > (actualPressure + 2.5)) then
					compressTime = pf.now() - compressTime
					checkForAir = false
					if (compressTime > 3) then			-- there is too much air in the pump
						pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_func)
						context:Log("Time for compressing pump {0} volume is too long: {1} sec. ", channel, compressTime)
						return false
					end
				end
			else
				if runTime >= 300 then
					local msg = DotNetString.Format("Pump {0:}:", channel)
					context:Report(msg, Severity.Error, true, "Could not build pressure within 300 seconds.\n\nLikely causes:\n- Large air bubble in the pump\n- Empty solvent reservoir\n- Leaking fitting or connection\n- Pump valve not seated correctly\n\nThe system will decompress. Check solvent levels and connections, then retry.")
					local pr = require "PreRunFunctions"
					pr.decompressSystem(context)
					context:Abort()
				end
				if (pump:GetCurrentPressure(channel) >= (pressure*0.1)) then
					if reduceSpeed == true then
				        pf.Manualmode_Pump_constantPressure(channel, pressure, pump, yield_func)
						reduceSpeed = false
					end
					if (pump:GetCurrentPressure(channel) >= pressure) then break end
				end
				if runTime > 30 and pump:GetPistonPosition(channel) > 650 then return false end
				yield_func()
				if oneSecondGone <= pf.now() then
					runTime = runTime + 1
					oneSecondGone = pf.now()+1
				end
			end
        end
		return true
    end

	local function getOneSecondPistonPositionAverage(yield_func)
		local val = 0
		for i=1, 10 do
			val = val + pump:GetPistonPosition(channel)
			yield_func()
		end
		return val/10
	end

    zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Compress, nil)
    parallel.sleep(yield_function, 2000)
	local posBefore, posAfter, posStartPressurizing = 0, 0, getOneSecondPistonPositionAverage(yield_function)
    if build_pressure(yield_function) then
		parallel.sleep(yield_function,10*1000)
		posBefore = getOneSecondPistonPositionAverage(yield_function) - posStartPressurizing
		parallel.sleep(yield_function, holdTime*1000)
		posAfter = getOneSecondPistonPositionAverage(yield_function) - posStartPressurizing
	else
		posAfter = getOneSecondPistonPositionAverage(yield_function) - posStartPressurizing
		context:Log("Too much air in pump {0}; compression volume: {1} uL", channel, posAfter)
	end
    pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_function)
    return posBefore, posAfter
end


---Degassing both pumps
---@param context IProcedureExecutionContext
---@param min_iterations number
---@param refillVolumeA number
---@param refillVolumeB number
---@param onTimeDegasser number
---@param yield_function function
---@return number|unknown
---@return number|unknown
---@return boolean|unknown
---@return boolean|unknown
function degas(context, min_iterations, refillVolumeA, refillVolumeB, onTimeDegasser, yield_function)
	---@type Pump
    local pump 		= context:GetProcedureParticipant(baltic.GradientPumpRole)
	---@type IProcedureStatusParticipant
	local status 	= context:GetProcedureParticipant(baltic.LcStatusRole)

	---@type Zirconium
    local zr = require "zirconium"

    local degas_zr_A = (pump:GetPistonPosition(zr.A) > refillVolumeA)
    local degas_zr_B = (pump:GetPistonPosition(zr.B) > refillVolumeB)

    context:Log(baltic.devider)
    context:Log("--- Optional Refill of Pump Heads:")
    context:Log("--- initiated by channel A:     {0}, B: {1}", degas_zr_A, degas_zr_B)
    context:Log("--- Piston position channel A:  {0}, B: {1}", pump:GetPistonPosition(zr.A), pump:GetPistonPosition(zr.B))
    context:Log(baltic.devider)

    local method = {}
    method.DegasPressure 		= 50				-- bar
    method.BuildPressureSpeed 	= 300				-- uL/min
    method.HoldTime 			= 30				-- seconds	
    method.MaxBuildVolumeA 		= 50				-- uL
    method.MaxBuildVolumeB 		= 50				-- uL
    method.MinIterations 		= min_iterations
    method.MaxIterations 		= math.max((min_iterations+1),3)

    local function nowTime()
        return os.clock()
    end

    local function sleep_100()
        context:Sleep(100)
    end

	local function empty_pump(channel)
		context:Log("Purge {0}", channel)
		if zr.IsEmpty(pump, channel) then return true end -- don't waste valve switch -- PBNE-647
		zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Waste, nil)
		parallel.sleep(yield_function, 2000)		-- PBNE-693
		pf.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpPurgeSpeed, pump, yield_function)
		while not zr.IsEmpty(pump, channel) do
			if pump:GetCurrentPressure(channel) >= 100 then					-- PBNE-586
				local Text = DotNetString.Format("Pump {0} failed", channel)
				context:Report(Text, Severity.Warn, true, "Pressure exceeded 100 bar while emptying to waste.\n\nLikely causes:\n- Waste tubing is blocked or kinked\n- Pump valve is installed incorrectly or stuck\n- Debris in the waste port\n\nCheck the waste line and verify the pump valve is properly installed.")
				return false
			end 
			yield_function()
		end
		return true
	end
	local function refill(channel)
		context:Log("Refill {0}", channel)
		if zr.IsFull(pump, channel) then return end -- don't waste valve switch
		parallel.sleep(yield_function, 2000)
		zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent, nil)
		pf.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpRefillSpeed, pump, yield_function)
		while not zr.IsFull(pump, channel) do yield_function() end
		parallel.sleep(yield_function, 2000)
	end
    local function degas_channel(channel, max_build_volume)
        local passed = true
		local n = 0
		local posBefore, posAfter = 0, 0
        local Text = "Pump A"
        if channel == zr.B then
            Text = "Pump B"
        end

        repeat
            n = n + 1
            if (n > method.MaxIterations) then return posBefore, passed end
            if not empty_pump(channel) then context:Abort() end			-- PBNE-586
            refill(channel)
			pf.Manualmode_Pump_constantSpeed(channel, 300, pump, yield_function)
			parallel.sleep(yield_function, 2000)
			pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_function)
            parallel.sleep(yield_function, 4000)

            posBefore, posAfter = degas_pump(context, zr, channel, method.DegasPressure, method.HoldTime, false, yield_function)
            context:Log("Degas {0}: Iteration={1}, build pressure vol={2:0.0}, hold pressure vol={3:0.0} ({4} seconds)", channel, n, posBefore, (posAfter-posBefore), method.HoldTime)
        until (n >= method.MinIterations and posBefore <= max_build_volume)
		if (posAfter-posBefore) > 1 then        -- is equal to leakage of >2uL/min 
			local msg = Text..": the leakage rate seems to be too high at low pressure."
            context:Log(msg)
			passed = false
		end
    --		refill(channel, yield_function)			-- refill is not neccessary due to max_build_volume is only 150uL
        return posBefore, passed
    end

    -- start degassing here
    local a, b, passedA, passedB
    if degas_zr_A or degas_zr_B then
		local msg = "Refilling pumps"
		status:SetStatus(msg)
		local settings = pump:GetSettings()
        zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)
        local digOut = pump:GetDigitalOutputs()
        if digOut < 32  then	-- 32 == DigitalOutput.PO2 is already true
            pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
        end
        
        pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 25, 20, sleep_100, baltic.smooth)
             
        -- wait until degasser is on for the time defined in baltic.preOntimeDegasser
        while (nowTime() < ( onTimeDegasser + baltic.preOntimeDegasser)) do yield_function() end

--        context:Signalize(baltic.ColorsRGB.Normal, baltic.SignalizeAll)
    --		if degas_zr_A and degas_zr_B then -- always fill both channels if one is below level
        local p_degas_a = { degas_channel, zr.A, method.MaxBuildVolumeA, parallel.yield }
        local p_degas_b = { degas_channel, zr.B, method.MaxBuildVolumeB, parallel.yield }
        a, passedA, b, passedB = parallel.run(sleep_100, p_degas_a, p_degas_b)
		context:Log("Compression Volume A: [{0}\181L]", a)
		context:Log("Compression Volume B: [{0}\181L]", b)

		zr.setPumpSettings(context, pump, settings)
--        pump:SetSettings(settings)
        local caller = "Degas"
        zr.logPIDs(context, settings.PressurePID, settings.FlowPID, caller)
        zr.logInstrSettings(context, settings, caller)
		status:RemoveStatus(msg)
		-- switch degasser off if it is switched on outside of this function
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
		if (not (a and b)) then
			context:Report("Refilling pumps failed", Severity.Warn, true, "One or both pumps could not be refilled.\n\nCheck that:\n- Solvent reservoirs are not empty\n- Solvent tubings are submerged in liquid\n- No air has entered the solvent lines\n\nRefill the reservoirs and run 'Preparation' again.")
		end
    else
        a, b = 0, 0
		passedA, passedB = true, true
    end
    return a, b, passedA, passedB
end

---This funktion is only used in Preparation and Service
---@param context IProcedureExecutionContext
---@param zr Zirconium
---@param channel Channel
---@param min_iterations number
---@param repeatIfAirDetected boolean
---@param refillVolume number
---@param aspirateSpeed integer?
---@param dispenseSpeed integer?
---@param onTimeDegasser number
---@param yield_function function
---@return number|nil
---@return integer
---@return boolean|nil
function degas_Channel(context, zr, channel, min_iterations, repeatIfAirDetected, refillVolume, aspirateSpeed, dispenseSpeed, onTimeDegasser, yield_function)
	---@type Pump
    local pump 		= context:GetProcedureParticipant(baltic.GradientPumpRole)
    local degas_channel = (pump:GetPistonPosition(channel) > refillVolume)
	local ch = "A"

	if channel == zr.B then ch = "B" end
    context:Log(baltic.devider)
    context:Log("--- Optional Refill of Pump Head {0}:     {1}", ch, degas_channel)
    context:Log("--- Piston position channel {0}:          {1}", ch, pump:GetPistonPosition(channel))
    context:Log(baltic.devider)

    local method = {}
    method.DegasPressure 		= 50				-- bar
    method.BuildPressureSpeed 	= 300				-- uL/min
    method.HoldTime 			= 30				-- seconds	
    method.MaxBuildVolume 		= 50				-- uL
    method.MinIterations 		= min_iterations
    method.MaxIterations 		= math.max((min_iterations+1),10)
	method.AspirateSpeed		= baltic.Settings.GradientPumpRefillSpeed
	method.DispenseSpeed		= baltic.Settings.GradientPumpPurgeSpeed

	if aspirateSpeed then method.AspirateSpeed = aspirateSpeed end
	if dispenseSpeed then method.DispenseSpeed = dispenseSpeed end

	local function empty_pump()
		context:Log("Purge {0}", channel)
		if zr.IsEmpty(pump, channel) then return true end -- don't waste valve switch
		zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Waste, nil)
		parallel.sleep(yield_function, 2000)		-- PBNE-693
		pf.Manualmode_Pump_constantSpeed(channel, method.DispenseSpeed, pump, yield_function)
		while not zr.IsEmpty(pump, channel) do
			if pump:GetCurrentPressure(channel) >= 100 then					-- PBNE-586
				local Text = DotNetString.Format("Pump {0} failed", channel)
				context:Report(Text, Severity.Warn, true, "Pressure exceeded 100 bar while emptying to waste.\n\nLikely causes:\n- Waste tubing is blocked or kinked\n- Pump valve is installed incorrectly or stuck\n- Debris in the waste port\n\nCheck the waste line and verify the pump valve is properly installed.")
				return false
			end 
			yield_function()
		end
		return true
	end
	local function refill()
		context:Log("Refill {0}", channel)
		if zr.IsFull(pump, channel) then return end -- don't waste valve switch
		parallel.sleep(yield_function, 2000)
		zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent, nil)
		pf.Manualmode_Pump_constantSpeed(channel, method.AspirateSpeed, pump, yield_function)
		while not zr.IsFull(pump, channel) do yield_function() end
		parallel.sleep(yield_function, 2000)
	end
	---Degas the channel
	---@param max_build_volume number
	---@return number
	---@return integer
	---@return boolean
    local function degas_ch(max_build_volume)
        local passed = true
		local checkForAir = false
        local degasEnabled = false
		local repetitionCycle = 0
		local repeatCycle = false
		local n = 0
        local posBefore = 0
        local posAfter = 0
        local Text = "Pump A"
        if channel == zr.B then
            Text = "Pump B"
        end

--        if not empty_pump(channel, yield_function) then context:Abort() end			-- PBNE-586
        -- wait until degasser is on for the time defined in baltic.preOntimeDegasser
        while (os.clock() < ( onTimeDegasser + baltic.preOntimeDegasser)) do yield_function() end

        repeat
            n = n + 1
            if (n > method.MaxIterations) then return 0, 0, false end
            if (n == method.MinIterations) then
                checkForAir = true
                degasEnabled = true
            end
--            if n > 1 then
				if not empty_pump() then context:Abort() end			-- PBNE-586
--			end
            refill()
            if degasEnabled then                            -- PBNE-976 degas the pump only after the last refill
                degasEnabled = false
				pf.Manualmode_Pump_constantSpeed(channel, 300, pump, yield_function)
				parallel.sleep(yield_function, 2000)
				pf.Manualmode_Pump_constantSpeed(channel, 0, pump, yield_function)
				parallel.sleep(yield_function, 4000)
				context:Log("Checking for air in pump {0}: {1}", channel, checkForAir)
                posBefore, posAfter = degas_pump(context, zr, channel, method.DegasPressure, method.HoldTime, checkForAir, yield_function)

				if (posBefore > 0) then
					context:Log("Degas {0}: Iteration={1}, build pressure vol={2:0.0}, hold pressure vol={3:0.0}", channel, (repetitionCycle+1), posBefore, posAfter-posBefore)
					if (posAfter-posBefore) > 1 then repeatCycle = repeatIfAirDetected end
				else
					context:Log("Degas {0}: Iteration={1}, build pressure vol={2:0.0}", channel, (repetitionCycle+1), posBefore)
					repeatCycle = repeatIfAirDetected
				end
				if repeatCycle then		-- run up to 5 degas cycles
					repeatCycle = false
				    n = 0
				    repetitionCycle = repetitionCycle + 1
				    if (repetitionCycle < 5) then
						context:Log("Degas pump {0}: there is too much air in the pump. Starting a new degas cycle.", channel)
					end
--				    if not empty_pump(channel, yield_function) then context:Abort() end			-- PBNE-586
			    end
		    end
        until ((n >= method.MinIterations and posBefore <= max_build_volume) or (repetitionCycle >= 5))

		if (posAfter-posBefore) > 1 then        -- is equal to leakage of >2uL/min 
-- PBNE-741 =>
			local msg = Text..": the leakage rate seems to be too high at low pressure."
            context:Log(msg)
--          context:Report(Text, Severity.Warn, true, "The leakage rate seems to be too high at low pressure.")
-- PBNE-741 <=
			passed = false
		end
    --		refill(channel, yield_function)			-- refill is not neccessary due to max_build_volume is only 150uL
        return posBefore, repetitionCycle, passed
    end

    -- start degassing here
    local vol, reps, passed
    if degas_channel then
        local digOut = pump:GetDigitalOutputs()
        if digOut < 32  then	-- 32 == DigitalOutput.PO2 is already true
            pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
        end

--		context:Signalize(baltic.ColorsRGB.Normal, baltic.SignalizeAll)
--		if degas_zr_A and degas_zr_B then -- always fill both channels if one is below level
        vol, reps, passed = degas_ch(method.MaxBuildVolume)
		context:Log("Compression Volume {0} {1}[\181L]:", ch, vol)
		if not vol then
			context:Report("Refilling pump failed", Severity.Error, true, "The pump could not be refilled.\n\nCheck that:\n- The solvent reservoir is not empty\n- The solvent tubing is submerged in liquid\n- No air has entered the solvent line\n\nRefill the reservoir and retry the procedure.")
		end
    else
        vol = 0
		passed = true
    end
    return vol, reps, passed
end
