local Date = "2025/07/22"

local math = require "math"
local baltic = require "baltic"

-- package
local P = {}

-- dependencies as locals here
luanet.load_assembly("Bruker.Lc")
luanet.load_assembly("PalPlusDriver")
luanet.load_assembly("PalPlusDriverObjects")

---@type LuaHelper
local LuaHelper = luanet.import_type("Bruker.Lc.Scripting.LuaHelper")

local CtcQuantity = luanet.import_type("Ctc.Palplus.Integration.Driver.Entities.Quantity")
local ModuleCapabilities = luanet.import_type("Ctc.Palplus.Integration.Driver.Modules.ModuleCapabilities")

local _error = error

-- replace global env with package env; no more global access
_ENV = P

P.Capabilities = ModuleCapabilities

-- LcToolPosition
P.LcToolValveOpen = 1
P.LcToolValveClose = 2
P.VolumetricPumpVolume = 350			-- [µl]

P.Aqueous = 1
P.Organic = 2
P.Waste = 3

P.ToggleWashLastStepVolume = 500
P.ToggleWashLastStepSpeed = 5


---Convert the object from radiant to degrees
---@param valveObject table
---@return table
function P.ConvertRadToDeg(valveObject)
	local unit = valveObject.ValveDrivePosition.Unit
	local val  = valveObject.ValveDrivePosition.Value
	if unit == "rad" then
		val  = val / math.pi * 180
		unit = "deg"
	end
	local object = {Value = val, Unit = unit}
	return object
end

---Check if the trap valve is in BLOCK position and return the position 
---@param palplus_participant IPalParticipant
---@return number|nil
---@return boolean
function P.isTrapValveBlocked(palplus_participant)
	local trapValvePosition = P.GetTrapValvePosition(palplus_participant)
	if (trapValvePosition == baltic.TrapValve.Waste) or
		(trapValvePosition == baltic.TrapValve.Trap) or
		(trapValvePosition == baltic.TrapValve.GradientA) or
		(trapValvePosition == baltic.TrapValve.Analytical) or
		(trapValvePosition == baltic.TrapValve.GradientT) or
		(trapValvePosition == baltic.TrapValve.InjectWaste) then
		return trapValvePosition, false
	else
		return trapValvePosition, true
	end
end

---Return the actual trap valve position in [deg]
---@param palplus_participant IPalParticipant
---@return number|nil
function P.GetTrapValvePosition(palplus_participant)
	local trapValve = P.QueryModule(palplus_participant, "SelectorValveDescription")
	if trapValve ~= nil then
		local newValveObject = P.ConvertRadToDeg(trapValve)
		return newValveObject.Value
	end
	return nil
end

---Return the actual injector valve position in [deg]
---@param palplus_participant IPalParticipant
---@return number|nil
function P.GetInjectorValvePosition(palplus_participant)
	local injectorValve = P.QueryModule(palplus_participant, "LcInjectorValveDescription")
	if injectorValve ~= nil then
		local newValveObject = P.ConvertRadToDeg(injectorValve)
		return newValveObject.Value
	end
	return nil
end

---Creates a pal Quantity instance
---@param value number|string
---@param unit string|nil
---@return Quantity
function P.Quantity(value, unit)
	if (unit) then
		return CtcQuantity(value, unit)
	else
		return CtcQuantity.Parse(value)
	end
end

---Query the pal for modules supporting a specific capability
---@param palplus_participant IPalParticipant
---@param module_type string
---@return table
function P.QueryModules(palplus_participant, module_type)
	local it = LuaHelper.WrapEnumerable(palplus_participant.ConfigurationService:QueryModules(module_type)):GetEnumerator()
	local table = {}
	local i = 1
	while it:MoveNext() do
		table[i] = it.Current
		i = i+1
	end
	return table
end


-- If is present: 1 module: return this module; 2 or more modules: return first module (module[1])
-- If no module is present: 'nil' is returned
-- (An error is thrown if zero or multiple modules support the capability.)
---Query the pal for a module supporting a specific capability
---@param palplus_participant IPalParticipant
---@param module_type string
---@return IModule
function P.QueryModule(palplus_participant, module_type)
	local module = P.QueryModules(palplus_participant, module_type)
	if module[1] ~= nil then
		return module[1]
	end
	_error("module type "..module_type.." does not exist", 2)
end

