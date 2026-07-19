-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
--
-- smart_data.lua — Intelligent data engine bridge
-- Replaces CSV logging with SQLite database, adds smart column/trap monitoring,
-- pressure anomaly detection, health counters, and REST API integration.
-------------------------------------------------------------------------------

local P = {}

-- .NET type imports
luanet.load_assembly("BalticWpfControlLib")
local ProteYoluteDb = luanet.import_type("BalticWpfControlLib.Data.ProteYoluteDb")
local SmartColumnManager = luanet.import_type("BalticWpfControlLib.Data.SmartColumnManager")
local PressureAnomalyDetector = luanet.import_type("BalticWpfControlLib.Data.PressureAnomalyDetector")
local ApiServer = luanet.import_type("BalticWpfControlLib.Api.ProteYoluteApiServer")

-- Singleton instances (initialized on first use)
local db = nil
local columnMgr = nil
local anomalyDetector = nil
local apiServer = nil
local currentRunId = nil

-- ─── Initialization ──────────────────────────────────────────────────

--- Initialize the ProteYOLUTE data engine. Call once at plugin startup.
function P.initialize()
    if db ~= nil then return end

    db = ProteYoluteDb.Instance
    columnMgr = SmartColumnManager(db)
    anomalyDetector = PressureAnomalyDetector(db)

    -- Start REST API server
    apiServer = ApiServer(db, columnMgr, anomalyDetector, 8742)
    apiServer:Start()

    db:IncrementCounter("system_start_count")
end

--- Shut down the data engine gracefully.
function P.shutdown()
    if apiServer then apiServer:Stop() end
    if db then db:Dispose() end
    db = nil
    columnMgr = nil
    anomalyDetector = nil
    apiServer = nil
end

-- ─── Run Management ──────────────────────────────────────────────────

--- Start tracking a new run. Call at the beginning of each procedure.
---@param method string The method/procedure name
---@param procedure string|nil Procedure type
---@param operator string|nil Operator name
---@return number run_id
function P.startRun(method, procedure, operator)
    P.initialize()
    local colId = columnMgr.ActiveColumnId
    local trapId = columnMgr.ActiveTrapId
    currentRunId = db:StartRun(method, procedure, operator,
        colId and colId.Value or nil,
        trapId and trapId.Value or nil)

    -- Notify smart column manager
    columnMgr:OnRunStart()

    -- Update API status
    if apiServer then
        apiServer.CurrentStatus.active_run_id = currentRunId
        apiServer.CurrentStatus.active_method = method
        apiServer.CurrentStatus.system_state = "running"
    end

    return currentRunId
end

--- End the current run.
---@param status string "completed", "failed", or "aborted"
---@param notes string|nil
function P.endRun(status, notes)
    if currentRunId == nil then return end
    status = status or "completed"

    -- Record column performance
    if apiServer and apiServer.CurrentStatus.pressure_a then
        local report = columnMgr:OnRunEnd(
            apiServer.CurrentStatus.pressure_a or 0,
            apiServer.CurrentStatus.flow_a or 0,
            apiServer.CurrentStatus.percent_b or 0,
            apiServer.CurrentStatus.temperature_c
        )

        -- Log column health alert if degraded
        if report and report.PerformanceScore < 50 then
            db:LogError(currentRunId, "Warning",
                string.format("Column '%s' performance at %.0f%%", report.Name, report.PerformanceScore),
                "SmartColumnManager")
        end
    end

    db:EndRun(currentRunId, status, notes)

    -- Update API status
    if apiServer then
        apiServer.CurrentStatus.active_run_id = nil
        apiServer.CurrentStatus.active_method = nil
        apiServer.CurrentStatus.system_state = "idle"
    end

    currentRunId = nil
end

--- Get the current run ID.
---@return number|nil
function P.getCurrentRunId()
    return currentRunId
end

-- ─── Real-Time Data Logging ──────────────────────────────────────────

