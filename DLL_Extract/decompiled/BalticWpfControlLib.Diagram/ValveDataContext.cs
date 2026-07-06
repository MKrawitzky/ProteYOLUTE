// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Windows;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class ValveDataContext : ActivatableAndErrorAwareDataContext, IValveViewModel
{
	private ObservableCollection<IConnectionViewModel> _connections = new ObservableCollection<IConnectionViewModel>();

	private double _angle;

	private double _actualAngle;

	private Thickness _angleMargin;

	private bool _isService;

	public ObservableCollection<IConnectionViewModel> Connections
	{
		get
		{
			return _connections;
		}
		set
		{
			SetProperty(ref _connections, value, "Connections");
		}
	}

	public double Angle
	{
		get
		{
			return _angle;
		}
		set
		{
			SetProperty(ref _angle, value, "Angle");
		}
	}

	public double ActualAngle
	{
		get
		{
			return _actualAngle;
		}
		set
		{
			SetProperty(ref _actualAngle, value, "ActualAngle");
		}
	}

	public Thickness AngleMargin
	{
		get
		{
			return _angleMargin;
		}
		set
		{
			SetProperty(ref _angleMargin, value, "AngleMargin");
		}
	}

	public bool IsService
	{
		get
		{
			return _isService;
		}
		set
		{
			SetProperty(ref _isService, value, "IsService");
		}
	}
}
