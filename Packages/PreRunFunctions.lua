-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/07/31"

local P = {}

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")

local pp = require "palplus"

local factor = 2

---Return the precision for Meta data
---@param value number
---@return string
function P.getMetaDataPrecision(value)
	local precision = 0
	while (value < 1000) do
		value = value * 10
		precision = precision + 1
		if (precision > 6) then break end
	end
	precision = DotNetString.Format("N{0}", precision)
	return precision
end

---comment
---@param list table
---@param x string
---@return integer|nil
local function contains(list, x)
	for i, v in ipairs(list) do
		if v == x then return i end
	end
	return nil
end

---Reduce the pressure of the system
---@param context IProcedureExecutionContext
function P.decompressSystem(context)
	local baltic = require "baltic"
	local pf = require "pump_functions"
	---@type Zirconium
	local zr = require "zirconium"
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local timeOut = 60				-- [sec]
	local targetPressure = 5

    local function sleep_100()
        context:Sleep(100)
    end
	pf.reducePressure(context, pump, zr, zr.A, zr.B, targetPressure, targetPressure, timeOut, sleep_100, baltic.smooth)	
end

---Print the capillary dimantions into the execution lag file
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function P.printCapillaryDimensions(installed, context)
	local baltic = require "baltic"
	local chrom = require "Chromatography"
	local N = baltic.Naming
	local id, length = chrom.getCapillaryIDandLength(installed, N.ValveAToFS)
	context:Log(baltic.devider)
	context:Log("Installed capillaries:")
	context:Log("Valve A to Flow Sensor:        ID: {0} mm; Length: {1} mm", id, length)
	id, length = chrom.getCapillaryIDandLength(installed, N.ValveBToFS)
	context:Log("Valve B to Flow Sensor:        ID: {0} mm; Length: {1} mm", id, length)
	id, length = chrom.getCapillaryIDandLength(installed, N.FSAToMixTee)
	context:Log("Flow Sensor A to MixTee:       ID: {0} mm; Length: {1} mm", id, length)
	id, length = chrom.getCapillaryIDandLength(installed, N.FSBToMixTee)
	context:Log("Flow Sensor B to MixTee:       ID: {0} mm; Length: {1} mm", id, length)
	id, length = chrom.getCapillaryIDandLength(installed, N.MixTeeToTrapValve)
	context:Log("MixTee to Trap Valve:          ID: {0} mm; Length: {1} mm", id, length)
	id, length = chrom.getCapillaryIDandLength(installed, N.TransferLine)
	context:Log("TransferLine:                  ID: {0} mm; Length: {1} mm", id, length)
	id, length = chrom.getCapillaryIDandLength(installed, N.ValveAToInjectionValve)
	context:Log("Loading Capillary:            ID: {0} mm; Length: {1} mm", id, length)
	id, length = chrom.getCapillaryIDandLength(installed, N.InjectToTrap)
	context:Log("Injection Capillary: ID: {0} mm; Length: {1} mm", id, length)
	id, length = chrom.getCapillaryIDandLength(installed, N.Trap)
	context:Log("Trap In and Out Capillary:     ID: {0} mm; Length: {1} mm", id, length)
	id, length = chrom.getCapillaryIDandLength(installed, N.Loop)
	context:Log("Loop:                          ID: {0} mm; Volume: {1} uL", id, length)
	context:Log(baltic.devider)
end

---comment
---@param idx number
---@param execAux IPalParticipant
---@return nil|any
local function getPALSignal(idx, execAux)
--	local pp = require "palplus"
--	local outputSignal = {"Injected1","Injected2","Injected3","Injected4","Injected5","Injected6"}
	local outputSignal = {"Output Signal 1","Output Signal 2","Output Signal 3","Output Signal 4","Output Signal 5","Output Signal 6"}
	local signal = pp.QueryModules(execAux, "OutputSignalDescription")
	
	local signalIdx = {}
	for i=1, 6 do
		if signal[i] == nil then break end
		signalIdx[i] = contains(outputSignal, signal[i].Name)
		-- !!! if signal[i] == nil then signal[i].Name results in an error and aborts the lua !!!
		-- !!! Therefor check signal[i] for nil before asking for parameters (e.g. Name) !!!
	end
	if signal[signalIdx[idx]] == nil then return nil end
	return signal[signalIdx[idx]]
end

---Set a PAL signal
---@param idx number
---@param execAux IPalParticipant
function P.SetSignal(idx, execAux)
--	local pp = require "palplus"
	local Signal = getPALSignal(idx, execAux)
	-- the signal is not set if the signal doesn't exist
	if Signal ~= nil then execAux:SetSignal(Signal) end
end

---Return the item position index
---@param idx number
---@param execAux IPalParticipant
---@return nil|number
function P.GetItemPosIdx(idx, execAux)
	local itemPosition = {"Item Position 1","Item Position 2","Item Position 3","Item Position 4","Item Position 5","Item Position 6"}
	local item = pp.QueryModules(execAux, "ItemPositionDescription")
	local itemSize = #item
	if idx > itemSize then return nil end
	for i=1, itemSize do
		if itemPosition[idx] == item[i].Name then return i end
	end
	return nil
end

---Check PAL FW version
---@param palplus_participant IPalParticipant
---@param minAutosamplerFWVersion number
---@param maxAutoSamplerFWVersion number
---@return string
---@return boolean
function P.isFWVersion_OK(palplus_participant, minAutosamplerFWVersion, maxAutoSamplerFWVersion)
	local FWVersion = palplus_participant.ConfigurationService.SystemInformation.AutosamplerSoftwareVersion
	local version = tonumber(FWVersion.Major.."."..FWVersion.Minor)
	local versionOK = false
	if (version >= minAutosamplerFWVersion) and (version <= maxAutoSamplerFWVersion) then
		versionOK = true
	end
	return FWVersion, versionOK
end

---Switch the valve and increment the valve switch counter
---@param palplus_participant IPalParticipant
---@param valve table
---@param setAngle number
function P.SetValvePosition(palplus_participant, valve, setAngle)
	palplus_participant:MoveValveDrive(valve, pp.Quantity(setAngle, "deg"))			-- send the command even it is already in this position
end

---Return the injection and trap valve positions
---@param palplus_participant IPalParticipant
---@return number|nil
---@return number|nil
function P.GetPalValvePositions(palplus_participant)
	local trapValve = pp.GetTrapValvePosition(palplus_participant)
	local injValve  = pp.GetInjectorValvePosition(palplus_participant)
	return injValve, trapValve
end

---Resetting the signaling in the LC-Control window
---@param context IProcedureExecutionContext
function P.Signalize_Reset(context)
	local baltic = require "baltic"
	local col = baltic.ColorsRGB.Normal
	context:Signalize(col, baltic.SignalizeAll)
	context:SignalizeText(col, baltic.Naming.FlowA, baltic.Naming.FlowB)
end

---Signal the injection valve flow path
---@param context IProcedureExecutionContext
---@param col ColorsRGB
---@param palplus_participant IPalParticipant
---@param forceSignaling number|nil
function P.SignalizeValveIFlowPath(context, col, palplus_participant, forceSignaling)
	local baltic = require "baltic"
	local valveI = forceSignaling
	if valveI == nil then
		valveI = pp.GetInjectorValvePosition(palplus_participant)
	end
	if valveI ~= nil then
		if valveI == baltic.InjectionValve.Inject then
			context:Signalize(col, baltic.Naming.ValveIGrooveLoadwoLoop, baltic.Naming.ValveIGrooveLoadwoLoop)
		else
			context:Signalize(col, baltic.Naming.ValveIGroovesLoad, baltic.Naming.Loop)
		end
	end
end

---Signal the injection valve flow path depending on the valve position
---@param context IProcedureExecutionContext
---@param col ColorsRGB
---@param palplus_participant IPalParticipant
---@param forceSignaling number|nil
function P.SignalizeValveIInjectionPath(context, col, palplus_participant, forceSignaling)
	local baltic = require "baltic"
	local valveI = forceSignaling
	if valveI == nil then
		valveI = pp.GetInjectorValvePosition(palplus_participant)
	end
	if valveI ~= nil then
		if valveI == baltic.InjectionValve.Inject then
			context:Signalize(col, baltic.Naming.ValveIGroovesInjectLoop, baltic.Naming.Loop, baltic.Naming.InjectionValveToWaste)
		else
			context:Signalize(col, baltic.Naming.ValveIGrooveInject, baltic.Naming.InjectionValveToWaste)
		end
	end
