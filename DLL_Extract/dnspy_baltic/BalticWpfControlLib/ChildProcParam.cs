using System;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x02000032 RID: 50
	internal class ChildProcParam : BaseProcParam
	{
		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600030A RID: 778 RVA: 0x000143F6 File Offset: 0x000125F6
		public string Header { get; private set; }

		// Token: 0x0600030B RID: 779 RVA: 0x000143FE File Offset: 0x000125FE
		public ChildProcParam(string header, ProcedureParameter parameter, string imagePath, bool isAppService)
			: base(parameter, imagePath, isAppService)
		{
			this.Header = header;
		}

		// Token: 0x0600030C RID: 780 RVA: 0x00014411 File Offset: 0x00012611
		public ChildProcParam(string header, ProcedureParameter parameter, ProcedureArgument argument, string imagePath, bool isAppService)
			: base(parameter, argument, imagePath, isAppService)
		{
			this.Header = header;
		}

		// Token: 0x0600030D RID: 781 RVA: 0x00014426 File Offset: 0x00012626
		public ChildProcParam(ChildProcParam item)
			: base(item)
		{
			this.Header = item.Header;
		}
	}
}
