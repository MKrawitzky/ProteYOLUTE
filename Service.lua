-- require('lldebugger').start()

local Date = "2025/11/17"

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
	context.Name		= "Service"
	context.Description	= "Procedures for servicing the LC system."
	context.Hidden		= false
	context.DecompressOnExit = true
	context.OverwriteLogFiles = false
	context.IsService = true
	context.LedState = LedState.Service

	context:DeclareParameter("Initialize system", false, nil, "check", false, "To initialize the system after installation or maintenance", "")

	context:DeclareParameter("Separator0", "", nil, "separator", "", "")

	context:DeclareParameter("Conditioning system", false, nil, "check", false, "To automatically condition the system", "")

	context:DeclareParameter("Separator1", "", nil, "separator", "", "")

	context:DeclareParameter("Flush system", false, nil, "check", "To flush the system, one cycle takes 6 hours", "")
	context:DeclareParameter("Flush autosampler", false, nil, "check", "To flush the autosampler, one cycle takes 6 hours", "")
	context:DeclareParameter("Prime volumetric wash pumps", false, nil, "check", "To prime the volumetric wash pumps, one cycle take 2:10 minutes", "")
	context:DeclareParameter("Cycles", 1, "cycle(s)", "integer", false, "", "", 30)

	context:DeclareParameter("Separator2", "", nil, "separator", "", "")

	context:DeclareParameter("Remove air from pumps", false, nil, "check", false, "To remove air from the pumps. Won't run if \"Initialize system\" or \"Conditioning system\" is selected.", "DirectFlowToWaste.PNG")

	context:DeclareParameter("Separator3", "", nil, "separator", "", "")

	context:DeclareParameter("Flow mode", false, nil, "check", false, "Apply a constant flow with the selected flow mode. Won't run if \"Remove air from pumps\" is selected.", "")

	context:DeclareParameter("Constant pressure", true, nil, "radio", false, "Apply a constant pressure for a given time on current path or path defined by \"Set valve positions\"", "", 30, "A")
	context:DeclareParameter("Pressure", 100, "bar", "integer", false, "Constant pressure value", "", 60)
	context:DeclareParameter("Flow mode time", 10, "minutes", "decimal", false, "Duration of constant pressure", "", 60, "", 1)

	context:DeclareParameter("Constant speed", false, nil, "radio", false, "Apply flow with constant piston speed on current path or path defined by \"Set valve positions\"", "", 30, "A")
	context:DeclareParameter("Piston speed", 100, "\181L / min", "decimal", false, "Constant speed value", "", 60, "", 3)
	context:DeclareParameter("Dispense volume", 100, "\181L", "integer", false, "Volume to dispense", "", 60)

	context:DeclareParameter("Set Valve Positions", false, "", "check", false, "Switch valve angles. Won't run if \"Initialize system\" or \"Conditioning system\" is selected.", "")
	context:DeclareParameter("Valve A", 180, "deg", "integer", false, "Pump A valve angle", "pump_valve.png")
	context:DeclareParameter("Valve B", 180, "deg", "integer", false, "Pump B valve angle", "pump_valve.png")
	context:DeclareParameter("Valve I", 60, "deg", "integer", false, "Injection valve angle", "injection_valve.png")
	context:DeclareParameter("Valve T", 0, "deg", "integer", false, "Trap valve angle", "trap_valve.png")

	context:DeclareParameter("Separator5", "", nil, "separator", "", "")

	local fullEmptyToolTip = "Move the piston, " ..baltic.MaxPumpVolume.. " is full, 0 is empty. Won't run if \"Initialize system\", \"Conditioning system\", \"Remove air from pumps\" or \"Flow mode\" is selected."
	context:DeclareParameter("Set Piston Positions", false, "", "check", false, fullEmptyToolTip, "")
	context:DeclareParameter("Pump A", baltic.MaxPumpVolume, "\181L", "integer", false, "", "")
	context:DeclareParameter("Pump B", baltic.MaxPumpVolume, "\181L", "integer", false, "", "")

	context:DeclareParameter("Separator6", "", nil, "separator", "", "")
	--> Service parameter
	context:DeclareParameter("Disable limits for leakage detection", false, nil, "check", true)
	--< Service parameter

	context:DeclareParameter("separator", nil, nil, "custom")
	context:DeclareParameter("trap", nil, nil, "custom")
end

---This function is never called?
---@param installed IInstalledHardwareContext
---@param context IProcedureValidationContext
function Validate(installed, context)
	local pf = require "pump_functions"
	local v  = require "validation"

	local function validateInjectionValveAngle()
		local value = context:GetArgumentValue("Valve I")
		if (value ~= 0) and (value ~= 30) and (value ~= 60) then 
			local msg = "Value must be 0, 30 or 60 degrees"
			context:Report("Valve I", Severity.Error, true, msg)
		end
	end

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)

	v.verify_specified(context, "Flush system")
	v.verify_specified(context, "Prime volumetric wash pumps")
	v.verify_specified(context, "Flush autosampler")
	v.verify_specified(context, "Remove air from pumps")
	v.verify_specified(context, "Flow mode")
	v.verify_specified(context, "Constant pressure")
	v.verify_specified(context, "Constant speed")
	v.verify_specified(context, "Initialize system")
	v.verify_specified(context, "Conditioning system")
	v.verify_specified(context, "Set Valve Positions")
	v.verify_specified(context, "Set Piston Positions")

	if (context:GetArgumentValue("Flush system") == true) or (context:GetArgumentValue("Flush autosampler") == true) or (context:GetArgumentValue("Prime volumetric wash pumps") == true) then
		v.verify_range(context, "Cycles", 1, 8)
	end

	v.verify_range(context, "Pressure", 0, pressSettings.GradientPumpMaxTargetPressure)
	v.verify_range(context, "Flow mode time", 0.1, 6000)

	v.verify_range(context, "Piston speed", 0.001, 5000)
	v.verify_range(context, "Dispense volume", 1, baltic.MaxPumpVolume)

	v.verify_range(context, "Valve A", 0, 359)
	v.verify_range(context, "Valve B", 0, 359)
	validateInjectionValveAngle()
	v.verify_range(context, "Valve T", 0, 359)

	v.verify_range(context, "Pump A", 0, baltic.MaxPumpVolume)
	v.verify_range(context, "Pump B", 0, baltic.MaxPumpVolume)
end

