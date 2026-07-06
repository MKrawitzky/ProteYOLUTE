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
	// Token: 0x02000069 RID: 105
	public class PanelDimensionsConverter : IMultiValueConverter
	{
		// Token: 0x060004C1 RID: 1217 RVA: 0x0001AD90 File Offset: 0x00018F90
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			double? totalWidth = new double?(0.0);
			for (int i = 0; i < values.Length; i++)
			{
				int current;
				if (int.TryParse(values[i].ToString(), out current))
				{
					totalWidth += (double)current;
				}
			}
			totalWidth *= (double)(-1);
			return totalWidth;
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
