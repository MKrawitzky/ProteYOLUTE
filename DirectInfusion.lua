-- require('lldebugger').start()

local Date = "2025/11/17"

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type GradientContainer
local GradientContainer = luanet.import_type("Bruker.Lc.Baltic.GradientContainer")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

local baltic = require "baltic"
local chrom = require "chromatography"
local ci = require "column_information"
local ew = require "extended_wash"
local gs = require "gradient_segment"
local pf = require "pump_functions"
local qaw = require "queue_autosampler_wash"

---@param context InitHelper
function Initialize (context)
    context.Name = "Direct infusion"
	context.DisplayName = "Direct infusion"
    context.Description = "Elute without chromatographic separation"
    context.Hidden = true
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Acquisition

	local i = require "PreRunFunctions"
	i.ini(context, false, true, true, 9*60, 0.1, 0.5, nil)
	--   (context, useTrap, useSep, isocratic, analysisTime, preAirVol, postAirVol)

	--> Service parameter
	context:DeclareASParameter("Exclude Injection", false, "", "boolean", true)
	context:DeclareASParameter("Alternative Transport Liquid", false, "", "boolean", true)
	context:DeclareASParameter("Disable Injector Cleaning", false, "", "boolean", true)
	--< Service parameter

	i.iniInjectionDelays(context)
	i.iniExtendedWashParameters(context, false)
	i.iniNeedleWashParameters(context)

	if context.HardwareContext:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then			-- special advanced parameters and dissolve
		context:DeclareASParameter("Injection Path Wash", false, "")	-- clean loop, injection capillary and valves first by filling the loop with solvent from item position 5
		context:DeclareASParameter("Injection Path Wash", "Time", 20, "sec", "integer")
		context:DeclareASHeader("Special Injection Parameters", baltic.ColorsRGB.Blue)
		i.iniSpecialASParameters(context)
		i.iniDissolveSample(context)
		i.iniDerivatizeSample(context)
		i.iniDiluteSeries(context)
	else
		i.iniInjectionParameters(context)
	end

end

--- This function is called when the generate button is pressed in the method editor
---@param experiment ExperimentInfo
---@param installed IInstalledHardwareContext
---@param context AdjustmentContext
function GenerateMethod (experiment, installed, context)
	local gm = require "PreRunFunctions"

	gm.genMethod(experiment, installed, context, false)
	-- ****** set to zero because there is no equilibration nor loading
	context:SetArgumentValue("separator_equil_time", 0)
	context:SetArgumentValue("column_load_time", 0)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local pressure = pressSettings.GradientPumpMaxTargetPressure
    if experiment.Separator.IsMaximumPressure then
		pressure = math.min(pressure, experiment.Separator.MaximumPressure)
	end

	local ovenTempSetPt = experiment.OvenTemperature
	local viscoMix = chrom.viscosity_mix(ovenTempSetPt, 0)
	local gradient_flow = gs.gradient_flow(chrom.column_flow(experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, pressure, viscoMix))
	gradient_flow = pf.getSepColumnFlow(nil, experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, gradient_flow, 0, ovenTempSetPt)
--	if experiment.Separator.IsMaximumFlow then gradient_flow = math.min(gradient_flow, experiment.Separator.MaximumFlow) end

	if not experiment.IsKeepGradient then
		local gradient = GradientContainer()
		gradient:AddSetPoint(GradientContainer.SetPoint(0, gradient_flow, 0))
		gradient:AddSetPoint(GradientContainer.SetPoint(experiment.AnalysisTime.TotalSeconds, gradient_flow, 0), true)
		context:SetArgumentValue("gradient", gradient)
	end
end

