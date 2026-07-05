using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

[ValueConversion(typeof(object), typeof(string))]
public sealed class ObjectToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}
		if (value is double num)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:0.00}", num);
		}
		return value.ToString();
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
