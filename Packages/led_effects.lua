-- led_effects.lua
-- Custom LED effects for proteoElute
-- Provides breathing, pulsing, rainbow, and custom two-tone LED patterns
-- Uses the Pump.SetLedPattern API with LedPattern and LedColor

local Date = "2026/07/04"

luanet.load_assembly("Bruker.Lc")
luanet.load_assembly("Bruker.Zirconium.Pump.Communication")

local LedColor = luanet.import_type("Bruker.Lc.Business.LedColor")
local LedPattern = luanet.import_type("Bruker.Zirconium.Pump.Communication.LedPattern")

local P = {}

-- ============================================================
-- Color Presets (RGB values for LedColor.FromRgb)
-- ============================================================
P.Colors = {
	Off         = {0, 0, 0},
	White       = {190, 150, 120},
	Red         = {80, 0, 0},
	Green       = {0, 180, 0},
	Blue        = {0, 0, 180},
	Yellow      = {150, 100, 0},
	Orange      = {180, 60, 0},
	Purple      = {80, 0, 120},
	Cyan        = {0, 120, 120},
	Pink        = {180, 0, 80},
	Amber       = {200, 80, 0},
	Teal        = {0, 100, 80},
	Indigo      = {40, 0, 160},
	Lime        = {60, 180, 0},
	Magenta     = {160, 0, 100},
	SoftWhite   = {55, 45, 35},
	WarmWhite   = {120, 90, 50},
	CoolBlue    = {0, 40, 120},
	DeepRed     = {120, 0, 0},
	Black       = {0, 0, 0},
}

-- Rainbow sequence for cycling
P.RainbowSequence = {
	P.Colors.Red,
	P.Colors.Orange,
	P.Colors.Yellow,
	P.Colors.Green,
	P.Colors.Cyan,
	P.Colors.Blue,
	P.Colors.Indigo,
	P.Colors.Purple,
	P.Colors.Magenta,
	P.Colors.Pink,
}

-- ============================================================
-- Named LED Themes (foreground + background pairs)
-- ============================================================
P.Themes = {
	["Rainbow"]         = {cycle = true, colors = nil},  -- special: cycles through RainbowSequence
	["Ocean"]           = {fg = P.Colors.Blue,      bg = P.Colors.Cyan},
	["Sunset"]          = {fg = P.Colors.Orange,    bg = P.Colors.Red},
	["Forest"]          = {fg = P.Colors.Green,     bg = P.Colors.Teal},
	["Lava"]            = {fg = P.Colors.Red,       bg = P.Colors.Orange},
	["Ice"]             = {fg = P.Colors.CoolBlue,  bg = P.Colors.White},
	["Night"]           = {fg = P.Colors.Indigo,    bg = P.Colors.Black},
	["Orange & Black"]  = {fg = P.Colors.Orange,    bg = P.Colors.Black},
	["Red & Black"]     = {fg = P.Colors.Red,       bg = P.Colors.Black},
	["Blue & White"]    = {fg = P.Colors.Blue,      bg = P.Colors.White},
	["Green & Black"]   = {fg = P.Colors.Green,     bg = P.Colors.Black},
	["Purple & Black"]  = {fg = P.Colors.Purple,    bg = P.Colors.Black},
	["Amber & Black"]   = {fg = P.Colors.Amber,     bg = P.Colors.Black},
	["Pink & Black"]    = {fg = P.Colors.Pink,      bg = P.Colors.Black},
	["Cyan & Black"]    = {fg = P.Colors.Cyan,      bg = P.Colors.Black},
	["White Pulse"]     = {fg = P.Colors.White,     bg = P.Colors.SoftWhite},
	["Warm Glow"]       = {fg = P.Colors.WarmWhite, bg = P.Colors.Amber},
}

-- List of theme names for UI parameter dropdowns
P.ThemeNames = {
	"Rainbow", "Ocean", "Sunset", "Forest", "Lava", "Ice", "Night",
	"Orange & Black", "Red & Black", "Blue & White", "Green & Black",
	"Purple & Black", "Amber & Black", "Pink & Black", "Cyan & Black",
	"White Pulse", "Warm Glow"
}

-- ============================================================
-- Core LED Functions
-- ============================================================

