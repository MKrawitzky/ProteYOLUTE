-------------------------------------------------------------------------------
-- ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
-- Licensed under proprietary terms. See LICENSE file for details.
-- https://github.com/MKrawitzky/ProteYOLUTE
-------------------------------------------------------------------------------

local Date = "2025/09/08"

luanet.load_assembly("Bruker.Lc")

---@type GradientContainer
local GradientContainer = luanet.import_type("Bruker.Lc.Baltic.GradientContainer")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")

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
---@param pumpMaxPressure integer [bar]
---@param pumpCutoffPressure integer [bar]
---@param flowA number|nil
---@param flowB number|nil
---@param splineTime number|nil
---@param setNewPID boolean
---@param useTrap boolean
function P.spline_Gradient(context, gradient, pump, zr, pumpMaxPressure, pumpCutoffPressure, flowA, flowB, splineTime, setNewPID, useTrap)
	local ovenTemp = math.max(pump:GetSetExternalTemperature(), 20)
	local viscosity = chrom.viscosity_mix(ovenTemp, 0)
	local trap = nil
	local useFastParameter = false

	if setNewPID == nil then setNewPID = true end
	if useTrap == true then
		---@type IChromatographyColumnType
		trap = context:GetArgumentValue("trap")
	end

	---Calculate parameters
	---@param p1 table
	---@param p3 table
	---@param p4 table
	---@return number
	---@return number
	---@return number
	---@return number
	local function calcPolyParameter(p1,p3,p4)
		local m1 = (p4.Flow-p3.Flow)/(p4.Time-p3.Time)
		local b1 = p3.Flow-p3.Time*m1

		local p0 = {Time = p1.Time-1, Flow = p1.Flow}
		local p2 = {Time = p3.Time-1, Flow = (m1*(p3.Time-1)+b1)}

		local d=p1.Flow															-- (= f2)
		local f1 = {a=p0.Time^3, b=p0.Time^2, c=p0.Time, y=p1.Flow-d}			-- f1-d
		local f3 = {a=p2.Time^3, b=p2.Time^2, c=p2.Time, y=p2.Flow-d }			-- f3-d
		local f4 = {a=p3.Time^3, b=p3.Time^2, c=p3.Time, y=p3.Flow-d }			-- f4-d

		local f3_1 = {a=f3.a/f3.c, b=f3.b/f3.c, c=1, y=f3.y/f3.c}				-- f3/c
		local f4_1 = {a=f4.a/f4.c, b=f4.b/f4.c, c=1, y=f4.y/f4.c}				-- f4/c
		
		local f1_2 = {a=f1.a+f4_1.a, b=f1.b+f4_1.b, y=f1.y+f4_1.y}				-- f1+f4_1
		local f3_2 = {a=f3_1.a-f4_1.a, b=f3_1.b-f4_1.b, y=f3_1.y-f4_1.y}		-- f3_1-f4_1
		
		local f1_3 = {a=f1_2.a/f1_2.b, b=1, y=f1_2.y/f1_2.b}					-- f1_2/b
		local f3_3 = {a=f3_2.a/f3_2.b, b=1, y=f3_2.y/f3_2.b}					-- f3_2/b
		
		local fa = {a=f1_3.a-f3_3.a, y=f1_3.y-f3_3.y}							-- f1_3-f3_3
		local a = fa.y/fa.a
		
		local f1_2a = {a=f1_2.a*a, b=f1_2.b, y=f1_2.y}							-- a in f1_2
		
		local fb = {b=f1_2a.b, y=f1_2a.y-f1_2a.a}								-- f1_2-a
		local b = fb.y/fb.b
		
		local f1_1c = {a=f1.a*a, b=f1.b*b, c=f1.c, y=f1.y}						-- a,b in f1
		local c = (f1_1c.y-f1_1c.a-f1_1c.b)/f1_1c.c

		context:Log("--- y = {0}x + {1}", m1, b1)
		context:Log("--- p0.Time {0}, p0.Flow {1}", p0.Time, p0.Flow)
		context:Log("--- p1.Time {0}, p1.Flow {1}", p1.Time, p1.Flow)
		context:Log("--- p2.Time {0}, p2.Flow {1}", p2.Time, p2.Flow)
		context:Log("--- p3.Time {0}, p3.Flow {1}, p3.Mix {2}", p3.Time, p3.Flow, p3.Mix)
		context:Log("--- p4.Time {0}, p4.Flow {1}, p4.Mix {2}", p4.Time, p4.Flow, p4.Mix)
