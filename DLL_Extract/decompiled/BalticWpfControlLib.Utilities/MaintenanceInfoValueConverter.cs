// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib.Utilities;

[ValueConversion(typeof(IMaintenanceInfoItem), typeof(string))]
public sealed class MaintenanceInfoValueConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values == null || values[0] == null)
		{
			return Binding.DoNothing;
		}
		object obj = values[0];
		string text = values[1] as string;
		bool flag = text != null && text.Length > 0;
		if (flag)
		{
			bool flag2 = ((obj is double || obj is float) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:" + text + "}", obj);
		}
		object obj2 = obj;
		if (!(obj2 is double num))
		{
			if (obj2 is float num2)
			{
				if ((int)(num2 * 10f) != 0)
				{
					if (num2 >= 100f)
					{
						float num3 = num2;
						return string.Format(CultureInfo.InvariantCulture, "{0:0.00}", num3);
					}
					float num4 = num2;
					return string.Format(CultureInfo.InvariantCulture, "{0:0.000#}", num4);
				}
				return string.Format(CultureInfo.InvariantCulture, "{0:0.00000}", num2);
			}
			return obj.ToString();
		}
		if ((int)(num * 10.0) != 0)
		{
			if (num >= 100.0)
			{
				double num5 = num;
				return string.Format(CultureInfo.InvariantCulture, "{0:0.00}", num5);
			}
			double num6 = num;
			return string.Format(CultureInfo.InvariantCulture, "{0:0.000#}", num6);
		}
		return string.Format(CultureInfo.InvariantCulture, "{0:0.00000}", num);
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
