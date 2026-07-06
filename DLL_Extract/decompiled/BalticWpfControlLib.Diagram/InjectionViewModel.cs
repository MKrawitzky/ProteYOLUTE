// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

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
