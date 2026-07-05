using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200005F RID: 95
	public class BoolToBorderThicknessConverter : IValueConverter
	{
		// Token: 0x060004A3 RID: 1187 RVA: 0x0001ABEE File Offset: 0x00018DEE
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((bool)value) ? new Thickness(0.0) : new Thickness(1.0);
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
