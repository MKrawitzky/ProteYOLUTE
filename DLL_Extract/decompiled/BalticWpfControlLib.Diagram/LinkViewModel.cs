using System.Windows.Input;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class LinkViewModel : ActivatableAndErrorAwareDataContext, ILinkViewModel
{
	private double _length = 1.0;

	private double _id = 1.0;

	private BalticHWProfile.CapillaryItem _hardwareCapillary;

	private readonly BalticPreferences.CapillaryPreference _pref;

	public string Header { get; set; } = "unknown";


	public double Length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
			OnPropertyChanged("Length");
		}
	}

	public string LengthUnit { get; set; } = "mm";


	public double MinLength { get; set; }

	public double MaxLength { get; set; } = 1000.0;


	public double DefaultLength { get; set; } = 1.0;


	public double ID
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
			OnPropertyChanged("ID");
		}
	}

	public double MinID { get; set; }

	public double MaxID { get; set; } = 1000.0;


	public double DefaultID { get; set; } = 1.0;


	public string IDUnit { get; set; } = "µm";


	public string LengthTitle { get; set; } = "Length:";


	public string InnerDiameterTitle { get; set; } = "Inner Diameter:";


	public bool IsEditable { get; }

	public bool IsPopupVisible { get; }

	public double FactoryLength { get; set; } = 1.0;


	public double FactoryID { get; set; } = 1.0;


	public ICommand EditLinkCommand { get; }

	public bool CanEditLink(object obj)
	{
		return true;
	}

	public void EditLink(object obj)
	{
		string text = obj as string;
		EditCapillaryWindow editCapillaryWindow = new EditCapillaryWindow(_pref);
		editCapillaryWindow.Title = "Edit " + text;
		editCapillaryWindow.Vm.Length = Length;
		editCapillaryWindow.Vm.ID = ID;
		EditCapillaryWindow editCapillaryWindow2 = editCapillaryWindow;
		if (editCapillaryWindow2.ShowDialog(HelperExtensions.GetActiveWindow()).GetValueOrDefault())
		{
			Length = editCapillaryWindow2.Vm.Length;
			ID = editCapillaryWindow2.Vm.ID;
		}
	}

	public LinkViewModel()
	{
	}

	public LinkViewModel(BalticHWProfile.CapillaryItem capillary, BalticPreferences.CapillaryPreference pref, bool isEditable, bool isPopupVisible)
	{
		_hardwareCapillary = capillary;
		IsEditable = isEditable;
		IsPopupVisible = isPopupVisible;
		_pref = pref;
		Header = capillary.Header;
		_length = capillary.Length;
		LengthUnit = ((pref.LengthUnit == "um") ? "µm" : pref.LengthUnit);
		MinLength = pref.MinLength;
		MaxLength = pref.MaxLength;
		DefaultLength = pref.DefaultLength;
		_id = capillary.ID;
		IDUnit = ((pref.IDUnit == "um") ? "µm" : pref.IDUnit);
		MinID = pref.MinID;
		MaxID = pref.MaxID;
		DefaultID = pref.DefaultID;
		LengthTitle = pref.LengthTitle;
		InnerDiameterTitle = pref.InnerDiameterTitle;
		FactoryLength = pref.FactoryLength;
		FactoryID = pref.FactoryID;
		EditLinkCommand = new RelayCommand(EditLink, CanEditLink);
	}

	public void Revert()
	{
		Length = (_pref.DefaultLength = DefaultLength);
		ID = (_pref.DefaultID = DefaultID);
	}

	public void RevertToFactory()
	{
		double length = (DefaultLength = (_pref.DefaultLength = FactoryLength));
		Length = length;
		length = (DefaultID = (_pref.DefaultID = FactoryID));
		ID = length;
	}
}
