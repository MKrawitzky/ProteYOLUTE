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

namespace BalticWpfControlLib
{
	// Token: 0x0200001C RID: 28
	public partial class ConfirmWindow : Window
	{
		// Token: 0x060000E9 RID: 233 RVA: 0x0000637E File Offset: 0x0000457E
		public ConfirmWindow(string message)
		{
			this.InitializeComponent();
			this.txtMessage.Text = message;
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00006398 File Offset: 0x00004598
		private void BtnOK_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}
	}
}