end

---Calculate the loading time
---@param installed IInstalledHardwareContext
---@param experiment ExperimentInfo
---@param pumpMaxPressure integer [bar]
---@param ovenTemp number
---@param loadPress number
---@param loadMultiplier number
---@param additionalLoadVolume number
---@param useTrap boolean
---@return number
function P.reCalcLoadTime(installed, experiment, pumpMaxPressure, ovenTemp, loadPress, loadMultiplier, additionalLoadVolume, useTrap)
	local chrom = require "chromatography"
	local sampleVolume = 1							-- assume 1 uL sample volume if not set at other place
	local loadTime = 5
	if useTrap == true then
		local te = require "trap_equilibration"
		loadTime = te.trapLoadTime(installed, pumpMaxPressure, loadPress, sampleVolume, loadMultiplier, additionalLoadVolume)
	else
		local se = require "sep_equilibration"
									-- loading capillary, loop, injection capillary, transfer capillary
		local systemUnityFlow = chrom.GetUnityFlow(installed, pumpMaxPressure, true, true, true, true, false, nil)
		local a_sL = se.sepLoadCalcParam(experiment.Separator, systemUnityFlow, ovenTemp, pumpMaxPressure, loadPress, sampleVolume, loadMultiplier, additionalLoadVolume)
		loadTime = a_sL.loadingTime
	end
	return loadTime/60
end

---Calculate the equilibration time of the trap column
---@param installed IInstalledHardwareContext
---@param trap IChromatographyColumnType
---@param pumpMaxPressure integer [bar]
---@param eqPress number
---@param equilMultiplier number
---@return number
function P.calcEquiTimeTrap(installed, trap, pumpMaxPressure, eqPress, equilMultiplier)
	local chrom = require "chromatography"
	local trapUnityFlow = chrom.column_flow(trap, pumpMaxPressure, 1, chrom.viscosity_H2O_20C)
	local trapFlow = chrom.TrapTragetFlow(trap)
	local trapPress = trapFlow / trapUnityFlow
	local eqSystemPres_10ul = chrom.GetEquilibrationSystemPressure(installed, trapFlow)
	local eqFlow = eqPress / (eqSystemPres_10ul + trapPress) * trapFlow
	local trapEquiVolume = chrom.column_volume(trap) * equilMultiplier + 5	-- [uL]
	
	return trapEquiVolume / eqFlow	-- [min]
end

---Calculate the equilibration time of the separation column
---@param installed IInstalledHardwareContext
---@param separator IChromatographyColumnType
---@param ovenTemp number
---@param pumpMaxPressure integer [bar]
---@param eqPress number
---@param equiMultiplier number
---@return number
function P.calcEquiTimeSeparator(installed, separator, pumpMaxPressure, ovenTemp, eqPress, equiMultiplier)
	local chrom = require "chromatography"
	local visco = chrom.viscosity_mix(ovenTemp, 0)
	local unityPressureSeparator = chrom.column_pressure(separator, pumpMaxPressure, 1, visco)
	local unityPressureSystem = chrom.GetEquilibrationSystemPressure(installed, 1)
	local eqFlow = eqPress / (unityPressureSeparator + unityPressureSystem)
	local sepEquiVolume = chrom.column_volume(separator) * equiMultiplier

	return sepEquiVolume / eqFlow	-- [min]
end

---Store the restriction in the PAL if microElute is used
---@param context IProcedureExecutionContext
---@param pump Pump
function P.iniFlowResistance(context, pump)
	local baltic 	= require "baltic"
	---@type Zirconium
	local zr 		= require "zirconium"
	local setNewSettings = false
	local settings = pump:GetSettings()
	zr.logInstrSettings(context, settings, "settings")
	if baltic.changeResistances then
		if settings.FlowResistanceA ~= baltic.flowResA_microElute then
			settings.FlowResistanceA = baltic.flowResA_microElute
			setNewSettings = true
		end
		if settings.FlowResistanceB ~= baltic.flowResB_microElute then
			settings.FlowResistanceB = baltic.flowResB_microElute
			setNewSettings = true
		end
		if settings.FlowResistanceOut ~= baltic.flowResOUT_microElute then
			settings.FlowResistanceOut = baltic.flowResOUT_microElute
			setNewSettings = true
		end
		if setNewSettings then
			zr.setPumpSettings(context, pump, settings)
			zr.logInstrSettings(context, settings, "new settings")
		end
	end
end

---Declare parameters
---@param context InitHelper
---@param useTrap boolean
---@param useSep boolean
---@param iso boolean
---@param analysisTime number
---@param preAirVol number
---@param postAirVol number
---@param preSampleVol number|nil
function P.ini(context, useTrap, useSep, iso, analysisTime, preAirVol, postAirVol, preSampleVol)
	local baltic = require "baltic"

	if (preSampleVol == nil)  then preSampleVol  = 2.0 end
	-- experiment premises
	context:DeclareParameter("uses_trap", useTrap)
	context:DeclareParameter("uses_separator", useSep)
	context:DeclareParameter("is_isocratic", iso)
	context:DeclareParameter("analysis_time", analysisTime, "seconds", "integer")
	-- PBNE-657: Column oven is diconnected but this is ignored by the software
	context:DeclareParameter("is_use_oven_temperature", true)
	context:DeclareParameter("oven_temperature", baltic.Settings.ColumnOvenDefaultTemperature, "deg", "decimal")

	-- all parameters that aren't set by the hystar plugin must have their values declared here
	context:DeclareParameter("sample_volume", 1, "\181L", "decimal")		-- assume 1 uL sample volume if not set at other place
	context:DeclareParameter("sample_position", nil, nil, "text")

	-- injection definition: thes may be modified in the ul/partial loop injection subroutines
	context:DeclareParameter("presample_solvent_volume", preSampleVol, "\181L", "decimal")
	context:DeclareParameter("presample_air_volume", preAirVol, "\181L", "decimal")
	context:DeclareParameter("postsample_air_volume", postAirVol, "\181L", "decimal")
	context:DeclareParameter("postsample_solvent_volume", 2.0, "\181L", "decimal")
	context:DeclareParameter("sample_aspirate_speed", 1, "\181L/s", "decimal")			-- PBNE-943
	context:DeclareParameter("sample_postaspirate_delay", 500, "ms", "decimal")
--	context:DeclareParameter("sample_postaspirate_delay", 2000, "ms", "decimal")
	context:DeclareParameter("sample_inject_speed", 1, "\181L/s", "decimal")
	context:DeclareParameter("calibrant_volume", 0.6, "\181L", "decimal")
	context:DeclareParameter("calibrantTime", 10, "min", "decimal") 
	context:DeclareParameter("penetration_depth", 30, "mm", "decimal") 
	-- Set default values
	context:DeclareParameter("is_bottom_sense", true)
	context:DeclareParameter("injection_method", "uLPickup", nil, "text") -- text options are "PartialLoop" and "uLPickup"

	-- these get initialized by GenerateMethod
	context:DeclareParameter("trap", nil, nil, "custom")
	if useTrap then
		context:DeclareParameter("trap_volume", nil, "\181L", "decimal")
		context:DeclareParameter("trap_unityflow", nil, "\181L/min/bar", "decimal") -- flow per pressure unit
		context:DeclareParameter("trap_equilibration_volumemultiplier", baltic.trapEquilVolMultiplier, nil, "decimal")
		context:DeclareParameter("trap_equilibration_pressure", nil, "bar", "decimal")
		context:DeclareParameter("trap_equil_time", 0, "min", "decimal")		-- initialize with zero for DirectInfusion
		factor = 4
	end

	context:DeclareParameter("separator", nil, nil, "custom")
	context:DeclareParameter("separator_volume", nil, "\181L", "decimal")
	context:DeclareParameter("separator_unityflow", nil, "\181L/min/bar", "decimal") -- flow per pressure unit
	context:DeclareParameter("separator_equilibration_volumemultiplier", baltic.sepEquilVolMultiplier, nil, "decimal")
	context:DeclareParameter("separator_equilibration_pressure", nil, "bar", "decimal")
	context:DeclareParameter("column_load_volumemultiplier", factor, nil, "decimal")
	context:DeclareParameter("column_load_pressure", nil, "bar", "decimal")

	context:DeclareParameter("separator_equil_time", 0, "min", "decimal")		-- initialize with zero for DirectInfusion
	context:DeclareParameter("column_load_time", 0, "min", "decimal")			-- initialize with zero for DirectInfusion

	context:DeclareParameter("gradient", nil, nil, "custom")

	context:DeclareASParameter("MS Calibrant Injection", false, "")
