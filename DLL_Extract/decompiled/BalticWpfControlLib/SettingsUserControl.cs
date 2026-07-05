using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using BalticWpfControlLib.Controls;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib;

public class SettingsUserControl : UserControl, INotifyPropertyChanged, IComponentConnector
{
	public class ComboBoxItemTrayTemperature
	{
		public BalticHWProfile.ASProfile.TrayTemperature TrayTemperatureEnum { get; set; }

		public string ValueString { get; set; }
	}

	public delegate void ValidationUpdate(bool isValid);

	public delegate void OkCancelButtonUpdate(bool isEnabled);

	private int _noOfErrorsOnScreen;

	private readonly BalticInstrumentFacade _instrument;

	private bool _isAppKeyError;

	private bool _isShowOvenInfo;

	private bool _isShowTrayError;

	private bool _isOvenInstalled;

	private bool _isShowTrapInjValve;

	private bool _isService;

	private readonly string _privatePath;

	private BalticPreferences _preferences;

	private int _maxPumpValveShifts;

	private int _maxTIValveShifts;

	private readonly Window _parent;

	private readonly double _version = 1.0;

	private readonly List<BalticHWProfile.ASProfile.TrayType> _trayTypeList = new List<BalticHWProfile.ASProfile.TrayType>();

	private List<ComboBoxItemTrayTemperature> _trayTemperatureListEnum = new List<ComboBoxItemTrayTemperature>(4)
	{
		new ComboBoxItemTrayTemperature
		{
			TrayTemperatureEnum = BalticHWProfile.ASProfile.TrayTemperature.Off,
			ValueString = "Off"
		},
		new ComboBoxItemTrayTemperature
		{
			TrayTemperatureEnum = BalticHWProfile.ASProfile.TrayTemperature.ExtraLow,
			ValueString = "Extra Low"
		},
		new ComboBoxItemTrayTemperature
		{
			TrayTemperatureEnum = BalticHWProfile.ASProfile.TrayTemperature.Low,
			ValueString = "Low"
		},
		new ComboBoxItemTrayTemperature
		{
			TrayTemperatureEnum = BalticHWProfile.ASProfile.TrayTemperature.Medium,
			ValueString = "Medium"
		}
	};

	private ObservableCollection<int> _maxPumpPressures = new ObservableCollection<int> { 1000, 1600 };

	internal TextBlock txtService;

	internal TextBox txtAppKey;

	internal TextBlock tbAppKeyError;

	internal IPControl ipctrlASIP;

	internal ComboBox comboTemperature;

	internal ComboBox comboSlot1;

	internal ComboBox comboSlot2;

	internal StackPanel stkTrayWarn;

	internal TextBlock txtTrapInjTitle;

	internal IntegerTextBox txtMaxTIShifts;

	internal TextBlock txtPumpShiftTitle;

	internal IntegerTextBox txtMaxPumpShifts;

	internal Image imgWarn;

	internal TextBlock tbMaxPressure;

	internal StackPanel spMaxPressure;

	internal ComboBox comboMaxPumpPressure;

	internal TextBlock tbPressureUnit;

	internal CheckBox chkIsOvenConnected;

	internal StackPanel stkOvenInfo;

	internal GroupBox grpConnect;

	internal TextBox txtConnectStatus;

	internal Button btnConnect;

	internal Viewbox progControl;

	internal TextBox txtHardware;

	private bool _contentLoaded;

	public List<ComboBoxItemTrayTemperature> TrayTemperatureListEnum
	{
		get
		{
			return _trayTemperatureListEnum;
		}
		set
		{
			_trayTemperatureListEnum = value;
			NotifyPropertyChanged("TrayTemperatureListEnum");
		}
	}

	public int MaxTIValveShifts
	{
		get
		{
			return _maxTIValveShifts;
		}
		set
		{
			_maxTIValveShifts = value;
			NotifyPropertyChanged("MaxTIValveShifts");
		}
	}

