using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace BrukerLC.Utils.Attached;

public static class APIcon
{
	private const string PROPERTY_CATEGORY = "Customer Icon Properties";

	public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(object), typeof(APIcon), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty IconSizeProperty = DependencyProperty.RegisterAttached("IconSize", typeof(double), typeof(APIcon), new FrameworkPropertyMetadata(16.0));

	public static readonly DependencyProperty IconBrushProperty = DependencyProperty.RegisterAttached("IconBrush", typeof(Brush), typeof(APIcon), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty IconFontFamilyProperty = DependencyProperty.RegisterAttached("IconFontFamily", typeof(FontFamily), typeof(APIcon), new FrameworkPropertyMetadata(new FontFamily("Segoe UI")));

	public static readonly DependencyProperty IconMarginProperty = DependencyProperty.RegisterAttached("IconMargin", typeof(Thickness), typeof(APIcon), new FrameworkPropertyMetadata(null));

	[Description("Gets the Icon.")]
	[Category("Customer Icon Properties")]
	public static object GetIcon(UIElement element)
	{
		return element.GetValue(IconProperty);
	}

	[Description("Sets the Icon.")]
	[Category("Customer Icon Properties")]
	public static void SetIcon(UIElement element, object value)
	{
		element.SetValue(IconProperty, value);
	}

	[Description("Gets the IconSize.")]
	[Category("Customer Icon Properties")]
	public static double GetIconSize(UIElement element)
	{
		return (double)element.GetValue(IconSizeProperty);
	}

	[Description("Sets the IconSize.")]
	[Category("Customer Icon Properties")]
	public static void SetIconSize(UIElement element, double value)
	{
		element.SetValue(IconSizeProperty, value);
	}

	[Description("Gets the IconBrush.")]
	[Category("Customer Icon Properties")]
	public static Brush GetIconBrush(UIElement element)
	{
		return (Brush)element.GetValue(IconBrushProperty);
	}

	[Description("Sets the IconBrush.")]
	[Category("Customer Icon Properties")]
	public static void SetIconBrush(UIElement element, Brush value)
	{
		element.SetValue(IconBrushProperty, value);
	}

	[Description("Gets the IconFontFamily.")]
	[Category("Customer Icon Properties")]
	public static FontFamily GetIconFontFamily(UIElement element)
	{
		return (FontFamily)element.GetValue(IconFontFamilyProperty);
	}

	[Description("Sets the IconFontFamily.")]
	[Category("Customer Icon Properties")]
	public static void SetIconFontFamily(UIElement element, FontFamily value)
	{
		element.SetValue(IconFontFamilyProperty, value);
	}

	[Description("Gets the IconMargin.")]
	[Category("Customer Icon Properties")]
	public static Thickness GetIconMargin(UIElement element)
	{
		return (Thickness)element.GetValue(IconMarginProperty);
	}

	[Description("Sets the IconMargin.")]
	[Category("Customer Icon Properties")]
	public static void SetIconMargin(UIElement element, Thickness value)
	{
		element.SetValue(IconMarginProperty, value);
	}
}
