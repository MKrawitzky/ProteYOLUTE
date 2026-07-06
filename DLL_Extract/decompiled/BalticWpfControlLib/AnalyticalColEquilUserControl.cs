// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
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

public class AnalyticalColEquilUserControl : UserControl, INotifyPropertyChanged, IComponentConnector
{
	public delegate void ValidationUpdate(bool isValid);

	public delegate void ModificationUpdate(bool isModified);

	private readonly bool _isPressurePSI;

	private readonly BalticInstrumentFacade _instrument;

	private bool _isValidateError;

	private bool _isValidatePressError;

	private bool _isValidateScaleError;

	private bool _isPressIllegalChar;

	private bool _isScaleIllegalChar;

	private ExperimentInfo _experiment;

	private string _pressureTolTip;

	internal Border bdrScale;

	internal DoubleTextBox txtScale;

	internal Border bdrPressure;

	internal DoubleTextBox txtPressure;

	internal Label lblPressUnits;

	internal Button btnRevert;

	private bool _contentLoaded;

	public double Scale
	{
		get
		{
			return Method.SeparationColumnEquil.Scale;
		}
		set
		{
			Method.SeparationColumnEquil.Scale = value;
			NotifyPropertyChanged("Scale");
		}
	}

	public double Pressure
	{
		get
		{
			if (!_isPressurePSI)
			{
				return Method.SeparationColumnEquil.Pressure;
			}
			return Method.SeparationColumnEquil.Pressure / 0.0689475729;
		}
		set
		{
			Method.SeparationColumnEquil.Pressure = (_isPressurePSI ? (value * 0.0689475729) : value);
			NotifyPropertyChanged("Pressure");
		}
	}

	public BindableBalticMethod Method { get; private set; }

	public double ColumnVolume => Method.SeparationColumnVolume;

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
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public AnalyticalColEquilUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
	{
		Method = method;
		_instrument = instrument;
		_experiment = experiment;
		InitializeComponent();
		base.DataContext = this;
		SetDefaultToolTip();
	}

