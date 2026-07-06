using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DiagramEditor
{
    public partial class ValveEditor : Window
    {
        // Port positions for 6-port valve (counter-clockwise from 12 o'clock)
        // P1=12, P2=10, P3=8, P4=6, P5=4, P6=2
        private static readonly double[] PortAngles = { -90, -150, -210, -270, -330, -390 }; // degrees from 3 o'clock

        private Dictionary<string, string[]> _portLabels = new Dictionary<string, string[]>
        {
            { "A", new[] { "", "", "", "", "", "" } },
            { "B", new[] { "", "", "", "", "", "" } },
            { "I", new[] { "", "", "", "", "", "" } },
            { "T", new[] { "", "", "", "", "", "" } }
        };

        public ValveEditor()
        {
            InitializeComponent();
            DrawValve(canvasA, "A", Colors.SteelBlue);
            DrawValve(canvasB, "B", Colors.IndianRed);
            DrawValve(canvasI, "I", Colors.DarkSeaGreen);
            DrawValve(canvasT, "T", Colors.Goldenrod);
        }

        void DrawValve(Canvas canvas, string valveId, Color color)
        {
            double cx = 125, cy = 115, radius = 55, portRadius = 80, dotRadius = 12;

            // Valve body circle
            var body = new Ellipse
            {
                Width = radius * 2, Height = radius * 2,
                Fill = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B)),
                Stroke = new SolidColorBrush(Colors.White), StrokeThickness = 2
            };
            Canvas.SetLeft(body, cx - radius);
            Canvas.SetTop(body, cy - radius);
            canvas.Children.Add(body);

            // Valve label
            var label = new TextBlock
            {
                Text = valveId, FontSize = 20, FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(label, cx - 8);
            Canvas.SetTop(label, cy - 14);
            canvas.Children.Add(label);

            // Rotor seal grooves (3 grooves connecting adjacent ports: 1-6, 2-3, 4-5)
            DrawRotorGroove(canvas, cx, cy, radius - 15, PortAngles[0], PortAngles[5], Colors.LightGray); // P1-P6
            DrawRotorGroove(canvas, cx, cy, radius - 15, PortAngles[1], PortAngles[2], Colors.LightGray); // P2-P3
            DrawRotorGroove(canvas, cx, cy, radius - 15, PortAngles[3], PortAngles[4], Colors.LightGray); // P4-P5

            // 6 ports
            for (int i = 0; i < 6; i++)
            {
                double angle = PortAngles[i] * Math.PI / 180.0;
                double px = cx + portRadius * Math.Cos(angle);
                double py = cy + portRadius * Math.Sin(angle);

                // Port dot
                var dot = new Ellipse
                {
                    Width = dotRadius * 2, Height = dotRadius * 2,
                    Fill = new SolidColorBrush(Colors.White),
                    Stroke = new SolidColorBrush(color), StrokeThickness = 2,
                    Cursor = Cursors.Hand,
                    Tag = new PortInfo { ValveId = valveId, PortNum = i + 1 },
                    ToolTip = $"Port {i + 1} ({GetClockPosition(i + 1)})"
                };
                dot.MouseLeftButtonDown += Port_Click;
                Canvas.SetLeft(dot, px - dotRadius);
                Canvas.SetTop(dot, py - dotRadius);
                canvas.Children.Add(dot);

                // Port number
                var num = new TextBlock
                {
                    Text = (i + 1).ToString(), FontSize = 11, FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(color),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(num, px - 4);
                Canvas.SetTop(num, py - 8);
                canvas.Children.Add(num);

                // Clock position label
                double labelDist = portRadius + 22;
                double lx = cx + labelDist * Math.Cos(angle);
                double ly = cy + labelDist * Math.Sin(angle);
                var clockLabel = new TextBlock
                {
                    Text = GetClockPosition(i + 1), FontSize = 8,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(clockLabel, lx - 10);
                Canvas.SetTop(clockLabel, ly - 6);
                canvas.Children.Add(clockLabel);

                // Connection label (editable)
                double connDist = portRadius + 38;
                double connX = cx + connDist * Math.Cos(angle);
                double connY = cy + connDist * Math.Sin(angle);
                var connLabel = new TextBox
                {
                    Width = 80, Height = 18, FontSize = 8,
                    Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                    Foreground = Brushes.LightGreen,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                    BorderThickness = new Thickness(1),
                    Text = _portLabels[valveId][i],
                    Tag = new PortInfo { ValveId = valveId, PortNum = i + 1 },
                    ToolTip = "Type what connects here"
                };
                connLabel.TextChanged += ConnLabel_TextChanged;
                Canvas.SetLeft(connLabel, connX - 40);
                Canvas.SetTop(connLabel, connY - 9);
                canvas.Children.Add(connLabel);
            }

            // Line from port to valve body
            for (int i = 0; i < 6; i++)
            {
                double angle = PortAngles[i] * Math.PI / 180.0;
                double px = cx + portRadius * Math.Cos(angle);
                double py = cy + portRadius * Math.Sin(angle);
                double bx = cx + radius * Math.Cos(angle);
                double by = cy + radius * Math.Sin(angle);

                var line = new Line
                {
                    X1 = bx, Y1 = by, X2 = px, Y2 = py,
                    Stroke = new SolidColorBrush(Colors.Gray), StrokeThickness = 1.5
                };
                canvas.Children.Add(line);
            }
        }

        void DrawRotorGroove(Canvas canvas, double cx, double cy, double r, double angle1Deg, double angle2Deg, Color color)
        {
            double a1 = angle1Deg * Math.PI / 180.0;
            double a2 = angle2Deg * Math.PI / 180.0;
            double x1 = cx + r * Math.Cos(a1);
            double y1 = cy + r * Math.Sin(a1);
            double x2 = cx + r * Math.Cos(a2);
            double y2 = cy + r * Math.Sin(a2);

            // Draw arc as a simple curved line
            var path = new Path
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 3,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Opacity = 0.6
            };

            var figure = new PathFigure { StartPoint = new Point(x1, y1) };
            var midAngle = (a1 + a2) / 2;
            double midR = r * 0.85;
            var midPoint = new Point(cx + midR * Math.Cos(midAngle), cy + midR * Math.Sin(midAngle));
            figure.Segments.Add(new QuadraticBezierSegment(midPoint, new Point(x2, y2), true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            path.Data = geometry;
            canvas.Children.Add(path);
        }

        string GetClockPosition(int port)
        {
            switch (port)
            {
                case 1: return "12 o'clk";
                case 2: return "10 o'clk";
                case 3: return "8 o'clk";
                case 4: return "6 o'clk";
                case 5: return "4 o'clk";
                case 6: return "2 o'clk";
                default: return "";
            }
        }

        void Port_Click(object sender, MouseButtonEventArgs e)
        {
            var dot = (Ellipse)sender;
            var info = (PortInfo)dot.Tag;
            txtStatus.Text = $"Selected: Valve {info.ValveId} Port {info.PortNum} ({GetClockPosition(info.PortNum)})";
        }

        void ConnLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            var info = (PortInfo)tb.Tag;
            _portLabels[info.ValveId][info.PortNum - 1] = tb.Text;
            UpdateStatus();
        }

        void UpdateStatus()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var kvp in _portLabels)
            {
                sb.AppendLine($"Valve {kvp.Key}:");
                for (int i = 0; i < 6; i++)
                {
                    string conn = string.IsNullOrEmpty(kvp.Value[i]) ? "(empty)" : kvp.Value[i];
                    sb.AppendLine($"  P{i + 1} ({GetClockPosition(i + 1)}): {conn}");
                }
            }
            txtStatus.Text = sb.ToString();
        }

        class PortInfo
        {
            public string ValveId { get; set; }
            public int PortNum { get; set; }
        }
    }
}
