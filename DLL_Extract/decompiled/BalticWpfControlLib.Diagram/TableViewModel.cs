using System.Collections.ObjectModel;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class TableViewModel : ActivatableAndErrorAwareDataContext, ITableViewModel
{
	private string _heading;

	private bool _isTableVisible;

	private ObservableCollection<TableItem> _tableItems = new ObservableCollection<TableItem>();

	public string Heading
	{
		get
		{
			return _heading;
		}
		set
		{
			SetProperty(ref _heading, value, "Heading");
		}
	}

	public bool IsTableVisible
	{
		get
		{
			return _isTableVisible;
		}
		set
		{
			SetProperty(ref _isTableVisible, value, "IsTableVisible");
		}
	}

	public ObservableCollection<TableItem> TableItems
	{
		get
		{
			return _tableItems;
		}
		set
		{
			SetProperty(ref _tableItems, value, "TableItems");
		}
	}

	public TableViewModel(string heading)
	{
		_heading = heading;
	}

	public void Reset()
	{
		TableItems.Clear();
	}
}
