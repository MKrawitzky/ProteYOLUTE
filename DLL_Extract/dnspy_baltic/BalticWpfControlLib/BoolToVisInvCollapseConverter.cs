using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib
{
	// Token: 0x02000014 RID: 20
	public class BoolToVisInvCollapseConverter : IValueConverter
	{
		// Token: 0x0600008A RID: 138 RVA: 0x000039A0 File Offset: 0x00001BA0
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Visibility ifFalse = Visibility.Collapsed;
			string s = parameter as string;
			if (s != null)
			{
				Enum.TryParse<Visibility>(s, out ifFalse);
			}
			else if (parameter is Visibility)
			{
				Visibility visibility = (Visibility)parameter;
				ifFalse = visibility;
			}
			return ((bool)value) ? ifFalse : Visibility.Visible;
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
