// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x0200000E RID: 14
	public class AdvProcParam : BindableBase
	{
		// Token: 0x14000007 RID: 7
		// (add) Token: 0x0600005B RID: 91 RVA: 0x000033F4 File Offset: 0x000015F4
		// (remove) Token: 0x0600005C RID: 92 RVA: 0x0000342C File Offset: 0x0000162C
		public event AdvProcParam.ModificationUpdate ModificationUpdateEvent;

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x0600005D RID: 93 RVA: 0x00003464 File Offset: 0x00001664
		// (remove) Token: 0x0600005E RID: 94 RVA: 0x0000349C File Offset: 0x0000169C
		public event AdvProcParam.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600005F RID: 95 RVA: 0x000034D1 File Offset: 0x000016D1
		// (set) Token: 0x06000060 RID: 96 RVA: 0x000034D9 File Offset: 0x000016D9
		public ProcedureArgument Argument
		{
			get
			{
				return this._argument;
			}
			set
			{
				base.SetField<ProcedureArgument>(ref this._argument, value, "Argument");
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000061 RID: 97 RVA: 0x000034EE File Offset: 0x000016EE
		// (set) Token: 0x06000062 RID: 98 RVA: 0x000034F6 File Offset: 0x000016F6
		public string Unit { get; private set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000063 RID: 99 RVA: 0x000034FF File Offset: 0x000016FF
		public Type Type { get; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000064 RID: 100 RVA: 0x00003507 File Offset: 0x00001707
		// (set) Token: 0x06000065 RID: 101 RVA: 0x0000350F File Offset: 0x0000170F
		public bool IsService { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000066 RID: 102 RVA: 0x00003518 File Offset: 0x00001718
		// (set) Token: 0x06000067 RID: 103 RVA: 0x00003520 File Offset: 0x00001720
		public bool IsAppService { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000068 RID: 104 RVA: 0x00003529 File Offset: 0x00001729
		public bool IsVisible
		{
			get
			{
				return (this.IsService && this.IsAppService) || !this.IsService;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000069 RID: 105 RVA: 0x00003546 File Offset: 0x00001746
		public ICommand ChildSettingsCmd { get; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600006A RID: 106 RVA: 0x0000354E File Offset: 0x0000174E
		// (set) Token: 0x0600006B RID: 107 RVA: 0x00003556 File Offset: 0x00001756
		public string ErrorMessage
		{
			get
			{
				return this._errorMessage;
			}
			set
			{
				base.SetField<string>(ref this._errorMessage, value, "ErrorMessage");
				this.OnPropertyChanged("HasError");
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600006C RID: 108 RVA: 0x00003576 File Offset: 0x00001776
		// (set) Token: 0x0600006D RID: 109 RVA: 0x0000357E File Offset: 0x0000177E
		public bool HasChildren
		{
			get
			{
				return this._hasChildren;
			}
			set
			{
				base.SetField<bool>(ref this._hasChildren, value, "HasChildren");
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600006E RID: 110 RVA: 0x00003593 File Offset: 0x00001793
		// (set) Token: 0x0600006F RID: 111 RVA: 0x0000359B File Offset: 0x0000179B
		public bool IsChildError
		{
			get
			{
				return this._isChildError;
			}
			set
			{
				base.SetField<bool>(ref this._isChildError, value, "IsChildError");
				this.OnPropertyChanged("HasError");
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000070 RID: 112 RVA: 0x000035BB File Offset: 0x000017BB
		public bool HasError
		{
			get
			{
				return this._errorMessage != null || this.IsChildError;
			}
		}

		// Token: 0x06000071 RID: 113 RVA: 0x000035CD File Offset: 0x000017CD
		public AdvProcParam(ProcedureParameter parameter, BindableBalticMethod method, ExperimentInfo experiment, BalticInstrumentFacade instrument, List<AdvChildProcParam> childParams, bool isAppService)
			: this(parameter, parameter.CreateArgument(), method, experiment, instrument, childParams, isAppService)
		{
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000035E4 File Offset: 0x000017E4
		public AdvProcParam(ProcedureParameter parameter, ProcedureArgument argument, BindableBalticMethod method, ExperimentInfo experiment, BalticInstrumentFacade instrument, List<AdvChildProcParam> childParams, bool isAppService)
		{
			this._argument = argument;
			this._method = method;
			this._experiment = experiment;
			this._instrument = instrument;
			this._childParams = childParams;
			this.Unit = parameter.Unit;
			this.Type = parameter.Type;
			this.IsService = parameter.IsService;
			this.IsAppService = isAppService;
			this.HasChildren = childParams.Any((AdvChildProcParam x) => x.Header == this._argument.Name && x.IsVisible);
			this.ChildSettingsCmd = new RelayCommand(new Action<object>(this.ChildSettings), new Predicate<object>(this.CanChildSettings));
			this.IsChildError = false;
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000073 RID: 115 RVA: 0x0000368C File Offset: 0x0000188C
		public string Name
		{
			get
			{
				return this.Argument.Name;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000074 RID: 116 RVA: 0x00003699 File Offset: 0x00001899
		// (set) Token: 0x06000075 RID: 117 RVA: 0x000036A6 File Offset: 0x000018A6
		public object Value
		{
			get
			{
				return this.Argument.Value;
			}
			set
			{
				if (value != null)
				{
					value = Convert.ChangeType(value, this.Type);
				}
				this.Argument.Value = value;
				this.OnPropertyChanged("Value");
			}
		}

		// Token: 0x06000076 RID: 118 RVA: 0x000036D0 File Offset: 0x000018D0
		public bool CanChildSettings(object obj)
		{
			return true;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x000036D4 File Offset: 0x000018D4
		public void ChildSettings(object obj)
		{
			Button btnParent = obj as Button;
			AdvancedChildParameterWindow advancedChildParameterWindow = new AdvancedChildParameterWindow(this._argument.Name, this._method, this._instrument, this._experiment);
			advancedChildParameterWindow.ValidationUpdateEvent += this.ChildWindow_ValidationUpdateEvent;
			advancedChildParameterWindow.ModificationUpdateEvent += this.ChildWindow_ModificationUpdateEvent;
			Point pt = btnParent.PointToScreen(new Point(30.0, 0.0));
			List<AdvChildProcParam> children = this._childParams.FindAll((AdvChildProcParam x) => x.Header == this._argument.Name);
			advancedChildParameterWindow.Left = pt.X;
			advancedChildParameterWindow.Top = pt.Y - (double)(((children != null) ? children.Count : 0) * 30);
			advancedChildParameterWindow.ShowDialog(HelperExtensions.GetActiveWindow());
		}

		// Token: 0x06000078 RID: 120 RVA: 0x0000379A File Offset: 0x0000199A
		private void ChildWindow_ModificationUpdateEvent(bool isModified)
		{
			AdvProcParam.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent == null)
			{
				return;
			}
			modificationUpdateEvent(isModified);
		}

		// Token: 0x06000079 RID: 121 RVA: 0x000037AD File Offset: 0x000019AD
		private void ChildWindow_ValidationUpdateEvent(bool isValid)
		{
			AdvProcParam.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent != null)
			{
				validationUpdateEvent(isValid);
			}
			this.IsChildError = !isValid;
		}

		// Token: 0x0400002B RID: 43
		private ProcedureArgument _argument;

		// Token: 0x0400002C RID: 44
		private readonly BindableBalticMethod _method;

		// Token: 0x0400002D RID: 45
		private readonly ExperimentInfo _experiment;

		// Token: 0x0400002E RID: 46
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x0400002F RID: 47
		private readonly List<AdvChildProcParam> _childParams;

		// Token: 0x04000037 RID: 55
		private string _errorMessage;

		// Token: 0x04000038 RID: 56
		private bool _hasChildren;

		// Token: 0x04000039 RID: 57
		private bool _isChildError;

		// Token: 0x02000094 RID: 148
		// (Invoke) Token: 0x06000682 RID: 1666
		public delegate void ModificationUpdate(bool isModified);

		// Token: 0x02000095 RID: 149
		// (Invoke) Token: 0x06000686 RID: 1670
		public delegate void ValidationUpdate(bool isValid);
	}
}
