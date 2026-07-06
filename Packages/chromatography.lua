-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

﻿local Date = "2025/06/17"

-- create package table
local P = {}

--Viscoisty of H2O/ACN @ 20 degrees C
P.viscosity_H2O_20C = 0.009331
P.viscosity_ACN_20C = 0.003685

-- nE1: tubings: 350/20um + 150/20um + 100/30um + 100/30um: backpressure: 214 bar @ 10ul/min
-- nE2: tubings: 350/100um + 150/20um + 100/30um + 100/30um: backpressure: 75 bar @ 10ul/min
-- P.eqSystemPres_10ul = 214

P.sepMaxFlow = 10
P.trapMaxFlow = 10
local default_porosity = 0.42

---Get the capillary ID and Length
---@param installed IInstalledHardwareContext
---@param capillaryName string
---@return number
---@return number
function P.getCapillaryIDandLength(installed, capillaryName)
	local length = installed:GetArgumentValue1(capillaryName)
	local id = installed:GetArgumentValue2(capillaryName)/1000
	return id, length		-- ID [mm], Length [mm]
end

---Calculate the capillary volume
---@param installed IInstalledHardwareContext
---@param capillaryName string
---@return number
function P.GetCapillaryVolume(installed, capillaryName)
	-- id [µm]; length [mm]
	local id, length = P.getCapillaryIDandLength(installed, capillaryName)
	return ((0.5*id)^2)*math.pi*length*1e-6		-- [µL]
end

---Calculate the gradient dead volume
---@param installed IInstalledHardwareContext
---@return number
function P.GetGradientDeadVolume(installed)
	local baltic = require "baltic"
	local N = baltic.Naming
	local deadVolumeMTtoVT = P.GetCapillaryVolume(installed, N.MixTeeToTrapValve)
	local deadVolumeVTtoColumn = P.GetCapillaryVolume(installed, N.TransferLine)
	local deadVolumemixTee = 0.066
	local deadVolumeVTlongGroove = 0.109
	return deadVolumeMTtoVT+deadVolumeVTtoColumn+deadVolumemixTee+deadVolumeVTlongGroove
end

---Calculate the volumetric flow through a capillary at a given back pressure
---@param id_mm number
---@param length_mm number
---@param pressure number
---@param viscosity number
---@return number
function P.capillary_flow(id_mm, length_mm, pressure, viscosity)
	local r_cm = id_mm / 10 * 0.5						-- mm
	return (pressure*r_cm^4*math.pi)/(8*viscosity*length_mm/10)*60000000000
end

---Calculate the volumetric flow through a capillary at a given back pressure
---@param installed IInstalledHardwareContext
---@param capillaryName string
---@param pressure number
---@param viscosity number
---@return number
function P.capillary_flow_byName(installed, capillaryName, pressure, viscosity)
	local id_mm, length_mm = P.getCapillaryIDandLength(installed, capillaryName)
	local r_cm = id_mm / 10 * 0.5						-- mm
	return (pressure*r_cm^4*math.pi)/(8*viscosity*length_mm/10)*60000000000
end

---Calculate the capillary generated back pressure at a given volumetric flow rate
---@param installed IInstalledHardwareContext
---@param capillaryName string
---@param flow number
---@param viscosity number
---@param useTolerance boolean
---@return number
function P.capillary_pressure(installed, capillaryName, flow, viscosity, useTolerance)
	local id, length = P.getCapillaryIDandLength(installed, capillaryName)
	if useTolerance == true then id = id-0.001 end		-- subtract 1µm from the ID
	local targetPress = flow / P.capillary_flow(id, length, 1, viscosity)	-- ID minus1µm tolerance
	return targetPress
end

---Calculate the loop back pressure at a given volumetric flow rate
---@param installed IInstalledHardwareContext
---@param flow number
---@param viscosity number
---@return number
function P.loop_pressure(installed, flow, viscosity)
	local id, volume = P.getCapillaryIDandLength(installed, "loop")
	local length = volume/(math.pi*(id*0.5)^2)
	local targetPress = flow / P.capillary_flow(id, length, 1, viscosity)
	return targetPress
end

---Calculate the loop flow at a given pressure
---@param installed IInstalledHardwareContext
---@param pressure number
---@param viscosity number
---@return number
function P.loopFlow(installed, pressure, viscosity)
	local id, volume = P.getCapillaryIDandLength(installed, "loop")
	local length = volume/(math.pi*(id*0.5)^2)
	local flow = P.capillary_flow(id, length, pressure, viscosity)
	return flow
