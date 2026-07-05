local Date = "2025/07/23"

luanet.load_assembly("Bruker.Lc")

local P = {}

local baltic 	= require "baltic"
local chrom 	= require "chromatography"
local gs 		= require "gradient_segment"
local pf		= require "pump_functions"
local pr 		= require "PreRunFunctions"

---Align solvents and build pressure under correct valve positions prior to gradient execution
---@param context IProcedureExecutionContext
---@param gradient GradientContainer
---@param pump Pump
---@param zr Zirconium
function P.spline_Gradient(context, gradient, pump, zr)
	local function sleep_100()
		context:Sleep(100)
	end

	local n = 0
	local flowA = 0
	local flowB = 0
	for segment in gs.dotnet_each(gradient) do
		if n == 0 then
			flowA = pf.noExp(segment.Flow*(1-segment.Mix*0.01))
			flowB = pf.noExp(segment.Flow*segment.Mix*0.01)
			n = n + 1
			break
		end
	end

	pf.Manualmode_Pump_constantFlow(zr.A, flowA, pump, sleep_100)
	local actPressA = pump:GetCurrentPressure(zr.A)
	local pressB = actPressA + (actPressA * context:GetArgumentValue("Pump B Starting Pressure Offset") * 0.01)
	pf.Manualmode_Pump_constantPressure(zr.B, pressB, pump, sleep_100)
	local actFlow = pump:GetCurrentFlow(zr.A)
	local flowDelta = math.max(flowA * 0.02, 0.005)
	while (math.abs(actFlow - flowA) > flowDelta) do
		sleep_100()
		actFlow = pump:GetCurrentFlow(zr.A)
		actPressA = pump:GetCurrentPressure(zr.A)
		pressB = actPressA + (actPressA * context:GetArgumentValue("Pump B Starting Pressure Offset") * 0.01)
		pf.Manualmode_Pump_constantPressure(zr.B, pressB, pump, sleep_100)
		if n > 601 then break end
		n = n + 1
	end

	local stabilizationTime = context:GetArgumentValue("Stabilization Time") * 1000			-- [ms]
	context:Sleep(stabilizationTime)
	zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee, false)
	pf.Manualmode_Pump_constantFlow(zr.B, flowB, pump, sleep_100)
	pr.Signalize_Reset(context)
	context:Sleep(2000)

	actFlow = pump:GetCurrentFlow(zr.B)
	while (math.abs(flowB-actFlow) > 0.01) do
		sleep_100()
		actFlow = pump:GetCurrentFlow(zr.B)
		if n > 601 then break end
		n = n + 1
	end

	context:Log("--- End GradientSplineFit")
end

---Preparing the gradient start condition
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param pp PalPlus
---@param execAux IPalParticipant
---@param pump Pump
---@param zr Zirconium
---@param gradient GradientContainer
function P.conditioning_Paths(installed, context, pp, execAux, pump, zr, gradient)
	context:Log("--- Starting conditioning paths ---")
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	local N = baltic.Naming

	local function sleep_100()
		context:Sleep(100)
	end

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_10_1_1)
	--pressure reduction, prior to opening TrapValve to waste. 

	local n = 0
	local flow = 0
	local mix = 0
	for segment in gs.dotnet_each(gradient) do
		if n == 0 then
			flow = pf.noExp(segment.Flow)
			mix = segment.Mix/100
			n = n + 1
			break
		end
	end

	local temperature = context:GetArgumentValue("oven_temperature")
	local viscosity = chrom.viscosity_mix(temperature, mix)
	local unityFlow = chrom.GetUnityFlowSystem(installed)
	local targetPressureA = flow / unityFlow
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	local startPressureA = chrom.column_pressure(context:GetArgumentValue("separator"), pressSettings.GradientPumpMaxTargetPressure, flow, viscosity)
	startPressureA = startPressureA + targetPressureA
	local startPressureB = startPressureA + (startPressureA * context:GetArgumentValue("Pump B Starting Pressure Offset")*0.01)

	pr.Signalize_Reset(context)

	if context:GetArgumentValue("Flush Channel B") then
		pf.Manualmode_Pump_constantPressure(zr.A, startPressureA, pump, sleep_100)

		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Waste)
		context:Sleep(250)

		zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee, false)
		context:Sleep(500)
		pf.Manualmode_Pump_constantFlow(zr.B, 1, pump, sleep_100)
		context:Sleep(2000)

		pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
		context:Sleep(250)

		zr.SetValvePosition(context, pump, zr.B, (baltic.PumpValve.MixTee + 30), true)
		context:Sleep(500)
		pf.Manualmode_Pump_constantPressure(zr.B, startPressureB, pump, sleep_100)
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee, false)

		context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.ValveAGroove, N.ValveAToFS, N.FlowA, N.FSAToMixTee, N.MixTeeToTrapValve, N.ValveTShortGroove, N.TrapValveToWaste)
		context:SignalizeText(baltic.ColorsRGB.White, N.FlowA)
		context:Sleep(10000)
	else
		zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee+30,true)
		context:Sleep(250)
		pf.reducePressure(context,pump,zr,zr.A, zr.B, startPressureA-10, startPressureB-15, 60, sleep_100, false)
		context:Signalize(baltic.ColorsRGB.Blue, N.PumpARearToValve, N.ValveAGroove, N.ValveAToInjectionValve, N.ValveIGroovesLoad, N.Loop, N.InjectToTrap, N.ValveTShortGroove, N.TransferLine, N.Separator)
		pf.Manualmode_Pump_constantPressure(zr.A, startPressureA, pump, sleep_100)
		pf.Manualmode_Pump_constantPressure(zr.B, startPressureB, pump, sleep_100)
		local timeOut = 60
		while (math.abs(pump:GetCurrentPressure(zr.A) - startPressureA) > 2) and (math.abs(pump:GetCurrentPressure(zr.B) - startPressureB) > 2) and (timeOut > 0) do
			sleep_100()
			timeOut = timeOut - 0.1
		end

		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee)
		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Analytical)
		pr.Signalize_Reset(context)
		context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.ValveAGroove, N.ValveAToFS, N.FlowA, N.FSAToMixTee, N.MixTeeToTrapValve, N.ValveTLongGroove, N.TransferLine, N.Separator)
		context:SignalizeText(baltic.ColorsRGB.White, N.FlowA)
	end
end

---Establish the gradient start condition
---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
---@param gradient GradientContainer
---@param pp PalPlus
---@param execAux IPalParticipant
---@param pump Pump
---@param zr Zirconium
---@param useTrap boolean
function P.strategyFastSplineFit(installed, context, gradient, pp, execAux, pump, zr, useTrap)
	P.conditioning_Paths(installed, context, pp, execAux, pump, zr, gradient)

	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	pr.Signalize_Reset(context)
	local N = baltic.Naming
	if (useTrap) then 
		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.GradientT)
		context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.ValveAGroove, N.ValveAToFS, N.FlowA, N.FSAToMixTee, N.MixTeeToTrapValve, N.ValveTLongGroove, N.ValveTShortGroove, N.Trap, N.TransferLine, N.Separator)
	else
		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.GradientA)
		context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.ValveAGroove, N.ValveAToFS, N.FlowA, N.FSAToMixTee, N.MixTeeToTrapValve, N.ValveTLongGroove, N.TransferLine, N.Separator)
	end
	context:SignalizeText(baltic.ColorsRGB.White, N.FlowA)
	P.spline_Gradient(context, gradient, pump, zr)
end
return P