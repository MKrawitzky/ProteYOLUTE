-- require('lldebugger').start()

local Date = "2025/08/27"

luanet.load_assembly("Bruker.Lc")

---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---@param context InitHelper
function Initialize(context)
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.Hidden = false
	context.IsService = true
	context.LedState = LedState.SelfTest

	context.Name		= "Self-test"
	context.Description	= "Automated self-test procedures"

	-- this is needed for 'diagnostic.lua'
	context:DeclareParameter("trap", "none", nil, "custom")
	context:DeclareParameter("separator", nil, nil, "custom")

	--> Service parameter
	context:DeclareParameter("Disable limits for leakage detection", false, nil, "check", true)
	--< Service parameter
	context:DeclareParameter("Self LT High pressure", false, nil, "check", false, "Leak test @ high pressure", "LeakTest.PNG", 30)
	context:DeclareParameter("Self HPLC seals conditioning", false, nil, "check", false, "High pressure leak test pump A and B @ high pressure", "", 30)
--	context:DeclareParameter("Self LT High pressure", false, nil, "check", false, "Leak test @ 1000/1200 bar", "LeakTest.PNG", 30)
--	context:DeclareParameter("Self HPLC seals conditioning", false, nil, "check", false, "High pressure leak test pump A and B @ 1200 bar", "", 30)
	context:DeclareParameter("Self LT Pressure ramp test", false, nil, "check", false, "Testing the pumps for the leakrate at different pressures", "", 30)
	context:DeclareParameter("Self LT Low pressure pump A and B", false, nil, "check", false, "Low pressure leak test pump A and B @ 50 bar", "", 30)

	context:DeclareParameter("Separator0", "", nil, "separator", "", "")

	context:DeclareParameter("Self FRT Injection system", false, nil, "check", false, "Flow restriction test injection system @ 100 bar", "FRT injection system.PNG", 30)
	context:DeclareParameter("Self FRT Pump A mixTee", false, nil, "check", false, "Flow restriction test pump A mixTee @ 100 bar", "FRT pump A mixTee.PNG", 30)
	context:DeclareParameter("Self FRT Pump B mixTee", false, nil, "check", false, "Flow restriction test pump B mixTee @ 100 bar", "FRT pump B mixTee.PNG", 30)
end

---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (_, context)
-- This function is called when the method is uploaded (Upload method or Acquisition start)
-- All errors are surpressed at the moment
	local v = require "validation"

	v.verify_specified(context, "Self LT High pressure")
	v.verify_specified(context, "Self LT Low pressure pump A and B")
	v.verify_specified(context, "Self HPLC seals conditioning")
	v.verify_specified(context, "Self LT Pressure ramp test")
	v.verify_specified(context, "Self FRT Pump B mixTee")
	v.verify_specified(context, "Self FRT Injection system")
	v.verify_specified(context, "Self FRT Pump A mixTee")
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
	---@type SelfTest
	local selfDiagnose = { showMessage = true,
						   isSelfTest = true,
						   isService = false,
						   self_LT_HP = context:GetArgumentValue("Self LT High pressure"),
						   self_LT_LP_AB = context:GetArgumentValue("Self LT Low pressure pump A and B"),
						   self_LT_HP_AB = context:GetArgumentValue("Self HPLC seals conditioning"),
						   self_LT_P_RT = context:GetArgumentValue("Self LT Pressure ramp test"),
						   self_FRT_B_MT = context:GetArgumentValue("Self FRT Pump B mixTee"),
						   self_FRT_IS = context:GetArgumentValue("Self FRT Injection system"),
						   self_FRT_A_MT = context:GetArgumentValue("Self FRT Pump A mixTee"),
						   self_LT_HP_IS_RT = false,
						   self_Prepare_and_Flush = true }

	context:Log("Starting Self Diagnostics")
	local fvd = require "Diagnostics"
	local pressure = {1000}
--	if (installed.MaxPumpPressure > 1200) then pressure = {1000, 1200} end			-- PNS-763
	if context:GetArgumentValue("Self LT Pressure ramp test") == true then
		if (installed.MaxPumpPressure > 1200) then
--			pressure = {50, 100, 250, 500, 800, 1000, 1200, 500, 50}				-- PNS-763
			pressure = {50, 100, 250, 500, 800, 1000, 500, 50}						-- PNS-763
		else
			pressure = {50, 100, 250, 500, 800, 1000, 500, 50}
		end
	end
	fvd.diagnostics(installed, context, pressure, false, selfDiagnose)
end
