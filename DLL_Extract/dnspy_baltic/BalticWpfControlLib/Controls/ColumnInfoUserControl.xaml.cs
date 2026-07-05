using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BalticClassLib;

namespace BalticWpfControlLib.Controls
{
	// Token: 0x02000084 RID: 132
	public partial class ColumnInfoUserControl : UserControl
	{
		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000621 RID: 1569 RVA: 0x0003A7C3 File Offset: 0x000389C3
		// (set) Token: 0x06000622 RID: 1570 RVA: 0x0003A7D5 File Offset: 0x000389D5
		[Description("Gets or sets the ColumnSource.")]
		[Category("ColumnInfoUserControl Properties")]
		public Column ColumnSource
		{
			get
			{
				return (Column)base.GetValue(ColumnInfoUserControl.ColumnSourceProperty);
			}
			set
			{
				base.SetValue(ColumnInfoUserControl.ColumnSourceProperty, value);
			}
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x0003A7E3 File Offset: 0x000389E3
		public ColumnInfoUserControl()
		{
			this.InitializeComponent();
		}

		// Token: 0x04000348 RID: 840
		private const string PROPERTY_CATEGORY = "ColumnInfoUserControl Properties";

		// Token: 0x04000349 RID: 841
		public static readonly DependencyProperty ColumnSourceProperty = DependencyProperty.Register("ColumnSource", typeof(Column), typeof(ColumnInfoUserControl), new FrameworkPropertyMetadata(null));
	}
}
