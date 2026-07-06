-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/05/05"

local P = {}

---Clean syringe and pick-up the sample
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param isParallel boolean
---@param sleeping_yield function
---@return number
---@return number
function P.injectSample(installed, context, isParallel, sleeping_yield)
	if (context:GetArgumentValue("sample_volume") > 0) then
		local baltic = require "baltic"
		local pp = require "palplus"
		---@type IPalParticipant
		local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
		---@type IPalParticipant
		local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
		---@type IProcedureStatusParticipant
		local status = context:GetProcedureParticipant(baltic.LcStatusRole)
		local wash_module = pp.QueryModule(execAux, pp.Capabilities.ILcMsWashStation)
		local depth = pp.Quantity(baltic.WashWasteLinerPenetrationDepth,"mm")
	    local preSampleVolume = 0
	    local sampleVolume = 0
		status:SetStatus(baltic.Status.Inject)

		pp.EmptySyringe_And_LeaveObject(context, execLeft, execAux, depth, installed.SyringeZeroPosition)
		if context:GetArgumentValue("Pre Injection Needle Wash") then
			context:Log("Pre injection needle wash")
			pp.PreInjectionNeedleWash(context, execLeft)
		end

		-- Pickup sample
		if ( context:GetArgumentValue("injection_method") == "uLPickup" ) then
			local qpaui = require "queue_pickup_and_uL_inject"
			sampleVolume, preSampleVolume = qpaui.queue_pickup_and_uL_inject(installed, context, isParallel, execLeft, execAux, pp, wash_module, sleeping_yield)
		else
			local qpapi = require "queue_pickup_and_partial_inject"
			sampleVolume, preSampleVolume = qpapi.queue_pickup_and_partial_inject(installed, context, isParallel, execLeft, execAux, pp, wash_module, sleeping_yield)
		end
	    return sampleVolume, preSampleVolume
	end
	return 0,0
end

---Dispense the sample into the loop (thwe loop must already bein "Inject" postion)
---@param context IProcedureExecutionContext
---@param pp PalPlus
---@param execAux IPalParticipant
---@param execLeft IPalParticipant
---@param sampleVolumeToBeInjected number
---@param preSampleVolume number
function P.injectIntoLoop(context, pp, execAux, execLeft, sampleVolumeToBeInjected, preSampleVolume)
	local injectionVolume = sampleVolumeToBeInjected

    context:Log("Inject sample")
	if ( context:GetArgumentValue("injection_method") == "uLPickup" ) then
		-- Transfer PostAirGap *0.25 + Sample + PreSampleAir + 1 ul of Transport Liquid
		injectionVolume = injectionVolume + preSampleVolume
	end
	execLeft:DispenseSyringe( pp.Quantity(injectionVolume, "uL"), pp.Quantity(context:GetArgumentValue("sample_inject_speed"), "uL/s"))
	local afterDispense = context:GetArgumentValue("Injection Delays", "After Dispense")
	execLeft:Wait(pp.Quantity(afterDispense, "ms"))
end

return P
