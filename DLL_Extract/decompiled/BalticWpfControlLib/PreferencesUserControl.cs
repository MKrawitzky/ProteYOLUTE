using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib;

public class PreferencesUserControl : UserControl, INotifyPropertyChanged, IComponentConnector
{
	public delegate void ValidationUpdate(bool isValid);

	private readonly BalticPreferences _preferences;

	private readonly BalticInstrumentFacade _instrument;

	private readonly ExperimentInfo _experiment;

	private bool _isValidateError;

	private bool _isValidateStandbyFlowError;

	private bool _isValidateCompError;

	private bool _isStandbyFlowIllegalChar;

	private const bool _isCompIllegalChar = false;

	private bool _isValidateOvenTempError;

	private bool _isOvenTempIllegalChar;

	internal CheckBox cbViaTrap;

	internal DoubleTextBox txtStandby;

	internal IntegerTextBox txtComposition;

	internal DoubleTextBox txtTemp;

	internal Button btnRevert;

	private bool _contentLoaded;

	public bool IsSkipVialAndContinue
	{
		get
		{
			return _preferences.Autosampler.IsSkipVialAndContinue;
		}
		set
		{
			_preferences.Autosampler.IsSkipVialAndContinue = value;
			NotifyPropertyChanged("IsSkipVialAndContinue");
		}
	}

	public bool IsViaTrap
	{
		get
		{
			return _preferences.Pump.IsViaTrap;
		}
		set
		{
			_preferences.Pump.IsViaTrap = value;
			NotifyPropertyChanged("IsViaTrap");
		}
	}

	public bool IsIdleFlowOnError
	{
		get
		{
			return _preferences.Pump.IsIdleFlowOnError;
		}
		set
		{
			_preferences.Pump.IsIdleFlowOnError = value;
			NotifyPropertyChanged("IsIdleFlowOnError");
		}
	}

	public bool IsIdleFlowOnStandby
	{
		get
		{
			return _preferences.Pump.IsIdleFlowOnStandby;
		}
		set
		{
			_preferences.Pump.IsIdleFlowOnStandby = value;
			NotifyPropertyChanged("IsIdleFlowOnStandby");
		}
	}

	public double StandbyFlow
	{
		get
		{
			return _preferences.Pump.StandbyFlow;
		}
		set
		{
			_preferences.Pump.StandbyFlow = value;
			NotifyPropertyChanged("StandbyFlow");
		}
	}

	public int Composition
	{
		get
		{
			return _preferences.Pump.Composition;
		}
		set
		{
			_preferences.Pump.Composition = value;
			NotifyPropertyChanged("Composition");
		}
	}

	public double OvenTemperature
	{
		get
		{
			return _preferences.Oven.TemperatureSetPt;
		}
		set
		{
			_preferences.Oven.TemperatureSetPt = value;
			NotifyPropertyChanged("OvenTemperature");
		}
	}

	public bool IsSelfDiagnosticsEnabled
	{
		get
		{
			return _preferences.IsSelfDiagnosticsEnabled;
		}
		set
		{
			_preferences.IsSelfDiagnosticsEnabled = value;
			NotifyPropertyChanged("IsSelfDiagnosticsEnabled");
		}
	}

	public bool IsExtendedLoggingEnabled
	{
		get
		{
			return _preferences.IsExtendedLoggingEnabled;
		}
		set
		{
			_preferences.IsExtendedLoggingEnabled = value;
			NotifyPropertyChanged("IsExtendedLoggingEnabled");
		}
	}

	public bool IsPumpFirmwareLoggingEnabled
	{
		get
		{
			return _preferences.Pump.IsFirmwareLoggingEnabled;
		}
		set
		{
			_preferences.Pump.IsFirmwareLoggingEnabled = value;
			NotifyPropertyChanged("IsPumpFirmwareLoggingEnabled");
		}
	}

	public LedBrightness LedBrightness
	{
		get
		{
			return _preferences.LedBrightness;
		}
		set
		{
			_preferences.LedBrightness = value;
			NotifyPropertyChanged("LedBrightness");
		}
	}

	public List<LedBrightness> LedBrightnessValues { get; } = new List<LedBrightness>(3)
	{
		LedBrightness.Off,
		LedBrightness.Intermediate,
		LedBrightness.Full
	};


