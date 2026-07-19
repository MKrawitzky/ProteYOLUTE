# ProteYOLUTE — Future Roadmap

**Author:** Michael Krawitzky
**Date:** July 18, 2026
**Vision:** Transform ProteYOLUTE from a plugin rewrite into the most intelligent nano-flow LC platform in existence

---

## Where We Are Today

ProteYOLUTE controls a dual binary pump system with 4 rotary valves, pressure/flow sensors, PAL3 autosampler, and LED indicators. The current codebase handles 6 separation modes, comprehensive diagnostics, sensor calibration, and manual control — all through 28,476 lines of Lua and two rebuilt .NET DLLs.

**What works well:** Pump PID control (13 profiles), gradient strategies with dead volume compensation, leak/flow restriction diagnostics, LED effects, manual valve control, CSV logging.

**What's missing:** Everything that makes software intelligent.

---

## Phase 1 — Foundation (Months 1-3)
### *"Make the invisible visible"*

### 1.1 Real-Time System Dashboard

**Problem:** The current system writes CSV at end-of-run. During a 120-minute gradient, you're blind to what's happening inside the LC.

**Solution:** A live dashboard overlay in the HyStar diagram panel showing:

- **Pressure traces** — Dual-channel real-time pressure graph (A & B) with rolling 60-second window
- **Flow rate monitor** — Actual vs. setpoint flow with deviation highlighting (currently only checked every 10s in a 12-value ring buffer)
- **Gradient progress** — Visual gradient profile with current position marker, elapsed/remaining time, %B composition
- **Pump volume remaining** — Cylinder fill level for both pumps (max 1350 µL per head) with refill countdown
- **Valve state indicator** — Current position of all 4 valves with last-switch timestamp
- **Temperature** — Column oven temp with setpoint vs. actual

**Implementation:** Extend the `signalize_helpers.lua` data update loop (currently 2s interval) to push data into a shared memory buffer. Add a WPF overlay in BalticWpfControlLib that reads this buffer and renders with LiveCharts or OxyPlot.

**Technical notes:**
- Current flow monitoring uses a 12-value ring buffer with ±5% deviation threshold — expose this to the UI
- Current pressure read: `pump:GetCurrentPressure(channel)` — sample at 500ms for smooth traces
- Gradient dead volume is 0.46 µL (66nL mix tee + capillary + valve grooves) — show this as a "gradient delay" indicator

---

### 1.2 Structured Data Logging (Replace CSV)

**Problem:** `csv_file_logging.lua` writes `Date Time;Test;Name;Value;Unit` to a flat file. No querying, no cross-run analysis, no audit trail.

**Solution:** SQLite database per instrument with structured tables:

```
runs (run_id, method, start_time, end_time, status, operator, notes)
pressure_log (run_id, timestamp_ms, channel, pressure_bar, setpoint_bar)
flow_log (run_id, timestamp_ms, channel, flow_ul_min, setpoint_ul_min)
valve_events (run_id, timestamp_ms, valve_name, from_position, to_position)
gradient_log (run_id, timestamp_ms, percent_b, flow_total)
diagnostics (run_id, test_name, parameter, value, unit, pass_fail)
calibrations (cal_id, timestamp, sensor, type, old_value, new_value, result)
errors (run_id, timestamp, severity, message, context)
```

**Why SQLite:** Zero-config, single-file database, ships with .NET Framework, no server needed. File lives alongside the plugin. Can be queried with any SQLite tool, exported to CSV/Excel on demand.

**Migration:** Keep CSV export as an option for backwards compatibility. New data goes to both SQLite and CSV during transition.

---

### 1.3 Method Templates & Presets

**Problem:** Every method is configured from scratch. Users repeatedly enter the same parameters for standard workflows.

**Solution:** A method template system:

- **Save current method as template** — stores all parameters as a named preset
- **Load template** — populates all fields, user can modify before running
- **Built-in templates:** Standard proteomics gradients (30min, 60min, 90min, 120min), trap-only loading, direct infusion, column conditioning
- **Template diff** — compare two templates side-by-side, highlight differences
- **Export/import** — share templates as JSON files between instruments

**Storage:** SQLite `templates` table with JSON parameter blob + metadata (name, author, date, description, column type, flow rate range).

---

### 1.4 System Health Ledger

**Problem:** No cumulative tracking. You don't know how many hours the pump has run, how many valve switches have occurred, or when the last calibration was.

