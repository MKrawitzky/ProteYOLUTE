-- require('lldebugger').start()

local Date = "2025/09/10"

luanet.load_assembly("Bruker.Lc")

---@type Severity
local Severity = luanet.import_type("Bruker.Lc.Business.Severity")
---@type JournalEntry
local JournalEntry = luanet.import_type("Bruker.Lc.JournalEntry")
---@type LogTo
local LogTo = luanet.import_type("Bruker.Lc.Business.LogTo")
---@type LoggingDictator
local LoggingDictator = luanet.import_type("Bruker.Lc.Business.LoggingDictator")
---@type LedState
local LedState = luanet.import_type("Bruker.Lc.Business.LedState")

local baltic = 	require "baltic"

--===========================================================================================
---@param context InitHelper
function Initialize (context)

	context.Name		= "Maintenance"
	context.Description	= "Perform manual maintenance."
	context.Hidden		= false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.LedState = LedState.Maintenance

	context:DeclareParameter("Prepare system for storage", false, nil, "check", "To safely ship or store the instrument", "")

	context:DeclareParameter("Separator0", "", nil, "separator", "", "")

	context:DeclareParameter("Move pump pistons back", false, nil, "check", "To safely remove the pump housing", "")
	context:DeclareParameter("Move pump pistons forward", false, nil, "check", "Moving piston to exchange position", "")
	context:DeclareParameter("Set all valves to service position", false, nil, "check", "For servicing the valves", "")
	context:DeclareParameter("Move autosampler to service position", false, nil, "check", "For servicing the syringe", "")
--	context:DeclareParameter("Calibrate pressure sensors", false)

	context:DeclareParameter("Separator1", "", nil, "separator", "", "")

	context:DeclareParameter("Flush autosampler", false, nil, "check", "Flushing the autosampler", "")

	context:DeclareParameter("Separator2", "", nil, "separator", "", "")

	context:DeclareParameter("Identify bottom depth", false, nil, "check", "Identifying the vial or well-plate bottom depth", "")
	context:DeclareParameter("Tray1", true, nil, "radio", false, "Identifying the bottom depth of tray 1", "", 30, "A")
	context:DeclareParameter("Tray2", false, nil, "radio", false, "Identifying the bottom depth of tray 2", "", 30, "A")


	context:DeclareParameter("trap", nil, nil, "custom")
end

---This function is never called?
---@param _ IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate(_, context)
	local v = require "validation"

	v.verify_specified(context, "Prepare system for storage")
	v.verify_specified(context, "Move pump pistons back")
	v.verify_specified(context, "Move pump pistons forward")
	v.verify_specified(context, "Set all valves to service position")
	v.verify_specified(context, "Move autosampler to service position")

	v.verify_specified(context, "Flush autosampler")
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
	context:Log("Maintenance Lua date: {0}", Date)

	require "degas"
	---@type Zirconium
	local zr = require "zirconium"
	local pp = require "palplus"
	local pf = require "pump_functions"
	local pr = require "PreRunFunctions"
	local parallel = require "parallel"

	---@type Pump
	local pump = context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IPalParticipant
	local execLeft = context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IPalParticipant
	local execAux = context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type IJournal
	local journal = context:GetProcedureParticipant(baltic.JournalRole)
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)

	local valveI = pp.QueryValveDrive(execLeft, pp.Capabilities.ILcInjectorValve)
	local valveT = pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)

	local robothelper = execLeft.RobotHelper
	local exchangehelper = robothelper:GetSyringeExchangeHelper(execLeft.PalResourceType.LeftHead)
	exchangehelper.ManageLocking = false

	local function sleep_200()
		context:Sleep(200)
	end

	local function sleep_1000()
		context:Sleep(1000)
	end

	local function flushAutosampler()
		local ew = require "extended_wash"
