using System;
using System.Globalization;
using System.Windows.Data;

namespace BrukerLC.Utils.Converters;

public class VolumeDecimalPlacesConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}
		double num = (double)System.Convert.ChangeType(value, typeof(double));
		string text = (string)parameter;
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
