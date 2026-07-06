// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class AdvChildProcParam : BindableBase
{
	private ChildProcedureArgument _childArgument;

	private string _errorMessage;

	public ChildProcedureArgument ChildArgument
	{
		get
		{
			return _childArgument;
		}
		set
		{
			_childArgument = value;
			OnPropertyChanged("ChildArgument");
		}
	}

	public string Header { get; }

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

	public string ErrorMessage
	{
		get
		{
			return _errorMessage;
		}
		set
		{
			if (SetProperty(ref _errorMessage, value, "ErrorMessage"))
			{
				OnPropertyChanged("HasError");
			}
		}
	}

	public bool HasError => _errorMessage != null;

	public string Name => ChildArgument.ProcArg.Name;

	public object Value
	{
		get
		{
			return ChildArgument.ProcArg.Value;
		}
		set
		{
			if (value != null)
			{
				value = Convert.ChangeType(value, Type);
			}
			ChildArgument.ProcArg.Value = value;
			OnPropertyChanged("Value");
		}
	}

	public AdvChildProcParam(ChildProcedureParameter parameter, bool isAppService)
	{
		Header = parameter.Header;
		_childArgument.ProcArg = parameter.CreateArgument();
		Unit = parameter.Unit;
		Type = parameter.Type;
		IsService = parameter.IsService;
		IsAppService = isAppService;
	}

	public AdvChildProcParam(ChildProcedureParameter parameter, ChildProcedureArgument argument, bool isAppService)
	{
		Header = parameter.Header;
		_childArgument = new ChildProcedureArgument(parameter.Header, argument.ProcArg);
		Unit = parameter.Unit;
		Type = parameter.Type;
		IsService = parameter.IsService;
		IsAppService = isAppService;
	}
}
