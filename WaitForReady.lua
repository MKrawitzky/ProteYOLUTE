-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2024/11/27"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

local baltic = require "baltic"

---@param context InitHelper
function Initialize (context)
	-- example
--	context.Name = "Waiting for Oven Temperature"
	context.Name = "Waiting for Oven Temperature, open context menu to stop waiting."
	context.Description = "Procedure for waiting for pre-run conditions to be satisfied"
	context.Hidden = true
	context.DecompressOnExit = false
	context.OverwriteLogFiles = false
	context.LedState = LedState.Idle

	-- PBNE-657: Column oven is diconnected but this is ignored by the software
	context:DeclareParameter("is_use_oven_temperature", true)
	context:DeclareParameter("oven_temperature", baltic.Settings.ColumnOvenMinTemperature, "deg", "decimal")
end

---@param _ IInstalledHardwareContext
---@param __ IProcedureValidationContext
function Validate (_, __)
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)

	-- check for oven and temperature only if the oven is intended to use ("is_use_oven_temperature") == true
	-- otherwise skip all and return
	if context:GetArgumentValue("is_use_oven_temperature") == true then
		context:Log("Waiting for oven temperature")

		local pr = require "PreRunFunctions"
		---@type Pump
		local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
		local dictator = LoggingDictator.Prevent(pump)
		if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	
		local function sleep_1000()
			context:Sleep(1000)
		end

		local function now()
			return os.clock()
		end

		-- check if column oven is intended to use and connected and temperature is set
		if not pr.IsOvenAndTemperatureOK(context, pump) then		-- PBNE-657
			context:Report("Oven", Severity.Error, true, "Missing column oven. Temperature is set but oven is not connected.")
		else
			-- get the method column oven temperature
			local ovenTempSetPt = context:GetArgumentValue("oven_temperature")
		--	local text = "waiting for a temperature of "..ovenTempSetPt.."C. Open preferences to stop waiting."
		--	context:Report(oven, Severity.Info, true, text)

			-- set the oven temp in case different
			pump:SetExternalTemperature(ovenTempSetPt)

			-- wait a maximum of 30 minutes to be within 0.5C of set point
			local isTempReached = false
			local maxMinutes = 30
			local max_time_allowed = now() + maxMinutes*60  

			-- Only if columnn oven is installed, then wait for oven temperature to achieve set point
			if (ovenTempSetPt >= 35) then
				while (math.abs(pump:GetCurrentExternalTemperature() - ovenTempSetPt) > 1.0) and (now() < max_time_allowed) do
					sleep_1000()
				end
				if now() < max_time_allowed then isTempReached = true end
			end
			context:Log("Oven temperature is reached: {0}", isTempReached)
			if (isTempReached == false) and (ovenTempSetPt >= 35)then
				local msg = DotNetString.Format("the set temperature has not been reached within {0} minutes", maxMinutes)
				context:Report("Oven", Severity.Error, true, msg)
			end
		end
		dictator:Dispose()
	end
end
