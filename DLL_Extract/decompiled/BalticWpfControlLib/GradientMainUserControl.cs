// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using BalticWpfControlLib.Controls;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Syncfusion.UI.Xaml.Charts;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib;

public class GradientMainUserControl : UserControl, INotifyPropertyChanged, IComponentConnector, IStyleConnector
{
	private delegate void SetErrorConditionCallback(string subject, string message);

	public delegate void ValidationUpdate(bool isValid);

	public delegate void GetInitialMethodParam();

	public delegate void GenerateMethod(bool isKeepGradient = false, bool isKeepAdvancedSettings = false);

	public delegate void EnableMethodControls(bool isEnable, bool isReset);

	public delegate void TrapSelectionWarning(bool isShow, string message = "");

	private bool _isValidateError;

	private bool _isValidateTempError;

	private bool _isValidateTimeError;

	private bool _isTempIllegalChar;

	private bool _isTimeIllegalChar;

	private readonly bool _isOvenDetected;

	private readonly BalticInstrumentFacade _instrument;

	private readonly ColumnSelections _columnSelections;

	private ExperimentInfo _experiment;

	private readonly BalticColumnList _columns;

	private string _tempToolTip;

	private string _analysisTimeToolTip;

	private BindableBalticMethod.ElutionType _selectedElutionType;

	private bool _legacyMode;

	internal ComboBox comboExpType;

	internal TextBlock tbTrapColumn;

	internal DockPanel trapColPanel;

	internal ComboBox comboTrapColumn;

	internal ColumnInfoUserControl TrapColumnInfoControl;

	internal TextBlock tbSepColumn;

	internal DockPanel sepColPanel;

	internal ComboBox comboSepColumn;

	internal ColumnInfoUserControl SepColumnInfoControl;

	internal StackPanel stkOvenTemp;

	internal Border bdrTemperature;

	internal DoubleTextBox txtOvenTemp;

	internal TextBlock tbDegC;

	internal Image imgTempInfo;

	internal TextBlock tbAnalysisTime;

	internal StackPanel stkGradient;

	internal Border bdrAnalysisTime;

	internal DoubleTextBox tcGradientTime;

	internal TextBlock tbMin;

	internal Button btnGenerate;

	internal Button btnAdapt;

	internal Button btnReset;

	internal SfChart sfGradientChart;

	internal ChartZoomPanBehavior sfChartBehavior;

	internal NumericalAxis FlowAxis;

	private bool _contentLoaded;

	public BindableBalticMethod Method { get; }

	public string TempToolTip
	{
		get
		{
			return _tempToolTip;
		}
		set
		{
			_tempToolTip = value;
			NotifyPropertyChanged("TempToolTip");
		}
	}

	public string AnalysisTimeToolTip
	{
		get
		{
			return _analysisTimeToolTip;
		}
		set
		{
			_analysisTimeToolTip = value;
			NotifyPropertyChanged("AnalysisTimeToolTip");
		}
	}

	public ObservableCollection<BindableBalticMethod.ElutionType> ExperimentNames { get; } = new ObservableCollection<BindableBalticMethod.ElutionType>();


	public BindableBalticMethod.ElutionType SelectedElutionType
	{
		get
		{
			return _selectedElutionType;
		}
		set
		{
			if (_selectedElutionType != value)
			{
				_selectedElutionType = value;
				NotifyPropertyChanged("SelectedElutionType");
				BindableBalticMethod.ElutionType selectedElutionType = SelectedElutionType;
				if (selectedElutionType != null && !selectedElutionType.HasLegacyOption)
				{
					LegacyMode = false;
				}
				UpdateMethod();
			}
		}
	}

	public bool LegacyMode
	{
		get
		{
			return _legacyMode;
		}
		set
		{
			_legacyMode = value;
			NotifyPropertyChanged("LegacyMode");
			UpdateMethod();
		}
	}

