-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/05/05"

local P = {}

---Dissolve and mix a sample
---@param context IProcedureExecutionContext
---@param sleeping_yield function
---@return number
---@return number
function P.dissolveAndMixSample(context, sleeping_yield)
	local baltic =	require "baltic"
	local pp = 		require "palplus"
	local pr =		require "PreRunFunctions"

	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
    local wash_module = pp.QueryModule(execAux, pp.Capabilities.ILcMsWashStation)

	local mixCycles = context:GetArgumentValue("Dissolve Sample", "Mixing Cycles")
	local mixSpeed = context:GetArgumentValue("Dissolve Sample", "Mixing Speed")
	local solventVialPenetrationDepth = context:GetArgumentValue("Dissolve Sample", "Solvent vial penetration depth")
	local solventVolume = context:GetArgumentValue("Dissolve Sample", "Solvent Volume")
	local solventReleaseSpeed = context:GetArgumentValue("Dissolve Sample", "Solvent Release Speed")
	local timeToDissolve = context:GetArgumentValue("Dissolve Sample", "Time to Dissolve")
	local mixSampleVolume = context:GetArgumentValue("Dissolve Sample", "Mixing Volume")
	local heightFromBottomSampleVial = context:GetArgumentValue("Dissolve Sample", "Needle Height for Mixing")
	local rearAirGap = context:GetArgumentValue("rearAirGap")
	local sampleInjectionAspirateFlowRate = context:GetArgumentValue("sampleInjectionAspirateFlowRate")
	local sampleVialPenetrationDepth = context:GetArgumentValue("Dissolve Sample", "Sample Vial Penetration Depth")
	local pullupDelay = context:GetArgumentValue("pullupDelay")
	local dispenseDelay = context:GetArgumentValue("dispenseDelay")
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local itemPosition = pp.QueryModules(execAux, "ItemPositionDescription")
	local solventVial = itemPosition[pr.GetItemPosIdx(context:GetArgumentValue("Dissolve Sample", "Dissolve solvent position"), execAux)]
	local transportLiquid_3 = itemPosition[pr.GetItemPosIdx(3, execAux)]
	local injector = pp.QueryModule(execAux, pp.Capabilities.IInjector)
	local sample_position = context:GetArgumentValue("sample_position")
	local container = pp.QueryModule(execAux, pp.Capabilities.TrayContainer)
	local vial = execLeft.ConfigurationService:GetVial(container.Name..":"..sample_position)
	local sampleVolume = context:GetArgumentValue("sample_volume")
	local speed = pp.Quantity(context:GetArgumentValue("sample_aspirate_speed"), "uL/s")
	local delay = pp.Quantity(context:GetArgumentValue("sample_postaspirate_delay"), "ms")
	local depth = pp.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)

	local needleHeight = 1
	local pauseDraw   = 0
	local preAirGap   = 0.5
	local postAirGap  = 1

	if context:GetArgumentValue("Injection Settings") == true  then
		needleHeight = context:GetArgumentValue("Injection Settings", "Needle Height")		-- [mm]
		pauseDraw   = context:GetArgumentValue("Injection Settings", "Pause Draw")*1000		-- [ms]
		delay = delay + pauseDraw
	end
	if context:GetArgumentValue("Air Gaps") == true  then
		preAirGap   = context:GetArgumentValue("Air Gaps", "Pre Sample Air Gap")	-- [uL]
		postAirGap  = context:GetArgumentValue("Air Gaps", "Post Sample Air Gap")	-- [uL]
	end
	context:Log(baltic.devider)
	context:Log("  Dissolve common parameter:")
	context:Log("    Solvent vial:                        {0}",context:GetArgumentValue("Dissolve Sample", "Dissolve solvent position"))
	context:Log("    Solvent vial penetration depth:      {0}",solventVialPenetrationDepth)
	context:Log("    Sample position:                     {0}",sample_position)
	context:Log("    Sample aspiration speed:             {0} uL/s",speed)
	context:Log("    Sample post aspirate delay:          {0} ms",delay)
	context:Log("    Pre sample air gap:                  {0} uL",preAirGap)
	context:Log("    Post sample air gap:                 {0} uL",postAirGap)
	context:Log("    Rear air gap:                        {0} uL",rearAirGap)
	context:Log("    Needle height:                       {0} mm",needleHeight)
