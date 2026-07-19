// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace BalticWpfControlLib.Data
{
    /// <summary>
    /// Statistical pressure anomaly detection. Builds baseline profiles from
    /// historical runs and detects deviations in real-time.
    /// </summary>
    public class PressureAnomalyDetector
    {
        private readonly ProteYoluteDb _db;

        // Configuration
        public double DeviationThresholdSigma { get; set; } = 2.5;
        public double SpikeThresholdBar { get; set; } = 50;
        public double DropThresholdPercent { get; set; } = 30;
        public int MinBaselineSamples { get; set; } = 5;
        public double GradientResolutionPercent { get; set; } = 1.0; // baseline resolution in % of gradient

        // Running state
        private string _currentMethod;
        private int? _currentColumnId;
        private double _gradientDurationMin;
        private DateTime _gradientStartTime;
        private bool _gradientActive;

        public PressureAnomalyDetector(ProteYoluteDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Start monitoring a gradient run.
        /// </summary>
        public void StartGradientMonitoring(string method, int? columnId, double gradientDurationMin)
        {
            _currentMethod = method;
            _currentColumnId = columnId;
            _gradientDurationMin = gradientDurationMin;
            _gradientStartTime = DateTime.Now;
            _gradientActive = true;
        }

        /// <summary>
        /// Check a pressure reading against the baseline for the current gradient position.
        /// Returns null if within normal range, or an AnomalyResult if anomalous.
        /// </summary>
        public AnomalyResult CheckPressure(double pressureBar, double? percentB = null)
        {
            if (!_gradientActive || string.IsNullOrEmpty(_currentMethod))
                return null;

            // Calculate gradient progress
            double elapsedMin = (DateTime.Now - _gradientStartTime).TotalMinutes;
            double gradientPct = _gradientDurationMin > 0
                ? Math.Min(100, (elapsedMin / _gradientDurationMin) * 100)
                : 0;

            // Round to nearest resolution step
            double step = Math.Round(gradientPct / GradientResolutionPercent) * GradientResolutionPercent;

            // Get baseline for this gradient position
            var baseline = _db.GetPressureBaseline(_currentMethod, _currentColumnId, step);

            if (baseline == null || baseline.SampleCount < MinBaselineSamples)
            {
                // Not enough data yet — record and return
                _db.UpdatePressureBaseline(_currentMethod, _currentColumnId, step, pressureBar);
                return null;
            }

            // Update baseline with new sample (online algorithm)
            _db.UpdatePressureBaseline(_currentMethod, _currentColumnId, step, pressureBar);

            // Check deviation
            double deviation = Math.Abs(pressureBar - baseline.MeanPressure);
            double sigma = baseline.StdPressure > 0 ? baseline.StdPressure : baseline.MeanPressure * 0.05;
            double zScore = sigma > 0 ? deviation / sigma : 0;

            if (zScore < DeviationThresholdSigma)
                return null; // Within normal range

            // Classify the anomaly
            var result = new AnomalyResult
            {
                GradientPercent = gradientPct,
                MeasuredPressure = pressureBar,
                ExpectedPressure = baseline.MeanPressure,
                StandardDeviation = sigma,
                ZScore = zScore,
                Timestamp = DateTime.Now
            };

            if (pressureBar > baseline.MeanPressure + SpikeThresholdBar)
            {
                result.Type = AnomalyType.PressureHigh;
                result.Severity = zScore > 4 ? "error" : "warning";
                result.Message = $"Pressure {pressureBar:F0} bar is {deviation:F0} bar above expected " +
                                 $"({baseline.MeanPressure:F0} ± {sigma:F0} bar) at {gradientPct:F1}% gradient";
                result.PossibleCauses = new List<string>
                {
                    "Column frit partially blocked",
                    "Particle contamination from sample",
                    "Column degradation (packing compression)",
                    "Tubing kink or obstruction",
                    "Wrong column installed (higher back-pressure)"
                };
                result.RecommendedActions = new List<string>
                {
                    "Check column back-pressure with solvent only (no sample)",
                    "Inspect inline filter/frit for contamination",
                    "Try backflushing the column if supported",
                    "Compare with column health history"
                };
            }
            else if (pressureBar < baseline.MeanPressure * (1 - DropThresholdPercent / 100))
            {
                result.Type = AnomalyType.PressureLow;
                result.Severity = zScore > 4 ? "error" : "warning";
                result.Message = $"Pressure {pressureBar:F0} bar is {deviation:F0} bar below expected " +
                                 $"({baseline.MeanPressure:F0} ± {sigma:F0} bar) at {gradientPct:F1}% gradient";
                result.PossibleCauses = new List<string>
                {
                    "Leak at a fitting or connection",
                    "Column void formation (channeling)",
                    "Valve not seating properly",
                    "Wrong flow rate or solvent composition",
                    "Pump seal failure"
                };
                result.RecommendedActions = new List<string>
                {
                    "Visually inspect all fittings for leaks",
                    "Run leak test diagnostic",
                    "Check valve positions are correct",
                    "Verify solvent bottles are not empty"
                };
            }
            else
            {
                result.Type = AnomalyType.PressureDeviation;
                result.Severity = "info";
                result.Message = $"Pressure deviation: {pressureBar:F0} bar vs expected " +
                                 $"{baseline.MeanPressure:F0} ± {sigma:F0} bar (z={zScore:F1})";
                result.PossibleCauses = new List<string>
                {
                    "Temperature change affecting viscosity",
                    "Solvent composition variation",
                    "Normal column aging"
                };
                result.RecommendedActions = new List<string>
                {
                    "Monitor trend over next few runs",
                    "Check room temperature and column oven"
                };
            }

            // Log to database
            _db.RaiseAlert($"anomaly.{result.Type.ToString().ToLowerInvariant()}", result.Severity,
                result.Message, "PressureAnomalyDetector",
                $"{{\"z_score\":{zScore:F2},\"expected\":{baseline.MeanPressure:F1}," +
                $"\"measured\":{pressureBar:F1},\"gradient_pct\":{gradientPct:F1}}}");

            return result;
        }

        /// <summary>
        /// Stop gradient monitoring.
        /// </summary>
        public void StopGradientMonitoring()
        {
            _gradientActive = false;
        }

        /// <summary>
        /// Get a summary of the pressure baseline for a method/column combination.
        /// Useful for method comparison and column QC.
        /// </summary>
        public List<BaselinePoint> GetBaselineProfile(string method, int? columnId)
        {
            var points = new List<BaselinePoint>();
            var rows = _db.Query(
                @"SELECT gradient_time_pct, mean_pressure, std_pressure, sample_count
                  FROM pressure_baselines
                  WHERE method = @m AND (column_id = @c OR (@c IS NULL AND column_id IS NULL))
                  ORDER BY gradient_time_pct",
                new[] { "@m", method, "@c", columnId?.ToString() ?? "" });

            foreach (var row in rows)
            {
                points.Add(new BaselinePoint
                {
                    GradientPercent = Convert.ToDouble(row["gradient_time_pct"]),
                    MeanPressure = Convert.ToDouble(row["mean_pressure"]),
                    StdPressure = Convert.ToDouble(row["std_pressure"]),
                    SampleCount = Convert.ToInt32(row["sample_count"])
                });
            }
            return points;
        }

        /// <summary>
        /// Compare pressure profiles between two runs of the same method.
        /// Detects systematic shifts that indicate column/system changes.
        /// </summary>
        public ProfileComparisonResult CompareProfiles(string method, int? columnId1, int? columnId2)
        {
            var profile1 = GetBaselineProfile(method, columnId1);
            var profile2 = GetBaselineProfile(method, columnId2);

            if (profile1.Count == 0 || profile2.Count == 0)
                return new ProfileComparisonResult { HasData = false };

            // Calculate average pressure difference across gradient
            double totalDiff = 0;
            int matchCount = 0;

            foreach (var p1 in profile1)
            {
                var p2 = profile2.FirstOrDefault(p => Math.Abs(p.GradientPercent - p1.GradientPercent) < 0.5);
                if (p2 != null)
                {
                    totalDiff += p2.MeanPressure - p1.MeanPressure;
                    matchCount++;
                }
            }

            double avgDiff = matchCount > 0 ? totalDiff / matchCount : 0;
            double avgPressure1 = profile1.Average(p => p.MeanPressure);
            double percentDiff = avgPressure1 > 0 ? (avgDiff / avgPressure1) * 100 : 0;

            return new ProfileComparisonResult
            {
                HasData = true,
                AveragePressureDifference = avgDiff,
                PercentDifference = percentDiff,
                MatchedPoints = matchCount,
                Interpretation = InterpretDifference(percentDiff)
            };
        }

        private string InterpretDifference(double percentDiff)
        {
            double abs = Math.Abs(percentDiff);
            if (abs < 5) return "Columns are performing similarly — within normal variation.";
            if (abs < 15)
            {
                return percentDiff > 0
                    ? "Second column has higher back-pressure — may indicate tighter packing or partial blockage."
                    : "Second column has lower back-pressure — may indicate wider packing or void formation.";
            }
            return percentDiff > 0
                ? "Significant pressure increase on second column — likely degradation or contamination."
                : "Significant pressure decrease on second column — possible channeling or connection issue.";
        }
    }

    // ─── Data Models ─────────────────────────────────────────────────────

    public enum AnomalyType
    {
        PressureHigh,
        PressureLow,
        PressureDeviation,
        PressureOscillation,
        FlowDeviation
    }

    public class AnomalyResult
    {
        public AnomalyType Type { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public double GradientPercent { get; set; }
        public double MeasuredPressure { get; set; }
        public double ExpectedPressure { get; set; }
        public double StandardDeviation { get; set; }
        public double ZScore { get; set; }
        public DateTime Timestamp { get; set; }
        public List<string> PossibleCauses { get; set; } = new List<string>();
        public List<string> RecommendedActions { get; set; } = new List<string>();
    }

    public class BaselinePoint
    {
        public double GradientPercent { get; set; }
        public double MeanPressure { get; set; }
        public double StdPressure { get; set; }
        public int SampleCount { get; set; }
    }

    public class ProfileComparisonResult
    {
        public bool HasData { get; set; }
        public double AveragePressureDifference { get; set; }
        public double PercentDifference { get; set; }
        public int MatchedPoints { get; set; }
        public string Interpretation { get; set; }
    }
}
