local Date = "2025/06/03"
local baltic = 	require "baltic"

-- create package table
local P = {}

luanet.load_assembly("Bruker.Lc")

---@type JournalEntry
local JournalEntry = luanet.import_type("Bruker.Lc.JournalEntry")
---@type LogTo
local LogTo = luanet.import_type("Bruker.Lc.Business.LogTo")

local chrom = require "chromatography"

---Log method data to TwinScape
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param journal IJournal
function P.LogTwinScapeData(installed, context, journal)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)

	-- log method information to TwinScape
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Separation Method", context.Name))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Column Oven Connected", installed.IsColumnOvenConnected))
	if (installed.IsColumnOvenConnected) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Column Oven Temperature", pump:GetCurrentExternalTemperature(), "degree"))
	end

	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Injection Method", context:GetArgumentValue("injection_method")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "MS Calibrant Injection", context:GetArgumentValue("MS Calibrant Injection")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flush Injector Port", context:GetArgumentValue("Flush Injector Port")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Toggle Wash In Gradient", context:GetArgumentValue("Toggle Wash In Gradient")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Extended Wash", context:GetArgumentValue("Extended Wash")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Pre Injection Needle Wash", context:GetArgumentValue("Pre Injection Needle Wash")))
	if (context:GetArgumentValue("Pre Injection Needle Wash")) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Pre Injection Needle Wash: Organic", context:GetArgumentValue("Pre Injection Needle Wash", "Wash In Organic"), "\181L"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Pre Injection Needle Wash: Aqueous", context:GetArgumentValue("Pre Injection Needle Wash", "Wash In Aqueous"), "\181L"))
	end
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Injection Path Wash", context:GetArgumentValue("Injection Path Wash")))
	if (context:GetArgumentValue("Injection Path Wash")) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Injection Path Wash: Time", context:GetArgumentValue("Injection Path Wash", "Time"), "sec"))
	end
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Additional Loading Volume", context:GetArgumentValue("Additional Loading Volume")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Injection Settings", context:GetArgumentValue("Injection Settings")))
	if (context:GetArgumentValue("Injection Settings")) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Injection Settings: Needle Height", context:GetArgumentValue("Injection Settings", "Needle Height"), "mm"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Injection Settings: Draw Speed", context:GetArgumentValue("Injection Settings", "Draw Speed"), "\181L/sec"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Injection Settings: Pause Draw", context:GetArgumentValue("Injection Settings", "Pause Draw")*1000, "ms"))
	end
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Add From Vials", context:GetArgumentValue("Add From Vials")))
	if (context:GetArgumentValue("Add From Vials")) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Add From Vials: 1st Vial", context:GetArgumentValue("Add From Vials", "1st Vial")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Add From Vials: 2nd Vial", context:GetArgumentValue("Add From Vials", "2nd Vial")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Add From Vials: 3rd Vial", context:GetArgumentValue("Add From Vials", "3rd Vial")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Add From Vials: 1st Volume", context:GetArgumentValue("Add From Vials", "1st Volume"), "\181L"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Add From Vials: 2nd Volume", context:GetArgumentValue("Add From Vials", "2nd Volume"), "\181L"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Add From Vials: 3rd Volume", context:GetArgumentValue("Add From Vials", "3rd Volume"), "\181L"))
	end
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Air Gaps", context:GetArgumentValue("Air Gaps")))
	if (context:GetArgumentValue("Air Gaps")) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Air Gaps: Pre Sample Air Gap", context:GetArgumentValue("Air Gaps", "Pre Sample Air Gap"), "\181L"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Air Gaps: Post Sample Air Gap", context:GetArgumentValue("Air Gaps", "Post Sample Air Gap"), "\181L"))
	end
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample", context:GetArgumentValue("Dissolve Sample")))
	if (context:GetArgumentValue("Dissolve Sample")) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Sample Vial Penetration Depth", context:GetArgumentValue("Dissolve Sample", "Sample Vial Penetration Depth")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Solvent Volume", context:GetArgumentValue("Dissolve Sample", "Solvent Volume"), "\181L"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Dissolve Solvent Position", context:GetArgumentValue("Dissolve Sample", "Dissolve solvent position")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Solvent Vial Penetration Depth", context:GetArgumentValue("Dissolve Sample", "Solvent vial penetration depth"), "mm"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Solvent Release Speed", context:GetArgumentValue("Dissolve Sample", "Solvent Release Speed"), "\181L/sec"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Time to Dissolve", context:GetArgumentValue("Dissolve Sample", "Time to Dissolve"), "min"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Needle Height for Mixing", context:GetArgumentValue("Dissolve Sample", "Needle Height for Mixing"), "mm"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Mixing Volume", context:GetArgumentValue("Dissolve Sample", "Mixing Volume"), "\181L"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Mixing Cycles", context:GetArgumentValue("Dissolve Sample", "Mixing Cycles")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Dissolve Sample: Mixing Speed", context:GetArgumentValue("Dissolve Sample", "Mixing Speed"), "\181L/sec"))
	end
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Column Load Pressure", context:GetArgumentValue("column_load_pressure"), "bar"))
end	

---Log infustion method data to TwinScape
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param journal IJournal
function P.LogTwinScapeInfusionData(installed, context, journal)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)

	-- log method information to TwinScape
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Separation Method", context.Name))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Column Oven Connected", installed.IsColumnOvenConnected))
	if (installed.IsColumnOvenConnected) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Column Oven Temperature", pump:GetCurrentExternalTemperature(), "degree"))
	end

	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Expected sample loading volume", context:GetArgumentValue("sample_volume") * context:GetArgumentValue("column_load_volumemultiplier") + 2 + context:GetArgumentValue("Additional Loading Volume"), "\181L"))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Expected column equilibration volume", chrom.column_volume(context:GetArgumentValue("separator"))*5, "\181L"))
end	
return P
