// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using BalticClassLib;
using Bruker.Lc.Baltic;

namespace BalticWpfControlLib
{
	// Token: 0x02000021 RID: 33
	public partial class HardwareSettingsWindow : Window
	{
		// Token: 0x0600015E RID: 350
		[DllImport("user32.dll")]
		private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

		// Token: 0x0600015F RID: 351
		[DllImport("user32.dll")]
		private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

		// Token: 0x06000160 RID: 352 RVA: 0x00009D80 File Offset: 0x00007F80
		public HardwareSettingsWindow(string privatePath, BalticHWProfile hardwareProfile, BalticInstrumentFacade instrument, BalticPreferences preferences, string displayName)
		{
			this.InitializeComponent();
			this._instrument = instrument;
			string model = Utility.CreateDisplayVersion(preferences.Version);
			base.Title = ((model == "") ? ("Bruker " + displayName + " Hardware Settings") : string.Concat(new string[] { "Bruker ", displayName, " ", model, " Hardware Settings" }));
			this._ucHardwareSettings = new SettingsUserControl(this, privatePath, hardwareProfile, instrument, displayName);
			this._ucHardwareSettings.ValidationUpdateEvent += this.ucHardwareSettings_UpdateInputValidation;
			this._ucHardwareSettings.OkCancelButtonUpdateEvent += this.UcHardwareSettings_OkCancelButtonUpdateEvent;
			Grid.SetRow(this._ucHardwareSettings, 0);
			Grid.SetColumn(this._ucHardwareSettings, 0);
			this._ucHardwareSettings.VerticalAlignment = VerticalAlignment.Top;
			this.gridSettings.Children.Add(this._ucHardwareSettings);
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00009E7C File Offset: 0x0000807C
		private void UcHardwareSettings_OkCancelButtonUpdateEvent(bool isEnabled)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				this.btnOK.IsEnabled = isEnabled;
				this.btnCanel.IsEnabled = isEnabled;
				HardwareSettingsWindow.EnableMenuItem(HardwareSettingsWindow.GetSystemMenu(new WindowInteropHelper(this).Handle, false), 61536U, (!isEnabled) ? 1U : 0U);
			}), Array.Empty<object>());
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00009EBA File Offset: 0x000080BA
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			this._ucHardwareSettings.WritePreferences();
			base.DialogResult = new bool?(true);
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00009ED4 File Offset: 0x000080D4
		private void ucHardwareSettings_UpdateInputValidation(bool isValid)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				this.btnOK.IsEnabled = isValid;
			}), Array.Empty<object>());
		}

		// Token: 0x06000164 RID: 356 RVA: 0x00009F14 File Offset: 0x00008114
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (!this.btnOK.IsEnabled && !this.btnCanel.IsEnabled)
			{
				e.Cancel = true;
			}
			bool? dialogResult = base.DialogResult;
			bool flag = false;
			if (!((dialogResult.GetValueOrDefault() == flag) & (dialogResult != null)))
			{
				this._ucHardwareSettings.ValidatePreferences();
			}
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00009F6C File Offset: 0x0000816C
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				base.Close();
			}
			if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Shift)
			{
				if (this._ucHardwareSettings.IsService)
				{
					this._instrument.ClearServiceMode();
					this._ucHardwareSettings.IsService = false;
					return;
				}
				if (new PasswordWindow
				{
					Owner = this
				}.ShowDialog().GetValueOrDefault())
				{
					this._instrument.CreateServiceMode();
					this._ucHardwareSettings.IsService = true;
				}
			}
		}

		// Token: 0x040000CA RID: 202
		private const uint MF_GRAYED = 1U;

		// Token: 0x040000CB RID: 203
		private const uint MF_ENABLED = 0U;

		// Token: 0x040000CC RID: 204
		private const uint SC_CLOSE = 61536U;

		// Token: 0x040000CD RID: 205
		private readonly SettingsUserControl _ucHardwareSettings;

		// Token: 0x040000CE RID: 206
		private readonly BalticInstrumentFacade _instrument;
	}
}
