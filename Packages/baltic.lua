local Date = "2025/11/12"

local P = {}

-- replace global env with package env; no more global access
_ENV = P

P.devider = "--------------------------------------------------------"

-- ******************************************************************

-- sample preparing types
Standard   = "Standard"
Dissolve   = "Dissolve"									-- dissolve sample,
Derivatize = "Derivatize"								-- derivatize sample
Dilute	   = "Dilute"									-- diluting sample
Special    = "Special"									-- special injection AND dissolve AND Derivatize AND Dilute
														-- add from vials:
																-- Tray
																-- 3 vials:
																	-- 1st vial
																	-- 1st volume
																	-- ...
														-- air gaps
														-- wash vial:
																-- item position
-- special types
StdDiag = "Std Diag"
OQDiag = "OQ"
FactoryDiag = "Factory"
Production = "Production"								-- this enables features like magicMix cleaning
ProductionPumpOnly = "PumpOnly"							-- this disables the autosampler
ProductionFaellanden = "PumpOnlyCHE"					-- pump cleaning procedures for Switzerland

-- ******************************************************************

-- additional values Application, BusinessLogic, Sample values (such as adding another application MyApp)
-- can be added without requiring a new build, but changes to the ApplicationKey structure will require 
-- changes in the plug-in code
ApplicationKey = {
	Special = {StdDiag, OQDiag, FactoryDiag, Production, ProductionPumpOnly, ProductionFaellanden},
	Sample = {Standard, Dissolve, Derivatize, Dilute, Special}
}

-- this is needed here so the plug-in can identify the advanced settings parameter that is a boolean
-- and tells the plug-in if this is a calibrant injection. If true the plug-in will pull in "calibrantTime"
-- that is calculated in the ValidateMethod function into the method for total run time calc
MSCalibrantInjection = "MS Calibrant Injection"
-- ******************************************************************



-- constants
--------------- capillary configuration --------------------
-------- unused configurations must be commented out -------
-- capillaryConfiguration = "nanoElute"
capillaryConfiguration = "nanoElute2"
-- capillaryConfiguration = "custom"
--------------- capillary configuration --------------------

------------ microElute constants and variables ------------
P.microElute			= false			-- "false" = nanoElute
P.changeResistances		= microElute	-- change restrictions only once if microElute is connected
P.flowResA_microElute   = 30			-- FlowResistanceA
P.flowResB_microElute   = 10 			-- FlowResistanceB
P.flowResOUT_microElute = 15			-- FlowResistanceOut
------------ microElute constants and variables ------------

P.diagnosticsPumpLevel  = 500			-- pumps are refilled at diagnostic start if there is more than 500 uL used
P.pumpLevel 			= 1000			-- used pump level before refilling
P.preOntimeDegasser 	= 30			-- 30 seconds,  is used for the degasser
P.stdby_depth			= 0 			-- mm; penetration depth in injector for standby mode (injection finished)

SystemVolume			= 0.22			-- capillary volume (700/20um) (15cm+55cm) [uL]
P.equiDeadVolume		= 0.46			-- Dead Volume Gradient: T-Piece + 35cm ID20 + TV long groove + 55cm ID20: 66nL + 110nl + 109nl + 173 nl = 460 nl
P.loopVolume			= 20			-- [uL]
MaxPumpVolume			= 1350			-- Pump head volume [uL]

P.basePressure			= 40			--(100bar/1000nL)
P.maxFlow				= 3.0			-- maximum of 3.0 µL/min
P.unityFlowSystem		= 0.006425354	-- unity flow PumpA -> MixTee -> ValveT -> transferline
P.unityFlowEQPumpSep  	= 0.02405		-- unity flow PumpA -> ValveI -> ValveT -> SeperationColumn  (sum: 105cm / 20um)
P.unityFlowEqLoadTrap 	= 0.0468		-- unity flow PumpA -> ValveI -> ValveT -> TrapColumn  		(sum: 50cm / 20um + 2* 10cm / 30um)
P.trapFlowFactor		= 1.045			-- with the same pump pressure the flow decreases by this factor if the trap goes off the line
P.tranferFlowFactor		= 1.13			-- with the same pump pressure the flow increases by this factor if the transfer line goes off the line

