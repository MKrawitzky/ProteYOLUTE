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
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BalticClassLib;
using BalticWpfControlLib.Diagram;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using BrukerLC.Utils.Controls;

namespace BalticWpfControlLib
{
	// Token: 0x0200001B RID: 27
	public partial class CapillariesDiagramWindow : Window, INotifyPropertyChanged
	{
		// Token: 0x060000D9 RID: 217 RVA: 0x00005FC0 File Offset: 0x000041C0
		static CapillariesDiagramWindow()
		{
			string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (!string.IsNullOrEmpty(directory))
			{
				Assembly.LoadFrom(Path.Combine(directory, "BrukerLC.Styling.dll"));
			}
		}

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x060000DA RID: 218 RVA: 0x00005FF8 File Offset: 0x000041F8
		// (remove) Token: 0x060000DB RID: 219 RVA: 0x00006030 File Offset: 0x00004230
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000DC RID: 220 RVA: 0x00006065 File Offset: 0x00004265
		public DiagramStateController Controller { get; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000DD RID: 221 RVA: 0x0000606D File Offset: 0x0000426D
		// (set) Token: 0x060000DE RID: 222 RVA: 0x00006075 File Offset: 0x00004275
		public bool IsModifiedFromFactory
		{
			get
			{
				return this._isModifiedFromFactory;
			}
			set
			{
				this._isModifiedFromFactory = value;
				this.NotifyPropertyChanged("IsModifiedFromFactory");
			}
		}

		// Token: 0x060000DF RID: 223 RVA: 0x0000608C File Offset: 0x0000428C
		public CapillariesDiagramWindow(BalticInstrumentFacade instrument, List<BalticHWProfile.CapillaryItem> capillaries, List<BalticPreferences.CapillaryPreference> prefCapillaries, bool isDisplayPressureAsPsi)
		{
			this.InitializeComponent();
			this.Controller = new DiagramStateController(instrument, capillaries, prefCapillaries, this.Diagram, true, isDisplayPressureAsPsi, false);
			this._instrument = instrument;
			this._prefCapillaries = prefCapillaries;
			base.DataContext = this;
			this.CheckFactoryDefaultModified();
			base.SourceInitialized += delegate(object _, EventArgs _)
			{
				this.HideMinimizeMinimizeButton();
			};
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x000060EA File Offset: 0x000042EA
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00006104 File Offset: 0x00004304
		private void CheckFactoryDefaultModified()
		{
			BalticPreferences preferences;
			try
			{
				using (StreamReader reader = new StreamReader(Path.Combine(this._instrument.PrivatePath, "Preferences.xml")))
				{
					preferences = (BalticPreferences)Utility.FromXML(reader.ReadToEnd(), typeof(BalticPreferences));
				}
			}
			catch (Exception)
			{
				preferences = new BalticPreferences();
			}
			this._prefCapillaries = preferences.Capillaries;
			bool isModified = false;
			foreach (BalticPreferences.CapillaryPreference capillary in this._prefCapillaries)
			{
				if ((int)(capillary.DefaultLength * 100.0) != (int)(capillary.FactoryLength * 100.0) || (int)(capillary.DefaultID * 100.0) != (int)(capillary.FactoryID * 100.0))
				{
					isModified = true;
					break;
				}
			}
			this.IsModifiedFromFactory = isModified;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x0000621C File Offset: 0x0000441C
		private void btnRevert_Click(object sender, RoutedEventArgs e)
		{
			this.Controller.RevertAllCapillaries();
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x00006229 File Offset: 0x00004429
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
			this.Controller.SetHwCapillaries();
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00006242 File Offset: 0x00004442
		private void btnSaveDefault_Click(object sender, RoutedEventArgs e)
		{
			this.Controller.SaveAsDefaultCapillaries();
			this.Controller.SetHwCapillaries();
			this.CheckFactoryDefaultModified();
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x00006260 File Offset: 0x00004460
		private void btnRevertFactory_Click(object sender, RoutedEventArgs e)
		{
			this.Controller.RevertAllCapillariesToFactory();
			this.CheckFactoryDefaultModified();
		}

		// Token: 0x04000076 RID: 118
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x04000077 RID: 119
		private bool _isModifiedFromFactory;

		// Token: 0x04000078 RID: 120
		private List<BalticPreferences.CapillaryPreference> _prefCapillaries;
	}
}
