using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BalticClassLib;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200004A RID: 74
	public class BindableBalticHardware : BindableBase, IDataErrorInfo
	{
		// Token: 0x060003F8 RID: 1016 RVA: 0x00018DC9 File Offset: 0x00016FC9
		public BindableBalticHardware(BalticHWProfile hardwareProfile)
		{
		}

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x060003F9 RID: 1017 RVA: 0x00018DD8 File Offset: 0x00016FD8
		// (set) Token: 0x060003FA RID: 1018 RVA: 0x00018DEA File Offset: 0x00016FEA
		public string AutosamplerIP
		{
			get
			{
				return this._hardwareProfile.Autosampler.DeviceIP;
			}
			set
			{
				this._hardwareProfile.Autosampler.DeviceIP = value;
				this.OnPropertyChanged("AutosamplerIP");
				this.OnPropertyChanged("PumpHost");
			}
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x060003FB RID: 1019 RVA: 0x00018E13 File Offset: 0x00017013
		// (set) Token: 0x060003FC RID: 1020 RVA: 0x00018E25 File Offset: 0x00017025
		public string PumpHost
		{
			get
			{
				return this._hardwareProfile.LC.DeviceIP;
			}
			set
			{
				this._hardwareProfile.LC.DeviceIP = value;
				this.OnPropertyChanged("PumpHost");
				this.OnPropertyChanged("AutosamplerIP");
			}
		}

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x060003FD RID: 1021 RVA: 0x00018E4E File Offset: 0x0001704E
		// (set) Token: 0x060003FE RID: 1022 RVA: 0x00018E5B File Offset: 0x0001705B
		public bool IsSimulate
		{
			get
			{
				return this._hardwareProfile.IsSimulate;
			}
			set
			{
				this._hardwareProfile.IsSimulate = value;
				if (value)
				{
					this.IsCTCSimulate = false;
				}
				this.OnPropertyChanged("IsSimulate");
			}
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x060003FF RID: 1023 RVA: 0x00018E7E File Offset: 0x0001707E
		// (set) Token: 0x06000400 RID: 1024 RVA: 0x00018E8B File Offset: 0x0001708B
		public bool IsCTCSimulate
		{
			get
			{
				return this._hardwareProfile.IsCTCSimulate;
			}
			set
			{
				this._hardwareProfile.IsCTCSimulate = value;
				if (value)
				{
					this.IsSimulate = false;
				}
				this.OnPropertyChanged("IsCTCSimulate");
			}
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000401 RID: 1025 RVA: 0x00018EAE File Offset: 0x000170AE
		// (set) Token: 0x06000402 RID: 1026 RVA: 0x00018EBB File Offset: 0x000170BB
		public bool IsPressurePSI
		{
			get
			{
				return this._hardwareProfile.IsPressurePSI;
			}
			set
			{
				this._hardwareProfile.IsPressurePSI = value;
				this.OnPropertyChanged("IsPressurePSI");
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000403 RID: 1027 RVA: 0x00018ED4 File Offset: 0x000170D4
		// (set) Token: 0x06000404 RID: 1028 RVA: 0x00018EE1 File Offset: 0x000170E1
		public int MaxPumpPressure
		{
			get
			{
				return this._hardwareProfile.MaxPumpPressure;
			}
			set
			{
				this._hardwareProfile.MaxPumpPressure = value;
				this.OnPropertyChanged("MaxPumpPressure");
			}
		}

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000405 RID: 1029 RVA: 0x00018EFA File Offset: 0x000170FA
		// (set) Token: 0x06000406 RID: 1030 RVA: 0x00018F07 File Offset: 0x00017107
		public bool UseIdleProcedure
		{
			get
			{
				return this._hardwareProfile.UseIdleProcedure;
			}
			set
			{
				this._hardwareProfile.UseIdleProcedure = value;
				this.OnPropertyChanged("UseIdleProcedure");
			}
		}

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000407 RID: 1031 RVA: 0x00018F20 File Offset: 0x00017120
		// (set) Token: 0x06000408 RID: 1032 RVA: 0x00018F2D File Offset: 0x0001712D
		public bool IsColumnOvenConnected
		{
			get
			{
				return this._hardwareProfile.IsColumnOvenConnected;
			}
			set
			{
				this._hardwareProfile.IsColumnOvenConnected = value;
				this.OnPropertyChanged("IsColumnOvenConnected");
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000409 RID: 1033 RVA: 0x00018F46 File Offset: 0x00017146
		// (set) Token: 0x0600040A RID: 1034 RVA: 0x00018F58 File Offset: 0x00017158
		public BalticHWProfile.ASProfile.TrayType Slot1TrayType
		{
			get
			{
				return this._hardwareProfile.Autosampler.Slot1TrayType;
			}
			set
			{
				this._hardwareProfile.Autosampler.Slot1TrayType = value;
				this.OnPropertyChanged("Slot1TrayType");
			}
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x0600040B RID: 1035 RVA: 0x00018F76 File Offset: 0x00017176
		// (set) Token: 0x0600040C RID: 1036 RVA: 0x00018F88 File Offset: 0x00017188
		public BalticHWProfile.ASProfile.TrayType Slot2TrayType
		{
			get
			{
				return this._hardwareProfile.Autosampler.Slot2TrayType;
			}
			set
			{
				this._hardwareProfile.Autosampler.Slot2TrayType = value;
				this.OnPropertyChanged("Slot2TrayType");
			}
		}

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x0600040D RID: 1037 RVA: 0x00018FA6 File Offset: 0x000171A6
		// (set) Token: 0x0600040E RID: 1038 RVA: 0x00018FB8 File Offset: 0x000171B8
		public string Slot1TrayTypeName
		{
			get
			{
				return this._hardwareProfile.Autosampler.Slot1TrayTypeName;
			}
			set
			{
				this._hardwareProfile.Autosampler.Slot1TrayTypeName = value;
				BalticHWProfile.ASProfile.TrayType selEnum;
				if (Enum.TryParse<BalticHWProfile.ASProfile.TrayType>(this._hardwareProfile.Autosampler.Slot1TrayTypeName, out selEnum))
				{
					this._hardwareProfile.Autosampler.Slot1TrayType = selEnum;
				}
				this.OnPropertyChanged("Slot1TrayTypeName");
			}
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x0600040F RID: 1039 RVA: 0x0001900B File Offset: 0x0001720B
		// (set) Token: 0x06000410 RID: 1040 RVA: 0x00019020 File Offset: 0x00017220
		public string Slot2TrayTypeName
		{
			get
			{
				return this._hardwareProfile.Autosampler.Slot2TrayTypeName;
			}
			set
			{
				this._hardwareProfile.Autosampler.Slot2TrayTypeName = value;
				BalticHWProfile.ASProfile.TrayType selection;
				if (Enum.TryParse<BalticHWProfile.ASProfile.TrayType>(this._hardwareProfile.Autosampler.Slot2TrayTypeName, out selection))
				{
					this._hardwareProfile.Autosampler.Slot2TrayType = selection;
				}
				this.OnPropertyChanged("Slot2TrayTypeName");
			}
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x06000411 RID: 1041 RVA: 0x00019073 File Offset: 0x00017273
		// (set) Token: 0x06000412 RID: 1042 RVA: 0x00019085 File Offset: 0x00017285
		public BalticHWProfile.ASProfile.TrayTemperature TrayTemperature
		{
			get
			{
				return this._hardwareProfile.Autosampler.SampleTrayTemperature;
			}
			set
			{
				this._hardwareProfile.Autosampler.SampleTrayTemperature = value;
				this.OnPropertyChanged("TrayTemperature");
			}
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000413 RID: 1043 RVA: 0x000190A3 File Offset: 0x000172A3
		// (set) Token: 0x06000414 RID: 1044 RVA: 0x000190B0 File Offset: 0x000172B0
		public string ApplicationKey
		{
			get
			{
				return this._hardwareProfile.ApplicationKey;
			}
			set
			{
				this._hardwareProfile.ApplicationKey = value;
				this.OnPropertyChanged("ApplicationKey");
			}
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x06000415 RID: 1045 RVA: 0x000190C9 File Offset: 0x000172C9
		// (set) Token: 0x06000416 RID: 1046 RVA: 0x000190D6 File Offset: 0x000172D6
		public List<BalticHWProfile.CapillaryItem> ConnectionProfile
		{
			get
			{
				return this._hardwareProfile.ConnectionProfile;
			}
			set
			{
				this._hardwareProfile.ConnectionProfile = value;
				this.OnPropertyChanged("ConnectionProfile");
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x06000417 RID: 1047 RVA: 0x000190EF File Offset: 0x000172EF
		public string Error
		{
			get
			{
				return string.Empty;
			}
		}

		// Token: 0x170000B3 RID: 179
		public string this[string columnName]
		{
			get
			{
				string result = string.Empty;
				if (!(columnName == "AutosamplerIP"))
				{
					if (columnName == "PumpHost")
					{
						if (!BindableBalticHardware._ipRegex.IsMatch(this.PumpHost))
						{
							result = "Invalid IP address";
						}
						else if (this.PumpHost.Equals(this.AutosamplerIP))
						{
							result = "IP address must not be the same as from the sampler";
						}
					}
				}
				else if (!BindableBalticHardware._ipRegex.IsMatch(this.AutosamplerIP))
				{
					result = "Invalid IP address";
				}
				else if (this.AutosamplerIP.Equals(this.PumpHost))
				{
					result = "IP address must not be the same as from the pump";
				}
				return result;
			}
		}

		// Token: 0x04000262 RID: 610
		[CompilerGenerated]
		private BalticHWProfile _hardwareProfile;

		// Token: 0x04000263 RID: 611
		private static readonly Regex _ipRegex = new Regex("^(\\d\\.)\\d");
	}
}
