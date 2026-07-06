// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System.Windows.Media;
using BalticWpfControlLib.Utilities;
using BrukerLC.Interfaces.ViewModelInterfaces;

namespace BalticWpfControlLib.Diagram;

public class ActivatableDataContext : BindableBase, IActivatable, ISignalizeAware, ISignalizeTextAware, IVisibleEnableable
{
	private bool _isActive;

	private Brush _activeBrush;

	private bool _isSignalize;

	private Brush _signalizeBrush;

	private const bool _isSignalizeText = false;

	private Brush _signalizeTextBrush;

	private bool _isVisible;

	private bool _isNearTransparent;

	public bool IsActive
	{
		get
		{
			if (_isActive)
			{
				return !_isSignalize;
			}
			return false;
		}
		set
		{
			SetProperty(ref _isActive, value, "IsActive");
		}
	}

	public Brush ActiveBrush
	{
		get
		{
			return _activeBrush;
		}
		set
		{
			SetProperty(ref _activeBrush, value, "ActiveBrush");
		}
	}

	public bool IsSignalize
	{
		get
		{
			return _isSignalize;
		}
		set
		{
			_isSignalize = value;
			OnPropertyChanged("IsSignalize");
			OnPropertyChanged("IsActive");
		}
	}

	public Brush SignalizeBrush
	{
		get
		{
			return _signalizeBrush;
		}
		set
		{
			SetProperty(ref _signalizeBrush, value, "SignalizeBrush");
		}
	}

	public bool IsSignalizeText
	{
		get
		{
			return false;
		}
		set
		{
			_isSignalize = value;
			OnPropertyChanged("IsSignalizeText");
			OnPropertyChanged("IsActive");
		}
	}

	public Brush SignalizeTextBrush
	{
		get
		{
			return _signalizeTextBrush;
		}
		set
		{
			SetProperty(ref _signalizeTextBrush, value, "SignalizeTextBrush");
		}
	}

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			_isVisible = value;
			OnPropertyChanged("IsVisible");
		}
	}

	public bool IsNearTransparent
	{
		get
		{
			return _isNearTransparent;
		}
		set
		{
			_isNearTransparent = value;
			OnPropertyChanged("IsNearTransparent");
		}
	}

	public ActivatableDataContext()
	{
	}

	public ActivatableDataContext(Brush activeBrush)
	{
		_activeBrush = activeBrush;
	}
}
