using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DiagramEditor
{
    public partial class MainWindow : Window
    {
        private FrameworkElement _dragElement;
        private Point _dragOffset;
        private bool _isDragging;
        private DiagramItem _selectedItem;
        private List<DiagramItem> _items = new List<DiagramItem>();
        private List<TubingPath> _tubingPaths = new List<TubingPath>();
        private TubingPath _activeTubing;
        private bool _updatingSize;
        private List<object> _actionHistory = new List<object>(); // tracks items and tubing in order added
        private static readonly double[] Port6Angles = { -90, -150, -210, -270, -330, -30 };
        private static readonly string[] Port6Clocks = { "12", "10", "8", "6", "4", "2" };
        private static readonly string SaveDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "cluade");
        private static readonly string SaveFile = "diagram_save.txt";

        public MainWindow() { InitializeComponent(); LoadPalette(); TryLoadSaved(); }

        class DiagramItem
        {
            public string Name, Type, Label;
            public double X, Y, W, H, Rotation;
            public Color FillColor;
            public UIElement Visual;
            public List<PortDot> Ports = new List<PortDot>();
            public int PortCount;
            public bool HasSnapPoints; // true for loops, traps, columns
        }
        class PortDot { public int PortNum; public Ellipse Dot; public DiagramItem Owner; public double AngleDeg; }
        class TubingAnchor { public PortDot Port; public int PointIndex; }
        class TubingPath
        {
            public List<Point> Points = new List<Point>();
            public double Thickness; public Color Color;
            public List<UIElement> Visuals = new List<UIElement>();
            public List<TubingAnchor> Anchors = new List<TubingAnchor>();
        }

        bool IsDrawMode => rbModeTube.IsChecked == true;
        double TubeWidth => rbThick.IsChecked == true ? 4 : rbMedium.IsChecked == true ? 2.5 : 1.5;
        Color TubeColor
        {
            get
            {
                if (rbColorBlue.IsChecked == true) return Color.FromRgb(68, 136, 204);
                if (rbColorRed.IsChecked == true) return Color.FromRgb(204, 68, 68);
                if (rbColorGreen.IsChecked == true) return Color.FromRgb(68, 170, 68);
                if (rbColorPurple.IsChecked == true) return Color.FromRgb(136, 68, 204);
                return Colors.Gray;
            }
        }

        double[] GetPortAngles(int count)
        {
            double[] angles = new double[count];
            for (int i = 0; i < count; i++)
                angles[i] = -90 - (360.0 / count) * i;
            return angles;
        }

        void LoadPalette()
        {
            string[][] comps = new[]
            {
                new[] { "Pump", "#FF3070B0" },
                new[] { "2-Port Valve", "#FF8A8A8A" },
                new[] { "4-Port Valve", "#FF9A9A9A" },
                new[] { "6-Port Valve", "#FFA0A0A0" },
                new[] { "8-Port Valve", "#FFB0B0B0" },
                new[] { "10-Port Valve", "#FFC0C0C0" },
                new[] { "Flow Sensor", "#FF2A6A50" },
                new[] { "Pressure Sensor", "#FF6A6A6A" },
                new[] { "Mixing Tee", "#FF808080" },
                new[] { "Solvent Bottle", "#FF4488CC" },
                new[] { "Waste Port", "#FFCC4444" },
                new[] { "Trap Column", "#FF886644" },
                new[] { "Loop 1uL", "#FF446644" },
                new[] { "Loop 5uL", "#FF558855" },
                new[] { "Loop 20uL", "#FF668866" },
                new[] { "Loop 50uL", "#FF779977" },
                new[] { "Trap 5cm", "#FF664422" },
                new[] { "Trap 15cm", "#FF886644" },
                new[] { "Trap 25cm", "#FFAA8866" },
                new[] { "Column 15cm", "#FF445566" },
                new[] { "Column 25cm", "#FF556677" },
                new[] { "Column 50cm", "#FF667788" },
                new[] { "Injection Port", "#FF444444" },
                new[] { "Peltier Stack", "#FF8090A0" },
                new[] { "Output", "#FF999999" },
                new[] { "Plug", "#FFCC2222" },
                new[] { "Custom", "#FF6A00B0" }
            };
            foreach (var c in comps)
            {
                var btn = new Button
                {
                    Content = c[0],
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(c[1])),
                    Foreground = Brushes.White, Margin = new Thickness(2), Padding = new Thickness(4, 2, 4, 2),
                    FontSize = 9, HorizontalContentAlignment = HorizontalAlignment.Left, Tag = c[0]
                };
                string type = c[0];
                btn.Click += (s, e) =>
                {
                    string name = type;
                    if (type.Contains("Valve"))
                    {
                        var dlg = new InputDialog("Valve Name", "Enter valve name (e.g. Valve A, Trap V, Inj V):");
                        if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.Result))
                            name = dlg.Result;
                        else return;
                    }
                    AddItem(type, 250, 250, name);
                };
                palette.Children.Add(btn);
            }

            // Load button
            var loadBtn = new Button
            {
                Content = "LOAD SAVED", Background = new SolidColorBrush(Colors.DarkCyan),
                Foreground = Brushes.White, Margin = new Thickness(2, 10, 2, 2), Padding = new Thickness(4, 4, 4, 4),
                FontSize = 10, FontWeight = FontWeights.Bold
            };
            loadBtn.Click += (s, e) => LoadFromFile();
            palette.Children.Add(loadBtn);

            // Presets
            var presetLabel = new TextBlock { Text = "PRESETS", Foreground = Brushes.Gray, Margin = new Thickness(4, 8, 0, 2), FontSize = 9, FontWeight = FontWeights.Bold };
            palette.Children.Add(presetLabel);

            var preset3 = new Button { Content = "3-Valve Setup", Background = Brushes.DarkSlateGray, Foreground = Brushes.White, Margin = new Thickness(2), Padding = new Thickness(4, 2, 4, 2), FontSize = 9 };
            preset3.Click += (s, e) => LoadPreset3Valve();
            palette.Children.Add(preset3);

            var preset4 = new Button { Content = "4-Valve Setup", Background = Brushes.DarkSlateGray, Foreground = Brushes.White, Margin = new Thickness(2), Padding = new Thickness(4, 2, 4, 2), FontSize = 9 };
            preset4.Click += (s, e) => LoadPreset4Valve();
            palette.Children.Add(preset4);

            var presetPE = new Button { Content = "proteoElute", Background = Brushes.SteelBlue, Foreground = Brushes.White, Margin = new Thickness(2), Padding = new Thickness(4, 2, 4, 2), FontSize = 9 };
            presetPE.Click += (s, e) => { ClearCanvas(); LoadProteoElute(); };
            palette.Children.Add(presetPE);
        }

        void AddItem(string type, double x, double y, string customName = null)
        {
            double w = 50, h = 40; Color fill = Colors.Gray; string label = customName ?? type; int ports = 0;
            switch (type)
            {
                case "Pump": w = 52; h = 102; fill = Colors.SteelBlue; break;
                case "2-Port Valve": w = 40; h = 40; fill = Colors.DarkGray; ports = 2; break;
                case "4-Port Valve": w = 50; h = 50; fill = Colors.DarkGray; ports = 4; break;
                case "6-Port Valve": w = 60; h = 60; fill = Colors.DarkGray; ports = 6; break;
                case "8-Port Valve": w = 70; h = 70; fill = Colors.DarkGray; ports = 8; break;
                case "10-Port Valve": w = 80; h = 80; fill = Colors.DarkGray; ports = 10; break;
                case "Flow Sensor": w = 54; h = 28; fill = Colors.Teal; break;
                case "Pressure Sensor": w = 22; h = 20; fill = Colors.DimGray; label = "P"; break;
                case "Mixing Tee": w = 30; h = 30; fill = Colors.Gray; label = "MT"; break;
                case "Solvent Bottle": w = 30; h = 50; fill = Colors.CornflowerBlue; break;
                case "Waste Port": w = 20; h = 20; fill = Colors.IndianRed; label = "W"; break;
                case "Plug": w = 16; h = 16; fill = Color.FromRgb(204, 34, 34); label = "X"; break;
                case "Loop 1uL": w = 20; h = 20; fill = Color.FromRgb(68,102,68); break;
                case "Loop 5uL": w = 30; h = 30; fill = Color.FromRgb(85,136,85); break;
                case "Loop 20uL": w = 45; h = 45; fill = Color.FromRgb(102,136,102); break;
                case "Loop 50uL": w = 60; h = 60; fill = Color.FromRgb(119,153,119); break;
                case "Trap 5cm": w = 15; h = 35; fill = Color.FromRgb(102,68,34); break;
                case "Trap 15cm": w = 18; h = 65; fill = Color.FromRgb(136,102,68); break;
                case "Trap 25cm": w = 20; h = 80; fill = Color.FromRgb(170,136,102); break;
                case "Column 15cm": w = 14; h = 60; fill = Color.FromRgb(68,85,102); break;
                case "Column 25cm": w = 16; h = 85; fill = Color.FromRgb(85,102,119); break;
                case "Column 50cm": w = 18; h = 120; fill = Color.FromRgb(102,119,136); break;
                case "Injection Port": w = 30; h = 40; fill = Colors.DarkSlateGray; break;
                case "Peltier Stack": w = 120; h = 60; fill = Colors.LightSteelBlue; break;
                case "Output": w = 50; h = 15; fill = Colors.Silver; break;
                case "Custom": w = 60; h = 40; fill = Colors.MediumPurple; break;
            }
            BuildVisual(customName ?? (type + "_" + _items.Count), type, x, y, w, h, fill, label, ports);
        }

        void BuildVisual(string name, string type, double x, double y, double w, double h, Color fill, string label, int ports, double rotation = 0)
        {
            var item = new DiagramItem { Name = name, Type = type, X = x, Y = y, W = w, H = h, FillColor = fill, Label = label, PortCount = ports, Rotation = rotation };
            var group = new Canvas { Width = w, Height = h };

            if (ports > 0)
            {
                // Valve body
                var body = new Ellipse
                {
                    Width = w, Height = h,
                    Fill = new SolidColorBrush(Color.FromArgb(200, fill.R, fill.G, fill.B)),
                    Stroke = Brushes.White, StrokeThickness = 2
                };
                group.Children.Add(body);

                // Rotor grooves (connect adjacent pairs)
                var angles = GetPortAngles(ports);
                for (int i = 0; i < ports; i += 2)
                {
                    int j = (i + ports - 1) % ports;
                    DrawGroove(group, w / 2, h / 2, w / 2 - 6, angles[j], angles[i]);
                }

                // Ports
                for (int i = 0; i < ports; i++)
                {
                    double a = angles[i] * Math.PI / 180;
                    double r = w / 2 + 5;
                    double px = w / 2 + r * Math.Cos(a) - 5;
                    double py = h / 2 + r * Math.Sin(a) - 5;
                    var dot = new Ellipse
                    {
                        Width = 10, Height = 10, Fill = Brushes.Yellow, Stroke = Brushes.Black,
                        StrokeThickness = 1, Cursor = Cursors.Cross,
                        ToolTip = name + " P" + (i + 1) + (ports == 6 ? " (" + Port6Clocks[i] + " o'clk)" : "")
                    };
                    Canvas.SetLeft(dot, px); Canvas.SetTop(dot, py);
                    group.Children.Add(dot);
                    var pd = new PortDot { PortNum = i + 1, Dot = dot, Owner = item, AngleDeg = angles[i] };
                    item.Ports.Add(pd);
                    dot.Tag = pd;
                    var num = new TextBlock { Text = (i + 1).ToString(), FontSize = 7, Foreground = Brushes.Black, IsHitTestVisible = false };
                    Canvas.SetLeft(num, px + 2); Canvas.SetTop(num, py);
                    group.Children.Add(num);
                }

                // Center label
                string shortLabel = label.Length > 6 ? label.Substring(0, 6) : label;
                var lbl = new TextBlock { Text = shortLabel, FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.White, IsHitTestVisible = false };
                Canvas.SetLeft(lbl, w / 2 - shortLabel.Length * 3); Canvas.SetTop(lbl, h / 2 - 7);
                group.Children.Add(lbl);
            }
            else if (type.StartsWith("Loop"))
            {
                // Invisible background for hit-testing (so you can drag it)
                var hitRect = new Rectangle { Width = w, Height = h, Fill = Brushes.Transparent };
                group.Children.Add(hitRect);

                // Loop as coiled tubing — two ends coming in, lasso coil in middle
                double tubeThick = 2.5;
                var brush = new SolidColorBrush(fill);
                double cx = w / 2, cy = h / 2;
                double coilR = Math.Min(w, h) / 2 - 8; // coil radius
                int coils = w < 30 ? 1 : w < 50 ? 2 : 3; // more coils for bigger loops

                // Inlet line (bottom-left coming in)
                var inlet = new Line { X1 = 0, Y1 = h, X2 = cx - coilR, Y2 = cy, Stroke = brush, StrokeThickness = tubeThick };
                group.Children.Add(inlet);

                // Coil spiral
                for (int c = 0; c < coils; c++)
                {
                    double offset = c * 4;
                    double r = coilR - offset;
                    var coilPath = new System.Windows.Shapes.Path { Stroke = brush, StrokeThickness = tubeThick, Fill = Brushes.Transparent };
                    var fig = new PathFigure { StartPoint = new Point(cx - r, cy) };
                    // Full circle arc (two semicircles)
                    fig.Segments.Add(new ArcSegment(new Point(cx + r, cy), new Size(r, r), 0, false, SweepDirection.Clockwise, true));
                    fig.Segments.Add(new ArcSegment(new Point(cx - r, cy), new Size(r, r), 0, false, SweepDirection.Clockwise, true));
                    var geom = new PathGeometry(); geom.Figures.Add(fig);
                    coilPath.Data = geom; coilPath.IsHitTestVisible = false;
                    group.Children.Add(coilPath);
                }

                // Outlet line (bottom-right going out)
                var outlet = new Line { X1 = cx + coilR, Y1 = cy, X2 = w, Y2 = h, Stroke = brush, StrokeThickness = tubeThick };
                group.Children.Add(outlet);

                // Label
                var lbl = new TextBlock
                {
                    Text = label, FontSize = 7, FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromArgb(180, fill.R, fill.G, fill.B)),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(lbl, cx - label.Length * 2); Canvas.SetTop(lbl, 0);
                group.Children.Add(lbl);
            }
            else if (type.StartsWith("Trap") || type.StartsWith("Column"))
            {
                // Column/Trap as inline cylinder with inlet/outlet stubs
                double tubeThick = 2;
                var brush = new SolidColorBrush(fill);
                double bodyW = w - 4, bodyH = h - 16; // leave room for stubs
                double bodyX = 2, bodyY = 8;

                // Inlet stub (top)
                var inlet = new Line { X1 = w / 2, Y1 = 0, X2 = w / 2, Y2 = bodyY, Stroke = brush, StrokeThickness = tubeThick };
                group.Children.Add(inlet);

                // Cylinder body with gradient for 3D effect
                var body = new Border
                {
                    Width = bodyW, Height = bodyH,
                    CornerRadius = new CornerRadius(bodyW / 2),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(200, fill.R, fill.G, fill.B)),
                    BorderThickness = new Thickness(1.5)
                };
                body.Background = new LinearGradientBrush(
                    Color.FromArgb(200, (byte)Math.Min(fill.R + 50, 255), (byte)Math.Min(fill.G + 50, 255), (byte)Math.Min(fill.B + 50, 255)),
                    Color.FromArgb(230, (byte)Math.Max(fill.R - 30, 0), (byte)Math.Max(fill.G - 30, 0), (byte)Math.Max(fill.B - 30, 0)),
                    new Point(0, 0.5), new Point(1, 0.5));
                body.Child = new TextBlock
                {
                    Text = label, FontSize = 6, FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(body, bodyX); Canvas.SetTop(body, bodyY);
                group.Children.Add(body);

                // Outlet stub (bottom)
                var outlet = new Line { X1 = w / 2, Y1 = bodyY + bodyH, X2 = w / 2, Y2 = h, Stroke = brush, StrokeThickness = tubeThick };
                group.Children.Add(outlet);

                // Packing indicator (horizontal lines inside)
                for (double py = bodyY + 8; py < bodyY + bodyH - 5; py += 6)
                {
                    var pk = new Line { X1 = bodyX + 3, Y1 = py, X2 = bodyX + bodyW - 3, Y2 = py,
                        Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), StrokeThickness = 0.5, IsHitTestVisible = false };
                    group.Children.Add(pk);
                }

                // Flow direction arrow (pointing down into column)
                var arrow = new Polygon
                {
                    Points = new PointCollection { new Point(w/2 - 4, 1), new Point(w/2 + 4, 1), new Point(w/2, 7) },
                    Fill = new SolidColorBrush(Color.FromArgb(200, fill.R, fill.G, fill.B)),
                    IsHitTestVisible = false
                };
                group.Children.Add(arrow);
            }
            else if (type == "Plug")
            {
                // Red X symbol
                var hitRect = new Rectangle { Width = w, Height = h, Fill = Brushes.Transparent };
                group.Children.Add(hitRect);
                var line1 = new Line { X1 = 2, Y1 = 2, X2 = w - 2, Y2 = h - 2, Stroke = Brushes.Red, StrokeThickness = 3 };
                var line2 = new Line { X1 = w - 2, Y1 = 2, X2 = 2, Y2 = h - 2, Stroke = Brushes.Red, StrokeThickness = 3 };
                group.Children.Add(line1); group.Children.Add(line2);
                var circle = new Ellipse { Width = w, Height = h, Stroke = Brushes.DarkRed, StrokeThickness = 1.5, Fill = Brushes.Transparent };
                group.Children.Add(circle);
                item.HasSnapPoints = true;
                // Single center snap point
                var snap = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Red, Stroke = Brushes.DarkRed,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name };
                Canvas.SetLeft(snap, w / 2 - 4); Canvas.SetTop(snap, h / 2 - 4);
                group.Children.Add(snap);
                item.Ports.Add(new PortDot { PortNum = 1, Dot = snap, Owner = item, AngleDeg = 0 }); snap.Tag = item.Ports.Last();
            }
            else
            {
                var border = new Border
                {
                    Width = w, Height = h, Background = new SolidColorBrush(fill),
                    BorderBrush = Brushes.Black, BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3), Opacity = 0.85
                };
                border.Child = new TextBlock
                {
                    Text = label.Length > 12 ? label.Substring(0, 12) : label,
                    Foreground = Brushes.White, FontSize = w < 30 ? 7 : 9, FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center
                };
                group.Children.Add(border);
            }
            // Add snap points for loops, traps, columns (inlet at top, outlet at bottom)
            if (type.StartsWith("Loop"))
            {
                // Snap points at the actual tubing ends: bottom-left and bottom-right
                item.HasSnapPoints = true;
                var snapL = new Ellipse { Width = 10, Height = 10, Fill = Brushes.Lime, Stroke = Brushes.DarkGreen,
                    StrokeThickness = 1.5, Cursor = Cursors.Cross, ToolTip = item.Name + " End A" };
                Canvas.SetLeft(snapL, -5); Canvas.SetTop(snapL, h - 5);
                group.Children.Add(snapL);
                item.Ports.Add(new PortDot { PortNum = 1, Dot = snapL, Owner = item, AngleDeg = 135 }); snapL.Tag = item.Ports.Last();

                var snapR = new Ellipse { Width = 10, Height = 10, Fill = Brushes.Orange, Stroke = Brushes.DarkOrange,
                    StrokeThickness = 1.5, Cursor = Cursors.Cross, ToolTip = item.Name + " End B" };
                Canvas.SetLeft(snapR, w - 5); Canvas.SetTop(snapR, h - 5);
                group.Children.Add(snapR);
                item.Ports.Add(new PortDot { PortNum = 2, Dot = snapR, Owner = item, AngleDeg = 45 }); snapR.Tag = item.Ports.Last();
            }
            else if (type == "Mixing Tee")
            {
                // Mixing Tee has 3 connection points: left input, right input, bottom output
                item.HasSnapPoints = true;
                // Input A (center top)
                var snapT = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Lime, Stroke = Brushes.DarkGreen,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name + " Input A (top)" };
                Canvas.SetLeft(snapT, w / 2 - 4); Canvas.SetTop(snapT, -4);
                group.Children.Add(snapT);
                item.Ports.Add(new PortDot { PortNum = 1, Dot = snapT, Owner = item, AngleDeg = -90 }); snapT.Tag = item.Ports.Last();

                // Input B (center bottom)
                var snapB = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Lime, Stroke = Brushes.DarkGreen,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name + " Input B (bottom)" };
                Canvas.SetLeft(snapB, w / 2 - 4); Canvas.SetTop(snapB, h - 4);
                group.Children.Add(snapB);
                item.Ports.Add(new PortDot { PortNum = 2, Dot = snapB, Owner = item, AngleDeg = 90 }); snapB.Tag = item.Ports.Last();

                // Output (left center)
                var snapL = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Orange, Stroke = Brushes.DarkOrange,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name + " Output (left)" };
                Canvas.SetLeft(snapL, -4); Canvas.SetTop(snapL, h / 2 - 4);
                group.Children.Add(snapL);
                item.Ports.Add(new PortDot { PortNum = 3, Dot = snapL, Owner = item, AngleDeg = 180 }); snapL.Tag = item.Ports.Last();
            }
            else if (type == "Pump")
            {
                item.HasSnapPoints = true;
                var snapT = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Lime, Stroke = Brushes.DarkGreen,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name + " Top (12 o'clk)" };
                Canvas.SetLeft(snapT, w / 2 - 4); Canvas.SetTop(snapT, -4);
                group.Children.Add(snapT);
                item.Ports.Add(new PortDot { PortNum = 1, Dot = snapT, Owner = item, AngleDeg = -90 }); snapT.Tag = item.Ports.Last();

                var snapBt = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Orange, Stroke = Brushes.DarkOrange,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name + " Bottom (6 o'clk)" };
                Canvas.SetLeft(snapBt, w / 2 - 4); Canvas.SetTop(snapBt, h - 4);
                group.Children.Add(snapBt);
                item.Ports.Add(new PortDot { PortNum = 2, Dot = snapBt, Owner = item, AngleDeg = 90 }); snapBt.Tag = item.Ports.Last();
            }
            else if (type == "Flow Sensor")
            {
                item.HasSnapPoints = true;
                var snapL = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Lime, Stroke = Brushes.DarkGreen,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name + " Inlet (left)" };
                Canvas.SetLeft(snapL, -4); Canvas.SetTop(snapL, h / 2 - 4);
                group.Children.Add(snapL);
                item.Ports.Add(new PortDot { PortNum = 1, Dot = snapL, Owner = item, AngleDeg = 180 }); snapL.Tag = item.Ports.Last();

                var snapR = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Orange, Stroke = Brushes.DarkOrange,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name + " Outlet (right)" };
                Canvas.SetLeft(snapR, w - 4); Canvas.SetTop(snapR, h / 2 - 4);
                group.Children.Add(snapR);
                item.Ports.Add(new PortDot { PortNum = 2, Dot = snapR, Owner = item, AngleDeg = 0 }); snapR.Tag = item.Ports.Last();
            }
            else if (type.StartsWith("Trap") || type.StartsWith("Column") || type == "Injection Port" || type == "Output" || type == "Pressure Sensor" || type == "Waste Port" || type == "Solvent Bottle")
            {
                item.HasSnapPoints = true;
                // Inlet snap point (top)
                var snapIn = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Lime, Stroke = Brushes.DarkGreen,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name + " Inlet" };
                Canvas.SetLeft(snapIn, w / 2 - 4); Canvas.SetTop(snapIn, -4);
                group.Children.Add(snapIn);
                item.Ports.Add(new PortDot { PortNum = 1, Dot = snapIn, Owner = item, AngleDeg = -90 }); snapIn.Tag = item.Ports.Last();

                // Outlet snap point (bottom)
                var snapOut = new Ellipse { Width = 8, Height = 8, Fill = Brushes.Orange, Stroke = Brushes.DarkOrange,
                    StrokeThickness = 1, Cursor = Cursors.Cross, ToolTip = item.Name + " Outlet" };
                Canvas.SetLeft(snapOut, w / 2 - 4); Canvas.SetTop(snapOut, h - 4);
                group.Children.Add(snapOut);
                item.Ports.Add(new PortDot { PortNum = 2, Dot = snapOut, Owner = item, AngleDeg = 90 }); snapOut.Tag = item.Ports.Last();
            }

            group.Tag = item; group.Cursor = Cursors.SizeAll;
            item.Visual = group;
            Canvas.SetLeft(group, x); Canvas.SetTop(group, y);

            // Apply rotation
            if (item.Rotation != 0)
                group.RenderTransform = new RotateTransform(item.Rotation, w / 2, h / 2);

            editorCanvas.Children.Add(group);
            _items.Add(item);
            _actionHistory.Add(item);
        }

        void DrawGroove(Canvas c, double cx, double cy, double r, double a1d, double a2d)
        {
            double a1 = a1d * Math.PI / 180, a2 = a2d * Math.PI / 180;
            double x1 = cx + r * Math.Cos(a1), y1 = cy + r * Math.Sin(a1);
            double x2 = cx + r * Math.Cos(a2), y2 = cy + r * Math.Sin(a2);
            double ma = (a1 + a2) / 2; double mr = r * 0.65;
            var fig = new PathFigure { StartPoint = new Point(x1, y1) };
            fig.Segments.Add(new QuadraticBezierSegment(new Point(cx + mr * Math.Cos(ma), cy + mr * Math.Sin(ma)), new Point(x2, y2), true));
            var geom = new PathGeometry(); geom.Figures.Add(fig);
            var path = new System.Windows.Shapes.Path
            {
                Data = geom, Stroke = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200)),
                StrokeThickness = 3, StrokeDashArray = new DoubleCollection { 3, 2 }, IsHitTestVisible = false
            };
            c.Children.Add(path);
        }

        void RebuildVisual(DiagramItem item)
        {
            double x = item.X, y = item.Y;
            editorCanvas.Children.Remove(item.Visual);
            _items.Remove(item);
            BuildVisual(item.Name, item.Type, x, y, item.W, item.H, item.FillColor, item.Label, item.PortCount);
        }

        // --- Z-ORDER ---
        void BringFront_Click(object s, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            editorCanvas.Children.Remove(_selectedItem.Visual);
            editorCanvas.Children.Add(_selectedItem.Visual);
        }
        void SendBack_Click(object s, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            editorCanvas.Children.Remove(_selectedItem.Visual);
            editorCanvas.Children.Insert(0, _selectedItem.Visual);
        }
        void BringUp_Click(object s, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            int idx = editorCanvas.Children.IndexOf(_selectedItem.Visual);
            if (idx < editorCanvas.Children.Count - 1)
            {
                editorCanvas.Children.Remove(_selectedItem.Visual);
                editorCanvas.Children.Insert(idx + 1, _selectedItem.Visual);
            }
        }
        void SendDown_Click(object s, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            int idx = editorCanvas.Children.IndexOf(_selectedItem.Visual);
            if (idx > 0)
            {
                editorCanvas.Children.Remove(_selectedItem.Visual);
                editorCanvas.Children.Insert(idx - 1, _selectedItem.Visual);
            }
        }

        // --- PRESETS ---
        void ClearCanvas()
        {
            editorCanvas.Children.Clear();
            _items.Clear();
            _tubingPaths.Clear();
            _activeTubing = null;
        }

        void LoadProteoElute()
        {
            AddItem("Pump", 5, 340, "Pump A");
            AddItem("Pump", 265, 340, "Pump B"); _items.Last().FillColor = Colors.IndianRed;
            AddItem("6-Port Valve", 80, 340, "Valve A");
            AddItem("6-Port Valve", 200, 340, "Valve B");
            AddItem("6-Port Valve", 360, 55, "Trap V"); _items.Last().FillColor = Colors.Goldenrod;
            AddItem("6-Port Valve", 360, 155, "Inj V"); _items.Last().FillColor = Colors.DarkSeaGreen;
            AddItem("Injection Port", 370, 10);
            AddItem("Output", 420, 75);
            AddItem("Pressure Sensor", 42, 385, "PS A");
            AddItem("Pressure Sensor", 254, 385, "PS B");
            AddItem("Flow Sensor", 140, 440, "FS A");
            AddItem("Flow Sensor", 140, 415, "FS B");
            AddItem("Mixing Tee", 330, 430);
            AddItem("Peltier Stack", 10, 30);
        }

        void LoadPreset3Valve()
        {
            ClearCanvas();
            AddItem("Pump", 20, 300, "Pump");
            AddItem("6-Port Valve", 120, 300, "Valve A");
            AddItem("6-Port Valve", 250, 150, "Valve B");
            AddItem("6-Port Valve", 380, 150, "Valve C");
            AddItem("Output", 420, 90);
            AddItem("Flow Sensor", 160, 420, "FS");
            AddItem("Pressure Sensor", 60, 360, "PS");
        }

        void LoadPreset4Valve()
        {
            ClearCanvas();
            AddItem("Pump", 20, 300, "Pump A");
            AddItem("Pump", 200, 300, "Pump B"); _items.Last().FillColor = Colors.IndianRed;
            AddItem("6-Port Valve", 100, 300, "Valve A");
            AddItem("6-Port Valve", 180, 300, "Valve B");
            AddItem("6-Port Valve", 320, 100, "Valve C");
            AddItem("6-Port Valve", 320, 200, "Valve D");
            AddItem("Output", 420, 100);
            AddItem("Flow Sensor", 140, 420, "FS A");
            AddItem("Flow Sensor", 140, 395, "FS B");
            AddItem("Mixing Tee", 300, 400);
        }

        void TryLoadSaved()
        {
            string path = System.IO.Path.Combine(SaveDir, SaveFile);
            if (File.Exists(path)) { LoadFromPath(path); }
            else { LoadProteoElute(); }
        }

        void LoadFromFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Load Diagram", Filter = "Diagram files (*.txt)|*.txt|All files (*.*)|*.*",
                InitialDirectory = SaveDir
            };
            if (dlg.ShowDialog() == true) { ClearCanvas(); LoadFromPath(dlg.FileName); }
        }

        void LoadFromPath(string path)
        {
            try
            {
                ClearCanvas();
                foreach (string line in File.ReadAllLines(path))
                {
                    if (line.StartsWith("COMP|"))
                    {
                        var parts = line.Split('|');
                        if (parts.Length >= 7)
                        {
                            string name = parts[1], type = parts[2];
                            double x = double.Parse(parts[3]), y = double.Parse(parts[4]);
                            double w = double.Parse(parts[5]), h = double.Parse(parts[6]);
                            double rot = parts.Length >= 8 ? double.Parse(parts[7]) : 0;
                            int portCount = 0;
                            if (type.Contains("Valve"))
                            {
                                string numStr = type.Replace("-Port Valve", "").Trim();
                                int.TryParse(numStr, out portCount);
                            }
                            Color fill = Colors.Gray;
                            switch (type)
                            {
                                case "Pump": fill = Colors.SteelBlue; break;
                                case "Flow Sensor": fill = Colors.Teal; break;
                                case "Pressure Sensor": fill = Colors.DimGray; break;
                                case "Mixing Tee": fill = Colors.Gray; break;
                                case "Peltier Stack": fill = Colors.LightSteelBlue; break;
                                default: if (type.Contains("Valve")) fill = Colors.DarkGray; break;
                            }
                            BuildVisual(name, type, x, y, w, h, fill, name, portCount, rot);
                        }
                    }
                    else if (line.StartsWith("TUBE|"))
                    {
                        var parts = line.Split('|');
                        if (parts.Length >= 4)
                        {
                            double thickness = double.Parse(parts[1]);
                            Color color = Colors.Gray;
                            try { color = (Color)ColorConverter.ConvertFromString(parts[2]); } catch { }
                            string ptsStr = parts[3];
                            var points = ptsStr.Split(';').Select(p =>
                            {
                                var xy = p.Split(',');
                                return new Point(double.Parse(xy[0]), double.Parse(xy[1]));
                            }).ToList();

                            var tp = new TubingPath { Thickness = thickness, Color = color, Points = points };
                            _tubingPaths.Add(tp);
                            var brush = new SolidColorBrush(color);
                            for (int i = 1; i < points.Count; i++)
                            {
                                var ln = new Line
                                {
                                    X1 = points[i - 1].X, Y1 = points[i - 1].Y,
                                    X2 = points[i].X, Y2 = points[i].Y,
                                    Stroke = brush, StrokeThickness = thickness, IsHitTestVisible = false
                                };
                                editorCanvas.Children.Add(ln);
                                tp.Visuals.Add(ln);
                            }
                        }
                    }
                }
                txtSelected.Text = "Loaded from " + System.IO.Path.GetFileName(path);
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); LoadProteoElute(); }
        }

        // --- INPUT ---
        PortDot FindNearestPort(Point pos, double maxDist = 20)
        {
            PortDot nearest = null; double best = maxDist;
            foreach (var item in _items)
                foreach (var pd in item.Ports)
                {
                    var pc = GetPortCenter(pd);
                    double d = Math.Sqrt(Math.Pow(pc.X - pos.X, 2) + Math.Pow(pc.Y - pos.Y, 2));
                    if (d < best) { best = d; nearest = pd; }
                }
            return nearest;
        }

        void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(editorCanvas);
            var hitPort = FindPort(e);

            // Connect ports mode — takes priority over everything
            if (_connectType != null)
            {
                if (hitPort != null) { HandleConnectClick(hitPort); e.Handled = true; return; }
                var nearPort = FindNearestPort(pos, 30);
                if (nearPort != null) { HandleConnectClick(nearPort); e.Handled = true; return; }
                txtConnectStatus.Text = "No port found. Click closer to a port dot.";
                e.Handled = true;
                return;
            }

            if (hitPort != null && IsDrawMode)
            {
                var center = GetPortCenter(hitPort);
                AddTubingPoint(center, hitPort);
                txtSelected.Text = "Anchored to " + hitPort.Owner.Name + " P" + hitPort.PortNum;
                return;
            }
            if (IsDrawMode)
            {
                var nearPort = FindNearestPort(pos);
                if (nearPort != null)
                {
                    var center = GetPortCenter(nearPort);
                    AddTubingPoint(center, nearPort);
                    txtSelected.Text = "Anchored to " + nearPort.Owner.Name + " P" + nearPort.PortNum;
                }
                else
                {
                    AddTubingPoint(pos);
                }
                return;
            }

            var item = FindItem(e);
            if (item != null)
            {
                _dragElement = (FrameworkElement)item.Visual; _dragOffset = e.GetPosition(_dragElement);
                _isDragging = true; _selectedItem = item; _dragElement.CaptureMouse();
                txtSelected.Text = item.Name + " (" + item.Type + ")"; UpdateCoords(item);
                _updatingSize = true;
                txtWidth.Text = item.W.ToString("F0"); txtHeight.Text = item.H.ToString("F0");
                txtRotation.Text = item.Rotation.ToString("F0");
                _updatingSize = false;
            }
        }

        void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _dragElement == null) return;
            var pos = e.GetPosition(editorCanvas);
            double nx = Math.Round((pos.X - _dragOffset.X) / 5) * 5;
            double ny = Math.Round((pos.Y - _dragOffset.Y) / 5) * 5;
            Canvas.SetLeft(_dragElement, nx); Canvas.SetTop(_dragElement, ny);
            _selectedItem.X = nx; _selectedItem.Y = ny; UpdateCoords(_selectedItem);

            // Update all anchored tubing that connects to this component's ports
            UpdateAnchoredTubing(_selectedItem);
        }

        void Canvas_MouseLeftButtonUp(object s, MouseButtonEventArgs e)
        {
            if (_isDragging && _dragElement != null)
            {
                // Try to dock: if this component has snap points, check if any snap point
                // is near a valve port — if so, dock the component onto that port
                if (_selectedItem != null && _selectedItem.HasSnapPoints)
                    TryDockToPort(_selectedItem);

                _dragElement.ReleaseMouseCapture();
                _isDragging = false;
                _dragElement = null;
            }
        }

        void TryDockToPort(DiagramItem item)
        {
            // For each of this item's snap points, find the nearest valve port or snap point
            PortDot bestMySnap = null, bestValvePort = null;
            double bestDist = 35; // increased range

            foreach (var mySnap in item.Ports)
            {
                var myPos = GetPortCenter(mySnap);
                foreach (var other in _items)
                {
                    if (other == item || other.Ports.Count == 0) continue;
                    foreach (var otherPort in other.Ports)
                    {
                        var otherPos = GetPortCenter(otherPort);
                        double dist = Math.Sqrt(Math.Pow(myPos.X - otherPos.X, 2) + Math.Pow(myPos.Y - otherPos.Y, 2));
                        if (dist < bestDist)
                        {
                            bestDist = dist; bestMySnap = mySnap; bestValvePort = otherPort;
                        }
                    }
                }
            }

            if (bestMySnap == null) return;

            // Dock first end: move item so this snap sits on the valve port
            var dockPos = GetPortCenter(bestValvePort);
            var snapPos = GetPortCenter(bestMySnap);
            double offsetX = snapPos.X - item.X;
            double offsetY = snapPos.Y - item.Y;
            item.X = dockPos.X - offsetX;
            item.Y = dockPos.Y - offsetY;
            Canvas.SetLeft(item.Visual, item.X);
            Canvas.SetTop(item.Visual, item.Y);

            // Color change: locked snap points turn red, valve port turns cyan
            bestMySnap.Dot.Fill = Brushes.Red;
            bestMySnap.Dot.Stroke = Brushes.DarkRed;
            bestValvePort.Dot.Fill = Brushes.Cyan;
            bestValvePort.Dot.Stroke = Brushes.DarkCyan;

            txtSelected.Text = "DOCKED: " + item.Name + " snap " + bestMySnap.PortNum + " → " + bestValvePort.Owner.Name + " P" + bestValvePort.PortNum;

            // Now check: does the OTHER snap point of this item have a nearby valve port?
            // If so, stretch/resize the component to bridge both ports
            if (item.Ports.Count >= 2)
            {
                var otherSnap = item.Ports.First(p => p != bestMySnap);
                var otherSnapPos = GetPortCenter(otherSnap);

                PortDot secondPort = null;
                double secondBest = 40; // wider search for second port

                foreach (var other in _items)
                {
                    if (other == item || other.PortCount == 0) continue;
                    foreach (var vp in other.Ports)
                    {
                        if (vp == bestValvePort) continue;
                        var vpPos = GetPortCenter(vp);
                        double d = Math.Sqrt(Math.Pow(otherSnapPos.X - vpPos.X, 2) + Math.Pow(otherSnapPos.Y - vpPos.Y, 2));
                        if (d < secondBest) { secondBest = d; secondPort = vp; }
                    }
                }

                if (secondPort != null)
                {
                    // Stretch: calculate needed size to bridge both ports
                    var port1Pos = GetPortCenter(bestValvePort);
                    var port2Pos = GetPortCenter(secondPort);
                    double dx = port2Pos.X - port1Pos.X;
                    double dy = port2Pos.Y - port1Pos.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    // Calculate rotation to align inlet→outlet with port1→port2
                    double angle = Math.Atan2(dy, dx) * 180 / Math.PI + 90; // +90 because inlet is top

                    // Resize height to match distance
                    item.H = distance;
                    item.Rotation = angle;

                    // Rebuild visual with new size and rotation
                    RebuildVisual(item);
                    _selectedItem = _items.Last(); // RebuildVisual creates new item

                    // Re-dock after rebuild (positions changed)
                    var newItem = _items.Last();
                    var newSnap1 = newItem.Ports.FirstOrDefault(p => p.PortNum == bestMySnap.PortNum);
                    if (newSnap1 != null)
                    {
                        var ns1Pos = GetPortCenter(newSnap1);
                        newItem.X += port1Pos.X - ns1Pos.X;
                        newItem.Y += port1Pos.Y - ns1Pos.Y;
                        Canvas.SetLeft(newItem.Visual, newItem.X);
                        Canvas.SetTop(newItem.Visual, newItem.Y);
                    }

                    // Color both valve ports cyan to show they're bridged
                    bestValvePort.Dot.Fill = Brushes.Cyan;
                    bestValvePort.Dot.Stroke = Brushes.DarkCyan;
                    secondPort.Dot.Fill = Brushes.Cyan;
                    secondPort.Dot.Stroke = Brushes.DarkCyan;

                    txtSelected.Text = "BRIDGED: " + newItem.Name + " between " +
                        bestValvePort.Owner.Name + " P" + bestValvePort.PortNum + " ↔ " +
                        secondPort.Owner.Name + " P" + secondPort.PortNum;
                }
            }

            UpdateCoords(_selectedItem ?? item);
            UpdateAnchoredTubing(item);
        }

        void UpdateAnchoredTubing(DiagramItem movedItem)
        {
            foreach (var tp in _tubingPaths)
            {
                bool changed = false;
                foreach (var anchor in tp.Anchors)
                {
                    if (anchor.Port.Owner == movedItem)
                    {
                        var newPos = GetPortCenter(anchor.Port);
                        if (anchor.PointIndex < tp.Points.Count)
                        {
                            tp.Points[anchor.PointIndex] = newPos;
                            changed = true;
                        }
                    }
                }
                if (changed) RedrawTubing(tp);
            }
        }

        void RedrawTubing(TubingPath tp)
        {
            // Remove old visuals
            foreach (var v in tp.Visuals) editorCanvas.Children.Remove(v);
            tp.Visuals.Clear();

            var brush = new SolidColorBrush(tp.Color);
            // Redraw dots and lines
            for (int i = 0; i < tp.Points.Count; i++)
            {
                var pt = tp.Points[i];
                var dot = new Ellipse { Width = 6, Height = 6, Fill = brush, IsHitTestVisible = false };
                Canvas.SetLeft(dot, pt.X - 3); Canvas.SetTop(dot, pt.Y - 3);
                editorCanvas.Children.Add(dot);
                tp.Visuals.Add(dot);

                if (i > 0)
                {
                    var prev = tp.Points[i - 1];
                    var line = new Line
                    {
                        X1 = prev.X, Y1 = prev.Y, X2 = pt.X, Y2 = pt.Y,
                        Stroke = brush, StrokeThickness = tp.Thickness, IsHitTestVisible = false
                    };
                    editorCanvas.Children.Add(line);
                    tp.Visuals.Add(line);
                }
            }
        }

        void Canvas_RightClick(object s, MouseButtonEventArgs e)
        {
            var port = FindPort(e);
            if (port != null)
                txtSelected.Text = port.Owner.Name + " Port " + port.PortNum +
                    (port.Owner.PortCount == 6 ? " (" + Port6Clocks[port.PortNum - 1] + " o'clk)" : "");
        }

        // --- CONNECT PORTS MODE ---
        private string _connectType;
        private PortDot _connectPort1;

        void StartConnectMode(string componentType)
        {
            _connectType = componentType;
            _connectPort1 = null;
            rbModeDrag.IsChecked = true; // switch to drag mode so clicks go to ports
            txtConnectStatus.Text = "Click FIRST port for " + componentType;
        }

        void HandleConnectClick(PortDot port)
        {
            if (_connectPort1 == null)
            {
                _connectPort1 = port;
                port.Dot.Fill = Brushes.Magenta;
                txtConnectStatus.Text = "Port 1: " + port.Owner.Name + " P" + port.PortNum + "\nClick SECOND port...";
            }
            else
            {
                try
                {
                    var p1 = GetPortCenter(_connectPort1);
                    var p2 = GetPortCenter(port);
                    port.Dot.Fill = Brushes.Magenta;

                    double dx = p2.X - p1.X, dy = p2.Y - p1.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    double angle = Math.Atan2(dy, dx) * 180 / Math.PI;
                    double midX = (p1.X + p2.X) / 2, midY = (p1.Y + p2.Y) / 2;

                    // Calculate size and position first
                    double cw, ch, cx, cy, crot;
                    string ctype = _connectType;

                    if (ctype.StartsWith("Loop"))
                    {
                        // Draw a curved tubing arc OUTSIDE the valve between the two ports
                        // Find valve center to calculate outward bulge direction
                        double valveCX = _connectPort1.Owner.X + _connectPort1.Owner.W / 2;
                        double valveCY = _connectPort1.Owner.Y + _connectPort1.Owner.H / 2;

                        // Outward direction from valve center through midpoint
                        double outDX = midX - valveCX;
                        double outDY = midY - valveCY;
                        double outLen = Math.Sqrt(outDX * outDX + outDY * outDY);
                        if (outLen < 1) outLen = 1;

                        // Bulge amount based on loop volume
                        double bulge = 30;
                        if (ctype.Contains("5u")) bulge = 40;
                        if (ctype.Contains("20u")) bulge = 55;
                        if (ctype.Contains("50u")) bulge = 70;

                        // Control point: pushed outward from valve center
                        double cpX = midX + (outDX / outLen) * bulge;
                        double cpY = midY + (outDY / outLen) * bulge;

                        // Draw the arc as a bezier curve
                        Color loopColor = Colors.DarkSeaGreen;
                        double loopThick = 3;
                        var fig = new PathFigure { StartPoint = p1 };
                        fig.Segments.Add(new QuadraticBezierSegment(new Point(cpX, cpY), p2, true));
                        var geom = new PathGeometry(); geom.Figures.Add(fig);
                        var arcPath = new System.Windows.Shapes.Path
                        {
                            Data = geom, Stroke = new SolidColorBrush(loopColor),
                            StrokeThickness = loopThick, Fill = Brushes.Transparent, IsHitTestVisible = false
                        };
                        editorCanvas.Children.Add(arcPath);

                        // Label at the control point
                        var lbl = new TextBlock
                        {
                            Text = ctype, FontSize = 8, Foreground = new SolidColorBrush(loopColor),
                            FontWeight = FontWeights.Bold, IsHitTestVisible = false
                        };
                        Canvas.SetLeft(lbl, cpX - 15); Canvas.SetTop(lbl, cpY - 5);
                        editorCanvas.Children.Add(lbl);

                        // Small dots at connection points
                        var d1 = new Ellipse { Width = 6, Height = 6, Fill = new SolidColorBrush(loopColor), IsHitTestVisible = false };
                        Canvas.SetLeft(d1, p1.X - 3); Canvas.SetTop(d1, p1.Y - 3);
                        editorCanvas.Children.Add(d1);
                        var d2 = new Ellipse { Width = 6, Height = 6, Fill = new SolidColorBrush(loopColor), IsHitTestVisible = false };
                        Canvas.SetLeft(d2, p2.X - 3); Canvas.SetTop(d2, p2.Y - 3);
                        editorCanvas.Children.Add(d2);

                        // Store as tubing path for save/undo
                        var tp = new TubingPath { Thickness = loopThick, Color = loopColor, Points = { p1, new Point(cpX, cpY), p2 } };
                        tp.Visuals.Add(arcPath); tp.Visuals.Add(lbl); tp.Visuals.Add(d1); tp.Visuals.Add(d2);
                        _tubingPaths.Add(tp);
                        _actionHistory.Add(tp);
                    }
                    else
                    {
                        // Trap/Column: draw as inline cylinder
                        double tcw = Math.Max(18, 18);
                        double tch = Math.Max(dist, 30);
                        double tcrot = angle - 90;
                        double tcx = midX - tcw / 2;
                        double tcy = midY - tch / 2;

                        Color fill = Colors.Gray;
                        switch (ctype)
                        {
                            case "Trap 5cm": fill = Color.FromRgb(102, 68, 34); break;
                            case "Trap 15cm": fill = Color.FromRgb(136, 102, 68); break;
                            case "Trap 25cm": fill = Color.FromRgb(170, 136, 102); break;
                            case "Column 15cm": fill = Color.FromRgb(68, 85, 102); break;
                            case "Column 25cm": fill = Color.FromRgb(85, 102, 119); break;
                            case "Column 50cm": fill = Color.FromRgb(102, 119, 136); break;
                        }
                        BuildVisual(ctype + "_conn", ctype, tcx, tcy, tcw, tch, fill, ctype, 0, tcrot);
                    }

                    // Color the ports
                    _connectPort1.Dot.Fill = Brushes.Cyan;
                    _connectPort1.Dot.Stroke = Brushes.DarkCyan;
                    port.Dot.Fill = Brushes.Cyan;
                    port.Dot.Stroke = Brushes.DarkCyan;

                    txtConnectStatus.Text = "CONNECTED: " + ctype + "\n" +
                        _connectPort1.Owner.Name + " P" + _connectPort1.PortNum + " ↔ " +
                        port.Owner.Name + " P" + port.PortNum;
                }
                catch (Exception ex)
                {
                    txtConnectStatus.Text = "Error: " + ex.Message;
                }

                _connectType = null;
                _connectPort1 = null;
                rbModeDrag.IsChecked = true; // switch back to drag mode
            }
        }

        void ConnectLoop1_Click(object s, RoutedEventArgs e) { StartConnectMode("Loop 1uL"); }
        void ConnectLoop5_Click(object s, RoutedEventArgs e) { StartConnectMode("Loop 5uL"); }
        void ConnectLoop20_Click(object s, RoutedEventArgs e) { StartConnectMode("Loop 20uL"); }
        void ConnectLoop50_Click(object s, RoutedEventArgs e) { StartConnectMode("Loop 50uL"); }
        void ConnectTrap5_Click(object s, RoutedEventArgs e) { StartConnectMode("Trap 5cm"); }
        void ConnectTrap15_Click(object s, RoutedEventArgs e) { StartConnectMode("Trap 15cm"); }
        void ConnectTrap25_Click(object s, RoutedEventArgs e) { StartConnectMode("Trap 25cm"); }
        void ConnectCol15_Click(object s, RoutedEventArgs e) { StartConnectMode("Column 15cm"); }
        void ConnectCol25_Click(object s, RoutedEventArgs e) { StartConnectMode("Column 25cm"); }
        void ConnectCol50_Click(object s, RoutedEventArgs e) { StartConnectMode("Column 50cm"); }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _activeTubing != null) FinishTubing();
            if (e.Key == Key.Escape)
            {
                if (_activeTubing != null) CancelTubing();
                if (_connectType != null) { _connectType = null; _connectPort1 = null; txtConnectStatus.Text = "Cancelled"; }
            }
        }

        DiagramItem FindItem(MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as FrameworkElement;
            while (hit != null)
            {
                if (hit.Tag is DiagramItem di) return di;
                if (hit.Tag is PortDot) return null;
                hit = VisualTreeHelper.GetParent(hit) as FrameworkElement;
            }
            return null;
        }

        PortDot FindPort(MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as FrameworkElement;
            while (hit != null)
            {
                if (hit.Tag is PortDot pd) return pd;
                hit = VisualTreeHelper.GetParent(hit) as FrameworkElement;
            }
            return null;
        }

        Point GetPortCenter(PortDot pd)
        {
            if (pd.Owner.HasSnapPoints)
            {
                if (pd.Owner.Type.StartsWith("Loop"))
                {
                    // End A = bottom-left, End B = bottom-right
                    if (pd.PortNum == 1) return new Point(pd.Owner.X, pd.Owner.Y + pd.Owner.H);
                    return new Point(pd.Owner.X + pd.Owner.W, pd.Owner.Y + pd.Owner.H);
                }
                if (pd.Owner.Type == "Mixing Tee")
                {
                    if (pd.PortNum == 1) return new Point(pd.Owner.X + pd.Owner.W / 2, pd.Owner.Y);
                    if (pd.PortNum == 2) return new Point(pd.Owner.X + pd.Owner.W / 2, pd.Owner.Y + pd.Owner.H);
                    return new Point(pd.Owner.X, pd.Owner.Y + pd.Owner.H / 2);
                }
                if (pd.Owner.Type == "Flow Sensor")
                {
                    // Horizontal: 1=left center, 2=right center
                    if (pd.PortNum == 1) return new Point(pd.Owner.X, pd.Owner.Y + pd.Owner.H / 2);
                    return new Point(pd.Owner.X + pd.Owner.W, pd.Owner.Y + pd.Owner.H / 2);
                }
                // Standard 2-port: port 1 = top center, port 2 = bottom center
                double cx = pd.Owner.X + pd.Owner.W / 2;
                if (pd.PortNum == 1) return new Point(cx, pd.Owner.Y);
                else return new Point(cx, pd.Owner.Y + pd.Owner.H);
            }
            double pcx = pd.Owner.X + pd.Owner.W / 2;
            double pcy = pd.Owner.Y + pd.Owner.H / 2;
            double a = pd.AngleDeg * Math.PI / 180;
            double r = pd.Owner.W / 2 + 5;
            return new Point(pcx + r * Math.Cos(a), pcy + r * Math.Sin(a));
        }

        void UpdateCoords(DiagramItem item)
        {
            txtCoords.Text = "Left=\"" + item.X.ToString("F0") + "\" Top=\"" + item.Y.ToString("F0") + "\"\n" + item.W.ToString("F0") + " x " + item.H.ToString("F0");
        }

        // --- TUBING (smooth curves) ---
        private PortDot _tubeStartPort;
        private Point _tubeStartPt;

        void AddTubingPoint(Point pt, PortDot anchorPort = null)
        {
            if (_tubeStartPort == null && anchorPort == null && _activeTubing == null)
            {
                // First click on empty space — start freeform
                _tubeStartPt = pt;
                _tubeStartPort = null;
                _activeTubing = new TubingPath { Thickness = TubeWidth, Color = TubeColor };
                _tubingPaths.Add(_activeTubing);
                _activeTubing.Points.Add(pt);
                txtSelected.Text = "Click end point or port...";
                return;
            }

            if (_tubeStartPort == null && anchorPort != null && _activeTubing == null)
            {
                // First click on a port — remember start
                _tubeStartPort = anchorPort;
                _tubeStartPt = pt;
                txtSelected.Text = "Start: " + anchorPort.Owner.Name + " P" + anchorPort.PortNum + "\nClick end port...";
                return;
            }

            // Second click — draw smooth curve from start to here
            Point startPt = _tubeStartPt;
            Point endPt = pt;
            PortDot startPort = _tubeStartPort;
            PortDot endPort = anchorPort;

            DrawSmoothTubing(startPt, endPt, startPort, endPort);

            _tubeStartPort = null;
            _activeTubing = null;
        }

        void DrawSmoothTubing(Point p1, Point p2, PortDot port1, PortDot port2)
        {
            var brush = new SolidColorBrush(TubeColor);
            double thick = TubeWidth;
            double dx = p2.X - p1.X, dy = p2.Y - p1.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // Calculate control points for smooth S-curve
            // Tubing exits in the direction the port faces, then curves to destination
            double cp1X, cp1Y, cp2X, cp2Y;
            double exitLen = Math.Max(dist * 0.4, 25); // how far the tubing extends before curving

            if (port1 != null)
            {
                // Exit in port direction
                double a1 = port1.AngleDeg * Math.PI / 180;
                cp1X = p1.X + Math.Cos(a1) * exitLen;
                cp1Y = p1.Y + Math.Sin(a1) * exitLen;
            }
            else
            {
                // No port — use midpoint offset
                cp1X = p1.X + dx * 0.3;
                cp1Y = p1.Y;
            }

            if (port2 != null)
            {
                // Enter from port direction (opposite)
                double a2 = port2.AngleDeg * Math.PI / 180;
                cp2X = p2.X + Math.Cos(a2) * exitLen;
                cp2Y = p2.Y + Math.Sin(a2) * exitLen;
            }
            else
            {
                // No port — use midpoint offset
                cp2X = p2.X - dx * 0.3;
                cp2Y = p2.Y;
            }

            // Draw cubic Bezier
            var fig = new PathFigure { StartPoint = p1 };
            fig.Segments.Add(new BezierSegment(new Point(cp1X, cp1Y), new Point(cp2X, cp2Y), p2, true));
            var geom = new PathGeometry(); geom.Figures.Add(fig);
            var path = new System.Windows.Shapes.Path
            {
                Data = geom, Stroke = brush, StrokeThickness = thick,
                Fill = Brushes.Transparent, IsHitTestVisible = false
            };
            editorCanvas.Children.Add(path);

            // Small dots at endpoints
            var d1 = new Ellipse { Width = 5, Height = 5, Fill = brush, IsHitTestVisible = false };
            Canvas.SetLeft(d1, p1.X - 2.5); Canvas.SetTop(d1, p1.Y - 2.5);
            editorCanvas.Children.Add(d1);
            var d2 = new Ellipse { Width = 5, Height = 5, Fill = brush, IsHitTestVisible = false };
            Canvas.SetLeft(d2, p2.X - 2.5); Canvas.SetTop(d2, p2.Y - 2.5);
            editorCanvas.Children.Add(d2);

            // Store
            var tp = new TubingPath { Thickness = thick, Color = TubeColor, Points = { p1, new Point(cp1X, cp1Y), new Point(cp2X, cp2Y), p2 } };
            tp.Visuals.Add(path); tp.Visuals.Add(d1); tp.Visuals.Add(d2);
            if (port1 != null) tp.Anchors.Add(new TubingAnchor { Port = port1, PointIndex = 0 });
            if (port2 != null) tp.Anchors.Add(new TubingAnchor { Port = port2, PointIndex = 3 });
            _tubingPaths.Add(tp);
            _actionHistory.Add(tp);

            string msg = "Tubing: ";
            if (port1 != null) msg += port1.Owner.Name + " P" + port1.PortNum;
            else msg += "(" + p1.X.ToString("F0") + "," + p1.Y.ToString("F0") + ")";
            msg += " → ";
            if (port2 != null) msg += port2.Owner.Name + " P" + port2.PortNum;
            else msg += "(" + p2.X.ToString("F0") + "," + p2.Y.ToString("F0") + ")";
            txtSelected.Text = msg;
        }

        void FinishTubing()
        {
            _activeTubing = null;
            _tubeStartPort = null;
        }

        void CancelTubing()
        {
            if (_activeTubing != null)
            {
                foreach (var v in _activeTubing.Visuals) editorCanvas.Children.Remove(v);
                _tubingPaths.Remove(_activeTubing);
                _activeTubing = null;
                txtSelected.Text = "Cancelled";
            }
        }

        // --- RESIZE ---
        void Size_TextChanged(object s, TextChangedEventArgs e)
        {
            if (_updatingSize || _selectedItem == null) return;
            try
            {
                double newW = _selectedItem.W, newH = _selectedItem.H;
                if (double.TryParse(txtWidth.Text, out double w) && w > 5) newW = w;
                if (double.TryParse(txtHeight.Text, out double h) && h > 5) newH = h;
                if (newW != _selectedItem.W || newH != _selectedItem.H)
                {
                    _selectedItem.W = newW; _selectedItem.H = newH;
                    SafeRebuild(_selectedItem);
                    UpdateCoords(_selectedItem);
                }
            }
            catch { }
        }

        void SafeRebuild(DiagramItem item)
        {
            double x = item.X, y = item.Y;
            editorCanvas.Children.Remove(item.Visual);
            _items.Remove(item);
            // Remove any tubing anchors referencing this item's ports
            foreach (var tp in _tubingPaths)
                tp.Anchors.RemoveAll(a => a.Port.Owner == item);
            BuildVisual(item.Name, item.Type, x, y, item.W, item.H, item.FillColor, item.Label, item.PortCount, item.Rotation);
            _selectedItem = _items.Last();
        }

        void ResizeSelected(double f)
        {
            if (_selectedItem == null) return;
            _selectedItem.W *= f; _selectedItem.H *= f;
            SafeRebuild(_selectedItem);
            _updatingSize = true; txtWidth.Text = _selectedItem.W.ToString("F0"); txtHeight.Text = _selectedItem.H.ToString("F0"); _updatingSize = false;
            UpdateCoords(_selectedItem);
        }

        // --- ROTATION ---
        void RotateSelected(double delta)
        {
            if (_selectedItem == null) return;
            _selectedItem.Rotation = (_selectedItem.Rotation + delta) % 360;
            if (_selectedItem.Rotation < 0) _selectedItem.Rotation += 360;
            var fe = (FrameworkElement)_selectedItem.Visual;
            fe.RenderTransformOrigin = new Point(0.5, 0.5);
            fe.RenderTransform = new RotateTransform(_selectedItem.Rotation);
            _updatingSize = true; txtRotation.Text = _selectedItem.Rotation.ToString("F0"); _updatingSize = false;
            UpdateCoords(_selectedItem);
        }
        void Rotation_TextChanged(object s, TextChangedEventArgs e)
        {
            if (_updatingSize || _selectedItem == null) return;
            if (double.TryParse(txtRotation.Text, out double deg))
            {
                _selectedItem.Rotation = deg % 360;
                var fe = (FrameworkElement)_selectedItem.Visual;
                fe.RenderTransformOrigin = new Point(0.5, 0.5);
                fe.RenderTransform = new RotateTransform(_selectedItem.Rotation);
            }
        }
        void RotLeft90_Click(object s, RoutedEventArgs e) { RotateSelected(-90); }
        void RotLeft45_Click(object s, RoutedEventArgs e) { RotateSelected(-45); }
        void RotLeft15_Click(object s, RoutedEventArgs e) { RotateSelected(-15); }
        void RotRight15_Click(object s, RoutedEventArgs e) { RotateSelected(15); }
        void RotRight45_Click(object s, RoutedEventArgs e) { RotateSelected(45); }
        void RotRight90_Click(object s, RoutedEventArgs e) { RotateSelected(90); }

        void ShrinkBtn_Click(object s, RoutedEventArgs e) { ResizeSelected(0.75); }
        void GrowBtn_Click(object s, RoutedEventArgs e) { ResizeSelected(1.25); }
        void Grow50Btn_Click(object s, RoutedEventArgs e) { ResizeSelected(1.5); }
        void DoubleBtn_Click(object s, RoutedEventArgs e) { ResizeSelected(2.0); }

        // --- ACTIONS ---
        void DeleteSelected_Click(object s, RoutedEventArgs e)
        {
            if (_selectedItem != null)
            {
                editorCanvas.Children.Remove(_selectedItem.Visual);
                _items.Remove(_selectedItem);
                _selectedItem = null; txtSelected.Text = "Deleted"; txtCoords.Text = "";
            }
        }

        void UndoTubing_Click(object s, RoutedEventArgs e)
        {
            if (_actionHistory.Count == 0) { txtSelected.Text = "Nothing to undo"; return; }

            var last = _actionHistory.Last();
            _actionHistory.RemoveAt(_actionHistory.Count - 1);

            if (last is TubingPath tp)
            {
                foreach (var v in tp.Visuals) editorCanvas.Children.Remove(v);
                _tubingPaths.Remove(tp);
                txtSelected.Text = "Tubing removed";
            }
            else if (last is DiagramItem di)
            {
                editorCanvas.Children.Remove(di.Visual);
                _items.Remove(di);
                foreach (var tpath in _tubingPaths)
                    tpath.Anchors.RemoveAll(a => a.Port.Owner == di);
                if (_selectedItem == di) _selectedItem = null;
                txtSelected.Text = "Component removed: " + di.Name;
            }
        }

        void AddCustom_Click(object s, RoutedEventArgs e)
        {
            var d = new InputDialog("Custom", "Name:");
            if (d.ShowDialog() == true && !string.IsNullOrEmpty(d.Result))
                AddItem("Custom", 200, 250, d.Result);
        }

        void Save_Click(object s, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# ProteYOLUTE Schematic — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            foreach (var i in _items)
                sb.AppendLine("COMP|" + i.Name + "|" + i.Type + "|" + i.X.ToString("F1") + "|" + i.Y.ToString("F1") + "|" + i.W.ToString("F1") + "|" + i.H.ToString("F1") + "|" + i.Rotation.ToString("F0"));
            foreach (var t in _tubingPaths.Where(tp => tp.Visuals.Count > 0))
            {
                string pts = string.Join(";", t.Points.Select(p => p.X.ToString("F0") + "," + p.Y.ToString("F0")));
                sb.AppendLine("TUBE|" + t.Thickness.ToString("F1") + "|" + t.Color + "|" + pts);
            }
            if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);
            string path = System.IO.Path.Combine(SaveDir, SaveFile);
            File.WriteAllText(path, sb.ToString());
            MessageBox.Show("Saved to:\n" + path, "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void ExportXaml_Click(object s, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!-- ProteYOLUTE Schematic Export -->");
            foreach (var i in _items)
                sb.AppendLine("<!-- " + i.Name + ": Left=\"" + i.X.ToString("F1") + "\" Top=\"" + i.Y.ToString("F1") + "\" " + i.W.ToString("F0") + "x" + i.H.ToString("F0") + " -->");
            foreach (var t in _tubingPaths.Where(tp => tp.Visuals.Count > 0))
            {
                string pts = string.Join(" -> ", t.Points.Select(p => "(" + p.X.ToString("F0") + "," + p.Y.ToString("F0") + ")"));
                sb.AppendLine("<!-- Tubing " + t.Thickness + "px: " + pts + " -->");
            }
            Clipboard.SetText(sb.ToString());
            string path = System.IO.Path.Combine(SaveDir, "diagram_export.txt");
            File.WriteAllText(path, sb.ToString());
            MessageBox.Show("Exported to clipboard and:\n" + path, "Export");
        }

        void BuildDeploy_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", "/c \"C:\\Users\\Admin\\AppData\\Local\\Temp\\build.bat\"")
                { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                var proc = Process.Start(psi); proc.WaitForExit(30000);
                string src = @"C:\BDalSystemData\HyStar\LcPlugin\PrivateData\Bruker proteoElute\DLL_Extract\dnspy_styling\BrukerLC.Styling\bin\Release\BrukerLC.Styling.dll";
                string dst = @"C:\Program Files (x86)\Bruker Daltonik\HyStar\proteoElute\BrukerLC.Styling.dll";
                if (File.Exists(src)) { File.Copy(src, dst, true); MessageBox.Show("Built & deployed! Restart HyStar.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                else MessageBox.Show("Build may have failed.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }

    public class InputDialog : Window
    {
        private TextBox _tb; public string Result { get; private set; }
        public InputDialog(string title, string prompt)
        {
            Title = title; Width = 300; Height = 150; WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var sp = new StackPanel { Margin = new Thickness(10) };
            sp.Children.Add(new TextBlock { Text = prompt });
            _tb = new TextBox { Margin = new Thickness(0, 8, 0, 8) }; sp.Children.Add(_tb);
            var btn = new Button { Content = "OK", Width = 80, HorizontalAlignment = HorizontalAlignment.Right };
            btn.Click += (s, e) => { Result = _tb.Text; DialogResult = true; }; sp.Children.Add(btn);
            Content = sp;
        }
    }
}
