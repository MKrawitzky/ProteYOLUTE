using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib;

public class SystemInfoWindow : Window, IComponentConnector
{
	private readonly DispatcherTimer _timer = new DispatcherTimer();

	private bool _isAppService;

	internal ItemsControl MaintenanceInfos;

	private bool _contentLoaded;

	public bool IsAppService
	{
		get
		{
			return _isAppService;
		}
		set
		{
			_isAppService = value;
			foreach (IMaintenanceInfo item in (IEnumerable<IMaintenanceInfo>)MaintenanceInfos.ItemsSource)
			{
				item.IsAppService = _isAppService;
				item.Refresh();
			}
		}
	}

	public SystemInfoWindow(IEnumerable<IMaintenanceInfo> maintenanceInfos, bool isAppService)
	{
		InitializeComponent();
		MaintenanceInfos.ItemsSource = maintenanceInfos;
		IsAppService = isAppService;
		_timer.Interval = TimeSpan.FromMilliseconds(2000.0);
		_timer.Tick += timer_Tick;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		_timer.Start();
	}

	private void Window_KeyDown(object sender, KeyEventArgs e)
	{
		if (Key.Escape == e.Key)
		{
			Close();
		}
	}

	private void timer_Tick(object sender, EventArgs e)
	{
		base.Dispatcher.Invoke(Action);
		void Action()
		{
			try
			{
				foreach (IMaintenanceInfo item in (IEnumerable<IMaintenanceInfo>)MaintenanceInfos.ItemsSource)
				{
					item.Refresh();
				}
			}
			catch (Exception)
			{
			}
		}
	}

	private void SystemInfoWindow_OnClosed(object sender, EventArgs e)
	{
		base.Owner.Activate();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/systeminfowindow.xaml", UriKind.Relative);
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
			((SystemInfoWindow)target).KeyDown += Window_KeyDown;
			((SystemInfoWindow)target).Loaded += Window_Loaded;
			((SystemInfoWindow)target).Closed += SystemInfoWindow_OnClosed;
			break;
		case 2:
			MaintenanceInfos = (ItemsControl)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
