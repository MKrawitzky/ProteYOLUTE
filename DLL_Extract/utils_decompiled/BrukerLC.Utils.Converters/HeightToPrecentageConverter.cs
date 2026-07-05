using System;
using System.Globalization;
using System.Windows.Data;

namespace BrukerLC.Utils.Converters;

public class HeightToPrecentageConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}
		if (parameter == null)
		{
			return null;
		}
		double result = 0.0;
		double.TryParse(value.ToString(), out result);
		double result2 = 0.0;
		double.TryParse(parameter.ToString(), out result2);
		return result2 * result / 100.0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
