local Date = "2025/11/17"


local P = {}

-- This function is needed outside to have the equilibration time
---Calculate the trap loading time
---@param installed IInstalledHardwareContext
---@param pumpMaxPressure integer [bar]
---@param loadPress number [bar]
---@param sampleVolume number [µL]
---@param loadMultiplier number
---@param additionalLoadingVolume number [µL]
---@return number [sec]
function P.trapLoadTime(installed, pumpMaxPressure, loadPress, sampleVolume, loadMultiplier, additionalLoadingVolume)
	local chrom = require "chromatography"
	local unitySystemFlowCalculated = chrom.GetUnityFlow(installed, pumpMaxPressure, true, true, true, false, true, nil)
	local trapFlow = unitySystemFlowCalculated * loadPress
--[[ disabled for plug-in 1.0
	if (installed:Contains("FRT: Route 3 with loop")) then
		local unitySystemFlow = installed:GetArgumentValue1("FRT: Route 3 with loop") / 100
		trapFlow = (trapFlow / unitySystemFlowCalculated) * unitySystemFlow
	end
--]]
	local sampleLoadVolume = sampleVolume * loadMultiplier + 2 + additionalLoadingVolume
	local loadTime = 60*sampleLoadVolume/trapFlow	-- [s]
	return loadTime
end

-- This function is needed outside to have the equilibration time
---Calculate the trap equilibration parameters
---@param installed IInstalledHardwareContext
---@param trap IChromatographyColumnType
---@param pumpMaxPressure integer [bar]
---@param loadPress number
---@param trapVolume number
---@param equilMultiplier number
---@return TrapColumnEquilibratingParameter
function P.trapEquiCalcParam(installed, trap, pumpMaxPressure, loadPress, trapVolume, equilMultiplier)
	local chrom = require "chromatography"

	---@type TrapColumnEquilibratingParameter
	local a_tE = {flowPerBar=0, equilibratingPress=0, trapFlow=0, trapPressure=0, equilibratingFlow=0, equilibratingVolume=0, equilibratingTime=0, maxEquilibratingVolume=0}
	local trapUnityFlow = chrom.column_flow(trap, pumpMaxPressure, 1, chrom.viscosity_H2O_20C)

	a_tE.flowPerBar = trapUnityFlow
	a_tE.trapFlow = chrom.TrapTragetFlow(trap)
	a_tE.trapPressure = a_tE.trapFlow / a_tE.flowPerBar
	a_tE.equilibratingFlow = a_tE.trapFlow
--[[ disabled for plug-in 1.0
	local unitySystemFlowCalculated = chrom.GetUnityFlow(installed, pumpMaxPressure, true, true, true, false, false, nil)
	if (installed:Contains("FRT: Route 3 with loop")) then
		local unitySystemFlow = installed:GetArgumentValue1("FRT: Route 3 with loop") / 100
		a_tE.equilibratingFlow = (a_tE.trapFlow / unitySystemFlowCalculated) * unitySystemFlow
	end
--]]
	a_tE.equilibratingPress = loadPress
	a_tE.equilibratingVolume = trapVolume * equilMultiplier + 5	-- [uL]
	a_tE.equilibratingTime = 60*a_tE.equilibratingVolume/a_tE.equilibratingFlow		-- [s]
	a_tE.maxEquilibratingVolume = a_tE.equilibratingVolume*10+a_tE.equilibratingTime/30		--(*2 uL leakage/min)

	return a_tE
end

---Equilibrate the trap column
---@param context IProcedureExecutionContext
---@param pumpMaxPressure integer [bar]
---@param a_tE TrapColumnEquilibratingParameter
---@param yield_func function
---@return string
---@return string
function P.trapEquilibration(context, pumpMaxPressure, a_tE, yield_func)
	local baltic = require "baltic"
	local cplae = require "const_pressure_load_and_equilibration"
	local pf = require "pump_functions"
	local pp = require "palplus"
	local pr = require "PreRunFunctions"
	---@type Zirconium
	local zr = require "zirconium"

	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	local err = ""
	local msg = ""
	local p,v,f,t

	-- pressure is reduced first if actual pressure > column pressure
	pf.setMaxPressureLimitA(context, pump, pumpMaxPressure, true, false, a_tE.equilibratingPress)
	pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Load)
	pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Trap)
