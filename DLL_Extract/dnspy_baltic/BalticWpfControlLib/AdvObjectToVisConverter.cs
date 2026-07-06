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
	// Token: 0x02000011 RID: 17
	public class AdvObjectToVisConverter : IValueConverter
	{
		// Token: 0x06000081 RID: 129 RVA: 0x00002A3C File Offset: 0x00000C3C
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (((string)value).Contains("Object"))
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00002A31 File Offset: 0x00000C31
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
