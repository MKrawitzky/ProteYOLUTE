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
using System.Windows.Threading;
using BalticClassLib;
using Bruker.Lc.Baltic;

namespace BalticWpfControlLib
{
	// Token: 0x0200002B RID: 43
	public partial class MethodEditorWindow : Window
	{
		// Token: 0x06000264 RID: 612 RVA: 0x0000FE8C File Offset: 0x0000E08C
		public MethodEditorWindow(BalticMethod method, BalticInstrumentFacade facade, BalticHWProfile hwProfile, BalticColumnList columns, ColumnSelections columnSelections, BalticPreferences preferences, string displayName, string versionStr, bool isPressurePSI = false, bool isOvenDetected = false)
		{
			this.InitializeComponent();
			string model = Utility.CreateDisplayVersion(versionStr);
			if (BalticInstrumentFacade.IsService)
			{
				base.Title = ((model == "") ? string.Format("Bruker {0} Instant Expertise Method Editor [SERVICE]", displayName) : string.Format("Bruker {0} {1} Instant Expertise Method Editor", displayName, model));
			}
			else
			{
				base.Title = ((model == "") ? string.Format("Bruker {0} Instant Expertise Method Editor", displayName) : string.Format("Bruker {0} {1} Instant Expertise Method Editor", displayName, model));
			}
			this.ucMethodEditor = new MethodUserControl(this, method, facade, columns, columnSelections, isPressurePSI, isOvenDetected);
			this.ucMethodEditor.ValidateInputUpdateEvent += this.wpfMethodCtrl_ValidInputDataUpdate;
			this.ucMethodEditor.EnableMethodCompleteEvent += this.wpfMethodCtrl_EnableMethodComplete;
			this.ucMethodEditor.TrapSelectionWarningEvent += this.wpfMethodCtrl_TrapSelectionWarning;
			Grid.SetRow(this.ucMethodEditor, 0);
			Grid.SetColumn(this.ucMethodEditor, 0);
			this.ucMethodEditor.VerticalAlignment = VerticalAlignment.Top;
			this.gridMethod.Children.Add(this.ucMethodEditor);
		}

		// Token: 0x06000265 RID: 613 RVA: 0x0000FFA8 File Offset: 0x0000E1A8
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			base.SizeToContent = SizeToContent.Height;
		}

		// Token: 0x06000266 RID: 614 RVA: 0x0000FFB1 File Offset: 0x0000E1B1
		private void wpfMethodCtrl_TrapSelectionWarning(bool isShow, string message = "")
		{
			if (message != "")
			{
				this.msgText.Text = message;
			}
			this.stkTrapWarn.Visibility = (isShow ? Visibility.Visible : Visibility.Hidden);
		}

		// Token: 0x06000267 RID: 615 RVA: 0x0000FFDE File Offset: 0x0000E1DE
		private void wpfMethodCtrl_EnableMethodComplete(bool isEnabled)
		{
			this.btnOK.IsEnabled = isEnabled;
		}

		// Token: 0x06000268 RID: 616 RVA: 0x0000FFDE File Offset: 0x0000E1DE
		private void wpfMethodCtrl_ValidInputDataUpdate(bool isValid)
		{
			this.btnOK.IsEnabled = isValid;
		}

		// Token: 0x06000269 RID: 617 RVA: 0x0000FFEC File Offset: 0x0000E1EC
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			this.ucMethodEditor.ToBalticMethod();
			base.DialogResult = new bool?(true);
		}

		// Token: 0x0600026A RID: 618 RVA: 0x00010008 File Offset: 0x0000E208
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			bool? dialogResult = base.DialogResult;
			bool flag = false;
			if ((dialogResult.GetValueOrDefault() == flag) & (dialogResult != null))
			{
				return;
			}
			if (this.btnOK.IsFocused || this.btnCancel.IsFocused)
			{
				return;
			}
			e.Cancel = true;
			this.btnCancel.Focus();
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				base.DialogResult = new bool?(false);
			}), DispatcherPriority.Background, Array.Empty<object>());
		}

		// Token: 0x04000173 RID: 371
		private MethodUserControl ucMethodEditor;
	}
}
