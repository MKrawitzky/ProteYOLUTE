using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class InjectionControl : Control
{
	private const string PROPERTY_CATEGORY = "Customer InjectionControl Properties";

	public static readonly DependencyProperty LabelProperty;

	[Description("Gets or sets the label.")]
	[Category("Customer InjectionControl Properties")]
	public string Label
	{
		get
		{
			return (string)GetValue(LabelProperty);
		}
		set
		{
			SetValue(LabelProperty, value);
		}
	}

	static InjectionControl()
	{
		LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(InjectionControl), new FrameworkPropertyMetadata(null));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(InjectionControl), new FrameworkPropertyMetadata(typeof(InjectionControl)));
	}
}
