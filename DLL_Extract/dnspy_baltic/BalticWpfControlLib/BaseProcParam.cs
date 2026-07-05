using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Shapes;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Business;

namespace BalticWpfControlLib
{
	// Token: 0x02000031 RID: 49
	internal class BaseProcParam : BindableBase
	{
		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060002EE RID: 750 RVA: 0x00013F0B File Offset: 0x0001210B
		public ProcedureArgument Argument { get; private set; }

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x060002EF RID: 751 RVA: 0x00013F13 File Offset: 0x00012113
		public string Unit { get; private set; }

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x060002F0 RID: 752 RVA: 0x00013F1B File Offset: 0x0001211B
		public Type Type { get; private set; }

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x00013F23 File Offset: 0x00012123
		public string Group { get; private set; }

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x00013F2B File Offset: 0x0001212B
		// (set) Token: 0x060002F3 RID: 755 RVA: 0x00013F33 File Offset: 0x00012133
		public bool IsBoolButton { get; set; }

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x00013F3C File Offset: 0x0001213C
		// (set) Token: 0x060002F5 RID: 757 RVA: 0x00013F44 File Offset: 0x00012144
		public bool IsStandard { get; set; }

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x060002F6 RID: 758 RVA: 0x00013F4D File Offset: 0x0001214D
		// (set) Token: 0x060002F7 RID: 759 RVA: 0x00013F55 File Offset: 0x00012155
		public bool IsSeparator { get; set; }

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x060002F8 RID: 760 RVA: 0x00013F5E File Offset: 0x0001215E
		public int Indent { get; private set; }

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x060002F9 RID: 761 RVA: 0x00013F66 File Offset: 0x00012166
		public int Decimals { get; private set; }

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x060002FA RID: 762 RVA: 0x00013F6E File Offset: 0x0001216E
		// (set) Token: 0x060002FB RID: 763 RVA: 0x00013F76 File Offset: 0x00012176
		public bool IsService { get; set; }

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x060002FC RID: 764 RVA: 0x00013F7F File Offset: 0x0001217F
		// (set) Token: 0x060002FD RID: 765 RVA: 0x00013F87 File Offset: 0x00012187
		public bool IsAppService { get; set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x060002FE RID: 766 RVA: 0x00013F90 File Offset: 0x00012190
		public bool IsVisible
		{
			get
			{
				return (this.IsService && this.IsAppService) || !this.IsService;
			}
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x060002FF RID: 767 RVA: 0x00013FAD File Offset: 0x000121AD
		public bool HasError
		{
			get
			{
				return this._errorMessage != null;
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000300 RID: 768 RVA: 0x00013FB8 File Offset: 0x000121B8
		public string Name
		{
			get
			{
				return this.Argument.Name;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000301 RID: 769 RVA: 0x00013FC5 File Offset: 0x000121C5
		public string ToolTipText
		{
			get
			{
				if (!this.HasError)
				{
					return this.Argument.ToolTipText;
				}
				return this.ErrorMessage;
			}
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000302 RID: 770 RVA: 0x00013FE1 File Offset: 0x000121E1
		public string ToolTipImageName
		{
			get
			{
				return global::System.IO.Path.Combine(this._imagePath, this.Argument.ToolTipImageName);
			}
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000303 RID: 771 RVA: 0x00013FF9 File Offset: 0x000121F9
		// (set) Token: 0x06000304 RID: 772 RVA: 0x00014008 File Offset: 0x00012208
		public object Value
		{
			get
			{
				return this.Argument.Value;
			}
			set
			{
				if (value != null)
				{
					if (this.Type == typeof(RadioButton) || this.Type == typeof(CheckBox))
					{
						value = Convert.ChangeType(value, typeof(bool));
					}
					else
					{
						value = Convert.ChangeType(value, this.Type);
					}
				}
				this.Argument.Value = value;
			}
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000305 RID: 773 RVA: 0x00014074 File Offset: 0x00012274
		// (set) Token: 0x06000306 RID: 774 RVA: 0x0001407C File Offset: 0x0001227C
		public string ErrorMessage
		{
			get
			{
				return this._errorMessage;
			}
			set
			{
				if (base.SetProperty<string>(ref this._errorMessage, value, "ErrorMessage"))
				{
					this.OnPropertyChanged("HasError");
					this.OnPropertyChanged("ToolTipText");
				}
			}
		}

		// Token: 0x06000307 RID: 775 RVA: 0x000140A8 File Offset: 0x000122A8
		public BaseProcParam(ProcedureParameter parameter, string imagePath, bool isAppService)
		{
			this.Argument = parameter.CreateArgument();
			this.Unit = parameter.Unit;
			this.Group = parameter.Group;
			this.Indent = parameter.Indent;
			this.Decimals = parameter.Decimals;
			this.IsService = parameter.IsService;
			this.IsAppService = isAppService;
			this._imagePath = imagePath;
			if (parameter.ControlType.ToLower() == "radio")
			{
				this.Type = typeof(RadioButton);
			}
			else if (parameter.ControlType.ToLower() == "check")
			{
				this.Type = typeof(CheckBox);
			}
			else if (parameter.ControlType.ToLower() == "separator")
			{
				this.Type = typeof(Line);
			}
			else
			{
				this.Type = parameter.Type;
			}
			if (parameter.ControlType.ToLower() == "radio" || parameter.ControlType.ToLower() == "check")
			{
				this.IsBoolButton = true;
				return;
			}
			if (parameter.ControlType.ToLower() == "separator")
			{
				this.IsSeparator = true;
				return;
			}
			this.IsStandard = true;
		}

		// Token: 0x06000308 RID: 776 RVA: 0x00014200 File Offset: 0x00012400
		public BaseProcParam(ProcedureParameter parameter, ProcedureArgument argument, string imagePath, bool isAppService)
		{
			this.Argument = argument;
			this.Unit = parameter.Unit;
			this.Group = parameter.Group;
			this.Indent = parameter.Indent;
			this.Decimals = parameter.Decimals;
			this.IsService = parameter.IsService;
			this.IsAppService = isAppService;
			this._imagePath = imagePath;
			if (parameter.ControlType.ToLower() == "radio")
			{
				this.Type = typeof(RadioButton);
			}
			else if (parameter.ControlType.ToLower() == "check")
			{
				this.Type = typeof(CheckBox);
			}
			else if (parameter.ControlType.ToLower() == "separator")
			{
				this.Type = typeof(Line);
			}
			else
			{
				this.Type = parameter.Type;
			}
			if (parameter.ControlType.ToLower() == "radio" || parameter.ControlType.ToLower() == "check")
			{
				this.IsBoolButton = true;
				return;
			}
			if (parameter.ControlType.ToLower() == "separator")
			{
				this.IsSeparator = true;
				return;
			}
			this.IsStandard = true;
		}

		// Token: 0x06000309 RID: 777 RVA: 0x00014354 File Offset: 0x00012554
		public BaseProcParam(BaseProcParam item)
		{
			this.Argument = item.Argument;
			this.Unit = item.Unit;
			this.Group = item.Group;
			this.Indent = item.Indent;
			this.Decimals = item.Decimals;
			this.IsBoolButton = item.IsBoolButton;
			this.IsSeparator = item.IsSeparator;
			this.IsStandard = item.IsStandard;
			this.Type = item.Type;
			this.IsService = item.IsService;
			this.IsAppService = item.IsAppService;
		}

		// Token: 0x040001CC RID: 460
		private readonly string _imagePath = "";

		// Token: 0x040001CD RID: 461
		private string _errorMessage;
	}
}
