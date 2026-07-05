using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BrukerLC.Utils.Converters;

public class ColorOpacityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}
		double result = 0.0;
		if (parameter == null)
		{
			result = 0.2;
		}
		else
		{
			double.TryParse(parameter.ToString(), out result);
		}
		return new SolidColorBrush
		{
			Color = ((SolidColorBrush)value).Color,
			Opacity = result
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
