// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace BalticWpfControlLib.Data
{
    /// <summary>
    /// Smart Column & Trap Manager. Monitors column/trap health in real-time,
    /// detects degradation patterns, predicts failures, and provides actionable alerts.
    /// </summary>
    public class SmartColumnManager
    {
        private readonly ProteYoluteDb _db;
        private int? _activeColumnId;
        private int? _activeTrapId;

        // Running statistics for current run
        private readonly List<double> _pressureBuffer = new List<double>();
        private readonly List<double> _flowBuffer = new List<double>();
        private double _runStartPressure;
        private double _runStartFlow;
        private DateTime _runStartTime;
        private bool _baselineRecorded;

        public int? ActiveColumnId => _activeColumnId;
        public int? ActiveTrapId => _activeTrapId;

        public SmartColumnManager(ProteYoluteDb db)
        {
            _db = db;
        }

        // ─── Column/Trap Registration ────────────────────────────────────

        /// <summary>
        /// Register a new analytical column and set it as active.
        /// </summary>
        public int InstallColumn(string name, string type, double? particleSizeUm = null,
            double? innerDiameterUm = null, double? lengthCm = null, double? maxPressureBar = null,
            string serial = null, string stationaryPhase = null)
        {
            int id = _db.RegisterColumn(name, type, "analytical",
                particleSizeUm, innerDiameterUm, lengthCm, maxPressureBar, serial, stationaryPhase);
            _activeColumnId = id;
            _db.Audit("column.installed", "column", id.ToString(),
                newValue: $"{name} ({type})", reason: "New column installed");
            _db.RaiseAlert("column.installed", "info",
                $"New column installed: {name} ({type}), ID #{id}", "SmartColumnManager");
            return id;
        }

        /// <summary>
        /// Register a new trap column and set it as active.
        /// </summary>
        public int InstallTrap(string name, string type, double? particleSizeUm = null,
            double? innerDiameterUm = null, double? lengthCm = null, double? maxPressureBar = null,
            string serial = null, string stationaryPhase = null)
        {
            int id = _db.RegisterColumn(name, type, "trap",
                particleSizeUm, innerDiameterUm, lengthCm, maxPressureBar, serial, stationaryPhase);
            _activeTrapId = id;
            _db.Audit("trap.installed", "column", id.ToString(),
                newValue: $"{name} ({type})", reason: "New trap column installed");
            _db.RaiseAlert("trap.installed", "info",
                $"New trap column installed: {name} ({type}), ID #{id}", "SmartColumnManager");
            return id;
        }

        /// <summary>
        /// Set the active column by ID (when switching between installed columns).
        /// </summary>
        public void SetActiveColumn(int columnId)
        {
            _activeColumnId = columnId;
            _db.Audit("column.activated", "column", columnId.ToString());
        }

        /// <summary>
        /// Set the active trap by ID.
        /// </summary>
        public void SetActiveTrap(int trapId)
        {
            _activeTrapId = trapId;
            _db.Audit("trap.activated", "column", trapId.ToString());
        }

        /// <summary>
        /// Retire a column (mark as no longer in use).
        /// </summary>
        public void RetireColumn(int columnId, string reason = null)
        {
            _db.Query("UPDATE columns SET status = 'retired', retire_date = datetime('now','localtime') WHERE column_id = @id",
                new[] { "@id", columnId.ToString() });
            _db.Audit("column.retired", "column", columnId.ToString(), reason: reason ?? "Column retired");
            if (_activeColumnId == columnId) _activeColumnId = null;
            if (_activeTrapId == columnId) _activeTrapId = null;
        }

        // ─── Real-Time Monitoring ────────────────────────────────────────

        /// <summary>
        /// Call at the start of each run to begin monitoring.
        /// </summary>
        public void OnRunStart()
        {
            _pressureBuffer.Clear();
            _flowBuffer.Clear();
            _baselineRecorded = false;
            _runStartTime = DateTime.Now;
        }

        /// <summary>
        /// Feed real-time pressure data during a run. Call at 500ms-2s intervals.
        /// Returns alerts if anomalies are detected.
        /// </summary>
        public List<SmartAlert> OnPressureReading(double pressureBar, double flowUlMin,
            double percentB, string channel = "A")
        {
            var alerts = new List<SmartAlert>();
            _pressureBuffer.Add(pressureBar);
            _flowBuffer.Add(flowUlMin);

            // Record baseline after first 30 seconds of stable flow
            if (!_baselineRecorded && _pressureBuffer.Count >= 15) // ~30s at 2s intervals
            {
                _runStartPressure = _pressureBuffer.Skip(_pressureBuffer.Count - 10).Average();
                _runStartFlow = _flowBuffer.Skip(_flowBuffer.Count - 10).Average();
                _baselineRecorded = true;
            }

            if (_pressureBuffer.Count < 5) return alerts; // Need minimum data

            // Check for sudden pressure spike
            double recentAvg = _pressureBuffer.Skip(Math.Max(0, _pressureBuffer.Count - 5)).Average();
            double previousAvg = _pressureBuffer.Count > 10
                ? _pressureBuffer.Skip(Math.Max(0, _pressureBuffer.Count - 15)).Take(5).Average()
                : recentAvg;

            if (recentAvg > previousAvg * 1.5 && recentAvg > 50)
            {
                alerts.Add(new SmartAlert
                {
                    Type = "pressure.spike",
                    Severity = "warning",
                    Message = $"Sudden pressure spike detected: {recentAvg:F0} bar (was {previousAvg:F0} bar)",
                    Recommendation = "Check for blockage in tubing, frit, or column inlet. Possible particle contamination.",
                    Channel = channel
                });
            }

            // Check for pressure drop (leak indicator)
            if (_baselineRecorded && recentAvg < _runStartPressure * 0.5 && flowUlMin > 0.01)
            {
                alerts.Add(new SmartAlert
                {
                    Type = "pressure.drop",
                    Severity = "warning",
                    Message = $"Pressure dropped to {recentAvg:F0} bar (baseline was {_runStartPressure:F0} bar)",
                    Recommendation = "Possible leak at a fitting, valve, or column connection. Check all connections.",
                    Channel = channel
                });
            }

            // Check for pressure oscillation (air bubble or pump seal issue)
            if (_pressureBuffer.Count >= 10)
            {
                var recent10 = _pressureBuffer.Skip(_pressureBuffer.Count - 10).ToList();
                double stdDev = CalculateStdDev(recent10);
                double mean = recent10.Average();
                double cv = mean > 0 ? (stdDev / mean) * 100 : 0;

                if (cv > 10 && mean > 20) // >10% coefficient of variation
                {
                    alerts.Add(new SmartAlert
                    {
                        Type = "pressure.oscillation",
                        Severity = "warning",
                        Message = $"Pressure oscillation detected: CV = {cv:F1}% (mean {mean:F0} bar, SD {stdDev:F1} bar)",
                        Recommendation = "Possible air bubble in pump head, degraded pump seal, or check valve malfunction. Run degas procedure.",
                        Channel = channel
                    });
                }
            }

            // Check for flow deviation
            if (_flowBuffer.Count >= 10 && flowUlMin > 0.01)
            {
                var recentFlow = _flowBuffer.Skip(_flowBuffer.Count - 10).ToList();
                double flowStdDev = CalculateStdDev(recentFlow);
                double flowMean = recentFlow.Average();
                double flowCv = flowMean > 0 ? (flowStdDev / flowMean) * 100 : 0;

                if (flowCv > 15)
                {
                    alerts.Add(new SmartAlert
                    {
                        Type = "flow.unstable",
                        Severity = "warning",
                        Message = $"Flow instability detected: CV = {flowCv:F1}% (mean {flowMean:F3} µL/min)",
                        Recommendation = "Check pump seals, solvent supply, and for air in lines.",
                        Channel = channel
                    });
                }
            }

            // Log alerts to database
            foreach (var alert in alerts)
            {
                _db.RaiseAlert(alert.Type, alert.Severity, alert.Message,
                    "SmartColumnManager",
                    $"{{\"channel\":\"{channel}\",\"pressure\":{pressureBar:F1},\"flow\":{flowUlMin:F4},\"recommendation\":\"{alert.Recommendation}\"}}");
            }

            return alerts;
        }

        /// <summary>
        /// Call at the end of each run to record column performance data.
        /// </summary>
        public ColumnHealthReport OnRunEnd(double finalPressure, double finalFlow, double percentB, double? tempC = null)
        {
            ColumnHealthReport report = null;

            if (_activeColumnId.HasValue && _baselineRecorded)
            {
                _db.RecordColumnInjection(_activeColumnId.Value, _runStartPressure, _runStartFlow, percentB, tempC);
                _db.IncrementCounter("total_injections");
                report = _db.GetColumnHealth(_activeColumnId.Value);

                // Raise alerts based on column health
                if (report.PerformanceScore < 50)
                {
                    _db.RaiseAlert("column.degraded", "warning",
                        $"Column '{report.Name}' performance at {report.PerformanceScore:F0}% — consider replacement",
                        "SmartColumnManager");
                }
                if (report.EstimatedRemainingInjections.HasValue && report.EstimatedRemainingInjections < 100)
                {
                    _db.RaiseAlert("column.end_of_life", "warning",
                        $"Column '{report.Name}' estimated {report.EstimatedRemainingInjections} injections remaining",
                        "SmartColumnManager");
                }
            }

            if (_activeTrapId.HasValue && _baselineRecorded)
            {
                _db.RecordColumnInjection(_activeTrapId.Value, _runStartPressure, _runStartFlow, percentB, tempC);
            }

            // Update pump hours
            double runHours = (DateTime.Now - _runStartTime).TotalHours;
            _db.IncrementCounter("pump_a_hours", runHours);
            _db.IncrementCounter("pump_b_hours", runHours);

            return report;
        }

        // ─── Pre-Run Smart Checks ────────────────────────────────────────

        /// <summary>
        /// Run before starting a separation. Returns a list of warnings/recommendations
        /// based on column history and system state.
        /// </summary>
        public List<SmartAlert> PreRunCheck()
        {
            var alerts = new List<SmartAlert>();

            // Check active column health
            if (_activeColumnId.HasValue)
            {
                var health = _db.GetColumnHealth(_activeColumnId.Value);
                if (health.PerformanceScore < 30)
                {
                    alerts.Add(new SmartAlert
                    {
                        Type = "column.critical",
                        Severity = "error",
                        Message = $"Column '{health.Name}' performance is critical ({health.PerformanceScore:F0}%)",
                        Recommendation = "Replace column before running. Continued use may damage the MS."
                    });
                }
                else if (health.PerformanceScore < 60)
                {
                    alerts.Add(new SmartAlert
                    {
                        Type = "column.degraded",
                        Severity = "warning",
                        Message = $"Column '{health.Name}' showing degradation ({health.PerformanceScore:F0}%)",
                        Recommendation = "Consider running a blank gradient to check peak shape. Schedule replacement."
                    });
                }

                if (health.EstimatedRemainingInjections.HasValue && health.EstimatedRemainingInjections < 20)
                {
                    alerts.Add(new SmartAlert
                    {
                        Type = "column.low_life",
                        Severity = "warning",
                        Message = $"Column '{health.Name}' has ~{health.EstimatedRemainingInjections} injections remaining",
                        Recommendation = "Order replacement column. Have backup ready."
                    });
                }
            }
            else
            {
                alerts.Add(new SmartAlert
                {
                    Type = "column.none",
                    Severity = "info",
                    Message = "No active column registered in the system",
                    Recommendation = "Register your column for smart health tracking."
                });
            }

            // Check active trap health
            if (_activeTrapId.HasValue)
            {
                var trapHealth = _db.GetColumnHealth(_activeTrapId.Value);
                if (trapHealth.PerformanceScore < 40)
                {
                    alerts.Add(new SmartAlert
                    {
                        Type = "trap.degraded",
                        Severity = "warning",
                        Message = $"Trap column '{trapHealth.Name}' showing degradation ({trapHealth.PerformanceScore:F0}%)",
                        Recommendation = "Trap columns degrade faster than analytical columns. Replace and equilibrate."
                    });
                }
            }

            // Check calibration age
            double lastCalTimestamp = _db.GetCounter("last_calibration_date");
            if (lastCalTimestamp > 0)
            {
                var lastCal = DateTimeOffset.FromUnixTimeSeconds((long)lastCalTimestamp).LocalDateTime;
                var daysSinceCal = (DateTime.Now - lastCal).TotalDays;
                if (daysSinceCal > 30)
                {
                    alerts.Add(new SmartAlert
                    {
                        Type = "calibration.overdue",
                        Severity = "warning",
                        Message = $"Last calibration was {daysSinceCal:F0} days ago",
                        Recommendation = "Run flow sensor and pressure sensor calibration for accurate measurements."
                    });
                }
            }

            // Check valve wear
            double valveISwitches = _db.GetCounter("valve_i_switches");
            if (valveISwitches > 20000)
            {
                alerts.Add(new SmartAlert
                {
                    Type = "valve.wear",
                    Severity = "info",
                    Message = $"Injection valve has {valveISwitches:F0} switches — approaching maintenance interval",
                    Recommendation = "Inspect rotor seal for wear. Schedule preventive maintenance."
                });
            }

            return alerts;
        }

        // ─── Column Comparison ───────────────────────────────────────────

        /// <summary>
        /// Compare performance of two columns (useful for QC).
        /// </summary>
        public ColumnComparisonReport CompareColumns(int columnId1, int columnId2)
        {
            var h1 = _db.GetColumnHealth(columnId1);
            var h2 = _db.GetColumnHealth(columnId2);

            return new ColumnComparisonReport
            {
                Column1 = h1,
                Column2 = h2,
                PerformanceDifference = h1.PerformanceScore - h2.PerformanceScore,
                RecommendedColumn = h1.PerformanceScore >= h2.PerformanceScore ? columnId1 : columnId2,
                Summary = GenerateComparisonSummary(h1, h2)
            };
        }

        private string GenerateComparisonSummary(ColumnHealthReport h1, ColumnHealthReport h2)
        {
            var better = h1.PerformanceScore >= h2.PerformanceScore ? h1 : h2;
            var worse = h1.PerformanceScore >= h2.PerformanceScore ? h2 : h1;

            return $"{better.Name} ({better.PerformanceScore:F0}%) is performing better than " +
                   $"{worse.Name} ({worse.PerformanceScore:F0}%). " +
                   $"{better.Name} has {better.TotalInjections} injections, " +
                   $"{worse.Name} has {worse.TotalInjections} injections.";
        }

        // ─── Utility ─────────────────────────────────────────────────────

        public List<Dictionary<string, object>> GetAllColumns(string role = null)
        {
            if (role != null)
                return _db.Query("SELECT * FROM columns WHERE role = @r AND status = 'active' ORDER BY name",
                    new[] { "@r", role });
            return _db.Query("SELECT * FROM columns WHERE status = 'active' ORDER BY name");
        }

        private static double CalculateStdDev(List<double> values)
        {
            if (values.Count < 2) return 0;
            double mean = values.Average();
            double sumSq = values.Sum(v => (v - mean) * (v - mean));
            return Math.Sqrt(sumSq / (values.Count - 1));
        }
    }

    // ─── Data Models ─────────────────────────────────────────────────────

    public class SmartAlert
    {
        public string Type { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public string Recommendation { get; set; }
        public string Channel { get; set; }
    }

    public class ColumnComparisonReport
    {
        public ColumnHealthReport Column1 { get; set; }
        public ColumnHealthReport Column2 { get; set; }
        public double PerformanceDifference { get; set; }
        public int RecommendedColumn { get; set; }
        public string Summary { get; set; }
    }
}