	public bool IsLegacyModeSelectable
	{
		get
		{
			if (SelectedElutionType != null && SelectedElutionType.HasLegacyOption)
			{
				return !btnReset.IsVisible;
			}
			return false;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public event ValidationUpdate ValidationUpdateEvent;

	public event GetInitialMethodParam GetInitialMethodParamEvent;

	public event GenerateMethod GenerateMethodEvent;

	public event EnableMethodControls EnableMethodControlsEvent;

	public event TrapSelectionWarning TrapSelectionWarningEvent;

	private void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public GradientMainUserControl(BindableBalticMethod method, BalticInstrumentFacade instrument, BalticColumnList columns, ColumnSelections columnSelections, bool isOvenDetected)
	{
		Method = method;
		_instrument = instrument;
		_columns = columns;
		_isOvenDetected = isOvenDetected;
		foreach (BindableBalticMethod.ElutionType experimentType in Method.ExperimentTypes)
		{
			if (Method.ElutionName == experimentType.Name)
			{
				_selectedElutionType = experimentType;
				_legacyMode = experimentType.IsLegacy;
				ExperimentNames.Add(experimentType);
			}
			else if (!experimentType.IsLegacy)
			{
				ExperimentNames.Add(experimentType);
			}
		}
		_columnSelections = columnSelections;
		BalticColumnList balticColumnList = new BalticColumnList();
		BalticColumnList balticColumnList2 = new BalticColumnList();
		foreach (Column column in columns)
		{
			switch (column.Type)
			{
			case Column.ColumnType.AnalyticalColumn:
				balticColumnList.Add(column);
				break;
			case Column.ColumnType.PreColumn:
				balticColumnList2.Add(column);
				break;
			case Column.ColumnType.Both:
				balticColumnList.Add(column);
				balticColumnList2.Add(column);
				break;
			}
		}
		InitializeComponent();
		base.DataContext = this;
		comboTrapColumn.ItemsSource = balticColumnList2;
		comboTrapColumn.DisplayMemberPath = "Name";
		comboTrapColumn.SelectedValuePath = "Name";
		comboSepColumn.ItemsSource = balticColumnList;
		comboSepColumn.DisplayMemberPath = "Name";
		comboSepColumn.SelectedValuePath = "Name";
		BindableBalticMethod method2 = Method;
		if (method2.TrapColumnName == null)
		{
			string text = (method2.TrapColumnName = _columnSelections.PreColumnName);
		}
		method2 = Method;
		if (method2.SeparationColumnName == null)
		{
			string text = (method2.SeparationColumnName = _columnSelections.AnalyticalColumnName);
		}
		if (balticColumnList2.Contains(Method.TrapColumnName))
		{
			comboTrapColumn.SelectedValue = Method.TrapColumnName;
		}
		comboTrapColumn.Text = Method.TrapColumnName;
		if (balticColumnList.Contains(Method.SeparationColumnName))
		{
			comboSepColumn.SelectedValue = Method.SeparationColumnName;
		}
		comboSepColumn.Text = Method.SeparationColumnName;
		TrapColumnInfoControl.DataContext = (Column)comboTrapColumn.SelectedItem;
		SepColumnInfoControl.DataContext = (Column)comboSepColumn.SelectedItem;
		if (Method.ElutionName != null)
		{
			UpdateExperimentInfo();
		}
		if (Method.UsesTrapColumn && Method.TrapColumnName == _instrument.BalticConfiguration.NoColumn.Name)
		{
			this.TrapSelectionWarningEvent?.Invoke(isShow: true, "\"None\" trap column selection is not recommended !");
		}
	}

	private void UpdateMethod()
	{
		if (SelectedElutionType == null)
		{
			Method.ElutionName = null;
			return;
		}
		Method.ElutionName = (LegacyMode ? SelectedElutionType.LegacyName : SelectedElutionType.Name);
		this.GetInitialMethodParamEvent?.Invoke();
		UpdateConfiguration(Method);
		Method.TrapColumnName = _columnSelections.PreColumnName;
		Method.SeparationColumnName = _columnSelections.AnalyticalColumnName;
		comboTrapColumn.SelectedValue = Method.TrapColumnName;
		comboSepColumn.SelectedValue = Method.SeparationColumnName;
		CheckGenerateEnable();
		SetDefaultToolTip();
		UpdateTrapWarning();
	}

	private void UpdateTrapWarning()
	{
		if (Method.UsesTrapColumn)
		{
			if (Method.TrapColumnName == _instrument.BalticConfiguration.NoColumn.Name)
			{
				this.TrapSelectionWarningEvent?.Invoke(isShow: true, "\"None\" trap column selection is not recommended !");
			}
			else
			{
				this.TrapSelectionWarningEvent?.Invoke(isShow: false);
			}
		}
		else
		{
			this.TrapSelectionWarningEvent?.Invoke(isShow: false);
		}
	}

	private void UpdateExperimentInfo()
	{
		_experiment = new ExperimentInfo
		{
			ElutionName = Method.ElutionName,
			AnalysisTime = TimeSpan.FromMinutes(Method.GradientTime),
			OvenTemperature = Method.OvenTemperature,
			AppKey = _instrument.AppKey
		};
		if (Method.UsesTrapColumn)
		{
			Column column = _columns.Find((Column item) => item.Name == Method.TrapColumnName);
			_experiment.Trap = new ColumnAdapter(column);
		}
		else
		{
			Column column2 = _columns.Find((Column item) => item.Name == Method.TrapColumnName) ?? _columns.Find((Column item) => item.Name == _instrument.BalticConfiguration.NoColumn.Name);
			_experiment.Trap = new ColumnAdapter(column2);
		}
		if (Method.UsesSepColumn)
		{
			Column column3 = _columns.Find((Column item) => item.Name == Method.SeparationColumnName);
			_experiment.Separator = new ColumnAdapter(column3);
		}
		else
		{
			Column column4 = _columns.Find((Column item) => item.Name == Method.SeparationColumnName) ?? _columns.Find((Column item) => item.Name == _instrument.BalticConfiguration.NoColumn.Name);
			_experiment.Separator = new ColumnAdapter(column4);
		}
		UpdateTrapWarning();
	}

	private void UpdateConfiguration(BindableBalticMethod method, bool isInitialize = false)
	{
		if (method.ElutionName == null)
		{
			tbAnalysisTime.Visibility = Visibility.Hidden;
			tcGradientTime.Visibility = Visibility.Hidden;
			tbMin.Visibility = Visibility.Hidden;
			stkOvenTemp.Visibility = Visibility.Hidden;
			txtOvenTemp.Visibility = Visibility.Hidden;
			tbDegC.Visibility = Visibility.Hidden;
			imgTempInfo.Visibility = Visibility.Hidden;
			stkGradient.Visibility = Visibility.Hidden;
			btnGenerate.Visibility = Visibility.Hidden;
			btnAdapt.Visibility = Visibility.Hidden;
			btnReset.Visibility = Visibility.Hidden;
			tbTrapColumn.Visibility = Visibility.Hidden;
			comboTrapColumn.Visibility = Visibility.Hidden;
			tbSepColumn.Visibility = Visibility.Hidden;
			comboSepColumn.Visibility = Visibility.Hidden;
			btnAdapt.IsEnabled = false;
			comboTrapColumn.IsEnabled = true;
			comboTrapColumn.IsEditable = false;
			comboSepColumn.IsEnabled = true;
			comboSepColumn.IsEditable = false;
			this.EnableMethodControlsEvent?.Invoke(isEnable: false, isReset: false);
		}
		else
		{
			tbTrapColumn.Visibility = ((!method.UsesTrapColumn) ? Visibility.Hidden : Visibility.Visible);
			comboTrapColumn.Visibility = ((!method.UsesTrapColumn) ? Visibility.Hidden : Visibility.Visible);
			tbSepColumn.Visibility = ((!method.UsesSepColumn) ? Visibility.Hidden : Visibility.Visible);
			comboSepColumn.Visibility = ((!method.UsesSepColumn) ? Visibility.Hidden : Visibility.Visible);
			tbAnalysisTime.Visibility = (isInitialize ? Visibility.Hidden : Visibility.Visible);
			tcGradientTime.Visibility = (isInitialize ? Visibility.Hidden : Visibility.Visible);
			tbMin.Visibility = (isInitialize ? Visibility.Hidden : Visibility.Visible);
			stkOvenTemp.Visibility = Visibility.Visible;
			imgTempInfo.Visibility = (_isOvenDetected ? Visibility.Hidden : Visibility.Visible);
			txtOvenTemp.Visibility = Visibility.Visible;
			tbDegC.Visibility = Visibility.Visible;
			stkGradient.Visibility = Visibility.Visible;
			btnGenerate.Visibility = (isInitialize ? Visibility.Hidden : Visibility.Visible);
			btnAdapt.Visibility = ((!isInitialize) ? Visibility.Hidden : Visibility.Visible);
			btnReset.Visibility = ((!isInitialize) ? Visibility.Hidden : Visibility.Visible);
			comboExpType.IsEnabled = !isInitialize;
			comboTrapColumn.IsEnabled = true;
			comboTrapColumn.IsEditable = false;
			comboSepColumn.IsEnabled = true;
			comboSepColumn.IsEditable = false;
			btnAdapt.IsEnabled = false;
			this.EnableMethodControlsEvent?.Invoke(isInitialize, isReset: false);
			CheckGenerateEnable();
		}
		NotifyPropertyChanged("IsLegacyModeSelectable");
		if (Method.UsesTrapColumn)
		{
			if (Method.TrapColumnName == _instrument.BalticConfiguration.NoColumn.Name)
			{
				this.TrapSelectionWarningEvent?.Invoke(isShow: true, "\"None\" trap column selection is not recommended !");
			}
			else
			{
				this.TrapSelectionWarningEvent?.Invoke(isShow: false);
			}
		}
		else
		{
			this.TrapSelectionWarningEvent?.Invoke(isShow: false);
		}
	}

	private void GradientMainUserControl_Loaded(object sender, RoutedEventArgs e)
	{
		sfGradientChart.Series[0].ItemsSource = Method.GradientTable;
		sfGradientChart.Series[1].ItemsSource = Method.GradientTable;
		UpdateConfiguration(Method, isInitialize: true);
	}

	private void PreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboTrapColumn.SelectedValue != null)
		{
			if (Method.TrapColumnName != (string)comboTrapColumn.SelectedValue && !btnGenerate.IsVisible)
			{
				btnAdapt.IsEnabled = true;
			}
			Method.TrapColumnName = (string)comboTrapColumn.SelectedValue;
			TrapColumnInfoControl.DataContext = _columns.Find((Column item) => item.Name == Method.TrapColumnName);
			SendValidationUpdateEvent();
			if (Method.UsesTrapColumn)
			{
				if (Method.TrapColumnName == _instrument.BalticConfiguration.NoColumn.Name)
				{
					this.TrapSelectionWarningEvent?.Invoke(isShow: true, "\"None\" trap column selection is not recommended !");
				}
				else
				{
					this.TrapSelectionWarningEvent?.Invoke(isShow: false);
				}
			}
			else
			{
				this.TrapSelectionWarningEvent?.Invoke(isShow: false);
			}
		}
		CheckGenerateEnable();
	}