---@param installed IInstalledHardwareContext
---@param context IProcedureExecutionContext
function Main (installed, context)
	context:Log("Service Lua date: {0}", Date)

	luanet.load_assembly("PalPlusDriver")
	luanet.load_assembly("PalPlusDriverObjects")

	require "degas"

	local chrom 	= require "chromatography"
	local diag 		= require "Diagnostics"
	local ew  		= require "extended_wash"
	local parallel  = require "parallel"
	local pf 		= require "pump_functions"
	local csv		= require "csv_file_logging"
	---@type PalPlus
	local pp 		= require "palplus"
	local pr 		= require "PreRunFunctions"
	---@type Zirconium
    local zr 		= require "zirconium"

	---@type IPalParticipant
	local execAux	= context:GetProcedureParticipant(baltic.AuxExecutorRole)
	---@type IPalParticipant
	local execLeft	= context:GetProcedureParticipant(baltic.LeftExecutorRole)
	---@type IJournal
	local journal	= context:GetProcedureParticipant(baltic.JournalRole)
	---@type Pump
	local pump		= context:GetProcedureParticipant(baltic.GradientPumpRole)
	local dictator = LoggingDictator.Prevent(pump)
	if installed.IsExtendedLoggingEnabled then dictator:Dispose() end
	---@type IProcedureStatusParticipant
	local status = context:GetProcedureParticipant(baltic.LcStatusRole)
	local valveI	= pp.QueryValveDrive(execAux, pp.Capabilities.ILcInjectorValve)
	local valveT	= pp.QueryValveDrive(execAux, pp.Capabilities.ISelectorValve)

	local pressSettings = pf.getPressureSensorSettings(installed.MaxPumpPressure)
	local maxTestPressure = pressSettings.GradientPumpMaxTargetPressure
	---@type ErrorCode
	local error_code = {err = "", message = ""}

	---@type SelfTest
	local selfDiagnose = { 	showMessage = false,
							isSelfTest = true,
							isService = true,
							self_LT_HP = false,
							self_LT_LP_AB = false,
							self_LT_HP_AB = false,
							self_LT_HP_IS_RT = false,
							self_LT_P_RT = false,
							self_FRT_B_MT = false,
							self_FRT_IS = false,
							self_FRT_A_MT = false,
							self_Prepare_and_Flush = false }

	local csvFileName = "Service_Results"

	local function sleep_50()
		context:Sleep(50)
	end

	local function sleep_200()
		context:Sleep(200)
	end

	local function sleep_1000()
		context:Sleep(1000)
	end

	local function clearJournal()
		local n = 1
		local msgIP = "--- Initial Preparation "
		local msgP= "Preparation"
		local msgA = " A "
		local msgB = " B "
		local msgn = tostring(n)
		local exist = true
		while exist do
			exist = false
			local msgI = msgIP..msgn
			local msgesA = msgP..msgA..msgn
			local msgesB = msgP..msgB..msgn
			if (installed:Contains(msgI)) then
				journal:Delete(msgI)
				exist = true
			end
			if (installed:Contains(msgesA)) then
				journal:Delete(msgesA)
			end
			if (installed:Contains(msgesB)) then
				journal:Delete(msgesB)
			end
			context:Sleep(250)
			n = n + 1
			msgn = tostring(n)
		end
	end


	---Function to set one or more valves, concurrently sets (optionally) all 4 valves
	---@param a_angle number|nil
	---@param b_angle number|nil
	---@param i_angle number|nil
	---@param t_angle number|nil
	local function set_valves(a_angle, b_angle, i_angle, t_angle)
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
	end

	---Switch valves to the desired positions
	---Reduce pressure before switching the valves
	---@param a_angle number?
	---@param b_angle number?
	---@param i_angle number?
	---@param t_angle number?
	local function setValvePositions(a_angle, b_angle, i_angle, t_angle)
		local isValveSwitch = (a_angle and a_angle ~= pump:GetSetManualValvePosition(zr.A)) or
		(b_angle and b_angle ~= pump:GetSetManualValvePosition(zr.B)) or
		(t_angle and t_angle ~= pp.GetTrapValvePosition(execAux)) or
		(i_angle and i_angle ~= pp.GetInjectorValvePosition(execAux))
		sleep_200()

		if (isValveSwitch == true) then
			context:Log("Setting valve positions")
			local actualPressureA = pump.GetCurrentPressure(pump, zr.A)
			local actualPressureB = pump.GetCurrentPressure(pump, zr.B)
			if (actualPressureA > 50) or (actualPressureB > 50)  then
				pf.reducePressure(context, pump, zr, zr.A, zr.B, 45, 45, 300, sleep_200, baltic.smooth)
			end
			set_valves(a_angle, b_angle, i_angle, t_angle)
			context:Sleep(1000)
			pr.Signalize_Reset(context)
		end
	end

	local function setPistonPositions()
		local actPistonA = pump:GetPistonPosition(zr.A)
		local actPistonB = pump:GetPistonPosition(zr.B)
		local desiredPistonA = baltic.MaxPumpVolume - context:GetArgumentValue("Pump A")
		local desiredPistonB = baltic.MaxPumpVolume - context:GetArgumentValue("Pump B")
		context:Log("Piston position A: {0}", pump:GetPistonPosition(zr.A))
		context:Log("Piston position B: {0}", pump:GetPistonPosition(zr.B))
		context:Log("Desired position A: {0}", context:GetArgumentValue("Pump A"))
		context:Log("Desired position B: {0}", context:GetArgumentValue("Pump B"))

		local function movePiston(ch, pos, yield_function)
			local actPiston = pump:GetPistonPosition(ch)
			local speed = baltic.Settings.GradientPumpRefillSpeed
			local delta = 20
			local moveToWaste = (pos - actPiston) > 0

			if (pos > baltic.MaxPumpVolume - 5) or (pos < 5) then delta = 0 end
			if moveToWaste then
				speed = baltic.Settings.GradientPumpPurgeSpeed
				zr.SetValvePosition(context, pump, ch, baltic.PumpValve.Waste, nil)
			elseif (pos - actPiston) < 0 then
				zr.SetValvePosition(context, pump, ch, baltic.PumpValve.Solvent, nil)
			end
			-- move piston back with full speed
			pf.Manualmode_Pump_constantSpeed(ch, speed, pump, yield_function)
			parallel.sleep(yield_function, 1000)
			while true do
				actPiston = pump:GetPistonPosition(ch)
				if moveToWaste == true then
					if ((delta > 10) and ((pos-actPiston) < delta)) then
						pf.Manualmode_Pump_constantSpeed(ch, (speed/10), pump, yield_function)
						delta = delta / 20
					end
					if (pos-actPiston) < delta then break end
				else
					if ((delta > 10) and ((actPiston-pos) < delta)) then
						pf.Manualmode_Pump_constantSpeed(ch, (speed/5), pump, yield_function)
						delta = delta / 20
					end
					if (actPiston-pos) < delta then break end
				end
				if (zr.IsFull(pump, ch) or zr.IsEmpty(pump, ch)) then break end
				yield_function()
			end
			pf.Manualmode_Pump_constantSpeed(ch, 0, pump, yield_function)
		end

		local waitForDegasser = 0
		if (desiredPistonA == baltic.PumpValve.Solvent) or (desiredPistonA == baltic.PumpValve.Solvent) then
			if pump:GetDigitalOutputs() < 32  then	-- 32 == DigitalOutput.PO2 is already true
				pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
				waitForDegasser = pf.now() + 30		-- wait time for the degasser if a desired valve position is to solvent
			end
		end
		if ((actPistonA ~= baltic.PumpValve.Waste) and (actPistonA ~= baltic.PumpValve.Solvent)) or ((actPistonB ~= baltic.PumpValve.Waste) and (actPistonB ~= baltic.PumpValve.Solvent)) then
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 60, sleep_50, false)
		end
		if waitForDegasser > pf.now() then
			waitForDegasser = waitForDegasser - pf.now()
			context:Sleep(waitForDegasser*1000)
		end

		local p_piston_A = { movePiston, zr.A, desiredPistonA, parallel.yield }
		local p_piston_B = { movePiston, zr.B, desiredPistonB, parallel.yield }
		parallel.run(sleep_50, p_piston_A, p_piston_B)
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
	end

	---Build pressure on both pumps
	---@param pA number [bar]
	---@param pB number [bar]
	---@param yield_function function
	---@return boolean
	local function build_pressureAB(pA, pB, yield_function)
		if zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) then return false end
		local bPTime = pf.now() + 120
		context:Log("build_pressure AB")
		pf.Manualmode_Pump_constantPressure(zr.A, pA, pump, yield_function)
		pf.Manualmode_Pump_constantPressure(zr.B, pB, pump, yield_function)
		while not zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) do
			yield_function()
			if ((math.abs(pump:GetCurrentPressure(zr.A) - pA) <= math.max((pA*0.02), 5)) and (math.abs(pump:GetCurrentPressure(zr.B) - pB) <= math.max((pB*0.02), 5)) or (pf.now() > bPTime)) then
				break
			end
		end
		if zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) then
			return false
		else
			return true
		end
	end

	local function stopPumpsAB()
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_200)
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_200)
	end
	---Setting a direct flow pressure driven
	local function manualFlowMode()
		local toWaste = context:GetArgumentValue("Remove air from pumps")
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Remove air from pumps", toWaste))

		local flowModePressure = 900
		local flowModeTime = 20
		if toWaste == false then
			flowModePressure = context:GetArgumentValue("Pressure")		-- [bar]
			flowModeTime = context:GetArgumentValue("Flow mode time")		-- [minutes]
		end	
		local endTime = flowModeTime*60 + pf.now()

		---Get valve position to potentially reset to their position if they are shifted during degassing and see if columns are included in path.
		local a_angle = pump:GetSetManualValvePosition(zr.A)
		local b_angle = pump:GetSetManualValvePosition(zr.B)
		local t_angle = pp.GetTrapValvePosition(execAux)
		local i_angle = pp.GetInjectorValvePosition(execAux)

		zr.ChangePressurePID(context, pump, baltic.PPID.PID_20_6_1)
		local msg = "Running direct flow."
		if (toWaste == true) then
			msg = "Removing air from pumps."
			setValvePositions(baltic.PumpValve.MixTee, baltic.PumpValve.MixTee, nil, baltic.TrapValve.Waste)
		else		-- let the valves as they are
			context:SetDiagramLoggingEnabled(true) 
		end
		msg = msg.." This finishes at about "..tostring(flowModeTime).." minutes (+ potential refilling time)"
		status:SetStatus(msg)

		pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false)

		pf.Manualmode_Pump_constantPressure(zr.A, flowModePressure, pump, sleep_200)
		pf.Manualmode_Pump_constantPressure(zr.B, flowModePressure, pump, sleep_200)

		local currentVolA = baltic.MaxPumpVolume
		local currentVolB = baltic.MaxPumpVolume
		local pistonPositionThreshold = 0.1
		while pf.now() < endTime do
			---Check if pump heads are empty. If so refill, add required time to end time and apply constant pressure again.
			currentVolA = baltic.MaxPumpVolume - pump:GetPistonPosition(zr.A)
			currentVolB = baltic.MaxPumpVolume - pump:GetPistonPosition(zr.B)
			if currentVolA <= pistonPositionThreshold or currentVolB <= pistonPositionThreshold then
				---stop pumps
				pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_200)
				pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_200)
				---Take time and reduce pressure before shiftig valve in refillAB
				local timeBeforeRefill = pf.now()
				pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_200, false)
				---refill
				pf.refillAB(context, pump, zr, 0, 0)
				---Reset valve positions after refilling
				if toWaste == true then
					setValvePositions(baltic.PumpValve.MixTee, baltic.PumpValve.MixTee, nil, baltic.TrapValve.Waste)
				else
					setValvePositions(a_angle, b_angle, i_angle, t_angle)
				end
				local timeTakenForRefill = pf.now() - timeBeforeRefill
				---Add time to endTime
				endTime = endTime + timeTakenForRefill
				sleep_200()
				pf.Manualmode_Pump_constantPressure(zr.A, flowModePressure, pump, sleep_200)
				pf.Manualmode_Pump_constantPressure(zr.B, flowModePressure, pump, sleep_200)
			else
				sleep_200()
			end
		end
		pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_200)			-- stop pump
		pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_200)			-- stop pump

		pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_200, false)
		status:RemoveStatus(msg)
		pump:SetPumpSideSignal(zr.A, zr.PumpsideSignal.Stop)
		pump:SetPumpSideSignal(zr.B, zr.PumpsideSignal.Stop)
	end
	---Setting a direct flow piston speed driven
	local function manualPistonSpeedMode()
		local pistonSpeed = context:GetArgumentValue("Piston speed")
		local volumeToDispense = context:GetArgumentValue("Dispense volume")

		---Get valve position to potentially reset to their position if they are shifted during degassing and see if columns are included in path.
		local a_angle = pump:GetSetManualValvePosition(zr.A)
		local b_angle = pump:GetSetManualValvePosition(zr.B)
		local t_angle = pp.GetTrapValvePosition(execAux)
		local i_angle = pp.GetInjectorValvePosition(execAux)

		local isTrapOnPath = false
		local isSeperationOnPath = false

		---Check path to see if trap and/or seperation column are on it
		if a_angle == baltic.PumpValve.MixTee or b_angle == baltic.PumpValve.MixTee then
			if t_angle == baltic.TrapValve.GradientT then
				isTrapOnPath = true
				isSeperationOnPath = true
			elseif t_angle == baltic.TrapValve.InjectWaste then
				isTrapOnPath = true
			end
		end
		if a_angle == baltic.PumpValve.Inject then
			if i_angle == baltic.InjectionValve.Inject or i_angle == baltic.InjectionValve.Load then
				if t_angle == baltic.TrapValve.Analytical then
					isSeperationOnPath = true
				elseif t_angle == baltic.TrapValve.Trap or t_angle == baltic.TrapValve.GradientA then
					isTrapOnPath = true
				end
			end
		end
		context:Log("Set pressure limits.")
		context:Log("Trap column on chosen path: {0}", isTrapOnPath)
		context:Log("Seperation Column on chosen path: {0}", isSeperationOnPath)
		pf.setMaxPressureLimitsAB(context, pump, pf.noExp(pressSettings.GradientPumpMaxTargetPressure,0), isTrapOnPath, isSeperationOnPath)

		---Refill pumps if leftover volume is smaller than volume to dispense.
		---Remember prior valve positions to reset to those after refilling.
		local currentVolA = baltic.MaxPumpVolume - pump:GetPistonPosition(zr.A)
		local currentVolB = baltic.MaxPumpVolume - pump:GetPistonPosition(zr.B)
		if currentVolA < volumeToDispense or currentVolB < volumeToDispense then
			degas(context, 1, -1, -1, 0, sleep_1000) ---used to refill pump heads
			setValvePositions(a_angle, b_angle, i_angle, t_angle)
		end
		
		currentVolA = baltic.MaxPumpVolume - pump:GetPistonPosition(zr.A)
		currentVolB = baltic.MaxPumpVolume - pump:GetPistonPosition(zr.B)
		---If pistons are not at position 0 after refilling, current Volume - volume to dispense can be smaller than 0 -> EndVolume = 0.
		local endVolumeA = math.max(currentVolA - volumeToDispense, 0)
		local endVolumeB = math.max(currentVolB - volumeToDispense, 0)

		zr.ChangePressurePID(context, pump,baltic.PPID.PID_16_1_1)
		local approximateTime = math.floor(volumeToDispense / pistonSpeed + 0.5)
		local msg = "Running direct flow with constant piston speed. This process takes approximately "..tostring(approximateTime).." minutes. Please make sure the selected path allows for the given piston speed."
		status:SetStatus(msg)

		---Constant speed until final volume is reached
		pf.Manualmode_Pump_constantSpeed(zr.A, pistonSpeed, pump, sleep_200)
		pf.Manualmode_Pump_constantSpeed(zr.B, pistonSpeed, pump, sleep_200)

		local aFinished = false
		local bFinished = false

		---Stay in loop until both pumps are finished. Piston speed is set to zero individually per side when finished.
		while aFinished==false or bFinished==false do
			if not aFinished then
				currentVolA = baltic.MaxPumpVolume - pump:GetPistonPosition(zr.A)
				if currentVolA <= endVolumeA then
					aFinished = true
					pf.Manualmode_Pump_constantSpeed(zr.A, 0, pump, sleep_200)			-- stop pump A
				end
			end
			if not bFinished then
				currentVolB = baltic.MaxPumpVolume - pump:GetPistonPosition(zr.B)
				if currentVolB <= endVolumeB then
					bFinished = true
					pf.Manualmode_Pump_constantSpeed(zr.B, 0, pump, sleep_200)			-- stop pump B
				end
			end
			sleep_50()
		end

		-- also stops the pumps
		pf.reducePressure(context, pump, zr, zr.A, zr.B, 50, 50, 60, sleep_200, false)
		status:RemoveStatus(msg)
	end

	---Run a high pressure leak test of the mixTee and injection system
	---@param pressure table [bar]
	---@param statusPrefix string "" if not requried
	local function highPressureLeakTest(pressure, statusPrefix)
		status:SetStatus(statusPrefix .. "Running high pressure leak test")
		context:Log("Running High Pressure Leak Test")
		selfDiagnose.self_LT_HP = true
		selfDiagnose.self_FRT_A_MT = false
		selfDiagnose.self_FRT_B_MT = false
		selfDiagnose.self_FRT_IS = false
		selfDiagnose.self_LT_HP_AB = false
		selfDiagnose.self_LT_HP_IS_RT = false
		selfDiagnose.self_LT_LP_AB = false
		selfDiagnose.self_LT_P_RT = false
		selfDiagnose.showMessage = false
		diag.diagnostics(installed, context, pressure, false, selfDiagnose)
		status:RemoveStatus(statusPrefix .. "Running high pressure leak test")
	end

	---Run a high pressure leak test of the mixTee and injection system
	---@param pressure table [bar]
	---@param statusPrefix string "" if not required
	local function highPressureLeakTestPumpAB(pressure, statusPrefix)
		status:SetStatus(statusPrefix .. "Running high pressure leak test pump A and B")
		context:Log("Running High Pressure Leak Test Pump A and B")
		selfDiagnose.self_LT_HP = false
		selfDiagnose.self_FRT_A_MT = false
		selfDiagnose.self_FRT_B_MT = false
		selfDiagnose.self_FRT_IS = false
		selfDiagnose.self_LT_HP_AB = true
		selfDiagnose.self_LT_HP_IS_RT = false
		selfDiagnose.self_LT_LP_AB = false
		selfDiagnose.self_LT_P_RT = false
		selfDiagnose.showMessage = false
		diag.diagnostics(installed, context, pressure, false, selfDiagnose)
		status:RemoveStatus(statusPrefix .. "Running high pressure leak test pump A and B")
	end

	---@param statusPrefix string "" if not required
	local function highPressureRampLeakTestInjectionSystem(pressure, statusPrefix)
		status:SetStatus(statusPrefix .. "Running high pressure leak test injection system")
		context:Log("Running High Pressure Leak Test Injection System")
		selfDiagnose.self_LT_HP = false
		selfDiagnose.self_FRT_A_MT = false
		selfDiagnose.self_FRT_B_MT = false
		selfDiagnose.self_FRT_IS = false
		selfDiagnose.self_LT_HP_AB = false
		selfDiagnose.self_LT_HP_IS_RT = true
		selfDiagnose.self_LT_LP_AB = false
		selfDiagnose.self_LT_P_RT = false
		selfDiagnose.showMessage = false
		diag.diagnostics(installed, context, pressure, false, selfDiagnose)
		status:RemoveStatus(statusPrefix .. "Running high pressure leak test injection system")
	end

	---Run all diagnostic test without user action
	---@param statusPrefix string "" if not required
	local function diagnosticTests(isExtendedDiag, statusPrefix)
		status:SetStatus(statusPrefix .. "Running diagnostic tests")
		context:Log("Running Diagnostic Tests")
		local pressure = {1000}
		selfDiagnose.self_LT_HP = true
		selfDiagnose.self_FRT_A_MT = true
		selfDiagnose.self_FRT_B_MT = true
		selfDiagnose.self_FRT_IS = true
		selfDiagnose.self_LT_HP_AB = true
		selfDiagnose.self_LT_HP_IS_RT = false
		selfDiagnose.self_LT_LP_AB = true
		selfDiagnose.self_LT_P_RT = false
		selfDiagnose.showMessage = false
		diag.diagnostics(installed, context, pressure, isExtendedDiag, selfDiagnose)
		status:RemoveStatus(statusPrefix .. "Running diagnostic tests")
	end

	---Run direct flow with/without the trap column
	---@param withTrap boolean
	---@param flow number [µL/min]
	---@param composition number [%]
	---@param duration number [minutes]
	local function directFlow(withTrap, flow, composition, duration)
		local runTime = duration*60+pf.now()		-- [sec]
		local function simpleDirectFlow()
			pf.reducePressure(context, pump, zr, zr.A, zr.B, 5, 5, 300, sleep_200, false)
			zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)
			local trapPosition = baltic.TrapValve.GradientA
			local flowA = flow * composition / 100
			local flowB = flow - flowA
			if withTrap == true then trapPosition = baltic.TrapValve.GradientT end
			setValvePositions( baltic.PumpValve.MixTee, baltic.PumpValve.MixTee, baltic.InjectionValve.Load, trapPosition )
			context:Signalize(baltic.ColorsRGB.LightGray, baltic.Naming.Separator, baltic.Naming.Separator)
			sleep_1000()
			pf.Manualmode_Pump_constantFlow_binary( context, flowA, flowB, pump, zr, sleep_200 )

			while pf.now() < runTime do
				sleep_1000()
				if zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) then break end
			end
			pr.Signalize_Reset(context)		end

		-- run direct flow until the user aborts the procedure
		while pf.now() < runTime do
			simpleDirectFlow()
			if pf.now() < runTime then		-- refill pumps if time has not left
				degas(context, 1, -1, -1, 0, sleep_1000)
			end
		end
	end

	---Run direct flow pressure driven with/without the trap column
	---@param trapValvePosition number
	---@param pressure number [bar]
	---@param duration number [minutes]
	---@param statusPrefix string "" if not required
	local function directFlowPressure(trapValvePosition, pressure, duration, statusPrefix)
		local runTime = duration*60+pf.now()		-- [sec]
		local msg = "Running direct flow with"
		local msg1 = "out"
		local msg2 = " trap"
		if trapValvePosition == baltic.TrapValve.GradientT then
			msg = msg..msg2
		else	-- trapValvePosition == baltic.TrapValve.GradientT or baltic.TrapValve.Waste
			msg = msg..msg1..msg2
		end
		local function simpleDirectPressure()
			status:SetStatus(statusPrefix .. msg)
			zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)
			setValvePositions( baltic.PumpValve.MixTee, baltic.PumpValve.MixTee, baltic.InjectionValve.Load, trapValvePosition )
			context:Signalize(baltic.ColorsRGB.LightGray, baltic.Naming.Separator, baltic.Naming.Separator)
			sleep_200()
			pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false)
			pf.Manualmode_Pump_constantPressure( zr.A, pressure, pump, sleep_200 )
			pf.Manualmode_Pump_constantPressure( zr.B, pressure, pump, sleep_200 )
			while pf.now() < runTime do
				sleep_1000()
				if zr.IsEmpty(pump, zr.A) or zr.IsEmpty(pump, zr.B) then break end
			end
			status:RemoveStatus(statusPrefix .. msg)
			pr.Signalize_Reset(context)
		end

		-- run direct flow until the user aborts the procedure
		while pf.now() < runTime do
			simpleDirectPressure()
			if (pf.now() < runTime) and (duration > 0.1) then
				degas(context, 1, -1, -1, 0, sleep_1000)
			end
		end
	end

	---Run a preparation with solvent exchange
	---@param numOfSolventExchange number
	---@param repeatIfAirDetected boolean
	---@param cnt number
	---@param statusPrefix string "" if not requried
	---@return number
	---@return boolean
	---@return number
	---@return boolean
	local function preparePumps(numOfSolventExchange, repeatIfAirDetected, cnt, statusPrefix)
		local iniPrepMsg = "--- Initial Preparation "
		local prepAMsg = "Preparation A "
		local prepBMsg = "Preparation B "
		local msg1 = "preparation with solvent exchange and priming the volumetric wash pumps"
		local msgStart = "Start "..msg1

		---Log the results to the journal
		---@param valA number
		---@param repetitionA integer
		---@param passedA boolean
		---@param valB number
		---@param repetitionB integer
		---@param passedB boolean
		local function reportPreparationValues(valA, repetitionA, passedA, valB, repetitionB, passedB)
			msg1 = iniPrepMsg .. tostring(cnt)
			local msgA = prepAMsg .. tostring(cnt)
			local msgB = prepBMsg .. tostring(cnt)
				journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, msg1, os.date("%c")))
			if passedA == true then
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, msgA, valA))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Repetitions A", repetitionA, "", "N0"))
				csv.logValueInCSVFile(context, csvFileName, msg1, msgA, valA, "", -1)
				csv.logValueInCSVFile(context, csvFileName, msg1, "Repetitions A", repetitionA, "", 0)
			end
			if passedB == true then
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, msgB, valB))
				journal:Add(JournalEntry.Set(LogTo.Both, context.Name, "Repetitions B", repetitionB, "", "N0"))
				csv.logValueInCSVFile(context, csvFileName, msg1, msgB, valB, "", -1)
				csv.logValueInCSVFile(context, csvFileName, msg1, "Repetitions B", repetitionB, "", 0)
			end
		end

		status:SetStatus(statusPrefix)
		context:Log(msgStart)
		local p_degas_A = { degas_Channel, context, zr, zr.A, numOfSolventExchange, repeatIfAirDetected, -1, nil, nil, 0, parallel.yield }
		local p_degas_B = { degas_Channel, context, zr, zr.B, numOfSolventExchange, repeatIfAirDetected, -1, nil, nil, 0, parallel.yield }
		local a, repA, passedA, b, repB, passedB = parallel.run(sleep_200, p_degas_A, p_degas_B)

		local msgEnd = "End "..msg1
		context:Log(msgEnd)
		reportPreparationValues(a, repA, passedA, b, repB, passedB)
		status:RemoveStatus(statusPrefix)
		return a, passedA, b, passedB
	end

	---Run the flow restriction test
	---@param pressure table [bar]
	---@param statusPrefix string "" if not required
	local function flowRestrictionTest(pressure, statusPrefix)
		status:SetStatus(statusPrefix)
		context:Log("Starting Flow Restriction Test")
		selfDiagnose.self_LT_HP = false
		selfDiagnose.self_FRT_A_MT = true
		selfDiagnose.self_FRT_B_MT = true
		selfDiagnose.self_FRT_IS = true
		selfDiagnose.self_LT_HP_AB = false
		selfDiagnose.self_LT_HP_IS_RT = false
		selfDiagnose.self_LT_LP_AB = false
		selfDiagnose.self_LT_P_RT = false
		selfDiagnose.showMessage = false
		diag.diagnostics(installed, context, pressure, false, selfDiagnose)
		status:RemoveStatus(statusPrefix)
	end

	---Pressurizes the pumps and depressurizes the pumps 20 times
	---@param highPressure table
	---@param statusPrefix string "" if not required
	local function pressureSwinging(highPressure, statusPrefix)
		-- Block trap valve and connect the pump A and B to mix T via  the sample loop
		local n = 1
		local maxCycle = 20
		local msg1 = "Pressure swing cycle "
		local msg2 = " of "
		local msg3 = ": pressurizing"
		local msg4 = ": depressurizing"
		local msg = ""
		local valveTPos, isTrapValveBlocked = pp.isTrapValveBlocked(execLeft)
		context:Sleep(250)
		if isTrapValveBlocked == false then valveTPos = valveTPos + 30 end
		setValvePositions(baltic.PumpValve.Compress, baltic.PumpValve.MixTee, baltic.InjectionValve.Load, valveTPos)

		pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false)

		-- Repeat this cycle 20 times
		while n <= maxCycle do
			msg = msg1..tostring(n)..msg2..tostring(maxCycle)..msg3
			status:SetStatus(statusPrefix .. msg)
			-- Increase the pressure to 1000 bar and keep it for 5 min (1200 bar of Psens =1600 bar)
			build_pressureAB(highPressure[1], highPressure[1], sleep_200)
			context:Sleep(5*60*1000)
			if n >= maxCycle then
				local leakInjPath = pump:GetCurrentFlow(zr.A)
				local leakMixTeePath = pump:GetCurrentFlow(zr.B) + leakInjPath
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Pressure swinging pumps A+B", os.date("%c")))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leakage rate injection path", -leakInjPath, "\181L/min"))
				journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Leakage rate mixTee path", leakMixTeePath, "\181L/min"))
				csv.logValueInCSVFile(context, csvFileName, "Pressure swinging pumps A+B", "Leakage rate injection path", -leakInjPath, "\181L/min", -1)
				csv.logValueInCSVFile(context, csvFileName, "Pressure swinging pumps A+B", "Leakage rate mixTee path", leakMixTeePath, "\181L/min", -1)
			end
			status:RemoveStatus(statusPrefix .. msg)

			msg = msg1..tostring(n)..msg2..tostring(maxCycle)..msg4
			status:SetStatus(statusPrefix .. msg)
			-- Reduce pressure to 0 bar and hold for 10s
			build_pressureAB(5, 5, sleep_200)
			context:Sleep(10*1000)
			status:RemoveStatus(statusPrefix .. msg)
			n=n+1
		end

		stopPumpsAB()
		valveTPos = valveTPos - 30
		setValvePositions(nil, nil, nil, valveTPos)
	end

	local function initializeSystem()
		-- total runtime: 12-13 hours
		-- User action required after 0 minutes of runtime
		-- User action required after 30 minutes of runtime if "Conditioning system" is also selected

		local cnt = 1

		local statusMessagePrefix = "Initialize System: "
		if context:GetArgumentValue("Conditioning system") == true then
			statusMessagePrefix = "Initialize + Conditioning System: "
		end

		local function calSensors()
			context:SetAbortEnabled(false)
			context:Sleep(2000)
			status:SetStatus(statusMessagePrefix .. "Pressure Sensor Calibration")
			context:Log(baltic.devider)
			context:Log("--- Pressure Sensor Offset Calibration:")

			local a, b, passed = pf.calibrate_press_sensors(installed, context, pump, zr, baltic.PumpValve.Waste)
			if not passed then
				context:SetAbortEnabled(true)
				context:Abort()
			end
			context:Log(baltic.devider)
			status:RemoveStatus(statusMessagePrefix .. "Pressure Sensor Calibration")
			journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Pressure sensor calibration", true))
			journal:Add(JournalEntry.Set(LogTo.Journal,context.Name, "--- Pressure Sensor Calibration", os.date("%c")))
			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Pressure sensor offset A", a, "bar", "N1"))
			journal:Add(JournalEntry.Set(LogTo.Journal, context.Name, "Pressure sensor offset B", b, "bar", "N1"))
			csv.logValueInCSVFile(context, csvFileName, "Pressure Sensor Calibration", "Pressure sensor offset A", a, "bar", 1)
			csv.logValueInCSVFile(context, csvFileName, "Pressure Sensor Calibration", "Pressure sensor offset B", b, "bar", 1)
			context:SetAbortEnabled(true)
			return true
		end

		local function executeExtendedWash()
			status:SetStatus(statusMessagePrefix .. "Running extended wash")
			context:Log("Running extended wash")
			local cyclesEW = 1
			local volumOrganic = 10
			local volumeAcqueous = 40
			local flowOrganic = 10
			local flowAcqueous = 10
			error_code = ew.extended_wash(installed, context, execLeft, execAux, pp, pump, false, false, cyclesEW, volumOrganic, volumeAcqueous, flowOrganic, flowAcqueous, sleep_200)
			status:RemoveStatus(statusMessagePrefix .. "Running extended wash")
			if ( error_code.err ~= "" ) then
				error_code.message = "Autosampler clean - "..error_code.message
				context:Log(error_code.message)
	--			handle_message()		-- don"t Abort() here, handle_message later
			else
				status:SetStatus(statusMessagePrefix .. "Running injection path wash")
				context:Log("Running injection path wash")
					local duration = 20		-- [sec]
				ew.injection_path_wash(installed, context, execLeft, execAux, chrom, pp, pr, pump, duration)
				status:RemoveStatus(statusMessagePrefix .. "Running injection path wash")
			end
		end

		-- initialization function starts here
		context:Report("Service", Severity.Info, true, "Please disconnect the separation column, then click 'Confirm' to continue.")

		-- set the maximum pump pressure
		pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, false, false)

		-- degasser on
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)
		status:SetStatus(statusMessagePrefix .. "Priming volumetric wash pump")
		execLeft:LeaveObject()
		-- move the LCP tool to the LCMS wash station,
		-- penetrate waste,
		-- dispense 10mL solv 2 and then 10mL solv1, both at 50uL/s
		pp.PrimeVolumetricPump( execLeft, 10000, 50)
		execLeft:LeaveObject()
		execLeft:MoveToHome()

		context:WaitForSignal("Service")
		context:Sleep(500)
		if context:GetArgumentValue("Conditioning system") == true then
			context:Report("", Severity.Info, true, "Next user action required in about 30 minutes.")
		end

		calSensors()

		clearJournal()			-- delete all preparation entries

		zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)

		-- Run 1 cycle of preparation+solvent replacement, and record the preparation and pressure test values after each cycle
		-- 19 minutes
		preparePumps(10, false, cnt, "")
		cnt = cnt + 1
		status:RemoveStatus(statusMessagePrefix .. "Priming volumetric wash pump")

		-- Run direct flow w/o trap for 5 min (50%B, 3uL/min)
		-- 5 minutes
		directFlowPressure(baltic.TrapValve.GradientA, 950, 5, statusMessagePrefix)		-- trap valve position, pressure, duration

		-- Run direct flow with trap (even if there is no trap installed) for 5 min (50%B, 3uL/min)
		-- 5 minutes
		directFlowPressure(baltic.TrapValve.GradientT, 950, 5, statusMessagePrefix)		-- trap valve position, pressure, duration

		if context:GetArgumentValue("Conditioning system") == true then
			context:Report("Block", Severity.Info, true, "Please block the transfer capillary, then click 'Confirm' to continue.")
			directFlowPressure(baltic.TrapValve.GradientT, 100, 0.2, statusMessagePrefix)		-- trap valve position, pressure, duration: set a low flow rate so as not to dry out the transfer capillary..
			context:WaitForSignal("Block")			-- this is in preparation to "Conditioning System"
		end

		-- Run the flow restriction test and record values
		-- 7 minutes
		local testPressure = {100}
		flowRestrictionTest(testPressure, statusMessagePrefix)

		-- Run HP leak test pump A and B
		-- 61 minutes
		testPressure = {maxTestPressure}
		highPressureLeakTestPumpAB(testPressure, statusMessagePrefix)

		-- Run 1 cycle of preparation+solvent replacement, and record the preparation and pressure test values after each cycle
		-- 20 minutes
		preparePumps(10, false, cnt, statusMessagePrefix)
		cnt = cnt + 1

		-- Run the flow restriction test and record values
		-- 7 minutes
		testPressure = {100}
		flowRestrictionTest(testPressure, statusMessagePrefix)

		-- Run HP leak test pump A and B
		-- 62 minutes
		testPressure = {maxTestPressure}
		highPressureLeakTestPumpAB(testPressure, statusMessagePrefix)

		-- Run 1 cycle of preparation+solvent replacement, and record the preparation and pressure test values after each cycle
		-- 19 minutes
		local a, _, b, _ = preparePumps(10, false, cnt, statusMessagePrefix)
		cnt = cnt + 1

		-- If preparation values are more than 8 for A and 15 for B do:
		-- 0-89 minutes
		if (a > 8) or (b > 15) then
			status:SetStatus(statusMessagePrefix .. "Extended pump preparation")
			-- set the maximum pump pressure
			local pressure = pf.setMaxPressureLimitsAB(context, pump, pressSettings.GradientPumpMaxTargetPressure, true, false)	-- no columns in path; set max pump pressure
			-- Pressurize at 1000bar for 1 min, depressurize to 0, and run a solvent replacement (repeate up to 4x)
			setValvePositions( baltic.PumpValve.Compress, baltic.PumpValve.Compress, nil, nil )
			local repeatition = 4
			while repeatition > 0 do
				status:SetStatus("Pressurizing pumps")
				build_pressureAB(pf.noExp(pressure), pf.noExp(pressure), sleep_200)
				local endTime = 60 + pf.now()	-- 60 seconds
				while pf.now() < endTime do sleep_200() end

				pf.Manualmode_Pump_constantPressure( zr.A, 0, pump, sleep_200 )
				pf.Manualmode_Pump_constantPressure( zr.B, 0, pump, sleep_200 )
				local continueA = true
				local continueB = true
				local timeOut = 60 + pf.now()
				while (continueA == true) or (continueB == true) do
					sleep_200()
					if pump:GetCurrentPressure(zr.A) <= 3 then continueA = false end
					if pump:GetCurrentPressure(zr.B) <= 3 then continueB = false end
					if pf.now() > timeOut then
						continueA = false
						continueB = false
					end
				end
				status:RemoveStatus("Pressurizing pumps")

				a, _, b, _ = preparePumps(10, false, cnt, statusMessagePrefix)
				repeatition = repeatition - 1
				-- stop pump preparation if the degas values are below the threshold
				if (a <= 8) and (b <= 15) then repeatition = 0 end
				cnt = cnt + 1
			end
			if (a > 20) or (b > 40) then
				local msg = "Too much air in pump"
				local ch = " A"
				if (a > 20) and (b > 40) then
					ch = "s A and B"
				elseif (a <= 20) then
					ch = " B"
				end
				local msg2 = " detected. Please check the solvent filling and for a leakage."
				msg = msg..ch..msg2
				context:Report("Initialization", Severity.Info, true, msg)
				return
			end
			status:RemoveStatus(statusMessagePrefix .. "Extended pump preparation")
		end

		-- Run the flow restriction test and record values
		-- 7 minutes
		testPressure = {100}
		flowRestrictionTest(testPressure, statusMessagePrefix)

		-- Run HP leak test pump A and B
		-- 62 minutes
		testPressure = {maxTestPressure}
		highPressureLeakTestPumpAB(testPressure, statusMessagePrefix)

		-- Run the leak HP test in diagnostics (use blocked trap valve instead of plugging the transfer capillary)
		-- 12 minutes
		testPressure = {1000}