end

---Declare toggle wash parameters
---@param context InitHelper
function P.iniToggleWashVolumePump(context)
	context:DeclareASParameter("Toggle Wash In Gradient", false, "")
	context:DeclareASParameter("Toggle Wash In Gradient", "Num Of Steps", 6, "steps", "integer")
	context:DeclareASParameter("Toggle Wash In Gradient", "Volume Organic Per Step", 125.0, "uL", "decimal")
	context:DeclareASParameter("Toggle Wash In Gradient", "Volume Aqueous Per Step", 250.0, "uL", "decimal")
	context:DeclareASParameter("Toggle Wash In Gradient", "Pump Speed", 10.0, "uL/sec", "decimal")
							-- (headerName, name, defaultValue, unit, type)
	context:DeclareASParameter("Toggle Wash In Gradient", "Toggle Organic and Aqueous", false, "")
	context:DeclareASParameter("Toggle Wash In Gradient", "Aqueous Only", false, "")
	context:DeclareASParameter("Toggle Wash In Gradient", "Only One Wash Step Organic", true, "")
	context:DeclareASParameter("Toggle Wash In Gradient", "Num Of Last Aqueous Steps", 0, "steps", "integer", true)		-- only visible in "Service" mode
end

---Declare extended wash parameters
---@param context InitHelper
---@param includeTrapSelectable boolean
function P.iniExtendedWashParameters(context, includeTrapSelectable)
	context:DeclareASParameter("Extended Wash", false, "")
	if includeTrapSelectable then context:DeclareASParameter("Extended Wash", "Include Trap Column", true, "") end
	context:DeclareASParameter("Extended Wash", "Num Of Cycles", 1, "cycles", "integer")
	context:DeclareASParameter("Extended Wash", "Flow Organic", 10.0, "uL/min", "decimal")
	context:DeclareASParameter("Extended Wash", "Volume Organic Per Cycle", 10.0, "uL", "decimal")
	context:DeclareASParameter("Extended Wash", "Flow Aqueous", 10.0, "uL/min", "decimal")
	context:DeclareASParameter("Extended Wash", "Volume Aqueous Per Cycle", 40.0, "uL", "decimal")
end

---Declare flush port parameters
---@param context InitHelper
function P.iniFlushPort(context)
	context:DeclareASParameter("Flush Injector Port", false, "")

	context:DeclareASParameter("Flush Injector Port", "Volume Organic Sealed", 300, "uL", "integer", true)		-- only visible in "Service" mode
	context:DeclareASParameter("Flush Injector Port", "Volume Organic Unsealed", 300, "uL", "integer", true)	-- only visible in "Service" mode
	context:DeclareASParameter("Flush Injector Port", "Volume Aqueous Sealed", 500, "uL", "integer", true)		-- only visible in "Service" mode
	context:DeclareASParameter("Flush Injector Port", "Volume Aqueous Unsealed", 500, "uL", "integer", true)	-- only visible in "Service" mode
	context:DeclareASParameter("Flush Injector Port", "Lift-Up Distance", 2.0, "mm", "decimal", true)			-- only visible in "Service" mode
	context:DeclareASParameter("Flush Injector Port", "Flow Rate", 10.0, "uL/s", "decimal", true)				-- only visible in "Service" mode
end

---Declare special parameters
---@param context InitHelper
function P.iniSpecialASParameters(context)
	context:DeclareASParameter("Additional Loading Volume", 0, "uL", "decimal")

	context:DeclareASParameter("Injection Settings", false, "") 
	context:DeclareASParameter("Injection Settings", "Needle Height", 0.2, "mm", "decimal") 
	context:DeclareASParameter("Injection Settings", "Draw Speed", 1, "uL/s", "decimal")
	context:DeclareASParameter("Injection Settings", "Pause Draw", 0, "sec", "decimal")

	context:DeclareASParameter("Add From Vials", false, "") 
	context:DeclareASParameter("Add From Vials", "Tray", 2, "tray no", "integer") 
	context:DeclareASParameter("Add From Vials", "1st Vial", 0, "vial no", "integer")
	context:DeclareASParameter("Add From Vials", "1st Volume", 1, "uL", "decimal")				-- "0" == ignore vial
	context:DeclareASParameter("Add From Vials", "2nd Vial", 0, "vial no", "integer") 
	context:DeclareASParameter("Add From Vials", "2nd Volume", 1, "uL", "decimal")				-- "0" == ignore vial
	context:DeclareASParameter("Add From Vials", "3rd Vial", 0, "vial no", "integer") 
	context:DeclareASParameter("Add From Vials", "3rd Volume", 1, "uL", "decimal")				-- "0" == ignore vial
	context:DeclareASParameter("Add From Vials", "Penetration Depth", 0, "mm", "decimal") 		-- "0" == bottom sense
	--	context:DeclareASParameter("  Sample Position after Vial", 0, "", "decimal") 

	context:DeclareASParameter("Air Gaps", true, "")
	context:DeclareASParameter("Air Gaps", "Pre Sample Air Gap", 0.5, "uL", "decimal") 
	context:DeclareASParameter("Air Gaps", "Post Sample Air Gap", 1.0, "uL", "decimal") 
end

---Declare injection parameters
---@param context InitHelper
function P.iniInjectionParameters(context)
	context:DeclareParameter("Additional Loading Volume", 0, "\181L", "decimal")
	context:DeclareParameter("Pre Sample Air Gap", 0.5, "\181L", "decimal")
	context:DeclareParameter("Post Sample Air Gap", 1.0, "\181L", "decimal")
end

---Declare parameters for pre and post injection needle wash
---@param context InitHelper
function P.iniNeedleWashParameters(context)
	context:DeclareASParameter("Pre Injection Needle Wash", true, "")
	context:DeclareASParameter("Pre Injection Needle Wash", "Wash In Organic", true, "")
	context:DeclareASParameter("Pre Injection Needle Wash", "Wash In Aqueous", true, "")
end

---Declare injection delays
---@param context InitHelper
function P.iniInjectionDelays(context)
	context:DeclareASParameter("Injection Delays", true, "")
	context:DeclareASParameter("Injection Delays", "After Loop to Inject", 1000, "ms", "decimal")	-- waiting time after the loop is switched to inject position
	context:DeclareASParameter("Injection Delays", "After Penetration", 1000, "ms", "decimal")		-- waiting time after injection port is penetrated with constant force
	context:DeclareASParameter("Injection Delays", "After Dispense", 1000, "ms", "decimal")			-- waiting time after the sample is dispensed into the loop before the loop is switched to load position
	context:DeclareASParameter("Injection Delays", "Before Loading Starts", 5000, "ms", "decimal")	-- waiting time before loading the sample onto the column starts
end

---Declare dissolve parameters
---@param context InitHelper
function P.iniDissolveSample(context)
	-- parameter declared as "DeclareASParameter" are shown in "Advanced Settings" in method editor
	-- Define Advanced Settings in method editor
	context:DeclareASParameter("Dissolve Sample", false, "") 
	context:DeclareASParameter("Dissolve Sample", "Sample Vial Penetration Depth", 30.0, "mm", "decimal") 
	context:DeclareASParameter("Dissolve Sample", "Solvent Volume", 10.0, "uL", "decimal") 
	context:DeclareASParameter("Dissolve Sample", "Dissolve solvent position", 4, "", "integer") 
	context:DeclareASParameter("Dissolve Sample", "Solvent vial penetration depth", 42.0, "mm", "decimal") 
	context:DeclareASParameter("Dissolve Sample", "Solvent Release Speed", 100.0, "uL/s", "decimal") 
	context:DeclareASParameter("Dissolve Sample", "Time to Dissolve", 60, "s", "decimal") 
	context:DeclareASParameter("Dissolve Sample", "Needle Height for Mixing", 0.5, "mm", "decimal") 
	context:DeclareASParameter("Dissolve Sample", "Mixing Volume", 5.0, "uL", "decimal") 
	context:DeclareASParameter("Dissolve Sample", "Mixing Cycles", 5, "", "integer")
	context:DeclareASParameter("Dissolve Sample", "Mixing Speed", 2.0, "uL/s", "decimal")

	-- this parameters are not visible in 'Advanced Settings' dialog --
	context:DeclareParameter("bottomSenseSampleVial", true)
	context:DeclareParameter("rearAirGap", 0.5, "\181L", "decimal")
	context:DeclareParameter("sampleInjectionAspirateFlowRate", 3, "\181L/s", "decimal")
	context:DeclareParameter("pullupDelay", 1, "s", "decimal")
	context:DeclareParameter("dispenseDelay", 1, "s", "decimal")
