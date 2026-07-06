-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/11/17"

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")

local P = {}

-- This function is needed outside to have the loading time
---Calculate the loading paramters for the separation column
---@param separator IChromatographyColumnType
---@param systemUnityFlow number
---@param ovenTemp number
---@param pumpMaxPressure integer [bar]
---@param loadPress number
---@param sampleVolume number
---@param loadMultiplier number
---@param additionalLoadVolume number
---@return SeparatorLoadingParameter
function P.sepLoadCalcParam(separator, systemUnityFlow, ovenTemp, pumpMaxPressure, loadPress, sampleVolume, loadMultiplier, additionalLoadVolume)
	local baltic = require "baltic"
	local chrom = require "chromatography"
	local visco = chrom.viscosity_mix(ovenTemp, 0)
	---@type SeparatorLoadingParameter
	local a_sL = {unityFlow=0, loadingPress=0, loadingFlow=0, loadingVolume=0, loadingTime=0, maxLoadingVolume=0, timeOut=0}
	a_sL.unityFlow = chrom.column_flow(separator, pumpMaxPressure, 1, visco)
	local unityFlow = 1/(1/systemUnityFlow + 1/a_sL.unityFlow)

	a_sL.loadingPress = loadPress
	a_sL.loadingFlow = unityFlow * a_sL.loadingPress  -- theoretical flow at analytical column equilibration.
			-- don't add the predefined volume of the tubings and valves (0.4 uL). 
	a_sL.loadingVolume = sampleVolume * loadMultiplier + 2 + additionalLoadVolume
	a_sL.loadingTime = 60*a_sL.loadingVolume/a_sL.loadingFlow	-- [s]
	a_sL.maxLoadingVolume = a_sL.loadingVolume*10+a_sL.loadingTime/30		--(+2 uL leakage/min)
	a_sL.timeOut = a_sL.loadingTime * 10
	if baltic.microElute == true then a_sL.timeOut = a_sL.timeOut * 10 end			-- time out must be much higher for microElute LC

	return a_sL
end

---Load the sample onto the separation column
---If available it is taken into account the "LT: Injection tubing @1000bar" or "LT: Pump A @1000bar" leakage rate
---Else if available it is taken into account the unity flow of "FRT: Route 3 with loop"
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param a_sL SeparatorLoadingParameter
---@param ovenTemp number
---@param yield_func function
---@return string
---@return string
function P.sepSampleLoading(installed, context, a_sL, ovenTemp, yield_func)
	---@type Severity
	Severity = luanet.import_type("Bruker.Lc.Business.Severity")
	local baltic = 	require "baltic"
	local cplae = 	require "const_pressure_load_and_equilibration"
	local pf = 		require "pump_functions"
	local pp = 		require "palplus"
	local pr = 		require "PreRunFunctions"
	local qaw = 	require "queue_autosampler_wash"
	---@type Zirconium
	local zr = 		require "zirconium"
	---@type IPalParticipant
	local execLeft 	= context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local err = ""
	local msg = ""
	local p,v,f,t = 1,2,3,4*60
