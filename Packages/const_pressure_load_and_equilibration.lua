local Date = "2025/07/23"

local P = {}

---Equilibration or loading using constant pressure and predicted flow
---@param pump Pump
---@param channel Channel
---@param pressure number
---@param volume number
---@param predictedFlow number
---@param yield_function function
---@param timeout number
---@param prePumping boolean
---@return number
---@return number
---@return number
---@return number
---@return string
---@return string
function P.const_pressure_load_and_equilibration(pump, channel, pressure, volume, predictedFlow, yield_function, timeout, prePumping)
	local pf = require "pump_functions"

	local err = ""
	local msg = ""

	pf.Manualmode_Pump_constantPressure(channel, pressure, pump, yield_function)
	-- pump until within 1 bar of pressure
	local acLoadTime = pf.now()
	local usedVolume = 0
	local timeOutPressurize = 300		-- fixed timeout for pressurizing is 5 minutes
	local minPressure = pressure - 10
	if (predictedFlow > 25) then minPressure = pressure - 30 end

	usedVolume, err, msg = pf.pump_until(pump, channel, function() local isPressureReached = pump:GetCurrentPressure(channel) > minPressure return isPressureReached end, yield_function, timeOutPressurize)
	local duration = pf.now() - acLoadTime		-- for pressure build up
	local flow = usedVolume / duration * 60		-- [uL\min]
	if ( err ~= "" ) then
		msg = msg..": pump pressure is unable to reach target pressure, check for trapped air, leakage, or damaged column"
		local actPressure = pump:GetCurrentPressure(channel)
		return actPressure, usedVolume, flow, duration, err, msg
	end

	if timeout>0 then
		acLoadTime = pf.now()
		local preUsedVolume = 0
		local prePumpingTime = 0
		local loadToTime = 60*volume / predictedFlow -- [s]
		local startVolume = pump:GetPistonPosition(channel)
		usedVolume = 0

		-- 30 seconds pre pumping to get a more accurate flow value
		if prePumping then
			prePumpingTime = 30		-- 30 seconds
			if loadToTime < 60 then loadToTime = 60 end		-- minimum pump time is 60 seconds (loadToTime + prePumpingTime)
			local prePumpingTimeEnd = prePumpingTime + pf.now()
			-- pump until prePumpingTime has passed
			preUsedVolume, err, msg = pf.pump_until(pump, channel, function() local isLeadTimeExpired = pf.now() >= prePumpingTimeEnd return isLeadTimeExpired end, yield_function, timeout)
			if ( err ~= "" ) then
				if msg == "Pump exceeded time limit" then
					msg = msg..": check for blocked column"
				else
					msg = msg..": check for trapped air, leakage, or damaged column"
				end
				duration = pf.now() - acLoadTime		-- for pre pumping
				flow = preUsedVolume / duration * 60		-- [uL\min]
				return pump:GetCurrentPressure(channel), preUsedVolume, flow, duration, err, msg
			end
		end

		local pumpTime2 = loadToTime - prePumpingTime + pf.now()
		-- pump until pumpTime2 has passed
		usedVolume, err, msg = pf.pump_until(pump, channel, function() local isTimeExpired = pf.now() >= pumpTime2 return isTimeExpired end, yield_function, timeout)
		if ( err ~= "" ) then
			if msg == "Pump exceeded time limit" then
				msg = msg..": check for blocked column"
			else
				msg = msg..": check for trapped air, leakage, or damaged column"
			end
			duration = pf.now() - acLoadTime
			flow = usedVolume / duration * 60		-- [uL\min]
			return pump:GetCurrentPressure(channel), (usedVolume + preUsedVolume), flow, (duration + prePumpingTime), err, msg
		end
		flow = usedVolume / (loadToTime - prePumpingTime) * 60		-- [uL\min]

--		usedVolume = usedVolume + preUsedVolume
		if (usedVolume + preUsedVolume < volume) then
			local vol = 0
			vol, err, msg = pf.pump_until(pump, channel, function() local isVolumeUsed = pump:GetPistonPosition(channel) - startVolume >= volume return isVolumeUsed end, yield_function, timeout)
			usedVolume = usedVolume + vol
		end
		duration = pf.now() - acLoadTime

		while (pf.now() < loadToTime) do
			yield_function()
		end
		local actPressure = pump:GetCurrentPressure(channel)
		return actPressure, usedVolume + preUsedVolume, flow, duration, err, msg
	else
		local actPressure = pump:GetCurrentPressure(channel)
		return actPressure, 0, 0, 0, err, msg
	end
end

return P