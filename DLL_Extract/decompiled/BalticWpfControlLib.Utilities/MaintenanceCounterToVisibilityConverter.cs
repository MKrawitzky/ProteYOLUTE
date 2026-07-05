using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib.Utilities;

internal class MaintenanceCounterToVisibilityConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] is IResetable)
		{
			object obj = values[1];
			if (obj is bool && (bool)obj)
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