--		testPressure = {maxTestPressure}
		highPressureLeakTest(testPressure, statusMessagePrefix)

		-- Run all tests in extended diagnostics (use blocked trap valve instead of plugging the transfer capillary)
		-- Run the low-pressure leak test
		-- 86 minutes
		diagnosticTests(false, statusMessagePrefix)

		-- Run extended wash
		-- 7 minutes
		executeExtendedWash()

		-- Run preparation, and note down the preparation values
		-- 3 minutes
		preparePumps(1, true, cnt, statusMessagePrefix)
		cnt = cnt + 1

		-- Run direct flow w/o trap for 10 min (50%B, 3uL/min) 
		-- 10 minutes
		directFlowPressure(baltic.TrapValve.GradientA, 950, 10, statusMessagePrefix)		-- trap valve position, pressure, duration

		-- Run direct flow w trap (even if there is no trap installed) for 5 min (50%B, 3uL/min) 
		-- 10 minutes
		directFlowPressure(baltic.TrapValve.GradientT, 950, 10, statusMessagePrefix)		-- trap valve position, pressure, duration

		-- Run the flow restriction test in smart diagnostics 
		-- 7 minutes
		testPressure = {100}
		flowRestrictionTest(testPressure, statusMessagePrefix)

		-- Run HP leak test pump A and B
		-- 62 minutes
		testPressure = {maxTestPressure}
		highPressureLeakTestPumpAB(testPressure, statusMessagePrefix)

		-- Run the HP leak test in diagnostics (use blocked trap valve instead of plugging the transfer capillary)
		-- 12 minutes
		testPressure = {1000}
