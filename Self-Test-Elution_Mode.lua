-- require('lldebugger').start()

local Date = "2025/08/27"

luanet.load_assembly("Bruker.Lc")

---@type GradientContainer
local GradientContainer = luanet.import_type("Bruker.Lc.Baltic.GradientContainer")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

---@param context InitHelper
function Initialize (context)
	context.Name = "Self-test elution mode"
	context.DisplayName = "Self-test"
	context.Description = "Self-test elution mode"
	context.Hidden = true
	context.DecompressOnExit = false
	context.OverwriteLogFiles = false
	context.LedState = LedState.SelfTest

	--> Service parameter
	context:DeclareASParameter("Disable limits for leakage detection", false, nil, "check", true)
	--< Service parameter
	context:DeclareASParameter("Self LT High pressure", false, "25 min")
	context:DeclareASParameter("Self LT Pressure ramp test", true, "45 min")
	context:DeclareASParameter("Self HPLC seals conditioning", false, "15 min")
	context:DeclareASParameter("Self LT Low pressure pump A and B", false, "15 min")
	context:DeclareASParameter("Self FRT Injection system", true, " 8 min")
	context:DeclareASParameter("Self FRT Pump A mixTee", true, "15 min")
	context:DeclareASParameter("Self FRT Pump B mixTee", true, "10 min")

	context:DeclareParameter("Basic", false, nil, "radio", false, "Group diagnostic routines", "", 0, "A")


	-- experiment premises
	context:DeclareParameter("uses_trap", false)
	context:DeclareParameter("uses_separator", true)
	context:DeclareParameter("is_isocratic", false)
	context:DeclareParameter("analysis_time", 60, "seconds", "integer")
	-- PBNE-657: Column oven is diconnected but this is ignored by the software
	context:DeclareParameter("is_use_oven_temperature", true)
	context:DeclareParameter("oven_temperature", 20, "deg", "decimal")

	-- all parameters that aren't set by the hystar plugin must have their values declared here
	context:DeclareParameter("sample_volume", 1, "\181L", "decimal")		-- assume 1 uL sample volume if not set at other place
	context:DeclareParameter("sample_position", nil, nil, "text")

	-- injection definition: thes may be modified in the ul/partial loop injection subroutines
	context:DeclareParameter("presample_solvent_volume", 2.0, "\181L", "decimal")
	context:DeclareParameter("presample_air_volume", 1.0, "\181L", "decimal")
	context:DeclareParameter("postsample_air_volume", 0.5, "\181L", "decimal")
	context:DeclareParameter("postsample_solvent_volume", 2.0, "\181L", "decimal")
	context:DeclareParameter("sample_aspirate_speed", 1, "\181L/s", "decimal")			-- PBNE-943
	context:DeclareParameter("sample_postaspirate_delay", 500, "ms", "decimal")
	context:DeclareParameter("sample_inject_speed", 1, "\181L/s", "decimal")
	context:DeclareParameter("calibrant_volume", 0.6, "\181L", "decimal")
	context:DeclareParameter("calibrantTime", 10, "min", "decimal") 
	context:DeclareParameter("penetration_depth", 30, "mm", "decimal") 
	-- Set default values
	context:DeclareParameter("is_bottom_sense", true)
	context:DeclareParameter("injection_method", "uLPickup", nil, "text") -- text options are "PartialLoop" and "uLPickup"

	-- these get initialized by GenerateMethod
	context:DeclareParameter("trap", nil, nil, "custom")

	context:DeclareParameter("separator", nil, nil, "custom")
	context:DeclareParameter("separator_volume", nil, "\181L", "decimal")
	context:DeclareParameter("separator_unityflow", nil, "\181L/min/bar", "decimal") -- flow per pressure unit
	context:DeclareParameter("separator_equilibration_volumemultiplier", 4, nil, "decimal")
	context:DeclareParameter("separator_equilibration_pressure", nil, "bar", "decimal")
	context:DeclareParameter("column_load_volumemultiplier", 2, nil, "decimal")
	context:DeclareParameter("column_load_pressure", nil, "bar", "decimal")

	context:DeclareParameter("separator_equil_time", 0, "min", "decimal")		-- initialize with zero for DirectInfusion
	context:DeclareParameter("column_load_time", 0, "min", "decimal")			-- initialize with zero for DirectInfusion

	context:DeclareParameter("gradient", nil, nil, "custom")
end

--- This function is called when the generate button is pressed in the method editor
function GenerateMethod (experiment, installed, context)
	local gm =    	require "PreRunFunctions"

	gm.genMethod(experiment, installed, context, false)

	local gradient = GradientContainer()
	gradient:AddSetPoint(GradientContainer.SetPoint(0, 0.1, 50))
	gradient:AddSetPoint(GradientContainer.SetPoint(experiment.AnalysisTime.TotalSeconds, 0.1, 50), true)
	context:SetArgumentValue("gradient", gradient)

end

function Validate (_, _)
-- No action required here
end

function ValidateMethod (_, _, _)
-- No action required here
end

function PreRun (_, _)
-- No action required here
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
--	if (installed.MaxPumpPressure > 1200) then pressure = {1000, 1200} end		-- PNS-763
	if context:GetArgumentValue("Self LT Pressure ramp test") == true then
		if (installed.MaxPumpPressure > 1200) then
--			pressure = {50, 100, 250, 500, 800, 1000, 1200, 500, 50}			-- PNS-763
			pressure = {50, 100, 250, 500, 800, 1000, 500, 50}					-- PNS-763
		else
			pressure = {50, 100, 250, 500, 800, 1000, 500, 50}
		end
	end
	local baltic = require "baltic"
	context:SetSignal(baltic.LcElutionReady)
	fvd.diagnostics(installed, context, pressure, false, selfDiagnose)
	context:SetSignal(baltic.LcElutionDone)
end
