// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Windows.Data;
using Bruker.Lc;

namespace BalticWpfControlLib
{
	// Token: 0x02000010 RID: 16
	public class AdvParameterValueConverter : IValueConverter
	{
		// Token: 0x0600007E RID: 126 RVA: 0x000038D4 File Offset: 0x00001AD4
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			IChromatographyColumnType type = value as IChromatographyColumnType;
			if (type != null)
			{
				if (targetType != typeof(string))
				{
					throw new InvalidCastException("Bruker.Lc.IChromatographyColumn can only be converted to System.String");
				}
				value = type.Name;
			}
			else if (value != null)
			{
				value = global::System.Convert.ChangeType(value, targetType, culture);
			}
			return value;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00002A31 File Offset: 0x00000C31
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