P.unityFlowRoute1_H2O	= 0.0063614		-- PumpA -> MixTee -> ValveT -> Trap -> Transferline
P.unityFlowRoute2_ACN	= 0.0189169		-- PumpB -> MixTee -> ValveT -> Waste
P.unityFlowRoute3_H2O	= 0.0505006		-- PumpA -> ValveI -> Loop -> ValveT -> Waste

P.sepEquilVolMultiplier  = 4			-- used in 'PreRunFunction()' in 'ini()'
P.trapEquilVolMultiplier = 10			-- used in 'PreRunFunction()' in 'ini()'

P.getSampleTime		= 90			-- [s]; time from start clean syringe until injected into the loop

P.smooth				= false			-- true = smooth pressure reduction; false = normal

LoggingOff 			= true			-- set LoggingOff to false to get extended loggings
ExecuteLogEnable 	= false

P.palSwitchCounter = {
	cntI = 0,
	cntT = 0
}

Settings = {
	GradientPumpRefillSpeed 			= -1000, -- ul/min
	GradientPumpPurgeSpeed 				= 4000, -- ul/min
	GradientPumpDefColumnEquilPressure 	= 800, -- bar
	ColumnOvenMinTemperature 			= 10, -- degC	-- PGNE-745
	ColumnOvenMaxTemperature 			= 60, -- degC	-- PBNE-652
	ColumnOvenDefaultTemperature		= 20 -- degC	-- PBNE-745
}

---@type PressureSensor
Settings_Sensor_1000 = {
	-- pressure sensor limits for pump a and b
	SensorLimitLow					= 100,		-- bar
	SensorLimitHigh					= 160,		-- bar
	SensorDefaultOffset				= 129.239,	-- bar
	SensorDefaultFactor 			= 0.01972,
	GradientPumpMaxTargetPressure 	= 1000, 	-- bar
	GradientPumpCutoffPressure 		= 1020, 	-- bar
}

---@type PressureSensor
Settings_Sensor_1600 = {
	-- pressure sensor limits for pump a and b
	SensorLimitLow					= 140,		-- bar	-- PNS-675: Modify the pressure sensor offset range (140 - 350)
	SensorLimitHigh					= 350, 		-- bar	-- PBNE-1042: changed value due to other pressure sensors (previous value: 160)
	SensorDefaultOffset				= 211,		-- bar
	SensorDefaultFactor 			= 0.03075,
	GradientPumpMaxTargetPressure 	= 1000, 	-- bar		-- PNS-763
--	GradientPumpMaxTargetPressure 	= 1200, 	-- bar		-- PNS-763
	GradientPumpCutoffPressure 		= 1250, 	-- bar
}

PPID_nano = {
	-- IMPORTANT initialize all PID settings in the table to be sure they reset all local customizations!!!

	PID_Test	 	= {P=1600, I=100, D=100, ResultMin=-4000000, ResultMax=4000000},
	PID_6_1_1	 	= {P=600, I=100, D=100, ResultMin=-2000000,  ResultMax=4000000},
	PID_6_2_1	 	= {P=600, I=200, D=100, ResultMin=-4000000,  ResultMax=4000000},
	PID_10_1_1	 	= {P=1000, I=100, D=100, ResultMin=-4000000,  ResultMax=4000000},
	PID_16_1_1	 	= {P=1600, I=100, D=100, ResultMin=-4000000,  ResultMax=4000000},
	PID_16_2_1	 	= {P=1600, I=200, D=100, ResultMin=-4000000,  ResultMax=4000000},
	PID_16_5_1	 	= {P=1600, I=500, D=100, ResultMin=-4000000,  ResultMax=4000000},
	PID_18_1_1	 	= {P=1800, I=100, D=100, ResultMin=-4000000,  ResultMax=4000000},
	PID_20_6_1	 	= {P=2000, I=600, D=100, ResultMin=-4000000,  ResultMax=4000000},
	PID_20_10_1	 	= {P=2000, I=1000, D=100, ResultMin=-800000, ResultMax=800000},
	PID_25_1_1 		= {P=2500, I=100, D=100, ResultMin=-4000000,  ResultMax=4000000},
	PID_25_12_1 	= {P=2500, I=1200, D=100, ResultMin=-4000000,  ResultMax=4000000},
	PID_50_2_1	 	= {P=5000, I=200, D=100, ResultMin=-4000000,  ResultMax=4000000}
}

