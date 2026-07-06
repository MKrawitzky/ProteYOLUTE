// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using BalticClassLib;
using BalticWpfControlLib.Controls;
using BalticWpfControlLib.Diagram;
using Bruker.Lc;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Bruker.Lc.Maintenance;
using Bruker.Lc.Metering;
using BrukerLC.Utils.Controls;
using Microsoft.CSharp.RuntimeBinder;

namespace BalticWpfControlLib
{
	// Token: 0x02000027 RID: 39
	public partial class LCUserControl : UserControl, INotifyPropertyChanged
	{
		private class _DC_48_2
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public dynamic _locals2;
			public Action _mg_RunScript_StopEnable_6;
			public dynamic e;
		}

		private class _DC_70_0
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public Action _mg_AbortEnabledConditions_AbortEnabledCollectionChanged_Action_0;
			public Action _mg_ApplyEnabledConditions_ApplyEnabledCollectionChanged_Action_0;
			public Action _mg_DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged_Action_0;
			public Action _mg_SetActivateAuxiliaryControlEnabled_Action_0;
			public Action _mg__facade_DecompressExecutionStateChanged_Action_0;
			public Action _mg_facade_ExecutionStateChanged_Action_1;
			public Action _mg_facade_ExecutionStateChanged_StopEnable_0;
			public Action _mg_facade_ProgressChanged_Action_0;
			public Action _r_9__4;
			public dynamic args;
			public dynamic childArgs;
			public dynamic conditions;
			public dynamic e;
			public bool enable;
			public bool enabled;
			public string msg;
			public dynamic script;
		}

		private class _DC_39_0
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public Action _mg_AbortEnabledConditions_AbortEnabledCollectionChanged_Action_0;
			public Action _mg_ApplyEnabledConditions_ApplyEnabledCollectionChanged_Action_0;
			public Action _mg_DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged_Action_0;
			public Action _mg_SetActivateAuxiliaryControlEnabled_Action_0;
			public Action _mg__facade_DecompressExecutionStateChanged_Action_0;
			public Action _mg_facade_ExecutionStateChanged_Action_1;
			public Action _mg_facade_ExecutionStateChanged_StopEnable_0;
			public Action _mg_facade_ProgressChanged_Action_0;
			public Action _r_9__4;
			public dynamic args;
			public dynamic childArgs;
			public dynamic conditions;
			public dynamic e;
			public bool enable;
			public bool enabled;
			public string msg;
			public dynamic script;
		}

		private class _DC_48_0
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public Action _mg_AbortEnabledConditions_AbortEnabledCollectionChanged_Action_0;
			public Action _mg_ApplyEnabledConditions_ApplyEnabledCollectionChanged_Action_0;
			public Action _mg_DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged_Action_0;
			public Action _mg_SetActivateAuxiliaryControlEnabled_Action_0;
			public Action _mg__facade_DecompressExecutionStateChanged_Action_0;
			public Action _mg_facade_ExecutionStateChanged_Action_1;
			public Action _mg_facade_ExecutionStateChanged_StopEnable_0;
			public Action _mg_facade_ProgressChanged_Action_0;
			public Action _r_9__4;
			public dynamic args;
			public dynamic childArgs;
			public dynamic conditions;
			public dynamic e;
			public bool enable;
			public bool enabled;
			public string msg;
			public dynamic script;
		}

		private class _DC_43_0
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public Action _mg_AbortEnabledConditions_AbortEnabledCollectionChanged_Action_0;
			public Action _mg_ApplyEnabledConditions_ApplyEnabledCollectionChanged_Action_0;
			public Action _mg_DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged_Action_0;
			public Action _mg_SetActivateAuxiliaryControlEnabled_Action_0;
			public Action _mg__facade_DecompressExecutionStateChanged_Action_0;
			public Action _mg_facade_ExecutionStateChanged_Action_1;
			public Action _mg_facade_ExecutionStateChanged_StopEnable_0;
			public Action _mg_facade_ProgressChanged_Action_0;
			public Action _r_9__4;
			public dynamic args;
			public dynamic childArgs;
			public dynamic conditions;
			public dynamic e;
			public bool enable;
			public bool enabled;
			public string msg;
			public dynamic script;
		}

		private class _DC_28_0
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public Action _mg_AbortEnabledConditions_AbortEnabledCollectionChanged_Action_0;
			public Action _mg_ApplyEnabledConditions_ApplyEnabledCollectionChanged_Action_0;
			public Action _mg_DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged_Action_0;
			public Action _mg_SetActivateAuxiliaryControlEnabled_Action_0;
			public Action _mg__facade_DecompressExecutionStateChanged_Action_0;
			public Action _mg_facade_ExecutionStateChanged_Action_1;
			public Action _mg_facade_ExecutionStateChanged_StopEnable_0;
			public Action _mg_facade_ProgressChanged_Action_0;
			public Action _r_9__4;
			public dynamic args;
			public dynamic childArgs;
			public dynamic conditions;
			public dynamic e;
			public bool enable;
			public bool enabled;
			public string msg;
			public dynamic script;
		}

		private class _DC_48_1
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public dynamic _locals1;
			public CancellationTokenSource cts;
			public ManualResetEventSlim wh;
		}

		private class _DC_40_0
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public Action _mg_AbortEnabledConditions_AbortEnabledCollectionChanged_Action_0;
			public Action _mg_ApplyEnabledConditions_ApplyEnabledCollectionChanged_Action_0;
			public Action _mg_DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged_Action_0;
			public Action _mg_SetActivateAuxiliaryControlEnabled_Action_0;
			public Action _mg__facade_DecompressExecutionStateChanged_Action_0;
			public Action _mg_facade_ExecutionStateChanged_Action_1;
			public Action _mg_facade_ExecutionStateChanged_StopEnable_0;
			public Action _mg_facade_ProgressChanged_Action_0;
			public Action _r_9__4;
			public dynamic args;
			public dynamic childArgs;
			public dynamic conditions;
			public dynamic e;
			public bool enable;
			public bool enabled;
			public string msg;
			public dynamic script;
		}

		private class _DC_30_0
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public Action _mg_AbortEnabledConditions_AbortEnabledCollectionChanged_Action_0;
			public Action _mg_ApplyEnabledConditions_ApplyEnabledCollectionChanged_Action_0;
			public Action _mg_DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged_Action_0;
			public Action _mg_SetActivateAuxiliaryControlEnabled_Action_0;
			public Action _mg__facade_DecompressExecutionStateChanged_Action_0;
			public Action _mg_facade_ExecutionStateChanged_Action_1;
			public Action _mg_facade_ExecutionStateChanged_StopEnable_0;
			public Action _mg_facade_ProgressChanged_Action_0;
			public Action _r_9__4;
			public dynamic args;
			public dynamic childArgs;
			public dynamic conditions;
			public dynamic e;
			public bool enable;
			public bool enabled;
			public string msg;
			public dynamic script;
		}

		private class _DC_29_0
		{
			public LCUserControl _cthis;
			public dynamic _closureLocals;
					public Action _mg_AbortEnabledConditions_AbortEnabledCollectionChanged_Action_0;
			public Action _mg_ApplyEnabledConditions_ApplyEnabledCollectionChanged_Action_0;
			public Action _mg_DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged_Action_0;
			public Action _mg_SetActivateAuxiliaryControlEnabled_Action_0;
			public Action _mg__facade_DecompressExecutionStateChanged_Action_0;
			public Action _mg_facade_ExecutionStateChanged_Action_1;
			public Action _mg_facade_ExecutionStateChanged_StopEnable_0;
			public Action _mg_facade_ProgressChanged_Action_0;
			public Action _r_9__4;
			public dynamic args;
			public dynamic childArgs;
			public dynamic conditions;
			public dynamic e;
			public bool enable;
			public bool enabled;
			public string msg;
			public dynamic script;
		}

		// Auto-generated callsite cache class
		private static class _co_27
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
		}

		// Token: 0x1400001D RID: 29
		// (add) Token: 0x060001A7 RID: 423 RVA: 0x0000AF78 File Offset: 0x00009178
		// (remove) Token: 0x060001A8 RID: 424 RVA: 0x0000AFB0 File Offset: 0x000091B0
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x060001A9 RID: 425 RVA: 0x0000AFE5 File Offset: 0x000091E5
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x060001AA RID: 426 RVA: 0x0000B000 File Offset: 0x00009200
		// (remove) Token: 0x060001AB RID: 427 RVA: 0x0000B038 File Offset: 0x00009238
		public event LCUserControl.ExecutionReport ExecutionReportEvent;

		// Token: 0x1400001F RID: 31
		// (add) Token: 0x060001AC RID: 428 RVA: 0x0000B070 File Offset: 0x00009270
		// (remove) Token: 0x060001AD RID: 429 RVA: 0x0000B0A8 File Offset: 0x000092A8
		public event LCUserControl.ConfirmButtonOnErrorCallback ConfirmButtonEvent;

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060001AE RID: 430 RVA: 0x0000B0DD File Offset: 0x000092DD
		// (set) Token: 0x060001AF RID: 431 RVA: 0x0000B0E5 File Offset: 0x000092E5
		public bool IsService
		{
			get
			{
				return this._isService;
			}
			set
			{
				this._isService = value;
				this.NotifyPropertyChanged("IsService");
				base.Dispatcher.Invoke(new Action(this._mg_set_IsService_Action_26_0));
			}
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x0000B110 File Offset: 0x00009310
		public LCUserControl(BalticInstrumentFacade instrument, List<BalticHWProfile.CapillaryItem> capillaries, List<BalticPreferences.CapillaryPreference> prefCapillaries, bool isOvenInstalled = false, bool displayPressureAsPsi = false, ExecutionStateChangedEventArgs initialExecutionState = null, Window parentWin = null)
		{
			this._facade = instrument;
			this._lastExecutionState = ((initialExecutionState != null) ? initialExecutionState.ExecutionState : ProcedureExecutionState.Completed);
			this.InitializeComponent();
			this._sysInfoOwner = parentWin;
			base.DataContext = this;
			this.btnStop.Visibility = Visibility.Collapsed;
			foreach (ProcedureInfo pi in this._facade.BusinessProcedures)
			{
				if (!pi.Hidden)
				{
					if (LCUserControl._co_27._cp_0 == null)
					{
						LCUserControl._co_27._cp_0 = CallSite<Func<CallSite, Type, ProcedureInfo, Window, string, object, BalticInstrumentFacade, ScriptUserControl>>.Create(Binder.InvokeConstructor(CSharpBinderFlags.None, typeof(LCUserControl), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
						}));
					}
					ScriptUserControl ucScript = LCUserControl._co_27._cp_0.Target(LCUserControl._co_27._cp_0, typeof(ScriptUserControl), pi, parentWin, instrument.PrivatePath, instrument.BalticSettings, instrument);
					ucScript.OnScriptActionClick += this.ucScript_ScriptActionClick;
					ucScript.OnScriptApplyClick += this.ucScript_ScriptApplyClick;
					ucScript.ScriptValidationRequest += this.ucScript_ScriptValidationRequest;
					ucScript.ScriptArgumentPresetRequest += this.ucScript_ScriptArgumentPresetRequest;
					if (ucScript.Info.IsService)
					{
						ucScript.Visibility = (BalticInstrumentFacade.IsService ? Visibility.Visible : Visibility.Collapsed);
					}
					this.wrapPanelScripts.Children.Add(ucScript);
				}
			}
			List<IMeteringChannel> channels = new List<IMeteringChannel>();
			LCUserControl.TransformChannels(this._facade.TraceChannels, channels, displayPressureAsPsi);
			IEnumerable<IMeteringChannel> enumerable = channels;
			if (LCUserControl._co_27._cp_2 == null)
			{
				LCUserControl._co_27._cp_2 = CallSite<Func<CallSite, object, double>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(double), typeof(LCUserControl)));
			}
			Func<CallSite, object, double> target = LCUserControl._co_27._cp_2.Target;
			CallSite _cpl = LCUserControl._co_27._cp_2;
			if (LCUserControl._co_27._cp_1 == null)
			{
				LCUserControl._co_27._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "LCControlMaxTimeBuffer", typeof(LCUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			this._realtimeChart = new LCChartUserControl(enumerable, target(_cpl, LCUserControl._co_27._cp_1.Target(LCUserControl._co_27._cp_1, this._facade.BalticConfiguration)))
			{
				Background = new SolidColorBrush(Colors.White)
			};
			Grid.SetColumn(this._realtimeChart, 2);
			this.chartGrid.Children.Add(this._realtimeChart);
			if (initialExecutionState != null)
			{
				this.facade_ExecutionStateChanged(this, initialExecutionState);
			}
			else
			{
				Severity? currentSeverity = this._facade.CurrentSeverity;
				Severity severity = Severity.Error;
				if ((currentSeverity.GetValueOrDefault() < severity) & (currentSeverity != null))
				{
					this.tbStatus.Text = "Ready";
				}
			}
			this._facade.ExecutionStateChanged += this.facade_ExecutionStateChanged;
			this._facade.DecompressExecutionStateChanged += this._facade_DecompressExecutionStateChanged;
			this.dgMessage.ItemsSource = new LCUserControl.NotifyCollectionChangedSynchronizer<SystemCondition>(this._facade.SystemConditions);
			this.dgMessage.Items.SortDescriptions.Add(new SortDescription("Raised", ListSortDirection.Ascending));
			INotifyCollectionChanged notifyCollectionChanged = this._facade.SystemConditions as INotifyCollectionChanged;
			if (notifyCollectionChanged != null)
			{
				notifyCollectionChanged.CollectionChanged += this.LCUserControl_ConditionsChanged;
			}
			this._facade.ProgressChanged += this.facade_ProgressChanged;
			this._facade.AbortEnabledConditions.AbortEnabledCollectionChanged += this.AbortEnabledConditions_AbortEnabledCollectionChanged;
			this._facade.AbortEnabledConditions.ApplyEnabledCollectionChanged += this.ApplyEnabledConditions_ApplyEnabledCollectionChanged;
			this._facade.DiagramLoggingEnabledConditions.DiagramLoggingEnabledCollectionChanged += this.DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged;
			foreach (IMeteringChannel channel in this._facade.GaugeChannels)
			{
				if (channel.ChannelInfo.Id.Name.Equals("activity"))
				{
					channel.ChannelDataChanged += this.activity_ChannelDataChanged;
					break;
				}
			}
			this.dgMessage.Focus();
			this._diagramStateController = new DiagramStateController(this._facade, capillaries, prefCapillaries, this.Diagram, false, isOvenInstalled, displayPressureAsPsi);
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x0000B5A4 File Offset: 0x000097A4
		private void DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LCUserControl._DC_28_0 _locals1 = new LCUserControl._DC_28_0();
			_locals1._cthis = this;
			_locals1.conditions = this._facade.DiagramLoggingEnabledConditions;
			base.Dispatcher.Invoke(new Action(_locals1._mg_DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged_Action_0));
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x0000B5E8 File Offset: 0x000097E8
		private void ApplyEnabledConditions_ApplyEnabledCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LCUserControl._DC_29_0 _locals1 = new LCUserControl._DC_29_0();
			_locals1._cthis = this;
			_locals1.conditions = this._facade.ApplyEnabledConditions;
			base.Dispatcher.Invoke(new Action(_locals1._mg_ApplyEnabledConditions_ApplyEnabledCollectionChanged_Action_0));
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x0000B62C File Offset: 0x0000982C
		private void AbortEnabledConditions_AbortEnabledCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LCUserControl._DC_30_0 _locals1 = new LCUserControl._DC_30_0();
			_locals1._cthis = this;
			_locals1.conditions = this._facade.AbortEnabledConditions;
			base.Dispatcher.Invoke(new Action(_locals1._mg_AbortEnabledConditions_AbortEnabledCollectionChanged_Action_0));
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x0000B670 File Offset: 0x00009870
		private void LCUserControl_ConditionsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action.Equals(NotifyCollectionChangedAction.Add))
			{
				IEnumerator enumerator = e.NewItems.GetEnumerator();
				while (enumerator.MoveNext())
				{
					SystemCondition condition = (SystemCondition)enumerator.Current;
					if (condition.ManualDismiss)
					{
						this.dgMessage.Dispatcher.Invoke<object>(() => this.dgMessage.SelectedItem = condition);
						break;
					}
				}
				goto IL_012A;
			}
			if (e.Action.Equals(NotifyCollectionChangedAction.Reset) && this._storyboardCondition != null)
			{
				bool foundCond = false;
				using (IEnumerator<SystemCondition> enumerator2 = this._facade.SystemConditions.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current == this._storyboardCondition)
						{
							foundCond = true;
							break;
						}
					}
				}
				if (!foundCond)
				{
					this.bottomPanel.Visibility = Visibility.Hidden;
					this.dgMessage.IsHitTestVisible = true;
					this.dgMessage.SelectedIndex = -1;
					this._storyboardCondition = null;
				}
			}
			IL_012A:
			this.dgMessage.Dispatcher.Invoke(delegate
			{
				if (this.dgMessage.Items.Count > 0)
				{
					this.dgMessage.ScrollIntoView(this.dgMessage.Items[this.dgMessage.Items.Count - 1]);
				}
			});
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x0000B7E0 File Offset: 0x000099E0
		private void ucScript_ScriptArgumentPresetRequest(object sender, ScriptControlEventArgs e)
		{
			ProcedureArguments args = e.ProcedureSourceArgs;
			if (args.Contains("trap"))
			{
				args["trap"].Value = this.TrapType;
			}
			if (args.Contains("separator"))
			{
				args["separator"].Value = this.SeparatorType;
			}
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x0000B83C File Offset: 0x00009A3C
		private void ucScript_ScriptActionClick(object sender, ScriptControlEventArgs args)
		{
			if (this._isScripInProgress)
			{
				return;
			}
			this.tbScriptAction.Text = "";
			if (this._storyboardCondition != null)
			{
				if (this.btnConfirm.Visibility == Visibility.Visible)
				{
					this.confirmButton_Click(this.btnConfirm, null);
				}
				else if (this.btnClose.Visibility == Visibility.Visible)
				{
					this.closeButton_Click(this.btnClose, null);
				}
			}
			this.RunScript(args.ProcedureSourceInfo, args.ProcedureSourceArgs, null);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x0000B8B3 File Offset: 0x00009AB3
		private void ucScript_ScriptApplyClick(object sender, ScriptControlEventArgs args)
		{
			this.ApplyScript(args.ProcedureSourceInfo, args.ProcedureSourceArgs, args.ProcedureSourceChildArgs);
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x0000B8D0 File Offset: 0x00009AD0
		private void ucScript_ScriptValidationRequest(object sender, ScriptValidationRequestEventArgs e)
		{
			EventHandler<ProcedureReportEventArgs> handler = delegate(object _, ProcedureReportEventArgs a)
			{
				e.AddReport(a);
			};
			this._facade.ValidationMessageReported += handler;
			this._facade.ValidateProcedureOffLine(e.ProcedureSourceInfo, e.ProcedureSourceArgs, e.ProcedureSourceChildArgs);
			this._facade.ValidationMessageReported -= handler;
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x0000B93C File Offset: 0x00009B3C
		private static void TransformChannels(IEnumerable<IMeteringChannel> channels, List<IMeteringChannel> collection, bool psi)
		{
			foreach (IMeteringChannel meteringChannel in channels)
			{
				MeteringChannelInfo info = meteringChannel.ChannelInfo;
				IMeteringChannel chan = meteringChannel;
				if (psi && "bar".Equals(info.Unit, StringComparison.InvariantCultureIgnoreCase))
				{
					info = new MeteringChannelInfo(info.Id.Name, info.Id.Source, "psi", info.DisplayDecimals, info.ValueType, info.IsDiagnostic, info.IsSevice);
					IMeteringChannel meteringChannel2 = chan;
					MeteringChannelInfo meteringChannelInfo = info;
					Func<MeteringDataPoint, MeteringDataPoint> func;
					if ((func = LCUserControl._SDC._cm_Bars2Psi) == null)
					{
						func = (LCUserControl._SDC._cm_Bars2Psi = new Func<MeteringDataPoint, MeteringDataPoint>(LCUserControl.Bars2Psi));
					}
					chan = new TransformingMeteringChannel(meteringChannel2, meteringChannelInfo, func);
				}
				collection.Add(chan);
			}
		}

		// Token: 0x060001BA RID: 442 RVA: 0x0000BA04 File Offset: 0x00009C04
		private static MeteringDataPoint Bars2Psi(MeteringDataPoint mdp)
		{
			return new MeteringDataPoint(mdp.Timestamp, (double)mdp.Value / 0.0689475729);
		}

		// Token: 0x060001BB RID: 443 RVA: 0x0000BA2C File Offset: 0x00009C2C
		private void _facade_DecompressExecutionStateChanged(object sender, ExecutionStateChangedEventArgs args)
		{
			LCUserControl._DC_39_0 _locals1 = new LCUserControl._DC_39_0();
			_locals1._cthis = this;
			_locals1.args = args;
			_locals1.enable = _locals1.args.ExecutionState > ProcedureExecutionState.Started;
			_locals1.msg = _locals1.args.Procedure + " " + _locals1.args.ExecutionState.ToString().ToLower();
			base.Dispatcher.Invoke(new Action(_locals1._mg__facade_DecompressExecutionStateChanged_Action_0));
		}

		// Token: 0x060001BC RID: 444 RVA: 0x0000BAB4 File Offset: 0x00009CB4
		private void facade_ExecutionStateChanged(object sender, ExecutionStateChangedEventArgs args)
		{
			LCUserControl._DC_40_0 _locals1 = new LCUserControl._DC_40_0();
			_locals1._cthis = this;
			_locals1.args = args;
			if (!string.IsNullOrEmpty(_locals1.args.BackGroundSelfCheckEventName))
			{
				base.Dispatcher.Invoke(new Action(_locals1._mg_facade_ExecutionStateChanged_StopEnable_0), Array.Empty<object>());
				ProcedureExecutionState executionState = _locals1.args.ExecutionState;
				if (executionState != ProcedureExecutionState.Started)
				{
					if (executionState - ProcedureExecutionState.Completed <= 1)
					{
						this.btnStop.Click -= this.OnAbortSelfcheckClick;
						this._isScripInProgress = false;
					}
				}
				else
				{
					this.btnStop.Click += this.OnAbortSelfcheckClick;
					this._isScripInProgress = true;
				}
			}
			this._realtimeChart.SetHidden(_locals1.args.IsHidden && !this.IsElutionProcedure(_locals1.args.Procedure));
			if (_locals1.args.IsHidden && !this.IsElutionProcedure(_locals1.args.Procedure))
			{
				return;
			}
			LCUserControl.ExecutionReport executionReportEvent = this.ExecutionReportEvent;
			if (executionReportEvent != null)
			{
				executionReportEvent(_locals1.args);
			}
			this._lastExecutionState = _locals1.args.ExecutionState;
			_locals1.msg = _locals1.args.Procedure + " " + _locals1.args.ExecutionState.ToString().ToLower();
			_locals1.enable = _locals1.args.ExecutionState > ProcedureExecutionState.Started;
			if (!_locals1.args.IsDecompressOnExit)
			{
				base.Dispatcher.Invoke(new Action(_locals1._mg_facade_ExecutionStateChanged_Action_1));
				return;
			}
			if (!_locals1.enable)
			{
				base.Dispatcher.Invoke(new Action(_locals1._mg_facade_ExecutionStateChanged_Action_1));
			}
		}

		// Token: 0x060001BD RID: 445 RVA: 0x0000BC64 File Offset: 0x00009E64
		private void OnAbortSelfcheckClick(object sender, RoutedEventArgs args)
		{
			SystemCondition storyboardCondition = this._storyboardCondition;
			if (storyboardCondition != null && storyboardCondition.ManualDismiss)
			{
				this.confirmButton_Click(this.btnConfirm, new RoutedEventArgs());
			}
			((Action)delegate
			{
				using (ManualResetEventSlim wh = new ManualResetEventSlim())
				{
					EventHandler<ExecutionStateChangedEventArgs> decompressState = delegate(object _, ExecutionStateChangedEventArgs e)
					{
						if (e.ExecutionState != ProcedureExecutionState.Started)
						{
							wh.Set();
						}
					};
					if (this._facade.GetDecompressProcedure().DecompressOnExit)
					{
						this._facade.DecompressExecutionStateChanged += decompressState;
						wh.Wait();
						this._facade.DecompressExecutionStateChanged -= decompressState;
					}
					base.Dispatcher.Invoke(new Action(this._mg_OnAbortSelfcheckClick_GuiAction_41_2));
					this._isScripInProgress = false;
				}
			}).BeginInvoke(null, null);
			this._facade.AbortSelfDiagnostics();
		}

		// Token: 0x060001BE RID: 446 RVA: 0x0000BCB4 File Offset: 0x00009EB4
		private void activity_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			if (e.Data != null)
			{
				foreach (string activity in (string[])e.Data[e.Data.Length - 1].Value)
				{
					sb.Append(activity).Append(" + ");
				}
				if (sb.Length > 0)
				{
					sb.Length -= " + ".Length;
				}
			}
			string msg = sb.ToString();
			base.Dispatcher.Invoke<string>(() => this.tbScriptAction.Text = msg);
		}

		// Token: 0x060001BF RID: 447 RVA: 0x0000BD64 File Offset: 0x00009F64
		private void facade_ProgressChanged(object sender, Bruker.Lc.Business.ProgressChangedEventArgs e)
		{
			LCUserControl._DC_43_0 _locals1 = new LCUserControl._DC_43_0();
			_locals1._cthis = this;
			_locals1.e = e;
			base.Dispatcher.Invoke(new Action(_locals1._mg_facade_ProgressChanged_Action_0));
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x0000BD9C File Offset: 0x00009F9C
		private bool IsElutionProcedure(string procedureName)
		{
			return this._facade.GetElutionProcedure(procedureName) != null;
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000BDAD File Offset: 0x00009FAD
		private bool IsEquilibrationProcedure(string procedureName)
		{
			return this._facade.GetEquilibrationProcedure(procedureName) != null;
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x0000BDBE File Offset: 0x00009FBE
		private void EnableUserControls(bool isEnable, string activity, bool isDecompress = false)
		{
			if (!isDecompress)
			{
				this.tbStopBtnText.Text = " Abort " + activity;
			}
			this.bdrScriptAction.Visibility = (isEnable ? Visibility.Hidden : Visibility.Visible);
			this.bdrScriptWrapPanel.Visibility = (isEnable ? Visibility.Visible : Visibility.Hidden);
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x0000BDFD File Offset: 0x00009FFD
		private void ApplyScript(ProcedureInfo script, ProcedureArguments args, ChildProcedureArguments childArgs)
		{
			if (!this._isScripInProgress)
			{
				this.RunScript(script, args, childArgs);
				return;
			}
			this._facade.ApplyScriptSettings(script, args, childArgs);
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x0000BE1F File Offset: 0x0000A01F
		private void RunScript(ProcedureInfo script, ProcedureArguments args, ChildProcedureArguments childArgs = null)
		{
			LCUserControl._DC_48_0 _locals1 = new LCUserControl._DC_48_0();
			_locals1._cthis = this;
			_locals1.script = script;
			_locals1.args = args;
			_locals1.childArgs = childArgs;
			this._isScripInProgress = true;
			((Action)delegate
			{
				LCUserControl._DC_48_1 _locals2 = new LCUserControl._DC_48_1();
				_locals2._locals1 = _locals1;
				_locals2.cts = new CancellationTokenSource();
				try
				{
					_locals1._cthis.Dispatcher.Invoke(new Action(delegate { _locals1._cthis.btnStop.Visibility = Visibility.Visible; }), Array.Empty<object>());
					_locals2.wh = new ManualResetEventSlim();
					try
					{
						BasicSyncHandler syncHandler = new BasicSyncHandler();
						RoutedEventHandler click = delegate(object _, RoutedEventArgs _)
						{
							SystemCondition storyboardCondition = _locals2._locals1._cthis._storyboardCondition;
							if (storyboardCondition != null && storyboardCondition.ManualDismiss)
							{
								_locals2._locals1._cthis.confirmButton_Click(_locals2._locals1._cthis.btnConfirm, new RoutedEventArgs());
							}
							_locals2.cts.Cancel();
							_locals2._locals1._cthis.btnStop.IsEnabled = false;
							if (!_locals2._locals1.script.DecompressOnExit)
							{
								_locals2._locals1._cthis._isScripInProgress = false;
							}
						};
						_locals1._cthis.btnStop.Click += click;
						EventHandler<ExecutionStateChangedEventArgs> state = delegate(object _, ExecutionStateChangedEventArgs e)
						{
							LCUserControl._DC_48_2 _locals3 = new LCUserControl._DC_48_2();
							_locals3._locals2 = _locals2;
							_locals3.e = e;
							if (_locals3.e.IsHidden && !_locals2._locals1._cthis.IsElutionProcedure(_locals3.e.Procedure))
							{
								return;
							}
							_locals2._locals1._cthis.Dispatcher.Invoke(new Action(_locals3._mg_RunScript_StopEnable_6), Array.Empty<object>());
							if (_locals3.e.ExecutionState == ProcedureExecutionState.Completed && _locals2._locals1.script.Name.ToLower().Contains("preparation"))
							{
								_locals2._locals1._cthis._facade.UnregisterPreProcedureRequired();
							}
							if (_locals3.e.ExecutionState != ProcedureExecutionState.Started && !_locals2._locals1.script.DecompressOnExit)
							{
								_locals2.wh.Set();
							}
						};
						_locals1._cthis._facade.ExecutionStateChanged += state;
						EventHandler<ExecutionStateChangedEventArgs> decompressState = delegate(object _, ExecutionStateChangedEventArgs e)
						{
							if (e.ExecutionState != ProcedureExecutionState.Started)
							{
								_locals2.wh.Set();
							}
						};
						if (_locals1.script.DecompressOnExit)
						{
							_locals1._cthis._facade.DecompressExecutionStateChanged += decompressState;
						}
						try
						{
							_locals1._cthis._facade.ExecuteProcedure(_locals1.script, _locals1.args, _locals1.childArgs ?? new ChildProcedureArguments(), syncHandler, _locals2.cts.Token, null);
							_locals2.wh.Wait();
						}
						finally
						{
							_locals1._cthis._facade.ExecutionStateChanged -= state;
							if (_locals1.script.DecompressOnExit)
							{
								_locals1._cthis._facade.DecompressExecutionStateChanged -= decompressState;
							}
							_locals1._cthis.btnStop.Click -= click;
							_locals1._cthis._isScripInProgress = false;
							_locals1._cthis.IsService = _locals1._cthis._facade.CheckForServiceMode();
							Dispatcher dispatcher = _locals1._cthis.Dispatcher;
							Action action;
							if ((action = _locals1._r_9__4) == null)
							{
								action = (_locals1._r_9__4 = (Action)delegate
								{
									if (_locals1._cthis._diagramStateController.IsDiagramLoggingEnabled)
									{
										_locals1._cthis._diagramStateController.IsDiagramLoggingEnabled = false;
									}
								});
							}
							dispatcher.BeginInvoke(action, Array.Empty<object>());
						}
					}
					finally
					{
						if (_locals2.wh != null)
						{
							((IDisposable)_locals2.wh).Dispose();
						}
					}
				}
				finally
				{
					if (_locals2.cts != null)
					{
						((IDisposable)_locals2.cts).Dispose();
					}
				}
			}).BeginInvoke(null, null);
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000BE5C File Offset: 0x0000A05C
		public void AbortActiveProcedure()
		{
			this.btnStop.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060001C6 RID: 454 RVA: 0x0000BE73 File Offset: 0x0000A073
		// (set) Token: 0x060001C7 RID: 455 RVA: 0x0000BE7B File Offset: 0x0000A07B
		public IChromatographyColumnType TrapType { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060001C8 RID: 456 RVA: 0x0000BE84 File Offset: 0x0000A084
		// (set) Token: 0x060001C9 RID: 457 RVA: 0x0000BE8C File Offset: 0x0000A08C
		public IChromatographyColumnType SeparatorType { get; set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060001CA RID: 458 RVA: 0x0000BE98 File Offset: 0x0000A098
		// (set) Token: 0x060001CB RID: 459 RVA: 0x0000BF74 File Offset: 0x0000A174
		public LCUserControlSettings Settings
		{
			get
			{
				GridLengthConverter converter = new GridLengthConverter();
				return new LCUserControlSettings
				{
					EnabledTracesList = this._realtimeChart.EnabledTracesList.ToList<string>(),
					IsDiagnosticTracesSelected = this._realtimeChart.IsDiagnosticTracesSelected,
					ChartGridWidthLeft = converter.ConvertToInvariantString(this.chartGrid.ColumnDefinitions[0].Width),
					ChartGridWidthRight = converter.ConvertToInvariantString(this.chartGrid.ColumnDefinitions[2].Width),
					MainGridHeightTopRow = converter.ConvertToInvariantString(this.storyGrid.RowDefinitions[0].Height),
					MainGridHeightBottomRow = converter.ConvertToInvariantString(this.storyGrid.RowDefinitions[2].Height)
				};
			}
			set
			{
				try
				{
					if (!string.IsNullOrEmpty(value.MainGridHeightTopRow) && !string.IsNullOrEmpty(value.MainGridHeightBottomRow) && !string.IsNullOrEmpty(value.ChartGridWidthLeft) && !string.IsNullOrEmpty(value.ChartGridWidthRight))
					{
						GridLengthConverter gridLengthConverter = new GridLengthConverter();
						object heightTop = gridLengthConverter.ConvertFromInvariantString(value.MainGridHeightTopRow);
						object heightBottom = gridLengthConverter.ConvertFromInvariantString(value.MainGridHeightBottomRow);
						object widthLeft = gridLengthConverter.ConvertFromInvariantString(value.ChartGridWidthLeft);
						object widthRight = gridLengthConverter.ConvertFromInvariantString(value.ChartGridWidthRight);
						if (heightTop is GridLength)
						{
							GridLength gridTop = (GridLength)heightTop;
							if (heightBottom is GridLength)
							{
								GridLength gridBottom = (GridLength)heightBottom;
								if (widthLeft is GridLength)
								{
									GridLength gridLeft = (GridLength)widthLeft;
									if (widthRight is GridLength)
									{
										GridLength gridRight = (GridLength)widthRight;
										this.storyGrid.RowDefinitions[0].Height = gridTop;
										this.storyGrid.RowDefinitions[2].Height = gridBottom;
										this.chartGrid.ColumnDefinitions[0].Width = gridLeft;
										this.chartGrid.ColumnDefinitions[2].Width = gridRight;
									}
								}
							}
						}
					}
				}
				catch (Exception)
				{
				}
				this._realtimeChart.IsDiagnosticTracesSelected = value.IsDiagnosticTracesSelected;
				this._realtimeChart.EnabledTracesList = value.EnabledTracesList;
			}
		}

		// Token: 0x060001CC RID: 460 RVA: 0x0000C0E4 File Offset: 0x0000A2E4
		private void ShowSystemCondition1(SystemCondition condition)
		{
			if (this.messagePanel.Dispatcher.CheckAccess())
			{
				this._storyboardCondition = condition;
				switch (condition.Severity)
				{
				case Severity.Dialog:
					this.messagePanel.Background = base.Resources["InfoBackgroundBrush"] as LinearGradientBrush;
					this.imgMessage.Source = new ImageConverter().Convert("Images/Info.png") as ImageSource;
					break;
				case Severity.Info:
					this.messagePanel.Background = base.Resources["InfoBackgroundBrush"] as LinearGradientBrush;
					this.imgMessage.Source = new ImageConverter().Convert("Images/Info.png") as ImageSource;
					break;
				case Severity.Warn:
					this.messagePanel.Background = base.Resources["WarningBackgroundBrush"] as LinearGradientBrush;
					this.imgMessage.Source = new ImageConverter().Convert("Images/Warning.png") as ImageSource;
					break;
				case Severity.Error:
					this.messagePanel.Background = base.Resources["ErrorBackgroundBrush"] as LinearGradientBrush;
					this.imgMessage.Source = new ImageConverter().Convert("Images/Error.png") as ImageSource;
					break;
				case Severity.Tip:
					this.messagePanel.Background = base.Resources["TipBackgroundBrush"] as LinearGradientBrush;
					this.imgMessage.Source = new ImageConverter().Convert("Images/lightbulb.png") as ImageSource;
					break;
				}
				this.btnConfirm.Visibility = (condition.ManualDismiss ? Visibility.Visible : Visibility.Hidden);
				this.SetConfirmEnabled();
				this.btnClose.Visibility = ((!condition.ManualDismiss) ? Visibility.Visible : Visibility.Hidden);
				this.lblSubject.Text = string.Format("{0:F} {1}", condition.Raised, condition.Subject);
				this.lblMessage.Text = condition.Description;
				this.bottomPanel.Visibility = Visibility.Hidden;
				double lineHeight = 16.0;
				int nLines = this.GetSubjectLines(this.lblMessage, ref lineHeight);
				if (this.dgMessage.ActualHeight < (double)(nLines * (int)lineHeight) + this.hdrBorder.Height)
				{
					this.storyGrid.RowDefinitions[0].Height = new GridLength((double)(nLines * (int)lineHeight) + this.hdrBorder.Height + 30.0);
				}
				if (nLines < 3)
				{
					this.propertiesPanel.Height = 50.0;
				}
				else
				{
					this.propertiesPanel.Height = (double)(nLines * (int)lineHeight);
				}
				this.bottomPanel.Visibility = Visibility.Visible;
				return;
			}
			LCUserControl.ShowSystemConditionCallback del = new LCUserControl.ShowSystemConditionCallback(this.ShowSystemCondition1);
			base.Dispatcher.BeginInvoke(del, new object[] { condition });
		}

		// Token: 0x060001CD RID: 461 RVA: 0x0000C3C0 File Offset: 0x0000A5C0
		private void SetConfirmEnabled()
		{
			UIElement uielement = this.btnConfirm;
			bool flag;
			if (this._lastExecutionState == ProcedureExecutionState.Started)
			{
				SystemCondition storyboardCondition = this._storyboardCondition;
				flag = storyboardCondition != null && storyboardCondition.ManualDismiss;
			}
			else
			{
				flag = true;
			}
			uielement.IsEnabled = flag;
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000C3F8 File Offset: 0x0000A5F8
		private int GetSubjectLines(TextBlock tbMessage, ref double lineHeight)
		{
			double pixelsPerDpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
			string[] array = tbMessage.Text.Split(new char[] { '\n' });
			int nLines = 1;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				FormattedText formattedMessageText = new FormattedText(array2[i], CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(this.lblMessage.FontFamily, this.lblMessage.FontStyle, this.lblMessage.FontWeight, this.lblMessage.FontStretch), this.lblMessage.FontSize, Brushes.Black, pixelsPerDpi);
				nLines += (int)(formattedMessageText.Width / this.stkPanel.ActualWidth) + 1;
				lineHeight = formattedMessageText.Height + 2.1;
			}
			return nLines;
		}

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x060001CF RID: 463 RVA: 0x0000C4C8 File Offset: 0x0000A6C8
		// (remove) Token: 0x060001D0 RID: 464 RVA: 0x0000C500 File Offset: 0x0000A700
		public event EventHandler ActivateAuxiliaryControl;

		// Token: 0x060001D1 RID: 465 RVA: 0x0000C538 File Offset: 0x0000A738
		private void Statistics_Click(object sender, RoutedEventArgs args)
		{
			EventHandler activateAuxiliaryControl = this.ActivateAuxiliaryControl;
			if (activateAuxiliaryControl != null)
			{
				activateAuxiliaryControl(this, EventArgs.Empty);
			}
			if (this._isSystemWindowOpen)
			{
				this._dlgSystemInfo.WindowState = WindowState.Normal;
				this._dlgSystemInfo.Show();
				this._dlgSystemInfo.Focus();
				return;
			}
			this._dlgSystemInfo = new SystemInfoWindow(this._facade.MaintenanceInfos, this._isService);
			this._dlgSystemInfo.Closing += this.dlgSystemInfo_Closing;
			this._dlgSystemInfo.Closed += this.dlgSystemInfo_Closed;
			if (this._sysInfoOwner != null)
			{
				this._dlgSystemInfo.Owner = this._sysInfoOwner;
			}
			this._dlgSystemInfo.Show();
			this._dlgSystemInfo.Focus();
			this._isSystemWindowOpen = true;
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x0000C609 File Offset: 0x0000A809
		private void dlgSystemInfo_Closing(object sender, CancelEventArgs e)
		{
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x0000C60B File Offset: 0x0000A80B
		private void dlgSystemInfo_Closed(object sender, EventArgs e)
		{
			this._isSystemWindowOpen = false;
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x0000C614 File Offset: 0x0000A814
		public void SetActivateAuxiliaryControlEnabled(bool enabled)
		{
			LCUserControl._DC_70_0 _locals1 = new LCUserControl._DC_70_0();
			_locals1._cthis = this;
			_locals1.enabled = enabled;
			base.Dispatcher.Invoke(new Action(_locals1._mg_SetActivateAuxiliaryControlEnabled_Action_0));
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x0000C64C File Offset: 0x0000A84C
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			GridView gridView = this.dgMessage.View as GridView;
			if (gridView != null)
			{
				double workingWidth = this.dgMessage.ActualWidth - SystemParameters.VerticalScrollBarWidth * 2.0;
				gridView.Columns[0].Width = workingWidth * 0.18;
				gridView.Columns[1].Width = workingWidth * 0.25;
				gridView.Columns[2].Width = workingWidth * 0.57;
			}
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x0000C6DD File Offset: 0x0000A8DD
		private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
		{
			if (this._dlgSystemInfo != null)
			{
				this._dlgSystemInfo.Close();
				this._dlgSystemInfo = null;
			}
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x0000C6FC File Offset: 0x0000A8FC
		public void confirmButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.dgMessage.Dispatcher.CheckAccess())
			{
				SystemCondition condition = this.dgMessage.SelectedItem as SystemCondition;
				this.bottomPanel.Visibility = Visibility.Hidden;
				if (condition != null)
				{
					if (condition.ManualDismiss)
					{
						this._facade.DismissCondition(condition);
					}
					if (condition.Severity == Severity.Error)
					{
						LCUserControl.ConfirmButtonOnErrorCallback confirmButtonEvent = this.ConfirmButtonEvent;
						if (confirmButtonEvent != null)
						{
							confirmButtonEvent(condition);
						}
					}
				}
				this.dgMessage.IsHitTestVisible = true;
				this.dgMessage.SelectedIndex = -1;
				this._storyboardCondition = null;
				return;
			}
			LCUserControl.ConfirmConditionCallback del = new LCUserControl.ConfirmConditionCallback(this.confirmButton_Click);
			base.Dispatcher.BeginInvoke(del, new object[] { sender, e });
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x0000C7B2 File Offset: 0x0000A9B2
		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			this.bottomPanel.Visibility = Visibility.Hidden;
			this.dgMessage.SelectedIndex = -1;
			this._storyboardCondition = null;
			this._facade.ActiveConditionClear();
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x0000C7E0 File Offset: 0x0000A9E0
		private void dgMessage_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SystemCondition condition = this.dgMessage.SelectedItem as SystemCondition;
			if (condition != null)
			{
				this.ShowSystemCondition1(condition);
			}
		}

		// Token: 0x060001DA RID: 474 RVA: 0x0000C808 File Offset: 0x0000AA08
		private void dgMessage_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.dgMessage.Items.Count > 0)
			{
				this.dgMessage.ScrollIntoView(this.dgMessage.Items[this.dgMessage.Items.Count - 1]);
			}
		}

		// Token: 0x060001DB RID: 475 RVA: 0x0000C858 File Offset: 0x0000AA58
		private void dgMessage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			SystemCondition systemCondition = this.dgMessage.SelectedItem as SystemCondition;
			if (systemCondition != null && systemCondition.ManualDismiss)
			{
				this.btnConfirm.BeginStoryboard((Storyboard)this.storyGrid.Resources["flashAnimation"]);
				e.Handled = true;
			}
		}

		// Token: 0x060001DF RID: 479 RVA: 0x0000CB44 File Offset: 0x0000AD44
		[CompilerGenerated]
		private void _mg_set_IsService_Action_26_0()
		{
			if (this.wrapPanelScripts != null)
			{
				foreach (object obj in this.wrapPanelScripts.Children)
				{
					ScriptUserControl script = obj as ScriptUserControl;
					if (script != null && script.Info.IsService)
					{
						script.Visibility = (this.IsService ? Visibility.Visible : Visibility.Collapsed);
					}
				}
			}
			LCChartUserControl realtimeChart = this._realtimeChart;
			if (realtimeChart != null)
			{
				realtimeChart.UpdateServiceTraces(this._isService);
			}
			this._diagramStateController.UpdateServiceMode(this._isService);
			if (this._dlgSystemInfo != null)
			{
				this._dlgSystemInfo.IsAppService = this._isService;
			}
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0000CCFC File Offset: 0x0000AEFC
		[CompilerGenerated]
		private void _mg_OnAbortSelfcheckClick_GuiAction_41_2()
		{
			this.btnStop.Visibility = Visibility.Collapsed;
			this.btnStop.IsEnabled = false;
			this.EnableUserControls(true, this._facade.GetDecompressProcedure().Name, false);
		}

		// Token: 0x040000EA RID: 234
		private readonly BalticInstrumentFacade _facade;

		// Token: 0x040000EB RID: 235
		private readonly LCChartUserControl _realtimeChart;

		// Token: 0x040000EC RID: 236
		private SystemCondition _storyboardCondition;

		// Token: 0x040000ED RID: 237
		private ProcedureExecutionState _lastExecutionState;

		// Token: 0x040000EE RID: 238
		private SystemInfoWindow _dlgSystemInfo;

		// Token: 0x040000EF RID: 239
		private readonly Window _sysInfoOwner;

		// Token: 0x040000F0 RID: 240
		private bool _isSystemWindowOpen;

		// Token: 0x040000F1 RID: 241
		private bool _isScripInProgress;

		// Token: 0x040000F2 RID: 242
		private bool _isService;

		// Token: 0x040000F3 RID: 243
		private readonly DiagramStateController _diagramStateController;

	private Window _popoutWindow;
	private void Diagram_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (_popoutWindow != null && _popoutWindow.IsVisible) { _popoutWindow.Activate(); return; }
		VisualBrush brush = new VisualBrush(this.Diagram); brush.Stretch = Stretch.Uniform;
		System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle(); rect.Fill = brush; rect.Margin = new Thickness(10);
		Border popBorder = new Border(); popBorder.Background = Brushes.White; popBorder.Child = rect;
		_popoutWindow = new Window();
		_popoutWindow.Title = "proteoElute - System Diagram (ProteYOLUTE by Michael Krawitzky)";
		_popoutWindow.Width = 900; _popoutWindow.Height = 750;
		_popoutWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		_popoutWindow.Content = popBorder; _popoutWindow.Background = Brushes.White;
		_popoutWindow.ResizeMode = ResizeMode.CanResizeWithGrip;
		_popoutWindow.Closed += delegate { _popoutWindow = null; };
		_popoutWindow.Show(); e.Handled = true;
	}

		// Token: 0x020000C0 RID: 192
		// (Invoke) Token: 0x06000700 RID: 1792
		private delegate void ShowSystemConditionCallback(SystemCondition condition);

		// Token: 0x020000C1 RID: 193
		// (Invoke) Token: 0x06000704 RID: 1796
		private delegate void ConfirmConditionCallback(object sender, RoutedEventArgs e);

		// Token: 0x020000C2 RID: 194
		// (Invoke) Token: 0x06000708 RID: 1800
		public delegate void ExecutionReport(ExecutionStateChangedEventArgs e);

		// Token: 0x020000C3 RID: 195
		// (Invoke) Token: 0x0600070C RID: 1804
		public delegate void ConfirmButtonOnErrorCallback(SystemCondition condition);

		// Token: 0x020000C4 RID: 196
		private class NotifyCollectionChangedSynchronizer<T> : INotifyCollectionChanged, IEnumerable<T>, IEnumerable
		{
			// Token: 0x14000044 RID: 68
			// (add) Token: 0x0600070F RID: 1807 RVA: 0x0003C2FC File Offset: 0x0003A4FC
			// (remove) Token: 0x06000710 RID: 1808 RVA: 0x0003C334 File Offset: 0x0003A534
			public event NotifyCollectionChangedEventHandler CollectionChanged;

			// Token: 0x06000711 RID: 1809 RVA: 0x0003C36C File Offset: 0x0003A56C
			public NotifyCollectionChangedSynchronizer(IEnumerable<T> source)
			{
				this._enumerable = source;
				INotifyCollectionChanged ncc = source as INotifyCollectionChanged;
				if (ncc != null)
				{
					ncc.CollectionChanged += this.ncc_CollectionChanged;
				}
			}

			// Token: 0x06000712 RID: 1810 RVA: 0x0003C3A4 File Offset: 0x0003A5A4
			private void ncc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				NotifyCollectionChangedEventHandler handlers = this.CollectionChanged;
				if (handlers == null)
				{
					return;
				}
				e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
				foreach (NotifyCollectionChangedEventHandler handler in handlers.GetInvocationList())
				{
					DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
					if (dispatcherObject != null && !dispatcherObject.CheckAccess())
					{
						dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, new object[] { e });
					}
					else
					{
						handler(this, e);
					}
				}
			}

			// Token: 0x06000713 RID: 1811 RVA: 0x0003C420 File Offset: 0x0003A620
			public IEnumerator<T> GetEnumerator()
			{
				return this._enumerable.GetEnumerator();
			}

			// Token: 0x06000714 RID: 1812 RVA: 0x0003C42D File Offset: 0x0003A62D
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			// Token: 0x040003B5 RID: 949
			private readonly IEnumerable<T> _enumerable;
		}

		// Token: 0x020000C5 RID: 197
		[CompilerGenerated]
		private static class _SDC
		{
			// Token: 0x040003B7 RID: 951
			public static Func<MeteringDataPoint, MeteringDataPoint> _cm_Bars2Psi;
		}
	}
}
