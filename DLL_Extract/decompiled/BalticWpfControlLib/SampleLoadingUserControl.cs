using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib;

public class SampleLoadingUserControl : UserControl, INotifyPropertyChanged, IComponentConnector
{
	public class ComboBoxItemInjMethod
	{
		public BalticInjectionType InjMethod { get; set; }

		public string ValueString { get; set; }
	}

	private delegate void SetErrorConditionCallback(string subject, string message);

	public delegate void ValidationUpdate(bool isValid);

	public delegate void ModificationUpdate(bool isModified);

	private bool _isPressurePSI;

	private bool _isValidateError;

	private bool _isValidatePressError;

	private bool _isValidateScaleError;

	private bool _isPressIllegalChar;

	private bool _isScaleIllegalChar;

	private bool _isValidatePenetrateError;

	private bool _isPenetrateIllegalChar;

	private List<ComboBoxItemInjMethod> _injMethodListEnum = new List<ComboBoxItemInjMethod>
	{
		new ComboBoxItemInjMethod
		{
			InjMethod = BalticInjectionType.PartialLoop,
			ValueString = "Partial Loop"
		},
		new ComboBoxItemInjMethod
		{
			InjMethod = BalticInjectionType.uLPickup,
			ValueString = "µL Pickup"
		}
	};

	public BindableBalticMethod _method;

	private BalticInstrumentFacade _instrument;

	private ExperimentInfo _experiment;

	private string _pressureTolTip;

	internal Border bdrScale;

	internal DoubleTextBox txtScale;

	internal Border bdrPressure;

	internal DoubleTextBox txtPressure;

	internal Label lblPressUnits;

	internal ComboBox comboInjMethod;

	internal CheckBox chkBottonSense;

	internal Border bdrPenetrate;

	internal DoubleTextBox txtPenetrateDepth;

	internal Label lblPenetrateDepthUnits;

	internal Button btnRevert;

	private bool _contentLoaded;

	public double Scale
	{
		get
		{
			return _method.SampleLoading.Scale;
		}
		set
		{
			_method.SampleLoading.Scale = value;
			NotifyPropertyChanged("Scale");
		}
	}

	public double Pressure
	{
		get
		{
			if (!_isPressurePSI)
			{
				return _method.SampleLoading.Pressure;
			}
			return _method.SampleLoading.Pressure / 0.0689475729;
		}
		set
		{
			_method.SampleLoading.Pressure = (_isPressurePSI ? (value * 0.0689475729) : value);
			NotifyPropertyChanged("Pressure");
		}
	}

	public BindableBalticMethod Method => _method;

	public double PenetrationDepth
	{
		get
		{
			return _method.SampleLoading.PenetrationDepth;
		}
		set
		{
			_method.SampleLoading.PenetrationDepth = value;
			NotifyPropertyChanged("PenetrationDepth");
		}
	}

	public double ColumnVolume => _method.SeparationColumnVolume;

	public bool IsBottomSense
	{
		get
		{
			return _method.SampleLoading.IsBottomSense;
		}
		set
		{
			_method.SampleLoading.IsBottomSense = value;
			NotifyPropertyChanged("IsBottomSense");
		}
	}

	public BalticInjectionType InjectionMethod
	{
		get
		{
			return _method.SampleLoading.InjectionMethod;
		}
		set
		{
			_method.SampleLoading.InjectionMethod = value;
			NotifyPropertyChanged("InjectionMethod");
		}
	}

	public List<ComboBoxItemInjMethod> InjMethodListEnum
	{
		get
		{
			return _injMethodListEnum;
		}
		set
		{
			_injMethodListEnum = value;
			NotifyPropertyChanged("InjMethodListEnum");
		}
	}

	public ExperimentInfo Experiment
	{
		get
		{
			return _experiment;
		}
		set
		{
			_experiment = value;
		}
	}

