using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using BalticClassLib;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Bruker.Lc.Maintenance;
using Bruker.Lc.Metering;
using BrukerLC.Interfaces.ViewModelInterfaces;
using BrukerLC.Utils.Controls;

namespace BalticWpfControlLib.Diagram;

public class DiagramStateController
{
	private class ActivatableSet
	{
		public readonly IActivatable Activatable;

		public readonly SolidColorBrush ActiveColor;

		public ActivatableSet(IActivatable act, SolidColorBrush col)
		{
			Activatable = act;
			ActiveColor = col;
		}
	}

	private class TableDataSync
	{
		public bool IsValidA { get; set; }

		public bool IsValidB { get; set; }

		public double VolumeA { get; set; }

		public double VolumeB { get; set; }

		public bool IsValid
		{
			get
			{
				if (IsValidA)
				{
					return IsValidB;
				}
				return false;
			}
		}

		public void Reset()
		{
			bool isValidA = (IsValidB = false);
			IsValidA = isValidA;
		}

		public TableDataSync()
		{
		}

		public TableDataSync(TableDataSync item)
		{
			IsValidA = item.IsValidA;
			IsValidB = item.IsValidB;
			VolumeA = item.VolumeA;
			VolumeB = item.VolumeB;
		}
	}

	private class CircularTableBuffer
	{
		private readonly Queue<TableDataSync> _queue;

		private readonly int _size;

		public bool IsValid
		{
			get
			{
				if (_queue.Count < _size)
				{
					return false;
				}
				for (int i = 0; i < _queue.Count; i++)
				{
					if (!_queue.ElementAt(i).IsValid)
					{
						return false;
					}
				}
				return true;
			}
		}

		public int nItems => _queue.Count;

		public CircularTableBuffer(int size)
		{
			_queue = new Queue<TableDataSync>(size);
			_size = size;
		}

		public void Add(TableDataSync obj)
		{
			if (_queue.Count == _size)
			{
				_queue.Dequeue();
			}
			_queue.Enqueue(obj);
		}

		public (double deltaVolumeA, double deltaVolumeB) Difference()
		{
			double item = 0.0;
			double item2 = 0.0;
			if (_queue.Count == _size && _queue.Count > 1)
			{
				item = _queue.ElementAt(_queue.Count - 1).VolumeA - _queue.ElementAt(_queue.Count - 2).VolumeA;
				item2 = _queue.ElementAt(_queue.Count - 1).VolumeB - _queue.ElementAt(_queue.Count - 2).VolumeB;
			}
			return (deltaVolumeA: item, deltaVolumeB: item2);
		}

		public void Reset()
		{
			_queue.Clear();
		}
	}

	private class ActiveStateConfigurationHelper
	{
		public readonly Dictionary<int, IActivatable> ValveALinks = new Dictionary<int, IActivatable>();

		public readonly Dictionary<int, IActivatable> ValveBLinks = new Dictionary<int, IActivatable>();

		public readonly Dictionary<int, IActivatable> ValveILinks = new Dictionary<int, IActivatable>();

		public readonly Dictionary<int, IActivatable> ValveTLinks = new Dictionary<int, IActivatable>();

		public readonly IActivatable ValveA;

		public readonly IActivatable ValveB;

		public readonly IActivatable ValveI;

		public readonly IActivatable ValveT;

		public readonly IActivatable PumpA;

		public readonly IActivatable PumpB;

		public readonly IActivatable FlowA;

		public readonly IActivatable FlowB;

		public readonly IActivatable Separator;

		public readonly IActivatable Trap;

		public readonly IActivatable Loop;

		public readonly IActivatable Oven;

		public readonly IActivatable PumpAFrontToValve;

		public readonly IActivatable PumpARearToValve;

		public readonly IActivatable SolventToValveA;

		public readonly IActivatable ValveAWaste;

		public readonly IActivatable ValveAToFS;

		public readonly IActivatable ValveAToInjectionValve;

		public readonly IActivatable InjectToTrap;

		public readonly IActivatable FSAToMixTee;

		public readonly IActivatable InjectionValveToWaste;

		public readonly IActivatable ValveBToFS;

		public readonly IActivatable PumpBRearToValve;

		public readonly IActivatable PumpBFrontToValve;

		public readonly IActivatable SolventToValveB;

		public readonly IActivatable ValveBWaste;

		public readonly IActivatable FSBToMixTee;

		public readonly IActivatable TrapValveToWaste;

		public readonly IActivatable MixTeeToTrapValve;

		public readonly IActivatable TransferLine;

		public readonly IActivatable SolventA;

		public readonly IActivatable SolventB;

		public readonly IActivatable WasteT;

		public readonly IActivatable LineBreakOnPumpALink1;

		public readonly IActivatable LineBreakOnPumpALink2;

		public readonly IActivatable ValveAToPlug;

		public readonly IActivatable ValveBToPlug;

		public readonly IActivatable MixTeeToInjectionValve;

		public readonly IActivatable InjectionValveToSeparator;

