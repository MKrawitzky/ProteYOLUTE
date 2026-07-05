using System;
using System.Globalization;
using System.Windows.Data;
using Bruker.Lc;

namespace BalticWpfControlLib
{
	// Token: 0x02000035 RID: 53
	public class ParameterValueConverter : IValueConverter
	{
		// Token: 0x06000314 RID: 788 RVA: 0x00014680 File Offset: 0x00012880
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			IChromatographyColumnType type = value as IChromatographyColumnType;
			if (type != null)
			{
				if (targetType != typeof(string))
				{
					throw new InvalidCastException("Bruker.Lc.IChromatographyColumn can only be converted to System.String");
				}
				value = type.Name;
			}
			else if (value != null)
			{
				value = global::System.Convert.ChangeType(value, targetType, culture);
			}
			return value;
		}

		// Token: 0x06000315 RID: 789 RVA: 0x00002A31 File Offset: 0x00000C31
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
