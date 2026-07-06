// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class ScriptSettingsWindow : Window, IComponentConnector
{
	public delegate void ApplySettingsDelegate(ProcedureArguments arguments, ChildProcedureArguments childArguments);

	internal Button btnApply;

	private bool _contentLoaded;

	public string Description { get; }

	public ProcedureParameterControl ParameterControl { get; }

	public bool IsApplyActive { get; set; }

	public event ApplySettingsDelegate ApplySettingsEvent;

	public ScriptSettingsWindow(ProcedureInfo info, ProcedureArguments presets, ChildProcedureArguments childPresets, string privatePath, dynamic balticSettings, bool isService, bool isApplySettings = false)
	{
		dynamic val = Path.Combine(privatePath, balticSettings.TooltipImageDirectoryName);
		InitializeComponent();
		Description = info.Description;
		ParameterControl = new ProcedureParameterControl(isService);
		ParameterControl.SetParameters(info.Parameters, info.AdvChildParameters, val, presets, childPresets);
		IsApplyActive = info.IsApplyActive;
		base.DataContext = this;
	}

	private void btnOK_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
	}

	private void btnCancel_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
	}

	private void btnApply_Click(object sender, RoutedEventArgs e)
	{
		var (procedureArguments, childArguments) = ParameterControl.GetParameters();
		if (procedureArguments.Count > 0)
		{
			this.ApplySettingsEvent?.Invoke(procedureArguments, childArguments);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/scriptsettingswindow.xaml", UriKind.Relative);
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
			btnApply = (Button)target;
			btnApply.Click += btnApply_Click;
			break;
		case 2:
			((Button)target).Click += btnOK_Click;
			break;
		case 3:
			((Button)target).Click += btnCancel_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
