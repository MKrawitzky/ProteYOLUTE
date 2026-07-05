using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200004E RID: 78
	internal class MaintenanceItemToVisibilityConverter : IMultiValueConverter
	{
		// Token: 0x06000424 RID: 1060 RVA: 0x000192B8 File Offset: 0x000174B8
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			object obj = values[0];
			if (!(obj is bool) || (bool)obj)
			{
				obj = values[1];
				if (!(obj is bool) || !(bool)obj)
				{
					return Visibility.Collapsed;
				}
			}
			return Visibility.Visible;
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
