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
        private static readonly double[] Port6Angles = { -90, -150, -210, -270, -330, -30 };
        private static readonly string[] Port6Clocks = { "12", "10", "8", "6", "4", "2" };
        private static readonly string SaveDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "cluade");
        private static readonly string SaveFile = "diagram_save.txt";

        public MainWindow() { InitializeComponent(); LoadPalette(); TryLoadSaved(); }

        class DiagramItem
        {
            public string Name, Type, Label;
            public double X, Y, W, H;
            public Color FillColor;
            public UIElement Visual;
            public List<PortDot> Ports = new List<PortDot>();
            public int PortCount;
        }
        class PortDot { public int PortNum; public Ellipse Dot; public DiagramItem Owner; public double AngleDeg; }
        class TubingPath
        {
            public List<Point> Points = new List<Point>();
            public double Thickness; public Color Color;
            public List<UIElement> Visuals = new List<UIElement>();
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
                new[] { "Trap 10cm", "#FF886644" },
                new[] { "Trap 25cm", "#FFAA8866" },
                new[] { "Column 15cm", "#FF445566" },
                new[] { "Column 25cm", "#FF556677" },
                new[] { "Column 50cm", "#FF667788" },
                new[] { "Injection Port", "#FF444444" },
                new[] { "Peltier Stack", "#FF8090A0" },
                new[] { "Output", "#FF999999" },
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
                case "Loop 1uL": w = 20; h = 20; fill = Color.FromRgb(68,102,68); break;
                case "Loop 5uL": w = 30; h = 30; fill = Color.FromRgb(85,136,85); break;
                case "Loop 20uL": w = 45; h = 45; fill = Color.FromRgb(102,136,102); break;
                case "Loop 50uL": w = 60; h = 60; fill = Color.FromRgb(119,153,119); break;
                case "Trap 5cm": w = 15; h = 35; fill = Color.FromRgb(102,68,34); break;
                case "Trap 10cm": w = 18; h = 55; fill = Color.FromRgb(136,102,68); break;
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

        void BuildVisual(string name, string type, double x, double y, double w, double h, Color fill, string label, int ports)
        {
            var item = new DiagramItem { Name = name, Type = type, X = x, Y = y, W = w, H = h, FillColor = fill, Label = label, PortCount = ports };
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
                // Loop as a circle with open ends
                var ellipse = new Ellipse
                {
                    Width = w, Height = h,
                    Stroke = new SolidColorBrush(fill), StrokeThickness = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(40, fill.R, fill.G, fill.B))
                };
                group.Children.Add(ellipse);
                var lbl = new TextBlock
                {
                    Text = label, FontSize = w < 30 ? 6 : 8, FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(fill), IsHitTestVisible = false,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(lbl, w / 2 - label.Length * 2.5); Canvas.SetTop(lbl, h / 2 - 6);
                group.Children.Add(lbl);
            }
            else if (type.StartsWith("Trap") || type.StartsWith("Column"))
            {
                // Column/Trap as a rounded rectangle with gradient
                var border = new Border
                {
                    Width = w, Height = h,
                    CornerRadius = new CornerRadius(w / 2),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(200, fill.R, fill.G, fill.B)),
                    BorderThickness = new Thickness(2), Opacity = 0.9
                };
                border.Background = new LinearGradientBrush(
                    Color.FromArgb(180, (byte)Math.Min(fill.R + 40, 255), (byte)Math.Min(fill.G + 40, 255), (byte)Math.Min(fill.B + 40, 255)),
                    Color.FromArgb(220, fill.R, fill.G, fill.B), 0);
                border.Child = new TextBlock
                {
                    Text = label, FontSize = 7, FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center
                };
                group.Children.Add(border);
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
            group.Tag = item; group.Cursor = Cursors.SizeAll;
            item.Visual = group;
            Canvas.SetLeft(group, x); Canvas.SetTop(group, y);
            editorCanvas.Children.Add(group);
            _items.Add(item);
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
                            BuildVisual(name, type, x, y, w, h, fill, name, portCount);
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
        void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(editorCanvas);
            var hitPort = FindPort(e);
            if (hitPort != null && IsDrawMode) { AddTubingPoint(GetPortCenter(hitPort)); return; }
            if (IsDrawMode) { AddTubingPoint(pos); return; }

            var item = FindItem(e);
            if (item != null)
            {
                _dragElement = (FrameworkElement)item.Visual; _dragOffset = e.GetPosition(_dragElement);
                _isDragging = true; _selectedItem = item; _dragElement.CaptureMouse();
                txtSelected.Text = item.Name + " (" + item.Type + ")"; UpdateCoords(item);
                _updatingSize = true; txtWidth.Text = item.W.ToString("F0"); txtHeight.Text = item.H.ToString("F0"); _updatingSize = false;
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
        }

        void Canvas_MouseLeftButtonUp(object s, MouseButtonEventArgs e)
        {
            if (_isDragging && _dragElement != null) { _dragElement.ReleaseMouseCapture(); _isDragging = false; _dragElement = null; }
        }

        void Canvas_RightClick(object s, MouseButtonEventArgs e)
        {
            var port = FindPort(e);
            if (port != null)
                txtSelected.Text = port.Owner.Name + " Port " + port.PortNum +
                    (port.Owner.PortCount == 6 ? " (" + Port6Clocks[port.PortNum - 1] + " o'clk)" : "");
        }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _activeTubing != null) FinishTubing();
            if (e.Key == Key.Escape && _activeTubing != null) CancelTubing();
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
            double cx = pd.Owner.X + pd.Owner.W / 2;
            double cy = pd.Owner.Y + pd.Owner.H / 2;
            double a = pd.AngleDeg * Math.PI / 180;
            double r = pd.Owner.W / 2 + 5;
            return new Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a));
        }

        void UpdateCoords(DiagramItem item)
        {
            txtCoords.Text = "Left=\"" + item.X.ToString("F0") + "\" Top=\"" + item.Y.ToString("F0") + "\"\n" + item.W.ToString("F0") + " x " + item.H.ToString("F0");
        }

        // --- TUBING ---
        void AddTubingPoint(Point pt)
        {
            if (_activeTubing == null)
            {
                _activeTubing = new TubingPath { Thickness = TubeWidth, Color = TubeColor };
                _tubingPaths.Add(_activeTubing);
                txtSelected.Text = "Click waypoints, ENTER to finish";
            }
            _activeTubing.Points.Add(pt);
            var dot = new Ellipse { Width = 6, Height = 6, Fill = new SolidColorBrush(_activeTubing.Color), IsHitTestVisible = false };
            Canvas.SetLeft(dot, pt.X - 3); Canvas.SetTop(dot, pt.Y - 3);
            editorCanvas.Children.Add(dot);
            _activeTubing.Visuals.Add(dot);

            if (_activeTubing.Points.Count > 1)
            {
                var prev = _activeTubing.Points[_activeTubing.Points.Count - 2];
                var line = new Line
                {
                    X1 = prev.X, Y1 = prev.Y, X2 = pt.X, Y2 = pt.Y,
                    Stroke = new SolidColorBrush(_activeTubing.Color), StrokeThickness = _activeTubing.Thickness, IsHitTestVisible = false
                };
                editorCanvas.Children.Add(line);
                _activeTubing.Visuals.Add(line);
            }
        }

        void FinishTubing()
        {
            if (_activeTubing != null && _activeTubing.Points.Count >= 2)
                txtSelected.Text = "Tubing done (" + _activeTubing.Points.Count + " pts)";
            _activeTubing = null;
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
            double newW = _selectedItem.W, newH = _selectedItem.H;
            if (double.TryParse(txtWidth.Text, out double w) && w > 5) newW = w;
            if (double.TryParse(txtHeight.Text, out double h) && h > 5) newH = h;
            if (newW != _selectedItem.W || newH != _selectedItem.H)
            {
                _selectedItem.W = newW; _selectedItem.H = newH;
                RebuildVisual(_selectedItem);
                _selectedItem = _items.Last();
                UpdateCoords(_selectedItem);
            }
        }

        void ResizeSelected(double f)
        {
            if (_selectedItem == null) return;
            _selectedItem.W *= f; _selectedItem.H *= f;
            RebuildVisual(_selectedItem);
            _selectedItem = _items.Last();
            _updatingSize = true; txtWidth.Text = _selectedItem.W.ToString("F0"); txtHeight.Text = _selectedItem.H.ToString("F0"); _updatingSize = false;
            UpdateCoords(_selectedItem);
        }

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
            if (_tubingPaths.Count > 0)
            {
                var last = _tubingPaths.Last();
                foreach (var v in last.Visuals) editorCanvas.Children.Remove(v);
                _tubingPaths.Remove(last);
                txtSelected.Text = "Tubing removed";
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
                sb.AppendLine("COMP|" + i.Name + "|" + i.Type + "|" + i.X.ToString("F1") + "|" + i.Y.ToString("F1") + "|" + i.W.ToString("F1") + "|" + i.H.ToString("F1"));
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
