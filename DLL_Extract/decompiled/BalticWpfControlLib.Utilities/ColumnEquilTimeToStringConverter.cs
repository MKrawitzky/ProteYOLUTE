using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

internal class ColumnEquilTimeToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is double num)
		{
			if (!(num > 0.0) || double.IsInfinity(num) || double.IsNaN(num))
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}: --- min", parameter);
			}
			return string.Format(CultureInfo.InvariantCulture, "{0}: {1:0.00} min", parameter, num);
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