PPID = PPID_nano
--if microElute then PPID = PPID_micro end

-- participant roles
GradientPumpRole = "pump"
LeftExecutorRole = "autosampler-left"
AuxExecutorRole = "autosampler-none"
LcStatusRole = "status"
JournalRole = "journal"

--[[
-- palplus modules
InjectionValveModule = "Valve Drive 1"
TrapValveModule = "Valve Drive 2"
WashPumpModule = "Pump 1"
WashStationModule = "LCMS Wash 1"
LcToolModule = "LCP 1"
LcInjectorModule = "LCInjector1"

SampleCoolingModule = "Peltier Stack 1"
SampleRackContainer = "Drawer 1"
]]
P.minAutoSamplerFWVersion = 4.16
P.maxAutoSamplerFWVersion = 4.99
P.maxItemPosition = 5						-- wash station
WashSolventLinerPenetrationDepth = 30 	-- mm
WashWasteLinerPenetrationDepth = 20 	-- mm
SyringePenetrationSpeed = 20			-- mm/s (5 = default from nE)
SyringeAspirationSpeed = 40				-- uL/s

-- sync points ('Lcxxx' are outgoing, 'Msxxx' are incoming)
LcElutionReady = "lc-ready"
MsAcquisitionStart = "ms-start"
LcElutionDone = "lc-done"
-- for backwards compatibility - deprecated
LcGradientStart = LcElutionReady
LcGradientDone = LcElutionDone

-- autosampler tray temperatures (Note: Off = 0 does not mean 0C, it means OFF)
--TrayTemperatures = { Off = 0, Low = 7, High = 14 }
TrayTemperatures = { Off = 0, ExtraLow = 7, Low = 9, Medium = 14, High = 14 }

-- valve positions
-- switch valve drive only if actual and set positions differs more then "palValveDriveTolerance"
P.palValveDriveTolerance = 3
-- angles in degrees clockwise
InjectionValve = { Inject = 0, Load = 60, Service = 0, Block = 30  }
-- angles in degrees clockwise
TrapValve = { Waste = 0, GradientT = 240, Analytical = 180, Trap = 60, GradientA = 120, Service = 0, InjectWaste = 300 }
-- angles in degrees clockwise
PumpValve = { Waste = 180, Solvent = 60, MixTee = 240, Compress = 300, Inject = 0, Service = 180, Compress120 = 120 }

-- virtual system component names - should this be in c# so we can access it from the plugin as well?
-- these should be treated as constants and be independent of locale, etc.
-- Elution lua files associated with a given application
ElutionTypes = {
	"OneColumnSeparation.lua",
	"FastOneColumnSeparation.lua",
	"TwoColumnSeparation.lua",
	-- "FastTwoColumnSeparation.lua",
	"DirectInfusion.lua",
	"Self-Test-Elution_Mode.lua"
	--	  "CustomMode.lua"
}

 -- Prepare.lua, Diagnose.lua, and Decompress.lua must remain separate and must always be present for programming reasons
MaintenanceTypes = {
	[StdDiag] = {"Prepare.lua", "DirectFlow.lua", "Column.lua", "Diagnose.lua", "Maintenance.lua", "Calibrate.lua", "Self_Testing.lua", "Service.lua", "ManualValveControl.lua"},
	[OQDiag] = {"Prepare.lua", "DirectFlow.lua", "Column.lua", "Diagnose.lua", "Maintenance.lua", "Calibrate.lua", "Service.lua", "ManualValveControl.lua"},
	[FactoryDiag] = {"Prepare.lua", "DirectFlow.lua", "Column.lua", "Diagnose.lua", "Maintenance.lua", "Calibrate.lua", "Service.lua", "MagicMix.lua", "PumpProduction.lua", "ValveLeakageTest.lua", "ValveSpeedTest.lua", "ValveTest.lua", "ProlabFunctions.lua", "ManualValveControl.lua"},
	[Production] = {"Prepare.lua", "DirectFlow.lua", "Column.lua", "Diagnose.lua", "Maintenance.lua", "Calibrate.lua", "MagicMix.lua", "Service.lua", "ManualValveControl.lua"},
	[ProductionPumpOnly] = {"Prepare.lua", "DirectFlow.lua", "Diagnose.lua", "MagicMix.lua", "PumpProduction.lua"},
	[ProductionFaellanden] = {"Prepare.lua", "PumpProduction.lua", "CapillaryID.lua"}
}

