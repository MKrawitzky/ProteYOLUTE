using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class LoopControl : Control
{
	private const string PROPERTY_CATEGORY = "Customer LoopControl Properties";

	public static readonly DependencyProperty PathDataProperty;

	[Description("Gets or sets the PathData.")]
	[Category("Customer LoopControl Properties")]
	public object PathData
	{
		get
		{
			return GetValue(PathDataProperty);
		}
		set
		{
			SetValue(PathDataProperty, value);
		}
	}

	static LoopControl()
	{
		PathDataProperty = DependencyProperty.Register("PathData", typeof(object), typeof(LoopControl), new FrameworkPropertyMetadata(null));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(LoopControl), new FrameworkPropertyMetadata(typeof(LoopControl)));
	}
}