end

---Declare derivatize parameters
---@param context InitHelper
function P.iniDerivatizeSample(context)
	-- parameter declared as "DeclareASParameter" are shown in "Advanced Settings" in method editor
	-- Define Advanced Settings in method editor
	context:DeclareASParameter("Derivatize Sample", false, "") 
	context:DeclareASParameter("Derivatize Sample", "Derivatize solvent position", 4, "", "integer") 
	context:DeclareASParameter("Derivatize Sample", "Solvent vial penetration depth", 42, "mm", "decimal") 
	context:DeclareASParameter("Derivatize Sample", "Tray", 2, "tray no", "integer") 
	context:DeclareASParameter("Derivatize Sample", "1st Vial", 1, "vial no", "integer")
	context:DeclareASParameter("Derivatize Sample", "1st Volume", 0, "uL", "decimal")				-- "0" == ignore vial
	context:DeclareASParameter("Derivatize Sample", "2nd Vial", 2, "vial no", "integer") 
	context:DeclareASParameter("Derivatize Sample", "2nd Volume", 0, "uL", "decimal")				-- "0" == ignore vial
	context:DeclareASParameter("Derivatize Sample", "Penetration Depth", 0, "mm", "decimal") 		-- "0" == bottom sense
	context:DeclareASParameter("Derivatize Sample", "Derivative Release Speed", 10, "uL/s", "decimal") 
	context:DeclareASParameter("Derivatize Sample", "Needle Height for Mixing", 1.0, "mm", "decimal") 
	context:DeclareASParameter("Derivatize Sample", "Mixing Volume", 10, "uL", "decimal")
	context:DeclareASParameter("Derivatize Sample", "Mixing Cycles", 10, "", "integer")
	context:DeclareASParameter("Derivatize Sample", "Mixing Speed", 50, "uL/s", "decimal")
	context:DeclareASParameter("Derivatize Sample", "Time to Derivatize", 30, "s", "decimal") 

	-- this parameters are not visible in 'Advanced Settings' dialog --
--	context:DeclareParameter("D-bottomSenseSampleVial", true)
	context:DeclareParameter("D-AirGap", 0.5, "\181L", "decimal")
	context:DeclareParameter("D-sampleInjectionAspirateFlowRate", 3, "\181L/sec", "decimal")
	context:DeclareParameter("D-pullupDelay", 1, "sec", "decimal")
	context:DeclareParameter("D-dispenseDelay", 1, "sec", "decimal")
end

---Declare dilute parameters
---@param context InitHelper
function P.iniDiluteSeries(context)
	-- parameter declared as "DeclareASParameter" are shown in "Advanced Settings" in method editor
	-- Define Advanced Settings in method editor
	context:DeclareASParameter("Dilution Series", false, "") 
	context:DeclareASParameter("Dilution Series", "Tray", 2, "tray no", "integer") 
	context:DeclareASParameter("Dilution Series", "Source-Vial", 1, "vial no", "integer")
	context:DeclareASParameter("Dilution Series", "Source-Volume", 10, "uL", "decimal")				-- "0" == ignore vial
	context:DeclareASParameter("Dilution Series", "Prefill Volume", 90, "uL", "decimal") 
	context:DeclareASParameter("Dilution Series", "Dilution solvent position", 4, "", "integer") 
	context:DeclareASParameter("Dilution Series", "Solvent vial penetration depth", 42, "mm", "decimal") 
	context:DeclareASParameter("Dilution Series", "Penetration Depth", 0, "mm", "decimal") 		-- "0" == bottom sense
	context:DeclareASParameter("Dilution Series", "Dilution Release Speed", 10, "uL/s", "decimal") 
	context:DeclareASParameter("Dilution Series", "Needle Height for Mixing", 1.0, "mm", "decimal") 
	context:DeclareASParameter("Dilution Series", "Mixing Volume", 50, "uL", "decimal")
	context:DeclareASParameter("Dilution Series", "Mixing Cycles", 10, "", "integer")
	context:DeclareASParameter("Dilution Series", "Mixing Speed", 50, "uL/s", "decimal")
	context:DeclareASParameter("Dilution Series", "Time to Dilute", 10, "s", "decimal") 
end

---Check if an oven is connected and the temperature is ok 
---@param context IProcedureExecutionContext
---@param pump Pump
---@return boolean
function P.IsOvenAndTemperatureOK(context, pump)
	local ovenTemp = context:GetArgumentValue("oven_temperature")
	context:Log("--- Column oven set temperature: {0}", ovenTemp)
	if context:GetArgumentValue("is_use_oven_temperature") == true then
		-- if the oven is not connected at nanoElute powered on: ovenTemp == 0
		-- if the oven is disconnected during nanoElute is already powered: ovenTemp == -0.0625 => (ovenTemp < 1)
		if (ovenTemp < 1) or (ovenTemp == 20) then
			context:Log("--- Oven is not connected or disabled")
			ovenTemp = 20
			context:Log("--- internal variable ovenTemp: {0}", ovenTemp)
		else
			local actualOvenTemperature = pump:GetCurrentExternalTemperature()
			if actualOvenTemperature < 1 then
				context:Log("--- Column oven is not connected but intended to use. Temperature actual: {0}C, set: {1}C.", actualOvenTemperature, ovenTemp)
				return false
			end
		end
	end
	return true
end

---Initialize a method
---@param experiment ExperimentInfo
---@param installed IInstalledHardwareContext
---@param context AdjustmentContext
---@param useTrap boolean
function P.genMethod(experiment, installed, context, useTrap)
	local baltic = require "baltic"
	local chrom = require "chromatography"
	local pf = require "pump_functions"

	-- Propose pressure Target for equilibration and loading
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local ovenTemp = experiment.OvenTemperature
	local viscoMix = chrom.viscosity_mix(ovenTemp, 0)
	local pressTarget = math.min(pf.getColumnMaxPressure(experiment.Separator, pressSettings.GradientPumpMaxTargetPressure), 800.0)
	local sepUnityFlow = chrom.column_flow(experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, 1, viscoMix)
--	local maxSepFlow = chrom.sepMaxFlow
--	if experiment.Separator.IsMaximumFlow then maxSepFlow = math.min(maxSepFlow, experiment.Separator.MaximumFlow) end
--	pressTarget = math.min(pressTarget, maxSepFlow/sepUnityFlow)
	local eqTimeSep = P.calcEquiTimeSeparator(installed, experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, ovenTemp, pressTarget, baltic.sepEquilVolMultiplier)

	-- PBNE-673
	if eqTimeSep < 1 then eqTimeSep = 1 end		-- [minute]
	-- At least 30 seconds are taken to determine the flow through the separation column.
	-- It takes also 30 seconds to reach a stable pressure.
	-- This time must not taken into account for the flow determination.
	-- => therefor the equilibration time must be at least 60 seconds (1 minute).

	context:SetArgumentValue("separator", experiment.Separator)
	context:SetArgumentValue("separator_volume", chrom.column_volume(experiment.Separator))
	context:SetArgumentValue("separator_unityflow", sepUnityFlow) -- Method Temperature not yet available
	context:SetArgumentValue("separator_equilibration_volumemultiplier", baltic.sepEquilVolMultiplier)
	context:SetArgumentValue("separator_equilibration_pressure", pressTarget)
	context:SetArgumentValue("analysis_time", experiment.AnalysisTime.TotalSeconds)
	context:SetArgumentValue("separator_equil_time", eqTimeSep)

	context:SetArgumentValue("trap", experiment.Trap)
	if useTrap then
		local function TrapEqLoad_pressure(column_type)
			local flow = chrom.TrapTragetFlow(column_type)
			local eqSystemPres_10ul = chrom.GetEquilibrationSystemPressure(installed, flow)
			local press = math.min(pf.getColumnMaxPressure(column_type, pressSettings.GradientPumpMaxTargetPressure), chrom.column_pressure(column_type, pressSettings.GradientPumpMaxTargetPressure, flow, chrom.viscosity_H2O_20C)+eqSystemPres_10ul)
			local pressure = math.min(press, baltic.Settings.GradientPumpDefColumnEquilPressure)
			return pressure
		end
		pressTarget = TrapEqLoad_pressure(experiment.Trap)
		local eqTime = P.calcEquiTimeTrap(installed, experiment.Trap, pressTarget, baltic.trapEquilVolMultiplier, pressSettings.GradientPumpMaxTargetPressure)
		context:SetArgumentValue("trap_volume", chrom.column_volume(experiment.Trap))
		context:SetArgumentValue("trap_unityflow", chrom.column_flow(experiment.Trap, pressSettings.GradientPumpMaxTargetPressure, 1, chrom.viscosity_H2O_20C)) -- trap always at room temperature (20C)
		context:SetArgumentValue("trap_equilibration_pressure", pressTarget)
		context:SetArgumentValue("trap_equil_time", eqTime)
		factor = 4
	end
	context:SetArgumentValue("column_load_pressure", pressTarget)
	context:SetArgumentValue("oven_temperature", ovenTemp)
	local loadTime = P.reCalcLoadTime(installed, experiment, pressSettings.GradientPumpMaxTargetPressure, ovenTemp, pressTarget, factor, 0, useTrap)
	context:SetArgumentValue("column_load_time", loadTime)
