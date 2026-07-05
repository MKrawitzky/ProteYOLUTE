using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BalticWpfControlLib
{
	// Token: 0x02000024 RID: 36
	public sealed class ImageConverter : IValueConverter
	{
		// Token: 0x06000192 RID: 402 RVA: 0x0000AE44 File Offset: 0x00009044
		public object Convert(object value)
		{
			object obj;
			try
			{
				obj = new BitmapImage(new Uri((string)value, UriKind.Relative));
			}
			catch
			{
				obj = new BitmapImage();
			}
			return obj;
		}

		// Token: 0x06000193 RID: 403 RVA: 0x0000AE80 File Offset: 0x00009080
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				string uriString = value as string;
				if (uriString != null)
				{
					return new BitmapImage(new Uri(uriString, UriKind.Relative));
				}
			}
			catch
			{
			}
			return new BitmapImage();
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
