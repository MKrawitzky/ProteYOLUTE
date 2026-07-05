using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000068 RID: 104
	public class ActualWidthConverter : IValueConverter
	{
		// Token: 0x060004BE RID: 1214 RVA: 0x0001AD76 File Offset: 0x00018F76
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value * -1.0;
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
