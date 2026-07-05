using System;
using System.Globalization;
using System.Windows.Data;

namespace BrukerLC.Utils.Converters;

public class OvenDecimalPlacesConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}
		double num = (double)System.Convert.ChangeType(value, typeof(double));
		string text = (string)parameter;
		if ((int)(num * 10.0) > 200)
		{
			return num.ToString(text, CultureInfo.InvariantCulture);
		}
		return " -";
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
