using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using BalticClassLib;
using Bruker.Lc.Baltic;

namespace BalticWpfControlLib;

public class HardwareSettingsWindow : Window, IComponentConnector
{
	private const uint MF_GRAYED = 1u;

	private const uint MF_ENABLED = 0u;

	private const uint SC_CLOSE = 61536u;

	private readonly SettingsUserControl _ucHardwareSettings;

	private readonly BalticInstrumentFacade _instrument;

	internal Grid gridSettings;

	internal Button btnOK;

	internal Button btnCanel;

	private bool _contentLoaded;

	[DllImport("user32.dll")]
	private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

	[DllImport("user32.dll")]
	private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

	public HardwareSettingsWindow(string privatePath, BalticHWProfile hardwareProfile, BalticInstrumentFacade instrument, BalticPreferences preferences, string displayName)
	{
		InitializeComponent();
		_instrument = instrument;
		string text = Utility.CreateDisplayVersion(preferences.Version);
		base.Title = ((text == "") ? ("Bruker " + displayName + " Hardware Settings") : ("Bruker " + displayName + " " + text + " Hardware Settings"));
		_ucHardwareSettings = new SettingsUserControl(this, privatePath, hardwareProfile, instrument, displayName);
		_ucHardwareSettings.ValidationUpdateEvent += ucHardwareSettings_UpdateInputValidation;
		_ucHardwareSettings.OkCancelButtonUpdateEvent += UcHardwareSettings_OkCancelButtonUpdateEvent;
		Grid.SetRow(_ucHardwareSettings, 0);
		Grid.SetColumn(_ucHardwareSettings, 0);
		_ucHardwareSettings.VerticalAlignment = VerticalAlignment.Top;
		gridSettings.Children.Add(_ucHardwareSettings);
	}

	private void UcHardwareSettings_OkCancelButtonUpdateEvent(bool isEnabled)
	{
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			btnOK.IsEnabled = isEnabled;
			btnCanel.IsEnabled = isEnabled;
			EnableMenuItem(GetSystemMenu(new WindowInteropHelper(this).Handle, bRevert: false), 61536u, (!isEnabled) ? 1u : 0u);
		});
	}

	private void btnOK_Click(object sender, RoutedEventArgs e)
	{
		_ucHardwareSettings.WritePreferences();
		base.DialogResult = true;
	}

	private void ucHardwareSettings_UpdateInputValidation(bool isValid)
	{
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			btnOK.IsEnabled = isValid;
		});
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (!btnOK.IsEnabled && !btnCanel.IsEnabled)
		{
			e.Cancel = true;
		}
		if (base.DialogResult != false)
		{
			_ucHardwareSettings.ValidatePreferences();
		}
	}

	private void Window_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			Close();
		}
		if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Shift)
		{
			if (_ucHardwareSettings.IsService)
			{
				_instrument.ClearServiceMode();
				_ucHardwareSettings.IsService = false;
			}
			else if (new PasswordWindow
			{
				Owner = this
			}.ShowDialog().GetValueOrDefault())
			{
				_instrument.CreateServiceMode();
				_ucHardwareSettings.IsService = true;
			}
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/hardwaresettingswindow.xaml", UriKind.Relative);
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
			((HardwareSettingsWindow)target).Closing += Window_Closing;
			((HardwareSettingsWindow)target).KeyDown += Window_KeyDown;
			break;
		case 2:
			gridSettings = (Grid)target;
			break;
		case 3:
			btnOK = (Button)target;
			btnOK.Click += btnOK_Click;
			break;
		case 4:
			btnCanel = (Button)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