end

---Validate parameters
---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
---@param excess any
function P.val(installed, context, excess)
	local baltic = require "baltic"
	local val = require "validation"
	local chrom = require "chromatography"
    val.verify_specified(context, "sample_volume")
	if excess == nil then excess = 2*context:GetArgumentValue("presample_solvent_volume") + 0.25*context:GetArgumentValue("postsample_air_volume") end
    -- loop volume is 20 uL - subtract excess to avoid pushing the sample front out of the loop
	local _, loopVolume = chrom.getCapillaryIDandLength(installed, baltic.Naming.Loop)		-- loopID is unused
    val.verify_range(context, "sample_volume", 0, loopVolume - excess)
end

---Validate toggle wash parameters and take "Flush Injector Port" into account
---@param context IProcedureValidationContext
---@param numOfSteps integer
---@param averageStepTime number
---@param lastCycleTime number
function P.valFlushInjectorPort(context, numOfSteps, averageStepTime, lastCycleTime)
	if context:GetArgumentValue("Flush Injector Port") == true then
		local gs  = require "gradient_segment"
		local val = require "validation"

		local totalGradientTime = 0
		---@type GradientContainer
		local gradient = context:GetArgumentValue("gradient")
		for segment in gs.dotnet_each(gradient) do
			totalGradientTime = segment.Time
		end
		local toggleWashEnabled = context:GetArgumentValue("Toggle Wash In Gradient")
		local toggleWashTime = 0
		if toggleWashEnabled == true then
			toggleWashTime = ((numOfSteps-1) * averageStepTime) + lastCycleTime
		end

		local remainingTimeForFlushPort = totalGradientTime - toggleWashTime
		local vol1 = context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Sealed")
		local vol2 = context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Unsealed")
		local vol = vol1 + vol2
		local speed = context:GetArgumentValue("Flush Injector Port", "Flow Rate")
		local neededTimeForFlushPort = vol / speed
		if neededTimeForFlushPort > remainingTimeForFlushPort then
			vol = remainingTimeForFlushPort * speed
--[[			local msg = nil
			if toggleWashEnabled == true then
				msg = "To keep the volume please decrease the number of steps in \"Toggle Wash In Gradient\""
			end
--]]
			if ((vol-vol2) > 0) or ((vol-vol1) < 0) then
				-- (vol-vol2))--, 
				local msg = "The sum of sealed and unsealed values must be between {0:#0} and {1:#0}"
				val.verify_rangeAS(context, "Flush Injector Port", "Volume Aqueous Sealed", 0, vol, msg)
			end
			if ((vol-vol1) > 0) or ((vol-vol2) < 0) then
				local msg = "The sum of sealed and unsealed values must be between {0:#0} and {1:#0}"
				val.verify_rangeAS(context, "Flush Injector Port", "Volume Aqueous Unsealed", 0, vol, msg)
			end
		end
		val.verify_rangeAS(context, "Flush Injector Port", "Volume Organic Sealed", 0, 500)
		val.verify_rangeAS(context, "Flush Injector Port", "Volume Organic Unsealed", 0, 500)
		val.verify_rangeAS(context, "Flush Injector Port", "Lift-Up Distance", 0.0, 10.0)
		val.verify_rangeAS(context, "Flush Injector Port", "Flow Rate", 1, 20.0)
	end
end

---Validate toggle wash parameters and take "Flush Injector Port" into account
---@param context IProcedureValidationContext
---@return integer
---@return number
---@return number
function P.valToggleWashVolumePump(context)
	if context:GetArgumentValue("Toggle Wash In Gradient") == true then
		local gs  = require "gradient_segment"
		local val = require "validation"

		local totalGradientTime = 0
		---@type GradientContainer
		local gradient = context:GetArgumentValue("gradient")
		for segment in gs.dotnet_each(gradient) do
			totalGradientTime = segment.Time
		end

		local flushLCPToolTime = 0
		if context:HasArgumentValue("Flush Tool After Toggle Wash") then
			if context:GetArgumentValue("Flush Tool After Toggle Wash") > 0 then
				local vol = context:GetArgumentValue("Flush Tool After Toggle Wash")
				local speed = context:GetArgumentValue("Flush Tool Pump Speed")
				local refillTime = math.floor((vol/pp.VolumetricPumpVolume)+0.5) * 2		-- 2 seconds for refilling the pump
				flushLCPToolTime = (vol/speed) + refillTime
			end
		end

		local flushPortTime = 0
		if context:GetArgumentValue("Flush Injector Port") == true then
			local vol = context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Sealed") + context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Unsealed")
			local refillTime = math.floor((vol/pp.VolumetricPumpVolume)+0.5) * 2		-- 2 seconds for refilling the pump
			flushPortTime = (vol/context:GetArgumentValue("Flush Injector Port", "Flow Rate")) + refillTime
		end

		val.verify_rangeAS(context, "Toggle Wash In Gradient", "Volume Organic Per Step", 50, 500)
		val.verify_rangeAS(context, "Toggle Wash In Gradient", "Volume Aqueous Per Step", 200, 1000)
		val.verify_rangeAS(context, "Toggle Wash In Gradient", "Pump Speed", 1, 20)

		local extraTimeForAqueous = 100 / context:GetArgumentValue("Toggle Wash In Gradient", "Pump Speed")
		local stepTimeOrganic = (context:GetArgumentValue("Toggle Wash In Gradient", "Volume Organic Per Step") / context:GetArgumentValue("Toggle Wash In Gradient", "Pump Speed")) + extraTimeForAqueous + 3
		local stepTimeAqueous = (context:GetArgumentValue("Toggle Wash In Gradient", "Volume Aqueous Per Step") / context:GetArgumentValue("Toggle Wash In Gradient", "Pump Speed")) + 3
		local lastStepsAqueous = context:GetArgumentValue("Toggle Wash In Gradient", "Num Of Last Aqueous Steps")
		local lastStepsTime = 0
		if lastStepsAqueous > 0 then
			lastStepsTime = stepTimeAqueous * lastStepsAqueous
		end
		local averageStepTime = (stepTimeOrganic + stepTimeAqueous) / 2
		if context:GetArgumentValue("Toggle Wash In Gradient", "Aqueous Only") == true then
			averageStepTime = stepTimeAqueous
		else
			if context:GetArgumentValue("Toggle Wash In Gradient", "Only One Wash Step Organic") == true then
				averageStepTime = (stepTimeOrganic + (stepTimeAqueous * (context:GetArgumentValue("Toggle Wash In Gradient", "Num Of Steps") - 2))) / (context:GetArgumentValue("Toggle Wash In Gradient", "Num Of Steps") - 1)
			end
		end
		local lastCycleTime = (pp.ToggleWashLastStepVolume / pp.ToggleWashLastStepSpeed) + math.floor(pp.VolumetricPumpVolume / pp.ToggleWashLastStepVolume)		-- refill time appr. 1 second per refill
		local numOfSteps = math.floor((totalGradientTime - lastCycleTime - flushPortTime - flushLCPToolTime - lastStepsTime) / averageStepTime)
		numOfSteps = math.max(numOfSteps,0)
		val.verify_rangeAS(context, "Toggle Wash In Gradient", "Num Of Steps", 0, numOfSteps)
		return numOfSteps, averageStepTime, lastCycleTime
	end
	return 0, 0, 0
