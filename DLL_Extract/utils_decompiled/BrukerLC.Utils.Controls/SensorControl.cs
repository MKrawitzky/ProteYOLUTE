using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BrukerLC.Utils.Enums;

namespace BrukerLC.Utils.Controls;

public class SensorControl : Control
{
	private const string PROPERTY_CATEGORY = "Customer SensorControl Properties";

	public static readonly DependencyProperty ThroughputProperty;

	public static readonly DependencyProperty ThroughputUnitProperty;

	public static readonly DependencyProperty SensorOrientationProperty;

	[Description("Gets or sets the Throughput.")]
	[Category("Customer SensorControl Properties")]
	public double Throughput
	{
		get
		{
			return (double)GetValue(ThroughputProperty);
		}
		set
		{
			SetValue(ThroughputProperty, value);
		}
	}

	[Description("Gets or sets the ThroughputUnit.")]
	[Category("Customer SensorControl Properties")]
	public string ThroughputUnit
	{
		get
		{
			return (string)GetValue(ThroughputUnitProperty);
		}
		set
		{
			SetValue(ThroughputUnitProperty, value);
		}
	}

	[Description("Gets or sets the SensorOrientation.")]
	[Category("Customer SensorControl Properties")]
	public ControlOrientation SensorOrientation
	{
		get
		{
			return (ControlOrientation)GetValue(SensorOrientationProperty);
		}
		set
		{
			SetValue(SensorOrientationProperty, value);
		}
	}

	static SensorControl()
	{
		ThroughputProperty = DependencyProperty.Register("Throughput", typeof(double), typeof(SensorControl), new FrameworkPropertyMetadata(0.0));
		ThroughputUnitProperty = DependencyProperty.Register("ThroughputUnit", typeof(string), typeof(SensorControl), new FrameworkPropertyMetadata(null));
		SensorOrientationProperty = DependencyProperty.Register("SensorOrientation", typeof(ControlOrientation), typeof(SensorControl), new FrameworkPropertyMetadata(ControlOrientation.Left));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SensorControl), new FrameworkPropertyMetadata(typeof(SensorControl)));
	}
}
