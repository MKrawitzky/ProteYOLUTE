-- require('lldebugger').start()

local Date = "2025/09/24"

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
	context.Name = "1C separation"
	context.DisplayName = "One column separation"
	context.LegacyName = "One column separation"
	context.Description = "Single column liquid chromatographic separation"
	context.Hidden = true
	context.DecompressOnExit = false
	context.OverwriteLogFiles = false
	context.LedState = LedState.Acquisition

	local i = require "PreRunFunctions"
	i.ini(context, false, true, false, 30*60, 0.5, 1.0)	
	--   (context, useTrap, useSep, iso, analysisTime, preAirVol, postAirVol)

	--> This has to be password protected
	context:DeclareASParameter("Exclude Injection", false, "", "boolean", true)
	context:DeclareASParameter("Exclude Equilibration", false, "", "boolean", true)
	context:DeclareASParameter("Exclude Sample Loading", false, "", "boolean", true)
	context:DeclareASParameter("Disable Injector Cleaning", false, "", "boolean", true)
	--< This has to be password protected

	context:DeclareASParameter("Flush Channel B", true, "")
	context:DeclareASParameter("Stabilization Time", 4, "sec", "integer")
	context:DeclareASParameter("Pump B Starting Pressure Offset", -20.0, "%", "decimal")

	context:DeclareASParameter("Set Equil. Press at Gradient End", true, "")

	--> Service parameter
	context:DeclareASParameter("Set Press Pump B at Gradient End", true, "", "boolean", true)
	context:DeclareASParameter("Flush Tool Before Aspirate Sample", 0, "\181L", "integer", true)
	context:DeclareASParameter("Flush Tool After Toggle Wash", 0, "\181L", "integer", true)
	context:DeclareASParameter("Flush Tool Pump Speed", 50, "\181L/sec", "integer", true)
	context:DeclareASParameter("Alternative Transport Liquid", false, "", "boolean", true)
	--< Service parameter

	i.iniInjectionDelays(context)
	i.iniToggleWashVolumePump(context)
	i.iniFlushPort(context)
	i.iniExtendedWashParameters(context, false)
	i.iniNeedleWashParameters(context)

	if context.HardwareContext:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then			-- special advanced parameters and dissolve
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
-- This function is called when the generate button is pressed in the method editor
	local chrom = 	require "chromatography"
	local gm =    	require "PreRunFunctions"
	local gs = 		require "gradient_segment"
	local pf = 		require "pump_functions"

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	gm.genMethod(experiment, installed, context, false)

	local pressure = pf.getColumnMaxPressure(experiment.Separator, pressSettings.GradientPumpMaxTargetPressure)
	--PNS-752: Keep the 800 default for release 1.0 because PepSep columns don't work reliably at 1000 bar at the moment
	--local pressTarget = math.min(pressure, 1000.0)
	--context:SetArgumentValue("separator_equilibration_pressure", pressTarget)
	--context:SetArgumentValue("column_load_pressure", pressTarget)

	local ovenTempSetPt = experiment.OvenTemperature
	local highSolventB = 95
	local highPressSolventB = 20*0.01	-- highest pressure at 20% B
	local viscoMix = chrom.viscosity_mix(ovenTempSetPt, highPressSolventB)
	local gradient_flow = gs.gradient_flow(chrom.column_flow(experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, pressure, viscoMix))
	gradient_flow = pf.getSepColumnFlow(nil, experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, gradient_flow, highPressSolventB, ovenTempSetPt)
	-- hsbgt: high solvent B gradient time
	local gradientDeadVolume = chrom.GetGradientDeadVolume(installed)
	local hsbgt = (chrom.column_volume(experiment.Separator)*4+gradientDeadVolume)/gradient_flow

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

