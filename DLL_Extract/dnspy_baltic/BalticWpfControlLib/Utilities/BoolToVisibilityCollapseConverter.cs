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
	// Token: 0x0200005D RID: 93
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BoolToVisibilityCollapseConverter : IValueConverter
	{
		// Token: 0x0600049D RID: 1181 RVA: 0x0001AB60 File Offset: 0x00018D60
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Visibility ifFalse = Visibility.Collapsed;
			string s = parameter as string;
			if (s != null)
			{
				Enum.TryParse<Visibility>(s, out ifFalse);
			}
			else if (parameter is Visibility)
			{
				Visibility visibility = (Visibility)parameter;
				ifFalse = visibility;
			}
			return ((bool)value) ? Visibility.Visible : ifFalse;
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
