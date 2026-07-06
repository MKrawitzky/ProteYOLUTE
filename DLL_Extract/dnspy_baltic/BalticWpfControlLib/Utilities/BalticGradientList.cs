// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BalticClassLib;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200004F RID: 79
	public class BalticGradientList : ObservableCollection<BalticGradientItem>
	{
		// Token: 0x06000427 RID: 1063 RVA: 0x000192FC File Offset: 0x000174FC
		public IEnumerable<BalticMethod.GradientItem> ToGradientList()
		{
			List<BalticMethod.GradientItem> gradientList = new List<BalticMethod.GradientItem>();
			foreach (BalticGradientItem item in this)
			{
				gradientList.Add(new BalticMethod.GradientItem(item.Time * 60.0, item.Flow / 1000.0, item.Composition / 100.0));
			}
			return gradientList;
		}
	}
}
