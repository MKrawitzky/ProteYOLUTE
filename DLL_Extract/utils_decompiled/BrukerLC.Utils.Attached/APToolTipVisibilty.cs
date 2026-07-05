using System.ComponentModel;
using System.Windows;

namespace BrukerLC.Utils.Attached;

public static class APToolTipVisibilty
{
	private const string PROPERTY_CATEGORY = "Customer ToolTipVisibilty Properties";

	public static readonly DependencyProperty IsToolTipVisibleProperty = DependencyProperty.RegisterAttached("IsToolTipVisible", typeof(bool), typeof(APToolTipVisibilty), new FrameworkPropertyMetadata(false));

	[Description("Gets the IsToolTipVisible.")]
	[Category("Customer ToolTipVisibilty Properties")]
	public static bool GetIsToolTipVisible(UIElement element)
	{
		return (bool)element.GetValue(IsToolTipVisibleProperty);
	}

	[Description("Sets the IsToolTipVisible.")]
	[Category("Customer ToolTipVisibilty Properties")]
	public static void SetIsToolTipVisible(UIElement element, bool value)
	{
		element.SetValue(IsToolTipVisibleProperty, value);
	}
}
