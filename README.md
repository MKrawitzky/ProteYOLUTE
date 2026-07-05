# ProteYOLUTE

**Proteo** (proteomics) + **YOLO** (you only live once) + **Elute** (chromatography)

*"We're eluting proteins and we don't care about the rules anymore."*

ProteYOLUTE is a community-driven rewrite of the Bruker proteoElute UHPLC HyStar plugin. Born from frustration with cryptic error messages, removed features, and uninspiring user experience — we're taking it back and making it better.

---

## The Story

The proteoElute is the latest in a long lineage of nano-flow liquid chromatography systems:

```
Proxeon UHPLC (Odense, Denmark) — touch screen, manual valve control
  -> Easy-nLC (Bruker/Thermo) — touch screen, manual valve control
    -> nanoElute 1 & 2 (Bruker) — Lua scripting, PAL3 autosampler
      -> proteoElute (Bruker) — PAL3 Series II, rainbow LED, outsourced plugin
```

When Bruker moved from nanoElute to proteoElute, they outsourced the plugin development to cut costs. The result: quality dropped, error messages became cryptic (`"!!! Reloading gradient failed 10 times !!!"` followed by `"Contact service department"`), manual valve control disappeared, and the rainbow LED hardware sat mostly unused.

ProteYOLUTE fixes all of that.

---

## What's Changed

### Phase 1 — Lua Business Logic (Complete)

**Error Messages Rewritten**
- 25+ Lua files modified across the entire plugin
- 40+ instances of bare `"Initialization failed"` replaced with actionable diagnostics
- Cryptic `"!!!"` marker messages replaced with clear explanations including likely causes and remediation steps
- `"Contact service department"` replaced with actual troubleshooting guidance
- Typos and grammar errors fixed (`"Could not establisch"`, `"Pressure could not reached"`)

**Manual Valve Control Restored** (`ManualValveControl.lua`)
- Interactive control of all four valve types (Pump A, Pump B, Injection, Trap)
- Named positions with degree labels — no memorizing angles
- Safety: automatic pressure reduction before switching
- Live pressure/flow/volume monitoring during operation
- Pump control: constant pressure, constant speed, or stop with timed operation

**LED Effects Engine** (`Packages/led_effects.lua`)
- 17 named color themes: Rainbow, Ocean, Sunset, Forest, Lava, Ice, Night, Orange & Black, Red & Black, Blue & White, Green & Black, Purple & Black, Amber & Black, Pink & Black, Cyan & Black, White Pulse, Warm Glow
- 3 pattern modes: Pulsating (breathing), Moving (chasing), Solid
- Rainbow cycling mode with 10-step color sequence
- Status-reactive LED: automatic color based on system state (idle/running/error/warning)
- Uses the discovered `Pump.SetLedPattern(LedPattern, foreground, background)` API

**Enhanced Diagram Signalization** (`Packages/signalize_helpers.lua`)
- Pre-built composite flow visualizations: elution, loading, idle, error highlighting
- Color-coded by solvent channel (Blue = A, Red = B, Purple = mixed)
- Error highlighting: dims everything, highlights problem components in red

**System Improvements** (`Packages/baltic.lua`)
- Human-readable valve position label tables
- Semantic flow color scheme
- ManualValveControl registered in all maintenance procedure lists

---

## Vision

### Phase 2 — UI Overhaul
- Modern dark-theme WPF interface inspired by NVIDIA GeForce Experience
- Interactive valve diagrams — click to switch, like the old Proxeon touch screens
- Real-time pressure/flow charts with smooth animations
- LED control panel in the UI with live preview
- Guided troubleshooting wizard when errors occur

### Phase 3 — Full Experience
- Custom LED animation engine with user-defined color sequences
- Visual gradient editor with drag-and-drop time points
- Method builder with real-time flow path preview
- Maintenance scheduling with clear, friendly reminders
- System health dashboard

### Long-Term
- Cross-platform compatibility research (nanoElute, Evosep One share the same CTC PAL3 platform)
- Open plugin architecture for community extensions
- Better integration with mass spectrometry acquisition software

---

## Architecture

The proteoElute HyStar plugin consists of:

- **HyStar** (`HyStarNT.exe`) — the host chromatography application
- **.NET DLLs** — `Bruker.Lc.dll`, `BalticWpfControlLib.dll`, `BalticHyStarControl.dll` — the compiled plugin framework
- **Lua scripts** (this repo) — business logic, procedures, and configuration loaded at runtime via NLua/KeraLua
- **PAL3 autosampler** — controlled via `PalPlusDriver.dll` and CTC PALplus API
- **Zirconium pump** — controlled via `ZrHAL.dll` and `Bruker.Zirconium.Pump.Communication.dll`
- **Rainbow LED** — controlled via `Pump.SetLedPattern()` with `LedColor.FromRgb()` and `LedPattern` enum

The Lua files in this repo are loaded from `C:\BDalSystemData\HyStar\LcPlugin\PrivateData\Bruker proteoElute\` and can be modified without recompiling any DLLs.

---

## File Structure

```
Bruker proteoElute/
├── ManualValveControl.lua          [NEW] Interactive valve/pump/LED control
├── Calibrate.lua                   Calibration procedures
├── Column.lua                      Column diagnostics
├── Decompress.lua                  System decompression
├── Diagnose.lua                    Diagnostic launcher
├── DirectFlow.lua                  Direct flow delivery
├── DirectInfusion.lua              Direct infusion mode
├── DotnetTypeDefinitions.lua       .NET API type definitions for Lua
├── FastOneColumnSeparation.lua     Fast single-column separation
├── FastTwoColumnSeparation.lua     Fast dual-column separation
├── Idle.lua                        Idle/standby flow
├── Maintenance.lua                 Maintenance procedures
├── OneColumnSeparation.lua         Single-column separation
├── Prepare.lua                     System preparation
├── Service.lua                     Service procedures
├── TwoColumnSeparation.lua         Dual-column separation
├── Images/                         System diagrams (PNG)
└── Packages/
    ├── led_effects.lua             [NEW] LED color themes and patterns
    ├── signalize_helpers.lua       [NEW] Enhanced diagram visualization
    ├── baltic.lua                  Core system configuration (enhanced)
    ├── zirconium.lua               Pump hardware control (errors improved)
    ├── pump_functions.lua          Pump operations (errors improved)
    ├── palplus.lua                 PAL autosampler integration
    ├── degas.lua                   Degassing procedures (errors improved)
    ├── Diagnostics.lua             Diagnostic test suite
    └── ...                         Additional support modules
```

---

## Disclaimer

This project is an independent community modification. It is not affiliated with, endorsed by, or supported by Bruker Corporation. Use at your own risk. Always maintain backups of your original plugin files before applying modifications.

---

*ProteYOLUTE — because your LC deserves better.*
