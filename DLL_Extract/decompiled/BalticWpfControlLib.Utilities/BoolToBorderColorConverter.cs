// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BalticWpfControlLib.Utilities;

public class BoolToBorderColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(bool)value)
		{
			return new SolidColorBrush(Colors.Red);
		}
		return new SolidColorBrush(Colors.LightGray);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
