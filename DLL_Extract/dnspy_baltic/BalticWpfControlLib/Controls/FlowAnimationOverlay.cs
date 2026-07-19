// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BalticWpfControlLib.Controls
{
    /// <summary>
    /// Animated flow visualization overlay for the LC diagram.
    /// Renders moving particles along tubing paths, with speed proportional to flow rate
    /// and color representing solvent composition (%B).
    /// </summary>
    public class FlowAnimationOverlay : Canvas
    {
        // Flow path definitions — each path is a series of (x,y) waypoints
        // that correspond to tubing paths on the diagram. Coordinates are relative
        // to the diagram canvas (normalized 0-1, scaled at render time).
        private readonly Dictionary<string, List<Point>> _flowPaths = new Dictionary<string, List<Point>>();
        private readonly Dictionary<string, FlowPathState> _pathStates = new Dictionary<string, FlowPathState>();
        private readonly List<Ellipse> _particles = new List<Ellipse>();

        private DispatcherTimer _animTimer;
        private double _animPhase;
        private bool _isRunning;

        // Current flow state
        private double _flowRateA; // µL/min
        private double _flowRateB;
        private double _percentB;
        private string _currentPhase = "idle"; // idle, loading, eluting, washing

        // Configuration
        public int ParticlesPerPath { get; set; } = 8;
        public double ParticleSize { get; set; } = 4;
        public double BaseSpeed { get; set; } = 0.005; // normalized units per tick

        public FlowAnimationOverlay()
        {
            ClipToBounds = true;
            IsHitTestVisible = false; // Let clicks pass through to the diagram
            Background = Brushes.Transparent;

            InitializeDefaultPaths();

            Loaded += (s, e) =>
            {
                _animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) }; // ~30fps
                _animTimer.Tick += OnAnimTick;
            };
            Unloaded += (s, e) => Stop();
        }

        /// <summary>
        /// Initialize default flow paths matching the proteoElute diagram layout.
        /// Coordinates are normalized (0-1) and scaled to actual canvas size.
        /// </summary>
        private void InitializeDefaultPaths()
        {
            // Pump A path: Solvent A → Pump A → Valve A → Pressure Sensor A → Flow Sensor A → Mix Tee
            _flowPaths["PumpA"] = new List<Point>
            {
                new Point(0.05, 0.15), // Solvent A bottle
                new Point(0.15, 0.15), // Pump A inlet
                new Point(0.25, 0.15), // Pump A outlet
                new Point(0.35, 0.15), // Valve A
                new Point(0.45, 0.15), // Pressure Sensor A
                new Point(0.55, 0.15), // Flow Sensor A
                new Point(0.65, 0.30), // Mix Tee
            };

            // Pump B path: Solvent B → Pump B → Valve B → Pressure Sensor B → Flow Sensor B → Mix Tee
            _flowPaths["PumpB"] = new List<Point>
            {
                new Point(0.05, 0.45), // Solvent B bottle
                new Point(0.15, 0.45), // Pump B inlet
                new Point(0.25, 0.45), // Pump B outlet
                new Point(0.35, 0.45), // Valve B
                new Point(0.45, 0.45), // Pressure Sensor B
                new Point(0.55, 0.45), // Flow Sensor B
                new Point(0.65, 0.30), // Mix Tee
            };

            // Mix to Trap: Mix Tee → Trap Valve → Trap Column
            _flowPaths["MixToTrap"] = new List<Point>
            {
                new Point(0.65, 0.30), // Mix Tee
                new Point(0.75, 0.30), // Trap Valve
                new Point(0.85, 0.30), // Trap Column inlet
                new Point(0.85, 0.45), // Trap Column outlet
            };

            // Separation: Trap Valve → Separator Column → Transfer Line → MS
            _flowPaths["Separation"] = new List<Point>
            {
                new Point(0.75, 0.30), // Trap Valve
                new Point(0.75, 0.60), // Separator Column inlet
                new Point(0.75, 0.80), // Separator Column outlet
                new Point(0.85, 0.80), // Transfer line
                new Point(0.95, 0.80), // MS inlet
            };

            // Injection path: Injection Valve → Sample Loop → Trap
            _flowPaths["Injection"] = new List<Point>
            {
                new Point(0.55, 0.65), // Injection Valve
                new Point(0.60, 0.65), // Sample Loop start
                new Point(0.65, 0.65), // Sample Loop end
                new Point(0.75, 0.50), // To Trap Valve
                new Point(0.75, 0.30), // Trap Valve
            };

            // Waste paths
            _flowPaths["WasteA"] = new List<Point>
            {
                new Point(0.35, 0.15), // Valve A
                new Point(0.35, 0.05), // Waste
            };
            _flowPaths["WasteT"] = new List<Point>
            {
                new Point(0.75, 0.30), // Trap Valve
                new Point(0.75, 0.10), // Waste
            };

            // Initialize path states
            foreach (var kvp in _flowPaths)
            {
                _pathStates[kvp.Key] = new FlowPathState
                {
                    IsActive = false,
                    FlowRate = 0,
                    Color = Colors.Gray
                };
            }
        }

        // ─── Public API ──────────────────────────────────────────────────

        /// <summary>
        /// Start the flow animation.
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _animTimer?.Start();
            CreateParticles();
        }

        /// <summary>
        /// Stop the flow animation.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _animTimer?.Stop();
            Children.Clear();
            _particles.Clear();
        }

        /// <summary>
        /// Update flow rates and solvent composition.
        /// </summary>
        public void UpdateFlow(double flowA, double flowB, double percentB)
        {
            _flowRateA = flowA;
            _flowRateB = flowB;
            _percentB = percentB;
            UpdatePathStates();
        }

        /// <summary>
        /// Set the current LC phase to control which paths are visualized.
        /// </summary>
        public void SetPhase(string phase)
        {
            _currentPhase = phase?.ToLowerInvariant() ?? "idle";
            UpdatePathStates();
        }

        /// <summary>
        /// Set custom flow path coordinates (for matching actual diagram layout).
        /// </summary>
        public void SetFlowPath(string name, List<Point> waypoints)
        {
            _flowPaths[name] = waypoints;
            if (!_pathStates.ContainsKey(name))
                _pathStates[name] = new FlowPathState();
        }

        // ─── Path State Management ───────────────────────────────────────

        private void UpdatePathStates()
        {
            // Color: Blue for aqueous (A), Red for organic (B), Purple for mixed
            Color colorA = Color.FromRgb(68, 136, 255);  // Blue
            Color colorB = Color.FromRgb(233, 69, 96);   // Red
            Color colorMix = InterpolateColor(colorA, colorB, _percentB / 100.0);
            Color colorGreen = Color.FromRgb(0, 200, 100);  // Column/separation
            Color colorGold = Color.FromRgb(218, 165, 32);  // Injection/loading
            Color colorGray = Color.FromRgb(100, 100, 100); // Inactive

            double totalFlow = _flowRateA + _flowRateB;

            switch (_currentPhase)
            {
                case "eluting":
                case "gradient":
                    SetPathState("PumpA", true, _flowRateA, colorA);
                    SetPathState("PumpB", true, _flowRateB, colorB);
                    SetPathState("MixToTrap", true, totalFlow, colorMix);
                    SetPathState("Separation", true, totalFlow, colorGreen);
                    SetPathState("Injection", false, 0, colorGray);
                    SetPathState("WasteA", false, 0, colorGray);
                    SetPathState("WasteT", false, 0, colorGray);
                    break;

                case "loading":
                case "trapping":
                    SetPathState("PumpA", true, _flowRateA, colorA);
                    SetPathState("PumpB", false, 0, colorGray);
                    SetPathState("MixToTrap", true, _flowRateA, colorA);
                    SetPathState("Separation", false, 0, colorGray);
                    SetPathState("Injection", true, _flowRateA, colorGold);
                    SetPathState("WasteA", false, 0, colorGray);
                    SetPathState("WasteT", true, _flowRateA * 0.3, colorGray);
                    break;

                case "washing":
                    SetPathState("PumpA", true, _flowRateA, colorA);
                    SetPathState("PumpB", true, _flowRateB, colorB);
                    SetPathState("MixToTrap", false, 0, colorGray);
                    SetPathState("Separation", false, 0, colorGray);
                    SetPathState("Injection", false, 0, colorGray);
                    SetPathState("WasteA", true, totalFlow, colorGray);
                    SetPathState("WasteT", false, 0, colorGray);
                    break;

                case "idle":
                default:
                    foreach (var key in _pathStates.Keys.ToList())
                        SetPathState(key, false, 0, colorGray);
                    break;
            }
        }

        private void SetPathState(string pathName, bool active, double flowRate, Color color)
        {
            if (!_pathStates.ContainsKey(pathName)) return;
            _pathStates[pathName].IsActive = active;
            _pathStates[pathName].FlowRate = flowRate;
            _pathStates[pathName].Color = color;
        }

        // ─── Particle Animation ──────────────────────────────────────────

        private void CreateParticles()
        {
            Children.Clear();
            _particles.Clear();

            // First, draw static tubing paths
            foreach (var kvp in _flowPaths)
            {
                if (kvp.Value.Count < 2) continue;
                var polyline = new Polyline
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(40, 100, 100, 120)),
                    StrokeThickness = 3,
                    StrokeLineJoin = PenLineJoin.Round
                };
                foreach (var pt in kvp.Value)
                    polyline.Points.Add(ScalePoint(pt));
                Children.Add(polyline);
            }

            // Create particle dots for each path
            foreach (var kvp in _flowPaths)
            {
                for (int i = 0; i < ParticlesPerPath; i++)
                {
                    var particle = new Ellipse
                    {
                        Width = ParticleSize,
                        Height = ParticleSize,
                        Fill = Brushes.Gray,
                        Opacity = 0,
                        Tag = new ParticleData
                        {
                            PathName = kvp.Key,
                            Phase = (double)i / ParticlesPerPath // Stagger particles along path
                        }
                    };
                    _particles.Add(particle);
                    Children.Add(particle);
                }
            }
        }

        private void OnAnimTick(object sender, EventArgs e)
        {
            if (!_isRunning) return;

            foreach (var particle in _particles)
            {
                var data = particle.Tag as ParticleData;
                if (data == null) continue;

                if (!_pathStates.ContainsKey(data.PathName)) continue;
                var state = _pathStates[data.PathName];

                if (!state.IsActive || state.FlowRate < 0.001)
                {
                    particle.Opacity = 0;
                    continue;
                }

                // Move particle
                double speed = BaseSpeed * Math.Max(0.2, Math.Min(3.0, state.FlowRate));
                data.Phase += speed;
                if (data.Phase >= 1.0) data.Phase -= 1.0;

                // Calculate position along path
                var path = _flowPaths[data.PathName];
                Point pos = GetPointAlongPath(path, data.Phase);
                Point scaled = ScalePoint(pos);

                Canvas.SetLeft(particle, scaled.X - ParticleSize / 2);
                Canvas.SetTop(particle, scaled.Y - ParticleSize / 2);

                // Update color and visibility
                particle.Fill = new SolidColorBrush(state.Color);
                particle.Opacity = 0.8;

                // Fade at path ends
                if (data.Phase < 0.05)
                    particle.Opacity = data.Phase / 0.05 * 0.8;
                else if (data.Phase > 0.95)
                    particle.Opacity = (1.0 - data.Phase) / 0.05 * 0.8;
            }
        }

        // ─── Geometry Helpers ────────────────────────────────────────────

        private Point GetPointAlongPath(List<Point> path, double t)
        {
            if (path.Count < 2) return path.FirstOrDefault();

            // Calculate total path length
            double totalLength = 0;
            var segments = new List<double>();
            for (int i = 1; i < path.Count; i++)
            {
                double segLen = Distance(path[i - 1], path[i]);
                segments.Add(segLen);
                totalLength += segLen;
            }

            if (totalLength <= 0) return path[0];

            // Find position along path at parameter t
            double targetDist = t * totalLength;
            double accumulated = 0;

            for (int i = 0; i < segments.Count; i++)
            {
                if (accumulated + segments[i] >= targetDist)
                {
                    double segT = (targetDist - accumulated) / segments[i];
                    return new Point(
                        path[i].X + (path[i + 1].X - path[i].X) * segT,
                        path[i].Y + (path[i + 1].Y - path[i].Y) * segT);
                }
                accumulated += segments[i];
            }

            return path.Last();
        }

        private Point ScalePoint(Point normalized)
        {
            return new Point(normalized.X * ActualWidth, normalized.Y * ActualHeight);
        }

        private double Distance(Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private Color InterpolateColor(Color a, Color b, double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return Color.FromRgb(
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t));
        }

        // ─── Inner Types ─────────────────────────────────────────────────

        private class FlowPathState
        {
            public bool IsActive { get; set; }
            public double FlowRate { get; set; }
            public Color Color { get; set; } = Colors.Gray;
        }

        private class ParticleData
        {
            public string PathName { get; set; }
            public double Phase { get; set; }
        }
    }
}
