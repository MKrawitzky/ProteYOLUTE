using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BrukerLC.Utils.Converters;

public class EqualityToBooleanConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		UIElement uIElement = values[1] as UIElement;
		return values[0].ToString().Equals(uIElement.GetHashCode().ToString());
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
