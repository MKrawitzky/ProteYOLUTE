using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200005A RID: 90
	[ValueConversion(typeof(object), typeof(string))]
	public sealed class ObjectToStringConverter : IValueConverter
	{
		// Token: 0x06000494 RID: 1172 RVA: 0x0001A93C File Offset: 0x00018B3C
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}
			if (value is double)
			{
				double d = (double)value;
				return string.Format(CultureInfo.InvariantCulture, "{0:0.00}", d);
			}
			return value.ToString();
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
