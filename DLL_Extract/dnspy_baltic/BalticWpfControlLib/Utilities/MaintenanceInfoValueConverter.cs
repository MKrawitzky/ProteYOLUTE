// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Windows.Data;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200005B RID: 91
	[ValueConversion(typeof(IMaintenanceInfoItem), typeof(string))]
	public sealed class MaintenanceInfoValueConverter : IMultiValueConverter
	{
		// Token: 0x06000497 RID: 1175 RVA: 0x0001A97C File Offset: 0x00018B7C
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values == null || values[0] == null)
			{
				return Binding.DoNothing;
			}
			object value = values[0];
			string format = values[1] as string;
			bool flag = format != null && format.Length > 0;
			if (flag)
			{
				bool flag2 = value is double || value is float;
				flag = flag2;
			}
			if (flag)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", value);
			}
			object obj = value;
			string text;
			if (obj is double)
			{
				double d = (double)obj;
				if ((int)(d * 10.0) == 0)
				{
					text = string.Format(CultureInfo.InvariantCulture, "{0:0.00000}", d);
				}
				else if (d < 100.0)
				{
					double d2 = d;
					text = string.Format(CultureInfo.InvariantCulture, "{0:0.000#}", d2);
				}
				else
				{
					double d3 = d;
					text = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", d3);
				}
			}
			else if (obj is float)
			{
				float f = (float)obj;
				if ((int)(f * 10f) == 0)
				{
					text = string.Format(CultureInfo.InvariantCulture, "{0:0.00000}", f);
				}
				else if (f < 100f)
				{
					float f2 = f;
					text = string.Format(CultureInfo.InvariantCulture, "{0:0.000#}", f2);
				}
				else
				{
					float f3 = f;
					text = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", f3);
				}
			}
			else
			{
				text = value.ToString();
			}
			return text;
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
