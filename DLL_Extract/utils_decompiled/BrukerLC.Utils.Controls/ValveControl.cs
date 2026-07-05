using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrukerLC.Utils.Controls;

public class ValveControl : ItemsControl
{
	private const string PROPERTY_CATEGORY = "Valve Control Properties";

	public static readonly DependencyProperty AngleProperty;

	public static readonly DependencyProperty ActualAngleProperty;

	public static readonly DependencyProperty AngleMarginProperty;

	public static readonly DependencyProperty IsServiceProperty;

	private Canvas PART_PortCanvas { get; set; }

	[Description("Gets or sets the Angle Property.")]
	[Category("Valve Control Properties")]
	public double Angle
	{
		get
		{
			return (double)GetValue(AngleProperty);
		}
		set
		{
			SetValue(AngleProperty, value);
		}
	}

	[Description("Gets or sets the ActualAngle Property.")]
	[Category("Valve Control Properties")]
	public double ActualAngle
	{
		get
		{
			return (double)GetValue(ActualAngleProperty);
		}
		set
		{
			SetValue(ActualAngleProperty, value);
		}
	}

	[Description("Gets or sets the AngleMargin Property.")]
	[Category("Valve Control Properties")]
	public Thickness AngleMargin
	{
		get
		{
			return (Thickness)GetValue(AngleMarginProperty);
		}
		set
		{
			SetValue(AngleMarginProperty, value);
		}
	}

	[Description("Gets or sets the IsService Property.")]
	[Category("Valve Control Properties")]
	public bool IsService
	{
		get
		{
			return (bool)GetValue(IsServiceProperty);
		}
		set
		{
			SetValue(IsServiceProperty, value);
		}
	}

	static ValveControl()
	{
		AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(ValveControl), new FrameworkPropertyMetadata(0.0));
		ActualAngleProperty = DependencyProperty.Register("ActualAngle", typeof(double), typeof(ValveControl), new FrameworkPropertyMetadata(0.0));
		AngleMarginProperty = DependencyProperty.Register("AngleMargin", typeof(Thickness), typeof(ValveControl), new FrameworkPropertyMetadata(new Thickness(0.0, 0.0, 0.0, 0.0)));
		IsServiceProperty = DependencyProperty.Register("IsService", typeof(bool), typeof(ValveControl), new FrameworkPropertyMetadata(false));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ValveControl), new FrameworkPropertyMetadata(typeof(ValveControl)));
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		PART_PortCanvas = GetTemplateChild("PART_PortCanvas") as Canvas;
	}

	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is ValveControlItem;
	}

	protected override DependencyObject GetContainerForItemOverride()
	{
		return new ValveControlItem();
	}

	public Point GetPointForPort(int port)
	{
		Matrix value = (GetTemplateChild("PART_Port" + port) as FrameworkElement).RenderTransform.Value;
		return new Point(value.OffsetX + base.Width / 2.0, value.OffsetY + base.Height / 2.0);
	}
}
