// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class ScriptUserControl : UserControl, IComponentConnector
{
	public delegate void ScriptControlEventHandler(object sender, ScriptControlEventArgs e);

	private readonly Window _ownerWindow;

	private readonly string _privatePath;

	private readonly dynamic _balticSettings;

	private readonly BalticInstrumentFacade _instrument;

	internal Image imgScriptImage;

	internal TextBlock tbScriptName;

	internal Button btnSettings;

	internal Image imgScriptSettings;

	private bool _contentLoaded;

	public ScriptSettingsWindow SettingsDlg { get; private set; }

	public ProcedureInfo Info { get; }

	public event ScriptControlEventHandler OnScriptActionClick;

	public event ScriptControlEventHandler OnScriptApplyClick;

	public event EventHandler<ScriptValidationRequestEventArgs> ScriptValidationRequest;

	public event EventHandler<ScriptControlEventArgs> ScriptArgumentPresetRequest;

	public ScriptUserControl(ProcedureInfo pInfo, Window parentWindow, string privatePath, dynamic balticSettings, BalticInstrumentFacade instrument)
	{
		InitializeComponent();
		Info = pInfo;
		_ownerWindow = parentWindow;
		_privatePath = privatePath;
		_balticSettings = balticSettings;
		_instrument = instrument;
		tbScriptName.Text = Info.Name;
		imgScriptImage.Source = (ImageSource)new MyImageConverter().Convert("Images/play_24_mba.png");
		imgScriptSettings.Source = (ImageSource)new MyImageConverter().Convert(pInfo.Parameters.Any() ? "Images/settings_24.png" : "Images/settingsDisabled_24.png");
		base.Margin = new Thickness(4.0, 4.0, 4.0, 4.0);
		base.ToolTip = Info.Description;
		btnSettings.IsEnabled = pInfo.Parameters.Any();
	}

	private void ScriptButton_Click(object sender, RoutedEventArgs e)
	{
		ScriptAction(forceShowSettings: false);
	}

	private void ScriptSettingsButton_Click(object sender, RoutedEventArgs e)
	{
		ScriptAction(forceShowSettings: true);
	}

	public void ScriptAction(bool forceShowSettings)
	{
		ProcedureArguments procedureArguments = Info.CreateArguments();
		ChildProcedureArguments childProcedureArguments = Info.CreateAdvancedChildArguments();
		if (this.ScriptArgumentPresetRequest != null)
		{
			ScriptControlEventArgs e = new ScriptControlEventArgs(Info, procedureArguments);
			this.ScriptArgumentPresetRequest(this, e);
		}
		ProcedureReportEventArgs[] array = RequestValidation(procedureArguments).ToArray();
		if (forceShowSettings || array.Length != 0)
		{
			SettingsDlg = new ScriptSettingsWindow(Info, procedureArguments, childProcedureArguments, _privatePath, _balticSettings, BalticInstrumentFacade.IsService);
			SettingsDlg.ApplySettingsEvent += Dlg_ApplySettingsEvent;
			SettingsDlg.Owner = _ownerWindow;
			HandleReports(SettingsDlg.ParameterControl, array);
			SettingsDlg.ParameterControl.ArgumentValueUpdated += ParameterControl_ArgumentValueUpdated;
			SettingsDlg.ShowDialog();
			if (!SettingsDlg.DialogResult.GetValueOrDefault())
			{
				SettingsDlg = null;
				return;
			}
			procedureArguments = SettingsDlg.ParameterControl.CreateArguments();
			childProcedureArguments = ProcedureParameterControl.CreateChildArguments();
			SettingsDlg = null;
		}
		ScriptControlEventArgs e2 = new ScriptControlEventArgs(Info, procedureArguments, childProcedureArguments);
		this.OnScriptActionClick?.Invoke(this, e2);
	}

	private void Dlg_ApplySettingsEvent(ProcedureArguments arguments, ChildProcedureArguments childArguments)
	{
		ScriptControlEventArgs e = new ScriptControlEventArgs(Info, arguments, childArguments);
		this.OnScriptApplyClick?.Invoke(this, e);
	}

	private void ParameterControl_ArgumentValueUpdated(object sender, EventArgs e)
	{
		ProcedureParameterControl procedureParameterControl = sender as ProcedureParameterControl;
		IEnumerable<ProcedureReportEventArgs> reports = RequestValidation(procedureParameterControl.CreateArguments(), ProcedureParameterControl.CreateChildArguments());
		HandleReports(procedureParameterControl, reports);
	}

	private static void HandleReports(ProcedureParameterControl ppc, IEnumerable<ProcedureReportEventArgs> reports)
	{
		ppc.ClearErrors();
		foreach (ProcedureReportEventArgs report in reports)
		{
			ppc.SetError(report.Subject, report.Message);
		}
	}

	private IEnumerable<ProcedureReportEventArgs> RequestValidation(ProcedureArguments args, ChildProcedureArguments childArgs = null)
	{
		if (this.ScriptValidationRequest == null)
		{
			return new List<ProcedureReportEventArgs>();
		}
		ScriptValidationRequestEventArgs scriptValidationRequestEventArgs = new ScriptValidationRequestEventArgs(Info, args, childArgs);
		this.ScriptValidationRequest(this, scriptValidationRequestEventArgs);
		return scriptValidationRequestEventArgs.Reports;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/scriptusercontrol.xaml", UriKind.Relative);
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
			((Button)target).Click += ScriptButton_Click;
			break;
		case 2:
			imgScriptImage = (Image)target;
			break;
		case 3:
			tbScriptName = (TextBlock)target;
			break;
		case 4:
			btnSettings = (Button)target;
			btnSettings.Click += ScriptSettingsButton_Click;
			break;
		case 5:
			imgScriptSettings = (Image)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
