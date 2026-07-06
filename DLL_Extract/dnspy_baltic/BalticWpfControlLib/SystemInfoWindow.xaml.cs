// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib
{
	// Token: 0x02000041 RID: 65
	public partial class SystemInfoWindow : Window
	{
		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060003B2 RID: 946 RVA: 0x00017C18 File Offset: 0x00015E18
		// (set) Token: 0x060003B3 RID: 947 RVA: 0x00017C20 File Offset: 0x00015E20
		public bool IsAppService
		{
			get
			{
				return this._isAppService;
			}
			set
			{
				this._isAppService = value;
				foreach (IMaintenanceInfo maintenanceInfo in ((IEnumerable<IMaintenanceInfo>)this.MaintenanceInfos.ItemsSource))
				{
					maintenanceInfo.IsAppService = this._isAppService;
					maintenanceInfo.Refresh();
				}
			}
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x00017C88 File Offset: 0x00015E88
		public SystemInfoWindow(IEnumerable<IMaintenanceInfo> maintenanceInfos, bool isAppService)
		{
			this.InitializeComponent();
			this.MaintenanceInfos.ItemsSource = maintenanceInfos;
			this.IsAppService = isAppService;
			this._timer.Interval = TimeSpan.FromMilliseconds(2000.0);
			this._timer.Tick += this.timer_Tick;
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x00017CEF File Offset: 0x00015EEF
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this._timer.Start();
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x00017CFC File Offset: 0x00015EFC
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (Key.Escape == e.Key)
			{
				base.Close();
			}
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x00017D0E File Offset: 0x00015F0E
		private void timer_Tick(object sender, EventArgs e)
		{
			base.Dispatcher.Invoke(new Action(this._mg_timer_Tick_Action_8_0));
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x00017D27 File Offset: 0x00015F27
		private void SystemInfoWindow_OnClosed(object sender, EventArgs e)
		{
			base.Owner.Activate();
		}

		// Token: 0x060003BB RID: 955 RVA: 0x00017DDC File Offset: 0x00015FDC
		[CompilerGenerated]
		private void _mg_timer_Tick_Action_8_0()
		{
			try
			{
				foreach (IMaintenanceInfo maintenanceInfo in ((IEnumerable<IMaintenanceInfo>)this.MaintenanceInfos.ItemsSource))
				{
					maintenanceInfo.Refresh();
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x04000247 RID: 583
		private readonly DispatcherTimer _timer = new DispatcherTimer();

		// Token: 0x04000248 RID: 584
		private bool _isAppService;
	}
}
