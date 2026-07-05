using System.ComponentModel;
using System.Windows;

namespace BrukerLC.Utils.Attached;

public static class APVisibleEnableable
{
	private const string PROPERTY_CATEGORY = "Customer VisibleEnableable Properties";

	public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.RegisterAttached("IsVisible", typeof(bool), typeof(APVisibleEnableable), new FrameworkPropertyMetadata(true));

	public static readonly DependencyProperty IsNearTransparentProperty = DependencyProperty.RegisterAttached("IsNearTransparent", typeof(bool), typeof(APVisibleEnableable), new FrameworkPropertyMetadata(false));

	[Description("Gets the IsVisible.")]
	[Category("Customer VisibleEnableable Properties")]
	public static bool GetIsVisible(UIElement element)
	{
		return (bool)element.GetValue(IsVisibleProperty);
	}

	[Description("Sets the IsVisible.")]
	[Category("Customer VisibleEnableable Properties")]
	public static void SetIsVisible(UIElement element, bool value)
	{
		element.SetValue(IsVisibleProperty, value);
	}

	[Description("Gets the IsNearTransparent.")]
	[Category("Customer VisibleEnableable Properties")]
	public static bool GetIsNearTransparent(UIElement element)
	{
		return (bool)element.GetValue(IsNearTransparentProperty);
	}

	[Description("Sets the IsNearTransparent.")]
	[Category("Customer VisibleEnableable Properties")]
	public static void SetIsNearTransparent(UIElement element, bool value)
	{
		element.SetValue(IsNearTransparentProperty, value);
	}
}
