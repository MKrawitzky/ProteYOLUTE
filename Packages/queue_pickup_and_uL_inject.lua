-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/03/17"

local P = {}

---Aspirate additional liquid from a vial
---@param context IProcedureExecutionContext
---@param pp PalPlus
---@param execLeft IPalParticipant
---@param execAux IPalParticipant
---@param needleHeight number
---@param drawSpeed number
---@param pauseDraw Quantity
---@param airGap number
---@return number
local function additionalAspirationFromVial(context, pp, execLeft, execAux, needleHeight, drawSpeed, pauseDraw, airGap)
	local baltic = require "baltic"
	local extraVolume = 0
	local sample_position = "Slot"..context:GetArgumentValue("Add From Vials", "Tray")..":1"
	local container = pp.QueryModule(execAux, pp.Capabilities.TrayContainer)
	local Vial = execLeft.ConfigurationService:GetVial(container.Name..":"..sample_position)
	local firstVial = context:GetArgumentValue("Add From Vials", "1st Vial")
	local secondVial = context:GetArgumentValue("Add From Vials", "2nd Vial")
	local thirdVial = context:GetArgumentValue("Add From Vials", "3rd Vial")
	local firstVolume = context:GetArgumentValue("Add From Vials", "1st Volume")
	local secondVolume = context:GetArgumentValue("Add From Vials", "2nd Volume")
	local thirdVolume = context:GetArgumentValue("Add From Vials", "3rd Volume")

	context:Log(baltic.devider)
	context:Log("  Add from vial parameter:")
	context:Log("    Tray:                        {0}",Vial.Tray)
	context:Log("    Penetration depth:           {0} mm",context:GetArgumentValue("Add From Vials", "Penetration Depth"))
	context:Log("    Aspiration post delay:       {0} ms",pauseDraw)
	context:Log("    Needle height:               {0} mm",needleHeight)
	context:Log("    Draw speed:                  {0} uL/s",drawSpeed)
	context:Log("    Air gap:                     {0} uL",airGap)
	context:Log(baltic.devider)

	---Add liquid from a vial
	---@param tray any
	---@param vial any
	---@param volume number
	---@param leaveDrawerOpen boolean
	local function addFromVial(tray, vial, volume, leaveDrawerOpen)