--		testPressure = {maxTestPressure}
		highPressureLeakTest(testPressure, statusMessagePrefix)

		-- Run all tests in extended diagnostics (use blocked trap valve instead of plugging the transfer capillary)
		-- Run the low-pressure leak test 
		-- 86 minutes
		diagnosticTests(false, statusMessagePrefix)

		-- Set piston position to a leftover volume of 500uL 
		-- Pressurize pump A and B to 1000 bar for 1h
		-- 62 minutes
		testPressure = {maxTestPressure}
		highPressureLeakTestPumpAB(testPressure, statusMessagePrefix)

		-- Run a preparation, record values
		-- 3 minutes
		preparePumps(1, true, cnt, statusMessagePrefix)
		cnt = cnt + 1

		-- Run the flow restriction test in smart diagnostics 
		-- 7 minutes
		testPressure = {100}
		flowRestrictionTest(testPressure, statusMessagePrefix)

		-- Run HP leak test pump A and B
		-- 62 minutes
		testPressure = {maxTestPressure}
		highPressureLeakTestPumpAB(testPressure, statusMessagePrefix)

		-- Run the HP leak test (use blocked trap valve instead of plugging the transfer capillary)
		-- 12 minutes
		testPressure = {1000}
--		testPressure = {maxTestPressure}
		highPressureLeakTest(testPressure, statusMessagePrefix)

		-- Run all tests diagnostics (use blocked trap valve instead of plugging the transfer capillary)
		-- Run the low-pressure leak test 
		-- 86 minutes
		diagnosticTests(false, statusMessagePrefix)

		-- Set direct flow without trap valve, 3uL/min 50% for 24 hours or until it is aborted
		-- 60 minutes/hour * 24 hours = 1440 minutes
		local duration = 1440
		local trapValvePosition = baltic.TrapValve.GradientA
		if context:GetArgumentValue("Conditioning system") == true then
			duration = 60
			trapValvePosition = baltic.TrapValve.Waste
		end
		directFlowPressure(trapValvePosition, 250, duration, statusMessagePrefix)		-- trap valve position, pressure, duration

		if context:GetArgumentValue("Conditioning system") == false then
			context:Report("Initialize System", Severity.Info, true, "Please reconnect the separation column, then click 'Confirm' to continue.")
			context:WaitForSignal("Initialize System")
		end

		-- degasser off
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)
	end

	local function conditioningSystem()
		-- total runtime ~ 8-10 hours
		-- User action required after 0 minutes of runtime if "Initilize system" is not selected

		local cnt = 1

		local statusMessagePrefix = "Conditioning System: "
		if context:GetArgumentValue("Initialize system") == true then
			statusMessagePrefix = "Initialize + Conditioning System: "
		end

		if context:GetArgumentValue("Initialize system") == false then
			context:Report("Block", Severity.Info, true, "Please block the transfer capillary, then click 'Confirm' to continue.")
		end

		-- degasser on
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, true)

		-- Run leak tests pump A and B, at 50, 100, 250, 500, 800, 1000, (1200 bar if P sens is 1600 bar), 1000 bar, 500, 50 bar, in this order, possibly without depressurizing in between.
		-- 32 minutes
		local testPressure = {50, 100, 250, 500, 800, maxTestPressure, 500, 50}
		highPressureLeakTestPumpAB(testPressure, statusMessagePrefix)

		-- Run leak tests injection system, at 500, 800, 1000
		-- 15 minutes
		testPressure = {500, 800, 1000}
