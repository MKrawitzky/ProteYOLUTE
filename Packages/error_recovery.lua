-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
--
-- error_recovery.lua — Intelligent error recovery system
-- Tiered error handling: auto-retry for transient errors, safe-state recovery
-- for persistent errors, graceful shutdown for critical errors.
-------------------------------------------------------------------------------

local P = {}

local smart_data = require("smart_data")

-- ─── Configuration ───────────────────────────────────────────────────

P.config = {
    -- Tier 1: Auto-retry thresholds
    pressure_spike_max_duration_s = 3,
    pressure_spike_max_bar_over = 80,
    flow_deviation_retry_pct = 8,
    flow_deviation_abort_pct = 20,
    max_retries_per_run = 5,
    retry_wait_s = 3,

    -- Tier 2: Safe-state recovery
    safe_state_wait_s = 10,
    safe_state_max_attempts = 2,

    -- Tier 3: Critical thresholds
    critical_pressure_bar = 1200,
    critical_flow_deviation_pct = 50,
    leak_rate_critical_ul_min = 2.0,

    -- Monitoring intervals
    pressure_check_interval_s = 1,
    flow_check_interval_s = 2,

    -- Enable/disable
    enabled = true,
    auto_retry_enabled = true,
    safe_state_enabled = true,
}

-- ─── State Tracking ──────────────────────────────────────────────────

local state = {
    retry_count = 0,
    last_pressure_a = 0,
    last_pressure_b = 0,
    last_flow_a = 0,
    last_flow_b = 0,
    pressure_spike_start = nil,
    in_recovery = false,
    recovery_tier = 0,
}

-- ─── Tier 1: Auto-Retry ─────────────────────────────────────────────

--- Wrap a function call with automatic retry on transient errors.
--- If the function throws an error that matches a transient pattern,
--- it will wait and retry up to max_retries times.
---@param context IProcedureExecutionContext
---@param func function The function to execute
---@param description string Human-readable description for logging
---@return boolean success
---@return any result_or_error
function P.withRetry(context, func, description)
    if not P.config.enabled or not P.config.auto_retry_enabled then
        return pcall(func)
    end

    local attempts = 0
    local max = P.config.max_retries_per_run - state.retry_count

    while attempts < math.max(1, max) do
        local ok, result = pcall(func)

        if ok then
            if attempts > 0 then
                context:Log("Recovery successful for '{0}' after {1} retry(ies)", description, attempts)
                smart_data.logError("Info",
                    string.format("Auto-recovered: %s after %d retries", description, attempts),
                    "error_recovery", true, "retry")
            end
            return true, result
        end

        -- Check if error is transient (retryable)
        local errMsg = tostring(result)
        if P.isTransientError(errMsg) and attempts < max - 1 then
            attempts = attempts + 1
            state.retry_count = state.retry_count + 1

            context:Log("Transient error in '{0}': {1} — retrying ({2}/{3})",
                description, errMsg, attempts, max)
            smart_data.logError("Warning",
                string.format("Transient error (retry %d/%d): %s — %s",
                    attempts, max, description, errMsg),
                "error_recovery", false, "retrying")

            context:Sleep(P.config.retry_wait_s * 1000)
        else
            -- Not transient or out of retries
            smart_data.logError("Error",
                string.format("Failed after %d attempts: %s — %s",
                    attempts + 1, description, errMsg),
                "error_recovery", false, nil)
            return false, result
        end
    end

    return false, "Max retries exceeded for: " .. description
end

--- Determine if an error message indicates a transient (retryable) condition.
---@param errMsg string
---@return boolean
function P.isTransientError(errMsg)
    local transientPatterns = {
        "timeout",
        "timed out",
        "temporarily unavailable",
        "communication error",
        "retry",
        "busy",
        "not ready",
        "pressure deviation",
        "flow deviation",
        "setpoint not reached",
    }

    local lower = errMsg:lower()
    for _, pattern in ipairs(transientPatterns) do
        if lower:find(pattern, 1, true) then
            return true
        end
    end
    return false
end

-- ─── Tier 2: Safe-State Recovery ─────────────────────────────────────

