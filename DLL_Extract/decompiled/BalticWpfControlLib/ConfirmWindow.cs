using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BalticWpfControlLib;

public class ConfirmWindow : Window, IComponentConnector
{
	internal Grid gridSettings;

	internal TextBlock txtMessage;

	internal Button btnOK;

	private bool _contentLoaded;

	public ConfirmWindow(string message)
	{
		InitializeComponent();
		txtMessage.Text = message;
	}

	private void BtnOK_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/confirmwindow.xaml", UriKind.Relative);
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
			gridSettings = (Grid)target;
			break;
		case 2:
			txtMessage = (TextBlock)target;
			break;
		case 3:
			btnOK = (Button)target;
			btnOK.Click += BtnOK_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