--		testPressure = {500, 800, maxTestPressure}
		highPressureRampLeakTestInjectionSystem(testPressure, statusMessagePrefix)

		-- preparation run
		-- 3 minutes
		preparePumps(1, false, cnt, statusMessagePrefix)
		cnt = cnt + 1

		-- Run all tests in diagnostics (via transfer capillary)
		if context:GetArgumentValue("Initialize system") == false then
			context:WaitForSignal("Block")
		end

		-- 86 minutes
		diagnosticTests(true, statusMessagePrefix)

		-- Block trap valve and connect the pump A and B to mix T via  the sample loop
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_16_1_1)
		-- 120 minutes
		testPressure = {1000}
--		testPressure = {maxTestPressure}
		pressureSwinging(testPressure, statusMessagePrefix)

		-- Run a preparation with included solvent replacement
		-- 45 minutes
		local a, _, b, _ = preparePumps(10, false, cnt, statusMessagePrefix)
		cnt = cnt + 1
		if (a > 8) or (b > 15) then
			directFlowPressure(baltic.TrapValve.Waste, 950, 90, statusMessagePrefix)
		end

		-- Run the leak test in smart diagnostics (via transfer capillary)
		-- Run leak tests (pump A and B ) at 50, 100, 250, 500, 800, 1000, 500, 50 bar, in this order, possibly without depressurizing in between
		-- 32 minutes
		testPressure = {50, 100, 250, 500, 800, maxTestPressure, 500, 50}
		highPressureLeakTestPumpAB(testPressure, statusMessagePrefix)

		-- Run leak tests injection system, at 500, 800, 1000
		-- 15 minutes
		testPressure = {500, 800, 1000}
