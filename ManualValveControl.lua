-- ManualValveControl.lua
-- Interactive manual valve and pump control procedure
-- Restores the hands-on control that existed in the Proxeon/Easy-nLC era
-- Allows direct switching of all valves with live pressure/flow readout

local Date = "2026/07/04"

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")
---@type JournalEntry
local JournalEntry = luanet.import_type("Bruker.Lc.JournalEntry")
---@type LogTo
local LogTo = luanet.import_type("Bruker.Lc.Business.LogTo")

local DotNetString = luanet.import_type("System.String")

local baltic = require "baltic"
local N = baltic.Naming

---@param context InitHelper
function Initialize(context)
	context.Name        = "Manual Control"
	context.Description = "Interactive manual control of valves, pumps, and LED.\nSwitch any valve to any position, monitor pressure and flow in real-time, and set custom LED themes."
	context.Hidden      = false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.IsService   = true
	context.LedState    = LedState.Service

	-- === Valve Control Section ===
	context:DeclareParameter("header_valves", "", nil, "separator", "", "")

	context:DeclareParameter("Enable valve control", false, nil, "check", false,
		"Check to manually switch valves to the selected positions", "")

	context:DeclareParameter("Valve A position", "MixTee (240)", nil, "text", false,
		"Pump A valve: Inject(0) / Solvent(60) / Compress120(120) / Waste(180) / MixTee(240) / Compress(300)",
		"pump_valve.png")

	context:DeclareParameter("Valve B position", "MixTee (240)", nil, "text", false,
		"Pump B valve: Inject(0) / Solvent(60) / Compress120(120) / Waste(180) / MixTee(240) / Compress(300)",
		"pump_valve.png")

	context:DeclareParameter("Valve I position", "Load (60)", nil, "text", false,
		"Injection valve: Inject(0) / Block(30) / Load(60)",
		"injection_valve.png")

	context:DeclareParameter("Valve T position", "Waste (0)", nil, "text", false,
		"Trap valve: Waste(0) / Trap(60) / GradientA(120) / Analytical(180) / GradientT(240) / InjectWaste(300)",
		"trap_valve.png")

	-- === Pump Control Section ===
	context:DeclareParameter("header_pumps", "", nil, "separator", "", "")

	context:DeclareParameter("Enable pump control", false, nil, "check", false,
		"Check to apply flow or pressure with the pumps", "")

	context:DeclareParameter("Pump mode", "Constant pressure", nil, "text", false,
		"Constant pressure / Constant speed / Stop", "")

	context:DeclareParameter("Pump A set point", 0, "bar or uL/min", "decimal", false,
		"Set point for pump A (pressure in bar or speed in uL/min depending on mode)", "")

	context:DeclareParameter("Pump B set point", 0, "bar or uL/min", "decimal", false,
		"Set point for pump B (pressure in bar or speed in uL/min depending on mode)", "")

	context:DeclareParameter("Duration", 60, "seconds", "integer", false,
		"How long to run the pumps (0 = until manually stopped)", "")

	-- === LED Control Section ===
	context:DeclareParameter("header_led", "", nil, "separator", "", "")

	context:DeclareParameter("Enable LED theme", false, nil, "check", false,
		"Set a custom LED color theme on the instrument", "")

	context:DeclareParameter("LED theme", "Blue & White", nil, "text", false,
		"Choose: Rainbow / Ocean / Sunset / Forest / Lava / Ice / Night / Orange & Black / Red & Black / Blue & White / Green & Black / Purple & Black / Amber & Black / Pink & Black / Cyan & Black / White Pulse / Warm Glow", "")

	context:DeclareParameter("LED pattern", "Pulsating", nil, "text", false,
		"Pulsating / Moving / Solid", "")

	-- === Live Monitoring Section ===
	context:DeclareParameter("header_monitor", "", nil, "separator", "", "")

	context:DeclareParameter("Enable live monitoring", true, nil, "check", false,
		"Show real-time pressure, flow, valve positions, and pump volume in the log", "")

	context:DeclareParameter("Monitor interval", 2, "seconds", "integer", false,
		"How often to update the live readout", "")
end

---@param _ IInstalledHardwareContext
---@param __ IProcedureValidationContext
function Validate(_, __)
	-- All parameters have safe defaults, no validation needed
end

