using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using BalticClassLib;
using BalticWpfControlLib.Controls;
using BalticWpfControlLib.Diagram;
using Bruker.Lc;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Bruker.Lc.Maintenance;
using Bruker.Lc.Metering;
using BrukerLC.Utils.Controls;

namespace BalticWpfControlLib
{

public partial class LCUserControl : UserControl, INotifyPropertyChanged, IComponentConnector
{
	private delegate void ShowSystemConditionCallback(SystemCondition condition);

	private delegate void ConfirmConditionCallback(object sender, RoutedEventArgs e);

	public delegate void ExecutionReport(ExecutionStateChangedEventArgs e);

	public delegate void ConfirmButtonOnErrorCallback(SystemCondition condition);

	private class NotifyCollectionChangedSynchronizer<T> : INotifyCollectionChanged, IEnumerable<T>, IEnumerable
	{
		private readonly IEnumerable<T> _enumerable;

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public NotifyCollectionChangedSynchronizer(IEnumerable<T> source)
		{
			_enumerable = source;
			if (source is INotifyCollectionChanged notifyCollectionChanged)
			{
				notifyCollectionChanged.CollectionChanged += ncc_CollectionChanged;
			}
		}

		private void ncc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
			if (collectionChanged == null)
			{
				return;
			}
			e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			Delegate[] invocationList = collectionChanged.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				NotifyCollectionChangedEventHandler notifyCollectionChangedEventHandler = (NotifyCollectionChangedEventHandler)invocationList[i];
				if (notifyCollectionChangedEventHandler.Target is DispatcherObject dispatcherObject && !dispatcherObject.CheckAccess())
				{
					dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, notifyCollectionChangedEventHandler, this, e);
				}
				else
				{
					notifyCollectionChangedEventHandler(this, e);
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _enumerable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private readonly BalticInstrumentFacade _facade;

	private readonly LCChartUserControl _realtimeChart;

	private SystemCondition _storyboardCondition;

	private ProcedureExecutionState _lastExecutionState;

	private SystemInfoWindow _dlgSystemInfo;

	private readonly Window _sysInfoOwner;

	private bool _isSystemWindowOpen;

	private bool _isScripInProgress;

	private bool _isService;

	private readonly DiagramStateController _diagramStateController;

	private Window _popoutWindow;

	private void Diagram_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (_popoutWindow != null && _popoutWindow.IsVisible)
		{
			_popoutWindow.Activate();
			return;
		}
		VisualBrush brush = new VisualBrush(this.Diagram);
		brush.Stretch = Stretch.Uniform;
		System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
		rect.Fill = brush;
		rect.Margin = new Thickness(10);
		Border popBorder = new Border();
		popBorder.Background = Brushes.White;
		popBorder.Child = rect;
		_popoutWindow = new Window();
		_popoutWindow.Title = "proteoElute - System Diagram (ProteYOLUTE by Michael Krawitzky)";
		_popoutWindow.Width = 900;
		_popoutWindow.Height = 750;
		_popoutWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		_popoutWindow.Content = popBorder;
		_popoutWindow.Background = Brushes.White;
		_popoutWindow.ResizeMode = ResizeMode.CanResizeWithGrip;
		_popoutWindow.Closed += delegate { _popoutWindow = null; };
		_popoutWindow.Show();
		e.Handled = true;
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
			base.Dispatcher.Invoke(Action);
			void Action()
			{
				if (wrapPanelScripts != null)
				{
					foreach (object child in wrapPanelScripts.Children)
					{
						if (child is ScriptUserControl scriptUserControl && scriptUserControl.Info.IsService)
						{
							scriptUserControl.Visibility = ((!IsService) ? Visibility.Collapsed : Visibility.Visible);
						}
					}
				}
				_realtimeChart?.UpdateServiceTraces(_isService);
				_diagramStateController.UpdateServiceMode(_isService);
				if (_dlgSystemInfo != null)
				{
					_dlgSystemInfo.IsAppService = _isService;
				}
			}
		}
	}

	public IChromatographyColumnType TrapType { get; set; }