--[[ disabled for plug-in 1.0
	local leakFlow = 0
	local leakVolume = 0

	-- take a leak into account when calculation of the expected equilibration volume
	if (installed:Contains("LT: Pump A @1000bar")) or (installed:Contains("LT: Injection tubing @1000bar")) then
		if (installed:Contains("LT: Pump A @1000bar")) then
			local unityLeakRate_PumpA = installed:GetArgumentValue1("LT: Pump A @1000bar") / 1000		-- leakage is measured @ 1000 bar
			leakFlow = unityLeakRate_PumpA * a_sL.loadingPress
			a_sL.loadingFlow = a_sL.loadingFlow + leakFlow
			context:Log("LT: Pump A / valve A unity leak rate from system information: {0} \181L/min/bar", unityLeakRate_PumpA)
			context:Log("Calculated leak flow of LT pumpA and valve A from system information: {0} \181L/min", leakFlow)
			context:Log("Calculated load flow with leak included: {0} \181L/min", a_sL.loadingFlow + leakFlow)
			context:Log("Calculated load volume to be used without a leak: {0} \181L", a_sL.loadingVolume)
			leakVolume = leakFlow * a_sL.loadingTime/60
			context:Log("Calculated load volume with leak included: {0} \181L", (a_sL.loadingVolume + leakVolume))
		end
		if (installed:Contains("LT: Injection tubing @1000bar")) then
			local unityLeakRate_InjTubings = installed:GetArgumentValue1("LT: Injection tubing @1000bar") / 1000		-- leakage is measured @ 1000 bar
			leakFlow = leakFlow + (unityLeakRate_InjTubings * a_sL.loadingPress)
			context:Log("LT: Injection tubing unity leak rate from system information: {0} \181L/min/bar", unityLeakRate_InjTubings)
			context:Log("Calculated leak flow of LT: Injection tubing from system information: {0} \181L/min", leakFlow)
			context:Log("Calculated load flow with leak included: {0} \181L/min", (a_sL.loadingFlow + leakFlow))
			context:Log("Calculated load volume to be used without a leak: {0} \181L", a_sL.loadingVolume)
			leakVolume = leakVolume + (leakFlow * a_sL.loadingTime/60)
			context:Log("Calculated load volume with leak included: {0} \181L", (a_sL.loadingVolume + leakVolume))
		end
		a_sL.loadingVolume = a_sL.loadingVolume + leakVolume
		a_sL.loadingFlow = a_sL.loadingFlow + leakFlow
	else
		if (installed:Contains("FRT: Route 3 with loop")) then
			local chrom = require "chromatography"
			local unitySystemFlow = installed:GetArgumentValue1("FRT: Route 3 with loop") / 100		-- flow restriction is measured @ 100 bar
			local unityTransferCapFlow = chrom.GetUnityFlow(installed, pressSettings.GradientPumpMaxTargetPressure, false, false, false, true, false, nil)
			local systemUnityFlow = 1/((1/unitySystemFlow) + (1/unityTransferCapFlow))
			context:Log("System unity flow calculated with FRT Route 3 from system information: {0}", systemUnityFlow)
		end
	end
--]]
	if (context:GetArgumentValue("Exclude Equilibration") == true) then		-- Set PIDs only if equilibration is excluded, otherwise it is set in equilibration
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_5_1)
	end

	-- pressure is reduced first if actual pressure > column pressure
	pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, true, a_sL.loadingPress)
	-- switch pump A on because it is switched off in 'setMaxPressureLimitA()'
	pf.Manualmode_Pump_constantPressure(zr.A, a_sL.loadingPress, pump, yield_func)

	while (pump:GetCurrentPressure(zr.A) < (a_sL.loadingPress - 100)) do
		context:Sleep(100)
	end

	-- This is for testing the PBNE-868 bugfix --
	-- To prevent the separation column for a high pressure drop
	-- the trap valve is switched to a 'in between' position to
	-- isolate the column from the injection system
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	local actualValveTPosition = pp.GetTrapValvePosition(execAux)
--	actualValveTPosition = tonumber(string.match(DotNetString.Format("{0}", actualValveTPosition), "%d+"))
	context:Sleep(500)
	context:Log("**** Trap valve position: {0}",actualValveTPosition)
	pr.SetValvePosition(execAux, valveT, actualValveTPosition+30)
	context:Signalize(baltic.ColorsRGB.LightGray, baltic.Naming.ValveTShortGroove, baltic.Naming.ValveTShortGroove)
	context:Sleep(500)
	context:Log("**** Trap valve position: {0}", actualValveTPosition+30)

	pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Load)
	pr.Signalize_Reset(context)
	pr.SignalizeValveIInjectionPath(context, baltic.ColorsRGB.Blue, execAux, baltic.InjectionValve.Load)
	local beforeLoadingStarts = context:GetArgumentValue("Injection Delays", "Before Loading Starts")
	context:Sleep(beforeLoadingStarts)
