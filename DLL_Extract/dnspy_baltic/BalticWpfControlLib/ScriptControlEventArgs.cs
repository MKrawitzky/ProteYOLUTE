using System;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x0200003E RID: 62
	public class ScriptControlEventArgs : EventArgs
	{
		// Token: 0x06000376 RID: 886 RVA: 0x00016699 File Offset: 0x00014899
		public ScriptControlEventArgs(ProcedureInfo pInfo, ProcedureArguments pArgs, ChildProcedureArguments pChildArgs = null)
		{
			this.ProcedureSourceInfo = pInfo;
			this.ProcedureSourceArgs = pArgs;
			this.ProcedureSourceChildArgs = pChildArgs;
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x06000377 RID: 887 RVA: 0x000166B6 File Offset: 0x000148B6
		// (set) Token: 0x06000378 RID: 888 RVA: 0x000166BE File Offset: 0x000148BE
		public ProcedureInfo ProcedureSourceInfo { get; set; }

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x06000379 RID: 889 RVA: 0x000166C7 File Offset: 0x000148C7
		// (set) Token: 0x0600037A RID: 890 RVA: 0x000166CF File Offset: 0x000148CF
		public ProcedureArguments ProcedureSourceArgs { get; set; }

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x0600037B RID: 891 RVA: 0x000166D8 File Offset: 0x000148D8
		// (set) Token: 0x0600037C RID: 892 RVA: 0x000166E0 File Offset: 0x000148E0
		public ChildProcedureArguments ProcedureSourceChildArgs { get; set; }
	}
}
