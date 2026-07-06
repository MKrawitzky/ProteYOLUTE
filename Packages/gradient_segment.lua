-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2024/11/27"

local P = {}

---Iterates over the gradient segments
---@param o any
---@return fun() : SetPoint?
function P.dotnet_each(o)
   local e = o:GetEnumerator()
   return function()
	  if e:MoveNext() then
		return e.Current
	  else
		e:Dispose()
	 end
   end
end

---Suggest flow values for selected column
---@param column_flow number
---@return number
function P.gradient_flow(column_flow)
	if column_flow < 0.25 then return 0.1 end
	if column_flow < 0.35 then return 0.2 end
	if column_flow >= 0.35 and column_flow < 1.0 then return 0.3 end
	return 0.5
end

---Set binary flow according to a gradient segment
---@param context IProcedureExecutionContext
---@param pump Pump
---@param duration number
---@param start_flow number
---@param start_mix number
---@param end_flow number
---@param end_mix number
function P.gradient_segment(context, pump, duration, start_flow, start_mix, end_flow, end_mix)
    local pf = require "pump_functions"
	---@type Zirconium
	local zr = require "zirconium"

	-- return current time in seconds.
	local function now()
		return os.clock()
	end
	local function sleep_100()
		context:Sleep(100)
	end

	local slope_flow = (end_flow - start_flow) / duration
	local slope_mix = (end_mix - start_mix) / duration
	local start = now()
--	context:Log("---duration: {0}, start_flow: {1}, start_mix: {2}, end_flow: {3}, end_mix: {4}", duration, start_flow, startFlow, start_mix, end_flow, end_mix)
	for n = 1,duration do
		local delay = start + n - now()
		if (delay > 0) then
			context:Sleep(delay*1000)
		end

		local flow = start_flow + slope_flow*n
		local mix = start_mix + slope_mix*n
--		context:Log("---binary flow A: {0}, binary flow B: {1}", flow*(1-mix), flow * mix)

		pf.Manualmode_Pump_constantFlow_binary(context, (flow*(1-mix)), (flow * mix), pump, zr, sleep_100)
--		pump:Manualmode_Pump_constantFlow_binary(pf.noExp(flow*(1-mix)), pf.noExp(flow * mix))
	end
end

return P