-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
--
-- topology.lua — Configurable system topology
-- Loads system configuration from topology.json, replacing all hardcoded
-- constants in baltic.lua. Supports multiple hardware configurations.
-------------------------------------------------------------------------------

local P = {}

-- ─── JSON Parser (pure Lua, no external dependency) ──────────────────

local function skipWhitespace(str, pos)
    return str:match("^%s*()", pos)
end

local function parseString(str, pos)
    local startChar = str:sub(pos, pos)
    if startChar ~= '"' then return nil, pos end
    local i = pos + 1
    local result = {}
    while i <= #str do
        local c = str:sub(i, i)
        if c == '\\' then
            i = i + 1
            local esc = str:sub(i, i)
            if esc == 'n' then result[#result+1] = '\n'
            elseif esc == 't' then result[#result+1] = '\t'
            elseif esc == '"' then result[#result+1] = '"'
            elseif esc == '\\' then result[#result+1] = '\\'
            elseif esc == '/' then result[#result+1] = '/'
            else result[#result+1] = esc end
        elseif c == '"' then
            return table.concat(result), i + 1
        else
            result[#result+1] = c
        end
        i = i + 1
    end
    error("Unterminated string at position " .. pos)
end

local parseValue -- forward declaration

local function parseArray(str, pos)
    if str:sub(pos, pos) ~= '[' then return nil, pos end
    local arr = {}
    pos = skipWhitespace(str, pos + 1)
    if str:sub(pos, pos) == ']' then return arr, pos + 1 end
    while true do
        local val
        val, pos = parseValue(str, pos)
        arr[#arr+1] = val
        pos = skipWhitespace(str, pos)
        if str:sub(pos, pos) == ']' then return arr, pos + 1 end
        if str:sub(pos, pos) ~= ',' then error("Expected ',' in array at " .. pos) end
        pos = skipWhitespace(str, pos + 1)
    end
end

local function parseObject(str, pos)
    if str:sub(pos, pos) ~= '{' then return nil, pos end
    local obj = {}
    pos = skipWhitespace(str, pos + 1)
    if str:sub(pos, pos) == '}' then return obj, pos + 1 end
    while true do
        local key
        key, pos = parseString(str, pos)
        if not key then error("Expected string key at " .. pos) end
        pos = skipWhitespace(str, pos)
        if str:sub(pos, pos) ~= ':' then error("Expected ':' at " .. pos) end
        pos = skipWhitespace(str, pos + 1)
        local val
        val, pos = parseValue(str, pos)
        obj[key] = val
        pos = skipWhitespace(str, pos)
        if str:sub(pos, pos) == '}' then return obj, pos + 1 end
        if str:sub(pos, pos) ~= ',' then error("Expected ',' in object at " .. pos) end
        pos = skipWhitespace(str, pos + 1)
    end
end

parseValue = function(str, pos)
    pos = skipWhitespace(str, pos)
    local c = str:sub(pos, pos)
    if c == '"' then return parseString(str, pos)
    elseif c == '{' then return parseObject(str, pos)
    elseif c == '[' then return parseArray(str, pos)
    elseif c == 't' then
        if str:sub(pos, pos+3) == 'true' then return true, pos+4 end
    elseif c == 'f' then
        if str:sub(pos, pos+4) == 'false' then return false, pos+5 end
    elseif c == 'n' then
        if str:sub(pos, pos+3) == 'null' then return nil, pos+4 end
    else
        local num = str:match("^-?%d+%.?%d*[eE]?[+-]?%d*", pos)
        if num then return tonumber(num), pos + #num end
    end
    error("Unexpected character '" .. c .. "' at position " .. pos)
end

local function parseJson(str)
    local val, _ = parseValue(str, 1)
    return val
end

-- ─── Topology Loading ────────────────────────────────────────────────

local _config = nil
local _configPath = nil

--- Load topology from JSON file.
---@param path string|nil Path to topology.json (defaults to plugin directory)
---@return table configuration
function P.load(path)
    if _config and path == _configPath then
        return _config
    end

    path = path or P.getDefaultPath()
    _configPath = path

    local file = io.open(path, "r")
    if not file then
        error("Cannot open topology file: " .. path)
    end

    local content = file:read("*a")
    file:close()

    _config = parseJson(content)
    return _config
end

--- Get the default topology file path.
function P.getDefaultPath()
    -- Plugin directory is where the Lua scripts live
    local info = debug.getinfo(1, "S")
    local scriptPath = info.source:gsub("^@", "")
    local dir = scriptPath:match("(.+)[/\\]Packages[/\\]")
    if dir then
        return dir .. "\\topology.json"
    end
    -- Fallback
    return "C:\\BDalSystemData\\HyStar\\LcPlugin\\PrivateData\\Bruker proteoElute\\topology.json"
end

--- Get the loaded config (loads if needed).
---@return table
function P.get()
    if not _config then P.load() end
    return _config
end

--- Reload config from disk.
function P.reload()
    _config = nil
    return P.load(_configPath)
end

-- ─── Accessor Functions ──────────────────────────────────────────────

--- Get a valve angle by valve name and position name.
---@param valveName string e.g., "PumpValveA"
---@param positionName string e.g., "MixTee"
---@return number angle in degrees
function P.getValveAngle(valveName, positionName)
    local cfg = P.get()
    local valve = cfg.valves[valveName]
    if not valve then error("Unknown valve: " .. valveName) end
    local pos = valve.positions[positionName]
    if not pos then error("Unknown position '" .. positionName .. "' for valve " .. valveName) end
    return pos.angle
end

--- Get all positions for a valve.
---@param valveName string
---@return table { name = angle, ... }
function P.getValvePositions(valveName)
    local cfg = P.get()
    local valve = cfg.valves[valveName]
    if not valve then error("Unknown valve: " .. valveName) end
    local positions = {}
    for name, data in pairs(valve.positions) do
        positions[name] = data.angle
    end
    return positions
end

--- Get a PID profile by name.
---@param profileName string e.g., "PID_16_1_1"
---@return number P, number I, number D
function P.getPidProfile(profileName)
    local cfg = P.get()
    local profile = cfg.pid_profiles[profileName]
    if not profile then error("Unknown PID profile: " .. profileName) end
    return profile.P, profile.I, profile.D
end

--- Auto-select the best PID profile for a given flow rate.
---@param flowRate number Flow rate in µL/min
---@return string profileName
function P.autoSelectPid(flowRate)
    local cfg = P.get()
    local rules = cfg.pid_auto_select.rules
    for _, rule in ipairs(rules) do
        if flowRate <= rule.flow_max_ul_min then
            return rule.profile
        end
    end
    return rules[#rules].profile -- Default to highest
end

--- Get a system parameter.
---@param key string e.g., "max_flow_ul_min", "gradient_dead_volume_ul"
---@return number|string
function P.getSystem(key)
    local cfg = P.get()
    return cfg.system[key]
end

--- Get a unity flow constant.
---@param key string e.g., "system", "equilibration_pump_separation"
---@return number
function P.getUnityFlow(key)
    local cfg = P.get()
    return cfg.unity_flows[key]
end

--- Get dead volume in nanoliters.
---@param key string e.g., "mix_tee_nl", "total_gradient_dead_volume_nl"
---@return number
function P.getDeadVolume(key)
    local cfg = P.get()
    return cfg.dead_volumes[key]
end

--- Get sensor configuration.
---@param sensorName string e.g., "PressureSensorA"
---@return table
function P.getSensor(sensorName)
    local cfg = P.get()
    return cfg.sensors[sensorName]
end

--- Get diagnostic thresholds.
---@return table
function P.getDiagThresholds()
    local cfg = P.get()
    return cfg.diagnostics.thresholds
end

--- Get calibration parameters.
---@return table
function P.getCalibrationParams()
    local cfg = P.get()
    return cfg.calibration
end

--- Get oven temperature limits.
---@return number min, number max, number default
function P.getOvenLimits()
    local cfg = P.get()
    local oven = cfg.oven
    return oven.min_temperature_c, oven.max_temperature_c, oven.default_temperature_c
end

--- Get active topology configuration.
---@param topologyName string|nil Defaults to "standard"
---@return table
function P.getTopology(topologyName)
    local cfg = P.get()
    topologyName = topologyName or "standard"
    return cfg.topologies[topologyName]
end

--- Check if the current topology has a trap column.
---@return boolean
function P.hasTrap(topologyName)
    local topo = P.getTopology(topologyName)
    return topo and topo.has_trap or false
end

--- Check if the current topology has an injection valve.
---@return boolean
function P.hasInjection(topologyName)
    local topo = P.getTopology(topologyName)
    return topo and topo.has_injection or false
end

--- Get flow path definition.
---@param pathName string e.g., "PumpA_to_MixTee"
---@return table { components, color_rgb, description }
function P.getFlowPath(pathName)
    local cfg = P.get()
    return cfg.flow_paths[pathName]
end

--- Get all flow paths.
---@return table
function P.getAllFlowPaths()
    local cfg = P.get()
    return cfg.flow_paths
end

return P