	public int MaxPumpValveShifts
	{
		get
		{
			return _maxPumpValveShifts;
		}
		set
		{
			_maxPumpValveShifts = value;
			if (_version < 1.4)
			{
				_maxTIValveShifts = _maxPumpValveShifts;
				NotifyPropertyChanged("MaxTIValveShifts");
			}
			NotifyPropertyChanged("MaxPumpValveShifts");
		}
	}

	public bool IsShowOvenInfo
	{
		get
		{
			return _isShowOvenInfo;
		}
		set
		{
			_isShowOvenInfo = value;
			NotifyPropertyChanged("IsShowOvenInfo");
		}
	}

	public bool IsShowTrayError
	{
		get
		{
			return _isShowTrayError;
		}
		set
		{
			_isShowTrayError = value;
			NotifyPropertyChanged("IsShowTrayError");
		}
	}

	public bool IsShowTrapInjValve
	{
		get
		{
			return _isShowTrapInjValve;
		}
		set
		{
			_isShowTrapInjValve = value;
			NotifyPropertyChanged("IsShowTrapInjValve");
		}
	}

	public bool IsColumnOvenConnected
	{
		get
		{
			return Settings.IsColumnOvenConnected;
		}
		set
		{
			Settings.IsColumnOvenConnected = value;
			if (value)
			{
				IsShowOvenInfo = false;
			}
			else if (_isOvenInstalled)
			{
				IsShowOvenInfo = true;
			}
			NotifyPropertyChanged("IsColumnOvenConnected");
			NotifyPropertyChanged("IsShowOvenInfo");
		}
	}

	public bool IsService
	{
		get
		{
			return _isService;
		}
		set
		{
			_isService = value;
			NotifyPropertyChanged("IsService");
			NotifyPropertyChanged("IsCTCSimulateVisible");
		}
	}

	public bool IsCTCSimulateVisible
	{
		get
		{
			if (!_isService)
			{
				return Settings.IsCTCSimulate;
			}
			return true;
		}
	}

	public ObservableCollection<int> MaxPumpPressures
	{
		get
		{
			return _maxPumpPressures;
		}
		set
		{
			_maxPumpPressures = value;
			NotifyPropertyChanged("MaxPumpPressures");
		}
	}

	public BindableBalticHardware Settings { get; }

	public event PropertyChangedEventHandler PropertyChanged;

	public event ValidationUpdate ValidationUpdateEvent;

	public event OkCancelButtonUpdate OkCancelButtonUpdateEvent;

