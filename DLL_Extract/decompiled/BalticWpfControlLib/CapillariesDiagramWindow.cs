using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BalticClassLib;
using BalticWpfControlLib.Diagram;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using BrukerLC.Utils.Controls;

namespace BalticWpfControlLib;

public class CapillariesDiagramWindow : Window, INotifyPropertyChanged, IComponentConnector
{
	private readonly BalticInstrumentFacade _instrument;

	private bool _isModifiedFromFactory;

	private List<BalticPreferences.CapillaryPreference> _prefCapillaries;

	internal DiagramControl Diagram;

	internal Button btnSaveDefault;

	internal Button btnRevertFactory;

	internal Button btnRevert;

	internal Button btnOK;

	private bool _contentLoaded;

	public DiagramStateController Controller { get; }

	public bool IsModifiedFromFactory
	{
		get
		{
			return _isModifiedFromFactory;
		}
		set
		{
			_isModifiedFromFactory = value;
			NotifyPropertyChanged("IsModifiedFromFactory");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	static CapillariesDiagramWindow()
	{
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if (!string.IsNullOrEmpty(directoryName))
		{
			Assembly.LoadFrom(Path.Combine(directoryName, "BrukerLC.Styling.dll"));
		}
	}

	public CapillariesDiagramWindow(BalticInstrumentFacade instrument, List<BalticHWProfile.CapillaryItem> capillaries, List<BalticPreferences.CapillaryPreference> prefCapillaries, bool isDisplayPressureAsPsi)
	{
		InitializeComponent();
		Controller = new DiagramStateController(instrument, capillaries, prefCapillaries, Diagram, isEditable: true, isDisplayPressureAsPsi);
		_instrument = instrument;
		_prefCapillaries = prefCapillaries;
		base.DataContext = this;
		CheckFactoryDefaultModified();
		base.SourceInitialized += delegate
		{
			this.HideMinimizeMinimizeButton();
		};
	}

	private void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void CheckFactoryDefaultModified()
	{
		BalticPreferences balticPreferences;
		try
		{
			using StreamReader streamReader = new StreamReader(Path.Combine(_instrument.PrivatePath, "Preferences.xml"));
			balticPreferences = (BalticPreferences)Utility.FromXML(streamReader.ReadToEnd(), typeof(BalticPreferences));
		}
		catch (Exception)
		{
			balticPreferences = new BalticPreferences();
		}
		_prefCapillaries = balticPreferences.Capillaries;
		bool isModifiedFromFactory = false;
		foreach (BalticPreferences.CapillaryPreference prefCapillary in _prefCapillaries)
		{
			if ((int)(prefCapillary.DefaultLength * 100.0) != (int)(prefCapillary.FactoryLength * 100.0) || (int)(prefCapillary.DefaultID * 100.0) != (int)(prefCapillary.FactoryID * 100.0))
			{
				isModifiedFromFactory = true;
				break;
			}
		}
		IsModifiedFromFactory = isModifiedFromFactory;
	}

	private void btnRevert_Click(object sender, RoutedEventArgs e)
	{
		Controller.RevertAllCapillaries();
	}

	private void btnOK_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
		Controller.SetHwCapillaries();
	}

	private void btnSaveDefault_Click(object sender, RoutedEventArgs e)
	{
		Controller.SaveAsDefaultCapillaries();
		Controller.SetHwCapillaries();
		CheckFactoryDefaultModified();
	}

	private void btnRevertFactory_Click(object sender, RoutedEventArgs e)
	{
		Controller.RevertAllCapillariesToFactory();
		CheckFactoryDefaultModified();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/capillariesdiagramwindow.xaml", UriKind.Relative);
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
			Diagram = (DiagramControl)target;
			break;
		case 2:
			btnSaveDefault = (Button)target;
			btnSaveDefault.Click += btnSaveDefault_Click;
			break;
		case 3:
			btnRevertFactory = (Button)target;
			btnRevertFactory.Click += btnRevertFactory_Click;
			break;
		case 4:
			btnRevert = (Button)target;
			btnRevert.Click += btnRevert_Click;
			break;
		case 5:
			btnOK = (Button)target;
			btnOK.Click += btnOK_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
