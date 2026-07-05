using System;
using System.Windows.Media;
using BalticWpfControlLib.Utilities;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x0200006F RID: 111
	public class ActivatableAndErrorAwareDataContext : BindableBase, IActivatable, IErrorAware, ISignalizeAware, ISignalizeTextAware, IVisibleEnableable, IToolTipable
	{
		// Token: 0x060004DB RID: 1243 RVA: 0x0001B074 File Offset: 0x00019274
		public ActivatableAndErrorAwareDataContext()
		{
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x0001B083 File Offset: 0x00019283
		public ActivatableAndErrorAwareDataContext(Brush activeBrush)
		{
			this._activeBrush = activeBrush;
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x060004DD RID: 1245 RVA: 0x0001B099 File Offset: 0x00019299
		// (set) Token: 0x060004DE RID: 1246 RVA: 0x0001B0B6 File Offset: 0x000192B6
		public bool IsActive
		{
			get
			{
				return this._isActive && !this._isSignalize && !this._isSignalizeText;
			}
			set
			{
				base.SetProperty<bool>(ref this._isActive, value, "IsActive");
			}
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x060004DF RID: 1247 RVA: 0x0001B0CB File Offset: 0x000192CB
		// (set) Token: 0x060004E0 RID: 1248 RVA: 0x0001B0D3 File Offset: 0x000192D3
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

		// Token: 0x170000E3 RID: 227
		// (set) Token: 0x060004E1 RID: 1249 RVA: 0x0001B0E8 File Offset: 0x000192E8
		private string Error
		{
			set
			{
				if (object.Equals(this.ErrorMessage, value))
				{
					return;
				}
				this.ErrorMessage = value;
				this.OnPropertyChanged("ErrorMessage");
				this.OnPropertyChanged("HasError");
			}
		}

		// Token: 0x060004E2 RID: 1250 RVA: 0x0001B116 File Offset: 0x00019316
		public void SetError(string errorMessage)
		{
			this.Error = errorMessage;
		}

		// Token: 0x060004E3 RID: 1251 RVA: 0x0001B11F File Offset: 0x0001931F
		public void ClearError()
		{
			this.Error = null;
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x060004E4 RID: 1252 RVA: 0x0001B128 File Offset: 0x00019328
		public bool HasError
		{
			get
			{
				return this.ErrorMessage != null;
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x060004E5 RID: 1253 RVA: 0x0001B133 File Offset: 0x00019333
		// (set) Token: 0x060004E6 RID: 1254 RVA: 0x0001B13B File Offset: 0x0001933B
		public string ErrorMessage { get; private set; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x060004E7 RID: 1255 RVA: 0x0001B144 File Offset: 0x00019344
		// (set) Token: 0x060004E8 RID: 1256 RVA: 0x0001B14C File Offset: 0x0001934C
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

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x060004E9 RID: 1257 RVA: 0x0001B16B File Offset: 0x0001936B
		// (set) Token: 0x060004EA RID: 1258 RVA: 0x0001B173 File Offset: 0x00019373
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

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x060004EB RID: 1259 RVA: 0x0001B188 File Offset: 0x00019388
		// (set) Token: 0x060004EC RID: 1260 RVA: 0x0001B190 File Offset: 0x00019390
		public bool IsSignalizeText
		{
			get
			{
				return this._isSignalizeText;
			}
			set
			{
				this._isSignalizeText = value;
				this.OnPropertyChanged("IsSignalizeText");
				this.OnPropertyChanged("IsActive");
			}
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x060004ED RID: 1261 RVA: 0x0001B1AF File Offset: 0x000193AF
		// (set) Token: 0x060004EE RID: 1262 RVA: 0x0001B1B7 File Offset: 0x000193B7
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

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x060004EF RID: 1263 RVA: 0x0001B1CC File Offset: 0x000193CC
		// (set) Token: 0x060004F0 RID: 1264 RVA: 0x0001B1D4 File Offset: 0x000193D4
		public bool IsVisible
		{
			get
			{
				return this._isVisible;
			}
			set
			{
				base.SetProperty<bool>(ref this._isVisible, value, "IsVisible");
			}
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x060004F1 RID: 1265 RVA: 0x0001B1E9 File Offset: 0x000193E9
		// (set) Token: 0x060004F2 RID: 1266 RVA: 0x0001B1F1 File Offset: 0x000193F1
		public bool IsNearTransparent
		{
			get
			{
				return this._isNearTransparent;
			}
			set
			{
				base.SetProperty<bool>(ref this._isNearTransparent, value, "IsNearTransparent");
			}
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x060004F3 RID: 1267 RVA: 0x0001B206 File Offset: 0x00019406
		// (set) Token: 0x060004F4 RID: 1268 RVA: 0x0001B20E File Offset: 0x0001940E
		public bool IsToolTipVisible
		{
			get
			{
				return this._isToolTipVisible;
			}
			set
			{
				base.SetProperty<bool>(ref this._isToolTipVisible, value, "IsToolTipVisible");
			}
		}

		// Token: 0x0400028F RID: 655
		private bool _isActive;

		// Token: 0x04000290 RID: 656
		private Brush _activeBrush;

		// Token: 0x04000292 RID: 658
		private bool _isSignalize;

		// Token: 0x04000293 RID: 659
		private Brush _signalizeBrush;

		// Token: 0x04000294 RID: 660
		private bool _isSignalizeText;

		// Token: 0x04000295 RID: 661
		private Brush _signalizeTextBrush;

		// Token: 0x04000296 RID: 662
		private bool _isVisible = true;

		// Token: 0x04000297 RID: 663
		private bool _isNearTransparent;

		// Token: 0x04000298 RID: 664
		private bool _isToolTipVisible;
	}
}
