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
using System.Windows.Markup;
using BalticClassLib;
using Bruker.Lc.Baltic;

namespace BalticWpfControlLib;

public class PreferencesWindow : Window, IComponentConnector
{
	private PreferencesUserControl ucPreferences;

	internal Grid prefGrid;

	internal Button btnOK;

	private bool _contentLoaded;

	public PreferencesWindow(BalticPreferences preferences, ExperimentInfo experiment, BalticInstrumentFacade instrument)
	{
		InitializeComponent();
		ucPreferences = new PreferencesUserControl(preferences, experiment, instrument);
		ucPreferences.ValidationUpdateEvent += ucPreferences_UpdateInputValidation;
		Grid.SetRow(ucPreferences, 0);
		Grid.SetColumn(ucPreferences, 0);
		Grid.SetColumnSpan(ucPreferences, 2);
		ucPreferences.VerticalAlignment = VerticalAlignment.Top;
		prefGrid.Children.Add(ucPreferences);
	}

	private void Button_OK_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
	}

	private void Button_Cancel_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void ucPreferences_UpdateInputValidation(bool isValid)
	{
		btnOK.IsEnabled = isValid;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/preferenceswindow.xaml", UriKind.Relative);
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
			prefGrid = (Grid)target;
			break;
		case 2:
			btnOK = (Button)target;
			btnOK.Click += Button_OK_Click;
			break;
		case 3:
			((Button)target).Click += Button_Cancel_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
