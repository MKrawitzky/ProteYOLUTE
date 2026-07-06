-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/03/06"

local ringbuffer = {}
ringbuffer.__index = ringbuffer

local function new(...)
	local rb = {}
	rb.items = {...}
	rb.current = 1
	return setmetatable(rb, ringbuffer)
end

function ringbuffer:insert(item, ...)
	if not item then return end
	-- insert rest before self so order is restored, e.g.:
	-- {1,<2>,3}:insert(4,5) -> {1,<2>,3}:insert(5) -> {1,<2>,5,3} -> {1,<2>,4,5,3} 
	self:insert(...)
	table.insert(self.items, self.current+1, item)
end

function ringbuffer:append(item, ...)
	if not item then return end
	self.items[#self.items+1] = item
	return self:append(...)
end

function ringbuffer:removeAt(k)
	-- wrap position
	local pos = (self.current + k) % #self.items
	while pos < 1 do pos = pos + #self.items end

	-- remove item
	local item = table.remove(self.items, pos)

	-- possibly adjust current pointer
	if pos < self.current then self.current = self.current - 1 end
	if self.current > #self.items then self.current = 1 end

	-- return item
	return item
end

function ringbuffer:remove()
	return table.remove(self.items, self.current)
end

function ringbuffer:get()
	return self.items[self.current]
end

function ringbuffer:getItem(k)
	return self.items[k]
end

function ringbuffer:getItemAtPosition(i)
	if i > #self.items then i = #self.items end
	return self.items[i]
end

function ringbuffer:getAvg()
	local n = #self.items
	local avgFA = 0
	local avgFB = 0
	local avgDA = 0
	local avgDB = 0
	if n>0 then
		for i=1, n do
			avgFA = avgFA + self.items[i].fA
			avgFB = avgFB + self.items[i].fB
			avgDA = avgDA + self.items[i].dA
			avgDB = avgDB + self.items[i].dB
		end
		return avgDA/n, avgDB/n, avgFA/n, avgFB/n
	end
	return 0,0,0,0
end

function ringbuffer:size()
	return #self.items
end

function ringbuffer:next()
	self.current = (self.current % #self.items) + 1
	return self:get()
end

function ringbuffer:prev()
	self.current = self.current - 1
	if self.current < 1 then
		self.current = #self.items
	end
	return self:get()
end

function ringbuffer:reset()
	local size = #self.items
	local i = size
	while (i>0) do 
		table.remove(self.items, i)
		i = i-1
	end
--	while #self.items > 1 do self:remove() end
	return self.items
end

-- the module
return setmetatable({new = new},
	{__call = function(_, ...) return new(...) end})
