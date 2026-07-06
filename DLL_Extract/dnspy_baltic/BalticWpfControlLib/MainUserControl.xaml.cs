// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using BalticClassLib;
using BalticWpfControlLib.Controls;
using BalticWpfControlLib.Diagram;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Bruker.Lc.Maintenance;
using Bruker.Lc.Metering;
using BrukerLC.Interfaces.ViewModelInterfaces;
using BrukerLC.Utils.Controls;
using Microsoft.CSharp.RuntimeBinder;

namespace BalticWpfControlLib
{
	// Token: 0x02000029 RID: 41
	public partial class MainUserControl : Canvas, INotifyPropertyChanged
	{
		private class _DC_172_1
		{
			public MainUserControl _cthis;
			public dynamic _closureLocals;
					public dynamic _locals1;
			public Action _mg_LoadAndBindColumns_Action_2;
			public dynamic list;
		}

		private class _DC_172_0
		{
			public MainUserControl _cthis;
			public dynamic _closureLocals;
					public dynamic analColumnName;
			public dynamic preColumnName;
		}

		// Auto-generated callsite cache class
		private static class _co_171
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
			public static dynamic _cp_3;
			public static dynamic _cp_4;
			public static dynamic _cp_5;
		}

		// Token: 0x14000021 RID: 33
		// (add) Token: 0x060001E8 RID: 488 RVA: 0x0000CDB4 File Offset: 0x0000AFB4
		// (remove) Token: 0x060001E9 RID: 489 RVA: 0x0000CDEC File Offset: 0x0000AFEC
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x060001EA RID: 490 RVA: 0x0000CE21 File Offset: 0x0000B021
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060001EB RID: 491 RVA: 0x0000CE3A File Offset: 0x0000B03A
		// (set) Token: 0x060001EC RID: 492 RVA: 0x0000CE42 File Offset: 0x0000B042
		public double OvenTemp
		{
			get
			{
				return this._ovenTemp;
			}
			set
			{
				this._ovenTemp = value;
				this.NotifyPropertyChanged("OvenTemp");
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060001ED RID: 493 RVA: 0x0000CE56 File Offset: 0x0000B056
		public string OvenTempTipTxt
		{
			get
			{
				return "All";
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060001EE RID: 494 RVA: 0x0000CE5D File Offset: 0x0000B05D
		// (set) Token: 0x060001EF RID: 495 RVA: 0x0000CE65 File Offset: 0x0000B065
		public double OvenTempSetPt
		{
			get
			{
				return this._ovenSetPtTemp;
			}
			set
			{
				this._ovenSetPtTemp = value;
				this.NotifyPropertyChanged("OvenTempSetPt");
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060001F0 RID: 496 RVA: 0x0000CE79 File Offset: 0x0000B079
		// (set) Token: 0x060001F1 RID: 497 RVA: 0x0000CE81 File Offset: 0x0000B081
		public bool IsUseIdleFlow
		{
			get
			{
				return this._isUseIdleFlow;
			}
			set
			{
				this._isUseIdleFlow = value;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060001F2 RID: 498 RVA: 0x0000CE8A File Offset: 0x0000B08A
		// (set) Token: 0x060001F3 RID: 499 RVA: 0x0000CE92 File Offset: 0x0000B092
		public BalticPreferences Preferences
		{
			get
			{
				return this._preferences;
			}
			set
			{
				this._preferences = value;
				base.Dispatcher.BeginInvoke(new Action(delegate
				{
					this.idleLED.Visibility = ((this._preferences.Pump.IsIdleFlowOnStandby || (this._preferences.Pump.IsIdleFlowOnError && this.IsClearErrorable)) ? Visibility.Visible : Visibility.Hidden);
					this.idleFlowViaTrapText.Text = (this.Preferences.Pump.IsViaTrap ? "On" : "Off");
				}), Array.Empty<object>());
			}
		}

		// Token: 0x14000022 RID: 34
		// (add) Token: 0x060001F4 RID: 500 RVA: 0x0000CEB8 File Offset: 0x0000B0B8
		// (remove) Token: 0x060001F5 RID: 501 RVA: 0x0000CEF0 File Offset: 0x0000B0F0
		public event MainUserControl.BalticColumnSelectionEventHandler OnColumnSelection;

		// Token: 0x14000023 RID: 35
		// (add) Token: 0x060001F6 RID: 502 RVA: 0x0000CF28 File Offset: 0x0000B128
		// (remove) Token: 0x060001F7 RID: 503 RVA: 0x0000CF60 File Offset: 0x0000B160
		public event MainUserControl.UpdateHyStarStatusEventHandler UpdateHyStarStatusEvent;

		// Token: 0x14000024 RID: 36
		// (add) Token: 0x060001F8 RID: 504 RVA: 0x0000CF98 File Offset: 0x0000B198
		// (remove) Token: 0x060001F9 RID: 505 RVA: 0x0000CFD0 File Offset: 0x0000B1D0
		public event MainUserControl.MaintenanceExecutionReport MaintenanceExecutionReportEvent;

		// Token: 0x14000025 RID: 37
		// (add) Token: 0x060001FA RID: 506 RVA: 0x0000D008 File Offset: 0x0000B208
		// (remove) Token: 0x060001FB RID: 507 RVA: 0x0000D040 File Offset: 0x0000B240
		public event MainUserControl.ConfirmButtonOnErrorCallback ConfirmButtonEvent;

		// Token: 0x14000026 RID: 38
		// (add) Token: 0x060001FC RID: 508 RVA: 0x0000D078 File Offset: 0x0000B278
		// (remove) Token: 0x060001FD RID: 509 RVA: 0x0000D0B0 File Offset: 0x0000B2B0
		public event MainUserControl.ShowPreferences OnShowPreferences;

		// Token: 0x14000027 RID: 39
		// (add) Token: 0x060001FE RID: 510 RVA: 0x0000D0E8 File Offset: 0x0000B2E8
		// (remove) Token: 0x060001FF RID: 511 RVA: 0x0000D120 File Offset: 0x0000B320
		public event MainUserControl.ShowLogBook OnShowLogBook;

		// Token: 0x14000028 RID: 40
		// (add) Token: 0x06000200 RID: 512 RVA: 0x0000D158 File Offset: 0x0000B358
		// (remove) Token: 0x06000201 RID: 513 RVA: 0x0000D190 File Offset: 0x0000B390
		public event EventHandler EndTempWait;

		// Token: 0x14000029 RID: 41
		// (add) Token: 0x06000202 RID: 514 RVA: 0x0000D1C8 File Offset: 0x0000B3C8
		// (remove) Token: 0x06000203 RID: 515 RVA: 0x0000D200 File Offset: 0x0000B400
		public event EventHandler ResetSystem;

		// Token: 0x1400002A RID: 42
		// (add) Token: 0x06000204 RID: 516 RVA: 0x0000D238 File Offset: 0x0000B438
		// (remove) Token: 0x06000205 RID: 517 RVA: 0x0000D270 File Offset: 0x0000B470
		public event EventHandler ClearError;

		// Token: 0x1400002B RID: 43
		// (add) Token: 0x06000206 RID: 518 RVA: 0x0000D2A8 File Offset: 0x0000B4A8
		// (remove) Token: 0x06000207 RID: 519 RVA: 0x0000D2E0 File Offset: 0x0000B4E0
		public event MainUserControl.AbortInjectionEventHandler AbortInjection;

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000208 RID: 520 RVA: 0x0000D315 File Offset: 0x0000B515
		public bool IsConnected
		{
			get
			{
				return this._instrument.IsConnected;
			}
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000D324 File Offset: 0x0000B524
		static MainUserControl()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MainUserControl), new FrameworkPropertyMetadata(typeof(MainUserControl)));
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000D388 File Offset: 0x0000B588
		public MainUserControl(BalticInstrumentFacade instrument, List<BalticHWProfile.CapillaryItem> capillaries, string displayName, string version, string productVersion, string creationDate, List<BalticPreferences.CapillaryPreference> prefCapillaries, bool isOvenInstalled, bool displayPressureAsPsi)
		{
			this._displayName = displayName;
			this._versionText = Utility.CreateDisplayVersion(version);
			this._productVersion = productVersion;
			this._creationDate = creationDate;
			this._isPressurePSI = displayPressureAsPsi;
			this._instrument = instrument;
			this._capillaries = capillaries;
			this._prefCapillaries = prefCapillaries;
			this._isOvenInstalled = isOvenInstalled;
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000D460 File Offset: 0x0000B660
		private void TransformChannels(IEnumerable<IMeteringChannel> channels, List<IMeteringChannel> collection, bool psi)
		{
			foreach (IMeteringChannel meteringChannel in channels)
			{
				MeteringChannelInfo info = meteringChannel.ChannelInfo;
				IMeteringChannel chan = meteringChannel;
				if (psi && "bar".Equals(info.Unit, StringComparison.InvariantCultureIgnoreCase))
				{
					info = new MeteringChannelInfo(info.Id.Name, info.Id.Source, "psi", info.DisplayDecimals, info.ValueType, info.IsDiagnostic, info.IsSevice);
					chan = new TransformingMeteringChannel(chan, info, new Func<MeteringDataPoint, MeteringDataPoint>(this.Bars2Psi));
				}
				collection.Add(chan);
			}
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000D514 File Offset: 0x0000B714
		private MeteringDataPoint Bars2Psi(MeteringDataPoint mdp)
		{
			return new MeteringDataPoint(mdp.Timestamp, (double)mdp.Value / 0.0689475729);
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x0600020E RID: 526 RVA: 0x0000D8A0 File Offset: 0x0000BAA0
		// (set) Token: 0x0600020D RID: 525 RVA: 0x0000D53C File Offset: 0x0000B73C
		public BalticInstrumentFacade InstrumentFacade
		{
			get
			{
				return this._instrument;
			}
			set
			{
				if (this._instrument != null)
				{
					this._instrument.ExecutionStateChanged -= this._instrument_ExecutionStateChanged;
					this._instrument.DecompressExecutionStateChanged -= this.Decompress_ExecutionStateChanged;
					(this._instrument.SystemConditions as INotifyCollectionChanged).CollectionChanged -= this.MainUserControl_ConditionsChanged;
					foreach (IMeteringChannel channel in this._channels)
					{
						MeteringChannelId id = channel.ChannelInfo.Id;
						if (id.Source.ToString().StartsWith("pump"))
						{
							channel.ChannelDataChanged -= this.pump_ChannelDataChanged;
						}
						else if ((id.Source.ToString().StartsWith("flowsensor") && id.Name.Equals("flow")) || (id.Source.ToString().StartsWith("oven") && id.Name.Equals("temperature")) || (id.Source.ToString().StartsWith("oven") && id.Name.Equals("setpoint")))
						{
							channel.ChannelDataChanged -= this.sensor_ChannelDataChanged;
						}
					}
					this._channels.Clear();
				}
				this._instrument = value;
				if (this._instrument != null)
				{
					this.TransformChannels(this._instrument.GaugeChannels, this._channels, this._isPressurePSI);
					foreach (IMeteringChannel channel2 in this._channels)
					{
						MeteringChannelId id2 = channel2.ChannelInfo.Id;
						if (id2.Source.ToString().StartsWith("pump"))
						{
							channel2.ChannelDataChanged += this.pump_ChannelDataChanged;
						}
						else if ((id2.Source.ToString().StartsWith("flowsensor") && id2.Name.Equals("flow")) || (id2.Source.ToString().StartsWith("oven") && id2.Name.Equals("temperature")) || (id2.Source.ToString().StartsWith("oven") && id2.Name.Equals("setpoint")))
						{
							channel2.ChannelDataChanged += this.sensor_ChannelDataChanged;
						}
					}
					this.MainUserControl_ConditionsChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					(this._instrument.SystemConditions as INotifyCollectionChanged).CollectionChanged += this.MainUserControl_ConditionsChanged;
					this._instrument.ExecutionStateChanged += this._instrument_ExecutionStateChanged;
					this._instrument.DecompressExecutionStateChanged += this.Decompress_ExecutionStateChanged;
					this._instrument.IdleFlowRunningEvent += this._instrument_IdleFlowRunningEvent;
					this._instrument.ActiveConditionClearEvent += this._instrument_ActiveConditionClearEvent;
					this._instrument.ShowCompositionConditions.ShowCompositionCollectionChanged += this.scc_ShowCompositionCollectionChanged;
				}
			}
		}

		// Token: 0x0600020F RID: 527 RVA: 0x0000D8A8 File Offset: 0x0000BAA8
		private void scc_ShowCompositionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.CheckShowCompositionConditions();
		}

		// Token: 0x06000210 RID: 528 RVA: 0x0000D8B0 File Offset: 0x0000BAB0
		private void Init(object sender, EventArgs e)
		{
			this.ConfigureDataContexts();
			this.txtDeviceStatus.Visibility = Visibility.Hidden;
			this.tbFlow.Text = "-";
			this.tbPercentB.Text = "-";
			this.tbOvenTemp.Text = "- °C";
			this.tbTempSetPt.Text = "- °C";
			this.tbTempMeasured.Text = "- °C";
			this.imgHeatWaves.Opacity = 0.4;
			this.tbOvenTemp.Opacity = 0.4;
			this.menuitemResetSystem.Header = string.Format("Reset {0}", this._displayName);
			this.UpdateMixColor(0);
			this.DrawConnections();
			this.CheckShowCompositionConditions();
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0000D978 File Offset: 0x0000BB78
		private void _instrument_IdleFlowRunningEvent(bool isRunning, bool isErrorState = false)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				if (this.Preferences.Pump.IsIdleFlowOnStandby || this.Preferences.Pump.IsIdleFlowOnError)
				{
					this.idleFlowPopupText.Text = (isRunning ? "Idle Flow Enabled" : "Idle Flow Disabled");
					this.idleLED.IsActive = new bool?(isRunning);
					if (isRunning)
					{
						this.idleFlowText.Text = string.Format(CultureInfo.InvariantCulture, "{0:0.0} µL/min", this.Preferences.Pump.StandbyFlow);
						this.idleFlowCompText.Text = string.Format(CultureInfo.InvariantCulture, "{0} %", this.Preferences.Pump.Composition);
						this.idleFlowViaTrapText.Text = (this.Preferences.Pump.IsViaTrap ? "On" : "Off");
					}
					else
					{
						this.idleFlowText.Text = "-";
						this.idleFlowCompText.Text = "-";
						this.idleFlowViaTrapText.Text = "-";
					}
					if (this.Preferences.Pump.IsIdleFlowOnError & isErrorState)
					{
						this.idleLED.Visibility = Visibility.Visible;
					}
				}
			}), Array.Empty<object>());
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0000D9BD File Offset: 0x0000BBBD
		private void Decompress_ExecutionStateChanged(object sender, ExecutionStateChangedEventArgs e)
		{
			this.UpdateHyStarStatus(e);
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0000D9C8 File Offset: 0x0000BBC8
		private void _instrument_ExecutionStateChanged(object sender, ExecutionStateChangedEventArgs e)
		{
			if (e.IsHidden && !this.IsElutionProcedure(e.Procedure) && !this.IsEquilibrationProcedure(e.Procedure))
			{
				return;
			}
			this._lastExecutionState = e;
			if (!this.IsElutionProcedure(e.Procedure))
			{
				this.UpdateHyStarStatus(e);
			}
			ProcedureExecutionState executionState = e.ExecutionState;
			if (executionState != ProcedureExecutionState.Started)
			{
				if (executionState - ProcedureExecutionState.Completed <= 1)
				{
					this.IsPreferenceable = true;
				}
			}
			else
			{
				this.IsPreferenceable = false;
			}
			bool enabled = e.ExecutionState > ProcedureExecutionState.Started;
			Action a = delegate
			{
				this.SetColumnSelectionEnabled(enabled);
			};
			base.Dispatcher.BeginInvoke(a, Array.Empty<object>());
		}

		// Token: 0x06000214 RID: 532 RVA: 0x0000DA78 File Offset: 0x0000BC78
		private void UpdateHyStarStatus(ExecutionStateChangedEventArgs e)
		{
			ProcedureExecutionState executionState = e.ExecutionState;
			if (executionState != ProcedureExecutionState.Started)
			{
				if (executionState - ProcedureExecutionState.Completed > 1)
				{
					return;
				}
				MainUserControl.UpdateHyStarStatusEventHandler updateHyStarStatusEvent = this.UpdateHyStarStatusEvent;
				if (updateHyStarStatusEvent == null)
				{
					return;
				}
				updateHyStarStatusEvent(!e.IsDecompressOnExit, e.Procedure, e.Version);
				return;
			}
			else
			{
				MainUserControl.UpdateHyStarStatusEventHandler updateHyStarStatusEvent2 = this.UpdateHyStarStatusEvent;
				if (updateHyStarStatusEvent2 == null)
				{
					return;
				}
				updateHyStarStatusEvent2(false, e.Procedure, e.Version);
				return;
			}
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0000DADC File Offset: 0x0000BCDC
		private bool IsElutionProcedure(string procedureName)
		{
			return this._instrument.ElutionTypeInfoList.Find((ProcedureInfo x) => x.Name == procedureName) != null;
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000DB15 File Offset: 0x0000BD15
		private bool IsEquilibrationProcedure(string procedureName)
		{
			return this._instrument.GetEquilibrationProcedure(procedureName) != null;
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0000DB28 File Offset: 0x0000BD28
		private void ConfigureDataContexts()
		{
			SolidColorBrush blueBrush = new SolidColorBrush(global::System.Windows.Media.Color.FromRgb(15, 112, 184));
			SolidColorBrush redBrush = new SolidColorBrush(global::System.Windows.Media.Color.FromRgb(193, 41, 45));
			this.pumpAControl.DataContext = new PumpViewModel
			{
				ActiveBrush = blueBrush,
				IsActive = true
			};
			this.pumpBControl.DataContext = new PumpViewModel
			{
				ActiveBrush = redBrush,
				IsActive = true
			};
		}

		// Token: 0x06000218 RID: 536 RVA: 0x0000DB9C File Offset: 0x0000BD9C
		private void pump_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
		{
			if (e.Data != null)
			{
				IMeteringChannel channel = sender as IMeteringChannel;
				object src = channel.ChannelInfo.Id.Source;
				double value = (double)e.Data[e.Data.Length - 1].Value;
				Action a = delegate
				{
					IPumpViewModel pvm = (src.ToString().EndsWith("a") ? ((IPumpViewModel)this.pumpAControl.DataContext) : ((IPumpViewModel)this.pumpBControl.DataContext));
					if (pvm != null)
					{
						string name = channel.ChannelInfo.Id.Name;
						if (name == "pressure")
						{
							pvm.Pressure = value;
							pvm.PressureUnit = channel.ChannelInfo.Unit;
							return;
						}
						if (name == "relative Volume")
						{
							pvm.FillLevel = 100.0 - value;
							pvm.VolumeUsed = value / 100.0 * 1350.0;
							pvm.VolumeLeft = (100.0 - value) / 100.0 * 1350.0;
							pvm.VolumeUnit = "µL";
							return;
						}
						if (!(name == "speed"))
						{
							return;
						}
						pvm.ThroughputSetPoint = value;
						pvm.ThroughputUnit = "µL/min";
					}
				};
				base.Dispatcher.BeginInvoke(a, Array.Empty<object>());
			}
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0000DC28 File Offset: 0x0000BE28
		private void sensor_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
		{
			if (e.Data != null)
			{
				IMeteringChannel channel = sender as IMeteringChannel;
				string src = channel.ChannelInfo.Id.Source.ToString();
				double value = (double)e.Data[e.Data.Length - 1].Value;
				Action a = delegate
				{
					string name = channel.ChannelInfo.Id.Name;
					if (!(name == "flow"))
					{
						if (!(name == "temperature"))
						{
							if (!(name == "setpoint"))
							{
								return;
							}
							if (channel.ChannelInfo.Id.Source.ToString().StartsWith("oven"))
							{
								this.OvenTempSetPt = value;
								this.tbTempSetPt.Text = string.Format(CultureInfo.InvariantCulture, "{0:0} °C", value);
								if ((int)(this._ovenTemp * 10.0) == 0)
								{
									this.tbOvenTemp.Text = "- °C";
									this.tbTempSetPt.Text = "- °C";
									this.tbTempMeasured.Text = "- °C";
								}
							}
						}
						else if (channel.ChannelInfo.Id.Source.ToString().StartsWith("oven"))
						{
							this.OvenTemp = value;
							if ((int)(value * 10.0) != 0)
							{
								this.tbOvenTemp.Text = string.Format(CultureInfo.InvariantCulture, "{0:0} °C", Math.Round(value - 0.06));
								this.tbTempMeasured.Text = this.tbOvenTemp.Text;
								if (!this._isOvenInstalled)
								{
									this.imgHeatWaves.Opacity = 0.4;
									this.tbOvenTemp.Opacity = 0.4;
									return;
								}
								if ((int)(this.imgHeatWaves.Opacity * 10.0) < 9)
								{
									this.imgHeatWaves.Opacity = 1.0;
								}
								if ((int)(this.tbOvenTemp.Opacity * 10.0) < 9)
								{
									this.tbOvenTemp.Opacity = 1.0;
									return;
								}
							}
							else
							{
								if ((int)(this.imgHeatWaves.Opacity * 10.0) > 4)
								{
									this.imgHeatWaves.Opacity = 0.4;
								}
								if ((int)(this.tbOvenTemp.Opacity * 10.0) > 4)
								{
									this.tbOvenTemp.Opacity = 0.4;
									return;
								}
							}
						}
					}
					else
					{
						if (src.EndsWith("a"))
						{
							this.flowPumpA = ((value > 0.0) ? value : 0.0);
							this._flowADisp.Add(this.flowPumpA);
						}
						else
						{
							this.flowPumpB = ((value > 0.0) ? value : 0.0);
							this._flowBDisp.Add(this.flowPumpB);
						}
						if (this._flowADisp.nItems > 0 && this._flowBDisp.nItems > 0)
						{
							double flow = this.flowPumpA + this.flowPumpB;
							this._flowMixDisp.Add(flow);
							double mix = 0.0;
							bool isMixValid = true;
							if ((int)(this._flowADisp.Average * 10000.0) == 0 && (int)(this._flowBDisp.Average * 10000.0) == 0)
							{
								mix = 0.0;
							}
							else if ((int)(this._flowBDisp.Average * 10000.0) == 0)
							{
								mix = 0.0;
							}
							else if ((int)(this._flowADisp.Average * 10000.0) == 0)
							{
								mix = 1.0;
							}
							else if (this._flowMixDisp.nItems > 0)
							{
								mix = this._flowBDisp.Average / this._flowMixDisp.Average;
								if (mix > 1.0)
								{
									mix = 1.0;
								}
								else if (mix < 0.0)
								{
									mix = 0.0;
								}
							}
							else
							{
								isMixValid = false;
							}
							if (isMixValid)
							{
								this.tbPercentB.Text = string.Format(CultureInfo.InvariantCulture, "{0:P1}B", mix);
							}
							string unit = "µL/min";
							if (Math.Abs(this._flowMixDisp.Average) < 1.0)
							{
								unit = "nL/min";
							}
							double mixfFlowAve = this._flowMixDisp.Average;
							if (unit.Contains("nL/min"))
							{
								mixfFlowAve *= 1000.0;
							}
							if (unit == "nL/min")
							{
								this.tbFlow.Text = string.Format((Math.Abs(mixfFlowAve) < 10.0) ? "{0:F1} {1}" : "{0:F0} {1}", mixfFlowAve, unit);
							}
							else
							{
								this.tbFlow.Text = string.Format(CultureInfo.InvariantCulture, "{0:F2} {1}", mixfFlowAve, unit);
							}
							this.UpdateMixColor((int)(mix * 100.0));
							return;
						}
					}
				};
				base.Dispatcher.BeginInvoke(a, Array.Empty<object>());
			}
		}

		// Token: 0x0600021A RID: 538 RVA: 0x0000DCB8 File Offset: 0x0000BEB8
		private void MainUserControl_ConditionsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.InstrumentFacade.IsConnected)
			{
				this.SetActivateAuxiliaryControlEnabled(true);
			}
			IEnumerable<SystemCondition> conditions = this._instrument.SystemConditions;
			SystemCondition alertCondition = null;
			foreach (SystemCondition condition in conditions)
			{
				if (condition.Severity.Equals(Severity.Error))
				{
					alertCondition = condition;
				}
			}
			if (alertCondition == null)
			{
				foreach (SystemCondition condition2 in conditions)
				{
					if (condition2.Severity.Equals(Severity.Warn))
					{
						alertCondition = condition2;
					}
				}
			}
			this.SetAlertMessage(alertCondition);
		}

		// Token: 0x1400002C RID: 44
		// (add) Token: 0x0600021B RID: 539 RVA: 0x0000DD98 File Offset: 0x0000BF98
		// (remove) Token: 0x0600021C RID: 540 RVA: 0x0000DDD0 File Offset: 0x0000BFD0
		public event EventHandler ActivateAuxiliaryControl;

		// Token: 0x0600021D RID: 541 RVA: 0x0000DE08 File Offset: 0x0000C008
		private void ButtonClicked(object sender, RoutedEventArgs e)
		{
			EventHandler activateAuxiliaryControl = this.ActivateAuxiliaryControl;
			if (activateAuxiliaryControl != null)
			{
				activateAuxiliaryControl(this, EventArgs.Empty);
			}
			if (this.dlgLCControl != null)
			{
				this.dlgLCControl.Show();
				if (this.dlgLCControl.WindowState == WindowState.Minimized)
				{
					this.dlgLCControl.WindowState = WindowState.Normal;
				}
				this.dlgLCControl.Focus();
				return;
			}
			this.dlgLCControl = new LCControlWindow(this._instrument, this._capillaries, this._prefCapillaries, this._displayName, this._isPressurePSI, this._isOvenInstalled, this._lastExecutionState);
			this.dlgLCControl.ExecutionReportEvent += this.dlgLCControl_ExecutionReportEvent;
			this.dlgLCControl.ConfirmButtonEvent += this.DlgLCControl_ConfirmButtonEvent;
			this.dlgLCControl.Closing += this.dlgLCControl_Closing;
			this.UpdateLCControlColumnTypes(Column.ColumnType.Both);
			this.dlgLCControl.Show();
			this.dlgLCControl.Focus();
		}

		// Token: 0x0600021E RID: 542 RVA: 0x0000DF00 File Offset: 0x0000C100
		private void DlgLCControl_ConfirmButtonEvent(SystemCondition condition)
		{
			MainUserControl.ConfirmButtonOnErrorCallback confirmButtonEvent = this.ConfirmButtonEvent;
			if (confirmButtonEvent == null)
			{
				return;
			}
			confirmButtonEvent(condition);
		}

		// Token: 0x0600021F RID: 543 RVA: 0x0000DF13 File Offset: 0x0000C113
		private void dlgLCControl_ExecutionReportEvent(ExecutionStateChangedEventArgs e)
		{
			MainUserControl.MaintenanceExecutionReport maintenanceExecutionReportEvent = this.MaintenanceExecutionReportEvent;
			if (maintenanceExecutionReportEvent == null)
			{
				return;
			}
			maintenanceExecutionReportEvent(e);
		}

		// Token: 0x06000220 RID: 544 RVA: 0x0000DF26 File Offset: 0x0000C126
		public void AbortActiveProcedure()
		{
			LCControlWindow lccontrolWindow = this.dlgLCControl;
			if (lccontrolWindow == null)
			{
				return;
			}
			lccontrolWindow.AbortActiveProcedure();
		}

		// Token: 0x06000221 RID: 545 RVA: 0x0000DF38 File Offset: 0x0000C138
		private void dlgLCControl_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
			this.dlgLCControl.Hide();
		}

		// Token: 0x06000222 RID: 546 RVA: 0x0000DF4C File Offset: 0x0000C14C
		public void SetActivateAuxiliaryControlEnabled(bool enabled)
		{
			Action action = delegate
			{
				this.btnLCControl.IsEnabled = enabled;
				if (!enabled && this.dlgLCControl != null)
				{
					this.dlgLCControl.Closing -= this.dlgLCControl_Closing;
					this.dlgLCControl.Close();
					this.dlgLCControl = null;
				}
			};
			base.Dispatcher.Invoke(action);
		}

		// Token: 0x06000223 RID: 547 RVA: 0x0000DF84 File Offset: 0x0000C184
		public void SetStatusBar(global::System.Drawing.Color newColor, global::System.Drawing.Color newTextColor, string strText)
		{
			Action action = delegate
			{
				this.SetStatusBarColor(newColor, newTextColor);
				if (strText != "")
				{
					this.txtHyStarStatus.Text = ((this._versionText == "") ? string.Format("Bruker {0} ({1})", this._displayName, strText) : string.Format("Bruker {0} {1} ({2})", this._displayName, this._versionText, strText));
				}
			};
			base.Dispatcher.Invoke(action);
		}

		// Token: 0x06000224 RID: 548 RVA: 0x0000DFCC File Offset: 0x0000C1CC
		public void SetStatusBarText(string strText)
		{
			Action action = delegate
			{
				if (strText != "")
				{
					this.txtHyStarStatus.Text = ((this._versionText == "") ? string.Format("Bruker {0} ({1})", this._displayName, strText) : string.Format("Bruker {0} {1} ({2})", this._displayName, this._versionText, strText));
				}
			};
			base.Dispatcher.Invoke(action);
		}

		// Token: 0x06000225 RID: 549 RVA: 0x0000E004 File Offset: 0x0000C204
		public void SetStatusBarColor(global::System.Drawing.Color newColor, global::System.Drawing.Color newTextColor)
		{
			if (this.txtHyStarStatus.Dispatcher.CheckAccess())
			{
				global::System.Windows.Media.Brush wpfBGBrush = new SolidColorBrush(global::System.Windows.Media.Color.FromRgb(newColor.R, newColor.G, newColor.B));
				global::System.Windows.Media.Brush wpfFGBrush = new SolidColorBrush(global::System.Windows.Media.Color.FromRgb(newTextColor.R, newTextColor.G, newTextColor.B));
				this.txtHyStarStatus.Background = wpfBGBrush;
				this.txtHyStarStatus.Foreground = wpfFGBrush;
				this.bdrDeviceStatus.Background = wpfBGBrush;
				return;
			}
			MainUserControl.SetStatusBarColorCallback del = new MainUserControl.SetStatusBarColorCallback(this.SetStatusBarColor);
			this.txtHyStarStatus.Dispatcher.BeginInvoke(del, new object[] { newColor, newTextColor });
		}

		// Token: 0x06000226 RID: 550 RVA: 0x0000E0C0 File Offset: 0x0000C2C0
		public void SetDeviceStatusBar(global::System.Drawing.Color newColor, string strText)
		{
			if (!this.txtDeviceStatus.Dispatcher.CheckAccess())
			{
				MainUserControl.SetDeviceStatusBarCallback del = new MainUserControl.SetDeviceStatusBarCallback(this.SetDeviceStatusBar);
				this.txtDeviceStatus.Dispatcher.BeginInvoke(del, new object[] { newColor, strText });
				return;
			}
			global::System.Windows.Media.Brush wpfBGBrush = new SolidColorBrush(global::System.Windows.Media.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B));
			global::System.Windows.Media.Brush wpfFGBrush = new SolidColorBrush(Colors.Black);
			this.txtDeviceStatus.Background = wpfBGBrush;
			this.txtDeviceStatus.Foreground = wpfFGBrush;
			if (wpfBGBrush == global::System.Windows.Media.Brushes.Red)
			{
				this.txtDeviceStatus.Foreground = new SolidColorBrush(global::System.Windows.Media.Color.FromArgb(byte.MaxValue, 250, byte.MaxValue, 250));
			}
			if (wpfBGBrush == global::System.Windows.Media.Brushes.Red)
			{
				this.gridMain.Background = base.Resources["ErrorBackgroundBrush"] as LinearGradientBrush;
				this.gridMain.Background.Opacity = 0.3;
				return;
			}
			this.gridMain.Background = new SolidColorBrush(Colors.Transparent);
		}

		// Token: 0x06000227 RID: 551 RVA: 0x0000E1E8 File Offset: 0x0000C3E8
		public void SetAlertMessage(SystemCondition condition)
		{
			if (!this.txtDeviceStatus.Dispatcher.CheckAccess())
			{
				MainUserControl.SetAlertMessageCallback del = new MainUserControl.SetAlertMessageCallback(this.SetAlertMessage);
				this.txtDeviceStatus.Dispatcher.BeginInvoke(del, new object[] { condition });
				return;
			}
			if (condition == null)
			{
				this.txtDeviceStatus.Visibility = Visibility.Hidden;
				this.gridMain.Background = new SolidColorBrush(Colors.Transparent);
				this.imgLCControlBtn.Source = new ImageConverter().Convert("Images/Settings_32.png") as ImageSource;
				this.btnLCControl.Style = base.Resources["BalticButtonStyle"] as Style;
				return;
			}
			LCControlMessage message = new LCControlMessage(condition);
			if (message.Type == LCControlMessage.MaintenanceType.Error)
			{
				this.gridMain.Background = base.Resources["ErrorBackgroundBrush"] as LinearGradientBrush;
				this.gridMain.Background.Opacity = 0.3;
				this.imgLCControlBtn.Source = new ImageConverter().Convert(message.ImageSource) as ImageSource;
				this.btnLCControl.Style = base.Resources["ErrorButtonStyle"] as Style;
				this.txtDeviceStatus.Background = base.Resources["ErrorBackgroundBrush"] as LinearGradientBrush;
				if (this.InjectionAbortable)
				{
					MainUserControl.AbortInjectionEventHandler abortInjection = this.AbortInjection;
					if (abortInjection != null)
					{
						abortInjection(false, this.InstrumentFacade.IsSkipVialAndContinue);
					}
				}
			}
			else if (message.Type == LCControlMessage.MaintenanceType.Warning)
			{
				this.gridMain.Background = base.Resources["WarningBackgroundBrush"] as LinearGradientBrush;
				this.gridMain.Background.Opacity = 0.3;
				this.imgLCControlBtn.Source = new ImageConverter().Convert(message.ImageSource) as ImageSource;
				this.btnLCControl.Style = base.Resources["WarningButtonStyle"] as Style;
				this.txtDeviceStatus.Background = base.Resources["WarningBackgroundBrush"] as LinearGradientBrush;
			}
			else if (message.Type == LCControlMessage.MaintenanceType.Tip)
			{
				this.gridMain.Background = base.Resources["TipBackgroundBrush"] as LinearGradientBrush;
				this.gridMain.Background.Opacity = 0.3;
				this.imgLCControlBtn.Source = new ImageConverter().Convert(message.ImageSource) as ImageSource;
				this.btnLCControl.Style = base.Resources["TipButtonStyle"] as Style;
				this.txtDeviceStatus.Background = base.Resources["TipBackgroundBrush"] as LinearGradientBrush;
			}
			else
			{
				this.gridMain.Background = base.Resources["InfoBackgroundBrush"] as LinearGradientBrush;
				this.gridMain.Background.Opacity = 0.3;
				this.imgLCControlBtn.Source = new ImageConverter().Convert(message.ImageSource) as ImageSource;
				this.btnLCControl.Style = base.Resources["InfoButtonStyle"] as Style;
				this.txtDeviceStatus.Background = base.Resources["InfoBackgroundBrush"] as LinearGradientBrush;
			}
			this.tbAlert.Text = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", condition.Subject, condition.Description);
			this.tbAlert.ToolTip = this.tbAlert.Text;
			this.imgAlert.Source = new ImageConverter().Convert(message.ImageSource) as ImageSource;
			this.txtDeviceStatus.Visibility = Visibility.Visible;
		}

		// Token: 0x06000228 RID: 552 RVA: 0x0000E5C9 File Offset: 0x0000C7C9
		private void _instrument_ActiveConditionClearEvent()
		{
			this.SetAlertMessage(null);
		}

		// Token: 0x06000229 RID: 553 RVA: 0x0000E5D4 File Offset: 0x0000C7D4
		public void SetSamplePosition(string samplePos)
		{
			if (this.tbSamplePos.Dispatcher.CheckAccess())
			{
				this.tbSamplePos.Text = samplePos ?? "-";
				return;
			}
			MainUserControl.SetSamplePositionCallback del = new MainUserControl.SetSamplePositionCallback(this.SetSamplePosition);
			this.tbSamplePos.Dispatcher.BeginInvoke(del, new object[] { samplePos });
		}

		// Token: 0x0600022A RID: 554 RVA: 0x0000E632 File Offset: 0x0000C832
		public void ClickConfirmButton()
		{
			if (this.dlgLCControl != null)
			{
				this.dlgLCControl.ClickConfirmButton();
				return;
			}
			this.InstrumentFacade.ActiveConditionClear();
		}

		// Token: 0x0600022B RID: 555 RVA: 0x0000E653 File Offset: 0x0000C853
		private void SetColumnSelectionEnabled(bool enabled)
		{
			this.comboAnalColumn.IsEnabled = enabled;
			this.comboPreColumn.IsEnabled = enabled;
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0000E66D File Offset: 0x0000C86D
		private void MenuItemAbortInjection_Click(object sender, ExecutedRoutedEventArgs e)
		{
			this.IsManualInjectionAbort = true;
			MainUserControl.AbortInjectionEventHandler abortInjection = this.AbortInjection;
			if (abortInjection == null)
			{
				return;
			}
			abortInjection(true, this.InstrumentFacade.IsSkipVialAndContinue);
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0000E692 File Offset: 0x0000C892
		private void cb_AbortCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.injectionAbortable;
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x0600022E RID: 558 RVA: 0x0000E6A2 File Offset: 0x0000C8A2
		// (set) Token: 0x0600022F RID: 559 RVA: 0x0000E6AC File Offset: 0x0000C8AC
		public bool InjectionAbortable
		{
			get
			{
				return this.injectionAbortable;
			}
			set
			{
				this.injectionAbortable = value;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x06000230 RID: 560 RVA: 0x0000E6B7 File Offset: 0x0000C8B7
		// (set) Token: 0x06000231 RID: 561 RVA: 0x0000E6C1 File Offset: 0x0000C8C1
		public bool IsManualInjectionAbort
		{
			get
			{
				return this.manualInjectionAbort;
			}
			set
			{
				this.manualInjectionAbort = value;
			}
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000233 RID: 563 RVA: 0x0000E6D7 File Offset: 0x0000C8D7
		// (set) Token: 0x06000232 RID: 562 RVA: 0x0000E6CC File Offset: 0x0000C8CC
		public bool IsIdleFlowActive
		{
			get
			{
				return this.idleFlowActive;
			}
			set
			{
				this.idleFlowActive = value;
			}
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000235 RID: 565 RVA: 0x0000E6EC File Offset: 0x0000C8EC
		// (set) Token: 0x06000234 RID: 564 RVA: 0x0000E6E1 File Offset: 0x0000C8E1
		public bool IsEndTempWaitActive
		{
			get
			{
				return this.isEndTempWaitActive;
			}
			set
			{
				this.isEndTempWaitActive = value;
			}
		}

		// Token: 0x06000236 RID: 566 RVA: 0x0000E6F6 File Offset: 0x0000C8F6
		private void MenuItemEndTempWaitCommand_Click(object sender, ExecutedRoutedEventArgs e)
		{
			EventHandler endTempWait = this.EndTempWait;
			if (endTempWait == null)
			{
				return;
			}
			endTempWait(this, EventArgs.Empty);
		}

		// Token: 0x06000237 RID: 567 RVA: 0x0000E70E File Offset: 0x0000C90E
		private void cb_EndTempWaitCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.isEndTempWaitActive;
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000238 RID: 568 RVA: 0x0000E71E File Offset: 0x0000C91E
		// (set) Token: 0x06000239 RID: 569 RVA: 0x0000E728 File Offset: 0x0000C928
		public bool SystemResettable
		{
			get
			{
				return this.systemResettable;
			}
			set
			{
				this.systemResettable = value;
			}
		}

		// Token: 0x0600023A RID: 570 RVA: 0x0000E733 File Offset: 0x0000C933
		private void MenuItemResetSystemCommand_Click(object sender, ExecutedRoutedEventArgs e)
		{
			EventHandler resetSystem = this.ResetSystem;
			if (resetSystem == null)
			{
				return;
			}
			resetSystem(this, EventArgs.Empty);
		}

		// Token: 0x0600023B RID: 571 RVA: 0x0000E74B File Offset: 0x0000C94B
		private void cb_ResetSystemCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.systemResettable;
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x0600023C RID: 572 RVA: 0x0000E75B File Offset: 0x0000C95B
		// (set) Token: 0x0600023D RID: 573 RVA: 0x0000E765 File Offset: 0x0000C965
		public bool IsClearErrorable
		{
			get
			{
				return this.isClearErrorable;
			}
			set
			{
				this.isClearErrorable = value;
			}
		}

		// Token: 0x0600023E RID: 574 RVA: 0x0000E770 File Offset: 0x0000C970
		private void MenuItemClearErrorCommand_Click(object sender, ExecutedRoutedEventArgs e)
		{
			EventHandler clearError = this.ClearError;
			if (clearError == null)
			{
				return;
			}
			clearError(this, EventArgs.Empty);
		}

		// Token: 0x0600023F RID: 575 RVA: 0x0000E788 File Offset: 0x0000C988
		private void cb_ClearErrorCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.isClearErrorable;
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000240 RID: 576 RVA: 0x0000E798 File Offset: 0x0000C998
		// (set) Token: 0x06000241 RID: 577 RVA: 0x0000E7A2 File Offset: 0x0000C9A2
		public bool IsPreferenceable
		{
			get
			{
				return this.isPreferenceable;
			}
			set
			{
				this.isPreferenceable = value;
			}
		}

		// Token: 0x06000242 RID: 578 RVA: 0x0000E7AD File Offset: 0x0000C9AD
		private void cb_PreferencesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.isPreferenceable;
		}

		// Token: 0x06000243 RID: 579 RVA: 0x0000E7BD File Offset: 0x0000C9BD
		private void MenuItemPreferences_Click(object sender, EventArgs e)
		{
			MainUserControl.ShowPreferences onShowPreferences = this.OnShowPreferences;
			if (onShowPreferences == null)
			{
				return;
			}
			onShowPreferences();
		}

		// Token: 0x06000244 RID: 580 RVA: 0x0000E7CF File Offset: 0x0000C9CF
		private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
		{
			new AboutWindow(this._displayName, this._versionText, this._productVersion, this._creationDate).ShowDialog(HelperExtensions.GetActiveWindow());
		}

		// Token: 0x06000245 RID: 581 RVA: 0x0000E7F9 File Offset: 0x0000C9F9
		private void MenuItemShowLogbook_Click(object sender, EventArgs e)
		{
			MainUserControl.ShowLogBook onShowLogBook = this.OnShowLogBook;
			if (onShowLogBook == null)
			{
				return;
			}
			onShowLogBook();
		}

		// Token: 0x06000246 RID: 582 RVA: 0x0000E80C File Offset: 0x0000CA0C
		private void MenuItemClearAlert_Click(object sender, EventArgs e)
		{
			this.txtDeviceStatus.Visibility = Visibility.Hidden;
			this.gridMain.Background = new SolidColorBrush(Colors.Transparent);
			this.txtLCControlBtn.Text = "LC Control";
			this.imgLCControlBtn.Source = new ImageConverter().Convert("Images/Settings_32.png") as ImageSource;
			this.btnLCControl.Style = base.Resources["BalticButtonStyle"] as Style;
		}

		// Token: 0x06000247 RID: 583 RVA: 0x0000E88C File Offset: 0x0000CA8C
		private void DrawConnections()
		{
			if (MainUserControl._co_171._cp_1 == null)
			{
				MainUserControl._co_171._cp_1 = CallSite<Func<CallSite, object, ExpandoObject>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(ExpandoObject), typeof(MainUserControl)));
			}
			Func<CallSite, object, ExpandoObject> target = MainUserControl._co_171._cp_1.Target;
			CallSite _cpl = MainUserControl._co_171._cp_1;
			if (MainUserControl._co_171._cp_0 == null)
			{
				MainUserControl._co_171._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "DiagramHideList", typeof(MainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			ExpandoObject diagramHide = target(_cpl, MainUserControl._co_171._cp_0.Target(MainUserControl._co_171._cp_0, this._instrument.BalticConfiguration));
			int i;
			int j;
			for (i = 1; i <= diagramHide.Count<KeyValuePair<string, object>>(); i = j + 1)
			{
				try
				{
					string name = (string)diagramHide.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == i.ToString()).Value;
					if (MainUserControl._co_171._cp_5 == null)
					{
						MainUserControl._co_171._cp_5 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(MainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, bool> target2 = MainUserControl._co_171._cp_5.Target;
					CallSite _cp_2 = MainUserControl._co_171._cp_5;
					if (MainUserControl._co_171._cp_4 == null)
					{
						MainUserControl._co_171._cp_4 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Contains", null, typeof(MainUserControl), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, string, object, object> target3 = MainUserControl._co_171._cp_4.Target;
					CallSite _cp_3 = MainUserControl._co_171._cp_4;
					string text = name;
					if (MainUserControl._co_171._cp_3 == null)
					{
						MainUserControl._co_171._cp_3 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ValveT", typeof(MainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target4 = MainUserControl._co_171._cp_3.Target;
					CallSite _cp_4 = MainUserControl._co_171._cp_3;
					if (MainUserControl._co_171._cp_2 == null)
					{
						MainUserControl._co_171._cp_2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Naming", typeof(MainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					if (target2(_cp_2, target3(_cp_3, text, target4(_cp_4, MainUserControl._co_171._cp_2.Target(MainUserControl._co_171._cp_2, this._instrument.BalticConfiguration)))))
					{
						this.comboPreColumn.Visibility = Visibility.Hidden;
						this.tbPreColumn.Visibility = Visibility.Hidden;
						break;
					}
				}
				catch (Exception)
				{
				}
				j = i;
			}
			global::System.Windows.Point pumpAStartLoc = this.pumpAControl.TranslatePoint(new global::System.Windows.Point(this.pumpAControl.ActualWidth, this.pumpAControl.ActualHeight / 2.0), this.canvasMain);
			global::System.Windows.Point pumpBStartLoc = this.pumpBControl.TranslatePoint(new global::System.Windows.Point(this.pumpBControl.ActualWidth, this.pumpBControl.ActualHeight / 2.0), this.canvasMain);
			global::System.Windows.Point rightMix = this.plyMixTriangle.TranslatePoint(new global::System.Windows.Point(this.plyMixTriangle.ActualWidth, this.plyMixTriangle.ActualHeight / 2.0), this.canvasMain);
			global::System.Windows.Point leftMix = this.ellipseMixLeft.TranslatePoint(new global::System.Windows.Point(0.0, this.ellipseMixLeft.ActualHeight / 2.0), this.canvasMain);
			global::System.Windows.Point preColumnStartLoc = this.comboPreColumn.TranslatePoint(new global::System.Windows.Point(0.0, this.comboPreColumn.ActualHeight / 2.0), this.canvasMain);
			global::System.Windows.Point analColumnStartLoc = this.comboAnalColumn.TranslatePoint(new global::System.Windows.Point(0.0, this.comboAnalColumn.ActualHeight / 2.0), this.canvasMain);
			global::System.Windows.Point arrowStartLoc = this.imgSyringe.TranslatePoint(new global::System.Windows.Point(this.imgSyringe.ActualWidth / 2.0, this.imgSyringe.ActualHeight), this.canvasMain);
			PointCollection pumpAPointCollection = new PointCollection
			{
				new global::System.Windows.Point(pumpAStartLoc.X, pumpAStartLoc.Y),
				new global::System.Windows.Point(pumpAStartLoc.X + 15.0, pumpAStartLoc.Y),
				new global::System.Windows.Point(pumpAStartLoc.X + 15.0, pumpAStartLoc.Y + (pumpBStartLoc.Y - pumpAStartLoc.Y) / 2.0),
				new global::System.Windows.Point(pumpAStartLoc.X + 25.0, pumpAStartLoc.Y + (pumpBStartLoc.Y - pumpAStartLoc.Y) / 2.0)
			};
			RoundPolyline pumpAPolyline = new RoundPolyline
			{
				Stroke = new SolidColorBrush(global::System.Windows.Media.Color.FromRgb(15, 112, 184)),
				StrokeThickness = 4.0,
				FillRule = FillRule.EvenOdd,
				Points = pumpAPointCollection,
				StrokeLineJoin = PenLineJoin.Round,
				SnapsToDevicePixels = true,
				Radius = 6.0
			};
			this.canvasMain.Children.Add(pumpAPolyline);
			PointCollection pumpBPointCollection = new PointCollection
			{
				new global::System.Windows.Point(pumpBStartLoc.X, pumpBStartLoc.Y),
				new global::System.Windows.Point(pumpBStartLoc.X + 15.0, pumpBStartLoc.Y),
				new global::System.Windows.Point(pumpBStartLoc.X + 15.0, pumpBStartLoc.Y + (pumpAStartLoc.Y - pumpBStartLoc.Y) / 2.0),
				new global::System.Windows.Point(pumpBStartLoc.X + 25.0, pumpAStartLoc.Y + (pumpBStartLoc.Y - pumpAStartLoc.Y) / 2.0)
			};
			RoundPolyline pumpBPolyline = new RoundPolyline
			{
				Stroke = new SolidColorBrush(global::System.Windows.Media.Color.FromRgb(193, 41, 45)),
				StrokeThickness = 4.0,
				FillRule = FillRule.EvenOdd,
				Points = pumpBPointCollection,
				StrokeLineJoin = PenLineJoin.Round,
				SnapsToDevicePixels = true
			};
			pumpAPolyline.Radius = 6.0;
			this.canvasMain.Children.Add(pumpBPolyline);
			PointCollection mixPointCollectionLeft = new PointCollection
			{
				new global::System.Windows.Point(pumpBStartLoc.X + 20.0, pumpAStartLoc.Y + (pumpBStartLoc.Y - pumpAStartLoc.Y) / 2.0),
				new global::System.Windows.Point(leftMix.X, pumpAStartLoc.Y + (pumpBStartLoc.Y - pumpAStartLoc.Y) / 2.0)
			};
			this.mixPolylineLeft.Stroke = global::System.Windows.Media.Brushes.RosyBrown;
			this.mixPolylineLeft.StrokeThickness = 4.0;
			this.mixPolylineLeft.FillRule = FillRule.EvenOdd;
			this.mixPolylineLeft.Points = mixPointCollectionLeft;
			this.mixPolylineLeft.StrokeLineJoin = PenLineJoin.Round;
			this.mixPolylineLeft.SnapsToDevicePixels = true;
			this.canvasMain.Children.Add(this.mixPolylineLeft);
			Panel.SetZIndex(this.mixPolylineLeft, 0);
			PointCollection mixPointCollectionRight = new PointCollection
			{
				new global::System.Windows.Point(rightMix.X - 3.0, pumpAStartLoc.Y + (pumpBStartLoc.Y - pumpAStartLoc.Y) / 2.0),
				new global::System.Windows.Point(preColumnStartLoc.X, pumpAStartLoc.Y + (pumpBStartLoc.Y - pumpAStartLoc.Y) / 2.0)
			};
			this.mixPolylineRight.Stroke = global::System.Windows.Media.Brushes.RosyBrown;
			this.mixPolylineRight.StrokeThickness = 4.0;
			this.mixPolylineRight.FillRule = FillRule.EvenOdd;
			this.mixPolylineRight.Points = mixPointCollectionRight;
			this.mixPolylineRight.StrokeLineJoin = PenLineJoin.Round;
			this.mixPolylineRight.SnapsToDevicePixels = true;
			this.canvasMain.Children.Add(this.mixPolylineRight);
			Panel.SetZIndex(this.mixPolylineRight, 1);
			PointCollection columnPointCollection = new PointCollection();
			if (this.comboPreColumn.IsVisible)
			{
				columnPointCollection.Add(new global::System.Windows.Point(preColumnStartLoc.X + this.comboPreColumn.ActualWidth, pumpBStartLoc.Y + (pumpAStartLoc.Y - pumpBStartLoc.Y) / 2.0));
			}
			else
			{
				columnPointCollection.Add(new global::System.Windows.Point(preColumnStartLoc.X, pumpBStartLoc.Y + (pumpAStartLoc.Y - pumpBStartLoc.Y) / 2.0));
			}
			columnPointCollection.Add(new global::System.Windows.Point(analColumnStartLoc.X, pumpBStartLoc.Y + (pumpAStartLoc.Y - pumpBStartLoc.Y) / 2.0));
			this.columnPolyline.Stroke = global::System.Windows.Media.Brushes.RosyBrown;
			this.columnPolyline.StrokeThickness = 4.0;
			this.columnPolyline.FillRule = FillRule.EvenOdd;
			this.columnPolyline.Points = columnPointCollection;
			this.columnPolyline.StrokeLineJoin = PenLineJoin.Round;
			this.columnPolyline.SnapsToDevicePixels = true;
			this.canvasMain.Children.Add(this.columnPolyline);
			ArrowLine aline = new ArrowLine
			{
				Stroke = global::System.Windows.Media.Brushes.Gray,
				StrokeThickness = 5.0,
				ArrowEnds = ArrowEnds.End,
				X1 = arrowStartLoc.X,
				Y1 = arrowStartLoc.Y,
				X2 = arrowStartLoc.X,
				Y2 = pumpAStartLoc.Y + (pumpBStartLoc.Y - pumpAStartLoc.Y) / 2.0 - 6.0
			};
			this.canvasMain.Children.Add(aline);
			Panel.SetZIndex(aline, 0);
			this.UpdateMixColor(70);
		}

		// Token: 0x06000248 RID: 584 RVA: 0x0000F288 File Offset: 0x0000D488
		public void LoadAndBindColumns(BalticColumnList columnList, string preColumnName, string analColumnName)
		{
			MainUserControl._DC_172_0 _locals1 = new MainUserControl._DC_172_0();
			_locals1._cthis = this;
			_locals1.preColumnName = preColumnName;
			_locals1.analColumnName = analColumnName;
			if (!base.Dispatcher.CheckAccess())
			{
				MainUserControl._DC_172_1 _locals2 = new MainUserControl._DC_172_1();
				_locals2._locals1 = _locals1;
				_locals2.list = new BalticColumnList(columnList);
				base.Dispatcher.BeginInvoke(new Action(_locals2._mg_LoadAndBindColumns_Action_2), Array.Empty<object>());
				return;
			}
			BalticColumnList separators = new BalticColumnList();
			BalticColumnList traps = new BalticColumnList();
			foreach (Column column in columnList)
			{
				new ColumnAdapter(column);
				switch (column.Type)
				{
				case Column.ColumnType.PreColumn:
					traps.Add(column);
					break;
				case Column.ColumnType.AnalyticalColumn:
					separators.Add(column);
					break;
				case Column.ColumnType.Both:
					separators.Add(column);
					traps.Add(column);
					break;
				}
			}
			this.comboPreColumn.SelectedValuePath = "Name";
			this.comboAnalColumn.SelectedValuePath = "Name";
			object oldTrap = this.comboPreColumn.SelectedValue;
			object oldSeparator = this.comboAnalColumn.SelectedValue;
			this.comboPreColumn.ItemsSource = traps;
			this.comboAnalColumn.ItemsSource = separators;
			this.comboPreColumn.SelectedItem = traps.SingleOrDefault((Column c) => c.Name.Equals(_locals1.preColumnName));
			this.comboAnalColumn.SelectedItem = separators.SingleOrDefault((Column c) => c.Name.Equals(_locals1.analColumnName));
			this.stkTrapWarn.Visibility = ((this.comboPreColumn.SelectedItem == null) ? Visibility.Visible : Visibility.Hidden);
			this.stkSepWarn.Visibility = ((this.comboAnalColumn.SelectedItem == null) ? Visibility.Visible : Visibility.Hidden);
			this.TrapColumnInfoControl.DataContext = (Column)this.comboPreColumn.SelectedItem;
			this.SepColumnInfoControl.DataContext = (Column)this.comboAnalColumn.SelectedItem;
			if (this.dlgLCControl != null && this.comboPreColumn.SelectedValue == oldTrap)
			{
				this.UpdateLCControlColumnTypes(Column.ColumnType.PreColumn);
			}
			if (this.dlgLCControl != null && this.comboAnalColumn.SelectedValue == oldSeparator)
			{
				this.UpdateLCControlColumnTypes(Column.ColumnType.AnalyticalColumn);
			}
		}

		// Token: 0x06000249 RID: 585 RVA: 0x0000F4C0 File Offset: 0x0000D6C0
		public void UpdateMixColor(int percentB)
		{
			this.mixPolylineLeft.Stroke = new SolidColorBrush(global::System.Windows.Media.Color.FromRgb(99, 50, 138));
			this.mixPolylineRight.Stroke = new SolidColorBrush(global::System.Windows.Media.Color.FromRgb(99, 50, 138));
			this.columnPolyline.Stroke = new SolidColorBrush(global::System.Windows.Media.Color.FromRgb(99, 50, 138));
		}

		// Token: 0x0600024A RID: 586 RVA: 0x0000F528 File Offset: 0x0000D728
		private void CheckShowCompositionConditions()
		{
			if (this._instrument != null)
			{
				SystemConditionManager conditions = this._instrument.ShowCompositionConditions;
				Action action = delegate
				{
					foreach (ShowCompositionCondition condition in conditions.ShowCompositionConditions.Keys)
					{
						this.tbPercentB.Visibility = (condition.IsShow ? Visibility.Visible : Visibility.Hidden);
					}
					conditions.ShowCompositionConditions.Clear();
				};
				base.Dispatcher.Invoke(action);
			}
		}

		// Token: 0x0600024B RID: 587 RVA: 0x0000F574 File Offset: 0x0000D774
		private void UpdateLCControlColumnTypes(Column.ColumnType chromatographyFunction)
		{
			if (chromatographyFunction == Column.ColumnType.PreColumn || Column.ColumnType.Both == chromatographyFunction)
			{
				Column column = (Column)this.comboPreColumn.SelectedItem;
				this.dlgLCControl.TrapColumnType = ((column != null) ? new ColumnAdapter(column) : null);
			}
			if (Column.ColumnType.AnalyticalColumn == chromatographyFunction || Column.ColumnType.Both == chromatographyFunction)
			{
				Column column2 = (Column)this.comboAnalColumn.SelectedItem;
				this.dlgLCControl.SeparatorColumnType = ((column2 != null) ? new ColumnAdapter(column2) : null);
			}
		}

		// Token: 0x0600024C RID: 588 RVA: 0x0000F5E0 File Offset: 0x0000D7E0
		private void ColumnSelectionChanged(Column.ColumnType chromatographyFunction)
		{
			if (this.dlgLCControl != null)
			{
				this.UpdateLCControlColumnTypes(chromatographyFunction);
			}
			string text = (string)this.comboPreColumn.SelectedValue;
			string analyticalColumnName = (string)this.comboAnalColumn.SelectedValue;
			this.TrapColumnInfoControl.DataContext = (Column)this.comboPreColumn.SelectedItem;
			this.SepColumnInfoControl.DataContext = (Column)this.comboAnalColumn.SelectedItem;
			this.stkTrapWarn.Visibility = ((this.comboPreColumn.SelectedItem == null) ? Visibility.Visible : Visibility.Hidden);
			this.stkSepWarn.Visibility = ((this.comboAnalColumn.SelectedItem == null) ? Visibility.Visible : Visibility.Hidden);
			if (this.comboAnalColumn.SelectedItem == null)
			{
				this.stkSepWarn.Visibility = Visibility.Visible;
			}
			BalticWpfColumnEventArgs retvals = new BalticWpfColumnEventArgs(text, analyticalColumnName, chromatographyFunction);
			MainUserControl.BalticColumnSelectionEventHandler onColumnSelection = this.OnColumnSelection;
			if (onColumnSelection == null)
			{
				return;
			}
			onColumnSelection(this, retvals);
		}

		// Token: 0x0600024D RID: 589 RVA: 0x0000F6BE File Offset: 0x0000D8BE
		private void comboPreColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.ColumnSelectionChanged(Column.ColumnType.PreColumn);
		}

		// Token: 0x0600024E RID: 590 RVA: 0x0000F6C7 File Offset: 0x0000D8C7
		private void comboAnalColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.ColumnSelectionChanged(Column.ColumnType.AnalyticalColumn);
		}

		// Token: 0x0600024F RID: 591 RVA: 0x0000F6D0 File Offset: 0x0000D8D0
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			if (this.dlgLCControl != null)
			{
				this.dlgLCControl.Close();
				this.dlgLCControl = null;
			}
		}

		// Token: 0x06000250 RID: 592 RVA: 0x0000F6EC File Offset: 0x0000D8EC
		private void TrapColumnItemLoaded(object sender, RoutedEventArgs e)
		{
			MainUserControl.RegisterMouseOver(sender, new EventHandler(this.TrapColumnOnIsMouseOver));
		}

		// Token: 0x06000251 RID: 593 RVA: 0x0000F700 File Offset: 0x0000D900
		private void TrapColumnItemUnloaded(object sender, RoutedEventArgs e)
		{
			MainUserControl.UnregisterMouseOver(sender, new EventHandler(this.TrapColumnOnIsMouseOver));
		}

		// Token: 0x06000252 RID: 594 RVA: 0x0000F714 File Offset: 0x0000D914
		private void SepColumnItemLoaded(object sender, RoutedEventArgs e)
		{
			MainUserControl.RegisterMouseOver(sender, new EventHandler(this.SepColumnOnIsMouseOver));
		}

		// Token: 0x06000253 RID: 595 RVA: 0x0000F728 File Offset: 0x0000D928
		private void SepColumnItemUnloaded(object sender, RoutedEventArgs e)
		{
			MainUserControl.UnregisterMouseOver(sender, new EventHandler(this.SepColumnOnIsMouseOver));
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0000F73C File Offset: 0x0000D93C
		private static void RegisterMouseOver(object sender, EventHandler handler)
		{
			ComboBoxItem item = sender as ComboBoxItem;
			if (item != null)
			{
				DependencyPropertyDescriptor dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(ComboBoxItem));
				if (dependencyPropertyDescriptor == null)
				{
					return;
				}
				dependencyPropertyDescriptor.AddValueChanged(item, handler);
			}
		}

		// Token: 0x06000255 RID: 597 RVA: 0x0000F774 File Offset: 0x0000D974
		private static void UnregisterMouseOver(object sender, EventHandler handler)
		{
			ComboBoxItem item = sender as ComboBoxItem;
			if (item != null)
			{
				DependencyPropertyDescriptor dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(ComboBoxItem));
				if (dependencyPropertyDescriptor == null)
				{
					return;
				}
				dependencyPropertyDescriptor.RemoveValueChanged(item, handler);
			}
		}

		// Token: 0x06000256 RID: 598 RVA: 0x0000F7AC File Offset: 0x0000D9AC
		private void SepColumnOnIsMouseOver(object sender, EventArgs e)
		{
			ComboBoxItem item = sender as ComboBoxItem;
			if (item != null && item.IsMouseOver)
			{
				this.SepColumnInfoControl.DataContext = item.DataContext;
			}
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0000F7DC File Offset: 0x0000D9DC
		private void TrapColumnOnIsMouseOver(object sender, EventArgs e)
		{
			ComboBoxItem item = sender as ComboBoxItem;
			if (item != null && item.IsMouseOver)
			{
				this.TrapColumnInfoControl.DataContext = item.DataContext;
			}
		}


		// Token: 0x04000115 RID: 277
		private const string PROPERTY_CATEGORY = "Main Pump Control Properties";

		// Token: 0x04000116 RID: 278
		private readonly string _versionText;

		// Token: 0x04000117 RID: 279
		private readonly string _productVersion;

		// Token: 0x04000118 RID: 280
		private readonly string _creationDate;

		// Token: 0x04000119 RID: 281
		private readonly List<BalticHWProfile.CapillaryItem> _capillaries;

		// Token: 0x0400011A RID: 282
		private readonly List<BalticPreferences.CapillaryPreference> _prefCapillaries;

		// Token: 0x0400011B RID: 283
		private readonly Polyline mixPolylineLeft = new Polyline();

		// Token: 0x0400011C RID: 284
		private readonly Polyline mixPolylineRight = new Polyline();

		// Token: 0x0400011D RID: 285
		private readonly Polyline columnPolyline = new Polyline();

		// Token: 0x0400011E RID: 286
		private readonly bool _isPressurePSI;

		// Token: 0x0400011F RID: 287
		private readonly bool _isOvenInstalled;

		// Token: 0x04000120 RID: 288
		private BalticInstrumentFacade _instrument;

		// Token: 0x04000121 RID: 289
		private readonly CircularDisplayBuffer _flowMixDisp = new CircularDisplayBuffer(10);

		// Token: 0x04000122 RID: 290
		private readonly CircularDisplayBuffer _flowADisp = new CircularDisplayBuffer(10);

		// Token: 0x04000123 RID: 291
		private readonly CircularDisplayBuffer _flowBDisp = new CircularDisplayBuffer(10);

		// Token: 0x04000124 RID: 292
		private double flowPumpA;

		// Token: 0x04000125 RID: 293
		private double flowPumpB;

		// Token: 0x04000126 RID: 294
		private readonly string _displayName;

		// Token: 0x04000127 RID: 295
		private double _ovenTemp = 20.0;

		// Token: 0x04000128 RID: 296
		private double _ovenSetPtTemp = 20.0;

		// Token: 0x04000129 RID: 297
		private LCControlWindow dlgLCControl;

		// Token: 0x0400012A RID: 298
		private bool _isUseIdleFlow;

		// Token: 0x0400012B RID: 299
		private BalticPreferences _preferences;

		// Token: 0x04000136 RID: 310
		private readonly List<IMeteringChannel> _channels = new List<IMeteringChannel>();

		// Token: 0x04000137 RID: 311
		private volatile ExecutionStateChangedEventArgs _lastExecutionState;

		// Token: 0x04000139 RID: 313
		public static readonly RoutedCommand AbortCommand = new RoutedCommand();

		// Token: 0x0400013A RID: 314
		private volatile bool injectionAbortable;

		// Token: 0x0400013B RID: 315
		private volatile bool manualInjectionAbort;

		// Token: 0x0400013C RID: 316
		private volatile bool idleFlowActive;

		// Token: 0x0400013D RID: 317
		private volatile bool isEndTempWaitActive;

		// Token: 0x0400013E RID: 318
		public static readonly RoutedCommand ResetSystemCommand = new RoutedCommand();

		// Token: 0x0400013F RID: 319
		public static readonly RoutedCommand ClearErrorCommand = new RoutedCommand();

		// Token: 0x04000140 RID: 320
		public static readonly RoutedCommand EndTempWaitCommand = new RoutedCommand();

		// Token: 0x04000141 RID: 321
		private volatile bool systemResettable;

		// Token: 0x04000142 RID: 322
		private volatile bool isClearErrorable;

		// Token: 0x04000143 RID: 323
		private volatile bool isPreferenceable = true;

		// Token: 0x04000144 RID: 324
		public static readonly RoutedCommand PreferencesCommand = new RoutedCommand();

		// Token: 0x020000D5 RID: 213
		// (Invoke) Token: 0x06000737 RID: 1847
		public delegate void BalticColumnSelectionEventHandler(object sender, BalticWpfColumnEventArgs args);

		// Token: 0x020000D6 RID: 214
		// (Invoke) Token: 0x0600073B RID: 1851
		public delegate void UpdateHyStarStatusEventHandler(bool isReady, string procName, string version);

		// Token: 0x020000D7 RID: 215
		// (Invoke) Token: 0x0600073F RID: 1855
		public delegate void MaintenanceExecutionReport(ExecutionStateChangedEventArgs e);

		// Token: 0x020000D8 RID: 216
		// (Invoke) Token: 0x06000743 RID: 1859
		public delegate void ConfirmButtonOnErrorCallback(SystemCondition condition);

		// Token: 0x020000D9 RID: 217
		// (Invoke) Token: 0x06000747 RID: 1863
		public delegate void ShowPreferences();

		// Token: 0x020000DA RID: 218
		// (Invoke) Token: 0x0600074B RID: 1867
		public delegate void ShowLogBook();

		// Token: 0x020000DB RID: 219
		// (Invoke) Token: 0x0600074F RID: 1871
		public delegate void AbortInjectionEventHandler(bool isManualAbort = false, bool isContinue = false);

		// Token: 0x020000DC RID: 220
		// (Invoke) Token: 0x06000753 RID: 1875
		private delegate void SetStatusBarCallback(global::System.Drawing.Color newColor, global::System.Drawing.Color newTextColor, string strText);

		// Token: 0x020000DD RID: 221
		// (Invoke) Token: 0x06000757 RID: 1879
		private delegate void SetStatusBarTextCallback(string strText);

		// Token: 0x020000DE RID: 222
		// (Invoke) Token: 0x0600075B RID: 1883
		private delegate void SetStatusBarColorCallback(global::System.Drawing.Color newColor, global::System.Drawing.Color newTextColor);

		// Token: 0x020000DF RID: 223
		// (Invoke) Token: 0x0600075F RID: 1887
		private delegate void SetDeviceStatusBarCallback(global::System.Drawing.Color newColor, string strText);

		// Token: 0x020000E0 RID: 224
		// (Invoke) Token: 0x06000763 RID: 1891
		private delegate void SetAlertMessageCallback(SystemCondition condition);

		// Token: 0x020000E1 RID: 225
		// (Invoke) Token: 0x06000767 RID: 1895
		private delegate void SetSamplePositionCallback(string samplePos);
	}
}
