// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using BalticClassLib;
using BalticWpfControlLib.Controls;
using BalticWpfControlLib.Diagram;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Bruker.Lc.Maintenance;
using Bruker.Lc.Metering;
using BrukerLC.Interfaces.ViewModelInterfaces;
using BrukerLC.Utils.Controls;

namespace BalticWpfControlLib;

public class MainUserControl : Canvas, INotifyPropertyChanged, IComponentConnector, IStyleConnector
{
	public delegate void BalticColumnSelectionEventHandler(object sender, BalticWpfColumnEventArgs args);

	public delegate void UpdateHyStarStatusEventHandler(bool isReady, string procName, string version);

	public delegate void MaintenanceExecutionReport(ExecutionStateChangedEventArgs e);

	public delegate void ConfirmButtonOnErrorCallback(SystemCondition condition);

	public delegate void ShowPreferences();

	public delegate void ShowLogBook();

	public delegate void AbortInjectionEventHandler(bool isManualAbort = false, bool isContinue = false);

	private delegate void SetStatusBarCallback(System.Drawing.Color newColor, System.Drawing.Color newTextColor, string strText);

	private delegate void SetStatusBarTextCallback(string strText);

	private delegate void SetStatusBarColorCallback(System.Drawing.Color newColor, System.Drawing.Color newTextColor);

	private delegate void SetDeviceStatusBarCallback(System.Drawing.Color newColor, string strText);

	private delegate void SetAlertMessageCallback(SystemCondition condition);

	private delegate void SetSamplePositionCallback(string samplePos);

	private const string PROPERTY_CATEGORY = "Main Pump Control Properties";

	private readonly string _versionText;

	private readonly string _productVersion;

	private readonly string _creationDate;

	private readonly List<BalticHWProfile.CapillaryItem> _capillaries;

	private readonly List<BalticPreferences.CapillaryPreference> _prefCapillaries;

	private readonly Polyline mixPolylineLeft = new Polyline();

	private readonly Polyline mixPolylineRight = new Polyline();

	private readonly Polyline columnPolyline = new Polyline();

	private readonly bool _isPressurePSI;

	private readonly bool _isOvenInstalled;

	private BalticInstrumentFacade _instrument;

	private readonly CircularDisplayBuffer _flowMixDisp = new CircularDisplayBuffer(10);

	private readonly CircularDisplayBuffer _flowADisp = new CircularDisplayBuffer(10);

	private readonly CircularDisplayBuffer _flowBDisp = new CircularDisplayBuffer(10);

	private double flowPumpA;

	private double flowPumpB;

	private readonly string _displayName;

	private double _ovenTemp = 20.0;

	private double _ovenSetPtTemp = 20.0;

	private LCControlWindow dlgLCControl;

	private bool _isUseIdleFlow;

	private BalticPreferences _preferences;

	private readonly List<IMeteringChannel> _channels = new List<IMeteringChannel>();

	private volatile ExecutionStateChangedEventArgs _lastExecutionState;

	public static readonly RoutedCommand AbortCommand;

	private volatile bool injectionAbortable;

	private volatile bool manualInjectionAbort;

	private volatile bool idleFlowActive;

	private volatile bool isEndTempWaitActive;

	public static readonly RoutedCommand ResetSystemCommand;

	public static readonly RoutedCommand ClearErrorCommand;

	public static readonly RoutedCommand EndTempWaitCommand;

	private volatile bool systemResettable;

	private volatile bool isClearErrorable;

	private volatile bool isPreferenceable = true;

	public static readonly RoutedCommand PreferencesCommand;

	internal MainUserControl canvasMain;

	internal MenuItem menuitemEndTempWait;

	internal MenuItem menuitemClearError;

	internal MenuItem menuitemResetSystem;

	internal Grid gridMain;

	internal Border bdrDeviceStatus;

	internal TextBlock txtHyStarStatus;

	internal TextBlock txtPumpStatus;

	internal TextBlock txtDeviceStatus;

	internal DockPanel dpDeviceStatus;

	internal System.Windows.Controls.Image imgAlert;

	internal TextBlock tbAlert;

	internal PumpControl pumpAControl;

	internal PumpControl pumpBControl;

	internal Ellipse ellipseMixLeft;

	internal TextBlock tbFlow;

	internal TextBlock tbPercentB;

	internal Polygon plyMixTriangle;

	internal System.Windows.Controls.Image imgSyringe;

	internal TextBlock tbSamplePos;

	internal DockPanel trapColPanel;

	internal ComboBox comboPreColumn;

	internal TextBlock tbPreColumn;

	internal ColumnInfoUserControl TrapColumnInfoControl;

	internal StackPanel stkTrapWarn;

	internal DockPanel sepColPanel;

	internal ComboBox comboAnalColumn;

	internal ColumnInfoUserControl SepColumnInfoControl;

	internal StackPanel stkSepWarn;

	internal Led idleLED;

	internal TextBlock idleFlowPopupText;

	internal TextBlock idleFlowText;

	internal TextBlock idleFlowCompText;

	internal TextBlock idleFlowViaTrapText;

	internal Button btnLCControl;

	internal System.Windows.Controls.Image imgLCControlBtn;

	internal TextBlock txtLCControlBtn;

	internal Grid gridOven;

	internal TextBlock tbOvenTemp;

	internal System.Windows.Controls.Image imgHeatWaves;

	internal TextBlock tbTempSetPt;

	internal TextBlock tbTempMeasured;

	private bool _contentLoaded;

	public double OvenTemp
	{
		get
		{
			return _ovenTemp;
		}
		set
		{
			_ovenTemp = value;
			NotifyPropertyChanged("OvenTemp");
		}
	}

	public string OvenTempTipTxt => "All";

	public double OvenTempSetPt
	{
		get
		{
			return _ovenSetPtTemp;
		}
		set
		{
			_ovenSetPtTemp = value;
			NotifyPropertyChanged("OvenTempSetPt");
		}
	}

	public bool IsUseIdleFlow
	{
		get
		{
			return _isUseIdleFlow;
		}
		set
		{
			_isUseIdleFlow = value;
		}
	}

