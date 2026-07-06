// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class ConnectionViewModel : ActivatableDataContext, IConnectionViewModel
{
	private int _firstPort;

	private int _secondPort;

	public int FirstPort
	{
		get
		{
			return _firstPort;
		}
		set
		{
			SetProperty(ref _firstPort, value, "FirstPort");
		}
	}

	public int SecondPort
	{
		get
		{
			return _secondPort;
		}
		set
		{
			SetProperty(ref _secondPort, value, "SecondPort");
		}
	}
}
