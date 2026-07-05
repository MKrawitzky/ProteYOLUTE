using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

public class ActualWidthConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (double)value * -1.0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