**Solution:** Persistent counters in SQLite:

```
health_counters:
  pump_a_hours, pump_b_hours
  pump_a_volume_total_ul, pump_b_volume_total_ul
  valve_a_switches, valve_b_switches, valve_i_switches, valve_t_switches
  max_pressure_ever_a, max_pressure_ever_b
  total_runs, total_injections
  last_calibration_date, last_diagnostic_date
  column_injections (per column serial number)
  seal_install_date_a, seal_install_date_b
```

**Display:** Summary card in the dashboard: "Pump A: 1,247 hours | Valve I: 14,892 switches | Last cal: 3 days ago"

**Alerts:** Configurable thresholds — "Valve A has exceeded 20,000 switches, recommend inspection" or "Last calibration was 30 days ago"

---

## Phase 2 — Intelligence (Months 3-6)
### *"The LC that learns from itself"*

### 2.1 Pressure Anomaly Detection

**Problem:** The current system has hardcoded leak thresholds (e.g., >0.05 µL/min = fail, Route 1: 0.5 µL/min, Route 2: 1.5 µL/min). These don't adapt to the specific instrument or column.

**Solution:** Statistical anomaly detection using the pressure/flow log:

- **Baseline profiling:** After 10+ runs with the same method, build a statistical model of expected pressure at each gradient timepoint (mean ± 2σ)
- **Runtime comparison:** During a run, compare live pressure to the baseline envelope. Flag deviations in real-time:
  - Pressure rising faster than expected → column clogging, frit blockage
  - Pressure dropping → leak developing, valve not seating
  - Pressure oscillation → air bubble, pump seal degradation
  - Sudden pressure spike → blockage forming
- **Trend analysis:** Track the pressure baseline drift over weeks. Gradual upward trend = column aging. Plot column lifetime curve.

**Implementation:** Moving average + standard deviation model stored in SQLite. No ML framework needed — pure statistics. Upgrade to LSTM or autoencoder models in Phase 4.

**Current data available:**
- Pressure sampled via `pump:GetCurrentPressure(channel)` (both A and B)
- Flow sampled via flow sensors with calibrated offset/factor
- PID profiles provide expected response characteristics (P=600-5000, I=100-1200, D=100)

---

### 2.2 Column Lifecycle Management

**Problem:** Columns degrade over time but there's no tracking. Users replace columns reactively (after separation quality drops) rather than proactively.

**Solution:**

- **Column registry:** Database of installed columns with serial number, type, particle size, ID, length, max pressure, install date
- **Injection counter per column:** Track total injections, total volume passed, total hours under pressure
- **Performance trending:** Track back-pressure at standard conditions over time. Increasing back-pressure = column aging. Calculate estimated remaining life based on degradation rate.
- **Column library:** Pre-loaded parameters for common columns:
  - PepSep Advanced (Bruker)
  - nanoViper (Thermo)
  - Aurora (IonOpticks)
  - Custom user-defined columns

**Column database fields:**
```
columns:
  serial, type, particle_size_um, inner_diameter_um, length_cm
  max_pressure_bar, pore_size_angstrom
  install_date, total_injections, total_volume_ul
  baseline_pressure_at_standard_flow
  current_performance_score (0-100%)
  estimated_remaining_injections
```

---

### 2.3 Intelligent Error Recovery

**Problem:** Most errors abort the procedure immediately. A single pressure spike or temporary flow deviation kills a 2-hour run.

**Solution:** Tiered error handling:

- **Tier 1 — Auto-retry:** Transient errors (pressure spike <2s, flow deviation <5%) → pause, wait 5s, retry. Log the event.
- **Tier 2 — Safe-state recovery:** Persistent errors → move to safe state (decompress, switch valves to waste), notify user, offer "Resume" or "Abort"
- **Tier 3 — Graceful shutdown:** Critical errors (>max pressure, hardware fault) → emergency decompress, all valves to waste, pumps stop. Full error report generated.

**Decision tree parameters** (configurable, not hardcoded):
```
pressure_spike_max_duration_s: 2
pressure_spike_max_bar_over: 50
flow_deviation_retry_threshold_percent: 5
flow_deviation_abort_threshold_percent: 15
max_retries_per_run: 3
recovery_wait_time_s: 5
```

**Current state:** `pump_functions.lua` has a 10-attempt retry loop for pressure setpoint verification and a basic `isGradientOK()` check. This would be extended, not replaced.

