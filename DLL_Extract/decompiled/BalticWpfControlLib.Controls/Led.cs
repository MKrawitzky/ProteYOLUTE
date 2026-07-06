// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace BalticWpfControlLib.Controls;

public class Led : UserControl, IComponentConnector
{
	public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool?), typeof(Led), new PropertyMetadata(null, IsActivePropertyChanced));

	public static readonly DependencyProperty ColorOnProperty = DependencyProperty.Register("ColorOn", typeof(Color), typeof(Led), new PropertyMetadata(Colors.Green, OnColorOnPropertyChanged));

	public static readonly DependencyProperty ColorOffProperty = DependencyProperty.Register("ColorOff", typeof(Color), typeof(Led), new PropertyMetadata(Colors.Red, OnColorOffPropertyChanged));

	public static readonly DependencyProperty ColorNullProperty = DependencyProperty.Register("ColorNull", typeof(Color), typeof(Led), new PropertyMetadata(Colors.Gray, OnColorNullPropertyChanged));

	public static readonly DependencyProperty FlashingProperty = DependencyProperty.Register("Flashing", typeof(bool), typeof(Led), new PropertyMetadata(false, OnFlashingPropertyChanged));

	public static readonly DependencyProperty FlashingPeriodProperty = DependencyProperty.Register("FlashingPeriod", typeof(int), typeof(Led), new PropertyMetadata(500, OnFlashingPeriodPropertyChanged));

	private DispatcherTimer timer = new DispatcherTimer();

	internal Grid gridBigLed;

	internal Border border1;

	internal GradientStop backgroundColor;

	private bool _contentLoaded;

	public bool? IsActive
	{
		get
		{
			return (bool?)GetValue(IsActiveProperty);
		}
		set
		{
			SetValue(IsActiveProperty, value);
		}
	}

	public Color ColorOn
	{
		get
		{
			return (Color)GetValue(ColorOnProperty);
		}
		set
		{
			SetValue(ColorOnProperty, value);
		}
	}

	public Color ColorOff
	{
		get
		{
			return (Color)GetValue(ColorOffProperty);
		}
		set
		{
			SetValue(ColorOffProperty, value);
		}
	}

	public Color ColorNull
	{
		get
		{
			return (Color)GetValue(ColorNullProperty);
		}
		set
		{
			SetValue(ColorNullProperty, value);
		}
	}

	public bool Flashing
	{
		get
		{
			return (bool)GetValue(FlashingProperty);
		}
		set
		{
			SetValue(FlashingProperty, value);
		}
	}

	public int FlashingPeriod
	{
		get
		{
			return (int)GetValue(FlashingPeriodProperty);
		}
		set
		{
			SetValue(FlashingPeriodProperty, value);
		}
	}

	public Led()
	{
		InitializeComponent();
		timer.Interval = TimeSpan.FromMilliseconds(FlashingPeriod);
		timer.Tick += timer_Tick;
		if (IsActive.GetValueOrDefault())
		{
			backgroundColor.Color = ColorOn;
		}
		else if (IsActive == false)
		{
			backgroundColor.Color = ColorOff;
		}
		else
		{
			backgroundColor.Color = ColorNull;
		}
	}

	private void timer_Tick(object sender, EventArgs e)
	{
		if (IsActive.GetValueOrDefault())
		{
			if (backgroundColor.Color == ColorOn)
			{
				backgroundColor.Color = ColorNull;
			}
			else
			{
				backgroundColor.Color = ColorOn;
			}
		}
		if (IsActive == false)
		{
			if (backgroundColor.Color == ColorOff)
			{
				backgroundColor.Color = ColorNull;
			}
			else
			{
				backgroundColor.Color = ColorOff;
			}
		}
		if (!IsActive.HasValue && timer.IsEnabled)
		{
			timer.Stop();
		}
	}

	private static void OnFlashingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Led led = (Led)d;
		if (led.timer.IsEnabled)
		{
			led.timer.Stop();
			if (led.backgroundColor.Color == led.ColorNull)
			{
				led.timer_Tick(null, new EventArgs());
			}
		}
		else
		{
			led.timer.Start();
		}
	}

	private static void OnFlashingPeriodPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Led)d).timer.Interval = TimeSpan.FromMilliseconds((int)e.NewValue);
	}

	private static void IsActivePropertyChanced(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Led led = (Led)d;
		if (!led.IsActive.HasValue)
		{
			led.backgroundColor.Color = led.ColorNull;
		}
		else if (led.IsActive.GetValueOrDefault())
		{
			led.backgroundColor.Color = led.ColorOn;
		}
		else
		{
			led.backgroundColor.Color = led.ColorOff;
		}
	}

	private static void OnColorOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Led led = (Led)d;
		led.ColorOn = (Color)e.NewValue;
		if (led.IsActive.GetValueOrDefault())
		{
			led.backgroundColor.Color = led.ColorOn;
		}
	}

	private static void OnColorOffPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Led led = (Led)d;
		led.ColorOff = (Color)e.NewValue;
		if (led.IsActive == false)
		{
			led.backgroundColor.Color = led.ColorOff;
		}
	}

	private static void OnColorNullPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Led led = (Led)d;
		led.ColorOff = (Color)e.NewValue;
		if (!led.IsActive.HasValue)
		{
			led.backgroundColor.Color = led.ColorNull;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/controls/led.xaml", UriKind.Relative);
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
			gridBigLed = (Grid)target;
			break;
		case 2:
			border1 = (Border)target;
			break;
		case 3:
			backgroundColor = (GradientStop)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