	public BalticPreferences Preferences
	{
		get
		{
			return _preferences;
		}
		set
		{
			_preferences = value;
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				idleLED.Visibility = ((!_preferences.Pump.IsIdleFlowOnStandby && (!_preferences.Pump.IsIdleFlowOnError || !IsClearErrorable)) ? Visibility.Hidden : Visibility.Visible);
				idleFlowViaTrapText.Text = (Preferences.Pump.IsViaTrap ? "On" : "Off");
			});
		}
	}

	public bool IsConnected => _instrument.IsConnected;

	public BalticInstrumentFacade InstrumentFacade
	{
		get
		{
			return _instrument;
		}
		set
		{
			if (_instrument != null)
			{
				_instrument.ExecutionStateChanged -= _instrument_ExecutionStateChanged;
				_instrument.DecompressExecutionStateChanged -= Decompress_ExecutionStateChanged;
				(_instrument.SystemConditions as INotifyCollectionChanged).CollectionChanged -= MainUserControl_ConditionsChanged;
				foreach (IMeteringChannel channel in _channels)
				{
					MeteringChannelId id = channel.ChannelInfo.Id;
					if (id.Source.ToString().StartsWith("pump"))
					{
						channel.ChannelDataChanged -= pump_ChannelDataChanged;
					}
					else if ((id.Source.ToString().StartsWith("flowsensor") && id.Name.Equals("flow")) || (id.Source.ToString().StartsWith("oven") && id.Name.Equals("temperature")) || (id.Source.ToString().StartsWith("oven") && id.Name.Equals("setpoint")))
					{
						channel.ChannelDataChanged -= sensor_ChannelDataChanged;
					}
				}
				_channels.Clear();
			}
			_instrument = value;
			if (_instrument == null)
			{
				return;
			}
			TransformChannels(_instrument.GaugeChannels, _channels, _isPressurePSI);
			foreach (IMeteringChannel channel2 in _channels)
			{
				MeteringChannelId id2 = channel2.ChannelInfo.Id;
				if (id2.Source.ToString().StartsWith("pump"))
				{
					channel2.ChannelDataChanged += pump_ChannelDataChanged;
				}
				else if ((id2.Source.ToString().StartsWith("flowsensor") && id2.Name.Equals("flow")) || (id2.Source.ToString().StartsWith("oven") && id2.Name.Equals("temperature")) || (id2.Source.ToString().StartsWith("oven") && id2.Name.Equals("setpoint")))
				{
					channel2.ChannelDataChanged += sensor_ChannelDataChanged;
				}
			}
			MainUserControl_ConditionsChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			(_instrument.SystemConditions as INotifyCollectionChanged).CollectionChanged += MainUserControl_ConditionsChanged;
			_instrument.ExecutionStateChanged += _instrument_ExecutionStateChanged;
			_instrument.DecompressExecutionStateChanged += Decompress_ExecutionStateChanged;
			_instrument.IdleFlowRunningEvent += _instrument_IdleFlowRunningEvent;
			_instrument.ActiveConditionClearEvent += _instrument_ActiveConditionClearEvent;
			_instrument.ShowCompositionConditions.ShowCompositionCollectionChanged += scc_ShowCompositionCollectionChanged;
		}
	}

	public bool InjectionAbortable
	{
		get
		{
			return injectionAbortable;
		}
		set
		{
			injectionAbortable = value;
		}
	}

	public bool IsManualInjectionAbort
	{
		get
		{
			return manualInjectionAbort;
		}
		set
		{
			manualInjectionAbort = value;
		}
	}

	public bool IsIdleFlowActive
	{
		get
		{
			return idleFlowActive;
		}
		set
		{
			idleFlowActive = value;
		}
	}

	public bool IsEndTempWaitActive
	{
		get
		{
			return isEndTempWaitActive;
		}
		set
		{
			isEndTempWaitActive = value;
		}
	}

	public bool SystemResettable
	{
		get
		{
			return systemResettable;
		}
		set
		{
			systemResettable = value;
		}
	}

	public bool IsClearErrorable
	{
		get
		{
			return isClearErrorable;
		}
		set
		{
			isClearErrorable = value;
		}
	}

	public bool IsPreferenceable
	{
		get
		{
			return isPreferenceable;
		}
		set
		{
			isPreferenceable = value;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public event BalticColumnSelectionEventHandler OnColumnSelection;

	public event UpdateHyStarStatusEventHandler UpdateHyStarStatusEvent;

	public event MaintenanceExecutionReport MaintenanceExecutionReportEvent;

	public event ConfirmButtonOnErrorCallback ConfirmButtonEvent;

	public event ShowPreferences OnShowPreferences;

	public event ShowLogBook OnShowLogBook;

	public event EventHandler EndTempWait;

	public event EventHandler ResetSystem;

	public event EventHandler ClearError;

	public event AbortInjectionEventHandler AbortInjection;

	public event EventHandler ActivateAuxiliaryControl;

	private void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	static MainUserControl()
	{
		AbortCommand = new RoutedCommand();
		ResetSystemCommand = new RoutedCommand();
		ClearErrorCommand = new RoutedCommand();
		EndTempWaitCommand = new RoutedCommand();
		PreferencesCommand = new RoutedCommand();
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MainUserControl), new FrameworkPropertyMetadata(typeof(MainUserControl)));
	}

	public MainUserControl(BalticInstrumentFacade instrument, List<BalticHWProfile.CapillaryItem> capillaries, string displayName, string version, string productVersion, string creationDate, List<BalticPreferences.CapillaryPreference> prefCapillaries, bool isOvenInstalled, bool displayPressureAsPsi)
	{
		_displayName = displayName;
		_versionText = Utility.CreateDisplayVersion(version);
		_productVersion = productVersion;
		_creationDate = creationDate;
		_isPressurePSI = displayPressureAsPsi;
		_instrument = instrument;
		_capillaries = capillaries;
		_prefCapillaries = prefCapillaries;
		_isOvenInstalled = isOvenInstalled;
	}

	private void TransformChannels(IEnumerable<IMeteringChannel> channels, List<IMeteringChannel> collection, bool psi)
	{
		foreach (IMeteringChannel channel in channels)
		{
			MeteringChannelInfo channelInfo = channel.ChannelInfo;
			IMeteringChannel meteringChannel = channel;
			if (psi && "bar".Equals(channelInfo.Unit, StringComparison.InvariantCultureIgnoreCase))
			{
				channelInfo = new MeteringChannelInfo(channelInfo.Id.Name, channelInfo.Id.Source, "psi", channelInfo.DisplayDecimals, channelInfo.ValueType, channelInfo.IsDiagnostic, channelInfo.IsSevice);
				meteringChannel = new TransformingMeteringChannel(meteringChannel, channelInfo, Bars2Psi);
			}
			collection.Add(meteringChannel);
		}
	}

	private MeteringDataPoint Bars2Psi(MeteringDataPoint mdp)
	{
		return new MeteringDataPoint(mdp.Timestamp, (double)mdp.Value / 0.0689475729);
	}

	private void scc_ShowCompositionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		CheckShowCompositionConditions();
	}

	private void Init(object sender, EventArgs e)
	{
		ConfigureDataContexts();
		txtDeviceStatus.Visibility = Visibility.Hidden;
		tbFlow.Text = "-";
		tbPercentB.Text = "-";
		tbOvenTemp.Text = "- °C";
		tbTempSetPt.Text = "- °C";
		tbTempMeasured.Text = "- °C";
		imgHeatWaves.Opacity = 0.4;
		tbOvenTemp.Opacity = 0.4;
		menuitemResetSystem.Header = $"Reset {_displayName}";
		UpdateMixColor(0);
		DrawConnections();
		CheckShowCompositionConditions();
	}

	private void _instrument_IdleFlowRunningEvent(bool isRunning, bool isErrorState = false)
	{
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			if (Preferences.Pump.IsIdleFlowOnStandby || Preferences.Pump.IsIdleFlowOnError)
			{
				idleFlowPopupText.Text = (isRunning ? "Idle Flow Enabled" : "Idle Flow Disabled");
				idleLED.IsActive = isRunning;
				if (isRunning)
				{
					idleFlowText.Text = string.Format(CultureInfo.InvariantCulture, "{0:0.0} µL/min", Preferences.Pump.StandbyFlow);
					idleFlowCompText.Text = string.Format(CultureInfo.InvariantCulture, "{0} %", Preferences.Pump.Composition);
					idleFlowViaTrapText.Text = (Preferences.Pump.IsViaTrap ? "On" : "Off");
				}
				else
				{
					idleFlowText.Text = "-";
					idleFlowCompText.Text = "-";
					idleFlowViaTrapText.Text = "-";
				}
				if (Preferences.Pump.IsIdleFlowOnError && isErrorState)
				{
					idleLED.Visibility = Visibility.Visible;
				}
			}
		});
	}

	private void Decompress_ExecutionStateChanged(object sender, ExecutionStateChangedEventArgs e)
	{
		UpdateHyStarStatus(e);
	}

	private void _instrument_ExecutionStateChanged(object sender, ExecutionStateChangedEventArgs e)
	{
		if (!e.IsHidden || IsElutionProcedure(e.Procedure) || IsEquilibrationProcedure(e.Procedure))
		{
			_lastExecutionState = e;
			if (!IsElutionProcedure(e.Procedure))
			{
				UpdateHyStarStatus(e);
			}
			switch (e.ExecutionState)
			{
			case ProcedureExecutionState.Started:
				IsPreferenceable = false;
				break;
			case ProcedureExecutionState.Completed:
			case ProcedureExecutionState.Aborted:
				IsPreferenceable = true;
				break;
			}
			bool enabled = e.ExecutionState != ProcedureExecutionState.Started;
			Action method = delegate
			{
				SetColumnSelectionEnabled(enabled);
			};
			base.Dispatcher.BeginInvoke(method);
		}
	}

	private void UpdateHyStarStatus(ExecutionStateChangedEventArgs e)
	{
		switch (e.ExecutionState)
		{
		case ProcedureExecutionState.Started:
			this.UpdateHyStarStatusEvent?.Invoke(isReady: false, e.Procedure, e.Version);
			break;
		case ProcedureExecutionState.Completed:
		case ProcedureExecutionState.Aborted:
			this.UpdateHyStarStatusEvent?.Invoke(!e.IsDecompressOnExit, e.Procedure, e.Version);
			break;
		}
	}

	private bool IsElutionProcedure(string procedureName)
	{
		return _instrument.ElutionTypeInfoList.Find((ProcedureInfo x) => x.Name == procedureName) != null;
	}

	private bool IsEquilibrationProcedure(string procedureName)
	{
		return _instrument.GetEquilibrationProcedure(procedureName) != null;
	}

	private void ConfigureDataContexts()
	{
		SolidColorBrush activeBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 112, 184));
		SolidColorBrush activeBrush2 = new SolidColorBrush(System.Windows.Media.Color.FromRgb(193, 41, 45));
		pumpAControl.DataContext = new PumpViewModel
		{
			ActiveBrush = activeBrush,
			IsActive = true
		};
		pumpBControl.DataContext = new PumpViewModel
		{
			ActiveBrush = activeBrush2,
			IsActive = true
		};
	}

	private void pump_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
	{
		if (e.Data == null)
		{
			return;
		}
		IMeteringChannel channel = sender as IMeteringChannel;
		object src = channel.ChannelInfo.Id.Source;
		double value = (double)e.Data[e.Data.Length - 1].Value;
		Action method = delegate
		{
			IPumpViewModel pumpViewModel = (src.ToString().EndsWith("a") ? ((IPumpViewModel)pumpAControl.DataContext) : ((IPumpViewModel)pumpBControl.DataContext));
			if (pumpViewModel != null)
			{
				switch (channel.ChannelInfo.Id.Name)
				{
				case "pressure":
					pumpViewModel.Pressure = value;
					pumpViewModel.PressureUnit = channel.ChannelInfo.Unit;
					break;
				case "relative Volume":
					pumpViewModel.FillLevel = 100.0 - value;
					pumpViewModel.VolumeUsed = value / 100.0 * 1350.0;
					pumpViewModel.VolumeLeft = (100.0 - value) / 100.0 * 1350.0;
					pumpViewModel.VolumeUnit = "µL";
					break;
				case "speed":
					pumpViewModel.ThroughputSetPoint = value;
					pumpViewModel.ThroughputUnit = "µL/min";
					break;
				}
			}
		};
		base.Dispatcher.BeginInvoke(method);
	}

	private void sensor_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
	{
		if (e.Data == null)
		{
			return;
		}
		IMeteringChannel channel = sender as IMeteringChannel;
		string src = channel.ChannelInfo.Id.Source.ToString();
		double value = (double)e.Data[e.Data.Length - 1].Value;
		Action method = delegate
		{
			switch (channel.ChannelInfo.Id.Name)
			{
			case "flow":
				if (src.EndsWith("a"))
				{
					flowPumpA = ((value > 0.0) ? value : 0.0);
					_flowADisp.Add(flowPumpA);
				}
				else
				{
					flowPumpB = ((value > 0.0) ? value : 0.0);
					_flowBDisp.Add(flowPumpB);
				}
				if (_flowADisp.nItems > 0 && _flowBDisp.nItems > 0)
				{
					double obj = flowPumpA + flowPumpB;
					_flowMixDisp.Add(obj);
					double num = 0.0;
					bool flag = true;
					if ((int)(_flowADisp.Average * 10000.0) == 0 && (int)(_flowBDisp.Average * 10000.0) == 0)
					{
						num = 0.0;
					}
					else if ((int)(_flowBDisp.Average * 10000.0) == 0)
					{
						num = 0.0;
					}
					else if ((int)(_flowADisp.Average * 10000.0) == 0)
					{
						num = 1.0;
					}
					else if (_flowMixDisp.nItems > 0)
					{
						num = _flowBDisp.Average / _flowMixDisp.Average;
						if (num > 1.0)
						{
							num = 1.0;
						}
						else if (num < 0.0)
						{
							num = 0.0;
						}
					}
					else
					{
						flag = false;
					}
					if (flag)
					{
						tbPercentB.Text = string.Format(CultureInfo.InvariantCulture, "{0:P1}B", num);
					}
					string text = "µL/min";
					if (Math.Abs(_flowMixDisp.Average) < 1.0)
					{
						text = "nL/min";
					}
					double num2 = _flowMixDisp.Average;
					if (text.Contains("nL/min"))
					{
						num2 *= 1000.0;
					}
					if (text == "nL/min")
					{
						tbFlow.Text = string.Format((Math.Abs(num2) < 10.0) ? "{0:F1} {1}" : "{0:F0} {1}", num2, text);
					}
					else
					{
						tbFlow.Text = string.Format(CultureInfo.InvariantCulture, "{0:F2} {1}", num2, text);
					}
					UpdateMixColor((int)(num * 100.0));
				}
				break;
			case "temperature":
				if (channel.ChannelInfo.Id.Source.ToString().StartsWith("oven"))
				{
					OvenTemp = value;
					if ((int)(value * 10.0) != 0)
					{
						tbOvenTemp.Text = string.Format(CultureInfo.InvariantCulture, "{0:0} °C", Math.Round(value - 0.06));
						tbTempMeasured.Text = tbOvenTemp.Text;
						if (!_isOvenInstalled)
						{
							imgHeatWaves.Opacity = 0.4;
							tbOvenTemp.Opacity = 0.4;
						}
						else
						{
							if ((int)(imgHeatWaves.Opacity * 10.0) < 9)
							{
								imgHeatWaves.Opacity = 1.0;
							}
							if ((int)(tbOvenTemp.Opacity * 10.0) < 9)
							{
								tbOvenTemp.Opacity = 1.0;
							}
						}
					}
					else
					{
						if ((int)(imgHeatWaves.Opacity * 10.0) > 4)
						{
							imgHeatWaves.Opacity = 0.4;
						}
						if ((int)(tbOvenTemp.Opacity * 10.0) > 4)
						{
							tbOvenTemp.Opacity = 0.4;
						}
					}
				}
				break;
			case "setpoint":
				if (channel.ChannelInfo.Id.Source.ToString().StartsWith("oven"))
				{
					OvenTempSetPt = value;
					tbTempSetPt.Text = string.Format(CultureInfo.InvariantCulture, "{0:0} °C", value);
					if ((int)(_ovenTemp * 10.0) == 0)
					{
						tbOvenTemp.Text = "- °C";
						tbTempSetPt.Text = "- °C";
						tbTempMeasured.Text = "- °C";
					}
				}
				break;
			}
		};
		base.Dispatcher.BeginInvoke(method);
	}

	private void MainUserControl_ConditionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (InstrumentFacade.IsConnected)
		{
			SetActivateAuxiliaryControlEnabled(enabled: true);
		}
		IEnumerable<SystemCondition> systemConditions = _instrument.SystemConditions;
		SystemCondition systemCondition = null;
		foreach (SystemCondition item in systemConditions)
		{
			if (item.Severity.Equals(Severity.Error))
			{
				systemCondition = item;
			}
		}
		if (systemCondition == null)
		{
			foreach (SystemCondition item2 in systemConditions)
			{
				if (item2.Severity.Equals(Severity.Warn))
				{
					systemCondition = item2;
				}
			}
		}
		SetAlertMessage(systemCondition);
	}

	private void ButtonClicked(object sender, RoutedEventArgs e)
	{
		this.ActivateAuxiliaryControl?.Invoke(this, EventArgs.Empty);
		if (dlgLCControl != null)
		{
			dlgLCControl.Show();
			if (dlgLCControl.WindowState == WindowState.Minimized)
			{
				dlgLCControl.WindowState = WindowState.Normal;
			}
			dlgLCControl.Focus();
		}
		else
		{
			dlgLCControl = new LCControlWindow(_instrument, _capillaries, _prefCapillaries, _displayName, _isPressurePSI, _isOvenInstalled, _lastExecutionState);
			dlgLCControl.ExecutionReportEvent += dlgLCControl_ExecutionReportEvent;
			dlgLCControl.ConfirmButtonEvent += DlgLCControl_ConfirmButtonEvent;
			dlgLCControl.Closing += dlgLCControl_Closing;
			UpdateLCControlColumnTypes(Column.ColumnType.Both);
			dlgLCControl.Show();
			dlgLCControl.Focus();
		}
	}

	private void DlgLCControl_ConfirmButtonEvent(SystemCondition condition)
	{
		this.ConfirmButtonEvent?.Invoke(condition);
	}

	private void dlgLCControl_ExecutionReportEvent(ExecutionStateChangedEventArgs e)
	{
		this.MaintenanceExecutionReportEvent?.Invoke(e);
	}

	public void AbortActiveProcedure()
	{
		dlgLCControl?.AbortActiveProcedure();
	}

	private void dlgLCControl_Closing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		dlgLCControl.Hide();
	}

	public void SetActivateAuxiliaryControlEnabled(bool enabled)
	{
		Action callback = delegate
		{
			btnLCControl.IsEnabled = enabled;
			if (!enabled && dlgLCControl != null)
			{
				dlgLCControl.Closing -= dlgLCControl_Closing;
				dlgLCControl.Close();
				dlgLCControl = null;
			}
		};
		base.Dispatcher.Invoke(callback);
	}

	public void SetStatusBar(System.Drawing.Color newColor, System.Drawing.Color newTextColor, string strText)
	{
		Action callback = delegate
		{
			SetStatusBarColor(newColor, newTextColor);
			if (strText != "")
			{
				txtHyStarStatus.Text = ((_versionText == "") ? $"Bruker {_displayName} ({strText})" : $"Bruker {_displayName} {_versionText} ({strText})");
			}
		};
		base.Dispatcher.Invoke(callback);
	}

	public void SetStatusBarText(string strText)
	{
		Action callback = delegate
		{
			if (strText != "")
			{
				txtHyStarStatus.Text = ((_versionText == "") ? $"Bruker {_displayName} ({strText})" : $"Bruker {_displayName} {_versionText} ({strText})");
			}
		};
		base.Dispatcher.Invoke(callback);
	}

	public void SetStatusBarColor(System.Drawing.Color newColor, System.Drawing.Color newTextColor)
	{
		if (txtHyStarStatus.Dispatcher.CheckAccess())
		{
			System.Windows.Media.Brush background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(newColor.R, newColor.G, newColor.B));
			System.Windows.Media.Brush foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(newTextColor.R, newTextColor.G, newTextColor.B));
			txtHyStarStatus.Background = background;
			txtHyStarStatus.Foreground = foreground;
			bdrDeviceStatus.Background = background;
		}
		else
		{
			SetStatusBarColorCallback method = SetStatusBarColor;
			txtHyStarStatus.Dispatcher.BeginInvoke(method, newColor, newTextColor);
		}
	}

	public void SetDeviceStatusBar(System.Drawing.Color newColor, string strText)
	{
		if (txtDeviceStatus.Dispatcher.CheckAccess())
		{
			System.Windows.Media.Brush brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B));
			System.Windows.Media.Brush foreground = new SolidColorBrush(Colors.Black);
			txtDeviceStatus.Background = brush;
			txtDeviceStatus.Foreground = foreground;
			if (brush == System.Windows.Media.Brushes.Red)
			{
				txtDeviceStatus.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(byte.MaxValue, 250, byte.MaxValue, 250));
			}
			if (brush == System.Windows.Media.Brushes.Red)
			{
				gridMain.Background = base.Resources["ErrorBackgroundBrush"] as LinearGradientBrush;
				gridMain.Background.Opacity = 0.3;
			}
			else
			{
				gridMain.Background = new SolidColorBrush(Colors.Transparent);
			}
		}
		else
		{
			SetDeviceStatusBarCallback method = SetDeviceStatusBar;
			txtDeviceStatus.Dispatcher.BeginInvoke(method, newColor, strText);
		}
	}

	public void SetAlertMessage(SystemCondition condition)
	{
		if (txtDeviceStatus.Dispatcher.CheckAccess())
		{
			if (condition == null)
			{
				txtDeviceStatus.Visibility = Visibility.Hidden;
				gridMain.Background = new SolidColorBrush(Colors.Transparent);
				imgLCControlBtn.Source = new ImageConverter().Convert("Images/Settings_32.png") as ImageSource;
				btnLCControl.Style = base.Resources["BalticButtonStyle"] as Style;
				return;
			}
			LCControlMessage lCControlMessage = new LCControlMessage(condition);
			if (lCControlMessage.Type == LCControlMessage.MaintenanceType.Error)
			{
				gridMain.Background = base.Resources["ErrorBackgroundBrush"] as LinearGradientBrush;
				gridMain.Background.Opacity = 0.3;
				imgLCControlBtn.Source = new ImageConverter().Convert(lCControlMessage.ImageSource) as ImageSource;
				btnLCControl.Style = base.Resources["ErrorButtonStyle"] as Style;
				txtDeviceStatus.Background = base.Resources["ErrorBackgroundBrush"] as LinearGradientBrush;
				if (InjectionAbortable)
				{
					this.AbortInjection?.Invoke(isManualAbort: false, InstrumentFacade.IsSkipVialAndContinue);
				}
			}
			else if (lCControlMessage.Type == LCControlMessage.MaintenanceType.Warning)
			{
				gridMain.Background = base.Resources["WarningBackgroundBrush"] as LinearGradientBrush;
				gridMain.Background.Opacity = 0.3;
				imgLCControlBtn.Source = new ImageConverter().Convert(lCControlMessage.ImageSource) as ImageSource;
				btnLCControl.Style = base.Resources["WarningButtonStyle"] as Style;
				txtDeviceStatus.Background = base.Resources["WarningBackgroundBrush"] as LinearGradientBrush;
			}
			else if (lCControlMessage.Type == LCControlMessage.MaintenanceType.Tip)
			{
				gridMain.Background = base.Resources["TipBackgroundBrush"] as LinearGradientBrush;
				gridMain.Background.Opacity = 0.3;
				imgLCControlBtn.Source = new ImageConverter().Convert(lCControlMessage.ImageSource) as ImageSource;
				btnLCControl.Style = base.Resources["TipButtonStyle"] as Style;
				txtDeviceStatus.Background = base.Resources["TipBackgroundBrush"] as LinearGradientBrush;
			}
			else
			{
				gridMain.Background = base.Resources["InfoBackgroundBrush"] as LinearGradientBrush;
				gridMain.Background.Opacity = 0.3;
				imgLCControlBtn.Source = new ImageConverter().Convert(lCControlMessage.ImageSource) as ImageSource;
				btnLCControl.Style = base.Resources["InfoButtonStyle"] as Style;
				txtDeviceStatus.Background = base.Resources["InfoBackgroundBrush"] as LinearGradientBrush;
			}
			tbAlert.Text = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", condition.Subject, condition.Description);
			tbAlert.ToolTip = tbAlert.Text;
			imgAlert.Source = new ImageConverter().Convert(lCControlMessage.ImageSource) as ImageSource;
			txtDeviceStatus.Visibility = Visibility.Visible;
		}
		else
		{
			SetAlertMessageCallback method = SetAlertMessage;
			txtDeviceStatus.Dispatcher.BeginInvoke(method, condition);
		}
	}

	private void _instrument_ActiveConditionClearEvent()
	{
		SetAlertMessage(null);
	}

	public void SetSamplePosition(string samplePos)
	{
		if (tbSamplePos.Dispatcher.CheckAccess())
		{
			tbSamplePos.Text = samplePos ?? "-";
			return;
		}
		SetSamplePositionCallback method = SetSamplePosition;
		tbSamplePos.Dispatcher.BeginInvoke(method, samplePos);
	}

	public void ClickConfirmButton()
	{
		if (dlgLCControl != null)
		{
			dlgLCControl.ClickConfirmButton();
		}
		else
		{
			InstrumentFacade.ActiveConditionClear();
		}
	}

	private void SetColumnSelectionEnabled(bool enabled)
	{
		comboAnalColumn.IsEnabled = enabled;
		comboPreColumn.IsEnabled = enabled;
	}

	private void MenuItemAbortInjection_Click(object sender, ExecutedRoutedEventArgs e)
	{
		IsManualInjectionAbort = true;
		this.AbortInjection?.Invoke(isManualAbort: true, InstrumentFacade.IsSkipVialAndContinue);
	}

	private void cb_AbortCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = injectionAbortable;
	}

	private void MenuItemEndTempWaitCommand_Click(object sender, ExecutedRoutedEventArgs e)
	{
		this.EndTempWait?.Invoke(this, EventArgs.Empty);
	}

	private void cb_EndTempWaitCommand(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = isEndTempWaitActive;
	}

	private void MenuItemResetSystemCommand_Click(object sender, ExecutedRoutedEventArgs e)
	{
		this.ResetSystem?.Invoke(this, EventArgs.Empty);
	}

	private void cb_ResetSystemCommand(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = systemResettable;
	}

	private void MenuItemClearErrorCommand_Click(object sender, ExecutedRoutedEventArgs e)
	{
		this.ClearError?.Invoke(this, EventArgs.Empty);
	}

	private void cb_ClearErrorCommand(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = isClearErrorable;
	}

	private void cb_PreferencesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = isPreferenceable;
	}

	private void MenuItemPreferences_Click(object sender, EventArgs e)
	{
		this.OnShowPreferences?.Invoke();
	}

	private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
	{
		new AboutWindow(_displayName, _versionText, _productVersion, _creationDate).ShowDialog(HelperExtensions.GetActiveWindow());
	}

	private void MenuItemShowLogbook_Click(object sender, EventArgs e)
	{
		this.OnShowLogBook?.Invoke();
	}

	private void MenuItemClearAlert_Click(object sender, EventArgs e)
	{
		txtDeviceStatus.Visibility = Visibility.Hidden;
		gridMain.Background = new SolidColorBrush(Colors.Transparent);
		txtLCControlBtn.Text = "LC Control";
		imgLCControlBtn.Source = new ImageConverter().Convert("Images/Settings_32.png") as ImageSource;
		btnLCControl.Style = base.Resources["BalticButtonStyle"] as Style;
	}

	private void DrawConnections()
	{
		ExpandoObject source = _instrument.BalticConfiguration.DiagramHideList;
		int i;
		for (i = 1; i <= source.Count(); i++)
		{
			try
			{
				string text = (string)source.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == i.ToString()).Value;
				if (text.Contains(_instrument.BalticConfiguration.Naming.ValveT))
				{
					comboPreColumn.Visibility = Visibility.Hidden;
					tbPreColumn.Visibility = Visibility.Hidden;
					break;
				}
			}
			catch (Exception)
			{
			}
		}
		System.Windows.Point point = pumpAControl.TranslatePoint(new System.Windows.Point(pumpAControl.ActualWidth, pumpAControl.ActualHeight / 2.0), canvasMain);
		System.Windows.Point point2 = pumpBControl.TranslatePoint(new System.Windows.Point(pumpBControl.ActualWidth, pumpBControl.ActualHeight / 2.0), canvasMain);
		System.Windows.Point point3 = plyMixTriangle.TranslatePoint(new System.Windows.Point(plyMixTriangle.ActualWidth, plyMixTriangle.ActualHeight / 2.0), canvasMain);
		System.Windows.Point point4 = ellipseMixLeft.TranslatePoint(new System.Windows.Point(0.0, ellipseMixLeft.ActualHeight / 2.0), canvasMain);
		System.Windows.Point point5 = comboPreColumn.TranslatePoint(new System.Windows.Point(0.0, comboPreColumn.ActualHeight / 2.0), canvasMain);
		System.Windows.Point point6 = comboAnalColumn.TranslatePoint(new System.Windows.Point(0.0, comboAnalColumn.ActualHeight / 2.0), canvasMain);
		System.Windows.Point point7 = imgSyringe.TranslatePoint(new System.Windows.Point(imgSyringe.ActualWidth / 2.0, imgSyringe.ActualHeight), canvasMain);
		PointCollection points = new PointCollection
		{
			new System.Windows.Point(point.X, point.Y),
			new System.Windows.Point(point.X + 15.0, point.Y),
			new System.Windows.Point(point.X + 15.0, point.Y + (point2.Y - point.Y) / 2.0),
			new System.Windows.Point(point.X + 25.0, point.Y + (point2.Y - point.Y) / 2.0)
		};
		RoundPolyline roundPolyline = new RoundPolyline
		{
			Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 112, 184)),
			StrokeThickness = 4.0,
			FillRule = FillRule.EvenOdd,
			Points = points,
			StrokeLineJoin = PenLineJoin.Round,
			SnapsToDevicePixels = true,
			Radius = 6.0
		};
		canvasMain.Children.Add(roundPolyline);
		PointCollection points2 = new PointCollection
		{
			new System.Windows.Point(point2.X, point2.Y),
			new System.Windows.Point(point2.X + 15.0, point2.Y),
			new System.Windows.Point(point2.X + 15.0, point2.Y + (point.Y - point2.Y) / 2.0),
			new System.Windows.Point(point2.X + 25.0, point.Y + (point2.Y - point.Y) / 2.0)
		};
		RoundPolyline element = new RoundPolyline
		{
			Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(193, 41, 45)),
			StrokeThickness = 4.0,
			FillRule = FillRule.EvenOdd,
			Points = points2,
			StrokeLineJoin = PenLineJoin.Round,
			SnapsToDevicePixels = true
		};
		roundPolyline.Radius = 6.0;
		canvasMain.Children.Add(element);
		PointCollection points3 = new PointCollection
		{
			new System.Windows.Point(point2.X + 20.0, point.Y + (point2.Y - point.Y) / 2.0),
			new System.Windows.Point(point4.X, point.Y + (point2.Y - point.Y) / 2.0)
		};
		mixPolylineLeft.Stroke = System.Windows.Media.Brushes.RosyBrown;
		mixPolylineLeft.StrokeThickness = 4.0;
		mixPolylineLeft.FillRule = FillRule.EvenOdd;
		mixPolylineLeft.Points = points3;
		mixPolylineLeft.StrokeLineJoin = PenLineJoin.Round;
		mixPolylineLeft.SnapsToDevicePixels = true;
		canvasMain.Children.Add(mixPolylineLeft);
		Panel.SetZIndex(mixPolylineLeft, 0);
		PointCollection points4 = new PointCollection
		{
			new System.Windows.Point(point3.X - 3.0, point.Y + (point2.Y - point.Y) / 2.0),
			new System.Windows.Point(point5.X, point.Y + (point2.Y - point.Y) / 2.0)
		};
		mixPolylineRight.Stroke = System.Windows.Media.Brushes.RosyBrown;
		mixPolylineRight.StrokeThickness = 4.0;
		mixPolylineRight.FillRule = FillRule.EvenOdd;
		mixPolylineRight.Points = points4;
		mixPolylineRight.StrokeLineJoin = PenLineJoin.Round;
		mixPolylineRight.SnapsToDevicePixels = true;
		canvasMain.Children.Add(mixPolylineRight);
		Panel.SetZIndex(mixPolylineRight, 1);
		PointCollection pointCollection = new PointCollection();
		if (comboPreColumn.IsVisible)
		{
			pointCollection.Add(new System.Windows.Point(point5.X + comboPreColumn.ActualWidth, point2.Y + (point.Y - point2.Y) / 2.0));
		}
		else
		{
			pointCollection.Add(new System.Windows.Point(point5.X, point2.Y + (point.Y - point2.Y) / 2.0));
		}
		pointCollection.Add(new System.Windows.Point(point6.X, point2.Y + (point.Y - point2.Y) / 2.0));
		columnPolyline.Stroke = System.Windows.Media.Brushes.RosyBrown;
		columnPolyline.StrokeThickness = 4.0;
		columnPolyline.FillRule = FillRule.EvenOdd;
		columnPolyline.Points = pointCollection;
		columnPolyline.StrokeLineJoin = PenLineJoin.Round;
		columnPolyline.SnapsToDevicePixels = true;
		canvasMain.Children.Add(columnPolyline);
		ArrowLine element2 = new ArrowLine
		{
			Stroke = System.Windows.Media.Brushes.Gray,
			StrokeThickness = 5.0,
			ArrowEnds = ArrowEnds.End,
			X1 = point7.X,
			Y1 = point7.Y,
			X2 = point7.X,
			Y2 = point.Y + (point2.Y - point.Y) / 2.0 - 6.0
		};
		canvasMain.Children.Add(element2);
		Panel.SetZIndex(element2, 0);
		UpdateMixColor(70);
	}

	public void LoadAndBindColumns(BalticColumnList columnList, string preColumnName, string analColumnName)
	{
		BalticColumnList list;
		if (!base.Dispatcher.CheckAccess())
		{
			list = new BalticColumnList(columnList);
			base.Dispatcher.BeginInvoke(new Action(Action));
			return;
		}
		BalticColumnList balticColumnList = new BalticColumnList();
		BalticColumnList balticColumnList2 = new BalticColumnList();
		foreach (Column column in columnList)
		{
			new ColumnAdapter(column);
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
		comboPreColumn.SelectedValuePath = "Name";
		comboAnalColumn.SelectedValuePath = "Name";
		object selectedValue = comboPreColumn.SelectedValue;
		object selectedValue2 = comboAnalColumn.SelectedValue;
		comboPreColumn.ItemsSource = balticColumnList2;
		comboAnalColumn.ItemsSource = balticColumnList;
		comboPreColumn.SelectedItem = balticColumnList2.SingleOrDefault((Column c) => c.Name.Equals(preColumnName));
		comboAnalColumn.SelectedItem = balticColumnList.SingleOrDefault((Column c) => c.Name.Equals(analColumnName));
		stkTrapWarn.Visibility = ((comboPreColumn.SelectedItem != null) ? Visibility.Hidden : Visibility.Visible);
		stkSepWarn.Visibility = ((comboAnalColumn.SelectedItem != null) ? Visibility.Hidden : Visibility.Visible);
		TrapColumnInfoControl.DataContext = (Column)comboPreColumn.SelectedItem;
		SepColumnInfoControl.DataContext = (Column)comboAnalColumn.SelectedItem;
		if (dlgLCControl != null && comboPreColumn.SelectedValue == selectedValue)
		{
			UpdateLCControlColumnTypes(Column.ColumnType.PreColumn);
		}
		if (dlgLCControl != null && comboAnalColumn.SelectedValue == selectedValue2)
		{
			UpdateLCControlColumnTypes(Column.ColumnType.AnalyticalColumn);
		}
		void Action()
		{
			LoadAndBindColumns(list, preColumnName, analColumnName);
		}
	}

	public void UpdateMixColor(int percentB)
	{
		mixPolylineLeft.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(99, 50, 138));
		mixPolylineRight.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(99, 50, 138));
		columnPolyline.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(99, 50, 138));
	}

	private void CheckShowCompositionConditions()
	{
		if (_instrument == null)
		{
			return;
		}
		SystemConditionManager conditions = _instrument.ShowCompositionConditions;
		Action callback = delegate
		{
			foreach (ShowCompositionCondition key in conditions.ShowCompositionConditions.Keys)
			{
				tbPercentB.Visibility = ((!key.IsShow) ? Visibility.Hidden : Visibility.Visible);
			}
			conditions.ShowCompositionConditions.Clear();
		};
		base.Dispatcher.Invoke(callback);
	}

	private void UpdateLCControlColumnTypes(Column.ColumnType chromatographyFunction)
	{
		if (chromatographyFunction == Column.ColumnType.PreColumn || Column.ColumnType.Both == chromatographyFunction)
		{
			Column column = (Column)comboPreColumn.SelectedItem;
			dlgLCControl.TrapColumnType = ((column != null) ? new ColumnAdapter(column) : null);
		}
		if (Column.ColumnType.AnalyticalColumn == chromatographyFunction || Column.ColumnType.Both == chromatographyFunction)
		{
			Column column2 = (Column)comboAnalColumn.SelectedItem;
			dlgLCControl.SeparatorColumnType = ((column2 != null) ? new ColumnAdapter(column2) : null);
		}
	}

	private void ColumnSelectionChanged(Column.ColumnType chromatographyFunction)
	{
		if (dlgLCControl != null)
		{
			UpdateLCControlColumnTypes(chromatographyFunction);
		}
		string precolumnname = (string)comboPreColumn.SelectedValue;
		string analcolumnname = (string)comboAnalColumn.SelectedValue;
		TrapColumnInfoControl.DataContext = (Column)comboPreColumn.SelectedItem;
		SepColumnInfoControl.DataContext = (Column)comboAnalColumn.SelectedItem;
		stkTrapWarn.Visibility = ((comboPreColumn.SelectedItem != null) ? Visibility.Hidden : Visibility.Visible);
		stkSepWarn.Visibility = ((comboAnalColumn.SelectedItem != null) ? Visibility.Hidden : Visibility.Visible);
		if (comboAnalColumn.SelectedItem == null)
		{
			stkSepWarn.Visibility = Visibility.Visible;
		}
		BalticWpfColumnEventArgs args = new BalticWpfColumnEventArgs(precolumnname, analcolumnname, chromatographyFunction);
		this.OnColumnSelection?.Invoke(this, args);
	}

	private void comboPreColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		ColumnSelectionChanged(Column.ColumnType.PreColumn);
	}

	private void comboAnalColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		ColumnSelectionChanged(Column.ColumnType.AnalyticalColumn);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (dlgLCControl != null)
		{
			dlgLCControl.Close();
			dlgLCControl = null;
		}
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

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/mainusercontrol.xaml", UriKind.Relative);
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
			canvasMain = (MainUserControl)target;
			canvasMain.Loaded += Init;
			canvasMain.Unloaded += OnUnloaded;
			break;
		case 2:
			((CommandBinding)target).CanExecute += cb_AbortCommand_CanExecute;
			((CommandBinding)target).Executed += MenuItemAbortInjection_Click;
			break;
		case 3:
			((CommandBinding)target).CanExecute += cb_EndTempWaitCommand;
			((CommandBinding)target).Executed += MenuItemEndTempWaitCommand_Click;
			break;
		case 4:
			((CommandBinding)target).CanExecute += cb_ResetSystemCommand;
			((CommandBinding)target).Executed += MenuItemResetSystemCommand_Click;
			break;
		case 5:
			((CommandBinding)target).CanExecute += cb_ClearErrorCommand;
			((CommandBinding)target).Executed += MenuItemClearErrorCommand_Click;
			break;
		case 6:
			((CommandBinding)target).CanExecute += cb_PreferencesCommand_CanExecute;
			((CommandBinding)target).Executed += MenuItemPreferences_Click;
			break;
		case 7:
			menuitemEndTempWait = (MenuItem)target;
			break;
		case 8:
			menuitemClearError = (MenuItem)target;
			break;
		case 9:
			menuitemResetSystem = (MenuItem)target;
			break;
		case 10:
			((MenuItem)target).Click += MenuItemShowLogbook_Click;
			break;
		case 11:
			((MenuItem)target).Click += MenuItemAbout_Click;
			break;
		case 12:
			gridMain = (Grid)target;
			break;
		case 13:
			bdrDeviceStatus = (Border)target;
			break;
		case 14:
			txtHyStarStatus = (TextBlock)target;
			break;
		case 15:
			txtPumpStatus = (TextBlock)target;
			break;
		case 16:
			txtDeviceStatus = (TextBlock)target;
			break;
		case 17:
			dpDeviceStatus = (DockPanel)target;
			break;
		case 18:
			imgAlert = (System.Windows.Controls.Image)target;
			break;
		case 19:
			tbAlert = (TextBlock)target;
			break;
		case 20:
			pumpAControl = (PumpControl)target;
			break;
		case 21:
			pumpBControl = (PumpControl)target;
			break;
		case 22:
			ellipseMixLeft = (Ellipse)target;
			break;
		case 23:
			tbFlow = (TextBlock)target;
			break;
		case 24:
			tbPercentB = (TextBlock)target;
			break;
		case 25:
			plyMixTriangle = (Polygon)target;
			break;
		case 26:
			imgSyringe = (System.Windows.Controls.Image)target;
			break;
		case 27:
			tbSamplePos = (TextBlock)target;
			break;
		case 28:
			trapColPanel = (DockPanel)target;
			break;
		case 29:
			comboPreColumn = (ComboBox)target;
			comboPreColumn.SelectionChanged += comboPreColumn_SelectionChanged;
			break;
		case 31:
			tbPreColumn = (TextBlock)target;
			break;
		case 32:
			TrapColumnInfoControl = (ColumnInfoUserControl)target;
			break;
		case 33:
			stkTrapWarn = (StackPanel)target;
			break;
		case 34:
			sepColPanel = (DockPanel)target;
			break;
		case 35:
			comboAnalColumn = (ComboBox)target;
			comboAnalColumn.SelectionChanged += comboAnalColumn_SelectionChanged;
			break;
		case 37:
			SepColumnInfoControl = (ColumnInfoUserControl)target;
			break;
		case 38:
			stkSepWarn = (StackPanel)target;
			break;
		case 39:
			idleLED = (Led)target;
			break;
		case 40:
			idleFlowPopupText = (TextBlock)target;
			break;
		case 41:
			idleFlowText = (TextBlock)target;
			break;
		case 42:
			idleFlowCompText = (TextBlock)target;
			break;
		case 43:
			idleFlowViaTrapText = (TextBlock)target;
			break;
		case 44:
			btnLCControl = (Button)target;
			btnLCControl.Click += ButtonClicked;
			break;
		case 45:
			imgLCControlBtn = (System.Windows.Controls.Image)target;
			break;
		case 46:
			txtLCControlBtn = (TextBlock)target;
			break;
		case 47:
			gridOven = (Grid)target;
			break;
		case 48:
			tbOvenTemp = (TextBlock)target;
			break;
		case 49:
			imgHeatWaves = (System.Windows.Controls.Image)target;
			break;
		case 50:
			tbTempSetPt = (TextBlock)target;
			break;
		case 51:
			tbTempMeasured = (TextBlock)target;
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
		case 30:
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
		case 36:
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
