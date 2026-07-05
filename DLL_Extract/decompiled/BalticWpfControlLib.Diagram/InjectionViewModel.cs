using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class InjectionViewModel : ActivatableAndErrorAwareDataContext, IInjectionViewModel
{
	private string _label;

	public string Label
	{
		get
		{
			return _label;
		}
		set
		{
			SetProperty(ref _label, value, "Label");
		}
	}
}
