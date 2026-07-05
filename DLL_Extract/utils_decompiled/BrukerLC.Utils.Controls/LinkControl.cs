using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrukerLC.Utils.Controls;

public class LinkControl : Control
{
	private const string PROPERTY_CATEGORY = "Customer LinkControl Properties";

	public static readonly DependencyProperty PathDataProperty;

	public static readonly DependencyProperty EndPointTopProperty;

	public static readonly DependencyProperty EndPointLeftProperty;

	public static readonly DependencyProperty StartPointLeftProperty;

	public static readonly DependencyProperty StartPointTopProperty;

	public static readonly DependencyProperty StartPointProperty;

	[Description("Gets or sets the PathData.")]
	[Category("Customer LinkControl Properties")]
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
	[Category("Customer LinkControl Properties")]
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
	[Category("Customer LinkControl Properties")]
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
	[Category("Customer LinkControl Properties")]
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
	[Category("Customer LinkControl Properties")]
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
	[Category("Customer LinkControl Properties")]
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

	static LinkControl()
	{
		PathDataProperty = DependencyProperty.Register("PathData", typeof(PathGeometry), typeof(LinkControl), new FrameworkPropertyMetadata(null));
		EndPointTopProperty = DependencyProperty.Register("EndPointTop", typeof(double), typeof(LinkControl), new FrameworkPropertyMetadata(0.0));
		EndPointLeftProperty = DependencyProperty.Register("EndPointLeft", typeof(double), typeof(LinkControl), new FrameworkPropertyMetadata(0.0));
		StartPointLeftProperty = DependencyProperty.Register("StartPointLeft", typeof(double), typeof(LinkControl), new FrameworkPropertyMetadata(0.0));
		StartPointTopProperty = DependencyProperty.Register("StartPointTop", typeof(double), typeof(LinkControl), new FrameworkPropertyMetadata(0.0));
		StartPointProperty = DependencyProperty.Register("StartPoint", typeof(bool), typeof(LinkControl), new FrameworkPropertyMetadata(false));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkControl), new FrameworkPropertyMetadata(typeof(LinkControl)));
	}
}
