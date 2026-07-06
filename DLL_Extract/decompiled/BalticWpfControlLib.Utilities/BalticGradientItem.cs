// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;

namespace BalticWpfControlLib.Utilities;

public class BalticGradientItem : BindableBase
{
	private double _time;

	private double _duration;

	private double _composition;

	private double _flow;

	private bool _isFirstRow;

	private bool _isLastRow;

	private bool _isIsocratic;

	private string _toolTip;

	private string _paramToolTip;

	private bool _isInTimeOrder = true;

	private bool _isParamValid = true;

	private bool _isLastRowValid = true;

	public bool IsIsocratic
	{
		get
		{
			return _isIsocratic;
		}
		set
		{
			SetField(ref _isIsocratic, value, "IsIsocratic");
		}
	}

	public double Duration
	{
		get
		{
			return _duration;
		}
		set
		{
			SetField(ref _duration, value, "Duration");
		}
	}

	public double Time
	{
		get
		{
			return _time;
		}
		set
		{
			value = Math.Round(value, 2);
			SetField(ref _time, value, "Time");
		}
	}

	public double Composition
	{
		get
		{
			return _composition;
		}
		set
		{
			value = Math.Round(value, 1);
			SetField(ref _composition, value, "Composition");
		}
	}

	public double Flow
	{
		get
		{
			return _flow;
		}
		set
		{
			value = Math.Round(value, 2);
			SetField(ref _flow, value, "Flow");
		}
	}

	public string SmartName { get; set; }

	public bool IsTimeEditable
	{
		get
		{
			return _isFirstRow;
		}
		set
		{
			SetField(ref _isFirstRow, value, "IsTimeEditable");
		}
	}

	public bool IsLastRow
	{
		get
		{
			return _isLastRow;
		}
		set
		{
			if (SetField(ref _isLastRow, value, "IsLastRow"))
			{
				OnPropertyChanged("IsLastRowValid");
			}
		}
	}

	public string ErrorToolTip
	{
		get
		{
			return _toolTip;
		}
		set
		{
			SetField(ref _toolTip, value, "ErrorToolTip");
		}
	}

	public string ParamToolTip
	{
		get
		{
			return _paramToolTip;
		}
		set
		{
			SetField(ref _paramToolTip, value, "ParamToolTip");
		}
	}

	public bool IsInTimeOrder
	{
		get
		{
			return _isInTimeOrder;
		}
		set
		{
			if (SetField(ref _isInTimeOrder, value, "IsInTimeOrder"))
			{
				OnPropertyChanged("IsValidState");
				OnPropertyChanged("IsLastRowValid");
			}
		}
	}

	public bool IsParamValid
	{
		get
		{
			return _isParamValid;
		}
		set
		{
			if (SetField(ref _isParamValid, value, "IsParamValid"))
			{
				OnPropertyChanged("IsValidState");
				OnPropertyChanged("IsLastRowValid");
			}
		}
	}

	public bool IsValidState
	{
		get
		{
			if (_isInTimeOrder)
			{
				return _isParamValid;
			}
			return false;
		}
	}

	public bool IsLastRowValid
	{
		get
		{
			return _isLastRowValid;
		}
		set
		{
			SetField(ref _isLastRowValid, value, "IsLastRowValid");
		}
	}

	public BalticGradientItem(BalticGradientItem sourceObj)
	{
		_time = sourceObj._time;
		_duration = sourceObj._duration;
		_composition = sourceObj._composition;
		_flow = sourceObj._flow;
		_isIsocratic = sourceObj._isIsocratic;
		SmartName = sourceObj.SmartName;
		_isFirstRow = sourceObj._isFirstRow;
		_isLastRow = sourceObj._isLastRow;
		_toolTip = sourceObj._toolTip;
		_paramToolTip = sourceObj._paramToolTip;
		_isInTimeOrder = sourceObj._isInTimeOrder;
		_isParamValid = sourceObj._isParamValid;
		_isLastRowValid = sourceObj._isLastRowValid;
	}

	public BalticGradientItem(double time, double duration, double composition, double flow, bool isIsocratic = false, string smartName = "")
	{
		_time = time;
		_duration = duration;
		_composition = composition;
		_flow = flow;
		_isIsocratic = isIsocratic;
		SmartName = smartName;
	}
}