---@param experiment ExperimentInfo
---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function ValidateMethod (experiment, installed, context)
-- This function is called whenever a data is changed in the method editor
	local pf = require "pump_functions"
	local v = require "PreRunFunctions"
	local val = require "validation"
	local useTrap = false
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	val.verify_range(context, "Stabilization Time", 0, 120)
	val.verify_range(context, "Pump B Starting Pressure Offset", -100, 100)
	if context:GetArgumentValue("Flush Tool After Toggle Wash") > 0 then
		val.verify_range(context, "Flush Tool After Toggle Wash", 300, 2000)
	end

	local numOfSteps, averageStepTime, lastCycleTime = v.valToggleWashVolumePump(context)
	v.valFlushInjectorPort(context, numOfSteps, averageStepTime, lastCycleTime)
	v.valOvenTemp(experiment, context)
	v.valSepPressure(experiment, context, pressSettings.GradientPumpMaxTargetPressure, useTrap)
	v.valFlow(experiment, context, pressSettings.GradientPumpMaxTargetPressure, useTrap, true)

	v.valInjectionPathWash(context)

	if context:GetArgumentValue("MS Calibrant Injection") then
		local t, _, _, _, vol = v.valCalibration(installed, experiment, context)
		context:SetArgumentValue("calibrant_volume", vol)
		context:SetArgumentValue("calibrantTime", t)
	end

	v.valExtendedWashParameters(installed, context, false)

	local noOfItemPositonsInstalled = installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions)
	if (noOfItemPositonsInstalled >= baltic.maxItemPosition) then			-- special advanced parameters and dissolve
		v.valSpecialASParameters(context)
		v.valDissolveSample(context, noOfItemPositonsInstalled)
		v.valDerivatizeSample(context, noOfItemPositonsInstalled)
		v.valDiluteSeries(context, baltic.maxItemPosition)
	else
		v.valInjectionParameters(context)
	end

	local eqTimeSep = v.calcEquiTimeSeparator(installed, experiment.Separator, pressSettings.GradientPumpMaxTargetPressure, context:GetArgumentValue("oven_temperature"), context:GetArgumentValue("separator_equilibration_pressure"), context:GetArgumentValue("separator_equilibration_volumemultiplier"))
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
	local chrom = 	require "chromatography"
	local ci = 		require "column_information"
	local gs = 		require "gradient_segment"
	local is = 		require "inject_sample"
	local parallel = require "parallel"
	local pf = 		require "pump_functions"
	local pp = 		require "palplus"
	local pr = 		require "PreRunFunctions"
	local qaw = 	require "queue_autosampler_wash"
	local se = 		require "sep_equilibration"
	local sfsf = 	require "strategyFastSplineFit"
	---@type Zirconium
	local zr = 		require "zirconium"
	local tw =		require "twinscape_utilities"

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
	local valveI = pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)
	local injector = pp.QueryModule(execAux, pp.Capabilities.IInjector)
	local N = baltic.Naming
	local preSampleVolume = 0
	local sampleVolume = 0
	local separationMode = "In fast one column: "
	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local pistonBPosition = 0
	local pistonBMeasureTime = 0
	local checkForPumpBLeakage = false

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
			context:Report(separationMode, Severity.Warn, true, error_code.message)
			pr.decompressSystem(context)
			context:Abort()
		elseif (error_code.err == "error" ) then
			context:Report(separationMode, Severity.Error, true, error_code.message)
			pr.decompressSystem(context)
			context:Abort()
		else
			context:Report(separationMode, Severity.Info, true, error_code.message)
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

	local fR = require "PreRunFunctions"
	fR.iniFlowResistance(context, pump)

	context:Log("Lua date:			   {0}", Date)
	context:Log("No of item positions: {0}", installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions))
	context:Log("--- Experiment:       {0}", context.Description)
	ci.logColumnInformation(context, context:GetArgumentValue("trap"), context:GetArgumentValue("separator"))

	context:Log("'{0}' is '{1}'", valveI.Name, pp.Capabilities.ILcInjectorValve)
	context:Log("'{0}' is '{1}'", valveT.Name, pp.Capabilities.ISelectorValve)

	if not chrom.IsColumnParameterSet(context, context:GetArgumentValue("separator")) then
		context:Abort()
	end

	---@type IChromatographyColumnType
	local sep = context:GetArgumentValue("separator")
	local ovenTemp = context:GetArgumentValue("oven_temperature")
	local viscoMix = chrom.viscosity_mix(ovenTemp, 0)
	context:Log("--- Seperator equil time:      {0}", pf.noExp(context:GetArgumentValue("separator_equil_time"),2))
	context:Log("--- Column loading time:       {0}", pf.noExp(context:GetArgumentValue("column_load_time"),2))
	context:Log("--- Sep.Length:                {0}", sep.Length)
	context:Log("--- Sep.ColumnDiameter:        {0}", sep.ColumnDiameter)
	context:Log("--- Sep.IsColumnPorosity:      {0}", sep.IsColumnPorosity)
	context:Log("--- Sep.ParticleDiameter:      {0}", sep.ParticleDiameter)
	context:Log("--- oven_temperature:          {0}", ovenTemp)
	context:Log("--- Column viscoMix:           {0}", viscoMix)
	context:Log("--- Column flow @ {1} bar:    {0}", chrom.column_flow(sep, pressSettings.GradientPumpMaxTargetPressure, pressSettings.GradientPumpMaxTargetPressure, viscoMix), pressSettings.GradientPumpMaxTargetPressure)
	context:Log("--- Column unity flow:         {0}", chrom.column_flow(sep, pressSettings.GradientPumpMaxTargetPressure, 1, viscoMix))

	local settings = pump:GetSettings()
	zr.logInstrSettings(context, settings, "Four Valves Separation")

	-- check if column oven is intended to use, connected and temperature is set
	if not pr.IsOvenAndTemperatureOK(context, pump) then
		context:Report("Oven", Severity.Error, true, "Missing column oven. Temperature is set but oven is not connected.")
	end

	pf.SetMaxPressureLimit(zr.A, pressSettings.GradientPumpCutoffPressure, pump, sleep_250)
	pf.SetMaxPressureLimit(zr.B, pressSettings.GradientPumpCutoffPressure, pump, sleep_250)

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)

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

	local a, b, _, _ = degas(context, 1, baltic.pumpLevel, baltic.pumpLevel, pf.now(), sleep_250)
	-- bail if a pump failed degassing..
	if (not (a and b)) then
		pr.decompressSystem(context)
		context:Abort()
	end

	-- do an equilibration at the beginning if the pumps are refilled or this is the first run
	local equilibrateAtTheGradientEnd = context:GetArgumentValue("Set Equil. Press at Gradient End")
	if pump:GetCurrentPressure(zr.A) < 100 then equilibrateAtTheGradientEnd = false end
	if equilibrateAtTheGradientEnd then
		local loadingPressure = context:GetArgumentValue("column_load_pressure")
		local actualPumpBPressure = pump:GetCurrentPressure(zr.B)
		zr.SetValvePosition(context, pump, zr.B, (baltic.PumpValve.MixTee + 30), nil)
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject,nil)
		pr.SetValvePosition(execAux,valveT, baltic.TrapValve.Analytical)
		context:Signalize(baltic.ColorsRGB.Blue, N.PumpARearToValve, N.ValveAGroove, N.ValveAToInjectionValve, N.ValveIGroovesLoad, N.Loop, N.InjectToTrap, N.ValveTShortGroove, N.TransferLine, N.Separator)
		pf.Manualmode_Pump_constantPressure(zr.A, loadingPressure, pump, sleep_100)
		pf.Manualmode_Pump_constantPressure(zr.B, actualPumpBPressure, pump, sleep_100)
		pistonBPosition = pump:GetPistonPosition(zr.B)
		pistonBMeasureTime = pf.now()
		checkForPumpBLeakage = true
	else
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 20, 15, 20, sleep_250, baltic.smooth)

		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Inject,nil)
		zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Waste,nil)
		pr.SetValvePosition(execAux,valveT, baltic.TrapValve.InjectWaste)
	end

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
	if installed:GetArgumentValue1("NoOfItemPositions") >= baltic.maxItemPosition then
		context:LogMeta(context.Name, "Extra injection path washed", "", "N", context:GetArgumentValue("Injection Path Wash"))
		if context:GetArgumentValue("Injection Path Wash") then
			local injectionPathCleaningVolume = pump:GetPistonPosition(zr.A)
			local ew = require "extended_wash"
			ew.injection_path_wash(installed, context, execLeft, execAux, chrom, pp, pr, pump, context:GetArgumentValue("Injection Path Wash", "Time"))		-- inject cleaning liquid from item position 5 into loop
			injectionPathCleaningVolume = pump:GetPistonPosition(zr.A) - injectionPathCleaningVolume
			context:Log("Injection path wash volume: {0} \181L", injectionPathCleaningVolume)
			pr.SetValvePosition(execAux,valveT, baltic.TrapValve.Analytical)
		else
			pr.SetValvePosition(execAux,valveI, baltic.InjectionValve.Load)
		end
	else
		pr.SetValvePosition(execAux,valveI, baltic.InjectionValve.Load)
	end

	if equilibrateAtTheGradientEnd and (context:GetArgumentValue("Injection Path Wash") == false) then
		local loadingPressure = context:GetArgumentValue("column_load_pressure")
		local minPressure = loadingPressure - 10
		if (a_sE.equilibratingFlow > 25) then minPressure = loadingPressure - 30 end
		while (pump:GetCurrentPressure(zr.A) < minPressure) do sleep_100() end
	end

	---Aspirate sample and optionally dissolve, derivatize or dilute it
	---@param yield_func function
	local function injectSample(yield_func)
		if context:GetArgumentValue("Exclude Injection") == true then
			context:Log("Sample injection is excluded")
		else
			if context:GetArgumentValue("Flush Tool Before Aspirate Sample") > 0 then
				local flushVolume = context:GetArgumentValue("Flush Tool Before Aspirate Sample")
				local flushToolPumpSpeed = context:GetArgumentValue("Flush Tool Pump Speed")
				pp.FlushLCPTool(flushVolume, flushToolPumpSpeed, execLeft, execAux)
				-- wait for flush tool finished
				while not execLeft.IsIdle do
					parallel.sleep(yield_func, 100)
				end
			end

			local doInjection = true
			if 	installed:GetArgumentValue1(baltic.Naming.NoOfItemPositions) >= baltic.maxItemPosition then
				if context:GetArgumentValue("Dissolve Sample") then
					doInjection = false
					local psp = require "PreSamplePrep"
					context:Log("--- Start dissolving and mixing sample")
					parallel.sleep(yield_func, 2000)
					local WashWastePenetrationDepth	= pp.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm")
					pp.EmptySyringe_And_LeaveObject(context, execLeft, execAux, WashWastePenetrationDepth, installed.SyringeZeroPosition)
					sampleVolume, preSampleVolume = psp.dissolveAndMixSample(context, yield_func)
					parallel.sleep(yield_func, 2000)
					context:LogMeta(context.Name, "Dissolve sample volume", "\181L", "N1", sampleVolume)
					context:Log("--- Finished dissolving and mixing sample")
				else
					if context:GetArgumentValue("Derivatize Sample") then
						doInjection = false
						local psp = require "PreSamplePrep"
						context:Log("--- Start derivatization of sample")
					parallel.sleep(yield_func, 2000)
						sampleVolume, preSampleVolume = psp.derivatize_and_inject_Sample(context, yield_func)
					parallel.sleep(yield_func, 2000)
						context:LogMeta(context.Name, "Derivatize sample volume", "\181L", "N1", sampleVolume)
						context:Log("--- Finished derivatization of sample")
					else
						if context:GetArgumentValue("Dilution Series") then
							doInjection = false
							local psp = require "PreSamplePrep"
							context:Log("--- Start dilution series")
							parallel.sleep(yield_func, 2000)
							sampleVolume, preSampleVolume = psp.dilute_and_inject_Sample(context, yield_func)
							parallel.sleep(yield_func, 2000)
							context:LogMeta(context.Name, "Dilution sample volume", "\181L", "N1", sampleVolume)
							context:Log("--- Finished dilution series")
						end
					end
				end
			end

			if doInjection then
				sampleVolume, preSampleVolume = is.injectSample(installed, context, true, yield_func)
			end
			-- wait for injection finished
			while not execLeft.IsIdle do
				parallel.sleep(yield_func, 100)
			end
			status:RemoveStatus(baltic.Status.Inject)
		end
	end

	---Equilibrate the separator column
	---@param yield_func function
	---@return string
	---@return string
	local function equiSep(yield_func)
		local err = ""
		local msg = ""

		if context:GetArgumentValue("Exclude Equilibration") == true then
			context:Log("Equilibration is excluded")
			if context:GetArgumentValue("Exclude Sample Loading") == false then pr.SetValvePosition(execAux, valveT, baltic.TrapValve.Analytical) end		-- this is needed for sample loading
		else
			if not equilibrateAtTheGradientEnd then
				local elapsedTime = pf.now()-startTime
				local txt = DotNetString.Format(", this finishes at about: {0:#0.00} min.", (elapsedTime+a_sE.equilibratingTime+40)/60)
				status:SetStatus(baltic.Status.EquilibrateSeparator..txt)
				err, msg = se.sepEquilibration(installed, context, a_sE, ovenTemp, true, yield_func)
				status:RemoveStatus(baltic.Status.EquilibrateSeparator..txt)
			else
				parallel.sleep(yield_func, (a_sE.equilibratingTime-15)*1000)
			end
		end
		return err, msg
	end

	---Sends the leak rate on pump B to twinscape
	---@param test string
	---@param startPositionB number
	---@param startTimeB number
	local function CheckForPumpBLeakage(test, startPositionB, startTimeB)
		local leakageB = pump:GetPistonPosition(zr.B) - startPositionB			-- [µL]
		pistonBMeasureTime = pf.now() - startTimeB						-- [sec]
		local leakagePerMinute = leakageB / (pistonBMeasureTime / 60)			-- [µL/min]
		context:LogMeta(context.Name, test, "\181L/min", "N2", leakagePerMinute)
	end

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)
	local p_equilibration = { equiSep, parallel.yield }
	local p_injection = { injectSample, parallel.yield }
	error_code.err, error_code.message = parallel.run(sleep_1000, p_equilibration, p_injection)
	if ( error_code.err ~= "" ) then
		context:Log("--- Separation column equilibration failed")
		error_code.message = "Separation column equilibration - "..error_code.message
		handle_message()
	end
	---------------------------------------------------------------------
	if context:GetArgumentValue("Exclude Injection") == true then
		context:Log("Sample injection is excluded")
	else
		is.injectIntoLoop(context, pp, execAux, execLeft, sampleVolume, preSampleVolume)
		-- wait for injection finished
		while not execLeft.IsIdle do
			sleep_100()
		end

		if checkForPumpBLeakage and (pf.now() - pistonBMeasureTime) > 10 then
			CheckForPumpBLeakage("LT: Pump B Equilibration", pistonBPosition, pistonBMeasureTime)
		end
	end

	if context:GetArgumentValue("Exclude Sample Loading") == true then
		context:Log("Sample loading is excluded")
	else
		context:LogMeta(context.Name, "Injected sample volume", "\181L", "N1", sampleVolume)
		local a_sL = se.sepLoadCalcParam(sep, systemUnityFlow, ovenTemp, pressSettings.GradientPumpMaxTargetPressure, context:GetArgumentValue("column_load_pressure"), context:GetArgumentValue("sample_volume"), context:GetArgumentValue("column_load_volumemultiplier"), context:GetArgumentValue("Additional Loading Volume"))
		local elapsedTime = pf.now()-startTime
		local loadingEndTime = math.max(a_sL.loadingTime+5, 55)
		local msg = DotNetString.Format(", this finishes at about: {0:#0.00} min.", (elapsedTime+loadingEndTime)/60)
		status:SetStatus(baltic.Status.LoadSepColumn..msg)
