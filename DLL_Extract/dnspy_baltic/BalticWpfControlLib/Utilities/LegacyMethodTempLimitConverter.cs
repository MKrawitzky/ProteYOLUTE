// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200006B RID: 107
	public class LegacyMethodTempLimitConverter : IValueConverter
	{
		// Token: 0x060004C7 RID: 1223 RVA: 0x0001AE80 File Offset: 0x00019080
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}
			double minTemp = 35.0;
			if (parameter != null)
			{
				minTemp = global::System.Convert.ToDouble(parameter as string);
			}
			double doubleValue = (double)global::System.Convert.ChangeType(value, typeof(double));
			if ((int)(doubleValue * 10.0) == 200)
			{
				doubleValue = minTemp;
			}
			if (parameter == null)
			{
				return doubleValue.ToString();
			}
			return doubleValue.ToString("0;-0");
		}

		// Token: 0x060004C8 RID: 1224 RVA: 0x0001AEF0 File Offset: 0x000190F0
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double minTemp = 35.0;
			if ((value as string).Length > 0)
			{
				return global::System.Convert.ToDouble(value).ToString("0;-0");
			}
			return minTemp.ToString("0;-0");
		}
	}
}
