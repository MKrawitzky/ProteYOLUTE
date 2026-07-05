using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BalticWpfControlLib;

public sealed class MyImageConverter : IValueConverter
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
			return new BitmapImage(new Uri((string)value, UriKind.Relative));
		}
		catch
		{
			return new BitmapImage();
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
