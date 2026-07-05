using System;
using System.Collections.Generic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x02000019 RID: 25
	public class AdvValidationRequestEventArgs : ScriptControlEventArgs
	{
		// Token: 0x060000B6 RID: 182 RVA: 0x000051F4 File Offset: 0x000033F4
		public AdvValidationRequestEventArgs(ProcedureInfo procedure, ProcedureArguments arguments, ChildProcedureArguments childArguments)
			: base(procedure, arguments, childArguments)
		{
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x0000520A File Offset: 0x0000340A
		public void AddReport(ProcedureReportEventArgs report)
		{
			this._reports.Add(report);
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000B8 RID: 184 RVA: 0x00005218 File Offset: 0x00003418
		public IEnumerable<ProcedureReportEventArgs> Reports
		{
			get
			{
				return this._reports;
			}
		}

		// Token: 0x04000061 RID: 97
		private readonly List<ProcedureReportEventArgs> _reports = new List<ProcedureReportEventArgs>();
	}
}
