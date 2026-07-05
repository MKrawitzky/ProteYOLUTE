using System;
using System.Collections.Generic;
using System.Linq;
using BalticClassLib;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000051 RID: 81
	public class BindableBalticMethod : BindableBase
	{
		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x06000446 RID: 1094 RVA: 0x00019675 File Offset: 0x00017875
		// (set) Token: 0x06000447 RID: 1095 RVA: 0x0001967D File Offset: 0x0001787D
		public List<BindableBalticMethod.ElutionType> ExperimentTypes { get; set; }

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x06000448 RID: 1096 RVA: 0x00019686 File Offset: 0x00017886
		// (set) Token: 0x06000449 RID: 1097 RVA: 0x0001968E File Offset: 0x0001788E
		public string ElutionName { get; set; }

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x0600044A RID: 1098 RVA: 0x00019697 File Offset: 0x00017897
		// (set) Token: 0x0600044B RID: 1099 RVA: 0x0001969F File Offset: 0x0001789F
		public string Description { get; set; }

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x0600044C RID: 1100 RVA: 0x000196A8 File Offset: 0x000178A8
		// (set) Token: 0x0600044D RID: 1101 RVA: 0x000196B0 File Offset: 0x000178B0
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

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x0600044E RID: 1102 RVA: 0x000196C5 File Offset: 0x000178C5
		// (set) Token: 0x0600044F RID: 1103 RVA: 0x000196CD File Offset: 0x000178CD
		public bool IsSetTemperature
		{
			get
			{
				return this._isSetTemperature;
			}
			set
			{
				base.SetField<bool>(ref this._isSetTemperature, value, "IsSetTemperature");
			}
		}

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x06000450 RID: 1104 RVA: 0x000196E2 File Offset: 0x000178E2
		// (set) Token: 0x06000451 RID: 1105 RVA: 0x000196EA File Offset: 0x000178EA
		public bool UsesTrapColumn { get; set; }

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x06000452 RID: 1106 RVA: 0x000196F3 File Offset: 0x000178F3
		// (set) Token: 0x06000453 RID: 1107 RVA: 0x000196FB File Offset: 0x000178FB
		public bool UsesSepColumn { get; set; }

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x06000454 RID: 1108 RVA: 0x00019704 File Offset: 0x00017904
		// (set) Token: 0x06000455 RID: 1109 RVA: 0x0001970C File Offset: 0x0001790C
		public BalticGradientList GradientTable
		{
			get
			{
				return this._gradientTable;
			}
			set
			{
				base.SetField<BalticGradientList>(ref this._gradientTable, value, "GradientTable");
			}
		}

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x06000456 RID: 1110 RVA: 0x00019721 File Offset: 0x00017921
		// (set) Token: 0x06000457 RID: 1111 RVA: 0x00019729 File Offset: 0x00017929
		public string TrapColumnName { get; set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x06000458 RID: 1112 RVA: 0x00019732 File Offset: 0x00017932
		// (set) Token: 0x06000459 RID: 1113 RVA: 0x0001973A File Offset: 0x0001793A
		public string SeparationColumnName { get; set; }

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x0600045A RID: 1114 RVA: 0x00019743 File Offset: 0x00017943
		// (set) Token: 0x0600045B RID: 1115 RVA: 0x0001974B File Offset: 0x0001794B
		public double TrapColumnVolume { get; set; }

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x0600045C RID: 1116 RVA: 0x00019754 File Offset: 0x00017954
		// (set) Token: 0x0600045D RID: 1117 RVA: 0x0001975C File Offset: 0x0001795C
		public double SeparationColumnVolume { get; set; }

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x0600045E RID: 1118 RVA: 0x00019765 File Offset: 0x00017965
		// (set) Token: 0x0600045F RID: 1119 RVA: 0x0001976D File Offset: 0x0001796D
		public BindableBalticMethod.ColumnEquilibration TrapColumnEquil { get; set; }

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x06000460 RID: 1120 RVA: 0x00019776 File Offset: 0x00017976
		// (set) Token: 0x06000461 RID: 1121 RVA: 0x0001977E File Offset: 0x0001797E
		public BindableBalticMethod.ColumnEquilibration SeparationColumnEquil { get; set; }

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x06000462 RID: 1122 RVA: 0x00019787 File Offset: 0x00017987
		// (set) Token: 0x06000463 RID: 1123 RVA: 0x0001978F File Offset: 0x0001798F
		public BindableBalticMethod.ColumnEquilibration SampleLoading { get; set; }

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000464 RID: 1124 RVA: 0x00019798 File Offset: 0x00017998
		// (set) Token: 0x06000465 RID: 1125 RVA: 0x000197A0 File Offset: 0x000179A0
		public BindableBalticMethod.AdvancedSett AdvancedSettings { get; set; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x06000466 RID: 1126 RVA: 0x000197A9 File Offset: 0x000179A9
		// (set) Token: 0x06000467 RID: 1127 RVA: 0x000197B1 File Offset: 0x000179B1
		public double OvenTemperature
		{
			get
			{
				return this._ovenTemperature;
			}
			set
			{
				base.SetField<double>(ref this._ovenTemperature, value, "OvenTemperature");
			}
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000468 RID: 1128 RVA: 0x000197C8 File Offset: 0x000179C8
		public double AcquisitionTime
		{
			get
			{
				if (this._gradientTable.Count <= 0)
				{
					return 0.0;
				}
				return this._gradientTable[this.GradientTable.Count - 1].Time + this._gradientTable[this.GradientTable.Count - 1].Duration;
			}
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000469 RID: 1129 RVA: 0x00019828 File Offset: 0x00017A28
		// (set) Token: 0x0600046A RID: 1130 RVA: 0x00019830 File Offset: 0x00017A30
		public double GradientTime
		{
			get
			{
				return this._gradientTime;
			}
			set
			{
				base.SetField<double>(ref this._gradientTime, value, "GradientTime");
			}
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x00019848 File Offset: 0x00017A48
		public BindableBalticMethod(BalticMethod balticMethod, BalticInstrumentFacade instrument)
		{
			this.ExperimentTypes = new List<BindableBalticMethod.ElutionType>();
			foreach (ProcedureInfo info in instrument.ElutionTypeInfoList)
			{
				this.ExperimentTypes.Add(new BindableBalticMethod.ElutionType(info.Name, info.DisplayName, info.LegacyName, info.IsLegacy));
			}
			this._balticMethod = balticMethod;
			this.SetMethod(balticMethod);
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x0001991C File Offset: 0x00017B1C
		public void Reset(double columnOvenMinTemperature)
		{
			this.SetMethod(new BalticMethod(columnOvenMinTemperature));
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x0001992C File Offset: 0x00017B2C
		public void Refresh(ChildProcedureArguments advChildArguments)
		{
			this.SetMethod(this._balticMethod);
			using (List<BindableBalticMethod.AdvancedSett.AdvancedChildParameter>.Enumerator enumerator = this.AdvancedSettings.ChildParameters.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BindableBalticMethod.AdvancedSett.AdvancedChildParameter setting = enumerator.Current;
					ChildProcedureArgument childArgDefaults = advChildArguments.SingleOrDefault((ChildProcedureArgument s) => s.Header == setting.Header && s.Name == setting.Name);
					if (childArgDefaults != null)
					{
						setting.IsService = childArgDefaults.IsService;
					}
				}
			}
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x000199BC File Offset: 0x00017BBC
		private void SetMethod(BalticMethod balticMethod)
		{
			this.ElutionName = balticMethod.ElutionName;
			this.OvenTemperature = balticMethod.OvenTemperature;
			this.TrapColumnName = balticMethod.TrapName;
			this.SeparationColumnName = balticMethod.SeparatorName;
			this.SeparationColumnVolume = balticMethod.SeparatorVolume;
			this.TrapColumnVolume = balticMethod.TrapVolume;
			this.TrapColumnEquil.Set(balticMethod.TrapColumnEquil);
			this.SeparationColumnEquil.Set(balticMethod.SeparationColumnEquil);
			this.SampleLoading.Set(balticMethod.SampleLoading);
			this.AdvancedSettings.Set(balticMethod.AdvancedSettings);
			this.IsIsocratic = balticMethod.IsIsocratic;
			this.UsesTrapColumn = balticMethod.UsesTrapColumn;
			this.UsesSepColumn = balticMethod.UsesSepColumn;
			this.IsSetTemperature = balticMethod.IsSetTemperature;
			this._gradientTable.Clear();
			for (int i = 0; i < balticMethod.Gradient.Count; i++)
			{
				BalticMethod.GradientItem item = balticMethod.Gradient[i];
				double duration = 0.0;
				if (i < balticMethod.Gradient.Count - 1)
				{
					duration = (balticMethod.Gradient[i + 1].Time - item.Time) / 60.0;
				}
				BalticGradientItem bgi = new BalticGradientItem(item.Time / 60.0, duration, Math.Round(item.Mix * 100.0, 1), item.Flow, this._isIsocratic, "")
				{
					IsTimeEditable = (i > 0)
				};
				this._gradientTable.Add(bgi);
			}
			if (this._gradientTable.Count > 0)
			{
				this._gradientTable[this._gradientTable.Count - 1].IsLastRow = true;
			}
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x00019B78 File Offset: 0x00017D78
		private static double ViscosityMix(double temperature, double percentageACN)
		{
			double temp = temperature + 273.15;
			return Math.Exp(percentageACN * (-3.476 + 726.0 / temp) + (1.0 - percentageACN) * (-5.414 + 1566.0 / temp) + percentageACN * (1.0 - percentageACN) * (-1.762 + 929.0 / temp)) / 100.0;
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x00019C00 File Offset: 0x00017E00
		private static double ColumnFlow(Column column, double pressure, double ovenTemp = 20.0)
		{
			double viscosityH2O = BindableBalticMethod.ViscosityMix(ovenTemp, 0.0);
			double r = column.InnerDiamater * 0.5;
			return Math.Pow(10.0, 3.0) * (pressure * Math.Pow(10.0, 6.0) * Math.Pow(column.ParticleDiameter * 0.0001, 2.0) * Math.Pow(0.42, 3.0) * 3.141592653589793 * Math.Pow(r * 0.1, 2.0) * 60.0) / (180.0 * viscosityH2O * column.Length * 0.1 * Math.Pow(0.5800000000000001, 2.0));
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00019CFC File Offset: 0x00017EFC
		public BalticMethod ToBalticMethod(BalticColumnList columns = null)
		{
			this._balticMethod.ElutionName = this.ElutionName;
			this._balticMethod.IsIsocratic = this._isIsocratic;
			this._balticMethod.IsSetTemperature = this._isSetTemperature;
			this._balticMethod.UsesTrapColumn = this.UsesTrapColumn;
			this._balticMethod.UsesSepColumn = this.UsesSepColumn;
			this._balticMethod.OvenTemperature = this.OvenTemperature;
			this._balticMethod.TrapName = this.TrapColumnName;
			this._balticMethod.SeparatorName = this.SeparationColumnName;
			this._balticMethod.TrapVolume = this.TrapColumnVolume;
			this._balticMethod.SeparatorVolume = this.SeparationColumnVolume;
			this._balticMethod.TrapColumnEquil = new BalticMethod.ColumnEquil(this.TrapColumnEquil.Pressure, this.TrapColumnEquil.DefaultPressure, this.TrapColumnEquil.Scale, this.TrapColumnEquil.DefaultScale, this.TrapColumnEquil.IsBottomSense, this.TrapColumnEquil.InjectionMethod, this.TrapColumnEquil.DefaultIsBottomSense, this.TrapColumnEquil.DefaultInjectionMethod, this.TrapColumnEquil.EquilTime, 30.0, 30.0);
			this._balticMethod.SeparationColumnEquil = new BalticMethod.ColumnEquil(this.SeparationColumnEquil.Pressure, this.SeparationColumnEquil.DefaultPressure, this.SeparationColumnEquil.Scale, this.SeparationColumnEquil.DefaultScale, this.SeparationColumnEquil.IsBottomSense, this.SeparationColumnEquil.InjectionMethod, this.SeparationColumnEquil.DefaultIsBottomSense, this.SeparationColumnEquil.DefaultInjectionMethod, this.SeparationColumnEquil.EquilTime, 30.0, 30.0);
			this._balticMethod.SampleLoading = new BalticMethod.ColumnEquil(this.SampleLoading.Pressure, this.SampleLoading.DefaultPressure, this.SampleLoading.Scale, this.SampleLoading.DefaultScale, this.SampleLoading.IsBottomSense, this.SampleLoading.InjectionMethod, this.SampleLoading.DefaultIsBottomSense, this.SampleLoading.DefaultInjectionMethod, this.SampleLoading.EquilTime, this.SampleLoading.PenetrationDepth, this.SampleLoading.DefaultPenetrationDepth);
			this._balticMethod.AdvancedSettings = new BalticMethod.AdvancedSett
			{
				Header = this.AdvancedSettings.Header,
				CalibrantTime = this.AdvancedSettings.CalibrantTime,
				CalibrantVolume = this.AdvancedSettings.CalibrantVolume,
				HeaderFgColor = this.AdvancedSettings.HeaderFgColor
			};
			foreach (BindableBalticMethod.AdvancedSett.AdvancedParameter item in this.AdvancedSettings.Parameters)
			{
				if (item.Name == "MS Calibrant Injection")
				{
					this._balticMethod.AdvancedSettings.IsCalibrantInject = (bool)item.Value;
				}
				this._balticMethod.AdvancedSettings.Parameters.Add(new BalticMethod.AdvancedSett.AdvancedParameter(item.Name, item.Value, item.DefaultValue, item.Unit));
			}
			foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter item2 in this.AdvancedSettings.ChildParameters)
			{
				this._balticMethod.AdvancedSettings.ChildParameters.Add(new BalticMethod.AdvancedSett.AdvancedChildParameter(item2.Header, item2.Name, item2.Value, item2.DefaultValue, item2.Unit));
			}
			if (this.UsesSepColumn)
			{
				Column sepColumn = ((columns != null) ? columns.FirstOrDefault((Column x) => x.Name == this.SeparationColumnName) : null);
				if (sepColumn != null)
				{
					if (sepColumn.IsAdvancedSettings && sepColumn.ColumnVolume > 0.0)
					{
						this._balticMethod.SeparatorVolume = sepColumn.ColumnVolume;
					}
					if (sepColumn.IsAdvancedSettings && sepColumn.UnityFlow > 0.0)
					{
						this._balticMethod.SeparatorUnityflow = sepColumn.UnityFlow;
					}
					else
					{
						this._balticMethod.SeparatorUnityflow = BindableBalticMethod.ColumnFlow(sepColumn, 1.0, this.OvenTemperature);
					}
				}
			}
			if (this.UsesTrapColumn)
			{
				Column trapColumn = ((columns != null) ? columns.FirstOrDefault((Column x) => x.Name == this.TrapColumnName) : null);
				if (trapColumn != null)
				{
					if (trapColumn.IsAdvancedSettings && trapColumn.ColumnVolume > 0.0)
					{
						this._balticMethod.TrapVolume = trapColumn.ColumnVolume;
					}
					if (trapColumn.IsAdvancedSettings && trapColumn.UnityFlow > 0.0)
					{
						this._balticMethod.TrapUnityflow = trapColumn.UnityFlow;
					}
					else
					{
						this._balticMethod.TrapUnityflow = BindableBalticMethod.ColumnFlow(trapColumn, 1.0, 20.0);
					}
				}
			}
			this._balticMethod.Gradient.Clear();
			foreach (BalticGradientItem item3 in this._gradientTable)
			{
				this._balticMethod.Gradient.Add(new BalticMethod.GradientItem(item3.Time * 60.0, item3.Flow, 0.01 * item3.Composition));
			}
			return this._balticMethod;
		}

		// Token: 0x04000273 RID: 627
		private readonly BalticMethod _balticMethod;

		// Token: 0x04000274 RID: 628
		private bool _isIsocratic;

		// Token: 0x04000275 RID: 629
		private bool _isSetTemperature = true;

		// Token: 0x04000276 RID: 630
		private BalticGradientList _gradientTable = new BalticGradientList();

		// Token: 0x04000277 RID: 631
		private double _gradientTime;

		// Token: 0x04000278 RID: 632
		private double _ovenTemperature;

		// Token: 0x02000115 RID: 277
		public class ColumnEquilibration : BindableBase
		{
			// Token: 0x1700017E RID: 382
			// (get) Token: 0x060007F5 RID: 2037 RVA: 0x0003E36B File Offset: 0x0003C56B
			// (set) Token: 0x060007F6 RID: 2038 RVA: 0x0003E373 File Offset: 0x0003C573
			public double Pressure
			{
				get
				{
					return this._pressure;
				}
				set
				{
					base.SetField<double>(ref this._pressure, value, "Pressure");
				}
			}

			// Token: 0x1700017F RID: 383
			// (get) Token: 0x060007F7 RID: 2039 RVA: 0x0003E388 File Offset: 0x0003C588
			// (set) Token: 0x060007F8 RID: 2040 RVA: 0x0003E390 File Offset: 0x0003C590
			public double Scale
			{
				get
				{
					return this._scale;
				}
				set
				{
					base.SetField<double>(ref this._scale, value, "Scale");
				}
			}

			// Token: 0x17000180 RID: 384
			// (get) Token: 0x060007F9 RID: 2041 RVA: 0x0003E3A5 File Offset: 0x0003C5A5
			// (set) Token: 0x060007FA RID: 2042 RVA: 0x0003E3AD File Offset: 0x0003C5AD
			public bool IsBottomSense
			{
				get
				{
					return this._isBottomSense;
				}
				set
				{
					base.SetField<bool>(ref this._isBottomSense, value, "IsBottomSense");
				}
			}

			// Token: 0x17000181 RID: 385
			// (get) Token: 0x060007FB RID: 2043 RVA: 0x0003E3C2 File Offset: 0x0003C5C2
			// (set) Token: 0x060007FC RID: 2044 RVA: 0x0003E3CA File Offset: 0x0003C5CA
			public BalticInjectionType InjectionMethod
			{
				get
				{
					return this._injectionMethod;
				}
				set
				{
					base.SetField<BalticInjectionType>(ref this._injectionMethod, value, "InjectionMethod");
				}
			}

			// Token: 0x17000182 RID: 386
			// (get) Token: 0x060007FD RID: 2045 RVA: 0x0003E3DF File Offset: 0x0003C5DF
			// (set) Token: 0x060007FE RID: 2046 RVA: 0x0003E3E7 File Offset: 0x0003C5E7
			public double DefaultPressure
			{
				get
				{
					return this._defPressure;
				}
				set
				{
					base.SetField<double>(ref this._defPressure, value, "DefaultPressure");
				}
			}

			// Token: 0x17000183 RID: 387
			// (get) Token: 0x060007FF RID: 2047 RVA: 0x0003E3FC File Offset: 0x0003C5FC
			// (set) Token: 0x06000800 RID: 2048 RVA: 0x0003E404 File Offset: 0x0003C604
			public double DefaultScale
			{
				get
				{
					return this._defScale;
				}
				set
				{
					base.SetField<double>(ref this._defScale, value, "DefaultScale");
				}
			}

			// Token: 0x17000184 RID: 388
			// (get) Token: 0x06000801 RID: 2049 RVA: 0x0003E419 File Offset: 0x0003C619
			// (set) Token: 0x06000802 RID: 2050 RVA: 0x0003E421 File Offset: 0x0003C621
			public bool DefaultIsBottomSense
			{
				get
				{
					return this._defIsBottomSense;
				}
				set
				{
					base.SetField<bool>(ref this._defIsBottomSense, value, "DefaultIsBottomSense");
				}
			}

			// Token: 0x17000185 RID: 389
			// (get) Token: 0x06000803 RID: 2051 RVA: 0x0003E436 File Offset: 0x0003C636
			// (set) Token: 0x06000804 RID: 2052 RVA: 0x0003E43E File Offset: 0x0003C63E
			public BalticInjectionType DefaultInjectionMethod
			{
				get
				{
					return this._defInjectionMethod;
				}
				set
				{
					base.SetField<BalticInjectionType>(ref this._defInjectionMethod, value, "DefaultInjectionMethod");
				}
			}

			// Token: 0x17000186 RID: 390
			// (get) Token: 0x06000805 RID: 2053 RVA: 0x0003E453 File Offset: 0x0003C653
			// (set) Token: 0x06000806 RID: 2054 RVA: 0x0003E45B File Offset: 0x0003C65B
			public double EquilTime
			{
				get
				{
					return this._equilTime;
				}
				set
				{
					base.SetField<double>(ref this._equilTime, value, "EquilTime");
				}
			}

			// Token: 0x17000187 RID: 391
			// (get) Token: 0x06000807 RID: 2055 RVA: 0x0003E470 File Offset: 0x0003C670
			// (set) Token: 0x06000808 RID: 2056 RVA: 0x0003E478 File Offset: 0x0003C678
			public double PenetrationDepth
			{
				get
				{
					return this._penetrationDepth;
				}
				set
				{
					base.SetField<double>(ref this._penetrationDepth, value, "PenetrationDepth");
				}
			}

			// Token: 0x17000188 RID: 392
			// (get) Token: 0x06000809 RID: 2057 RVA: 0x0003E48D File Offset: 0x0003C68D
			// (set) Token: 0x0600080A RID: 2058 RVA: 0x0003E495 File Offset: 0x0003C695
			public double DefaultPenetrationDepth
			{
				get
				{
					return this._defPenetrationDepth;
				}
				set
				{
					base.SetField<double>(ref this._defPenetrationDepth, value, "DefaultPenetrationDepth");
				}
			}

			// Token: 0x0600080B RID: 2059 RVA: 0x0003E4AC File Offset: 0x0003C6AC
			public ColumnEquilibration()
			{
			}

			// Token: 0x0600080C RID: 2060 RVA: 0x0003E4FC File Offset: 0x0003C6FC
			public ColumnEquilibration(double pressure, int scale, double defPressure, int defScale, bool isBottomSense, BalticInjectionType injectionMethod, bool defIsBottomSense, BalticInjectionType defInjectionMethod, double equilTime, double penetrationDepth = 30.0, double defPenetrationDepth = 30.0)
			{
				this._pressure = pressure;
				this._scale = (double)scale;
				this.EquilTime = equilTime;
				this._defPressure = defPressure;
				this._defScale = (double)defScale;
				this._isBottomSense = isBottomSense;
				this._injectionMethod = injectionMethod;
				this._penetrationDepth = penetrationDepth;
				this._defIsBottomSense = defIsBottomSense;
				this._defInjectionMethod = defInjectionMethod;
				this._defPenetrationDepth = defPenetrationDepth;
			}

			// Token: 0x0600080D RID: 2061 RVA: 0x0003E5A2 File Offset: 0x0003C7A2
			public void RevertToDefault()
			{
				this._pressure = this._defPressure;
				this._scale = this._defScale;
				this._isBottomSense = this._defIsBottomSense;
				this._injectionMethod = this._defInjectionMethod;
				this._penetrationDepth = this._defPenetrationDepth;
			}

			// Token: 0x0600080E RID: 2062 RVA: 0x0003E5E0 File Offset: 0x0003C7E0
			public void Set(BalticMethod.ColumnEquil equil)
			{
				this._pressure = equil.Pressure;
				this._scale = equil.Scale;
				this.EquilTime = equil.EquilTime;
				this._defPressure = equil.DefaultPressure;
				this._defScale = equil.DefaultScale;
				this._isBottomSense = equil.IsBottomSense;
				this._injectionMethod = equil.InjectionMethod;
				this._defIsBottomSense = equil.DefaultIsBottomSense;
				this._defInjectionMethod = equil.DefaultInjectionMethod;
				this._penetrationDepth = equil.PenetrationDepth;
				this._defPenetrationDepth = equil.DefaultPenetrationDepth;
			}

			// Token: 0x0600080F RID: 2063 RVA: 0x0003E674 File Offset: 0x0003C874
			public BalticMethod.ColumnEquil ToColumnEquil()
			{
				return new BalticMethod.ColumnEquil(this._pressure, this._defPressure, this._scale, this._defScale, this._isBottomSense, this._injectionMethod, this._defIsBottomSense, this._defInjectionMethod, this._equilTime, this._penetrationDepth, this._defPenetrationDepth);
			}

			// Token: 0x0400043A RID: 1082
			private double _pressure;

			// Token: 0x0400043B RID: 1083
			private double _scale = 1.0;

			// Token: 0x0400043C RID: 1084
			private double _defPressure;

			// Token: 0x0400043D RID: 1085
			private double _defScale = 1.0;

			// Token: 0x0400043E RID: 1086
			private double _equilTime;

			// Token: 0x0400043F RID: 1087
			private double _penetrationDepth = 30.0;

			// Token: 0x04000440 RID: 1088
			private BalticInjectionType _injectionMethod;

			// Token: 0x04000441 RID: 1089
			private bool _isBottomSense;

			// Token: 0x04000442 RID: 1090
			private BalticInjectionType _defInjectionMethod;

			// Token: 0x04000443 RID: 1091
			private bool _defIsBottomSense;

			// Token: 0x04000444 RID: 1092
			private double _defPenetrationDepth = 30.0;
		}

		// Token: 0x02000116 RID: 278
		public class AdvancedSett : BindableBase
		{
			// Token: 0x17000189 RID: 393
			// (get) Token: 0x06000810 RID: 2064 RVA: 0x0003E6C8 File Offset: 0x0003C8C8
			// (set) Token: 0x06000811 RID: 2065 RVA: 0x0003E6D0 File Offset: 0x0003C8D0
			public string Header
			{
				get
				{
					return this._header;
				}
				set
				{
					base.SetField<string>(ref this._header, value, "Header");
				}
			}

			// Token: 0x1700018A RID: 394
			// (get) Token: 0x06000812 RID: 2066 RVA: 0x0003E6E5 File Offset: 0x0003C8E5
			// (set) Token: 0x06000813 RID: 2067 RVA: 0x0003E6ED File Offset: 0x0003C8ED
			public int[] HeaderFgColor
			{
				get
				{
					return this._headerFgColor;
				}
				set
				{
					base.SetField<int[]>(ref this._headerFgColor, value, "HeaderFgColor");
				}
			}

			// Token: 0x1700018B RID: 395
			// (get) Token: 0x06000814 RID: 2068 RVA: 0x0003E702 File Offset: 0x0003C902
			// (set) Token: 0x06000815 RID: 2069 RVA: 0x0003E70A File Offset: 0x0003C90A
			public List<BindableBalticMethod.AdvancedSett.AdvancedParameter> Parameters
			{
				get
				{
					return this._parameters;
				}
				set
				{
					base.SetField<List<BindableBalticMethod.AdvancedSett.AdvancedParameter>>(ref this._parameters, value, "Parameters");
				}
			}

			// Token: 0x1700018C RID: 396
			// (get) Token: 0x06000816 RID: 2070 RVA: 0x0003E71F File Offset: 0x0003C91F
			// (set) Token: 0x06000817 RID: 2071 RVA: 0x0003E727 File Offset: 0x0003C927
			public List<BindableBalticMethod.AdvancedSett.AdvancedChildParameter> ChildParameters
			{
				get
				{
					return this._childParameters;
				}
				set
				{
					base.SetField<List<BindableBalticMethod.AdvancedSett.AdvancedChildParameter>>(ref this._childParameters, value, "ChildParameters");
				}
			}

			// Token: 0x1700018D RID: 397
			// (get) Token: 0x06000818 RID: 2072 RVA: 0x0003E73C File Offset: 0x0003C93C
			// (set) Token: 0x06000819 RID: 2073 RVA: 0x0003E744 File Offset: 0x0003C944
			public bool IsCalibrantInject { get; set; }

			// Token: 0x1700018E RID: 398
			// (get) Token: 0x0600081A RID: 2074 RVA: 0x0003E74D File Offset: 0x0003C94D
			// (set) Token: 0x0600081B RID: 2075 RVA: 0x0003E755 File Offset: 0x0003C955
			public bool IsExtendedWash { get; set; }

			// Token: 0x1700018F RID: 399
			// (get) Token: 0x0600081C RID: 2076 RVA: 0x0003E75E File Offset: 0x0003C95E
			// (set) Token: 0x0600081D RID: 2077 RVA: 0x0003E766 File Offset: 0x0003C966
			public double CalibrantVolume
			{
				get
				{
					return this._calibrantVolume;
				}
				set
				{
					base.SetField<double>(ref this._calibrantVolume, value, "CalibrantVolume");
				}
			}

			// Token: 0x17000190 RID: 400
			// (get) Token: 0x0600081E RID: 2078 RVA: 0x0003E77B File Offset: 0x0003C97B
			// (set) Token: 0x0600081F RID: 2079 RVA: 0x0003E783 File Offset: 0x0003C983
			public double CalibrantTime
			{
				get
				{
					return this._calibrantTime;
				}
				set
				{
					base.SetField<double>(ref this._calibrantTime, value, "CalibrantTime");
				}
			}

			// Token: 0x06000820 RID: 2080 RVA: 0x0003E798 File Offset: 0x0003C998
			public AdvancedSett()
			{
			}

			// Token: 0x06000821 RID: 2081 RVA: 0x0003E7E8 File Offset: 0x0003C9E8
			public AdvancedSett(BindableBalticMethod.AdvancedSett advParams)
			{
				this.Header = advParams.Header;
				this.IsCalibrantInject = advParams.IsCalibrantInject;
				this.IsExtendedWash = advParams.IsExtendedWash;
				this.CalibrantVolume = advParams.CalibrantVolume;
				this.CalibrantTime = advParams.CalibrantTime;
				this.HeaderFgColor = advParams.HeaderFgColor.ToArray<int>();
				foreach (BindableBalticMethod.AdvancedSett.AdvancedParameter item in advParams.Parameters)
				{
					this.Parameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedParameter(item));
				}
				foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter item2 in advParams.ChildParameters)
				{
					this.ChildParameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedChildParameter(item2));
				}
			}

			// Token: 0x06000822 RID: 2082 RVA: 0x0003E920 File Offset: 0x0003CB20
			public void RevertToDefault()
			{
				foreach (BindableBalticMethod.AdvancedSett.AdvancedParameter advancedParameter in this.Parameters)
				{
					advancedParameter.Value = advancedParameter.DefaultValue;
				}
				foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter advancedChildParameter in this.ChildParameters)
				{
					advancedChildParameter.Value = advancedChildParameter.DefaultValue;
				}
			}

			// Token: 0x06000823 RID: 2083 RVA: 0x0003E9BC File Offset: 0x0003CBBC
			public void RevertChildrenToDefault()
			{
				foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter advancedChildParameter in this.ChildParameters)
				{
					advancedChildParameter.Value = advancedChildParameter.DefaultValue;
				}
			}

			// Token: 0x06000824 RID: 2084 RVA: 0x0003EA14 File Offset: 0x0003CC14
			public void Set(BalticMethod.AdvancedSett advSett)
			{
				this._header = advSett.Header;
				this.IsCalibrantInject = advSett.IsCalibrantInject;
				this.IsExtendedWash = advSett.IsCalibrantInject;
				this._calibrantVolume = advSett.CalibrantVolume;
				this._calibrantTime = advSett.CalibrantTime;
				this._headerFgColor = advSett.HeaderFgColor.ToArray<int>();
				this._parameters.Clear();
				foreach (BalticMethod.AdvancedSett.AdvancedParameter item in advSett.Parameters)
				{
					this.Parameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedParameter(item.Name, item.Value, item.DefaultValue, item.Unit));
				}
				this._childParameters.Clear();
				foreach (BalticMethod.AdvancedSett.AdvancedChildParameter item2 in advSett.ChildParameters)
				{
					this.ChildParameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedChildParameter(item2.Header, item2.AdvParameter.Name, item2.AdvParameter.Value, item2.AdvParameter.DefaultValue, item2.AdvParameter.Unit, false));
				}
				if (this.Parameters.Count == 0)
				{
					this.Parameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedParameter("MS Calibrant Injection", this.IsCalibrantInject, false, ""));
					this.Parameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedParameter("Extended Wash", this.IsExtendedWash, false, ""));
				}
			}

			// Token: 0x04000445 RID: 1093
			private string _header = "";

			// Token: 0x04000446 RID: 1094
			private double _calibrantVolume = 1.0;

			// Token: 0x04000447 RID: 1095
			private double _calibrantTime;

			// Token: 0x04000448 RID: 1096
			private int[] _headerFgColor = new int[3];

			// Token: 0x04000449 RID: 1097
			private List<BindableBalticMethod.AdvancedSett.AdvancedParameter> _parameters = new List<BindableBalticMethod.AdvancedSett.AdvancedParameter>();

			// Token: 0x0400044A RID: 1098
			private List<BindableBalticMethod.AdvancedSett.AdvancedChildParameter> _childParameters = new List<BindableBalticMethod.AdvancedSett.AdvancedChildParameter>();

			// Token: 0x02000141 RID: 321
			public class AdvancedParameter : BindableBase
			{
				// Token: 0x1700019E RID: 414
				// (get) Token: 0x060008B4 RID: 2228 RVA: 0x0004733B File Offset: 0x0004553B
				// (set) Token: 0x060008B5 RID: 2229 RVA: 0x00047343 File Offset: 0x00045543
				public string Name
				{
					get
					{
						return this._name;
					}
					set
					{
						base.SetField<string>(ref this._name, value, "Name");
					}
				}

				// Token: 0x1700019F RID: 415
				// (get) Token: 0x060008B6 RID: 2230 RVA: 0x00047358 File Offset: 0x00045558
				// (set) Token: 0x060008B7 RID: 2231 RVA: 0x00047360 File Offset: 0x00045560
				public object Value
				{
					get
					{
						return this._value;
					}
					set
					{
						base.SetField<object>(ref this._value, value, "Value");
					}
				}

				// Token: 0x170001A0 RID: 416
				// (get) Token: 0x060008B8 RID: 2232 RVA: 0x00047375 File Offset: 0x00045575
				// (set) Token: 0x060008B9 RID: 2233 RVA: 0x0004737D File Offset: 0x0004557D
				public object DefaultValue
				{
					get
					{
						return this._defaultValue;
					}
					set
					{
						base.SetField<object>(ref this._defaultValue, value, "DefaultValue");
					}
				}

				// Token: 0x170001A1 RID: 417
				// (get) Token: 0x060008BA RID: 2234 RVA: 0x00047392 File Offset: 0x00045592
				// (set) Token: 0x060008BB RID: 2235 RVA: 0x0004739A File Offset: 0x0004559A
				public string Unit
				{
					get
					{
						return this._unit;
					}
					set
					{
						base.SetField<string>(ref this._unit, value, "Unit");
					}
				}

				// Token: 0x060008BC RID: 2236 RVA: 0x0001B223 File Offset: 0x00019423
				public AdvancedParameter()
				{
				}

				// Token: 0x060008BD RID: 2237 RVA: 0x000473AF File Offset: 0x000455AF
				public AdvancedParameter(BindableBalticMethod.AdvancedSett.AdvancedParameter advParam)
				{
					this.Name = advParam.Name;
					this.Value = advParam.Value;
					this.DefaultValue = advParam.DefaultValue;
					this.Unit = advParam.Unit;
				}

				// Token: 0x060008BE RID: 2238 RVA: 0x000473E7 File Offset: 0x000455E7
				public AdvancedParameter(string name, object value, object defaultValue, string unit)
				{
					this.Name = name;
					this.Value = value;
					this.DefaultValue = defaultValue;
					this.Unit = unit;
				}

				// Token: 0x040009F8 RID: 2552
				private string _name;

				// Token: 0x040009F9 RID: 2553
				private object _value;

				// Token: 0x040009FA RID: 2554
				private object _defaultValue;

				// Token: 0x040009FB RID: 2555
				private string _unit;
			}

			// Token: 0x02000142 RID: 322
			public class AdvancedChildParameter : BindableBase
			{
				// Token: 0x170001A2 RID: 418
				// (get) Token: 0x060008BF RID: 2239 RVA: 0x0004740C File Offset: 0x0004560C
				// (set) Token: 0x060008C0 RID: 2240 RVA: 0x00047414 File Offset: 0x00045614
				public string Header
				{
					get
					{
						return this._header;
					}
					set
					{
						base.SetField<string>(ref this._header, value, "Header");
					}
				}

				// Token: 0x170001A3 RID: 419
				// (get) Token: 0x060008C1 RID: 2241 RVA: 0x00047429 File Offset: 0x00045629
				// (set) Token: 0x060008C2 RID: 2242 RVA: 0x00047431 File Offset: 0x00045631
				public string Name
				{
					get
					{
						return this._name;
					}
					set
					{
						base.SetField<string>(ref this._name, value, "Name");
					}
				}

				// Token: 0x170001A4 RID: 420
				// (get) Token: 0x060008C3 RID: 2243 RVA: 0x00047446 File Offset: 0x00045646
				// (set) Token: 0x060008C4 RID: 2244 RVA: 0x0004744E File Offset: 0x0004564E
				public object Value
				{
					get
					{
						return this._value;
					}
					set
					{
						base.SetField<object>(ref this._value, value, "Value");
					}
				}

				// Token: 0x170001A5 RID: 421
				// (get) Token: 0x060008C5 RID: 2245 RVA: 0x00047463 File Offset: 0x00045663
				// (set) Token: 0x060008C6 RID: 2246 RVA: 0x0004746B File Offset: 0x0004566B
				public object DefaultValue
				{
					get
					{
						return this._defaultValue;
					}
					set
					{
						base.SetField<object>(ref this._defaultValue, value, "DefaultValue");
					}
				}

				// Token: 0x170001A6 RID: 422
				// (get) Token: 0x060008C7 RID: 2247 RVA: 0x00047480 File Offset: 0x00045680
				// (set) Token: 0x060008C8 RID: 2248 RVA: 0x00047488 File Offset: 0x00045688
				public string Unit
				{
					get
					{
						return this._unit;
					}
					set
					{
						base.SetField<string>(ref this._unit, value, "Unit");
					}
				}

				// Token: 0x170001A7 RID: 423
				// (get) Token: 0x060008C9 RID: 2249 RVA: 0x0004749D File Offset: 0x0004569D
				// (set) Token: 0x060008CA RID: 2250 RVA: 0x000474A5 File Offset: 0x000456A5
				public bool IsService
				{
					get
					{
						return this._isService;
					}
					set
					{
						base.SetField<bool>(ref this._isService, value, "IsService");
					}
				}

				// Token: 0x060008CB RID: 2251 RVA: 0x000474BA File Offset: 0x000456BA
				public AdvancedChildParameter()
				{
				}

				// Token: 0x060008CC RID: 2252 RVA: 0x000474D8 File Offset: 0x000456D8
				public AdvancedChildParameter(BindableBalticMethod.AdvancedSett.AdvancedChildParameter advParam)
				{
					this.Header = advParam.Header;
					this.Name = advParam.Name;
					this.Value = advParam.Value;
					this.DefaultValue = advParam.DefaultValue;
					this.Unit = advParam.Unit;
					this.IsService = advParam.IsService;
				}

				// Token: 0x060008CD RID: 2253 RVA: 0x0004754C File Offset: 0x0004574C
				public AdvancedChildParameter(string header, string name, object value, object defaultValue, string unit, bool isService = false)
				{
					this.Header = header;
					this.Name = name;
					this.Value = value;
					this.DefaultValue = defaultValue;
					this.Unit = unit;
					this._isService = isService;
				}

				// Token: 0x040009FC RID: 2556
				private string _header = "";

				// Token: 0x040009FD RID: 2557
				private string _name = "";

				// Token: 0x040009FE RID: 2558
				private object _value;

				// Token: 0x040009FF RID: 2559
				private object _defaultValue;

				// Token: 0x04000A00 RID: 2560
				private string _unit;

				// Token: 0x04000A01 RID: 2561
				private bool _isService;
			}
		}

		// Token: 0x02000117 RID: 279
		public class ElutionType
		{
			// Token: 0x06000825 RID: 2085 RVA: 0x0003EBCC File Offset: 0x0003CDCC
			public ElutionType(string name, string displayName, string legacyName, bool isLegacy)
			{
			}

			// Token: 0x17000191 RID: 401
			// (get) Token: 0x06000826 RID: 2086 RVA: 0x0003EBF1 File Offset: 0x0003CDF1
			// (set) Token: 0x06000827 RID: 2087 RVA: 0x0003EBF9 File Offset: 0x0003CDF9
			public string Name { get; set; }

			// Token: 0x17000192 RID: 402
			// (get) Token: 0x06000828 RID: 2088 RVA: 0x0003EC02 File Offset: 0x0003CE02
			// (set) Token: 0x06000829 RID: 2089 RVA: 0x0003EC0A File Offset: 0x0003CE0A
			public string DisplayName { get; set; }

			// Token: 0x17000193 RID: 403
			// (get) Token: 0x0600082A RID: 2090 RVA: 0x0003EC13 File Offset: 0x0003CE13
			// (set) Token: 0x0600082B RID: 2091 RVA: 0x0003EC1B File Offset: 0x0003CE1B
			public string LegacyName { get; set; }

			// Token: 0x17000194 RID: 404
			// (get) Token: 0x0600082C RID: 2092 RVA: 0x0003EC24 File Offset: 0x0003CE24
			public bool HasLegacyOption
			{
				get
				{
					return !string.IsNullOrEmpty(this.LegacyName);
				}
			}

			// Token: 0x17000195 RID: 405
			// (get) Token: 0x0600082D RID: 2093 RVA: 0x0003EC34 File Offset: 0x0003CE34
			// (set) Token: 0x0600082E RID: 2094 RVA: 0x0003EC3C File Offset: 0x0003CE3C
			public bool IsLegacy { get; set; }
		}
	}
}
