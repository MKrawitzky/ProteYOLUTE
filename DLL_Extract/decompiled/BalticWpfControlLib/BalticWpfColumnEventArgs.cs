using System;
using BalticClassLib;

namespace BalticWpfControlLib;

public class BalticWpfColumnEventArgs : EventArgs
{
	private Column.ColumnType _columntype;

	private string _preColumnName;

	private string _analColumnName;

	public string PreColumnName
	{
		get
		{
			return _preColumnName;
		}
		set
		{
			_preColumnName = value;
		}
	}

	public string AnalyticalColumnName
	{
		get
		{
			return _analColumnName;
		}
		set
		{
			_analColumnName = value;
		}
	}

	public Column.ColumnType TypeOfColumn
	{
		get
		{
			return _columntype;
		}
		set
		{
			_columntype = value;
		}
	}

	public BalticWpfColumnEventArgs(string precolumnname, string analcolumnname, Column.ColumnType columntype)
	{
		_preColumnName = precolumnname;
		_analColumnName = analcolumnname;
		_columntype = columntype;
	}
}
