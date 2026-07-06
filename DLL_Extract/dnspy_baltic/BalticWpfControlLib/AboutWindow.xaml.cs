// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace BalticWpfControlLib
{
	// Token: 0x02000006 RID: 6
	public partial class AboutWindow : Window
	{
		// Token: 0x0600000B RID: 11 RVA: 0x000020E0 File Offset: 0x000002E0
		public AboutWindow(string displayName, string model, string productVersion, string creationDate)
		{
			this.InitializeComponent();
			this.versionDataLabel.Text = productVersion;
			this.createDateLabel.Text = creationDate;
			base.Title = ((model == "") ? string.Format("About Bruker Plug-in for {0}", displayName) : string.Format("About Bruker Plug-in for  {0} {1}", displayName, model));
			this.textSubTitle.Text = ((model == "") ? string.Format("Bruker Plug-in for {0}™ UHPLC", displayName) : string.Format("Bruker Plug-in for {0}™ {1} UHPLC", displayName, model));
			base.KeyDown += this.AboutWindow_KeyDown;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002181 File Offset: 0x00000381
		private void AboutWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				base.Close();
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002194 File Offset: 0x00000394
		private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				if (assemblyDirectory != null)
				{
					ProcessStartInfo psi = new ProcessStartInfo
					{
						UseShellExecute = true,
						Verb = "open",
						FileName = Path.Combine(assemblyDirectory, "Licenses.html")
					};
					if (psi.FileName.Length > 0)
					{
						using (Process.Start(psi))
						{
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