---

### 2.4 Adaptive PID Tuning

**Problem:** 13 PID profiles are hardcoded in `baltic.lua` (P=600-5000, I=100-1200, D=100). The correct profile depends on column, flow rate, solvent composition, and system configuration — but the user must choose manually.

**Solution:**

- **Auto-select PID profile** based on method parameters:
  - Flow rate <0.3 µL/min → conservative PID (6_1_1)
  - Flow rate 0.3-1.0 µL/min → moderate PID (16_1_1)
  - Flow rate >1.0 µL/min → aggressive PID (25_12_1)
  - High %B (>60% ACN) → adjusted for lower viscosity
- **Runtime PID adaptation:** Monitor pressure oscillation amplitude. If oscillating:
  - Reduce P (proportional) by 10%
  - Increase I (integral) by 10%
  - Log the adaptation
- **Learning:** Store optimal PID parameters per method in the template. Over time, each method converges to its ideal PID tuning.

---

## Phase 3 — Connected Lab (Months 6-12)
### *"The LC that talks to everything"*

### 3.1 REST API

**Problem:** ProteYOLUTE is locked inside HyStar. No external software can query its status, start a run, or retrieve data.

**Solution:** Lightweight HTTP API server running alongside the plugin:

```
GET  /api/status                → System state, pressures, flows, valve positions
GET  /api/health                → Health counters, alerts, last calibration
GET  /api/runs                  → Run history with filtering
GET  /api/runs/{id}/data        → Pressure/flow/gradient data for a specific run
GET  /api/columns               → Column registry
GET  /api/templates             → Method templates
POST /api/runs/start            → Start a run with parameters
POST /api/runs/{id}/stop        → Stop a running procedure
GET  /api/diagnostics/latest    → Latest diagnostic results
GET  /api/alerts                → Active alerts and warnings
WS   /api/stream                → WebSocket for real-time pressure/flow/status
```

**Implementation:** .NET `HttpListener` embedded in the plugin DLL, running on localhost:8742. JSON responses. Optional API key authentication for remote access.

**Use cases:**
- LIMS integration (pull results, push sample lists)
- Jupyter notebooks (analyze run data programmatically)
- Mobile monitoring (phone browser → dashboard)
- Multi-instrument fleet view (aggregate API from multiple LCs)

---

### 3.2 Web Dashboard

**Problem:** You must be at the HyStar workstation to see anything.

**Solution:** Single-page web application served by the REST API:

- **Live view:** Real-time pressure/flow/gradient charts via WebSocket
- **Run history:** Searchable table with sorting, filtering, export
- **System health:** Counters, alerts, maintenance schedule
- **Column tracker:** Performance trends, injection counts, estimated life
- **Method library:** Browse, compare, and export templates
- **Responsive design:** Works on phone, tablet, and desktop

**Tech stack:** Vanilla HTML/JS/CSS served from embedded resources in the DLL. No Node.js, no build step, no dependencies. Just open a browser.

---

### 3.3 Webhook & Event System

**Problem:** No way to trigger external actions based on LC events.

**Solution:** Configurable webhooks:

```
Event triggers:
  run.started     → POST to LIMS with run metadata
  run.completed   → POST to LIMS with results summary
  run.failed      → POST to Slack/Teams with error details
  alert.triggered → POST to email gateway
  calibration.due → POST to maintenance scheduler
  column.degraded → POST to ordering system
```

**Configuration:** JSON file defining URL, event type, payload template, retry policy.

**Protocol support:** HTTP POST (webhooks), MQTT publish (IoT), and file drop (for legacy LIMS that watch a folder).

---

### 3.4 SiLA 2 Interface

**Problem:** No standard instrument control protocol. HyStar uses proprietary COM objects.

**Solution:** SiLA 2 (Standardization in Lab Automation) server exposing the LC as a discoverable lab device:

- **Feature: PumpControl** — SetFlow, SetPressure, Stop, GetStatus
- **Feature: ValveControl** — SetPosition, GetPosition (all 4 valves)
- **Feature: GradientControl** — LoadMethod, StartGradient, GetProgress
- **Feature: SampleInjection** — Inject, SetVolume, GetLoopStatus
- **Feature: Diagnostics** — RunLeakTest, RunFlowTest, GetResults
- **Feature: Observable Properties** — Stream pressure, flow, temperature, valve state

