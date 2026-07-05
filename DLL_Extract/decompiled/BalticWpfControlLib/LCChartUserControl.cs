using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BalticClassLib;
using Bruker.Lc.Baltic;
using Bruker.Lc.Metering;
using Syncfusion.UI.Xaml.Charts;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib;

public class LCChartUserControl : UserControl, INotifyPropertyChanged, IComponentConnector
{
	public class DataPt
	{
		public double Time { get; set; }

		public double Value { get; set; }

		public DataPt(double time, double value)
		{
			Time = time;
			Value = value;
			base._002Ector();
		}
	}

	public class TraceSourceItem : NotificationObject
	{
		[CompilerGenerated]
		private SfChart _003Cchart_003EP;

		[CompilerGenerated]
		private IMeteringChannel _003Cchannel_003EP;

		[CompilerGenerated]
		private double _003CmaxTimeBufferMin_003EP;

		private FastLineBitmapSeries _series;

		private bool _isHidden;

		private bool _isChecked;

		private TimeSpan _lastTime;

		private readonly TimeSpan _chartInterval;

		public string TraceName { get; }

		public bool IsDiagnostic => _003Cchannel_003EP.ChannelInfo.IsDiagnostic;

		public bool IsService => _003Cchannel_003EP.ChannelInfo.IsSevice;

		public bool IsHidden
		{
			get
			{
				return _isHidden;
			}
			set
			{
				_isHidden = value;
				RaisePropertyChanged("IsHidden");
			}
		}

		public Brush TraceColor { get; }

		public bool IsChecked
		{
			get
			{
				return _isChecked;
			}
			set
			{
				_003Cchart_003EP.Dispatcher.VerifyAccess();
				if (_isChecked != value)
				{
					_isChecked = value;
					if (value)
					{
						ObservableCollection<DataPt> itemsSource = new ObservableCollection<DataPt>();
						_series = new FastLineBitmapSeries
						{
							ItemsSource = itemsSource,
							EnableAntiAliasing = true,
							StrokeThickness = 1.0,
							Interior = TraceColor,
							Label = _003Cchannel_003EP.ChannelInfo.Id.ToString(),
							LegendIcon = ChartLegendIcon.SeriesType,
							XBindingPath = "Time",
							YBindingPath = "Value"
						};
						_003Cchart_003EP.Series.Add(_series);
						_003Cchannel_003EP.ChannelDataChanged += TraceDataChanged;
					}
					else
					{
						_003Cchannel_003EP.ChannelDataChanged -= TraceDataChanged;
						_003Cchart_003EP.Series.Remove(_series);
						_series = null;
						_lastTime = TimeSpan.Zero;
					}
				}
			}
		}

		public TraceSourceItem(SfChart chart, IMeteringChannel channel, Color color, double maxTimeBufferMin)
		{
			_003Cchart_003EP = chart;
			_003Cchannel_003EP = channel;
			_003CmaxTimeBufferMin_003EP = maxTimeBufferMin;
			_isHidden = true;
			_lastTime = TimeSpan.Zero;
			_chartInterval = TimeSpan.FromMilliseconds(333.0);
			TraceName = _003Cchannel_003EP.ChannelInfo.CreateTraceName() + " [" + _003Cchannel_003EP.ChannelInfo.Unit + "]";
			TraceColor = new SolidColorBrush(color);
			base._002Ector();
		}

		public override string ToString()
		{
			return $"{_003Cchannel_003EP.ChannelInfo.Id.Source}:{_003Cchannel_003EP.ChannelInfo.Id.Name}";
		}