--		execLeft:Wait(pp.Quantity("5000 ms"))
		-- clean the needle outside not to contaminate the following vial content
		pp.CleanNeedle(execLeft, execAux, airGap, drawSpeed)
		execLeft:MoveToObject( tray, vial, true, false, true )	-- leave drawer open
		if context:GetArgumentValue("Add From Vials", "Penetration Depth") == 0.0 then
			execLeft:PenetrateWithBottomSense( tray, vial, pp.Quantity(needleHeight, "mm"), nil)
		else
			execLeft:PenetrateObject( tray, vial, pp.Quantity(context:GetArgumentValue("Add From Vials", "Penetration Depth"), "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
		end
		execLeft:AspirateSyringe( pp.Quantity(volume, "uL"), pp.Quantity(drawSpeed, "uL/s"), nil, pauseDraw)
		execLeft:LeaveObject(pp.Quantity("50 mm"), leaveDrawerOpen, false)
		while not execLeft.IsIdle do
			context:Sleep(250)
		end
	end

	if firstVial >= 1 then
		context:Log("    1st vial:                    {0}",firstVial)
		context:Log("    1st volume:                  {0} uL",firstVolume)
		local leaveDrawerOpen = (secondVial >= 1) or (thirdVial >= 1)		-- don't close the drawer if an additional volume is followed
		addFromVial(Vial.Tray, context:GetArgumentValue("Add From Vials", "1st Vial"), firstVolume, leaveDrawerOpen)
		extraVolume = extraVolume + firstVolume + airGap
	end
	if secondVial >= 1 then
		context:Log("    2nd vial:                    {0}",secondVial)
		context:Log("    2nd volume:                  {0} uL",secondVolume)
		local leaveDrawerOpen = (thirdVial >= 1)		-- don't close the drawer if an additional volume is followed
		addFromVial(Vial.Tray, context:GetArgumentValue("Add From Vials", "2nd Vial"), secondVolume, leaveDrawerOpen)
		extraVolume = extraVolume + secondVolume + airGap
	end
	if thirdVial >= 1 then
		context:Log("    3rd vial:                    {0}",thirdVial)
		context:Log("    3rd volume:                  {0} uL",thirdVolume)
		local leaveDrawerOpen = false
		addFromVial(Vial.Tray, context:GetArgumentValue("Add From Vials", "3rd Vial"), thirdVolume, leaveDrawerOpen)
		extraVolume = extraVolume + thirdVolume + airGap
	end
	return extraVolume
end

-- injection valve must be in the inject position before calling this function
---Picking-up the sample, dispense part of the post airgap
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param isParallel boolean
---@param execLeft IPalParticipant
---@param execAux IPalParticipant
---@param pp PalPlus
---@param wash_module IModule
---@param sleeping_yield function
---@return number
---@return number
function P.queue_pickup_and_uL_inject(installed, context, isParallel, execLeft, execAux, pp, wash_module, sleeping_yield)
	local baltic = require "baltic"
	local pr = require "PreRunFunctions"

	local tool = pp.QueryModule(execAux, pp.Capabilities.IToolLc)
	local injector = pp.QueryModule(execAux, pp.Capabilities.IInjector)
	local delay = pp.Quantity(context:GetArgumentValue("sample_postaspirate_delay"), "ms")
	local detergent_depth = pp.Quantity(baltic.WashSolventLinerPenetrationDepth, "mm")
	local itemPosition = pp.QueryModules(execAux, "ItemPositionDescription")
	local transportLiquid_3 = itemPosition[pr.GetItemPosIdx(3, execAux)]
	local sample_position = context:GetArgumentValue("sample_position")
	local container = pp.QueryModule(execAux, pp.Capabilities.TrayContainer)
	local vial = execLeft.ConfigurationService:GetVial(container.Name..":"..sample_position)
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)

	local needleHeight = 1
	local drawSpeed   = 1
	local pauseDraw   = 0
	local preAirGap   = 0.5
	local postAirGap  = 1

	if installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then
		if context:GetArgumentValue("Injection Settings") == true then
			needleHeight = context:GetArgumentValue("Injection Settings", "Needle Height")		-- [mm]
			drawSpeed   = context:GetArgumentValue("Injection Settings", "Draw Speed")			-- [uL/s]
			pauseDraw   = context:GetArgumentValue("Injection Settings", "Pause Draw")*1000		-- [ms]
			delay = delay + pauseDraw
		end
		if context:GetArgumentValue("Air Gaps") == true then
			preAirGap   = context:GetArgumentValue("Air Gaps", "Pre Sample Air Gap")	-- [uL]
			postAirGap  = context:GetArgumentValue("Air Gaps", "Post Sample Air Gap")	-- [uL]
		end
	end
--	local washVial    = context:GetArgumentValue("  Position")
	local extraSampleVolume = 0

	execLeft:ChangeTool( tool )

	if (context:GetArgumentValue("sample_volume")> 0) then
		-- get Solvent
		context:Log("Sample pickup and \181L injection")
		if context:GetArgumentValue("Alternative Transport Liquid") == true then
			context:Log("Take transport liquid from volumetric wash pump")
			local washpump = pp.QueryModule(execAux, pp.Capabilities.IPumpModule)
			execLeft:MoveToObject( wash_module, pp.Waste, true, true, true )
			execLeft:PenetrateObject( wash_module, pp.Waste, pp.Quantity(30, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
			pp.SetLCToolValve(execLeft, pp.LcToolValveOpen)
			-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
			execLeft:SetPump( washpump, pp.Aqueous, pp.Quantity(150, "uL"), pp.Quantity(30, "uL/s"), nil, true, false)
			execLeft:Wait(pp.Quantity(250, "ms"))
			execLeft:AspirateSyringe( pp.Quantity(context:GetArgumentValue("presample_solvent_volume"), "uL"), pp.Quantity(1, "uL/s"), nil, pp.Quantity(500, "ms"))
			execLeft:Wait(pp.Quantity(200, "ms"))
			-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
			execLeft:SetPump( washpump, pp.Aqueous, pp.Quantity(0, "uL"), pp.Quantity(30, "uL/s"), nil, false, true)
			pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
		else
			context:Log("Take transport liquid from item position 3")
			execLeft:MoveToObject( transportLiquid_3,1, true, true, true )
			execLeft:PenetrateWithBottomSense( transportLiquid_3, 1, pp.Quantity(1, "mm"), nil, nil)
			execLeft:AspirateSyringe( pp.Quantity(context:GetArgumentValue("presample_solvent_volume"), "uL"), pp.Quantity(drawSpeed, "uL/s"), nil, delay)
		end
		execLeft:LeaveObject()
		-- get Rear Airgap: Between Solvent and Sample
		execLeft:AspirateSyringe( pp.Quantity(preAirGap, "uL"), pp.Quantity(drawSpeed, "uL/s"), nil, delay)
		-- get sample
		execLeft:MoveToObject( vial.Tray, vial.Index, true, true, true )
		if context:GetArgumentValue("is_bottom_sense") then
			execLeft:PenetrateWithBottomSense( vial.Tray, vial.Index, pp.Quantity(needleHeight, "mm"), nil)
		else
			execLeft:PenetrateObject( vial.Tray, vial.Index, pp.Quantity(context:GetArgumentValue("penetration_depth"), "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
		end
		execLeft:AspirateSyringe(pp.Quantity(context:GetArgumentValue("sample_volume"), "uL"), pp.Quantity(drawSpeed, "uL/s"), nil, delay)


		if (installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition) and (context:GetArgumentValue("Add From Vials") == true) then
			execLeft:LeaveObject(nil, true, false)	-- leave drawer open
			extraSampleVolume = additionalAspirationFromVial(context, pp, execLeft, execAux, needleHeight, drawSpeed, delay, preAirGap)
		else
			execLeft:LeaveObject(pp.Quantity("50 mm"), false, false)	-- close drawer
		end
		if isParallel == true then
			local parallel = require "parallel"
			while not execLeft.IsIdle do
				parallel.sleep(sleeping_yield, 200)
			end
		end
		-- get Front Airgap
		execLeft:AspirateSyringe( pp.Quantity(postAirGap, "uL"), pp.Quantity(drawSpeed, "uL/s"), nil, delay)
		-- dip needle exterior in Solvent 1 Wash Module
		execLeft:MoveToObject( wash_module, pp.Aqueous, true, true, true )
		for _=1,2 do
			execLeft:PenetrateObject(wash_module, pp.Aqueous, detergent_depth, pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
			execLeft:Depenetrate()
		end
		execLeft:LeaveObject()

		-- Inject Sample
		execLeft:MoveToObject(injector)
		execLeft:DispenseSyringe( pp.Quantity(postAirGap*0.75, "uL"), pp.Quantity(context:GetArgumentValue("sample_inject_speed"), "uL/s"))
		if isParallel == true then
			local parallel = require "parallel"
			while not execLeft.IsIdle do
				parallel.sleep(sleeping_yield, 200)
			end
		end
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)			-- PNS-92
		pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Blue, execAux, baltic.InjectionValve.Inject)
		local afterLoopToInject = context:GetArgumentValue("Injection Delays", "After Loop to Inject")
		execLeft:Wait(pp.Quantity(afterLoopToInject, "ms"))
		execLeft:PenetrateWithConstForce( injector )
		local afterPenetration = context:GetArgumentValue("Injection Delays", "After Penetration")
		execLeft:Wait(pp.Quantity(afterPenetration, "ms"))
		local sampleVolume = context:GetArgumentValue("sample_volume")+extraSampleVolume
		return sampleVolume, postAirGap*0.25+preAirGap+1
	else
		return 0,0
	end
end

return P