end

---Calcuate the system pressure at a given flow
---@param installed IInstalledHardwareContext
---@param desiredFlow number
---@return number
function P.GetEquilibrationSystemPressure(installed, desiredFlow)
	local baltic = require "baltic"
	local eqPressureSystem = P.capillary_pressure(installed, baltic.Naming.ValveAToInjectionValve, desiredFlow, P.viscosity_H2O_20C, false)+P.capillary_pressure(installed, baltic.Naming.InjectToTrap, desiredFlow, P.viscosity_H2O_20C, false)
	return eqPressureSystem
end

---comment
---@param installed IInstalledHardwareContext
---@param pumpMaxPressure integer [bar]
---@param isLoadingCap boolean
---@param isLoop boolean
---@param isInjectionCap boolean
---@param isTransferCap boolean
---@param isTrap boolean
---@param sepColumn IChromatographyColumnType?
---@return number
function P.GetUnityFlow(installed, pumpMaxPressure, isLoadingCap, isLoop, isInjectionCap, isTransferCap, isTrap, sepColumn)
	local baltic = require "baltic"
	local N = baltic.Naming
	local pressure = 1
	local reciprocalValue = 0
	if isLoadingCap then reciprocalValue = 1/P.capillary_flow_byName(installed, N.ValveAToInjectionValve, pressure, P.viscosity_H2O_20C) end
	if isLoop then reciprocalValue = reciprocalValue + 1/P.loopFlow(installed, pressure, P.viscosity_H2O_20C) end
	if isInjectionCap then reciprocalValue = reciprocalValue + 1/P.capillary_flow_byName(installed, N.InjectToTrap, pressure, P.viscosity_H2O_20C) end
	if isTransferCap then reciprocalValue = reciprocalValue + 1/P.capillary_flow_byName(installed, N.TransferLine, pressure, P.viscosity_H2O_20C) end
	if isTrap then reciprocalValue = reciprocalValue + 1/P.capillary_flow_byName(installed, N.Trap, pressure, P.viscosity_H2O_20C) end
	if sepColumn ~= nil then reciprocalValue = reciprocalValue + 1/P.column_flow(sepColumn, pumpMaxPressure, pressure, P.viscosity_H2O_20C) end

	local unityFlow = 1/reciprocalValue
	return unityFlow
end

---Calculate the unity flow thrue the flowSensor A capillary -> restriction capillary -> mixTee capillary -> transfer capillary
---@param installed IInstalledHardwareContext
---@return number
function P.GetUnityFlowSystem(installed)
	local baltic = require "baltic"
	local N = baltic.Naming
	local pressure = 1
	local unityFlowVAtoFlowA = P.capillary_flow_byName(installed, N.ValveAToFS, pressure, P.viscosity_H2O_20C)
	local unityFlowAToMixTee = P.capillary_flow_byName(installed, N.FSAToMixTee, pressure, P.viscosity_H2O_20C)
	local unityMixTeetoVT = P.capillary_flow_byName(installed, N.MixTeeToTrapValve, pressure, P.viscosity_H2O_20C)
	local unityFlowVTtoColumn = P.capillary_flow_byName(installed, N.TransferLine, pressure, P.viscosity_H2O_20C)
	local unityFlow = 1/(1/unityFlowVAtoFlowA+1/unityFlowAToMixTee+1/unityMixTeetoVT+1/unityFlowVTtoColumn)
	return unityFlow
end

---Calculate the unity flow thrue the loading capillary, injection capillary, trap capillary
---@param installed IInstalledHardwareContext
---@return number
function P.GetUnityFlowTrap(installed)
	local baltic = require "baltic"
	local N = baltic.Naming
	local pressure = 1
	local unityFlowVAtoVI = P.capillary_flow_byName(installed, N.ValveAToInjectionValve, pressure, P.viscosity_H2O_20C)
	local unityFlowVItoVT = P.capillary_flow_byName(installed, N.InjectToTrap, pressure, P.viscosity_H2O_20C)
	local unityFlowTrap = P.capillary_flow_byName(installed, N.Trap, pressure, P.viscosity_H2O_20C)
	local unityFlow = 1/(1/unityFlowVAtoVI+1/unityFlowVItoVT+1/unityFlowTrap)
	return unityFlow
end

