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
	// Token: 0x02000061 RID: 97
	public class BoolToGroupEndMarginConverter : IValueConverter
	{
		// Token: 0x060004A9 RID: 1193 RVA: 0x0001AC3C File Offset: 0x00018E3C
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((bool)value) ? new Thickness(0.0, 4.0, 4.0, 6.0) : new Thickness(0.0, 4.0, 4.0, 0.0);
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
