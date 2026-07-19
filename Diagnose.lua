-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

﻿-- require('lldebugger').start()

local Date = "2025/08/27"

luanet.load_assembly("Bruker.Lc")

---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---@param context InitHelper
function Initialize(context)
    local baltic = require "baltic"

	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.Hidden = false
	context.LedState = LedState.Diagnostics

	context:DeclareParameter("separator", nil, nil, "custom")
	context:DeclareParameter("trap", "none", nil, "custom")

	context.Name		= "Diagnostics"
	context.Description	= "Diagnose the LC system"

	context:DeclareParameter("Basic", true, nil, "radio", false, "Group diagnostic routines", "", 0, "A")
	context:DeclareParameter("Flow restriction test", true, nil, "check", false, "Flow restriction test @ 100 bar", "FlowTest.PNG", 30)
	context:DeclareParameter("High-pressure leak test", true, nil, "check", false, "Leak test @ high pressure", "LeakTest.PNG", 30)
--	context:DeclareParameter("High-pressure leak test", true, nil, "check", false, "Leak test @ 1000/1200 bar", "LeakTest.PNG", 30)
	context:DeclareParameter("Low-pressure leak test", true, nil, "check", false, "Low pressure leak test pump A and B @ 50 bar", "LPLT pumps AB.PNG", 30)

	context:DeclareParameter("Advanced", false, nil, "radio", false, "Individual diagnostic routines", "", 0, "A")
	context:DeclareParameter("Flow restriction test injection system", false, nil, "check", false, "Flow restriction test injection system @ 100 bar", "FRT injection system.PNG", 30)
	context:DeclareParameter("Flow restriction test pump A mixTee", false, nil, "check", false, "Flow restriction test pump A mixTee @ 100 bar", "FRT pump A mixTee.PNG", 30)
	context:DeclareParameter("Flow restriction test pump B mixTee", false, nil, "check", false, "Flow restriction test pump B mixTee @ 100 bar", "FRT pump B mixTee.PNG", 30)
	context:DeclareParameter("Leak test pump A", false, nil, "check", false, "Leak test pump A @ high pressure", "LT pump A.PNG", 30)
	context:DeclareParameter("Leak test pump B", false, nil, "check", false, "Leak test pump B @ high pressure", "LT pump B.PNG", 30)
--	context:DeclareParameter("Leak test pump A", false, nil, "check", false, "Leak test pump A @ 1000/1200 bar", "LT pump A.PNG", 30)
--	context:DeclareParameter("Leak test pump B", false, nil, "check", false, "Leak test pump B @ 1000/1200 bar", "LT pump B.PNG", 30)
	if context.AppKey.Special == baltic.FactoryDiag then
		context:DeclareParameter("Continuous leak test pumps", false, nil, "check", false, "Continuous display of pump leakage", "", 30)
	end
	context:DeclareParameter("Leak test mixTee system", false, nil, "check", false, "Leak test mixTee system @ high pressure", "LT mixTee system.PNG", 30)
	context:DeclareParameter("Leak test injection system", false, nil, "check", false, "Leak test injection system @ high pressure", "LT injection system.PNG", 30)
--	context:DeclareParameter("Leak test mixTee system", false, nil, "check", false, "Leak test mixTee system @ 1000/1200 bar", "LT mixTee system.PNG", 30)
--	context:DeclareParameter("Leak test injection system", false, nil, "check", false, "Leak test injection system @ 1000/1200 bar", "LT injection system.PNG", 30)
	--> Service parameter
	context:DeclareParameter("Disable limits for leakage detection", false, nil, "check", true)
	--< Service parameter

	context:DeclareParameter("Separator0", "", nil, "separator", "", "")

	context:DeclareParameter("Test volumetric wash pump", false, nil, "radio", false, "Pump fluid into washstation item positon 5", "ItemPositions.PNG", 0, "A")
	context:DeclareParameter("Pump 1", true, "", "radio", false, "Pumping fluid into washstation item position 5", "", 30, "B")
	context:DeclareParameter("Pump 2", false, "", "radio", false, "Pumping fluid into washstation item position 5", "", 30, "B")

	if context.AppKey.Special == baltic.Production then
		context:DeclareParameter("Flush system", false, nil, "check", "Purge pumps, flush mixTee", nil)
		context:DeclareParameter("Flush cycles", 70, "cycles", "integer")
		context:DeclareParameter("Low pressure test", false, nil, "check", "Low pressure leak test", nil)
		context:DeclareParameter("High pressure test", false, nil, "check", "High pressure leak test", nil)
		context:DeclareParameter("Test pressure", 1000, "bar", "integer")
		context:DeclareParameter("Wait time T1", 30,"sec", "integer")
		context:DeclareParameter("Wait time T2", 30, "sec", "integer")
	end
end

---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (_, context)
-- This function is called when the method is uploaded (Upload method or Acquisition start)
-- All errors are surpressed at the moment
	local validation = require "validation"
	validation.verify_specified(context, "trap")
	validation.verify_specified(context, "separator")
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
	---@type SelfTest
	local selfDiagnose = { 	showMessage = true,
							isSelfTest = false,
							isService = false,
							self_LT_HP = false,
							self_LT_LP_AB = false,
							self_LT_HP_AB = false,
							self_LT_HP_IS_RT = false,
							self_LT_P_RT = false,
							self_FRT_B_MT = false,
							self_FRT_IS = false,
							self_FRT_A_MT = false,
						    self_Prepare_and_Flush = false }

	local isAdvanced = context:GetArgumentValue("Advanced")
	if  isAdvanced then
		context:Log("Starting Advanced Diagnostics")
	else
		context:Log("Starting Basic Diagnostics")
	end
	local fvd = require "Diagnostics"
	local core = require("proteyolute_core")
	-- Diagnostics manages its own pump reference

	local pressure = {1000}
--	if (installed.MaxPumpPressure > 1200) then pressure = {1000, 1200} end		-- PNS-763
	fvd.diagnostics(installed, context, pressure, isAdvanced, selfDiagnose)
end