**Why SiLA 2:** It's becoming the standard for lab instrument interoperability (gRPC/HTTP/2, mDNS discovery, typed commands). Adopted by Sartorius, Hamilton, Beckman, and others. This makes ProteYOLUTE compatible with any SiLA 2 orchestration platform.

---

## Phase 4 — Predictive & Autonomous (Months 12-24)
### *"The LC that fixes itself"*

### 4.1 Machine Learning Pressure Models

**Problem:** Statistical anomaly detection (Phase 2) catches obvious problems. Subtle degradation patterns require pattern recognition.

**Solution:** Lightweight ML models trained on the instrument's own data:

- **LSTM autoencoder** for pressure time-series anomaly detection
  - Input: 60-second pressure windows (120 samples at 2Hz)
  - Training: Reconstruct "normal" pressure patterns from historical good runs
  - Anomaly: High reconstruction error = something is wrong
  - Deployed as ONNX model, runs in .NET (ML.NET or ONNX Runtime)

- **Gradient fingerprint classifier**
  - Each column+method combination produces a characteristic pressure "fingerprint"
  - Train a classifier to detect: normal, partially blocked, leaking, air bubble, wrong column
  - Alert: "Pressure pattern suggests partial frit blockage — recommend backflush"

- **Remaining useful life (RUL) predictor**
  - Track column back-pressure trend over injections
  - Fit degradation curve (exponential + noise)
  - Predict: "Column has ~150 injections remaining before back-pressure exceeds 800 bar"

**Data requirements:** 50+ runs for baseline, 200+ runs for reliable ML models. Models retrain monthly or on-demand. All inference runs locally — no cloud dependency.

---

### 4.2 Digital Twin Simulation

**Problem:** You can't preview a method without running the actual pump. This wastes sample, solvent, and column life.

**Solution:** A physics-informed virtual LC that simulates the fluidic system:

**Fluid dynamics model:**
- Hagen-Poiseuille flow through capillaries (ID 20-75 µm, lengths from `baltic.lua`)
- Kozeny-Carman column model (particle size, porosity, column dimensions)
- Viscosity model already exists: `chromatography.lua` has H₂O/ACN viscosity calculation with temperature dependence
- Dead volume model: 0.46 µL total (mix tee 66nL + capillaries + valve grooves)

**What the simulator calculates:**
- Expected pressure at every gradient timepoint (using real column parameters)
- Gradient delay through dead volume (when does %B actually reach the column?)
- Total solvent consumption (do the 1350 µL pump heads have enough volume?)
- Estimated run time including refills, valve switches, equilibration
- Peak compression/broadening estimates based on flow rate and column dimensions

**UI:** Side panel in the method editor showing simulated pressure trace, gradient profile at column inlet (after dead volume delay), and predicted solvent consumption. "Virtual Run" button that completes in 2 seconds.

**Accuracy target:** ±10% pressure prediction within 10 runs of calibration data for a given column.

---

### 4.3 Autonomous Method Optimization

**Problem:** Method development is manual. The user guesses gradient parameters, runs the method, evaluates results, adjusts, repeats.

**Solution:** Bayesian optimization loop:

1. **User defines:** Target analytes (optional), column type, max run time, acceptable pressure range
2. **System runs:** Scouting gradient (wide, slow ramp)
3. **Evaluation:** Peak count, resolution, asymmetry, total analysis time (from MS data via API or user input)
4. **Optimizer proposes:** Next gradient to try (adjusted slope, start %B, end %B, flow rate)
5. **Iterate:** 5-10 runs converges to optimized method
6. **Save:** Optimized method stored as template with optimization history

**Algorithm:** Gaussian Process surrogate model with Expected Improvement acquisition function. Constraints: max pressure, min flow, valve timing limits. Prior knowledge from chromatographic theory (higher %B = lower viscosity = lower pressure, etc.).

**Near-term implementation:** Manual loop — system proposes, user approves and runs. Future: fully autonomous scouting with auto-injection.

---

### 4.4 Self-Calibrating Sensors

**Problem:** Calibration requires dedicated procedures (60+ minutes for flow sensor, manual pressure offset at zero flow). Users skip calibration until results degrade.

**Solution:** Continuous background calibration:

- **Pressure offset:** At every idle transition (pump stopped, valves to waste), sample pressure for 5 seconds. If reading deviates from expected zero by >2 bar, apply correction. Log drift over time.
  - Current offset values: 129.239 bar (1000 bar sensor), 211 bar (1600 bar sensor)
  - Acceptable range: 140-350 bar (1600 bar sensor)
  
