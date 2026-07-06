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
	// Token: 0x02000047 RID: 71
	public class PopupWidthConverter : IMultiValueConverter
	{
		// Token: 0x060003EE RID: 1006 RVA: 0x00018C73 File Offset: 0x00016E73
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
			{
				return 0.0;
			}
			return (double)values[1] - (double)values[0];
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypea, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
