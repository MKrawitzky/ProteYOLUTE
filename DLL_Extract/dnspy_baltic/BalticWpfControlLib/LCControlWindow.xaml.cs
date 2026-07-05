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

namespace BalticWpfControlLib
{
	// Token: 0x02000023 RID: 35
	public partial class LCControlWindow : Window
	{
		// Token: 0x1400001B RID: 27
		// (add) Token: 0x06000181 RID: 385 RVA: 0x0000A8EC File Offset: 0x00008AEC
		// (remove) Token: 0x06000182 RID: 386 RVA: 0x0000A924 File Offset: 0x00008B24
		public event LCControlWindow.ExecutionReport ExecutionReportEvent;

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x06000183 RID: 387 RVA: 0x0000A95C File Offset: 0x00008B5C
		// (remove) Token: 0x06000184 RID: 388 RVA: 0x0000A994 File Offset: 0x00008B94
		public event LCControlWindow.ConfirmButtonOnErrorCallback ConfirmButtonEvent;

		// Token: 0x06000185 RID: 389 RVA: 0x0000A9CC File Offset: 0x00008BCC
		public LCControlWindow(BalticInstrumentFacade instrument, List<BalticHWProfile.CapillaryItem> capillaries, List<BalticPreferences.CapillaryPreference> prefCapillaries, string displayName, bool isOvenInstalled, bool displayPressureAsPsi, ExecutionStateChangedEventArgs initialExecutionState)
		{
			this.InitializeComponent();
			this._instrument = instrument;
			this._ucLCControl = new LCUserControl(instrument, capillaries, prefCapillaries, displayPressureAsPsi, isOvenInstalled, initialExecutionState, this);
			this._ucLCControl.ExecutionReportEvent += this.ucLCControl_ExecutionReportEvent;
			this._ucLCControl.ConfirmButtonEvent += this.UcLCControl_ConfirmButtonEvent;
			this._ucLCControl.VerticalAlignment = VerticalAlignment.Stretch;
			this.gridLCControl.Children.Add(this._ucLCControl);
			base.Title = displayName + " control and diagnostics";
			this._ucLCControl.IsService = this._instrument.CheckForServiceMode();
		}

		// Token: 0x06000186 RID: 390 RVA: 0x0000AA7A File Offset: 0x00008C7A
		private void UcLCControl_ConfirmButtonEvent(SystemCondition condition)
		{
			LCControlWindow.ConfirmButtonOnErrorCallback confirmButtonEvent = this.ConfirmButtonEvent;
			if (confirmButtonEvent == null)
			{
				return;
			}
			confirmButtonEvent(condition);
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000AA90 File Offset: 0x00008C90
		private void OnKeyDown(object sender, global::System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				base.Close();
			}
			if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Shift)
			{
				if (this._ucLCControl.IsService)
				{
					this._instrument.ClearServiceMode();
					this._ucLCControl.IsService = false;
					return;
				}
				if (new PasswordWindow
				{
					Owner = this
				}.ShowDialog().GetValueOrDefault())
				{
					this._instrument.CreateServiceMode();
					this._ucLCControl.IsService = true;
				}
			}
		}

		// Token: 0x06000188 RID: 392 RVA: 0x0000AB1B File Offset: 0x00008D1B
		private void ucLCControl_ExecutionReportEvent(ExecutionStateChangedEventArgs e)
		{
			LCControlWindow.ExecutionReport executionReportEvent = this.ExecutionReportEvent;
			if (executionReportEvent == null)
			{
				return;
			}
			executionReportEvent(e);
		}

		// Token: 0x17000039 RID: 57
		// (set) Token: 0x06000189 RID: 393 RVA: 0x0000AB2E File Offset: 0x00008D2E
		public IChromatographyColumnType SeparatorColumnType
		{
			set
			{
				this._ucLCControl.SeparatorType = value;
			}
		}

		// Token: 0x1700003A RID: 58
		// (set) Token: 0x0600018A RID: 394 RVA: 0x0000AB3C File Offset: 0x00008D3C
		public IChromatographyColumnType TrapColumnType
		{
			set
			{
				this._ucLCControl.TrapType = value;
			}
		}

		// Token: 0x0600018B RID: 395 RVA: 0x0000AB4C File Offset: 0x00008D4C
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			LCUserControlSettings settings = Settings.Default.LCUserControlV2;
			if (settings != null)
			{
				this._ucLCControl.Settings = settings;
			}
			Rectangle rect = LCControlWindow.GetFittingRectangleForScreen(Settings.Default.LCControlPos, Settings.Default.LCControlSize);
			base.Left = (double)rect.Left;
			base.Top = (double)rect.Top;
			base.Width = (double)rect.Width;
			base.Height = (double)rect.Height;
		}

		// Token: 0x0600018C RID: 396 RVA: 0x0000ABC8 File Offset: 0x00008DC8
		private void Window_Closed(object sender, EventArgs e)
		{
			Settings.Default.LCUserControlV2 = this._ucLCControl.Settings;
			Settings.Default.LCControlSize = new global::System.Drawing.Size((int)base.Width, (int)base.Height);
			Settings.Default.LCControlPos = new global::System.Drawing.Point((int)base.Left, (int)base.Top);
			Settings.Default.Save();
		}

		// Token: 0x0600018D RID: 397 RVA: 0x0000AC30 File Offset: 0x00008E30
		private static Rectangle GetFittingRectangleForScreen(global::System.Drawing.Point point, global::System.Drawing.Size size)
		{
			Rectangle rect = new Rectangle(point, size);
			Screen screen = Screen.FromRectangle(rect);
			if (rect.Width > screen.WorkingArea.Width)
			{
				rect.Width = screen.WorkingArea.Width;
			}
			if (rect.Height > screen.WorkingArea.Height)
			{
				rect.Height = screen.WorkingArea.Height;
			}
			if (rect.X < screen.WorkingArea.X)
			{
				rect.X = screen.WorkingArea.X;
			}
			else if (rect.Right > screen.WorkingArea.Right)
			{
				rect.X -= rect.Right - screen.WorkingArea.Right;
			}
			if (rect.Y < screen.WorkingArea.Y)
			{
				rect.Y = screen.WorkingArea.Y;
			}
			else if (rect.Bottom > screen.WorkingArea.Bottom)
			{
				rect.Y -= rect.Bottom - screen.WorkingArea.Bottom;
			}
			return rect;
		}

		// Token: 0x0600018E RID: 398 RVA: 0x0000AD78 File Offset: 0x00008F78
		public void AbortActiveProcedure()
		{
			LCUserControl ucLCControl = this._ucLCControl;
			if (ucLCControl == null)
			{
				return;
			}
			ucLCControl.AbortActiveProcedure();
		}

		// Token: 0x0600018F RID: 399 RVA: 0x0000AD8A File Offset: 0x00008F8A
		public void ClickConfirmButton()
		{
			LCUserControl ucLCControl = this._ucLCControl;
			if (ucLCControl == null)
			{
				return;
			}
			ucLCControl.confirmButton_Click(this, null);
		}

		// Token: 0x040000DC RID: 220
		private readonly LCUserControl _ucLCControl;

		// Token: 0x040000DD RID: 221
		private readonly BalticInstrumentFacade _instrument;

		// Token: 0x020000BE RID: 190
		// (Invoke) Token: 0x060006F8 RID: 1784
		public delegate void ExecutionReport(ExecutionStateChangedEventArgs e);

		// Token: 0x020000BF RID: 191
		// (Invoke) Token: 0x060006FC RID: 1788
		public delegate void ConfirmButtonOnErrorCallback(SystemCondition condition);
	}
}
