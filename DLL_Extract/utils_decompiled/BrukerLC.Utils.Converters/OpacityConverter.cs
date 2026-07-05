using System;
using System.Globalization;
using System.Windows.Data;

namespace BrukerLC.Utils.Converters;

public class OpacityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}
		return ((bool)value) ? 0.3 : 1.0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
