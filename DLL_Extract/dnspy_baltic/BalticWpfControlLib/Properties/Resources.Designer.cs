// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace BalticWpfControlLib.Properties
{
	// Token: 0x0200006D RID: 109
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x060004CD RID: 1229 RVA: 0x00002A34 File Offset: 0x00000C34
		internal Resources()
		{
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x060004CE RID: 1230 RVA: 0x0001AF8F File Offset: 0x0001918F
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("BalticWpfControlLib.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x060004CF RID: 1231 RVA: 0x0001AFBB File Offset: 0x000191BB
		// (set) Token: 0x060004D0 RID: 1232 RVA: 0x0001AFC2 File Offset: 0x000191C2
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x060004D1 RID: 1233 RVA: 0x0001AFCA File Offset: 0x000191CA
		internal static Bitmap blue_dot
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("blue_dot", Resources.resourceCulture);
			}
		}

		// Token: 0x0400028C RID: 652
		private static ResourceManager resourceMan;

		// Token: 0x0400028D RID: 653
		private static CultureInfo resourceCulture;
	}
}
