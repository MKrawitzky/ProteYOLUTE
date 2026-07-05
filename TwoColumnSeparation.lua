-- require('lldebugger').start()

local Date = "2025/11/17"

luanet.load_assembly("Bruker.Lc")

local DotNetString = luanet.import_type("System.String")
---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type JournalEntry
local JournalEntry = luanet.import_type("Bruker.Lc.JournalEntry")
---@type LogTo
local LogTo = luanet.import_type("Bruker.Lc.Business.LogTo")
---@type GradientContainer
local GradientContainer = luanet.import_type("Bruker.Lc.Baltic.GradientContainer")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

local baltic = 	require "baltic"

---@param context InitHelper
function Initialize (context)
	context.Name = "2C separation"
	context.DisplayName = "Two column separation"
	context.IsLegacy = false
	context.Description = "Two column liquid chromatographic separation"
	context.Hidden = true
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Acquisition

	local i = require "PreRunFunctions"
	i.ini(context, true, true, false, 30*60, 0.5, 1.0)
	--   (context, useTrap, useSep, isocratic, analysisTime, preAirVol, postAirVol)

	--> This has to be password protected
	context:DeclareASParameter("Exclude Injection", false, "", "boolean", true)
	context:DeclareASParameter("Exclude Equilibration", false, "", "boolean", true)
	context:DeclareASParameter("Exclude Sample Loading", false, "", "boolean", true)
	context:DeclareASParameter("Alternative Transport Liquid", false, "", "boolean", true)
	context:DeclareASParameter("Disable Injector Cleaning", false, "", "boolean", true)
	--< This has to be password protected

	i.iniInjectionDelays(context)
	i.iniToggleWashVolumePump(context)
	i.iniExtendedWashParameters(context, true)
	i.iniFlushPort(context)
	i.iniNeedleWashParameters(context)

	if context.HardwareContext:GetArgumentValue1("NoOfItemPositions") >= baltic.maxItemPosition then			-- special advanced parameters and dissolve
		context:DeclareASParameter("Injection Path Wash", false, "")
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
	local chrom = 	require "chromatography"
	local gm = 		require "PreRunFunctions"
	local gs = 		require "gradient_segment"
	local pf = 		require "pump_functions"

	gm.genMethod(experiment, installed, context, true)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local pressure = pf.getMaxColumnPressure(experiment.Trap, experiment.Separator, pressSettings.GradientPumpMaxTargetPressure)

	local ovenTempSetPt = experiment.OvenTemperature
	local highSolventB = 95
	local highPressSolventB = 20*0.01	-- highest pressure at 20% B
	local viscoMix = chrom.viscosity_mix(ovenTempSetPt, highPressSolventB)
	local gradient_flow = gs.gradient_flow(chrom.column_flow(experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, pressure, viscoMix))
	gradient_flow = pf.getTrapColumnFlow(experiment.Trap, gradient_flow)
	gradient_flow = pf.getSepColumnFlow(experiment.Trap, experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, gradient_flow, highPressSolventB, ovenTempSetPt)
	-- hsbgt: high solvent B gradient time
	local gradientDeadVolume = chrom.GetGradientDeadVolume(installed)
	local hsbgt = ((chrom.column_volume(experiment.Trap)+chrom.column_volume(experiment.Separator))*2 + gradientDeadVolume)/gradient_flow

	if not experiment.IsKeepGradient then
		local gradient = GradientContainer()
		gradient:AddSetPoint(GradientContainer.SetPoint(0, gradient_flow, 0))
		gradient:AddSetPoint(GradientContainer.SetPoint(experiment.AnalysisTime.TotalSeconds, gradient_flow, 35), true)
		gradient:AddSetPoint(GradientContainer.SetPoint(1*30, gradient_flow, highSolventB), true)
		gradient:AddSetPoint(GradientContainer.SetPoint(hsbgt*60, gradient_flow, highSolventB), true)
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
	if context:GetArgumentValue("Toggle Wash In Gradient") == true then v.valToggleWashVolumePump(context) end

	v.valInjectionPathWash(context)

	if (installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition) then			-- special advanced parameters and dissolve
		v.valSpecialASParameters(context)
		v.valDissolveSample(context, baltic.maxItemPosition)
		v.valDerivatizeSample(context, baltic.maxItemPosition)
		v.valDiluteSeries(context, baltic.maxItemPosition)
	else
		v.valInjectionParameters(context)
	end
end

