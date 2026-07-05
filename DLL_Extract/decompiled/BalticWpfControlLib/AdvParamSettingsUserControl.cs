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

public class AdvParamSettingsUserControl : UserControl, IComponentConnector
{
	public delegate void ModificationUpdate(bool isModified);

	public delegate void ValidationUpdate(bool isValid);

	private readonly BalticInstrumentFacade _instrument;

	private bool _isValidateError;

	private readonly ProcedureInfo _pInfo;

	private BindableBalticMethod _method;

	private ExperimentInfo _experiment;

	private readonly AdvancedParameterControl _advParamUserControl;

	private readonly List<ProcedureParameter> _advProcParams = new List<ProcedureParameter>();

	private readonly List<ChildProcedureParameter> _advChildProcParams = new List<ChildProcedureParameter>();

	private readonly ProcedureArguments _presets = new ProcedureArguments();

	private readonly ProcedureArguments _procArgs = new ProcedureArguments();

	private readonly ChildProcedureArguments _childPresets = new ChildProcedureArguments();

	private readonly ChildProcedureArguments _childProcArgs = new ChildProcedureArguments();

	internal TextBlock tbHeader;

	internal ScrollViewer advScroll;

	internal Button btnRevert;

	private bool _contentLoaded;

	public string Header { get; }

	public SolidColorBrush HeaderFgColor { get; }

	public event ModificationUpdate ModificationUpdateEvent;

	public event ValidationUpdate ValidationUpdateEvent;