--		testPressure = {500, 800, maxTestPressure}
		highPressureRampLeakTestInjectionSystem(testPressure, statusMessagePrefix)

		-- Run all tests in diagnostics (via transfer capillary)
		-- 86 minutes
		diagnosticTests(true, statusMessagePrefix)

		-- degasser off
		pump:SetDigitalOutput(zr.DigitalOutput.PO2, false)

		context:Report("Unblock", Severity.Info, true, "Please remove the block of the transfer capillary and reconnect the separation column.\nThen click 'Confirm' to continue.")
		context:WaitForSignal("Unblock")			-- this is in preparation to "Conditioning System"

		-- Set idle flow
		-- Idle flow is set automatically if it is enabled in "Preferences"
	end

	local function flushSystem()
--		The procedure runs in 6, 12, 24 and 48h itervals. Every 6h, the following has to be done:
		local statusMsg = "Flush system"
		local cycles = context:GetArgumentValue("Cycles")
		local actualCycle = 1
		local msgCycle = " (cycle "..actualCycle.." of "..cycles..")."
		local msgToBeShown = statusMsg..msgCycle
		status:SetStatus(msgToBeShown)		-- show this message until the system is prepared for executing the procedure
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

--		Message: put all lines (1, 2, A, B ) in the wash solution. Disconnect the separation column.
		local msg = "Prepare flushing system"
		context:Report(msg, Severity.Info, true, "Put tubings 1, 2, A and B into the bottle with wash solution, then click 'Confirm' to continue.")
		context:WaitForSignal(msg)
		context:Sleep(500)
		msg = "Flush system"
		context:Report(msg, Severity.Info, true, "Disconnect the separation column, then click 'Confirm' to continue.")
		context:WaitForSignal(msg)

		local duration = 6							--[hours]
		local startTime = pf.noExp(((pf.now()/3600) + (cycles*duration)), 1)				--[hours]
		local remainingTime = startTime				--[hours]
		local msgRemainigTime = " Time remaining: "..remainingTime.."h"

		local function showStatusMsg(msgProcedure)
			msgCycle = " (cycle "..actualCycle.." of "..cycles..")."
			remainingTime = pf.noExp((startTime - (pf.now()/3600)), 1)
			msgRemainigTime = " Time remaining: "..remainingTime.."h"
			msgToBeShown = statusMsg..msgCycle..msgProcedure..msgRemainigTime
			status:SetStatus(msgToBeShown)
		end

		context:Log("--- Duration [h]:                       {0}", (duration*cycles))

		status:RemoveStatus(msgToBeShown)
		while (actualCycle <= cycles) do
