// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x0200007A RID: 122
	public class PumpViewModel : ActivatableAndErrorAwareDataContext, IPumpViewModel
	{
		// Token: 0x17000129 RID: 297
		// (get) Token: 0x060005A1 RID: 1441 RVA: 0x000396D1 File Offset: 0x000378D1
		// (set) Token: 0x060005A2 RID: 1442 RVA: 0x000396D9 File Offset: 0x000378D9
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

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x060005A3 RID: 1443 RVA: 0x000396EE File Offset: 0x000378EE
		// (set) Token: 0x060005A4 RID: 1444 RVA: 0x000396F6 File Offset: 0x000378F6
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

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x060005A5 RID: 1445 RVA: 0x0003970B File Offset: 0x0003790B
		// (set) Token: 0x060005A6 RID: 1446 RVA: 0x00039713 File Offset: 0x00037913
		public double ThroughputSetPoint
		{
			get
			{
				return this._throughputSetPoint;
			}
			set
			{
				base.SetProperty<double>(ref this._throughputSetPoint, value, "ThroughputSetPoint");
			}
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x060005A7 RID: 1447 RVA: 0x00039728 File Offset: 0x00037928
		// (set) Token: 0x060005A8 RID: 1448 RVA: 0x00039730 File Offset: 0x00037930
		public double Pressure
		{
			get
			{
				return this._pressure;
			}
			set
			{
				base.SetProperty<double>(ref this._pressure, value, "Pressure");
			}
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x060005A9 RID: 1449 RVA: 0x00039745 File Offset: 0x00037945
		// (set) Token: 0x060005AA RID: 1450 RVA: 0x0003974D File Offset: 0x0003794D
		public string PressureUnit
		{
			get
			{
				return this._pressureUnit;
			}
			set
			{
				base.SetProperty<string>(ref this._pressureUnit, value, "PressureUnit");
			}
		}

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x060005AB RID: 1451 RVA: 0x00039762 File Offset: 0x00037962
		// (set) Token: 0x060005AC RID: 1452 RVA: 0x0003976A File Offset: 0x0003796A
		public double VolumeUsed
		{
			get
			{
				return this._volumeUsed;
			}
			set
			{
				base.SetProperty<double>(ref this._volumeUsed, value, "VolumeUsed");
			}
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x060005AD RID: 1453 RVA: 0x0003977F File Offset: 0x0003797F
		// (set) Token: 0x060005AE RID: 1454 RVA: 0x00039787 File Offset: 0x00037987
		public double VolumeLeft
		{
			get
			{
				return this._volumeLeft;
			}
			set
			{
				base.SetProperty<double>(ref this._volumeLeft, value, "VolumeLeft");
			}
		}

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x060005AF RID: 1455 RVA: 0x0003979C File Offset: 0x0003799C
		// (set) Token: 0x060005B0 RID: 1456 RVA: 0x000397A4 File Offset: 0x000379A4
		public string VolumeUnit
		{
			get
			{
				return this._volumeUnit;
			}
			set
			{
				base.SetProperty<string>(ref this._volumeUnit, value, "VolumeUnit");
			}
		}

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x060005B1 RID: 1457 RVA: 0x000397B9 File Offset: 0x000379B9
		// (set) Token: 0x060005B2 RID: 1458 RVA: 0x000397C1 File Offset: 0x000379C1
		public double FillLevel
		{
			get
			{
				return this._fillLevel;
			}
			set
			{
				base.SetProperty<double>(ref this._fillLevel, value, "FillLevel");
			}
		}

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x060005B3 RID: 1459 RVA: 0x000397D6 File Offset: 0x000379D6
		// (set) Token: 0x060005B4 RID: 1460 RVA: 0x000397DE File Offset: 0x000379DE
		public string Title
		{
			get
			{
				return this._title;
			}
			set
			{
				base.SetProperty<string>(ref this._title, value, "Title");
			}
		}

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x060005B5 RID: 1461 RVA: 0x000397F3 File Offset: 0x000379F3
		// (set) Token: 0x060005B6 RID: 1462 RVA: 0x000397FB File Offset: 0x000379FB
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

		// Token: 0x04000300 RID: 768
		private double _throughput;

		// Token: 0x04000301 RID: 769
		private string _throughputUnit;

		// Token: 0x04000302 RID: 770
		private double _throughputSetPoint;

		// Token: 0x04000303 RID: 771
		private double _pressure;

		// Token: 0x04000304 RID: 772
		private string _pressureUnit;

		// Token: 0x04000305 RID: 773
		private double _volumeUsed;

		// Token: 0x04000306 RID: 774
		private double _volumeLeft;

		// Token: 0x04000307 RID: 775
		private string _volumeUnit;

		// Token: 0x04000308 RID: 776
		private double _fillLevel;

		// Token: 0x04000309 RID: 777
		private string _title;

		// Token: 0x0400030A RID: 778
		private bool _isService;
	}
}
