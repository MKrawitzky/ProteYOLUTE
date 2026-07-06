// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Windows.Interop;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200004C RID: 76
	public interface IWin32OwnerWindow : IWin32Window
	{
		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x06000420 RID: 1056
		bool? IsMaximized { get; set; }
	}
}