--		error_code.err, error_code.message = se.sepSampleLoading(installed, context, a_sL, ovenTemp, sleep_250)
		---@type number?
		local offsetA, offsetB
		local p_loadSample = { se.sepSampleLoading, installed, context, a_sL, ovenTemp, parallel.yield }
		local p_logPressSensorOffsetA = { pf.flowSensorOffset, context, zr, pump, zr.A, true, 30, parallel.yield }
		local p_logPressSensorOffsetB = { pf.flowSensorOffset, context, zr, pump, zr.B, true, 30, parallel.yield }
		error_code.err, error_code.message, offsetA, offsetB = parallel.run(sleep_100, p_loadSample, p_logPressSensorOffsetA, p_logPressSensorOffsetB)
		if ( error_code.err ~= "" ) then
			context:Log("--- Sample loading failed with:")
			error_code.message = "Loading column - "..error_code.message
			handle_message()
		end
		if offsetA ~= nil then
--	Don't save the new offset. Only report it.				
--				pf.SetFlowCalibrationOffset(zr.A, offsetA, pump, sleep_200)
--				settings.FlowCalibrationOffsetA = offsetA
			journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Flow Sensor A Offset Fast1C Loading", offsetA, "\181L/min", "N5"))
		end
		if offsetB ~= nil then