---Check if the parameter is available
---@param context IProcedureExecutionContext
---@param column IChromatographyColumnType
---@return boolean
function P.IsColumnParameterSet(context, column)
	local parameterOK = true
	local msg = ""
	if column == nil then
		---@type Severity
		local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
		context:Report("Missing column", Severity.Error, true, "please choose a column")
		return false
	end
	if column.Length == nil or column.Length <= 0 then
		msg = " - Length"
		parameterOK = false
	end
	if column.ParticleDiameter == nil or column.ParticleDiameter <= 0 then
		msg = msg.. " - Particle size"
		parameterOK = false
	end
	if column.ColumnDiameter == nil or column.ColumnDiameter <= 0 then
		msg = msg.. " - Inner diameter"
		parameterOK = false
	end
	if column.IsAdvancedSettings then
		if column.ColumnVolume == nil or column.ColumnVolume <= 0 then
			msg = msg.. " - Column volume"
			parameterOK = false
		end
		if column.UnityFlow == nil or column.UnityFlow <= 0 then
			msg = msg.. " - Column unity flow"
			parameterOK = false
		end
	end
	if not parameterOK then
		---@type Severity
		local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
		context:Report("Column editor", Severity.Error, true, "missing column parameters, please set: {0}", msg)
	end
	return parameterOK
end

---Calculate the viscosity of water/acetonitrile mixture
---@param temperature number
---@param percentageACN number
---@return number
function P.viscosity_mix(temperature, percentageACN)
	local kelvin = 273.15
	temperature = temperature + kelvin
	local viscosityMix = (math.exp(percentageACN*(-3.476+726/temperature)+(1-percentageACN)*(-5.414+1566/temperature)+percentageACN*(1-percentageACN)*(-1.762+929/temperature)))/100
	return viscosityMix
end

---Return the trap target flow
---@param trap_type IChromatographyColumnType
---@return number
function P.TrapTragetFlow(trap_type)
     -- a max pressure value in the column properites is not considered as the trap is always open during equilibration and loading
    local targetflow = 10 -- this is the standard trap equilibration / loading flow
	if trap_type.IsMaximumFlow and (trap_type.MaximumFlow > 1) then		-- 1uL is the min expected flow
		targetflow = math.min(trap_type.MaximumFlow, targetflow)
	end
	return targetflow
end

---Return the column porosity
---@param column_type IChromatographyColumnType
---@return number
local function determine_porosity(column_type)
	local porosity = default_porosity
	if (column_type.IsColumnPorosity) then
		porosity = column_type.ColumnPorosity
	end
	return porosity
end

---Calculate the column volume
---@param column_type IChromatographyColumnType
---@return number
function P.column_volume(column_type)
	if column_type.IsAdvancedSettings then
		if column_type.ColumnVolume ~= nil and column_type.ColumnVolume > 0 then
			return column_type.ColumnVolume
		end
	end
	local p = determine_porosity(column_type)
	local r = column_type.ColumnDiameter * 0.5
	local porosity = column_type.Length * math.pi * r^2 * p
	return porosity
end

---Calculate the volumetric flow through a column at a given back pressure
---@param column_type IChromatographyColumnType
---@param pumpMaxPressure integer [bar]
---@param pressure number [bar]
---@param viscosity number
---@return number
function P.column_flow(column_type, pumpMaxPressure, pressure, viscosity)
	local pf = require "pump_functions"
	local press = math.min(pf.getColumnMaxPressure(column_type, pumpMaxPressure), pressure)
	if column_type.IsAdvancedSettings then
		if column_type.UnityFlow ~= nil and column_type.UnityFlow > 0 then
			return column_type.UnityFlow*press
		end
	end
	local p = determine_porosity(column_type)
	local r = column_type.ColumnDiameter * 0.5
	local columnFlow = 10^3 * (press * 10^6 * (column_type.ParticleDiameter*0.0001)^2 * p^3 * math.pi * (r*0.1)^2 * 60) / (180 * viscosity * column_type.Length * 0.1 * (1-p)^2)
	return columnFlow
end

---Calculate the column generated back pressure at a given volumetric flow rate
---@param column_type IChromatographyColumnType
---@param pumpMaxPressure integer [bar]
---@param flow number
---@param viscosity number
---@return number
function P.column_pressure(column_type, pumpMaxPressure, flow, viscosity)
	local pf = require "pump_functions"
	local press = pf.getColumnMaxPressure(column_type, pumpMaxPressure)
	local columnUnityFlow = P.column_flow(column_type, pumpMaxPressure, 1, viscosity)
	local targetPress = flow / columnUnityFlow
	local columnPressure = math.min(press, targetPress)
	return columnPressure
end

return P