local Date = "2024/10/10"

local P = {}

---Clean the injector and the loop (25µL organic then 500µL water) and close the buerkert valve
---@param execLeft IPalParticipant
---@param execAux IPalParticipant
---@param pp PalPlus
---@param moveToInjector boolean
---@param leaveInjector boolean
function P.queue_clean_injector_loop(execLeft, execAux, pp, moveToInjector, leaveInjector)
	local baltic = require "baltic"
	local pr = require "PreRunFunctions"
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)

	pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)
	-- clean loop using solvent 2 and 1
	pp.CleanInjector(execLeft, execAux, pp.Organic, 25, moveToInjector, false)		-- IdexFix05
	pp.CleanInjector(execLeft, execAux, pp.Aqueous, 500, false, leaveInjector)		-- IdexFix05
	if leaveInjector == false then
		-- close the buerkert valve, otherwise it is closed in the CleanInjector function
		execLeft:Wait(pp.Quantity(1000, "ms"))
		pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
	end
end

---Clean the injector without the loop (25µL organic then 500µL water) and close the buerkert valve
---@param execLeft IPalParticipant
---@param execAux IPalParticipant
---@param pp PalPlus
---@param moveToInjector boolean
---@param leaveInjector boolean
function P.queue_clean_injector(execLeft, execAux, pp, moveToInjector, leaveInjector)
	-- clean injector using solvent 2 and 1
	pp.CleanInjector(execLeft, execAux, pp.Organic, 25, moveToInjector, false)
	pp.CleanInjector(execLeft, execAux, pp.Aqueous, 500, false, leaveInjector)
	if leaveInjector == false then
		-- close the buerkert valve, otherwise it is closed in the CleanInjector function
		execLeft:Wait(pp.Quantity(1000, "ms"))
		pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
	end
end
--[[
---Clean the syringe twice with organic then 3 times with water
---@param execLeft IPalParticipant
---@param execAux IPalParticipant
---@param pp PalPlus
function P.queue_clean_syringe(execLeft, execAux, pp)
	-- clean Syringe with organic and aqueous
	pp.CleanSyringe(execLeft, execAux, pp.Organic, 2)
	pp.CleanSyringe(execLeft, execAux, pp.Aqueous, 3)
end
--]]
return P