using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

public class PanelDimensionsConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		double? num = 0.0;
		for (int i = 0; i < values.Length; i++)
		{
			if (int.TryParse(values[i].ToString(), out var result))
			{
				num += (double)result;
			}
		}
		num *= -1.0;
		return num;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