end

---Validate parameters
---@param context IProcedureValidationContext
---@param useTrap boolean
---@param useSep boolean
---@param iso boolean
function P.valColumns(context, useTrap, useSep, iso)
-- This function is called whenever a data is changed in the method editor
	local val = require "validation"

	val.verify_equals(context, "uses_trap", useTrap)
	val.verify_equals(context, "uses_separator", useSep)
	val.verify_equals(context, "is_isocratic", iso)
	val.verify_text(context, "injection_method", "PartialLoop", "uLPickup")
	val.verify_specified(context, "presample_solvent_volume")
	val.verify_specified(context, "presample_air_volume")
	val.verify_specified(context, "postsample_air_volume")
	val.verify_specified(context, "postsample_solvent_volume")
	val.verify_specified(context, "sample_aspirate_speed")
	val.verify_specified(context, "sample_postaspirate_delay")
	val.verify_specified(context, "sample_inject_speed")
end

---Validate the oven temperature setting
---@param experiment ExperimentInfo
---@param context IProcedureValidationContext
function P.valOvenTemp(experiment, context)
-- This function is called whenever a data is changed in the method editor
	-- column oven temperature must be between min oven setting temp and max temp of separation column
	local val = require "validation"
	local baltic = require "baltic"

	local tempLimit = baltic.Settings.ColumnOvenMaxTemperature
	
	if experiment.Separator.IsMaximumTemperature and (experiment.Separator.MaximumTemperature > 1) then
		tempLimit = math.min(experiment.Separator.MaximumTemperature, tempLimit)
	end
	if (experiment.OvenTemperature ~= 1) then
		val.verify_specified(context, "oven_temperature")
		val.verify_range(context, "oven_temperature", baltic.Settings.ColumnOvenMinTemperature, tempLimit)
	end
end

---Validate the trap column pressure
---@param experiment ExperimentInfo
---@param context IProcedureValidationContext
---@param pumpMaxPressure integer [bar]
function P.valTrapPressure(experiment, context, pumpMaxPressure)
-- This function is called whenever a data is changed in the method editor
	--don't restrict to Trap.MaximumPressure
	local val = require "validation"
	local pf = require "pump_functions"

	local pressLimit = pf.getColumnMaxPressure(experiment.Trap, pumpMaxPressure)
	val.verify_specified(context, "column_load_pressure")
	val.verify_range(context, "column_load_pressure", 50, pressLimit)
	val.verify_specified(context, "column_load_volumemultiplier")
	val.verify_range(context,"column_load_volumemultiplier", 1, 50)
	val.verify_specified(context, "trap_equilibration_pressure")
	val.verify_range(context, "trap_equilibration_pressure", 50, pressLimit, nil, 2)
	val.verify_specified(context, "trap_equilibration_volumemultiplier")
	val.verify_range(context,"trap_equilibration_volumemultiplier", 0, 50)
	val.verify_specified(context, "trap_unityflow")
	local trapunityflow = context:GetArgumentValue("trap_unityflow")
	if (trapunityflow and trapunityflow <= 0) then
		context:Report("trap_unityflow", Severity.Error, true, "value must be greater than {0:#0.0}", 0)
	end
end

---Validate the separator column pressure
---@param experiment ExperimentInfo
---@param context IProcedureValidationContext
---@param pumpMaxPressure integer [bar]
---@param useTrap boolean
function P.valSepPressure(experiment, context, pumpMaxPressure, useTrap)
-- This function is called whenever a data is changed in the method editor
	-- check loading and equilibration pressure
	local val = require "validation"
	local pf = require "pump_functions"

	local pressLimit = pf.getColumnMaxPressure(experiment.Separator, pumpMaxPressure)
	if not useTrap then
		val.verify_specified(context, "column_load_pressure")
		val.verify_range(context, "column_load_pressure", 50, pressLimit, nil, 2)
		val.verify_specified(context, "column_load_volumemultiplier")
		val.verify_range(context,"column_load_volumemultiplier", 1, 50)
	end
	val.verify_specified(context, "separator_equilibration_pressure")
	val.verify_range(context, "separator_equilibration_pressure", 50, pressLimit, nil, 2)
	val.verify_specified(context, "separator_equilibration_volumemultiplier")
	val.verify_range(context,"separator_equilibration_volumemultiplier", 1, 50)		-- PBNE-608
	val.verify_specified(context, "separator_unityflow")
	local unityflow = context:GetArgumentValue("separator_unityflow")
	if (unityflow and unityflow <= 0) then
		context:Report("separator_unityflow", Severity.Error, true, "value must be greater than {0:#0.0}", 0)
	end
end

---Validate the flow setting
---@param experiment ExperimentInfo
---@param context IProcedureValidationContext
---@param pumpMaxPressure integer [bar]
---@param useTrap boolean
---@param isFastMode boolean
function P.valFlow(experiment, context, pumpMaxPressure, useTrap, isFastMode)
-- This function is called whenever a data is changed in the method editor
	local baltic = require "baltic"
	local gs = require "gradient_segment"
	local pf = require "pump_functions"

	local trap = nil
	if useTrap then trap = experiment.Trap end
	local sep = experiment.Separator
	local solventB = 20			--	context:GetArgumentValue("composition")
	local ovenTemp = experiment.OvenTemperature
	local flowLimit = pf.getSepColumnFlow(trap, sep, pumpMaxPressure, baltic.maxFlow, solventB*0.01, ovenTemp)
	local prev = nil
	local row = 0
	---@type GradientContainer
	local gradient = context:GetArgumentValue("gradient")
	for segment in gs.dotnet_each(gradient) do
		local msg = ""
		if (segment.Flow < 0.01 or segment.Flow > flowLimit) then
			msg = msg..DotNetString.Format("Flow must be between {0:#0.00} and {1:#0.00}\n", 0.01, flowLimit)
		end
		if (prev) then -- perform inter-segment checks here
			if (segment.Time - prev.Time <= 0) then
				msg = msg.."Time difference to previous must be at least a second\n"
			end
		end

		-- report
		if (string.len(msg) > 0) then
			msg = string.sub(msg, 1, -2) -- strip trailing newline
			context:Report("Gradient"..row, Severity.Error, true, msg)
		end
		prev = segment
		row = row + 1
	end
end

---Validate that the composition is zero (for DirectInfusion only)
---@param context IProcedureValidationContext
function P.valCompositionZero(context)		-- PBNE-662
-- This function is called whenever a data is changed in the method editor
	local gs = require "gradient_segment"
	local row = 0

	---@type GradientContainer
	local gradient = context:GetArgumentValue("gradient")
	for segment in gs.dotnet_each(gradient) do
		local msg = ""
		if (segment.Mix < 0.0 or segment.Mix > 0.0) then
			msg = msg..DotNetString.Format("Composition must be zero \n")
		end
		-- report
		if (string.len(msg) > 0) then
			msg = string.sub(msg, 1, -2) -- strip trailing newline
			context:Report("Gradient"..row, Severity.Error, true, msg)
		end
		row = row + 1
	end
end

---Calculate the calibrant time
---@param installed IInstalledHardwareContext
---@param experiment ExperimentInfo
---@param context IProcedureValidationContext
---@return number
function P.valCalibration(installed, experiment, context)
	local baltic 		= require "baltic"
	local ic 			= require "InjectCalibrant"
	local pf			= require "pump_functions"
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local pressure 		= pf.getMaxColumnPressure(nil, experiment.Separator, pressSettings.GradientPumpMaxTargetPressure)
	local t, _, _, _, _ = ic.calibrantInjectionTime(installed,
		(context:GetArgumentValue("separator_unityflow")*context:GetArgumentValue("separator_equilibration_pressure")),
		context:GetArgumentValue("separator_unityflow"),
		(baltic.SystemVolume+context:GetArgumentValue("separator_volume")),
		context:GetArgumentValue("calibrant_volume"),
		pressure,
		pressSettings.GradientPumpMaxTargetPressure)
	return t
end

---Validate injection path wash parameters
---@param context IProcedureValidationContext
function P.valInjectionPathWash(context)
	if context:GetArgumentValue("Injection Path Wash") == true then
		local val = require "validation"
		val.verify_specified(context, "Injection Path Wash")
		val.verify_rangeAS(context, "Injection Path Wash", "Time", 1, 300)
	end
