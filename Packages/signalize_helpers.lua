-- signalize_helpers.lua
-- Enhanced diagram signalization for real-time flow path visualization
-- Shows active flow paths with color-coded solvent channels

local Date = "2026/07/04"

local P = {}

---Signalize the complete Pump A flow path from solvent to valve output
---@param context IProcedureExecutionContext
---@param color table RGB color {r, g, b} from baltic.ColorsRGB
function P.SignalizePumpAPath(context, color)
	local baltic = require "baltic"
	local N = baltic.Naming
	context:Signalize(color,
		N.SolventA, N.SolventToValveA,
		N.PumpA, N.PumpARearToValve, N.PumpAFrontToValve,
		N.ValveA, N.ValveAGroove,
		N.ValveAToFS, N.FlowA, N.FSAToMixTee
	)
end

---Signalize the complete Pump B flow path from solvent to valve output
---@param context IProcedureExecutionContext
---@param color table RGB color {r, g, b} from baltic.ColorsRGB
function P.SignalizePumpBPath(context, color)
	local baltic = require "baltic"
	local N = baltic.Naming
	context:Signalize(color,
		N.SolventB, N.SolventToValveB,
		N.PumpB, N.PumpBRearToValve, N.PumpBFrontToValve,
		N.ValveB, N.ValveBGroove,
		N.ValveBToFS, N.FlowB, N.FSBToMixTee
	)
end

---Signalize the mixing tee and downstream path through the trap valve
---@param context IProcedureExecutionContext
---@param color table RGB color
function P.SignalizeMixTeePath(context, color)
	local baltic = require "baltic"
	local N = baltic.Naming
	context:Signalize(color, N.MixTee, N.MixTeeToTrapValve, N.ValveT)
end

---Signalize the separation column path
---@param context IProcedureExecutionContext
---@param color table RGB color
function P.SignalizeSeparationPath(context, color)
	local baltic = require "baltic"
	local N = baltic.Naming
	context:Signalize(color, N.ValveTToSeparator, N.Separator, N.TransferLine)
end

---Signalize the trap column path
---@param context IProcedureExecutionContext
---@param color table RGB color
function P.SignalizeTrapPath(context, color)
	local baltic = require "baltic"
	local N = baltic.Naming
	context:Signalize(color, N.Trap, N.ValveTShortGroove)
end

---Signalize the injection path (loop + injection valve)
---@param context IProcedureExecutionContext
---@param color table RGB color
function P.SignalizeInjectionPath(context, color)
	local baltic = require "baltic"
	local N = baltic.Naming
	context:Signalize(color, N.ValveI, N.Loop, N.ValveAToInjectionValve, N.InjectToTrap)
end

---Signalize the waste paths
---@param context IProcedureExecutionContext
---@param color table RGB color
function P.SignalizeWaste(context, color)
	local baltic = require "baltic"
	local N = baltic.Naming
	context:Signalize(color, N.TrapValveToWaste, N.WasteForValveT)
end

-- ============================================================
-- Composite Flow Visualizations
-- ============================================================

---Show the full elution flow path: both pumps -> mixing tee -> trap valve -> column -> waste
---@param context IProcedureExecutionContext
function P.ShowElutionFlow(context)
	local baltic = require "baltic"
	local C = baltic.ColorsRGB
	P.SignalizePumpAPath(context, C.Blue)
	P.SignalizePumpBPath(context, C.Red)
	P.SignalizeMixTeePath(context, C.Purple)
	P.SignalizeSeparationPath(context, C.Green)
end

---Show the loading flow path: pump A -> injection valve -> loop -> trap valve -> trap -> waste
---@param context IProcedureExecutionContext
function P.ShowLoadingFlow(context)
	local baltic = require "baltic"
	local C = baltic.ColorsRGB
	P.SignalizePumpAPath(context, C.Blue)
	P.SignalizeInjectionPath(context, C.Goldenrod)
	P.SignalizeTrapPath(context, C.Green)
	P.SignalizeWaste(context, C.Gray)
end

---Show the idle state: everything dim
---@param context IProcedureExecutionContext
function P.ShowIdleState(context)
	local baltic = require "baltic"
	context:Signalize(baltic.ColorsRGB.Gray, baltic.SignalizeAll)
end

---Show error state: highlight the problem component in red
---@param context IProcedureExecutionContext
---@param componentNames table list of component naming strings to highlight
function P.ShowErrorHighlight(context, componentNames)
	local baltic = require "baltic"
	-- First dim everything
	context:Signalize(baltic.ColorsRGB.LightGray, baltic.SignalizeAll)
	-- Then highlight the problem components in red
	for _, name in ipairs(componentNames) do
		context:Signalize(baltic.ColorsRGB.Red, name)
	end
end

---Update flow sensor text displays with current values
---@param context IProcedureExecutionContext
---@param pump Pump
---@param zr Zirconium
function P.UpdateFlowDisplay(context, pump, zr)
	local baltic = require "baltic"
	local pf = require "pump_functions"
	local fA = pf.noExp(pump:GetCurrentFlow(zr.A), 3)
	local fB = pf.noExp(pump:GetCurrentFlow(zr.B), 3)

	-- Color-code based on whether flow is active
	local colorA = baltic.ColorsRGB.Gray
	local colorB = baltic.ColorsRGB.Gray
	if fA > 0.01 then colorA = baltic.ColorsRGB.Blue end
	if fB > 0.01 then colorB = baltic.ColorsRGB.Red end

	context:SignalizeText(colorA, baltic.Naming.FlowA)
	context:SignalizeText(colorB, baltic.Naming.FlowB)
end

return P