-- This is useful to identify valve drives with child module capabilities such as ISelectorValve or ILcInjectorValve.
-- @param palplus_participant an object implementing IBusinessProcedureParticipant.
-- @param child_type a string describing a module type or capability (see "Ctc.Palplus.Integration.Driver.Modules.ModuleCapabilities").
-- @return module matching the child type/capability or nil if none found.
---Query the pal for an IValveDrive module with a specific child module capability
---@param palplus_participant IPalParticipant
---@param child_type string
---@return unknown
function P.QueryValveDrive(palplus_participant, child_type)
	-- GetChildrenByType requires a module type of the form "SelectorValveDescription",
	-- so the strings in ModuleCapabilities cannot be used directly, therefore we "translate" it.
	child_type = P.QueryModule(palplus_participant, child_type).ModuleTypeName
	local valves = P.QueryModules(palplus_participant, P.Capabilities.IValveDrive)
	for i=1,#valves do
		local child_it = LuaHelper.WrapEnumerable(valves[i]:GetChildrenByType(child_type)):GetEnumerator()
		-- check for child presence
		if (child_it:MoveNext()) then
			return valves[i]
		end
	end
	return nil
end

---Empty the syringe and leave the object
---@param context IProcedureExecutionContext
---@param palplus_left IPalParticipant
---@param palplus_aux IPalParticipant
---@param penetrationDepth Quantity
---@param syringeZeroPosition number [mm]
function P.EmptySyringe_And_LeaveObject(context, palplus_left, palplus_aux, penetrationDepth, syringeZeroPosition)
	local syringePosition = P.GetSyringePosition(palplus_left)		-- [mm]
	if (syringePosition - 0.02) > syringeZeroPosition then
		context:Log("Syringe position: ({0}) mm", (math.floor(syringePosition*10)/10))
		local wash_module = P.QueryModule(palplus_aux, P.Capabilities.ILcMsWashStation)

		palplus_left:LeaveObject()
		palplus_left:MoveToObject(wash_module, P.Waste)
		palplus_left:PenetrateObject(wash_module, P.Waste, penetrationDepth, P.Quantity(baltic.SyringePenetrationSpeed, "mm/s") )
		palplus_left:EmptySyringe()
		palplus_left:LeaveObject()
		palplus_left:MoveToObject(wash_module, P.Organic)
		palplus_left:PenetrateObject(wash_module, P.Waste, penetrationDepth, P.Quantity(baltic.SyringePenetrationSpeed, "mm/s") )
		palplus_left:LeaveObject()
		palplus_left:MoveToObject(wash_module, P.Aqueous)
		palplus_left:PenetrateObject(wash_module, P.Waste, penetrationDepth, P.Quantity(baltic.SyringePenetrationSpeed, "mm/s") )
	end
	palplus_left:LeaveObject()
end

---Open/close the buerkert valve
---@param palplus_left IPalParticipant
---@param state number
function P.SetLCToolValve(palplus_left, state)
	local tool = P.QueryModule(palplus_left, P.Capabilities.IToolLc)
	palplus_left:Wait(P.Quantity(50,"ms"))
	palplus_left:SetLcToolPosition( tool, 1, state )
	if (state == P.LcToolValveOpen) then palplus_left:Wait(P.Quantity(1000,"ms")) end
end

---Open the buerkert valve and pump the desired volume
---@param palplus_left IPalParticipant
---@param channel number
---@param volumeToBePumped number [µL]
---@param pumpSpeed number [µL/s]
function P.VolumeToBePumped(palplus_left, channel, volumeToBePumped, pumpSpeed)
	local washpump 	= P.QueryModule(palplus_left, P.Capabilities.IPumpModule)
	if pumpSpeed == nil then pumpSpeed = 10 end
	P.SetLCToolValve(palplus_left, P.LcToolValveOpen)
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, channel, P.Quantity(volumeToBePumped, "uL"), P.Quantity(pumpSpeed, "uL/s"), nil, true, true)
end

