// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace BalticWpfControlLib.Properties
{
	// Token: 0x0200006E RID: 110
	internal sealed partial class Settings : ApplicationSettingsBase
	{
		// Token: 0x170000DD RID: 221
		// (get) Token: 0x060004D2 RID: 1234 RVA: 0x0001AFE5 File Offset: 0x000191E5
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x060004D5 RID: 1237 RVA: 0x0001B00C File Offset: 0x0001920C
		// (set) Token: 0x060004D6 RID: 1238 RVA: 0x0001B01E File Offset: 0x0001921E
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("900, 567")]
		public Size LCControlSize
		{
			get
			{
				return (Size)this["LCControlSize"];
			}
			set
			{
				this["LCControlSize"] = value;
			}
		}

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x060004D7 RID: 1239 RVA: 0x0001B031 File Offset: 0x00019231
		// (set) Token: 0x060004D8 RID: 1240 RVA: 0x0001B043 File Offset: 0x00019243
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("0, 0")]
		public Point LCControlPos
		{
			get
			{
				return (Point)this["LCControlPos"];
			}
			set
			{
				this["LCControlPos"] = value;
			}
		}

		// Token: 0x0400028E RID: 654
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
	}
}