--		move the LCP tool to the LCMS wash station, penetrate waste
		local wash_module = pp.QueryModule(execAux, pp.Capabilities.ILcMsWashStation)
		local statusMsg = "Flush autosampler: "
		local startTime = pf.noExp(((pf.now()/60) + 14), 1)				--[min]
		---@type string
		local msgToBeShown

		local function showStatusMsg(msgProcedure)
			local remainingTime = pf.noExp((startTime - (pf.now()/60)), 1)
			local msgRemainigTime = ": Time remaining: "..remainingTime.." min"
			msgToBeShown = statusMsg..msgProcedure..msgRemainigTime
			status:SetStatus(msgToBeShown)
		end

		execLeft:LeaveObject()
		execLeft:MoveToObject( wash_module, pp.Waste, true, true, true )
		execLeft:PenetrateObject( wash_module, pp.Waste, pp.Quantity(baltic.WashWasteLinerPenetrationDepth, "mm"), pp.Quantity(baltic.SyringePenetrationSpeed, "mm/s"))
--		dispense 10mL solv 2 at 50uL/s
		pp.VolumeToBePumped(execLeft, pp.Organic, 10000, 50)
		while not execLeft.IsIdle do
			showStatusMsg("Pumping organic")
			sleep_1000()
			status:RemoveStatus(msgToBeShown)
		end
--		dispense 10mL solv 1 at 50uL/s
		pp.VolumeToBePumped(execLeft, pp.Aqueous, 10000, 50)
		while not execLeft.IsIdle do
			showStatusMsg("Pumping aqueous")
			sleep_1000()
			status:RemoveStatus(msgToBeShown)
		end
		execLeft:LeaveObject()
--		run extended wash
		showStatusMsg("Running extended wash procedure")
		ew.extended_wash(installed, context, execLeft, execAux, pp, pump, true, true, 1, 10, 40, 10, 10, sleep_200)
		status:RemoveStatus(msgToBeShown)
		while not execLeft.IsIdle do
			showStatusMsg("Running extended wash procedure")
			sleep_1000()
			status:RemoveStatus(msgToBeShown)
		end
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_200)
		showStatusMsg("Priming the syringe")
		execLeft:LeaveObject()
--		Perform PrimeSyringeWithVolumePump
		pp.PrimeSyringeWithVolumePump(context, execLeft, 1, sleep_200, false)
		status:RemoveStatus(msgToBeShown)
		while not execLeft.IsIdle do
			showStatusMsg("Running extended wash procedure")
			sleep_1000()
			status:RemoveStatus(msgToBeShown)
		end
		showStatusMsg("Priming the injector")