---Print the toggle wash parameters into the execution log file
---@param context IProcedureExecutionContext
---@param ToggleWashContainer ToggleWashContainer
function P.PrintToggleWashParameter(context, ToggleWashContainer)
	context:Log(baltic.devider)
	context:Log("--- ToggleWashParameter ---")
	context:Log("    Number of steps:            {0}", ToggleWashContainer.numOfSteps)
	context:Log("    Step time:                  {0} sec", ToggleWashContainer.stepTime)
	context:Log("    Organic volume per step:    {0} \181L", ToggleWashContainer.organicStepVolume)
	context:Log("    Aqueous volume per step:    {0} \181L", ToggleWashContainer.aqueousStepVolume)
	context:Log("    Pump speed:                 {0} \181L/sec", ToggleWashContainer.pumpSpeed)
	context:Log("    Last step volume:           {0} \181L", ToggleWashContainer.lastStepVolume)
	context:Log("    Last step pump speed:       {0} \181L/sec", ToggleWashContainer.lastStepSpeed)
	context:Log("    Time to next step:          {0} sec", math.floor(ToggleWashContainer.timeToNextStepStart))
	context:Log("    Total time:                 {0} sec", math.floor(ToggleWashContainer.totalTime))
	context:Log("    Toggle aqueous and organic: {0}",ToggleWashContainer.toggleAqueousOrganic)
	context:Log("    Only aqueous steps:         {0}", ToggleWashContainer.aqueousOnly)
	context:Log("    Only one step organic:      {0}",ToggleWashContainer.onlyOneStepOrganic)
	context:Log("    Num Of Last Aqueous Steps:  {0}",ToggleWashContainer.numOfLastAqueousSteps)
	context:Log(baltic.devider)
end

---Initialize the toggle wash parameters
---@param context IProcedureExecutionContext
---@param ToggleWashContainer ToggleWashContainer
---@param totalTime number
---@return ToggleWashContainer
function P.ToggleWashInitialization(context, ToggleWashContainer, totalTime)
	local extraTimeForAqueous = 100 / context:GetArgumentValue("Toggle Wash In Gradient", "Pump Speed")
	local stepTimeOrganic = (context:GetArgumentValue("Toggle Wash In Gradient", "Volume Organic Per Step") / context:GetArgumentValue("Toggle Wash In Gradient", "Pump Speed")) + extraTimeForAqueous + 3
	local stepTimeAqueous = (context:GetArgumentValue("Toggle Wash In Gradient", "Volume Aqueous Per Step") / context:GetArgumentValue("Toggle Wash In Gradient", "Pump Speed")) + 3
	local averageStepTime = (stepTimeOrganic + stepTimeAqueous) / 2
	if context:GetArgumentValue("Toggle Wash In Gradient", "Aqueous Only") == true then
		averageStepTime = stepTimeAqueous
	else
		if context:GetArgumentValue("Toggle Wash In Gradient", "Only One Wash Step Organic") == true then
			averageStepTime = (stepTimeOrganic + (stepTimeAqueous * (context:GetArgumentValue("Toggle Wash In Gradient", "Num Of Steps") - 2)))
		end
	end
	ToggleWashContainer.lastStepVolume = P.ToggleWashLastStepVolume
	ToggleWashContainer.lastStepSpeed = P.ToggleWashLastStepSpeed
	local lastCycleTime = (ToggleWashContainer.lastStepVolume / ToggleWashContainer.lastStepSpeed) + 5
	local numOfLastAqueousSteps = context:GetArgumentValue("Toggle Wash In Gradient", "Num Of Last Aqueous Steps")
	local numOfSteps = context:GetArgumentValue("Toggle Wash In Gradient", "Num Of Steps")
	local timeToNextStepStart = (totalTime - lastCycleTime - (averageStepTime * (numOfSteps-1+numOfLastAqueousSteps))) / (numOfSteps+numOfLastAqueousSteps)
	if (timeToNextStepStart < 1) then timeToNextStepStart = 0 end
	ToggleWashContainer.numOfSteps  = context:GetArgumentValue("Toggle Wash In Gradient", "Num Of Steps")
	ToggleWashContainer.nextStepOrganic = context:GetArgumentValue("Toggle Wash In Gradient", "Toggle Organic and Aqueous") or context:GetArgumentValue("Toggle Wash In Gradient", "Only One Wash Step Organic")
	ToggleWashContainer.organicStepVolume = context:GetArgumentValue("Toggle Wash In Gradient", "Volume Organic Per Step")
	ToggleWashContainer.aqueousStepVolume = context:GetArgumentValue("Toggle Wash In Gradient", "Volume Aqueous Per Step")
	ToggleWashContainer.pumpSpeed = context:GetArgumentValue("Toggle Wash In Gradient", "Pump Speed")
	ToggleWashContainer.stepTime = averageStepTime
	ToggleWashContainer.timeToNextStepStart = timeToNextStepStart
	ToggleWashContainer.totalTime = totalTime
	ToggleWashContainer.onlyOneStepOrganic = context:GetArgumentValue("Toggle Wash In Gradient", "Only One Wash Step Organic")
	ToggleWashContainer.aqueousOnly = context:GetArgumentValue("Toggle Wash In Gradient", "Aqueous Only")
	ToggleWashContainer.toggleAqueousOrganic = context:GetArgumentValue("Toggle Wash In Gradient", "Toggle Organic and Aqueous")
	ToggleWashContainer.numOfLastAqueousSteps = numOfLastAqueousSteps

	return ToggleWashContainer
