using System;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x02000075 RID: 117
	public class InjectionViewModel : ActivatableAndErrorAwareDataContext, IInjectionViewModel
	{
		// Token: 0x17000106 RID: 262
		// (get) Token: 0x06000553 RID: 1363 RVA: 0x00038E78 File Offset: 0x00037078
		// (set) Token: 0x06000554 RID: 1364 RVA: 0x00038E80 File Offset: 0x00037080
		public string Label
		{
			get
			{
				return this._label;
			}
			set
			{
				base.SetProperty<string>(ref this._label, value, "Label");
			}
		}

		// Token: 0x040002D9 RID: 729
		private string _label;
	}
}
