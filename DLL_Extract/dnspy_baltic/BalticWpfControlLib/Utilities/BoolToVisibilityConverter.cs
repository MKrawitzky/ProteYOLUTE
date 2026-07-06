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
	// Token: 0x0200005C RID: 92
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BoolToVisibilityConverter : IValueConverter
	{
		// Token: 0x0600049A RID: 1178 RVA: 0x0001AB18 File Offset: 0x00018D18
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Visibility ifFalse = Visibility.Hidden;
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

		// Token: 0x0600049B RID: 1179 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
