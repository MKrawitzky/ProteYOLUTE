// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200004D RID: 77
	internal class MaintenanceCounterToVisibilityConverter : IMultiValueConverter
	{
		// Token: 0x06000421 RID: 1057 RVA: 0x00019280 File Offset: 0x00017480
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] is IResetable)
			{
				object obj = values[1];
				if (obj is bool && (bool)obj)
				{
					return Visibility.Visible;
				}
			}
			return Visibility.Collapsed;
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x00003996 File Offset: 0x00001B96
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
