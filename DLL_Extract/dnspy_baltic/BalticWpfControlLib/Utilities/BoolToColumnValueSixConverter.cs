// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000046 RID: 70
	public class BoolToColumnValueSixConverter : IMultiValueConverter
	{
		// Token: 0x060003EB RID: 1003 RVA: 0x00018C20 File Offset: 0x00016E20
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
			return string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", (double)values[1]);
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypea, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
