using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000063 RID: 99
	public class BoolToHeightErrorConverter : IValueConverter
	{
		// Token: 0x060004AF RID: 1199 RVA: 0x0001ACBF File Offset: 0x00018EBF
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((bool)value) ? 18 : 16;
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
