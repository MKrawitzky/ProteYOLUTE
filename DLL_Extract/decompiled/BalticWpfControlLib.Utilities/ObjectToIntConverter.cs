using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

internal class ObjectToIntConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (int?)value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value;
	}
}