--- This function is called whenever a data is changed in the method editor
---@param experiment ExperimentInfo
---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function ValidateMethod (experiment, installed, context)
-- This function is called whenever a data is changed in the method editor
	local v = require "PreRunFunctions"
	local pf = require "pump_functions"
	local useTrap = true
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	local numOfSteps, averageStepTime, lastCycleTime = v.valToggleWashVolumePump(context)
	v.valFlushInjectorPort(context, numOfSteps, averageStepTime, lastCycleTime)
	v.valOvenTemp(experiment, context)
	v.valTrapPressure(experiment, context, pressSettings.GradientPumpMaxTargetPressure)
	v.valSepPressure(experiment, context, pressSettings.GradientPumpMaxTargetPressure, useTrap)
	v.valFlow(experiment, context, pressSettings.GradientPumpMaxTargetPressure, useTrap, false)

	v.valInjectionPathWash(context)

	if context:GetArgumentValue("MS Calibrant Injection") then
		local t = v.valCalibration(installed, experiment, context)
		context:SetArgumentValue("calibrantTime", t)
	end

	v.valExtendedWashParameters(installed, context, true)

	if installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then			-- special advanced parameters and dissolve
		v.valSpecialASParameters(context)
		v.valDissolveSample(context, baltic.maxItemPosition)
		v.valDerivatizeSample(context, baltic.maxItemPosition)
		v.valDiluteSeries(context, baltic.maxItemPosition)
	else
		v.valInjectionParameters(context)
	end

	local eqTimeTrap = v.calcEquiTimeTrap(installed, experiment.Trap, pressSettings.GradientPumpMaxTargetPressure, context:GetArgumentValue("trap_equilibration_pressure"), context:GetArgumentValue("trap_equilibration_volumemultiplier"))
	context:SetArgumentValue("trap_equil_time", eqTimeTrap)

	local eqTimeSep = v.calcEquiTimeSeparator(installed, experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, context:GetArgumentValue("oven_temperature"), context:GetArgumentValue("separator_equilibration_pressure"), context:GetArgumentValue("separator_equilibration_volumemultiplier"))
	if eqTimeSep < 1 then eqTimeSep = 1 end						-- PBNE-673
	context:SetArgumentValue("separator_equil_time", eqTimeSep)

	local loadTime = v.reCalcLoadTime(installed, experiment, pressSettings.GradientPumpMaxTargetPressure, context:GetArgumentValue("oven_temperature"), context:GetArgumentValue("column_load_pressure"), context:GetArgumentValue("column_load_volumemultiplier"), context:GetArgumentValue("Additional Loading Volume"), useTrap)
	context:SetArgumentValue("column_load_time", loadTime)
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
	local chrom = 		require "chromatography"
	local ci = 			require "column_information"
	local cplae = 		require "const_pressure_load_and_equilibration"
	local gs = 			require "gradient_segment"
	local is = 			require "inject_sample"
	local parallel = 	require "parallel"
	local pf = 			require "pump_functions"
	local pp = 			require "palplus"
	local pr = 			require "PreRunFunctions"
	local qaw = 		require "queue_autosampler_wash"
	local se = 			require "sep_equilibration"
	local ssf = 		require "strategySplineFit"
	local te = 			require "trap_equilibration"
	---@type Zirconium
	local zr = 			require "zirconium"
	local tw =			require "twinscape_utilities"

	---@type IJournal
	local journal =		context:GetProcedureParticipant(baltic.JournalRole)
	---@type IPalParticipant
	local execLeft = 	context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = 	context:GetProcedureParticipant(baltic.AuxExecutorRole)
	local ovenTemp = 	context:GetArgumentValue("oven_temperature")
	---@type Pump
	local pump = 		context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IProcedureStatusParticipant
	local status = 		context:GetProcedureParticipant(baltic.LcStatusRole)
	local valveI = 		pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = 		pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local injector = 	pp.QueryModule(execAux, pp.Capabilities.IInjector)
	local separationMode = "In two column: "

	---@type ErrorCode
	local error_code = {err = "", message = ""}

	local function sleep_100()
		context:Sleep(100)
	end
	local function sleep_250()
		context:Sleep(250)
	end

	local function sleep_1000()
		context:Sleep(1000)
	end

	---Handle an error message
	local function handle_message()
		if ( error_code.err == "warning" ) then
			context:Report("Two column separation", Severity.Warn, true, error_code.message)
			pr.decompressSystem(context)
			context:Abort()
		elseif (error_code.err == "error" ) then
			context:Report("Two column separation", Severity.Error, true, error_code.message)
			pr.decompressSystem(context)
			context:Abort()
		else
			context:Report("Two column separation", Severity.Info, true, error_code.message)
		end
	end

	-- log method data to TwinScape before method is run
	tw.LogTwinScapeData(installed, context, journal)

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)

	--	Signalize_Reset
	pr.Signalize_Reset(context)
	context:ShowComposition(false)

	pr.iniFlowResistance(context, pump)

	context:Log("Lua date:			{0}", Date)
	context:Log("--- Experiment:    {0}", context.Description)
	ci.logColumnInformation(context, context:GetArgumentValue("trap"), context:GetArgumentValue("separator"))
	context:Log("'{0}' is '{1}'", valveI.Name, pp.Capabilities.ILcInjectorValve)
	context:Log("'{0}' is '{1}'", valveT.Name, pp.Capabilities.ISelectorValve)

	if not chrom.IsColumnParameterSet(context, context:GetArgumentValue("separator")) then
		context:Abort()
	end
	if not chrom.IsColumnParameterSet(context, context:GetArgumentValue("trap")) then
		context:Abort()
	end

	local settings = pump:GetSettings()
	zr.logInstrSettings(context, settings, "Two Column Separation")

	-- check if column oven is intended to use and connected and temperature is set
	if not pr.IsOvenAndTemperatureOK(context, pump) then
		context:Report("Oven", Severity.Error, true, "Missing column oven. Temperature is set but oven is not connected.")
	end

	-- init zirconium channels, abort if unsuccessful
	if not zr.InitChannel(context, pump, zr.A) then
		context:Report(baltic.Naming.PumpA, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end
	if not zr.InitChannel(context, pump, zr.B) then
		context:Report(baltic.Naming.PumpB, Severity.Error, true, "Pump initialization failed. The pump could not enter manual control mode. Check that the instrument is powered on and connected. Try restarting the instrument and running this procedure again.")
		context:Abort()
	end

	-- Main function starts here...
	local startTime = pf.now()

	pf.SetMaxPressureLimit(zr.A, pressSettings.GradientPumpCutoffPressure, pump, sleep_250)
	pf.SetMaxPressureLimit(zr.B, pressSettings.GradientPumpCutoffPressure, pump, sleep_250)

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

	local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_250)
	-- bail if a pump failed degassing..
	if (not (a and b)) then
		pr.decompressSystem(context)
		context:Abort()
	end

	pf.reducePressure(context, pump, zr, zr.A, zr.B, 10, 5, 20, sleep_250, baltic.smooth)
	zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject,nil) 
	zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste,nil)

							-- isLoadingCap, isLoop, isInjectionCap, isTransferCap, isTrap, sepColumn
	local systemUnityFlow = chrom.GetUnityFlow(installed, pressSettings.GradientPumpMaxTargetPressure, true, true, true, true, false, nil)
	context:Log("System unity flow calculated: {0}", systemUnityFlow)
