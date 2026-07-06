// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
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

namespace BalticWpfControlLib
{
	// Token: 0x02000022 RID: 34
	public partial class LCChartUserControl : UserControl, INotifyPropertyChanged
	{
		// Token: 0x1400001A RID: 26
		// (add) Token: 0x06000168 RID: 360 RVA: 0x0000A0C4 File Offset: 0x000082C4
		// (remove) Token: 0x06000169 RID: 361 RVA: 0x0000A0FC File Offset: 0x000082FC
		public event PropertyChangedEventHandler PropertyChanged;

		// Token: 0x0600016A RID: 362 RVA: 0x0000A131 File Offset: 0x00008331
		private void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x0600016B RID: 363 RVA: 0x0000A14A File Offset: 0x0000834A
		// (set) Token: 0x0600016C RID: 364 RVA: 0x0000A154 File Offset: 0x00008354
		public bool IsDiagnosticTracesSelected
		{
			get
			{
				return this._isDiagnosticTracesSelected;
			}
			set
			{
				this._isDiagnosticTracesSelected = value;
				if (value)
				{
					using (List<LCChartUserControl.TraceSourceItem>.Enumerator enumerator = this._traceSelectionSourceDiagnostic.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							LCChartUserControl.TraceSourceItem trace = enumerator.Current;
							this.TraceSelectionSource.Add(trace);
						}
						goto IL_0088;
					}
				}
				foreach (LCChartUserControl.TraceSourceItem trace2 in this._traceSelectionSourceDiagnostic)
				{
					trace2.IsChecked = false;
					this.TraceSelectionSource.Remove(trace2);
				}
				IL_0088:
				this.UpdateServiceTraces(BalticInstrumentFacade.IsService);
				this.NotifyPropertyChanged("IsDiagnosticTracesSelected");
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600016D RID: 365 RVA: 0x0000A21C File Offset: 0x0000841C
		public ObservableCollection<LCChartUserControl.TraceSourceItem> TraceSelectionSource { get; } = new ObservableCollection<LCChartUserControl.TraceSourceItem>();

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x0600016E RID: 366 RVA: 0x0000A224 File Offset: 0x00008424
		// (set) Token: 0x0600016F RID: 367 RVA: 0x0000A284 File Offset: 0x00008484
		public IEnumerable<string> EnabledTracesList
		{
			get
			{
				return (from item in this._allTraces
					where item.IsChecked
					select item.ToString()).ToList<string>();
			}
			set
			{
				foreach (LCChartUserControl.TraceSourceItem item in this._allTraces)
				{
					if (item.IsDiagnostic)
					{
						item.IsChecked = value.Contains(item.ToString()) && this.IsDiagnosticTracesSelected;
					}
					else
					{
						item.IsChecked = value.Contains(item.ToString());
					}
				}
			}
		}

		// Token: 0x06000170 RID: 368 RVA: 0x0000A30C File Offset: 0x0000850C
		public void UpdateServiceTraces(bool isService)
		{
			if (isService)
			{
				using (List<LCChartUserControl.TraceSourceItem>.Enumerator enumerator = this._traceSelectionSourceService.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						LCChartUserControl.TraceSourceItem trace = enumerator.Current;
						if (!this.TraceSelectionSource.Contains(trace) && (!trace.IsDiagnostic || this.IsDiagnosticTracesSelected))
						{
							this.TraceSelectionSource.Add(trace);
						}
					}
					return;
				}
			}
			foreach (LCChartUserControl.TraceSourceItem trace2 in this._traceSelectionSourceService)
			{
				trace2.IsChecked = false;
				this.TraceSelectionSource.Remove(trace2);
			}
		}

		// Token: 0x06000171 RID: 369 RVA: 0x0000A3D4 File Offset: 0x000085D4
		public LCChartUserControl(IEnumerable<IMeteringChannel> channelCollection, double maxTimeBufferMin)
		{
			this.InitializeComponent();
			base.DataContext = this;
			this.SetupChart(channelCollection, maxTimeBufferMin);
		}

