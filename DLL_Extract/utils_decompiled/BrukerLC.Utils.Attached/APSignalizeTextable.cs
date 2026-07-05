using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace BrukerLC.Utils.Attached;

public static class APSignalizeTextable
{
	private const string PROPERTY_CATEGORY = "Customer Signalizable Text Properties";

	public static readonly DependencyProperty IsSignalizeTextProperty = DependencyProperty.RegisterAttached("IsSignalizeText", typeof(bool), typeof(APSignalizeTextable), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty SignalizeTextBrushProperty = DependencyProperty.RegisterAttached("SignalizeTextBrush", typeof(Brush), typeof(APSignalizeTextable), new FrameworkPropertyMetadata(null));

	[Description("Gets the IsSignalizeText.")]
	[Category("Customer Signalizable Text Properties")]
	public static bool GetIsSignalizeText(UIElement element)
	{
		return (bool)element.GetValue(IsSignalizeTextProperty);
	}

	[Description("Sets the IsSignalizeText.")]
	[Category("Customer Signalizable Text Properties")]
	public static void SetIsSignalizeText(UIElement element, bool value)
	{
		element.SetValue(IsSignalizeTextProperty, value);
	}

	[Description("Gets the SignalizeTextBrush.")]
	[Category("Customer Signalizable Text Properties")]
	public static Brush GetSignalizeTextBrush(UIElement element)
	{
		return (Brush)element.GetValue(SignalizeTextBrushProperty);
	}

	[Description("Sets the SignalizeTextBrush.")]
	[Category("Customer Signalizable Text Properties")]
	public static void SetSignalizeTextBrush(UIElement element, Brush value)
	{
		element.SetValue(SignalizeTextBrushProperty, value);
	}
}
