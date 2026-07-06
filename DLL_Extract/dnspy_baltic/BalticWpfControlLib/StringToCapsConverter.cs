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
	// Token: 0x02000015 RID: 21
	public class StringToCapsConverter : IValueConverter
	{
		// Token: 0x0600008D RID: 141 RVA: 0x000039E8 File Offset: 0x00001BE8
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string s = value as string;
			if (s != null)
			{
				return s.ToUpper();
			}
			return value;
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
