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
	// Token: 0x0200006C RID: 108
	public class PressureValueToVisConverter : IMultiValueConverter
	{
		// Token: 0x060004CA RID: 1226 RVA: 0x0001AF38 File Offset: 0x00019138
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values != null && values.Length >= 2)
			{
				object obj = values[0];
				if (obj is bool && !(bool)obj)
				{
					return Visibility.Collapsed;
				}
				obj = values[1];
				if (obj is int && (int)obj > 1000)
				{
					return Visibility.Visible;
				}
			}
			return Visibility.Collapsed;
		}

		// Token: 0x060004CB RID: 1227 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
