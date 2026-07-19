-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
--
-- proteyolute_core.lua — Master integration module
-- Drop this into any procedure's Main() function to automatically enable:
--   - SQLite data logging (replaces CSV)
--   - Smart column/trap monitoring
--   - Pressure anomaly detection
--   - Intelligent error recovery
--   - Real-time dashboard updates
--   - Health counter tracking
--   - Valve switch tracking
--   - Proactive pre-run checks
--
-- Usage in any procedure's Main():
--   local core = require("proteyolute_core")
--   local session = core.beginProcedure(context, installed, pump, zr)
--   -- ... your procedure code ...
--   session:finish("completed")
--
-- Usage for monitored pressure/flow loops:
--   session:tick(pump)  -- call every 1-2 seconds during gradient
--
-- Usage for valve switches:
--   session:valve("A", "MixTee", 240)
-------------------------------------------------------------------------------

local P = {}

local smart_data = require("smart_data")
local error_recovery = require("error_recovery")

-- ─── Session Object ──────────────────────────────────────────────────

local Session = {}
Session.__index = Session

--- Begin a new procedure session. Call at the start of Main().
---@param context IProcedureExecutionContext
---@param installed IInstalledHardwareContext
---@param pump Pump
---@param zr table Zirconium reference
---@param procedureName string|nil Override procedure name
---@return Session
function P.beginProcedure(context, installed, pump, zr, procedureName)
    local self = setmetatable({}, Session)

    self.context = context
    self.installed = installed
    self.pump = pump
    self.zr = zr
    self.procedureName = procedureName or context.Name or "Unknown"
    self.startTime = os.clock()
    self.tickCount = 0
    self.lastValveA = nil
    self.lastValveB = nil
    self.lastValveI = nil
    self.lastValveT = nil
    self.gradientActive = false
    self.gradientDuration = 0
    self.finished = false

    -- Initialize smart data engine
    smart_data.initialize()

    -- Reset error recovery for new run
    error_recovery.resetForNewRun()

    -- Start database run
    local method = self.procedureName
    local operator = nil -- Could be extended to read from context
    self.runId = smart_data.startRun(method, self.procedureName, operator)

    -- Update system state
    smart_data.updateStatus("system_state", "running")

    -- Run pre-run checks and log warnings
    local alerts = smart_data.preRunCheck()
    if alerts then
        local alertCount = 0
        -- Iterate through .NET list
        local ok, _ = pcall(function()
            for i = 0, alerts.Count - 1 do
                local alert = alerts[i]
                alertCount = alertCount + 1
                if alert.Severity == "error" then
                    context:Log("PRE-RUN ALERT: {0}", alert.Message)
                    context:Log("  Recommendation: {0}", alert.Recommendation)
                elseif alert.Severity == "warning" then
                    context:Log("Pre-run warning: {0}", alert.Message)
                end
            end
        end)
        -- Try Lua table iteration if .NET iteration failed
        if not ok then
            pcall(function()
                for _, alert in ipairs(alerts) do
                    alertCount = alertCount + 1
                    context:Log("Pre-run: [{0}] {1}", alert.Severity or "info", alert.Message or "")
                end
            end)
        end
        if alertCount > 0 then
            context:Log("ProteYOLUTE: {0} pre-run alert(s) logged", alertCount)
        end
    end

    context:Log("ProteYOLUTE session started — Run #{0}", self.runId)

    return self
end

-- ─── Real-Time Monitoring ────────────────────────────────────────────

--- Call every 1-2 seconds during a run to log pressure/flow and check for anomalies.
--- Returns "continue", "retry", "safe_state", or "shutdown"
---@param pump Pump (optional, uses self.pump if nil)
---@param expectedPressure number|nil Expected pressure for anomaly detection
---@param expectedFlow number|nil Expected flow for anomaly detection
---@return string action
---@return string|nil reason
function Session:tick(pump, expectedPressure, expectedFlow)
    pump = pump or self.pump
    self.tickCount = self.tickCount + 1

    -- Read current values
    local pressA = 0
    local pressB = 0
    local flowA = 0
    local flowB = 0

    pcall(function() pressA = pump:GetCurrentPressure("A") or 0 end)
    pcall(function() pressB = pump:GetCurrentPressure("B") or 0 end)
    pcall(function()
        flowA = pump:GetCurrentFlow("A") or 0
        if flowA == 0 then flowA = pump:GetSetFlow("A") or 0 end
    end)
    pcall(function()
        flowB = pump:GetCurrentFlow("B") or 0
        if flowB == 0 then flowB = pump:GetSetFlow("B") or 0 end
    end)

    -- Log to database
    smart_data.logPressure("A", pressA)
    smart_data.logPressure("B", pressB)
    smart_data.logFlow("A", flowA)
    smart_data.logFlow("B", flowB)

    -- Log gradient if active
    if self.gradientActive then
        local elapsed = os.clock() - self.gradientStartTime
        local pctB = 0
        pcall(function()
            pctB = pump:GetComposition("B") or 0
        end)
        smart_data.logGradient(pctB, flowA + flowB, pressA, pressB)

        local progress = self.gradientDuration > 0
            and math.min(100, (elapsed / 60 / self.gradientDuration) * 100) or 0
        smart_data.updateStatus("gradient_progress_pct", progress)
    end

    -- Check system health (error recovery)
    local action, reason = error_recovery.checkSystemHealth(
        self.context, pump, expectedPressure or 0, expectedFlow or 0)

    if action == "shutdown" then
        self.context:Log("CRITICAL: {0}", reason)
        error_recovery.emergencyShutdown(self.context, pump, self.zr, reason)
        self.finished = true
    elseif action == "safe_state" then
        self.context:Log("WARNING: {0} — attempting safe-state recovery", reason)
        local recovered = error_recovery.enterSafeState(self.context, pump, self.zr, reason)
        if not recovered then
            error_recovery.emergencyShutdown(self.context, pump, self.zr,
                "Safe-state recovery failed: " .. reason)
            self.finished = true
        end
    end

    return action, reason
