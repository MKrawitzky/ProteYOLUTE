using System;
using System.Collections.Generic;

namespace BalticWpfControlLib
{
	// Token: 0x02000026 RID: 38
	[Serializable]
	public class LCUserControlSettings
	{
		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600019A RID: 410 RVA: 0x0000AF11 File Offset: 0x00009111
		// (set) Token: 0x0600019B RID: 411 RVA: 0x0000AF19 File Offset: 0x00009119
		public List<string> EnabledTracesList { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600019C RID: 412 RVA: 0x0000AF22 File Offset: 0x00009122
		// (set) Token: 0x0600019D RID: 413 RVA: 0x0000AF2A File Offset: 0x0000912A
		public bool IsDiagnosticTracesSelected { get; set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600019E RID: 414 RVA: 0x0000AF33 File Offset: 0x00009133
		// (set) Token: 0x0600019F RID: 415 RVA: 0x0000AF3B File Offset: 0x0000913B
		public string ChartGridWidthLeft { get; set; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060001A0 RID: 416 RVA: 0x0000AF44 File Offset: 0x00009144
		// (set) Token: 0x060001A1 RID: 417 RVA: 0x0000AF4C File Offset: 0x0000914C
		public string ChartGridWidthRight { get; set; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x0000AF55 File Offset: 0x00009155
		// (set) Token: 0x060001A3 RID: 419 RVA: 0x0000AF5D File Offset: 0x0000915D
		public string MainGridHeightTopRow { get; set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060001A4 RID: 420 RVA: 0x0000AF66 File Offset: 0x00009166
		// (set) Token: 0x060001A5 RID: 421 RVA: 0x0000AF6E File Offset: 0x0000916E
		public string MainGridHeightBottomRow { get; set; }
	}
}