		private void TraceDataChanged(object sender, MeteringChannelDataEventArgs args)
		{
			FastLineBitmapSeries series = _series;
			if (args == null || series == null || !_isChecked)
			{
				return;
			}
			MeteringDataPoint[] data2 = args.Data;
			if ((data2 != null && data2.Length == 0) || _isHidden)
			{
				return;
			}
			List<DataPt> data = null;
			bool clearChart = false;
			if (args.Data != null)
			{
				data = new List<DataPt>(args.Data.Length);
				MeteringDataPoint[] data3 = args.Data;
				foreach (MeteringDataPoint meteringDataPoint in data3)
				{
					double num;
					try
					{
						num = Convert.ToDouble(meteringDataPoint.Value, CultureInfo.InvariantCulture);
					}
					catch (Exception)
					{
						continue;
					}
					if (!double.IsNaN(num))
					{
						TimeSpan timeSpan = new TimeSpan(meteringDataPoint.Timestamp);
						data.Add(new DataPt(timeSpan.TotalMinutes, Math.Round(num, _003Cchannel_003EP.ChannelInfo.DisplayDecimals)));
					}
				}
				if (data.Count == 0)
				{
					return;
				}
			}
			else
			{
				clearChart = true;
			}
			_003Cchart_003EP.Dispatcher.BeginInvoke((Action)delegate
			{
				try
				{
					if (_series != null)
					{
						series = _series;
						ObservableCollection<DataPt> observableCollection = (ObservableCollection<DataPt>)series.ItemsSource;
						if (observableCollection != null)
						{
							if (clearChart)
							{
								observableCollection.Clear();
								_lastTime = TimeSpan.Zero;
							}
							else if (data != null && IsChecked)
							{
								while (observableCollection.Count > 0 && Math.Abs(data[0].Time - observableCollection[0].Time) > _003CmaxTimeBufferMin_003EP)
								{
									observableCollection.RemoveAt(0);
								}
								{
									foreach (DataPt item in data)
									{
										if (!(item.Time < _lastTime.TotalMinutes + _chartInterval.TotalMinutes))
										{
											observableCollection.Add(item);
											_lastTime = TimeSpan.FromMinutes(item.Time);
										}
									}
									return;
								}
							}
						}
					}
				}
				catch (Exception)
				{
				}
			});
		}
	}

	private bool _isDiagnosticTracesSelected;

	private readonly List<TraceSourceItem> _allTraces = new List<TraceSourceItem>();

	private readonly List<TraceSourceItem> _traceSelectionSourceDiagnostic = new List<TraceSourceItem>();

	private readonly List<TraceSourceItem> _traceSelectionSourceService = new List<TraceSourceItem>();

	internal SfChart sfChartRealTime;

	internal ChartZoomPanBehavior sfChartBehavior;

	private bool _contentLoaded;

	public bool IsDiagnosticTracesSelected
	{
		get
		{
			return _isDiagnosticTracesSelected;
		}
		set
		{
			_isDiagnosticTracesSelected = value;
			if (value)
			{
				foreach (TraceSourceItem item in _traceSelectionSourceDiagnostic)
				{
					TraceSelectionSource.Add(item);
				}
			}
			else
			{
				foreach (TraceSourceItem item2 in _traceSelectionSourceDiagnostic)
				{
					item2.IsChecked = false;
					TraceSelectionSource.Remove(item2);
				}
			}
			UpdateServiceTraces(BalticInstrumentFacade.IsService);
			NotifyPropertyChanged("IsDiagnosticTracesSelected");
		}
	}

	public ObservableCollection<TraceSourceItem> TraceSelectionSource { get; } = new ObservableCollection<TraceSourceItem>();


