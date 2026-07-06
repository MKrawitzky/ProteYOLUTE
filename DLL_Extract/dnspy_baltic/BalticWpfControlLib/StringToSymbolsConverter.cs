// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace BalticWpfControlLib
{
	// Token: 0x02000012 RID: 18
	public class StringToSymbolsConverter : IValueConverter
	{
		// Token: 0x06000084 RID: 132 RVA: 0x00003924 File Offset: 0x00001B24
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string s = value as string;
			if (s != null)
			{
				return s.Replace("uL", "µL");
			}
			return value;
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00002A31 File Offset: 0x00000C31
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