	private void AnalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (comboSepColumn.SelectedValue != null)
		{
			if (Method.SeparationColumnName != (string)comboSepColumn.SelectedValue && !btnGenerate.IsVisible)
			{
				btnAdapt.IsEnabled = true;
			}
			Method.SeparationColumnName = (string)comboSepColumn.SelectedValue;
			SepColumnInfoControl.DataContext = _columns.Find((Column item) => item.Name == Method.SeparationColumnName);
			SendValidationUpdateEvent();
		}
		CheckGenerateEnable();
	}

	private void CheckGenerateEnable()
	{
		if (btnGenerate != null)
		{
			UpdateExperimentInfo();
			bool isEnabled = true;
			if (Method.UsesTrapColumn && Method.TrapColumnName == null)
			{
				isEnabled = false;
				comboTrapColumn.SelectedValue = null;
			}
			if (Method.UsesSepColumn && Method.SeparationColumnName == null)
			{
				isEnabled = false;
				comboSepColumn.SelectedValue = null;
			}
			if (_isValidateError || _isValidateTimeError || _isTimeIllegalChar)
			{
				isEnabled = false;
			}
			btnGenerate.IsEnabled = isEnabled;
		}
	}

	private void GradientTime_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (e.Delta < 0)
		{
			if (Method.GradientTime - 1.0 >= 0.0)
			{
				Method.GradientTime -= 1.0;
			}
		}
		else if (Method.GradientTime + 1.0 <= 999.0)
		{
			Method.GradientTime += 1.0;
		}
	}

	private void tcGradientTime_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (Method.ElutionName != null && Method.SeparationColumnName != null)
		{
			double gradientTime = Method.GradientTime;
			if ((gradientTime < 0.0 || gradientTime > 999.0) ? true : false)
			{
				SetErrorCondition("analysis_time", "Value must be between 0 and 999");
				return;
			}
			_isValidateTimeError = false;
			ResetErrorConditions();
		}
	}

	private void FlowAxis_ActualRangeChanged(object sender, ActualRangeChangedEventArgs e)
	{
		double num = 0.0;
		foreach (BalticGradientItem item in Method.GradientTable)
		{
			if (item.Flow > num)
			{
				num = item.Flow;
			}
		}
		e.ActualMaximum = ((num * 2.0 < 1.0) ? 1.0 : (num * 2.0));
	}

	public void UpdateFromGradientTable(WPFConstants.UpdateType updateType)
	{
		if (updateType == WPFConstants.UpdateType.AddRemove)
		{
			this.EnableMethodControlsEvent?.Invoke(isEnable: true, isReset: false);
		}
	}

	private void btnReset_Click(object sender, RoutedEventArgs e)
	{
		comboExpType.IsEnabled = true;
		btnAdapt.IsEnabled = false;
		BindableBalticMethod.ElutionType[] array = ExperimentNames.Where((BindableBalticMethod.ElutionType exp) => exp.IsLegacy).ToArray();
		foreach (BindableBalticMethod.ElutionType item in array)
		{
			ExperimentNames.Remove(item);
		}
		Method.Reset(_instrument.BalticConfiguration.Settings.ColumnOvenMinTemperature);
		UpdateConfiguration(Method, isInitialize: true);
		this.EnableMethodControlsEvent?.Invoke(isEnable: true, isReset: true);
		_isValidateTempError = (_isValidateTimeError = (_isTempIllegalChar = (_isTimeIllegalChar = (_isValidateError = false))));
		btnGenerate.IsEnabled = true;
		SelectedElutionType = null;
		LegacyMode = false;
		ResetErrorConditions();
		NotifyPropertyChanged("IsLegacyModeSelectable");
	}

	private void btnGenerate_Click(object sender, RoutedEventArgs e)
	{
		GenerateOrAdaptMethod(keepGradient: false, keepAdvancedSettings: false);
		SendValidationUpdateEvent();
	}

	private void btnAdapt_Click(object sender, RoutedEventArgs e)
	{
		GenerateOrAdaptMethod(keepGradient: true, keepAdvancedSettings: true);
		SendValidationUpdateEvent();
	}

	private void GenerateOrAdaptMethod(bool keepGradient, bool keepAdvancedSettings)
	{
		this.GenerateMethodEvent?.Invoke(keepGradient, keepAdvancedSettings);
		UpdateConfiguration(Method);
		btnGenerate.Visibility = Visibility.Hidden;
		btnAdapt.Visibility = Visibility.Visible;
		btnReset.Visibility = Visibility.Visible;
		tbAnalysisTime.Visibility = Visibility.Hidden;
		tcGradientTime.Visibility = Visibility.Hidden;
		stkOvenTemp.Visibility = Visibility.Visible;
		txtOvenTemp.Visibility = Visibility.Visible;
		tbDegC.Visibility = Visibility.Visible;
		imgTempInfo.Visibility = (_isOvenDetected ? Visibility.Hidden : Visibility.Visible);
		tbMin.Visibility = Visibility.Hidden;
		comboExpType.IsEnabled = false;
		comboTrapColumn.IsEnabled = true;
		comboSepColumn.IsEnabled = true;
		btnAdapt.IsEnabled = false;
		NotifyPropertyChanged("IsLegacyModeSelectable");
		this.EnableMethodControlsEvent?.Invoke(isEnable: true, isReset: false);
		ValidateParameters();
	}

	private void txtOvenTemp_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ValidateParameters();
	}

	private void ValidateParameters()
	{
		if (_instrument == null || Method.ElutionName == null || Method.SeparationColumnName == null)
		{
			return;
		}
		BalticMethod balticMethod = Method.ToBalticMethod(_columns);
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
			_isValidateError = (_isValidateTempError = false);
			if (_experiment != null)
			{
				_instrument.ValidateMethodProcedureOffLine(_experiment, elutionProcedure, procedureArguments, childProcedureArguments);
				if (procedureArguments.Contains("calibrantTime"))
				{
					Method.AdvancedSettings.CalibrantTime = (double)procedureArguments["calibrantTime"].Value;
				}
				if (procedureArguments.Contains("column_load_time"))
				{
					Method.SampleLoading.EquilTime = (double)procedureArguments["column_load_time"].Value;
				}
				if (procedureArguments.Contains("separator_equil_time"))
				{
					Method.SeparationColumnEquil.EquilTime = (double)procedureArguments["separator_equil_time"].Value;
				}
				if (procedureArguments.Contains("trap_equil_time"))
				{
					Method.TrapColumnEquil.EquilTime = (double)procedureArguments["trap_equil_time"].Value;
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
		CheckGenerateEnable();
	}

	private void ValidationErrorHandler(object sender, ProcedureReportEventArgs e)
	{
		if (Severity.Error.Equals(e.Severity) && e.Subject.Equals("oven_temperature"))
		{
			_isValidateError = true;
			SetErrorCondition(e.Subject, e.Message);
		}
	}

	private void SetErrorCondition(string subject, string message)
	{
		if (txtOvenTemp.Dispatcher.CheckAccess())
		{
			if (subject == "oven_temperature")
			{
				bdrTemperature.BorderBrush = Brushes.Red;
				TempToolTip = message;
				_isValidateTempError = true;
				if (btnGenerate.Visibility == Visibility.Visible)
				{
					btnGenerate.IsEnabled = false;
				}
			}
			else if (subject == "analysis_time")
			{
				bdrAnalysisTime.BorderBrush = Brushes.Red;
				AnalysisTimeToolTip = message;
				_isValidateTimeError = true;
				if (btnGenerate.Visibility == Visibility.Visible)
				{
					btnGenerate.IsEnabled = false;
				}
			}
			if (!_isValidateTempError && !_isTempIllegalChar)
			{
				bdrTemperature.BorderBrush = Brushes.Transparent;
				txtOvenTemp.ClearValue(FrameworkElement.ToolTipProperty);
				SetDefaultToolTip();
			}
			if (!_isValidateTimeError && !_isTimeIllegalChar)
			{
				bdrAnalysisTime.BorderBrush = Brushes.Transparent;
				tcGradientTime.ClearValue(FrameworkElement.ToolTipProperty);
				SetDefaultToolTip();
				if (btnGenerate.Visibility == Visibility.Visible)
				{
					btnGenerate.IsEnabled = true;
				}
			}
			SendValidationUpdateEvent();
		}
		else
		{
			SetErrorConditionCallback method = SetErrorCondition;
			base.Dispatcher.BeginInvoke(method, subject, message);
		}
	}

	private void ResetErrorConditions()
	{
		if (btnAdapt != null)
		{
			if (!_isValidateTempError)
			{
				bdrTemperature.BorderBrush = Brushes.Transparent;
				txtOvenTemp.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!_isValidateTimeError)
			{
				bdrAnalysisTime.BorderBrush = Brushes.Transparent;
				tcGradientTime.ClearValue(FrameworkElement.ToolTipProperty);
			}
			if (!_isValidateTimeError && !_isValidateTempError && btnGenerate.Visibility == Visibility.Visible)
			{
				btnGenerate.IsEnabled = true;
			}
			SendValidationUpdateEvent();
		}
	}

	private void SendValidationUpdateEvent()
	{
		this.ValidationUpdateEvent?.Invoke(!_isValidateTempError && !_isValidateTimeError && !btnAdapt.IsEnabled && !btnGenerate.IsVisible && SelectedElutionType != null && IsColumnsValid());
	}

	private void Validation_Error(object sender, ValidationErrorEventArgs e)
	{
		TextBox textBox = (TextBox)sender;
		if (e.Action == ValidationErrorEventAction.Added)
		{
			if (textBox == txtOvenTemp)
			{
				_isTempIllegalChar = true;
			}
			if (textBox == tcGradientTime)
			{
				_isTimeIllegalChar = true;
			}
		}
		else
		{
			if (textBox == txtOvenTemp)
			{
				_isTempIllegalChar = false;
			}
			if (textBox == tcGradientTime)
			{
				_isTimeIllegalChar = false;
			}
		}
		SendValidationUpdateEvent();
	}

	private void SetDefaultToolTip()
	{
		try
		{
			if (_experiment != null && Method.SeparationColumnName != null)
			{
				TempToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0} - {1} {2}C", _instrument.BalticConfiguration.Settings.ColumnOvenMinTemperature, _experiment.Separator.MaximumTemperature, "°");
			}
			else
			{
				TempToolTip = string.Format(CultureInfo.InvariantCulture, "Values between {0} - {1} {2}C", _instrument.BalticConfiguration.Settings.ColumnOvenMinTemperature, _instrument.BalticConfiguration.Settings.ColumnOvenMaxTemperature, "°");
			}
		}
		catch (Exception)
		{
		}
	}

	private bool IsColumnsValid()
	{
		if (Method.UsesSepColumn && !_columns.Contains(Method.SeparationColumnName))
		{
			return false;
		}
		if (Method.UsesTrapColumn && !_columns.Contains(Method.TrapColumnName))
		{
			return false;
		}
		return true;
	}

	private void TrapColumnItemLoaded(object sender, RoutedEventArgs e)
	{
		RegisterMouseOver(sender, TrapColumnOnIsMouseOver);
	}

	private void TrapColumnItemUnloaded(object sender, RoutedEventArgs e)
	{
		UnregisterMouseOver(sender, TrapColumnOnIsMouseOver);
	}

	private void SepColumnItemLoaded(object sender, RoutedEventArgs e)
	{
		RegisterMouseOver(sender, SepColumnOnIsMouseOver);
	}

	private void SepColumnItemUnloaded(object sender, RoutedEventArgs e)
	{
		UnregisterMouseOver(sender, SepColumnOnIsMouseOver);
	}

	private static void RegisterMouseOver(object sender, EventHandler handler)
	{
		if (sender is ComboBoxItem component)
		{
			DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(ComboBoxItem))?.AddValueChanged(component, handler);
		}
	}

	private static void UnregisterMouseOver(object sender, EventHandler handler)
	{
		if (sender is ComboBoxItem component)
		{
			DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(ComboBoxItem))?.RemoveValueChanged(component, handler);
		}
	}

	private void SepColumnOnIsMouseOver(object sender, EventArgs e)
	{
		if (sender is ComboBoxItem { IsMouseOver: not false } comboBoxItem)
		{
			SepColumnInfoControl.DataContext = comboBoxItem.DataContext;
		}
	}

	private void TrapColumnOnIsMouseOver(object sender, EventArgs e)
	{
		if (sender is ComboBoxItem { IsMouseOver: not false } comboBoxItem)
		{
			TrapColumnInfoControl.DataContext = comboBoxItem.DataContext;
		}
	}

	private void On_LabelCreated(object sender, LabelCreatedEventArgs e)
	{
		if (double.TryParse(e.AxisLabel.LabelContent.ToString(), out var result))
		{
			e.AxisLabel.LabelContent = result.ToString(CultureInfo.InvariantCulture);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/gradientmainusercontrol.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			((GradientMainUserControl)target).Loaded += GradientMainUserControl_Loaded;
			break;
		case 2:
			comboExpType = (ComboBox)target;
			break;
		case 3:
			tbTrapColumn = (TextBlock)target;
			break;
		case 4:
			trapColPanel = (DockPanel)target;
			break;
		case 5:
			comboTrapColumn = (ComboBox)target;
			comboTrapColumn.SelectionChanged += PreComboBox_SelectionChanged;
			break;
		case 7:
			TrapColumnInfoControl = (ColumnInfoUserControl)target;
			break;
		case 8:
			tbSepColumn = (TextBlock)target;
			break;
		case 9:
			sepColPanel = (DockPanel)target;
			break;
		case 10:
			comboSepColumn = (ComboBox)target;
			comboSepColumn.SelectionChanged += AnalComboBox_SelectionChanged;
			break;
		case 12:
			SepColumnInfoControl = (ColumnInfoUserControl)target;
			break;
		case 13:
			stkOvenTemp = (StackPanel)target;
			break;
		case 14:
			bdrTemperature = (Border)target;
			break;
		case 15:
			txtOvenTemp = (DoubleTextBox)target;
			txtOvenTemp.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			txtOvenTemp.ValueChanged += txtOvenTemp_ValueChanged;
			break;
		case 16:
			tbDegC = (TextBlock)target;
			break;
		case 17:
			imgTempInfo = (Image)target;
			break;
		case 18:
			tbAnalysisTime = (TextBlock)target;
			break;
		case 19:
			stkGradient = (StackPanel)target;
			break;
		case 20:
			bdrAnalysisTime = (Border)target;
			break;
		case 21:
			tcGradientTime = (DoubleTextBox)target;
			tcGradientTime.MouseWheel += GradientTime_MouseWheel;
			tcGradientTime.ValueChanged += tcGradientTime_ValueChanged;
			break;
		case 22:
			tbMin = (TextBlock)target;
			break;
		case 23:
			btnGenerate = (Button)target;
			btnGenerate.Click += btnGenerate_Click;
			break;
		case 24:
			btnAdapt = (Button)target;
			btnAdapt.Click += btnAdapt_Click;
			break;
		case 25:
			btnReset = (Button)target;
			btnReset.Click += btnReset_Click;
			break;
		case 26:
			sfGradientChart = (SfChart)target;
			break;
		case 27:
			sfChartBehavior = (ChartZoomPanBehavior)target;
			break;
		case 28:
			((NumericalAxis)target).LabelCreated += On_LabelCreated;
			break;
		case 29:
			((NumericalAxis)target).LabelCreated += On_LabelCreated;
			break;
		case 30:
			FlowAxis = (NumericalAxis)target;
			FlowAxis.ActualRangeChanged += FlowAxis_ActualRangeChanged;
			FlowAxis.LabelCreated += On_LabelCreated;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 6:
		{
			EventSetter eventSetter = new EventSetter();
			eventSetter.Event = FrameworkElement.Loaded;
			eventSetter.Handler = new RoutedEventHandler(TrapColumnItemLoaded);
			((Style)target).Setters.Add(eventSetter);
			eventSetter = new EventSetter();
			eventSetter.Event = FrameworkElement.Unloaded;
			eventSetter.Handler = new RoutedEventHandler(TrapColumnItemUnloaded);
			((Style)target).Setters.Add(eventSetter);
			break;
		}
		case 11:
		{
			EventSetter eventSetter = new EventSetter();
			eventSetter.Event = FrameworkElement.Loaded;
			eventSetter.Handler = new RoutedEventHandler(SepColumnItemLoaded);
			((Style)target).Setters.Add(eventSetter);
			eventSetter = new EventSetter();
			eventSetter.Event = FrameworkElement.Unloaded;
			eventSetter.Handler = new RoutedEventHandler(SepColumnItemUnloaded);
			((Style)target).Setters.Add(eventSetter);
			break;
		}
		}
	}
}
