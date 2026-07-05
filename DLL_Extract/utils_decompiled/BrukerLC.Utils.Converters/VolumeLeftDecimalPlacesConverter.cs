using System;
using System.Globalization;
using System.Windows.Data;

namespace BrukerLC.Utils.Converters;

public class VolumeLeftDecimalPlacesConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}
		double value2 = (double)System.Convert.ChangeType(value, typeof(double));
		double num = Math.Abs(value2);
		string text = ((num >= 1000.0) ? " 0;-0" : ((num >= 100.0) ? " 0.0;-0.0" : ((!(num >= 10.0)) ? " 0.000;-0.000" : " 0.00;-0.00")));
		string text2 = text;
		return value2.ToString(text2, CultureInfo.InvariantCulture);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
