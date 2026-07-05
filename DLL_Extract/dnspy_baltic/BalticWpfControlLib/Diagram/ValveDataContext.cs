using System;
using System.Collections.ObjectModel;
using System.Windows;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x0200007F RID: 127
	public class ValveDataContext : ActivatableAndErrorAwareDataContext, IValveViewModel
	{
		// Token: 0x1700014D RID: 333
		// (get) Token: 0x060005F0 RID: 1520 RVA: 0x00039C5D File Offset: 0x00037E5D
		// (set) Token: 0x060005F1 RID: 1521 RVA: 0x00039C65 File Offset: 0x00037E65
		public ObservableCollection<IConnectionViewModel> Connections
		{
			get
			{
				return this._connections;
			}
			set
			{
				base.SetProperty<ObservableCollection<IConnectionViewModel>>(ref this._connections, value, "Connections");
			}
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x060005F2 RID: 1522 RVA: 0x00039C7A File Offset: 0x00037E7A
		// (set) Token: 0x060005F3 RID: 1523 RVA: 0x00039C82 File Offset: 0x00037E82
		public double Angle
		{
			get
			{
				return this._angle;
			}
			set
			{
				base.SetProperty<double>(ref this._angle, value, "Angle");
			}
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x060005F4 RID: 1524 RVA: 0x00039C97 File Offset: 0x00037E97
		// (set) Token: 0x060005F5 RID: 1525 RVA: 0x00039C9F File Offset: 0x00037E9F
		public double ActualAngle
		{
			get
			{
				return this._actualAngle;
			}
			set
			{
				base.SetProperty<double>(ref this._actualAngle, value, "ActualAngle");
			}
		}

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x060005F6 RID: 1526 RVA: 0x00039CB4 File Offset: 0x00037EB4
		// (set) Token: 0x060005F7 RID: 1527 RVA: 0x00039CBC File Offset: 0x00037EBC
		public Thickness AngleMargin
		{
			get
			{
				return this._angleMargin;
			}
			set
			{
				base.SetProperty<Thickness>(ref this._angleMargin, value, "AngleMargin");
			}
		}

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x060005F8 RID: 1528 RVA: 0x00039CD1 File Offset: 0x00037ED1
		// (set) Token: 0x060005F9 RID: 1529 RVA: 0x00039CD9 File Offset: 0x00037ED9
		public bool IsService
		{
			get
			{
				return this._isService;
			}
			set
			{
				base.SetProperty<bool>(ref this._isService, value, "IsService");
			}
		}

		// Token: 0x04000326 RID: 806
		private ObservableCollection<IConnectionViewModel> _connections = new ObservableCollection<IConnectionViewModel>();

		// Token: 0x04000327 RID: 807
		private double _angle;

		// Token: 0x04000328 RID: 808
		private double _actualAngle;

		// Token: 0x04000329 RID: 809
		private Thickness _angleMargin;

		// Token: 0x0400032A RID: 810
		private bool _isService;
	}
}
