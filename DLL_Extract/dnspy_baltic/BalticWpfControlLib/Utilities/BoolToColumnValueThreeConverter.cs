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
	// Token: 0x02000045 RID: 69
	public class BoolToColumnValueThreeConverter : IMultiValueConverter
	{
		// Token: 0x060003E8 RID: 1000 RVA: 0x00018BCC File Offset: 0x00016DCC
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
			return string.Format(CultureInfo.InvariantCulture, "{0:0.000}", (double)values[1]);
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypea, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