--	Don't save the new offset. Only report it.				
--				pf.SetFlowCalibrationOffset(zr.B, offsetB, pump, sleep_200)
--				settings.FlowCalibrationOffsetB = offsetB
			journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Flow Sensor B Offset Fast1C Loading", offsetB, "\181L/min", "N5"))
		end
		pr.Signalize_Reset(context)
		status:RemoveStatus(baltic.Status.LoadSepColumn..msg)

		if checkForPumpBLeakage then
			CheckForPumpBLeakage("LT: Pump B Loading", pistonBPosition, pistonBMeasureTime)
		end
	end

	context:ShowComposition(true)
	------------------------------------------------------------------------	

	local totalToggleTime = 0

	local maxPress = pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, true)	-- set seperation column max pressure
	local gradient_method = zr.CreateGradient(0, maxPress, true, ovenTemp)
	---@type GradientContainer
	local gradient = context:GetArgumentValue("gradient")
	for segment in gs.dotnet_each(gradient) do
		zr.AddGradientSegment(gradient_method, segment.Time, segment.Flow, segment.Mix)
		totalToggleTime = segment.Time
	end

	------------------------------------------------------------------------	
	status:SetStatus("preparing gradient start")
	------------------------------------------------------------------------
	sfsf.strategyFastSplineFit(installed, context, gradient, pp, execAux, pump, zr, false)
	status:RemoveStatus("preparing gradient start")

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

	zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)			-- PNS-686

	-- Gradient has started
	pr.SetSignal(1, execAux) -- set SWOut1 signal for 1 second

	local switchValveIisEnabled = false			-- is needed during the gradient is running
	---@type ToggleWashContainer
	local ToggleWashContainer = {numOfSteps=0, nextStepOrganic=false, organicStepVolume=0, aqueousStepVolume=0, pumpSpeed=0, timeToNextStepStart=0, stepTime=0, lastStepVolume=0, lastStepSpeed=0, totalTime=0, onlyOneStepOrganic=false, aqueousOnly=false, toggleAqueousOrganic=false, isToggleWashEnabled=false, numOfLastAqueousSteps=0}
	ToggleWashContainer.isToggleWashEnabled = context:GetArgumentValue("Toggle Wash In Gradient")
	-- Start AS wash
	status:SetStatus("wash")
	if ToggleWashContainer.isToggleWashEnabled == true then
		if context:GetArgumentValue("Flush Injector Port") == true then
			local vol = context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Sealed") + context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Unsealed")
			totalToggleTime = totalToggleTime - (vol / context:GetArgumentValue("Flush Injector Port", "Flow Rate")) - (vol / pp.VolumetricPumpVolume)
		end
		ToggleWashContainer = pp.ToggleWashInitialization(context, ToggleWashContainer, totalToggleTime)
		pp.PrintToggleWashParameter(context, ToggleWashContainer)
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Inject)			-- to clean the loop during toggle wash
	elseif context:GetArgumentValue("Flush Injector Port") == false and context:GetArgumentValue("Disable Injector Cleaning") == false then
		context:Log("queue_clean_injector_loop")
		qaw.queue_clean_injector_loop(execLeft, execAux, pp, false, true) -- with valveI in INJECT position
		switchValveIisEnabled = true			-- true: switch valveI to load after cleaning is complete
	end
	if context:GetArgumentValue("Flush Injector Port") == true or ToggleWashContainer.isToggleWashEnabled == true then
		execLeft:LeaveObject()
		execLeft:MoveToObject(injector)
		execLeft:PenetrateWithConstForce(injector)
	end
	---------------------------------------------------------------
	status:SetStatus("elution")
	---------------------------------------------------------------
	-- wait for gradient to complete
	pf.iniFlowObserver(180)
	local flushTool = context:GetArgumentValue("Flush Tool After Toggle Wash") > 0
	local moveBackToInjector = false
	local firstFlushInjectorEnabled = context:GetArgumentValue("Flush Injector Port")
	local secondFlushInjectorEnabled = firstFlushInjectorEnabled
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
			execLeft:LeaveObject()
			execLeft:MoveToHome()
			switchValveIisEnabled = false
			status:RemoveStatus("wash")
		end
		if (ToggleWashContainer.isToggleWashEnabled) then
			if (pf.now() >= timeToWait) then
				ToggleWashContainer = pp.RunToggleWashVolumePump(execLeft, ToggleWashContainer)
				timeToWait = pf.now() + ToggleWashContainer.timeToNextStepStart
			end
		elseif flushTool then
			if (pf.now() >= timeToWait) and execLeft.IsIdle then
				context:Log("--- Running flush LCP tool procedure")
				flushTool = false
				local flushVolume = context:GetArgumentValue("Flush Tool After Toggle Wash")
				local flushToolPumpSpeed = context:GetArgumentValue("Flush Tool Pump Speed")
				timeToWait = pp.FlushLCPTool(flushVolume, flushToolPumpSpeed, execLeft, execAux)
				timeToWait = timeToWait + pf.now()
				if firstFlushInjectorEnabled or secondFlushInjectorEnabled then
					moveBackToInjector = true
				end
			end
		else
			if firstFlushInjectorEnabled or secondFlushInjectorEnabled then
				if (pf.now() >= timeToWait) and execLeft.IsIdle then
					if moveBackToInjector then
						moveBackToInjector = false					-- do it just once
						execLeft:LeaveObject()
						execLeft:MoveToObject(injector)
						execLeft:PenetrateWithConstForce(injector)
					end
					if firstFlushInjectorEnabled then
						context:Log("--- Running first flush port procedure")
						firstFlushInjectorEnabled = false			-- do it just once
						local solvent     = pp.Aqueous
						local volume      = context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Sealed")
						local speed       = context:GetArgumentValue("Flush Injector Port", "Flow Rate")
						local liftUpHight = 0
						pp.PrintFlushPortParameter(context, solvent, volume, speed, liftUpHight)
						pp.FlushPort(solvent, volume, speed, liftUpHight, execLeft, execAux)							-- flush port sealed
					end
					if secondFlushInjectorEnabled and execLeft.IsIdle then
						context:Log("--- Running second flush port procedure")
						secondFlushInjectorEnabled = false
						local solvent     = pp.Aqueous
						local volume      = context:GetArgumentValue("Flush Injector Port", "Volume Aqueous Unsealed")
						local speed       = context:GetArgumentValue("Flush Injector Port", "Flow Rate")
						local liftUpHight = context:GetArgumentValue("Flush Injector Port", "Lift-Up Distance")
						pp.PrintFlushPortParameter(context, solvent, volume, speed, liftUpHight)
						pp.FlushPort(solvent, volume, speed, liftUpHight, execLeft, execAux)							-- flush port unsealed
					end
				end
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
		code = pf.isGradientOK(context, pressSettings.GradientPumpMaxTargetPressure, 20, maxPress, pump, zr, code, separationMode)
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
	status:RemoveStatus("elution")

	-- switch pumps off, otherwise they may switch to flo mode and set the last gradient flow
	pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
	pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)

	pf.evaluatePressure(context, sumPressA, sumPressB, numOfValues)
	pcall(ci.logMaxColumnPress, context, maxMeasuredPress, flow, actComp, ovenTemp, sep.Name)

	-- wait for queued autosampler wash to finish
	while not execLeft.IsIdle do
		sleep_250()
	end

	if context:GetArgumentValue("MS Calibrant Injection") and (code == 0 or code == 2) then
		-- Calibrant Injection
		local ic = require "InjectCalibrant"
		---@type CalibrantInjectionParameter
		local qpcaui_params = {
			sample_aspirate_speed = 	pp.Quantity(context:GetArgumentValue("sample_aspirate_speed"), "uL/s"),
			sample_postaspirate_delay = pp.Quantity(context:GetArgumentValue("sample_postaspirate_delay"), "ms"),
			presample_air_volume = 		pp.Quantity(context:GetArgumentValue("presample_air_volume"), "uL"),
			calibrant_volume = 			context:GetArgumentValue("calibrant_volume"),						-- do not pp.Quantity due to calculations with this value
			postsample_air_volume = 	pp.Quantity(context:GetArgumentValue("postsample_air_volume"), "uL"),
			sample_inject_speed = 		pp.Quantity(context:GetArgumentValue("sample_inject_speed"), "uL/s"),
			flowPerBar = 				context:GetArgumentValue("separator_unityflow"),
			systemVolume =				context:GetArgumentValue("separator_volume") + baltic.SystemVolume	-- column volume + capillary volume
		}
		-- reduce pressure to not to switch valves under high pressure during calibrant injection or extended wash
		-- !!! make sure that: pump.valve==MixTee AND NOT (trapValve==Analytical OR trapValve==Trap) !!!
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 10, 5, 30, sleep_250, baltic.smooth)

		ic.injectCalibrant(installed, context, qpcaui_params, a_sE.equilibratingFlow, sleep_100)
	end

	if context:GetArgumentValue("Extended Wash") then
		local ew = require "extended_wash"
		context:Log(baltic.devider)
		context:Log("--- Extended Wash:")
		status:SetStatus("clean injector")
		pr.Signalize_Reset(context)
		error_code = ew.extended_wash(installed, context, execLeft, execAux, pp, pump, false, false, context:GetArgumentValue("Extended Wash", "Num Of Cycles"), context:GetArgumentValue("Extended Wash", "Volume Organic Per Cycle"), context:GetArgumentValue("Extended Wash", "Volume Aqueous Per Cycle"), context:GetArgumentValue("Extended Wash", "Flow Organic"), context:GetArgumentValue("Extended Wash", "Flow Aqueous"), sleep_100)
		if ( error_code.err ~= "" ) then
			error_code.message = "Autosampler clean column - "..error_code.message
