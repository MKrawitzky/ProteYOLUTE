// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class AdvProcParam : BindableBase
{
	public delegate void ModificationUpdate(bool isModified);

	public delegate void ValidationUpdate(bool isValid);

	private ProcedureArgument _argument;

	private readonly BindableBalticMethod _method;

	private readonly ExperimentInfo _experiment;

	private readonly BalticInstrumentFacade _instrument;

	private readonly List<AdvChildProcParam> _childParams;

	private string _errorMessage;

	private bool _hasChildren;

	private bool _isChildError;

	public ProcedureArgument Argument
	{
		get
		{
			return _argument;
		}
		set
		{
			SetField(ref _argument, value, "Argument");
		}
	}

	public string Unit { get; private set; }

	public Type Type { get; }

	public bool IsService { get; set; }

	public bool IsAppService { get; set; }

	public bool IsVisible
	{
		get
		{
			if (!IsService || !IsAppService)
			{
				return !IsService;
			}
			return true;
		}
	}

	public ICommand ChildSettingsCmd { get; }

	public string ErrorMessage
	{
		get
		{
			return _errorMessage;
		}
		set
		{
			SetField(ref _errorMessage, value, "ErrorMessage");
			OnPropertyChanged("HasError");
		}
	}

	public bool HasChildren
	{
		get
		{
			return _hasChildren;
		}
		set
		{
			SetField(ref _hasChildren, value, "HasChildren");
		}
	}

	public bool IsChildError
	{
		get
		{
			return _isChildError;
		}
		set
		{
			SetField(ref _isChildError, value, "IsChildError");
			OnPropertyChanged("HasError");
		}
	}

	public bool HasError
	{
		get
		{
			if (_errorMessage == null)
			{
				return IsChildError;
			}
			return true;
		}
	}

	public string Name => Argument.Name;

	public object Value
	{
		get
		{
			return Argument.Value;
		}
		set
		{
			if (value != null)
			{
				value = Convert.ChangeType(value, Type);
			}
			Argument.Value = value;
			OnPropertyChanged("Value");
		}
	}

	public event ModificationUpdate ModificationUpdateEvent;

	public event ValidationUpdate ValidationUpdateEvent;

	public AdvProcParam(ProcedureParameter parameter, BindableBalticMethod method, ExperimentInfo experiment, BalticInstrumentFacade instrument, List<AdvChildProcParam> childParams, bool isAppService)
		: this(parameter, parameter.CreateArgument(), method, experiment, instrument, childParams, isAppService)
	{
	}

	public AdvProcParam(ProcedureParameter parameter, ProcedureArgument argument, BindableBalticMethod method, ExperimentInfo experiment, BalticInstrumentFacade instrument, List<AdvChildProcParam> childParams, bool isAppService)
	{
		_argument = argument;
		_method = method;
		_experiment = experiment;
		_instrument = instrument;
		_childParams = childParams;
		Unit = parameter.Unit;
		Type = parameter.Type;
		IsService = parameter.IsService;
		IsAppService = isAppService;
		HasChildren = childParams.Any((AdvChildProcParam x) => x.Header == _argument.Name && x.IsVisible);
		ChildSettingsCmd = new RelayCommand(ChildSettings, CanChildSettings);
		IsChildError = false;
	}

	public bool CanChildSettings(object obj)
	{
		return true;
	}

	public void ChildSettings(object obj)
	{
		Button button = obj as Button;
		AdvancedChildParameterWindow advancedChildParameterWindow = new AdvancedChildParameterWindow(_argument.Name, _method, _instrument, _experiment);
		advancedChildParameterWindow.ValidationUpdateEvent += ChildWindow_ValidationUpdateEvent;
		advancedChildParameterWindow.ModificationUpdateEvent += ChildWindow_ModificationUpdateEvent;
		Point point = button.PointToScreen(new Point(30.0, 0.0));
		List<AdvChildProcParam> list = _childParams.FindAll((AdvChildProcParam x) => x.Header == _argument.Name);
		advancedChildParameterWindow.Left = point.X;
		advancedChildParameterWindow.Top = point.Y - (double)((list?.Count ?? 0) * 30);
		advancedChildParameterWindow.ShowDialog(HelperExtensions.GetActiveWindow());
	}

	private void ChildWindow_ModificationUpdateEvent(bool isModified)
	{
		this.ModificationUpdateEvent?.Invoke(isModified);
	}

	private void ChildWindow_ValidationUpdateEvent(bool isValid)
	{
		this.ValidationUpdateEvent?.Invoke(isValid);
		IsChildError = !isValid;
	}
}