--	execAux:MoveValveDrive(valveT, pp.Quantity(baltic.TrapValve.Trap, "deg"))

	context:Log(baltic.devider)
	context:Log("--- Trap Column Equilibration")
	context:Log("--- Trap Temperature: fixed to 20 C")
	context:Log("--- Trap Unity-Flow  @ 20C [ul/bar]:      {0}", context:GetArgumentValue("trap_unityflow"))
	context:Log("--- Trap Volume [ul]:                     {0}", context:GetArgumentValue("trap_volume"))
	context:Log("--- Trap Equilibration Volume Multiplier: {0}", context:GetArgumentValue("trap_equilibration_volumemultiplier"))
	context:Log("--- Trap Equilibration Pressure [bar]:    {0}", a_tE.equilibratingPress)
	context:Log("--- Trap Equilibration Flow [ul/min]:     {0}", a_tE.equilibratingFlow)
	context:Log("--- Trap Equilibration Volume (+5) [ul]:  {0}", a_tE.equilibratingVolume)
	context:Log("--- Trap Equilibration Time [min]:        {0}", a_tE.equilibratingTime/60)
	context:Log("--- max. Volume (10*V+2ul/min*t) [ul]:    {0}", a_tE.maxEquilibratingVolume)
	context:Log(baltic.devider)
	p,v,f,t,err,msg = cplae.const_pressure_load_and_equilibration(pump, zr.A, a_tE.equilibratingPress, a_tE.equilibratingVolume, a_tE.equilibratingFlow, yield_func, math.max(a_tE.equilibratingTime*10,120), false)
	context:Log(baltic.devider)
	context:Log("--- Finished: Trap Column Equilibration (const_pressure_load_and_equilibration)")
	context:Log("--- act. pressure [bar]:           {0}",p)
	context:Log("--- used volume [uL]:              {0}",v)
	context:Log("--- average flow [uL/min]:         {0}",f)
	context:Log("--- act. equilibration time [min]: {0}",t/60)
	context:Log(baltic.devider)
	context:LogMeta(context.Name, "Expected trap equilibration volume", "\181L/min", "N2", a_tE.equilibratingVolume)
	context:LogMeta(context.Name, "Trap equilibration volume", "\181L", "N2", v)
	context:LogMeta(context.Name, "Expected trap equilibration flow", "\181L/min", "N2", a_tE.equilibratingFlow)
	context:LogMeta(context.Name, "Trap equilibration flow", "\181L/min", "N2", f)
	context:LogMeta(context.Name, "Expected trap equilibration time", "min", "N2", a_tE.equilibratingTime/60)
	context:LogMeta(context.Name, "Trap equilibration time", "min", "N2", t/60)
	if (f<0.5*a_tE.equilibratingFlow) then
		local msg1 = "equilibration flow ("..pf.noExp(f,3)..") is too low (expected: "..pf.noExp(a_tE.equilibratingFlow,3)..")."
		local msg2 = msg1.." Carry out a column diagnosis to check whether the column is blocked."
		---@type Severity
		local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
		if (f<(0.2*a_tE.equilibratingFlow)) then
			context:Report("Trap column:", Severity.Warn, true, msg2)
		else
			context:Report("Trap column:", Severity.Tip, true, msg2)
		end
		context:Report("Trap column:", Severity.Tip, true, msg2)
	end
	if (f>1.5*a_tE.equilibratingFlow) then
		local msg1 = "equilibration flow ("..pf.noExp(f,3)..") is too high (expected: "..pf.noExp(a_tE.equilibratingFlow,3)..")."
		local msg2 = msg1.." Carry out a column diagnosis to check whether the column is leaking."
		---@type Severity
		local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
		if (f>2*a_tE.equilibratingFlow) then
			context:Report("Trap column:", Severity.Warn, true, msg2)
		else
			context:Report("Trap column:", Severity.Tip, true, msg2)
		end
	end
	return err, msg
end

return P
