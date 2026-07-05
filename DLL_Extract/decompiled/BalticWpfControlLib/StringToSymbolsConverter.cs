using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib;

public class StringToSymbolsConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string text)
		{
			return text.Replace("uL", "µL");
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}
