// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000065 RID: 101
	public class BoolToForegroundConverter : IValueConverter
	{
		// Token: 0x060004B5 RID: 1205 RVA: 0x0001AD42 File Offset: 0x00018F42
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(bool)value)
			{
				return Brushes.Black;
			}
			return Brushes.Transparent;
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
