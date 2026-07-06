// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Threading;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using Bruker.Lc;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib
{
	// Token: 0x02000020 RID: 32
	public partial class GradientTableUserControl : UserControl, INotifyPropertyChanged
	{
		// Token: 0x14000017 RID: 23
		// (add) Token: 0x06000137 RID: 311 RVA: 0x00008F34 File Offset: 0x00007134
		// (remove) Token: 0x06000138 RID: 312 RVA: 0x00008F6C File Offset: 0x0000716C
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x06000139 RID: 313 RVA: 0x00008FA1 File Offset: 0x000071A1
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x0600013A RID: 314 RVA: 0x00008FBC File Offset: 0x000071BC
		// (remove) Token: 0x0600013B RID: 315 RVA: 0x00008FF4 File Offset: 0x000071F4
		public event GradientTableUserControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x0600013C RID: 316 RVA: 0x0000902C File Offset: 0x0000722C
		// (remove) Token: 0x0600013D RID: 317 RVA: 0x00009064 File Offset: 0x00007264
		public event GradientTableUserControl.GradientMainUpdate GradientMainUpdateEvent;

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600013E RID: 318 RVA: 0x00009099 File Offset: 0x00007299
		// (set) Token: 0x0600013F RID: 319 RVA: 0x000090A1 File Offset: 0x000072A1
		public ExperimentInfo Experiment
		{
			get
			{
				return this._experiment;
			}
			set
			{
				this._experiment = value;
				this._hasMinMaxFlow = false;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000140 RID: 320 RVA: 0x000090B4 File Offset: 0x000072B4
		public double MaxFlow
		{
			get
			{
				if (this._experiment != null)
				{
					return 9.0;
				}
				ExperimentInfo experiment = this._experiment;
				double? num;
				if (experiment == null)
				{
					num = null;
				}
				else
				{
					IChromatographyColumnType separator = experiment.Separator;
					num = ((separator != null) ? new double?(separator.MaximumFlow) : null);
				}
				double? num2 = num;
				return num2.GetValueOrDefault(9.0);
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000141 RID: 321 RVA: 0x00009117 File Offset: 0x00007317
		public BindableBalticMethod Method { get; }

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000142 RID: 322 RVA: 0x0000911F File Offset: 0x0000731F
		// (set) Token: 0x06000143 RID: 323 RVA: 0x00009127 File Offset: 0x00007327
		public bool IsTwoColumn
		{
			get
			{
				return this._isTwoColumn;
			}
			set
			{
				this._isTwoColumn = value;
				this.NotifyPropertyChanged("IsTwoColumn");
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000144 RID: 324 RVA: 0x0000913B File Offset: 0x0000733B
		// (set) Token: 0x06000145 RID: 325 RVA: 0x00009143 File Offset: 0x00007343
		public bool IsOneOrTwoColumn
		{
			get
			{
				return this._isOneOrTwoColumn;
			}
			set
			{
				this._isOneOrTwoColumn = value;
				this.NotifyPropertyChanged("IsOneOrTwoColumn");
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000146 RID: 326 RVA: 0x00009157 File Offset: 0x00007357
		// (set) Token: 0x06000147 RID: 327 RVA: 0x00009164 File Offset: 0x00007364
		public BalticGradientList GradientTable
		{
			get
			{
				return this.Method.GradientTable;
			}
			set
			{
				this.Method.GradientTable = value;
				this.NotifyPropertyChanged("GradientTable");
			}
		}

		// Token: 0x06000148 RID: 328 RVA: 0x00009180 File Offset: 0x00007380
		public GradientTableUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
		{
			this.Method = method;
			this._instrument = instrument;
			this._experiment = experiment;
			this.InitializeComponent();
			this.bdrTable.DataContext = this;
			base.DataContext = this;
			this.LoadMethod();
			this.IsTwoColumn = this.Method.UsesSepColumn && this.Method.UsesTrapColumn;
			this.IsOneOrTwoColumn = this.Method.UsesSepColumn || this.Method.UsesTrapColumn;
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00009214 File Offset: 0x00007414
		public void UpdateGradientTableControls()
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				this.IsTwoColumn = this.Method.UsesSepColumn && this.Method.UsesTrapColumn;
				this.IsOneOrTwoColumn = this.Method.UsesSepColumn || this.Method.UsesTrapColumn;
			}), Array.Empty<object>());
		}

		// Token: 0x0600014A RID: 330 RVA: 0x00009233 File Offset: 0x00007433
		private void GradientColumnHeader_Click(object sender, RoutedEventArgs e)
		{
			DataGridColumnHeader dataGridColumnHeader = (DataGridColumnHeader)sender;
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0000923C File Offset: 0x0000743C
		private void GradientAddEvent_Click(object sender, RoutedEventArgs e)
		{
			if (this.dgGradient.SelectedCells.Count > 0)
			{
				int rowIndex = DataGridHelper.GetRowIndex(DataGridHelper.GetCell(this.dgGradient.SelectedCells[0]));
				BalticGradientItem item = new BalticGradientItem(this.Method.GradientTable[rowIndex])
				{
					Duration = 0.0
				};
				this.Method.GradientTable.Insert(rowIndex + 1, item);
				GradientTableUserControl.GradientMainUpdate gradientMainUpdateEvent = this.GradientMainUpdateEvent;
				if (gradientMainUpdateEvent != null)
				{
					gradientMainUpdateEvent(WPFConstants.UpdateType.AddRemove);
				}
				this.CheckGradientTableEnable();
				this.ValidateParameters();
				this.dgGradient.ScrollIntoView(item);
			}
		}

		// Token: 0x0600014C RID: 332 RVA: 0x000092E0 File Offset: 0x000074E0
		private void GradientRemoveEvent_Click(object sender, RoutedEventArgs e)
		{
			if (this.dgGradient.SelectedCells.Count > 0 && this.dgGradient.Items.Count > 1)
			{
				DataGridCell cell = DataGridHelper.GetCell(this.dgGradient.SelectedCells[0]);
				this.Method.GradientTable.RemoveAt(DataGridHelper.GetRowIndex(cell));
				if (this.Method.GradientTable[this.Method.GradientTable.Count - 1].Duration > 0.0)
				{
					this.Method.GradientTable[this.Method.GradientTable.Count - 1].Duration = 0.0;
				}
				if (this.Method.GradientTable[0].Time > 0.0)
				{
					this.Method.GradientTable[0].Time = 0.0;
					if (this.Method.GradientTable.Count > 1)
					{
						this.Method.GradientTable[0].Duration = this.Method.GradientTable[1].Time;
					}
					else
					{
						this.Method.GradientTable[0].Duration = 0.0;
					}
				}
				GradientTableUserControl.GradientMainUpdate gradientMainUpdateEvent = this.GradientMainUpdateEvent;
				if (gradientMainUpdateEvent != null)
				{
					gradientMainUpdateEvent(WPFConstants.UpdateType.AddRemove);
				}
				this.CheckGradientTableEnable();
				this.ValidateParameters();
			}
		}

		// Token: 0x0600014D RID: 333 RVA: 0x00009468 File Offset: 0x00007668
		private void CheckGradientTableEnable()
		{
			for (int i = 0; i < this.Method.GradientTable.Count; i++)
			{
				this.Method.GradientTable[i].IsTimeEditable = i > 0;
				this.Method.GradientTable[i].IsLastRow = i == this.Method.GradientTable.Count - 1;
			}
		}

		// Token: 0x0600014E RID: 334 RVA: 0x000094D5 File Offset: 0x000076D5
		public void LoadMethod()
		{
			this.dgGradient.ItemsSource = this.Method.GradientTable;
			this.CheckGradientTableEnable();
		}

		// Token: 0x0600014F RID: 335 RVA: 0x000094F3 File Offset: 0x000076F3
		private void FlowTextBox_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			this.ValidateParameters();
			GradientTableUserControl.GradientMainUpdate gradientMainUpdateEvent = this.GradientMainUpdateEvent;
			if (gradientMainUpdateEvent == null)
			{
				return;
			}
			gradientMainUpdateEvent(WPFConstants.UpdateType.FlowRate);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0000950C File Offset: 0x0000770C
		public void UpdateGradientTime(double gradientTime, bool reset)
		{
			if (reset)
			{
				this.dgGradient.Items.Refresh();
				return;
			}
			for (int i = 0; i < this.Method.GradientTable.Count; i++)
			{
				if (this.Method.GradientTable[i].SmartName == "GradInitial")
				{
					this.Method.GradientTable[i].Duration = gradientTime;
					for (int j = i + 1; j < this.Method.GradientTable.Count; j++)
					{
						this.Method.GradientTable[j].Time = this.Method.GradientTable[j - 1].Time + this.Method.GradientTable[j - 1].Duration;
					}
					return;
				}
			}
		}

		// Token: 0x06000151 RID: 337 RVA: 0x000095EC File Offset: 0x000077EC
		private void CompositionTextBox_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DoubleTextBox doubleTextBox = d as DoubleTextBox;
			if (string.Format(CultureInfo.InvariantCulture, "{0:0.0}", (double)e.NewValue) == "0.0")
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
				{
					if (doubleTextBox != null)
					{
						doubleTextBox.SelectionStart = 1;
					}
				}));
			}
			this.ValidateParameters();
			GradientTableUserControl.GradientMainUpdate gradientMainUpdateEvent = this.GradientMainUpdateEvent;
			if (gradientMainUpdateEvent == null)
			{
				return;
			}
			gradientMainUpdateEvent(WPFConstants.UpdateType.Composition);
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00009667 File Offset: 0x00007867
		private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			this.ValidateParameters();
			GradientTableUserControl.GradientMainUpdate gradientMainUpdateEvent = this.GradientMainUpdateEvent;
			if (gradientMainUpdateEvent == null)
			{
				return;
			}
			gradientMainUpdateEvent(WPFConstants.UpdateType.Time);
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00009680 File Offset: 0x00007880
		private void TimeTextBox_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DoubleTextBox doubleTextBox = d as DoubleTextBox;
			if (string.Format(CultureInfo.InvariantCulture, "{0:0.00}", (double)e.NewValue) == "0.00")
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
				{
					if (doubleTextBox != null)
					{
						doubleTextBox.SelectionStart = 1;
					}
				}));
			}
			this.ValidateParameters();
			GradientTableUserControl.GradientMainUpdate gradientMainUpdateEvent = this.GradientMainUpdateEvent;
			if (gradientMainUpdateEvent != null)
			{
				gradientMainUpdateEvent(WPFConstants.UpdateType.Time);
			}
			try
			{
				for (int rowIndex = 0; rowIndex < this.Method.GradientTable.Count; rowIndex++)
				{
					if (rowIndex > 0)
					{
						if (this.Method.GradientTable[rowIndex].Time >= this.Method.GradientTable[rowIndex - 1].Time + 0.01)
						{
							this.Method.GradientTable[rowIndex - 1].Duration = this.Method.GradientTable[rowIndex].Time - this.Method.GradientTable[rowIndex - 1].Time;
						}
						else
						{
							this.Method.GradientTable[rowIndex - 1].Duration = this.Method.GradientTable[rowIndex].Time - this.Method.GradientTable[rowIndex].Time;
						}
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000154 RID: 340 RVA: 0x00009804 File Offset: 0x00007A04
		private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
		{
			if (Severity.Error.Equals(e.Severity))
			{
				this.SetErrorCondition(e, false);
				if (!this._hasMinMaxFlow && e.Message.ToLower().Contains("flow must be between"))
				{
					try
					{
						string[] array = Regex.Split(e.Message, "([-+]?[0-9]*\\.?[0-9]+)");
						int idx = 0;
						string[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							double retNum;
							if (double.TryParse(Convert.ToString(array2[i]), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum))
							{
								if (idx == 0)
								{
									this._minFlow = retNum;
								}
								else if (idx == 1)
								{
									this._maxFlow = retNum;
								}
								this._hasMinMaxFlow = true;
								idx++;
							}
						}
					}
					catch (Exception)
					{
					}
				}
			}
		}

		// Token: 0x06000155 RID: 341 RVA: 0x000098CC File Offset: 0x00007ACC
		private void ValidateParameters()
		{
			BalticMethod baltic = this.Method.ToBalticMethod(null);
			ProcedureInfo procedure = this._instrument.GetElutionProcedure(this.Method.ElutionName);
			ProcedureArguments arguments = procedure.CreateArguments();
			ProcedureArguments advArguments = procedure.CreateAdvancedArguments();
			ChildProcedureArguments advChildArguments = procedure.CreateAdvancedChildArguments();
			ElutionMethodUtil.PopulateMethodArguments(this._instrument.IsColumnOvenConnected, baltic, arguments, advArguments, advChildArguments, null, null);
			foreach (ProcedureArgument item in advArguments)
			{
				arguments.Add(new ProcedureArgument(item));
			}
			this._instrument.ValidationMessageReported += this.ValidationErrorHandler;
			try
			{
				this.ResetErrorConditions();
				if (this._experiment != null)
				{
					this._instrument.ValidateMethodProcedureOffLine(this._experiment, procedure, arguments, advChildArguments);
				}
				else
				{
					this._instrument.ValidateProcedureOffLine(procedure, arguments, advChildArguments);
				}
			}
			finally
			{
				this._instrument.ValidationMessageReported -= this.ValidationErrorHandler;
			}
		}

		// Token: 0x06000156 RID: 342 RVA: 0x000099E4 File Offset: 0x00007BE4
		private void SetErrorCondition(ProcedureReportEventArgs e, bool isTimeError)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				if (e.IsGradientSubject)
				{
					int rowIdx = Convert.ToInt32(e.Subject.Substring(8));
					if (rowIdx >= 0 && rowIdx < this.Method.GradientTable.Count)
					{
						if (isTimeError)
						{
							this.Method.GradientTable[rowIdx].IsInTimeOrder = false;
						}
						else
						{
							this.Method.GradientTable[rowIdx].IsParamValid = false;
							this.Method.GradientTable[rowIdx].ParamToolTip = e.Message;
						}
						if (isTimeError && !this.Method.GradientTable[rowIdx].IsParamValid)
						{
							this.Method.GradientTable[rowIdx].ErrorToolTip = e.Message + "\n" + this.Method.GradientTable[rowIdx].ParamToolTip;
						}
						else
						{
							this.Method.GradientTable[rowIdx].ErrorToolTip = e.Message;
						}
					}
				}
				bool isValid = true;
				foreach (BalticGradientItem t in this.Method.GradientTable)
				{
					if (!t.IsParamValid || !t.IsInTimeOrder)
					{
						isValid = false;
						break;
					}
				}
				if (this.Method.GradientTable.Count > 0)
				{
					foreach (BalticGradientItem balticGradientItem in this.Method.GradientTable)
					{
						balticGradientItem.IsLastRowValid = this.Method.GradientTable[this.Method.GradientTable.Count - 1].IsValidState;
					}
				}
				GradientTableUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
				if (validationUpdateEvent == null)
				{
					return;
				}
				validationUpdateEvent(isValid && this._nIllegalCharEntries == 0, e.Header, e.Subject, e.Message);
			}), Array.Empty<object>());
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00009A29 File Offset: 0x00007C29
		private void ResetErrorConditions()
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				bool isValid = true;
				foreach (BalticGradientItem t in this.Method.GradientTable)
				{
					t.IsParamValid = true;
					t.ParamToolTip = null;
					if (t.IsInTimeOrder)
					{
						t.ErrorToolTip = null;
					}
					else
					{
						isValid = false;
					}
				}
				if (this.Method.GradientTable.Count > 0)
				{
					foreach (BalticGradientItem balticGradientItem in this.Method.GradientTable)
					{
						balticGradientItem.IsLastRowValid = this.Method.GradientTable[this.Method.GradientTable.Count - 1].IsValidState;
					}
				}
				GradientTableUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
				if (validationUpdateEvent == null)
				{
					return;
				}
				validationUpdateEvent(isValid && this._nIllegalCharEntries == 0, "", "", "");
			}), Array.Empty<object>());
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00009A48 File Offset: 0x00007C48
		private void Validation_Error(object sender, ValidationErrorEventArgs e)
		{
			if (e.Action == ValidationErrorEventAction.Added)
			{
				this._nIllegalCharEntries++;
			}
			else
			{
				this._nIllegalCharEntries--;
			}
			GradientTableUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(this._nIllegalCharEntries == 0, "", "", "");
		}


		// Token: 0x040000BB RID: 187
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x040000BC RID: 188
		private int _nIllegalCharEntries;

		// Token: 0x040000BD RID: 189
		private bool _hasMinMaxFlow;

		// Token: 0x040000BE RID: 190
		private double _minFlow;

		// Token: 0x040000BF RID: 191
		private double _maxFlow;

		// Token: 0x040000C0 RID: 192
		private bool _isTwoColumn;

		// Token: 0x040000C1 RID: 193
		private bool _isOneOrTwoColumn;

		// Token: 0x040000C2 RID: 194
		public readonly int DesignWidth = 249;

		// Token: 0x040000C5 RID: 197
		private ExperimentInfo _experiment;

		// Token: 0x020000B3 RID: 179
		// (Invoke) Token: 0x060006D0 RID: 1744
		public delegate void ValidationUpdate(bool isValid, string header = "", string subject = "", string message = "");

		// Token: 0x020000B4 RID: 180
		// (Invoke) Token: 0x060006D4 RID: 1748
		public delegate void GradientMainUpdate(WPFConstants.UpdateType updateType);
	}
}
