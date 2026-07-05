using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Markup;

namespace BalticWpfControlLib;

[ValueConversion(typeof(string), typeof(string))]
public class SubjectListConverter : MarkupExtension, IValueConverter
{
	private readonly Regex _regex = new Regex("@\\S+\\s*");

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return this;
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null)
		{
			string input = value.ToString();
			return _regex.Replace(input, string.Empty).Trim();
		}
		return null;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}
