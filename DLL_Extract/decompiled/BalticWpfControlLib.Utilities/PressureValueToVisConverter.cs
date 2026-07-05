using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

public class PressureValueToVisConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values != null && values.Length >= 2)
		{
			object obj = values[0];
			if (obj is bool && !(bool)obj)
			{
				return Visibility.Collapsed;
			}
			obj = values[1];
			if (obj is int && (int)obj > 1000)
			{
				return Visibility.Visible;
			}
		}
		return Visibility.Collapsed;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
