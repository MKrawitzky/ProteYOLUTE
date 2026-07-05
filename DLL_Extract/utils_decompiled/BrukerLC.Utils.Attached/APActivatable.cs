using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace BrukerLC.Utils.Attached;

public static class APActivatable
{
	private const string PROPERTY_CATEGORY = "Customer Activatable Properties";

	public static readonly DependencyProperty IsActiveProperty = DependencyProperty.RegisterAttached("IsActive", typeof(bool), typeof(APActivatable), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty ActiveBrushProperty = DependencyProperty.RegisterAttached("ActiveBrush", typeof(Brush), typeof(APActivatable), new FrameworkPropertyMetadata(null));

	[Description("Gets the IsActive.")]
	[Category("Customer Activatable Properties")]
	public static bool GetIsActive(UIElement element)
	{
		return (bool)element.GetValue(IsActiveProperty);
	}

	[Description("Sets the IsActive.")]
	[Category("Customer Activatable Properties")]
	public static void SetIsActive(UIElement element, bool value)
	{
		element.SetValue(IsActiveProperty, value);
	}

	[Description("Gets the ActiveBrush.")]
	[Category("Customer Activatable Properties")]
	public static Brush GetActiveBrush(UIElement element)
	{
		return (Brush)element.GetValue(ActiveBrushProperty);
	}

	[Description("Sets the ActiveBrush.")]
	[Category("Customer Activatable Properties")]
	public static void SetActiveBrush(UIElement element, Brush value)
	{
		element.SetValue(ActiveBrushProperty, value);
	}
}
