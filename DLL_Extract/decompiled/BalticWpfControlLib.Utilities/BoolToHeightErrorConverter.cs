using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

public class BoolToHeightErrorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ((bool)value) ? 18 : 16;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