		// Token: 0x06000172 RID: 370 RVA: 0x0000A428 File Offset: 0x00008628
		public void SetHidden(bool isHidden)
		{
			base.Dispatcher.Invoke(delegate
			{
				foreach (LCChartUserControl.TraceSourceItem traceSourceItem in this._allTraces)
				{
					traceSourceItem.IsHidden = isHidden;
				}
			});
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000A460 File Offset: 0x00008660
		private void SetupChart(IEnumerable<IMeteringChannel> channels, double maxTimeBufferMin)
		{
			byte sumRed = 0;
			byte sumGreen = 0;
			byte sumBlue = 0;
			byte blue = byte.MaxValue;
			byte red = byte.MaxValue;
			byte green = byte.MaxValue;
			foreach (IMeteringChannel channel in channels)
			{
				if (!(typeof(double) != channel.ChannelInfo.ValueType))
				{
					Color color;
					if (channel.ChannelInfo.Id.Source.ToString().EndsWith("-a", StringComparison.InvariantCultureIgnoreCase))
					{
						color = Color.FromRgb(sumRed, 0, blue);
						sumRed += 37;
						blue -= 20;
					}
					else if (channel.ChannelInfo.Id.Source.ToString().EndsWith("-b", StringComparison.InvariantCultureIgnoreCase))
					{
						color = Color.FromRgb(red, sumGreen, 0);
						sumGreen += 31;
						red -= 25;
					}
					else
					{
						color = Color.FromRgb(0, green, sumBlue);
						sumBlue += 100;
						green -= 20;
					}
					LCChartUserControl.TraceSourceItem traceItem = new LCChartUserControl.TraceSourceItem(this.sfChartRealTime, channel, color, maxTimeBufferMin);
					traceItem.IsChecked = (traceItem.IsDiagnostic && this.IsDiagnosticTracesSelected) || !traceItem.IsDiagnostic;
					if (traceItem.IsService)
					{
						this._traceSelectionSourceService.Add(traceItem);
						if (BalticInstrumentFacade.IsService && this.IsDiagnosticTracesSelected)
						{
							this.TraceSelectionSource.Add(traceItem);
						}
					}
					else if (traceItem.IsDiagnostic)
					{
						this._traceSelectionSourceDiagnostic.Add(traceItem);
						if (this.IsDiagnosticTracesSelected)
						{
							this.TraceSelectionSource.Add(traceItem);
						}
					}
					else
					{
						this.TraceSelectionSource.Add(traceItem);
					}
					this._allTraces.Add(traceItem);
				}
			}
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0000A638 File Offset: 0x00008838
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			foreach (LCChartUserControl.TraceSourceItem traceSourceItem in this._allTraces)
			{
				traceSourceItem.IsChecked = false;
			}
		}

		// Token: 0x06000175 RID: 373 RVA: 0x0000A68C File Offset: 0x0000888C
		private void NumericalAxis_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			this.sfChartBehavior.ZoomMode = Syncfusion.UI.Xaml.Charts.ZoomMode.X;
		}

		// Token: 0x06000176 RID: 374 RVA: 0x0000A69A File Offset: 0x0000889A
		private void SecondaryAxis_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			this.sfChartBehavior.ZoomMode = Syncfusion.UI.Xaml.Charts.ZoomMode.Y;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x0000A6A8 File Offset: 0x000088A8
		private void sfChartRealTime_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			this.sfChartBehavior.ZoomMode = Syncfusion.UI.Xaml.Charts.ZoomMode.XY;
		}

