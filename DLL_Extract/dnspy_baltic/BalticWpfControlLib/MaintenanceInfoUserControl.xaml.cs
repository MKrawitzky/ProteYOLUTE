using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Bruker.Lc.Maintenance;

namespace BalticWpfControlLib
{
	// Token: 0x02000028 RID: 40
	public partial class MaintenanceInfoUserControl : UserControl
	{
		// Token: 0x060001E3 RID: 483 RVA: 0x0000CD2E File Offset: 0x0000AF2E
		public MaintenanceInfoUserControl()
		{
			this.InitializeComponent();
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x0000CD3C File Offset: 0x0000AF3C
		private void ResetBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			((IResetable)e.Parameter).Reset();
		}

		// Token: 0x04000112 RID: 274
		public static RoutedCommand ResetCommand = new RoutedCommand();
	}
}