- **Flow sensor:** During every constant-flow segment, compare measured flow to pump displacement rate. Track deviation over time. If deviation exceeds 3%, flag for manual calibration. Small drifts (<1%) can be auto-corrected.

- **Cross-validation:** During diagnostic tests, compare pump A and pump B readings at the same flow path. Discrepancy indicates sensor drift vs. actual leak.

---

## Phase 5 — Next-Generation (Years 2-5)
### *"The LC from 2036"*

### 5.1 Natural Language Method Builder

**Problem:** Method parameters are numeric fields in a form. Learning the system requires training.

**Solution:** LLM-powered method interpreter:

```
User: "Run a 90-minute gradient from 2% to 35% B at 300 nL/min 
       with a 15-minute trap loading step at 800 nL/min"

System: [Generates method parameters]
  Gradient: 2%B → 35%B over 90 min
  Flow: 0.300 µL/min
  Trap loading: 0.800 µL/min for 15 min
  Column: [current installed column]
  Pressure limit: [from column spec]
  Equilibration: 10 column volumes at 2%B
  
  [Preview] [Edit] [Run]
```

**Implementation:** Local LLM (Phi-3, Llama-3 8B quantized) or API call to cloud model. Input: natural language + system context (installed column, last method, hardware config). Output: structured method parameters validated against hardware limits.

**Training data:** 500+ example method descriptions paired with parameter sets. Generated from existing templates + proteomics literature.

---

### 5.2 Animated Flow Visualization

**Problem:** The diagram is static. Colors change to show which path is active, but you can't see the actual flow direction or rate.

**Solution:** Animated particles flowing through the tubing paths:

- **Particle speed** proportional to actual flow rate (faster flow = faster animation)
- **Particle color** shows solvent composition (blue = aqueous, red = organic, gradient = purple blend)
- **Gradient front** visible as a color transition moving through the tubing
- **Valve switching** animated with smooth rotation
- **Pressure visualization** — tubing thickness or glow intensity proportional to pressure
- **Blockage indication** — particles slow down and accumulate before a restriction
- **Dead volume** — visible as the delay between gradient start and color change reaching the column

**Implementation:** WPF animation on the existing diagram XAML. Particle system using `CompositionTarget.Rendering` for 60fps updates. Data driven by the real-time pressure/flow buffer.

---

### 5.3 Configurable Hardware Topologies

**Problem:** The code assumes a fixed 4-valve, 2-pump configuration. Single-cell setups, trap-less configurations, and high-flow applications require different plumbing.

**Solution:** A topology configuration system:

**Pre-built topologies:**
- **Standard proteoElute** — 2 pumps, 4 valves, trap column, analytical column (current)
- **Trap-less** — 2 pumps, 3 valves, direct injection to analytical column
- **Single-cell** — 2 pumps, 4 valves, ultra-low dead volume path, picoliter injection
- **High-flow** — 2 pumps, 2 valves, capillary/micro-flow columns, higher flow rates
- **Custom** — user-defined topology via DiagramEditor

**Implementation:**
- Topology defined as JSON: valves (name, positions, angles), pumps (channels), sensors, flow paths (sequences of components)
- Lua procedures read topology config instead of hardcoded valve angles from `baltic.lua`
- Diagram XAML generated from topology definition (using the DiagramEditor engine)
- Signalization paths auto-configured from flow path definitions
- Validation rules auto-generated from topology constraints

**Current hardcoded values that become configurable:**
```
From baltic.lua:
  Valve angles: Inject=0°, Solvent=60°, Compress120=120°, etc.
  System volume: 0.22 µL
  Dead volume: 0.46 µL
  Max pump volume: 1350 µL
  Unity flow constants: 0.006425354, 0.02405, 0.0468
  All flow path definitions
```

---

### 5.4 Regulatory Compliance Module (21 CFR Part 11)

**Problem:** Pharma and clinical proteomics labs require FDA-compliant audit trails. ProteYOLUTE has none.

**Solution:**

