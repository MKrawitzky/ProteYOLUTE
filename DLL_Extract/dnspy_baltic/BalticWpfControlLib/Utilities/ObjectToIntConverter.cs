using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000052 RID: 82
	internal class ObjectToIntConverter : IValueConverter
	{
		// Token: 0x06000474 RID: 1140 RVA: 0x0001A2BE File Offset: 0x000184BE
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int?)value;
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x0001A2CB File Offset: 0x000184CB
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
}
