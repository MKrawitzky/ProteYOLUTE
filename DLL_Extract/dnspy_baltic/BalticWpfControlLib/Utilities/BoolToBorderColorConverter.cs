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
	// Token: 0x02000060 RID: 96
	public class BoolToBorderColorConverter : IValueConverter
	{
		// Token: 0x060004A6 RID: 1190 RVA: 0x0001AC1B File Offset: 0x00018E1B
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(bool)value)
			{
				return new SolidColorBrush(Colors.Red);
			}
			return new SolidColorBrush(Colors.LightGray);
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