	public AdvParamSettingsUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment, bool isService)
	{
		InitializeComponent();
		_method = method;
		_instrument = instrument;
		_experiment = experiment;
		_advParamUserControl = new AdvancedParameterControl(method, instrument, experiment, isService);
		_advParamUserControl.ValidationUpdateEvent += AdvParamUserControlValidationUpdateEvent;
		_advParamUserControl.ModificationUpdateEvent += AdvParamUserControlModificationUpdateEvent;
		advScroll.Content = _advParamUserControl;
		advScroll.DataContext = _advParamUserControl;
		_advParamUserControl.ArgumentValueUpdated += AdvParameterControl_ArgumentValueUpdated;
		_pInfo = _instrument.GetElutionProcedure(method.ElutionName);
		foreach (BindableBalticMethod.AdvancedSett.AdvancedParameter item in method.AdvancedSettings.Parameters)
		{
			ProcedureParameter procedureParameter = _pInfo.AdvParameters.FirstOrDefault((ProcedureParameter x) => x.Name == item.Name);
			if (procedureParameter != null)
			{
				_advProcParams.Add(new ProcedureParameter(item.Name, "", item.Value.GetType(), item.Value, item.Unit, procedureParameter.IsService, BalticInstrumentFacade.IsService));
				_presets.Add(new ProcedureArgument(item.Name, item.DefaultValue, item.Unit, procedureParameter.IsService, BalticInstrumentFacade.IsService));
				_procArgs.Add(new ProcedureArgument(item.Name, item.Value, item.Unit, procedureParameter.IsService, BalticInstrumentFacade.IsService));
			}
			else
			{
				_advProcParams.Add(new ProcedureParameter(item.Name, "", item.Value.GetType(), item.Value, item.Unit));
				_presets.Add(new ProcedureArgument(item.Name, item.DefaultValue, item.Unit));
				_procArgs.Add(new ProcedureArgument(item.Name, item.Value, item.Unit));
			}
		}
		foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter item2 in method.AdvancedSettings.ChildParameters)
		{
			if (_pInfo.AdvChildParameters.FirstOrDefault((ChildProcedureParameter x) => x.Header == item2.Header && x.Name == item2.Name) != null)
			{
				_advChildProcParams.Add(new ChildProcedureParameter(item2.Header, item2.Name, "", item2.Value.GetType(), item2.Value, item2.Unit, item2.IsService, BalticInstrumentFacade.IsService));
				_childPresets.Add(new ChildProcedureArgument(item2.Header, item2.Name, item2.DefaultValue, item2.Unit, item2.IsService, BalticInstrumentFacade.IsService));
				_childProcArgs.Add(new ChildProcedureArgument(item2.Header, item2.Name, item2.Value, item2.Unit, item2.IsService, BalticInstrumentFacade.IsService));
			}
			else
			{
				_advChildProcParams.Add(new ChildProcedureParameter(item2.Header, item2.Name, "", item2.Value.GetType(), item2.Value, item2.Unit, item2.IsService, BalticInstrumentFacade.IsService));
				_childPresets.Add(new ChildProcedureArgument(item2.Header, item2.Name, item2.DefaultValue, item2.Unit, item2.IsService, BalticInstrumentFacade.IsService));
				_childProcArgs.Add(new ChildProcedureArgument(item2.Header, item2.Name, item2.Value, item2.Unit, item2.IsService, BalticInstrumentFacade.IsService));
			}
		}
		_advParamUserControl.SetParameters(_advProcParams, _procArgs, _advChildProcParams, _childProcArgs, _presets, _childPresets);
		Header = method.AdvancedSettings.Header;
		HeaderFgColor = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)method.AdvancedSettings.HeaderFgColor[0], (byte)method.AdvancedSettings.HeaderFgColor[1], (byte)method.AdvancedSettings.HeaderFgColor[2]));
		base.DataContext = this;
		ValidateParameters();
	}

	private void AdvParamUserControlModificationUpdateEvent(bool isModified)
	{
		this.ModificationUpdateEvent?.Invoke(isModified);
		if (!isModified)
		{
			CheckModified();
		}
	}

	private void AdvParamUserControlValidationUpdateEvent(bool isValid)
	{
		this.ValidationUpdateEvent?.Invoke(isValid);
		ValidateParameters();
	}

	public void RefreshParameters(ExperimentInfo experiment, BindableBalticMethod method)
	{
		_experiment = experiment;
		_method = method;
		CheckModified();
	}

	private void CheckModified()
	{
		bool isModified = false;
		if (this.ModificationUpdateEvent == null)
		{
			return;
		}
		foreach (BindableBalticMethod.AdvancedSett.AdvancedParameter parameter in _method.AdvancedSettings.Parameters)
		{
			if (parameter.Value is bool flag)
			{
				if (flag != (bool)parameter.DefaultValue)
				{
					isModified = true;
					break;
				}
			}
			else if (parameter.Value is double num)
			{
				if ((int)(num * 1000.0) != (int)((double)parameter.DefaultValue * 1000.0))
				{
					isModified = true;
					break;
				}
			}
			else if (parameter.Value is int num2)
			{
				if (num2 != (int)parameter.DefaultValue)
				{
					isModified = true;
					break;
				}
			}
			else if (parameter.Value is string text && text != (string)parameter.DefaultValue)
			{
				isModified = true;
				break;
			}
		}
		foreach (BindableBalticMethod.AdvancedSett.AdvancedChildParameter childParameter in _method.AdvancedSettings.ChildParameters)
		{
			if (childParameter.Value is bool flag2)
			{
				if (flag2 != (bool)childParameter.DefaultValue)
				{
					isModified = true;
					break;
				}
			}
			else if (childParameter.Value is double num3)
			{
				if ((int)(num3 * 1000.0) != (int)((double)childParameter.DefaultValue * 1000.0))
				{
					isModified = true;
					break;
				}
			}
			else if (childParameter.Value is int num4)
			{
				if (num4 != (int)childParameter.DefaultValue)
				{
					isModified = true;
					break;
				}
			}
			else if (childParameter.Value is string text2 && text2 != (string)childParameter.DefaultValue)
			{
				isModified = true;
				break;
			}
		}
		this.ModificationUpdateEvent(isModified);
	}

	private void BlueDotTileButton_Click(object sender, RoutedEventArgs e)
	{
		_method.AdvancedSettings.RevertToDefault();
		_advParamUserControl.ResetParameterValues();
		ValidateParameters(_advParamUserControl);
		CheckModified();
	}

	private void AdvParameterControl_ArgumentValueUpdated(object sender, EventArgs e)
	{
		AdvancedParameterControl advancedParameterControl = sender as AdvancedParameterControl;
		if (advancedParameterControl != null)
		{
			foreach (ProcedureArgument item in advancedParameterControl.CreateArguments())
			{
				BindableBalticMethod.AdvancedSett.AdvancedParameter advancedParameter = _method.AdvancedSettings.Parameters.Find((BindableBalticMethod.AdvancedSett.AdvancedParameter x) => x.Name == item.Name);
				if (advancedParameter != null)
				{
					advancedParameter.Value = item.Value;
				}
			}
		}
		ValidateParameters(advancedParameterControl);
	}

	public void ValidateParameters()
	{
		ValidateParameters(_advParamUserControl);
	}

	private void ValidateParameters(AdvancedParameterControl ppc)
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

	private void HandleReports(AdvancedParameterControl ppc, IEnumerable<ProcedureReportEventArgs> reports)
	{
		ppc.ClearErrors();
		ProcedureReportEventArgs[] array = (reports as ProcedureReportEventArgs[]) ?? reports.ToArray();
		ProcedureReportEventArgs[] array2 = array;
		foreach (ProcedureReportEventArgs procedureReportEventArgs in array2)
		{
			ppc.SetError(procedureReportEventArgs.Header, procedureReportEventArgs.Subject, procedureReportEventArgs.Message);
		}
		_isValidateError = array.Any((ProcedureReportEventArgs r) => !r.IsGradientSubject);
		this.ValidationUpdateEvent?.Invoke(!_isValidateError);
	}

	private IEnumerable<ProcedureReportEventArgs> RequestValidation(ProcedureArguments args, ChildProcedureArguments childArgs)
	{
		AdvValidationRequestEventArgs val = new AdvValidationRequestEventArgs(_pInfo, args, childArgs);
		EventHandler<ProcedureReportEventArgs> value = delegate(object _, ProcedureReportEventArgs a)
		{
			val.AddReport(a);
		};
		_instrument.ValidationMessageReported += value;
		_instrument.ValidateMethodProcedureOffLine(_experiment, val.ProcedureSourceInfo, val.ProcedureSourceArgs, val.ProcedureSourceChildArgs);
		_instrument.ValidationMessageReported -= value;
		if (val.ProcedureSourceArgs.Contains("calibrantTime"))
		{
			_method.AdvancedSettings.CalibrantTime = (double)val.ProcedureSourceArgs["calibrantTime"].Value;
		}
		return val.Reports;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/advparamsettingsusercontrol.xaml", UriKind.Relative);
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
			advScroll = (ScrollViewer)target;
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
