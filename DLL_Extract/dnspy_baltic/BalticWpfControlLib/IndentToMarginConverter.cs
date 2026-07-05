using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib
{
	// Token: 0x02000037 RID: 55
	public class IndentToMarginConverter : IValueConverter
	{
		// Token: 0x0600031A RID: 794 RVA: 0x000146FC File Offset: 0x000128FC
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int)
			{
				int margin = (int)value;
				return new Thickness((double)((margin < 4) ? 4 : margin), 4.0, 4.0, 4.0);
			}
			return null;
		}

		// Token: 0x0600031B RID: 795 RVA: 0x00002A31 File Offset: 0x00000C31
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
