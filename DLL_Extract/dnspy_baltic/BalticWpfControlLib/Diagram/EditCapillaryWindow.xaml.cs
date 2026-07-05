using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x02000074 RID: 116
	public partial class EditCapillaryWindow : Window
	{
		// Token: 0x17000105 RID: 261
		// (get) Token: 0x0600054A RID: 1354 RVA: 0x00038B70 File Offset: 0x00036D70
		public EditCapillaryViewModel Vm
		{
			get
			{
				return this._vm;
			}
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x00038B78 File Offset: 0x00036D78
		public EditCapillaryWindow(BalticPreferences.CapillaryPreference pref)
		{
			this.InitializeComponent();
			this._vm = new EditCapillaryViewModel(pref);
			base.DataContext = this._vm;
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x00038B9E File Offset: 0x00036D9E
		private void btnRevert_Click(object sender, RoutedEventArgs e)
		{
			this._vm.Revert();
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x00006398 File Offset: 0x00004598
		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}

		// Token: 0x0600054E RID: 1358 RVA: 0x00038BAB File Offset: 0x00036DAB
		private void DoubleTextBox_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (this._vm != null)
			{
				this.ValidateParameters();
			}
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x00038BBC File Offset: 0x00036DBC
		private void ValidateParameters()
		{
			if (this._vm.ID < this._vm.MinID || this._vm.ID > this._vm.MaxID)
			{
				this.txtInnerDiameter.BorderBrush = Brushes.Red;
				this.txtInnerDiameter.BorderThickness = new Thickness(2.0);
				this._isValidateIDError = true;
			}
			else
			{
				this.txtInnerDiameter.ClearValue(Control.BorderBrushProperty);
				this.txtInnerDiameter.ClearValue(Control.BorderThicknessProperty);
				this.txtInnerDiameter.ClearValue(FrameworkElement.ToolTipProperty);
				this._isValidateIDError = false;
			}
			if (this._vm.Length < this._vm.MinLength || this._vm.Length > this._vm.MaxLength)
			{
				this.txtLength.BorderBrush = Brushes.Red;
				this.txtLength.BorderThickness = new Thickness(2.0);
				this._isValidateLengthError = true;
			}
			else
			{
				this.txtLength.ClearValue(Control.BorderBrushProperty);
				this.txtInnerDiameter.ClearValue(Control.BorderThicknessProperty);
				this.txtLength.ClearValue(FrameworkElement.ToolTipProperty);
				this._isValidateLengthError = false;
			}
			this.btnOK.IsEnabled = !this._isValidateIDError && !this._isValidateLengthError;
			this.txtLength.ToolTip = this._vm.LengthRangeToolTip;
			this.txtInnerDiameter.ToolTip = this._vm.IDRangeToolTip;
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x00038BAB File Offset: 0x00036DAB
		private void DoubleTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (this._vm != null)
			{
				this.ValidateParameters();
			}
		}

		// Token: 0x040002D0 RID: 720
		private bool _isValidateIDError;

		// Token: 0x040002D1 RID: 721
		private bool _isValidateLengthError;

		// Token: 0x040002D2 RID: 722
		private EditCapillaryViewModel _vm;
	}
}
