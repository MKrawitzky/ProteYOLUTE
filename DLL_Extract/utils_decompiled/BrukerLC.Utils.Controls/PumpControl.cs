using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BrukerLC.Utils.Enums;

namespace BrukerLC.Utils.Controls;

public class PumpControl : Control
{
	private const string PROPERTY_CATEGORY = "Customer PumpControl Properties";

	public static readonly DependencyProperty ThroughputProperty;

	public static readonly DependencyProperty ThroughputUnitProperty;

	public static readonly DependencyProperty ThroughputSetPointProperty;

	public static readonly DependencyProperty PressureProperty;

	public static readonly DependencyProperty PressureUnitProperty;

	public static readonly DependencyProperty FillLevelProperty;

	public static readonly DependencyProperty VolumeUsedProperty;

	public static readonly DependencyProperty VolumeLeftProperty;

	public static readonly DependencyProperty VolumeUnitProperty;

	public static readonly DependencyProperty CornerRadiusProperty;

	public static readonly DependencyProperty PumpOrientationProperty;

	public static readonly DependencyProperty TitleProperty;

	[Description("Gets or sets the current flow of the pump.")]
	[Category("Customer PumpControl Properties")]
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

	[Description("Gets or sets the throughput unit.")]
	[Category("Customer PumpControl Properties")]
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

	[Description("Gets or sets the current set point flow of the pump.")]
	[Category("Customer PumpControl Properties")]
	public double ThroughputSetPoint
	{
		get
		{
			return (double)GetValue(ThroughputSetPointProperty);
		}
		set
		{
			SetValue(ThroughputSetPointProperty, value);
		}
	}

	[Description("Gets or sets the current pressure of the pump.")]
	[Category("Customer PumpControl Properties")]
	public double Pressure
	{
		get
		{
			return (double)GetValue(PressureProperty);
		}
		set
		{
			SetValue(PressureProperty, value);
		}
	}

	[Description("Gets or sets the pressure unit.")]
	[Category("Customer PumpControl Properties")]
	public string PressureUnit
	{
		get
		{
			return (string)GetValue(PressureUnitProperty);
		}
		set
		{
			SetValue(PressureUnitProperty, value);
		}
	}

	[Description("Gets or sets the current volume used of the pump.")]
	[Category("Customer PumpControl Properties")]
	public double VolumeUsed
	{
		get
		{
			return (double)GetValue(VolumeUsedProperty);
		}
		set
		{
			SetValue(VolumeUsedProperty, value);
		}
	}

	[Description("Gets or sets the current volume left of the pump.")]
	[Category("Customer PumpControl Properties")]
	public double VolumeLeft
	{
		get
		{
			return (double)GetValue(VolumeLeftProperty);
		}
		set
		{
			SetValue(VolumeLeftProperty, value);
		}
	}

	[Description("Gets or sets the volume unit.")]
	[Category("Customer PumpControl Properties")]
	public string VolumeUnit
	{
		get
		{
			return (string)GetValue(VolumeUnitProperty);
		}
		set
		{
			SetValue(VolumeUnitProperty, value);
		}
	}

	[Description("Gets or sets the percentage of full volume.")]
	[Category("Customer PumpControl Properties")]
	public double FillLevel
	{
		get
		{
			return (double)GetValue(FillLevelProperty);
		}
		set
		{
			SetValue(FillLevelProperty, value);
		}
	}

	[Description("Gets or sets the corner radius.")]
	[Category("Customer PumpControl Properties")]
	public CornerRadius CornerRadius
	{
		get
		{
			return (CornerRadius)GetValue(CornerRadiusProperty);
		}
		set
		{
			SetValue(CornerRadiusProperty, value);
		}
	}

	[Description("Gets or sets the PumpOrientation.")]
	[Category("Customer PumpControl Properties")]
	public ControlOrientation PumpOrientation
	{
		get
		{
			return (ControlOrientation)GetValue(PumpOrientationProperty);
		}
		set
		{
			SetValue(PumpOrientationProperty, value);
		}
	}

	[Description("Gets or sets the pump title.")]
	[Category("Customer PumpControl Properties")]
	public string Title
	{
		get
		{
			return (string)GetValue(TitleProperty);
		}
		set
		{
			SetValue(TitleProperty, value);
		}
	}

	static PumpControl()
	{
		ThroughputProperty = DependencyProperty.Register("Throughput", typeof(double), typeof(PumpControl), new FrameworkPropertyMetadata(0.0));
		ThroughputUnitProperty = DependencyProperty.Register("ThroughputUnit", typeof(string), typeof(PumpControl), new FrameworkPropertyMetadata(null));
		ThroughputSetPointProperty = DependencyProperty.Register("ThroughputSetPoint", typeof(double), typeof(PumpControl), new FrameworkPropertyMetadata(0.0));
		PressureProperty = DependencyProperty.Register("Pressure", typeof(double), typeof(PumpControl), new FrameworkPropertyMetadata(0.0));
		PressureUnitProperty = DependencyProperty.Register("PressureUnit", typeof(string), typeof(PumpControl), new FrameworkPropertyMetadata(null));
		FillLevelProperty = DependencyProperty.Register("FillLevel", typeof(double), typeof(PumpControl), new FrameworkPropertyMetadata(0.0));
		VolumeUsedProperty = DependencyProperty.Register("VolumeUsed", typeof(double), typeof(PumpControl), new FrameworkPropertyMetadata(0.0));
		VolumeLeftProperty = DependencyProperty.Register("VolumeLeft", typeof(double), typeof(PumpControl), new FrameworkPropertyMetadata(0.0));
		VolumeUnitProperty = DependencyProperty.Register("VolumeUnit", typeof(string), typeof(PumpControl), new FrameworkPropertyMetadata(null));
		CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(PumpControl), new FrameworkPropertyMetadata(null));
		PumpOrientationProperty = DependencyProperty.Register("PumpOrientation", typeof(ControlOrientation), typeof(PumpControl), new FrameworkPropertyMetadata(ControlOrientation.Left));
		TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PumpControl), new FrameworkPropertyMetadata(null));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PumpControl), new FrameworkPropertyMetadata(typeof(PumpControl)));
	}
}