Naming = {
	ValveI = "valve-i",
	ValveT = "valve-t",
	ValveA = "valve-a",
	ValveB = "valve-b",
	PumpA = "pump-a",
	PumpB = "pump-b",
	SolventA = "solvent-a",
	SolventB = "solvent-b",
	FlowA = "flowsensor-a",
	FlowB = "flowsensor-b",
	MixTee = "mixing-tee",
	Trap = "trap-column",
	Separator = "separation-column",
	Loop = "loop",
	SampleCompartment = "sample-tray",
	TransferLine = "transfer-line",
	SetFlowA = "setflow-a",
	SetFlowB = "setflow-b",
    Oven = "oven",
	SetPoint = "setpoint",
	Plug = "plug",
	Waste = "waste",
	LineBreak = "linebreak",
	None = "none"
}

--Naming.ValveAGroove = Naming.ValveA
Naming.ValveAGroove = Naming.ValveA..":4-5"
Naming.ValveAWaste = Naming.ValveA.."_waste"
Naming.SolventToValveA = Naming.SolventA.."_to_"..Naming.ValveA
Naming.PumpARearToValve = Naming.PumpA.."_rear_to_"..Naming.ValveA
Naming.PumpAFrontToValve = Naming.PumpA.."_front_to_"..Naming.ValveA
Naming.ValveAToFS = Naming.ValveA.."_to_"..Naming.FlowA
Naming.ValveAToInjectionValve = Naming.ValveA.."_to_"..Naming.ValveI
Naming.ValveAToPlug = Naming.ValveA.."_to_"..Naming.Plug
Naming.FSAToMixTee = Naming.FlowA.."_to_"..Naming.MixTee

Naming.MixTeeToTrapValve = Naming.MixTee.."_to_"..Naming.ValveT
Naming.TrapValveToWaste = Naming.ValveT.."_waste"
Naming.InjectToTrap = Naming.ValveI.."_to_"..Naming.ValveT
Naming.InjectionValveToWaste = Naming.ValveI.."_waste"
Naming.ValveTLongGroove = Naming.ValveT..":5-1"
Naming.ValveTShortGroove = Naming.ValveT..":2-3"
Naming.ValveTGrooves = Naming.ValveT..":2-3:5-1"
Naming.ValveIGrooveInject = Naming.ValveI..":2-3"
Naming.ValveIGroovesLoad = Naming.ValveI..":6-1:5-4"
Naming.ValveIGrooveLoadwoLoop = Naming.ValveI..":5-4"
Naming.ValveIGroovesInjectLoop = Naming.ValveI..":6-1:2-3"

Naming.ValveBGroove = Naming.ValveB..":4-5"
Naming.ValveBWaste = Naming.ValveB.."_waste"
Naming.SolventToValveB = Naming.SolventB.."_to_"..Naming.ValveB
Naming.PumpBRearToValve = Naming.PumpB.."_rear_to_"..Naming.ValveB
Naming.PumpBFrontToValve = Naming.PumpB.."_front_to_"..Naming.ValveB
Naming.ValveBToFS = Naming.ValveB.."_to_"..Naming.FlowB
Naming.FSBToMixTee = Naming.FlowB.."_to_"..Naming.MixTee
Naming.ValveBToPlug = Naming.ValveB.."_to_"..Naming.Plug
Naming.ValveBPlug = Naming.ValveB.."_"..Naming.Plug

Naming.ValveTToSeparator = Naming.ValveT.."_to_"..Naming.Separator
Naming.WasteForValveT = Naming.Waste.."_for_"..Naming.ValveT
Naming.MixTeeToInjectionValve = Naming.MixTee.."_to_"..Naming.ValveI
Naming.InjectionValveToSeparator = Naming.ValveI.."_to_"..Naming.Separator
Naming.ValveAPlug = Naming.ValveA.."_"..Naming.Plug