end

---Execute the toggle wash procedure
---@param palplus_left IPalParticipant
---@param ToggleWashContainer ToggleWashContainer
---@return ToggleWashContainer
function P.RunToggleWashVolumePump(palplus_left, ToggleWashContainer)
	if palplus_left.IsIdle then
		if (ToggleWashContainer.numOfSteps > 1) then
			local channel = P.Aqueous
			local volume = ToggleWashContainer.aqueousStepVolume
			if (ToggleWashContainer.aqueousOnly == false) then
				if (ToggleWashContainer.nextStepOrganic == true) then
					channel = P.Organic
					volume = ToggleWashContainer.organicStepVolume
					ToggleWashContainer.nextStepOrganic = false
				end
			end
			P.VolumeToBePumped(palplus_left, channel, volume, ToggleWashContainer.pumpSpeed)
			if channel == P.Organic then	-- add 100µL aqueous to the organic wash
				P.VolumeToBePumped(palplus_left, P.Aqueous, 100, ToggleWashContainer.pumpSpeed)
			end
			ToggleWashContainer.numOfSteps = ToggleWashContainer.numOfSteps - 1
			if (ToggleWashContainer.toggleAqueousOrganic == true and channel == P.Aqueous) then
				ToggleWashContainer.nextStepOrganic =  true
			end
		elseif ToggleWashContainer.numOfLastAqueousSteps > 1 then
			P.VolumeToBePumped(palplus_left, P.Aqueous, ToggleWashContainer.aqueousStepVolume, ToggleWashContainer.pumpSpeed)
			ToggleWashContainer.numOfLastAqueousSteps = ToggleWashContainer.numOfLastAqueousSteps - 1
		else
			P.VolumeToBePumped(palplus_left, P.Aqueous, ToggleWashContainer.lastStepVolume, ToggleWashContainer.lastStepSpeed)
			ToggleWashContainer.isToggleWashEnabled = false
		end
	end
	return ToggleWashContainer
end

---Prime the volumetric wash pump without syringe strokes
---@param palplus_left IPalParticipant
---@param volume number [µL]
---@param speed number [µL/s]
function P.PrimeVolumetricPump(palplus_left, volume, speed)
	local washpump 			= P.QueryModule(palplus_left, P.Capabilities.IPumpModule)
	local volumeToBePumped	= P.Quantity(volume, "uL")
	local pumpSpeed			= P.Quantity(speed, "uL/s")
	local wash_module		= P.QueryModule(palplus_left, P.Capabilities.ILcMsWashStation)
	local penetrationDepth	= P.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
	local penetrationSpeed	= P.Quantity(baltic.SyringePenetrationSpeed, "mm/s")

	palplus_left:MoveToObject( wash_module, P.Waste )
	palplus_left:PenetrateObject( wash_module, P.Waste, penetrationDepth, penetrationSpeed )
	P.SetLCToolValve(palplus_left, P.LcToolValveOpen)
	palplus_left:Wait(P.Quantity(1000, "ms"))
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, P.Organic, volumeToBePumped, pumpSpeed, nil, true, true)
	palplus_left:Wait(P.Quantity(1000, "ms"))
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, P.Aqueous, volumeToBePumped, pumpSpeed, nil, true, true)
	palplus_left:Wait(P.Quantity(2000, "ms"))
	P.SetLCToolValve(palplus_left, P.LcToolValveClose)
end