--			handle_message(error_code)		-- don"t Abort() here, handle_message later
		end
		status:RemoveStatus("clean injector")
		context:Log(baltic.devider)
	end

	while not execLeft.IsIdle do
		sleep_250()
	end

	if context:GetArgumentValue("Set Equil. Press at Gradient End") then
		local pressure = context:GetArgumentValue("separator_equilibration_pressure")
		local duration = 10
		local actualPressureLimitA = pump:GetMaxPressureLimit(zr.A)

		local limitA = pf.GetMaxPressureLimitWithDelta(installed.MaxPumpPressure, pressure)

		if limitA > actualPressureLimitA then pump:SetMaxPressureLimit(zr.A, limitA) end

		context:Log("Equilibration Pressure: {0}", pressure)
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_2_1)
		zr.SetValvePosition(context, pump, zr.B, (baltic.PumpValve.MixTee + 30), true)			-- turn clockwise to block
		if context:GetArgumentValue("Extended Wash") or context:GetArgumentValue("MS Calibrant Injection") then
			context:Sleep(1000)
			zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.MixTee, nil)
			pr.SetValvePosition(execLeft, valveT, baltic.TrapValve.GradientA)
		end
		context:Signalize(baltic.ColorsRGB.Blue, N.PumpAFrontToValve, N.ValveAGroove, N.ValveAToFS, N.FlowA, N.FSAToMixTee, N.MixTeeToTrapValve, N.ValveTLongGroove, N.TransferLine, N.Separator)
		context:SignalizeText(baltic.ColorsRGB.White, N.FlowA)

		if context:GetArgumentValue("Set Press Pump B at Gradient End") == true then
			local pressureB = pressure + (pressure * context:GetArgumentValue("Pump B Starting Pressure Offset") * 0.01)
			local actualPressureLimitB = pump:GetMaxPressureLimit(zr.B)
			local limitB = pf.GetMaxPressureLimitWithDelta(installed.MaxPumpPressure, pressureB)
			if limitB > actualPressureLimitB then pump:SetMaxPressureLimit(zr.B, limitB) end
			pf.Manualmode_Pump_constantPressure(zr.B, pressureB, pump, sleep_100)
		end

		pf.Manualmode_Pump_constantPressure(zr.A, pressure, pump, sleep_100)

		-- rising the pressure to 1000 bar takes <10 seconds.
		-- Timeout = 30 seconds.
		-- If timed out a warning message will be generated
		local timeout = pf.now() + 30
		while math.abs(pump:GetCurrentPressure(zr.A) - pressure) > math.max((pressure * 0.1), 20) do
			sleep_100()
			if pf.now() > timeout then
				context:Report("Separator equilibration", Severity.Warn, true, "The equilibration pressure was not reached.\nThis can lead to worse results for the following analysis.")
				break
			end
		end
		pump:GetCurrentPressure(zr.A)
		duration = duration + pf.now()
		while pf.now() < duration do sleep_100() end
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_100)
		pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_100)
	else
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 20, 15, 30, sleep_250, baltic.smooth)
		-- wait for pump to stop completely
		pf.isPumpIdle(pump, sleep_250)
	end

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


	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)

	dictator:Dispose()

	if ( error_code.err ~= "" ) then
		handle_message()
	end
end
