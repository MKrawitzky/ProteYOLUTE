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
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class AdvChildParamSettingsUserControl : UserControl, IComponentConnector
{
	public delegate void ModificationUpdate(bool isModified);

	public delegate void ValidationUpdate(bool isValid);

	private readonly BalticInstrumentFacade _instrument;

	private bool _isValidateError;

	private readonly ProcedureInfo _pInfo;

	private readonly BindableBalticMethod _method;

	private readonly ExperimentInfo _experiment;

	private readonly List<ChildProcedureParameter> _advChildProcParams = new List<ChildProcedureParameter>();

	private readonly ChildProcedureArguments _presets = new ChildProcedureArguments();

	private readonly ChildProcedureArguments _childProcArgs = new ChildProcedureArguments();

	private readonly AdvancedChildParameterControl _advChildParameterControl;

	internal TextBlock tbHeader;

	internal ScrollViewer svContent;

	internal Button btnRevert;

	private bool _contentLoaded;

	public string Header { get; }

	public SolidColorBrush HeaderFgColor { get; }

	public event ModificationUpdate ModificationUpdateEvent;

	public event ValidationUpdate ValidationUpdateEvent;

	public AdvChildParamSettingsUserControl(string header, BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
	{
		InitializeComponent();
		_method = method;
		Header = header;
		_instrument = instrument;
		_experiment = experiment;
		_advChildParameterControl = new AdvancedChildParameterControl(BalticInstrumentFacade.IsService);
		svContent.Content = _advChildParameterControl;
		svContent.DataContext = _advChildParameterControl;
		_advChildParameterControl.ArgumentValueUpdated += AdvParameterControl_ArgumentValueUpdated;
		_pInfo = _instrument.GetElutionProcedure(method.ElutionName);
		foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter item in method.AdvancedSettings.ChildParameters.FindAll((BindableBalticMethod.AdvancedSett.AdvancedChildParameter x) => x.Header == Header))
		{
			ChildProcedureParameter childProcedureParameter = _pInfo.AdvChildParameters.FirstOrDefault((ChildProcedureParameter x) => x.Header == item.Header && x.Name == item.Name);
			if (childProcedureParameter != null)
			{
				_advChildProcParams.Add(new ChildProcedureParameter(item.Header, item.Name, "", item.Value.GetType(), item.Value, item.Unit, childProcedureParameter.IsService, BalticInstrumentFacade.IsService));
				_presets.Add(new ChildProcedureArgument(item.Header, item.Name, item.DefaultValue, item.Unit, childProcedureParameter.IsService, BalticInstrumentFacade.IsService));
				_childProcArgs.Add(new ChildProcedureArgument(item.Header, item.Name, item.Value, item.Unit, childProcedureParameter.IsService, BalticInstrumentFacade.IsService));
			}
			else
			{
				_advChildProcParams.Add(new ChildProcedureParameter(item.Header, item.Name, "", item.Value.GetType(), item.Value, item.Unit, item.IsService, BalticInstrumentFacade.IsService));
				_presets.Add(new ChildProcedureArgument(item.Header, item.Name, item.DefaultValue, item.Unit, item.IsService, BalticInstrumentFacade.IsService));
				_childProcArgs.Add(new ChildProcedureArgument(item.Header, item.Name, item.Value, item.Unit, item.IsService, BalticInstrumentFacade.IsService));
			}
		}
		_advChildParameterControl.SetParameters(_advChildProcParams, _childProcArgs, _presets);
		HeaderFgColor = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)method.AdvancedSettings.HeaderFgColor[0], (byte)method.AdvancedSettings.HeaderFgColor[1], (byte)method.AdvancedSettings.HeaderFgColor[2]));
		base.DataContext = this;
		ValidateParameters(_advChildParameterControl);
	}

	private void CheckModified()
	{
		bool isModified = false;
		if (this.ModificationUpdateEvent == null)
		{
			return;
		}
		foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter childParameter in _method.AdvancedSettings.ChildParameters)
		{
			if (childParameter.Value is bool flag)
			{
				if (flag != (bool)childParameter.DefaultValue)
				{
					isModified = true;
					break;
				}
			}
			else if (childParameter.Value is double num)
			{
				if ((int)(num * 1000.0) != (int)((double)childParameter.DefaultValue * 1000.0))
				{
					isModified = true;
					break;
				}
			}
			else if (childParameter.Value is int num2)
			{
				if (num2 != (int)childParameter.DefaultValue)
				{
					isModified = true;
					break;
				}
			}
			else if (childParameter.Value is string text && text != (string)childParameter.DefaultValue)
			{
				isModified = true;
				break;
			}
		}
		this.ModificationUpdateEvent(isModified);
	}

	private void BlueDotTileButton_Click(object sender, RoutedEventArgs e)
	{
		_method.AdvancedSettings.RevertChildrenToDefault();
		_advChildParameterControl.ResetParameterValues();
		ValidateParameters(_advChildParameterControl);
		CheckModified();
	}

	private void AdvParameterControl_ArgumentValueUpdated(object sender, EventArgs e)
	{
		AdvancedChildParameterControl advancedChildParameterControl = sender as AdvancedChildParameterControl;
		if (advancedChildParameterControl != null)
		{
			foreach (ChildProcedureArgument item in advancedChildParameterControl.CreateChildArguments())
			{
				BindableBalticMethod.AdvancedSett.AdvancedChildParameter advancedChildParameter = _method.AdvancedSettings.ChildParameters.Find((BindableBalticMethod.AdvancedSett.AdvancedChildParameter x) => x.Header == item.Header && x.Name == item.ProcArg.Name);
				if (advancedChildParameter != null)
				{
					advancedChildParameter.Value = item.ProcArg.Value;
				}
			}
		}
		ValidateParameters(advancedChildParameterControl);
	}

	private void ValidateParameters(AdvancedChildParameterControl ppc)
	{
		BalticMethod balticMethod = _method.ToBalticMethod();
		ProcedureInfo elutionProcedure = _instrument.GetElutionProcedure(balticMethod.ElutionName);
		ProcedureArguments procedureArguments = elutionProcedure.CreateArguments();
		ProcedureArguments procedureArguments2 = elutionProcedure.CreateAdvancedArguments();
		ChildProcedureArguments childProcedureArguments = elutionProcedure.CreateAdvancedChildArguments();
		ElutionMethodUtil.PopulateMethodArguments(_instrument.IsColumnOvenConnected, balticMethod, procedureArguments, procedureArguments2, childProcedureArguments);
		foreach (ProcedureArgument item in procedureArguments2)
		{
			procedureArguments.Add(new ProcedureArgument(item));
		}
		IEnumerable<ProcedureReportEventArgs> reports = RequestValidation(procedureArguments, childProcedureArguments);
		HandleReports(ppc, reports);
		CheckModified();
	}

	private void HandleReports(AdvancedChildParameterControl ppc, IEnumerable<ProcedureReportEventArgs> reports)
	{
		ppc.ClearErrors();
		ProcedureReportEventArgs[] array = (reports as ProcedureReportEventArgs[]) ?? reports.ToArray();
		ProcedureReportEventArgs[] array2 = array;
		foreach (ProcedureReportEventArgs procedureReportEventArgs in array2)
		{
			ppc.SetError(procedureReportEventArgs.Subject, procedureReportEventArgs.Message);
		}
		_isValidateError = false;
		if (array.Length != 0)
		{
			array2 = array;
			foreach (ProcedureReportEventArgs procedureReportEventArgs2 in array2)
			{
				if (procedureReportEventArgs2.Header != "" && ppc.Exists(procedureReportEventArgs2.Subject))
				{
					_isValidateError = true;
					break;
				}
			}
		}
		this.ValidationUpdateEvent?.Invoke(!_isValidateError);
	}

	private IEnumerable<ProcedureReportEventArgs> RequestValidation(ProcedureArguments args, ChildProcedureArguments childArgs)
	{
		AdvChildValidationRequestEventArgs val = new AdvChildValidationRequestEventArgs(_pInfo, args, childArgs);
		EventHandler<ProcedureReportEventArgs> value = delegate(object _, ProcedureReportEventArgs a)
		{
			val.AddReport(a);
		};
		_instrument.ValidationMessageReported += value;
		_instrument.ValidateMethodProcedureOffLine(_experiment, val.ProcedureSourceInfo, val.ProcedureSourceArgs, val.ProcedureSourceChildArgs);
		_instrument.ValidationMessageReported -= value;
		return val.Reports;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/advchildparamsettingsusercontrol.xaml", UriKind.Relative);
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
			tbHeader = (TextBlock)target;
			break;
		case 2:
			svContent = (ScrollViewer)target;
			break;
		case 3:
			btnRevert = (Button)target;
			btnRevert.Click += BlueDotTileButton_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