	public IChromatographyColumnType SeparatorType { get; set; }

	public LCUserControlSettings Settings
	{
		get
		{
			GridLengthConverter gridLengthConverter = new GridLengthConverter();
			return new LCUserControlSettings
			{
				EnabledTracesList = _realtimeChart.EnabledTracesList.ToList(),
				IsDiagnosticTracesSelected = _realtimeChart.IsDiagnosticTracesSelected,
				ChartGridWidthLeft = gridLengthConverter.ConvertToInvariantString(chartGrid.ColumnDefinitions[0].Width),
				ChartGridWidthRight = gridLengthConverter.ConvertToInvariantString(chartGrid.ColumnDefinitions[2].Width),
				MainGridHeightTopRow = gridLengthConverter.ConvertToInvariantString(storyGrid.RowDefinitions[0].Height),
				MainGridHeightBottomRow = gridLengthConverter.ConvertToInvariantString(storyGrid.RowDefinitions[2].Height)
			};
		}
		set
		{
			try
			{
				if (!string.IsNullOrEmpty(value.MainGridHeightTopRow) && !string.IsNullOrEmpty(value.MainGridHeightBottomRow) && !string.IsNullOrEmpty(value.ChartGridWidthLeft) && !string.IsNullOrEmpty(value.ChartGridWidthRight))
				{
					GridLengthConverter gridLengthConverter = new GridLengthConverter();
					object obj = gridLengthConverter.ConvertFromInvariantString(value.MainGridHeightTopRow);
					object obj2 = gridLengthConverter.ConvertFromInvariantString(value.MainGridHeightBottomRow);
					object obj3 = gridLengthConverter.ConvertFromInvariantString(value.ChartGridWidthLeft);
					object obj4 = gridLengthConverter.ConvertFromInvariantString(value.ChartGridWidthRight);
					if (obj is GridLength height && obj2 is GridLength height2 && obj3 is GridLength width && obj4 is GridLength width2)
					{
						storyGrid.RowDefinitions[0].Height = height;
						storyGrid.RowDefinitions[2].Height = height2;
						chartGrid.ColumnDefinitions[0].Width = width;
						chartGrid.ColumnDefinitions[2].Width = width2;
					}
				}
			}
			catch (Exception)
			{
			}
			_realtimeChart.IsDiagnosticTracesSelected = value.IsDiagnosticTracesSelected;
			_realtimeChart.EnabledTracesList = value.EnabledTracesList;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public event ExecutionReport ExecutionReportEvent;

	public event ConfirmButtonOnErrorCallback ConfirmButtonEvent;

	public event EventHandler ActivateAuxiliaryControl;

	private void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public LCUserControl(BalticInstrumentFacade instrument, List<BalticHWProfile.CapillaryItem> capillaries, List<BalticPreferences.CapillaryPreference> prefCapillaries, bool isOvenInstalled = false, bool displayPressureAsPsi = false, ExecutionStateChangedEventArgs initialExecutionState = null, Window parentWin = null)
	{
		_facade = instrument;
		_lastExecutionState = initialExecutionState?.ExecutionState ?? ProcedureExecutionState.Completed;
		InitializeComponent();
		_sysInfoOwner = parentWin;
		base.DataContext = this;
		btnStop.Visibility = Visibility.Collapsed;
		foreach (ProcedureInfo businessProcedure in _facade.BusinessProcedures)
		{
			if (!businessProcedure.Hidden)
			{
				ScriptUserControl scriptUserControl = new ScriptUserControl(businessProcedure, parentWin, instrument.PrivatePath, instrument.BalticSettings, instrument);
				scriptUserControl.OnScriptActionClick += ucScript_ScriptActionClick;
				scriptUserControl.OnScriptApplyClick += ucScript_ScriptApplyClick;
				scriptUserControl.ScriptValidationRequest += ucScript_ScriptValidationRequest;
				scriptUserControl.ScriptArgumentPresetRequest += ucScript_ScriptArgumentPresetRequest;
				if (scriptUserControl.Info.IsService)
				{
					scriptUserControl.Visibility = ((!BalticInstrumentFacade.IsService) ? Visibility.Collapsed : Visibility.Visible);
				}
				wrapPanelScripts.Children.Add(scriptUserControl);
			}
		}
		List<IMeteringChannel> list = new List<IMeteringChannel>();
		TransformChannels(_facade.TraceChannels, list, displayPressureAsPsi);
		_realtimeChart = new LCChartUserControl(list, (double)_facade.BalticConfiguration.LCControlMaxTimeBuffer)
		{
			Background = new SolidColorBrush(Colors.White)
		};
		Grid.SetColumn(_realtimeChart, 2);
		chartGrid.Children.Add(_realtimeChart);
		if (initialExecutionState != null)
		{
			facade_ExecutionStateChanged(this, initialExecutionState);
		}
		else if (_facade.CurrentSeverity < Severity.Error)
		{
			tbStatus.Text = "Ready";
		}
		_facade.ExecutionStateChanged += facade_ExecutionStateChanged;
		_facade.DecompressExecutionStateChanged += _facade_DecompressExecutionStateChanged;
		dgMessage.ItemsSource = new NotifyCollectionChangedSynchronizer<SystemCondition>(_facade.SystemConditions);
		dgMessage.Items.SortDescriptions.Add(new SortDescription("Raised", ListSortDirection.Ascending));
		if (_facade.SystemConditions is INotifyCollectionChanged notifyCollectionChanged)
		{
			notifyCollectionChanged.CollectionChanged += LCUserControl_ConditionsChanged;
		}
		_facade.ProgressChanged += facade_ProgressChanged;
		_facade.AbortEnabledConditions.AbortEnabledCollectionChanged += AbortEnabledConditions_AbortEnabledCollectionChanged;
		_facade.AbortEnabledConditions.ApplyEnabledCollectionChanged += ApplyEnabledConditions_ApplyEnabledCollectionChanged;
		_facade.DiagramLoggingEnabledConditions.DiagramLoggingEnabledCollectionChanged += DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged;
		foreach (IMeteringChannel gaugeChannel in _facade.GaugeChannels)
		{
			if (gaugeChannel.ChannelInfo.Id.Name.Equals("activity"))
			{
				gaugeChannel.ChannelDataChanged += activity_ChannelDataChanged;
				break;
			}
		}
		dgMessage.Focus();
		_diagramStateController = new DiagramStateController(_facade, capillaries, prefCapillaries, Diagram, isEditable: false, isOvenInstalled, displayPressureAsPsi);
	}

	private void DiagramLoggingEnabledConditions_DiagramLoggingEnabledCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		SystemConditionManager conditions = _facade.DiagramLoggingEnabledConditions;
		base.Dispatcher.Invoke(Action);
		void Action()
		{
			foreach (DiagramLoggingEnabledCondition key in conditions.DiagramLoggingEnabledConditions.Keys)
			{
				_diagramStateController.IsDiagramLoggingEnabled = key.IsLoggingEnabled;
			}
			conditions.DiagramLoggingEnabledConditions.Clear();
		}
	}

	private void ApplyEnabledConditions_ApplyEnabledCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		SystemConditionManager conditions = _facade.ApplyEnabledConditions;
		base.Dispatcher.Invoke(Action);
		void Action()
		{
			foreach (ApplyEnabledCondition key in conditions.ApplyEnabledConditions.Keys)
			{
				foreach (object child in wrapPanelScripts.Children)
				{
					if (child is ScriptUserControl { SettingsDlg: not null } scriptUserControl)
					{
						scriptUserControl.SettingsDlg.btnApply.IsEnabled = key.IsApplyEnabled;
					}
				}
			}
			conditions.ApplyEnabledConditions.Clear();
		}
	}