	public AnalyticalColEquilUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, bool isPressurePSI, ExperimentInfo experiment)
	{
		Method = method;
		_instrument = instrument;
		_isPressurePSI = isPressurePSI;
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
				if (_experiment.Separator != null)
				{
					PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (_isPressurePSI ? 0.0689475729 : 1.0), _experiment.Separator.MaximumPressure / (_isPressurePSI ? 0.0689475729 : 1.0), _isPressurePSI ? "PSI" : "bar");
				}
				else
				{
					PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (_isPressurePSI ? 0.0689475729 : 1.0), _experiment.Separator.MaximumPressure / (_isPressurePSI ? 0.0689475729 : 1.0), _isPressurePSI ? "PSI" : "bar");
				}
			}
			else
			{
				PressureToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0:0.0} - {1:0.0} {2}", 50.0 / (_isPressurePSI ? 0.0689475729 : 1.0), _experiment.Separator.MaximumPressure / (_isPressurePSI ? 0.0689475729 : 1.0), _isPressurePSI ? "PSI" : "bar");
			}
		}
		catch (Exception)
		{
		}
	}

	public void RefreshParameters(ExperimentInfo experiment, BindableBalticMethod method)
	{
		_experiment = experiment;
		Method = method;
		Scale = Method.SeparationColumnEquil.Scale;
		Pressure = Method.SeparationColumnEquil.Pressure / (_isPressurePSI ? 0.0689475729 : 1.0);
		this.ModificationUpdateEvent?.Invoke((int)(Scale * 10.0) != (int)(Method.SeparationColumnEquil.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(Method.SeparationColumnEquil.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)));
		_isPressIllegalChar = false;
		_isScaleIllegalChar = false;
		this.ValidationUpdateEvent?.Invoke(isValid: true);
		ResetErrorConditions();
	}

	private void BlueDotTileButton_Click(object sender, RoutedEventArgs e)
	{
		Method.SeparationColumnEquil.RevertToDefault();
		Scale = Method.SeparationColumnEquil.Scale;
		Pressure = Method.SeparationColumnEquil.Pressure / (_isPressurePSI ? 0.0689475729 : 1.0);
		this.ModificationUpdateEvent?.Invoke((int)(Scale * 10.0) != (int)(Method.SeparationColumnEquil.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(Method.SeparationColumnEquil.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)));
		_isPressIllegalChar = false;
		_isScaleIllegalChar = false;
		this.ValidationUpdateEvent?.Invoke(isValid: true);
		ResetErrorConditions();
	}

	private void txtScale_MouseWheel(object sender, MouseWheelEventArgs e)
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

	private void txtScale_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ValidateParameters();
		this.ModificationUpdateEvent?.Invoke((int)(Scale * 10.0) != (int)(Method.SeparationColumnEquil.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(Method.SeparationColumnEquil.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)));
	}

	private void txtPressure_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ValidateParameters();
		this.ModificationUpdateEvent?.Invoke((int)(Scale * 10.0) != (int)(Method.SeparationColumnEquil.DefaultScale * 10.0) || (int)(Pressure * 10.0) != (int)(Method.SeparationColumnEquil.DefaultPressure * 10.0 / (_isPressurePSI ? 0.0689475729 : 1.0)));
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
		BalticMethod balticMethod = Method.ToBalticMethod();
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
			_isValidateError = (_isValidateScaleError = (_isValidatePressError = false));
			if (_experiment != null)
			{
				_instrument.ValidateMethodProcedureOffLine(_experiment, elutionProcedure, procedureArguments, childProcedureArguments);
				Method.SeparationColumnEquil.EquilTime = (double)procedureArguments["separator_equil_time"].Value;
				if (procedureArguments.Contains("calibrantTime"))
				{
					Method.AdvancedSettings.CalibrantTime = (double)procedureArguments["calibrantTime"].Value;
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
			if (subject == "separator_equilibration_pressure")
			{
				string pressureToolTip = message;
				if (_isPressurePSI && message.ToLower().Contains("value must be between"))
				{
					double num = 50.0;
					double num2 = 1000.0;
					try
					{
						string[] array = Regex.Split(message, "([-+]?[0-9]*\\.?[0-9]+)");
						int num3 = 0;
						if (array.Length > 1)
						{
							string[] array2 = array;
							for (int i = 0; i < array2.Length; i++)
							{
								if (double.TryParse(Convert.ToString(array2[i]), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var result))
								{
									switch (num3)
									{
									case 0:
										num = result;
										break;
									case 1:
										num2 = result;
										break;
									}
									num3++;
								}
							}
							pressureToolTip = string.Format(CultureInfo.InvariantCulture, "Value must be between {0:0.0} and {1:0.0}", num / 0.0689475729, num2 / 0.0689475729);
						}
					}
					catch (Exception)
					{
					}
				}
				bdrPressure.BorderBrush = Brushes.Red;
				PressureToolTip = pressureToolTip;
				_isValidatePressError = true;
			}
			else if (subject == "separator_equilibration_volumemultiplier")
			{
				bdrScale.BorderBrush = Brushes.Red;
				txtScale.ToolTip = message;
				_isValidateScaleError = true;
			}
			if (!_isValidateScaleError && !_isScaleIllegalChar)
			{
				bdrScale.BorderBrush = Brushes.Transparent;
				txtScale.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!_isValidatePressError && !_isPressIllegalChar)
			{
				bdrPressure.BorderBrush = Brushes.Transparent;
				SetDefaultToolTip();
			}
			this.ValidationUpdateEvent?.Invoke(!_isValidatePressError && !_isValidateScaleError);
		});
	}

	private void ResetErrorConditions()
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
		this.ValidationUpdateEvent?.Invoke(!_isPressIllegalChar && !_isScaleIllegalChar);
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
			else
			{
				_isScaleIllegalChar = true;
			}
		}
		else if (textBox == txtPressure)
		{
			_isPressIllegalChar = false;
		}
		else
		{
			_isScaleIllegalChar = false;
		}
		this.ValidationUpdateEvent?.Invoke(!_isPressIllegalChar && !_isScaleIllegalChar);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/analyticalcolequilusercontrol.xaml", UriKind.Relative);
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
			txtScale.MouseWheel += txtScale_MouseWheel;
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
			btnRevert = (Button)target;
			btnRevert.Click += BlueDotTileButton_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