	private void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public SettingsUserControl(Window parent, string privatePath, BalticHWProfile hardwareProfile, BalticInstrumentFacade instrument, string displayName)
	{
		Settings = new BindableBalticHardware(hardwareProfile);
		_instrument = instrument;
		_privatePath = privatePath;
		_parent = parent;
		InitializeComponent();
		LoadPreferences();
		InitializeTrayTypes();
		if (_preferences.Version.Length > 0 && double.TryParse(_preferences.Version, NumberStyles.Any, CultureInfo.InvariantCulture, out _version))
		{
			_isShowTrapInjValve = !(_version < 1.4);
			if ((int)(_version * 10.0) == 10)
			{
				grpConnect.Header = "CHECK CONNECTION - " + displayName + " 1";
			}
			else if ((int)(_version * 10.0) == 20)
			{
				grpConnect.Header = "CHECK CONNECTION - " + displayName + " 2";
			}
			else if ((int)(_version * 10.0) == 0 || (int)(_version * 10.0) == 30)
			{
				grpConnect.Header = "CHECK CONNECTION - " + displayName;
			}
			else
			{
				grpConnect.Header = "CHECK CONNECTION - " + displayName + " upgraded";
			}
		}
		if (!IsShowTrapInjValve)
		{
			_maxTIValveShifts = _maxPumpValveShifts;
		}
		List<string> list = _trayTypeList.Select((BalticHWProfile.ASProfile.TrayType trayType) => trayType.ToString()).ToList();
		if (!_preferences.IsProteo144Support)
		{
			list = list.FindAll((string x) => !x.ToLower().Contains("proteochip144"));
		}
		if (!_preferences.IsProteo36Support)
		{
			list = list.FindAll((string x) => !x.ToLower().Contains("proteochip36"));
		}
		if (instrument.BalticConfiguration.AdditionalTrayTypes is ExpandoObject source)
		{
			int i;
			for (i = 0; i < source.Count(); i++)
			{
				try
				{
					foreach (KeyValuePair<string, object> item in (IEnumerable<KeyValuePair<string, object>>)(ExpandoObject)source.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == i.ToString()).Value)
					{
						if (item.Key == "1")
						{
							list.Add((string)item.Value);
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}
		base.DataContext = this;
		if (Settings.TrayTemperature == BalticHWProfile.ASProfile.TrayTemperature.High)
		{
			Settings.TrayTemperature = BalticHWProfile.ASProfile.TrayTemperature.Medium;
		}
		comboTemperature.ItemsSource = TrayTemperatureListEnum;
		comboSlot1.ItemsSource = list;
		comboSlot2.ItemsSource = list;
		txtConnectStatus.Text = "not connected";
		txtConnectStatus.Background = Brushes.LightGray;
		_isService = _instrument.CheckForServiceMode();
	}

	private void Validation_Error(object sender, ValidationErrorEventArgs e)
	{
		if (e.Action == ValidationErrorEventAction.Added)
		{
			_noOfErrorsOnScreen++;
		}
		else
		{
			_noOfErrorsOnScreen--;
		}
		this.ValidationUpdateEvent?.Invoke(_noOfErrorsOnScreen == 0);
	}

	private void comboSlot1_selectionChanged(object sender, SelectionChangedEventArgs e)
	{
		ValidateTraySelection();
	}

	private void comboSlot2_selectionChanged(object sender, SelectionChangedEventArgs e)
	{
		ValidateTraySelection();
	}

	private void ValidateTraySelection()
	{
		if (comboSlot1.SelectedItem != null && comboSlot2.SelectedItem != null)
		{
			if (comboSlot1.SelectedItem.ToString() == "VT54" && (comboSlot2.SelectedItem.ToString() == "MTP96" || comboSlot2.SelectedItem.ToString() == "MTP384" || comboSlot2.SelectedItem.ToString() == "ProteoChip144" || comboSlot2.SelectedItem.ToString() == "ProteoChip36"))
			{
				IsShowTrayError = true;
			}
			else
			{
				IsShowTrayError = false;
			}
			this.ValidationUpdateEvent?.Invoke(!_isAppKeyError && !IsShowTrayError);
		}
	}

	private void TxtAppKey_TextChanged(object sender, TextChangedEventArgs e)
	{
		try
		{
			uint num = Convert.ToUInt32(txtAppKey.Text, 16);
			uint special = (num >> 12) & 0xFu;
			special++;
			uint num2 = (num >> 8) & 0xFu;
			num2++;
			uint sample = (num >> 4) & 0xFu;
			sample++;
			uint num3 = num & 0xFu;
			try
			{
				_ = (string)((ExpandoObject)_instrument.BalticConfiguration.ApplicationKey.Sample).FirstOrDefault((KeyValuePair<string, object> x) => x.Key == sample.ToString()).Value;
				_ = (string)((ExpandoObject)_instrument.BalticConfiguration.ApplicationKey.Special).FirstOrDefault((KeyValuePair<string, object> x) => x.Key == special.ToString()).Value;
			}
			catch (Exception)
			{
				tbAppKeyError.Visibility = Visibility.Visible;
				_isAppKeyError = true;
				this.ValidationUpdateEvent?.Invoke(!_isAppKeyError && !IsShowTrayError);
				throw;
			}
			if (num2 + sample + special - 3 == num3)
			{
				_isAppKeyError = false;
				tbAppKeyError.Visibility = Visibility.Hidden;
			}
			else
			{
				tbAppKeyError.Visibility = Visibility.Visible;
				_isAppKeyError = true;
			}
			this.ValidationUpdateEvent?.Invoke(!_isAppKeyError && !IsShowTrayError);
		}
		catch (Exception)
		{
		}
	}

	private void btnConnect_Click(object sender, RoutedEventArgs e)
	{
		if (!_instrument.IsConnected)
		{
			txtHardware.Text = "";
			txtConnectStatus.Text = "Connecting...";
			txtConnectStatus.Background = Brushes.LightGray;
			progControl.Visibility = Visibility.Visible;
			((Action)delegate
			{
				new Timer(TimerProc).Change(0, -1);
			}).BeginInvoke(null, null);
		}
	}

	private void TimerProc(object state)
	{
		try
		{
			((Timer)state).Dispose();
			this.OkCancelButtonUpdateEvent?.Invoke(isEnabled: false);
			_instrument.IsColumnOvenConnected = IsColumnOvenConnected;
			_instrument.SetConnectionProperties(Settings.PumpHost, Settings.AutosamplerIP, Settings.IsSimulate, Settings.IsCTCSimulate);
			_instrument.CheckConnectionEvent += _instrument_CheckConnectionEvent;
			_instrument.Connect(24, base.Dispatcher);
			_instrument.CheckConnectionEvent -= _instrument_CheckConnectionEvent;
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				progControl.Visibility = Visibility.Hidden;
			});
			this.OkCancelButtonUpdateEvent?.Invoke(isEnabled: true);
			this.ValidationUpdateEvent?.Invoke(!_isAppKeyError && !IsShowTrayError);
		}
		catch (Exception)
		{
		}
	}

	private void _instrument_CheckConnectionEvent(object sender, BalticInstrumentFacade.ConnectionEventArgs e)
	{
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			if (e.IsConnected)
			{
				txtConnectStatus.Text = "connected";
				txtConnectStatus.Background = Brushes.LightGreen;
				btnConnect.IsEnabled = false;
				txtHardware.Text = "PAL firmware version: " + e.PALFirmwareVersion + "\r\nPump firmware version: " + e.ZirconiumFirmwareVersion + "\r\nColumn oven: " + (e.IsOvenInstalled ? "installed" : "not installed");
			}
			else
			{
				txtConnectStatus.Text = "not connected";
				txtConnectStatus.Background = Brushes.LightGray;
				txtHardware.Text = ((e.ErrorException != null) ? ("unable to connect to device: " + e.ErrorException.Message) : "unable to connect to device");
			}
			_isOvenInstalled = e.IsOvenInstalled;
			IsShowOvenInfo = e.IsOvenInstalled && !IsColumnOvenConnected;
		});
	}

	public void ValidatePreferences()
	{
		base.Dispatcher.Invoke(delegate
		{
			if (MaxPumpValveShifts != _preferences.Pump.MaxShifts || MaxTIValveShifts != _preferences.MaxTIShifts)
			{
				if (new ConfirmWindow("Would you like to save changes to the PM Maximum shift value(s) ?").ShowDialog(HelperExtensions.GetActiveWindow()) == false)
				{
					MaxPumpValveShifts = _preferences.Pump.MaxShifts;
					MaxTIValveShifts = _preferences.MaxTIShifts;
				}
				else
				{
					_preferences.Pump.MaxShifts = MaxPumpValveShifts;
					_preferences.MaxTIShifts = MaxTIValveShifts;
				}
				try
				{
					string value = Utility.ToXML(_preferences);
					using StreamWriter streamWriter = File.CreateText(Path.Combine(_privatePath, "Preferences.xml"));
					streamWriter.Write(value);
					streamWriter.Close();
				}
				catch (IOException)
				{
				}
			}
		});
	}

	public void WritePreferences()
	{
		try
		{
			string value = Utility.ToXML(_preferences);
			using StreamWriter streamWriter = File.CreateText(Path.Combine(_privatePath, "Preferences.xml"));
			streamWriter.Write(value);
			streamWriter.Close();
		}
		catch (IOException)
		{
		}
	}

	private void LoadPreferences()
	{
		try
		{
			using StreamReader streamReader = new StreamReader(Path.Combine(_privatePath, "Preferences.xml"));
			string xmlString = streamReader.ReadToEnd();
			_preferences = (BalticPreferences)Utility.FromXML(xmlString, typeof(BalticPreferences));
			MaxPumpValveShifts = _preferences.Pump.MaxShifts;
			MaxTIValveShifts = _preferences.MaxTIShifts;
		}
		catch (IOException)
		{
		}
	}

	private void EditCapillaries_Click(object sender, RoutedEventArgs e)
	{
		CapillariesDiagramWindow capillariesDiagramWindow = new CapillariesDiagramWindow(_instrument, Settings.ConnectionProfile, _preferences.Capillaries, Settings.IsPressurePSI)
		{
			Owner = _parent
		};
		capillariesDiagramWindow.ShowDialog(HelperExtensions.GetActiveWindow());
		if (!capillariesDiagramWindow.DialogResult.GetValueOrDefault())
		{
			return;
		}
		foreach (BalticHWProfile.CapillaryItem item in capillariesDiagramWindow.Controller.HwCapillaries)
		{
			BalticHWProfile.CapillaryItem capillaryItem = Settings.ConnectionProfile.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == item.LinkName);
			if (capillaryItem != null)
			{
				capillaryItem.Length = item.Length;
				capillaryItem.ID = item.ID;
			}
		}
	}

	private void InitializeTrayTypes()
	{
		_trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.VT54);
		_trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.MTP96);
		_trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.MTP384);
		_trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.ProteoChip144);
		_trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.ProteoChip36);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/settingsusercontrol.xaml", UriKind.Relative);
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
			((SettingsUserControl)target).AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(Validation_Error));
			break;
		case 2:
			txtService = (TextBlock)target;
			break;
		case 3:
			txtAppKey = (TextBox)target;
			txtAppKey.TextChanged += TxtAppKey_TextChanged;
			break;
		case 4:
			tbAppKeyError = (TextBlock)target;
			break;
		case 5:
			ipctrlASIP = (IPControl)target;
			break;
		case 6:
			comboTemperature = (ComboBox)target;
			break;
		case 7:
			comboSlot1 = (ComboBox)target;
			comboSlot1.SelectionChanged += comboSlot1_selectionChanged;
			break;
		case 8:
			comboSlot2 = (ComboBox)target;
			comboSlot2.SelectionChanged += comboSlot2_selectionChanged;
			break;
		case 9:
			stkTrayWarn = (StackPanel)target;
			break;
		case 10:
			txtTrapInjTitle = (TextBlock)target;
			break;
		case 11:
			txtMaxTIShifts = (IntegerTextBox)target;
			break;
		case 12:
			txtPumpShiftTitle = (TextBlock)target;
			break;
		case 13:
			txtMaxPumpShifts = (IntegerTextBox)target;
			break;
		case 14:
			imgWarn = (Image)target;
			break;
		case 15:
			tbMaxPressure = (TextBlock)target;
			break;
		case 16:
			spMaxPressure = (StackPanel)target;
			break;
		case 17:
			comboMaxPumpPressure = (ComboBox)target;
			break;
		case 18:
			tbPressureUnit = (TextBlock)target;
			break;
		case 19:
			((Button)target).Click += EditCapillaries_Click;
			break;
		case 20:
			chkIsOvenConnected = (CheckBox)target;
			break;
		case 21:
			stkOvenInfo = (StackPanel)target;
			break;
		case 22:
			grpConnect = (GroupBox)target;
			break;
		case 23:
			txtConnectStatus = (TextBox)target;
			break;
		case 24:
			btnConnect = (Button)target;
			btnConnect.Click += btnConnect_Click;
			break;
		case 25:
			progControl = (Viewbox)target;
			break;
		case 26:
			txtHardware = (TextBox)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
