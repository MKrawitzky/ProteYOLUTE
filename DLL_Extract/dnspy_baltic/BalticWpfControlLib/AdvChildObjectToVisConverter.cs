// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BalticWpfControlLib
{
	// Token: 0x0200000B RID: 11
	public class AdvChildObjectToVisConverter : IValueConverter
	{
		// Token: 0x06000037 RID: 55 RVA: 0x00002A3C File Offset: 0x00000C3C
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (((string)value).Contains("Object"))
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00002A31 File Offset: 0x00000C31
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
