using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrukerLC.Utils.Controls;

public class LinkControlBasic : Control
{
	private const string PROPERTY_CATEGORY = "Customer LinkControlBasic Properties";

	public static readonly DependencyProperty PathDataProperty;

	public static readonly DependencyProperty EndPointTopProperty;

	public static readonly DependencyProperty EndPointLeftProperty;

	public static readonly DependencyProperty StartPointLeftProperty;

	public static readonly DependencyProperty StartPointTopProperty;

	public static readonly DependencyProperty StartPointProperty;

	[Description("Gets or sets the PathData.")]
	[Category("Customer LinkControlBasic Properties")]
	public PathGeometry PathData
	{
		get
		{
			return (PathGeometry)GetValue(PathDataProperty);
		}
		set
		{
			SetValue(PathDataProperty, value);
		}
	}

	[Description("Gets or sets the EndPointTop.")]
	[Category("Customer LinkControlBasic Properties")]
	public double EndPointTop
	{
		get
		{
			return (double)GetValue(EndPointTopProperty);
		}
		set
		{
			SetValue(EndPointTopProperty, value);
		}
	}

	[Description("Gets or sets the EndPointLeft.")]
	[Category("Customer LinkControlBasic Properties")]
	public double EndPointLeft
	{
		get
		{
			return (double)GetValue(EndPointLeftProperty);
		}
		set
		{
			SetValue(EndPointLeftProperty, value);
		}
	}

	[Description("Gets or sets the StartPointLeft.")]
	[Category("Customer LinkControlBasic Properties")]
	public double StartPointLeft
	{
		get
		{
			return (double)GetValue(StartPointLeftProperty);
		}
		set
		{
			SetValue(StartPointLeftProperty, value);
		}
	}

	[Description("Gets or sets the StartPointTop.")]
	[Category("Customer LinkControlBasic Properties")]
	public double StartPointTop
	{
		get
		{
			return (double)GetValue(StartPointTopProperty);
		}
		set
		{
			SetValue(StartPointTopProperty, value);
		}
	}

	[Description("Gets or sets the StartPoint.")]
	[Category("Customer LinkControlBasic Properties")]
	public bool StartPoint
	{
		get
		{
			return (bool)GetValue(StartPointProperty);
		}
		set
		{
			SetValue(StartPointProperty, value);
		}
	}

	static LinkControlBasic()
	{
		PathDataProperty = DependencyProperty.Register("PathData", typeof(PathGeometry), typeof(LinkControlBasic), new FrameworkPropertyMetadata(null));
		EndPointTopProperty = DependencyProperty.Register("EndPointTop", typeof(double), typeof(LinkControlBasic), new FrameworkPropertyMetadata(0.0));
		EndPointLeftProperty = DependencyProperty.Register("EndPointLeft", typeof(double), typeof(LinkControlBasic), new FrameworkPropertyMetadata(0.0));
		StartPointLeftProperty = DependencyProperty.Register("StartPointLeft", typeof(double), typeof(LinkControlBasic), new FrameworkPropertyMetadata(0.0));
		StartPointTopProperty = DependencyProperty.Register("StartPointTop", typeof(double), typeof(LinkControlBasic), new FrameworkPropertyMetadata(0.0));
		StartPointProperty = DependencyProperty.Register("StartPoint", typeof(bool), typeof(LinkControlBasic), new FrameworkPropertyMetadata(false));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkControlBasic), new FrameworkPropertyMetadata(typeof(LinkControlBasic)));
	}
}
