// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

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

namespace BalticWpfControlLib.Diagram;

public class EditCapillaryWindow : Window, IComponentConnector
{
	private bool _isValidateIDError;

	private bool _isValidateLengthError;

	private EditCapillaryViewModel _vm;

	internal Grid gridSettings;

	internal DoubleTextBox txtLength;

	internal DoubleTextBox txtInnerDiameter;

	internal Button btnRevert;

	internal Button btnOK;

	private bool _contentLoaded;

	public EditCapillaryViewModel Vm => _vm;

	public EditCapillaryWindow(BalticPreferences.CapillaryPreference pref)
	{
		InitializeComponent();
		_vm = new EditCapillaryViewModel(pref);
		base.DataContext = _vm;
	}

	private void btnRevert_Click(object sender, RoutedEventArgs e)
	{
		_vm.Revert();
	}

	private void btnOK_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
	}

	private void DoubleTextBox_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (_vm != null)
		{
			ValidateParameters();
		}
	}

	private void ValidateParameters()
	{
		if (_vm.ID < _vm.MinID || _vm.ID > _vm.MaxID)
		{
			txtInnerDiameter.BorderBrush = Brushes.Red;
			txtInnerDiameter.BorderThickness = new Thickness(2.0);
			_isValidateIDError = true;
		}
		else
		{
			txtInnerDiameter.ClearValue(Control.BorderBrushProperty);
			txtInnerDiameter.ClearValue(Control.BorderThicknessProperty);
			txtInnerDiameter.ClearValue(FrameworkElement.ToolTipProperty);
			_isValidateIDError = false;
		}
		if (_vm.Length < _vm.MinLength || _vm.Length > _vm.MaxLength)
		{
			txtLength.BorderBrush = Brushes.Red;
			txtLength.BorderThickness = new Thickness(2.0);
			_isValidateLengthError = true;
		}
		else
		{
			txtLength.ClearValue(Control.BorderBrushProperty);
			txtInnerDiameter.ClearValue(Control.BorderThicknessProperty);
			txtLength.ClearValue(FrameworkElement.ToolTipProperty);
			_isValidateLengthError = false;
		}
		btnOK.IsEnabled = !_isValidateIDError && !_isValidateLengthError;
		txtLength.ToolTip = _vm.LengthRangeToolTip;
		txtInnerDiameter.ToolTip = _vm.IDRangeToolTip;
	}

	private void DoubleTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (_vm != null)
		{
			ValidateParameters();
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/diagram/editcapillarywindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			gridSettings = (Grid)target;
			break;
		case 2:
			txtLength = (DoubleTextBox)target;
			txtLength.ValueChanged += DoubleTextBox_ValueChanged;
			txtLength.MouseWheel += DoubleTextBox_MouseWheel;
			break;
		case 3:
			txtInnerDiameter = (DoubleTextBox)target;
			txtInnerDiameter.ValueChanged += DoubleTextBox_ValueChanged;
			txtInnerDiameter.MouseWheel += DoubleTextBox_MouseWheel;
			break;
		case 4:
			btnRevert = (Button)target;
			btnRevert.Click += btnRevert_Click;
			break;
		case 5:
			btnOK = (Button)target;
			btnOK.Click += btnOK_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
