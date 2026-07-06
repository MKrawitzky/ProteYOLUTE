-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------


local Date = "2025/07/31"

local P = {}

---Logging the trap and separation column information
---@param context IProcedureExecutionContext
---@param trap IChromatographyColumnType?
---@param separator IChromatographyColumnType?
function P.logColumnInformation(context, trap, separator)
	if trap ~= nil then
		-- output the trap column information
		context:Log("--------------------------------------------------------")
		context:Log("--- Trap Column:              {0}", trap.Name)
		context:Log("---    Manufacturer:          {0}", trap.Manufacturer)
		context:Log("---    Part Number:           {0}", trap.PartNumber)
		context:Log("---    Serial Number:         {0}", trap.SerialNumber)
		context:Log("---    Type/Material:         {0}", trap.TypeMaterial)
		context:Log("---    Length:                {0} mm", trap.Length)
		context:Log("---    Inner Diameter:        {0} mm", trap.ColumnDiameter)
		context:Log("---    Particle Diameter:     {0} um", trap.ParticleDiameter)
		context:Log("---    Is Particle Pore Size: {0}", trap.IsParticlePoreSize)
		context:Log("---    Particle Pore Size:    {0}", trap.ParticlePoreSize)
		context:Log("---    Is Column Porosity:    {0}", trap.IsColumnPorosity)
		context:Log("---    Column Porosity:       {0} (0.0-1.0)", trap.ColumnPorosity)
		context:Log("---    Is Max Temperature:    {0}", trap.IsMaximumTemperature)
		context:Log("---    max Temperature:       {0} C", trap.MaximumTemperature)
		context:Log("---    Is Max Pressure:       {0}", trap.IsMaximumPressure)
		context:Log("---    max Pressure:          {0} bar", trap.MaximumPressure)
		context:Log("---    Is Max Flow:           {0}", trap.IsMaximumFlow)
		context:Log("---    max Flow:              {0} uL/min", trap.MaximumFlow)
		context:Log("---    Advanced Settings:     {0}", trap.IsAdvancedSettings)
		if (trap.IsAdvancedSettings) then
			context:Log("---    Column Volume:         {0} uL", trap.ColumnVolume)	
			context:Log("---    Unity Flow:            {0} uL/min/bar", trap.UnityFlow)	
		end
		context:Log("---    Column Comments:       {0}", trap.Comment)
	end

	if separator ~= nil then
		context:Log("--------------------------------------------------------")
		-- output the separation column information
		context:Log("--- Separation Column:        {0}", separator.Name)
		context:Log("---    Manufacturer:          {0}", separator.Manufacturer)
		context:Log("---    Part Number:           {0}", separator.PartNumber)
		context:Log("---    Serial Number:         {0}", separator.SerialNumber)
		context:Log("---    Type/Material:         {0}", separator.TypeMaterial)
		context:Log("---    Length:                {0} mm", separator.Length)
		context:Log("---    Inner Diameter:        {0} mm", separator.ColumnDiameter)
		context:Log("---    Particle Diameter:     {0} um", separator.ParticleDiameter)
		context:Log("---    Is Particle Pore Size: {0}", separator.IsParticlePoreSize)
		context:Log("---    Particle Pore Size:    {0}", separator.ParticlePoreSize)
		context:Log("---    Is Column Porosity:    {0}", separator.IsColumnPorosity)
		context:Log("---    Column Porosity:       {0} (0.0-1.0)", separator.ColumnPorosity)
		context:Log("---    Is Max Temperature:    {0}", separator.IsMaximumTemperature)
		context:Log("---    max Temperature:       {0} C", separator.MaximumTemperature)
		context:Log("---    Is Max Pressure:       {0}", separator.IsMaximumPressure)
		context:Log("---    max Pressure:          {0} bar", separator.MaximumPressure)
		context:Log("---    Is Max Flow:           {0}", separator.IsMaximumFlow)
		context:Log("---    max Flow:              {0} uL/min", separator.MaximumFlow)
		context:Log("---    Advanced Settings:     {0}", separator.IsAdvancedSettings)
		if (separator.IsAdvancedSettings) then
			context:Log("---    Column Volume:         {0} uL", separator.ColumnVolume)
			context:Log("---    Unity Flow:            {0} uL/min/bar", separator.UnityFlow)
		end
		context:Log("---    Column Comments:           {0}", separator.Comment)
		context:Log("--------------------------------------------------------")
	end
end

---Logging the maximum pressure
---@param context IProcedureExecutionContext
---@param maxPress number
---@param flow number
---@param comp number
---@param ovenTemp number
---@param colname string
function P.logMaxColumnPress(context, maxPress, flow, comp, ovenTemp, colname)
	local function file_exists(name)
	   local f = io.open(name, "r")
	   local isExist = f ~= nil and io.close(f)
	   return isExist
	end
	context:LogMeta(context.Name, "Maximum Pressure During The Gradient", "bar", "N0", maxPress)
	local fileName = "/BDALSystemData/HyStar/LogFiles/Bruker proteoElute maxPressLog.txt"
	local strg = tostring(maxPress)..", @ "..tostring(flow)..", "..tostring(comp)..", "..tostring(ovenTemp).."C, "..colname..", "..os.date().."\n"
	---@type file*?
	local file = nil

	if file_exists(fileName) == false then
		local header = "max Press, flow, composition B, oven temp, column, date\n"
		file = io.open(fileName, "a")			-- Opens a file in append mode
		if file ~= nil then
			io.output(file)
			io.write(header)
		end
	else
		file = io.open(fileName, "a")			-- Opens a file in append mode
		if file ~= nil then
			io.output(file)
		end
	end
	if file ~= nil then
		io.write(strg)
		io.close(file)
	end
end

return P