--	context:Sleep(5000)
	context:Log("**** Trap valve position to: {0}", actualValveTPosition)
	if actualValveTPosition ~= nil then pr.SetValvePosition(execAux, valveT, actualValveTPosition) end
	-- This is for testing the PBNE-868 bugfix --

	context:Sleep(100)
	local N = baltic.Naming
	context:Signalize(baltic.ColorsRGB.Blue, N.PumpARearToValve, N.ValveAGroove, N.ValveAToInjectionValve, N.ValveIGroovesLoad, N.Loop, N.InjectToTrap, N.ValveTShortGroove, N.TransferLine, N.Separator)
	context:Sleep(1000)
	if (context:GetArgumentValue("sample_volume") > 0) then
		execLeft:EmptySyringe()
		--Injector wash
		if context:GetArgumentValue("Flush Injector Port") == true then
			local solvent     = pp.Organic
			local volume      = context:GetArgumentValue("Flush Injector Port", "Volume Organic Sealed")
			local speed       = context:GetArgumentValue("Flush Injector Port", "Flow Rate")
			local liftUpHight = context:GetArgumentValue("Flush Injector Port", "Lift-Up Distance")
			pp.PrintFlushPortParameter(context, solvent, volume, speed, 0)
			pp.FlushPort(solvent, volume, speed, 0, execLeft, execAux)							-- flush port sealed
			volume      = context:GetArgumentValue("Flush Injector Port", "Volume Organic Unsealed")
			pp.PrintFlushPortParameter(context, solvent, volume, speed, liftUpHight)
			pp.FlushPort(solvent, volume, speed, liftUpHight, execLeft, execAux)							-- flush port unsealed
		elseif context:GetArgumentValue("Disable Injector Cleaning") == false then
			context:Log("queue_clean_injector")
			qaw.queue_clean_injector(execLeft, execAux, pp, false, false)			-- with valveI in LOAD position
		end
	end

	context:Log(baltic.devider)
	context:Log("--- Sample Loading on Separation Column:")
	context:Log("--- ovenTemp [C]:                      {0}", ovenTemp)
	context:Log("--- Separator Unity-Flow [ul/bar]:     {0}", a_sL.unityFlow)
	context:Log("--- Separator Volume [ul]:             {0}", context:GetArgumentValue("separator_volume"))
	context:Log("--- Sample Volume [ul]:                {0}", context:GetArgumentValue("sample_volume"))
	context:Log("--- Separator Load Pressure [bar]:     {0}", a_sL.loadingPress)
	context:Log("--- Separator Load Flow [ul/min]:      {0}", a_sL.loadingFlow)
	context:Log("--- Separator Volume Multiplier:       {0}", context:GetArgumentValue("column_load_volumemultiplier"))
	context:Log("--- Separator Load Volume (+2) [ul]:   {0}", a_sL.loadingVolume)
	context:Log("--- Separator Load Time [min]:         {0}", a_sL.loadingTime/60)
	context:Log("--- max. Volume (10*V+2ul/min*t) [ul]: {0}", a_sL.maxLoadingVolume)
	context:Log("--- Separator Load Time Out [min]:     {0}", a_sL.timeOut/60)
	context:Log(baltic.devider)
	p,v,f,t, err, msg = cplae.const_pressure_load_and_equilibration(pump, zr.A, a_sL.loadingPress, a_sL.loadingVolume, a_sL.loadingFlow, yield_func, a_sL.timeOut, false)
	context:Log(baltic.devider)
	context:Log("--- Finished: Sample Loading on Separation Column: (const_pressure_load_and_equilibration)")
	context:Log("--- act. pressure [bar]:                 {0}",p)
	context:Log("--- used volume [uL]:                    {0}",v)
	context:Log("--- average flow (incl. leaks) [uL/min]: {0}",f)
	context:Log("--- act. loading time [min]:             {0}",t/60)
	context:Log(baltic.devider)
	context:LogMeta(context.Name, "Expected separator loaded volume", "\181L/min", "N2", a_sL.loadingVolume)
	context:LogMeta(context.Name, "Separator loaded volume", "\181L", "N2", v)
	context:LogMeta(context.Name, "Expected separator loading flow", "\181L/min", "N2", a_sL.loadingFlow)
	context:LogMeta(context.Name, "Separator loading flow", "\181L/min", "N2", f)
	context:LogMeta(context.Name, "Expected separator loading time", "min", "N2", a_sL.loadingTime/60)
	context:LogMeta(context.Name, "Separator loading time", "min", "N2", t/60)
	if (f<0.5*a_sL.loadingFlow) then
		local msg1 = "loading flow ("..pf.noExp(f,3)..") is too low (expected: "..pf.noExp(a_sL.loadingFlow,3)..")."
		local msg2 = msg1.." Check whether the right oven temperature is set in the method editor or carry out a column diagnosis to check whether the column is blocked."
		if (f<(0.2*a_sL.loadingFlow)) then
			context:Report("Separation column:", Severity.Warn, true, msg2)
		else
			context:Report("Separation column:", Severity.Tip, true, msg2)
		end
	end
	if (f>2*a_sL.loadingFlow) then
		local msg1 = "loading flow ("..pf.noExp(f,3)..") is too high (expected: "..pf.noExp(a_sL.loadingFlow,3)..")."
		local msg2 = msg1.." Check whether the right oven temperature is set in the method editor or carry out a column diagnosis to check whether the column is leaking."
		if (f>(a_sL.loadingFlow+2)) then
			context:Report("Separation column:", Severity.Warn, true, msg2)
		else
			context:Report("Separation column:", Severity.Tip, true, msg2)
		end
	end
	return err, msg