end

---Validate extended wash parameters
---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
---@param includeTrapColumn boolean
function P.valExtendedWashParameters(installed, context, includeTrapColumn)
	if context:GetArgumentValue("Extended Wash") == true then
		local baltic = require "baltic"
		local chrom = require "chromatography"
		local pf = require "pump_functions"
		local val = require "validation"

		local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
		local maxPressure = 800
		local N = baltic.Naming

		val.verify_specified(context, "Extended Wash")
		val.verify_specified(context, "Extended Wash", "Flow Organic")
		val.verify_specified(context, "Extended Wash", "Volume Organic Per Cycle")
		val.verify_specified(context, "Extended Wash", "Flow Aqueous")
		val.verify_specified(context, "Extended Wash", "Volume Aqueous Per Cycle")

		if (includeTrapColumn == true) then
			-- get maxPressure from trap column configuration
			maxPressure = pf.getTrapColumnPressure(context:GetArgumentValue("trap"), pressSettings.GradientPumpMaxTargetPressure, maxPressure)
		end

		local organicUnityFlowInjectionPath = 1/(1/chrom.capillary_flow_byName(installed, N.ValveAToInjectionValve, 1, chrom.viscosity_ACN_20C)+1/chrom.capillary_flow_byName(installed, N.InjectToTrap, 1, chrom.viscosity_ACN_20C))
		local maxOrganicFlow = maxPressure * organicUnityFlowInjectionPath

		local aqueousUnityFlowInjectionPath = 1/(1/chrom.capillary_flow_byName(installed, N.ValveAToInjectionValve, 1, chrom.viscosity_H2O_20C)+1/chrom.capillary_flow_byName(installed, N.InjectToTrap, 1, chrom.viscosity_H2O_20C))
		local maxAqueousFlow = maxPressure * aqueousUnityFlowInjectionPath

		local maxOrganicVolume = 100	--chrom.getCapillaryIDandLength(installed, "loop")
		local maxAqueousVolume = 300

		val.verify_rangeAS(context, "Extended Wash", "Flow Organic", 0.1, maxOrganicFlow)
		val.verify_rangeAS(context, "Extended Wash", "Volume Organic Per Cycle", 0.1, maxOrganicVolume)
		val.verify_rangeAS(context, "Extended Wash", "Flow Aqueous", 0.1, maxAqueousFlow)
		val.verify_rangeAS(context, "Extended Wash", "Volume Aqueous Per Cycle", 0.1, maxAqueousVolume)
	end
end

---Validate special parameters
---@param context IProcedureValidationContext
function P.valSpecialASParameters(context)
	local val = require "validation"
	val.verify_specified(context,"Additional Loading Volume")
	val.verify_range(context,"Additional Loading Volume", -10, 20)

	if context:GetArgumentValue("Injection Settings") == true then
		val.verify_specified(context,"Injection Settings") 
		val.verify_specifiedAS(context,"Injection Settings", "Needle Height")
		val.verify_rangeAS(context,"Injection Settings", "Needle Height", 0.1,42)
		val.verify_specifiedAS(context,"Injection Settings", "Draw Speed")
		val.verify_rangeAS(context,"Injection Settings", "Draw Speed", 0.1,100)
		val.verify_specifiedAS(context,"Injection Settings", "Pause Draw")
		val.verify_rangeAS(context,"Injection Settings", "Pause Draw", 0,120)
	end

	if context:GetArgumentValue("Add From Vials") == true then
		val.verify_specified(context,"Add From Vials") 
		val.verify_specifiedAS(context,"Add From Vials", "Tray")
		val.verify_rangeAS(context,"Add From Vials", "Tray", 1,2)
		val.verify_specifiedAS(context,"Add From Vials", "1st Vial")
		val.verify_rangeAS(context,"Add From Vials", "1st Vial", 0,384)				-- "0" == ignore vial
		val.verify_specifiedAS(context,"Add From Vials", "1st Volume")
		val.verify_rangeAS(context,"Add From Vials", "1st Volume", 0.5,10)
		val.verify_specifiedAS(context,"Add From Vials", "2nd Vial")
		val.verify_rangeAS(context,"Add From Vials", "2nd Vial", 0,384)				-- "0" == ignore vial 
		val.verify_specifiedAS(context,"Add From Vials", "2nd Volume")
		val.verify_rangeAS(context,"Add From Vials", "2nd Volume", 0.5,10)
		val.verify_specifiedAS(context,"Add From Vials", "3rd Vial")
		val.verify_rangeAS(context,"Add From Vials", "3rd Vial", 0,384)				-- "0" == ignore vial 
		val.verify_specifiedAS(context,"Add From Vials", "3rd Volume")
		val.verify_rangeAS(context,"Add From Vials", "3rd Volume", 0.5,10)
		val.verify_specifiedAS(context,"Add From Vials", "Penetration Depth")
		val.verify_rangeAS(context,"Add From Vials", "Penetration Depth", 0,42)		-- "0" == bottom sense
	end

	if context:GetArgumentValue("Air Gaps") == true then
		val.verify_specified(context,"Air Gaps") 
		val.verify_specifiedAS(context, "Air Gaps", "Pre Sample Air Gap")
		val.verify_rangeAS(context,"Air Gaps", "Pre Sample Air Gap", 0,5) 
		val.verify_specifiedAS(context,"Air Gaps", "Post Sample Air Gap")
		val.verify_rangeAS(context,"Air Gaps", "Post Sample Air Gap", 0,5)
	end
	if not context:GetArgumentValue("is_bottom_sense") then
		val.verify_specified(context, "penetration_depth")
		val.verify_range(context, "penetration_depth", 0, 45)
	end
end

---Validate injection parameters
---@param context IProcedureValidationContext
function P.valInjectionParameters(context)
	local val = require "validation"
	val.verify_specified(context,"Additional Loading Volume")
	val.verify_range(context,"Additional Loading Volume", -10,20)

	val.verify_specified(context,"Pre Sample Air Gap")
	val.verify_range(context,"Pre Sample Air Gap",0,5) 
	val.verify_specified(context,"Post Sample Air Gap")
	val.verify_range(context,"Post Sample Air Gap",0,5)
	if not context:GetArgumentValue("is_bottom_sense") then
		val.verify_specified(context, "penetration_depth")
		val.verify_range(context, "penetration_depth", 0, 45)
	end
--[[
	val.verify_specifiedAS(context,"Additional Loading Volume", "Pre Sample Air Gap")
	val.verify_rangeAS(context,"Additional Loading Volume", "Pre Sample Air Gap",0,5) 
	val.verify_specifiedAS(context,"Additional Loading Volume", "Post Sample Air Gap")
	val.verify_rangeAS(context,"Additional Loading Volume", "Post Sample Air Gap",0,5) 
--]]
--	context:DeclareParameter(context,"Wash vial", false, "") 
--	val.verify_specified(context,"  Position")
--	val.verify_range(context,"  Position",0,4) 				-- Item Position (3 == standard)
end