Naming.LineBreakPumpA1 = Naming.LineBreak.."_"..Naming.PumpA.."1"
Naming.LineBreakPumpA2 = Naming.LineBreak.."_"..Naming.PumpA.."2"

Naming.IsColumnOvenConnected = "IsColumnOvenConnected"
Naming.NoOfItemPositions = "NoOfItemPositions"
Naming.ValveType = "ValveType"

Status = {
	Inject = "aspirate & inject",
	EquilibrateSeparator = "equilibrate "..Naming.Separator,
	EquilibrateTrap = "equilibrate "..Naming.Trap,
	LoadColumn = "load "..Naming.Trap,
	LoadSepColumn = "load "..Naming.Separator,
	InjectLoop = "inject "..Naming.Loop
}

SignalizeAll = {
	Naming.ValveI, Naming.ValveA, Naming.ValveB, Naming.PumpA, Naming.PumpB, Naming.SolventA, Naming.SolventB, Naming.FlowA, Naming.FlowB, Naming.MixTee, Naming.Separator, Naming.Loop, Naming.FSAToMixTee, Naming.ValveAToFS, Naming.ValveAGroove, Naming.ValveIGrooveLoadwoLoop, Naming.ValveIGroovesLoad, Naming.ValveIGrooveInject, Naming.ValveAWaste, Naming.SolventToValveA, Naming.PumpARearToValve, Naming.PumpAFrontToValve, Naming.InjectionValveToWaste, Naming.ValveBWaste, Naming.SolventToValveB, Naming.PumpBRearToValve, Naming.PumpBFrontToValve, Naming.ValveBToFS, Naming.FSBToMixTee, Naming.ValveT, Naming.Trap, Naming.MixTeeToTrapValve, Naming.TrapValveToWaste, Naming.ValveAToInjectionValve, Naming.InjectToTrap, Naming.ValveTGrooves, Naming.ValveTShortGroove, Naming.ValveTLongGroove, Naming.ValveTToSeparator, Naming.WasteForValveT, Naming.TransferLine
}

-- Length = mm, InnerDiameter = micron, ParticleDiameter = micron, particlePoreSize = Angstrom, MaxFlow = uL/min, 
-- MaxPressure = bar, MaxTemperature = degC, ColumnVolume = uL, UnityFlow = uL/bar, IsAdvancedSettings = 1 -> true
NoColumn = {
	Name = "None",
	Comment = "No column option",
	Length = "1",
	InnerDiameter = "0.2",
	ParticalDiameter = "2",
	ParticlePoreSize = "120",
	MaxFlow = "10",
--	MaxPressure = "1200",			-- PNS-763
	MaxPressure = "1000",			-- PNS-763
	MaxTemperature = "100",
	IsAdvancedSettings = "1",
	ColumnVolume = "0.001",
	UnityFlow = "1.0",
	ColumnPorosity = "0.42"
}

DiagramHideList = {
	Naming.ValveAToPlug,
	Naming.ValveAPlug,
	Naming.MixTeeToInjectionValve,
	Naming.InjectionValveToSeparator
}

ValveAngleToLinkList = {
		[Naming.ValveA] = {
			Naming.PumpAFrontToValve,			-- 0
			Naming.ValveAToFS,					-- 60
			Naming.ValveAToInjectionValve,		-- 120
			Naming.PumpARearToValve,			-- 180
			Naming.SolventToValveA,				-- 240
			Naming.ValveAWaste					-- 300
		},
		[Naming.ValveB] = {
			Naming.PumpBFrontToValve,			-- 0
			Naming.ValveBToFS,					-- 60
			Naming.None,						-- 120
			Naming.PumpBRearToValve,			-- 180
			Naming.SolventToValveB,				-- 240
			Naming.ValveBWaste					-- 300
		},
		[Naming.ValveI] = {
			Naming.None,						-- 0
			Naming.Loop,						-- 60
			Naming.InjectToTrap,				-- 120
			Naming.ValveAToInjectionValve,		-- 180
			Naming.Loop,						-- 240
			Naming.InjectionValveToWaste		-- 300
		},
		[Naming.ValveT] = {
			Naming.Trap,						-- 0
			Naming.InjectToTrap,				-- 60
			Naming.TransferLine,				-- 120
			Naming.Trap,						-- 180
			Naming.MixTeeToTrapValve,			-- 240
			Naming.TrapValveToWaste				-- 300
		}
}