--[[ disabled for plug-in 1.0
	if (installed:Contains("FRT: Route 3 with loop")) then
		local unitySystemFlow = installed:GetArgumentValue1("FRT: Route 3 with loop") / 100		-- flow restriction is measured @ 100 bar
		local unityTransferCapFlow = chrom.GetUnityFlow(installed, pressSettings.GradientPumpMaxTargetPressure, false, false, false, true, false, nil)
		systemUnityFlow = 1/((1/unitySystemFlow) + (1/unityTransferCapFlow))
		context:Log("System unity flow calculated with FRT Route 3 from system information: {0}", systemUnityFlow)
	end
--]]
	local a_sE = se.sepEquiCalcParam(context, pressSettings.GradientPumpMaxTargetPressure, systemUnityFlow, ovenTemp)
	local a_tE = te.trapEquiCalcParam(installed, context:GetArgumentValue("trap"), pressSettings.GradientPumpMaxTargetPressure, context:GetArgumentValue("trap_equilibration_pressure"), context:GetArgumentValue("trap_volume"), baltic.trapEquilVolMultiplier)

	if installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then
		context:LogMeta(context.Name, "Extra injection path washed", "", "N", context:GetArgumentValue("Injection Path Wash"))
		if context:GetArgumentValue("Injection Path Wash") then
			local injectionPathCleaningVolume = pump:GetPistonPosition(zr.A)
			local ew = require "extended_wash"
			ew.injection_path_wash(installed, context, execLeft, execAux, chrom, pp, pr, pump, context:GetArgumentValue("Injection Path Wash", "Time"))		-- inject cleaning liquid from item position 5 into loop
			injectionPathCleaningVolume = pump:GetPistonPosition(zr.A) - injectionPathCleaningVolume
			context:Log("Injection path wash volume: {0} \181L", injectionPathCleaningVolume)
		else
			pr.SetValvePosition(execAux,valveI, baltic.InjectionValve.Load)
		end
	else
		pr.SetValvePosition(execAux,valveI, baltic.InjectionValve.Load)
	end

	---Aspirate sample and optionally dissolve, derivatize or dilute it
	---@param yield_func function
	---@return number
	---@return number
	local function injectSample(yield_func)
