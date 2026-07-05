using System;
using System.Globalization;
using System.Windows.Data;

namespace BrukerLC.Utils.Converters;

public class DecimalPlacesConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}
		double num = (double)System.Convert.ChangeType(value, typeof(double));
		string text = (string)parameter;
		if (Math.Abs((int)num) >= 10)
		{
			text = " 0;-0";
		}
		if (parameter == null)
		{
			return num.ToString(CultureInfo.InvariantCulture);
		}
		return num.ToString(text, CultureInfo.InvariantCulture);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