- **Immutable audit log:** Every action recorded with timestamp, user ID, action type, old value, new value, reason for change
- **Electronic signatures:** User authentication for method approval, run start, and result release. Dual-signature support (operator + reviewer)
- **Role-based access control:** Operator (run methods), Analyst (modify methods), Admin (system config), Auditor (read-only full access)
- **Change tracking:** Method version history with full diff capability. No destructive overwrites — all versions preserved.
- **Data integrity:** SHA-256 checksums on all run data files. Tamper detection on database records.
- **Export:** Audit trail exportable as PDF report for inspector review
- **Annex 11 compliance:** EU GMP data integrity requirements (updated draft 2025, expected final 2026)

**Implementation:** SQLite `audit_log` table with hash chains (each entry includes hash of previous entry → tamper-evident). User auth via Windows domain credentials (Active Directory integration).

---

## Phase 6 — Autonomous Lab (Years 5-10)
### *"The LC that runs itself"*

### 6.1 Self-Driving LC

**Concept:** The instrument autonomously executes a full proteomics experiment:

1. **Receives sample list** from LIMS (via REST API or SiLA 2)
2. **Selects method** from template library based on sample type and analysis goals
3. **Runs system checks** — verifies calibration, runs quick diagnostic, checks column health
4. **Executes separation** — monitors quality in real-time, adjusts if needed
5. **Evaluates results** — checks peak quality metrics against acceptance criteria
6. **Decides next action:**
   - Results good → report to LIMS, proceed to next sample
   - Results marginal → re-run with adjusted method
   - Results bad → flag for human review, pause queue
7. **Maintains itself** — schedules calibration, orders consumables, tracks column life

**Decision engine:** Rule-based initially (if peak_count < expected, retry with longer gradient). Evolves to ML-based decision making as training data accumulates.

---

### 6.2 Multi-Instrument Fleet Management

**Concept:** One dashboard for all LCs in the lab:

- **Unified view:** Status of every instrument (running, idle, error, maintenance)
- **Load balancing:** Route samples to available instruments based on method compatibility and queue depth
- **Comparative analytics:** Which instrument has the best column performance? Which one needs maintenance?
- **Centralized method library:** Methods pushed to instruments, not configured per-instrument
- **Fleet-wide health monitoring:** Aggregate failure patterns, consumable usage, uptime metrics

**Architecture:** Each instrument runs its REST API. Fleet manager aggregates via polling or event subscription. Web dashboard shows fleet overview.

---

### 6.3 Microfluidic Chip Interface

**Concept:** Future single-cell proteomics will use microfluidic chips (like MiProChip) for sample preparation. The LC needs to interface with these chips.

- **Chip docking station:** Physical connection between chip output and LC injection port
- **Volume handoff:** Chip delivers prepared peptides in picoliter-to-nanoliter volumes directly to the injection valve
- **Protocol coordination:** LC and chip coordinate timing — chip signals "sample ready," LC switches injection valve, loads sample, begins gradient
- **Integrated control:** One software interface controls both chip and LC

**Timeline:** 5-10 years. Requires hardware partnership or custom adapter design.

---

### 6.4 Real-Time Adaptive Methods

**Concept:** The gradient adjusts during the run based on what the MS is seeing:

- **MS feedback loop:** MS sends real-time signal intensity to LC via API
- **Adaptive gradient:** If MS sees high signal (many peptides eluting), slow the gradient to improve separation. If signal is low (gap between peaks), speed up to save time.
- **Result:** Same separation quality in 30-50% less time, or better separation in the same time
- **Constraint:** Pump response time (~2-5 seconds for flow rate change) limits adaptation rate. Best suited for gradients >60 minutes where there's time to adjust.

**Implementation:** WebSocket connection from MS acquisition software to LC REST API. LC exposes `PATCH /api/gradient/rate` endpoint for real-time flow adjustment. Safety limits enforce max flow change rate and pressure boundaries.

---

## Phase 7 — The 2046 Vision (Years 10-20)
### *"The LC that doesn't exist"*

### 7.1 Chipified Separation

LC columns shrink to chip-integrated channels. No more column connections, no more ferrule tightening, no more leaks. The "LC" becomes a card that slides into the MS:

- Microfluidic channels etched in silicon or polymer
- Integrated stationary phase (monolithic or pillar array)
- On-chip gradient mixing (no external pumps for some applications)
- Disposable per-experiment or per-week
- ProteYOLUTE software drives the chip controller, not a traditional pump

### 7.2 AI Scientist Integration

The LC is one node in an AI-driven experimental design loop:

- AI proposes biological hypothesis
- AI designs proteomics experiment (sample prep, LC method, MS acquisition)
- LC executes autonomously
- AI analyzes results, updates hypothesis
- AI proposes next experiment
- Human reviews conclusions, not raw data

