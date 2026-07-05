using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;

namespace BalticWpfControlLib;

public class AdvancedChildParameterWindow : Window, IComponentConnector
{
	public delegate void ModificationUpdate(bool isModified);

	public delegate void ValidationUpdate(bool isValid);

	private readonly AdvChildParamSettingsUserControl _advChildSettingsUC;

	internal Grid gridParam;

	private bool _contentLoaded;

	public event ModificationUpdate ModificationUpdateEvent;

	public event ValidationUpdate ValidationUpdateEvent;

	public AdvancedChildParameterWindow(string header, BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
	{
		InitializeComponent();
		_advChildSettingsUC = new AdvChildParamSettingsUserControl(header, method, instrument, experiment);
		_advChildSettingsUC.ValidationUpdateEvent += _advChildSettingsUC_ValidationUpdateEvent;
		_advChildSettingsUC.ModificationUpdateEvent += _advChildSettingsUC_ModificationUpdateEvent;
		Grid.SetRow(_advChildSettingsUC, 0);
		Grid.SetColumn(_advChildSettingsUC, 0);
		_advChildSettingsUC.VerticalAlignment = VerticalAlignment.Top;
		gridParam.Children.Add(_advChildSettingsUC);
	}

	private void _advChildSettingsUC_ModificationUpdateEvent(bool isModified)
	{
		this.ModificationUpdateEvent?.Invoke(isModified);
	}

	private void _advChildSettingsUC_ValidationUpdateEvent(bool isValid)
	{
		this.ValidationUpdateEvent?.Invoke(isValid);
	}

	private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (base.IsLoaded)
		{
			UpdateLocation();
		}
	}

	private void UpdateLocation()
	{
		Rectangle rectangle = default(Rectangle);
		rectangle.Width = (int)base.ActualWidth;
		rectangle.Height = (int)base.ActualHeight;
		rectangle.X = (int)base.Left;
		rectangle.Y = (int)base.Top;
		Rectangle rect = rectangle;
		rect = UIHelper.CalcOnScreenBounds(rect);
		base.Top = rect.Y;
		base.Left = rect.X;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/advancedchildparameterwindow.xaml", UriKind.Relative);
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
			((AdvancedChildParameterWindow)target).SizeChanged += Window_SizeChanged;
			break;
		case 2:
			gridParam = (Grid)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
