// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class SensorViewModel : ActivatableAndErrorAwareDataContext, ISensorViewModel
{
	private double _throughput;

	private string _throughputUnit;

	public double Throughput
	{
		get
		{
			return _throughput;
		}
		set
		{
			SetProperty(ref _throughput, value, "Throughput");
		}
	}

	public string ThroughputUnit
	{
		get
		{
			return _throughputUnit;
		}
		set
		{
			SetProperty(ref _throughputUnit, value, "ThroughputUnit");
		}
	}
}
