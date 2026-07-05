using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000067 RID: 103
	public class PanelWidthConverter : IValueConverter
	{
		// Token: 0x060004BB RID: 1211 RVA: 0x0001AD67 File Offset: 0x00018F67
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int)value * -1;
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
