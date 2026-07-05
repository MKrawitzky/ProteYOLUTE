local Date = "2025/01/06"

local M = {}

-- Note that the supplied varargs is modified by this function and thus cannot be reused.
---Run the supplied functions in 'parallel' (using co-routines)
---@param yield_func function
---@param ... table
---@return unknown
function M.run(yield_func, ...)
	local args = table.pack(...)
	-- convert incoming functions to co-routines
	for i = 1, args.n do
		args[i][1] = coroutine.create(args[i][1])
	end
	local running = false
	local out = {}
	repeat
		running = false
		for i = 1, args.n do
			local r = table.pack(coroutine.resume(table.unpack(args[i])))
			-- if we resumed, assign return value(s) and set running
			if (r[1]) then
				out[i] = r
				running = true
			end
		end
		yield_func()
	until not running
	-- unpacks the return-arrays into varargs, to support concurrent functions returning multiple values
	local function outpack(i,j)
		local t = out[i]
		if (t ~= nil) then
			if (t[j] ~= nil) then
				return t[j], outpack(i, j+1)
			else
				return outpack(i+1,2)
			end
		end
	end

	return outpack(1,2)
end

--- Functions supplied to run must use this function when they want to hand over the cpu to another task.
function M.yield()
	coroutine.yield()
end

---Concurrent delay function
---@param yield_func function
---@param delay_ms number
function M.sleep(yield_func, delay_ms)
	local delay_s = delay_ms * 0.001
	local start = os.clock()
	while (os.clock()-start < delay_s) do
		yield_func()
	end
end

return M