	public IEnumerable<string> EnabledTracesList
	{
		get
		{
			return (from item in _allTraces
				where item.IsChecked
				select item.ToString()).ToList();
		}
		set
		{
			foreach (TraceSourceItem allTrace in _allTraces)
			{
				if (allTrace.IsDiagnostic)
				{
					allTrace.IsChecked = value.Contains(allTrace.ToString()) && IsDiagnosticTracesSelected;
				}
				else
				{
					allTrace.IsChecked = value.Contains(allTrace.ToString());
				}
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	private void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public void UpdateServiceTraces(bool isService)
	{
		if (isService)
		{
			foreach (TraceSourceItem item in _traceSelectionSourceService)
			{
				if (!TraceSelectionSource.Contains(item) && (!item.IsDiagnostic || IsDiagnosticTracesSelected))
				{
					TraceSelectionSource.Add(item);
				}
			}
			return;
		}
		foreach (TraceSourceItem item2 in _traceSelectionSourceService)
		{
			item2.IsChecked = false;
			TraceSelectionSource.Remove(item2);
		}
	}

	public LCChartUserControl(IEnumerable<IMeteringChannel> channelCollection, double maxTimeBufferMin)
	{
		InitializeComponent();
		base.DataContext = this;
		SetupChart(channelCollection, maxTimeBufferMin);
	}

	public void SetHidden(bool isHidden)
	{
		base.Dispatcher.Invoke(delegate
		{
			foreach (TraceSourceItem allTrace in _allTraces)
			{
				allTrace.IsHidden = isHidden;
			}
		});
	}

	private void SetupChart(IEnumerable<IMeteringChannel> channels, double maxTimeBufferMin)
	{
		byte b = 0;
		byte b2 = 0;
		byte b3 = 0;
		byte b4 = byte.MaxValue;
		byte b5 = byte.MaxValue;
		byte b6 = byte.MaxValue;
		foreach (IMeteringChannel channel in channels)
		{
			if (typeof(double) != channel.ChannelInfo.ValueType)
			{
				continue;
			}
			Color color;
			if (channel.ChannelInfo.Id.Source.ToString().EndsWith("-a", StringComparison.InvariantCultureIgnoreCase))
			{
				color = Color.FromRgb(b, 0, b4);
				b += 37;
				b4 -= 20;
			}
			else if (channel.ChannelInfo.Id.Source.ToString().EndsWith("-b", StringComparison.InvariantCultureIgnoreCase))
			{
				color = Color.FromRgb(b5, b2, 0);
				b2 += 31;
				b5 -= 25;
			}
			else
			{
				color = Color.FromRgb(0, b6, b3);
				b3 += 100;
				b6 -= 20;
			}
			TraceSourceItem traceSourceItem = new TraceSourceItem(sfChartRealTime, channel, color, maxTimeBufferMin);
			traceSourceItem.IsChecked = (traceSourceItem.IsDiagnostic && IsDiagnosticTracesSelected) || !traceSourceItem.IsDiagnostic;
			if (traceSourceItem.IsService)
			{
				_traceSelectionSourceService.Add(traceSourceItem);
				if (BalticInstrumentFacade.IsService && IsDiagnosticTracesSelected)
				{
					TraceSelectionSource.Add(traceSourceItem);
				}
			}
			else if (traceSourceItem.IsDiagnostic)
			{
				_traceSelectionSourceDiagnostic.Add(traceSourceItem);
				if (IsDiagnosticTracesSelected)
				{
					TraceSelectionSource.Add(traceSourceItem);
				}
			}
			else
			{
				TraceSelectionSource.Add(traceSourceItem);
			}
			_allTraces.Add(traceSourceItem);
		}
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		foreach (TraceSourceItem allTrace in _allTraces)
		{
			allTrace.IsChecked = false;
		}
	}

	private void NumericalAxis_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		sfChartBehavior.ZoomMode = Syncfusion.UI.Xaml.Charts.ZoomMode.X;
	}

	private void SecondaryAxis_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		sfChartBehavior.ZoomMode = Syncfusion.UI.Xaml.Charts.ZoomMode.Y;
	}

	private void sfChartRealTime_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		sfChartBehavior.ZoomMode = Syncfusion.UI.Xaml.Charts.ZoomMode.XY;
	}

	private void NumericalAxis_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		sfChartRealTime.PrimaryAxis.ZoomFactor = 1.0;
	}

	private void SecondaryAxis_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		sfChartRealTime.SecondaryAxis.ZoomFactor = 1.0;
	}

	private void NumericalAxis_MouseEnter(object sender, MouseEventArgs e)
	{
		base.Cursor = Cursors.SizeWE;
	}

	private void NumericalAxis_MouseLeave(object sender, MouseEventArgs e)
	{
		base.Cursor = Cursors.Arrow;
	}

	private void SecondaryAxis_MouseEnter(object sender, MouseEventArgs e)
	{
		base.Cursor = Cursors.SizeNS;
	}

	private void SecondaryAxis_MouseLeave(object sender, MouseEventArgs e)
	{
		base.Cursor = Cursors.Arrow;
	}

	private void NumericalAxis_LabelCreated(object sender, LabelCreatedEventArgs e)
	{
		if (double.TryParse(e.AxisLabel.LabelContent.ToString(), out var result))
		{
			e.AxisLabel.LabelContent = result.ToString(CultureInfo.InvariantCulture);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/lcchartusercontrol.xaml", UriKind.Relative);
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
			((LCChartUserControl)target).Unloaded += OnUnloaded;
			break;
		case 2:
			sfChartRealTime = (SfChart)target;
			sfChartRealTime.MouseWheel += sfChartRealTime_MouseWheel;
			break;
		case 3:
			sfChartBehavior = (ChartZoomPanBehavior)target;
			break;
		case 4:
			((NumericalAxis)target).MouseWheel += NumericalAxis_MouseWheel;
			((NumericalAxis)target).MouseDoubleClick += NumericalAxis_MouseDoubleClick;
			((NumericalAxis)target).MouseEnter += NumericalAxis_MouseEnter;
			((NumericalAxis)target).MouseLeave += NumericalAxis_MouseLeave;
			((NumericalAxis)target).LabelCreated += NumericalAxis_LabelCreated;
			break;
		case 5:
			((NumericalAxis)target).MouseWheel += SecondaryAxis_MouseWheel;
			((NumericalAxis)target).MouseDoubleClick += SecondaryAxis_MouseDoubleClick;
			((NumericalAxis)target).MouseEnter += SecondaryAxis_MouseEnter;
			((NumericalAxis)target).MouseLeave += SecondaryAxis_MouseLeave;
			((NumericalAxis)target).LabelCreated += NumericalAxis_LabelCreated;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