--- Log pressure reading. Call at 500ms-2s intervals during runs.
---@param channel string "A" or "B"
---@param pressure number Pressure in bar
---@param setpoint number|nil Setpoint pressure
function P.logPressure(channel, pressure, setpoint)
    if db == nil then return end
    if currentRunId then
        db:LogPressure(currentRunId, channel, pressure, setpoint)
    end

    -- Update live status
    if apiServer then
        if channel == "A" then
            apiServer.CurrentStatus.pressure_a = pressure
        else
            apiServer.CurrentStatus.pressure_b = pressure
        end
    end

    -- Feed to anomaly detector
    if currentRunId and channel == "A" then
        local result = anomalyDetector:CheckPressure(pressure)
        if result then
            -- Anomaly detected — log it
            db:LogError(currentRunId, result.Severity,
                result.Message, "PressureAnomalyDetector")
        end
    end

    -- Feed to smart column monitor
    if currentRunId and channel == "A" then
        local flow = apiServer and apiServer.CurrentStatus.flow_a or 0
        local pctB = apiServer and apiServer.CurrentStatus.percent_b or 0
        local alerts = columnMgr:OnPressureReading(pressure, flow, pctB, channel)
        -- Alerts are logged inside the SmartColumnManager
    end
end

--- Log flow reading.
---@param channel string "A" or "B"
---@param flow number Flow in µL/min
---@param setpoint number|nil Setpoint flow
function P.logFlow(channel, flow, setpoint)
    if db == nil then return end
    if currentRunId then
        db:LogFlow(currentRunId, channel, flow, setpoint)
    end

    -- Update live status
    if apiServer then
        if channel == "A" then
            apiServer.CurrentStatus.flow_a = flow
        else
            apiServer.CurrentStatus.flow_b = flow
        end
    end
end

--- Log valve switch event.
---@param valve string Valve name ("A", "B", "I", "T")
---@param fromPos string|nil Previous position name
---@param toPos string New position name
---@param angle number|nil Angle in degrees
function P.logValveSwitch(valve, fromPos, toPos, angle)
    if db == nil then return end
    if currentRunId then
        db:LogValveSwitch(currentRunId, valve, fromPos, toPos, angle)
    end

    -- Update live status
    if apiServer then
        if valve == "A" then apiServer.CurrentStatus.valve_a = toPos
        elseif valve == "B" then apiServer.CurrentStatus.valve_b = toPos
        elseif valve == "I" then apiServer.CurrentStatus.valve_i = toPos
        elseif valve == "T" then apiServer.CurrentStatus.valve_t = toPos
        end
    end
end

--- Log gradient position.
---@param percentB number Current %B composition
---@param flowTotal number|nil Total flow rate
---@param pressureA number|nil Channel A pressure
---@param pressureB number|nil Channel B pressure
function P.logGradient(percentB, flowTotal, pressureA, pressureB)
    if db == nil then return end
    if currentRunId then
        db:LogGradient(currentRunId, percentB, flowTotal, pressureA, pressureB)
    end

    -- Update live status
    if apiServer then
        apiServer.CurrentStatus.percent_b = percentB
    end
end

-- ─── Diagnostics & Calibration (replaces csv_file_logging) ───────────

--- Log a diagnostic result. Drop-in replacement for csv_file_logging.logValueInCSVFile
---@param context IProcedureExecutionContext
---@param fileName string (kept for compatibility, maps to test_name)
---@param test string Test category
---@param parameter string Parameter name
---@param value number|string Value
---@param unit string Unit of measurement
---@param decimalPlaces number|nil Decimal places for rounding
function P.logValue(context, fileName, test, parameter, value, unit, decimalPlaces)
    P.initialize()
    decimalPlaces = decimalPlaces or 5

    local numValue = nil
    local textValue = nil

    if type(value) == "number" then
        local mult = 10 ^ (decimalPlaces or 5)
        numValue = math.floor(value * mult + 0.5) / mult
    else
        textValue = tostring(value)
    end

    db:LogDiagnostic(currentRunId, test, parameter, numValue, unit, nil, nil, nil, textValue)

    -- Also log to context for HyStar visibility
    if context and context.Log then
        context:Log("{0}: {1} = {2} {3}", test, parameter, tostring(value), unit or "")
    end

    -- Also write CSV for backwards compatibility
    local csv = require("csv_file_logging")
    pcall(function()
        csv.logValueInCSVFile(context, fileName, test, parameter, value, unit, decimalPlaces)
    end)