	public event PropertyChangedEventHandler PropertyChanged;

	public event ValidationUpdate ValidationUpdateEvent;

	private void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public PreferencesUserControl(BalticPreferences preferences, ExperimentInfo experiment, BalticInstrumentFacade instrument)
	{
		InitializeComponent();
		_preferences = preferences;
		_instrument = instrument;
		_experiment = experiment;
		base.DataContext = this;
	}

	private void txtStandby_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ValidateParameters();
	}

	private void StandbyFlowDoubleTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta < 0)
		{
			StandbyFlow -= 0.1;
		}
		else
		{
			StandbyFlow += 0.1;
		}
		ValidateParameters();
	}

	private void CompIntegerTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		ValidateParameters();
	}

	private void CompIntegerTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta < 0)
		{
			if (Composition - 1 >= 0)
			{
				Composition--;
			}
		}
		else if (Composition + 1 <= 100)
		{
			Composition++;
		}
		ValidateParameters();
	}

	private void txtTemp_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ValidateParameters();
	}

	private void OvenTempSetPtDoubleTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta < 0)
		{
			OvenTemperature -= 0.1;
		}
		else
		{
			OvenTemperature += 0.1;
		}
		ValidateParameters();
	}

	private void btnRevert_Click(object sender, RoutedEventArgs e)
	{
		ProcedureArguments procedureArguments = _instrument.GenerateIdleArguments(_experiment);
		BalticPreferences balticPreferences = new BalticPreferences();
		IsSkipVialAndContinue = balticPreferences.Autosampler.IsSkipVialAndContinue;
		IsViaTrap = (bool)procedureArguments["via_trap"].Value;
		IsIdleFlowOnError = balticPreferences.Pump.IsIdleFlowOnError;
		IsIdleFlowOnStandby = balticPreferences.Pump.IsIdleFlowOnStandby;
		IsSelfDiagnosticsEnabled = balticPreferences.IsSelfDiagnosticsEnabled;
		IsExtendedLoggingEnabled = balticPreferences.IsExtendedLoggingEnabled;
		StandbyFlow = (double)procedureArguments["idle_flow_rate"].Value;
		Composition = (int)(double)procedureArguments["composition"].Value;
		OvenTemperature = (double)procedureArguments["default_oven_temperature"].Value;
		LedBrightness = balticPreferences.LedBrightness;
	}

	private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
	{
		if (Severity.Error.Equals(e.Severity))
		{
			_isValidateError = true;
			SetErrorCondition(e.Subject, e.Message);
		}
	}

	private void ValidateParameters()
	{
		if (_instrument != null)
		{
			ProcedureArguments procedureArguments = _instrument.CreateIdleArguments();
			_isValidateError = (_isValidateStandbyFlowError = (_isValidateCompError = (_isValidateOvenTempError = false)));
			procedureArguments["composition"].Value = Composition;
			procedureArguments["idle_flow_rate"].Value = StandbyFlow;
			procedureArguments["default_oven_temperature"].Value = OvenTemperature;
			procedureArguments["default_oven_temperature"].Value = OvenTemperature;
			procedureArguments["via_trap"].Value = IsViaTrap;
			_instrument.ValidationMessageReported += ValidationErrorHandler;
			try
			{
				_instrument.ValidateMethodProcedureOffLine(_experiment, _instrument.GetIdleProcedure(), procedureArguments, new ChildProcedureArguments());
			}
			finally
			{
				_instrument.ValidationMessageReported -= ValidationErrorHandler;
			}
			if (!_isValidateError)
			{
				ResetErrorConditions();
			}
		}
	}

	private void SetErrorCondition(string subject, string message)
	{
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			if (subject == "composition")
			{
				txtComposition.BorderBrush = Brushes.Red;
				txtComposition.BorderThickness = new Thickness(2.0);
				txtComposition.ToolTip = message;
				_isValidateCompError = true;
			}
			if (subject == "idle_flow_rate")
			{
				txtStandby.BorderBrush = Brushes.Red;
				txtStandby.BorderThickness = new Thickness(2.0);
				txtStandby.ToolTip = message;
				_isValidateStandbyFlowError = true;
			}
			if (subject == "default_oven_temperature")
			{
				txtTemp.BorderBrush = Brushes.Red;
				txtTemp.BorderThickness = new Thickness(2.0);
				txtTemp.ToolTip = message;
				_isValidateOvenTempError = true;
			}
			if (!_isValidateCompError)
			{
				txtComposition.ClearValue(Control.BorderBrushProperty);
				txtComposition.ClearValue(Control.BorderThicknessProperty);
				txtComposition.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!_isValidateStandbyFlowError && !_isStandbyFlowIllegalChar)
			{
				txtStandby.ClearValue(Control.BorderBrushProperty);
				txtStandby.ClearValue(Control.BorderThicknessProperty);
				txtStandby.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!_isValidateOvenTempError && !_isOvenTempIllegalChar)
			{
				txtTemp.ClearValue(Control.BorderBrushProperty);
				txtTemp.ClearValue(Control.BorderThicknessProperty);
				txtTemp.ClearValue(FrameworkElement.ToolTipProperty);
			}
			this.ValidationUpdateEvent?.Invoke(!_isValidateCompError && !_isValidateStandbyFlowError && !_isStandbyFlowIllegalChar && !_isValidateOvenTempError && !_isOvenTempIllegalChar);
		});
	}

	private void ResetErrorConditions()
	{
		if (!_isStandbyFlowIllegalChar && !_isStandbyFlowIllegalChar)
		{
			txtStandby.ClearValue(Control.BorderBrushProperty);
			txtStandby.ClearValue(Control.BorderThicknessProperty);
			txtStandby.ClearValue(FrameworkElement.ToolTipProperty);
		}
		if (!_isValidateCompError)
		{
			txtComposition.ClearValue(Control.BorderBrushProperty);
			txtComposition.ClearValue(Control.BorderThicknessProperty);
			txtComposition.ClearValue(FrameworkElement.ToolTipProperty);
		}
		if (!_isOvenTempIllegalChar && !_isValidateOvenTempError)
		{
			txtTemp.ClearValue(Control.BorderBrushProperty);
			txtTemp.ClearValue(Control.BorderThicknessProperty);
			txtTemp.ClearValue(FrameworkElement.ToolTipProperty);
		}
		this.ValidationUpdateEvent?.Invoke(!_isValidateCompError && !_isValidateStandbyFlowError && !_isStandbyFlowIllegalChar && !_isValidateOvenTempError && !_isOvenTempIllegalChar);
	}

	private void Validation_Error(object sender, ValidationErrorEventArgs e)
	{
		TextBox textBox = (TextBox)sender;
		if (e.Action == ValidationErrorEventAction.Added)
		{
			if (textBox == txtStandby)
			{
				_isStandbyFlowIllegalChar = true;
			}
			else if (textBox == txtTemp)
			{
				_isOvenTempIllegalChar = true;
			}
		}
		else if (textBox == txtStandby)
		{
			_isStandbyFlowIllegalChar = false;
		}
		else if (textBox == txtTemp)
		{
			_isOvenTempIllegalChar = false;
		}
		this.ValidationUpdateEvent?.Invoke(!_isValidateCompError && !_isValidateStandbyFlowError && !_isStandbyFlowIllegalChar && !_isValidateOvenTempError && !_isOvenTempIllegalChar);
	}

	private void ViaTrapCheckBox_Click(object sender, RoutedEventArgs e)
	{
		ValidateParameters();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/preferencesusercontrol.xaml", UriKind.Relative);
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
			cbViaTrap = (CheckBox)target;
			cbViaTrap.Click += ViaTrapCheckBox_Click;
			break;
		case 2:
			txtStandby = (DoubleTextBox)target;
			txtStandby.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			txtStandby.ValueChanged += txtStandby_ValueChanged;
			txtStandby.MouseWheel += StandbyFlowDoubleTextBox_MouseWheel;
			break;
		case 3:
			txtComposition = (IntegerTextBox)target;
			txtComposition.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			txtComposition.TextChanged += CompIntegerTextBox_TextChanged;
			txtComposition.MouseWheel += CompIntegerTextBox_MouseWheel;
			break;
		case 4:
			txtTemp = (DoubleTextBox)target;
			txtTemp.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			txtTemp.ValueChanged += txtTemp_ValueChanged;
			txtTemp.MouseWheel += OvenTempSetPtDoubleTextBox_MouseWheel;
			break;
		case 5:
			btnRevert = (Button)target;
			btnRevert.Click += btnRevert_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
