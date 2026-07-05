using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;

namespace BalticWpfControlLib
{
	// Token: 0x0200000C RID: 12
	public partial class AdvancedChildParameterWindow : Window
	{
		// Token: 0x14000002 RID: 2
		// (add) Token: 0x0600003A RID: 58 RVA: 0x00002A60 File Offset: 0x00000C60
		// (remove) Token: 0x0600003B RID: 59 RVA: 0x00002A98 File Offset: 0x00000C98
		public event AdvancedChildParameterWindow.ModificationUpdate ModificationUpdateEvent;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x0600003C RID: 60 RVA: 0x00002AD0 File Offset: 0x00000CD0
		// (remove) Token: 0x0600003D RID: 61 RVA: 0x00002B08 File Offset: 0x00000D08
		public event AdvancedChildParameterWindow.ValidationUpdate ValidationUpdateEvent;

		// Token: 0x0600003E RID: 62 RVA: 0x00002B40 File Offset: 0x00000D40
		public AdvancedChildParameterWindow(string header, BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment)
		{
			this.InitializeComponent();
			this._advChildSettingsUC = new AdvChildParamSettingsUserControl(header, method, instrument, experiment);
			this._advChildSettingsUC.ValidationUpdateEvent += this._advChildSettingsUC_ValidationUpdateEvent;
			this._advChildSettingsUC.ModificationUpdateEvent += this._advChildSettingsUC_ModificationUpdateEvent;
			Grid.SetRow(this._advChildSettingsUC, 0);
			Grid.SetColumn(this._advChildSettingsUC, 0);
			this._advChildSettingsUC.VerticalAlignment = VerticalAlignment.Top;
			this.gridParam.Children.Add(this._advChildSettingsUC);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002BD2 File Offset: 0x00000DD2
		private void _advChildSettingsUC_ModificationUpdateEvent(bool isModified)
		{
			AdvancedChildParameterWindow.ModificationUpdate modificationUpdateEvent = this.ModificationUpdateEvent;
			if (modificationUpdateEvent == null)
			{
				return;
			}
			modificationUpdateEvent(isModified);
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00002BE5 File Offset: 0x00000DE5
		private void _advChildSettingsUC_ValidationUpdateEvent(bool isValid)
		{
			AdvancedChildParameterWindow.ValidationUpdate validationUpdateEvent = this.ValidationUpdateEvent;
			if (validationUpdateEvent == null)
			{
				return;
			}
			validationUpdateEvent(isValid);
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00002BF8 File Offset: 0x00000DF8
		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (!base.IsLoaded)
			{
				return;
			}
			this.UpdateLocation();
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00002C0C File Offset: 0x00000E0C
		private void UpdateLocation()
		{
			Rectangle bounds = new Rectangle
			{
				Width = (int)base.ActualWidth,
				Height = (int)base.ActualHeight,
				X = (int)base.Left,
				Y = (int)base.Top
			};
			bounds = UIHelper.CalcOnScreenBounds(bounds);
			base.Top = (double)bounds.Y;
			base.Left = (double)bounds.X;
		}

		// Token: 0x04000018 RID: 24
		private readonly AdvChildParamSettingsUserControl _advChildSettingsUC;

		// Token: 0x0200008D RID: 141
		// (Invoke) Token: 0x0600066A RID: 1642
		public delegate void ModificationUpdate(bool isModified);

		// Token: 0x0200008E RID: 142
		// (Invoke) Token: 0x0600066E RID: 1646
		public delegate void ValidationUpdate(bool isValid);
	}
}