--		local waitTime = pf.now() + delay
		local doInjection = true
		local sampleVol, preSampleVol = 0,0
		if context:GetArgumentValue("Exclude Injection") == true then
			context:Log("Sample injection is excluded")
		else
			if installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then
				if context:GetArgumentValue("Dissolve Sample") then
					doInjection = false
					local psp = require "PreSamplePrep"
					context:Log("--- Start dissolving and mixing sample")
					context:Sleep(2000)
					local WashWastePenetrationDepth	= pp.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
					pp.EmptySyringe_And_LeaveObject(context, execLeft, execAux, WashWastePenetrationDepth, installed.SyringeZeroPosition)
					sampleVol, preSampleVol = psp.dissolveAndMixSample(context, yield_func)
					context:Sleep(2000)
					context:LogMeta(context.Name, "Dissolve sample volume", "\181L", "N1", sampleVol)
					context:Log("--- Finished dissolving and mixing sample")
				else
					if context:GetArgumentValue("Derivatize Sample") then
						doInjection = false
						local psp = require "PreSamplePrep"
						context:Log("--- Start derivatization of sample")
						context:Sleep(2000)
						sampleVol, preSampleVol = psp.derivatize_and_inject_Sample(context, yield_func)
						context:Sleep(2000)
						context:LogMeta(context.Name, "Derivatize sample volume", "\181L", "N1", sampleVol)
						context:Log("--- Finished derivatization of sample")
					else
						if context:GetArgumentValue("Dilution Series") then
							doInjection = false
							local psp = require "PreSamplePrep"
							context:Log("--- Start dilution series")
							context:Sleep(2000)
							sampleVol, preSampleVol = psp.dilute_and_inject_Sample(context, yield_func)
							context:Sleep(2000)
							context:LogMeta(context.Name, "Dilution sample volume", "\181L", "N1", sampleVol)
							context:Log("--- Finished dilution series")
						end
					end
				end
			end

			if doInjection then
				sampleVol, preSampleVol = is.injectSample(installed, context, true, yield_func)
			end
			-- wait for injection finished
			while not execLeft.IsIdle do
				parallel.sleep(yield_func, 1000)
			end
			status:RemoveStatus(baltic.Status.Inject)
		end
		return sampleVol, preSampleVol
	end

	---Equilibrate the trap column
	---@param yield_func function
	---@return string
	---@return string
	local function trapColumnEquilibration(yield_func)
		local err = ""
		local msg = ""

		if context:GetArgumentValue("Exclude Equilibration") == false then
			local elapsedTime = pf.now()-startTime
			local txt = DotNetString.Format(", this finishes at about: {0:#0.00} min.", (elapsedTime+a_tE.equilibratingTime+30)/60)
			status:SetStatus(baltic.Status.EquilibrateTrap..txt)
			zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_6_1)
			err, msg = te.trapEquilibration(context, pressSettings.GradientPumpMaxTargetPressure, a_tE, yield_func)
			status:RemoveStatus(baltic.Status.EquilibrateTrap..txt)
			if ( err ~= "" ) then
				context:Log("--- Trap Equilibration failed")
				msg = "Trap column equilibration - "..msg
			end
		else
			if context:GetArgumentValue("Exclude Sample Loading") == false then pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Analytical) end		-- this is needed for sample loading
		end
		return err, msg
	end

	if context:GetArgumentValue("Exclude Equilibration") == false then
		local elapsedTime = pf.now()-startTime
		local txt = DotNetString.Format(", this finishes at about: {0:#0.00} min.", (elapsedTime+a_sE.equilibratingTime+45)/60)
		status:SetStatus(baltic.Status.EquilibrateSeparator..txt)
		---@type number?
		local offsetA, offsetB
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)
		local p_sepEquilibration = {se.sepEquilibration, installed, context, a_sE, ovenTemp, false, parallel.yield}
		local p_logPressSensorOffsetA = { pf.flowSensorOffset, context, zr, pump, zr.A, true, 30, parallel.yield }
		local p_logPressSensorOffsetB = { pf.flowSensorOffset, context, zr, pump, zr.B, true, 30, parallel.yield }
		error_code.err, error_code.message, offsetA, offsetB = parallel.run(sleep_100, p_sepEquilibration, p_logPressSensorOffsetA, p_logPressSensorOffsetB)
		sleep_250()
		status:RemoveStatus(baltic.Status.EquilibrateSeparator..txt)
		if ( error_code.err ~= "" ) then
			context:Log("--- Separator Equilibration failed")
			error_code.message = "Separation column equilibration - "..error_code.message
			handle_message()
		end
		if offsetA ~= nil then
--	Don't save the new offset. Only report it.				
--				pf.SetFlowCalibrationOffset(zr.A, offsetA, pump, sleep_200)
--				settings.FlowCalibrationOffsetA = offsetA
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow Sensor A Offset 2C Equilibration", offsetA, "\181L/min", "N4"))
		end
		if offsetB ~= nil then
--	Don't save the new offset. Only report it.				
--				pf.SetFlowCalibrationOffset(zr.B, offsetB, pump, sleep_200)
--				settings.FlowCalibrationOffsetB = offsetB
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow Sensor B Offset 2C Equilibration", offsetB, "\181L/min", "N4"))
		end
	end

	local sampleVolume, preSampleVolume = 0, 0
	local p_equilibration = { trapColumnEquilibration, parallel.yield }
	local p_injection = { injectSample, parallel.yield }
	error_code.err, error_code.message, sampleVolume, preSampleVolume = parallel.run(sleep_1000, p_equilibration, p_injection)
	if ( error_code.err ~= "" ) then
		handle_message()
	end
