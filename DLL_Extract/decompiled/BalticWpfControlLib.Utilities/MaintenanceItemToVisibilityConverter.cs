using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

internal class MaintenanceItemToVisibilityConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		object obj = values[0];
		if (!(obj is bool) || (bool)obj)
		{
			obj = values[1];
			if (!(obj is bool) || !(bool)obj)
			{
				return Visibility.Collapsed;
			}
		}
		return Visibility.Visible;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