---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate (installed, context)
-- This function is called when the method is uploaded (Upload method or Acquisition start)
-- All errors are surpressed at the moment

	local v = require "PreRunFunctions"
	v.val(installed, context, nil)

	v.valInjectionPathWash(context)
	v.valExtendedWashParameters(installed, context, false)

	if (installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition) then			-- special advanced parameters and dissolve
		v.valSpecialASParameters(context)
		v.valDissolveSample(context, baltic.maxItemPosition)
		v.valDerivatizeSample(context, baltic.maxItemPosition)
		v.valDiluteSeries(context, baltic.maxItemPosition)
	else
		v.valInjectionParameters(context)
	end
end

---This function is called whenever a data is changed in the method editor
---@param experiment ExperimentInfo
---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function ValidateMethod (experiment, installed, context)
	local v = require "PreRunFunctions"
	local useTrap = false
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	v.valOvenTemp(experiment, context)
--    v.valColumns(context, useTrap, true, true)
	v.valCompositionZero(context)		-- PBNE-662
	v.valSepPressure(experiment, context, pressSettings.GradientPumpMaxTargetPressure, useTrap)
	v.valFlow(experiment, context, pressSettings.GradientPumpMaxTargetPressure, useTrap, false)

	v.valInjectionPathWash(context)

	if context:GetArgumentValue("MS Calibrant Injection") then
		local t, _, _, _, _ = v.valCalibration(installed, experiment, context)
		context:SetArgumentValue("calibrantTime", t)
	end

	v.valExtendedWashParameters(installed, context, true)

	if (installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition) then			-- special advanced parameters and dissolve
		v.valSpecialASParameters(context)
		v.valDissolveSample(context, baltic.maxItemPosition)
		v.valDerivatizeSample(context, baltic.maxItemPosition)
		v.valDiluteSeries(context, baltic.maxItemPosition)
	else
		v.valInjectionParameters(context)
	end
end

---@param _ IInstalledHardwareContext
---@param context IProcedureExecutionContext
function PreRun (_, context)
	local pr = require "PreRunFunctions"
	pr.run(context)
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)

	require "degas"
	---@type IJournal
	local journal =	context:GetProcedureParticipant(baltic.JournalRole)
	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local is = require "inject_sample"
	---@type Zirconium
	local zr = require "zirconium"
	local pp = require "palplus"
	local pr = require "PreRunFunctions"
	local parallel = require "parallel"
	local tw =	require "twinscape_utilities"

	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	---@type ErrorCode
	local error_code = {err = "", message = ""}

	local maxPress = pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, true, nil)	-- set max separation column pressure +20

	local function sleep_100()
		context:Sleep(100)
	end

	local function sleep_1000()
		context:Sleep(1000)
	end

	local function handle_message()
		local method = "Direct Infusion"
		if ( error_code.err == "warning" ) then
			context:Report(method, Severity.Warn, true, error_code.message)
			context:Abort()
		elseif (error_code.err == "error" ) then
			context:Report(method, Severity.Error, true, error_code.message)
			context:Abort()
		else
			context:Report(method, Severity.Info, true, error_code.message)
		end
	end

	-- log method data to TwinScape before method is run
	tw.LogTwinScapeInfusionData(installed, context, journal)

	---Function to execute a gradient
	---@param dur number
	---@param f1 number
	---@param f2 number
	---@return boolean
	local function runGradient(dur, f1, f2)
		pf.deletePressABandCompositionFile()
		local pressA = 0
		local pressB = 0
		local sumPressA = 0
		local sumPressB = 0
		local composition = 0
		local numOfValues = 0
		local maxMeasuredPress = 0
		local flow = 0
		local actComp = 0
		local mode = "In direct infusion: "
		local code = 0
		local cnt = 0
		if ((dur > 6) and (f1 ~= f2)) then
			local df = ((f2-f1)/dur)
			local flow1 = f1
			while dur > 0 do
				flow1 = flow1+df
				pf.Manualmode_Pump_constantSpeed(zr.A, flow1, pump, sleep_100)
				context:LogMeta(context.Name, "Flow", "\181L/min", "N3", flow1)
