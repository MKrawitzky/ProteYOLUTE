// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using BalticWpfControlLib.Data;

namespace BalticWpfControlLib.Controls
{
    public partial class LiveDashboardPanel : UserControl
    {
        private DispatcherTimer _updateTimer;
        private readonly List<double> _pressureHistoryA = new List<double>();
        private readonly List<double> _pressureHistoryB = new List<double>();
        private readonly List<double> _flowHistoryA = new List<double>();
        private readonly List<double> _flowHistoryB = new List<double>();
        private const int MaxHistory = 120;

        public LiveDashboardPanel()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _updateTimer.Tick += OnUpdateTick;
            _updateTimer.Start();

            // Subscribe to real-time data events
            try
            {
                var db = ProteYoluteDb.Instance;
                db.DataLogged += OnDataLogged;
            }
            catch { /* DB not initialized yet */ }

            RefreshHealthCounters();
            RefreshAlerts();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer?.Stop();
            try
            {
                ProteYoluteDb.Instance.DataLogged -= OnDataLogged;
            }
            catch { }
        }

        private void OnDataLogged(string eventType, object data)
        {
            // Marshal to UI thread
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dynamic d = data;
                    switch (eventType)
                    {
                        case "pressure":
                            UpdatePressure((string)d.channel, (double)d.pressure);
                            break;
                        case "flow":
                            UpdateFlow((string)d.channel, (double)d.flow);
                            break;
                        case "valve":
                            UpdateValve((string)d.valve, (string)d.to_position);
                            break;
                        case "gradient":
                            UpdateGradient((double)d.percent_b);
                            break;
                        case "alert":
                            RefreshAlerts();
                            break;
                        case "run.started":
                        case "run.ended":
                            RefreshHealthCounters();
                            break;
                        case "column.injection":
                            RefreshColumnHealth();
                            break;
                    }
                }
                catch { }
            }));
        }

        private void OnUpdateTick(object sender, EventArgs e)
        {
            // Poll API status if SSE events aren't flowing
            try
            {
                var db = ProteYoluteDb.Instance;
                // Refresh counters every 30 seconds
                if (DateTime.Now.Second % 30 == 0)
                {
                    RefreshHealthCounters();
                    RefreshColumnHealth();
                    RefreshAlerts();
                }
            }
            catch { }

            // Redraw sparkline charts
            DrawSparkline(canvasPressure, _pressureHistoryA, _pressureHistoryB,
                Color.FromRgb(68, 136, 255), Color.FromRgb(233, 69, 96));
            DrawSparkline(canvasFlow, _flowHistoryA, _flowHistoryB,
                Color.FromRgb(68, 136, 255), Color.FromRgb(233, 69, 96));
        }

        // ─── Data Updates ────────────────────────────────────────────────

        private void UpdatePressure(string channel, double pressure)
        {
            if (channel == "A")
            {
                txtPressureA.Text = pressure.ToString("F0");
                txtPressureA.Foreground = GetPressureBrush(pressure);
                _pressureHistoryA.Add(pressure);
                if (_pressureHistoryA.Count > MaxHistory) _pressureHistoryA.RemoveAt(0);
            }
            else
            {
                txtPressureB.Text = pressure.ToString("F0");
                txtPressureB.Foreground = GetPressureBrush(pressure);
                _pressureHistoryB.Add(pressure);
                if (_pressureHistoryB.Count > MaxHistory) _pressureHistoryB.RemoveAt(0);
            }
        }

        private void UpdateFlow(string channel, double flow)
        {
            if (channel == "A")
            {
                txtFlowA.Text = flow.ToString("F3");
                _flowHistoryA.Add(flow);
                if (_flowHistoryA.Count > MaxHistory) _flowHistoryA.RemoveAt(0);
            }
            else
            {
                txtFlowB.Text = flow.ToString("F3");
                _flowHistoryB.Add(flow);
                if (_flowHistoryB.Count > MaxHistory) _flowHistoryB.RemoveAt(0);
            }
        }

        private void UpdateValve(string valve, string position)
        {
            switch (valve.ToUpperInvariant())
            {
                case "A": case "VALVE_A": case "PUMPA": txtValveA.Text = position; break;
                case "B": case "VALVE_B": case "PUMPB": txtValveB.Text = position; break;
                case "I": case "INJECTION": txtValveI.Text = position; break;
                case "T": case "TRAP": txtValveT.Text = position; break;
            }
        }

        private void UpdateGradient(double percentB)
        {
            txtPercentB.Text = percentB.ToString("F1") + "%";
            // Color interpolation: blue at 0%B, red at 100%B
            byte r = (byte)(68 + (233 - 68) * percentB / 100);
            byte g = (byte)(136 + (69 - 136) * percentB / 100);
            byte b = (byte)(255 + (96 - 255) * percentB / 100);
            txtPercentB.Foreground = new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        public void UpdateSystemState(string state)
        {
            txtSystemState.Text = state ?? "Idle";
            txtSystemState.Foreground = state == "running"
                ? new SolidColorBrush(Color.FromRgb(0, 255, 136))
                : new SolidColorBrush(Color.FromRgb(136, 136, 170));
        }

        public void UpdateGradientProgress(double progressPct)
        {
            txtGradientPct.Text = progressPct.ToString("F1") + "%";
            double parentWidth = barGradient.Parent is FrameworkElement parent ? parent.ActualWidth : 200;
            barGradient.Width = Math.Max(0, parentWidth * progressPct / 100);
        }

        // ─── Health & Column Refresh ─────────────────────────────────────

        private void RefreshHealthCounters()
        {
            try
            {
                var counters = ProteYoluteDb.Instance.GetAllCounters();
                txtTotalRuns.Text = GetCounterString(counters, "total_runs");
                txtTotalInj.Text = GetCounterString(counters, "total_injections");
                txtPumpHours.Text = counters.ContainsKey("pump_a_hours")
                    ? counters["pump_a_hours"].ToString("F1") : "0";
                double valveTotal =
                    GetCounterValue(counters, "valve_a_switches") +
                    GetCounterValue(counters, "valve_b_switches") +
                    GetCounterValue(counters, "valve_i_switches") +
                    GetCounterValue(counters, "valve_t_switches");
                txtValveSwitches.Text = valveTotal.ToString("F0");
            }
            catch { }
        }

        private void RefreshColumnHealth()
        {
            try
            {
                var db = ProteYoluteDb.Instance;
                var colMgr = new SmartColumnManager(db);

                if (colMgr.ActiveColumnId.HasValue)
                {
                    var health = db.GetColumnHealth(colMgr.ActiveColumnId.Value);
                    txtColumnName.Text = health.Name ?? "--";
                    txtColumnScore.Text = health.PerformanceScore.ToString("F0") + "%";
                    txtColumnScore.Foreground = GetScoreBrush(health.PerformanceScore);
                    txtColumnInj.Text = health.TotalInjections.ToString();
                    txtColumnRemain.Text = health.EstimatedRemainingInjections?.ToString() ?? "--";
                    UpdateHealthBar(barColumn, health.PerformanceScore);
                }

                if (colMgr.ActiveTrapId.HasValue)
                {
                    var health = db.GetColumnHealth(colMgr.ActiveTrapId.Value);
                    txtTrapName.Text = health.Name ?? "--";
                    txtTrapScore.Text = health.PerformanceScore.ToString("F0") + "%";
                    txtTrapScore.Foreground = GetScoreBrush(health.PerformanceScore);
                    txtTrapInj.Text = health.TotalInjections.ToString();
                    txtTrapHealth.Text = health.HealthStatus ?? "--";
                    UpdateHealthBar(barTrap, health.PerformanceScore);
                }
            }
            catch { }
        }

        private void RefreshAlerts()
        {
            try
            {
                var alerts = ProteYoluteDb.Instance.GetActiveAlerts();
                alertsPanel.Children.Clear();

                if (alerts.Count == 0)
                {
                    var border = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(0, 17, 51)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(68, 136, 255)),
                        BorderThickness = new Thickness(1, 0, 0, 0),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(8, 6, 8, 6),
                        Margin = new Thickness(0, 2, 0, 2)
                    };
                    border.Child = new TextBlock
                    {
                        Text = "System ready — no active alerts",
                        Foreground = new SolidColorBrush(Color.FromRgb(68, 136, 255)),
                        FontSize = 12
                    };
                    alertsPanel.Children.Add(border);
                    return;
                }

                foreach (var alert in alerts.Take(10))
                {
                    string severity = alert.ContainsKey("severity") ? alert["severity"]?.ToString() : "info";
                    string message = alert.ContainsKey("message") ? alert["message"]?.ToString() : "";
                    string timestamp = alert.ContainsKey("timestamp") ? alert["timestamp"]?.ToString() : "";

                    Color bgColor, borderColor;
                    switch (severity)
                    {
                        case "error": bgColor = Color.FromRgb(51, 0, 17); borderColor = Color.FromRgb(255, 68, 85); break;
                        case "warning": bgColor = Color.FromRgb(51, 40, 0); borderColor = Color.FromRgb(255, 170, 0); break;
                        default: bgColor = Color.FromRgb(0, 17, 51); borderColor = Color.FromRgb(68, 136, 255); break;
                    }

                    var border = new Border
                    {
                        Background = new SolidColorBrush(bgColor),
                        BorderBrush = new SolidColorBrush(borderColor),
                        BorderThickness = new Thickness(3, 0, 0, 0),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(8, 6, 8, 6),
                        Margin = new Thickness(0, 2, 0, 2)
                    };
                    var stack = new StackPanel();
                    stack.Children.Add(new TextBlock
                    {
                        Text = message,
                        Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap
                    });
                    stack.Children.Add(new TextBlock
                    {
                        Text = timestamp,
                        Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                        FontSize = 10,
                        Margin = new Thickness(0, 2, 0, 0)
                    });
                    border.Child = stack;
                    alertsPanel.Children.Add(border);
                }
            }
            catch { }
        }

        // ─── Sparkline Chart Drawing ─────────────────────────────────────

        private void DrawSparkline(Canvas canvas, List<double> seriesA, List<double> seriesB,
            Color colorA, Color colorB)
        {
            canvas.Children.Clear();
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            // Draw grid lines
            for (int i = 1; i < 4; i++)
            {
                double y = h * i / 4;
                canvas.Children.Add(new Line
                {
                    X1 = 0, Y1 = y, X2 = w, Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(26, 26, 42)),
                    StrokeThickness = 1
                });
            }

            DrawSeries(canvas, seriesA, colorA, w, h);
            DrawSeries(canvas, seriesB, colorB, w, h);
        }

        private void DrawSeries(Canvas canvas, List<double> data, Color color, double w, double h)
        {
            if (data.Count < 2) return;

            double maxVal = data.Max() * 1.1;
            if (maxVal <= 0) maxVal = 1;
            double minVal = Math.Min(0, data.Min());

            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1.5,
                StrokeLineJoin = PenLineJoin.Round
            };

            for (int i = 0; i < data.Count; i++)
            {
                double x = (double)i / (data.Count - 1) * w;
                double y = h - ((data[i] - minVal) / (maxVal - minVal) * h);
                polyline.Points.Add(new Point(x, y));
            }

            canvas.Children.Add(polyline);
        }

        // ─── Helpers ─────────────────────────────────────────────────────

        private void UpdateHealthBar(Border bar, double score)
        {
            double parentWidth = bar.Parent is FrameworkElement parent ? parent.ActualWidth : 200;
            bar.Width = Math.Max(0, parentWidth * score / 100);
            bar.Background = new SolidColorBrush(
                score >= 70 ? Color.FromRgb(0, 255, 136) :
                score >= 40 ? Color.FromRgb(255, 170, 0) :
                Color.FromRgb(255, 68, 85));
        }

        private Brush GetPressureBrush(double pressure)
        {
            if (pressure > 1000) return new SolidColorBrush(Color.FromRgb(255, 68, 85));
            if (pressure > 800) return new SolidColorBrush(Color.FromRgb(255, 170, 0));
            return new SolidColorBrush(Color.FromRgb(0, 255, 136));
        }

        private Brush GetScoreBrush(double score)
        {
            if (score >= 70) return new SolidColorBrush(Color.FromRgb(0, 255, 136));
            if (score >= 40) return new SolidColorBrush(Color.FromRgb(255, 170, 0));
            return new SolidColorBrush(Color.FromRgb(255, 68, 85));
        }

        private string GetCounterString(Dictionary<string, double> c, string key)
        {
            return c.ContainsKey(key) ? c[key].ToString("F0") : "0";
        }

        private double GetCounterValue(Dictionary<string, double> c, string key)
        {
            return c.ContainsKey(key) ? c[key] : 0;
        }
    }
}
