using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{

public partial class ScriptSettingsWindow : Window, IComponentConnector
{
	public delegate void ApplySettingsDelegate(ProcedureArguments arguments, ChildProcedureArguments childArguments);



	public string Description { get; private set; }

	public ProcedureParameterControl ParameterControl { get; private set; }

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


}
}
