// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x02000008 RID: 8
	public class AdvChildProcParam : BindableBase
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002750 File Offset: 0x00000950
		// (set) Token: 0x06000020 RID: 32 RVA: 0x00002758 File Offset: 0x00000958
		public ChildProcedureArgument ChildArgument
		{
			get
			{
				return this._childArgument;
			}
			set
			{
				this._childArgument = value;
				this.OnPropertyChanged("ChildArgument");
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000021 RID: 33 RVA: 0x0000276C File Offset: 0x0000096C
		public string Header { get; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000022 RID: 34 RVA: 0x00002774 File Offset: 0x00000974
		// (set) Token: 0x06000023 RID: 35 RVA: 0x0000277C File Offset: 0x0000097C
		public string Unit { get; private set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000024 RID: 36 RVA: 0x00002785 File Offset: 0x00000985
		public Type Type { get; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000025 RID: 37 RVA: 0x0000278D File Offset: 0x0000098D
		// (set) Token: 0x06000026 RID: 38 RVA: 0x00002795 File Offset: 0x00000995
		public bool IsService { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000027 RID: 39 RVA: 0x0000279E File Offset: 0x0000099E
		// (set) Token: 0x06000028 RID: 40 RVA: 0x000027A6 File Offset: 0x000009A6
		public bool IsAppService { get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000029 RID: 41 RVA: 0x000027AF File Offset: 0x000009AF
		public bool IsVisible
		{
			get
			{
				return (this.IsService && this.IsAppService) || !this.IsService;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600002A RID: 42 RVA: 0x000027CC File Offset: 0x000009CC
		// (set) Token: 0x0600002B RID: 43 RVA: 0x000027D4 File Offset: 0x000009D4
		public string ErrorMessage
		{
			get
			{
				return this._errorMessage;
			}
			set
			{
				if (base.SetProperty<string>(ref this._errorMessage, value, "ErrorMessage"))
				{
					this.OnPropertyChanged("HasError");
				}
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600002C RID: 44 RVA: 0x000027F5 File Offset: 0x000009F5
		public bool HasError
		{
			get
			{
				return this._errorMessage != null;
			}
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002800 File Offset: 0x00000A00
		public AdvChildProcParam(ChildProcedureParameter parameter, bool isAppService)
		{
			this.Header = parameter.Header;
			this._childArgument.ProcArg = parameter.CreateArgument();
			this.Unit = parameter.Unit;
			this.Type = parameter.Type;
			this.IsService = parameter.IsService;
			this.IsAppService = isAppService;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x0000285C File Offset: 0x00000A5C
		public AdvChildProcParam(ChildProcedureParameter parameter, ChildProcedureArgument argument, bool isAppService)
		{
			this.Header = parameter.Header;
			this._childArgument = new ChildProcedureArgument(parameter.Header, argument.ProcArg);
			this.Unit = parameter.Unit;
			this.Type = parameter.Type;
			this.IsService = parameter.IsService;
			this.IsAppService = isAppService;
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600002F RID: 47 RVA: 0x000028BD File Offset: 0x00000ABD
		public string Name
		{
			get
			{
				return this.ChildArgument.ProcArg.Name;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000030 RID: 48 RVA: 0x000028CF File Offset: 0x00000ACF
		// (set) Token: 0x06000031 RID: 49 RVA: 0x000028E1 File Offset: 0x00000AE1
		public object Value
		{
			get
			{
				return this.ChildArgument.ProcArg.Value;
			}
			set
			{
				if (value != null)
				{
					value = Convert.ChangeType(value, this.Type);
				}
				this.ChildArgument.ProcArg.Value = value;
				this.OnPropertyChanged("Value");
			}
		}

		// Token: 0x04000010 RID: 16
		private ChildProcedureArgument _childArgument;

		// Token: 0x04000011 RID: 17
		private string _errorMessage;
	}
}
