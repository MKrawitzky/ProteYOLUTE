using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace BrukerLC.Utils.Attached;

public static class APSignalizable
{
	private const string PROPERTY_CATEGORY = "Customer Signalizable Properties";

	public static readonly DependencyProperty IsSignalizeProperty = DependencyProperty.RegisterAttached("IsSignalize", typeof(bool), typeof(APSignalizable), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty SignalizeBrushProperty = DependencyProperty.RegisterAttached("SignalizeBrush", typeof(Brush), typeof(APSignalizable), new FrameworkPropertyMetadata(null));

	[Description("Gets the IsSignalize.")]
	[Category("Customer Signalizable Properties")]
	public static bool GetIsSignalize(UIElement element)
	{
		return (bool)element.GetValue(IsSignalizeProperty);
	}

	[Description("Sets the IsSignalize.")]
	[Category("Customer Signalizable Properties")]
	public static void SetIsSignalize(UIElement element, bool value)
	{
		element.SetValue(IsSignalizeProperty, value);
	}

	[Description("Gets the SignalizeBrush.")]
	[Category("Customer Signalizable Properties")]
	public static Brush GetSignalizeBrush(UIElement element)
	{
		return (Brush)element.GetValue(SignalizeBrushProperty);
	}

	[Description("Sets the SignalizeBrush.")]
	[Category("Customer Signalizable Properties")]
	public static void SetSignalizeBrush(UIElement element, Brush value)
	{
		element.SetValue(SignalizeBrushProperty, value);
	}
}