---Prime the volumetric wash pump with aqueous
---@param palplus_left IPalParticipant
---@param moveToWashStation boolean
---@param moveToHome boolean
function P.PrimeLCPToolLoop(palplus_left, moveToWashStation, moveToHome)
	local tool 				= P.QueryModule(palplus_left, P.Capabilities.IToolLc)
	local washpump 			= P.QueryModule(palplus_left, P.Capabilities.IPumpModule)
	local channel			= P.Aqueous
	local volumeToBePumped	= P.Quantity(200, "uL")
	local pumpSpeed			= P.Quantity(30, "uL/s")

	if moveToWashStation == true then
		local wash_module		= P.QueryModule(palplus_left, P.Capabilities.ILcMsWashStation)
		local penetrationDepth	= P.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
		local penetrationSpeed	= P.Quantity(baltic.SyringePenetrationSpeed, "mm/s")

		palplus_left:MoveToObject( wash_module, P.Waste )
		palplus_left:PenetrateObject( wash_module, P.Waste, penetrationDepth, penetrationSpeed )
	end
	palplus_left:SetLcToolPosition( tool, channel, P.LcToolValveOpen )
	palplus_left:Wait(P.Quantity(500, "ms"))
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, channel, volumeToBePumped, pumpSpeed, nil, true, true)
	palplus_left:Wait(P.Quantity(1000, "ms"))
	palplus_left:SetLcToolPosition( tool, channel, P.LcToolValveClose )
	if moveToHome == true then
		palplus_left:LeaveObject()
		palplus_left:MoveToHome()
	end
end

---Prime the syringe with the volumetric wash pump 2x organic, 4x aqueous
---@param context IProcedureExecutionContext
---@param palplus_left IPalParticipant
---@param cycles integer
---@param yield_function function
---@param showStatus boolean
function P.PrimeSyringeWithVolumePump(context, palplus_left, cycles, yield_function, showStatus)
	---@type IProcedureStatusParticipant
	local status 			= context:GetProcedureParticipant(baltic.LcStatusRole)
	local washpump 			= P.QueryModule(palplus_left, P.Capabilities.IPumpModule)
	local tool 				= P.QueryModule(palplus_left, P.Capabilities.IToolLc)
	local pumpSpeed     	= 50
	local volumeToBePumped  = 300
	local waitTime 			= 500		-- ms
	local cycle 			= 0

	local function primePumpAndSyringe(detergent)
				-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
		palplus_left:SetPump( washpump, detergent, P.Quantity(volumeToBePumped, "uL"), P.Quantity(pumpSpeed, "uL/s"), nil, true, false)
		palplus_left:Wait(P.Quantity(waitTime*2, "ms"))
		palplus_left:AspirateSyringe( P.Quantity(100, "uL"), P.Quantity(20, "uL/s"), nil, nil)
		palplus_left:Wait(P.Quantity(waitTime, "ms"))
		palplus_left:DispenseSyringe( P.Quantity(100, "uL"), P.Quantity(100, "uL/s"))
		while not palplus_left.IsIdle do
			yield_function()
		end
	end

	local msg = "Priming the syringe"
	if showStatus == true then status:SetStatus(msg) end

	P.PrimeLCPToolLoop(palplus_left, true, false)

	palplus_left:Wait(P.Quantity(waitTime, "ms"))
	palplus_left:SetLcToolPosition( tool, P.Organic, P.LcToolValveOpen )
	while cycle < cycles do
		primePumpAndSyringe(P.Organic)
		palplus_left:Wait(P.Quantity(1000, "ms"))
		for i=1, 3 do
			primePumpAndSyringe(P.Aqueous)
			i = i + 1
		end
		palplus_left:Wait(P.Quantity(1000, "ms"))
		cycle = cycle + 1
	end
	P.SetLCToolValve( palplus_left, P.LcToolValveClose )

	if showStatus == true then status:RemoveStatus(msg) end
	palplus_left:LeaveObject()
end