	public string PressureToolTip
	{
		get
		{
			return _pressureTolTip;
		}
		set
		{
			_pressureTolTip = value;
			NotifyPropertyChanged("PressureToolTip");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public event ValidationUpdate ValidationUpdateEvent;

	public event ModificationUpdate ModificationUpdateEvent;

	private void NotifyPropertyChanged(string propertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public SampleLoadingUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
	{
		InitializeComponent();
		_method = method;
		_instrument = instrument;
		_experiment = experiment;
		base.DataContext = this;
		SetDefaultToolTip();
	}

	public SampleLoadingUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, bool ispressurePSI, ExperimentInfo experiment)
	{
		_method = method;
		_instrument = instrument;
		_isPressurePSI = ispressurePSI;
		_experiment = experiment;
		InitializeComponent();
		base.DataContext = this;
		lblPressUnits.Content = (_isPressurePSI ? "PSI" : "bar");
		SetDefaultToolTip();
	}

	private void SetDefaultToolTip()
	{
		try
		{
			if (_experiment != null)
			{
				PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (_isPressurePSI ? 0.0689475729 : 1.0), _method.UsesTrapColumn ? (_experiment.Trap.MaximumPressure / (_isPressurePSI ? 0.0689475729 : 1.0)) : (_experiment.Separator.MaximumPressure / (_isPressurePSI ? 0.0689475729 : 1.0)), _isPressurePSI ? "PSI" : "bar");
			}
			else
			{
				PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (_isPressurePSI ? 0.0689475729 : 1.0), _experiment.Trap.MaximumPressure / (_isPressurePSI ? 0.0689475729 : 1.0), _isPressurePSI ? "PSI" : "bar");
			}
		}
		catch (Exception)
		{
		}
	}

	public void RefreshParameters(ExperimentInfo experiment, BindableBalticMethod method)
	{
		_experiment = experiment;
		_method = method;
		Pressure = _method.SampleLoading.Pressure / (_isPressurePSI ? 0.0689475729 : 1.0);
		Scale = _method.SampleLoading.Scale;
		IsBottomSense = _method.SampleLoading.IsBottomSense;
		InjectionMethod = _method.SampleLoading.InjectionMethod;
		if (this.ModificationUpdateEvent != null)
		{
			this.ModificationUpdateEvent((int)(Scale * 10.0) != (int)(_method.SampleLoading.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(_method.SampleLoading.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)) || IsBottomSense != _method.SampleLoading.DefaultIsBottomSense || InjectionMethod != _method.SampleLoading.DefaultInjectionMethod);
		}
		_isPressIllegalChar = false;
		_isScaleIllegalChar = false;
		_isPenetrateIllegalChar = false;
		this.ValidationUpdateEvent?.Invoke(isValid: true);
		ResetErrorConditions();
		SetDefaultToolTip();
	}

	private void BlueDotTileButton_Click(object sender, RoutedEventArgs e)
	{
		_method.SampleLoading.RevertToDefault();
		Pressure = _method.SampleLoading.Pressure / (_isPressurePSI ? 0.0689475729 : 1.0);
		Scale = _method.SampleLoading.Scale;
		IsBottomSense = _method.SampleLoading.IsBottomSense;
		InjectionMethod = _method.SampleLoading.InjectionMethod;
		PenetrationDepth = _method.SampleLoading.PenetrationDepth;
		if (this.ModificationUpdateEvent != null)
		{
			this.ModificationUpdateEvent((int)(Scale * 10.0) != (int)(_method.SampleLoading.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(_method.SampleLoading.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)) || IsBottomSense != _method.SampleLoading.DefaultIsBottomSense || InjectionMethod != _method.SampleLoading.DefaultInjectionMethod);
		}
		_isPressIllegalChar = false;
		_isScaleIllegalChar = false;
		_isPenetrateIllegalChar = false;
		this.ValidationUpdateEvent?.Invoke(isValid: true);
		ResetErrorConditions();
	}