end

--- Log a calibration result.
---@param sensor string Sensor name (e.g., "FlowSensorA")
---@param calType string Calibration type
---@param channel string|nil Channel
---@param oldValue number|nil Previous value
---@param newValue number|nil New value
---@param result string "pass" or "fail"
---@param notes string|nil Additional notes
function P.logCalibration(sensor, calType, channel, oldValue, newValue, result, notes)
    P.initialize()
    db:LogCalibration(sensor, calType, channel, oldValue, newValue, result, notes)
end

--- Log an error with optional recovery information.
---@param severity string "Info", "Warning", or "Error"
---@param message string Error message
---@param context string|nil Additional context
---@param recovered boolean|nil Whether the error was recovered from
---@param recoveryAction string|nil What recovery action was taken
function P.logError(severity, message, context, recovered, recoveryAction)
    P.initialize()
    db:LogError(currentRunId, severity, message, context, recovered or false, recoveryAction)
end

-- ─── Smart Column & Trap Management ──────────────────────────────────

--- Install and activate a new analytical column.
---@param name string Column name (e.g., "PepSep C18 25cm")
---@param columnType string Type (e.g., "C18", "C4", "HILIC")
---@param particleSizeUm number|nil Particle size in µm
---@param innerDiameterUm number|nil Inner diameter in µm
---@param lengthCm number|nil Length in cm
---@param maxPressureBar number|nil Maximum pressure in bar
---@param serial string|nil Serial number
---@return number column_id
function P.installColumn(name, columnType, particleSizeUm, innerDiameterUm, lengthCm, maxPressureBar, serial)
    P.initialize()
    return columnMgr:InstallColumn(name, columnType, particleSizeUm,
        innerDiameterUm, lengthCm, maxPressureBar, serial)
end

--- Install and activate a new trap column.
---@param name string Trap name
---@param trapType string Type
---@param particleSizeUm number|nil Particle size
---@param innerDiameterUm number|nil Inner diameter
---@param lengthCm number|nil Length
---@param maxPressureBar number|nil Max pressure
---@param serial string|nil Serial number
---@return number trap_id
function P.installTrap(name, trapType, particleSizeUm, innerDiameterUm, lengthCm, maxPressureBar, serial)
    P.initialize()
    return columnMgr:InstallTrap(name, trapType, particleSizeUm,
        innerDiameterUm, lengthCm, maxPressureBar, serial)
end

--- Get health report for the active column.
---@return table|nil health report
function P.getColumnHealth()
    P.initialize()
    if not columnMgr.ActiveColumnId then return nil end
    return db:GetColumnHealth(columnMgr.ActiveColumnId.Value)
end

--- Get health report for the active trap.
---@return table|nil health report
function P.getTrapHealth()
    P.initialize()
    if not columnMgr.ActiveTrapId then return nil end
    return db:GetColumnHealth(columnMgr.ActiveTrapId.Value)
end

--- Run pre-run smart checks. Returns list of alerts/recommendations.
---@return table list of SmartAlert objects
function P.preRunCheck()
    P.initialize()
    return columnMgr:PreRunCheck()
end

--- Set the active column by ID.
---@param columnId number
function P.setActiveColumn(columnId)
    P.initialize()
    columnMgr:SetActiveColumn(columnId)
end

--- Set the active trap by ID.
---@param trapId number
function P.setActiveTrap(trapId)
    P.initialize()
    columnMgr:SetActiveTrap(trapId)
end