--- Move the system to a safe state. Called when a persistent error occurs
--- that can't be auto-retried but doesn't require immediate shutdown.
---@param context IProcedureExecutionContext
---@param pump Pump reference
---@param zr Zirconium reference
---@param reason string Why we're entering safe state
---@return boolean recovered Whether we recovered to a usable state
function P.enterSafeState(context, pump, zr, reason)
    if not P.config.enabled or not P.config.safe_state_enabled then
        return false
    end

    state.in_recovery = true
    state.recovery_tier = 2

    context:Log("Entering safe state: {0}", reason)
    smart_data.logError("Warning",
        "Entering safe state: " .. reason,
        "error_recovery", false, "safe_state")
    smart_data.raiseAlert("recovery.safe_state", "warning",
        "System entering safe state: " .. reason, "error_recovery")

    local ok = pcall(function()
        -- Step 1: Stop gradient / reduce flow
        context:Log("Safe state: reducing flow to minimum...")
        pcall(function()
            pump:Manualmode_Pump_constantFlow("A", 0.01)
            pump:Manualmode_Pump_constantFlow("B", 0.01)
        end)

        -- Step 2: Switch valves to waste
        context:Log("Safe state: routing to waste...")
        pcall(function()
            zr:SetValvePosition("PumpValveA", 180) -- Waste position
            zr:SetValvePosition("PumpValveB", 180) -- Waste position
            zr:SetValvePosition("TrapValve", 0)    -- Waste position
        end)

        -- Step 3: Wait for pressure to stabilize
        context:Log("Safe state: waiting for pressure stabilization ({0}s)...",
            P.config.safe_state_wait_s)
        context:Sleep(P.config.safe_state_wait_s * 1000)

        -- Step 4: Check if pressure is stable and within safe range
        local pressA = pump:GetCurrentPressure("A") or 0
        local pressB = pump:GetCurrentPressure("B") or 0
        context:Log("Safe state: pressure A={0} bar, B={1} bar", pressA, pressB)

        if pressA < 100 and pressB < 100 then
            context:Log("Safe state: pressure stabilized successfully")
            return true
        end
    end)

    state.in_recovery = false
    state.recovery_tier = 0

    if ok then
        smart_data.logError("Info",
            "Safe state recovery successful: " .. reason,
            "error_recovery", true, "safe_state_recovery")
        return true
    else
        context:Log("Safe state recovery FAILED — escalating to emergency shutdown")
        return false
    end
end

-- ─── Tier 3: Emergency Shutdown ──────────────────────────────────────

--- Emergency shutdown: decompress, all valves to waste, pumps stop.
--- Called for critical errors that risk hardware damage.
---@param context IProcedureExecutionContext
---@param pump Pump reference
---@param zr Zirconium reference
---@param reason string Why we're shutting down
function P.emergencyShutdown(context, pump, zr, reason)
    state.in_recovery = true
    state.recovery_tier = 3

    context:Report("Error", "EMERGENCY SHUTDOWN: " .. reason)
    smart_data.logError("Error",
        "EMERGENCY SHUTDOWN: " .. reason,
        "error_recovery", false, "emergency_shutdown")
    smart_data.raiseAlert("recovery.emergency", "error",
        "Emergency shutdown initiated: " .. reason, "error_recovery")

    -- Stop everything as safely as possible
    pcall(function() pump:Manualmode_Pump_constantFlow("A", 0) end)
    pcall(function() pump:Manualmode_Pump_constantFlow("B", 0) end)
    pcall(function() zr:SetValvePosition("PumpValveA", 180) end) -- Waste
    pcall(function() zr:SetValvePosition("PumpValveB", 180) end) -- Waste
    pcall(function() zr:SetValvePosition("TrapValve", 0) end)    -- Waste
    pcall(function() zr:SetValvePosition("InjectionValve", 30) end) -- Block

    -- Gentle decompression
    pcall(function()
        context:Log("Emergency: decompressing system...")
        local decompress = require("Decompress")
        decompress.run(context, pump, zr)
    end)

    -- End the run as failed
    pcall(function() smart_data.endRun("failed", "Emergency shutdown: " .. reason) end)

    state.in_recovery = false
    state.recovery_tier = 0
end

-- ─── Real-Time Monitoring ────────────────────────────────────────────

