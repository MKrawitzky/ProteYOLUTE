// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Collections.ObjectModel;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram
{
	// Token: 0x0200007C RID: 124
	public class TableViewModel : ActivatableAndErrorAwareDataContext, ITableViewModel
	{
		// Token: 0x17000136 RID: 310
		// (get) Token: 0x060005BD RID: 1469 RVA: 0x0003984A File Offset: 0x00037A4A
		// (set) Token: 0x060005BE RID: 1470 RVA: 0x00039852 File Offset: 0x00037A52
		public string Heading
		{
			get
			{
				return this._heading;
			}
			set
			{
				base.SetProperty<string>(ref this._heading, value, "Heading");
			}
		}

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x060005BF RID: 1471 RVA: 0x00039867 File Offset: 0x00037A67
		// (set) Token: 0x060005C0 RID: 1472 RVA: 0x0003986F File Offset: 0x00037A6F
		public bool IsTableVisible
		{
			get
			{
				return this._isTableVisible;
			}
			set
			{
				base.SetProperty<bool>(ref this._isTableVisible, value, "IsTableVisible");
			}
		}

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x060005C1 RID: 1473 RVA: 0x00039884 File Offset: 0x00037A84
		// (set) Token: 0x060005C2 RID: 1474 RVA: 0x0003988C File Offset: 0x00037A8C
		public ObservableCollection<TableItem> TableItems
		{
			get
			{
				return this._tableItems;
			}
			set
			{
				base.SetProperty<ObservableCollection<TableItem>>(ref this._tableItems, value, "TableItems");
			}
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x000398A1 File Offset: 0x00037AA1
		public TableViewModel(string heading)
		{
			this._heading = heading;
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x000398BB File Offset: 0x00037ABB
		public void Reset()
		{
			this.TableItems.Clear();
		}

		// Token: 0x0400030D RID: 781
		private string _heading;

		// Token: 0x0400030E RID: 782
		private bool _isTableVisible;

		// Token: 0x0400030F RID: 783
		private ObservableCollection<TableItem> _tableItems = new ObservableCollection<TableItem>();
	}
}
