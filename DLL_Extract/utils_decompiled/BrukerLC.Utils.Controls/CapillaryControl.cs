using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrukerLC.Utils.Controls;

public class CapillaryControl : Control
{
	private const string PROPERTY_CATEGORY = "Custom CapillaryControl Properties";

	public static readonly DependencyProperty PathDataProperty;

	public static readonly DependencyProperty HeadingProperty;

	public static readonly DependencyProperty LengthProperty;

	public static readonly DependencyProperty LengthUnitProperty;

	public static readonly DependencyProperty IDProperty;

	public static readonly DependencyProperty IDUnitProperty;

	[Description("Gets or sets the PathData.")]
	[Category("Custom CapillaryControl Properties")]
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

	[Description("Gets or sets the Heading.")]
	[Category("Custom CapillaryControl Properties")]
	public string Heading
	{
		get
		{
			return (string)GetValue(HeadingProperty);
		}
		set
		{
			SetValue(HeadingProperty, value);
		}
	}

	[Description("Gets or sets the Length.")]
	[Category("Custom CapillaryControl Properties")]
	public double Length
	{
		get
		{
			return (double)GetValue(LengthProperty);
		}
		set
		{
			SetValue(LengthProperty, value);
		}
	}

	[Description("Gets or sets the LengthUnit.")]
	[Category("Custom CapillaryControl Properties")]
	public string LengthUnit
	{
		get
		{
			return (string)GetValue(LengthUnitProperty);
		}
		set
		{
			SetValue(LengthUnitProperty, value);
		}
	}

	[Description("Gets or sets the ID")]
	[Category("Custom CapillaryControl Properties")]
	public double ID
	{
		get
		{
			return (double)GetValue(IDProperty);
		}
		set
		{
			SetValue(IDProperty, value);
		}
	}

	[Description("Gets or sets the ID Unit")]
	[Category("Custom CapillaryControl Properties")]
	public string IDUnit
	{
		get
		{
			return (string)GetValue(IDUnitProperty);
		}
		set
		{
			SetValue(IDUnitProperty, value);
		}
	}

	static CapillaryControl()
	{
		PathDataProperty = DependencyProperty.Register("PathData", typeof(PathGeometry), typeof(CapillaryControl), new FrameworkPropertyMetadata(null));
		HeadingProperty = DependencyProperty.Register("Heading", typeof(string), typeof(CapillaryControl), new FrameworkPropertyMetadata("CAPILLARY"));
		LengthProperty = DependencyProperty.Register("Length", typeof(double), typeof(CapillaryControl), new FrameworkPropertyMetadata(0.0));
		LengthUnitProperty = DependencyProperty.Register("LengthUnit", typeof(string), typeof(CapillaryControl), new FrameworkPropertyMetadata("mm"));
		IDProperty = DependencyProperty.Register("ID", typeof(double), typeof(CapillaryControl), new FrameworkPropertyMetadata(0.0));
		IDUnitProperty = DependencyProperty.Register("IDUnit", typeof(string), typeof(CapillaryControl), new FrameworkPropertyMetadata("µm"));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CapillaryControl), new FrameworkPropertyMetadata(typeof(CapillaryControl)));
	}
}
