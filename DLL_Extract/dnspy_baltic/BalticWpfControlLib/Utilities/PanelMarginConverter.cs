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
	// Token: 0x0200006A RID: 106
	public class PanelMarginConverter : IValueConverter
	{
		// Token: 0x060004C4 RID: 1220 RVA: 0x0001AE2C File Offset: 0x0001902C
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double)
			{
				double width = (double)value;
				return new Thickness(0.0, 0.0, 0.0, width * -1.0);
			}
			throw new NotImplementedException();
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
