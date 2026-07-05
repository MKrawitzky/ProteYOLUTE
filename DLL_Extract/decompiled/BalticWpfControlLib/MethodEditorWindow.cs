using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;
using BalticClassLib;
using Bruker.Lc.Baltic;

namespace BalticWpfControlLib;

public class MethodEditorWindow : Window, IComponentConnector
{
	private MethodUserControl ucMethodEditor;

	internal Grid gridMethod;

	internal StackPanel stkTrapWarn;

	internal TextBlock msgText;

	internal Button btnOK;

	internal Button btnCancel;

	private bool _contentLoaded;

	public MethodEditorWindow(BalticMethod method, BalticInstrumentFacade facade, BalticHWProfile hwProfile, BalticColumnList columns, ColumnSelections columnSelections, BalticPreferences preferences, string displayName, string versionStr, bool isPressurePSI = false, bool isOvenDetected = false)
	{
		InitializeComponent();
		string text = Utility.CreateDisplayVersion(versionStr);
		if (BalticInstrumentFacade.IsService)
		{
			base.Title = ((text == "") ? $"Bruker {displayName} Instant Expertise Method Editor [SERVICE]" : $"Bruker {displayName} {text} Instant Expertise Method Editor");
		}
		else
		{
			base.Title = ((text == "") ? $"Bruker {displayName} Instant Expertise Method Editor" : $"Bruker {displayName} {text} Instant Expertise Method Editor");
		}
		ucMethodEditor = new MethodUserControl(this, method, facade, columns, columnSelections, isPressurePSI, isOvenDetected);
		ucMethodEditor.ValidateInputUpdateEvent += wpfMethodCtrl_ValidInputDataUpdate;
		ucMethodEditor.EnableMethodCompleteEvent += wpfMethodCtrl_EnableMethodComplete;
		ucMethodEditor.TrapSelectionWarningEvent += wpfMethodCtrl_TrapSelectionWarning;
		Grid.SetRow(ucMethodEditor, 0);
		Grid.SetColumn(ucMethodEditor, 0);
		ucMethodEditor.VerticalAlignment = VerticalAlignment.Top;
		gridMethod.Children.Add(ucMethodEditor);
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		base.SizeToContent = SizeToContent.Height;
	}

	private void wpfMethodCtrl_TrapSelectionWarning(bool isShow, string message = "")
	{
		if (message != "")
		{
			msgText.Text = message;
		}
		stkTrapWarn.Visibility = ((!isShow) ? Visibility.Hidden : Visibility.Visible);
	}

	private void wpfMethodCtrl_EnableMethodComplete(bool isEnabled)
	{
		btnOK.IsEnabled = isEnabled;
	}

	private void wpfMethodCtrl_ValidInputDataUpdate(bool isValid)
	{
		btnOK.IsEnabled = isValid;
	}

	private void btnOK_Click(object sender, RoutedEventArgs e)
	{
		ucMethodEditor.ToBalticMethod();
		base.DialogResult = true;
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (base.DialogResult != false && !btnOK.IsFocused && !btnCancel.IsFocused)
		{
			e.Cancel = true;
			btnCancel.Focus();
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				base.DialogResult = false;
			}, DispatcherPriority.Background);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/methodeditorwindow.xaml", UriKind.Relative);
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
			((MethodEditorWindow)target).Loaded += OnLoaded;
			((MethodEditorWindow)target).Closing += Window_Closing;
			break;
		case 2:
			gridMethod = (Grid)target;
			break;
		case 3:
			stkTrapWarn = (StackPanel)target;
			break;
		case 4:
			msgText = (TextBlock)target;
			break;
		case 5:
			btnOK = (Button)target;
			btnOK.Click += btnOK_Click;
			break;
		case 6:
			btnCancel = (Button)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
