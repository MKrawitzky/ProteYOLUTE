// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BalticWpfControlLib
{
	// Token: 0x0200003B RID: 59
	public sealed class MyImageConverter : IValueConverter
	{
		// Token: 0x0600035A RID: 858 RVA: 0x00015F80 File Offset: 0x00014180
		public object Convert(object value)
		{
			object obj;
			try
			{
				obj = new BitmapImage(new Uri((string)value, UriKind.Relative));
			}
			catch
			{
				obj = new BitmapImage();
			}
			return obj;
		}

		// Token: 0x0600035B RID: 859 RVA: 0x00015FBC File Offset: 0x000141BC
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			object obj;
			try
			{
				obj = new BitmapImage(new Uri((string)value, UriKind.Relative));
			}
			catch
			{
				obj = new BitmapImage();
			}
			return obj;
		}

		// Token: 0x0600035C RID: 860 RVA: 0x00003996 File Offset: 0x00001B96
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