---Prime the injector port
---@param context IProcedureExecutionContext
---@param palplus_left IPalParticipant
---@param yield_function function
---@param showStatus boolean
function P.PrimeInjectorWithVolumePump(context, palplus_left, yield_function, showStatus)
	---@type IProcedureStatusParticipant
	local status	= context:GetProcedureParticipant(baltic.LcStatusRole)
	local injector	= P.QueryModule(palplus_left, P.Capabilities.IInjector)
	local washpump 	= P.QueryModule(palplus_left, P.Capabilities.IPumpModule)
	local tool 		= P.QueryModule(palplus_left, P.Capabilities.IToolLc)
	local valveI 	= P.QueryValveDrive(palplus_left, P.Capabilities.ILcInjectorValve)

	local msg = "Priming the injector port"
	if showStatus == true then status:SetStatus(msg) end

	-- Set injection valve to inject
	palplus_left:MoveValveDrive(valveI, P.Quantity(baltic.InjectionValve.Inject, "deg"))

	-- Penetrate injection port
	palplus_left:MoveToObject( injector )
	palplus_left:PenetrateWithConstForce( injector )
	-- Turn on Bürkert valve and wait 1s
	palplus_left:SetLcToolPosition( tool, P.Organic, P.LcToolValveOpen )
	palplus_left:Wait(P.Quantity(500, "ms"))

	-- Dispense 150uL of solv 2 at 10uL/s
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, P.Organic, P.Quantity(150, "uL"), P.Quantity(10, "uL/s"), nil, true, true)
	palplus_left:Wait(P.Quantity(250, "ms"))

	-- switch the valve to load
	palplus_left:MoveValveDrive(valveI, P.Quantity(baltic.InjectionValve.Load, "deg"))

	-- Dispense 50uL of solv 2 at 10uL/s
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, P.Organic, P.Quantity(50, "uL"), P.Quantity(10, "uL/s"), nil, true, true)

	-- Depenetrate 2mm and dispense 100uL of solv2 at 10uL/s
	palplus_left:Depenetrate(false, P.Quantity(2, "mm"))
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, P.Organic, P.Quantity(100, "uL"), P.Quantity(10, "uL/s"), nil, true, true)
	palplus_left:Wait(P.Quantity(250, "ms"))

	-- Dispense 500uL of solv1 at 10uL/s
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, P.Aqueous, P.Quantity(500, "uL"), P.Quantity(10, "uL/s"), nil, true, true)
	palplus_left:Wait(P.Quantity(250, "ms"))

	-- Seal the injector and dispense 200uL of solv 1 at 10uL/s in load position
	palplus_left:PenetrateWithConstForce( injector )
	palplus_left:Wait(P.Quantity(250, "ms"))
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, P.Aqueous, P.Quantity(200, "uL"), P.Quantity(10, "uL/s"), nil, true, true)
	palplus_left:Wait(P.Quantity(250, "ms"))

	-- switch injection valve to inject
	palplus_left:MoveValveDrive(valveI, P.Quantity(baltic.InjectionValve.Inject, "deg"))

	-- Dispense 50uL of solv 1 at 10uL/s
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, P.Aqueous, P.Quantity(50, "uL"), P.Quantity(10, "uL/s"), nil, true, true)
	-- wait 1s
	palplus_left:Wait(P.Quantity(1000, "ms"))

	-- Close Bürkert
	palplus_left:SetLcToolPosition( tool, P.Organic, P.LcToolValveClose )

	-- Depenetrate injector
	palplus_left:LeaveObject()

	while not palplus_left.IsIdle do
		yield_function()
	end

	if showStatus == true then status:RemoveStatus(msg) end
end

--[[
---Clean the syringe in the wash station
---@param palplus_left IPalParticipant
---@param palplus_aux IPalParticipant
---@param detergent number
---@param cycles number
function P.CleanSyringe(palplus_left, palplus_aux, detergent, cycles)
	local wash_module = P.QueryModule(palplus_aux, Capabilities.ILcMsWashStation)
	palplus_left:CleanSyringe( wash_module, detergent, P.Quantity("30 mm"), P.Quantity(40, "uL/s"), P.Quantity(500, "ms"), nil, nil, P.Quantity("30 mm"), P.Quantity(baltic.SyringeAspirationSpeed, "uL/s"), P.Quantity(80, "%"), nil, cycles )
end
--]]

---Clean the injector port
---@param palplus_left IPalParticipant
---@param palplus_aux IPalParticipant
---@param channel number
---@param volume number
---@param moveToObject boolean
---@param leaveObject boolean
function P.CleanInjector(palplus_left, palplus_aux, channel, volume, moveToObject, leaveObject)
	local injector = P.QueryModule(palplus_aux, P.Capabilities.IInjector)

	if moveToObject == true then
		palplus_left:LeaveObject()
		palplus_left:MoveToObject(injector)
		palplus_left:PenetrateWithConstForce(injector)
	end
	P.VolumeToBePumped(palplus_left, channel, volume, 10)		-- changed to a saver value -- LCToolValve is opened here
	if leaveObject == true then
		palplus_left:Wait(P.Quantity(1000,"ms"))
		P.SetLCToolValve(palplus_left, P.LcToolValveClose)
		palplus_left:Wait(P.Quantity(500,"ms"))
		palplus_left:LeaveObject()
	end
end