--			move the LCP tool to the LCMS wash station, penetrate waste, dispense 5mL solv 2 and then 5mL solv1, both at 50uL/s
			showStatusMsg(" Priming volumetric wash pumps.")
			execLeft:LeaveObject()
			pp.PrimeVolumetricPump( execLeft, 5000, 50)
			execLeft:LeaveObject()
			execLeft:MoveToHome()
			while not execLeft.IsIdle do sleep_200() end
			status:RemoveStatus(msgToBeShown)

--			Run 1x clean injector with vol pump
			showStatusMsg(" Cleaning the injector,")
			local qaw = require "queue_autosampler_wash"
			qaw.queue_clean_injector(execLeft, execAux, pp, true, true)
			while not execLeft.IsIdle do sleep_200() end
			status:RemoveStatus(msgToBeShown)

--			Run 1 cycle of preparation+solvent replacement, and record the preparation and pressure test values
			showStatusMsg(" Preparing pump A + B.")
			preparePumps(10, false, 1, "")
			status:RemoveStatus(msgToBeShown)

--			run flow restriction test
			showStatusMsg(" Running flow restriction test.")
			local testPressure = {100}
			flowRestrictionTest(testPressure, "")
			status:RemoveStatus(msgToBeShown)

--			Run direct flow with trap for 10 min (50%B, 3uL/min) 
			showStatusMsg(" Running direct flow with trap.")
			pf.setMaxPressureLimitA(context, pump, pressSettings.GradientPumpMaxTargetPressure, true, false, nil)
			directFlow(true, 3, 50, 10)
			status:RemoveStatus(msgToBeShown)

--			Run direct flow w/o trap (50%B, 3uL/min) until the end of the 6h
			showStatusMsg(" Running direct flow without trap.")
			local directFlowTime = (remainingTime - ((cycles - actualCycle) * duration)) * 60	-- [min]
			directFlow(false, 3, 50, directFlowTime)
			status:RemoveStatus(msgToBeShown)
			actualCycle = actualCycle + 1
		end
		status:RemoveStatus(statusMsg)
	end

	local function flushAutosampler()
