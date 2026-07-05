using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib;

public class ParamToVisCollapseConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
		{
			return Visibility.Collapsed;
		}
		if (((string)values[1]).Contains("Object"))
		{
			return Visibility.Collapsed;
		}
		return (!(bool)values[0]) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object[] ConvertBack(object value, Type[] targetTypea, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
