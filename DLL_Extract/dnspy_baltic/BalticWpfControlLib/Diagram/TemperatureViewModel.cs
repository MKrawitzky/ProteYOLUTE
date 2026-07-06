// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x0200007D RID: 125
	public class TemperatureViewModel : ActivatableAndErrorAwareDataContext, ITemperatureViewModel
	{
		// Token: 0x17000139 RID: 313
		// (get) Token: 0x060005C5 RID: 1477 RVA: 0x000398C8 File Offset: 0x00037AC8
		// (set) Token: 0x060005C6 RID: 1478 RVA: 0x000398D0 File Offset: 0x00037AD0
		public double Temperature
		{
			get
			{
				return this._temperature;
			}
			set
			{
				base.SetProperty<double>(ref this._temperature, value, "Temperature");
			}
		}

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x060005C7 RID: 1479 RVA: 0x000398E5 File Offset: 0x00037AE5
		// (set) Token: 0x060005C8 RID: 1480 RVA: 0x000398ED File Offset: 0x00037AED
		public string TemperatureUnit
		{
			get
			{
				return this._temperatureUnit;
			}
			set
			{
				base.SetProperty<string>(ref this._temperatureUnit, value, "TemperatureUnit");
			}
		}

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x060005C9 RID: 1481 RVA: 0x00039902 File Offset: 0x00037B02
		// (set) Token: 0x060005CA RID: 1482 RVA: 0x0003990A File Offset: 0x00037B0A
		public double TemperatureSetPoint
		{
			get
			{
				return this._temperatureSetPoint;
			}
			set
			{
				base.SetProperty<double>(ref this._temperatureSetPoint, value, "TemperatureSetPoint");
			}
		}

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x060005CB RID: 1483 RVA: 0x0003991F File Offset: 0x00037B1F
		// (set) Token: 0x060005CC RID: 1484 RVA: 0x00039927 File Offset: 0x00037B27
		public string TemperatureSetPointUnit
		{
			get
			{
				return this._temperatureSetPointUnit;
			}
			set
			{
				base.SetProperty<string>(ref this._temperatureSetPointUnit, value, "TemperatureSetPointUnit");
			}
		}

		// Token: 0x04000310 RID: 784
		private double _temperature = 20.0;

		// Token: 0x04000311 RID: 785
		private string _temperatureUnit;

		// Token: 0x04000312 RID: 786
		private double _temperatureSetPoint;

		// Token: 0x04000313 RID: 787
		private string _temperatureSetPointUnit;
	}
}
