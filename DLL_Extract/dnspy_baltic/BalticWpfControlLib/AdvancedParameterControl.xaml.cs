// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x0200000D RID: 13
	public partial class AdvancedParameterControl : UserControl
	{
		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000045 RID: 69 RVA: 0x00002CE8 File Offset: 0x00000EE8
		// (remove) Token: 0x06000046 RID: 70 RVA: 0x00002D20 File Offset: 0x00000F20
		public event AdvancedParameterControl.ModificationUpdate ModificationUpdateEvent;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000047 RID: 71 RVA: 0x00002D58 File Offset: 0x00000F58
		// (remove) Token: 0x06000048 RID: 72 RVA: 0x00002D90 File Offset: 0x00000F90
		public event AdvancedParameterControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x06000049 RID: 73 RVA: 0x00002DC8 File Offset: 0x00000FC8
		// (remove) Token: 0x0600004A RID: 74 RVA: 0x00002E00 File Offset: 0x00001000
		public event EventHandler ArgumentValueUpdated;

		// Token: 0x0600004B RID: 75 RVA: 0x00002E38 File Offset: 0x00001038
		public AdvancedParameterControl()
		{
			this.InitializeComponent();
			base.DataContext = this;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00002E84 File Offset: 0x00001084
		public AdvancedParameterControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment, bool isAppServiceMode)
		{
			this.InitializeComponent();
			this._method = method;
			this._instrument = instrument;
			this._experiment = experiment;
			this._isAppServiceMode = isAppServiceMode;
			base.DataContext = this;
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600004D RID: 77 RVA: 0x00002EED File Offset: 0x000010ED
		[Description("Gets if there's an error registered for any parameter.")]
		public bool HasError
		{
			get
			{
				return (bool)base.GetValue(AdvancedParameterControl.HasErrorProperty);
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600004E RID: 78 RVA: 0x00002EFF File Offset: 0x000010FF
		public ObservableCollection<AdvProcParam> Parameters { get; } = new ObservableCollection<AdvProcParam>();

		// Token: 0x0600004F RID: 79 RVA: 0x00002F08 File Offset: 0x00001108
		public void SetParameters(IEnumerable<ProcedureParameter> procedureParameters, ProcedureArguments procArgs, IEnumerable<ChildProcedureParameter> childProcedureParameters, ChildProcedureArguments childProcArgs, ProcedureArguments valuePresets, ChildProcedureArguments childValuePresets)
		{
			this.Parameters.Clear();
			this._childParameters.Clear();
			this._valuePresets.Clear();
			this._childValuePresets.Clear();
			foreach (ProcedureArgument item in valuePresets)
			{
				this._valuePresets.Add(new ProcedureArgument(item));
			}
			foreach (ChildProcedureArgument item2 in childValuePresets)
			{
				this._childValuePresets.Add(new ChildProcedureArgument(item2));
			}
			using (IEnumerator<ChildProcedureParameter> enumerator3 = childProcedureParameters.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					ChildProcedureParameter param = enumerator3.Current;
					AdvChildProcParam cpp = null;
					if (childValuePresets.FirstOrDefault((ChildProcedureArgument x) => x.Header == param.Header && x.ProcArg.Name == param.Name) != null)
					{
						ChildProcedureArgument arg = childProcArgs.Find((ChildProcedureArgument x) => x.Header == param.Header && x.ProcArg.Name == param.Name);
						if (arg != null)
						{
							cpp = new AdvChildProcParam(param, arg, this._isAppServiceMode);
						}
					}
					if (cpp == null)
					{
						cpp = new AdvChildProcParam(param, this._isAppServiceMode);
					}
					this._childParameters.Add(cpp);
				}
			}
			foreach (ProcedureParameter param2 in procedureParameters)
			{
				AdvProcParam pp = (valuePresets.Contains(param2.Name) ? new AdvProcParam(param2, procArgs[param2.Name], this._method, this._experiment, this._instrument, this._childParameters, this._isAppServiceMode) : new AdvProcParam(param2, this._method, this._experiment, this._instrument, this._childParameters, this._isAppServiceMode));
				pp.ValidationUpdateEvent += this.Pp_ValidationUpdateEvent;
				pp.ModificationUpdateEvent += this.Pp_ModificationUpdateEvent;
				this.Parameters.Add(pp);
			}
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00003160 File Offset: 0x00001360
		private void Pp_ModificationUpdateEvent(bool isModified)
		{
			AdvancedParameterControl.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent == null)
			{
				return;
			}
			modificationUpdateEvent(isModified);
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00003173 File Offset: 0x00001373
		private void Pp_ValidationUpdateEvent(bool isValid)
		{
			AdvancedParameterControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(isValid);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00003188 File Offset: 0x00001388
		public void ResetParameterValues()
		{
			using (IEnumerator<ProcedureArgument> enumerator = this._valuePresets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ProcedureArgument param = enumerator.Current;
					AdvProcParam p = this.Parameters.SingleOrDefault((AdvProcParam x) => x.Name == param.Name);
					if (p != null)
					{
						p.Value = param.Value;
					}
				}
			}
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00003208 File Offset: 0x00001408
		public void ClearErrors()
		{
			foreach (AdvProcParam advProcParam in this.Parameters)
			{
				advProcParam.ErrorMessage = null;
				advProcParam.IsChildError = false;
			}
			base.SetValue(AdvancedParameterControl.HasErrorProperty, false);
		}

		// Token: 0x06000054 RID: 84 RVA: 0x0000326C File Offset: 0x0000146C
		public void SetError(string header, string parameter, string error)
		{
			AdvProcParam param = this.Parameters.FirstOrDefault((AdvProcParam pp) => pp.Name.Equals(parameter));
			if (param != null)
			{
				param.ErrorMessage = error;
				param.IsChildError = true;
				base.SetValue(AdvancedParameterControl.HasErrorProperty, true);
				return;
			}
			param = this.Parameters.FirstOrDefault((AdvProcParam pp) => pp.Name.Equals(header));
			if (param != null)
			{
				param.ErrorMessage = error;
				param.IsChildError = true;
				base.SetValue(AdvancedParameterControl.HasErrorProperty, true);
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00003304 File Offset: 0x00001504
		public ProcedureArguments CreateArguments()
		{
			ProcedureArguments arguments = new ProcedureArguments();
			foreach (AdvProcParam param in this.Parameters)
			{
				arguments.Add(param.Argument);
			}
			return arguments;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00003360 File Offset: 0x00001560
		private void ParameterItems_SourceUpdated(object sender, DataTransferEventArgs e)
		{
			EventHandler argumentValueUpdated = this.ArgumentValueUpdated;
			if (argumentValueUpdated == null)
			{
				return;
			}
			argumentValueUpdated(this, EventArgs.Empty);
		}

		// Token: 0x0400001D RID: 29
		public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register("HasError", typeof(bool), typeof(AdvancedParameterControl), new PropertyMetadata(false));

		// Token: 0x0400001E RID: 30
		private readonly ProcedureArguments _valuePresets = new ProcedureArguments();

		// Token: 0x0400001F RID: 31
		private readonly List<AdvChildProcParam> _childParameters = new List<AdvChildProcParam>();

		// Token: 0x04000020 RID: 32
		private readonly ChildProcedureArguments _childValuePresets = new ChildProcedureArguments();

		// Token: 0x04000021 RID: 33
		private readonly BindableBalticMethod _method;

		// Token: 0x04000022 RID: 34
		private readonly ExperimentInfo _experiment;

		// Token: 0x04000023 RID: 35
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x04000024 RID: 36
		private readonly bool _isAppServiceMode;

		// Token: 0x0200008F RID: 143
		// (Invoke) Token: 0x06000672 RID: 1650
		public delegate void ModificationUpdate(bool isModified);

		// Token: 0x02000090 RID: 144
		// (Invoke) Token: 0x06000676 RID: 1654
		public delegate void ValidationUpdate(bool isValid);
	}
}
