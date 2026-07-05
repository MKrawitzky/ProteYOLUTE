using System.Windows.Input;
using BalticClassLib;
using BalticWpfControlLib.Utilities;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class LoopViewModel : ActivatableAndErrorAwareDataContext, ILoopViewModel
{
	private double _length;

	private double _id;

	private readonly BalticHWProfile.CapillaryItem _hardwareCapillary;

	private readonly BalticPreferences.CapillaryPreference _pref;

	public string Header { get; set; }

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

	public string LengthUnit { get; set; }

	public double MinLength { get; set; }

	public double MaxLength { get; set; }

	public double DefaultLength { get; set; }

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

	public double MaxID { get; set; }

	public double DefaultID { get; set; }

	public string IDUnit { get; set; }

	public bool IsEditable { get; }

	public bool IsPopupVisible { get; }

	public double FactoryLength { get; set; }

	public double FactoryID { get; set; }

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
		editCapillaryWindow.Vm.LengthTitle = _pref.LengthTitle;
		editCapillaryWindow.Vm.InnerDiameterTitle = _pref.InnerDiameterTitle;
		editCapillaryWindow.Vm.Length = _hardwareCapillary.Length;
		editCapillaryWindow.Vm.ID = _hardwareCapillary.ID;
		if (editCapillaryWindow.ShowDialog(HelperExtensions.GetActiveWindow()).GetValueOrDefault())
		{
			double length2 = (_hardwareCapillary.Length = editCapillaryWindow.Vm.Length);
			Length = length2;
			length2 = (_hardwareCapillary.ID = editCapillaryWindow.Vm.ID);
			ID = length2;
		}
	}

	public LoopViewModel(BalticHWProfile.CapillaryItem capillary, BalticPreferences.CapillaryPreference pref, bool isEditable)
	{
		_hardwareCapillary = capillary;
		IsEditable = isEditable;
		_pref = pref;
		Header = capillary.Header;
		_length = capillary.Length;
		LengthUnit = ((pref.LengthUnit == "uL") ? "µm" : pref.LengthUnit);
		MinLength = pref.MinLength;
		MaxLength = pref.MaxLength;
		DefaultLength = pref.DefaultLength;
		_id = capillary.ID;
		IDUnit = ((pref.IDUnit == "uL") ? "µL" : pref.IDUnit);
		MinID = pref.MinID;
		MaxID = pref.MaxID;
		DefaultID = pref.DefaultID;
		FactoryLength = pref.FactoryLength;
		FactoryID = pref.FactoryID;
		EditLinkCommand = new RelayCommand(EditLink, CanEditLink);
	}

	public void Revert()
	{
		Length = DefaultLength;
		ID = DefaultID;
	}

	public void RevertToFactory()
	{
		Length = FactoryLength;
		ID = FactoryID;
	}
}