-- colors for diagram Signalize
ColorsRGB = {
	Normal = {0, 0, 0},
	Blue = {15, 112, 184},
	Red = {193, 41, 45},
	Purple = {99, 50, 138},
	LightGray = {229, 229, 229},
	Gray = {204, 204, 204},
	DarkGray = {102, 102, 102},
	Goldenrod = {218, 165, 32},
	Green = { 0, 128, 0},
	GreenYellow = {173, 255, 47},
	Gold = {255, 215, 0},
	White = {255, 255, 255},
	Black = {0, 0, 0}
}

LedColors = {
	Full = {
		Red = {80, 0, 0},
		Green = {0, 180, 0},
		Blue = {0, 0, 180},
		White = {190, 150, 120},
		Yellow = {150, 100, 0}
	},
	Intermediate = {
		Red = {40, 0, 0},
		Green = {0, 75, 0},
		Blue = {0, 0, 75},
		White = {55, 45, 35},
		Yellow = {60, 40, 0}
	}
}

BackupJournalName = "BackupJournal.xml"
LCControlMaxTimeBuffer = 480 -- minutes
MaxElutionExeLogFiles = 5
TooltipImageDirectoryName = "Images"

-- Maintenance intervals
PrepareTimeInterval =			 { Days =  0, Hours = 12, Minutes = 0, Seconds = 0  }
ServiceModeTimeInterval =		 { Days =  1, Hours =  0, Minutes = 0, Seconds = 0  }
ExtLowTempWarnTimeInterval =	 { Days =  7, Hours =  0, Minutes = 0, Seconds = 0  }
TwinScapeDataTimeInterval =		 { Days =  0, Hours =  0, Minutes = 1, Seconds = 0  }  -- time interval to report TwinScape chart data
SelfTestWarnTimeInterval =		 { Days = 14, Hours =  0, Minutes = 0, Seconds = 0  }  -- time interval to report if self-testing has not been performed
SelfTestBkgdWaitTimeInterval =  { Days =  3, Hours =  0, Minutes = 0, Seconds = 0  }  -- time in standby or idle flow to wait until starting self-test procedures if queued
SelfTestBkgdCheckTimeInterval = { Days =  0, Hours =  0, Minutes = 0, Seconds = 10 }  -- polling time interval to check slf-diagnostic queued items for run

-- device status polling intervals
StatusDataIntervalMs = 100

-- Pump Volume logging
SystemInfoLoggingIntervalMin = 1.0
SystemInfoMaxLoggingEntries = 8

-- self test context Name
BkgdSelfTestName = "Self-test"

-- self diagnostic tests
FlowRestrictionAndLeakTest = "Flow Restriction and Leak Test"
Preparation = "Preparation"
LeakTestRamp = "Leak Test Ramp"

-- self diagnostic procedure arguments - MUST match exactly the "Self_Testing.lua Initialize function parameters"
LeakTestHighPressure = "Self LT High pressure"
LeakTestLowPressureAB = "Self LT Low pressure pump A and B"
LeakTestHighPressureAB = "Self HPLC seals conditioning"
LeakTestPressureRamp = "Self LT Pressure ramp test"
FlowRestrictPumpBMixTee = "Self FRT Pump B mixTee"
FlowRestrictInject = "Self FRT Injection system"
FlowRestrictPumpAMixTee = "Self FRT Pump A mixTee"
Preparation = "Preparation"

-- Self-diagnotic time intervals
BkgdFlowRestrictionAndLeakTest	= {Days =  3, Hours =  0, Minutes = 0, Seconds = 0 }
BkgdPreparation					= {Days =  3, Hours =  0, Minutes = 0, Seconds = 0 }
BkgdLeakTestRamp				= {Days =  7, Hours =  0, Minutes = 0, Seconds = 0 }

