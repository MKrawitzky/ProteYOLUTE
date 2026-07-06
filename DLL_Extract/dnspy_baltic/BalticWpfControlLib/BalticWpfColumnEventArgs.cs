// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using BalticClassLib;

namespace BalticWpfControlLib
{
	// Token: 0x0200002A RID: 42
	public class BalticWpfColumnEventArgs : EventArgs
	{
		// Token: 0x0600025D RID: 605 RVA: 0x0000FE39 File Offset: 0x0000E039
		public BalticWpfColumnEventArgs(string precolumnname, string analcolumnname, Column.ColumnType columntype)
		{
			this._preColumnName = precolumnname;
			this._analColumnName = analcolumnname;
			this._columntype = columntype;
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x0600025E RID: 606 RVA: 0x0000FE56 File Offset: 0x0000E056
		// (set) Token: 0x0600025F RID: 607 RVA: 0x0000FE5E File Offset: 0x0000E05E
		public string PreColumnName
		{
			get
			{
				return this._preColumnName;
			}
			set
			{
				this._preColumnName = value;
			}
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000260 RID: 608 RVA: 0x0000FE67 File Offset: 0x0000E067
		// (set) Token: 0x06000261 RID: 609 RVA: 0x0000FE6F File Offset: 0x0000E06F
		public string AnalyticalColumnName
		{
			get
			{
				return this._analColumnName;
			}
			set
			{
				this._analColumnName = value;
			}
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000262 RID: 610 RVA: 0x0000FE78 File Offset: 0x0000E078
		// (set) Token: 0x06000263 RID: 611 RVA: 0x0000FE80 File Offset: 0x0000E080
		public Column.ColumnType TypeOfColumn
		{
			get
			{
				return this._columntype;
			}
			set
			{
				this._columntype = value;
			}
		}

		// Token: 0x04000170 RID: 368
		private Column.ColumnType _columntype;

		// Token: 0x04000171 RID: 369
		private string _preColumnName;

		// Token: 0x04000172 RID: 370
		private string _analColumnName;
	}
}