--[[
		context:Log("--- f1.a {0}, f1.b {1}, f1.c {2}, f1.y {3}", f1.a, f1.b, f1.c, f1.y)
		context:Log("--- f3.a {0}, f3.b {1}, f3.c {2}, f3.y {3}", f3.a, f3.b, f3.c, f3.y)
		context:Log("--- f4.a {0}, f4.b {1}, f4.c {2}, f4.y {3}", f4.a, f4.b, f4.c, f4.y)
		context:Log("--- f3_1.a {0}, f3_1.b {1}, f3_1.c {2}, f3_1.y {3}", f3_1.a, f3_1.b, f3_1.c, f3_1.y)
		context:Log("--- f4_1.a {0}, f4_1.b {1}, f4_1.c {2}, f4_1.y {3}", f4_1.a, f4_1.b, f4_1.c, f4_1.y)
		context:Log("--- f1_2.a {0}, f1_2.b {1}, f1_2.y {2}", f1_2.a, f1_2.b, f1_2.y)
		context:Log("--- f3_2.a {0}, f3_2.b {1}, f3_2.y {2}", f3_2.a, f3_2.b, f3_2.y)
		context:Log("--- f1_3.a {0}, f1_3.b {1}, f1_3.y {2}", f1_3.a, f1_3.b, f1_3.y)
		context:Log("--- f3_3.a {0}, f3_3.b {1}, f3_3.y {2}", f3_3.a, f3_3.b, f3_3.y)
		context:Log("--- fa.a {0}, fa.b {1}, fa.y {2}", fa.a, fa.b, fa.y)
		context:Log("--- f1_2.a {0}, f1_2.b {1}, f1_2.y {2}", f1_2.a, f1_2.b, f1_2.y)
		context:Log("--- fb.b {0}, fb.y {1}", fb.b, fb.y)
		context:Log("--- f1_1c.a {0}, f1_1c.b {1}, f1_1c.c {2}, f1_1c.y {3}", f1_1c.a, f1_1c.b, f1_1c.c, f1_1c.y)
		context:Log("--- f(x) = {0}*x^3 + {1}*x^2 + {2}*x + {3}", a, b, c, d)
--]]
		context:Log("--- Strategy function end")
		return a,b,c,d
	end
	
	---Calculate the flow depending on the given parameters
	---@param s number
	---@param a number
	---@param b number
	---@param c number
	---@param d number
	---@return number
	local function getFlow(s,a,b,c,d)
		local y = a*s^3+b*s^2+c*s+d
		if y<0 then y=0 end
--		context:Log("getFlow: {0}", y)
		return y
	end

	---Calculate the flow and composition depending on flow A and B
	---@param fA number
	---@param fB number
	---@return number
	---@return number
	local function getFlowMix(fA, fB)
		local m = 0
		if (fA+fB) > 0 then
			m = 100/(fA+fB)*fB
		end
		local flow = math.max(fA,0)+math.max(fB,0)
		return flow, m
	end
	
	local flowB_StartTime = 0	--[s]
	local pA1 = {Time=0, Flow=0.01}
	local pA3 = {Time=0, Flow=0, Mix=0}
	local pA4 = {Time=0, Flow=0, Mix=0}
	local pB1 = {Time=flowB_StartTime, Flow=0}
	local pB3 = {Time=0, Flow=0, Mix=0}
	local pB4 = {Time=0, Flow=0, Mix=0}

	-- return the splineFitTime depending on the unity flow of the separation column

	context:Log("--- Oven temperatur: {0}", ovenTemp)
	context:Log("--- Separator: {0}", context:GetArgumentValue("separator"))

	local function calcSetPID(flow,mixB)
