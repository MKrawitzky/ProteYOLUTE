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
using System.Windows.Media;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class MethodUserControl : UserControl, IComponentConnector
{
	public delegate void ValidateInputUpdate(bool isValid);

	public delegate void EnableMethodComplete(bool isEnabled);

	public delegate void TrapSelectionWarning(bool isShow, string message = "");

	private const int _heightCollapsed = 505;

	private const int _heightExpanded = 705;

	private bool _methodStateEnabled;

	private readonly bool _isPressurePSI;

	private readonly bool _isOvenDetected;

	private bool _isTrapColEquilValid = true;

	private bool _isAnalColEquilValid = true;

	private bool _isSampleLoadValid = true;

	private bool _isGradTableValid = true;

	private bool _isGradMainValid = true;

	private bool _isAdvSettingsValid = true;

	private readonly bool _isService;

	private readonly BindableBalticMethod _method;

	private readonly BalticInstrumentFacade _facade;

	private Expander _expSettings;

	private GradientMainUserControl _ucGradMain;

	private GradientTableUserControl _ucGradTable;

	private readonly RadioButton _btnTrapColEquil = new RadioButton();

	private readonly RadioButton _btnSepColEquil = new RadioButton();

	private readonly RadioButton _btnSampleLoad = new RadioButton();

	private readonly RadioButton _btnAdvancedSett = new RadioButton();

	private TextBlock _tbTrapModified;

	private TextBlock _tbSepModified;

	private TextBlock _tbLoadModified;

	private TextBlock _tbAdvModified;

	private TextBlock _tbModifiedAll;

	private Image _imgTrapErrorNotify;

	private Image _imgSepErrorNotify;

	private Image _imgLoadErrorNotify;

	private Image _imgAdvErrorNotify;

	private Image _imgAllErrorNotify;

	private UserControl _activeUc;

	private RadioButton _activeBtn;

	private Grid _expGrid;

	private double _ucWidth;

	private double _ucHeight;

	private readonly Window _owner;

	private ExperimentInfo _experiment;

	private readonly BalticColumnList _columns;

	private readonly ColumnSelections _columnSelections;

	private readonly ResourceDictionary _myResDictionary = new ResourceDictionary();

	internal Canvas cvMethodEditor;

	private bool _contentLoaded;

	private bool IsMethodDataValid
	{
		get
		{
			if (_isTrapColEquilValid && _isAnalColEquilValid && _isSampleLoadValid && _isGradTableValid && _isGradMainValid)
			{
				return _isAdvSettingsValid;
			}
			return false;
		}
	}

	private bool IsOverrideSettingsValid
	{
		get
		{
			if (_isTrapColEquilValid && _isAnalColEquilValid && _isSampleLoadValid)
			{
				return _isAdvSettingsValid;
			}
			return false;
		}
	}

	public event ValidateInputUpdate ValidateInputUpdateEvent;

	public event EnableMethodComplete EnableMethodCompleteEvent;

	public event TrapSelectionWarning TrapSelectionWarningEvent;

	public MethodUserControl(Window owner, BalticMethod method, BalticInstrumentFacade facade, BalticColumnList columns, ColumnSelections columnSelections, bool isPressurePSI = false, bool isOvenDetected = false)
	{
		_owner = owner;
		_method = new BindableBalticMethod(method, facade);
		_facade = facade;
		_columns = columns;
		_columnSelections = columnSelections;
		_isPressurePSI = isPressurePSI;
		_isOvenDetected = isOvenDetected;
		_isService = BalticInstrumentFacade.IsService;
		InitializeComponent();
		_myResDictionary.Source = new Uri("pack://application:,,,/BalticWpfControlLib;component/Resources/BrukerIcons.xaml", UriKind.RelativeOrAbsolute);
		if (_method.ElutionName != null)
		{
			UpdateExperimentInfo();
			CheckExistingMethod();
		}
		UpdateMethodDisplay();
	}

	private void UpdateExperimentInfo()
	{
		_experiment = new ExperimentInfo
		{
			ElutionName = _method.ElutionName,
			AnalysisTime = TimeSpan.FromMinutes(_method.GradientTime),
			OvenTemperature = _method.OvenTemperature,
			AppKey = _facade.AppKey
		};
		if (_method.UsesTrapColumn)
		{
			Column column = _columns.Find((Column item) => item.Name == _method.TrapColumnName);
			if (column != null)
			{
				_experiment.Trap = new ColumnAdapter(column);
			}
			else
			{
				MessageBox.Show(_owner, "Method trap column \"" + _method.TrapColumnName + "\" cannot be found in the system column list - Reset Method, or Select a column and Adapt !", "Error - Unknown Column Name ", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}
		if (_method.UsesSepColumn)
		{
			Column column2 = _columns.Find((Column item) => item.Name == _method.SeparationColumnName);
			if (column2 != null)
			{
				_experiment.Separator = new ColumnAdapter(column2);
			}
			else
			{
				MessageBox.Show(_owner, "Method separation column \"" + _method.SeparationColumnName + "\" cannot be found in the system column list - Reset Method, or Select a column and Adapt !", "Error - Unknown Column Name ", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}
	}

	private void UpdateMethodDisplay()
	{
		_ucGradMain = new GradientMainUserControl(_method, _facade, _columns, _columnSelections, _isOvenDetected);
		_ucGradMain.GetInitialMethodParamEvent += ucGradMain_GetInitialMethodParamEvent;
		_ucGradMain.GenerateMethodEvent += ucGradMain_GenerateMethodEvent;
		_ucGradMain.EnableMethodControlsEvent += ucGradMain_EnableMethodControlsEvent;
		_ucGradMain.ValidationUpdateEvent += ucGradMain_UpdateInputValidation;
		_ucGradMain.TrapSelectionWarningEvent += ucGradMain_TrapSelectionWarning;
		Canvas.SetLeft(_ucGradMain, 0.0);
		Canvas.SetTop(_ucGradMain, 0.0);
		cvMethodEditor.Children.Add(_ucGradMain);
		_ucGradTable = new GradientTableUserControl(_method, _facade, _experiment);
		_ucGradTable.Width = _ucGradTable.DesignWidth;
		_ucGradTable.Height = _ucGradMain.Height;
		_ucGradTable.GradientMainUpdateEvent += ucGradTable_GradientMainUpdateEvent;
		_ucGradTable.ValidationUpdateEvent += ucGradTable_UpdateInputValidation;
		Canvas.SetLeft(_ucGradTable, _ucGradMain.Width - 5.0);
		Canvas.SetTop(_ucGradTable, 0.0);
		cvMethodEditor.Children.Add(_ucGradTable);
		_expSettings = new Expander();
		StackPanel stackPanel = new StackPanel
		{
			Orientation = Orientation.Horizontal
		};
		TextBlock element = new TextBlock
		{
			Foreground = new SolidColorBrush(Colors.SteelBlue),
			FontSize = 10.0,
			Text = "OVERRIDE DEFAULT SETTINGS"
		};
		_tbModifiedAll = new TextBlock
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Text = "",
			Foreground = new SolidColorBrush(Colors.Red),
			FontSize = 8.0,
			FontStyle = FontStyles.Italic
		};
		_imgAllErrorNotify = new Image
		{
			Height = 12.0,
			Width = 12.0,
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
			Source = (_myResDictionary["IconErrorRed"] as DrawingImage),
			ToolTip = "Override Settings Error(s)",
			Visibility = Visibility.Hidden
		};
		stackPanel.Children.Add(element);
		stackPanel.Children.Add(_tbModifiedAll);
		stackPanel.Children.Add(_imgAllErrorNotify);
		_expSettings.Header = stackPanel;
		_expSettings.Width = _ucGradMain.Width + _ucGradTable.Width - 9.0;
		_expSettings.BorderBrush = new SolidColorBrush(Colors.LightGray);
		_expGrid = new Grid
		{
			Margin = new Thickness(4.0, 4.0, 0.0, 4.0)
		};
		ColumnDefinition value = new ColumnDefinition
		{
			Width = new GridLength(_expSettings.Width * 0.45, GridUnitType.Pixel)
		};
		_expGrid.ColumnDefinitions.Add(value);
		ColumnDefinition value2 = new ColumnDefinition
		{
			Width = new GridLength(_expSettings.Width * 0.55 - 12.0, GridUnitType.Pixel)
		};
		_expGrid.ColumnDefinitions.Add(value2);
		for (int i = 0; i < 5; i++)
		{
			RowDefinition value3 = new RowDefinition
			{
				Height = new GridLength(38.0)
			};
			_expGrid.RowDefinitions.Add(value3);
		}
		UpdateOverrideControls();
		UpdateGradientTableControls();
		_expSettings.Content = _expGrid;
		_expSettings.Collapsed += expSettings_Collapsed;
		_expSettings.Expanded += expSettings_Expanded;
		Canvas.SetLeft(_expSettings, 2.0);
		Canvas.SetTop(_expSettings, _ucGradMain.Height + 5.0);
		cvMethodEditor.Children.Add(_expSettings);
		base.Height = 505.0;
	}

	private void UpdateGradientTableControls()
	{
		_ucGradTable.UpdateGradientTableControls();
	}

	private void UpdateOverrideControls(bool isKeepGradient = false)
	{
		if (isKeepGradient)
		{
			if (_activeUc is TrapColEquilUserControl trapColEquilUserControl)
			{
				trapColEquilUserControl.RefreshParameters(_experiment, _method);
			}
			else if (_activeUc is AnalyticalColEquilUserControl analyticalColEquilUserControl)
			{
				analyticalColEquilUserControl.RefreshParameters(_experiment, _method);
			}
			else if (_activeUc is SampleLoadingUserControl sampleLoadingUserControl)
			{
				sampleLoadingUserControl.RefreshParameters(_experiment, _method);
			}
			else if (_activeUc is AdvParamSettingsUserControl advParamSettingsUserControl)
			{
				advParamSettingsUserControl.RefreshParameters(_experiment, _method);
			}
			return;
		}
		if (_method.ElutionName == null)
		{
			_expGrid.Children.Clear();
			_expSettings.Visibility = Visibility.Hidden;
			return;
		}
		_activeUc = null;
		if (_method.UsesTrapColumn)
		{
			StackPanel stackPanel = new StackPanel();
			TextBlock textBlock = new TextBlock();
			stackPanel.Orientation = Orientation.Horizontal;
			textBlock.Text = "Trap Column Equilibration";
			_tbTrapModified = new TextBlock
			{
				Text = "",
				Foreground = new SolidColorBrush(Colors.Red),
				FontSize = 8.0,
				FontStyle = FontStyles.Italic,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center
			};
			_imgTrapErrorNotify = new Image
			{
				Height = 12.0,
				Width = 12.0,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
				Source = (_myResDictionary["IconErrorRed"] as DrawingImage),
				ToolTip = "Trap Column Equilibration Error(s)",
				Visibility = Visibility.Hidden
			};
			stackPanel.Children.Add(textBlock);
			stackPanel.Children.Add(_tbTrapModified);
			stackPanel.Children.Add(_imgTrapErrorNotify);
			_btnTrapColEquil.GroupName = "Overrides";
			_btnTrapColEquil.Content = stackPanel;
			_btnTrapColEquil.HorizontalContentAlignment = HorizontalAlignment.Left;
			_btnTrapColEquil.Width = _expSettings.Width * 0.45;
			_btnTrapColEquil.Height = 38.0;
			_btnTrapColEquil.Margin = new Thickness(0.0);
			_btnTrapColEquil.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
			_btnTrapColEquil.Click += gridChild_Click;
			Grid.SetRow(_btnTrapColEquil, _expGrid.Children.Count);
			Grid.SetColumn(_btnTrapColEquil, 0);
			_expGrid.Children.Add(_btnTrapColEquil);
			if (_activeUc == null)
			{
				TrapColEquilUserControl trapColEquilUserControl2 = new TrapColEquilUserControl(_method, _facade, _isPressurePSI, _experiment);
				trapColEquilUserControl2.ValidationUpdateEvent += ucTrapColEquil_UpdateInputValidation;
				trapColEquilUserControl2.ModificationUpdateEvent += ucTrapColEquil_ModificationUpdate;
				_activeUc = trapColEquilUserControl2;
				_activeBtn = _btnTrapColEquil;
			}
		}
		if (_method.UsesSepColumn)
		{
			StackPanel stackPanel2 = new StackPanel();
			TextBlock textBlock2 = new TextBlock();
			stackPanel2.Orientation = Orientation.Horizontal;
			textBlock2.Text = "Separation Column Equilibration";
			_tbSepModified = new TextBlock
			{
				Text = "",
				Foreground = new SolidColorBrush(Colors.Red),
				FontSize = 8.0,
				FontStyle = FontStyles.Italic,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center
			};
			_imgSepErrorNotify = new Image
			{
				Height = 12.0,
				Width = 12.0,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
				Source = (_myResDictionary["IconErrorRed"] as DrawingImage),
				ToolTip = "Separation Column Equilibration Error(s)",
				Visibility = Visibility.Hidden
			};
			stackPanel2.Children.Add(textBlock2);
			stackPanel2.Children.Add(_tbSepModified);
			stackPanel2.Children.Add(_imgSepErrorNotify);
			_btnSepColEquil.GroupName = "Overrides";
			_btnSepColEquil.Content = stackPanel2;
			_btnSepColEquil.HorizontalContentAlignment = HorizontalAlignment.Left;
			_btnSepColEquil.Width = _expSettings.Width * 0.45;
			_btnSepColEquil.Height = 38.0;
			_btnSepColEquil.Margin = new Thickness(0.0);
			_btnSepColEquil.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
			_btnSepColEquil.Click += gridChild_Click;
			Grid.SetRow(_btnSepColEquil, _expGrid.Children.Count);
			Grid.SetColumn(_btnSepColEquil, 0);
			_expGrid.Children.Add(_btnSepColEquil);
			if (_activeUc == null)
			{
				AnalyticalColEquilUserControl analyticalColEquilUserControl2 = new AnalyticalColEquilUserControl(_method, _facade, _isPressurePSI, _experiment);
				analyticalColEquilUserControl2.ValidationUpdateEvent += ucAnalyticalColEquil_UpdateInputValidation;
				analyticalColEquilUserControl2.ModificationUpdateEvent += ucAnalyticalColEquil_ModificationUpdate;
				_activeUc = analyticalColEquilUserControl2;
				_activeBtn = _btnSepColEquil;
			}
		}
		if (_method.UsesTrapColumn | _method.UsesSepColumn)
		{
			StackPanel stackPanel3 = new StackPanel();
			TextBlock textBlock3 = new TextBlock();
			stackPanel3.Orientation = Orientation.Horizontal;
			textBlock3.Text = "Sample Loading";
			_tbLoadModified = new TextBlock
			{
				Text = "",
				Foreground = new SolidColorBrush(Colors.Red),
				FontSize = 8.0,
				FontStyle = FontStyles.Italic,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center
			};
			_imgLoadErrorNotify = new Image
			{
				Height = 12.0,
				Width = 12.0,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
				Source = (_myResDictionary["IconErrorRed"] as DrawingImage),
				ToolTip = "Sample Loading Errors(s)",
				Visibility = Visibility.Hidden
			};
			stackPanel3.Children.Add(textBlock3);
			stackPanel3.Children.Add(_tbLoadModified);
			stackPanel3.Children.Add(_imgLoadErrorNotify);
			_btnSampleLoad.GroupName = "Overrides";
			_btnSampleLoad.Content = stackPanel3;
			_btnSampleLoad.HorizontalContentAlignment = HorizontalAlignment.Left;
			_btnSampleLoad.Width = _expSettings.Width * 0.45;
			_btnSampleLoad.Height = 38.0;
			_btnSampleLoad.Margin = new Thickness(0.0);
			_btnSampleLoad.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
			_btnSampleLoad.Click += gridChild_Click;
			Grid.SetRow(_btnSampleLoad, _expGrid.Children.Count);
			Grid.SetColumn(_btnSampleLoad, 0);
			_expGrid.Children.Add(_btnSampleLoad);
		}
		StackPanel stackPanel4 = new StackPanel();
		TextBlock textBlock4 = new TextBlock();
		stackPanel4.Orientation = Orientation.Horizontal;
		textBlock4.Text = "Advanced Settings";
		_tbAdvModified = new TextBlock
		{
			Text = "",
			Foreground = new SolidColorBrush(Colors.Red),
			FontSize = 8.0,
			FontStyle = FontStyles.Italic,
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center
		};
		_imgAdvErrorNotify = new Image
		{
			Height = 12.0,
			Width = 12.0,
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
			Source = (_myResDictionary["IconErrorRed"] as DrawingImage),
			ToolTip = "Advanced Settings Error(s)",
			Visibility = Visibility.Hidden
		};
		stackPanel4.Children.Add(textBlock4);
		stackPanel4.Children.Add(_tbAdvModified);
		stackPanel4.Children.Add(_imgAdvErrorNotify);
		_btnAdvancedSett.GroupName = "Overrides";
		_btnAdvancedSett.Content = stackPanel4;
		_btnAdvancedSett.HorizontalContentAlignment = HorizontalAlignment.Left;
		_btnAdvancedSett.Width = _expSettings.Width * 0.45;
		_btnAdvancedSett.Height = 38.0;
		_btnAdvancedSett.Margin = new Thickness(0.0);
		_btnAdvancedSett.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
		_btnAdvancedSett.Click += gridChild_Click;
		Grid.SetRow(_btnAdvancedSett, _expGrid.Children.Count);
		Grid.SetColumn(_btnAdvancedSett, 0);
		_expGrid.Children.Add(_btnAdvancedSett);
		Border element = new Border
		{
			Width = _expSettings.Width * 0.45,
			Margin = new Thickness(0.0),
			BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0),
			BorderBrush = new SolidColorBrush(Colors.SlateGray),
			Height = double.NaN
		};
		Grid.SetRow(element, _expGrid.Children.Count);
		Grid.SetColumn(element, 0);
		Grid.SetRowSpan(element, _expGrid.RowDefinitions.Count - _expGrid.Children.Count);
		_expGrid.Children.Add(element);
		if (_activeUc != null)
		{
			_ucWidth = _expSettings.Width - _btnSampleLoad.Width - 12.0;
			_ucHeight = _btnSampleLoad.Height * 5.0;
			_activeUc.Width = _ucWidth;
			_activeUc.Height = _ucHeight;
			_activeUc.HorizontalAlignment = HorizontalAlignment.Center;
			Grid.SetRow(_activeUc, 0);
			Grid.SetColumn(_activeUc, 1);
			Grid.SetRowSpan(_activeUc, 5);
			_expGrid.Children.Add(_activeUc);
			SetOverridePressedState();
		}
		if (_method.UsesTrapColumn | _method.UsesSepColumn)
		{
			_expSettings.Visibility = Visibility.Visible;
		}
		else
		{
			_ucWidth = _expSettings.Width - 12.0;
			_ucHeight = 38.0;
			_expSettings.IsExpanded = false;
			_expSettings.Visibility = Visibility.Hidden;
		}
		InitialCheckForMethodModification();
	}

	private void InitialCheckForMethodModification()
	{
		if (_method.UsesSepColumn)
		{
			ucAnalyticalColEquil_ModificationUpdate(Math.Abs(_method.SeparationColumnEquil.Scale - _method.SeparationColumnEquil.DefaultScale) > 0.0001 || (int)(_method.SeparationColumnEquil.Pressure * 100.0) != (int)(_method.SeparationColumnEquil.DefaultPressure * 100.0));
		}
		if (_method.UsesTrapColumn)
		{
			ucTrapColEquil_ModificationUpdate(Math.Abs(_method.TrapColumnEquil.Scale - _method.TrapColumnEquil.DefaultScale) > 0.0001 || (int)(_method.TrapColumnEquil.Pressure * 100.0) != (int)(_method.TrapColumnEquil.DefaultPressure * 100.0));
		}
		if (_method.UsesSepColumn || _method.UsesTrapColumn)
		{
			ucSampleLoading_ModificationUpdate(Math.Abs(_method.SampleLoading.Scale - _method.SampleLoading.DefaultScale) > 0.0001 || (int)(_method.SampleLoading.Pressure * 100.0) != (int)(_method.SampleLoading.DefaultPressure * 100.0) || _method.SampleLoading.InjectionMethod != _method.SampleLoading.DefaultInjectionMethod || _method.SampleLoading.IsBottomSense != _method.SampleLoading.DefaultIsBottomSense);
		}
		bool isModified = false;
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
		ucAdvancedSett_ModificationUpdate(isModified);
	}

	private void CheckModifiedAll()
	{
		bool flag = _method.UsesTrapColumn && _tbTrapModified.Text == "(MODIFIED)";
		bool flag2 = _method.UsesSepColumn && _tbSepModified.Text == "(MODIFIED)";
		bool flag3 = (_method.UsesSepColumn || _method.UsesTrapColumn) && _tbLoadModified.Text == "(MODIFIED)";
		bool flag4 = _tbAdvModified.Text == "(MODIFIED)";
		_tbModifiedAll.Text = ((flag || flag2 || flag3 || flag4) ? "(MODIFIED)" : "");
	}

	private void gridChild_Click(object sender, RoutedEventArgs e)
	{
		if (_activeUc != null)
		{
			_expGrid.Children.Remove(_activeUc);
		}
		RadioButton radioButton = sender as RadioButton;
		if (radioButton == _btnTrapColEquil)
		{
			TrapColEquilUserControl trapColEquilUserControl = new TrapColEquilUserControl(_method, _facade, _isPressurePSI, _experiment)
			{
				Height = _ucHeight,
				Width = _ucWidth
			};
			trapColEquilUserControl.ValidationUpdateEvent += ucTrapColEquil_UpdateInputValidation;
			trapColEquilUserControl.ModificationUpdateEvent += ucTrapColEquil_ModificationUpdate;
			_activeBtn = _btnTrapColEquil;
			_activeUc = trapColEquilUserControl;
			Grid.SetRow(trapColEquilUserControl, 0);
			Grid.SetColumn(trapColEquilUserControl, 1);
			Grid.SetRowSpan(trapColEquilUserControl, 5);
			_expGrid.Children.Add(trapColEquilUserControl);
			SetOverridePressedState();
		}
		else if (radioButton == _btnSepColEquil)
		{
			AnalyticalColEquilUserControl analyticalColEquilUserControl = new AnalyticalColEquilUserControl(_method, _facade, _isPressurePSI, _experiment)
			{
				Height = _ucHeight,
				Width = _ucWidth
			};
			analyticalColEquilUserControl.ValidationUpdateEvent += ucAnalyticalColEquil_UpdateInputValidation;
			analyticalColEquilUserControl.ModificationUpdateEvent += ucAnalyticalColEquil_ModificationUpdate;
			_activeBtn = _btnSepColEquil;
			_activeUc = analyticalColEquilUserControl;
			Grid.SetRow(analyticalColEquilUserControl, 0);
			Grid.SetColumn(analyticalColEquilUserControl, 1);
			Grid.SetRowSpan(analyticalColEquilUserControl, 5);
			_expGrid.Children.Add(analyticalColEquilUserControl);
			SetOverridePressedState();
		}
		else if (radioButton == _btnSampleLoad)
		{
			SampleLoadingUserControl sampleLoadingUserControl = new SampleLoadingUserControl(_method, _facade, _isPressurePSI, _experiment)
			{
				Height = _ucHeight,
				Width = _ucWidth
			};
			sampleLoadingUserControl.ValidationUpdateEvent += ucSampleLoading_UpdateInputValidation;
			sampleLoadingUserControl.ModificationUpdateEvent += ucSampleLoading_ModificationUpdate;
			_activeBtn = _btnSampleLoad;
			_activeUc = sampleLoadingUserControl;
			Grid.SetRow(sampleLoadingUserControl, 0);
			Grid.SetColumn(sampleLoadingUserControl, 1);
			Grid.SetRowSpan(sampleLoadingUserControl, 5);
			_expGrid.Children.Add(sampleLoadingUserControl);
			SetOverridePressedState();
		}
		else if (radioButton == _btnAdvancedSett)
		{
			AdvParamSettingsUserControl advParamSettingsUserControl = new AdvParamSettingsUserControl(_method, _facade, _experiment, _isService)
			{
				Height = _ucHeight,
				Width = _ucWidth
			};
			advParamSettingsUserControl.ModificationUpdateEvent += ucAdvancedSett_ModificationUpdate;
			advParamSettingsUserControl.ValidationUpdateEvent += ucAdvancedSett_ValidationUpdateEvent;
			_activeBtn = _btnAdvancedSett;
			_activeUc = advParamSettingsUserControl;
			Grid.SetRow(advParamSettingsUserControl, 0);
			Grid.SetColumn(advParamSettingsUserControl, 1);
			Grid.SetRowSpan(advParamSettingsUserControl, 5);
			_expGrid.Children.Add(advParamSettingsUserControl);
			SetOverridePressedState();
		}
	}

	private void CheckExistingMethod()
	{
		ProcedureInfo elutionProcedure = _facade.GetElutionProcedure(_method.ElutionName);
		if (elutionProcedure == null)
		{
			return;
		}
		ProcedureArguments procedureArguments = elutionProcedure.CreateAdvancedArguments();
		ChildProcedureArguments childProcedureArguments = elutionProcedure.CreateAdvancedChildArguments();
		foreach (ProcedureArgument item in procedureArguments)
		{
			if (_method.AdvancedSettings.Parameters.Find((BindableBalticMethod.AdvancedSett.AdvancedParameter x) => x.Name == item.Name) == null)
			{
				_method.AdvancedSettings.Parameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedParameter(item.Name, item.Value, item.Value, item.Unit));
			}
		}
		foreach (ChildProcedureArgument item2 in childProcedureArguments)
		{
			BindableBalticMethod.AdvancedSett.AdvancedChildParameter advancedChildParameter = _method.AdvancedSettings.ChildParameters.Find((BindableBalticMethod.AdvancedSett.AdvancedChildParameter x) => x.Name == item2.Name && x.Header == item2.Header);
			if (advancedChildParameter == null)
			{
				_method.AdvancedSettings.ChildParameters.Add(new BindableBalticMethod.AdvancedSett.AdvancedChildParameter(item2.Header, item2.ProcArg.Name, item2.ProcArg.Value, item2.ProcArg.Value, item2.ProcArg.Unit, item2.IsService));
			}
			else
			{
				advancedChildParameter.IsService = item2.IsService;
			}
		}
		_method.ToBalticMethod(_columns);
	}

	private void GenerateMethod(bool isKeepGradient = false, bool isKeepAdvancedSettings = false)
	{
		ProcedureInfo elutionProcedure = _facade.GetElutionProcedure(_method.ElutionName);
		ProcedureArguments procedureArguments = elutionProcedure.CreateArguments();
		ProcedureArguments procedureArguments2 = elutionProcedure.CreateAdvancedArguments();
		ChildProcedureArguments advChildArguments = elutionProcedure.CreateAdvancedChildArguments();
		string advHeader = elutionProcedure.AdvHeader;
		int[] advHeaderFgColor = elutionProcedure.AdvHeaderFgColor;
		_experiment = new ExperimentInfo
		{
			ElutionName = _method.ElutionName,
			AnalysisTime = TimeSpan.FromMinutes(_method.GradientTime),
			OvenTemperature = _method.OvenTemperature,
			IsKeepGradient = isKeepGradient,
			IsKeepAdvancedSettings = isKeepAdvancedSettings,
			AppKey = _facade.AppKey
		};
		if ((bool)procedureArguments["uses_trap"].Value)
		{
			Column column = _columns.Find((Column item) => item.Name == _method.TrapColumnName);
			_experiment.Trap = new ColumnAdapter(column);
		}
		else
		{
			Column column2 = _columns.Find((Column item) => item.Name == _method.TrapColumnName) ?? _columns.Find((Column item) => item.Name == _facade.BalticConfiguration.NoColumn.Name);
			_experiment.Trap = new ColumnAdapter(column2);
		}
		if ((bool)procedureArguments["uses_separator"].Value)
		{
			Column column3 = _columns.Find((Column item) => item.Name == _method.SeparationColumnName);
			_experiment.Separator = new ColumnAdapter(column3);
		}
		else
		{
			Column column4 = _columns.Find((Column item) => item.Name == _method.SeparationColumnName) ?? _columns.Find((Column item) => item.Name == _facade.BalticConfiguration.NoColumn.Name);
			_experiment.Separator = new ColumnAdapter(column4);
		}
		_ucGradTable.Experiment = _experiment;
		procedureArguments = _facade.GenerateArguments(_experiment);
		procedureArguments2 = _facade.GenerateAdvArguments(_experiment);
		_method.ToBalticMethod(_columns).Initialize(_experiment, procedureArguments, procedureArguments2, advChildArguments, advHeader, advHeaderFgColor);
		_method.Refresh(advChildArguments);
		UpdateOverrideControls(isKeepGradient);
		UpdateGradientTableControls();
	}

	public void ToBalticMethod()
	{
		_method.ToBalticMethod(_columns);
	}

	private void expSettings_Expanded(object sender, RoutedEventArgs e)
	{
		base.Height = 705.0;
		SetOverridePressedState();
	}

	private void SetOverridePressedState()
	{
		if (_activeBtn == _btnTrapColEquil)
		{
			_btnTrapColEquil.IsChecked = true;
		}
		else if (_activeBtn == _btnSepColEquil)
		{
			_btnSepColEquil.IsChecked = true;
		}
		else if (_activeBtn == _btnSampleLoad)
		{
			_btnSampleLoad.IsChecked = true;
		}
		else if (_activeBtn == _btnAdvancedSett)
		{
			_btnAdvancedSett.IsChecked = true;
		}
	}

	private void expSettings_Collapsed(object sender, RoutedEventArgs e)
	{
		base.Height = 505.0;
	}

	private void ucGradMain_TrapSelectionWarning(bool isShow, string message = "")
	{
		this.TrapSelectionWarningEvent?.Invoke(isShow, message);
	}

	private void ucGradMain_EnableMethodControlsEvent(bool isEnable, bool isReset)
	{
		if (isReset)
		{
			_expGrid.Children.Clear();
			_expSettings.Visibility = Visibility.Hidden;
			_imgAllErrorNotify.Visibility = Visibility.Hidden;
			_isTrapColEquilValid = (_isAnalColEquilValid = (_isSampleLoadValid = (_isGradTableValid = (_isGradMainValid = (_isAdvSettingsValid = true)))));
			if (_imgTrapErrorNotify != null)
			{
				_imgTrapErrorNotify.Visibility = Visibility.Collapsed;
			}
			if (_imgLoadErrorNotify != null)
			{
				_imgLoadErrorNotify.Visibility = Visibility.Collapsed;
			}
			if (_imgSepErrorNotify != null)
			{
				_imgSepErrorNotify.Visibility = Visibility.Collapsed;
			}
			if (_imgAdvErrorNotify != null)
			{
				_imgAdvErrorNotify.Visibility = Visibility.Collapsed;
			}
			UpdateOverrideDefaultErrorIcon();
		}
		_methodStateEnabled = isEnable;
		this.EnableMethodCompleteEvent?.Invoke(isEnable);
		_expSettings.IsEnabled = isEnable;
		if (!isEnable)
		{
			_expSettings.IsExpanded = false;
		}
		UpdateGradientTableControls();
	}

	private void ucGradMain_GetInitialMethodParamEvent()
	{
		ProcedureArguments procedureArguments = _facade.GetElutionProcedure(_method.ElutionName).CreateArguments();
		_method.IsIsocratic = (bool)procedureArguments["is_isocratic"].Value;
		_method.UsesTrapColumn = (bool)procedureArguments["uses_trap"].Value;
		_method.UsesSepColumn = (bool)procedureArguments["uses_separator"].Value;
		_method.GradientTime = (double)(int)procedureArguments["analysis_time"].Value / 60.0;
		_method.OvenTemperature = (double)procedureArguments["oven_temperature"].Value;
	}

	private void ucGradMain_GenerateMethodEvent(bool isKeepGradient = false, bool isKeepAdvancedSettings = false)
	{
		GenerateMethod(isKeepGradient, isKeepAdvancedSettings);
	}

	private void ucGradTable_GradientMainUpdateEvent(WPFConstants.UpdateType updateType)
	{
		_ucGradMain.UpdateFromGradientTable(updateType);
	}

	private void ucTrapColEquil_UpdateInputValidation(bool isValid)
	{
		_isTrapColEquilValid = isValid;
		_imgTrapErrorNotify.Visibility = (_isTrapColEquilValid ? Visibility.Collapsed : Visibility.Visible);
		UpdateOverrideDefaultErrorIcon();
		this.ValidateInputUpdateEvent?.Invoke(IsMethodDataValid);
	}

	private void ucAnalyticalColEquil_UpdateInputValidation(bool isValid)
	{
		_isAnalColEquilValid = isValid;
		_imgSepErrorNotify.Visibility = (_isAnalColEquilValid ? Visibility.Collapsed : Visibility.Visible);
		UpdateOverrideDefaultErrorIcon();
		this.ValidateInputUpdateEvent?.Invoke(IsMethodDataValid && _methodStateEnabled);
	}

	private void ucSampleLoading_UpdateInputValidation(bool isValid)
	{
		_isSampleLoadValid = isValid;
		_imgLoadErrorNotify.Visibility = (_isSampleLoadValid ? Visibility.Collapsed : Visibility.Visible);
		UpdateOverrideDefaultErrorIcon();
		this.ValidateInputUpdateEvent?.Invoke(IsMethodDataValid && _methodStateEnabled);
	}

	private void ucGradMain_UpdateInputValidation(bool isValid)
	{
		_isGradMainValid = isValid;
		this.ValidateInputUpdateEvent?.Invoke(IsMethodDataValid && _methodStateEnabled);
	}

	private void UpdateOverrideDefaultErrorIcon()
	{
		_imgAllErrorNotify.Visibility = (IsOverrideSettingsValid ? Visibility.Collapsed : Visibility.Visible);
	}

	private void ucGradTable_UpdateInputValidation(bool isValid, string header = "", string subject = "", string message = "")
	{
		_isGradTableValid = isValid;
		if (header != "")
		{
			ucAdvancedSett_ValidationUpdateEvent(isValid: false);
			if (_activeUc is AdvParamSettingsUserControl advParamSettingsUserControl)
			{
				advParamSettingsUserControl.ValidateParameters();
			}
		}
		else
		{
			ucAdvancedSett_ValidationUpdateEvent(isValid: true);
			UpdateActiveUserControl();
			this.ValidateInputUpdateEvent?.Invoke(IsMethodDataValid && _methodStateEnabled);
		}
	}

	private void UpdateActiveUserControl()
	{
		if (_activeUc is AdvParamSettingsUserControl advParamSettingsUserControl)
		{
			advParamSettingsUserControl.ValidateParameters();
		}
		else if (_activeUc is TrapColEquilUserControl trapColEquilUserControl)
		{
			trapColEquilUserControl.ValidateParameters();
		}
		else if (_activeUc is AnalyticalColEquilUserControl analyticalColEquilUserControl)
		{
			analyticalColEquilUserControl.ValidateParameters();
		}
		else if (_activeUc is SampleLoadingUserControl sampleLoadingUserControl)
		{
			sampleLoadingUserControl.ValidateParameters();
		}
	}

	private void ucAdvancedSett_ValidationUpdateEvent(bool isValid)
	{
		_isAdvSettingsValid = isValid;
		_imgAdvErrorNotify.Visibility = (_isAdvSettingsValid ? Visibility.Collapsed : Visibility.Visible);
		UpdateOverrideDefaultErrorIcon();
		this.ValidateInputUpdateEvent?.Invoke(IsMethodDataValid && _methodStateEnabled);
	}

	private void ucTrapColEquil_ModificationUpdate(bool isModified)
	{
		_tbTrapModified.Text = (isModified ? "(MODIFIED)" : "");
		CheckModifiedAll();
		UpdateGradientTableControls();
	}

	private void ucAnalyticalColEquil_ModificationUpdate(bool isModified)
	{
		_tbSepModified.Text = (isModified ? "(MODIFIED)" : "");
		CheckModifiedAll();
		UpdateGradientTableControls();
	}

	private void ucSampleLoading_ModificationUpdate(bool isModified)
	{
		_tbLoadModified.Text = (isModified ? "(MODIFIED)" : "");
		CheckModifiedAll();
		UpdateGradientTableControls();
	}

	private void ucAdvancedSett_ModificationUpdate(bool isModified)
	{
		_tbAdvModified.Text = (isModified ? "(MODIFIED)" : "");
		CheckModifiedAll();
		UpdateGradientTableControls();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/methodusercontrol.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 1)
		{
			cvMethodEditor = (Canvas)target;
		}
		else
		{
			_contentLoaded = true;
		}
	}
}
