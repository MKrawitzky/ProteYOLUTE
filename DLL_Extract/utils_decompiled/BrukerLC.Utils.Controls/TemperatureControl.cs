using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class TemperatureControl : Control
{
	private const string PROPERTY_CATEGORY = "Customer TemperatureControl Properties";

	public static readonly DependencyProperty TemperatureProperty;

	public static readonly DependencyProperty TemperatureUnitProperty;

	public static readonly DependencyProperty TemperatureSetPointProperty;

	public static readonly DependencyProperty TemperatureSetPointUnitProperty;

	[Description("Gets or sets the Temperature.")]
	[Category("Customer TemperatureControl Properties")]
	public double Temperature
	{
		get
		{
			return (double)GetValue(TemperatureProperty);
		}
		set
		{
			SetValue(TemperatureProperty, value);
		}
	}

	[Description("Gets or sets the TemperatureUnit.")]
	[Category("Customer TemperatureControl Properties")]
	public string TemperatureUnit
	{
		get
		{
			return (string)GetValue(TemperatureUnitProperty);
		}
		set
		{
			SetValue(TemperatureUnitProperty, value);
		}
	}

	[Description("Gets or sets the Temperature Set Point.")]
	[Category("Customer TemperatureControl Properties")]
	public double TemperatureSetPoint
	{
		get
		{
			return (double)GetValue(TemperatureSetPointProperty);
		}
		set
		{
			SetValue(TemperatureSetPointProperty, value);
		}
	}

	[Description("Gets or sets the Temperature SetPoint Unit.")]
	[Category("Customer TemperatureControl Properties")]
	public string TemperatureSetPointUnit
	{
		get
		{
			return (string)GetValue(TemperatureSetPointUnitProperty);
		}
		set
		{
			SetValue(TemperatureSetPointUnitProperty, value);
		}
	}

	static TemperatureControl()
	{
		TemperatureProperty = DependencyProperty.Register("Temperature", typeof(double), typeof(TemperatureControl), new FrameworkPropertyMetadata(0.0));
		TemperatureUnitProperty = DependencyProperty.Register("TemperatureUnit", typeof(string), typeof(TemperatureControl), new FrameworkPropertyMetadata(null));
		TemperatureSetPointProperty = DependencyProperty.Register("TemperatureSetPoint", typeof(double), typeof(TemperatureControl), new FrameworkPropertyMetadata(0.0));
		TemperatureSetPointUnitProperty = DependencyProperty.Register("TemperatureSetPointUnit", typeof(string), typeof(TemperatureControl), new FrameworkPropertyMetadata(null));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TemperatureControl), new FrameworkPropertyMetadata(typeof(TemperatureControl)));
	}
}
