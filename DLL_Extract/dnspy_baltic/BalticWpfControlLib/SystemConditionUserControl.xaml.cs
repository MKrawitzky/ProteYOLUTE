// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib
{
	// Token: 0x02000040 RID: 64
	public partial class SystemConditionUserControl : UserControl
	{
		// Token: 0x060003AC RID: 940 RVA: 0x00017AA4 File Offset: 0x00015CA4
		public SystemConditionUserControl()
		{
			this.InitializeComponent();
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x060003AD RID: 941 RVA: 0x00017AB2 File Offset: 0x00015CB2
		// (set) Token: 0x060003AE RID: 942 RVA: 0x00017ABA File Offset: 0x00015CBA
		public SystemCondition SystemCondition
		{
			get
			{
				return this._condition;
			}
			set
			{
				this._condition = value;
				this.Update();
			}
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00017ACC File Offset: 0x00015CCC
		private void Update()
		{
			this.Raised.Text = this._condition.Raised.ToShortDateString();
			this.Subject.Text = this._condition.Subject;
			this.Description.Text = this._condition.Description;
			this.Severity.Text = this._condition.Severity.ToString();
			this.Confirm.Visibility = (this._condition.ManualDismiss ? Visibility.Visible : Visibility.Hidden);
		}

		// Token: 0x0400023F RID: 575
		private SystemCondition _condition;
	}
}
