using System;
using System.Globalization;
using System.Windows.Data;
using Bruker.Lc;

namespace BalticWpfControlLib;

public class AdvParameterValueConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is IChromatographyColumnType chromatographyColumnType)
		{
			if (targetType != typeof(string))
			{
				throw new InvalidCastException("Bruker.Lc.IChromatographyColumn can only be converted to System.String");
			}
			value = chromatographyColumnType.Name;
		}
		else if (value != null)
		{
			value = System.Convert.ChangeType(value, targetType, culture);
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}