--				pump:Manualmode_Pump_constantSpeed(zr.A, flow1)
--				context:Log("--- Gradient: {0} s; {1} -> {2}; setFlow {3}",dur, f1, f2, flow1)
				dur = dur-1
				context:Sleep(1000) 
				if (cnt > 20) then
					code = pf.isPressureInRange(context, pressSettings.GradientPumpCutoffPressure, maxPress, pump, zr, code)
					if (code == 4) then				-- code == 4 == error
						-- Stop the direct infusion
						return false
					end
					cnt = 10
				end
				cnt = cnt + 1
				if cnt >= 10 then
					pressA, pressB, composition = pf.getPressAndComp(pump, zr)
					sumPressA = sumPressA + pressA
					sumPressB = sumPressB + pressB
					pf.storePressABandComposition(pressA, pressB, composition)
					numOfValues = numOfValues + 1
				end
				maxMeasuredPress, flow, actComp = pf.getMaxPressAndComp(maxMeasuredPress, flow, actComp, pump, zr)
					end
		else
			pf.Manualmode_Pump_constantSpeed(zr.A, f1, pump, sleep_100)
			local n = 0
			while (n < dur) do
				context:Sleep(1000)
				if (cnt > 20) then
					code = pf.isPressureInRange(context, pressSettings.GradientPumpCutoffPressure, maxPress, pump, zr, code)
					if (code == 4) then				-- code == 4 == error
						-- Stop the direct infusion
						return false
					end
					cnt = 10
				end
				cnt = cnt + 1
				n = n + 1
				if cnt >= 10 then
					pressA, pressB, composition = pf.getPressAndComp(pump, zr)
					sumPressA = sumPressA + pressA
					sumPressB = sumPressB + pressB
					pf.storePressABandComposition(pressA, pressB, composition)
					numOfValues = numOfValues + 1
				end
			end
		end
		pf.evaluatePressure(context, sumPressA, sumPressB, numOfValues)
		pf.showGradientMassage(context, code, mode)
		return true
	end

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)

	--	Signalize_Reset
--	pr.Signalize_Reset(context)
	context:ShowComposition(false)

	local fR = require "PreRunFunctions"
	fR.iniFlowResistance(context, pump)

	context:Log("Lua date:			   {0}", Date)
	context:Log("No of item positions: {0}", installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions))
	context:Log("--- Experiment:       {0}", context.Description)
    ci.logColumnInformation(context, nil, context:GetArgumentValue("separator"))
    context:Log("'{0}' is '{1}'", valveI.Name, pp.Capabilities.ILcInjectorValve)

    context:Log("'{0}' is '{1}'", valveT.Name, pp.Capabilities.ISelectorValve)

	if not chrom.IsColumnParameterSet(context, context:GetArgumentValue("separator")) then
		context:Abort()
	end

	context:Log("--- Seperator equil time:  {0}", pf.noExp(context:GetArgumentValue("separator_equil_time"),2))
	context:Log("--- Column loading time:   {0}", pf.noExp(context:GetArgumentValue("column_load_time"),2))

	local settings = pump:GetSettings()
	zr.logInstrSettings(context, settings, "Direct Infusion")

	-- check if column oven is intended to use and connected and temperature is set
	-- Direct Infusion can run with or without a column therefore the IsOvenAndTemperatureOK() function must be called
	if not pr.IsOvenAndTemperatureOK(context, pump) then
		context:Report("Oven", Severity.Error, true, "Missing column oven. Temperature is set but oven is not connected.")
	end

   zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

	-- init zirconium channels, abort if unsuccessful
	if not zr.InitChannel(context, pump, zr.A) then
			context:Report(baltic.Naming.PumpA, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode.

Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
			context:Abort()
	end
	if not zr.InitChannel(context, pump, zr.B) then
			context:Report(baltic.Naming.PumpB, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode.

Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
			context:Abort()
	end

	-- Main function starts here...
	pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_100, false)
	local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_100)
	-- bail if a pump failed degassing..
	if (not (a and b)) then
		context:LogMeta(context.Name, "Degassing Failed", "", "N", "True")
		pr.decompressSystem(context)
		context:Abort()
	end

	if context.HardwareContext:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then			-- special advanced parameters and dissolve
		if context:GetArgumentValue("Injection Path Wash") == true then
			pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Load)
		else
			pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Analytical)
		end
	else
		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Analytical)
	end
	zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject)
	-- set valve to compress because if the valve is in a "block" position the segnaling will not work
	zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Compress)
