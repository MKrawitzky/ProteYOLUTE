// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Markup;

namespace BalticWpfControlLib
{
	// Token: 0x02000025 RID: 37
	[ValueConversion(typeof(string), typeof(string))]
	public class SubjectListConverter : MarkupExtension, IValueConverter
	{
		// Token: 0x06000196 RID: 406 RVA: 0x0000AEC4 File Offset: 0x000090C4
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}

		// Token: 0x06000197 RID: 407 RVA: 0x0000AEC8 File Offset: 0x000090C8
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				string input = value.ToString();
				return this._regex.Replace(input, string.Empty).Trim();
			}
			return null;
		}

		// Token: 0x06000198 RID: 408 RVA: 0x00002A31 File Offset: 0x00000C31
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}

		// Token: 0x040000E2 RID: 226
		private readonly Regex _regex = new Regex("@\\S+\\s*");
	}
}
