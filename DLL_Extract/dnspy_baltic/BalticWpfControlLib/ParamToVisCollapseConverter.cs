using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib
{
	// Token: 0x02000038 RID: 56
	public class ParamToVisCollapseConverter : IMultiValueConverter
	{
		// Token: 0x0600031D RID: 797 RVA: 0x00014748 File Offset: 0x00012948
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
			{
				return Visibility.Collapsed;
			}
			if (((string)values[1]).Contains("Object"))
			{
				return Visibility.Collapsed;
			}
			return ((bool)values[0]) ? Visibility.Visible : Visibility.Collapsed;
		}

		// Token: 0x0600031E RID: 798 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypea, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