--		The procedure uses 50mL of solvent per channel (total of 100mL). The user selects if the procedure runs for 6, 12, 24 or 48h. The solvent dispense is made out of packets of 2mL
		local statusMsg = "Flush autosampler"
		local cycles = context:GetArgumentValue("Cycles")
		local actualCycle = 1
		local msgCycle = " (cycle "..actualCycle.." of "..cycles..")."
		local msgToBeShown = statusMsg..msgCycle

		status:SetStatus(msgToBeShown)
		zr.ChangePressurePID(context, pump, baltic.PPID.PID_6_1_1)

--		Message: place lines 1 and 2 in the wash solvent. Disconnect the separation column.
		local msg = "Prepare flushing autosampler"
		context:Report(msg, Severity.Info, true, "Put tubings 1 and 2 into the bottle with wash solution, then click 'Confirm' to continue.")
		context:WaitForSignal(msg)

--		dispense 2mL solv 2 at 50uL/s
--		dispense 2mL solv 1 at 50uL/s
--		 wait time to equally space in the wash runs in the selected time.
		local duration = 6					--[hours]
		local timeForDispense = 1.46		--[min]: = 2* (2000µL / 50µL/s / 60s)
		local numOfDispenses = 25			--[cycles]: = 100mL / (2*2mL)
		local timeInjectorCleaning = 3		--[min]
		local waitTime = 13.354166			--[min]
		local actTime = pf.now()/3600
		local startTime = pf.noExp((actTime + (cycles*duration)), 0)				--[hours]
		local remainingTime = startTime		--[hours]
		local msgRemainigTime = " Time remaining: "..remainingTime.."h"

		local function showStatusMsg(msgProcedure)
			msgCycle = " (cycle "..actualCycle.." of "..cycles..")."
			actTime = pf.now()/3600
			remainingTime = pf.noExp((startTime - actTime), 1)
			msgRemainigTime = " Time remaining: "..remainingTime.."h"
			msgToBeShown = statusMsg..msgCycle..msgProcedure..msgRemainigTime
			status:SetStatus(msgToBeShown)
		end

		context:Log("--- Duration [h]:                     	   {0}", duration)
		context:Log("--- Time for dispense [min]:              {0}", timeForDispense)
		context:Log("--- Number of dispenses:                  {0}", numOfDispenses)
		context:Log("--- Time for cleaning the injector [min]: {0}", timeInjectorCleaning)
		context:Log("--- Time between dispenses [min]:         {0}", waitTime)

		local dispenses = numOfDispenses
		status:RemoveStatus(msgToBeShown)
		execLeft:LeaveObject()
		while (actualCycle <= cycles) do
			while numOfDispenses > 0 do
				showStatusMsg(" Priming volumetric wash pumps.")
				pp.PrimeVolumetricPump(execLeft, 2000, 50)
				while not execLeft.IsIdle do sleep_200() end
				numOfDispenses = numOfDispenses - 1
				if numOfDispenses > 0 then
					context:Log("Waiting for {0} minutes", waitTime)
					-- do not log the status, only show it in LC-Control
					local dictator = LoggingDictator.Prevent(status)
					local cnt = 60
					while (cnt > 0) do
						cnt = cnt - 1
						status:RemoveStatus(msgToBeShown)
						showStatusMsg(" Priming volumetric wash pumps.")
						context:Sleep(waitTime*1000)
					end
				end	--[ms]
				dictator:Dispose()
				status:RemoveStatus(msgToBeShown)
			end
			actualCycle = actualCycle + 1
			numOfDispenses = dispenses
		end
		execLeft:LeaveObject()

--		At the end, perform clean injector with volume pump 3 times
		local qaw = require "queue_autosampler_wash"
		msgToBeShown = "Flush autosampler. Cleaning the injector "
		cycles = 3
		actualCycle = 0
		while (actualCycle < cycles) do
			msg = msgToBeShown..(cycles-actualCycle)
			status:SetStatus(msg)
			qaw.queue_clean_injector(execLeft, execAux, pp, actualCycle == 0, actualCycle == cycles - 1)
			while not execLeft.IsIdle do sleep_200() end
			status:RemoveStatus(msg)
			actualCycle = actualCycle + 1
		end
	end

	-- Main starts here

	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Initialize system", context:GetArgumentValue("Initialize system")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Conditioning system", context:GetArgumentValue("Conditioning system")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flush system", context:GetArgumentValue("Flush system")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flush autosampler", context:GetArgumentValue("Flush autosampler")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Prime volumetric wash pumps", context:GetArgumentValue("Prime volumetric wash pumps")))
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Remove air from pumps", false)) -- will be overwritten with true if actually executed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow mode", false)) -- will be overwritten with true if actually executed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set Valve Positions", false)) -- will be overwritten with true if actually executed
	journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set Piston Positions", false)) -- will be overwritten with true if actually executed

	csv.logValueInCSVFile(context, csvFileName, "Service started", "", "", "", -1)

	if context:GetArgumentValue("Prime volumetric wash pumps") == true then
		local cycles = context:GetArgumentValue("Cycles")
		local actualCycle = 1
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Cycles", cycles, "N0"))
		status:SetStatus("Priming volumetric wash pumps")
		execLeft:LeaveObject()
		while (actualCycle <= cycles) do
			local msg = "(cycle "..actualCycle.." of "..cycles..")"
			status:SetStatus(msg)
			pp.PrimeVolumetricPump(execLeft, 3000, 50)
			while not execLeft.IsIdle do sleep_200() end
			status:RemoveStatus(msg)
			actualCycle = actualCycle + 1
		end
		execLeft:LeaveObject()
		status:RemoveStatus("Priming volumetric wash pumps")
	end

	if context:GetArgumentValue("Flush autosampler") == true then
		flushAutosampler()
	end

	if context:GetArgumentValue("Initialize system") == true then
		initializeSystem()
	end

	if context:GetArgumentValue("Flush system") == true then
		flushSystem()
	end

	if context:GetArgumentValue("Conditioning system") == true then
		conditioningSystem()
	end

	if  (context:GetArgumentValue("Set Valve Positions") == true) and
		(context:GetArgumentValue("Conditioning system") == false) and
		(context:GetArgumentValue("Initialize system") == false) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set Valve Positions", true))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Valve A", context:GetArgumentValue("Valve A"), "degree", "N0"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Valve B", context:GetArgumentValue("Valve B"), "degree", "N0"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Valve I", context:GetArgumentValue("Valve I"), "degree", "N0"))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Valve T", context:GetArgumentValue("Valve T"), "degree", "N0"))
		setValvePositions(context:GetArgumentValue("Valve A"), context:GetArgumentValue("Valve B"), context:GetArgumentValue("Valve I"), context:GetArgumentValue("Valve T"))
	end

	if 	((context:GetArgumentValue("Constant pressure") == true and context:GetArgumentValue("Flow mode") == true) or
		(context:GetArgumentValue("Remove air from pumps") == true)) and
		(context:GetArgumentValue("Conditioning system") == false) and
		(context:GetArgumentValue("Initialize system") == false) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow mode", true))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Constant pressure", true))
		manualFlowMode()
	end

	if context:GetArgumentValue("Constant speed") == true and context:GetArgumentValue("Flow mode") == true and context:GetArgumentValue("Remove air from pumps") == false then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Flow mode", true))
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Constant speed", true))
		manualPistonSpeedMode()
	end

	if  (context:GetArgumentValue("Set Piston Positions") == true) and
		(context:GetArgumentValue("Conditioning system") == false) and
		(context:GetArgumentValue("Initialize system") == false) and
		(context:GetArgumentValue("Remove air from pumps") == false) and
		(context:GetArgumentValue("Flow mode") == false) then
		journal:Add(JournalEntry.Set(LogTo.Twin, context.Name, "Set Piston Positions", true))
		setPistonPositions()
	end

	dictator:Dispose()

	while not execLeft.IsIdle do
		sleep_200()
	end

end