--	context:Signalize(baltic.ColorsRGB.Blue, baltic.Naming.ValveTShortGroove, baltic.Naming.TransferLine, baltic.Naming.Separator)

	-- local basePressure = 40
	-- local flowPerBar = context:GetArgumentValue("separator_unityflow")
	context:Log(baltic.devider)
	context:Log("--- Building pressure")
													-- loading capillary, loop, injection capillary, transfer capillary
	local systemUnityFlow = chrom.GetUnityFlow(installed, pressSettings.GradientPumpMaxTargetPressure, true, true, true, true, false, nil)
	local unityFlowSum = 1/(1/systemUnityFlow + 1/context:GetArgumentValue("separator_unityflow"))
	context:Log("--- Column Unity Flow:         {0} uL/(bar*min)",context:GetArgumentValue("separator_unityflow") )
	context:Log("--- Total Unity Flow:          {0} uL/(bar*min)",unityFlowSum )

	---@type GradientContainer
	local gradient = context:GetArgumentValue("gradient")
	local segmentFlow = gradient:GetSetPoint(0).Flow

	local targetA =  segmentFlow/unityFlowSum
	if targetA > (maxPress*0.95) then	targetA = (maxPress*0.95) end
    context:Log("--- Target pressure:           {0} bar",targetA )

	---Function runs in a parallel thread
	---@param yield_func function
	local function injectSample(yield_func)
		if context:GetArgumentValue("Exclude Injection") == true then
			context:Log("Sample injection is excluded")
		else
			local doInjection = true
			local sampleVolume, preSampleVolume = 0,0

			if installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then
				if context:GetArgumentValue("Dissolve Sample") then
					local psp = require "PreSamplePrep"
					local WashWastePenetrationDepth	= pp.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
					pp.EmptySyringe_And_LeaveObject(context, execLeft, execAux, WashWastePenetrationDepth, installed.SyringeZeroPosition)
					sampleVolume, preSampleVolume = psp.dissolveAndMixSample(context, yield_func)
					doInjection = false
					context:LogMeta(context.Name, "Dissolve sample volume", "\181L", "N1", sampleVolume)
				else
					if context:GetArgumentValue("Derivatize Sample") == true then
						local psp = require "PreSamplePrep"
						context:Log("--- Start derivatization of sample")
						context:Sleep(2000)
						sampleVolume, preSampleVolume = psp.derivatize_and_inject_Sample(context, yield_func)
						context:Sleep(2000)
						context:LogMeta(context.Name, "Derivatize sample volume", "\181L", "N1", sampleVolume)
						context:Log("--- Finished derivatization of sample")
						doInjection = false
					else
						if context:GetArgumentValue("Dilution Series") == true then
							local psp = require "PreSamplePrep"
							context:Log("--- Start dilution series")
							context:Sleep(2000)
							sampleVolume, preSampleVolume = psp.dilute_and_inject_Sample(context, yield_func)
							context:Sleep(2000)
							context:LogMeta(context.Name, "Dilution sample volume", "\181L", "N1", sampleVolume)
							context:Log("--- Finished dilution series")
							doInjection = false
						end
					end
				end
			end
			if doInjection == true then
				sampleVolume, preSampleVolume = is.injectSample(installed, context, true, yield_func)
			end
			if context.HardwareContext:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then			-- special advanced parameters and dissolve
				if context:GetArgumentValue("Injection Path Wash") == true then 
					pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Analytical) 
				end
			end
			is.injectIntoLoop(context, pp, execAux, execLeft, sampleVolume, preSampleVolume)
			-- wait for queued pickup and inject to complete
			while not execLeft.IsIdle do
				parallel.sleep(yield_func, 1000)
			end
			context:LogMeta(context.Name, "Injected sample volume", "\181L", "N1", sampleVolume)
			context:Log("Injection finished")
			status:RemoveStatus(baltic.Status.Inject)
		end
	end

	---Function to establish the desired flow
	---@param pressure number
	---@param yield_func function
	local function establishFlow(pressure, yield_func)
		if installed:GetArgumentValue1("NoOfItemPositions") >= baltic.maxItemPosition then
			context:LogMeta(context.Name, "Extra injection path washed", "",  "N0", context:GetArgumentValue("Injection Path Wash"))
			if context:GetArgumentValue("Injection Path Wash") then
				local injectionPathCleaningVolume = pump:GetPistonPosition(zr.A)
				ew.injection_path_wash(installed, context, execLeft, execAux, chrom, pp, pr, pump, context:GetArgumentValue("Injection Path Wash", "Time"))
				injectionPathCleaningVolume = pump:GetPistonPosition(zr.A) - injectionPathCleaningVolume
				context:Log("Injection path wash volume: {0} \181L", injectionPathCleaningVolume)
			end
		end
		pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Analytical)
		pf.Manualmode_Pump_constantPressure(zr.A, pressure, pump, yield_func)
		local timeOut = pf.now() + 30		-- 30 seconds timeout
		local dTLimit = math.min(pressure*0.1, 10)
		context:Log("Waiting for pressure established")
		while (math.abs(pump:GetCurrentPressure(zr.A) - pressure) > dTLimit) and (pf.now() < timeOut) do
			parallel.sleep(yield_func, 1000)
		end
		context:Log("Flow established")
		parallel.sleep(yield_func, 5*1000)
		pf.Manualmode_Pump_constantSpeed(zr.A, segmentFlow, pump, yield_func)
		parallel.sleep(yield_func, 20*1000)
	end

	local p_flow = { establishFlow, targetA, parallel.yield }
	local p_injection = { injectSample, parallel.yield }
	parallel.run(sleep_1000, p_flow, p_injection)
	context:SetSignal(baltic.LcElutionReady)
	context:WaitForSignal(baltic.MsAcquisitionStart)
	context:Sleep(1000)
	pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)
	if (context:GetArgumentValue("sample_volume") > 0) and context:GetArgumentValue("Disable Injector Cleaning") == false then
		execLeft:EmptySyringe()
		-- clean injector and syringe
		qaw.queue_clean_injector(execLeft, execAux, pp, false, false)
		execLeft:Wait(pp.Quantity(1000, "ms"))
	end

	-- Gradient has started
	pr.SetSignal(1, execAux) -- set SWOut1 signal for 1 second

	---------------------------------------------------------------
	status:SetStatus("elution")
	---------------------------------------------------------------
	context:Log(baltic.devider)
	context:Log("--- Executing gradient table")

	local lastTime = 0
	local flow1 = segmentFlow
	local code = 0
	for segment in gs.dotnet_each(gradient) do
		context:Log("--- Segment:   {0} s  {1} uL/min, duration: {2}",segment.Time, segment.Flow, (segment.Time-lastTime))
		if not runGradient((segment.Time-lastTime), flow1, segment.Flow) then
			context:Report("Direct infusion:", Severity.Error, true, "Pressure exceeded the maximum value. Check column for blockage.")
			context:LogMeta(context.Name, "Direct Infusion: Pressure Higher Maximum", "bar", "N0", math.max(pump:GetCurrentPressure(zr.A), pump:GetCurrentPressure(zr.B)))
			code = 4
			zr.abortGradient(pump)
			pf.reducePressure(context, pump, zr, zr.A, nil, 50, 50, 20, sleep_100, baltic.smooth)
			break
		end
		lastTime = segment.Time
		flow1 = segment.Flow
	end

	context:Log(baltic.devider)
	context:SetSignal(baltic.LcElutionDone)
	status:RemoveStatus("elution")

	-- make sure that syringe is empty
	execLeft:EmptySyringe(pp.Quantity("5 uL/s"))

	-- wait for queued autosampler wash to finish
	while not execLeft.IsIdle do
			sleep_100()
	end	
    if context:GetArgumentValue("MS Calibrant Injection") and (code == 0 or code == 2) then
		----------------------------------------------------
		-- Calibrant Injection
		----------------------------------------------------
		local ic = require "InjectCalibrant"
		---@type CalibrantInjectionParameter
		local qpcaui_params = {
			sample_aspirate_speed = 	pp.Quantity(context:GetArgumentValue("sample_aspirate_speed"), "uL/s"),
			sample_postaspirate_delay = pp.Quantity(context:GetArgumentValue("sample_postaspirate_delay"), "ms"),
			presample_air_volume = 		pp.Quantity(context:GetArgumentValue("presample_air_volume"), "uL"),
			calibrant_volume = 			flow1, -- this is for 1 minute calibrant signal	-- context:GetArgumentValue("calibrant_volume"),						-- do not pp.Quantity due to calculations with this value
			postsample_air_volume = 	pp.Quantity(context:GetArgumentValue("postsample_air_volume"), "uL"),
			sample_inject_speed = 		pp.Quantity(context:GetArgumentValue("sample_inject_speed"), "uL/s"),
			flowPerBar = 				context:GetArgumentValue("separator_unityflow"),
			systemVolume =				context:GetArgumentValue("separator_volume") + baltic.SystemVolume  -- column volume + capillary volume
		}

		ic.injectCalibrant(installed, context, qpcaui_params, flow1, sleep_100)
	end

	status:SetStatus("Cleaning autosampler")
	if context:GetArgumentValue("Extended Wash") then
		context:Log(baltic.devider)
		context:Log("--- Extended Wash:")
		-- usetrap=false
		error_code = ew.extended_wash(installed, context, execLeft, execAux, pp, pump, false, false, context:GetArgumentValue("Extended Wash", "Num Of Cycles"), context:GetArgumentValue("Extended Wash", "Volume Organic Per Cycle"), context:GetArgumentValue("Extended Wash", "Volume Aqueous Per Cycle"), context:GetArgumentValue("Extended Wash", "Flow Organic"), context:GetArgumentValue("Extended Wash", "Flow Aqueous"), sleep_100)
		if ( error_code.err ~= "" ) then
			error_code.message = "Autosampler clean column - "..error_code.message
