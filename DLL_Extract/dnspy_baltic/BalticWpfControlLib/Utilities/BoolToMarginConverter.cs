using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000064 RID: 100
	public class BoolToMarginConverter : IValueConverter
	{
		// Token: 0x060004B2 RID: 1202 RVA: 0x0001ACD4 File Offset: 0x00018ED4
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((bool)value) ? new Thickness(1.0, 1.0, 1.0, 1.0) : new Thickness(0.0, 1.0, 1.0, 1.0);
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