	private void AbortEnabledConditions_AbortEnabledCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		SystemConditionManager conditions = _facade.AbortEnabledConditions;
		base.Dispatcher.Invoke(Action);
		void Action()
		{
			foreach (AbortEnabledCondition key in conditions.AbortEnabledConditions.Keys)
			{
				btnStop.IsEnabled = key.IsAbortEnabled;
			}
			conditions.AbortEnabledConditions.Clear();
		}
	}

	private void LCUserControl_ConditionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action.Equals(NotifyCollectionChangedAction.Add))
		{
			foreach (SystemCondition condition in e.NewItems)
			{
				if (condition.ManualDismiss)
				{
					dgMessage.Dispatcher.Invoke(() => dgMessage.SelectedItem = condition);
					break;
				}
			}
		}
		else if (e.Action.Equals(NotifyCollectionChangedAction.Reset) && _storyboardCondition != null)
		{
			bool flag = false;
			foreach (SystemCondition systemCondition2 in _facade.SystemConditions)
			{
				if (systemCondition2 == _storyboardCondition)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				bottomPanel.Visibility = Visibility.Hidden;
				dgMessage.IsHitTestVisible = true;
				dgMessage.SelectedIndex = -1;
				_storyboardCondition = null;
			}
		}
		dgMessage.Dispatcher.Invoke(delegate
		{
			if (dgMessage.Items.Count > 0)
			{
				dgMessage.ScrollIntoView(dgMessage.Items[dgMessage.Items.Count - 1]);
			}
		});
	}

	private void ucScript_ScriptArgumentPresetRequest(object sender, ScriptControlEventArgs e)
	{
		ProcedureArguments procedureSourceArgs = e.ProcedureSourceArgs;
		if (procedureSourceArgs.Contains("trap"))
		{
			procedureSourceArgs["trap"].Value = TrapType;
		}
		if (procedureSourceArgs.Contains("separator"))
		{
			procedureSourceArgs["separator"].Value = SeparatorType;
		}
	}

	private void ucScript_ScriptActionClick(object sender, ScriptControlEventArgs args)
	{
		if (_isScripInProgress)
		{
			return;
		}
		tbScriptAction.Text = "";
		if (_storyboardCondition != null)
		{
			if (btnConfirm.Visibility == Visibility.Visible)
			{
				confirmButton_Click(btnConfirm, null);
			}
			else if (btnClose.Visibility == Visibility.Visible)
			{
				closeButton_Click(btnClose, null);
			}
		}
		RunScript(args.ProcedureSourceInfo, args.ProcedureSourceArgs);
	}

	private void ucScript_ScriptApplyClick(object sender, ScriptControlEventArgs args)
	{
		ApplyScript(args.ProcedureSourceInfo, args.ProcedureSourceArgs, args.ProcedureSourceChildArgs);
	}

	private void ucScript_ScriptValidationRequest(object sender, ScriptValidationRequestEventArgs e)
	{
		EventHandler<ProcedureReportEventArgs> value = delegate(object _, ProcedureReportEventArgs a)
		{
			e.AddReport(a);
		};
		_facade.ValidationMessageReported += value;
		_facade.ValidateProcedureOffLine(e.ProcedureSourceInfo, e.ProcedureSourceArgs, e.ProcedureSourceChildArgs);
		_facade.ValidationMessageReported -= value;
	}

	private static void TransformChannels(IEnumerable<IMeteringChannel> channels, List<IMeteringChannel> collection, bool psi)
	{
		foreach (IMeteringChannel channel in channels)
		{
			MeteringChannelInfo channelInfo = channel.ChannelInfo;
			IMeteringChannel meteringChannel = channel;
			if (psi && "bar".Equals(channelInfo.Unit, StringComparison.InvariantCultureIgnoreCase))
			{
				channelInfo = new MeteringChannelInfo(channelInfo.Id.Name, channelInfo.Id.Source, "psi", channelInfo.DisplayDecimals, channelInfo.ValueType, channelInfo.IsDiagnostic, channelInfo.IsSevice);
				meteringChannel = new TransformingMeteringChannel(meteringChannel, channelInfo, Bars2Psi);
			}
			collection.Add(meteringChannel);
		}
	}

	private static MeteringDataPoint Bars2Psi(MeteringDataPoint mdp)
	{
		return new MeteringDataPoint(mdp.Timestamp, (double)mdp.Value / 0.0689475729);
	}

	private void _facade_DecompressExecutionStateChanged(object sender, ExecutionStateChangedEventArgs args)
	{
		bool enable = args.ExecutionState != ProcedureExecutionState.Started;
		string msg = args.Procedure + " " + args.ExecutionState.ToString().ToLower();
		base.Dispatcher.Invoke(Action);
		void Action()
		{
			EnableUserControls(enable, args.Procedure, isDecompress: true);
			tbStatus.Text = msg;
			progScriptAction.IsIndeterminate = !enable;
			SetConfirmEnabled();
		}
	}

	private void facade_ExecutionStateChanged(object sender, ExecutionStateChangedEventArgs args)
	{
		if (!string.IsNullOrEmpty(args.BackGroundSelfCheckEventName))
		{
			base.Dispatcher.Invoke(new Action(StopEnable), Array.Empty<object>());
			switch (args.ExecutionState)
			{
			case ProcedureExecutionState.Started:
				btnStop.Click += OnAbortSelfcheckClick;
				_isScripInProgress = true;
				break;
			case ProcedureExecutionState.Completed:
			case ProcedureExecutionState.Aborted:
				btnStop.Click -= OnAbortSelfcheckClick;
				_isScripInProgress = false;
				break;
			}
		}
		_realtimeChart.SetHidden(args.IsHidden && !IsElutionProcedure(args.Procedure));
		string msg;
		bool enable;
		if (!args.IsHidden || IsElutionProcedure(args.Procedure))
		{
			this.ExecutionReportEvent?.Invoke(args);
			_lastExecutionState = args.ExecutionState;
			msg = args.Procedure + " " + args.ExecutionState.ToString().ToLower();
			enable = args.ExecutionState != ProcedureExecutionState.Started;
			if (!args.IsDecompressOnExit)
			{
				base.Dispatcher.Invoke(Action);
			}
			else if (!enable)
			{
				base.Dispatcher.Invoke(Action);
			}
		}
		void Action()
		{
			EnableUserControls(enable, args.Procedure);
			tbStatus.Text = msg;
			progScriptAction.IsIndeterminate = !enable;
			SetConfirmEnabled();
		}
		void StopEnable()
		{
			btnStop.Visibility = ((args.ExecutionState != 0) ? Visibility.Collapsed : Visibility.Visible);
			btnStop.IsEnabled = args.ExecutionState == ProcedureExecutionState.Started;
		}
	}

	private void OnAbortSelfcheckClick(object sender, RoutedEventArgs args)
	{
		SystemCondition storyboardCondition = _storyboardCondition;
		if (storyboardCondition != null && storyboardCondition.ManualDismiss)
		{
			confirmButton_Click(btnConfirm, new RoutedEventArgs());
		}
		((Action)delegate
		{
			ManualResetEventSlim wh = new ManualResetEventSlim();
			try
			{
				EventHandler<ExecutionStateChangedEventArgs> value = delegate(object _, ExecutionStateChangedEventArgs e)
				{
					if (e.ExecutionState != 0)
					{
						wh.Set();
					}
				};
				if (_facade.GetDecompressProcedure().DecompressOnExit)
				{
					_facade.DecompressExecutionStateChanged += value;
					wh.Wait();
					_facade.DecompressExecutionStateChanged -= value;
				}
				base.Dispatcher.Invoke(GuiAction);
				_isScripInProgress = false;
			}
			finally
			{
				if (wh != null)
				{
					((IDisposable)wh).Dispose();
				}
			}
		}).BeginInvoke(null, null);
		_facade.AbortSelfDiagnostics();
		void GuiAction()
		{
			btnStop.Visibility = Visibility.Collapsed;
			btnStop.IsEnabled = false;
			EnableUserControls(isEnable: true, _facade.GetDecompressProcedure().Name);
		}
	}

	private void activity_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (e.Data != null)
		{
			string[] array = (string[])e.Data[e.Data.Length - 1].Value;
			foreach (string value in array)
			{
				stringBuilder.Append(value).Append(" + ");
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Length -= " + ".Length;
			}
		}
		string msg = stringBuilder.ToString();
		base.Dispatcher.Invoke(() => tbScriptAction.Text = msg);
	}

	private void facade_ProgressChanged(object sender, Bruker.Lc.Business.ProgressChangedEventArgs e)
	{
		base.Dispatcher.Invoke(Action);
		void Action()
		{
			progScriptAction.IsIndeterminate = false;
			progScriptAction.Value = e.CurrentProgress;
		}
	}

	private bool IsElutionProcedure(string procedureName)
	{
		return _facade.GetElutionProcedure(procedureName) != null;
	}

	private bool IsEquilibrationProcedure(string procedureName)
	{
		return _facade.GetEquilibrationProcedure(procedureName) != null;
	}

	private void EnableUserControls(bool isEnable, string activity, bool isDecompress = false)
	{
		if (!isDecompress)
		{
			tbStopBtnText.Text = " Abort " + activity;
		}
		bdrScriptAction.Visibility = (isEnable ? Visibility.Hidden : Visibility.Visible);
		bdrScriptWrapPanel.Visibility = ((!isEnable) ? Visibility.Hidden : Visibility.Visible);
	}

	private void ApplyScript(ProcedureInfo script, ProcedureArguments args, ChildProcedureArguments childArgs)
	{
		if (!_isScripInProgress)
		{
			RunScript(script, args, childArgs);
		}
		else
		{
			_facade.ApplyScriptSettings(script, args, childArgs);
		}
	}

	private void RunScript(ProcedureInfo script, ProcedureArguments args, ChildProcedureArguments childArgs = null)
	{
		_isScripInProgress = true;
		((Action)delegate
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			try
			{
				base.Dispatcher.Invoke(new Action(StopEn), Array.Empty<object>());
				ManualResetEventSlim wh = new ManualResetEventSlim();
				try
				{
					BasicSyncHandler syncHandler = new BasicSyncHandler();
					RoutedEventHandler value = delegate
					{
						SystemCondition storyboardCondition = _storyboardCondition;
						if (storyboardCondition != null && storyboardCondition.ManualDismiss)
						{
							confirmButton_Click(btnConfirm, new RoutedEventArgs());
						}
						cts.Cancel();
						btnStop.IsEnabled = false;
						if (!script.DecompressOnExit)
						{
							_isScripInProgress = false;
						}
					};
					btnStop.Click += value;
					EventHandler<ExecutionStateChangedEventArgs> value2 = delegate(object _, ExecutionStateChangedEventArgs e)
					{
						if (!e.IsHidden || IsElutionProcedure(e.Procedure))
						{
							base.Dispatcher.Invoke(new Action(StopEnable), Array.Empty<object>());
							if (e.ExecutionState == ProcedureExecutionState.Completed && script.Name.ToLower().Contains("preparation"))
							{
								_facade.UnregisterPreProcedureRequired();
							}
							if (e.ExecutionState != 0 && !script.DecompressOnExit)
							{
								wh.Set();
							}
						}
						void StopEnable()
						{
							btnStop.Visibility = ((e.ExecutionState != 0) ? Visibility.Collapsed : Visibility.Visible);
							btnStop.IsEnabled = e.ExecutionState == ProcedureExecutionState.Started;
						}
					};
					_facade.ExecutionStateChanged += value2;
					EventHandler<ExecutionStateChangedEventArgs> value3 = delegate(object _, ExecutionStateChangedEventArgs e)
					{
						if (e.ExecutionState != 0)
						{
							wh.Set();
						}
					};
					if (script.DecompressOnExit)
					{
						_facade.DecompressExecutionStateChanged += value3;
					}
					try
					{
						_facade.ExecuteProcedure(script, args, childArgs ?? new ChildProcedureArguments(), syncHandler, cts.Token);
						wh.Wait();
					}
					finally
					{
						_facade.ExecutionStateChanged -= value2;
						if (script.DecompressOnExit)
						{
							_facade.DecompressExecutionStateChanged -= value3;
						}
						btnStop.Click -= value;
						_isScripInProgress = false;
						IsService = _facade.CheckForServiceMode();
						base.Dispatcher.BeginInvoke((Action)delegate
						{
							if (_diagramStateController.IsDiagramLoggingEnabled)
							{
								_diagramStateController.IsDiagramLoggingEnabled = false;
							}
						});
					}
				}
				finally
				{
					if (wh != null)
					{
						((IDisposable)wh).Dispose();
					}
				}
			}
			finally
			{
				if (cts != null)
				{
					((IDisposable)cts).Dispose();
				}
			}
		}).BeginInvoke(null, null);
		void StopEn()
		{
			btnStop.IsEnabled = true;
		}
	}

	public void AbortActiveProcedure()
	{
		btnStop.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
	}

	private void ShowSystemCondition1(SystemCondition condition)
	{
		if (messagePanel.Dispatcher.CheckAccess())
		{
			_storyboardCondition = condition;
			switch (condition.Severity)
			{
			case Severity.Error:
				messagePanel.Background = base.Resources["ErrorBackgroundBrush"] as LinearGradientBrush;
				imgMessage.Source = new ImageConverter().Convert("Images/Error.png") as ImageSource;
				break;
			case Severity.Warn:
				messagePanel.Background = base.Resources["WarningBackgroundBrush"] as LinearGradientBrush;
				imgMessage.Source = new ImageConverter().Convert("Images/Warning.png") as ImageSource;
				break;
			case Severity.Info:
				messagePanel.Background = base.Resources["InfoBackgroundBrush"] as LinearGradientBrush;
				imgMessage.Source = new ImageConverter().Convert("Images/Info.png") as ImageSource;
				break;
			case Severity.Dialog:
				messagePanel.Background = base.Resources["InfoBackgroundBrush"] as LinearGradientBrush;
				imgMessage.Source = new ImageConverter().Convert("Images/Info.png") as ImageSource;
				break;
			case Severity.Tip:
				messagePanel.Background = base.Resources["TipBackgroundBrush"] as LinearGradientBrush;
				imgMessage.Source = new ImageConverter().Convert("Images/lightbulb.png") as ImageSource;
				break;
			}
			btnConfirm.Visibility = ((!condition.ManualDismiss) ? Visibility.Hidden : Visibility.Visible);
			SetConfirmEnabled();
			btnClose.Visibility = (condition.ManualDismiss ? Visibility.Hidden : Visibility.Visible);
			lblSubject.Text = "{condition.Raised:F} {condition.Subject}";
			lblMessage.Text = condition.Description;
			bottomPanel.Visibility = Visibility.Hidden;
			double lineHeight = 16.0;
			int subjectLines = GetSubjectLines(lblMessage, ref lineHeight);
			if (dgMessage.ActualHeight < (double)(subjectLines * (int)lineHeight) + hdrBorder.Height)
			{
				storyGrid.RowDefinitions[0].Height = new GridLength((double)(subjectLines * (int)lineHeight) + hdrBorder.Height + 30.0);
			}
			if (subjectLines < 3)
			{
				propertiesPanel.Height = 50.0;
			}
			else
			{
				propertiesPanel.Height = subjectLines * (int)lineHeight;
			}
			bottomPanel.Visibility = Visibility.Visible;
		}
		else
		{
			ShowSystemConditionCallback method = ShowSystemCondition1;
			base.Dispatcher.BeginInvoke(method, condition);
		}
	}

	private void SetConfirmEnabled()
	{
		btnConfirm.IsEnabled = _lastExecutionState != 0 || (_storyboardCondition?.ManualDismiss ?? false);
	}

	private int GetSubjectLines(TextBlock tbMessage, ref double lineHeight)
	{
		double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
		string[] array = tbMessage.Text.Split('\n');
		int num = 1;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			FormattedText formattedText = new FormattedText(array2[i], CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(lblMessage.FontFamily, lblMessage.FontStyle, lblMessage.FontWeight, lblMessage.FontStretch), lblMessage.FontSize, Brushes.Black, pixelsPerDip);
			num += (int)(formattedText.Width / stkPanel.ActualWidth) + 1;
			lineHeight = formattedText.Height + 2.1;
		}
		return num;
	}

	private void Statistics_Click(object sender, RoutedEventArgs args)
	{
		this.ActivateAuxiliaryControl?.Invoke(this, EventArgs.Empty);
		if (_isSystemWindowOpen)
		{
			_dlgSystemInfo.WindowState = WindowState.Normal;
			_dlgSystemInfo.Show();
			_dlgSystemInfo.Focus();
			return;
		}
		_dlgSystemInfo = new SystemInfoWindow(_facade.MaintenanceInfos, _isService);
		_dlgSystemInfo.Closing += dlgSystemInfo_Closing;
		_dlgSystemInfo.Closed += dlgSystemInfo_Closed;
		if (_sysInfoOwner != null)
		{
			_dlgSystemInfo.Owner = _sysInfoOwner;
		}
		_dlgSystemInfo.Show();
		_dlgSystemInfo.Focus();
		_isSystemWindowOpen = true;
	}

	private void dlgSystemInfo_Closing(object sender, CancelEventArgs e)
	{
	}

	private void dlgSystemInfo_Closed(object sender, EventArgs e)
	{
		_isSystemWindowOpen = false;
	}

	public void SetActivateAuxiliaryControlEnabled(bool enabled)
	{
		base.Dispatcher.Invoke(Action);
		void Action()
		{
			_dlgSystemInfo.IsEnabled = enabled;
			if (!enabled && _dlgSystemInfo != null)
			{
				_dlgSystemInfo.Closing -= dlgSystemInfo_Closing;
				_dlgSystemInfo.Close();
				_dlgSystemInfo = null;
			}
		}
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		if (dgMessage.View is GridView gridView)
		{
			double num = dgMessage.ActualWidth - SystemParameters.VerticalScrollBarWidth * 2.0;
			gridView.Columns[0].Width = num * 0.18;
			gridView.Columns[1].Width = num * 0.25;
			gridView.Columns[2].Width = num * 0.57;
		}
	}

	private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
	{
		if (_dlgSystemInfo != null)
		{
			_dlgSystemInfo.Close();
			_dlgSystemInfo = null;
		}
	}

	public void confirmButton_Click(object sender, RoutedEventArgs e)
	{
		if (dgMessage.Dispatcher.CheckAccess())
		{
			SystemCondition systemCondition = dgMessage.SelectedItem as SystemCondition;
			bottomPanel.Visibility = Visibility.Hidden;
			if (systemCondition != null)
			{
				if (systemCondition.ManualDismiss)
				{
					_facade.DismissCondition(systemCondition);
				}
				if (systemCondition.Severity == Severity.Error)
				{
					this.ConfirmButtonEvent?.Invoke(systemCondition);
				}
			}
			dgMessage.IsHitTestVisible = true;
			dgMessage.SelectedIndex = -1;
			_storyboardCondition = null;
		}
		else
		{
			ConfirmConditionCallback method = confirmButton_Click;
			base.Dispatcher.BeginInvoke(method, sender, e);
		}
	}

	private void closeButton_Click(object sender, RoutedEventArgs e)
	{
		bottomPanel.Visibility = Visibility.Hidden;
		dgMessage.SelectedIndex = -1;
		_storyboardCondition = null;
		_facade.ActiveConditionClear();
	}

	private void dgMessage_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (dgMessage.SelectedItem is SystemCondition condition)
		{
			ShowSystemCondition1(condition);
		}
	}

	private void dgMessage_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (dgMessage.Items.Count > 0)
		{
			dgMessage.ScrollIntoView(dgMessage.Items[dgMessage.Items.Count - 1]);
		}
	}

	private void dgMessage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (dgMessage.SelectedItem is SystemCondition { ManualDismiss: not false })
		{
			btnConfirm.BeginStoryboard((Storyboard)storyGrid.Resources["flashAnimation"]);
			e.Handled = true;
		}
	}



}
}