--			handle_message()		-- don"t Abort() here, handle_message later
		end
		context:Log(baltic.devider)
	elseif context:GetArgumentValue("Disable Injector Cleaning") == false then
        if (context:GetArgumentValue("sample_volume") > 0) then
			context:Log("queue_clean_injector_loop")
			qaw.queue_clean_injector_loop(execLeft, execAux, pp, false, true) -- with valveI in INJECT position
		end
    end

	-- is done in 'Decompress.lua'
	-- pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 20, sleep_100, baltic.smooth)

	while not execLeft.IsIdle do
		sleep_100()
	end

    pf.reducePressure(context, pump, zr, zr.A, nil, 50, 50, 20, sleep_100, baltic.smooth)
	status:RemoveStatus("cleaning autosampler")
	-- stop pumps before ending
	pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
	context:Log("--- Set pump speed to zero")

  -- wait for pump to stop completely
	pf.isPumpIdle(pump, sleep_100)

    pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)
--	execLeft:MoveValveDrive(valveI, pp.Quantity(baltic.InjectionValve.Load, "deg"))   -- switch valve I to load to be sure that air doesnt enter loop
    context:Sleep(1000)
	execLeft:MoveToHome()
  -- waiting for autosampler stopps completely
	while not execLeft.IsIdle do
			sleep_100()
	end	

	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)

	dictator:Dispose()

	if ( error_code.err ~= "" ) then
		handle_message()
	end
end