---Validate dissolve parameters
---@param context IProcedureValidationContext
---@param maxItemPosition any
function P.valDissolveSample(context, maxItemPosition)
	if context:GetArgumentValue("Dissolve Sample") == true then
		local val = require "validation"
		local maxSolventPrenetrationDepth = 42
		val.verify_specified(context, "Dissolve Sample")
		val.verify_specifiedAS(context, "Dissolve Sample", "Solvent Volume")
		val.verify_specifiedAS(context,"Dissolve Sample", "Sample Vial Penetration Depth")
		val.verify_rangeAS(context,"Dissolve Sample", "Sample Vial Penetration Depth", 0, 42)
		val.verify_rangeAS(context,"Dissolve Sample", "Solvent Volume", 0, 50)
		val.verify_specifiedAS(context,"Dissolve Sample", "Dissolve solvent position")
		val.verify_rangeAS(context,"Dissolve Sample","Dissolve solvent position", 1, maxItemPosition)
		if (context:GetArgumentValue("Dissolve Sample", "Dissolve solvent position") <= 3) then maxSolventPrenetrationDepth = 30 end
		val.verify_specifiedAS(context,"Dissolve Sample", "Solvent vial penetration depth")
		val.verify_rangeAS(context,"Dissolve Sample","Solvent vial penetration depth", 0, maxSolventPrenetrationDepth)
		val.verify_specifiedAS(context,"Dissolve Sample", "Solvent Release Speed")
		val.verify_rangeAS(context,"Dissolve Sample", "Solvent Release Speed", 0.1, 100)
		val.verify_specifiedAS(context,"Dissolve Sample", "Time to Dissolve")
		val.verify_rangeAS(context,"Dissolve Sample", "Time to Dissolve", 0, 120)
		val.verify_specifiedAS(context,"Dissolve Sample", "Needle Height for Mixing")
		val.verify_rangeAS(context,"Dissolve Sample", "Needle Height for Mixing", 0, 14)
		val.verify_specifiedAS(context,"Dissolve Sample", "Mixing Volume")
		val.verify_rangeAS(context,"Dissolve Sample", "Mixing Volume", 0, 50)
		val.verify_specifiedAS(context,"Dissolve Sample", "Mixing Cycles")
		val.verify_rangeAS(context,"Dissolve Sample", "Mixing Cycles", 0, 50)
		val.verify_specifiedAS(context,"Dissolve Sample", "Mixing Speed")
		val.verify_rangeAS(context,"Dissolve Sample", "Mixing Speed", 0.1, 100)

		val.verify_specified(context,"rearAirGap")
		val.verify_range(context,"rearAirGap", 0, 10)
		val.verify_specified(context,"sampleInjectionAspirateFlowRate")
		val.verify_range(context,"sampleInjectionAspirateFlowRate", 0.1, 100)
		val.verify_specified(context,"pullupDelay")
		val.verify_range(context,"pullupDelay", 0, 60)
		val.verify_specified(context,"dispenseDelay")
		val.verify_range(context,"dispenseDelay", 0, 60)
	end
end

---Validate derivatize parameters
---@param context IProcedureValidationContext
---@param maxItemPosition number
function P.valDerivatizeSample(context, maxItemPosition)
	if context:GetArgumentValue("Derivatize Sample") == true then
		local val = require "validation"
		local maxSolventPrenetrationDepth = 42
		val.verify_specified(context,"Derivatize Sample") 
		val.verify_specifiedAS(context,"Derivatize Sample", "Derivatize solvent position")
		val.verify_rangeAS(context,"Derivatize Sample", "Derivatize solvent position", 1, maxItemPosition)
		if (context:GetArgumentValue("Derivatize Sample", "Derivatize solvent position") <= 3) then maxSolventPrenetrationDepth = 30 end
		val.verify_specifiedAS(context,"Derivatize Sample", "Solvent vial penetration depth")
		val.verify_rangeAS(context,"Derivatize Sample","Solvent vial penetration depth", 0, maxSolventPrenetrationDepth)
		val.verify_specifiedAS(context,"Derivatize Sample", "Tray")
		val.verify_rangeAS(context,"Derivatize Sample", "Tray",1,2) 
		val.verify_specifiedAS(context,"Derivatize Sample", "1st Vial")
		val.verify_rangeAS(context,"Derivatize Sample", "1st Vial", 1,384)
		val.verify_specifiedAS(context,"Derivatize Sample", "1st Volume")
		val.verify_rangeAS(context,"Derivatize Sample", "1st Volume", 0,10)
		val.verify_specifiedAS(context,"Derivatize Sample", "2nd Vial")
		val.verify_rangeAS(context,"Derivatize Sample", "2nd Vial", 1,384)
		val.verify_specifiedAS(context,"Derivatize Sample", "2nd Volume")
		val.verify_rangeAS(context,"Derivatize Sample", "2nd Volume", 0,10)
		val.verify_specifiedAS(context,"Derivatize Sample", "Penetration Depth")
		val.verify_rangeAS(context,"Derivatize Sample", "Penetration Depth",0,42) 
		val.verify_specifiedAS(context,"Derivatize Sample", "Derivative Release Speed")
		val.verify_rangeAS(context,"Derivatize Sample", "Derivative Release Speed",0.1,100) 
		val.verify_specifiedAS(context,"Derivatize Sample", "Needle Height for Mixing")
		val.verify_rangeAS(context,"Derivatize Sample", "Needle Height for Mixing",0,14) 
		val.verify_specifiedAS(context,"Derivatize Sample", "Mixing Volume")
		val.verify_rangeAS(context,"Derivatize Sample", "Mixing Volume",0,50) 
		val.verify_specifiedAS(context,"Derivatize Sample", "Mixing Cycles")
		val.verify_rangeAS(context,"Derivatize Sample", "Mixing Cycles",0,50) 
		val.verify_specifiedAS(context,"Derivatize Sample", "Mixing Speed")
		val.verify_rangeAS(context,"Derivatize Sample", "Mixing Speed",0.1,100) 
		val.verify_specifiedAS(context,"Derivatize Sample", "Time to Derivatize")
		val.verify_rangeAS(context,"Derivatize Sample", "Time to Derivatize",0,600) 
		val.verify_specified(context, "D-AirGap")
		val.verify_range(context, "D-AirGap",0,10) 
		val.verify_specified(context, "D-sampleInjectionAspirateFlowRate")
		val.verify_range(context, "D-sampleInjectionAspirateFlowRate",1,100) 
		val.verify_specified(context, "D-pullupDelay")
		val.verify_range(context, "D-pullupDelay",0,60) 
		val.verify_specified(context, "D-dispenseDelay")
		val.verify_range(context, "D-dispenseDelay",0,60)
	end
end

---Validate dilution parameters
---@param context IProcedureValidationContext
---@param maxItemPosition number
function P.valDiluteSeries(context, maxItemPosition)
	if context:GetArgumentValue("Dilution Series") == true then
		local val = require "validation"
		val.verify_specified(context, "Dilution Series")
		val.verify_specifiedAS(context,"Dilution Series", "Tray")
		val.verify_rangeAS(context,"Dilution Series", "Tray", 1, 2)
		val.verify_specifiedAS(context,"Dilution Series", "Source-Vial")
		val.verify_rangeAS(context,"Dilution Series", "Source-Vial", 1, 384)
		val.verify_specifiedAS(context,"Dilution Series", "Source-Volume")
		val.verify_rangeAS(context,"Dilution Series", "Source-Volume", 0, 100)
		val.verify_specifiedAS(context,"Dilution Series", "Prefill Volume")
		val.verify_rangeAS(context,"Dilution Series", "Prefill Volume", 0, 1000)
		val.verify_specifiedAS(context,"Dilution Series", "Dilution solvent position")
		val.verify_rangeAS(context,"Dilution Series", "Dilution solvent position", 1, maxItemPosition)
		val.verify_specifiedAS(context,"Dilution Series", "Solvent vial penetration depth")
		val.verify_rangeAS(context,"Dilution Series", "Solvent vial penetration depth", 0, 42)
		val.verify_specifiedAS(context,"Dilution Series", "Penetration Depth")
		val.verify_rangeAS(context,"Dilution Series", "Penetration Depth", 0, 42)
		val.verify_specifiedAS(context,"Dilution Series", "Dilution Release Speed")
		val.verify_rangeAS(context,"Dilution Series", "Dilution Release Speed", 0.1, 100)
		val.verify_specifiedAS(context,"Dilution Series", "Needle Height for Mixing")
		val.verify_rangeAS(context,"Dilution Series", "Needle Height for Mixing", 0, 14)
		val.verify_specifiedAS(context,"Dilution Series", "Mixing Volume")
		val.verify_rangeAS(context,"Dilution Series", "Mixing Volume", 0, 50)
		val.verify_specifiedAS(context,"Dilution Series", "Mixing Cycles")
		val.verify_rangeAS(context,"Dilution Series", "Mixing Cycles", 0, 50)
		val.verify_specifiedAS(context,"Dilution Series", "Mixing Speed")
		val.verify_rangeAS(context,"Dilution Series", "Mixing Speed", 0.1, 100)
		val.verify_specifiedAS(context,"Dilution Series", "Time to Dilute")
		val.verify_rangeAS(context,"Dilution Series", "Time to Dilute", 0, 600)
	end
end

---Set external oven temperature
---@param context IProcedureExecutionContext
function P.run(context)
	local baltic = require "baltic"
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)	

	-- set the column oven temperature
	local ovenTempSetPt = context:GetArgumentValue("oven_temperature")
	pump:SetExternalTemperature(ovenTempSetPt)
	context:Log("Set oven temperature to {0}C", ovenTempSetPt)
end

return P
