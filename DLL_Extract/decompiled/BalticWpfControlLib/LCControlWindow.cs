// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using BalticClassLib;
using BalticWpfControlLib.Properties;
using Bruker.Lc;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib;

public class LCControlWindow : Window, IComponentConnector
{
	public delegate void ExecutionReport(ExecutionStateChangedEventArgs e);

	public delegate void ConfirmButtonOnErrorCallback(SystemCondition condition);

	private readonly LCUserControl _ucLCControl;

	private readonly BalticInstrumentFacade _instrument;

	internal DockPanel gridLCControl;

	private bool _contentLoaded;

	public IChromatographyColumnType SeparatorColumnType
	{
		set
		{
			_ucLCControl.SeparatorType = value;
		}
	}

	public IChromatographyColumnType TrapColumnType
	{
		set
		{
			_ucLCControl.TrapType = value;
		}
	}

	public event ExecutionReport ExecutionReportEvent;

	public event ConfirmButtonOnErrorCallback ConfirmButtonEvent;

	public LCControlWindow(BalticInstrumentFacade instrument, List<BalticHWProfile.CapillaryItem> capillaries, List<BalticPreferences.CapillaryPreference> prefCapillaries, string displayName, bool isOvenInstalled, bool displayPressureAsPsi, ExecutionStateChangedEventArgs initialExecutionState)
	{
		InitializeComponent();
		_instrument = instrument;
		_ucLCControl = new LCUserControl(instrument, capillaries, prefCapillaries, displayPressureAsPsi, isOvenInstalled, initialExecutionState, this);
		_ucLCControl.ExecutionReportEvent += ucLCControl_ExecutionReportEvent;
		_ucLCControl.ConfirmButtonEvent += UcLCControl_ConfirmButtonEvent;
		_ucLCControl.VerticalAlignment = VerticalAlignment.Stretch;
		gridLCControl.Children.Add(_ucLCControl);
		base.Title = displayName + " control and diagnostics";
		_ucLCControl.IsService = _instrument.CheckForServiceMode();
	}

	private void UcLCControl_ConfirmButtonEvent(SystemCondition condition)
	{
		this.ConfirmButtonEvent?.Invoke(condition);
	}

	private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			Close();
		}
		if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Shift)
		{
			if (_ucLCControl.IsService)
			{
				_instrument.ClearServiceMode();
				_ucLCControl.IsService = false;
			}
			else if (new PasswordWindow
			{
				Owner = this
			}.ShowDialog().GetValueOrDefault())
			{
				_instrument.CreateServiceMode();
				_ucLCControl.IsService = true;
			}
		}
	}

	private void ucLCControl_ExecutionReportEvent(ExecutionStateChangedEventArgs e)
	{
		this.ExecutionReportEvent?.Invoke(e);
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		LCUserControlSettings lCUserControlV = Settings.Default.LCUserControlV2;
		if (lCUserControlV != null)
		{
			_ucLCControl.Settings = lCUserControlV;
		}
		Rectangle fittingRectangleForScreen = GetFittingRectangleForScreen(Settings.Default.LCControlPos, Settings.Default.LCControlSize);
		base.Left = fittingRectangleForScreen.Left;
		base.Top = fittingRectangleForScreen.Top;
		base.Width = fittingRectangleForScreen.Width;
		base.Height = fittingRectangleForScreen.Height;
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		Settings.Default.LCUserControlV2 = _ucLCControl.Settings;
		Settings.Default.LCControlSize = new System.Drawing.Size((int)base.Width, (int)base.Height);
		Settings.Default.LCControlPos = new System.Drawing.Point((int)base.Left, (int)base.Top);
		Settings.Default.Save();
	}

	private static Rectangle GetFittingRectangleForScreen(System.Drawing.Point point, System.Drawing.Size size)
	{
		Rectangle rectangle = new Rectangle(point, size);
		Screen screen = Screen.FromRectangle(rectangle);
		if (rectangle.Width > screen.WorkingArea.Width)
		{
			rectangle.Width = screen.WorkingArea.Width;
		}
		if (rectangle.Height > screen.WorkingArea.Height)
		{
			rectangle.Height = screen.WorkingArea.Height;
		}
		if (rectangle.X < screen.WorkingArea.X)
		{
			rectangle.X = screen.WorkingArea.X;
		}
		else if (rectangle.Right > screen.WorkingArea.Right)
		{
			rectangle.X -= rectangle.Right - screen.WorkingArea.Right;
		}
		if (rectangle.Y < screen.WorkingArea.Y)
		{
			rectangle.Y = screen.WorkingArea.Y;
		}
		else if (rectangle.Bottom > screen.WorkingArea.Bottom)
		{
			rectangle.Y -= rectangle.Bottom - screen.WorkingArea.Bottom;
		}
		return rectangle;
	}

	public void AbortActiveProcedure()
	{
		_ucLCControl?.AbortActiveProcedure();
	}

	public void ClickConfirmButton()
	{
		_ucLCControl?.confirmButton_Click(this, null);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/lccontrolwindow.xaml", UriKind.Relative);
			System.Windows.Application.LoadComponent(this, resourceLocator);
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
			((LCControlWindow)target).KeyDown += OnKeyDown;
			((LCControlWindow)target).Loaded += Window_Loaded;
			((LCControlWindow)target).Closed += Window_Closed;
			break;
		case 2:
			gridLCControl = (DockPanel)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
