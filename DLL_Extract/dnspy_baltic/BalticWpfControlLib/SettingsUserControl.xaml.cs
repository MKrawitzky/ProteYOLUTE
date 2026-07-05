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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using BalticWpfControlLib.Controls;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Microsoft.CSharp.RuntimeBinder;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib
{
	// Token: 0x0200003F RID: 63
	public partial class SettingsUserControl : UserControl, INotifyPropertyChanged
	{
		// Auto-generated callsite cache class
		private static class _co_67
		{
			public static dynamic _cp_0;
			public static dynamic _cp_1;
			public static dynamic _cp_2;
			public static dynamic _cp_3;
			public static dynamic _cp_4;
			public static dynamic _cp_5;
		}

		// Auto-generated callsite cache class
		private static class _co_59
		{
			public static dynamic _cp_0;
		}

		// Token: 0x1400003C RID: 60
		// (add) Token: 0x0600037D RID: 893 RVA: 0x000166EC File Offset: 0x000148EC
		// (remove) Token: 0x0600037E RID: 894 RVA: 0x00016724 File Offset: 0x00014924
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x0600037F RID: 895 RVA: 0x00016759 File Offset: 0x00014959
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x1400003D RID: 61
		// (add) Token: 0x06000380 RID: 896 RVA: 0x00016774 File Offset: 0x00014974
		// (remove) Token: 0x06000381 RID: 897 RVA: 0x000167AC File Offset: 0x000149AC
		public event SettingsUserControl.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x1400003E RID: 62
		// (add) Token: 0x06000382 RID: 898 RVA: 0x000167E4 File Offset: 0x000149E4
		// (remove) Token: 0x06000383 RID: 899 RVA: 0x0001681C File Offset: 0x00014A1C
		public event SettingsUserControl.OkCancelButtonUpdate OkCancelButtonUpdateEvent;

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x06000384 RID: 900 RVA: 0x00016851 File Offset: 0x00014A51
		// (set) Token: 0x06000385 RID: 901 RVA: 0x00016859 File Offset: 0x00014A59
		public List<SettingsUserControl.ComboBoxItemTrayTemperature> TrayTemperatureListEnum
		{
			get
			{
				return this._trayTemperatureListEnum;
			}
			set
			{
				this._trayTemperatureListEnum = value;
				this.NotifyPropertyChanged("TrayTemperatureListEnum");
			}
		}

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x06000386 RID: 902 RVA: 0x0001686D File Offset: 0x00014A6D
		// (set) Token: 0x06000387 RID: 903 RVA: 0x00016875 File Offset: 0x00014A75
		public int MaxTIValveShifts
		{
			get
			{
				return this._maxTIValveShifts;
			}
			set
			{
				this._maxTIValveShifts = value;
				this.NotifyPropertyChanged("MaxTIValveShifts");
			}
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x06000388 RID: 904 RVA: 0x00016889 File Offset: 0x00014A89
		// (set) Token: 0x06000389 RID: 905 RVA: 0x00016891 File Offset: 0x00014A91
		public int MaxPumpValveShifts
		{
			get
			{
				return this._maxPumpValveShifts;
			}
			set
			{
				this._maxPumpValveShifts = value;
				if (this._version < 1.4)
				{
					this._maxTIValveShifts = this._maxPumpValveShifts;
					this.NotifyPropertyChanged("MaxTIValveShifts");
				}
				this.NotifyPropertyChanged("MaxPumpValveShifts");
			}
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x0600038A RID: 906 RVA: 0x000168CD File Offset: 0x00014ACD
		// (set) Token: 0x0600038B RID: 907 RVA: 0x000168D5 File Offset: 0x00014AD5
		public bool IsShowOvenInfo
		{
			get
			{
				return this._isShowOvenInfo;
			}
			set
			{
				this._isShowOvenInfo = value;
				this.NotifyPropertyChanged("IsShowOvenInfo");
			}
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x0600038C RID: 908 RVA: 0x000168E9 File Offset: 0x00014AE9
		// (set) Token: 0x0600038D RID: 909 RVA: 0x000168F1 File Offset: 0x00014AF1
		public bool IsShowTrayError
		{
			get
			{
				return this._isShowTrayError;
			}
			set
			{
				this._isShowTrayError = value;
				this.NotifyPropertyChanged("IsShowTrayError");
			}
		}

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x0600038E RID: 910 RVA: 0x00016905 File Offset: 0x00014B05
		// (set) Token: 0x0600038F RID: 911 RVA: 0x0001690D File Offset: 0x00014B0D
		public bool IsShowTrapInjValve
		{
			get
			{
				return this._isShowTrapInjValve;
			}
			set
			{
				this._isShowTrapInjValve = value;
				this.NotifyPropertyChanged("IsShowTrapInjValve");
			}
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x06000390 RID: 912 RVA: 0x00016921 File Offset: 0x00014B21
		// (set) Token: 0x06000391 RID: 913 RVA: 0x0001692E File Offset: 0x00014B2E
		public bool IsColumnOvenConnected
		{
			get
			{
				return this.Settings.IsColumnOvenConnected;
			}
			set
			{
				this.Settings.IsColumnOvenConnected = value;
				if (value)
				{
					this.IsShowOvenInfo = false;
				}
				else if (this._isOvenInstalled)
				{
					this.IsShowOvenInfo = true;
				}
				this.NotifyPropertyChanged("IsColumnOvenConnected");
				this.NotifyPropertyChanged("IsShowOvenInfo");
			}
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x06000392 RID: 914 RVA: 0x0001696D File Offset: 0x00014B6D
		// (set) Token: 0x06000393 RID: 915 RVA: 0x00016975 File Offset: 0x00014B75
		public bool IsService
		{
			get
			{
				return this._isService;
			}
			set
			{
				this._isService = value;
				this.NotifyPropertyChanged("IsService");
				this.NotifyPropertyChanged("IsCTCSimulateVisible");
			}
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x06000394 RID: 916 RVA: 0x00016994 File Offset: 0x00014B94
		public bool IsCTCSimulateVisible
		{
			get
			{
				return this._isService || this.Settings.IsCTCSimulate;
			}
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x06000395 RID: 917 RVA: 0x000169AB File Offset: 0x00014BAB
		// (set) Token: 0x06000396 RID: 918 RVA: 0x000169B3 File Offset: 0x00014BB3
		public ObservableCollection<int> MaxPumpPressures
		{
			get
			{
				return this._maxPumpPressures;
			}
			set
			{
				this._maxPumpPressures = value;
				this.NotifyPropertyChanged("MaxPumpPressures");
			}
		}

		// Token: 0x06000397 RID: 919 RVA: 0x000169C8 File Offset: 0x00014BC8
		public SettingsUserControl(Window parent, string privatePath, BalticHWProfile hardwareProfile, BalticInstrumentFacade instrument, string displayName)
		{
			this.Settings = new BindableBalticHardware(hardwareProfile);
			this._instrument = instrument;
			this._privatePath = privatePath;
			this._parent = parent;
			this.InitializeComponent();
			this.LoadPreferences();
			this.InitializeTrayTypes();
			if (this._preferences.Version.Length > 0 && double.TryParse(this._preferences.Version, NumberStyles.Any, CultureInfo.InvariantCulture, out this._version))
			{
				this._isShowTrapInjValve = this._version >= 1.4;
				if ((int)(this._version * 10.0) == 10)
				{
					this.grpConnect.Header = "CHECK CONNECTION - " + displayName + " 1";
				}
				else if ((int)(this._version * 10.0) == 20)
				{
					this.grpConnect.Header = "CHECK CONNECTION - " + displayName + " 2";
				}
				else if ((int)(this._version * 10.0) == 0 || (int)(this._version * 10.0) == 30)
				{
					this.grpConnect.Header = "CHECK CONNECTION - " + displayName;
				}
				else
				{
					this.grpConnect.Header = "CHECK CONNECTION - " + displayName + " upgraded";
				}
			}
			if (!this.IsShowTrapInjValve)
			{
				this._maxTIValveShifts = this._maxPumpValveShifts;
			}
			List<string> trayTypes = this._trayTypeList.Select((BalticHWProfile.ASProfile.TrayType trayType) => trayType.ToString()).ToList<string>();
			if (!this._preferences.IsProteo144Support)
			{
				trayTypes = trayTypes.FindAll((string x) => !x.ToLower().Contains("proteochip144"));
			}
			if (!this._preferences.IsProteo36Support)
			{
				trayTypes = trayTypes.FindAll((string x) => !x.ToLower().Contains("proteochip36"));
			}
			if (SettingsUserControl._co_59._cp_0 == null)
			{
				SettingsUserControl._co_59._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "AdditionalTrayTypes", typeof(SettingsUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
			}
			ExpandoObject addTrayTypes = SettingsUserControl._co_59._cp_0.Target(SettingsUserControl._co_59._cp_0, instrument.BalticConfiguration) as ExpandoObject;
			if (addTrayTypes != null)
			{
				int j;
				int i;
				for (i = 0; i < addTrayTypes.Count<KeyValuePair<string, object>>(); i = j + 1)
				{
					try
					{
						foreach (KeyValuePair<string, object> item in ((IEnumerable<KeyValuePair<string, object>>)((ExpandoObject)addTrayTypes.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == i.ToString()).Value)))
						{
							if (item.Key == "1")
							{
								trayTypes.Add((string)item.Value);
							}
						}
					}
					catch (Exception)
					{
					}
					j = i;
				}
			}
			base.DataContext = this;
			if (this.Settings.TrayTemperature == BalticHWProfile.ASProfile.TrayTemperature.High)
			{
				this.Settings.TrayTemperature = BalticHWProfile.ASProfile.TrayTemperature.Medium;
			}
			this.comboTemperature.ItemsSource = this.TrayTemperatureListEnum;
			this.comboSlot1.ItemsSource = trayTypes;
			this.comboSlot2.ItemsSource = trayTypes;
			this.txtConnectStatus.Text = "not connected";
			this.txtConnectStatus.Background = Brushes.LightGray;
			this._isService = this._instrument.CheckForServiceMode();
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x06000398 RID: 920 RVA: 0x00016E2C File Offset: 0x0001502C
		public BindableBalticHardware Settings { get; }

		// Token: 0x06000399 RID: 921 RVA: 0x00016E34 File Offset: 0x00015034
		private void Validation_Error(object sender, ValidationErrorEventArgs e)
		{
			if (e.Action == ValidationErrorEventAction.Added)
			{
				this._noOfErrorsOnScreen++;
			}
			else
			{
				this._noOfErrorsOnScreen--;
			}
			SettingsUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(this._noOfErrorsOnScreen == 0);
		}

		// Token: 0x0600039A RID: 922 RVA: 0x00016E80 File Offset: 0x00015080
		private void comboSlot1_selectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.ValidateTraySelection();
		}

		// Token: 0x0600039B RID: 923 RVA: 0x00016E80 File Offset: 0x00015080
		private void comboSlot2_selectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.ValidateTraySelection();
		}

		// Token: 0x0600039C RID: 924 RVA: 0x00016E88 File Offset: 0x00015088
		private void ValidateTraySelection()
		{
			if (this.comboSlot1.SelectedItem != null && this.comboSlot2.SelectedItem != null)
			{
				if (this.comboSlot1.SelectedItem.ToString() == "VT54" && (this.comboSlot2.SelectedItem.ToString() == "MTP96" || this.comboSlot2.SelectedItem.ToString() == "MTP384" || this.comboSlot2.SelectedItem.ToString() == "ProteoChip144" || this.comboSlot2.SelectedItem.ToString() == "ProteoChip36"))
				{
					this.IsShowTrayError = true;
				}
				else
				{
					this.IsShowTrayError = false;
				}
				SettingsUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
				if (validationUpdateEvent == null)
				{
					return;
				}
				validationUpdateEvent(!this._isAppKeyError && !this.IsShowTrayError);
			}
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00016F78 File Offset: 0x00015178
		private void TxtAppKey_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				uint valBase16 = Convert.ToUInt32(this.txtAppKey.Text, 16);
				uint special = (valBase16 >> 12) & 15U;
				uint num = special;
				special = num + 1U;
				uint busLogic = (valBase16 >> 8) & 15U;
				busLogic += 1U;
				uint sample = (valBase16 >> 4) & 15U;
				num = sample;
				sample = num + 1U;
				uint chkSum = valBase16 & 15U;
				try
				{
					if (SettingsUserControl._co_67._cp_2 == null)
					{
						SettingsUserControl._co_67._cp_2 = CallSite<Func<CallSite, object, ExpandoObject>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(ExpandoObject), typeof(SettingsUserControl)));
					}
					Func<CallSite, object, ExpandoObject> target = SettingsUserControl._co_67._cp_2.Target;
					CallSite _cpl = SettingsUserControl._co_67._cp_2;
					if (SettingsUserControl._co_67._cp_1 == null)
					{
						SettingsUserControl._co_67._cp_1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Sample", typeof(SettingsUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target2 = SettingsUserControl._co_67._cp_1.Target;
					CallSite _cp_2 = SettingsUserControl._co_67._cp_1;
					if (SettingsUserControl._co_67._cp_0 == null)
					{
						SettingsUserControl._co_67._cp_0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ApplicationKey", typeof(SettingsUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					string text = (string)((IDictionary<string, object>)target(_cpl, target2(_cp_2, SettingsUserControl._co_67._cp_0.Target(SettingsUserControl._co_67._cp_0, this._instrument.BalticConfiguration)))).FirstOrDefault((KeyValuePair<string, object> x) => x.Key == sample.ToString()).Value;
					if (SettingsUserControl._co_67._cp_5 == null)
					{
						SettingsUserControl._co_67._cp_5 = CallSite<Func<CallSite, object, ExpandoObject>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(ExpandoObject), typeof(SettingsUserControl)));
					}
					Func<CallSite, object, ExpandoObject> target3 = SettingsUserControl._co_67._cp_5.Target;
					CallSite _cp_3 = SettingsUserControl._co_67._cp_5;
					if (SettingsUserControl._co_67._cp_4 == null)
					{
						SettingsUserControl._co_67._cp_4 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Special", typeof(SettingsUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					Func<CallSite, object, object> target4 = SettingsUserControl._co_67._cp_4.Target;
					CallSite _cp_4 = SettingsUserControl._co_67._cp_4;
					if (SettingsUserControl._co_67._cp_3 == null)
					{
						SettingsUserControl._co_67._cp_3 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "ApplicationKey", typeof(SettingsUserControl), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
					}
					string text2 = (string)((IDictionary<string, object>)target3(_cp_3, target4(_cp_4, SettingsUserControl._co_67._cp_3.Target(SettingsUserControl._co_67._cp_3, this._instrument.BalticConfiguration)))).FirstOrDefault((KeyValuePair<string, object> x) => x.Key == special.ToString()).Value;
				}
				catch (Exception)
				{
					this.tbAppKeyError.Visibility = Visibility.Visible;
					this._isAppKeyError = true;
					SettingsUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
					if (validationUpdateEvent != null)
					{
						validationUpdateEvent(!this._isAppKeyError && !this.IsShowTrayError);
					}
					throw;
				}
				if (busLogic + sample + special - 3U == chkSum)
				{
					this._isAppKeyError = false;
					this.tbAppKeyError.Visibility = Visibility.Hidden;
				}
				else
				{
					this.tbAppKeyError.Visibility = Visibility.Visible;
					this._isAppKeyError = true;
				}
				SettingsUserControl.ValidationUpdate validationUpdateEvent2 = this.ValidationUpdateEvent;
				if (validationUpdateEvent2 != null)
				{
					validationUpdateEvent2(!this._isAppKeyError && !this.IsShowTrayError);
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600039E RID: 926 RVA: 0x000172C0 File Offset: 0x000154C0
		private void btnConnect_Click(object sender, RoutedEventArgs e)
		{
			if (this._instrument.IsConnected)
			{
				return;
			}
			this.txtHardware.Text = "";
			this.txtConnectStatus.Text = "Connecting...";
			this.txtConnectStatus.Background = Brushes.LightGray;
			this.progControl.Visibility = Visibility.Visible;
			((Action)delegate
			{
				new Timer(new TimerCallback(this.TimerProc)).Change(0, -1);
			}).BeginInvoke(null, null);
		}

		// Token: 0x0600039F RID: 927 RVA: 0x0001732C File Offset: 0x0001552C
		private void TimerProc(object state)
		{
			try
			{
				((Timer)state).Dispose();
				SettingsUserControl.OkCancelButtonUpdate okCancelButtonUpdateEvent = this.OkCancelButtonUpdateEvent;
				if (okCancelButtonUpdateEvent != null)
				{
					okCancelButtonUpdateEvent(false);
				}
				this._instrument.IsColumnOvenConnected = this.IsColumnOvenConnected;
				this._instrument.SetConnectionProperties(this.Settings.PumpHost, this.Settings.AutosamplerIP, this.Settings.IsSimulate, this.Settings.IsCTCSimulate);
				this._instrument.CheckConnectionEvent += this._instrument_CheckConnectionEvent;
				this._instrument.Connect(24, base.Dispatcher);
				this._instrument.CheckConnectionEvent -= this._instrument_CheckConnectionEvent;
				base.Dispatcher.BeginInvoke(new Action(delegate
				{
					this.progControl.Visibility = Visibility.Hidden;
				}), Array.Empty<object>());
				SettingsUserControl.OkCancelButtonUpdate okCancelButtonUpdateEvent2 = this.OkCancelButtonUpdateEvent;
				if (okCancelButtonUpdateEvent2 != null)
				{
					okCancelButtonUpdateEvent2(true);
				}
				SettingsUserControl.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
				if (validationUpdateEvent != null)
				{
					validationUpdateEvent(!this._isAppKeyError && !this.IsShowTrayError);
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x00017448 File Offset: 0x00015648
		private void _instrument_CheckConnectionEvent(object sender, BalticInstrumentFacade.ConnectionEventArgs e)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate
			{
				if (e.IsConnected)
				{
					this.txtConnectStatus.Text = "connected";
					this.txtConnectStatus.Background = Brushes.LightGreen;
					this.btnConnect.IsEnabled = false;
					this.txtHardware.Text = string.Concat(new string[]
					{
						"PAL firmware version: ",
						e.PALFirmwareVersion,
						"\r\nPump firmware version: ",
						e.ZirconiumFirmwareVersion,
						"\r\nColumn oven: ",
						e.IsOvenInstalled ? "installed" : "not installed"
					});
				}
				else
				{
					this.txtConnectStatus.Text = "not connected";
					this.txtConnectStatus.Background = Brushes.LightGray;
					this.txtHardware.Text = ((e.ErrorException != null) ? ("unable to connect to device: " + e.ErrorException.Message) : "unable to connect to device");
				}
				this._isOvenInstalled = e.IsOvenInstalled;
				this.IsShowOvenInfo = e.IsOvenInstalled && !this.IsColumnOvenConnected;
			}), Array.Empty<object>());
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x00017486 File Offset: 0x00015686
		public void ValidatePreferences()
		{
			base.Dispatcher.Invoke(delegate
			{
				if (this.MaxPumpValveShifts != this._preferences.Pump.MaxShifts || this.MaxTIValveShifts != this._preferences.MaxTIShifts)
				{
					bool? flag = new ConfirmWindow("Would you like to save changes to the PM Maximum shift value(s) ?").ShowDialog(HelperExtensions.GetActiveWindow());
					bool flag2 = false;
					if ((flag.GetValueOrDefault() == flag2) & (flag != null))
					{
						this.MaxPumpValveShifts = this._preferences.Pump.MaxShifts;
						this.MaxTIValveShifts = this._preferences.MaxTIShifts;
					}
					else
					{
						this._preferences.Pump.MaxShifts = this.MaxPumpValveShifts;
						this._preferences.MaxTIShifts = this.MaxTIValveShifts;
					}
					try
					{
						string xml = Utility.ToXML(this._preferences);
						using (StreamWriter writer = File.CreateText(Path.Combine(this._privatePath, "Preferences.xml")))
						{
							writer.Write(xml);
							writer.Close();
						}
					}
					catch (IOException)
					{
					}
				}
			});
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x000174A0 File Offset: 0x000156A0
		public void WritePreferences()
		{
			try
			{
				string xml = Utility.ToXML(this._preferences);
				using (StreamWriter writer = File.CreateText(Path.Combine(this._privatePath, "Preferences.xml")))
				{
					writer.Write(xml);
					writer.Close();
				}
			}
			catch (IOException)
			{
			}
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x0001750C File Offset: 0x0001570C
		private void LoadPreferences()
		{
			try
			{
				using (StreamReader reader = new StreamReader(Path.Combine(this._privatePath, "Preferences.xml")))
				{
					string xml = reader.ReadToEnd();
					this._preferences = (BalticPreferences)Utility.FromXML(xml, typeof(BalticPreferences));
					this.MaxPumpValveShifts = this._preferences.Pump.MaxShifts;
					this.MaxTIValveShifts = this._preferences.MaxTIShifts;
				}
			}
			catch (IOException)
			{
			}
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x000175A8 File Offset: 0x000157A8
		private void EditCapillaries_Click(object sender, RoutedEventArgs e)
		{
			CapillariesDiagramWindow dlg = new CapillariesDiagramWindow(this._instrument, this.Settings.ConnectionProfile, this._preferences.Capillaries, this.Settings.IsPressurePSI)
			{
				Owner = this._parent
			};
			dlg.ShowDialog(HelperExtensions.GetActiveWindow());
			if (dlg.DialogResult.GetValueOrDefault())
			{
				using (List<BalticHWProfile.CapillaryItem>.Enumerator enumerator = dlg.Controller.HwCapillaries.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						BalticHWProfile.CapillaryItem item = enumerator.Current;
						BalticHWProfile.CapillaryItem hwItem = this.Settings.ConnectionProfile.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == item.LinkName);
						if (hwItem != null)
						{
							hwItem.Length = item.Length;
							hwItem.ID = item.ID;
						}
					}
				}
			}
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x000176A4 File Offset: 0x000158A4
		private void InitializeTrayTypes()
		{
			this._trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.VT54);
			this._trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.MTP96);
			this._trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.MTP384);
			this._trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.ProteoChip144);
			this._trayTypeList.Add(BalticHWProfile.ASProfile.TrayType.ProteoChip36);
		}

		// Token: 0x04000212 RID: 530
		private int _noOfErrorsOnScreen;

		// Token: 0x04000213 RID: 531
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x04000214 RID: 532
		private bool _isAppKeyError;

		// Token: 0x04000215 RID: 533
		private bool _isShowOvenInfo;

		// Token: 0x04000216 RID: 534
		private bool _isShowTrayError;

		// Token: 0x04000217 RID: 535
		private bool _isOvenInstalled;

		// Token: 0x04000218 RID: 536
		private bool _isShowTrapInjValve;

		// Token: 0x04000219 RID: 537
		private bool _isService;

		// Token: 0x0400021A RID: 538
		private readonly string _privatePath;

		// Token: 0x0400021B RID: 539
		private BalticPreferences _preferences;

		// Token: 0x0400021C RID: 540
		private int _maxPumpValveShifts;

		// Token: 0x0400021D RID: 541
		private int _maxTIValveShifts;

		// Token: 0x0400021E RID: 542
		private readonly Window _parent;

		// Token: 0x0400021F RID: 543
		private readonly double _version = 1.0;

		// Token: 0x04000220 RID: 544
		private readonly List<BalticHWProfile.ASProfile.TrayType> _trayTypeList = new List<BalticHWProfile.ASProfile.TrayType>();

		// Token: 0x04000221 RID: 545
		private List<SettingsUserControl.ComboBoxItemTrayTemperature> _trayTemperatureListEnum = new List<SettingsUserControl.ComboBoxItemTrayTemperature>(4)
		{
			new SettingsUserControl.ComboBoxItemTrayTemperature
			{
				TrayTemperatureEnum = BalticHWProfile.ASProfile.TrayTemperature.Off,
				ValueString = "Off"
			},
			new SettingsUserControl.ComboBoxItemTrayTemperature
			{
				TrayTemperatureEnum = BalticHWProfile.ASProfile.TrayTemperature.ExtraLow,
				ValueString = "Extra Low"
			},
			new SettingsUserControl.ComboBoxItemTrayTemperature
			{
				TrayTemperatureEnum = BalticHWProfile.ASProfile.TrayTemperature.Low,
				ValueString = "Low"
			},
			new SettingsUserControl.ComboBoxItemTrayTemperature
			{
				TrayTemperatureEnum = BalticHWProfile.ASProfile.TrayTemperature.Medium,
				ValueString = "Medium"
			}
		};

		// Token: 0x04000222 RID: 546
		private ObservableCollection<int> _maxPumpPressures = new ObservableCollection<int> { 1000, 1600 };

		// Token: 0x02000105 RID: 261
		public class ComboBoxItemTrayTemperature
		{
			// Token: 0x17000178 RID: 376
			// (get) Token: 0x060007C5 RID: 1989 RVA: 0x0003DE26 File Offset: 0x0003C026
			// (set) Token: 0x060007C6 RID: 1990 RVA: 0x0003DE2E File Offset: 0x0003C02E
			public BalticHWProfile.ASProfile.TrayTemperature TrayTemperatureEnum { get; set; }

			// Token: 0x17000179 RID: 377
			// (get) Token: 0x060007C7 RID: 1991 RVA: 0x0003DE37 File Offset: 0x0003C037
			// (set) Token: 0x060007C8 RID: 1992 RVA: 0x0003DE3F File Offset: 0x0003C03F
			public string ValueString { get; set; }
		}

		// Token: 0x02000106 RID: 262
		// (Invoke) Token: 0x060007CB RID: 1995
		public delegate void ValidationUpdate(bool isValid);

		// Token: 0x02000107 RID: 263
		// (Invoke) Token: 0x060007CF RID: 1999
		public delegate void OkCancelButtonUpdate(bool isEnabled);
	}
}
