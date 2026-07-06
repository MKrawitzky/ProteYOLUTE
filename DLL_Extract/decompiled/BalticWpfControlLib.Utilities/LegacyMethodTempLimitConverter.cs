// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities;

public class LegacyMethodTempLimitConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}
		double num = 35.0;
		if (parameter != null)
		{
			num = System.Convert.ToDouble(parameter as string);
		}
		double num2 = (double)System.Convert.ChangeType(value, typeof(double));
		if ((int)(num2 * 10.0) == 200)
		{
			num2 = num;
		}
		if (parameter == null)
		{
			return num2.ToString();
		}
		return num2.ToString("0;-0");
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double num = 35.0;
		if ((value as string).Length > 0)
		{
			return System.Convert.ToDouble(value).ToString("0;-0");
		}
		return num.ToString("0;-0");
	}
}
