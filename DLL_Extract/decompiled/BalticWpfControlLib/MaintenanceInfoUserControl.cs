using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib;

public class MaintenanceInfoUserControl : UserControl, IComponentConnector
{
	public static RoutedCommand ResetCommand = new RoutedCommand();

	private bool _contentLoaded;

	public MaintenanceInfoUserControl()
	{
		InitializeComponent();
	}

	private void ResetBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		((IResetable)e.Parameter).Reset();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/maintenanceinfousercontrol.xaml", UriKind.Relative);
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
			((CommandBinding)target).Executed += ResetBinding_Executed;
		}
		else
		{
			_contentLoaded = true;
		}
	}
}
