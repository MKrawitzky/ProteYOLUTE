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
	// Token: 0x02000056 RID: 86
	public class WidthToBorderThickConverter : IValueConverter
	{
		// Token: 0x0600048D RID: 1165 RVA: 0x0001A80F File Offset: 0x00018A0F
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value * 4.0 / 100.0;
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