--		Perform PrimeInjectorWithVOlumePump
		pp.PrimeInjectorWithVolumePump(context, execLeft, sleep_200, false)
		status:RemoveStatus(msgToBeShown)
	end

	---Move the piston back
	---@param channel Channel
	---@param yield_function function
	local function moveBackPiston(channel, yield_function)
		if zr.IsFull(pump, channel) then return end
		zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Solvent, nil)
		-- move piston back with full speed
		pf.Manualmode_Pump_constantSpeed(channel, -baltic.Settings.GradientPumpPurgeSpeed, pump, yield_function)
		while not zr.IsFull(pump, channel) do yield_function() end
		zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Compress, nil)
	end

	---Purge the channel
	---@param channel Channel
	---@param yield_function function
	local function purge_channel(channel, yield_function)
		if zr.IsEmpty(pump, channel) then return end
		zr.SetValvePosition(context, pump, channel, baltic.PumpValve.Waste, nil)
		pf.Manualmode_Pump_constantSpeed(channel, baltic.Settings.GradientPumpPurgeSpeed, pump, yield_function)
		while not zr.IsEmpty(pump, channel) do yield_function() end
	end

	---Switching all valves to service position
	local function setValvesToService()
		pr.SetValvePosition(execLeft, valveI, baltic.InjectionValve.Service)
		pr.SetValvePosition(execLeft, valveT, baltic.TrapValve.Service)
		zr.SetValvePosition(context, pump, zr.A, baltic.PumpValve.Service)
		zr.SetValvePosition(context, pump, zr.B, baltic.PumpValve.Service)
	end

	---Function to set one or more valves
	---@param a_angle number|nil
	---@param b_angle number|nil
	---@param i_angle number|nil
	---@param t_angle number|nil
	local function set_valves(a_angle, b_angle, i_angle, t_angle) -- concurrently sets (optionally) all 4 valves.
		if (i_angle) then
			pr.SetValvePosition(execAux, valveI, i_angle)
		end
		if (t_angle) then
			pr.SetValvePosition(execAux, valveT, t_angle)
		end
		local tasks = {}
		if (a_angle) then
			tasks[#tasks+1] = { zr.SetValvePosition, context, pump, zr.A, a_angle, nil }
		end
		if (b_angle) then
			tasks[#tasks+1] = { zr.SetValvePosition, context, pump, zr.B, b_angle, nil }
		end
		parallel.run(sleep_200, table.unpack(tasks))
--		while (not execLeft.IsIdle) do sleep_200() end
	end

	---Run a preparation with solvent exchange
	---@param numOfSolventExchange number
	---@param repeatIfAirDetected boolean
	---@param cnt number
	---@param degasserStartTime number
	---@return number
	---@return boolean
	---@return number
	---@return boolean
	local function preparePumps(numOfSolventExchange, repeatIfAirDetected, cnt, degasserStartTime)
		local prepAMsg = "Preparation A "
		local prepBMsg = "Preparation B "
		local msg1 = "preparation with solvent exchange and priming the volumetric wash pumps"
		local msgStart = "Start "..msg1
		local aspirationSpeed = -750		-- PNS-626
		local dispenseSpeed = 4000			-- PNS-626
		local degasserOnTime = pf.now() - degasserStartTime

		---Log the results to the journal
		---@param valA number
		---@param passedA boolean
		---@param valB number
		---@param passedB boolean
		local function reportPreparationValues(valA, passedA, valB, passedB)
			msg1 = "--- System preparation for storage"
			-- if 'preparePumps()' runs in a loop then disable the variable above and enable the variable below
			-- msg1 = "--- System preparation for storage " .. tostring(cnt)
			local msgA = prepAMsg
			local msgB = prepBMsg
			-- if 'preparePumps()' runs in a loop then disable the variables above and enable the variables below
			-- local msgA = prepAMsg .. tostring(cnt)
			-- local msgB = prepBMsg .. tostring(cnt)
			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, msg1, os.date("%c")))
			if passedA == true then
				journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, msgA, valA))
			end
			if passedB == true then
				journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, msgB, valB))
			end
		end

		status:SetStatus("Preparing pump A + B")
		context:Log(msgStart)
		local p_degas_A = { degas_Channel, context, zr, zr.A, numOfSolventExchange, repeatIfAirDetected, -1, aspirationSpeed, dispenseSpeed, degasserOnTime, parallel.yield }
		local p_degas_B = { degas_Channel, context, zr, zr.B, numOfSolventExchange, repeatIfAirDetected, -1, aspirationSpeed, dispenseSpeed, degasserOnTime, parallel.yield }
		local a, _, passedA, b, _, passedB = parallel.run(sleep_200, p_degas_A, p_degas_B)
		-- repetition is always zero 

		local msgEnd = "End "..msg1
		context:Log(msgEnd)
		reportPreparationValues(a, passedA, b, passedB)
		status:RemoveStatus("Preparing pump A + B")
		return a, passedA, b, passedB
	end

	---Run direct flow with/without the trap column
	---@param withTrap boolean
	---@param flow number [µL/min]
	---@param composition number [%]
	---@param duration number [minutes]
	local function directFlow(withTrap, flow, composition, duration)
		local runTime = duration*60+pf.now()		-- [sec]
		local msg = "Running direct flow with"
		local msg1 = "out"
		local msg2 = " trap"
		if withTrap == false then msg = msg..msg1 end
		msg = msg..msg2
		local function simpleDirectFlow()
			status:SetStatus(msg)
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 300, sleep_200, false)
			zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)
			local trapPosition = baltic.TrapValve.GradientA
			local flowA = flow * composition / 100
			local flowB = flow - flowA
			if withTrap == true then trapPosition = baltic.TrapValve.GradientT end
			set_valves( baltic.PumpValve.MixTee, baltic.PumpValve.MixTee, baltic.InjectionValve.Load, trapPosition )
			context:Signalize(baltic.ColorsRGB.LightGray, baltic.Naming.Separator, baltic.Naming.Separator)
			sleep_200()
			pf.Manualmode_Pump_constantFlow_binary( context, flowA, flowB, pump, zr, sleep_200 )
			while pf.now() < runTime do
				sleep_1000()
				if zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) then break end
			end
			status:RemoveStatus(msg)
			pr.Signalize_Reset(context)
		end

		-- run direct flow until the user aborts the procedure
		while pf.now() < runTime do
			simpleDirectFlow()
			if pf.now() < runTime then
				degas(context, 1, -1, -1, 0, sleep_1000)
			end
		end
	end

	---Returns the average vial depth
	local function determineBottomDepth()
		--- Returns the depth of the selected vial
		---@param target IModuleReference	[vial.Tray]
		---@param index integer				[vial index]
		---@return number, number
		local function getPlateDepth(target, index)
			local leaveDrawerOpen = true
			local averageDepth, deviation, vialHead = 0, 0, 0
			local depth = {}

			for n=1, 3 do
				execLeft:MoveToObject( target, index, true, true, true )
				if execLeft:DetectObject(target, index) == false then return 0,0 end
				while not execLeft.IsIdle do context:Sleep(200) end
				vialHead = math.floor(execLeft:GetAxisPositionSync(3).ReturnValue.Value * 100000) / 100	--[mm]
				execLeft:PenetrateWithBottomSense( target, index, pp.Quantity(0, "mm"), nil)
				while not execLeft.IsIdle do context:Sleep(200) end
				depth[n] = (math.floor(execLeft:GetAxisPositionSync(3).ReturnValue.Value * 100000) / 100) - vialHead	--[mm]
				averageDepth = averageDepth + depth[n]
				execLeft:LeaveObject(nil, leaveDrawerOpen)
			end

			averageDepth = averageDepth / 3
			for i=1, 3 do
				deviation = math.max(deviation, math.abs(averageDepth - depth[i]))
			end

			return averageDepth, deviation
		end

		local depth = {}
		local deviation = {}
		local averageDepth = 0
		local averageDeviation = 0
		local tray = "Slot1"
		local trayName = installed.Tray1Type
		local trayNr = 1
		local positionName = " Vial "
		if (trayName ~= "VT54") and (trayName ~= "MTP96") and (trayName ~= "MTP384") then
			local msg = " Tray type "..trayName.." is not supported"
			context:Report("Unsupported Tray", Severity.Info, true, msg)
			context:Abort()
		end
		local vialPosition = {
			VT54 = {1,9,46,54},
			MTP96 = {1,12,85,96},
			MTP386 = {1,24,361,384}
		}
		local testPosition = vialPosition.VT54
		if (context:GetArgumentValue("Tray1") == false) then
			tray = "Slot2"
			trayName = installed.Tray2Type
			trayNr = 2
		end
		local container = pp.QueryModule(execAux, pp.Capabilities.TrayContainer)
		local vial = execLeft.ConfigurationService:GetVial(container.Name..":"..tray..":1")
		if (trayName == "MTP96") then
			testPosition = vialPosition.MTP96
			positionName = " Well "
		elseif (trayName == "MTP384") then
			testPosition = vialPosition.MTP386
			positionName = " Well "
		end

		local msg = "Identifying the "..trayName.." bottom depth"
		status:SetStatus(msg)
		execLeft:LeaveObject()
		for i=1, 4 do
			local positionMsg = "@ Tray"..trayNr..positionName..tostring(testPosition[i])
			status:SetStatus(positionMsg)
			depth[i], deviation[i] = getPlateDepth(vial.Tray, testPosition[i])
			depth[i] = math.floor(depth[i]*100)/100
			deviation[i] = math.floor(deviation[i]*1000)/1000
			averageDepth = averageDepth + depth[i]
			status:RemoveStatus(positionMsg)
		end
		execLeft:LeaveObject(nil, false)	-- close drawer

		averageDepth = averageDepth / 4
		local roundedAverageDepth = string.format("%.2f", averageDepth)
		local msg2 = "Average depth @ Tray "..trayNr..": "..roundedAverageDepth.." mm"

		if installed.IsService == false then
			context:Report(msg, Severity.Info, true, msg2)
		else
			for i=1, 4 do
				averageDeviation = math.max(averageDeviation, math.abs(averageDepth - depth[i]))
			end
			local roundedAverageDeviation = string.format("%.2f", averageDeviation)
			local msg3 = "The average deviation  @ Tray "..trayNr..": "..roundedAverageDeviation.." mm\n"
			local valueToBeShown = msg2.."\n"..msg3.."@ Tray"..trayNr..positionName..tostring(testPosition[1])..": "..depth[1].." mm, deviation: "..tostring(deviation[1]).."mm\n".."@ Tray"..trayNr..positionName..tostring(testPosition[2])..": "..depth[2].." mm, deviation: "..tostring(deviation[2]).."mm\n".."@ Tray"..trayNr..positionName..tostring(testPosition[3])..": "..depth[3].." mm, deviation: "..tostring(deviation[3]).."mm\n".."@ Tray"..trayNr..positionName..tostring(testPosition[4])..": "..depth[4].." mm, deviation: "..tostring(deviation[4]).."mm"
			context:Report(msg, Severity.Info, true, valueToBeShown)
		end
		status:RemoveStatus(msg)
		execLeft:MoveToHome()
	end

	zr.resetValveABShiftCounterPosition()
	zr.logValveABShiftCounterPosition(context, pump)
	zr.storePumpVolume(pump, true)

	--	Signalize_Reset
	pr.Signalize_Reset(context)

	local fR = require "PreRunFunctions"
	fR.iniFlowResistance(context, pump)

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

	local dictatorStatus = LoggingDictator.Prevent(status)
	if installed.IsExtendedLoggingEnabled then dictatorStatus:Dispose() end

	local settings = pump:GetSettings()
	zr.logInstrSettings(context, settings, "Maintenance")

	-- log procedure parameters to twinscape
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Prepare system for storage", context:GetArgumentValue("Prepare system for storage")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flush autosampler", false)) -- will be overwritten with true if actually performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Move pump pistons back", false)) -- will be overwritten with true if actually performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Move pump pistons forward", false)) -- will be overwritten with true if actually performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set all valves to service position", false)) -- will be overwritten with true if actually performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Move autosampler to service position", false)) -- will be overwritten with true if actually performed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Identify bottom depth", context:GetArgumentValue("Identify bottom depth")))

	if context:GetArgumentValue("Identify bottom depth") == true then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Tray1", context:GetArgumentValue("Tray1")))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Tray2", context:GetArgumentValue("Tray2")))
	end

	if context:GetArgumentValue("Prepare system for storage") == true then
		-- Ask user to confirm that isopropanol has been placed on all solvent bottles, and that the column is disconnected.
		local msg = "Prepare system for storage"
		context:Report(msg, Severity.Info, true, "Confirm the bottles are filled with Isopropanol and the separation column is disconnected. Click 'Confirm' to continue.")
		context:WaitForSignal(msg)

		-- move the LCP tool to the LCMS wash station, penetrate waste, dispense 10mL solv 2 and then 10mL solv1, both at 50uL/s
		-- degasser on
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
		local degasserStartTime = pf.now()
		status:SetStatus("Priming volumetric wash pump")
		execLeft:LeaveObject()
		-- move the LCP tool to the LCMS wash station,
		-- penetrate waste,
		-- dispense 10mL solv 2 and then 10mL solv1, both at 50uL/s
		pp.PrimeVolumetricPump( execLeft, 10000, 50)
		execLeft:LeaveObject()
		execLeft:MoveToHome()

		-- Run 1 cycle of preparation+solvent replacement, and record the preparation values.
		-- 19 minutes
		local cnt = 1
		preparePumps(10, false, cnt, degasserStartTime)
		status:RemoveStatus("Priming volumetric wash pump")

		-- Run direct flow w/o trap for 10 min (50%B, 3uL/min)
		-- 10 minutes
		directFlow(false, 3, 50, 10)			-- with trap, flow, composition, duration

		-- Run direct flow w trap (even if there is no trap installed) for 5 min (50%B, 3uL/min)
		-- 5 minutes
		directFlow(true, 3, 50, 5)			-- with trap, flow, composition, duration

		-- Set piston position to fully extended
		local pA = { purge_channel, zr.A, parallel.yield }
		local pB = { purge_channel, zr.B, parallel.yield }
		parallel.run(sleep_200, pA, pB)

		-- Set the pump valve position to compress
		-- Set autosampler valves to service position
		set_valves(baltic.PumpValve.Compress, baltic.PumpValve.Compress, baltic.InjectionValve.Load, baltic.TrapValve.GradientA)

		-- Ask the user to plug the transfer line
		context:Report("Plug", Severity.Info, true, "Please plug the transfer capillary. Then click 'Confirm' to finish.")
		context:WaitForSignal("Plug")
	else
		if context:GetArgumentValue("Flush autosampler") == true then
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flush autosampler", true))
			flushAutosampler()
		else
			local text = "When manual service is finalized click 'Confirm'."
			context:ShowComposition(false)

			pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 20, sleep_200, baltic.smooth)

			if context:GetArgumentValue("Move pump pistons back") then
				pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
				context:Sleep(10*1000)
				local pA = { moveBackPiston, zr.A, parallel.yield }
				local pB = { moveBackPiston, zr.B, parallel.yield }
				parallel.run(sleep_200, pA, pB)
				pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Move pump pistons back", true))
				if context:GetArgumentValue("Move pump pistons forward") then
					context:Report("Service position", Severity.Info, true, "When pump heads are dismounted click 'Confirm' to move pistons out.")
					context:WaitForSignal("Service position")
				end
			end

			if context:GetArgumentValue("Set all valves to service position") then
				-- valves to service position
				setValvesToService()
				journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Last Set All Valves To Service Position", os.date("%c")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set all valves to service position", true))
			end

			if context:GetArgumentValue("Move pump pistons forward") then
				-- workaround for catching an abort during use of SyringeExchangeHelper (the Home below is mandatory until Reset is able to handle it).
				local function protected()
					if context:GetArgumentValue("Move autosampler to service position") then
						exchangehelper:MoveToExchangePosition()
					end
					-- protrude pistons
					local a = { purge_channel, zr.A, parallel.yield }
					local b = { purge_channel, zr.B, parallel.yield }
					parallel.run(sleep_200, a, b)

					-- valves to service position
					setValvesToService()

					context:Report("Piston exchange", Severity.Info, true, "When pistons are exchanged click 'Confirm' to continue with reinstalling pump heads.")
					context:WaitForSignal("Piston exchange")
				end
				pcall(protected)
				exchangehelper:Home()

				pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
				context:Sleep(10*1000)
				local a = { moveBackPiston, zr.A, parallel.yield }
				local b = { moveBackPiston, zr.B, parallel.yield }
				parallel.run(sleep_200, a, b)
				pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Move pump pistons forward", true))
				journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Last Move Pump Pistons Forward", os.date("%c")))
				text = "When pump heads are reinstalled click 'Confirm'. Run preparation to fill and degas the pumps."
			end

			if context:GetArgumentValue("Move autosampler to service position") and not context:GetArgumentValue("Move pump pistons forward") then
				-- workaround for catching an abort during use of SyringeExchangeHelper (the Home below is mandatory until Reset is able to handle it).
				local function protected()
					exchangehelper:MoveToExchangePosition()
					journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Last Move Autosampler To Service Position", os.date("%c")))
					journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Move autosampler to service position", true))
					context:Report("Move robot", Severity.Info, true, "After manual service click 'Confirm' to continue. Be aware that the robot arm will move to home position.")
					context:WaitForSignal("Move robot")
				end
				pcall(protected)
				exchangehelper:Home()
			end

			if context:GetArgumentValue("Move pump pistons forward") or context:GetArgumentValue("Set all valves to service position") or context:GetArgumentValue("Move pump pistons back") then
				context:Report("Maintenance", Severity.Info, true, text)
				context:WaitForSignal("Maintenance")
			end
		end
	end

	if context:GetArgumentValue("Identify bottom depth") then
		determineBottomDepth()
	end

	-- wait for participants to stop
	pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_200)
	pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_200)
	pf.isPumpIdle(pump, sleep_200)

	context:ShowComposition(true)

	while not execLeft.IsIdle do
		sleep_200()
	end

	dictator:Dispose()
	dictatorStatus:Dispose()

	zr.logValveABShiftCounterPosition(context, pump)
	zr.logPumpVolume(context, pump)
end
