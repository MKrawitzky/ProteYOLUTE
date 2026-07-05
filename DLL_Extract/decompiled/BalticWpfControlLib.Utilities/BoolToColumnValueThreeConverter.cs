using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

public class BoolToColumnValueThreeConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
		{
			return "";
		}
		if (!(bool)values[0])
		{
			return "    ";
		}
		return string.Format(CultureInfo.InvariantCulture, "{0:0.000}", (double)values[1]);
	}

	public object[] ConvertBack(object value, Type[] targetTypea, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
