// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x02000017 RID: 23
	public class AdvChildValidationRequestEventArgs : ScriptControlEventArgs
	{
		// Token: 0x060000A0 RID: 160 RVA: 0x000043AF File Offset: 0x000025AF
		public AdvChildValidationRequestEventArgs(ProcedureInfo procedure, ProcedureArguments arguments, ChildProcedureArguments childArguments)
			: base(procedure, arguments, childArguments)
		{
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x000043C5 File Offset: 0x000025C5
		public void AddReport(ProcedureReportEventArgs report)
		{
			this._reports.Add(report);
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000A2 RID: 162 RVA: 0x000043D3 File Offset: 0x000025D3
		public IEnumerable<ProcedureReportEventArgs> Reports
		{
			get
			{
				return this._reports;
			}
		}

		// Token: 0x0400004C RID: 76
		private readonly List<ProcedureReportEventArgs> _reports = new List<ProcedureReportEventArgs>();
	}
}
