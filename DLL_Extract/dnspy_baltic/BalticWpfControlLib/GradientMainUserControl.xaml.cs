// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using BalticWpfControlLib.Controls;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Microsoft.CSharp.RuntimeBinder;
using Syncfusion.UI.Xaml.Charts;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib
{
	// Token: 0x0200001F RID: 31
	public partial class GradientMainUserControl : UserControl, INotifyPropertyChanged
	{
		// Auto-generated callsite cache class
		private static class _co_83
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
			public static dynamic _cp_3;
			public static dynamic _cp_4;
			public static dynamic _cp_5;
			public static dynamic _cp_6;
			public static dynamic _cp_7;
			public static dynamic _cp_8;
			public static dynamic _cp_9;
		}

		// Auto-generated callsite cache class
		private static class _co_72
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
		}

		// Auto-generated callsite cache class
		private static class _co_65
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
			public static dynamic _cp_3;
		}

		// Auto-generated callsite cache class
		private static class _co_63
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
			public static dynamic _cp_3;
		}

		// Auto-generated callsite cache class
		private static class _co_62
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
			public static dynamic _cp_3;
			public static dynamic _cp_4;
			public static dynamic _cp_5;
			public static dynamic _cp_6;
			public static dynamic _cp_7;
		}

		// Auto-generated callsite cache class
		private static class _co_61
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
			public static dynamic _cp_3;
		}

		// Auto-generated callsite cache class
		private static class _co_49
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
			public static dynamic _cp_3;
			public static dynamic _cp_4;
		}

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x060000F0 RID: 240 RVA: 0x00006454 File Offset: 0x00004654
		// (remove) Token: 0x060000F1 RID: 241 RVA: 0x0000648C File Offset: 0x0000468C
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x060000F2 RID: 242 RVA: 0x000064C1 File Offset: 0x000046C1
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x14000012 RID: 18
		// (add) Token: 0x060000F3 RID: 243 RVA: 0x000064DC File Offset: 0x000046DC
		// (remove) Token: 0x060000F4 RID: 244 RVA: 0x00006514 File Offset: 0x00004714
		public event GradientMainUserControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x060000F5 RID: 245 RVA: 0x0000654C File Offset: 0x0000474C
		// (remove) Token: 0x060000F6 RID: 246 RVA: 0x00006584 File Offset: 0x00004784
		public event GradientMainUserControl.GetInitialMethodParam GetInitialMethodParamEvent;

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x060000F7 RID: 247 RVA: 0x000065BC File Offset: 0x000047BC
		// (remove) Token: 0x060000F8 RID: 248 RVA: 0x000065F4 File Offset: 0x000047F4
		public event GradientMainUserControl.GenerateMethod GenerateMethodEvent;

		// Token: 0x14000015 RID: 21
		// (add) Token: 0x060000F9 RID: 249 RVA: 0x0000662C File Offset: 0x0000482C
		// (remove) Token: 0x060000FA RID: 250 RVA: 0x00006664 File Offset: 0x00004864
		public event GradientMainUserControl.EnableMethodControls EnableMethodControlsEvent;

		// Token: 0x14000016 RID: 22
		// (add) Token: 0x060000FB RID: 251 RVA: 0x0000669C File Offset: 0x0000489C
		// (remove) Token: 0x060000FC RID: 252 RVA: 0x000066D4 File Offset: 0x000048D4
		public event GradientMainUserControl.TrapSelectionWarning TrapSelectionWarningEvent;

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000FD RID: 253 RVA: 0x00006709 File Offset: 0x00004909
		public BindableBalticMethod Method { get; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000FE RID: 254 RVA: 0x00006711 File Offset: 0x00004911
		// (set) Token: 0x060000FF RID: 255 RVA: 0x00006719 File Offset: 0x00004919
		public string TempToolTip
		{
			get
			{
				return this._tempToolTip;
			}
			set
			{
				this._tempToolTip = value;
				this.NotifyPropertyChanged("TempToolTip");
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000100 RID: 256 RVA: 0x0000672D File Offset: 0x0000492D
		// (set) Token: 0x06000101 RID: 257 RVA: 0x00006735 File Offset: 0x00004935
		public string AnalysisTimeToolTip
		{
			get
			{
				return this._analysisTimeToolTip;
			}
			set
			{
				this._analysisTimeToolTip = value;
				this.NotifyPropertyChanged("AnalysisTimeToolTip");
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000102 RID: 258 RVA: 0x00006749 File Offset: 0x00004949
		public ObservableCollection<BindableBalticMethod.ElutionType> ExperimentNames { get; } = new ObservableCollection<BindableBalticMethod.ElutionType>();

		// Token: 0x06000103 RID: 259 RVA: 0x00006754 File Offset: 0x00004954
		public GradientMainUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, BalticColumnList columns, ColumnSelections columnSelections, bool isOvenDetected)
		{
			this.Method = method;
			this._instrument = instrument;
			this._columns = columns;
			this._isOvenDetected = isOvenDetected;
			foreach (BindableBalticMethod.ElutionType t in this.Method.ExperimentTypes)
			{
				if (this.Method.ElutionName == t.Name)
				{
					this._selectedElutionType = t;
					this._legacyMode = t.IsLegacy;
					this.ExperimentNames.Add(t);
				}
				else if (!t.IsLegacy)
				{
					this.ExperimentNames.Add(t);
				}
			}
			this._columnSelections = columnSelections;
			BalticColumnList separators = new BalticColumnList();
			BalticColumnList traps = new BalticColumnList();
			foreach (Column column in columns)
			{
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
			this.InitializeComponent();
			base.DataContext = this;
			this.comboTrapColumn.ItemsSource = traps;
			this.comboTrapColumn.DisplayMemberPath = "Name";
			this.comboTrapColumn.SelectedValuePath = "Name";
			this.comboSepColumn.ItemsSource = separators;
			this.comboSepColumn.DisplayMemberPath = "Name";
			this.comboSepColumn.SelectedValuePath = "Name";
			BindableBalticMethod bindableBalticMethod = this.Method;
			if (bindableBalticMethod.TrapColumnName == null)
			{
				bindableBalticMethod.TrapColumnName = this._columnSelections.PreColumnName;
			}
			bindableBalticMethod = this.Method;
			if (bindableBalticMethod.SeparationColumnName == null)
			{
				bindableBalticMethod.SeparationColumnName = this._columnSelections.AnalyticalColumnName;
			}
			if (traps.Contains(this.Method.TrapColumnName))
			{
				this.comboTrapColumn.SelectedValue = this.Method.TrapColumnName;
			}
			this.comboTrapColumn.Text = this.Method.TrapColumnName;
			if (separators.Contains(this.Method.SeparationColumnName))
			{
				this.comboSepColumn.SelectedValue = this.Method.SeparationColumnName;
			}
			this.comboSepColumn.Text = this.Method.SeparationColumnName;
			this.TrapColumnInfoControl.DataContext = (Column)this.comboTrapColumn.SelectedItem;
			this.SepColumnInfoControl.DataContext = (Column)this.comboSepColumn.SelectedItem;
			if (this.Method.ElutionName != null)
			{
				this.UpdateExperimentInfo();
			}
			if (GradientMainUserControl._co_49._cp_4 == null)
			{
				GradientMainUserControl._co_49._cp_4 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			Func<CallSite, object, bool> target = GradientMainUserControl._co_49._cp_4.Target;
			CallSite _cpl = GradientMainUserControl._co_49._cp_4;
			bool usesTrapColumn = this.Method.UsesTrapColumn;
			object obj;
			if (usesTrapColumn)
			{
				if (GradientMainUserControl._co_49._cp_3 == null)
				{
					GradientMainUserControl._co_49._cp_3 = CallSite<Func<CallSite, bool, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, bool, object, object> target2 = GradientMainUserControl._co_49._cp_3.Target;
				CallSite _cp_2 = GradientMainUserControl._co_49._cp_3;
				bool flag = usesTrapColumn;
				if (GradientMainUserControl._co_49._cp_2 == null)
				{
					GradientMainUserControl._co_49._cp_2 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, string, object, object> target3 = GradientMainUserControl._co_49._cp_2.Target;
				CallSite _cp_3 = GradientMainUserControl._co_49._cp_2;
				string trapColumnName = this.Method.TrapColumnName;
				if (GradientMainUserControl._co_49._cp_1 == null)
				{
					GradientMainUserControl._co_49._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				Func<CallSite, object, object> target4 = GradientMainUserControl._co_49._cp_1.Target;
				CallSite _cp_4 = GradientMainUserControl._co_49._cp_1;
				if (GradientMainUserControl._co_49._cp_0 == null)
				{
					GradientMainUserControl._co_49._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "NoColumn", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				obj = target2(_cp_2, flag, target3(_cp_3, trapColumnName, target4(_cp_4, GradientMainUserControl._co_49._cp_0.Target(GradientMainUserControl._co_49._cp_0, this._instrument.BalticConfiguration))));
			}
			else
			{
				obj = usesTrapColumn;
			}
			if (target(_cpl, obj))
			{
				GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent = this.TrapSelectionWarningEvent;
				if (trapSelectionWarningEvent == null)
				{
					return;
				}
				trapSelectionWarningEvent(true, "\"None\" trap column selection is not recommended !");
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000104 RID: 260 RVA: 0x00006BF0 File Offset: 0x00004DF0
		// (set) Token: 0x06000105 RID: 261 RVA: 0x00006BF8 File Offset: 0x00004DF8
		public BindableBalticMethod.ElutionType SelectedElutionType
		{
			get
			{
				return this._selectedElutionType;
			}
			set
			{
				if (this._selectedElutionType == value)
				{
					return;
				}
				this._selectedElutionType = value;
				this.NotifyPropertyChanged("SelectedElutionType");
				BindableBalticMethod.ElutionType selectedElutionType = this.SelectedElutionType;
				if (selectedElutionType != null && !selectedElutionType.HasLegacyOption)
				{
					this.LegacyMode = false;
				}
				this.UpdateMethod();
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000106 RID: 262 RVA: 0x00006C45 File Offset: 0x00004E45
		// (set) Token: 0x06000107 RID: 263 RVA: 0x00006C4D File Offset: 0x00004E4D
		public bool LegacyMode
		{
			get
			{
				return this._legacyMode;
			}
			set
			{
				this._legacyMode = value;
				this.NotifyPropertyChanged("LegacyMode");
				this.UpdateMethod();
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000108 RID: 264 RVA: 0x00006C67 File Offset: 0x00004E67
		public bool IsLegacyModeSelectable
		{
			get
			{
				return this.SelectedElutionType != null && this.SelectedElutionType.HasLegacyOption && !this.btnReset.IsVisible;
			}
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00006C90 File Offset: 0x00004E90
		private void UpdateMethod()
		{
			if (this.SelectedElutionType == null)
			{
				this.Method.ElutionName = null;
				return;
			}
			this.Method.ElutionName = (this.LegacyMode ? this.SelectedElutionType.LegacyName : this.SelectedElutionType.Name);
			GradientMainUserControl.GetInitialMethodParam getInitialMethodParamEvent = this.GetInitialMethodParamEvent;
			if (getInitialMethodParamEvent != null)
			{
				getInitialMethodParamEvent();
			}
			this.UpdateConfiguration(this.Method, false);
			this.Method.TrapColumnName = this._columnSelections.PreColumnName;
			this.Method.SeparationColumnName = this._columnSelections.AnalyticalColumnName;
			this.comboTrapColumn.SelectedValue = this.Method.TrapColumnName;
			this.comboSepColumn.SelectedValue = this.Method.SeparationColumnName;
			this.CheckGenerateEnable();
			this.SetDefaultToolTip();
			this.UpdateTrapWarning();
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00006D68 File Offset: 0x00004F68
		private void UpdateTrapWarning()
		{
			if (this.Method.UsesTrapColumn)
			{
				if (GradientMainUserControl._co_61._cp_3 == null)
				{
					GradientMainUserControl._co_61._cp_3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				Func<CallSite, object, bool> target = GradientMainUserControl._co_61._cp_3.Target;
				CallSite _cpl = GradientMainUserControl._co_61._cp_3;
				if (GradientMainUserControl._co_61._cp_2 == null)
				{
					GradientMainUserControl._co_61._cp_2 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, string, object, object> target2 = GradientMainUserControl._co_61._cp_2.Target;
				CallSite _cp_2 = GradientMainUserControl._co_61._cp_2;
				string trapColumnName = this.Method.TrapColumnName;
				if (GradientMainUserControl._co_61._cp_1 == null)
				{
					GradientMainUserControl._co_61._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				Func<CallSite, object, object> target3 = GradientMainUserControl._co_61._cp_1.Target;
				CallSite _cp_3 = GradientMainUserControl._co_61._cp_1;
				if (GradientMainUserControl._co_61._cp_0 == null)
				{
					GradientMainUserControl._co_61._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "NoColumn", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				if (target(_cpl, target2(_cp_2, trapColumnName, target3(_cp_3, GradientMainUserControl._co_61._cp_0.Target(GradientMainUserControl._co_61._cp_0, this._instrument.BalticConfiguration)))))
				{
					GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent = this.TrapSelectionWarningEvent;
					if (trapSelectionWarningEvent == null)
					{
						return;
					}
					trapSelectionWarningEvent(true, "\"None\" trap column selection is not recommended !");
					return;
				}
				else
				{
					GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent2 = this.TrapSelectionWarningEvent;
					if (trapSelectionWarningEvent2 == null)
					{
						return;
					}
					trapSelectionWarningEvent2(false, "");
					return;
				}
			}
			else
			{
				GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent3 = this.TrapSelectionWarningEvent;
				if (trapSelectionWarningEvent3 == null)
				{
					return;
				}
				trapSelectionWarningEvent3(false, "");
				return;
			}
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00006F10 File Offset: 0x00005110
		private void UpdateExperimentInfo()
		{
			this._experiment = new ExperimentInfo
			{
				ElutionName = this.Method.ElutionName,
				AnalysisTime = TimeSpan.FromMinutes(this.Method.GradientTime),
				OvenTemperature = this.Method.OvenTemperature,
				AppKey = this._instrument.AppKey
			};
			if (this.Method.UsesTrapColumn)
			{
				Column column = this._columns.Find((Column item) => item.Name == this.Method.TrapColumnName);
				this._experiment.Trap = new ColumnAdapter(column);
			}
			else
			{
				Column column2 = this._columns.Find((Column item) => item.Name == this.Method.TrapColumnName) ?? this._columns.Find(delegate(Column item)
				{
					if (GradientMainUserControl._co_62._cp_3 == null)
					{
						GradientMainUserControl._co_62._cp_3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(GradientMainUserControl)));
					}
					Func<CallSite, object, bool> target = GradientMainUserControl._co_62._cp_3.Target;
					CallSite _cpl = GradientMainUserControl._co_62._cp_3;
					if (GradientMainUserControl._co_62._cp_2 == null)
					{
						GradientMainUserControl._co_62._cp_2 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, string, object, object> target2 = GradientMainUserControl._co_62._cp_2.Target;
					CallSite _cp_2 = GradientMainUserControl._co_62._cp_2;
					string name = item.Name;
					if (GradientMainUserControl._co_62._cp_1 == null)
					{
						GradientMainUserControl._co_62._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target3 = GradientMainUserControl._co_62._cp_1.Target;
					CallSite _cp_3 = GradientMainUserControl._co_62._cp_1;
					if (GradientMainUserControl._co_62._cp_0 == null)
					{
						GradientMainUserControl._co_62._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "NoColumn", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					return target(_cpl, target2(_cp_2, name, target3(_cp_3, GradientMainUserControl._co_62._cp_0.Target(GradientMainUserControl._co_62._cp_0, this._instrument.BalticConfiguration))));
				});
				this._experiment.Trap = new ColumnAdapter(column2);
			}
			if (this.Method.UsesSepColumn)
			{
				Column column3 = this._columns.Find((Column item) => item.Name == this.Method.SeparationColumnName);
				this._experiment.Separator = new ColumnAdapter(column3);
			}
			else
			{
				Column column4 = this._columns.Find((Column item) => item.Name == this.Method.SeparationColumnName) ?? this._columns.Find(delegate(Column item)
				{
					if (GradientMainUserControl._co_62._cp_7 == null)
					{
						GradientMainUserControl._co_62._cp_7 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(GradientMainUserControl)));
					}
					Func<CallSite, object, bool> target4 = GradientMainUserControl._co_62._cp_7.Target;
					CallSite _cp_4 = GradientMainUserControl._co_62._cp_7;
					if (GradientMainUserControl._co_62._cp_6 == null)
					{
						GradientMainUserControl._co_62._cp_6 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, string, object, object> target5 = GradientMainUserControl._co_62._cp_6.Target;
					CallSite _cp_5 = GradientMainUserControl._co_62._cp_6;
					string name2 = item.Name;
					if (GradientMainUserControl._co_62._cp_5 == null)
					{
						GradientMainUserControl._co_62._cp_5 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target6 = GradientMainUserControl._co_62._cp_5.Target;
					CallSite _cp_6 = GradientMainUserControl._co_62._cp_5;
					if (GradientMainUserControl._co_62._cp_4 == null)
					{
						GradientMainUserControl._co_62._cp_4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "NoColumn", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					return target4(_cp_4, target5(_cp_5, name2, target6(_cp_6, GradientMainUserControl._co_62._cp_4.Target(GradientMainUserControl._co_62._cp_4, this._instrument.BalticConfiguration))));
				});
				this._experiment.Separator = new ColumnAdapter(column4);
			}
			this.UpdateTrapWarning();
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00007070 File Offset: 0x00005270
		private void UpdateConfiguration(BindableBalticMethod method, bool isInitialize = false)
		{
			if (method.ElutionName == null)
			{
				this.tbAnalysisTime.Visibility = Visibility.Hidden;
				this.tcGradientTime.Visibility = Visibility.Hidden;
				this.tbMin.Visibility = Visibility.Hidden;
				this.stkOvenTemp.Visibility = Visibility.Hidden;
				this.txtOvenTemp.Visibility = Visibility.Hidden;
				this.tbDegC.Visibility = Visibility.Hidden;
				this.imgTempInfo.Visibility = Visibility.Hidden;
				this.stkGradient.Visibility = Visibility.Hidden;
				this.btnGenerate.Visibility = Visibility.Hidden;
				this.btnAdapt.Visibility = Visibility.Hidden;
				this.btnReset.Visibility = Visibility.Hidden;
				this.tbTrapColumn.Visibility = Visibility.Hidden;
				this.comboTrapColumn.Visibility = Visibility.Hidden;
				this.tbSepColumn.Visibility = Visibility.Hidden;
				this.comboSepColumn.Visibility = Visibility.Hidden;
				this.btnAdapt.IsEnabled = false;
				this.comboTrapColumn.IsEnabled = true;
				this.comboTrapColumn.IsEditable = false;
				this.comboSepColumn.IsEnabled = true;
				this.comboSepColumn.IsEditable = false;
				GradientMainUserControl.EnableMethodControls enableMethodControlsEvent = this.EnableMethodControlsEvent;
				if (enableMethodControlsEvent != null)
				{
					enableMethodControlsEvent(false, false);
				}
			}
			else
			{
				this.tbTrapColumn.Visibility = (method.UsesTrapColumn ? Visibility.Visible : Visibility.Hidden);
				this.comboTrapColumn.Visibility = (method.UsesTrapColumn ? Visibility.Visible : Visibility.Hidden);
				this.tbSepColumn.Visibility = (method.UsesSepColumn ? Visibility.Visible : Visibility.Hidden);
				this.comboSepColumn.Visibility = (method.UsesSepColumn ? Visibility.Visible : Visibility.Hidden);
				this.tbAnalysisTime.Visibility = (isInitialize ? Visibility.Hidden : Visibility.Visible);
				this.tcGradientTime.Visibility = (isInitialize ? Visibility.Hidden : Visibility.Visible);
				this.tbMin.Visibility = (isInitialize ? Visibility.Hidden : Visibility.Visible);
				this.stkOvenTemp.Visibility = Visibility.Visible;
				this.imgTempInfo.Visibility = ((!this._isOvenDetected) ? Visibility.Visible : Visibility.Hidden);
				this.txtOvenTemp.Visibility = Visibility.Visible;
				this.tbDegC.Visibility = Visibility.Visible;
				this.stkGradient.Visibility = Visibility.Visible;
				this.btnGenerate.Visibility = (isInitialize ? Visibility.Hidden : Visibility.Visible);
				this.btnAdapt.Visibility = (isInitialize ? Visibility.Visible : Visibility.Hidden);
				this.btnReset.Visibility = (isInitialize ? Visibility.Visible : Visibility.Hidden);
				this.comboExpType.IsEnabled = !isInitialize;
				this.comboTrapColumn.IsEnabled = true;
				this.comboTrapColumn.IsEditable = false;
				this.comboSepColumn.IsEnabled = true;
				this.comboSepColumn.IsEditable = false;
				this.btnAdapt.IsEnabled = false;
				GradientMainUserControl.EnableMethodControls enableMethodControlsEvent2 = this.EnableMethodControlsEvent;
				if (enableMethodControlsEvent2 != null)
				{
					enableMethodControlsEvent2(isInitialize, false);
				}
				this.CheckGenerateEnable();
			}
			this.NotifyPropertyChanged("IsLegacyModeSelectable");
			if (this.Method.UsesTrapColumn)
			{
				if (GradientMainUserControl._co_63._cp_3 == null)
				{
					GradientMainUserControl._co_63._cp_3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				Func<CallSite, object, bool> target = GradientMainUserControl._co_63._cp_3.Target;
				CallSite _cpl = GradientMainUserControl._co_63._cp_3;
				if (GradientMainUserControl._co_63._cp_2 == null)
				{
					GradientMainUserControl._co_63._cp_2 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
					}));
				}
				Func<CallSite, string, object, object> target2 = GradientMainUserControl._co_63._cp_2.Target;
				CallSite _cp_2 = GradientMainUserControl._co_63._cp_2;
				string trapColumnName = this.Method.TrapColumnName;
				if (GradientMainUserControl._co_63._cp_1 == null)
				{
					GradientMainUserControl._co_63._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				Func<CallSite, object, object> target3 = GradientMainUserControl._co_63._cp_1.Target;
				CallSite _cp_3 = GradientMainUserControl._co_63._cp_1;
				if (GradientMainUserControl._co_63._cp_0 == null)
				{
					GradientMainUserControl._co_63._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "NoColumn", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
				}
				if (target(_cpl, target2(_cp_2, trapColumnName, target3(_cp_3, GradientMainUserControl._co_63._cp_0.Target(GradientMainUserControl._co_63._cp_0, this._instrument.BalticConfiguration)))))
				{
					GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent = this.TrapSelectionWarningEvent;
					if (trapSelectionWarningEvent == null)
					{
						return;
					}
					trapSelectionWarningEvent(true, "\"None\" trap column selection is not recommended !");
					return;
				}
				else
				{
					GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent2 = this.TrapSelectionWarningEvent;
					if (trapSelectionWarningEvent2 == null)
					{
						return;
					}
					trapSelectionWarningEvent2(false, "");
					return;
				}
			}
			else
			{
				GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent3 = this.TrapSelectionWarningEvent;
				if (trapSelectionWarningEvent3 == null)
				{
					return;
				}
				trapSelectionWarningEvent3(false, "");
				return;
			}
		}

		// Token: 0x0600010D RID: 269 RVA: 0x000074AC File Offset: 0x000056AC
		private void GradientMainUserControl_Loaded(object sender, RoutedEventArgs e)
		{
			this.sfGradientChart.Series[0].ItemsSource = this.Method.GradientTable;
			this.sfGradientChart.Series[1].ItemsSource = this.Method.GradientTable;
			this.UpdateConfiguration(this.Method, true);
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00007508 File Offset: 0x00005708
		private void PreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.comboTrapColumn.SelectedValue != null)
			{
				if (this.Method.TrapColumnName != (string)this.comboTrapColumn.SelectedValue && !this.btnGenerate.IsVisible)
				{
					this.btnAdapt.IsEnabled = true;
				}
				this.Method.TrapColumnName = (string)this.comboTrapColumn.SelectedValue;
				this.TrapColumnInfoControl.DataContext = this._columns.Find((Column item) => item.Name == this.Method.TrapColumnName);
				this.SendValidationUpdateEvent();
				if (this.Method.UsesTrapColumn)
				{
					if (GradientMainUserControl._co_65._cp_3 == null)
					{
						GradientMainUserControl._co_65._cp_3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, bool> target = GradientMainUserControl._co_65._cp_3.Target;
					CallSite _cpl = GradientMainUserControl._co_65._cp_3;
					if (GradientMainUserControl._co_65._cp_2 == null)
					{
						GradientMainUserControl._co_65._cp_2 = CallSite<Func<CallSite, string, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
						}));
					}
					Func<CallSite, string, object, object> target2 = GradientMainUserControl._co_65._cp_2.Target;
					CallSite _cp_2 = GradientMainUserControl._co_65._cp_2;
					string trapColumnName = this.Method.TrapColumnName;
					if (GradientMainUserControl._co_65._cp_1 == null)
					{
						GradientMainUserControl._co_65._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target3 = GradientMainUserControl._co_65._cp_1.Target;
					CallSite _cp_3 = GradientMainUserControl._co_65._cp_1;
					if (GradientMainUserControl._co_65._cp_0 == null)
					{
						GradientMainUserControl._co_65._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "NoColumn", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					if (target(_cpl, target2(_cp_2, trapColumnName, target3(_cp_3, GradientMainUserControl._co_65._cp_0.Target(GradientMainUserControl._co_65._cp_0, this._instrument.BalticConfiguration)))))
					{
						GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent = this.TrapSelectionWarningEvent;
						if (trapSelectionWarningEvent != null)
						{
							trapSelectionWarningEvent(true, "\"None\" trap column selection is not recommended !");
						}
					}
					else
					{
						GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent2 = this.TrapSelectionWarningEvent;
						if (trapSelectionWarningEvent2 != null)
						{
							trapSelectionWarningEvent2(false, "");
						}
					}
				}
				else
				{
					GradientMainUserControl.TrapSelectionWarning trapSelectionWarningEvent3 = this.TrapSelectionWarningEvent;
					if (trapSelectionWarningEvent3 != null)
					{
						trapSelectionWarningEvent3(false, "");
					}
				}
			}
			this.CheckGenerateEnable();
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00007748 File Offset: 0x00005948
		private void AnalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.comboSepColumn.SelectedValue != null)
			{
				if (this.Method.SeparationColumnName != (string)this.comboSepColumn.SelectedValue && !this.btnGenerate.IsVisible)
				{
					this.btnAdapt.IsEnabled = true;
				}
				this.Method.SeparationColumnName = (string)this.comboSepColumn.SelectedValue;
				this.SepColumnInfoControl.DataContext = this._columns.Find((Column item) => item.Name == this.Method.SeparationColumnName);
				this.SendValidationUpdateEvent();
			}
			this.CheckGenerateEnable();
		}

		// Token: 0x06000110 RID: 272 RVA: 0x000077E8 File Offset: 0x000059E8
		private void CheckGenerateEnable()
		{
			if (this.btnGenerate == null)
			{
				return;
			}
			this.UpdateExperimentInfo();
			bool isGenEnable = true;
			if (this.Method.UsesTrapColumn && this.Method.TrapColumnName == null)
			{
				isGenEnable = false;
				this.comboTrapColumn.SelectedValue = null;
			}
			if (this.Method.UsesSepColumn && this.Method.SeparationColumnName == null)
			{
				isGenEnable = false;
				this.comboSepColumn.SelectedValue = null;
			}
			if (this._isValidateError || this._isValidateTimeError || this._isTimeIllegalChar)
			{
				isGenEnable = false;
			}
			this.btnGenerate.IsEnabled = isGenEnable;
		}

		// Token: 0x06000111 RID: 273 RVA: 0x0000787C File Offset: 0x00005A7C
		private void GradientTime_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				if (this.Method.GradientTime - 1.0 >= 0.0)
				{
					this.Method.GradientTime -= 1.0;
					return;
				}
			}
			else if (this.Method.GradientTime + 1.0 <= 999.0)
			{
				this.Method.GradientTime += 1.0;
			}
		}

		// Token: 0x06000112 RID: 274 RVA: 0x0000790C File Offset: 0x00005B0C
		private void tcGradientTime_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (this.Method.ElutionName != null && this.Method.SeparationColumnName != null)
			{
				double gradientTime = this.Method.GradientTime;
				bool flag = gradientTime < 0.0 || gradientTime > 999.0;
				if (flag)
				{
					this.SetErrorCondition("analysis_time", "Value must be between 0 and 999");
					return;
				}
				this._isValidateTimeError = false;
				this.ResetErrorConditions();
			}
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00007980 File Offset: 0x00005B80
		private void FlowAxis_ActualRangeChanged(object sender, ActualRangeChangedEventArgs e)
		{
			double yMax = 0.0;
			foreach (BalticGradientItem item in this.Method.GradientTable)
			{
				if (item.Flow > yMax)
				{
					yMax = item.Flow;
				}
			}
			e.ActualMaximum = ((yMax * 2.0 < 1.0) ? 1.0 : (yMax * 2.0));
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00007A1C File Offset: 0x00005C1C
		public void UpdateFromGradientTable(WPFConstants.UpdateType updateType)
		{
			if (updateType == WPFConstants.UpdateType.AddRemove)
			{
				GradientMainUserControl.EnableMethodControls enableMethodControlsEvent = this.EnableMethodControlsEvent;
				if (enableMethodControlsEvent == null)
				{
					return;
				}
				enableMethodControlsEvent(true, false);
			}
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00007A34 File Offset: 0x00005C34
		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
			this.comboExpType.IsEnabled = true;
			this.btnAdapt.IsEnabled = false;
			foreach (BindableBalticMethod.ElutionType item in this.ExperimentNames.Where((BindableBalticMethod.ElutionType exp) => exp.IsLegacy).ToArray<BindableBalticMethod.ElutionType>())
			{
				this.ExperimentNames.Remove(item);
			}
			if (GradientMainUserControl._co_72._cp_2 == null)
			{
				GradientMainUserControl._co_72._cp_2 = CallSite<Action<CallSite, BindableBalticMethod, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "Reset", null, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			Action<CallSite, BindableBalticMethod, object> target = GradientMainUserControl._co_72._cp_2.Target;
			CallSite _cpl = GradientMainUserControl._co_72._cp_2;
			BindableBalticMethod method = this.Method;
			if (GradientMainUserControl._co_72._cp_1 == null)
			{
				GradientMainUserControl._co_72._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ColumnOvenMinTemperature", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			Func<CallSite, object, object> target2 = GradientMainUserControl._co_72._cp_1.Target;
			CallSite _cp_2 = GradientMainUserControl._co_72._cp_1;
			if (GradientMainUserControl._co_72._cp_0 == null)
			{
				GradientMainUserControl._co_72._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Settings", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			target(_cpl, method, target2(_cp_2, GradientMainUserControl._co_72._cp_0.Target(GradientMainUserControl._co_72._cp_0, this._instrument.BalticConfiguration)));
			this.UpdateConfiguration(this.Method, true);
			GradientMainUserControl.EnableMethodControls enableMethodControlsEvent = this.EnableMethodControlsEvent;
			if (enableMethodControlsEvent != null)
			{
				enableMethodControlsEvent(true, true);
			}
			this._isValidateTempError = (this._isValidateTimeError = (this._isTempIllegalChar = (this._isTimeIllegalChar = (this._isValidateError = false))));
			this.btnGenerate.IsEnabled = true;
			this.SelectedElutionType = null;
			this.LegacyMode = false;
			this.ResetErrorConditions();
			this.NotifyPropertyChanged("IsLegacyModeSelectable");
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00007C1C File Offset: 0x00005E1C
		private void btnGenerate_Click(object sender, RoutedEventArgs e)
		{
			this.GenerateOrAdaptMethod(false, false);
			this.SendValidationUpdateEvent();
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00007C2C File Offset: 0x00005E2C
		private void btnAdapt_Click(object sender, RoutedEventArgs e)
		{
			this.GenerateOrAdaptMethod(true, true);
			this.SendValidationUpdateEvent();
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00007C3C File Offset: 0x00005E3C
		private void GenerateOrAdaptMethod(bool keepGradient, bool keepAdvancedSettings)
		{
			GradientMainUserControl.GenerateMethod generateMethodEvent = this.GenerateMethodEvent;
			if (generateMethodEvent != null)
			{
				generateMethodEvent(keepGradient, keepAdvancedSettings);
			}
			this.UpdateConfiguration(this.Method, false);
			this.btnGenerate.Visibility = Visibility.Hidden;
			this.btnAdapt.Visibility = Visibility.Visible;
			this.btnReset.Visibility = Visibility.Visible;
			this.tbAnalysisTime.Visibility = Visibility.Hidden;
			this.tcGradientTime.Visibility = Visibility.Hidden;
			this.stkOvenTemp.Visibility = Visibility.Visible;
			this.txtOvenTemp.Visibility = Visibility.Visible;
			this.tbDegC.Visibility = Visibility.Visible;
			this.imgTempInfo.Visibility = ((!this._isOvenDetected) ? Visibility.Visible : Visibility.Hidden);
			this.tbMin.Visibility = Visibility.Hidden;
			this.comboExpType.IsEnabled = false;
			this.comboTrapColumn.IsEnabled = true;
			this.comboSepColumn.IsEnabled = true;
			this.btnAdapt.IsEnabled = false;
			this.NotifyPropertyChanged("IsLegacyModeSelectable");
			GradientMainUserControl.EnableMethodControls enableMethodControlsEvent = this.EnableMethodControlsEvent;
			if (enableMethodControlsEvent != null)
			{
				enableMethodControlsEvent(true, false);
			}
			this.ValidateParameters();
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00007D40 File Offset: 0x00005F40
		private void txtOvenTemp_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00007D48 File Offset: 0x00005F48
		private void ValidateParameters()
		{
			if (this._instrument == null || this.Method.ElutionName == null || this.Method.SeparationColumnName == null)
			{
				return;
			}
			BalticMethod method = this.Method.ToBalticMethod(this._columns);
			ProcedureInfo procedure = this._instrument.GetElutionProcedure(method.ElutionName);
			ProcedureArguments arguments = procedure.CreateArguments();
			ProcedureArguments advArguments = procedure.CreateAdvancedArguments();
			ChildProcedureArguments advChildArguments = procedure.CreateAdvancedChildArguments();
			ElutionMethodUtil.PopulateMethodArguments(this._instrument.IsColumnOvenConnected, method, arguments, advArguments, advChildArguments, null, null);
			foreach (ProcedureArgument item in advArguments)
			{
				arguments.Add(new ProcedureArgument(item));
			}
			this._instrument.ValidationMessageReported += this.ValidationErrorHandler;
			try
			{
				this._isValidateError = (this._isValidateTempError = false);
				if (this._experiment != null)
				{
					this._instrument.ValidateMethodProcedureOffLine(this._experiment, procedure, arguments, advChildArguments);
					if (arguments.Contains("calibrantTime"))
					{
						this.Method.AdvancedSettings.CalibrantTime = (double)arguments["calibrantTime"].Value;
					}
					if (arguments.Contains("column_load_time"))
					{
						this.Method.SampleLoading.EquilTime = (double)arguments["column_load_time"].Value;
					}
					if (arguments.Contains("separator_equil_time"))
					{
						this.Method.SeparationColumnEquil.EquilTime = (double)arguments["separator_equil_time"].Value;
					}
					if (arguments.Contains("trap_equil_time"))
					{
						this.Method.TrapColumnEquil.EquilTime = (double)arguments["trap_equil_time"].Value;
					}
				}
				else
				{
					this._instrument.ValidateProcedureOffLine(procedure, arguments, advChildArguments);
				}
				if (!this._isValidateError)
				{
					this.ResetErrorConditions();
				}
			}
			finally
			{
				this._instrument.ValidationMessageReported -= this.ValidationErrorHandler;
			}
			this.CheckGenerateEnable();
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00007F84 File Offset: 0x00006184
		private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
		{
			if (Severity.Error.Equals(e.Severity) && e.Subject.Equals("oven_temperature"))
			{
				this._isValidateError = true;
				this.SetErrorCondition(e.Subject, e.Message);
			}
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00007FD8 File Offset: 0x000061D8
		private void SetErrorCondition(string subject, string message)
		{
			if (this.txtOvenTemp.Dispatcher.CheckAccess())
			{
				if (subject == "oven_temperature")
				{
					this.bdrTemperature.BorderBrush = Brushes.Red;
					this.TempToolTip = message;
					this._isValidateTempError = true;
					if (this.btnGenerate.Visibility == Visibility.Visible)
					{
						this.btnGenerate.IsEnabled = false;
					}
				}
				else if (subject == "analysis_time")
				{
					this.bdrAnalysisTime.BorderBrush = Brushes.Red;
					this.AnalysisTimeToolTip = message;
					this._isValidateTimeError = true;
					if (this.btnGenerate.Visibility == Visibility.Visible)
					{
						this.btnGenerate.IsEnabled = false;
					}
				}
				if (!this._isValidateTempError && !this._isTempIllegalChar)
				{
					this.bdrTemperature.BorderBrush = Brushes.Transparent;
					this.txtOvenTemp.ClearValue(FrameworkElement.ToolTipProperty);
					this.SetDefaultToolTip();
				}
				if (!this._isValidateTimeError && !this._isTimeIllegalChar)
				{
					this.bdrAnalysisTime.BorderBrush = Brushes.Transparent;
					this.tcGradientTime.ClearValue(FrameworkElement.ToolTipProperty);
					this.SetDefaultToolTip();
					if (this.btnGenerate.Visibility == Visibility.Visible)
					{
						this.btnGenerate.IsEnabled = true;
					}
				}
				this.SendValidationUpdateEvent();
				return;
			}
			GradientMainUserControl.SetErrorConditionCallback del = new GradientMainUserControl.SetErrorConditionCallback(this.SetErrorCondition);
			base.Dispatcher.BeginInvoke(del, new object[] { subject, message });
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00008138 File Offset: 0x00006338
		private void ResetErrorConditions()
		{
			if (this.btnAdapt == null)
			{
				return;
			}
			if (!this._isValidateTempError)
			{
				this.bdrTemperature.BorderBrush = Brushes.Transparent;
				this.txtOvenTemp.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!this._isValidateTimeError)
			{
				this.bdrAnalysisTime.BorderBrush = Brushes.Transparent;
				this.tcGradientTime.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!this._isValidateTimeError && !this._isValidateTempError && this.btnGenerate.Visibility == Visibility.Visible)
			{
				this.btnGenerate.IsEnabled = true;
			}
			this.SendValidationUpdateEvent();
		}

		// Token: 0x0600011E RID: 286 RVA: 0x000081D0 File Offset: 0x000063D0
		private void SendValidationUpdateEvent()
		{
			GradientMainUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(!this._isValidateTempError && !this._isValidateTimeError && !this.btnAdapt.IsEnabled && !this.btnGenerate.IsVisible && this.SelectedElutionType != null && this.IsColumnsValid());
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00008228 File Offset: 0x00006428
		private void Validation_Error(object sender, ValidationErrorEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (e.Action == ValidationErrorEventAction.Added)
			{
				if (textBox == this.txtOvenTemp)
				{
					this._isTempIllegalChar = true;
				}
				if (textBox == this.tcGradientTime)
				{
					this._isTimeIllegalChar = true;
				}
			}
			else
			{
				if (textBox == this.txtOvenTemp)
				{
					this._isTempIllegalChar = false;
				}
				if (textBox == this.tcGradientTime)
				{
					this._isTimeIllegalChar = false;
				}
			}
			this.SendValidationUpdateEvent();
		}

		// Token: 0x06000120 RID: 288 RVA: 0x0000828C File Offset: 0x0000648C
		private void SetDefaultToolTip()
		{
			try
			{
				if (this._experiment != null && this.Method.SeparationColumnName != null)
				{
					if (GradientMainUserControl._co_83._cp_3 == null)
					{
						GradientMainUserControl._co_83._cp_3 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(GradientMainUserControl)));
					}
					Func<CallSite, object, string> target = GradientMainUserControl._co_83._cp_3.Target;
					CallSite _cpl = GradientMainUserControl._co_83._cp_3;
					if (GradientMainUserControl._co_83._cp_2 == null)
					{
						GradientMainUserControl._co_83._cp_2 = CallSite<Func<CallSite, Type, CultureInfo, string, object, double, string, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Format", null, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, Type, CultureInfo, string, object, double, string, object> target2 = GradientMainUserControl._co_83._cp_2.Target;
					CallSite _cp_2 = GradientMainUserControl._co_83._cp_2;
					Type typeFromHandle = typeof(string);
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					string text = "Values between {0} - {1} {2}C";
					if (GradientMainUserControl._co_83._cp_1 == null)
					{
						GradientMainUserControl._co_83._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ColumnOvenMinTemperature", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target3 = GradientMainUserControl._co_83._cp_1.Target;
					CallSite _cp_3 = GradientMainUserControl._co_83._cp_1;
					if (GradientMainUserControl._co_83._cp_0 == null)
					{
						GradientMainUserControl._co_83._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Settings", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					this.TempToolTip = target(_cpl, target2(_cp_2, typeFromHandle, invariantCulture, text, target3(_cp_3, GradientMainUserControl._co_83._cp_0.Target(GradientMainUserControl._co_83._cp_0, this._instrument.BalticConfiguration)), this._experiment.Separator.MaximumTemperature, "°"));
				}
				else
				{
					if (GradientMainUserControl._co_83._cp_9 == null)
					{
						GradientMainUserControl._co_83._cp_9 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(GradientMainUserControl)));
					}
					Func<CallSite, object, string> target4 = GradientMainUserControl._co_83._cp_9.Target;
					CallSite _cp_4 = GradientMainUserControl._co_83._cp_9;
					if (GradientMainUserControl._co_83._cp_8 == null)
					{
						GradientMainUserControl._co_83._cp_8 = CallSite<Func<CallSite, Type, CultureInfo, string, object, object, string, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Format", null, typeof(GradientMainUserControl), new CSharpArgumentInfo[]
						{
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
							CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
						}));
					}
					Func<CallSite, Type, CultureInfo, string, object, object, string, object> target5 = GradientMainUserControl._co_83._cp_8.Target;
					CallSite _cp_5 = GradientMainUserControl._co_83._cp_8;
					Type typeFromHandle2 = typeof(string);
					CultureInfo invariantCulture2 = CultureInfo.InvariantCulture;
					string text2 = "Values between {0} - {1} {2}C";
					if (GradientMainUserControl._co_83._cp_5 == null)
					{
						GradientMainUserControl._co_83._cp_5 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ColumnOvenMinTemperature", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target6 = GradientMainUserControl._co_83._cp_5.Target;
					CallSite _cp_6 = GradientMainUserControl._co_83._cp_5;
					if (GradientMainUserControl._co_83._cp_4 == null)
					{
						GradientMainUserControl._co_83._cp_4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Settings", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					object obj = target6(_cp_6, GradientMainUserControl._co_83._cp_4.Target(GradientMainUserControl._co_83._cp_4, this._instrument.BalticConfiguration));
					if (GradientMainUserControl._co_83._cp_7 == null)
					{
						GradientMainUserControl._co_83._cp_7 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ColumnOvenMaxTemperature", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target7 = GradientMainUserControl._co_83._cp_7.Target;
					CallSite _cp_7 = GradientMainUserControl._co_83._cp_7;
					if (GradientMainUserControl._co_83._cp_6 == null)
					{
						GradientMainUserControl._co_83._cp_6 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Settings", typeof(GradientMainUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					this.TempToolTip = target4(_cp_4, target5(_cp_5, typeFromHandle2, invariantCulture2, text2, obj, target7(_cp_7, GradientMainUserControl._co_83._cp_6.Target(GradientMainUserControl._co_83._cp_6, this._instrument.BalticConfiguration)), "°"));
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00008680 File Offset: 0x00006880
		private bool IsColumnsValid()
		{
			return (!this.Method.UsesSepColumn || this._columns.Contains(this.Method.SeparationColumnName)) && (!this.Method.UsesTrapColumn || this._columns.Contains(this.Method.TrapColumnName));
		}

		// Token: 0x06000122 RID: 290 RVA: 0x000086DC File Offset: 0x000068DC
		private void TrapColumnItemLoaded(object sender, RoutedEventArgs e)
		{
			GradientMainUserControl.RegisterMouseOver(sender, new EventHandler(this.TrapColumnOnIsMouseOver));
		}

		// Token: 0x06000123 RID: 291 RVA: 0x000086F0 File Offset: 0x000068F0
		private void TrapColumnItemUnloaded(object sender, RoutedEventArgs e)
		{
			GradientMainUserControl.UnregisterMouseOver(sender, new EventHandler(this.TrapColumnOnIsMouseOver));
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00008704 File Offset: 0x00006904
		private void SepColumnItemLoaded(object sender, RoutedEventArgs e)
		{
			GradientMainUserControl.RegisterMouseOver(sender, new EventHandler(this.SepColumnOnIsMouseOver));
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00008718 File Offset: 0x00006918
		private void SepColumnItemUnloaded(object sender, RoutedEventArgs e)
		{
			GradientMainUserControl.UnregisterMouseOver(sender, new EventHandler(this.SepColumnOnIsMouseOver));
		}

		// Token: 0x06000126 RID: 294 RVA: 0x0000872C File Offset: 0x0000692C
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

		// Token: 0x06000127 RID: 295 RVA: 0x00008764 File Offset: 0x00006964
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

		// Token: 0x06000128 RID: 296 RVA: 0x0000879C File Offset: 0x0000699C
		private void SepColumnOnIsMouseOver(object sender, EventArgs e)
		{
			ComboBoxItem item = sender as ComboBoxItem;
			if (item != null && item.IsMouseOver)
			{
				this.SepColumnInfoControl.DataContext = item.DataContext;
			}
		}

		// Token: 0x06000129 RID: 297 RVA: 0x000087CC File Offset: 0x000069CC
		private void TrapColumnOnIsMouseOver(object sender, EventArgs e)
		{
			ComboBoxItem item = sender as ComboBoxItem;
			if (item != null && item.IsMouseOver)
			{
				this.TrapColumnInfoControl.DataContext = item.DataContext;
			}
		}

		// Token: 0x0600012A RID: 298 RVA: 0x000087FC File Offset: 0x000069FC
		private void On_LabelCreated(object sender, LabelCreatedEventArgs e)
		{
			double value;
			if (double.TryParse(e.AxisLabel.LabelContent.ToString(), out value))
			{
				e.AxisLabel.LabelContent = value.ToString(CultureInfo.InvariantCulture);
			}
		}


		// Token: 0x0400008B RID: 139
		private bool _isValidateError;

		// Token: 0x0400008C RID: 140
		private bool _isValidateTempError;

		// Token: 0x0400008D RID: 141
		private bool _isValidateTimeError;

		// Token: 0x0400008E RID: 142
		private bool _isTempIllegalChar;

		// Token: 0x0400008F RID: 143
		private bool _isTimeIllegalChar;

		// Token: 0x04000090 RID: 144
		private readonly bool _isOvenDetected;

		// Token: 0x04000091 RID: 145
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x04000092 RID: 146
		private readonly ColumnSelections _columnSelections;

		// Token: 0x04000093 RID: 147
		private ExperimentInfo _experiment;

		// Token: 0x04000094 RID: 148
		private readonly BalticColumnList _columns;

		// Token: 0x0400009B RID: 155
		private string _tempToolTip;

		// Token: 0x0400009C RID: 156
		private string _analysisTimeToolTip;

		// Token: 0x0400009E RID: 158
		private BindableBalticMethod.ElutionType _selectedElutionType;

		// Token: 0x0400009F RID: 159
		private bool _legacyMode;

		// Token: 0x020000A5 RID: 165
		// (Invoke) Token: 0x060006B5 RID: 1717
		private delegate void SetErrorConditionCallback(string subject, string message);

		// Token: 0x020000A6 RID: 166
		// (Invoke) Token: 0x060006B9 RID: 1721
		public delegate void ValidationUpdate(bool isValid);

		// Token: 0x020000A7 RID: 167
		// (Invoke) Token: 0x060006BD RID: 1725
		public delegate void GetInitialMethodParam();

		// Token: 0x020000A8 RID: 168
		// (Invoke) Token: 0x060006C1 RID: 1729
		public delegate void GenerateMethod(bool isKeepGradient = false, bool isKeepAdvancedSettings = false);

		// Token: 0x020000A9 RID: 169
		// (Invoke) Token: 0x060006C5 RID: 1733
		public delegate void EnableMethodControls(bool isEnable, bool isReset);

		// Token: 0x020000AA RID: 170
		// (Invoke) Token: 0x060006C9 RID: 1737
		public delegate void TrapSelectionWarning(bool isShow, string message = "");
	}
}