--	context:Log("    Draw speed:                          {0}",drawSpeed)					-- unused
--	context:Log("    Pause draw:                          {0}",pauseDraw)					-- unused
	context:Log("  Dissolve mixing parameter:")
	context:Log("    Solvent volume:                      {0} uL",solventVolume)
	context:Log("    Solvent release speed:               {0} uL/s",solventReleaseSpeed)
	context:Log("    Mixing volume:                       {0} uL",mixSampleVolume)
	context:Log("    Mixing cycles:                       {0}",mixCycles)
	context:Log("    Mixing speed:                        {0} uL/s",mixSpeed)
	context:Log("    Needle height for mixing:            {0} mm",heightFromBottomSampleVial)
	context:Log("    Time to dissolve:                    {0} s",timeToDissolve)
	context:Log("  Dissolve sample parameter:")
	context:Log("    Sample injection aspirate flow rate: {0} uL/s",sampleInjectionAspirateFlowRate)
	context:Log("    Sample vial penetration depth:       {0} mm",sampleVialPenetrationDepth)
--	context:Log("    Bottom sense sample vial:            {0}",bottomSenseSampleVial)		-- unused
	context:Log("    Pullup delay:                        {0} ms",pullupDelay)
	context:Log("    Dispense delay:                      {0} s",dispenseDelay)
	context:Log(baltic.devider)

	--clean syringe and needle
	context:Log("Cleaning syringe and needle exterior")
	pp.PrimeLCPToolLoop(execLeft, true, false)
	execLeft:LeaveObject()
	-- get Rear Airgap: Between Solvent and Sample
	status:SetStatus("Aspirate solvent")
	execLeft:AspirateSyringe( pp.Quantity(rearAirGap, "uL"), pp.Quantity(sampleInjectionAspirateFlowRate, "uL/s"), nil, pp.Quantity(pullupDelay, "ms"))		
	-- pick up Rear Air Gap
	execLeft:MoveToObject( solventVial, 1, true, true, true )
	if (solventVialPenetrationDepth == 0.0) then
		-- needle lifting up 1 mm after bottom is sensed
		execLeft:PenetrateWithBottomSense( solventVial, 1, pp.Quantity(1, "mm"))
	else
		execLeft:PenetrateObject( solventVial, 1, pp.Quantity(solventVialPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
	end
	execLeft:AspirateSyringe( pp.Quantity(solventVolume, "uL"), pp.Quantity(sampleInjectionAspirateFlowRate, "uL/s"), nil, pp.Quantity(pullupDelay, "ms"))
	execLeft:LeaveObject()
	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus("Aspirate solvent")

	local statusText = "Dissolve sample"
	status:SetStatus(statusText)
	execLeft:MoveToObject( vial.Tray, vial.Index )
	if sampleVialPenetrationDepth == 0.0 then
		-- needle lifting up after bottom is sensed
		execLeft:PenetrateWithBottomSense( vial.Tray, vial.Index, pp.Quantity(needleHeight, "mm"))
	else
		execLeft:PenetrateObject( vial.Tray, vial.Index, pp.Quantity(sampleVialPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
	end
	execLeft:DispenseSyringe( pp.Quantity(solventVolume, "uL"), pp.Quantity(solventReleaseSpeed, "uL/s"))
	execLeft:LeaveObject(pp.Quantity("50 mm"), true, false)
	execLeft:Wait(pp.Quantity(timeToDissolve, "s"))
	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus(statusText)
	statusText = "Mixing sample " .. mixCycles .. " times"
	status:SetStatus(statusText)
	execLeft:MoveToObject( vial.Tray, vial.Index )
	if sampleVialPenetrationDepth == 0.0 then
		-- needle lifting up after bottom is sensed
		execLeft:PenetrateWithBottomSense( vial.Tray, vial.Index, pp.Quantity(heightFromBottomSampleVial, "mm"))
	else
		execLeft:PenetrateObject( vial.Tray, vial.Index, pp.Quantity(sampleVialPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
	end

	for i=1, mixCycles do
		execLeft:AspirateSyringe( pp.Quantity(mixSampleVolume, "uL"), pp.Quantity(mixSpeed, "uL/s"))
		execLeft:DispenseSyringe( pp.Quantity(mixSampleVolume, "uL"), pp.Quantity(mixSpeed, "uL/s"))
		execLeft:Wait(dispenseDelay)
	end
	execLeft:LeaveObject(nil, true)		-- leave drawer open
	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus(statusText)

	--clean syringe and needle
	statusText = "Clean syringe and aspirate transport liquid"
	status:SetStatus(statusText)
	pp.PrimeLCPToolLoop(execLeft, true, false)
	execLeft:LeaveObject()
	execLeft:MoveToObject( transportLiquid_3,1, true, true, true )
	execLeft:PenetrateWithBottomSense( transportLiquid_3, 1, pp.Quantity(1, "mm"), nil, nil)
	execLeft:AspirateSyringe( pp.Quantity(2, "uL"), pp.Quantity(1, "uL/s"), nil, pp.Quantity(pullupDelay, "ms"))
	execLeft:LeaveObject()
	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus(statusText)

	-- draw diluted sample volume
	statusText = "Aspirate dissolved sample"
	status:SetStatus(statusText)
	execLeft:AspirateSyringe( pp.Quantity(postAirGap, "uL"), speed, nil, pp.Quantity(pullupDelay, "ms"))		
	execLeft:MoveToObject( vial.Tray, vial.Index )
	if sampleVialPenetrationDepth == 0.0 then
		-- needle lifting up 1 mm after bottom is sensed
		execLeft:PenetrateWithBottomSense( vial.Tray, vial.Index, pp.Quantity(heightFromBottomSampleVial, "mm"))
	else
		execLeft:PenetrateObject( vial.Tray, vial.Index, pp.Quantity(sampleVialPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
	end
	execLeft:AspirateSyringe(pp.Quantity(sampleVolume, "uL"), speed, nil, pp.Quantity(pullupDelay, "ms"))
	execLeft:LeaveObject()		-- close drawer
	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus(statusText)
	statusText = "Inject dissolved sample"
	status:SetStatus(statusText)
	-- get Front Airgap
	execLeft:AspirateSyringe( pp.Quantity(preAirGap, "uL"), speed, nil, pp.Quantity(0, "ms"))
	-- dip needle exterior in Solvent 1 Wash Module
	execLeft:MoveToObject( wash_module, pp.Aqueous, true, true, true )
	execLeft:PenetrateObject(wash_module, pp.Aqueous, depth, pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
	execLeft:Wait(pp.Quantity("5000 ms"))
	execLeft:LeaveObject()

	execLeft:MoveToObject(injector)
	-- expulse part of the post sample air volume that was necessary to protect the sample during exernal needle wash
	execLeft:DispenseSyringe( pp.Quantity(preAirGap*0.75, "uL"), pp.Quantity(context:GetArgumentValue("sample_inject_speed"), "uL/s"))
	pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)			-- PNS-92
	local afterLoopToInject = context:GetArgumentValue("Injection Delays", "After Loop to Inject")
	execLeft:Wait(pp.Quantity(afterLoopToInject, "ms"))
	execLeft:PenetrateWithConstForce( injector )
	local afterPenetration = context:GetArgumentValue("Injection Delays", "After Penetration")
	execLeft:Wait(pp.Quantity(afterPenetration, "ms"))

	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus(statusText)
	return sampleVolume, (preAirGap*0.25)+postAirGap+1

end

---Derivatize a sample
---@param context IProcedureExecutionContext
---@param sleeping_yield function
---@return number
---@return number
function P.derivatize_and_inject_Sample(context, sleeping_yield)
	local baltic =	require "baltic"
	local pp = 		require "palplus"
	local pr =		require "PreRunFunctions"

	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)

	local D_position = nil
	local D_container = nil
	local D_vial = nil

	local mixCycles = nil
	local mixSpeed = 5
	local timeToDerivatize = 10000
	local mixingVolume = 10
	local heightFromBottomSampleVial = 1
	local dispenseDelay = 1000
	local airGap = 0.0
	local preAirGap = 0.0

	if context:GetArgumentValue("Air Gaps") == true then
		preAirGap = context:GetArgumentValue("Air Gaps", "Pre Sample Air Gap")	-- [uL]
	else
		preAirGap = context:GetArgumentValue("Pre Sample Air Gap")	-- [uL]
	end
	local sampleInjectionAspirateFlowRate = context:GetArgumentValue("sampleInjectionAspirateFlowRate")
	local pullupDelay = 0

	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local itemPosition = pp.QueryModules(execAux, "ItemPositionDescription")
	local solventVial = itemPosition[pr.GetItemPosIdx(context:GetArgumentValue("Derivatize Sample", "Derivatize solvent position"), execAux)]
	local solventVialPenetrationDepth = context:GetArgumentValue("Derivatize Sample", "Solvent vial penetration depth")
	local injector = pp.QueryModule(execAux, pp.Capabilities.IInjector)
	local sample_position = context:GetArgumentValue("sample_position")
	local container = pp.QueryModule(execAux, pp.Capabilities.TrayContainer)
	local sampleVial = execLeft.ConfigurationService:GetVial(container.Name..":"..sample_position)
	local sampleVolume = context:GetArgumentValue("sample_volume")
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)

	local speed = pp.Quantity(context:GetArgumentValue("sample_aspirate_speed"), "uL/s")
	local delay = pp.Quantity(context:GetArgumentValue("sample_postaspirate_delay"), "ms")
	local derivativeVialPenetrationDepth = context:GetArgumentValue("Derivatize Sample", "Penetration Depth")
	local D_volume = 0

	-- 1. draw transport liquid
	-- get Rear Airgap: Between Solvent and Sample
	status:SetStatus("Aspirate transport liquid")
	execLeft:LeaveObject()
	execLeft:MoveToObject( solventVial, 1, true, true, true )
	if (solventVialPenetrationDepth == 0.0) then
		-- needle lifting up 1 mm after bottom is sensed
		execLeft:PenetrateWithBottomSense( solventVial, 1, pp.Quantity(1, "mm"))
	else
		execLeft:PenetrateObject( solventVial, 1, pp.Quantity(solventVialPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
	end
	execLeft:AspirateSyringe( pp.Quantity(context:GetArgumentValue("presample_solvent_volume"), "uL"), pp.Quantity(sampleInjectionAspirateFlowRate, "uL/s"), nil, pp.Quantity(pullupDelay, "ms"))
	execLeft:LeaveObject()
	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus("Aspirate transport liquid")

	pp.CleanNeedle(execLeft, execAux, preAirGap, 5)

	-- 3. goto vial x
	local statusText = "Aspirate derivative 1"

	if ((context:GetArgumentValue("Derivatize Sample", "1st Volume") > 0.1) or (context:GetArgumentValue("Derivatize Sample", "2nd Volume") > 0.1)) then
		-- set derivatization parameters
		D_position = "Slot"..context:GetArgumentValue("Derivatize Sample", "Tray")..":1"
		D_container = pp.QueryModule(execAux, pp.Capabilities.TrayContainer)
		D_vial = execLeft.ConfigurationService:GetVial(D_container.Name..":"..D_position)
		mixCycles = context:GetArgumentValue("Derivatize Sample", "Mixing Cycles")
		mixSpeed = context:GetArgumentValue("Derivatize Sample", "Mixing Speed")

		timeToDerivatize = context:GetArgumentValue("Derivatize Sample", "Time to Derivatize")*1000		-- [ms]
		mixingVolume = context:GetArgumentValue("Derivatize Sample", "Mixing Volume")
		heightFromBottomSampleVial = context:GetArgumentValue("Derivatize Sample", "Needle Height for Mixing")
		airGap = context:GetArgumentValue("D-AirGap")
		dispenseDelay = context:GetArgumentValue("D-dispenseDelay")*1000		-- [ms]
		pullupDelay = context:GetArgumentValue("D-pullupDelay")*1000			-- [ms]

		context:Log(baltic.devider)
		context:Log("  Derivatize common parameter:")
		context:Log("    Solvent vial:                        {0}",context:GetArgumentValue("Derivatize Sample", "Derivatize solvent position"))
		context:Log("    Solvent vial penetration depth:      {0}",solventVialPenetrationDepth)
		context:Log("    Sample position:                     {0}",sample_position)
		context:Log("    Sample aspiration speed:             {0} uL/s",speed)
		context:Log("    Sample post aspirate delay:          {0} ms",delay)
		context:Log("    Air gap:                             {0} uL",airGap)
		context:Log("    Dispense delay:                      {0} ms",dispenseDelay)
		context:Log("    Pullup delay:                        {0} ms",pullupDelay)
		context:Log("  Derivatize mixing parameter:")
		context:Log("    Vial penetration depth:              {0} mm",derivativeVialPenetrationDepth)
		context:Log("    Tray:                                {0}",D_vial.Tray)
		context:Log("    Vial:                                {0}",D_position)
		context:Log("    Mixing volume:                       {0} uL",mixingVolume)
		context:Log("    Mixing cycles:                       {0}",mixCycles)
		context:Log("    Mixing speed:                        {0} uL/s",mixSpeed)
		context:Log("    Needle height for mixing:            {0} mm",heightFromBottomSampleVial)
		context:Log("    Time to derivatize:                  {0} s",timeToDerivatize/1000)
		context:Log(baltic.devider)
	end

	if (context:GetArgumentValue("Derivatize Sample", "1st Volume") > 0.1) and (D_vial ~= nil) then
		status:SetStatus(statusText)
		execLeft:MoveToObject( D_vial.Tray, context:GetArgumentValue("Derivatize Sample", "1st Vial") )
		if derivativeVialPenetrationDepth < 1.0 then
			execLeft:PenetrateWithBottomSense( D_vial.Tray, context:GetArgumentValue("Derivatize Sample", "1st Vial"), pp.Quantity(1, "mm"), nil, nil)
		else
			execLeft:PenetrateObject( D_vial.Tray, context:GetArgumentValue("Derivatize Sample", "1st Vial"), pp.Quantity(derivativeVialPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
		end
		-- 4. draw derivatization reagent1 from vial x
		execLeft:AspirateSyringe( pp.Quantity(context:GetArgumentValue("Derivatize Sample", "1st Volume"), "uL"), pp.Quantity(sampleInjectionAspirateFlowRate, "uL/s"), nil, pp.Quantity(pullupDelay, "ms"))
		execLeft:LeaveObject(pp.Quantity("50 mm"), true, false)
		D_volume = D_volume + context:GetArgumentValue("Derivatize Sample", "1st Volume") + airGap
		pp.CleanNeedle(execLeft, execAux, preAirGap, 5)
		while not execLeft.IsIdle do
			sleeping_yield()
		end
		context:Log(baltic.devider)
		context:Log("    Derivatize Sample 1st Volume:        {0}",D_volume)
		context:Log(baltic.devider)
		status:RemoveStatus(statusText)
	end

	-- 6. goto vial y
	statusText = "Aspirate derivative 2"
	if (context:GetArgumentValue("Derivatize Sample", "2nd Volume") > 0.1) and (D_vial ~= nil) then
		status:SetStatus(statusText)
		execLeft:MoveToObject( D_vial.Tray, context:GetArgumentValue("Derivatize Sample", "2nd Vial") )
		if derivativeVialPenetrationDepth < 1.0 then
			execLeft:PenetrateWithBottomSense(D_vial.Tray, context:GetArgumentValue("Derivatize Sample", "2nd Vial"), pp.Quantity(1, "mm"), nil, nil)
		else
			execLeft:PenetrateObject( D_vial.Tray, context:GetArgumentValue("Derivatize Sample", "2nd Vial"), pp.Quantity(derivativeVialPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
		end
		-- 7. draw derivatisation reagent2 from vial y
		execLeft:AspirateSyringe( pp.Quantity(context:GetArgumentValue("Derivatize Sample", "2nd Volume"), "uL"), pp.Quantity(sampleInjectionAspirateFlowRate, "uL/s"), nil, pp.Quantity(pullupDelay, "ms"))
		execLeft:LeaveObject(pp.Quantity("50 mm"), true, false)
		D_volume = D_volume + context:GetArgumentValue("Derivatize Sample", "2nd Volume") + airGap
		pp.CleanNeedle(execLeft, execAux, preAirGap, 5)
		while not execLeft.IsIdle do
			sleeping_yield()
		end
		context:Log(baltic.devider)
		context:Log("    Derivatize Sample 2nd Volume:        {0}",D_volume)
		context:Log(baltic.devider)
		status:RemoveStatus(statusText)
	end

	if (context:GetArgumentValue("Derivatize Sample", "1st Volume") > 0.1) or (context:GetArgumentValue("Derivatize Sample", "2nd Volume") > 0.1) then
		-- 8. release derivatization regents into sample vial
		statusText = "Mixing derivative with sample " .. mixCycles .. " times"
		status:SetStatus(statusText)
		execLeft:MoveToObject( sampleVial.Tray, sampleVial.Index )
		execLeft:PenetrateWithBottomSense( sampleVial.Tray, sampleVial.Index, pp.Quantity(heightFromBottomSampleVial, "mm"), nil, nil)
		execLeft:DispenseSyringe( pp.Quantity(D_volume, "uL"), pp.Quantity(context:GetArgumentValue("Derivatize Sample", "Derivative Release Speed"), "uL/s"))
		execLeft:Wait(pp.Quantity(1000, "ms"))

		-- 9. mix
		for i=1, mixCycles do
			execLeft:AspirateSyringe( pp.Quantity(mixingVolume, "uL"), pp.Quantity(mixSpeed, "uL/s"), nil, pp.Quantity(pullupDelay, "ms"))
			execLeft:DispenseSyringe( pp.Quantity(mixingVolume, "uL"), pp.Quantity(mixSpeed, "uL/s"))
			execLeft:Wait(pp.Quantity(dispenseDelay, "ms"))
		end
		while not execLeft.IsIdle do
			sleeping_yield()
		end
		execLeft:LeaveObject(pp.Quantity("50 mm"), true, false)

		-- 10. wait (reaction time)
		execLeft:Wait(pp.Quantity(timeToDerivatize, "ms"))
		while not execLeft.IsIdle do
			sleeping_yield()
		end
		status:RemoveStatus(statusText)
	else
		-- this is not neccessary if derivatization has been done
		execLeft:MoveToObject( sampleVial.Tray, sampleVial.Index )
	end

	-- 11. draw derivated sample volume
	statusText = "Aspirate derivated sample"
	status:SetStatus(statusText)
	execLeft:MoveToObject( sampleVial.Tray, sampleVial.Index )
	execLeft:PenetrateWithBottomSense( sampleVial.Tray, sampleVial.Index, pp.Quantity(1, "mm"), nil, nil)
	execLeft:AspirateSyringe(pp.Quantity(sampleVolume, "uL"), speed, nil, delay)
	execLeft:LeaveObject()		-- close drawer
	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus(statusText)

	-- get Front Airgap

	pp.CleanNeedle(execLeft, execAux, preAirGap, 5)

	-- 12. goto injector
	execLeft:MoveToObject(injector)
	-- expulse part of the post sample air volume that was necessary to protect the sample during exernal needle wash
	execLeft:DispenseSyringe( pp.Quantity(preAirGap*0.75, "uL"), pp.Quantity(context:GetArgumentValue("sample_inject_speed"), "uL/s"))
	pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)			-- PNS-92
	local afterLoopToInject = context:GetArgumentValue("Injection Delays", "After Loop to Inject")
	execLeft:Wait(pp.Quantity(afterLoopToInject, "ms"))
	execLeft:PenetrateWithConstForce( injector )
	local afterPenetration = context:GetArgumentValue("Injection Delays", "After Penetration")
	execLeft:Wait(pp.Quantity(afterPenetration, "ms"))

	while not execLeft.IsIdle do
		sleeping_yield()
	end
	return sampleVolume, (preAirGap*0.25)+airGap+1
end

---Dilute a sample
---@param context IProcedureExecutionContext
---@param sleeping_yield function
---@return number
---@return number
function P.dilute_and_inject_Sample(context, sleeping_yield)
	local baltic =	require "baltic"
	local pp = 		require "palplus"
	local pr =		require "PreRunFunctions"

	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)

	local pullupDelay = 0

	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local injector = pp.QueryModule(execAux, pp.Capabilities.IInjector)

	local speed = pp.Quantity(context:GetArgumentValue("sample_aspirate_speed"), "uL/s")
	local delay = context:GetArgumentValue("sample_postaspirate_delay")

	local S_container = pp.QueryModule(execAux, pp.Capabilities.TrayContainer)
	local sourceVial = "Slot"..context:GetArgumentValue("Dilution Series", "Tray")..":"..context:GetArgumentValue("Dilution Series", "Source-Vial")
	local S_Source = execLeft.ConfigurationService:GetVial(S_container.Name..":"..sourceVial)
	local S_Target = execLeft.ConfigurationService:GetVial(S_container.Name..":"..sourceVial)
	local sample_position = context:GetArgumentValue("sample_position")
	local sampleVial = execLeft.ConfigurationService:GetVial(S_container.Name..":"..sample_position)
	local sampleVolume = context:GetArgumentValue("sample_volume")
	local itemPosition = pp.QueryModules(execAux, "ItemPositionDescription")
	local solventVial = itemPosition[pr.GetItemPosIdx(context:GetArgumentValue("Dilution Series", "Dilution solvent position"), execAux)]
	local preFillVolume = context:GetArgumentValue("Dilution Series", "Prefill Volume")
	local diluteVialPenetrationDepth = context:GetArgumentValue("Dilution Series", "Needle Height for Mixing")
	local mixSpeed = context:GetArgumentValue("Dilution Series", "Mixing Speed")
	local releaseSpeed = context:GetArgumentValue("Dilution Series", "Dilution Release Speed")
	local mixCycles = context:GetArgumentValue("Dilution Series", "Mixing Cycles")
	local timeToDilute = context:GetArgumentValue("Dilution Series", "Time to Dilute")
	local mixingVolume = context:GetArgumentValue("Dilution Series", "Mixing Volume")
	local sourceVolume = pp.Quantity(context:GetArgumentValue("Dilution Series", "Source-Volume"), "uL")
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)

	local needleHeight = 1
	local drawSpeed   = 5
	local pauseDraw   = 0
	local preAirGap   = 0.5
	local postAirGap  = 1

	if context:GetArgumentValue("Injection Settings") == true  then
		needleHeight = context:GetArgumentValue("Injection Settings", "Needle Height")		-- [mm]
		drawSpeed   = context:GetArgumentValue("Injection Settings", "Draw Speed")			-- [uL/s]
		pauseDraw   = context:GetArgumentValue("Injection Settings", "Pause Draw")*1000		-- [ms]
		delay = delay + pauseDraw
	end
	if context:GetArgumentValue("Air Gaps") == true  then
		preAirGap   = context:GetArgumentValue("Air Gaps", "Pre Sample Air Gap")	-- [uL]
		postAirGap  = context:GetArgumentValue("Air Gaps", "Post Sample Air Gap")	-- [uL]
	end

	context:Log(baltic.devider)
	context:Log("  Dilute common parameter:")
	context:Log("    Source vial:                         {0}",context:GetArgumentValue("Dilution Series", "Source-Vial"))
	context:Log("    Solvent vial:                        {0}",context:GetArgumentValue("Dilution Series", "Dilution solvent position"))
	context:Log("    Sample position:                     {0}",sample_position)
	context:Log("    Sample volume:                       {0} uL",sampleVolume)
	context:Log("    Sample aspiration speed:             {0} ",speed)
	context:Log("    Sample post aspirate delay:          {0} ms",delay)
	context:Log("    Pre sample air gap:                  {0} uL",preAirGap)
	context:Log("    Post sample air gap:                 {0} uL",postAirGap)
	context:Log("    Needle height:                       {0} mm",needleHeight)
	context:Log("    Draw speed:                          {0} uL/s",drawSpeed)
	context:Log("    Pause draw:                          {0} s",pauseDraw/1000)
	context:Log("  Dilute mixing parameter:")
	context:Log("    Pre fill volume:                     {0} uL",preFillVolume)
	context:Log("    Source volume:                       {0} ",sourceVolume)
	context:Log("    Dilution Release Speed:              {0} uL/s",releaseSpeed)
	context:Log("    Mixing volume:                       {0} uL",mixingVolume)
	context:Log("    Mixing cycles:                       {0}",mixCycles)
	context:Log("    Mixing speed:                        {0} uL/s",mixSpeed)
	context:Log("    Needle height for mixing:            {0} mm",diluteVialPenetrationDepth)
	context:Log("    Time to dilute:                      {0} s",timeToDilute)
	context:Log(baltic.devider)

	local function prefillSampleVialWithSolvent(sample_Vial)
      status:SetStatus("Prefill sample vial")
      while (preFillVolume>0) do
          local solventVolume = math.min(preFillVolume,(100-postAirGap))
          preFillVolume = preFillVolume - solventVolume
          execLeft:AspirateSyringe( pp.Quantity(postAirGap, "uL"), pp.Quantity(drawSpeed, "uL/s"), nil, pp.Quantity(delay, "ms"))	
          -- move to wash station
          execLeft:MoveToObject( solventVial, 1, true, true, true )
		  if (context:GetArgumentValue("Dilution Series", "Dilution solvent position") < 4) then
			execLeft:PenetrateObject( solventVial, 1, pp.Quantity(30, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))					-- position 1 - 3 is used
		  else
			execLeft:PenetrateObject( solventVial, 1, pp.Quantity(42, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))					-- position 4 - 5 is used
		  end
          execLeft:AspirateSyringe( pp.Quantity(solventVolume, "uL"), pp.Quantity(40, "uL/s"), nil, pp.Quantity(delay, "ms"))
          execLeft:LeaveObject()

          execLeft:MoveToObject( sample_Vial.Tray, sample_Vial.Index )
          execLeft:PenetrateWithBottomSense( sample_Vial.Tray, sample_Vial.Index, pp.Quantity(1, "mm"), nil, nil)
          execLeft:DispenseSyringe(pp.Quantity(solventVolume+postAirGap, "uL"), pp.Quantity(releaseSpeed, "uL/s"))
          execLeft:LeaveObject(pp.Quantity("50 mm"), true, false)
          while not execLeft.IsIdle do
              sleeping_yield()
          end
      end
      status:RemoveStatus("Prefill sample vial")
	end
	local function drawFromSourceVial(source)
		-- 1. draw sample from source vial
		status:SetStatus("Aspirate source volume")
		execLeft:AspirateSyringe( pp.Quantity(postAirGap, "uL"), pp.Quantity(drawSpeed, "uL/s"), nil, pp.Quantity(delay, "ms"))		
		execLeft:MoveToObject(source.Tray, source.Index)
		if diluteVialPenetrationDepth <= 1.0 then
			execLeft:PenetrateWithBottomSense( source.Tray, source.Index, pp.Quantity(needleHeight, "mm"), nil, nil)
		else
			execLeft:PenetrateObject( source.Tray, context:GetArgumentValue("Dilution Series", "Source-Vial"), pp.Quantity(diluteVialPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
		end
		execLeft:AspirateSyringe(sourceVolume, pp.Quantity(drawSpeed, "uL/s"), nil, pp.Quantity(delay, "ms"))
		execLeft:LeaveObject(pp.Quantity("50 mm"), true, false)
		while not execLeft.IsIdle do
			sleeping_yield()
		end
		status:RemoveStatus("Aspirate source volume")
	end
	local function dispenseToTargetVialAndMixing(target)
		status:SetStatus("Dispense source volume")
		execLeft:MoveToObject(target.Tray, target.Index)
		if diluteVialPenetrationDepth <= 1.0 then
			execLeft:PenetrateWithBottomSense( target.Tray, target.Index, pp.Quantity(needleHeight, "mm"), nil, nil)
		else
			execLeft:PenetrateObject( target.Tray, target.Index, pp.Quantity(diluteVialPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
		end
		execLeft:DispenseSyringe(pp.Quantity((preAirGap), "uL")+sourceVolume, pp.Quantity(releaseSpeed, "uL/s"))
		for i=1, mixCycles do
			execLeft:Wait(pp.Quantity(timeToDilute, "s"))
			execLeft:AspirateSyringe(pp.Quantity(mixingVolume, "uL"), pp.Quantity(mixSpeed, "uL/s"), nil, pp.Quantity(pullupDelay, "ms"))
			execLeft:DispenseSyringe(pp.Quantity((mixingVolume), "uL"), pp.Quantity(mixSpeed, "uL/s"))
		end
		execLeft:LeaveObject(pp.Quantity("50 mm"), true, false)
		while not execLeft.IsIdle do
			sleeping_yield()
		end
		status:RemoveStatus("Dispense source volume")
	end

	execLeft:LeaveObject()

	-- prefill vials for dilution
	prefillSampleVialWithSolvent(sampleVial)

	S_Source = S_Target
	context:Log("--- Draw from {0}", S_Source)
	drawFromSourceVial(S_Source)
	pp.CleanNeedle(execLeft, execAux, preAirGap, 5)
	while not execLeft.IsIdle do
		sleeping_yield()
	end
	context:Log("--- Dispense into {0}", sampleVial)
	dispenseToTargetVialAndMixing(sampleVial)
	-- draw diluted sample volume
	local statusText = "Aspirate diluted sample"
	status:SetStatus(statusText)
	execLeft:MoveToObject( sampleVial.Tray, sampleVial.Index )
	execLeft:AspirateSyringe( pp.Quantity(postAirGap, "uL"), pp.Quantity(drawSpeed, "uL/s"), nil, pp.Quantity(pullupDelay, "ms"))		
	execLeft:PenetrateWithBottomSense( sampleVial.Tray, sampleVial.Index, pp.Quantity(needleHeight, "mm"), nil, nil)
	execLeft:AspirateSyringe(pp.Quantity(sampleVolume, "uL"), speed, nil, pp.Quantity(pullupDelay, "ms"))
	execLeft:LeaveObject()		-- close drawer
	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus(statusText)

	statusText = "Inject diluted sample"
	status:SetStatus(statusText)
	-- get Front Airgap
	pp.CleanNeedle(execLeft, execAux, preAirGap, 5)

	execLeft:MoveToObject(injector)
	-- expulse part of the post sample air volume that was necessary to protect the sample during exernal needle wash
	execLeft:DispenseSyringe( pp.Quantity(preAirGap*0.75, "uL"), pp.Quantity(context:GetArgumentValue("sample_inject_speed"), "uL/s"))
	pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)			-- PNS-92
	local afterLoopToInject = context:GetArgumentValue("Injection Delays", "After Loop to Inject")
	execLeft:Wait(pp.Quantity(afterLoopToInject, "ms"))
	execLeft:PenetrateWithConstForce( injector )
	local afterPenetration = context:GetArgumentValue("Injection Delays", "After Penetration")
	execLeft:Wait(pp.Quantity(afterPenetration, "ms"))

	while not execLeft.IsIdle do
		sleeping_yield()
	end
	status:RemoveStatus(statusText)
	return sampleVolume, (preAirGap*0.25)+postAirGap
end

return P