---Create a LedColor from an RGB table {r, g, b}
---@param rgb table
---@return LedColor
function P.ColorFromRGB(rgb)
	return LedColor.FromRgb(rgb[1], rgb[2], rgb[3])
end

---Set LED to a solid color (no pattern)
---@param pump Pump
---@param rgb table {r, g, b}
function P.SetSolid(pump, rgb)
	local color = P.ColorFromRGB(rgb)
	pump:SetLedPattern(LedPattern.PatternNone, color, color)
end

---Set LED to pulsating pattern with two colors
---@param pump Pump
---@param fgRgb table foreground {r, g, b}
---@param bgRgb table background {r, g, b}
function P.SetPulsating(pump, fgRgb, bgRgb)
	local fg = P.ColorFromRGB(fgRgb)
	local bg = P.ColorFromRGB(bgRgb)
	pump:SetLedPattern(LedPattern.PatternPulsating, fg, bg)
end

---Set LED to moving pattern with two colors
---@param pump Pump
---@param fgRgb table foreground {r, g, b}
---@param bgRgb table background {r, g, b}
function P.SetMoving(pump, fgRgb, bgRgb)
	local fg = P.ColorFromRGB(fgRgb)
	local bg = P.ColorFromRGB(bgRgb)
	pump:SetLedPattern(LedPattern.PatternMoving, fg, bg)
end

---Turn LED off
---@param pump Pump
function P.SetOff(pump)
	P.SetSolid(pump, P.Colors.Off)
end

-- ============================================================
-- Theme Application
-- ============================================================

---Apply a named theme with pulsating pattern
---@param pump Pump
---@param themeName string
---@param pattern string|nil "pulsating", "moving", or "solid" (default: "pulsating")
function P.ApplyTheme(pump, themeName, pattern)
	local theme = P.Themes[themeName]
	if theme == nil then
		-- Default to white pulse if theme not found
		theme = P.Themes["White Pulse"]
	end

	-- Rainbow is special - just set the first color pair, cycling handled separately
	if theme.cycle then
		local seq = P.RainbowSequence
		P.SetPulsating(pump, seq[1], seq[2])
		return
	end

	pattern = pattern or "pulsating"
	if pattern == "moving" then
		P.SetMoving(pump, theme.fg, theme.bg)
	elseif pattern == "solid" then
		P.SetSolid(pump, theme.fg)
	else
		P.SetPulsating(pump, theme.fg, theme.bg)
	end
end

---Run a rainbow color cycle (call this in a loop with yield)
---Each call advances to the next color pair in the sequence
---@param pump Pump
---@param step number current step index (1-based, wraps automatically)
---@return number next step index
function P.RainbowStep(pump, step)
	local seq = P.RainbowSequence
	local count = #seq
	local idx1 = ((step - 1) % count) + 1
	local idx2 = (step % count) + 1
	P.SetMoving(pump, seq[idx1], seq[idx2])
	return step + 1
end

-- ============================================================
-- Status-Reactive LED Helpers
-- ============================================================

---Set LED color based on system state — call from any procedure
---@param pump Pump
---@param state string "idle"|"running"|"error"|"warning"|"ready"|"busy"
function P.SetStatusColor(pump, state)
	if state == "idle" then
		P.SetPulsating(pump, P.Colors.Blue, P.Colors.CoolBlue)
	elseif state == "running" then
		P.SetMoving(pump, P.Colors.Green, P.Colors.Teal)
	elseif state == "error" then
		P.SetPulsating(pump, P.Colors.Red, P.Colors.Black)
	elseif state == "warning" then
		P.SetPulsating(pump, P.Colors.Orange, P.Colors.Black)
	elseif state == "ready" then
		P.SetSolid(pump, P.Colors.Green)
	elseif state == "busy" then
		P.SetMoving(pump, P.Colors.Amber, P.Colors.Orange)
	else
		P.SetSolid(pump, P.Colors.SoftWhite)
	end
end

---Flash the LED between two colors for attention (e.g., user action needed)
---Call in a loop with context:Sleep between calls
---@param pump Pump
---@param onRgb table {r, g, b}
---@param toggle boolean alternates each call
---@return boolean next toggle value
function P.Flash(pump, onRgb, toggle)
	if toggle then
		P.SetSolid(pump, onRgb)
	else
		P.SetSolid(pump, P.Colors.Off)
	end
	return not toggle
end

return P