-- proteoElute Capillary default list
--            LinkName, Header, Length, LengthUnit, MinLength, MaxLength, DefaultLength, ID, MinID, MaxID, DefaultID, IDUnit, InnerDiameterTitle, LengthTitle, FactoryLength, FactoryID
CapillaryDefaults = {
	[0] =	{ Naming.Loop, "Loop", 20, "mm", 1, 1000, 20, 150, 1, 1000, 150, "uL", "Inner Diameter:", "Volume:", 20, 150 },
	[1] =	{ Naming.ValveAToInjectionValve, "Loading Capillary", 350, "mm", 10, 1000, 350, 25, 1, 1000, 25, "um", "Inner Diameter:", "Length:", 350, 25 },
	[2] =	{ Naming.ValveAToFS, "Flow Sensor A Capillary", 250, "mm", 10, 1000, 250, 25, 1, 1000, 25, "um", "Inner Diameter:", "Length:", 250, 25 },
	[3] =	{ Naming.InjectToTrap, "Injection Capillary", 150, "mm", 10, 1000, 150, 25, 1, 1000, 25, "um", "Inner Diameter:", "Length:", 150, 25 },
	[4] =	{ Naming.MixTeeToTrapValve, "Mix-Tee Capillary", 100, "mm", 10, 1000, 100, 25, 1, 1000, 25, "um", "Inner Diameter:", "Length:", 100, 25 },
	[5] =	{ Naming.FSAToMixTee, "Restriction A Capillary", 180, "mm", 10, 1000, 180, 10, 1, 1000, 10, "um", "Inner Diameter:", "Length:", 180, 10 },
	[6] =	{ Naming.FSBToMixTee, "Restriction B Capillary", 180, "mm", 10, 1000, 180, 10, 1, 1000, 10, "um", "Inner Diameter:", "Length:", 180, 10 },
	[7] =	{ Naming.ValveBToFS, "Flow Sensor B Capillary", 250, "mm", 10, 1000, 250, 25, 1, 1000, 25, "um", "Inner Diameter:", "Length:", 250, 25 },
	[8] =	{ Naming.Trap, "Trap Capillary", 200, "mm", 10, 1000, 200, 30, 1, 1000, 30, "um", "Inner Diameter:", "Length:", 200, 30 },
	[9] =	{ Naming.TransferLine, "Transfer Capillary", 550, "mm", 10, 1000, 550, 25, 1, 1000, 25, "um", "Inner Diameter:", "Length:", 550, 25 }
}

-- Additional tray types can be added here.
-- IMPORTANT! - added tray types must conform to the vial layout as defined in one of the pre-defined tray types ("proteoElute VT54", "proteoElute MTP96", "proteoEluteMTP384"),
--				In the HyStar sample table tray type selection, one of these must be selected that represents rows and columns of the added tray
--
--	[index] = { "plug-in display name", "CTC PAL object name" }
-- IMPORTANT! - The added tray "CTC PAL object name" must be available in the CTC as a tray type object. If not avaialble in the PAL, the plug-in will report an error at startup
-- EXAMPLE - [0] = { "Bruker Custom", "Custom1"}
AdditionalTrayTypes = {
}

-- ============================================================
-- Valve Position Labels (for human-readable display)
-- ============================================================
PumpValveLabels = {
	[0]   = "Inject",
	[60]  = "Solvent",
	[120] = "Compress120",
	[180] = "Waste",
	[240] = "MixTee",
	[300] = "Compress"
}
InjectionValveLabels = {
	[0]  = "Inject",
	[30] = "Block",
	[60] = "Load"
}
TrapValveLabels = {
	[0]   = "Waste",
	[60]  = "Trap",
	[120] = "GradientA",
	[180] = "Analytical",
	[240] = "GradientT",
	[300] = "InjectWaste"
}

-- ============================================================
-- Signalization Color Schemes for different flow states
-- ============================================================
FlowColors = {
	SolventA    = ColorsRGB.Blue,
	SolventB    = ColorsRGB.Red,
	Mixed       = ColorsRGB.Purple,
	Idle        = ColorsRGB.Gray,
	Active      = ColorsRGB.Green,
	Warning     = ColorsRGB.Goldenrod,
	Error       = ColorsRGB.Red,
	Highlighted = ColorsRGB.GreenYellow
}

return P
