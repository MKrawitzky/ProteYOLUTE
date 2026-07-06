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
        private UIElement _dragElement;
        private Point _dragOffset;
        private bool _isDragging;
        private UIElement _selected;
        private List<DiagramItem> _items = new List<DiagramItem>();
        private bool _isDrawingTubing;
        private Line _currentTubingLine;
        private Point _tubingStart;

        private static readonly string XamlPath = @"C:\BDalSystemData\HyStar\LcPlugin\PrivateData\Bruker proteoElute\DLL_Extract\dnspy_styling\BrukerLC.Styling\styles\customcontrols\diagramcontrol.xaml";
        private static readonly string BuildBat = @"C:\Users\Admin\AppData\Local\Temp\build.bat";
        private static readonly string DllDest = @"C:\Program Files (x86)\Bruker Daltonik\HyStar\proteoElute\BrukerLC.Styling.dll";
        private static readonly string DllSrc = @"C:\BDalSystemData\HyStar\LcPlugin\PrivateData\Bruker proteoElute\DLL_Extract\dnspy_styling\BrukerLC.Styling\bin\Release\BrukerLC.Styling.dll";

        public MainWindow()
        {
            InitializeComponent();
            LoadPalette();
            LoadCurrentDiagram();
        }

        class DiagramItem
        {
            public string Name { get; set; }
            public string Type { get; set; } // pump, valve, sensor, custom, tubing, etc.
            public double X { get; set; }
            public double Y { get; set; }
            public double W { get; set; }
            public double H { get; set; }
            public UIElement Visual { get; set; }
            public Color FillColor { get; set; }
            public string Label { get; set; }
            public string XamlTag { get; set; } // original XAML element name
        }

        void LoadPalette()
        {
            var components = new[] {
                ("Pump", "#FF3070B0", "Pump"),
                ("Valve", "#FFA0A0A0", "Valve"),
                ("Flow Sensor", "#FF2A6A50", "Sensor"),
                ("Pressure Sensor", "#FF6A6A6A", "PSensor"),
                ("Mixing Tee", "#FF808080", "MixTee"),
                ("Solvent Bottle", "#FF4488CC", "Solvent"),
                ("Waste Port", "#FFCC4444", "Waste"),
                ("Column Oven", "#FFDD8800", "Oven"),
                ("Trap Column", "#FF886644", "Trap"),
                ("Loop", "#FF668866", "Loop"),
                ("Injection Port", "#FF444444", "Inject"),
                ("Peltier Stack", "#FF8090A0", "Peltier"),
                ("Transfer Line", "#FF999999", "Transfer"),
                ("Custom Box", "#FF6A00B0", "Custom"),
            };

            foreach (var (name, color, type) in components)
            {
                var btn = new Button
                {
                    Content = name,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                    Foreground = Brushes.White,
                    Margin = new Thickness(2),
                    Padding = new Thickness(6, 3, 6, 3),
                    FontSize = 10,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Tag = type
                };
                btn.Click += PaletteItem_Click;
                palette.Children.Add(btn);
            }
        }

        void PaletteItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            string type = (string)btn.Tag;
            string name = btn.Content.ToString();
            AddComponent(name, type, 200, 250, null);
        }

        void AddComponent(string name, string type, double x, double y, Color? color)
        {
            double w = 52, h = 40;
            Color fill = color ?? Colors.Gray;
            string label = name;

            switch (type)
            {
                case "Pump": w = 52; h = 102; fill = Colors.SteelBlue; break;
                case "Valve": w = 36; h = 36; fill = Colors.DarkGray; break;
                case "Sensor": w = 54; h = 28; fill = Colors.Teal; break;
                case "PSensor": w = 22; h = 20; fill = Colors.DimGray; label = "P"; break;
                case "MixTee": w = 26; h = 26; fill = Colors.Gray; label = "T"; break;
                case "Solvent": w = 30; h = 50; fill = Colors.CornflowerBlue; break;
                case "Waste": w = 20; h = 20; fill = Colors.IndianRed; label = "W"; break;
                case "Oven": w = 40; h = 30; fill = Colors.DarkOrange; break;
                case "Trap": w = 20; h = 50; fill = Colors.SaddleBrown; break;
                case "Loop": w = 20; h = 30; fill = Colors.DarkSeaGreen; break;
                case "Inject": w = 30; h = 40; fill = Colors.DarkSlateGray; break;
                case "Peltier": w = 140; h = 80; fill = Colors.LightSteelBlue; break;
                case "Transfer": w = 60; h = 10; fill = Colors.Silver; break;
                case "Custom": w = 60; h = 40; fill = Colors.MediumPurple; break;
            }

            var item = new DiagramItem
            {
                Name = name + "_" + _items.Count,
                Type = type,
                X = x, Y = y, W = w, H = h,
                FillColor = fill,
                Label = label
            };

            var visual = CreateVisual(item);
            item.Visual = visual;
            Canvas.SetLeft(visual, x);
            Canvas.SetTop(visual, y);
            editorCanvas.Children.Add(visual);
            _items.Add(item);
        }

        UIElement CreateVisual(DiagramItem item)
        {
            var border = new Border
            {
                Width = item.W,
                Height = item.H,
                Background = new SolidColorBrush(item.FillColor),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                CornerRadius = item.Type == "Valve" || item.Type == "MixTee"
                    ? new CornerRadius(item.W / 2) : new CornerRadius(3),
                Opacity = 0.85,
                Tag = item,
                Cursor = Cursors.SizeAll,
                ToolTip = item.Name
            };

            var text = new TextBlock
            {
                Text = item.Label.Length > 8 ? item.Label.Substring(0, 8) : item.Label,
                Foreground = Brushes.White,
                FontSize = item.W < 30 ? 7 : 9,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };
            border.Child = text;
            return border;
        }

        void LoadCurrentDiagram()
        {
            // Load existing components from the current positions
            // Pumps
            AddExisting("Pump A", "Pump", -0.5, 333, Colors.SteelBlue);
            AddExisting("Pump B", "Pump", 370.9, 333, Colors.IndianRed);

            // Valves
            AddExisting("Valve A (Trap)", "Valve", 275.6, 80.1, Colors.DarkGray);
            AddExisting("Valve B (Inj)", "Valve", 275.6, 125.1, Colors.DarkGray);
            AddExisting("Valve C (PmpB)", "Valve", 273, 373, Colors.DarkGray);
            AddExisting("Valve D (PmpA)", "Valve", 99, 373, Colors.DarkGray);

            // Injection
            AddExisting("Injection", "Inject", 285.7, 30, Colors.DarkSlateGray);

            // Output
            AddExisting("Output/Sep", "Transfer", 360, 198.1, Colors.Silver);

            // Oven
            AddExisting("Oven", "Oven", 377, 220, Colors.DarkOrange);

            // Flow Sensors
            AddExisting("Flow Sensor A", "Sensor", 185, 430, Colors.Teal);
            AddExisting("Flow Sensor B", "Sensor", 185, 400, Colors.Teal);

            // Waste
            AddExisting("Waste A", "Waste", 238.4, 79, Colors.IndianRed);
            AddExisting("Waste B", "Waste", 238.4, 124, Colors.IndianRed);
            AddExisting("Waste C", "Waste", 251.9, 348, Colors.IndianRed);
            AddExisting("Waste D", "Waste", 78, 348, Colors.IndianRed);

            // Solvents
            AddExisting("Solvent A", "Solvent", 78, 418, Colors.CornflowerBlue);
            AddExisting("Solvent B", "Solvent", 251.9, 418, Colors.Brown);

            // Loop & Trap
            AddExisting("Loop", "Loop", 277.9, 91, Colors.DarkSeaGreen);
            AddExisting("Trap Column", "Trap", 293.1, 177.5, Colors.SaddleBrown);

            // Pressure Sensors
            AddExisting("PSensor A", "PSensor", 20, 380, Colors.DimGray);
            AddExisting("PSensor B", "PSensor", 411, 380, Colors.DimGray);

            // Mixing Tee
            AddExisting("Mixing Tee", "MixTee", 435, 390, Colors.Gray);

            // Peltier
            AddExisting("Peltier Stack", "Peltier", 5, 30, Colors.LightSteelBlue);
        }

        void AddExisting(string name, string type, double x, double y, Color fill)
        {
            double w = 52, h = 40;
            string label = name;
            switch (type)
            {
                case "Pump": w = 52; h = 102; break;
                case "Valve": w = 36; h = 36; break;
                case "Sensor": w = 54; h = 28; break;
                case "PSensor": w = 22; h = 20; label = "P"; break;
                case "MixTee": w = 26; h = 26; label = "T"; break;
                case "Solvent": w = 30; h = 50; break;
                case "Waste": w = 20; h = 20; label = "W"; break;
                case "Oven": w = 40; h = 30; break;
                case "Trap": w = 20; h = 50; break;
                case "Loop": w = 20; h = 30; break;
                case "Inject": w = 30; h = 40; break;
                case "Peltier": w = 140; h = 80; break;
                case "Transfer": w = 60; h = 10; break;
                case "Custom": w = 60; h = 40; break;
            }

            var item = new DiagramItem
            {
                Name = name, Type = type,
                X = x, Y = y, W = w, H = h,
                FillColor = fill, Label = label
            };
            var visual = CreateVisual(item);
            item.Visual = visual;
            Canvas.SetLeft(visual, x);
            Canvas.SetTop(visual, y);
            editorCanvas.Children.Add(visual);
            _items.Add(item);
        }

        // --- DRAG AND DROP ---

        void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as FrameworkElement;
            while (hit != null && !(hit is Border && hit.Tag is DiagramItem))
                hit = VisualTreeHelper.GetParent(hit) as FrameworkElement;

            if (hit != null && hit.Tag is DiagramItem item)
            {
                if (_isDrawingTubing)
                {
                    _tubingStart = new Point(item.X + item.W / 2, item.Y + item.H / 2);
                    _currentTubingLine = new Line
                    {
                        X1 = _tubingStart.X, Y1 = _tubingStart.Y,
                        X2 = _tubingStart.X, Y2 = _tubingStart.Y,
                        Stroke = new SolidColorBrush(Colors.Gray),
                        StrokeThickness = 2
                    };
                    editorCanvas.Children.Add(_currentTubingLine);
                    return;
                }

                _dragElement = hit;
                _dragOffset = e.GetPosition(hit);
                _isDragging = true;
                _selected = hit;
                hit.CaptureMouse();

                // Update properties
                txtSelected.Text = $"{item.Name}\nType: {item.Type}\nSize: {item.W}x{item.H}";
                UpdateCoords(item);
            }
        }

        void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _dragElement != null)
            {
                var pos = e.GetPosition(editorCanvas);
                double newX = pos.X - _dragOffset.X;
                double newY = pos.Y - _dragOffset.Y;

                // Snap to grid (5px)
                newX = Math.Round(newX / 5) * 5;
                newY = Math.Round(newY / 5) * 5;

                Canvas.SetLeft(_dragElement, newX);
                Canvas.SetTop(_dragElement, newY);

                var item = (DiagramItem)((Border)_dragElement).Tag;
                item.X = newX;
                item.Y = newY;
                UpdateCoords(item);
            }

            if (_isDrawingTubing && _currentTubingLine != null)
            {
                var pos = e.GetPosition(editorCanvas);
                _currentTubingLine.X2 = pos.X;
                _currentTubingLine.Y2 = pos.Y;
            }
        }

        void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && _dragElement != null)
            {
                _dragElement.ReleaseMouseCapture();
                _isDragging = false;
                _dragElement = null;
            }

            if (_isDrawingTubing && _currentTubingLine != null)
            {
                _isDrawingTubing = false;
                _currentTubingLine = null;
            }
        }

        void UpdateCoords(DiagramItem item)
        {
            txtCoords.Text = $"Canvas.Left=\"{item.X:F1}\"\nCanvas.Top=\"{item.Y:F1}\"";
        }

        // --- ACTIONS ---

        void AddCustom_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new InputDialog("Custom Component", "Enter name:");
            if (dlg.ShowDialog() == true)
            {
                AddComponent(dlg.Result, "Custom", 200, 250, Colors.MediumPurple);
            }
        }

        void AddTubing_Click(object sender, RoutedEventArgs e)
        {
            _isDrawingTubing = true;
            MessageBox.Show("Click on a component to start the tubing, then click on another to end it.", "Draw Tubing");
        }

        void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_selected != null)
            {
                var item = (DiagramItem)((Border)_selected).Tag;
                editorCanvas.Children.Remove(_selected);
                _items.Remove(item);
                _selected = null;
                txtSelected.Text = "Deleted";
                txtCoords.Text = "";
            }
        }

        void ExportXaml_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!-- ProteYOLUTE Diagram Editor Export -->");
            sb.AppendLine("<!-- Component Positions -->");
            foreach (var item in _items)
            {
                sb.AppendLine($"<!-- {item.Name}: Canvas.Left=\"{item.X:F1}\" Canvas.Top=\"{item.Y:F1}\" ({item.Type} {item.W}x{item.H}) -->");
            }

            // Also export tubing lines
            foreach (UIElement child in editorCanvas.Children)
            {
                if (child is Line line)
                {
                    sb.AppendLine($"<!-- Tubing: ({line.X1:F0},{line.Y1:F0}) -> ({line.X2:F0},{line.Y2:F0}) -->");
                }
            }

            string exportPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(XamlPath),
                "diagram_export.txt");
            File.WriteAllText(exportPath, sb.ToString());

            Clipboard.SetText(sb.ToString());
            MessageBox.Show($"Exported to:\n{exportPath}\n\nAlso copied to clipboard.", "Export Complete");
        }

        void BuildDeploy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", "/c \"" + BuildBat + "\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var proc = Process.Start(psi);
                proc.WaitForExit(30000);

                if (File.Exists(DllSrc))
                {
                    File.Copy(DllSrc, DllDest, true);
                    MessageBox.Show("Build succeeded and deployed!\nRestart HyStar to see changes.", "Build & Deploy",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Build may have failed. Check output.", "Build & Deploy",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Build & Deploy", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Simple input dialog
    public class InputDialog : Window
    {
        private TextBox _textBox;
        public string Result { get; private set; }

        public InputDialog(string title, string prompt)
        {
            Title = title;
            Width = 300; Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var sp = new StackPanel { Margin = new Thickness(10) };
            sp.Children.Add(new TextBlock { Text = prompt });
            _textBox = new TextBox { Margin = new Thickness(0, 8, 0, 8) };
            sp.Children.Add(_textBox);
            var btn = new Button { Content = "OK", Width = 80, HorizontalAlignment = HorizontalAlignment.Right };
            btn.Click += (s, e) => { Result = _textBox.Text; DialogResult = true; };
            sp.Children.Add(btn);
            Content = sp;
        }
    }
}