ProteYOLUTE's role: the execution layer that translates experimental intent into fluidic action, with full traceability and reproducibility.

### 7.3 Quantum Sensor Integration

Next-generation pressure and flow sensors based on MEMS or quantum sensing:

- Pressure resolution: 0.001 bar (vs. current ~0.1 bar)
- Flow resolution: 0.001 nL/min (vs. current ~1 nL/min)
- Temperature resolution: 0.001°C
- Enables single-molecule-level chromatography optimization
- ProteYOLUTE software handles sensor data at MHz sampling rates, stores compressed time-series

### 7.4 Federated Learning Across Labs

With user consent, anonymized performance data from ProteYOLUTE installations worldwide feeds a federated ML model:

- Column performance benchmarks across thousands of instruments
- Method optimization informed by global experience
- Early detection of bad consumable batches (multiple labs see degradation simultaneously)
- Privacy-preserving: raw data never leaves the instrument. Only model gradients are shared.
- Result: Every ProteYOLUTE instrument benefits from the collective experience of all ProteYOLUTE instruments

---

## Implementation Priority Matrix

| Feature | Impact | Effort | Priority |
|---------|--------|--------|----------|
| Real-time dashboard | High | Medium | P0 |
| SQLite data logging | High | Low | P0 |
| System health ledger | High | Low | P0 |
| Method templates | Medium | Low | P0 |
| Pressure anomaly detection | High | Medium | P1 |
| Column lifecycle management | High | Low | P1 |
| Intelligent error recovery | High | Medium | P1 |
| Adaptive PID tuning | Medium | Medium | P1 |
| REST API | High | Medium | P2 |
| Web dashboard | High | Medium | P2 |
| Webhooks/events | Medium | Low | P2 |
| SiLA 2 interface | Medium | High | P2 |
| ML pressure models | High | High | P3 |
| Digital twin simulation | High | High | P3 |
| Autonomous method optimization | Very High | Very High | P3 |
| Self-calibrating sensors | Medium | Medium | P3 |
| Natural language methods | Medium | Medium | P4 |
| Animated flow visualization | Medium | Medium | P4 |
| Configurable topologies | High | High | P4 |
| 21 CFR Part 11 compliance | High (pharma) | High | P4 |
| Self-driving LC | Very High | Very High | P5 |
| Fleet management | High (multi-lab) | High | P5 |
| Real-time adaptive methods | Very High | Very High | P5 |

---

## Competitive Positioning

| Feature | Bruker TwinScape | Evosep | Thermo Chromeleon | ProteYOLUTE (Roadmap) |
|---------|-----------------|--------|-------------------|----------------------|
| Real-time health monitoring | Basic alerts | No | Basic | ML-driven anomaly detection |
| Method optimization | Manual | Standardized only | Manual | Bayesian auto-optimization |
| Digital twin | No | No | No | Physics-informed simulation |
| REST API | No | No | Limited | Full CRUD + WebSocket |
| SiLA 2 | No | No | No | Native support |
| Audit trail (21 CFR 11) | No | No | Yes | Planned |
| Column lifecycle tracking | No | No | Basic | ML degradation prediction |
| Natural language methods | No | No | No | LLM-powered |
| Animated flow visualization | No | No | No | Particle-based real-time |
| Configurable topologies | No | No | No | JSON-defined, editor-built |
| Open source / hackable | No | No | No | Yes (Lua + .NET) |

**ProteYOLUTE's unique advantage:** It's the only LC platform where the user owns the source code and can modify anything. Every competitor is a black box. This is the foundation that makes everything else possible.

---

## Technical Debt to Address First

Before building new features, these current limitations should be resolved:

1. **Replace magic numbers in `baltic.lua`** with a JSON configuration file
2. **Refactor valve positions** from string parsing ("MixTee (240)") to typed enums
3. **Add Lua type annotations** (Lua 5.4 / EmmyLua) for IDE support
4. **Extract PID profiles** from hardcoded table to configurable file
5. **Standardize error handling** — consistent try/catch pattern across all procedures
6. **Ring buffer size** — make the 12-value flow monitoring buffer configurable (currently hardcoded)
7. **Flow monitoring interval** — make the 10-second check interval configurable
8. **Decouple HyStar dependency** where possible — isolate the API surface

---

*ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.*

*"Because your LC deserves to be smarter than you."*
