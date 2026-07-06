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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BalticWpfControlLib.Controls;

public class CircularProgressBar : UserControl, IComponentConnector
{
	private readonly DispatcherTimer animationTimer;

	internal Grid LayoutRoot;

	internal Ellipse C0;

	internal Ellipse C1;

	internal Ellipse C2;

	internal Ellipse C3;

	internal Ellipse C4;

	internal Ellipse C5;

	internal Ellipse C6;

	internal Ellipse C7;

	internal Ellipse C8;

	internal RotateTransform SpinnerRotate;

	private bool _contentLoaded;

	public CircularProgressBar()
	{
		InitializeComponent();
		animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, base.Dispatcher);
		animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 75);
	}

	private void Start()
	{
		Mouse.OverrideCursor = Cursors.Wait;
		animationTimer.Tick += HandleAnimationTick;
		animationTimer.Start();
	}

	private void Stop()
	{
		animationTimer.Stop();
		Mouse.OverrideCursor = Cursors.Arrow;
		animationTimer.Tick -= HandleAnimationTick;
	}

	private void HandleAnimationTick(object sender, EventArgs e)
	{
		SpinnerRotate.Angle = (SpinnerRotate.Angle + 36.0) % 360.0;
	}

	private void HandleLoaded(object sender, RoutedEventArgs e)
	{
		SetPosition(C0, Math.PI, 0.0, Math.PI / 5.0);
		SetPosition(C1, Math.PI, 1.0, Math.PI / 5.0);
		SetPosition(C2, Math.PI, 2.0, Math.PI / 5.0);
		SetPosition(C3, Math.PI, 3.0, Math.PI / 5.0);
		SetPosition(C4, Math.PI, 4.0, Math.PI / 5.0);
		SetPosition(C5, Math.PI, 5.0, Math.PI / 5.0);
		SetPosition(C6, Math.PI, 6.0, Math.PI / 5.0);
		SetPosition(C7, Math.PI, 7.0, Math.PI / 5.0);
		SetPosition(C8, Math.PI, 8.0, Math.PI / 5.0);
	}

	private static void SetPosition(Ellipse ellipse, double offset, double posOffSet, double step)
	{
		ellipse.SetValue(Canvas.LeftProperty, 50.0 + Math.Sin(offset + posOffSet * step) * 50.0);
		ellipse.SetValue(Canvas.TopProperty, 50.0 + Math.Cos(offset + posOffSet * step) * 50.0);
	}

	private void HandleUnloaded(object sender, RoutedEventArgs e)
	{
		Stop();
	}

	private void HandleVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			Start();
		}
		else
		{
			Stop();
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/controls/circularprogressbar.xaml", UriKind.Relative);
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
			((CircularProgressBar)target).IsVisibleChanged += HandleVisibleChanged;
			break;
		case 2:
			LayoutRoot = (Grid)target;
			break;
		case 3:
			((Canvas)target).Loaded += HandleLoaded;
			((Canvas)target).Unloaded += HandleUnloaded;
			break;
		case 4:
			C0 = (Ellipse)target;
			break;
		case 5:
			C1 = (Ellipse)target;
			break;
		case 6:
			C2 = (Ellipse)target;
			break;
		case 7:
			C3 = (Ellipse)target;
			break;
		case 8:
			C4 = (Ellipse)target;
			break;
		case 9:
			C5 = (Ellipse)target;
			break;
		case 10:
			C6 = (Ellipse)target;
			break;
		case 11:
			C7 = (Ellipse)target;
			break;
		case 12:
			C8 = (Ellipse)target;
			break;
		case 13:
			SpinnerRotate = (RotateTransform)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