--		local chrom = require "chromatography"
		if setNewPID then
			local visco = chrom.viscosity_mix(ovenTemp, mixB)
			local colPress = chrom.column_pressure(context:GetArgumentValue("separator"), pumpMaxPressure, flow, visco)
			local press = math.min((colPress + 100*flow), 1000)
			local i = math.min(math.max(-0.7*(press)+600,200), 500)		-- calculate I-part due to pressure
			context:Log("--- Press: {0}; colPress: {1}, visco: {2}, flow > {3}", press, colPress, visco, flow)
			context:Log("--- Flow: {0}; Mix: {1}, calcI: {2}", flow, mixB, i)
			context:Log("--- Ki calc: {0}", (-0.7*(press)+600))
			-- values ok for 15cm, 300nL, 400bar (Kp=200, Ki=600, Kd=1000)
			-- values ok for 15cm, 600nL, 800bar (Kp=200, Ki=300, Kd=1000)
			-- for new fw we are not finetuning I parameter
			--zr.ChangePressurePID(context, pump, baltic.PPID.splineFit2)
			--zr.ChangePressurePID(context, pump, {I=i})
		end
	end

	-- use the actual flow as starting flow
	if flowA ~= nil then
		pA1.Flow = flowA
	end
	if flowB ~= nil then
		pB1.Flow = flowB
	end

	local uF = chrom.column_flow(context:GetArgumentValue("separator"), pumpMaxPressure, 1, viscosity)
	local splineFitTime = 120		--[s]
	if not baltic.microElute then
		splineFitTime = 0.025/uF+55
	end
	context:Log("--- Spline fitting time: {0} s", splineFitTime)
	local n = 0
	for segment in gs.dotnet_each(gradient) do
		if n == 0 then
			pA3.Flow = segment.Flow*(1-segment.Mix*0.01)		-- flow a
			pA3.Mix  = segment.Mix
			pB3.Flow = segment.Flow*segment.Mix*0.01			-- flow b
			pB3.Mix  = segment.Mix
			if useFastParameter then							-- adapt spline fitting time according to the actual flow
				local dFlow = math.max((pA3.Flow-pA1.Flow), (pB3.Flow-pB1.Flow))
				if not baltic.microElute then					-- nanoElute
					splineFitTime = splineFitTime * math.abs(dFlow+0.25)
					context:Log("--- dFlow: {0}", dFlow)
				else
					splineFitTime = math.min(math.max(35,dFlow*12),120)
				end
			else
				local flow = math.max(pA3.Flow, pB3.Flow)+0.25
				if not baltic.microElute then					-- nanoElute
					splineFitTime = splineFitTime * flow
				else
					splineFitTime = math.min(math.max(35,flow*12),120)
				end
			end
			if splineTime ~= nil then splineFitTime = splineTime end
			context:Log("--- Spline fitting time adapted: {0} s", splineFitTime)
			pA3.Time = segment.Time+splineFitTime
			pB3.Time = segment.Time+splineFitTime
		end
		if n == 1 then
			pA4.Time = segment.Time+splineFitTime
			pA4.Flow = segment.Flow*(1-segment.Mix*0.01)		-- flow a
			pA4.Mix  = segment.Mix
			pB4.Time = segment.Time+splineFitTime
			pB4.Flow = segment.Flow*segment.Mix*0.01			-- flow b
			pB4.Mix  = segment.Mix
			break
		end
		n = n+1
	end

	calcSetPID((pA3.Flow+pB3.Flow), (pA3.Mix*0.01))

	local aA,bA,cA,dA = calcPolyParameter(pA1,pA3,pA4)
	local aB,bB,cB,dB = calcPolyParameter(pB1,pB3,pB4)
    local p = math.min(pf.getMaxColumnPressure(trap, context:GetArgumentValue("separator"), pumpMaxPressure), pumpCutoffPressure)

	local prepare_gradient_method = zr.CreateGradient(0, p, true, ovenTemp)
	context:Log("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")
	local f,m = getFlowMix(pA1.Flow, pB1.Flow)
	local i = 1
	local fineStepTime = splineFitTime/4
	local step = 1
	zr.AddGradientSegment(prepare_gradient_method, 0, pf.noExp(f,5), pf.noExp(m,5))
	context:Log("XXXXX Time 0, Flow {0}, Mix {1}", pf.noExp(f,5), pf.noExp(m,5))
	while i < splineFitTime do
		if splineFitTime <= 60 then
			step = 1
		else
			if (i >= fineStepTime*3) or (i <= fineStepTime) then
				step = math.max(math.floor(fineStepTime/10),1)
			else
				step = math.max(math.floor(fineStepTime/2),1)
			end
		end
		local flow_A = getFlow(i,aA,bA,cA,dA)
		local flow_B = getFlow(i,aB,bB,cB,dB)
		f,m = getFlowMix(flow_A, flow_B)
		zr.AddGradientSegment(prepare_gradient_method, i, pf.noExp(f,5), pf.noExp(m,5))
		context:Log("XXXXX Time {0}, Flow {1}, Mix {2}", i, pf.noExp(f,5), pf.noExp(m,5))
		i=i+step
	end

	local error = zr.LoadGradient(context, pump, prepare_gradient_method)
	if error then
		context:Report("Gradient", Severity.Error, true, error)
		context:Abort()
	end
	context:Log("--- Set gradient start")
	-- wait for gradient to start
	-- restart gradient every second if not already started
	-- abort gradient if not started within 10 seconds
	zr.StartGradient(context, pump)
	context:Log("--- Start GradientSplineFit")
	local mode = "In spline fit: "
	local code = 0
	local showMsg = true
	while (zr.IsGradientRunning(pump)) do
		context:Sleep(1000)
		code = pf.isPressureInRange(context, pumpCutoffPressure, p, pump, zr, code)
		if (code == 4) then				-- code == 4 == error
			-- The gradient is stopped but the runtime in HyStar continues
			-- After the runtime is left the state stays at Acquisition and no new run is started
			local msg = "Pressure exceeded "..pumpCutoffPressure.." bar or the maximum pressure set in the trap or separation column editor. Check column for blockage."
			context:Report(mode, Severity.Error, true, msg)
			zr.abortGradient(pump)
			pr.decompressSystem(context)
			context:Abort()
		end
		if showMsg then
			context:Log("--- GradientSplineFit is running")
			showMsg = false
		end
	end
	context:Log("--- GradientSplineFit has finished")
	if (code < 3) then		-- show error, warning or nothing depending on code
		pf.showGradientMassage(context, code, mode)
	end

	context:Log("--- End GradientSplineFit")
