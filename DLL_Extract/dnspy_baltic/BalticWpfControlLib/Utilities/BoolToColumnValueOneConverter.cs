using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000044 RID: 68
	public class BoolToColumnValueOneConverter : IMultiValueConverter
	{
		// Token: 0x060003E5 RID: 997 RVA: 0x00018B78 File Offset: 0x00016D78
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
			{
				return "";
			}
			if (!(bool)values[0])
			{
				return "    ";
			}
			return string.Format(CultureInfo.InvariantCulture, "{0:0.0}", (double)values[1]);
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypea, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
