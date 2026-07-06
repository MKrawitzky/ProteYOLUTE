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
	// Token: 0x0200005E RID: 94
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BoolToVisibilityCollapseInvConverter : IValueConverter
	{
		// Token: 0x060004A0 RID: 1184 RVA: 0x0001ABA8 File Offset: 0x00018DA8
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
			return ((bool)value) ? ifFalse : Visibility.Visible;
		}

		// Token: 0x060004A1 RID: 1185 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
