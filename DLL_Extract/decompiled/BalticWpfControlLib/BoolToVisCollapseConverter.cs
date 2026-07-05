using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib;

public class BoolToVisCollapseConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		Visibility result = Visibility.Collapsed;
		if (parameter is string value2)
		{
			Enum.TryParse<Visibility>(value2, out result);
		}
		else if (parameter is Visibility visibility)
		{
			result = visibility;
		}
		return (!(bool)value) ? result : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