	private void txtFactor_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta < 0)
		{
			Scale -= 1.0;
		}
		else
		{
			Scale += 1.0;
		}
		ValidateParameters();
	}

	private void txtPressure_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta < 0)
		{
			if (_isPressurePSI)
			{
				Pressure -= 10.0;
			}
			else
			{
				Pressure -= 0.1;
			}
		}
		else if (_isPressurePSI)
		{
			Pressure += (_isPressurePSI ? 10.0 : 0.1);
		}
		else
		{
			Pressure += 0.1;
		}
		ValidateParameters();
	}

	private void txtPenetrateDepth_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta < 0)
		{
			PenetrationDepth -= 0.1;
		}
		else
		{
			PenetrationDepth += 0.1;
		}
		ValidateParameters();
	}

	private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
	{
		if (Severity.Error.Equals(e.Severity))
		{
			_isValidateError = true;
			SetErrorCondition(e.Subject, e.Message);
		}
	}

	public void ValidateParameters()
	{
		if (_instrument == null)
		{
			return;
		}
		ResetErrorIndicators();
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
		_instrument.ValidationMessageReported += ValidationErrorHandler;
		try
		{
			_isValidateError = (_isValidateScaleError = (_isValidatePressError = (_isValidatePenetrateError = false)));
			if (_experiment != null)
			{
				_instrument.ValidateMethodProcedureOffLine(_experiment, elutionProcedure, procedureArguments, childProcedureArguments);
				if (procedureArguments.Contains("column_load_time"))
				{
					Method.SampleLoading.EquilTime = (double)procedureArguments["column_load_time"].Value;
				}
			}
			else
			{
				_instrument.ValidateProcedureOffLine(elutionProcedure, procedureArguments, childProcedureArguments);
			}
			if (!_isValidateError)
			{
				ResetErrorConditions();
			}
		}
		finally
		{
			_instrument.ValidationMessageReported -= ValidationErrorHandler;
		}
	}

	private void SetErrorCondition(string subject, string message)
	{
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			if (subject == "column_load_pressure")
			{
				bdrPressure.BorderBrush = Brushes.Red;
				PressureToolTip = message;
				_isValidatePressError = true;
			}
			else if (subject == "column_load_volumemultiplier")
			{
				bdrScale.BorderBrush = Brushes.Red;
				txtScale.ToolTip = message;
				_isValidateScaleError = true;
			}
			else if (subject == "penetration_depth")
			{
				bdrPenetrate.BorderBrush = Brushes.Red;
				txtPenetrateDepth.ToolTip = message;
				_isValidatePenetrateError = true;
			}
			if (!_isValidateScaleError && !_isScaleIllegalChar)
			{
				bdrScale.BorderBrush = Brushes.Transparent;
				txtScale.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!_isValidatePressError && !_isPressIllegalChar)
			{
				txtPressure.BorderBrush = Brushes.Transparent;
				SetDefaultToolTip();
			}
			if (!_isValidatePenetrateError && !_isPenetrateIllegalChar)
			{
				txtPenetrateDepth.BorderBrush = Brushes.Transparent;
				SetDefaultToolTip();
			}
			this.ValidationUpdateEvent?.Invoke(!_isValidatePressError && !_isValidateScaleError && !_isValidatePenetrateError);
		});
	}

	private void ResetErrorIndicators()
	{
		if (!_isScaleIllegalChar && txtScale != null)
		{
			bdrScale.BorderBrush = Brushes.Transparent;
			txtScale.ClearValue(FrameworkElement.ToolTipProperty);
		}
		if (!_isPressIllegalChar && txtPressure != null)
		{
			bdrPressure.BorderBrush = Brushes.Transparent;
			SetDefaultToolTip();
		}
		if (!_isPenetrateIllegalChar && txtPenetrateDepth != null)
		{
			bdrPenetrate.BorderBrush = Brushes.Transparent;
			SetDefaultToolTip();
		}
	}

	private void ResetErrorConditions()
	{
		ResetErrorIndicators();
		this.ValidationUpdateEvent?.Invoke(!_isValidatePressError && !_isValidateScaleError && !_isValidatePenetrateError);
	}

	private void Validation_Error(object sender, ValidationErrorEventArgs e)
	{
		TextBox textBox = (TextBox)sender;
		if (e.Action == ValidationErrorEventAction.Added)
		{
			if (textBox == txtPressure)
			{
				_isPressIllegalChar = true;
			}
			else if (textBox == txtScale)
			{
				_isScaleIllegalChar = true;
			}
			else
			{
				_isPenetrateIllegalChar = true;
			}
			if (textBox == txtPressure)
			{
				_isPressIllegalChar = true;
			}
			else
			{
				_isScaleIllegalChar = true;
			}
		}
		else if (textBox == txtPressure)
		{
			_isPressIllegalChar = false;
		}
		else if (textBox == txtScale)
		{
			_isScaleIllegalChar = false;
		}
		else
		{
			_isPenetrateIllegalChar = false;
		}
		this.ValidationUpdateEvent?.Invoke(!_isValidatePressError && !_isValidateScaleError && !_isValidatePenetrateError);
	}

	private void chkBottomSense_Click(object sender, RoutedEventArgs e)
	{
		if (this.ModificationUpdateEvent != null)
		{
			this.ModificationUpdateEvent((int)(Scale * 10.0) != (int)(_method.SampleLoading.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(_method.SampleLoading.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)) || IsBottomSense != _method.SampleLoading.DefaultIsBottomSense || InjectionMethod != _method.SampleLoading.DefaultInjectionMethod || (int)(PenetrationDepth * 10.0) != (int)(_method.SampleLoading.DefaultPenetrationDepth * 10.0));
		}
	}

	private void comboInjeMethod_Selchanged(object sender, SelectionChangedEventArgs e)
	{
		if (this.ModificationUpdateEvent != null)
		{
			this.ModificationUpdateEvent((int)(Scale * 10.0) != (int)(_method.SampleLoading.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(_method.SampleLoading.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)) || IsBottomSense != _method.SampleLoading.DefaultIsBottomSense || InjectionMethod != _method.SampleLoading.DefaultInjectionMethod || (int)(PenetrationDepth * 10.0) != (int)(_method.SampleLoading.DefaultPenetrationDepth * 10.0));
		}
	}

	private void txtScale_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ValidateParameters();
		if (this.ModificationUpdateEvent != null)
		{
			this.ModificationUpdateEvent((int)(Scale * 10.0) != (int)(_method.SampleLoading.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(_method.SampleLoading.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)) || IsBottomSense != _method.SampleLoading.DefaultIsBottomSense || InjectionMethod != _method.SampleLoading.DefaultInjectionMethod || (int)(PenetrationDepth * 10.0) != (int)(_method.SampleLoading.DefaultPenetrationDepth * 10.0));
		}
	}

	private void txtPressure_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ValidateParameters();
		if (this.ModificationUpdateEvent != null)
		{
			this.ModificationUpdateEvent((int)(Scale * 10.0) != (int)(_method.SampleLoading.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(_method.SampleLoading.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)) || IsBottomSense != _method.SampleLoading.DefaultIsBottomSense || InjectionMethod != _method.SampleLoading.DefaultInjectionMethod || (int)(PenetrationDepth * 10.0) != (int)(_method.SampleLoading.DefaultPenetrationDepth * 10.0));
		}
	}

	private void txtPenetrateDepth_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ValidateParameters();
		if (this.ModificationUpdateEvent != null)
		{
			this.ModificationUpdateEvent((int)(Scale * 10.0) != (int)(_method.SampleLoading.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(_method.SampleLoading.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)) || IsBottomSense != _method.SampleLoading.DefaultIsBottomSense || InjectionMethod != _method.SampleLoading.DefaultInjectionMethod || (int)(PenetrationDepth * 10.0) != (int)(_method.SampleLoading.DefaultPenetrationDepth * 10.0));
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/sampleloadingusercontrol.xaml", UriKind.Relative);
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
			bdrScale = (Border)target;
			break;
		case 2:
			txtScale = (DoubleTextBox)target;
			txtScale.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			txtScale.MouseWheel += txtFactor_MouseWheel;
			txtScale.ValueChanged += txtScale_ValueChanged;
			break;
		case 3:
			bdrPressure = (Border)target;
			break;
		case 4:
			txtPressure = (DoubleTextBox)target;
			txtPressure.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			txtPressure.MouseWheel += txtPressure_MouseWheel;
			txtPressure.ValueChanged += txtPressure_ValueChanged;
			break;
		case 5:
			lblPressUnits = (Label)target;
			break;
		case 6:
			comboInjMethod = (ComboBox)target;
			comboInjMethod.SelectionChanged += comboInjeMethod_Selchanged;
			break;
		case 7:
			chkBottonSense = (CheckBox)target;
			chkBottonSense.Click += chkBottomSense_Click;
			break;
		case 8:
			bdrPenetrate = (Border)target;
			break;
		case 9:
			txtPenetrateDepth = (DoubleTextBox)target;
			txtPenetrateDepth.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			txtPenetrateDepth.MouseWheel += txtPenetrateDepth_MouseWheel;
			txtPenetrateDepth.ValueChanged += txtPenetrateDepth_ValueChanged;
			break;
		case 10:
			lblPenetrateDepthUnits = (Label)target;
			break;
		case 11:
			btnRevert = (Button)target;
			btnRevert.Click += BlueDotTileButton_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
