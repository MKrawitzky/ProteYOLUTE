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
        private Ellipse _previewDot;
        private bool _updatingSize;
        private static readonly double[] PortAngles = { -90, -150, -210, -270, -330, -30 };
        private static readonly string[] PortClocks = { "12", "10", "8", "6", "4", "2" };
        private static readonly string SaveDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "cluade");

        public MainWindow() { InitializeComponent(); LoadPalette(); LoadCurrentDiagram(); }

        class DiagramItem {
            public string Name; public string Type; public double X, Y, W, H;
            public Color FillColor; public string Label; public UIElement Visual;
            public List<PortDot> Ports = new List<PortDot>();
        }
        class PortDot { public int PortNum; public Ellipse Dot; public DiagramItem Owner; public double AngleDeg; }
        class TubingPath {
            public List<Point> Points = new List<Point>();
            public double Thickness; public Color Color;
            public List<UIElement> Visuals = new List<UIElement>();
        }

        bool IsDrawMode => rbModeTube.IsChecked == true;
        double TubeWidth => rbThick.IsChecked == true ? 4 : rbMedium.IsChecked == true ? 2.5 : 1.5;
        Color TubeColor {
            get {
                if (rbColorBlue.IsChecked == true) return Color.FromRgb(68, 136, 204);
                if (rbColorRed.IsChecked == true) return Color.FromRgb(204, 68, 68);
                if (rbColorGreen.IsChecked == true) return Color.FromRgb(68, 170, 68);
                if (rbColorPurple.IsChecked == true) return Color.FromRgb(136, 68, 204);
                return Colors.Gray;
            }
        }

        void LoadPalette()
        {
            string[][] comps = new[] {
                new[]{"Pump","#FF3070B0"}, new[]{"6-Port Valve","#FFA0A0A0"}, new[]{"Flow Sensor","#FF2A6A50"},
                new[]{"Pressure Sensor","#FF6A6A6A"}, new[]{"Mixing Tee","#FF808080"}, new[]{"Solvent Bottle","#FF4488CC"},
                new[]{"Waste Port","#FFCC4444"}, new[]{"Trap Column","#FF886644"}, new[]{"Sample Loop","#FF668866"},
                new[]{"Injection Port","#FF444444"}, new[]{"Peltier Stack","#FF8090A0"}, new[]{"Output","#FF999999"},
                new[]{"Custom","#FF6A00B0"}
            };
            foreach (var c in comps) {
                var btn = new Button { Content = c[0], Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(c[1])),
                    Foreground = Brushes.White, Margin = new Thickness(2), Padding = new Thickness(4,2,4,2), FontSize = 9,
                    HorizontalContentAlignment = HorizontalAlignment.Left, Tag = c[0] };
                btn.Click += (s, e) => AddItem(c[0], 250, 250);
                palette.Children.Add(btn);
            }
        }

        void AddItem(string type, double x, double y)
        {
            double w=50,h=40; Color fill=Colors.Gray; string label=type; bool isValve=false;
            switch(type) {
                case "Pump": w=52;h=102;fill=Colors.SteelBlue;break;
                case "6-Port Valve": w=60;h=60;fill=Colors.DarkGray;isValve=true;break;
                case "Flow Sensor": w=54;h=28;fill=Colors.Teal;break;
                case "Pressure Sensor": w=22;h=20;fill=Colors.DimGray;label="P";break;
                case "Mixing Tee": w=30;h=30;fill=Colors.Gray;label="MT";break;
                case "Solvent Bottle": w=30;h=50;fill=Colors.CornflowerBlue;break;
                case "Waste Port": w=20;h=20;fill=Colors.IndianRed;label="W";break;
                case "Trap Column": w=20;h=60;fill=Colors.SaddleBrown;label="Trap";break;
                case "Sample Loop": w=40;h=40;fill=Colors.DarkSeaGreen;label="Loop";break;
                case "Injection Port": w=30;h=40;fill=Colors.DarkSlateGray;break;
                case "Peltier Stack": w=120;h=60;fill=Colors.LightSteelBlue;break;
                case "Output": w=50;h=15;fill=Colors.Silver;break;
                case "Custom": w=60;h=40;fill=Colors.MediumPurple;break;
            }
            var item = new DiagramItem { Name=type+"_"+_items.Count, Type=type, X=x, Y=y, W=w, H=h, FillColor=fill, Label=label };
            var group = new Canvas { Width=w, Height=h };

            // Body
            if (isValve) {
                var body = new Ellipse { Width=w, Height=h, Fill=new SolidColorBrush(Color.FromArgb(200,fill.R,fill.G,fill.B)),
                    Stroke=Brushes.White, StrokeThickness=2 };
                group.Children.Add(body);
                // Rotor grooves
                DrawGroove(group, w/2, h/2, w/2-8, PortAngles[0], PortAngles[5]); // P1-P6
                DrawGroove(group, w/2, h/2, w/2-8, PortAngles[1], PortAngles[2]); // P2-P3
                DrawGroove(group, w/2, h/2, w/2-8, PortAngles[3], PortAngles[4]); // P4-P5
                // Ports
                for (int i=0;i<6;i++) {
                    double a=PortAngles[i]*Math.PI/180; double r=w/2+4;
                    double px=w/2+r*Math.Cos(a)-5; double py=h/2+r*Math.Sin(a)-5;
                    var dot = new Ellipse { Width=10, Height=10, Fill=Brushes.Yellow, Stroke=Brushes.Black,
                        StrokeThickness=1, Cursor=Cursors.Cross,
                        ToolTip="P"+(i+1)+" ("+PortClocks[i]+" o'clk)" };
                    Canvas.SetLeft(dot,px); Canvas.SetTop(dot,py);
                    group.Children.Add(dot);
                    var pd = new PortDot{PortNum=i+1,Dot=dot,Owner=item,AngleDeg=PortAngles[i]};
                    item.Ports.Add(pd);
                    dot.Tag = pd;
                    // Port number
                    var num = new TextBlock{Text=(i+1).ToString(),FontSize=7,Foreground=Brushes.Black,IsHitTestVisible=false};
                    Canvas.SetLeft(num,px+2); Canvas.SetTop(num,py);
                    group.Children.Add(num);
                }
                // Center label
                var lbl = new TextBlock{Text=label.Length>6?label.Substring(0,6):label,FontSize=10,FontWeight=FontWeights.Bold,
                    Foreground=Brushes.White,IsHitTestVisible=false};
                Canvas.SetLeft(lbl,w/2-12); Canvas.SetTop(lbl,h/2-7);
                group.Children.Add(lbl);
            } else {
                var border = new Border { Width=w,Height=h,Background=new SolidColorBrush(fill),
                    BorderBrush=Brushes.Black,BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(3),Opacity=0.85 };
                border.Child = new TextBlock{Text=label.Length>10?label.Substring(0,10):label,
                    Foreground=Brushes.White,FontSize=w<30?7:9,FontWeight=FontWeights.Bold,
                    HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,
                    TextWrapping=TextWrapping.Wrap,TextAlignment=TextAlignment.Center};
                group.Children.Add(border);
            }
            group.Tag = item; group.Cursor = Cursors.SizeAll;
            item.Visual = group;
            Canvas.SetLeft(group,x); Canvas.SetTop(group,y);
            editorCanvas.Children.Add(group);
            _items.Add(item);
        }

        void DrawGroove(Canvas c, double cx, double cy, double r, double a1d, double a2d)
        {
            double a1=a1d*Math.PI/180, a2=a2d*Math.PI/180;
            double x1=cx+r*Math.Cos(a1), y1=cy+r*Math.Sin(a1);
            double x2=cx+r*Math.Cos(a2), y2=cy+r*Math.Sin(a2);
            double ma=(a1+a2)/2; double mr=r*0.7;
            var fig = new PathFigure{StartPoint=new Point(x1,y1)};
            fig.Segments.Add(new QuadraticBezierSegment(new Point(cx+mr*Math.Cos(ma),cy+mr*Math.Sin(ma)),new Point(x2,y2),true));
            var geom = new PathGeometry(); geom.Figures.Add(fig);
            var path = new System.Windows.Shapes.Path{Data=geom,Stroke=new SolidColorBrush(Color.FromArgb(100,200,200,200)),
                StrokeThickness=3,StrokeDashArray=new DoubleCollection{3,2},IsHitTestVisible=false};
            c.Children.Add(path);
        }

        void LoadCurrentDiagram()
        {
            AddItem("Pump",5,340); _items.Last().Name="Pump A"; _items.Last().FillColor=Colors.SteelBlue;
            AddItem("Pump",265,340); _items.Last().Name="Pump B"; _items.Last().FillColor=Colors.IndianRed;
            AddItem("6-Port Valve",80,340); _items.Last().Name="Valve A"; _items.Last().Label="A";
            AddItem("6-Port Valve",200,340); _items.Last().Name="Valve B"; _items.Last().Label="B";
            AddItem("6-Port Valve",360,55); _items.Last().Name="Trap V"; _items.Last().Label="T"; _items.Last().FillColor=Colors.Goldenrod;
            AddItem("6-Port Valve",360,155); _items.Last().Name="Inj V"; _items.Last().Label="I"; _items.Last().FillColor=Colors.DarkSeaGreen;
            AddItem("Injection Port",370,10);
            AddItem("Output",420,75);
            AddItem("Pressure Sensor",42,385);  _items.Last().Name="PS A";
            AddItem("Pressure Sensor",254,385); _items.Last().Name="PS B";
            AddItem("Flow Sensor",140,440); _items.Last().Name="FS A";
            AddItem("Flow Sensor",140,415); _items.Last().Name="FS B";
            AddItem("Mixing Tee",330,430);
            AddItem("Peltier Stack",10,30);
        }

        // --- INPUT ---
        void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(editorCanvas);

            // Check if we hit a port dot first
            var hitPort = FindPort(e);
            if (hitPort != null && IsDrawMode) { AddTubingPoint(GetPortCenter(hitPort)); return; }
            if (IsDrawMode) { AddTubingPoint(pos); return; }

            // Drag mode — find component
            var item = FindItem(e);
            if (item != null) {
                _dragElement = (FrameworkElement)item.Visual; _dragOffset = e.GetPosition(_dragElement);
                _isDragging = true; _selectedItem = item; _dragElement.CaptureMouse();
                txtSelected.Text = item.Name + " (" + item.Type + ")"; UpdateCoords(item);
                _updatingSize=true; txtWidth.Text=item.W.ToString("F0"); txtHeight.Text=item.H.ToString("F0"); _updatingSize=false;
            }
        }

        void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _dragElement==null) return;
            var pos=e.GetPosition(editorCanvas);
            double nx=Math.Round((pos.X-_dragOffset.X)/5)*5, ny=Math.Round((pos.Y-_dragOffset.Y)/5)*5;
            Canvas.SetLeft(_dragElement,nx); Canvas.SetTop(_dragElement,ny);
            _selectedItem.X=nx; _selectedItem.Y=ny; UpdateCoords(_selectedItem);
        }

        void Canvas_MouseLeftButtonUp(object s, MouseButtonEventArgs e)
        { if(_isDragging&&_dragElement!=null){_dragElement.ReleaseMouseCapture();_isDragging=false;_dragElement=null;} }

        void Canvas_RightClick(object s, MouseButtonEventArgs e)
        {
            var port = FindPort(e);
            if (port != null) {
                txtSelected.Text = port.Owner.Name + "\nPort " + port.PortNum + " (" + PortClocks[port.PortNum-1] + " o'clk)";
            }
        }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _activeTubing != null) { FinishTubing(); }
            if (e.Key == Key.Escape && _activeTubing != null) { CancelTubing(); }
        }

        DiagramItem FindItem(MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as FrameworkElement;
            while (hit != null) {
                if (hit.Tag is DiagramItem di) return di;
                if (hit.Tag is PortDot) return null; // don't drag when clicking port
                hit = VisualTreeHelper.GetParent(hit) as FrameworkElement;
            }
            return null;
        }

        PortDot FindPort(MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as FrameworkElement;
            while (hit != null) {
                if (hit.Tag is PortDot pd) return pd;
                hit = VisualTreeHelper.GetParent(hit) as FrameworkElement;
            }
            return null;
        }

        Point GetPortCenter(PortDot pd)
        {
            double cx = pd.Owner.X + pd.Owner.W/2;
            double cy = pd.Owner.Y + pd.Owner.H/2;
            double a = pd.AngleDeg * Math.PI / 180;
            double r = pd.Owner.W/2 + 4;
            return new Point(cx + r*Math.Cos(a), cy + r*Math.Sin(a));
        }

        void UpdateCoords(DiagramItem item) { txtCoords.Text="Left=\""+item.X.ToString("F0")+"\" Top=\""+item.Y.ToString("F0")+"\"\n"+item.W.ToString("F0")+"x"+item.H.ToString("F0"); }

        // --- TUBING (multi-point) ---
        void AddTubingPoint(Point pt)
        {
            if (_activeTubing == null) {
                _activeTubing = new TubingPath { Thickness=TubeWidth, Color=TubeColor };
                _tubingPaths.Add(_activeTubing);
                txtSelected.Text = "Tubing: click waypoints, ENTER to finish";
            }
            _activeTubing.Points.Add(pt);

            // Draw dot at waypoint
            var dot = new Ellipse{Width=6,Height=6,Fill=new SolidColorBrush(_activeTubing.Color),IsHitTestVisible=false};
            Canvas.SetLeft(dot,pt.X-3); Canvas.SetTop(dot,pt.Y-3);
            editorCanvas.Children.Add(dot);
            _activeTubing.Visuals.Add(dot);

            // Draw line from previous point
            if (_activeTubing.Points.Count > 1) {
                var prev = _activeTubing.Points[_activeTubing.Points.Count-2];
                var line = new Line{X1=prev.X,Y1=prev.Y,X2=pt.X,Y2=pt.Y,
                    Stroke=new SolidColorBrush(_activeTubing.Color),StrokeThickness=_activeTubing.Thickness,IsHitTestVisible=false};
                editorCanvas.Children.Add(line);
                _activeTubing.Visuals.Add(line);
            }
        }

        void FinishTubing()
        {
            if (_activeTubing != null && _activeTubing.Points.Count >= 2) {
                txtSelected.Text = "Tubing finished (" + _activeTubing.Points.Count + " points)";
            }
            _activeTubing = null;
        }

        void CancelTubing()
        {
            if (_activeTubing != null) {
                foreach (var v in _activeTubing.Visuals) editorCanvas.Children.Remove(v);
                _tubingPaths.Remove(_activeTubing);
                _activeTubing = null;
                txtSelected.Text = "Tubing cancelled";
            }
        }

        // --- RESIZE ---
        void Size_TextChanged(object s, TextChangedEventArgs e)
        {
            if(_updatingSize||_selectedItem==null)return;
            if(double.TryParse(txtWidth.Text,out double w)&&w>5){_selectedItem.W=w;((FrameworkElement)_selectedItem.Visual).Width=w;}
            if(double.TryParse(txtHeight.Text,out double h)&&h>5){_selectedItem.H=h;((FrameworkElement)_selectedItem.Visual).Height=h;}
            UpdateCoords(_selectedItem);
        }
        void ResizeSelected(double f){if(_selectedItem==null)return;_selectedItem.W*=f;_selectedItem.H*=f;
            ((FrameworkElement)_selectedItem.Visual).Width=_selectedItem.W;((FrameworkElement)_selectedItem.Visual).Height=_selectedItem.H;
            _updatingSize=true;txtWidth.Text=_selectedItem.W.ToString("F0");txtHeight.Text=_selectedItem.H.ToString("F0");_updatingSize=false;UpdateCoords(_selectedItem);}
        void ShrinkBtn_Click(object s,RoutedEventArgs e){ResizeSelected(0.75);}
        void GrowBtn_Click(object s,RoutedEventArgs e){ResizeSelected(1.25);}
        void Grow50Btn_Click(object s,RoutedEventArgs e){ResizeSelected(1.5);}
        void DoubleBtn_Click(object s,RoutedEventArgs e){ResizeSelected(2.0);}

        // --- ACTIONS ---
        void DeleteSelected_Click(object s, RoutedEventArgs e)
        {
            if(_selectedItem!=null){editorCanvas.Children.Remove(_selectedItem.Visual);_items.Remove(_selectedItem);
                _selectedItem=null;txtSelected.Text="Deleted";txtCoords.Text="";}
        }

        void UndoTubing_Click(object s, RoutedEventArgs e)
        {
            if (_tubingPaths.Count > 0) {
                var last = _tubingPaths.Last();
                foreach (var v in last.Visuals) editorCanvas.Children.Remove(v);
                _tubingPaths.Remove(last);
                txtSelected.Text = "Last tubing removed";
            }
        }

        void AddCustom_Click(object s, RoutedEventArgs e)
        { var d=new InputDialog("Custom","Name:"); if(d.ShowDialog()==true)AddItem("Custom",200,250); _items.Last().Name=d.Result; }

        void Save_Click(object s, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# ProteYOLUTE Schematic — " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            foreach (var i in _items)
                sb.AppendLine("COMP|"+i.Name+"|"+i.Type+"|"+i.X.ToString("F1")+"|"+i.Y.ToString("F1")+"|"+i.W.ToString("F1")+"|"+i.H.ToString("F1"));
            foreach (var t in _tubingPaths) {
                string pts = string.Join(";", t.Points.Select(p=>p.X.ToString("F0")+","+p.Y.ToString("F0")));
                sb.AppendLine("TUBE|"+t.Thickness.ToString("F1")+"|"+t.Color+"|"+pts);
            }
            if(!Directory.Exists(SaveDir))Directory.CreateDirectory(SaveDir);
            string path = System.IO.Path.Combine(SaveDir,"diagram_save.txt");
            File.WriteAllText(path,sb.ToString());
            MessageBox.Show("Saved to:\n"+path,"Saved",MessageBoxButton.OK,MessageBoxImage.Information);
        }

        void ExportXaml_Click(object s, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!-- ProteYOLUTE Schematic Export -->");
            foreach (var i in _items)
                sb.AppendLine("<!-- "+i.Name+": Left=\""+i.X.ToString("F1")+"\" Top=\""+i.Y.ToString("F1")+"\" "+i.W.ToString("F0")+"x"+i.H.ToString("F0")+" -->");
            foreach (var t in _tubingPaths) {
                string pts = string.Join(" → ", t.Points.Select(p=>"("+p.X.ToString("F0")+","+p.Y.ToString("F0")+")"));
                sb.AppendLine("<!-- Tubing "+t.Thickness+"px: "+pts+" -->");
            }
            Clipboard.SetText(sb.ToString());
            string path = System.IO.Path.Combine(SaveDir,"diagram_export.txt");
            File.WriteAllText(path,sb.ToString());
            MessageBox.Show("Exported to clipboard and:\n"+path,"Export");
        }

        void BuildDeploy_Click(object s, RoutedEventArgs e)
        {
            try {
                var psi=new ProcessStartInfo("cmd.exe","/c \"C:\\Users\\Admin\\AppData\\Local\\Temp\\build.bat\"")
                    {RedirectStandardOutput=true,UseShellExecute=false,CreateNoWindow=true};
                var proc=Process.Start(psi);proc.WaitForExit(30000);
                string src=@"C:\BDalSystemData\HyStar\LcPlugin\PrivateData\Bruker proteoElute\DLL_Extract\dnspy_styling\BrukerLC.Styling\bin\Release\BrukerLC.Styling.dll";
                string dst=@"C:\Program Files (x86)\Bruker Daltonik\HyStar\proteoElute\BrukerLC.Styling.dll";
                if(File.Exists(src)){File.Copy(src,dst,true);MessageBox.Show("Built & deployed! Restart HyStar.","Success",MessageBoxButton.OK,MessageBoxImage.Information);}
                else MessageBox.Show("Build may have failed.","Warning",MessageBoxButton.OK,MessageBoxImage.Warning);
            } catch(Exception ex){MessageBox.Show("Error: "+ex.Message);}
        }
    }

    public class InputDialog : Window {
        private TextBox _tb; public string Result{get;private set;}
        public InputDialog(string title,string prompt){
            Title=title;Width=300;Height=150;WindowStartupLocation=WindowStartupLocation.CenterOwner;
            var sp=new StackPanel{Margin=new Thickness(10)};
            sp.Children.Add(new TextBlock{Text=prompt});
            _tb=new TextBox{Margin=new Thickness(0,8,0,8)};sp.Children.Add(_tb);
            var btn=new Button{Content="OK",Width=80,HorizontalAlignment=HorizontalAlignment.Right};
            btn.Click+=(s,e)=>{Result=_tb.Text;DialogResult=true;};sp.Children.Add(btn);
            Content=sp;
        }
    }
}
