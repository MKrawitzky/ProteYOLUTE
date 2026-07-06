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
	// Token: 0x02000036 RID: 54
	public class ToolTipTextToVisibilityConverter : IValueConverter
	{
		// Token: 0x06000317 RID: 791 RVA: 0x000146D0 File Offset: 0x000128D0
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string text = value as string;
			return (text != null && text.Length > 0) ? Visibility.Visible : Visibility.Collapsed;
		}

		// Token: 0x06000318 RID: 792 RVA: 0x00002A31 File Offset: 0x00000C31
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