-- ============================================================
-- Helper: Parse valve position from parameter string
-- ============================================================
local function parseValvePosition(paramStr, valveType)
	-- Extract the number in parentheses, e.g. "MixTee (240)" -> 240
	local angle = string.match(paramStr, "%((%d+)%)")
	if angle then return tonumber(angle) end

	-- Try parsing as plain number
	local num = tonumber(paramStr)
	if num then return num end

	-- Try matching by name (case-insensitive first word)
	local name = string.lower(string.match(paramStr, "^(%w+)") or "")

	if valveType == "pump" then
		local map = {inject=0, solvent=60, compress120=120, waste=180, mixtee=240, compress=300}
		return map[name] or 180
	elseif valveType == "injection" then
		local map = {inject=0, block=30, load=60}
		return map[name] or 60
	elseif valveType == "trap" then
		local map = {waste=0, trap=60, gradienta=120, analytical=180, gradientt=240, injectwaste=300}
		return map[name] or 0
	end
	return 0
end

-- ============================================================
-- Helper: Get valve position name from angle
-- ============================================================
local function pumpValveName(angle)
	local names = {[0]="Inject", [60]="Solvent", [120]="Compress120", [180]="Waste", [240]="MixTee", [300]="Compress"}
	return names[angle] or tostring(angle).."deg"
end
local function injValveName(angle)
	local names = {[0]="Inject", [30]="Block", [60]="Load"}
	return names[angle] or tostring(angle).."deg"
end
local function trapValveName(angle)
	local names = {[0]="Waste", [60]="Trap", [120]="GradientA", [180]="Analytical", [240]="GradientT", [300]="InjectWaste"}
	return names[angle] or tostring(angle).."deg"
end

