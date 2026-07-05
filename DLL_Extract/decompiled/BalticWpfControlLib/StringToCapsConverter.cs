using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib;

public class StringToCapsConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string text)
		{
			return text.ToUpper();
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
