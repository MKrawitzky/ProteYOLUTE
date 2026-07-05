using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BalticClassLib;
using Bruker.Lc.Baltic;

namespace BalticWpfControlLib
{
	// Token: 0x0200002F RID: 47
	public partial class PreferencesWindow : Window
	{
		// Token: 0x060002D0 RID: 720 RVA: 0x0001378C File Offset: 0x0001198C
		public PreferencesWindow(BalticPreferences preferences, ExperimentInfo experiment, BalticInstrumentFacade instrument)
		{
			this.InitializeComponent();
			this.ucPreferences = new PreferencesUserControl(preferences, experiment, instrument);
			this.ucPreferences.ValidationUpdateEvent += this.ucPreferences_UpdateInputValidation;
			Grid.SetRow(this.ucPreferences, 0);
			Grid.SetColumn(this.ucPreferences, 0);
			Grid.SetColumnSpan(this.ucPreferences, 2);
			this.ucPreferences.VerticalAlignment = VerticalAlignment.Top;
			this.prefGrid.Children.Add(this.ucPreferences);
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x00006398 File Offset: 0x00004598
		private void Button_OK_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x00013811 File Offset: 0x00011A11
		private void Button_Cancel_Click(object sender, RoutedEventArgs e)
		{
			base.Close();
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x00013819 File Offset: 0x00011A19
		private void ucPreferences_UpdateInputValidation(bool isValid)
		{
			this.btnOK.IsEnabled = isValid;
		}

		// Token: 0x040001C1 RID: 449
		private PreferencesUserControl ucPreferences;
	}
}