-- ============================================================
-- Main Execution
-- ============================================================

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main(installed, context)
	local pf  = require "pump_functions"
	local pp  = require "palplus"
	local pr  = require "PreRunFunctions"
	local zr  = require "zirconium"
	local led = require "led_effects"

	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)

	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end

	-- Reset diagram
	pr.Signalize_Reset(context)
	context:ShowComposition(true)

	context:Log("=== Manual Control started: {0} ===", os.date())
	context:Log("Lua date: {0}", Date)

	-- Init pumps
	if not zr.InitChannel(context, pump, zr.A) then
		context:Report(N.PumpA, Severity.Error, true,
			"Pump A initialization failed. The pump could not enter manual control mode.\n\nCheck that the instrument is powered on and connected.")
		context:Abort()
	end
	if not zr.InitChannel(context, pump, zr.B) then
		context:Report(N.PumpB, Severity.Error, true,
			"Pump B initialization failed. The pump could not enter manual control mode.\n\nCheck that the instrument is powered on and connected.")
		context:Abort()
	end

	local function sleep_100() context:Sleep(100) end

	-- Get PAL valve drives
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)

	-- ========================================
	-- LED Theme
	-- ========================================
	local enableLed = context:GetArgumentValue("Enable LED theme")
	if enableLed then
		local themeName = context:GetArgumentValue("LED theme")
		local patternName = string.lower(context:GetArgumentValue("LED pattern") or "pulsating")
		context:Log("Setting LED theme: {0} ({1})", themeName, patternName)
		status:SetStatus("Setting LED theme: "..themeName)

		local theme = led.Themes[themeName]
		if theme and theme.cycle then
			-- Rainbow: run a few cycles so the user sees it
			led.ApplyTheme(pump, themeName, patternName)
			context:Log("Rainbow LED mode activated")
		else
			led.ApplyTheme(pump, themeName, patternName)
		end
		status:RemoveStatus("Setting LED theme: "..themeName)
	end

	-- ========================================
	-- Valve Control
	-- ========================================
	local enableValves = context:GetArgumentValue("Enable valve control")
	if enableValves then
		context:Log("--- Valve Control ---")
		status:SetStatus("Switching valves...")

		-- Read current positions first
		local curA = pump:GetSetManualValvePosition(zr.A)
		local curB = pump:GetSetManualValvePosition(zr.B)
		local curI = pp.GetInjectorValvePosition(execAux)
		local curT = pp.GetTrapValvePosition(execAux)

		context:Log("Current valve positions:")
		context:Log("  Valve A: {0} ({1})", curA, pumpValveName(curA))
		context:Log("  Valve B: {0} ({1})", curB, pumpValveName(curB))
		if curI then context:Log("  Valve I: {0} ({1})", curI, injValveName(curI)) end
		if curT then context:Log("  Valve T: {0} ({1})", curT, trapValveName(curT)) end

		-- Parse requested positions
		local reqA = parseValvePosition(context:GetArgumentValue("Valve A position"), "pump")
		local reqB = parseValvePosition(context:GetArgumentValue("Valve B position"), "pump")
		local reqI = parseValvePosition(context:GetArgumentValue("Valve I position"), "injection")
		local reqT = parseValvePosition(context:GetArgumentValue("Valve T position"), "trap")

		-- Reduce pressure before switching if pressure is high
		local pressA = pump:GetCurrentPressure(zr.A)
		local pressB = pump:GetCurrentPressure(zr.B)
		if pressA > 20 or pressB > 20 then
			context:Log("Reducing pressure before valve switch (A: {0} bar, B: {1} bar)", pf.noExp(pressA,1), pf.noExp(pressB,1))
			status:SetStatus("Reducing pressure before valve switch...")
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 15, 15, 120, sleep_100, false)
			status:RemoveStatus("Reducing pressure before valve switch...")
		end

		-- Switch pump valves
		if reqA ~= curA then
			context:Log("Switching Valve A: {0} -> {1} ({2})", pumpValveName(curA), pumpValveName(reqA), reqA)
			-- Signalize the target path
			local linkIdx = (reqA / 60) + 1
			local linkName = baltic.ValveAngleToLinkList[N.ValveA][linkIdx]
			if linkName and linkName ~= N.None then
				context:Signalize(baltic.ColorsRGB.Blue, N.ValveA, linkName)
			end
			zr.SetValvePosition(context, pump, zr.A, reqA)
		end

		if reqB ~= curB then
			context:Log("Switching Valve B: {0} -> {1} ({2})", pumpValveName(curB), pumpValveName(reqB), reqB)
			local linkIdx = (reqB / 60) + 1
			local linkName = baltic.ValveAngleToLinkList[N.ValveB][linkIdx]
			if linkName and linkName ~= N.None then
				context:Signalize(baltic.ColorsRGB.Red, N.ValveB, linkName)
			end
			zr.SetValvePosition(context, pump, zr.B, reqB)
		end

		-- Switch injection valve
		if curI == nil or math.abs(reqI - curI) > baltic.palValveDriveTolerance then
			context:Log("Switching Valve I: {0} -> {1} ({2})", injValveName(curI or 0), injValveName(reqI), reqI)
			context:Signalize(baltic.ColorsRGB.Purple, N.ValveI)
			pr.SetValvePosition(execAux, valveI, reqI)
		end

		-- Switch trap valve
		if curT == nil or math.abs(reqT - curT) > baltic.palValveDriveTolerance then
			context:Log("Switching Valve T: {0} -> {1} ({2})", trapValveName(curT or 0), trapValveName(reqT), reqT)
			context:Signalize(baltic.ColorsRGB.Goldenrod, N.ValveT)
			pr.SetValvePosition(execAux, valveT, reqT)
		end

		context:Sleep(1000)

		-- Log final positions
		context:Log("Final valve positions:")
		context:Log("  Valve A: {0} ({1})", pump:GetSetManualValvePosition(zr.A), pumpValveName(pump:GetSetManualValvePosition(zr.A)))
		context:Log("  Valve B: {0} ({1})", pump:GetSetManualValvePosition(zr.B), pumpValveName(pump:GetSetManualValvePosition(zr.B)))
		local finalI = pp.GetInjectorValvePosition(execAux)
		local finalT = pp.GetTrapValvePosition(execAux)
		if finalI then context:Log("  Valve I: {0} ({1})", finalI, injValveName(finalI)) end
		if finalT then context:Log("  Valve T: {0} ({1})", finalT, trapValveName(finalT)) end

		status:RemoveStatus("Switching valves...")
		context:Report("Valve Control", Severity.Tip, false, "All valves switched successfully.")
	end

	-- ========================================
	-- Pump Control
	-- ========================================
	local enablePump = context:GetArgumentValue("Enable pump control")
	if enablePump then
		local mode = string.lower(context:GetArgumentValue("Pump mode") or "stop")
		local setA = context:GetArgumentValue("Pump A set point") or 0
		local setB = context:GetArgumentValue("Pump B set point") or 0
		local duration = context:GetArgumentValue("Duration") or 60

		context:Log("--- Pump Control ---")
		context:Log("Mode: {0}, Pump A: {1}, Pump B: {2}, Duration: {3}s", mode, setA, setB, duration)

		if string.find(mode, "pressure") then
			status:SetStatus(DotNetString.Format("Constant pressure: A={0} bar, B={1} bar", setA, setB))
			pf.Manualmode_Pump_constantPressure(zr.A, setA, pump, sleep_100)
			pf.Manualmode_Pump_constantPressure(zr.B, setB, pump, sleep_100)
		elseif string.find(mode, "speed") then
			status:SetStatus(DotNetString.Format("Constant speed: A={0} uL/min, B={1} uL/min", setA, setB))
			pf.Manualmode_Pump_constantSpeed(zr.A, setA, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, setB, pump, sleep_100)
		else
			status:SetStatus("Pumps stopped")
			pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
			pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
		end

		-- Run for duration with live monitoring
		if duration > 0 then
			local endTime = os.clock() + duration
			while os.clock() < endTime do
				local pA = pf.noExp(pump:GetCurrentPressure(zr.A), 1)
				local pB = pf.noExp(pump:GetCurrentPressure(zr.B), 1)
				local fA = pf.noExp(pump:GetCurrentFlow(zr.A), 3)
				local fB = pf.noExp(pump:GetCurrentFlow(zr.B), 3)
				local remaining = math.floor(endTime - os.clock())
				context:Log("  P(A)={0} bar  P(B)={1} bar  |  F(A)={2} uL/min  F(B)={3} uL/min  |  {4}s remaining", pA, pB, fA, fB, remaining)
				context:Sleep(2000)
			end
		end

		-- Stop pumps
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
		pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
		status:RemoveStatus(nil)
		context:Report("Pump Control", Severity.Tip, false, "Pump operation completed.")
	end

	-- ========================================
	-- Live Monitoring (standalone)
	-- ========================================
	local enableMonitor = context:GetArgumentValue("Enable live monitoring")
	if enableMonitor and not enablePump then
		local interval = (context:GetArgumentValue("Monitor interval") or 2) * 1000
		context:Log("--- Live Monitoring (press Abort to stop) ---")
		context:Log("Updating every {0} seconds", interval / 1000)
		status:SetStatus("Live monitoring active...")

		-- Rainbow LED cycling during monitoring
		local rainbowStep = 1
		local isRainbow = enableLed and led.Themes[context:GetArgumentValue("LED theme") or ""] ~= nil
			and (led.Themes[context:GetArgumentValue("LED theme")].cycle == true)

		while true do
			local pA = pf.noExp(pump:GetCurrentPressure(zr.A), 1)
			local pB = pf.noExp(pump:GetCurrentPressure(zr.B), 1)
			local fA = pf.noExp(pump:GetCurrentFlow(zr.A), 3)
			local fB = pf.noExp(pump:GetCurrentFlow(zr.B), 3)
			local volA = pf.noExp(pump:GetPistonPosition(zr.A), 1)
			local volB = pf.noExp(pump:GetPistonPosition(zr.B), 1)
			local vA = pumpValveName(pump:GetSetManualValvePosition(zr.A))
			local vB = pumpValveName(pump:GetSetManualValvePosition(zr.B))
			local vI = pp.GetInjectorValvePosition(execAux)
			local vT = pp.GetTrapValvePosition(execAux)

			context:Log("PRESSURE  A: {0} bar  |  B: {1} bar", pA, pB)
			context:Log("FLOW      A: {0} uL/min  |  B: {1} uL/min", fA, fB)
			context:Log("VOLUME    A: {0} uL  |  B: {1} uL", volA, volB)
			context:Log("VALVES    A: {0}  |  B: {1}  |  I: {2}  |  T: {3}",
				vA, vB,
				vI and injValveName(vI) or "?",
				vT and trapValveName(vT) or "?")
			context:Log("---")

			-- Rainbow cycling
			if isRainbow then
				rainbowStep = led.RainbowStep(pump, rainbowStep)
			end

			context:Sleep(interval)
		end
	end

	-- ========================================
	-- Cleanup
	-- ========================================
	context:Log("=== Manual Control finished: {0} ===", os.date())
	dictator:Dispose()
end
