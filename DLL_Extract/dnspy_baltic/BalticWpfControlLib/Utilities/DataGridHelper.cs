using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000059 RID: 89
	public static class DataGridHelper
	{
		// Token: 0x06000491 RID: 1169 RVA: 0x0001A88C File Offset: 0x00018A8C
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

		// Token: 0x06000492 RID: 1170 RVA: 0x0001A8C8 File Offset: 0x00018AC8
		public static int GetRowIndex(DataGridCell dataGridCell)
		{
			PropertyInfo rowDataItemProperty = dataGridCell.GetType().GetProperty("RowDataItem", BindingFlags.Instance | BindingFlags.NonPublic);
			return DataGridHelper.GetDataGridFromChild(dataGridCell).Items.IndexOf(rowDataItemProperty.GetValue(dataGridCell, null));
		}

		// Token: 0x06000493 RID: 1171 RVA: 0x0001A900 File Offset: 0x00018B00
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
			return DataGridHelper.GetDataGridFromChild(VisualTreeHelper.GetParent(dataGridPart));
		}
	}
}