--- Monitor pressure and flow during a run. Call this at regular intervals.
--- Returns an action recommendation.
---@param context IProcedureExecutionContext
---@param pump Pump reference
---@param expectedPressure number Expected pressure at current gradient point
---@param expectedFlow number Expected flow rate
---@return string action "continue", "retry", "safe_state", or "shutdown"
---@return string|nil reason
function P.checkSystemHealth(context, pump, expectedPressure, expectedFlow)
    if not P.config.enabled then return "continue", nil end

    local pressA = pump:GetCurrentPressure("A") or 0
    local pressB = pump:GetCurrentPressure("B") or 0
    local flowA = pump:GetCurrentFlow("A") or 0

    -- Critical: over-pressure
    if pressA > P.config.critical_pressure_bar or pressB > P.config.critical_pressure_bar then
        return "shutdown",
            string.format("Critical over-pressure: A=%.0f bar, B=%.0f bar (limit: %.0f bar)",
                pressA, pressB, P.config.critical_pressure_bar)
    end

    -- Pressure spike detection
    if expectedPressure > 0 then
        local deviation = math.abs(pressA - expectedPressure) / expectedPressure * 100

        if deviation > P.config.flow_deviation_abort_pct then
            return "safe_state",
                string.format("Pressure deviation %.1f%% exceeds abort threshold (%.0f%%)",
                    deviation, P.config.flow_deviation_abort_pct)
        end

        if deviation > P.config.flow_deviation_retry_pct then
            -- Check if this is a transient spike
            if state.pressure_spike_start == nil then
                state.pressure_spike_start = os.clock()
            elseif os.clock() - state.pressure_spike_start > P.config.pressure_spike_max_duration_s then
                state.pressure_spike_start = nil
                return "retry",
                    string.format("Persistent pressure deviation: %.1f%% for >%ds",
                        deviation, P.config.pressure_spike_max_duration_s)
            end
            -- Still within transient window — continue monitoring
            return "continue", nil
        else
            state.pressure_spike_start = nil -- Reset spike timer if pressure is normal
        end
    end

    -- Flow deviation check
    if expectedFlow > 0.01 then
        local flowDev = math.abs(flowA - expectedFlow) / expectedFlow * 100

        if flowDev > P.config.critical_flow_deviation_pct then
            return "safe_state",
                string.format("Critical flow deviation: %.1f%% (expected %.3f, got %.3f µL/min)",
                    flowDev, expectedFlow, flowA)
        end

        if flowDev > P.config.flow_deviation_retry_pct then
            return "retry",
                string.format("Flow deviation: %.1f%% (expected %.3f, got %.3f µL/min)",
                    flowDev, expectedFlow, flowA)
        end
    end

    -- Update state
    state.last_pressure_a = pressA
    state.last_pressure_b = pressB
    state.last_flow_a = flowA

    return "continue", nil
end

--- Execute a monitored procedure step with full error recovery.
--- Wraps the step function with retry + safe-state + shutdown cascade.
---@param context IProcedureExecutionContext
---@param pump Pump reference
---@param zr Zirconium reference
---@param stepFunc function The procedure step to execute
---@param stepName string Human-readable step name
---@return boolean success
function P.executeStep(context, pump, zr, stepFunc, stepName)
    -- Tier 1: Try with auto-retry
    local ok, result = P.withRetry(context, stepFunc, stepName)
    if ok then return true end

    -- Tier 2: Try safe-state recovery
    context:Log("Step '{0}' failed after retries — attempting safe-state recovery", stepName)
    local recovered = P.enterSafeState(context, pump, zr,
        string.format("Step '%s' failed: %s", stepName, tostring(result)))

    if recovered then
        -- Try the step one more time after recovery
        context:Log("Retrying step '{0}' after safe-state recovery", stepName)
        ok, result = pcall(stepFunc)
        if ok then
            smart_data.logError("Info",
                string.format("Step '%s' succeeded after safe-state recovery", stepName),
                "error_recovery", true, "safe_state_retry")
            return true
        end
    end

    -- Tier 3: Emergency shutdown
    P.emergencyShutdown(context, pump, zr,
        string.format("Step '%s' unrecoverable: %s", stepName, tostring(result)))
    return false
end

-- ─── Run Lifecycle ───────────────────────────────────────────────────

--- Reset error recovery state at the start of a new run.
function P.resetForNewRun()
    state.retry_count = 0
    state.pressure_spike_start = nil
    state.in_recovery = false
    state.recovery_tier = 0
end

--- Get current error recovery status.
---@return table status
function P.getStatus()
    return {
        retry_count = state.retry_count,
        in_recovery = state.in_recovery,
        recovery_tier = state.recovery_tier,
        max_retries = P.config.max_retries_per_run,
        retries_remaining = P.config.max_retries_per_run - state.retry_count,
    }
end

return P