		public ActiveStateConfigurationHelper(dynamic balticConfig, IDictionary<string, object> models)
		{
			dynamic naming = balticConfig.Naming;
			ValveA = (IActivatable)models[naming.ValveA];
			ValveB = (IActivatable)models[naming.ValveB];
			ValveI = (IActivatable)models[naming.ValveI];
			ValveT = (IActivatable)models[naming.ValveT];
			PumpA = (IActivatable)models[naming.PumpA];
			PumpB = (IActivatable)models[naming.PumpB];
			FlowA = (IActivatable)models[naming.FlowA];
			FlowB = (IActivatable)models[naming.FlowB];
			Separator = (IActivatable)models[naming.Separator];
			Trap = (IActivatable)models[naming.Trap];
			Loop = (IActivatable)models[naming.Loop];
			Oven = (IActivatable)models[naming.Oven];
			PumpAFrontToValve = (IActivatable)models[naming.PumpAFrontToValve];
			PumpARearToValve = (IActivatable)models[naming.PumpARearToValve];
			SolventToValveA = (IActivatable)models[naming.SolventToValveA];
			ValveAWaste = (IActivatable)models[naming.ValveAWaste];
			ValveAToFS = (IActivatable)models[naming.ValveAToFS];
			ValveAToInjectionValve = (IActivatable)models[naming.ValveAToInjectionValve];
			InjectToTrap = (IActivatable)models[naming.InjectToTrap];
			FSAToMixTee = (IActivatable)models[naming.FSAToMixTee];
			InjectionValveToWaste = (IActivatable)models[naming.InjectionValveToWaste];
			LineBreakOnPumpALink1 = (IActivatable)models[naming.LineBreakPumpA1];
			LineBreakOnPumpALink2 = (IActivatable)models[naming.LineBreakPumpA2];
			ValveBToFS = (IActivatable)models[naming.ValveBToFS];
			PumpBRearToValve = (IActivatable)models[naming.PumpBRearToValve];
			PumpBFrontToValve = (IActivatable)models[naming.PumpBFrontToValve];
			SolventToValveB = (IActivatable)models[naming.SolventToValveB];
			ValveBWaste = (IActivatable)models[naming.ValveBWaste];
			FSBToMixTee = (IActivatable)models[naming.FSBToMixTee];
			TrapValveToWaste = (IActivatable)models[naming.TrapValveToWaste];
			MixTeeToTrapValve = (IActivatable)models[naming.MixTeeToTrapValve];
			TransferLine = (IActivatable)models[naming.TransferLine];
			SolventA = (IActivatable)models[naming.SolventA];
			SolventB = (IActivatable)models[naming.SolventB];
			WasteT = (IActivatable)models[naming.WasteForValveT];
			ValveAToPlug = (IActivatable)models[naming.ValveAToPlug];
			MixTeeToInjectionValve = (IActivatable)models[naming.MixTeeToInjectionValve];
			InjectionValveToSeparator = (IActivatable)models[naming.InjectionValveToSeparator];
			ValveBToPlug = (IActivatable)models[naming.ValveBToPlug];
			ExpandoObject source = balticConfig.ValveAngleToLinkList;
			ExpandoObject expandoObject = source.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == naming.ValveA).Value as ExpandoObject;
			int num = 0;
			if (expandoObject != null)
			{
				int l;
				for (l = 1; l <= expandoObject.Count(); l++)
				{
					string text = (string)expandoObject.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == l.ToString()).Value;
					ValveALinks.Add(ValvePosition(num), (text.Equals(naming.None)) ? null : ((IActivatable)models[text]));
					num += 60;
				}
			}
			if (source.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == naming.ValveB).Value is ExpandoObject source2)
			{
				num = 0;
				int k;
				for (k = 1; k <= source2.Count(); k++)
				{
					string text2 = (string)source2.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == k.ToString()).Value;
					ValveBLinks.Add(ValvePosition(num), (text2.Equals(naming.None)) ? null : ((IActivatable)models[text2]));
					num += 60;
				}
			}
			if (source.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == naming.ValveI).Value is ExpandoObject source3)
			{
				num = 0;
				int j;
				for (j = 1; j <= source3.Count(); j++)
				{
					string text3 = (string)source3.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == j.ToString()).Value;
					ValveILinks.Add(ValvePosition(num), (text3.Equals(naming.None)) ? null : ((IActivatable)models[text3]));
					num += 60;
				}
			}
			if (!(source.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == naming.ValveT).Value is ExpandoObject source4))
			{
				return;
			}
			num = 0;
			int i;
			for (i = 1; i <= source4.Count(); i++)
			{
				string text4 = (string)source4.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == i.ToString()).Value;
				ValveTLinks.Add(ValvePosition(num), (text4.Equals(naming.None)) ? null : ((IActivatable)models[text4]));
				num += 60;
			}
		}
	}

	private const int _maxValvePositions = 12;

	private readonly SolidColorBrush _blueBrush = new SolidColorBrush(Color.FromRgb(15, 112, 184));

	private readonly SolidColorBrush _redBrush = new SolidColorBrush(Color.FromRgb(193, 41, 45));

	private readonly SolidColorBrush _purpleBrush = new SolidColorBrush(Color.FromRgb(99, 50, 138));

	private readonly BalticInstrumentFacade _facade;

	private readonly DiagramControl _diagram;

	private readonly List<IMeteringChannel> _channels = new List<IMeteringChannel>();

	private int _valveAPos = -1;

	private int _valveBPos = -1;

	private int _valveIPos = -1;

	private int _valveTPos = -1;

	private int _valveAAngle;

	private int _valveBAngle;

	private int _valveIAngle;

	private int _valveTAngle;

	private readonly Dictionary<int, ICollection<ActivatableSet>> _states = new Dictionary<int, ICollection<ActivatableSet>>();

	private readonly Dictionary<string, object> _models = new Dictionary<string, object>();

	private readonly string _privatePath;

	private readonly bool _isEditable;

	private readonly bool _isOvenInstalled;

	private static bool _isCTCSimulate;

	private List<BalticPreferences.CapillaryPreference> _prefCapillaries;

	private DateTime _lastLoggingDateTime = DateTime.MinValue;

	private DateTime _startLoggingDateTime = DateTime.MinValue;

	private TableDataSync _currentTableItem = new TableDataSync();

	private CircularTableBuffer _tableRowBuffer = new CircularTableBuffer(2);

	private int _sysInfoLoggingIntervalMs;

	private int _systemInfoMaxLoggingEntries;

	private bool _isDiagramLoggingEnabled;

	private object _loggingLock = new object();

	public List<BalticHWProfile.CapillaryItem> HwCapillaries { get; }

	public bool IsDiagramLoggingEnabled
	{
		get
		{
			return _isDiagramLoggingEnabled;
		}
		set
		{
			lock (_loggingLock)
			{
				ITableViewModel tableViewModel = (ITableViewModel)(_diagram?.PumpLogDataContext);
				if (tableViewModel != null)
				{
					if (_isDiagramLoggingEnabled != value)
					{
						tableViewModel.Reset();
						_tableRowBuffer.Reset();
					}
					tableViewModel.IsTableVisible = value;
				}
				_isDiagramLoggingEnabled = value;
			}
		}
	}

	public DiagramStateController(BalticInstrumentFacade facade, List<BalticHWProfile.CapillaryItem> capillaries, List<BalticPreferences.CapillaryPreference> prefCapillaries, DiagramControl diagram, bool isEditable, bool isOvenInstalled = false, bool displayPressureAsPsi = false)
	{
		_privatePath = facade.PrivatePath;
		_isEditable = isEditable;
		HwCapillaries = capillaries;
		_prefCapillaries = prefCapillaries;
		_isOvenInstalled = isOvenInstalled;
		_facade = facade;
		_isCTCSimulate = facade.IsCTCSimulate;
		_diagram = diagram;
		_sysInfoLoggingIntervalMs = (int)((double)_facade.BalticConfiguration.SystemInfoLoggingIntervalMin * 60000.0);
		_systemInfoMaxLoggingEntries = (int)_facade.BalticConfiguration.SystemInfoMaxLoggingEntries;
		ConfigureDataContexts();
		MapDataContexts();
		ConfigureActiveStates();
		TransformChannels(_facade.GaugeChannels, _channels, displayPressureAsPsi);
		foreach (IMeteringChannel channel in _channels)
		{
			if (channel.ChannelInfo.Id.Source.ToString().Contains("valve"))
			{
				channel.ChannelDataChanged += valve_ChannelDataChanged;
			}
			if (channel.ChannelInfo.Id.Source.ToString().Contains("pump"))
			{
				channel.ChannelDataChanged += pump_ChannelDataChanged;
			}
			if (channel.ChannelInfo.Id.Source.ToString().Contains("flowsensor"))
			{
				channel.ChannelDataChanged += sensor_ChannelDataChanged;
			}
			if (channel.ChannelInfo.Id.Source.ToString().Contains("oven"))
			{
				channel.ChannelDataChanged += oven_ChannelDataChanged;
			}
		}
		CheckSystemConditions();
		CheckSignalizeConditions();
		CheckSignalizeTextConditions();
		((INotifyCollectionChanged)facade.SystemConditions).CollectionChanged += scc_CollectionChanged;
		facade.SignalizeConditions.SignalizeCollectionChanged += scc_SignalizeCollectionChanged;
		facade.SignalizeTextConditions.SignalizeTextCollectionChanged += SignalizeTextConditions_SignalizeTextCollectionChanged;
		facade.ExecutionStateChanged += facade_ExecutionStateChanged;
		diagram.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
	}

	private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
	{
		_facade.ExecutionStateChanged -= facade_ExecutionStateChanged;
		((INotifyCollectionChanged)_facade.SystemConditions).CollectionChanged -= scc_CollectionChanged;
		foreach (IMeteringChannel channel in _channels)
		{
			if (channel.ChannelInfo.Id.Source.ToString().Contains("valve"))
			{
				channel.ChannelDataChanged -= valve_ChannelDataChanged;
			}
			if (channel.ChannelInfo.Id.Source.ToString().Contains("pump"))
			{
				channel.ChannelDataChanged -= pump_ChannelDataChanged;
			}
			if (channel.ChannelInfo.Id.Source.ToString().Contains("flowsensor"))
			{
				channel.ChannelDataChanged -= sensor_ChannelDataChanged;
			}
		}
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

	private void facade_ExecutionStateChanged(object sender, ExecutionStateChangedEventArgs e)
	{
		IEnumerable<string> subjects;
		if (e.ExecutionState == ProcedureExecutionState.Started)
		{
			subjects = _models.Keys;
			_diagram.Dispatcher.Invoke(Action);
		}
		void Action()
		{
			ClearErrors(subjects);
		}
	}

	private void scc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		CheckSystemConditions();
	}

	private void CheckSystemConditions()
	{
		SystemCondition[] conditions = _facade.SystemConditions.ToArray();
		_diagram.Dispatcher.Invoke(Action);
		void Action()
		{
			List<string> list = new List<string>();
			SystemCondition[] array = conditions;
			foreach (SystemCondition systemCondition in array)
			{
				string[] array2 = ExtractComponents(systemCondition);
				foreach (string text in array2)
				{
					string error = $"{systemCondition.Raised}\n{systemCondition.Source}\n{systemCondition.Description}";
					if (_models.TryGetValue(text, out var value))
					{
						list.Add(text);
						if (value is IErrorAware errorAware)
						{
							errorAware.SetError(error);
						}
					}
				}
			}
			ClearErrors(_models.Keys.Except(list));
		}
	}

	private static string[] ExtractComponents(SystemCondition condition)
	{
		MatchCollection matchCollection = Regex.Matches(condition.Subject, "@(\\S+)");
		string[] array = new string[matchCollection.Count];
		int num = 0;
		foreach (Match item in matchCollection)
		{
			array[num++] = item.Groups[1].Value;
		}
		return array;
	}

	private void ClearErrors(IEnumerable<string> subjects)
	{
		foreach (string subject in subjects)
		{
			if (_models[subject] is IErrorAware errorAware)
			{
				errorAware.ClearError();
			}
		}
	}

	private void scc_SignalizeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		CheckSignalizeConditions();
	}

	private void SignalizeTextConditions_SignalizeTextCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		CheckSignalizeTextConditions();
	}

	private void CheckSignalizeConditions()
	{
		SystemConditionManager conditions = _facade.SignalizeConditions;
		_diagram.Dispatcher.Invoke(Action);
		void Action()
		{
			foreach (SignalizeCondition key in conditions.SignalizeConditions.Keys)
			{
				bool isSignalize = key.RGBColor[0] != 0 || key.RGBColor[1] != 0 || key.RGBColor[2] != 0;
				object[] args = key.Args;
				for (int i = 0; i < args.Length; i++)
				{
					string[] array = ((string)args[i]).Split(':');
					if (array.Length != 0 && _models.TryGetValue(array[0], out var value) && value is ISignalizeAware signalizeAware)
					{
						if (!(signalizeAware is IValveViewModel { Connections: var connections }))
						{
							signalizeAware.IsSignalize = isSignalize;
							if (signalizeAware.IsSignalize)
							{
								signalizeAware.SignalizeBrush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(key.RGBColor[0]), Convert.ToByte(key.RGBColor[1]), Convert.ToByte(key.RGBColor[2])));
							}
						}
						else if (array.Length == 1)
						{
							signalizeAware.IsSignalize = isSignalize;
							if (signalizeAware.IsSignalize)
							{
								signalizeAware.SignalizeBrush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(key.RGBColor[0]), Convert.ToByte(key.RGBColor[1]), Convert.ToByte(key.RGBColor[2])));
							}
							foreach (ConnectionViewModel item in connections)
							{
								item.IsSignalize = isSignalize;
								item.SignalizeBrush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(key.RGBColor[0]), Convert.ToByte(key.RGBColor[1]), Convert.ToByte(key.RGBColor[2])));
							}
						}
						else
						{
							int port1;
							int port2;
							for (int j = 1; j < array.Length; j++)
							{
								string[] array2 = array[j].Split('-');
								if (array2.Length > 1)
								{
									try
									{
										port1 = Convert.ToInt32(array2[0]);
										port2 = Convert.ToInt32(array2[1]);
										ConnectionViewModel connectionViewModel = (ConnectionViewModel)connections.FirstOrDefault((IConnectionViewModel x) => x.FirstPort == port1 && x.SecondPort == port2);
										if (connectionViewModel != null)
										{
											connectionViewModel.IsSignalize = isSignalize;
											connectionViewModel.SignalizeBrush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(key.RGBColor[0]), Convert.ToByte(key.RGBColor[1]), Convert.ToByte(key.RGBColor[2])));
										}
										else
										{
											connectionViewModel = (ConnectionViewModel)connections.FirstOrDefault((IConnectionViewModel x) => x.FirstPort == port2 && x.SecondPort == port1);
											if (connectionViewModel != null)
											{
												connectionViewModel.IsSignalize = isSignalize;
												connectionViewModel.SignalizeBrush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(key.RGBColor[0]), Convert.ToByte(key.RGBColor[1]), Convert.ToByte(key.RGBColor[2])));
											}
										}
									}
									catch (Exception)
									{
									}
								}
							}
						}
					}
				}
			}
		}
	}

	private void CheckSignalizeTextConditions()
	{
		SystemConditionManager conditions = _facade.SignalizeTextConditions;
		_diagram.Dispatcher.Invoke(Action);
		void Action()
		{
			foreach (SignalizeTextCondition key in conditions.SignalizeTextConditions.Keys)
			{
				bool isSignalizeText = key.RGBColor[0] != 0 || key.RGBColor[1] != 0 || key.RGBColor[2] != 0;
				object[] args = key.Args;
				for (int i = 0; i < args.Length; i++)
				{
					string[] array = ((string)args[i]).Split(':');
					if (array.Length != 0 && _models.TryGetValue(array[0], out var value) && value is ISignalizeTextAware signalizeTextAware)
					{
						signalizeTextAware.IsSignalizeText = isSignalizeText;
						if (signalizeTextAware.IsSignalizeText)
						{
							signalizeTextAware.SignalizeTextBrush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(key.RGBColor[0]), Convert.ToByte(key.RGBColor[1]), Convert.ToByte(key.RGBColor[2])));
						}
						if (signalizeTextAware is IValveViewModel { Connections: var connections })
						{
							if (array.Length == 1)
							{
								foreach (ConnectionViewModel item in connections)
								{
									item.IsSignalizeText = isSignalizeText;
									item.SignalizeTextBrush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(key.RGBColor[0]), Convert.ToByte(key.RGBColor[1]), Convert.ToByte(key.RGBColor[2])));
								}
							}
							else
							{
								int port1;
								int port2;
								for (int j = 1; j < array.Length; j++)
								{
									string[] array2 = array[j].Split('-');
									if (array2.Length > 1)
									{
										try
										{
											port1 = Convert.ToInt32(array2[0]);
											port2 = Convert.ToInt32(array2[1]);
											ConnectionViewModel connectionViewModel = (ConnectionViewModel)connections.FirstOrDefault((IConnectionViewModel x) => x.FirstPort == port1 && x.SecondPort == port2);
											if (connectionViewModel != null)
											{
												connectionViewModel.IsSignalizeText = isSignalizeText;
												connectionViewModel.SignalizeTextBrush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(key.RGBColor[0]), Convert.ToByte(key.RGBColor[1]), Convert.ToByte(key.RGBColor[2])));
											}
											else
											{
												connectionViewModel = (ConnectionViewModel)connections.FirstOrDefault((IConnectionViewModel x) => x.FirstPort == port2 && x.SecondPort == port1);
												if (connectionViewModel != null)
												{
													connectionViewModel.IsSignalizeText = isSignalizeText;
													connectionViewModel.SignalizeTextBrush = new SolidColorBrush(Color.FromRgb(Convert.ToByte(key.RGBColor[0]), Convert.ToByte(key.RGBColor[1]), Convert.ToByte(key.RGBColor[2])));
												}
											}
										}
										catch (Exception)
										{
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	private void MapDataContexts()
	{
		dynamic val = _facade.BalticConfiguration.Naming;
		_models.Add(val.ValveA, _diagram.ValveDDataContext);
		_models.Add(val.ValveB, _diagram.ValveCDataContext);
		_models.Add(val.ValveI, _diagram.ValveADataContext);
		_models.Add(val.ValveT, _diagram.ValveBDataContext);
		_models.Add(val.PumpA, _diagram.PumpADataContext);
		_models.Add(val.PumpB, _diagram.PumpBDataContext);
		_models.Add(val.FlowA, _diagram.SensorADataContext);
		_models.Add(val.FlowB, _diagram.SensorBDataContext);
		_models.Add(val.Separator, _diagram.OutputDataContext);
		_models.Add(val.Trap, _diagram.TrapDataContext);
		_models.Add(val.Loop, _diagram.LoopDataContext);
		_models.Add(val.Oven, _diagram.TemperatureDataContext);
		_models.Add(val.PumpAFrontToValve, _diagram.PumpAToValveDTopLinkDataContext);
		_models.Add(val.PumpARearToValve, _diagram.PumpAToValveDBottomLinkDataContext);
		_models.Add(val.SolventToValveA, _diagram.SolventAtoValveDLinkDataContext);
		_models.Add(val.ValveAWaste, _diagram.WasteDtoValveDLinkDataContext);
		_models.Add(val.ValveAToFS, _diagram.SensorAtoValveDLinkDataContext);
		_models.Add(val.ValveAToInjectionValve, _diagram.ValveDToValveALinkDataContext);
		_models.Add(val.InjectToTrap, _diagram.ValveAToValveBLinkDataContext);
		_models.Add(val.FSAToMixTee, _diagram.SensorAToMidLinkDataContext);
		_models.Add(val.InjectionValveToWaste, _diagram.WasteAtoValveALinkDataContext);
		_models.Add(val.ValveBToFS, _diagram.SensorBtoValveCLinkDataContext);
		_models.Add(val.PumpBRearToValve, _diagram.PumpBToValveCBottomLinkDataContext);
		_models.Add(val.PumpBFrontToValve, _diagram.PumpBtoValveCTopLinkDataContext);
		_models.Add(val.SolventToValveB, _diagram.SolventBtoValveCTopLinkDataContext);
		_models.Add(val.ValveBWaste, _diagram.WasteCtoValveCLinkDataContext);
		_models.Add(val.FSBToMixTee, _diagram.SensorBToMidLinkDataContext);
		_models.Add(val.TrapValveToWaste, _diagram.WasteBtoValveBLinkDataContext);
		_models.Add(val.MixTeeToTrapValve, _diagram.MidToValveBLinkDataContext);
		_models.Add(val.TransferLine, _diagram.ValveBToOutputLinkDataContext);
		_models.Add(val.SolventA, _diagram.SolventADataContext);
		_models.Add(val.SolventB, _diagram.SolventBDataContext);
		_models.Add(val.WasteForValveT, _diagram.WasteBDataContext);
		_models.Add(val.LineBreakPumpA1, _diagram.LineBreakOnPumpALink1DataContext);
		_models.Add(val.LineBreakPumpA2, _diagram.LineBreakOnPumpALink2DataContext);
		_models.Add(val.ValveBToPlug, _diagram.BlingPlugToValveCLinkDataContext);
		_models.Add(val.ValveAToPlug, _diagram.ValveAToPlugLinkDataContext);
		_models.Add(val.MixTeeToInjectionValve, _diagram.MixTeeToInjectionValveLinkDataContext);
		_models.Add(val.InjectionValveToSeparator, _diagram.InjectionValveToSeparatorLinkDataContext);
		_models.Add(val.ValveAPlug, _diagram.ValveAPlugDataContextDataContext);
		foreach (IConnectionViewModel connection in ((IValveViewModel)_diagram.ValveADataContext).Connections)
		{
			_models.Add(connection.GetHashCode().ToString(), connection);
		}
		foreach (IConnectionViewModel connection2 in ((IValveViewModel)_diagram.ValveBDataContext).Connections)
		{
			_models.Add(connection2.GetHashCode().ToString(), connection2);
		}
		foreach (IConnectionViewModel connection3 in ((IValveViewModel)_diagram.ValveCDataContext).Connections)
		{
			_models.Add(connection3.GetHashCode().ToString(), connection3);
		}
		foreach (IConnectionViewModel connection4 in ((IValveViewModel)_diagram.ValveDDataContext).Connections)
		{
			_models.Add(connection4.GetHashCode().ToString(), connection4);
		}
		ExpandoObject source = _facade.BalticConfiguration.DiagramHideList;
		int i;
		for (i = 1; i <= source.Count(); i++)
		{
			try
			{
				string key = (string)source.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == i.ToString()).Value;
				if (_models.TryGetValue(key, out var value))
				{
					((IVisibleEnableable)value).IsVisible = false;
				}
			}
			catch (Exception)
			{
			}
		}
		object obj = default(object);
		if ((!_facade.IsOvenDetected || !_isOvenInstalled) && (_models.TryGetValue(val.Oven, out obj) ? true : false))
		{
			((IVisibleEnableable)obj).IsNearTransparent = true;
		}
	}

	public void UpdateServiceMode(bool isService)
	{
		if (_diagram != null)
		{
			((IValveViewModel)_diagram.ValveDDataContext).IsService = isService;
			((IValveViewModel)_diagram.ValveCDataContext).IsService = isService;
			((IValveViewModel)_diagram.ValveADataContext).IsService = isService;
			((IValveViewModel)_diagram.ValveBDataContext).IsService = isService;
			((IPumpViewModel)_diagram.PumpADataContext).IsService = isService;
			((IPumpViewModel)_diagram.PumpBDataContext).IsService = isService;
		}
	}

	public void RevertAllCapillaries()
	{
		LoadCapillaryPreferences();
		((LinkViewModel)_diagram.ValveDToValveALinkDataContext).Revert();
		((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).Revert();
		((TrapViewModel)_diagram.TrapDataContext).Revert();
		((LoopViewModel)_diagram.LoopDataContext).Revert();
		((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).Revert();
		((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).Revert();
		((LinkViewModel)_diagram.SensorAToMidLinkDataContext).Revert();
		((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).Revert();
		((LinkViewModel)_diagram.SensorBToMidLinkDataContext).Revert();
		((LinkViewModel)_diagram.MidToValveBLinkDataContext).Revert();
		SetHwCapillaries();
		SetPreferenceDefaults();
	}

	public void LoadCapillaryPreferences()
	{
		BalticPreferences balticPreferences;
		try
		{
			using StreamReader streamReader = new StreamReader(Path.Combine(_privatePath, "Preferences.xml"));
			balticPreferences = (BalticPreferences)Utility.FromXML(streamReader.ReadToEnd(), typeof(BalticPreferences));
		}
		catch (Exception)
		{
			balticPreferences = new BalticPreferences();
		}
		_prefCapillaries = balticPreferences.Capillaries;
		dynamic naming = _facade.BalticConfiguration.Naming;
		BalticPreferences.CapillaryPreference capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.Loop);
		if (capillaryPreference != null)
		{
			((LoopViewModel)_diagram.LoopDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((LoopViewModel)_diagram.LoopDataContext).DefaultID = capillaryPreference.DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.Trap);
		if (capillaryPreference != null)
		{
			((TrapViewModel)_diagram.TrapDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((TrapViewModel)_diagram.TrapDataContext).DefaultID = capillaryPreference.DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.TransferLine);
		if (capillaryPreference != null)
		{
			((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).DefaultID = capillaryPreference.DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveAToInjectionValve);
		if (capillaryPreference != null)
		{
			((LinkViewModel)_diagram.ValveDToValveALinkDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((LinkViewModel)_diagram.ValveDToValveALinkDataContext).DefaultID = capillaryPreference.DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveAToFS);
		if (capillaryPreference != null)
		{
			((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).DefaultID = capillaryPreference.DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.InjectToTrap);
		if (capillaryPreference != null)
		{
			((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).DefaultID = capillaryPreference.DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.FSAToMixTee);
		if (capillaryPreference != null)
		{
			((LinkViewModel)_diagram.SensorAToMidLinkDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((LinkViewModel)_diagram.SensorAToMidLinkDataContext).DefaultID = capillaryPreference.DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveBToFS);
		if (capillaryPreference != null)
		{
			((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).DefaultID = capillaryPreference.DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.FSBToMixTee);
		if (capillaryPreference != null)
		{
			((LinkViewModel)_diagram.SensorBToMidLinkDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((LinkViewModel)_diagram.SensorBToMidLinkDataContext).DefaultID = capillaryPreference.DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.MixTeeToTrapValve);
		if (capillaryPreference != null)
		{
			((LinkViewModel)_diagram.MidToValveBLinkDataContext).DefaultLength = capillaryPreference.DefaultLength;
			((LinkViewModel)_diagram.MidToValveBLinkDataContext).DefaultID = capillaryPreference.DefaultID;
		}
	}

	public void SetPreferenceDefaults()
	{
		dynamic naming = _facade.BalticConfiguration.Naming;
		BalticPreferences.CapillaryPreference capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.Loop);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LoopViewModel)_diagram.LoopDataContext).DefaultLength;
			capillaryPreference.DefaultID = ((LoopViewModel)_diagram.LoopDataContext).DefaultID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.Trap);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((TrapViewModel)_diagram.TrapDataContext).Length;
			capillaryPreference.DefaultID = ((TrapViewModel)_diagram.TrapDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.TransferLine);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveAToInjectionValve);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.ValveDToValveALinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.ValveDToValveALinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveAToFS);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.InjectToTrap);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.FSAToMixTee);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = (capillaryPreference.Length = ((LinkViewModel)_diagram.SensorAToMidLinkDataContext).Length);
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.SensorAToMidLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveBToFS);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.FSBToMixTee);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.SensorBToMidLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.SensorBToMidLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.MixTeeToTrapValve);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.MidToValveBLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.MidToValveBLinkDataContext).ID;
		}
		BalticPreferences balticPreferences;
		try
		{
			using StreamReader streamReader = new StreamReader(Path.Combine(_privatePath, "Preferences.xml"));
			balticPreferences = (BalticPreferences)Utility.FromXML(streamReader.ReadToEnd(), typeof(BalticPreferences));
		}
		catch (Exception)
		{
			balticPreferences = new BalticPreferences();
		}
		balticPreferences.Capillaries = _prefCapillaries;
		try
		{
			string value = Utility.ToXML(balticPreferences);
			using StreamWriter streamWriter = File.CreateText(Path.Combine(_privatePath, "Preferences.xml"));
			streamWriter.Write(value);
			streamWriter.Close();
		}
		catch (IOException)
		{
		}
	}

	public void SetHwCapillaries()
	{
		dynamic naming = _facade.BalticConfiguration.Naming;
		BalticHWProfile.CapillaryItem capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.Loop);
		BalticPreferences.CapillaryPreference capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.Loop);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = ((LoopViewModel)_diagram.LoopDataContext).Length);
			capillaryItem.ID = (capillaryPreference.ID = ((LoopViewModel)_diagram.LoopDataContext).ID);
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.Trap);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.Trap);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = ((TrapViewModel)_diagram.TrapDataContext).Length);
			capillaryItem.ID = (capillaryPreference.ID = ((TrapViewModel)_diagram.TrapDataContext).ID);
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.TransferLine);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.TransferLine);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = ((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).Length);
			capillaryItem.ID = (capillaryPreference.ID = ((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).ID);
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.ValveAToInjectionValve);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveAToInjectionValve);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = ((LinkViewModel)_diagram.ValveDToValveALinkDataContext).Length);
			capillaryItem.ID = (capillaryPreference.ID = ((LinkViewModel)_diagram.ValveDToValveALinkDataContext).ID);
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.ValveAToFS);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveAToFS);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = ((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).Length);
			capillaryItem.ID = (capillaryPreference.ID = ((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).ID);
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.InjectToTrap);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.InjectToTrap);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = ((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).Length);
			capillaryItem.ID = (capillaryPreference.ID = ((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).ID);
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.FSAToMixTee);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.FSAToMixTee);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = (capillaryPreference.Length = ((LinkViewModel)_diagram.SensorAToMidLinkDataContext).Length));
			capillaryItem.ID = (capillaryPreference.ID = ((LinkViewModel)_diagram.SensorAToMidLinkDataContext).ID);
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.ValveBToFS);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveBToFS);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = ((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).Length);
			capillaryItem.ID = (capillaryPreference.ID = ((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).ID);
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.FSBToMixTee);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.FSBToMixTee);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = ((LinkViewModel)_diagram.SensorBToMidLinkDataContext).Length);
			capillaryItem.ID = (capillaryPreference.ID = ((LinkViewModel)_diagram.SensorBToMidLinkDataContext).ID);
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName == naming.MixTeeToTrapValve);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.MixTeeToTrapValve);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryItem.Length = (capillaryPreference.Length = ((LinkViewModel)_diagram.MidToValveBLinkDataContext).Length);
			capillaryItem.ID = (capillaryPreference.ID = ((LinkViewModel)_diagram.MidToValveBLinkDataContext).ID);
		}
		BalticPreferences balticPreferences;
		try
		{
			using StreamReader streamReader = new StreamReader(Path.Combine(_privatePath, "Preferences.xml"));
			balticPreferences = (BalticPreferences)Utility.FromXML(streamReader.ReadToEnd(), typeof(BalticPreferences));
		}
		catch (Exception)
		{
			balticPreferences = new BalticPreferences();
		}
		balticPreferences.Capillaries = _prefCapillaries;
		try
		{
			string value = Utility.ToXML(balticPreferences);
			using StreamWriter streamWriter = File.CreateText(Path.Combine(_privatePath, "Preferences.xml"));
			streamWriter.Write(value);
			streamWriter.Close();
		}
		catch (IOException)
		{
		}
	}

	public void SaveAsDefaultCapillaries()
	{
		dynamic naming = _facade.BalticConfiguration.Naming;
		BalticPreferences.CapillaryPreference capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.Loop);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LoopViewModel)_diagram.LoopDataContext).Length;
			capillaryPreference.DefaultID = ((LoopViewModel)_diagram.LoopDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.Trap);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((TrapViewModel)_diagram.TrapDataContext).Length;
			capillaryPreference.DefaultID = ((TrapViewModel)_diagram.TrapDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.TransferLine);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveAToInjectionValve);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.ValveDToValveALinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.ValveDToValveALinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveAToFS);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.InjectToTrap);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.FSAToMixTee);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.SensorAToMidLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.SensorAToMidLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.ValveBToFS);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.FSBToMixTee);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.SensorBToMidLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.SensorBToMidLinkDataContext).ID;
		}
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName == naming.MixTeeToTrapValve);
		if (capillaryPreference != null)
		{
			capillaryPreference.DefaultLength = ((LinkViewModel)_diagram.MidToValveBLinkDataContext).Length;
			capillaryPreference.DefaultID = ((LinkViewModel)_diagram.MidToValveBLinkDataContext).ID;
		}
		BalticPreferences balticPreferences;
		try
		{
			using StreamReader streamReader = new StreamReader(Path.Combine(_privatePath, "Preferences.xml"));
			balticPreferences = (BalticPreferences)Utility.FromXML(streamReader.ReadToEnd(), typeof(BalticPreferences));
		}
		catch (Exception)
		{
			balticPreferences = new BalticPreferences();
		}
		balticPreferences.Capillaries = _prefCapillaries;
		try
		{
			string value = Utility.ToXML(balticPreferences);
			using StreamWriter streamWriter = File.CreateText(Path.Combine(_privatePath, "Preferences.xml"));
			streamWriter.Write(value);
			streamWriter.Close();
		}
		catch (IOException)
		{
		}
	}

	public void RevertAllCapillariesToFactory()
	{
		((LinkViewModel)_diagram.ValveDToValveALinkDataContext).RevertToFactory();
		((LinkViewModel)_diagram.ValveBToOutputLinkDataContext).RevertToFactory();
		((TrapViewModel)_diagram.TrapDataContext).RevertToFactory();
		((LoopViewModel)_diagram.LoopDataContext).RevertToFactory();
		((LinkViewModel)_diagram.SensorAtoValveDLinkDataContext).RevertToFactory();
		((LinkViewModel)_diagram.ValveAToValveBLinkDataContext).RevertToFactory();
		((LinkViewModel)_diagram.SensorAToMidLinkDataContext).RevertToFactory();
		((LinkViewModel)_diagram.SensorBtoValveCLinkDataContext).RevertToFactory();
		((LinkViewModel)_diagram.SensorBToMidLinkDataContext).RevertToFactory();
		((LinkViewModel)_diagram.MidToValveBLinkDataContext).RevertToFactory();
		SetHwCapillaries();
		SetPreferenceDefaults();
	}

	private void ConfigureDataContexts()
	{
		dynamic naming = _facade.BalticConfiguration.Naming;
		_diagram.ValveADataContext = new ValveDataContext
		{
			AngleMargin = new Thickness(-6.0, 46.0, 0.0, 0.0),
			Connections = new ObservableCollection<IConnectionViewModel>
			{
				new ConnectionViewModel
				{
					FirstPort = 2,
					SecondPort = 3,
					ActiveBrush = _blueBrush
				},
				new ConnectionViewModel
				{
					FirstPort = 4,
					SecondPort = 5,
					ActiveBrush = _blueBrush
				},
				new ConnectionViewModel
				{
					FirstPort = 6,
					SecondPort = 1,
					ActiveBrush = _blueBrush
				}
			}
		};
		_diagram.ValveBDataContext = new ValveDataContext
		{
			AngleMargin = new Thickness(-6.0, 46.0, 0.0, 0.0),
			Connections = new ObservableCollection<IConnectionViewModel>
			{
				new ConnectionViewModel
				{
					FirstPort = 2,
					SecondPort = 3,
					ActiveBrush = _purpleBrush
				},
				new ConnectionViewModel
				{
					FirstPort = 5,
					SecondPort = 1,
					ActiveBrush = _purpleBrush
				}
			}
		};
		_diagram.ValveDDataContext = new ValveDataContext
		{
			AngleMargin = new Thickness(-8.0, 74.0, 0.0, 0.0),
			Connections = new ObservableCollection<IConnectionViewModel>
			{
				new ConnectionViewModel
				{
					FirstPort = 4,
					SecondPort = 5,
					ActiveBrush = _blueBrush
				}
			}
		};
		_diagram.ValveCDataContext = new ValveDataContext
		{
			AngleMargin = new Thickness(-8.0, 74.0, 0.0, 0.0),
			Connections = new ObservableCollection<IConnectionViewModel>
			{
				new ConnectionViewModel
				{
					FirstPort = 4,
					SecondPort = 5,
					ActiveBrush = _redBrush
				}
			}
		};
		_diagram.PumpADataContext = new PumpViewModel
		{
			ActiveBrush = _blueBrush,
			IsActive = true
		};
		_diagram.PumpBDataContext = new PumpViewModel
		{
			ActiveBrush = _redBrush,
			IsActive = true
		};
		_diagram.SensorADataContext = new SensorViewModel
		{
			ActiveBrush = _blueBrush
		};
		_diagram.SensorBDataContext = new SensorViewModel
		{
			ActiveBrush = _redBrush
		};
		_diagram.TemperatureDataContext = new TemperatureViewModel
		{
			ActiveBrush = _redBrush
		};
		_diagram.PumpLogDataContext = new TableViewModel("Pump Volume Log")
		{
			ActiveBrush = _blueBrush
		};
		_diagram.OutputDataContext = new ActivatableAndErrorAwareDataContext
		{
			ActiveBrush = _purpleBrush
		};
		_diagram.InjectionDataContext = new InjectionViewModel();
		BalticHWProfile.CapillaryItem capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.TransferLine);
		BalticPreferences.CapillaryPreference capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.TransferLine);
		if (capillaryItem != null && capillaryPreference != null)
		{
			_diagram.ValveBToOutputLinkDataContext = new LinkViewModel(capillaryItem, capillaryPreference, _isEditable, isPopupVisible: true)
			{
				ActiveBrush = _purpleBrush
			};
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.Trap);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.Trap);
		if (capillaryItem != null && capillaryPreference != null)
		{
			_diagram.TrapDataContext = new TrapViewModel(capillaryItem, capillaryPreference, _isEditable)
			{
				ActiveBrush = _purpleBrush
			};
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.Loop);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.Loop);
		if (capillaryItem != null && capillaryPreference != null)
		{
			capillaryPreference.LengthTitle = "Volume:";
			capillaryPreference.InnerDiameterTitle = "Inner Diameter:";
			capillaryPreference.LengthUnit = "µL";
			capillaryPreference.IDUnit = "µm";
			_diagram.LoopDataContext = new LoopViewModel(capillaryItem, capillaryPreference, _isEditable)
			{
				ActiveBrush = _purpleBrush
			};
		}
		_diagram.PumpAToValveDTopLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		_diagram.PumpAToValveDBottomLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		_diagram.SolventAtoValveDLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		_diagram.WasteDtoValveDLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.ValveAToFS);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.ValveAToFS);
		if (capillaryItem != null && capillaryPreference != null)
		{
			_diagram.SensorAtoValveDLinkDataContext = new LinkViewModel(capillaryItem, capillaryPreference, _isEditable, isPopupVisible: true)
			{
				ActiveBrush = _blueBrush
			};
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.ValveAToInjectionValve);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.ValveAToInjectionValve);
		if (capillaryItem != null && capillaryPreference != null)
		{
			_diagram.ValveDToValveALinkDataContext = new LinkViewModel(capillaryItem, capillaryPreference, _isEditable, isPopupVisible: true)
			{
				ActiveBrush = _blueBrush
			};
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.InjectToTrap);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.InjectToTrap);
		if (capillaryItem != null && capillaryPreference != null)
		{
			_diagram.ValveAToValveBLinkDataContext = new LinkViewModel(capillaryItem, capillaryPreference, _isEditable, isPopupVisible: true)
			{
				ActiveBrush = _blueBrush
			};
		}
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.FSAToMixTee);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.FSAToMixTee);
		if (capillaryItem != null && capillaryPreference != null)
		{
			_diagram.SensorAToMidLinkDataContext = new LinkViewModel(capillaryItem, capillaryPreference, _isEditable, isPopupVisible: true)
			{
				ActiveBrush = _blueBrush
			};
		}
		_diagram.WasteAtoValveALinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.ValveBToFS);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.ValveBToFS);
		if (capillaryItem != null && capillaryPreference != null)
		{
			_diagram.SensorBtoValveCLinkDataContext = new LinkViewModel(capillaryItem, capillaryPreference, _isEditable, isPopupVisible: true)
			{
				ActiveBrush = _redBrush
			};
		}
		_diagram.PumpBToValveCBottomLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _redBrush
		};
		_diagram.PumpBtoValveCTopLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _redBrush
		};
		_diagram.SolventBtoValveCTopLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _redBrush
		};
		_diagram.WasteCtoValveCLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _redBrush
		};
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.FSBToMixTee);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.FSBToMixTee);
		if (capillaryItem != null && capillaryPreference != null)
		{
			_diagram.SensorBToMidLinkDataContext = new LinkViewModel(capillaryItem, capillaryPreference, _isEditable, isPopupVisible: true)
			{
				ActiveBrush = _redBrush
			};
		}
		_diagram.WasteBtoValveBLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _purpleBrush
		};
		capillaryItem = HwCapillaries.Find((BalticHWProfile.CapillaryItem x) => x.LinkName.ToLower() == naming.MixTeeToTrapValve);
		capillaryPreference = _prefCapillaries.Find((BalticPreferences.CapillaryPreference x) => x.LinkName.ToLower() == naming.MixTeeToTrapValve);
		if (capillaryItem != null && capillaryPreference != null)
		{
			_diagram.MidToValveBLinkDataContext = new LinkViewModel(capillaryItem, capillaryPreference, _isEditable, isPopupVisible: true)
			{
				ActiveBrush = _purpleBrush
			};
		}
		_diagram.SolventBDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _redBrush
		};
		_diagram.SolventADataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		_diagram.WasteBDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		_diagram.LineBreakOnPumpALink1DataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		_diagram.LineBreakOnPumpALink2DataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		_diagram.ValveAToPlugLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
		_diagram.BlingPlugToValveCLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _redBrush
		};
		_diagram.MixTeeToInjectionValveLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _purpleBrush
		};
		_diagram.InjectionValveToSeparatorLinkDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _purpleBrush
		};
		_diagram.ValveAPlugDataContextDataContext = new LinkBasicViewModel
		{
			ActiveBrush = _blueBrush
		};
	}

	private void ConfigureActiveStates()
	{
		dynamic balticConfiguration = _facade.BalticConfiguration;
		ActiveStateConfigurationHelper activeStateConfigurationHelper = new ActiveStateConfigurationHelper(balticConfiguration, _models);
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Waste, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve, balticConfiguration.TrapValve, new ActivatableSet[4]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveAWaste, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Waste, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve, balticConfiguration.TrapValve, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveAWaste, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Waste, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve, balticConfiguration.TrapValve, new ActivatableSet[4]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveAWaste, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve, balticConfiguration.TrapValve, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve, balticConfiguration.TrapValve, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve, balticConfiguration.TrapValve, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Compress, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve, balticConfiguration.TrapValve, new ActivatableSet[4]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Compress, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve, balticConfiguration.TrapValve, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Compress, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve, balticConfiguration.TrapValve, new ActivatableSet[4]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Waste, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Waste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Waste, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientT, new ActivatableSet[10]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Waste, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[2]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Waste, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Analytical, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Waste, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Trap, new ActivatableSet[2]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Waste, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientA, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Waste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientT, new ActivatableSet[10]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[2]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Analytical, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Trap, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientA, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Compress, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Waste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Compress, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientT, new ActivatableSet[10]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Compress, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[2]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Compress, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Analytical, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Compress, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Trap, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Compress, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientA, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Waste, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Waste, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Waste, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Solvent, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Waste, new ActivatableSet[2]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveA, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Waste, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[4]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientT, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientT, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientT, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientT, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientT, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Analytical, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Analytical, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Analytical, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Analytical, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Analytical, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Trap, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Trap, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Trap, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Trap, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.Trap, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientA, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientA, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientA, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientA, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Inject, balticConfiguration.TrapValve.GradientA, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Waste, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Waste, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Waste, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Waste, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Waste, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[5]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientT, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientT, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientT, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientT, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientT, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Analytical, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Analytical, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Analytical, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Analytical, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Analytical, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Trap, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Trap, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Trap, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Trap, new ActivatableSet[6]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.Trap, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientA, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientA, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientA, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientA, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve.Load, balticConfiguration.TrapValve.GradientA, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToPlug, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpARearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToInjectionValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Loop, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.InjectToTrap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Waste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Waste, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Waste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Waste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Waste, new ActivatableSet[10]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.TrapValveToWaste, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[2]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.InjectWaste, new ActivatableSet[4]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientT, new ActivatableSet[10]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientT, new ActivatableSet[11]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientT, new ActivatableSet[10]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientT, new ActivatableSet[10]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientT, new ActivatableSet[12]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.Trap, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Analytical, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Analytical, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Analytical, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Analytical, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Analytical, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Trap, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Trap, new ActivatableSet[8]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Trap, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Trap, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.Trap, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Waste, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientA, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.ValveBWaste, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Solvent, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientA, new ActivatableSet[10]
		{
			new ActivatableSet(activeStateConfigurationHelper.SolventToValveB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.SolventB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Compress, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientA, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.Inject, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientA, new ActivatableSet[9]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBRearToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.MixTee, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve, balticConfiguration.TrapValve.GradientA, new ActivatableSet[11]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _purpleBrush),
			new ActivatableSet(activeStateConfigurationHelper.PumpAFrontToValve, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveAToFS, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowA, _blueBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSAToMixTee, _blueBrush)
		});
		Add4ValveState(activeStateConfigurationHelper, balticConfiguration.PumpValve.Inject, balticConfiguration.PumpValve.MixTee, balticConfiguration.InjectionValve.Block, balticConfiguration.TrapValve.GradientA, new ActivatableSet[7]
		{
			new ActivatableSet(activeStateConfigurationHelper.PumpBFrontToValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.ValveBToFS, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FlowB, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.FSBToMixTee, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.MixTeeToTrapValve, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.TransferLine, _redBrush),
			new ActivatableSet(activeStateConfigurationHelper.Separator, _redBrush)
		});
	}

	private void valve_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
	{
		if (!(sender is IMeteringChannel meteringChannel) || !meteringChannel.ChannelInfo.Id.Name.Equals("angle", StringComparison.InvariantCultureIgnoreCase) || e.Data == null)
		{
			return;
		}
		object source = meteringChannel.ChannelInfo.Id.Source;
		double num = (double)e.Data[e.Data.Length - 1].Value;
		int num2 = ValvePosition(num);
		if (source.Equals("valve-a"))
		{
			_valveAPos = num2;
			_valveAAngle = (int)Math.Round(num);
		}
		else if (source.Equals("valve-b"))
		{
			_valveBPos = num2;
			_valveBAngle = (int)Math.Round(num);
		}
		else if (source.Equals("valve-i"))
		{
			_valveIPos = num2;
			_valveIAngle = (int)Math.Round(num);
		}
		else
		{
			if (!source.Equals("valve-t"))
			{
				return;
			}
			_valveTPos = num2;
			_valveTAngle = (int)Math.Round(num);
		}
		if (!_states.TryGetValue(Synth4ValveStateKey(_valveAPos, _valveBPos, _valveIPos, _valveTPos), out var activatables))
		{
			activatables = new List<ActivatableSet>(0);
		}
		_diagram.Dispatcher.Invoke(Action);
		void Action()
		{
			foreach (object model in _models.Values)
			{
				if (model is IActivatable activatable)
				{
					ActivatableSet activatableSet = activatables.FirstOrDefault((ActivatableSet x) => x.Activatable == model);
					if (activatableSet != null)
					{
						activatable.ActiveBrush = activatableSet.ActiveColor;
						activatable.IsActive = true;
					}
					else if (model != _models[_facade.BalticConfiguration.Naming.PumpA] && model != _models[_facade.BalticConfiguration.Naming.PumpB])
					{
						activatable.IsActive = false;
					}
				}
			}
			((IValveViewModel)_diagram.ValveDDataContext).Angle = _valveAPos * 30;
			((IValveViewModel)_diagram.ValveCDataContext).Angle = _valveBPos * 30;
			((IValveViewModel)_diagram.ValveADataContext).Angle = _valveIPos * 30;
			((IValveViewModel)_diagram.ValveBDataContext).Angle = _valveTPos * 30;
			((IValveViewModel)_diagram.ValveDDataContext).ActualAngle = _valveAAngle;
			((IValveViewModel)_diagram.ValveCDataContext).ActualAngle = _valveBAngle;
			((IValveViewModel)_diagram.ValveADataContext).ActualAngle = _valveIAngle;
			((IValveViewModel)_diagram.ValveBDataContext).ActualAngle = _valveTAngle;
		}
	}

	private void pump_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
	{
		if (e.Data == null)
		{
			return;
		}
		IMeteringChannel channel = sender as IMeteringChannel;
		double value;
		object model;
		if (channel != null)
		{
			object source = channel.ChannelInfo.Id.Source;
			value = (double)e.Data[e.Data.Length - 1].Value;
			if (_models.TryGetValue(source.ToString(), out model))
			{
				_diagram.Dispatcher.Invoke(Action);
			}
		}
		void Action()
		{
			if (model is IPumpViewModel pumpViewModel)
			{
				switch (channel.ChannelInfo.Id.Name)
				{
				case "pressure":
					pumpViewModel.Pressure = value;
					pumpViewModel.PressureUnit = channel.ChannelInfo.Unit;
					break;
				case "relative Volume":
					pumpViewModel.FillLevel = 100.0 - value;
					break;
				case "speed":
					if ((int)(Math.Abs(value) * 1000.0) < 1000)
					{
						pumpViewModel.Throughput = 1000.0 * value;
						pumpViewModel.ThroughputUnit = "nL/min";
					}
					else
					{
						pumpViewModel.Throughput = value;
						pumpViewModel.ThroughputUnit = "µL/min";
					}
					break;
				case "volume":
					pumpViewModel.VolumeLeft = _facade.TotalPistonVolume - value;
					pumpViewModel.VolumeUnit = "µL";
					lock (_loggingLock)
					{
						if (_isDiagramLoggingEnabled)
						{
							DateTime now = DateTime.Now;
							if (_tableRowBuffer.nItems == 0 && !_currentTableItem.IsValidA && !_currentTableItem.IsValidB)
							{
								_startLoggingDateTime = now;
							}
							if (now.Subtract(_lastLoggingDateTime).TotalMilliseconds >= (double)_sysInfoLoggingIntervalMs)
							{
								if (_tableRowBuffer.nItems == 0 && !_currentTableItem.IsValidA && !_currentTableItem.IsValidB)
								{
									try
									{
										using StreamWriter streamWriter = File.CreateText(Path.Combine(_facade.LogFactory.CurrentLogDirectory.FullName, "pump volume log.txt"));
										streamWriter.Write("Local Date/Time\tRelative Time\tVolume A [μL]\tVolume B [μL]\tΔ Volume A [nL]\tΔ Volume B [nL]\n");
										streamWriter.Close();
									}
									catch (IOException)
									{
									}
								}
								if (channel.ChannelInfo.Id.Source.ToString().Contains("pump-a"))
								{
									if (!_currentTableItem.IsValidA)
									{
										_currentTableItem.VolumeA = pumpViewModel.VolumeLeft;
										_currentTableItem.IsValidA = true;
									}
								}
								else if (!_currentTableItem.IsValidB)
								{
									_currentTableItem.VolumeB = pumpViewModel.VolumeLeft;
									_currentTableItem.IsValidB = true;
								}
								if (_currentTableItem.IsValid)
								{
									_tableRowBuffer.Add(new TableDataSync(_currentTableItem));
									if (_tableRowBuffer.IsValid)
									{
										ITableViewModel tableViewModel = _diagram.PumpLogDataContext as ITableViewModel;
										var (num, num2) = _tableRowBuffer.Difference();
										if (tableViewModel.TableItems.Count < _systemInfoMaxLoggingEntries)
										{
											tableViewModel.TableItems.Add(new TableItem($"{now:hh:mm:ss}", _currentTableItem.VolumeA, _currentTableItem.VolumeB, num * 1000.0, num2 * 1000.0));
										}
										else
										{
											tableViewModel.TableItems.RemoveAt(0);
											tableViewModel.TableItems.Add(new TableItem($"{now:hh:mm:ss}", _currentTableItem.VolumeA, _currentTableItem.VolumeB, num * 1000.0, num2 * 1000.0));
										}
										try
										{
											TableItem tableItem = tableViewModel.TableItems.Last();
											using StreamWriter streamWriter2 = File.AppendText(Path.Combine(_facade.LogFactory.CurrentLogDirectory.FullName, "pump volume log.txt"));
											streamWriter2.Write(string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2:F3}\t{3:F3}\t{4:F0}\t{5:F0}\n", now.ToLocalTime(), RelativeFormatter(now.Subtract(_startLoggingDateTime).Ticks), tableItem.VolumeA, tableItem.VolumeB, tableItem.DeltaVolumeA, tableItem.DeltaVolumeB));
											streamWriter2.Close();
										}
										catch (IOException)
										{
										}
									}
									_lastLoggingDateTime = now;
									_currentTableItem.Reset();
								}
							}
						}
						break;
					}
				}
			}
		}
	}

	private static string RelativeFormatter(long timestamp)
	{
		TimeSpan timeSpan = new TimeSpan(timestamp);
		int num = (int)timeSpan.TotalHours;
		return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:mm\\:ss\\.fff}", num, timeSpan);
	}

	private void sensor_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
	{
		if (e.Data == null)
		{
			return;
		}
		IMeteringChannel channel = sender as IMeteringChannel;
		double value;
		object model;
		if (channel != null)
		{
			object source = channel.ChannelInfo.Id.Source;
			value = (double)e.Data[e.Data.Length - 1].Value;
			if (_models.TryGetValue(source.ToString(), out model))
			{
				_diagram.Dispatcher.Invoke(Action);
			}
		}
		void Action()
		{
			if (channel.ChannelInfo.Id.Name.Equals("flow") && model is ISensorViewModel sensorViewModel)
			{
				if ((int)(Math.Abs(value) * 1000.0) < 1000)
				{
					sensorViewModel.Throughput = 1000.0 * value;
					sensorViewModel.ThroughputUnit = "nL/min";
				}
				else
				{
					sensorViewModel.Throughput = value;
					sensorViewModel.ThroughputUnit = "µL/min";
				}
			}
		}
	}

	private void oven_ChannelDataChanged(object sender, MeteringChannelDataEventArgs e)
	{
		if (e.Data == null)
		{
			return;
		}
		IMeteringChannel channel = sender as IMeteringChannel;
		double value;
		object model;
		if (channel != null)
		{
			object source = channel.ChannelInfo.Id.Source;
			value = (double)e.Data[e.Data.Length - 1].Value;
			if (_models.TryGetValue(source.ToString(), out model))
			{
				_diagram.Dispatcher.Invoke(Action);
			}
		}
		void Action()
		{
			if (model is ITemperatureViewModel temperatureViewModel && channel.ChannelInfo.Id.Name.Equals("temperature"))
			{
				if ((int)(value * 10.0) != 0)
				{
					temperatureViewModel.Temperature = Math.Round(value - 0.06);
				}
				temperatureViewModel.TemperatureUnit = "°C";
			}
		}
	}

	private void Add4ValveState(ActiveStateConfigurationHelper helper, object valveA, object valveB, object valveI, object valveT, ActivatableSet[] activatables)
	{
		double[] array = EstablishAngles(valveA);
		double[] array2 = EstablishAngles(valveB);
		double[] array3 = EstablishAngles(valveI);
		double[] array4 = EstablishAngles(valveT);
		double[] array5 = array;
		foreach (double angle in array5)
		{
			double[] array6 = array2;
			foreach (double angle2 in array6)
			{
				double[] array7 = array3;
				foreach (double angle3 in array7)
				{
					double[] array8 = array4;
					foreach (double angle4 in array8)
					{
						int key = Synth4ValveStateKey(ValvePosition(angle), ValvePosition(angle2), ValvePosition(angle3), ValvePosition(angle4));
						HashSet<ActivatableSet> hashSet = new HashSet<ActivatableSet>();
						foreach (ActivatableSet item in activatables)
						{
							hashSet.Add(item);
						}
						AddValveConnections(hashSet, (IValveViewModel)helper.ValveA, angle, helper.ValveALinks);
						AddValveConnections(hashSet, (IValveViewModel)helper.ValveB, angle2, helper.ValveBLinks);
						AddValveConnections(hashSet, (IValveViewModel)helper.ValveI, angle3, helper.ValveILinks);
						AddValveConnections(hashSet, (IValveViewModel)helper.ValveT, angle4, helper.ValveTLinks);
						hashSet.Add(new ActivatableSet(helper.PumpA, _blueBrush));
						hashSet.Add(new ActivatableSet(helper.PumpB, _redBrush));
						_states.Add(key, hashSet);
					}
				}
			}
		}
	}

	private static void AddValveConnections(ICollection<ActivatableSet> activatables, IValveViewModel valve, double angle, Dictionary<int, IActivatable> links)
	{
		foreach (IConnectionViewModel connection in valve.Connections)
		{
			int key = ValvePosition((double)((1 - connection.FirstPort) * 60) + angle);
			if (links.TryGetValue(key, out var link))
			{
				ActivatableSet activatableSet = activatables.FirstOrDefault((ActivatableSet x) => x.Activatable == link);
				if (activatableSet != null)
				{
					activatables.Add(new ActivatableSet(connection as IActivatable, activatableSet.ActiveColor));
				}
			}
		}
	}

	private static double[] EstablishAngles(object o)
	{
		if (o != null)
		{
			if (o is IDictionary<string, object> dictionary)
			{
				return dictionary.Values.Cast<double>().Distinct().ToArray();
			}
			return new double[1] { (double)o };
		}
		return Array.Empty<double>();
	}

	private static int Synth4ValveStateKey(int valveAPos, int valveBPos, int valveIPos, int valveTPos)
	{
		return valveAPos + valveBPos * 12 + valveIPos * 12 * 12 + valveTPos * 12 * 12 * 12;
	}

	private static int ValvePosition(double angle)
	{
		if (_isCTCSimulate && Math.Floor(angle) != angle)
		{
			angle = 0.0;
		}
		if (angle < 0.0)
		{
			angle += 360.0;
		}
		else if (angle >= 360.0)
		{
			angle -= 360.0;
		}
		else if ((angle < 0.0 || angle >= 360.0) ? true : false)
		{
			throw new ArgumentOutOfRangeException("angle", angle, "Specified angle must be within [-360,360]");
		}
		return (int)Math.Round(angle / 30.0);
	}
}
