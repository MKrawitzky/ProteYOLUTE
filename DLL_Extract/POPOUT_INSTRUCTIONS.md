# How to Add Diagram Pop-Out using dnSpy GUI

## Overview
The decompiled C# code has 28,000+ cascading errors from compiler-generated artifacts 
(`<>o__39.<>p__1` style names). Recompiling the full project requires extensive cleanup.

The fastest approach: use dnSpy GUI to edit a single method and save.

## Steps

### 1. Open dnSpy
Run: `C:\tools\dnspy\dnSpy.exe`

### 2. Load the DLL  
File > Open > Navigate to:
`C:\Program Files (x86)\Bruker Daltonik\HyStar\proteoElute\BalticWpfControlLib.dll`

### 3. Navigate to LCUserControl
In the left tree: `BalticWpfControlLib > BalticWpfControlLib > LCUserControl`

### 4. Find the constructor
Look for the constructor method `LCUserControl(...)` 

### 5. Right-click > Edit Method (C#)
At the END of the constructor, before the closing `}`, add:

```csharp
this.Diagram.MouseDoubleClick += delegate(object s, System.Windows.Input.MouseButtonEventArgs args) {
    var brush = new System.Windows.Media.VisualBrush(this.Diagram);
    brush.Stretch = System.Windows.Media.Stretch.Uniform;
    var rect = new System.Windows.Shapes.Rectangle();
    rect.Fill = brush;
    rect.Margin = new System.Windows.Thickness(10);
    var border = new System.Windows.Controls.Border();
    border.Background = System.Windows.Media.Brushes.White;
    border.Child = rect;
    var win = new System.Windows.Window();
    win.Title = "proteoElute - System Diagram (ProteYOLUTE)";
    win.Width = 900;
    win.Height = 750;
    win.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
    win.Content = border;
    win.Background = System.Windows.Media.Brushes.White;
    win.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
    win.Owner = System.Windows.Window.GetWindow(this);
    win.Show();
    args.Handled = true;
};
this.Diagram.ToolTip = "Double-click to pop out";
```

### 6. Click Compile
dnSpy will compile just this method in-place.

### 7. Save
File > Save Module > Save to the same path (or a new path first to test)

### 8. Test
Close HyStar, restart, double-click the diagram.

## Alternative: Full Project Recompile
To do a full recompile, we need to:
1. Replace all `<>o__39.<>p__1` compiler-generated names with valid identifiers
2. Rewrite all `dynamic` call site caches as proper `dynamic` calls
3. Fix ~319 string interpolation decompilation artifacts
4. This is ~11 files, ~28k cascading errors from ~50 root causes
