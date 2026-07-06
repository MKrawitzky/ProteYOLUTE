// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BalticWpfControlLib.Utilities;

public static class DataGridHelper
{
	public static DataGridCell GetCell(DataGridCellInfo dataGridCellInfo)
	{
		if (!dataGridCellInfo.IsValid)
		{
			return null;
		}
		FrameworkElement cellContent = dataGridCellInfo.Column.GetCellContent(dataGridCellInfo.Item);
		if (cellContent != null)
		{
			return (DataGridCell)cellContent.Parent;
		}
		return null;
	}

	public static int GetRowIndex(DataGridCell dataGridCell)
	{
		PropertyInfo property = dataGridCell.GetType().GetProperty("RowDataItem", BindingFlags.Instance | BindingFlags.NonPublic);
		return GetDataGridFromChild(dataGridCell).Items.IndexOf(property.GetValue(dataGridCell, null));
	}

	public static DataGrid GetDataGridFromChild(DependencyObject dataGridPart)
	{
		if (VisualTreeHelper.GetParent(dataGridPart) == null)
		{
			throw new NullReferenceException("Control is null.");
		}
		if (VisualTreeHelper.GetParent(dataGridPart) is DataGrid)
		{
			return (DataGrid)VisualTreeHelper.GetParent(dataGridPart);
		}
		return GetDataGridFromChild(VisualTreeHelper.GetParent(dataGridPart));
	}
}