end

---Preparing the gradient start condition
---@param context IProcedureExecutionContext
---@param pp PalPlus
---@param execAux IPalParticipant
---@param pump Pump
---@param zr Zirconium
---@param pumpMaxPressure integer [bar]
---@param pumpCutoffPressure integer [bar]
---@param useTrap boolean
function P.conditioning_Paths(context, pp, execAux, pump, zr, pumpMaxPressure, pumpCutoffPressure, useTrap)
	context:Log("--- Starting conditioning paths ---")	
	local function sleep_100()
		context:Sleep(100)
	end

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)
	--pressure reduction, prior to opening TrapValve to waste. 
	local startPressure = 0
	local offsetA = 15 
--	local offsetB = 5  

	pr.Signalize_Reset(context)
	pf.reducePressure(context, pump, zr, zr.A, nil, startPressure, startPressure, 300, sleep_100, baltic.smooth)

	--wait for A, max 5 min
	for n=1,300 do
		if (pump:GetCurrentPressure(zr.A) < startPressure+offsetA) then
			break
		end
		context:Sleep(1000)
	end

	zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee,false)
	zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.MixTee,false)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Waste)
	pr.Signalize_Reset(context)
	context:Sleep(1000)

	local f = 1.0

	pf.Manualmode_Pump_constantFlow_binary(context, f, f, pump, zr, sleep_100)

	local splineTimeB = 20
	local splineTimeA = 60

	local gradientB = GradientContainer()
	gradientB:AddSetPoint(GradientContainer.SetPoint(0, f, 0))
	gradientB:AddSetPoint(GradientContainer.SetPoint(30, f, 0), true)

	local gradientA = GradientContainer()
	gradientA:AddSetPoint(GradientContainer.SetPoint(0, 0.01, 0))
	gradientA:AddSetPoint(GradientContainer.SetPoint(30, 0.01, 0), true)

	context:Log("\n")	
	context:Log("--- Starting spline flow reduction B/A ---")	
	P.spline_Gradient(context, gradientB, pump, zr, pumpMaxPressure, pumpCutoffPressure, f, f, splineTimeB, false, useTrap)
	context:Log("--- Finished spline flow reduction B ---")	
	context:Sleep(1000)
	P.spline_Gradient(context, gradientA, pump, zr, pumpMaxPressure, pumpCutoffPressure, f, 0, splineTimeA, false, useTrap)
	context:Log("--- Finished spline flow reduction A ---\n")	
	context:Log("--- Finished conditioning paths ---")	
end

---Establish the gradient start condition
---@param context IProcedureExecutionContext
---@param gradient GradientContainer
---@param pp PalPlus
---@param execAux IPalParticipant
---@param pump Pump
---@param zr Zirconium
---@param pumpMaxPressure integer [bar]
---@param pumpCutoffPressure integer [bar]
---@param useTrap boolean
function P.strategySplineFit(context, gradient, pp, execAux, pump, zr, pumpMaxPressure, pumpCutoffPressure, useTrap)
	P.conditioning_Paths(context, pp, execAux, pump, zr, pumpMaxPressure, pumpCutoffPressure, useTrap)
	context:Sleep(5000)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	if (useTrap) then 
		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.GradientT)
	else
		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.GradientA)
	end
	P.spline_Gradient(context, gradient, pump, zr, pumpMaxPressure, pumpCutoffPressure, nil, nil, nil, true, useTrap)
end
return P
