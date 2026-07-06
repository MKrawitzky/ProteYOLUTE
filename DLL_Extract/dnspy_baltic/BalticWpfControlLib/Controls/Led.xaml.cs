// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace BalticWpfControlLib.Controls
{
	// Token: 0x02000087 RID: 135
	public partial class Led : UserControl
	{
		// Token: 0x17000165 RID: 357
		// (get) Token: 0x06000644 RID: 1604 RVA: 0x0003B01D File Offset: 0x0003921D
		// (set) Token: 0x06000645 RID: 1605 RVA: 0x0003B02F File Offset: 0x0003922F
		public bool? IsActive
		{
			get
			{
				return (bool?)base.GetValue(Led.IsActiveProperty);
			}
			set
			{
				base.SetValue(Led.IsActiveProperty, value);
			}
		}

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x06000646 RID: 1606 RVA: 0x0003B042 File Offset: 0x00039242
		// (set) Token: 0x06000647 RID: 1607 RVA: 0x0003B054 File Offset: 0x00039254
		public Color ColorOn
		{
			get
			{
				return (Color)base.GetValue(Led.ColorOnProperty);
			}
			set
			{
				base.SetValue(Led.ColorOnProperty, value);
			}
		}

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x06000648 RID: 1608 RVA: 0x0003B067 File Offset: 0x00039267
		// (set) Token: 0x06000649 RID: 1609 RVA: 0x0003B079 File Offset: 0x00039279
		public Color ColorOff
		{
			get
			{
				return (Color)base.GetValue(Led.ColorOffProperty);
			}
			set
			{
				base.SetValue(Led.ColorOffProperty, value);
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x0600064A RID: 1610 RVA: 0x0003B08C File Offset: 0x0003928C
		// (set) Token: 0x0600064B RID: 1611 RVA: 0x0003B09E File Offset: 0x0003929E
		public Color ColorNull
		{
			get
			{
				return (Color)base.GetValue(Led.ColorNullProperty);
			}
			set
			{
				base.SetValue(Led.ColorNullProperty, value);
			}
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x0600064C RID: 1612 RVA: 0x0003B0B1 File Offset: 0x000392B1
		// (set) Token: 0x0600064D RID: 1613 RVA: 0x0003B0C3 File Offset: 0x000392C3
		public bool Flashing
		{
			get
			{
				return (bool)base.GetValue(Led.FlashingProperty);
			}
			set
			{
				base.SetValue(Led.FlashingProperty, value);
			}
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x0600064E RID: 1614 RVA: 0x0003B0D6 File Offset: 0x000392D6
		// (set) Token: 0x0600064F RID: 1615 RVA: 0x0003B0E8 File Offset: 0x000392E8
		public int FlashingPeriod
		{
			get
			{
				return (int)base.GetValue(Led.FlashingPeriodProperty);
			}
			set
			{
				base.SetValue(Led.FlashingPeriodProperty, value);
			}
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x0003B0FC File Offset: 0x000392FC
		public Led()
		{
			this.InitializeComponent();
			this.timer.Interval = TimeSpan.FromMilliseconds((double)this.FlashingPeriod);
			this.timer.Tick += this.timer_Tick;
			if (this.IsActive.GetValueOrDefault())
			{
				this.backgroundColor.Color = this.ColorOn;
				return;
			}
			bool? isActive = this.IsActive;
			bool flag = false;
			if ((isActive.GetValueOrDefault() == flag) & (isActive != null))
			{
				this.backgroundColor.Color = this.ColorOff;
				return;
			}
			this.backgroundColor.Color = this.ColorNull;
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x0003B1B0 File Offset: 0x000393B0
		private void timer_Tick(object sender, EventArgs e)
		{
			if (this.IsActive.GetValueOrDefault())
			{
				if (this.backgroundColor.Color == this.ColorOn)
				{
					this.backgroundColor.Color = this.ColorNull;
				}
				else
				{
					this.backgroundColor.Color = this.ColorOn;
				}
			}
			bool? isActive = this.IsActive;
			bool flag = false;
			if ((isActive.GetValueOrDefault() == flag) & (isActive != null))
			{
				if (this.backgroundColor.Color == this.ColorOff)
				{
					this.backgroundColor.Color = this.ColorNull;
				}
				else
				{
					this.backgroundColor.Color = this.ColorOff;
				}
			}
			if (this.IsActive == null && this.timer.IsEnabled)
			{
				this.timer.Stop();
			}
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x0003B28C File Offset: 0x0003948C
		private static void OnFlashingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			if (led.timer.IsEnabled)
			{
				led.timer.Stop();
				if (led.backgroundColor.Color == led.ColorNull)
				{
					led.timer_Tick(null, new EventArgs());
					return;
				}
			}
			else
			{
				led.timer.Start();
			}
		}

		// Token: 0x06000653 RID: 1619 RVA: 0x0003B2E8 File Offset: 0x000394E8
		private static void OnFlashingPeriodPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Led)d).timer.Interval = TimeSpan.FromMilliseconds((double)((int)e.NewValue));
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x0003B30C File Offset: 0x0003950C
		private static void IsActivePropertyChanced(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			if (led.IsActive == null)
			{
				led.backgroundColor.Color = led.ColorNull;
				return;
			}
			if (led.IsActive.GetValueOrDefault())
			{
				led.backgroundColor.Color = led.ColorOn;
				return;
			}
			led.backgroundColor.Color = led.ColorOff;
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x0003B378 File Offset: 0x00039578
		private static void OnColorOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			led.ColorOn = (Color)e.NewValue;
			if (led.IsActive.GetValueOrDefault())
			{
				led.backgroundColor.Color = led.ColorOn;
			}
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0003B3C0 File Offset: 0x000395C0
		private static void OnColorOffPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			led.ColorOff = (Color)e.NewValue;
			bool? isActive = led.IsActive;
			bool flag = false;
			if ((isActive.GetValueOrDefault() == flag) & (isActive != null))
			{
				led.backgroundColor.Color = led.ColorOff;
			}
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0003B414 File Offset: 0x00039614
		private static void OnColorNullPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			led.ColorOff = (Color)e.NewValue;
			if (led.IsActive == null)
			{
				led.backgroundColor.Color = led.ColorNull;
			}
		}

		// Token: 0x04000352 RID: 850
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool?), typeof(Led), new PropertyMetadata(null, new PropertyChangedCallback(Led.IsActivePropertyChanced)));

		// Token: 0x04000353 RID: 851
		public static readonly DependencyProperty ColorOnProperty = DependencyProperty.Register("ColorOn", typeof(Color), typeof(Led), new PropertyMetadata(Colors.Green, new PropertyChangedCallback(Led.OnColorOnPropertyChanged)));

		// Token: 0x04000354 RID: 852
		public static readonly DependencyProperty ColorOffProperty = DependencyProperty.Register("ColorOff", typeof(Color), typeof(Led), new PropertyMetadata(Colors.Red, new PropertyChangedCallback(Led.OnColorOffPropertyChanged)));

		// Token: 0x04000355 RID: 853
		public static readonly DependencyProperty ColorNullProperty = DependencyProperty.Register("ColorNull", typeof(Color), typeof(Led), new PropertyMetadata(Colors.Gray, new PropertyChangedCallback(Led.OnColorNullPropertyChanged)));

		// Token: 0x04000356 RID: 854
		public static readonly DependencyProperty FlashingProperty = DependencyProperty.Register("Flashing", typeof(bool), typeof(Led), new PropertyMetadata(false, new PropertyChangedCallback(Led.OnFlashingPropertyChanged)));

		// Token: 0x04000357 RID: 855
		public static readonly DependencyProperty FlashingPeriodProperty = DependencyProperty.Register("FlashingPeriod", typeof(int), typeof(Led), new PropertyMetadata(500, new PropertyChangedCallback(Led.OnFlashingPeriodPropertyChanged)));

		// Token: 0x04000358 RID: 856
		private DispatcherTimer timer = new DispatcherTimer();
	}
}