end

-- This function is needed outside to have the equilibration time
---Calculate the equilibration parameters for the separation column
---@param context IProcedureExecutionContext
---@param pumpMaxPressure integer [bar]
---@param systemUnityFlow number
---@param ovenTemp number
---@return SeparatorEquilibratingParameter
function P.sepEquiCalcParam(context, pumpMaxPressure, systemUnityFlow, ovenTemp)
	local baltic = require "baltic"
	local chrom = require "chromatography"
	local visco = chrom.viscosity_mix(ovenTemp, 0)
	---@type SeparatorEquilibratingParameter
	local a_sE = {unityFlow=0, equilibratingPress=0, equilibratingFlow=0, equilibratingVolume=0, equilibratingTime=0, maxEquilibratingVolume=0, timeOut=0}
	a_sE.unityFlow = chrom.column_flow(context:GetArgumentValue("separator"), pumpMaxPressure, 1, visco)
	local unityFlow = 1/(1/systemUnityFlow + 1/a_sE.unityFlow)

	a_sE.equilibratingPress = context:GetArgumentValue("separator_equilibration_pressure")
	a_sE.equilibratingFlow = unityFlow * a_sE.equilibratingPress  -- theoretical flow at analytical column equilibration.
			-- don't add the predefined volume of the tubings and valves (0.4 uL). 
	a_sE.equilibratingVolume = context:GetArgumentValue("separator_volume")*context:GetArgumentValue("separator_equilibration_volumemultiplier")
	a_sE.equilibratingTime = 60*a_sE.equilibratingVolume/a_sE.equilibratingFlow	-- [s]
	a_sE.maxEquilibratingVolume = a_sE.equilibratingVolume*10+a_sE.equilibratingTime/30		--(*2 uL leakage/min)
	a_sE.timeOut = a_sE.equilibratingTime * 10
	if baltic.microElute then a_sE.timeOut = a_sE.timeOut * 10 end			-- time out must be much higher for microElute LC

	return a_sE
end

---Equilibrate the separation column
---If available it is taken into account the "LT: Injection tubing @1000bar" or "LT: Pump A @1000bar" leakage rate
---Else if available it is taken into account the unity flow of "FRT: Route 3 with loop"
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param a_sE SeparatorEquilibratingParameter
---@param ovenTemp number
---@param flushTrapValve boolean
---@param yield_func function
---@return string
---@return string
function P.sepEquilibration(installed, context, a_sE, ovenTemp, flushTrapValve, yield_func)
	---@type Severity
	Severity = luanet.import_type("Bruker.Lc.Business.Severity")
	local baltic = require "baltic"
	local chrom = require "chromatography"
	local cplae = require "const_pressure_load_and_equilibration"
	local parallel = require "parallel"
	local pf = require "pump_functions"
	local pp = require "palplus"
	local pr = require "PreRunFunctions"
	---@type Zirconium
	local zr = require "zirconium"
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local evalFlow = context:GetArgumentValue("uses_trap")
	local err = ""
	local msg = ""
	local p,v,f,t = 1,2,3,4*60
