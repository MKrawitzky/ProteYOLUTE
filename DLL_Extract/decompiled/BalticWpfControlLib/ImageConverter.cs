// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BalticWpfControlLib;

public sealed class ImageConverter : IValueConverter
{
	public object Convert(object value)
	{
		try
		{
			return new BitmapImage(new Uri((string)value, UriKind.Relative));
		}
		catch
		{
			return new BitmapImage();
		}
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			if (value is string uriString)
			{
				return new BitmapImage(new Uri(uriString, UriKind.Relative));
			}
		}
		catch
		{
		}
		return new BitmapImage();
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