---------------------------------------------------------------------
	if context:GetArgumentValue("Exclude Injection") == false then
		is.injectIntoLoop(context, pp, execAux, execLeft, sampleVolume, preSampleVolume)
		-- wait for injection finished
		while not execLeft.IsIdle do
			sleep_100()
		end
	end

	if context:GetArgumentValue("Exclude Sample Loading") == true then
		context:Log("Sample loading is excluded")
	else
		context:LogMeta(context.Name, "Injected sample volume", "\181L", "N1", sampleVolume)
		local loadPress = context:GetArgumentValue("column_load_pressure")
		local loadPressureFactor = loadPress/context:GetArgumentValue("trap_equilibration_pressure")
		local loadFlow = a_tE.equilibratingFlow * loadPressureFactor
		local sampleLoadVolume = context:GetArgumentValue("sample_volume") * context:GetArgumentValue("column_load_volumemultiplier") + 2 + context:GetArgumentValue("Additional Loading Volume")
		local loadTime = 60*sampleLoadVolume/loadFlow	-- [s]
		local elapsedTime = pf.now()-startTime
		local msg = DotNetString.Format(", this finishes at about: {0:#0.00} min.", (elapsedTime+loadTime+5)/60)
--[[ disabled for plug-in 1.0
		local leakFlow = 0
		local leakVolume = 0

		-- take a leak into account when calculation of the expected loading volume
		if (installed:Contains("LT: Pump A @1000bar")) or (installed:Contains("LT: Injection tubing @1000bar")) then
			if (installed:Contains("LT: Pump A @1000bar")) then
				local unityLeakRate_PumpA = installed:GetArgumentValue1("LT: Pump A @1000bar") / 1000		-- leakage is measured @ 1000 bar
				leakFlow = unityLeakRate_PumpA * loadPress
				loadFlow = loadFlow + leakFlow
				context:Log("LT: Pump A / valve A unity leak rate from system information: {0} \181L/min/bar", unityLeakRate_PumpA)
				context:Log("Calculated leak flow of LT pumpA and valve A from system information: {0} \181L/min", leakFlow)
				context:Log("Calculated loading flow with leak included: {0} \181L/min", loadFlow)
				context:Log("Calculated loading volume to be used without a leak: {0} \181L", sampleLoadVolume)
				leakVolume = leakFlow * loadTime/60
				context:Log("Calculated loading volume with leak included: {0} \181L", (sampleLoadVolume + leakVolume))
			end
			if (installed:Contains("LT: Injection tubing @1000bar")) then
				local unityLeakRate_InjTubings = installed:GetArgumentValue1("LT: Injection tubing @1000bar") / 1000		-- leakage is measured @ 1000 bar
				leakFlow = unityLeakRate_InjTubings * loadPress
				context:Log("LT: Injection tubing unity leak rate from system information: {0} \181L/min/bar", unityLeakRate_InjTubings)
				context:Log("Calculated leak flow of LT: Injection tubing from system information: {0} \181L/min", leakFlow)
				context:Log("Calculated loading flow with leak included: {0} \181L/min", (loadFlow + leakFlow))
				context:Log("Calculated loading volume to be used without a leak: {0} \181L", sampleLoadVolume)
				leakVolume = leakVolume + (leakFlow * loadTime/60)
				context:Log("Calculated loading volume with leak included: {0} \181L", (sampleLoadVolume + leakVolume))
			end
			sampleLoadVolume = sampleLoadVolume + leakVolume
			loadFlow = loadFlow + leakFlow
		else
			if (installed:Contains("FRT: Route 3 with loop")) then
				local unitySystemFlow = installed:GetArgumentValue1("FRT: Route 3 with loop") / 100		-- flow restriction is measured @ 100 bar
				local unityTransferCapFlow = chrom.GetUnityFlow(installed, pressSettings.GradientPumpMaxTargetPressure, false, false, false, true, false, nil)
				systemUnityFlow = 1/((1/unitySystemFlow) + (1/unityTransferCapFlow))
				context:Log("System unity flow calculated with FRT Route 3 from system information: {0}", systemUnityFlow)
			end
		end
--]]
		-- set trap column or baltic max pressure for sample loading
		pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, true, false, loadPress)

		status:SetStatus(baltic.Status.LoadColumn..msg)

		pr.SetValvePosition(execAux, valveI, baltic.InjectionValve.Load)
		local beforeLoadingStarts = context:GetArgumentValue("Injection Delays", "Before Loading Starts")
		context:Sleep(beforeLoadingStarts)
	--	context:Sleep(1000)

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
		context:Log("--- Sample Loading on Trap Column")
		context:Log("--- Trap Temperature: fixed to 20 C")
		context:Log("--- Trap Unity-Flow [ul/bar]:          {0}", context:GetArgumentValue("trap_unityflow"))
		context:Log("--- Trap Volume [ul]:                  {0}", context:GetArgumentValue("trap_volume"))
		context:Log("--- Sample Volume [ul]:                {0}", context:GetArgumentValue("sample_volume"))
		context:Log("--- Sample Volume Multiplier:          {0}", context:GetArgumentValue("column_load_volumemultiplier"))
		context:Log("--- Trap Loading Pressure [bar]:       {0}", loadPress)
		context:Log("--- Trap Loading Flow [uL/min]:        {0}", loadFlow)
		context:Log("--- Trap Loading Volume (+2) [uL]:     {0}", sampleLoadVolume)
		context:Log("--- Trap Loading Time [min]:           {0}", loadTime/60)
		context:Log(baltic.devider)
		local p,v,f,t = nil,nil,nil,nil
		p,v,f,t, error_code.err, error_code.message = cplae.const_pressure_load_and_equilibration(pump, zr.A, loadPress, sampleLoadVolume, loadFlow, sleep_250, loadTime*10, false)
		context:Log(baltic.devider)
		context:Log("--- Finished: Sample Loading on Trap Column (const_pressure_load_and_equilibration)")
		context:Log("--- act. pressure [bar]:     {0}",p)
		context:Log("--- used volume [uL]:        {0}",v)
		context:Log("--- average flow [uL/min]:   {0}",f)
		context:Log("--- act. loading time [min]: {0}",t/60)
		context:Log(baltic.devider)
		context:LogMeta(context.Name, "Expected trap loaded volume", "\181L/min", "N2", sampleLoadVolume)
		context:LogMeta(context.Name, "Trap loaded volume", "\181L", "N2", v)
		context:LogMeta(context.Name, "Expected trap loading flow", "\181L/min", "N2", loadFlow)
		context:LogMeta(context.Name, "Trap loading flow", "\181L/min", "N2", f)
		context:LogMeta(context.Name, "Expected trap loading time", "min", "N2", loadTime/60)
		context:LogMeta(context.Name, "Trap loading time", "min", "N2", t/60)
		if ( error_code.err ~= "" ) then
			context:Log("--- Sample loading failed")
			error_code.message = "Loading column - "..error_code.message
			handle_message()
		end
		if (f<0.5*loadFlow) then
			local msg1 = "Loading flow ("..pf.noExp(f,3)..") is too low (expected: "..pf.noExp(loadFlow,3).."). Carry out a column diagnosis to check whether the column is blocked."
			if (f<(0.2*loadFlow)) then
				context:Report("Trap column:", Severity.Warn, true, msg1)
			else
				context:Report("Trap column:", Severity.Tip, true, msg1)
			end
		elseif (f>1.5*loadFlow) then
			local msg1 = "Loading flow ("..pf.noExp(f,3)..") is too high (expected: "..pf.noExp(loadFlow,3).."). Carry out a column diagnosis to check whether the column is leaking."
			if (f>2*loadFlow) then
				context:Report("Trap column:", Severity.Warn, true, msg1)
			else
				context:Report("Trap column:", Severity.Tip, true, msg1)
			end
		end

		status:RemoveStatus(baltic.Status.LoadColumn..msg)
	end

	-- set trap or separation column or baltic max pressure for gradient
	local maxPress = pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, true, true)	-- set column max pressure

	context:ShowComposition(true)

	------------------------------------------------------------------------	
	status:SetStatus("preparing gradient start")
	------------------------------------------------------------------------	
	---@type GradientContainer
	local gradient = context:GetArgumentValue("gradient")
	ssf.strategySplineFit(context, gradient, pp, execAux, pump, zr, pressSettings.GradientPumpMaxTargetPressure, pressSettings.GradientPumpCutoffPressure, true)
	status:RemoveStatus("preparing gradient start")

	local totalToggleTime = 0

	local gradient_method = zr.CreateGradient(0, maxPress, true, ovenTemp)
	for segment in gs.dotnet_each(gradient) do
		zr.AddGradientSegment(gradient_method, segment.Time, segment.Flow, segment.Mix)
		totalToggleTime = segment.Time
	end

	context:SetSignal(baltic.LcElutionReady)
	context:WaitForSignal(baltic.MsAcquisitionStart)
	local err = zr.LoadGradient(context, pump, gradient_method)
	if err then
		context:Report("Gradient", Severity.Error, true, err)
		context:Abort()
	end
	context:Log("--- Set gradient start")
	-- wait for gradient to start
	-- restart gradient every second if not already started
	-- abort gradient if not started within 10 seconds
	zr.StartGradient(context, pump)

	-- Gradient has started
	pr.SetSignal(1, execAux) -- set SWOut1 signal for 1 second

	local switchValveIisEnabled = false			-- is needed during the gradient is running
	---@type ToggleWashContainer
	local ToggleWashContainer = {numOfSteps=0, nextStepOrganic=false, organicStepVolume=0, aqueousStepVolume=0, pumpSpeed=0, timeToNextStepStart=0, stepTime=0, lastStepVolume=0, lastStepSpeed=0, totalTime=0, onlyOneStepOrganic=false, aqueousOnly=false, toggleAqueousOrganic=false, isToggleWashEnabled=false, numOfLastAqueousSteps=0}
	ToggleWashContainer.isToggleWashEnabled = context:GetArgumentValue("Toggle Wash In Gradient")
	-- Start AS wash
	status:SetStatus("wash")
	if ToggleWashContainer.isToggleWashEnabled then
		if context:GetArgumentValue("Flush Injector Port") == true then
			local vol = context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Sealed") + context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Unsealed")
			totalToggleTime = totalToggleTime - (vol / context:GetArgumentValue("Flush Injector Port", "Flow Rate")) - (vol / pp.VolumetricPumpVolume)
		end
		ToggleWashContainer = pp.ToggleWashInitialization(context, ToggleWashContainer, totalToggleTime)
		pp.PrintToggleWashParameter(context, ToggleWashContainer)
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)			-- to clean the loop during toggle wash
	elseif context:GetArgumentValue("Disable Injector Cleaning") == false then
		context:Log("queue_clean_injector_loop")
		qaw.queue_clean_injector_loop(execLeft, execAux, pp, false, false) -- with valveI in INJECT position
		switchValveIisEnabled = true			-- true: switch valveI to load after cleaning is complete
	end
	if context:GetArgumentValue("Flush Injector Port") == true or ToggleWashContainer.isToggleWashEnabled == true then
		execLeft:LeaveObject()
		execLeft:MoveToObject(injector)
		execLeft:PenetrateWithConstForce(injector)
	end