--[[ disabled for plug-in 1.0
	local leakFlow = 0
	local leakVolume = 0

	-- take a leak into account when calculation of the expected equilibration volume
	if (installed:Contains("LT: Pump A @1000bar")) or (installed:Contains("LT: Injection tubing @1000bar")) then
		if (installed:Contains("LT: Pump A @1000bar")) then
			local unityLeakRate_PumpA = installed:GetArgumentValue1("LT: Pump A @1000bar") / 1000		-- leakage is measured @ 1000 bar
			leakFlow = unityLeakRate_PumpA * a_sE.equilibratingPress
			a_sE.equilibratingFlow = a_sE.equilibratingFlow + leakFlow
			context:Log("LT: Pump A / valve A unity leak rate from system information: {0} \181L/min/bar", unityLeakRate_PumpA)
			context:Log("Calculated leak flow of LT pumpA and valve A from system information: {0} \181L/min", leakFlow)
			context:Log("Calculated equilibration flow with leak included: {0} \181L/min", a_sE.equilibratingFlow + leakFlow)
			context:Log("Calculated equilibration volume to be used without a leak: {0} \181L", a_sE.equilibratingVolume)
			leakVolume = leakFlow * a_sE.equilibratingTime/60
			context:Log("Calculated equilibration volume with leak included: {0} \181L", (a_sE.equilibratingVolume + leakVolume))
		end
		if (installed:Contains("LT: Injection tubing @1000bar")) then
			local unityLeakRate_InjTubings = installed:GetArgumentValue1("LT: Injection tubing @1000bar") / 1000		-- leakage is measured @ 1000 bar
			leakFlow = unityLeakRate_InjTubings * a_sE.equilibratingPress
			context:Log("LT: Injection tubing unity leak rate from system information: {0} \181L/min/bar", unityLeakRate_InjTubings)
			context:Log("Calculated leak flow of LT: Injection tubing from system information: {0} \181L/min", leakFlow)
			context:Log("Calculated equilibration flow with leak included: {0} \181L/min", (a_sE.equilibratingFlow + leakFlow))
			context:Log("Calculated equilibration volume to be used without a leak: {0} \181L", a_sE.equilibratingVolume)
			leakVolume = leakVolume + (leakFlow * a_sE.equilibratingTime/60)
			context:Log("Calculated equilibration volume with leak included: {0} \181L", (a_sE.equilibratingVolume + leakVolume))
		end
		a_sE.equilibratingVolume = a_sE.equilibratingVolume + leakVolume
		a_sE.equilibratingFlow = a_sE.equilibratingFlow + leakFlow
	else
		if (installed:Contains("FRT: Route 3 with loop")) then
			local unitySystemFlow = installed:GetArgumentValue1("FRT: Route 3 with loop") / 100		-- flow restriction is measured @ 100 bar
			local unityTransferCapFlow = chrom.GetUnityFlow(installed, pressSettings.GradientPumpMaxTargetPressure, false, false, false, true, false, nil)
			local systemUnityFlow = 1/((1/unitySystemFlow) + (1/unityTransferCapFlow))
			context:Log("System unity flow calculated with FRT Route 3 from system information: {0}", systemUnityFlow)
		end
	end
--]]
	if flushTrapValve then
		-- TrapFlush - ensure no high organic is left in either short or long groove on TrapValve
		local eqSystemPres_10ul = chrom.column_pressure(context:GetArgumentValue("trap"), pressSettings.GradientPumpMaxTargetPressure, chrom.trapMaxFlow, chrom.viscosity_H2O_20C) + chrom.GetEquilibrationSystemPressure(installed, chrom.trapMaxFlow)
		local press = math.min(pf.getColumnMaxPressure(context:GetArgumentValue("trap"), pressSettings.GradientPumpMaxTargetPressure), eqSystemPres_10ul)
		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Trap)				-- flush the trap and valveT grooves
		-- flush loop and trap and grooves with low pressure
		pf.Manualmode_Pump_constantPressure(zr.A, press, pump, yield_func)
		parallel.sleep(yield_func, 15000)
		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.InjectWaste)		-- flush loop only with increased pressure
		local highPressure = math.min(baltic.Settings.GradientPumpDefColumnEquilPressure , press*6)
		-- flush loop with high pressure to remove all organic residues
		pf.Manualmode_Pump_constantPressure(zr.A, highPressure, pump, yield_func)
		parallel.sleep(yield_func, 15000)
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, yield_func)
	end

  -- pressure is reduced first if actual pressure > column pressure
	pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, true, a_sE.equilibratingPress)

	context:Signalize(baltic.ColorsRGB.Normal, baltic.Naming.TrapValveToWaste, baltic.Naming.Trap)
	pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Analytical)

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_5_1)

	context:Log(baltic.devider)
	context:Log("--- Separation Column Equilibration:")
	context:Log("--- ovenTemp [C]:                                 {0}", ovenTemp)
	context:Log("--- Separator Unity-Flow @ ovenTemp [ul/min/bar]: {0}", a_sE.unityFlow)
	context:Log("--- Separator Volume [ul]:                        {0}", context:GetArgumentValue("separator_volume"))
	context:Log("--- Separator Equilibration Volume Multiplier:    {0}", context:GetArgumentValue("separator_equilibration_volumemultiplier"))
	context:Log("--- Separator Equilibration Pressure [bar]:       {0}", a_sE.equilibratingPress)
	context:Log("--- Separator Equilibration Flow [ul/min]:        {0}", a_sE.equilibratingFlow)
	context:Log("--- Separator Equilibration Volume [ul]:          {0}", a_sE.equilibratingVolume)
	context:Log("--- Separator Equilibration Time [min]:           {0}", a_sE.equilibratingTime/60)
	context:Log("--- max. Volume (10*V+2ul/min*t) [ul]:            {0}", a_sE.maxEquilibratingVolume)
	context:Log("--- Separator Equilibration Time Out [min]:       {0}", a_sE.timeOut/60)
	context:Log(baltic.devider)
	p,v,f,t,err, msg = cplae.const_pressure_load_and_equilibration(pump, zr.A, a_sE.equilibratingPress, a_sE.equilibratingVolume, a_sE.equilibratingFlow, yield_func, a_sE.timeOut, evalFlow)
	context:Log("--- act. pressure [bar]:                 {0}",p)
	context:Log("--- used volume [uL]:                    {0}",v)
	context:Log("--- average flow (incl. leaks) [uL/min]: {0}",f)
	context:Log("--- act. equilibration time [min]:       {0}",t/60)
	context:Log(baltic.devider)
	context:LogMeta(context.Name, "Expected separator equilibration volume", "\181L/min", "N2", a_sE.equilibratingVolume)
	context:LogMeta(context.Name, "Separator equilibration volume", "\181L", "N2", v)
	context:LogMeta(context.Name, "Expected separator equilibration flow", "\181L/min", "N2", a_sE.equilibratingFlow)
	context:LogMeta(context.Name, "Separator equilibration flow", "\181L/min", "N2", f)
	context:LogMeta(context.Name, "Expected separator equilibration time", "min", "N2", a_sE.equilibratingTime/60)
	context:LogMeta(context.Name, "Separator equilibration time", "min", "N2", t/60)
	if evalFlow then					-- PBNE-673
		if (f<0.5*a_sE.equilibratingFlow) then
			local msg1 = "equilibration flow ("..pf.noExp(f,3)..") is too low (expected: "..pf.noExp(a_sE.equilibratingFlow,3)..")."
			local msg2 = msg1.." Check whether the right oven temperature is set in the method editor or carry out a column diagnosis to check whether the column is blocked."
			if (f<(0.2*a_sE.equilibratingFlow)) then
				context:Report("Separation column:", Severity.Warn, true, msg2)
			else
				context:Report("Separation column:", Severity.Tip, true, msg2)
			end
		end
		if (f>2*a_sE.equilibratingFlow) then
			local msg1 = "equilibration flow ("..pf.noExp(f,3)..") is too high (expected: "..pf.noExp(a_sE.equilibratingFlow,3)..")."
			local msg2 = msg1.." Check whether the right oven temperature is set in the method editor or carry out a column diagnosis to check whether the column is leaking."
			if (f>(a_sE.equilibratingFlow+2)) then
				context:Report("Separation column:", Severity.Warn, true, msg2)
			else
				context:Report("Separation column:", Severity.Tip, true, msg2)
			end
		end
	end
	return err, msg
end

return P
