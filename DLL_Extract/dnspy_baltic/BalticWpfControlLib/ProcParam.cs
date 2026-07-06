// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Collections.ObjectModel;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x02000033 RID: 51
	internal class ProcParam : BaseProcParam
	{
		// Token: 0x1700007A RID: 122
		// (get) Token: 0x0600030E RID: 782 RVA: 0x0001443B File Offset: 0x0001263B
		// (set) Token: 0x0600030F RID: 783 RVA: 0x00014443 File Offset: 0x00012643
		public ObservableCollection<ChildProcParam> ChildProcParams { get; set; } = new ObservableCollection<ChildProcParam>();

		// Token: 0x06000310 RID: 784 RVA: 0x0001444C File Offset: 0x0001264C
		public ProcParam(ProcedureParameter parameter, string imagePath, bool isAppService, ObservableCollection<ChildProcParam> childProcParams)
			: base(parameter, imagePath, isAppService)
		{
			foreach (ChildProcParam item in childProcParams)
			{
				this.ChildProcParams.Add(new ChildProcParam(item));
			}
		}

		// Token: 0x06000311 RID: 785 RVA: 0x000144B4 File Offset: 0x000126B4
		public ProcParam(ProcedureParameter parameter, ProcedureArgument argument, string imagePath, bool isAppService, ObservableCollection<ChildProcParam> childProcParams)
			: base(parameter, argument, imagePath, isAppService)
		{
			foreach (ChildProcParam item in childProcParams)
			{
				this.ChildProcParams.Add(new ChildProcParam(item));
			}
		}
	}
}
