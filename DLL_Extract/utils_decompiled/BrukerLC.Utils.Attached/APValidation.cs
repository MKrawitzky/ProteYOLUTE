using System.ComponentModel;
using System.Windows;
using BrukerLC.Utils.Events;

namespace BrukerLC.Utils.Attached;

public static class APValidation
{
	private const string PROPERTY_CATEGORY = "Customer Validation Properties";

	public static readonly DependencyProperty HasErrorProperty = DependencyProperty.RegisterAttached("HasError", typeof(bool), typeof(APValidation), new FrameworkPropertyMetadata(false, OnHasErrorChanged));

	public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.RegisterAttached("ErrorMessage", typeof(string), typeof(APValidation), new FrameworkPropertyMetadata(null));

	public static readonly RoutedEvent ErrorStatusChanged = EventManager.RegisterRoutedEvent("ErrorStatusChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(APValidation));

	public static void AddErrorStatusChangedHandler(DependencyObject d, RoutedEventHandler handler)
	{
		if (d is UIElement uIElement)
		{
			uIElement.AddHandler(ErrorStatusChanged, handler);
		}
	}

	public static void RemoveErrorStatusChangedHandler(DependencyObject d, RoutedEventHandler handler)
	{
		if (d is UIElement uIElement)
		{
			uIElement.RemoveHandler(ErrorStatusChanged, handler);
		}
	}

	private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FrameworkElement frameworkElement)
		{
			frameworkElement.RaiseEvent(new ErrorStatusChangedEventArgs(ErrorStatusChanged, frameworkElement.GetHashCode().ToString(), GetErrorMessage(frameworkElement), GetHasError(frameworkElement)));
		}
	}

	[Description("Gets the HasError.")]
	[Category("Customer Validation Properties")]
	public static bool GetHasError(UIElement element)
	{
		return (bool)element.GetValue(HasErrorProperty);
	}

	[Description("Sets the HasError.")]
	[Category("Customer Validation Properties")]
	public static void SetHasError(UIElement element, bool value)
	{
		element.SetValue(HasErrorProperty, value);
	}

	[Description("Gets the ErrorMessage.")]
	[Category("Customer Validation Properties")]
	public static string GetErrorMessage(UIElement element)
	{
		return (string)element.GetValue(ErrorMessageProperty);
	}

	[Description("Sets the ErrorMessage.")]
	[Category("Customer Validation Properties")]
	public static void SetErrorMessage(UIElement element, string value)
	{
		element.SetValue(ErrorMessageProperty, value);
	}
}
