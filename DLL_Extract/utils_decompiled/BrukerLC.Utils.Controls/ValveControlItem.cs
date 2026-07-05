using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BrukerLC.Interfaces.ViewModelInterfaces;
using BrukerLC.Utils.Helpers;

namespace BrukerLC.Utils.Controls;

public class ValveControlItem : Control
{
	private const string PROPERTY_CATEGORY = "Customer ValveControlItem Properties";

	public static readonly DependencyProperty IsActiveProperty;

	public static readonly DependencyProperty IsSignalizeProperty;

	public static readonly DependencyProperty ActiveBrushProperty;

	public static readonly DependencyProperty SignalizeBrushProperty;

	public new static readonly DependencyProperty IsVisibleProperty;

	private Path PART_Path { get; set; }

	[Description("Gets or sets the IsActive Property.")]
	[Category("Customer ValveControlItem Properties")]
	public bool IsActive
	{
		get
		{
			return (bool)GetValue(IsActiveProperty);
		}
		set
		{
			SetValue(IsActiveProperty, value);
		}
	}

	[Description("Gets or sets the IsSignalize Property.")]
	[Category("Customer ValveControlItem Properties")]
	public bool IsSignalize
	{
		get
		{
			return (bool)GetValue(IsSignalizeProperty);
		}
		set
		{
			SetValue(IsSignalizeProperty, value);
		}
	}

	[Description("Gets or sets the ActiveBrush.")]
	[Category("Customer ValveControlItem Properties")]
	public Brush ActiveBrush
	{
		get
		{
			return (Brush)GetValue(ActiveBrushProperty);
		}
		set
		{
			SetValue(ActiveBrushProperty, value);
		}
	}

	[Description("Gets or sets the SignalizeBrush.")]
	[Category("Customer ValveControlItem Properties")]
	public Brush SignalizeBrush
	{
		get
		{
			return (Brush)GetValue(SignalizeBrushProperty);
		}
		set
		{
			SetValue(SignalizeBrushProperty, value);
		}
	}

	[Description("Gets or sets the IsVisible Property.")]
	[Category("Customer ValveControlItem Properties")]
	public new bool IsVisible
	{
		get
		{
			return (bool)GetValue(IsVisibleProperty);
		}
		set
		{
			SetValue(IsVisibleProperty, value);
		}
	}

	static ValveControlItem()
	{
		IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(ValveControlItem), new FrameworkPropertyMetadata(false));
		IsSignalizeProperty = DependencyProperty.Register("IsSignalize", typeof(bool), typeof(ValveControlItem), new FrameworkPropertyMetadata(false));
		ActiveBrushProperty = DependencyProperty.Register("ActiveBrush", typeof(Brush), typeof(ValveControlItem), new FrameworkPropertyMetadata(null));
		SignalizeBrushProperty = DependencyProperty.Register("SignalizeBrush", typeof(Brush), typeof(ValveControlItem), new FrameworkPropertyMetadata(null));
		IsVisibleProperty = DependencyProperty.Register("IsVisible", typeof(bool), typeof(ValveControlItem), new FrameworkPropertyMetadata(true));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ValveControlItem), new FrameworkPropertyMetadata(typeof(ValveControlItem)));
	}

	public ValveControlItem()
	{
		base.Loaded += delegate
		{
			CreatePath();
		};
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		PART_Path = GetTemplateChild("PART_Path") as Path;
	}

	private void CreatePath()
	{
		if (base.DataContext is IConnectionViewModel connectionViewModel)
		{
			ValveControl valveControl = WPFHelpers.FindAncestor<ValveControl>(this);
			if (valveControl != null && PART_Path != null)
			{
				Point pointForPort = valveControl.GetPointForPort(connectionViewModel.FirstPort);
				Point pointForPort2 = valveControl.GetPointForPort(connectionViewModel.SecondPort);
				Point midPoint = GetMidPoint(pointForPort, pointForPort2);
				Point controlPoint = GetControlPoint(pointForPort, pointForPort2, midPoint, 8f);
				PathGeometry pathGeometry = new PathGeometry();
				PathFigureCollection pathFigureCollection = new PathFigureCollection();
				PathFigure pathFigure = new PathFigure();
				pathFigure.StartPoint = pointForPort;
				PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();
				BezierSegment value = new BezierSegment(pointForPort, controlPoint, pointForPort2, isStroked: true);
				pathSegmentCollection.Add(value);
				pathFigure.Segments = pathSegmentCollection;
				pathFigureCollection.Add(pathFigure);
				pathGeometry.Figures = pathFigureCollection;
				PART_Path.Data = pathGeometry;
			}
		}
	}

	private static Point GetMidPoint(Point p1, Point p2)
	{
		return new Point((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0);
	}

	private Point GetControlPoint(Point p1, Point p2, Point midPoint, float distance)
	{
		Vector vector = new Vector(p2.X - p1.X, p2.Y - p1.Y);
		Vector vector2 = new Vector(vector.Y, 0.0 - vector.X);
		vector2.Normalize();
		double num = vector.Y / vector.X;
		_ = p1.Y;
		_ = p1.X;
		Point point = new Point(midPoint.X + (double)distance * vector2.X, midPoint.Y + (double)distance * vector2.Y);
		vector2.Negate();
		Point point2 = new Point(midPoint.X + (double)distance * vector2.X, midPoint.Y + (double)distance * vector2.Y);
		Point point3 = new Point(base.Width / 2.0, base.Height / 2.0);
		double num2 = Math.Sqrt(Math.Pow(point.X - point3.X, 2.0) + Math.Pow(point.Y - point3.Y, 2.0));
		double num3 = Math.Sqrt(Math.Pow(point2.X - point3.X, 2.0) + Math.Pow(point2.Y - point3.Y, 2.0));
		Point result = ((num2 < num3) ? point : point2);
		if (num2 == num3)
		{
			result = midPoint;
		}
		return result;
	}
}
