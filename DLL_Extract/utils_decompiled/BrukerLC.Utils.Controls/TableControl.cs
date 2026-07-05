using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BrukerLC.Utils.Controls;

public class TableControl : Control
{
	private const string PROPERTY_CATEGORY = "Customer TableControl Properties";

	public static readonly DependencyProperty HeadingProperty;

	[Description("Gets or sets the heading.")]
	[Category("Customer TableControl Properties")]
	public string Heading
	{
		get
		{
			return (string)GetValue(HeadingProperty);
		}
		set
		{
			SetValue(HeadingProperty, value);
		}
	}

	static TableControl()
	{
		HeadingProperty = DependencyProperty.Register("Heading", typeof(string), typeof(TableControl), new FrameworkPropertyMetadata(null));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TableControl), new FrameworkPropertyMetadata(typeof(TableControl)));
	}
}
