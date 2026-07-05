using System;
using System.Windows.Media;
using BalticWpfControlLib.Utilities;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x02000070 RID: 112
	public class ActivatableDataContext : BindableBase, IActivatable, ISignalizeAware, ISignalizeTextAware, IVisibleEnableable
	{
		// Token: 0x060004F5 RID: 1269 RVA: 0x0001B223 File Offset: 0x00019423
		public ActivatableDataContext()
		{
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x0001B22B File Offset: 0x0001942B
		public ActivatableDataContext(Brush activeBrush)
		{
			this._activeBrush = activeBrush;
		}

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x060004F7 RID: 1271 RVA: 0x0001B23A File Offset: 0x0001943A
		// (set) Token: 0x060004F8 RID: 1272 RVA: 0x0001B24F File Offset: 0x0001944F
		public bool IsActive
		{
			get
			{
				return this._isActive && !this._isSignalize;
			}
			set
			{
				base.SetProperty<bool>(ref this._isActive, value, "IsActive");
			}
		}

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x060004F9 RID: 1273 RVA: 0x0001B264 File Offset: 0x00019464
		// (set) Token: 0x060004FA RID: 1274 RVA: 0x0001B26C File Offset: 0x0001946C
		public Brush ActiveBrush
		{
			get
			{
				return this._activeBrush;
			}
			set
			{
				base.SetProperty<Brush>(ref this._activeBrush, value, "ActiveBrush");
			}
		}

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x060004FB RID: 1275 RVA: 0x0001B281 File Offset: 0x00019481
		// (set) Token: 0x060004FC RID: 1276 RVA: 0x0001B289 File Offset: 0x00019489
		public bool IsSignalize
		{
			get
			{
				return this._isSignalize;
			}
			set
			{
				this._isSignalize = value;
				this.OnPropertyChanged("IsSignalize");
				this.OnPropertyChanged("IsActive");
			}
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x060004FD RID: 1277 RVA: 0x0001B2A8 File Offset: 0x000194A8
		// (set) Token: 0x060004FE RID: 1278 RVA: 0x0001B2B0 File Offset: 0x000194B0
		public Brush SignalizeBrush
		{
			get
			{
				return this._signalizeBrush;
			}
			set
			{
				base.SetProperty<Brush>(ref this._signalizeBrush, value, "SignalizeBrush");
			}
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x060004FF RID: 1279 RVA: 0x00006447 File Offset: 0x00004647
		// (set) Token: 0x06000500 RID: 1280 RVA: 0x0001B2C5 File Offset: 0x000194C5
		public bool IsSignalizeText
		{
			get
			{
				return false;
			}
			set
			{
				this._isSignalize = value;
				this.OnPropertyChanged("IsSignalizeText");
				this.OnPropertyChanged("IsActive");
			}
		}

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000501 RID: 1281 RVA: 0x0001B2E4 File Offset: 0x000194E4
		// (set) Token: 0x06000502 RID: 1282 RVA: 0x0001B2EC File Offset: 0x000194EC
		public Brush SignalizeTextBrush
		{
			get
			{
				return this._signalizeTextBrush;
			}
			set
			{
				base.SetProperty<Brush>(ref this._signalizeTextBrush, value, "SignalizeTextBrush");
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x06000503 RID: 1283 RVA: 0x0001B301 File Offset: 0x00019501
		// (set) Token: 0x06000504 RID: 1284 RVA: 0x0001B309 File Offset: 0x00019509
		public bool IsVisible
		{
			get
			{
				return this._isVisible;
			}
			set
			{
				this._isVisible = value;
				this.OnPropertyChanged("IsVisible");
			}
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x06000505 RID: 1285 RVA: 0x0001B31D File Offset: 0x0001951D
		// (set) Token: 0x06000506 RID: 1286 RVA: 0x0001B325 File Offset: 0x00019525
		public bool IsNearTransparent
		{
			get
			{
				return this._isNearTransparent;
			}
			set
			{
				this._isNearTransparent = value;
				this.OnPropertyChanged("IsNearTransparent");
			}
		}

		// Token: 0x04000299 RID: 665
		private bool _isActive;

		// Token: 0x0400029A RID: 666
		private Brush _activeBrush;

		// Token: 0x0400029B RID: 667
		private bool _isSignalize;

		// Token: 0x0400029C RID: 668
		private Brush _signalizeBrush;

		// Token: 0x0400029D RID: 669
		private const bool _isSignalizeText = false;

		// Token: 0x0400029E RID: 670
		private Brush _signalizeTextBrush;

		// Token: 0x0400029F RID: 671
		private bool _isVisible;

		// Token: 0x040002A0 RID: 672
		private bool _isNearTransparent;
	}
}
