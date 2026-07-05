using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace BalticWpfControlLib;

public class AboutWindow : Window, IComponentConnector
{
	internal TextBlock textSubTitle;

	internal TextBlock versionLabel;

	internal TextBlock versionDataLabel;

	internal TextBlock createLabel;

	internal TextBlock createDateLabel;

	internal TextBlock copyrightLabel;

	private bool _contentLoaded;

	public AboutWindow(string displayName, string model, string productVersion, string creationDate)
	{
		InitializeComponent();
		versionDataLabel.Text = productVersion;
		createDateLabel.Text = creationDate;
		base.Title = ((model == "") ? $"About Bruker Plug-in for {displayName}" : $"About Bruker Plug-in for  {displayName} {model}");
		textSubTitle.Text = ((model == "") ? $"Bruker Plug-in for {displayName}™ UHPLC" : $"Bruker Plug-in for {displayName}™ {model} UHPLC");
		base.KeyDown += AboutWindow_KeyDown;
	}

	private void AboutWindow_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			Close();
		}
	}

	private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (directoryName == null)
			{
				return;
			}
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				UseShellExecute = true,
				Verb = "open",
				FileName = Path.Combine(directoryName, "Licenses.html")
			};
			if (processStartInfo.FileName.Length <= 0)
			{
				return;
			}
			using (Process.Start(processStartInfo))
			{
			}
		}
		catch (Exception)
		{
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/aboutwindow.xaml", UriKind.Relative);
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
			((AboutWindow)target).KeyDown += AboutWindow_KeyDown;
			break;
		case 2:
			textSubTitle = (TextBlock)target;
			break;
		case 3:
			versionLabel = (TextBlock)target;
			break;
		case 4:
			versionDataLabel = (TextBlock)target;
			break;
		case 5:
			createLabel = (TextBlock)target;
			break;
		case 6:
			createDateLabel = (TextBlock)target;
			break;
		case 7:
			copyrightLabel = (TextBlock)target;
			break;
		case 8:
			((Hyperlink)target).Click += Hyperlink_OnClick;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
