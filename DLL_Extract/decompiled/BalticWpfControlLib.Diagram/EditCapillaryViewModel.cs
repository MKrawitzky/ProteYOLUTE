using System.Globalization;
using BalticClassLib;
using Syncfusion.Windows.Shared;

namespace BalticWpfControlLib.Diagram;

public class EditCapillaryViewModel : NotificationObject
{
	private double _length;

	private readonly double _defaultLength;

	private double _id;

	private readonly double _defaultID;

	public double Length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
			RaisePropertyChanged("Length");
		}
	}

	public string LengthUnit { get; set; }

	public double MinLength { get; set; }

	public double MaxLength { get; set; }

	public double ID
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
			RaisePropertyChanged("ID");
		}
	}

	public string IDUnit { get; set; }

	public double MinID { get; set; }

	public double MaxID { get; set; }

	public string LengthTitle { get; set; }

	public string InnerDiameterTitle { get; set; }

	public string LengthRangeToolTip { get; set; }

	public string IDRangeToolTip { get; set; }

	public EditCapillaryViewModel(BalticPreferences.CapillaryPreference pref)
	{
		_length = pref.Length;
		LengthUnit = pref.LengthUnit;
		_defaultLength = pref.DefaultLength;
		MinLength = pref.MinLength;
		MaxLength = pref.MaxLength;
		_id = pref.ID;
		IDUnit = ((pref.IDUnit == "um") ? "µm" : pref.IDUnit);
		_defaultID = pref.DefaultID;
		MinID = pref.MinID;
		MaxID = pref.MaxID;
		LengthTitle = pref.LengthTitle;
		InnerDiameterTitle = pref.InnerDiameterTitle;
		LengthRangeToolTip = string.Format(CultureInfo.InvariantCulture, "Valid range is {0} {1} - {2} {3}", MinLength, LengthUnit, MaxLength, LengthUnit);
		IDRangeToolTip = string.Format(CultureInfo.InvariantCulture, "Valid range is {0} {1} - {2} {3}", MinID, IDUnit, MaxID, IDUnit);
	}

	public void Revert()
	{
		Length = _defaultLength;
		ID = _defaultID;
	}
}
