using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000049 RID: 73
	internal class ColumnEquilTimeToStringConverter : IValueConverter
	{
		// Token: 0x060003F5 RID: 1013 RVA: 0x00018D68 File Offset: 0x00016F68
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double))
			{
				return value;
			}
			double time = (double)value;
			if (time <= 0.0 || double.IsInfinity(time) || double.IsNaN(time))
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}: --- min", parameter);
			}
			return string.Format(CultureInfo.InvariantCulture, "{0}: {1:0.00} min", parameter, time);
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
