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
	// Token: 0x02000013 RID: 19
	public class BoolToVisCollapseConverter : IValueConverter
	{
		// Token: 0x06000087 RID: 135 RVA: 0x00003950 File Offset: 0x00001B50
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

		// Token: 0x06000088 RID: 136 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