---------------------------------------------------------------
	status:SetStatus("elution")  --- Pump is running gradient

	-- wait for gradient to complete
	pf.iniFlowObserver(180)
	local flushInjectorEnabled = context:GetArgumentValue("Flush Injector Port")
	local switchInjectionValve = true
	local showMsg1 = true
	local showMsg2 = true
	local timeToWait = pf.now()
	pf.deletePressABandCompositionFile()
	local pressA = 0
	local pressB = 0
	local sumPressA = 0
	local sumPressB = 0
	local composition = 0
	local cnt = 0
	local numOfValues = 0
	local maxMeasuredPress = 0
	local flow = 0
	local actComp = 0
	local code = 0
	while (zr.IsGradientRunning(pump)) do
		context:Sleep(1000)
		cnt = cnt + 1
		if execLeft.IsIdle and switchValveIisEnabled then
			-- switch valveI to LOAD after cleaning the injector and loop
			pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)
			execLeft:MoveToHome()
			switchValveIisEnabled = false
			status:RemoveStatus("wash")
		end
		if (ToggleWashContainer.isToggleWashEnabled) then
			if (pf.now() >= timeToWait) then
				ToggleWashContainer = pp.RunToggleWashVolumePump(execLeft, ToggleWashContainer)
				timeToWait = pf.now() + ToggleWashContainer.timeToNextStepStart
			end
		else
			if flushInjectorEnabled == true then
				context:Log("--- Running forward flush port")
				flushInjectorEnabled = false			-- do it just once
				local solvent     = pp.Aqueous
				local volume      = context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Sealed")
				local speed       = context:GetArgumentValue("Flush Injector Port", "Flow Rate")
				local liftUpHight = 0
				pp.PrintFlushPortParameter(context, solvent, volume, speed, liftUpHight)
				pp.FlushPort(solvent, volume, speed, liftUpHight, execLeft, execAux)							-- flush port sealed
				volume      = context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Unsealed")
				liftUpHight = context:GetArgumentValue("Flush Injector Port", "Lift-Up Distance")
				pp.PrintFlushPortParameter(context, solvent, volume, speed, liftUpHight)
				pp.FlushPort(solvent, volume, speed, liftUpHight, execLeft, execAux)							-- flush port unsealed
			end
			if (execLeft.IsIdle and switchInjectionValve == true) then
				pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)			-- PNS-92
				switchInjectionValve = false
				pp.SetLCToolValve(execLeft, pp.LcToolValveClose)
			end
		end
		if cnt >= 10 then
			pressA, pressB, composition = pf.getPressAndComp(pump, zr)
			sumPressA = sumPressA + pressA
			sumPressB = sumPressB + pressB
			pf.storePressABandComposition(pressA, pressB, composition)
			numOfValues = numOfValues + 1
		end
		maxMeasuredPress, flow, actComp = pf.getPressAndComp(pump, zr)
		code = pf.isGradientOK(context, pressSettings.GradientPumpCutoffPressure, 20, maxPress, pump, zr, code, separationMode)
		if (code == 4) then
			context:LogMeta(context.Name, "Pressure Higher Maximum", "bar", "N0", maxPress)
			zr.abortGradient(pump)
			break
		else
			if (code == 1) and (showMsg1 == true) then
				context:LogMeta(context.Name, "Unstable Flow", "", "N", "True")
				showMsg1 = false
			end
			if (code == 2) and (showMsg2 == true) then
				context:LogMeta(context.Name, "Pressure within 5% Of Maximum", "bar", "N0", maxPress)
				showMsg2 = false
			end
		end
	end
	context:SetSignal(baltic.LcElutionDone)
	status:RemoveStatus("elution")   --- Pump has finished running gradient
