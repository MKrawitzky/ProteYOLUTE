using System;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x0200007B RID: 123
	public class SensorViewModel : ActivatableAndErrorAwareDataContext, ISensorViewModel
	{
		// Token: 0x17000134 RID: 308
		// (get) Token: 0x060005B8 RID: 1464 RVA: 0x00039810 File Offset: 0x00037A10
		// (set) Token: 0x060005B9 RID: 1465 RVA: 0x00039818 File Offset: 0x00037A18
		public double Throughput
		{
			get
			{
				return this._throughput;
			}
			set
			{
				base.SetProperty<double>(ref this._throughput, value, "Throughput");
			}
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x060005BA RID: 1466 RVA: 0x0003982D File Offset: 0x00037A2D
		// (set) Token: 0x060005BB RID: 1467 RVA: 0x00039835 File Offset: 0x00037A35
		public string ThroughputUnit
		{
			get
			{
				return this._throughputUnit;
			}
			set
			{
				base.SetProperty<string>(ref this._throughputUnit, value, "ThroughputUnit");
			}
		}

		// Token: 0x0400030B RID: 779
		private double _throughput;

		// Token: 0x0400030C RID: 780
		private string _throughputUnit;
	}
}
