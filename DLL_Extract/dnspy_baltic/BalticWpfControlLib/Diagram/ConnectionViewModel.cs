using System;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x02000071 RID: 113
	public class ConnectionViewModel : ActivatableDataContext, IConnectionViewModel
	{
		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x06000507 RID: 1287 RVA: 0x0001B339 File Offset: 0x00019539
		// (set) Token: 0x06000508 RID: 1288 RVA: 0x0001B341 File Offset: 0x00019541
		public int FirstPort
		{
			get
			{
				return this._firstPort;
			}
			set
			{
				base.SetProperty<int>(ref this._firstPort, value, "FirstPort");
			}
		}

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x06000509 RID: 1289 RVA: 0x0001B356 File Offset: 0x00019556
		// (set) Token: 0x0600050A RID: 1290 RVA: 0x0001B35E File Offset: 0x0001955E
		public int SecondPort
		{
			get
			{
				return this._secondPort;
			}
			set
			{
				base.SetProperty<int>(ref this._secondPort, value, "SecondPort");
			}
		}

		// Token: 0x040002A1 RID: 673
		private int _firstPort;

		// Token: 0x040002A2 RID: 674
		private int _secondPort;
	}
}