---------------------------------------------------------------

	pf.evaluatePressure(context, sumPressA, sumPressB, numOfValues)
	pcall(ci.logMaxColumnPress, context, maxMeasuredPress, flow, actComp, ovenTemp, context:GetArgumentValue("separator").Name)

	-- wait for queued autosampler wash to finish
	while not execLeft.IsIdle do
		sleep_250()
	end

	-- reduce pressure to not to switch valves under high pressure during calibrant injection or extended wash
	-- !!! make sure that: pump.valve==MixTee AND NOT (trapValve==Analytical OR trapValve==Trap) !!!
	pf.reducePressure(context, pump, zr, zr.A, zr.B, 10, 5, 30, sleep_250, baltic.smooth)

	if context:GetArgumentValue("MS Calibrant Injection") and (code == 0 or code == 2) then
		-- Calibrant Injection
		local ic = require "InjectCalibrant"
		---@type CalibrantInjectionParameter
		local qpcaui_params = {
			sample_aspirate_speed = 	pp.Quantity(context:GetArgumentValue("sample_aspirate_speed"), "uL/s"),
			sample_postaspirate_delay = pp.Quantity(context:GetArgumentValue("sample_postaspirate_delay"), "ms"),
			presample_air_volume = 		pp.Quantity(context:GetArgumentValue("presample_air_volume"), "uL"),
			calibrant_volume = 			a_sE.equilibratingFlow, -- this is for 1 minute calibrant signal	-- context:GetArgumentValue("calibrant_volume"),						-- do not pp.Quantity due to calculations with this value
			postsample_air_volume = 	pp.Quantity(context:GetArgumentValue("postsample_air_volume"), "uL"),
			sample_inject_speed = 		pp.Quantity(context:GetArgumentValue("sample_inject_speed"), "uL/s"),
			flowPerBar = 				context:GetArgumentValue("separator_unityflow"),
			systemVolume =				context:GetArgumentValue("separator_volume") + baltic.SystemVolume	-- column volume + capillary volume
		}

		ic.injectCalibrant(installed, context, qpcaui_params, a_sE.equilibratingFlow, sleep_100)
	end

	pr.SetValvePosition(execLeft, valveT, baltic.TrapValve.Waste)

	if context:GetArgumentValue("Extended Wash") then
		local ew = require "extended_wash"
		context:Log(baltic.devider)
		context:Log("--- Extended Wash:")
		status:SetStatus("clean injector & trap column")
		local includeTrapColumn = context:GetArgumentValue("Extended Wash", "Include Trap Column")
		error_code = ew.extended_wash(installed, context, execLeft, execAux, pp, pump, includeTrapColumn, false, context:GetArgumentValue("Extended Wash", "Num Of Cycles"), context:GetArgumentValue("Extended Wash", "Volume Organic Per Cycle"), context:GetArgumentValue("Extended Wash", "Volume Aqueous Per Cycle"), context:GetArgumentValue("Extended Wash", "Flow Organic"), context:GetArgumentValue("Extended Wash", "Flow Aqueous"), sleep_100)
		if ( error_code.err ~= "" ) then
			error_code.message = "Autosampler clean column - "..error_code.message
--			handle_message(error_code)		-- don"t Abort() here, handle_message later
		end
		status:RemoveStatus("clean injector & trap column")
		context:Log(baltic.devider)
	end

	-- is done in 'Decompress.lua'
	--pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 20, sleep_250, baltic.smooth)

	while not execLeft.IsIdle do
		sleep_250()
	end
	-- stop pumps before ending
	pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
	pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
	-- wait for pump to stop completely
	pf.isPumpIdle(pump, sleep_100)

	pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Load)
	execLeft:MoveToHome()

	-- waiting for autosampler stopps completely
	while not execLeft.IsIdle do
		sleep_250()
	end	

	if (code < 3) then		-- show error, warning or nothing depending on code
		local mode = "In gradient"
		pf.showGradientMassage(context, code, mode)
	end

	dictator:Dispose()

	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)

	if ( error_code.err ~= "" ) then
		handle_message()
	end
end