		// Token: 0x06000178 RID: 376 RVA: 0x0000A6B6 File Offset: 0x000088B6
		private void NumericalAxis_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			this.sfChartRealTime.PrimaryAxis.ZoomFactor = 1.0;
		}

		// Token: 0x06000179 RID: 377 RVA: 0x0000A6D1 File Offset: 0x000088D1
		private void SecondaryAxis_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			this.sfChartRealTime.SecondaryAxis.ZoomFactor = 1.0;
		}

		// Token: 0x0600017A RID: 378 RVA: 0x0000A6EC File Offset: 0x000088EC
		private void NumericalAxis_MouseEnter(object sender, MouseEventArgs e)
		{
			base.Cursor = Cursors.SizeWE;
		}

		// Token: 0x0600017B RID: 379 RVA: 0x0000A6F9 File Offset: 0x000088F9
		private void NumericalAxis_MouseLeave(object sender, MouseEventArgs e)
		{
			base.Cursor = Cursors.Arrow;
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0000A706 File Offset: 0x00008906
		private void SecondaryAxis_MouseEnter(object sender, MouseEventArgs e)
		{
			base.Cursor = Cursors.SizeNS;
		}

		// Token: 0x0600017D RID: 381 RVA: 0x0000A6F9 File Offset: 0x000088F9
		private void SecondaryAxis_MouseLeave(object sender, MouseEventArgs e)
		{
			base.Cursor = Cursors.Arrow;
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000A714 File Offset: 0x00008914
		private void NumericalAxis_LabelCreated(object sender, LabelCreatedEventArgs e)
		{
			double value;
			if (double.TryParse(e.AxisLabel.LabelContent.ToString(), out value))
			{
				e.AxisLabel.LabelContent = value.ToString(CultureInfo.InvariantCulture);
			}
		}

		// Token: 0x040000D4 RID: 212
		private bool _isDiagnosticTracesSelected;

		// Token: 0x040000D5 RID: 213
		private readonly List<LCChartUserControl.TraceSourceItem> _allTraces = new List<LCChartUserControl.TraceSourceItem>();

		// Token: 0x040000D6 RID: 214
		private readonly List<LCChartUserControl.TraceSourceItem> _traceSelectionSourceDiagnostic = new List<LCChartUserControl.TraceSourceItem>();

		// Token: 0x040000D7 RID: 215
		private readonly List<LCChartUserControl.TraceSourceItem> _traceSelectionSourceService = new List<LCChartUserControl.TraceSourceItem>();

		// Token: 0x020000BA RID: 186
		public class DataPt
		{
			// Token: 0x060006E1 RID: 1761 RVA: 0x0003BEDB File Offset: 0x0003A0DB
			public DataPt(double time, double value)
			{
				this.Time = time;
				this.Value = value;
			}

			// Token: 0x1700016B RID: 363
			// (get) Token: 0x060006E2 RID: 1762 RVA: 0x0003BEF1 File Offset: 0x0003A0F1
			// (set) Token: 0x060006E3 RID: 1763 RVA: 0x0003BEF9 File Offset: 0x0003A0F9
			public double Time { get; set; }

			// Token: 0x1700016C RID: 364
			// (get) Token: 0x060006E4 RID: 1764 RVA: 0x0003BF02 File Offset: 0x0003A102
			// (set) Token: 0x060006E5 RID: 1765 RVA: 0x0003BF0A File Offset: 0x0003A10A
			public double Value { get; set; }
		}

		// Token: 0x020000BB RID: 187
		public class TraceSourceItem : NotificationObject
		{
			// Token: 0x060006E6 RID: 1766 RVA: 0x0003BF14 File Offset: 0x0003A114
			public TraceSourceItem(SfChart chart, IMeteringChannel channel, Color color, double maxTimeBufferMin)
			{
				this._fp_chart = chart;
				this._fp_channel = channel;
				this._fp_maxTimeBufferMin = maxTimeBufferMin;
				this.TraceName = this._fp_channel.ChannelInfo.CreateTraceName() + " [" + this._fp_channel.ChannelInfo.Unit + "]";
				this.TraceColor = new SolidColorBrush(color);
				// base constructor call (decompilation artifact removed)
			}

			// Token: 0x1700016D RID: 365
			// (get) Token: 0x060006E7 RID: 1767 RVA: 0x0003BFA4 File Offset: 0x0003A1A4
			public string TraceName { get; }

			// Token: 0x1700016E RID: 366
			// (get) Token: 0x060006E8 RID: 1768 RVA: 0x0003BFAC File Offset: 0x0003A1AC
			public bool IsDiagnostic
			{
				get
				{
					return this._fp_channel.ChannelInfo.IsDiagnostic;
				}
			}

			// Token: 0x1700016F RID: 367
			// (get) Token: 0x060006E9 RID: 1769 RVA: 0x0003BFBE File Offset: 0x0003A1BE
			public bool IsService
			{
				get
				{
					return this._fp_channel.ChannelInfo.IsSevice;
				}
			}

			// Token: 0x17000170 RID: 368
			// (get) Token: 0x060006EA RID: 1770 RVA: 0x0003BFD0 File Offset: 0x0003A1D0
			// (set) Token: 0x060006EB RID: 1771 RVA: 0x0003BFD8 File Offset: 0x0003A1D8
			public bool IsHidden
			{
				get
				{
					return this._isHidden;
				}
				set
				{
					this._isHidden = value;
					this.RaisePropertyChanged("IsHidden");
				}
			}

			// Token: 0x17000171 RID: 369
			// (get) Token: 0x060006EC RID: 1772 RVA: 0x0003BFEC File Offset: 0x0003A1EC
			public Brush TraceColor { get; }

			// Token: 0x17000172 RID: 370
			// (get) Token: 0x060006ED RID: 1773 RVA: 0x0003BFF4 File Offset: 0x0003A1F4
			// (set) Token: 0x060006EE RID: 1774 RVA: 0x0003BFFC File Offset: 0x0003A1FC
			public bool IsChecked
			{
				get
				{
					return this._isChecked;
				}
				set
				{
					this._fp_chart.Dispatcher.VerifyAccess();
					if (this._isChecked != value)
					{
						this._isChecked = value;
						if (value)
						{
							ObservableCollection<LCChartUserControl.DataPt> ds = new ObservableCollection<LCChartUserControl.DataPt>();
							this._series = new FastLineBitmapSeries
							{
								ItemsSource = ds,
								EnableAntiAliasing = true,
								StrokeThickness = 1.0,
								Interior = this.TraceColor,
								Label = this._fp_channel.ChannelInfo.Id.ToString(),
								LegendIcon = ChartLegendIcon.SeriesType,
								XBindingPath = "Time",
								YBindingPath = "Value"
							};
							this._fp_chart.Series.Add(this._series);
							this._fp_channel.ChannelDataChanged += this.TraceDataChanged;
							return;
						}
						this._fp_channel.ChannelDataChanged -= this.TraceDataChanged;
						this._fp_chart.Series.Remove(this._series);
						this._series = null;
						this._lastTime = TimeSpan.Zero;
					}
				}
			}

			// Token: 0x060006EF RID: 1775 RVA: 0x0003C112 File Offset: 0x0003A312
			public override string ToString()
			{
				return string.Format("{0}:{1}", this._fp_channel.ChannelInfo.Id.Source, this._fp_channel.ChannelInfo.Id.Name);
			}

			// Token: 0x060006F0 RID: 1776 RVA: 0x0003C148 File Offset: 0x0003A348
			private void TraceDataChanged(object sender, MeteringChannelDataEventArgs args)
			{
				FastLineBitmapSeries series = this._series;
				if (args != null && series != null && this._isChecked)
				{
					MeteringDataPoint[] data3 = args.Data;
					if ((data3 == null || data3.Length != 0) && !this._isHidden)
					{
						List<LCChartUserControl.DataPt> data = null;
						bool clearChart = false;
						if (args.Data != null)
						{
							data = new List<LCChartUserControl.DataPt>(args.Data.Length);
							MeteringDataPoint[] data2 = args.Data;
							int i = 0;
							while (i < data2.Length)
							{
								MeteringDataPoint point = data2[i];
								double value;
								try
								{
									value = Convert.ToDouble(point.Value, CultureInfo.InvariantCulture);
								}
								catch (Exception)
								{
									goto IL_00DE;
								}
								goto IL_009A;
								IL_00DE:
								i++;
								continue;
								IL_009A:
								if (!double.IsNaN(value))
								{
									TimeSpan time = new TimeSpan(point.Timestamp);
									data.Add(new LCChartUserControl.DataPt(time.TotalMinutes, Math.Round(value, this._fp_channel.ChannelInfo.DisplayDecimals)));
									goto IL_00DE;
								}
								goto IL_00DE;
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
						this._fp_chart.Dispatcher.BeginInvoke(new Action(delegate
						{
							try
							{
								if (this._series != null)
								{
									series = this._series;
									ObservableCollection<LCChartUserControl.DataPt> ds = (ObservableCollection<LCChartUserControl.DataPt>)series.ItemsSource;
									if (ds != null)
									{
										if (clearChart)
										{
											ds.Clear();
											this._lastTime = TimeSpan.Zero;
										}
										else if (data != null && this.IsChecked)
										{
											while (ds.Count > 0 && Math.Abs(data[0].Time - ds[0].Time) > this._fp_maxTimeBufferMin)
											{
												ds.RemoveAt(0);
											}
											foreach (LCChartUserControl.DataPt point2 in data)
											{
												if (point2.Time >= this._lastTime.TotalMinutes + this._chartInterval.TotalMinutes)
												{
													ds.Add(point2);
													this._lastTime = TimeSpan.FromMinutes(point2.Time);
												}
											}
										}
									}
								}
							}
							catch (Exception)
							{
							}
						}), Array.Empty<object>());
						return;
					}
				}
			}

			// Token: 0x040003A6 RID: 934
			[CompilerGenerated]
			private SfChart _fp_chart;

			// Token: 0x040003A7 RID: 935
			[CompilerGenerated]
			private IMeteringChannel _fp_channel;

			// Token: 0x040003A8 RID: 936
			[CompilerGenerated]
			private double _fp_maxTimeBufferMin;

			// Token: 0x040003A9 RID: 937
			private FastLineBitmapSeries _series;

			// Token: 0x040003AA RID: 938
			private bool _isHidden = true;

			// Token: 0x040003AB RID: 939
			private bool _isChecked;

			// Token: 0x040003AC RID: 940
			private TimeSpan _lastTime = TimeSpan.Zero;

			// Token: 0x040003AD RID: 941
			private readonly TimeSpan _chartInterval = TimeSpan.FromMilliseconds(333.0);
		}
	}
}