--- Retire a column.
---@param columnId number
---@param reason string|nil
function P.retireColumn(columnId, reason)
    P.initialize()
    columnMgr:RetireColumn(columnId, reason)
end

--- List all active columns.
---@param role string|nil "analytical" or "trap" or nil for all
---@return table list of column records
function P.listColumns(role)
    P.initialize()
    return columnMgr:GetAllColumns(role)
end

-- ─── Anomaly Detection ───────────────────────────────────────────────

--- Start gradient monitoring for anomaly detection.
---@param method string Method name
---@param gradientDurationMin number Gradient duration in minutes
function P.startGradientMonitoring(method, gradientDurationMin)
    P.initialize()
    local colId = columnMgr.ActiveColumnId
    anomalyDetector:StartGradientMonitoring(method, colId and colId.Value or nil, gradientDurationMin)
end

--- Stop gradient monitoring.
function P.stopGradientMonitoring()
    if anomalyDetector then
        anomalyDetector:StopGradientMonitoring()
    end
end

-- ─── Health Counters ─────────────────────────────────────────────────

--- Get a health counter value.
---@param key string Counter key
---@return number
function P.getCounter(key)
    P.initialize()
    return db:GetCounter(key)
end

--- Get all health counters.
---@return table key-value pairs
function P.getAllCounters()
    P.initialize()
    return db:GetAllCounters()
end

--- Increment a health counter.
---@param key string Counter key
---@param amount number|nil Amount to increment (default 1)
function P.incrementCounter(key, amount)
    P.initialize()
    db:IncrementCounter(key, amount or 1)
end

-- ─── Method Templates ────────────────────────────────────────────────

--- Save the current method parameters as a template.
---@param name string Template name
---@param parametersJson string JSON string of parameters
---@param description string|nil
---@param category string|nil
---@return number template_id
function P.saveTemplate(name, parametersJson, description, category)
    P.initialize()
    return db:SaveTemplate(name, parametersJson, description, category, "ProteYOLUTE")
end

--- Load a method template by name.
---@param name string Template name
---@return string|nil JSON parameters
function P.loadTemplate(name)
    P.initialize()
    return db:GetTemplate(name)
end

--- List all available templates.
---@return table list of template records
function P.listTemplates()
    P.initialize()
    return db:ListTemplates()
end

-- ─── Alerts ──────────────────────────────────────────────────────────

--- Get all unacknowledged alerts.
---@return table list of alert records
function P.getActiveAlerts()
    P.initialize()
    return db:GetActiveAlerts()
end

--- Raise a custom alert.
---@param alertType string Alert type identifier
---@param severity string "info", "warning", or "error"
---@param message string Alert message
---@param source string|nil Source module
function P.raiseAlert(alertType, severity, message, source)
    P.initialize()
    db:RaiseAlert(alertType, severity, message, source)
end

-- ─── Audit Trail ─────────────────────────────────────────────────────

--- Record an audit trail entry.
---@param action string Action taken
---@param entityType string|nil Entity type (e.g., "method", "column")
---@param entityId string|nil Entity ID
---@param oldValue string|nil Previous value
---@param newValue string|nil New value
---@param reason string|nil Reason for change
function P.audit(action, entityType, entityId, oldValue, newValue, reason)
    P.initialize()
    db:Audit(action, entityType, entityId, oldValue, newValue, reason)
end

-- ─── Update Live Status (for API dashboard) ─────────────────────────

--- Update the live system status displayed on the dashboard.
---@param key string Status key (e.g., "temperature_c", "system_state")
---@param value any Status value
function P.updateStatus(key, value)
    if apiServer == nil then return end
    local s = apiServer.CurrentStatus
    if key == "temperature_c" then s.temperature_c = value
    elseif key == "system_state" then s.system_state = value
    elseif key == "gradient_progress_pct" then s.gradient_progress_pct = value
    elseif key == "pump_a_volume_remaining" then s.pump_a_volume_remaining = value
    elseif key == "pump_b_volume_remaining" then s.pump_b_volume_remaining = value
    end
end

return P
