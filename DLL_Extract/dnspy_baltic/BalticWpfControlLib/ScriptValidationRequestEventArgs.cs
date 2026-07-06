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
	// Token: 0x0200003D RID: 61
	public class ScriptValidationRequestEventArgs : ScriptControlEventArgs
	{
		// Token: 0x06000373 RID: 883 RVA: 0x0001666D File Offset: 0x0001486D
		public ScriptValidationRequestEventArgs(ProcedureInfo procedure, ProcedureArguments arguments, ChildProcedureArguments childArguments)
			: base(procedure, arguments, childArguments)
		{
		}

		// Token: 0x06000374 RID: 884 RVA: 0x00016683 File Offset: 0x00014883
		public void AddReport(ProcedureReportEventArgs report)
		{
			this._reports.Add(report);
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x06000375 RID: 885 RVA: 0x00016691 File Offset: 0x00014891
		public IEnumerable<ProcedureReportEventArgs> Reports
		{
			get
			{
				return this._reports;
			}
		}

		// Token: 0x0400020D RID: 525
		private readonly List<ProcedureReportEventArgs> _reports = new List<ProcedureReportEventArgs>();
	}
}
