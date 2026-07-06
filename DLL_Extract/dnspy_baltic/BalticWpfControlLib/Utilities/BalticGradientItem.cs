// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000050 RID: 80
	public class BalticGradientItem : BindableBase
	{
		// Token: 0x06000429 RID: 1065 RVA: 0x00019388 File Offset: 0x00017588
		public BalticGradientItem(BalticGradientItem sourceObj)
		{
			this._time = sourceObj._time;
			this._duration = sourceObj._duration;
			this._composition = sourceObj._composition;
			this._flow = sourceObj._flow;
			this._isIsocratic = sourceObj._isIsocratic;
			this.SmartName = sourceObj.SmartName;
			this._isFirstRow = sourceObj._isFirstRow;
			this._isLastRow = sourceObj._isLastRow;
			this._toolTip = sourceObj._toolTip;
			this._paramToolTip = sourceObj._paramToolTip;
			this._isInTimeOrder = sourceObj._isInTimeOrder;
			this._isParamValid = sourceObj._isParamValid;
			this._isLastRowValid = sourceObj._isLastRowValid;
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x0001944C File Offset: 0x0001764C
		public BalticGradientItem(double time, double duration, double composition, double flow, bool isIsocratic = false, string smartName = "")
		{
			this._time = time;
			this._duration = duration;
			this._composition = composition;
			this._flow = flow;
			this._isIsocratic = isIsocratic;
			this.SmartName = smartName;
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x0600042B RID: 1067 RVA: 0x000194A1 File Offset: 0x000176A1
		// (set) Token: 0x0600042C RID: 1068 RVA: 0x000194A9 File Offset: 0x000176A9
		public bool IsIsocratic
		{
			get
			{
				return this._isIsocratic;
			}
			set
			{
				base.SetField<bool>(ref this._isIsocratic, value, "IsIsocratic");
			}
		}

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x0600042D RID: 1069 RVA: 0x000194BE File Offset: 0x000176BE
		// (set) Token: 0x0600042E RID: 1070 RVA: 0x000194C6 File Offset: 0x000176C6
		public double Duration
		{
			get
			{
				return this._duration;
			}
			set
			{
				base.SetField<double>(ref this._duration, value, "Duration");
			}
		}

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x0600042F RID: 1071 RVA: 0x000194DB File Offset: 0x000176DB
		// (set) Token: 0x06000430 RID: 1072 RVA: 0x000194E3 File Offset: 0x000176E3
		public double Time
		{
			get
			{
				return this._time;
			}
			set
			{
				value = Math.Round(value, 2);
				base.SetField<double>(ref this._time, value, "Time");
			}
		}

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x06000431 RID: 1073 RVA: 0x00019501 File Offset: 0x00017701
		// (set) Token: 0x06000432 RID: 1074 RVA: 0x00019509 File Offset: 0x00017709
		public double Composition
		{
			get
			{
				return this._composition;
			}
			set
			{
				value = Math.Round(value, 1);
				base.SetField<double>(ref this._composition, value, "Composition");
			}
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x06000433 RID: 1075 RVA: 0x00019527 File Offset: 0x00017727
		// (set) Token: 0x06000434 RID: 1076 RVA: 0x0001952F File Offset: 0x0001772F
		public double Flow
		{
			get
			{
				return this._flow;
			}
			set
			{
				value = Math.Round(value, 2);
				base.SetField<double>(ref this._flow, value, "Flow");
			}
		}

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000435 RID: 1077 RVA: 0x0001954D File Offset: 0x0001774D
		// (set) Token: 0x06000436 RID: 1078 RVA: 0x00019555 File Offset: 0x00017755
		public string SmartName { get; set; }

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000437 RID: 1079 RVA: 0x0001955E File Offset: 0x0001775E
		// (set) Token: 0x06000438 RID: 1080 RVA: 0x00019566 File Offset: 0x00017766
		public bool IsTimeEditable
		{
			get
			{
				return this._isFirstRow;
			}
			set
			{
				base.SetField<bool>(ref this._isFirstRow, value, "IsTimeEditable");
			}
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x06000439 RID: 1081 RVA: 0x0001957B File Offset: 0x0001777B
		// (set) Token: 0x0600043A RID: 1082 RVA: 0x00019583 File Offset: 0x00017783
		public bool IsLastRow
		{
			get
			{
				return this._isLastRow;
			}
			set
			{
				if (base.SetField<bool>(ref this._isLastRow, value, "IsLastRow"))
				{
					this.OnPropertyChanged("IsLastRowValid");
				}
			}
		}

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x0600043B RID: 1083 RVA: 0x000195A4 File Offset: 0x000177A4
		// (set) Token: 0x0600043C RID: 1084 RVA: 0x000195AC File Offset: 0x000177AC
		public string ErrorToolTip
		{
			get
			{
				return this._toolTip;
			}
			set
			{
				base.SetField<string>(ref this._toolTip, value, "ErrorToolTip");
			}
		}

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x0600043D RID: 1085 RVA: 0x000195C1 File Offset: 0x000177C1
		// (set) Token: 0x0600043E RID: 1086 RVA: 0x000195C9 File Offset: 0x000177C9
		public string ParamToolTip
		{
			get
			{
				return this._paramToolTip;
			}
			set
			{
				base.SetField<string>(ref this._paramToolTip, value, "ParamToolTip");
			}
		}

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x0600043F RID: 1087 RVA: 0x000195DE File Offset: 0x000177DE
		// (set) Token: 0x06000440 RID: 1088 RVA: 0x000195E6 File Offset: 0x000177E6
		public bool IsInTimeOrder
		{
			get
			{
				return this._isInTimeOrder;
			}
			set
			{
				if (base.SetField<bool>(ref this._isInTimeOrder, value, "IsInTimeOrder"))
				{
					this.OnPropertyChanged("IsValidState");
					this.OnPropertyChanged("IsLastRowValid");
				}
			}
		}

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x06000441 RID: 1089 RVA: 0x00019612 File Offset: 0x00017812
		// (set) Token: 0x06000442 RID: 1090 RVA: 0x0001961A File Offset: 0x0001781A
		public bool IsParamValid
		{
			get
			{
				return this._isParamValid;
			}
			set
			{
				if (base.SetField<bool>(ref this._isParamValid, value, "IsParamValid"))
				{
					this.OnPropertyChanged("IsValidState");
					this.OnPropertyChanged("IsLastRowValid");
				}
			}
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x06000443 RID: 1091 RVA: 0x00019646 File Offset: 0x00017846
		public bool IsValidState
		{
			get
			{
				return this._isInTimeOrder && this._isParamValid;
			}
		}

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x06000444 RID: 1092 RVA: 0x00019658 File Offset: 0x00017858
		// (set) Token: 0x06000445 RID: 1093 RVA: 0x00019660 File Offset: 0x00017860
		public bool IsLastRowValid
		{
			get
			{
				return this._isLastRowValid;
			}
			set
			{
				base.SetField<bool>(ref this._isLastRowValid, value, "IsLastRowValid");
			}
		}

		// Token: 0x04000264 RID: 612
		private double _time;

		// Token: 0x04000265 RID: 613
		private double _duration;

		// Token: 0x04000266 RID: 614
		private double _composition;

		// Token: 0x04000267 RID: 615
		private double _flow;

		// Token: 0x04000268 RID: 616
		private bool _isFirstRow;

		// Token: 0x04000269 RID: 617
		private bool _isLastRow;

		// Token: 0x0400026A RID: 618
		private bool _isIsocratic;

		// Token: 0x0400026B RID: 619
		private string _toolTip;

		// Token: 0x0400026C RID: 620
		private string _paramToolTip;

		// Token: 0x0400026D RID: 621
		private bool _isInTimeOrder = true;

		// Token: 0x0400026E RID: 622
		private bool _isParamValid = true;

		// Token: 0x0400026F RID: 623
		private bool _isLastRowValid = true;
	}
}
