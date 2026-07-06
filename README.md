# ProteYOLUTE

**Proteo** (proteomics) + **YOLO** (you only live once) + **Elute** (chromatography)

*"We're eluting proteins and we don't care about the rules anymore."*

**Owner: Michael Krawitzky**

ProteYOLUTE is a complete rewrite of the Bruker proteoElute UHPLC HyStar plugin — both the Lua business logic and the compiled .NET DLLs. What started as error message improvements evolved into full ownership of both plugin DLLs, a pop-out diagram viewer, LED effects engine, manual valve control, and custom tooling.

---

## What's Been Done

### Phase 1 — Lua Business Logic
- **50+ error messages rewritten** across 25 Lua files with clear, actionable diagnostics
- **ManualValveControl.lua** — interactive valve/pump/LED control procedure
- **led_effects.lua** — 17 LED color themes with pulsating/moving/solid patterns
- **signalize_helpers.lua** — enhanced diagram flow visualization

### Phase 2 — DLL Ownership (Both DLLs fully decompiled and buildable)

**BalticWpfControlLib.dll** — Main plugin UI control
- Fully decompiled from 28,433 errors to 0
- All compiler-generated artifacts fixed
- 8 local functions inlined from original IL
- Pop-out diagram: double-click to open in resizable window
- Build pipeline: Framework MSBuild + WinFX targets + Roslyn 4.12

**BrukerLC.Styling.dll** — Diagram layout and component styles
- Fully decompiled XAML templates
- Diagram layout redesigned with correct component positions
- Pressure sensors, mixing tee, peltier stack added
- 3D cylindrical pump visuals with gradient shading
- 3-second build-deploy cycle

### Tools
- **Diagram Editor** (`Tools/DiagramEditor/`) — drag-and-drop component positioning with export
- **Valve Port Editor** (`Tools/ValveEditor/`) — 6-port IDEX valve assignment tool with rotor seal visualization

---

## Valve Port Assignments (IDEX 6-port, CCW: P1=12, P2=10, P3=8, P4=6, P5=4, P6=2 o'clock)

### Valve A (Pump A)
| Port | Clock | Connection |
|------|-------|------------|
| P1 | 12 | From Pressure Sensor A |
| P2 | 10 | To Waste |
| P3 | 8 | From Solvent Bottle A |
| P4 | 6 | From Pump Head |
| P5 | 4 | To Injection Valve P4 |
| P6 | 2 | To Pressure Sensor A |

### Valve B (Pump B)
| Port | Clock | Connection |
|------|-------|------------|
| P1 | 12 | From Pressure Sensor B |
| P2 | 10 | To Waste |
| P3 | 8 | From Solvent Bottle B |
| P4 | 6 | From Pump Head |
| P5 | 4 | **PLUGGED** |
| P6 | 2 | To Pressure Sensor B |

### Injection Valve (I)
| Port | Clock | Connection |
|------|-------|------------|
| P1 | 12 | Injector Cup/Port |
| P2 | 10 | Waste |
| P3 | 8 | Sample Loop (to P6) |
| P4 | 6 | From Valve A P5 |
| P5 | 4 | To Trap Valve P6 |
| P6 | 2 | Sample Loop (to P3) |

### Trap Valve (T)
| Port | Clock | Connection |
|------|-------|------------|
| P1 | 12 | Trap Column (to P4) |
| P2 | 10 | Waste |
| P3 | 8 | From Mixing Tee |
| P4 | 6 | Trap Column (to P1) |
| P5 | 4 | To Transfer Line/Column/MS |
| P6 | 2 | From Injection Valve P5 |

### Flow Path
```
Pump Head → Valve P4 → P1 → Pressure Sensor → P6 → Flow Sensor (inline) → Mixing Tee
Mixing Tee → Trap Valve P3
Valve A P5 → Injection Valve P4
Injection Valve P5 → Trap Valve P6
Trap Valve P5 → Transfer Line → Column → MS
```

---

## File Structure

```
Bruker proteoElute/
├── ManualValveControl.lua              Interactive valve/pump/LED control
├── Packages/
│   ├── led_effects.lua                 LED color themes and patterns
│   ├── signalize_helpers.lua           Enhanced diagram visualization
│   ├── baltic.lua                      Core system configuration
│   ├── zirconium.lua                   Pump hardware control
│   ├── pump_functions.lua              Pump operations
│   └── ...                             All other Lua modules
├── Tools/
│   ├── DiagramEditor/                  Drag-and-drop diagram layout editor
│   └── ValveEditor/                    6-port valve assignment tool
├── DLL_Extract/
│   ├── dnspy_baltic/BalticWpfControlLib/   Decompiled main plugin DLL (buildable)
│   ├── dnspy_styling/BrukerLC.Styling/     Decompiled styling DLL (buildable)
│   └── fix_decompile_v2.py                 Decompilation artifact fixer
└── Images/                             System diagram tooltips (PNG)
```

---

## Build Pipeline

### Lua (no build needed)
Edit files in `C:\BDalSystemData\HyStar\LcPlugin\PrivateData\Bruker proteoElute\` — changes take effect next procedure run.

### BrukerLC.Styling.dll (diagram layout)
```
MSBuild BrukerLC.Styling.csproj → copy to proteoElute folder → restart HyStar
```
3-second build-deploy cycle.

### BalticWpfControlLib.dll (main plugin)
```
MSBuild + Roslyn 4.12 → copy to proteoElute folder → restart HyStar
```
Requires .NET Framework MSBuild with WinFX targets + standalone Roslyn compiler.

---

## The Story

The proteoElute descends from a long lineage of nano-flow LC systems:

```
Proxeon UHPLC (Odense, Denmark) — touch screen, manual valve control
  → Easy-nLC (Bruker/Thermo) — touch screen, manual valve control
    → nanoElute 1 & 2 (Bruker) — Lua scripting, PAL3 autosampler
      → proteoElute (Bruker) — PAL3 Series II, rainbow LED
        → ProteYOLUTE — community rewrite, full ownership
```

---

## Vision

- Configurable hardware setups: single cell, high flow, trap-less
- Animated flow arrows showing real-time solvent direction
- Custom LED animation sequences
- Visual gradient editor
- System health dashboard
- Cross-platform research (nanoElute, Evosep share CTC PAL3 platform)

---

## Disclaimer

This project is an independent modification by Michael Krawitzky. It is not affiliated with, endorsed by, or supported by Bruker Corporation. Use at your own risk. Always maintain backups of original plugin files before applying modifications.

---

*ProteYOLUTE — because your LC deserves better.*