---Return the syringe filling
---@param execLeft IPalParticipant
---@return number [mm]
function P.GetSyringePosition(execLeft)
	---@type number
	local syringe_position = execLeft:GetAxisPositionSync(5).ReturnValue.Value
	return syringe_position * 1000		--[mm]
end

---Wash the needle before penetrating the injector
---@param context IProcedureExecutionContext
---@param palplus_left IPalParticipant
function P.PreInjectionNeedleWash(context, palplus_left)
	-- After sample aspiration, with the Bürkert valve closed, the tool penetrates completely the wash station in position 1, 2, or 2+1 for 1s
		local detergent_depth = P.Quantity(baltic.WashSolventLinerPenetrationDepth, "mm")
		local wash_module = P.QueryModule(palplus_left, P.Capabilities.ILcMsWashStation)
		local washInOrganic = context:GetArgumentValue("Pre Injection Needle Wash", "Wash In Organic")
		local washInAqueous = context:GetArgumentValue("Pre Injection Needle Wash", "Wash In Aqueous")

		P.SetLCToolValve(palplus_left, P.LcToolValveClose)
		if washInOrganic then
			palplus_left:MoveToObject( wash_module, P.Organic, true, true, true )
			palplus_left:PenetrateObject( wash_module, P.Organic, detergent_depth, P.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
			palplus_left:Wait(P.Quantity(1, "s"))
			palplus_left:LeaveObject()
		end
		if washInAqueous then
			palplus_left:MoveToObject( wash_module, P.Aqueous, true, true, true )
			palplus_left:PenetrateObject( wash_module, P.Aqueous, detergent_depth, P.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
			palplus_left:Wait(P.Quantity(1, "s"))
			palplus_left:LeaveObject()
		end
	end

---Wash the needle after sample injection
---@param palplus_left IPalParticipant
function P.PostInjectionNeedleWash(palplus_left)
	-- the LCMS tool penetrates the wash station in position 2, the Bürker valve is opened, 100uL of solvent 1 are dispensed (20uL/s), then 300uL of solvent 2 are dispensed (20uL/s). 
	local waste_depth = P.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
	local wash_module = P.QueryModule(palplus_left, P.Capabilities.ILcMsWashStation)

	P.SetLCToolValve(palplus_left, P.LcToolValveOpen)
	palplus_left:MoveToObject( wash_module, P.Waste, true, true, true )
	palplus_left:PenetrateObject( wash_module, P.Waste, waste_depth, P.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
	P.VolumeToBePumped(palplus_left, P.Organic, 200, 20)
	palplus_left:Wait(P.Quantity(1, "s"))
	P.VolumeToBePumped(palplus_left, P.Aqueous, 300, 20)
	palplus_left:Wait(P.Quantity(1, "s"))
	palplus_left:LeaveObject()
end

---Clean the injection needle in wash aqueous port and leave the object
---@param palplus_left IPalParticipant
---@param palplus_aux IPalParticipant
---@param airGap number
---@param drawSpeed number
function P.CleanNeedle(palplus_left, palplus_aux, airGap, drawSpeed)
	local detergent_depth = P.Quantity(baltic.WashSolventLinerPenetrationDepth, "mm")
	local wash_module = P.QueryModule(palplus_left, P.Capabilities.ILcMsWashStation)

	palplus_left:MoveToObject( wash_module, P.Aqueous, true, true, true )
	palplus_left:AspirateSyringe( P.Quantity(airGap, "uL"), P.Quantity(drawSpeed, "uL/s"), nil, P.Quantity(1000, "ms"))
	palplus_left:PenetrateObject( wash_module, P.Aqueous, detergent_depth, P.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
	palplus_left:Wait(P.Quantity(5,"s"))
	palplus_left:LeaveObject()
end

---Clean the injection path with the solvent from item position 5
---@param execLeft IPalParticipant
---@param execAux IPalParticipant
---@param solventVolume number
---@param pr any
---@param sleeping_yield function
function P.cleanInjectionPath(execLeft, execAux, solventVolume, pr, sleeping_yield)
	local injector = P.QueryModule(execAux, P.Capabilities.IInjector)
	local valveI = P.QueryValveDrive(execAux, P.Capabilities.ILcInjectorValve)
	local valveT = P.QueryValveDrive(execAux, P.Capabilities.ISelectorValve)

	execLeft:LeaveObject()
	execLeft:MoveToObject(injector)
	execLeft:PenetrateWithConstForce( injector )
	while not execLeft.IsIdle do sleeping_yield() end
	pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Inject)
	pr.SetValvePosition(execAux, valveT, baltic.TrapValve.InjectWaste)
	execLeft:Wait(P.Quantity(100,"ms"))
	P.VolumeToBePumped(execLeft, P.Organic, solventVolume, 10)
	while not execLeft.IsIdle do sleeping_yield() end
	execLeft:Wait(P.Quantity(1000,"ms"))
	P.SetLCToolValve(execLeft, P.LcToolValveClose)
	execLeft:Wait(P.Quantity(100,"ms"))
	execLeft:LeaveObject()
	while not execLeft.IsIdle do sleeping_yield() end
	pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Load)
end

---Print the toggle wash parameters into the execution log file
---@param context IProcedureExecutionContext
---@param solvent number|string
---@param volume number
---@param speed number
---@param liftUpHight number
function P.PrintFlushPortParameter(context, solvent, volume, speed, liftUpHight)
	local refillTime = math.floor(volume / P.VolumetricPumpVolume)						-- the pump needs less then one second for refilling the pump. Calculation is: needed volume / pump volume which indicates the number of refills
	local time = (volume/speed) + refillTime

	if solvent == 1 then
		solvent = "Aqueous"
	else
		solvent = "Organic"
	end

	context:Log(baltic.devider)
	context:Log("--- FlushPortParameter ---")
	context:Log("    Solvent:        {0}", solvent)
	context:Log("    Volume:         {0} \181L", volume)
	context:Log("    Speed:          {0} \181L/sec", speed)
	context:Log("    Lift-Up Needle: {0} mm", liftUpHight)
	context:Log("    Time:           {0} sec", time)
	context:Log(baltic.devider)
end

---Flush the injector port (the LC-Tool must be already in the injector port)
---@param solvent number
---@param volume number
---@param speed number
---@param liftUpHight number
---@param palplus_left IPalParticipant
---@param palplus_aux IPalParticipant
function P.FlushPort(solvent, volume, speed, liftUpHight, palplus_left, palplus_aux)
	local tool = 			P.QueryModule(palplus_aux, P.Capabilities.IToolLc)
	local washpump = 		P.QueryModule(palplus_aux, P.Capabilities.IPumpModule)

	palplus_left:SetLcToolPosition(tool, solvent, P.LcToolValveOpen)			-- open Buerkert valve
	palplus_left:Wait(P.Quantity(100, "ms"))
	if liftUpHight > 0.1 then
		palplus_left:Depenetrate(false, P.Quantity(liftUpHight, "mm"))
	end
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump(washpump, solvent, P.Quantity(volume, "uL"), P.Quantity(speed, "uL/s"), nil, true, true)

	palplus_left:Wait(P.Quantity(100, "ms"))
	palplus_left:SetLcToolPosition(tool, solvent, P.LcToolValveClose)			-- close Buerkert valve
end

---Flushing the LCP tool to the washing stations waste port
---@param volume integer
---@param palplus_left IPalParticipant
---@param palplus_aux IPalParticipant
---@return number [sec]
function P.FlushLCPTool(volume, pumpSpeed, palplus_left, palplus_aux)
	local wash_module = P.QueryModule(palplus_aux, P.Capabilities.ILcMsWashStation)
	local tool = P.QueryModule(palplus_aux, P.Capabilities.IToolLc)
	local washpump = P.QueryModule(palplus_aux, P.Capabilities.IPumpModule)
	local timeToFinish = (volume / pumpSpeed) + 2

	palplus_left:LeaveObject()
	palplus_left:MoveToObject( wash_module, P.Waste )
	palplus_left:PenetrateObject( wash_module, P.Waste, P.Quantity(baltic.WashSolventLinerPenetrationDepth, "mm"), P.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))

	palplus_left:SetLcToolPosition( tool, P.Aqueous, P.LcToolValveOpen )
	palplus_left:Wait(P.Quantity(500, "ms"))
	-- SetPump( target, detergent, volume, speed, nil, turn pump on, block until volume is pumped)
	palplus_left:SetPump( washpump, P.Aqueous, P.Quantity(volume, "uL"), P.Quantity(pumpSpeed, "uL/s"), nil, true, true)
	palplus_left:Wait(P.Quantity(1000, "ms"))
	palplus_left:SetLcToolPosition( tool, P.Aqueous, P.LcToolValveClose )

	palplus_left:LeaveObject()
	palplus_left:MoveToHome()

	return timeToFinish
end

return P
