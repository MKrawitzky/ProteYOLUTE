// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace BalticWpfControlLib
{
	// Token: 0x0200001E RID: 30
	public class ContainerTemplatableItemsControl : ItemsControl
	{
		// Token: 0x060000ED RID: 237 RVA: 0x00006440 File Offset: 0x00004640
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ContentControl();
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00006447 File Offset: 0x00004647
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return false;
		}
	}
}