end

-- ─── Valve Tracking ──────────────────────────────────────────────────

--- Log a valve switch. Call whenever you change a valve position.
---@param valve string "A", "B", "I", or "T"
---@param position string Position name (e.g., "MixTee", "Inject", "Load")
---@param angle number|nil Angle in degrees
function Session:valve(valve, position, angle)
    local fromPos = nil
    if valve == "A" then fromPos = self.lastValveA; self.lastValveA = position
    elseif valve == "B" then fromPos = self.lastValveB; self.lastValveB = position
    elseif valve == "I" then fromPos = self.lastValveI; self.lastValveI = position
    elseif valve == "T" then fromPos = self.lastValveT; self.lastValveT = position
    end

    smart_data.logValveSwitch(valve, fromPos, position, angle)
end

-- ─── Gradient Tracking ───────────────────────────────────────────────

--- Mark gradient start. Enables gradient progress tracking and anomaly baselining.
---@param durationMin number Gradient duration in minutes
---@param method string|nil Method name for baseline profiling
function Session:startGradient(durationMin, method)
    self.gradientActive = true
    self.gradientDuration = durationMin
    self.gradientStartTime = os.clock()
    smart_data.startGradientMonitoring(method or self.procedureName, durationMin)
    smart_data.updateStatus("system_state", "gradient")
end

--- Mark gradient end.
function Session:stopGradient()
    self.gradientActive = false
    smart_data.stopGradientMonitoring()
    smart_data.updateStatus("gradient_progress_pct", 100)
end

-- ─── Data Logging (CSV replacement) ──────────────────────────────────

--- Log a diagnostic/calibration value. Drop-in replacement for csv_file_logging.
---@param test string Test name
---@param parameter string Parameter name
---@param value number|string Value
---@param unit string Unit
---@param decimalPlaces number|nil
function Session:log(test, parameter, value, unit, decimalPlaces)
    smart_data.logValue(self.context, "results", test, parameter, value, unit, decimalPlaces)
end

--- Log an error with context.
---@param severity string "Info", "Warning", "Error"
---@param message string
function Session:logError(severity, message)
    smart_data.logError(severity, message, self.procedureName)
end

-- ─── Protected Execution ─────────────────────────────────────────────

--- Execute a procedure step with full error recovery (retry + safe-state + shutdown).
---@param stepFunc function The step to execute
---@param stepName string Human-readable name
---@return boolean success
function Session:execute(stepFunc, stepName)
    return error_recovery.executeStep(self.context, self.pump, self.zr, stepFunc, stepName)
end

--- Execute with simple retry (no safe-state escalation).
---@param func function
---@param description string
---@return boolean success
---@return any result
function Session:retry(func, description)
    return error_recovery.withRetry(self.context, func, description)
end

-- ─── Session Lifecycle ───────────────────────────────────────────────

--- Finish the session. Call at the end of Main().
---@param status string "completed", "failed", or "aborted"
---@param notes string|nil
function Session:finish(status)
    if self.finished then return end
    self.finished = true

    status = status or "completed"
    local elapsed = os.clock() - self.startTime

    -- Stop gradient if still running
    if self.gradientActive then
        self:stopGradient()
    end

    -- End the database run
    smart_data.endRun(status, string.format(
        "Duration: %.1f min, %d data points logged",
        elapsed / 60, self.tickCount))

    -- Update status
    smart_data.updateStatus("system_state", "idle")
    smart_data.updateStatus("gradient_progress_pct", 0)

    self.context:Log("ProteYOLUTE session ended — %s (%.1f min, %d ticks)",
        status, elapsed / 60, self.tickCount)
end

--- Finish with failure status. Convenience for error paths.
---@param reason string
function Session:fail(reason)
    self:logError("Error", reason or "Procedure failed")
    self:finish("failed")
end

--- Finish with abort status.
function Session:abort()
    self:finish("aborted")
end

return P
