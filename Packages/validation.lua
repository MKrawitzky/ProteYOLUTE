-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

﻿local Date = "2025/01/29"

-- dependencies as locals here
luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")

-- create package table
local P = {}


---This function truncates a number to specified number of decimals
---@param number number
---@param decimals number|nil
---@return number
local function truncate(number, decimals)
	if (decimals == nil) then
		return number
	end

	local factor = 10 ^ decimals
	return math.floor(number * factor) / factor
end

---Validate that a value is present for an argument (compare against nil) and report an error if not
---@param context IProcedureValidationContext
---@param argument_name string
---@param message string|nil
function P.verify_specified(context, argument_name, message)
	local value = context:GetArgumentValue(argument_name)
	if (value == nil) then
	local msg = message or "Value required"
	  context:Report(argument_name, Severity.Error, true, msg)
	end
end

---Validate that a number is within the specified range and report an error if not
---@param context IProcedureValidationContext
---@param argument_name string
---@param value_min number|nil
---@param value_max number|nil
---@param message_format string|nil
---@param decimals number|nil
function P.verify_range(context, argument_name, value_min, value_max, message_format, decimals)
	local value = context:GetArgumentValue(argument_name)
	-- bail if nil
	if (value == nil) then return end
	-- truncate if decimals are specified
	if (decimals ~= nil) then value = truncate(value, decimals) end

	if (value_min and value_max and (value < value_min or value > value_max)) then
		local msg = message_format or "Value must be between {0:#0.00} and {1:#0.00}"
		context:Report(argument_name, Severity.Error, true, msg, value_min, value_max)
	end
end

---Validate that a number is within the specified range and report an error if not
---@param context IProcedureValidationContext
---@param argument_name string
---@param value number
---@param value_min number|nil
---@param value_max number|nil
---@param message_format string|nil
---@param decimals number|nil
function P.verify_number_in_range(context, argument_name, value, value_min, value_max, message_format, decimals)
	-- truncate if decimals are specified
	if (decimals ~= nil) then value = truncate(value, decimals) end

	if (value_min and value_max and (value < value_min or value > value_max)) then
		local msg = message_format or "Value must be between {0:#0.00} and {1:#0.00}"
		context:Report(argument_name, Severity.Error, true, msg, value_min, value_max)
	end
end

---Validate that a value is present for an argument (compare against nil) and report an error if not
---@param context IProcedureValidationContext
---@param argument_header string
---@param argument_name string
---@param message string|nil
function P.verify_specifiedAS(context, argument_header, argument_name, message)
	local value = context:GetArgumentValue(argument_header, argument_name)
	if (value == nil) then
	local msg = message or "Value required"
	  context:Report(argument_header, argument_name, Severity.Error, true, msg)
	end
end

---Validate that a number is within the specified range and report an error if not
---@param context IProcedureValidationContext
---@param argument_header string
---@param argument_name string
---@param value_min number|nil
---@param value_max number|nil
---@param message_format string|nil
function P.verify_rangeAS(context, argument_header, argument_name, value_min, value_max, message_format)
	local value = context:GetArgumentValue(argument_header, argument_name)
	-- bail if nil
	if (value == nil) then return end

	-- from here on, we know value is not nil
	if (value_min and value_max and (value < value_min or value > value_max)) then
		local msg = message_format or "Value must be between {0:#0.00} and {1:#0.00}"
		context:Report(argument_header, argument_name, Severity.Error, true, msg, value_min, value_max)
	end
end

---Validate that a number is equal to a required value and report an error if not
---@param context IProcedureValidationContext
---@param argument_name string
---@param value_required number|boolean
---@param message string|nil
function P.verify_equals(context, argument_name, value_required, message)
	local value = context:GetArgumentValue(argument_name)
	local msg = message or "Value must equal {0}"
	if (value ~= value_required) then
		context:Report(argument_name, Severity.Error, true, msg, value_required)
	end
end

---Validate that a text is equal to a required text1 or text2 and report an error if not
---@param context IProcedureValidationContext
---@param argument_name string
---@param text1 string
---@param text2 string
---@param message string|nil
function P.verify_text(context, argument_name, text1, text2, message)
	local value = context:GetArgumentValue(argument_name)
	local msg = message or "Text must equal {0} or {1}"
	if (value ~= text1) and (value ~= text2) then
		context:Report(argument_name, Severity.Error, true, msg, text1, text2)
	end
end

return P
