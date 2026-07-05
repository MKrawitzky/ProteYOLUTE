using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Shapes;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

internal class BaseProcParam : BindableBase
{
	private readonly string _imagePath = "";

	private string _errorMessage;

	public ProcedureArgument Argument { get; }

	public string Unit { get; }

	public Type Type { get; }

	public string Group { get; }

	public bool IsBoolButton { get; set; }

	public bool IsStandard { get; set; }

	public bool IsSeparator { get; set; }

	public int Indent { get; }

	public int Decimals { get; }

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

	public bool HasError => _errorMessage != null;

	public string Name => Argument.Name;

	public string ToolTipText
	{
		get
		{
			if (!HasError)
			{
				return Argument.ToolTipText;
			}
			return ErrorMessage;
		}
	}

	public string ToolTipImageName => System.IO.Path.Combine(_imagePath, Argument.ToolTipImageName);

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
				value = ((!(Type == typeof(RadioButton)) && !(Type == typeof(CheckBox))) ? Convert.ChangeType(value, Type) : Convert.ChangeType(value, typeof(bool)));
			}
			Argument.Value = value;
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
				OnPropertyChanged("ToolTipText");
			}
		}
	}

	public BaseProcParam(ProcedureParameter parameter, string imagePath, bool isAppService)
	{
		Argument = parameter.CreateArgument();
		Unit = parameter.Unit;
		Group = parameter.Group;
		Indent = parameter.Indent;
		Decimals = parameter.Decimals;
		IsService = parameter.IsService;
		IsAppService = isAppService;
		_imagePath = imagePath;
		if (parameter.ControlType.ToLower() == "radio")
		{
			Type = typeof(RadioButton);
		}
		else if (parameter.ControlType.ToLower() == "check")
		{
			Type = typeof(CheckBox);
		}
		else if (parameter.ControlType.ToLower() == "separator")
		{
			Type = typeof(Line);
		}
		else
		{
			Type = parameter.Type;
		}
		if (parameter.ControlType.ToLower() == "radio" || parameter.ControlType.ToLower() == "check")
		{
			IsBoolButton = true;
		}
		else if (parameter.ControlType.ToLower() == "separator")
		{
			IsSeparator = true;
		}
		else
		{
			IsStandard = true;
		}
	}

	public BaseProcParam(ProcedureParameter parameter, ProcedureArgument argument, string imagePath, bool isAppService)
	{
		Argument = argument;
		Unit = parameter.Unit;
		Group = parameter.Group;
		Indent = parameter.Indent;
		Decimals = parameter.Decimals;
		IsService = parameter.IsService;
		IsAppService = isAppService;
		_imagePath = imagePath;
		if (parameter.ControlType.ToLower() == "radio")
		{
			Type = typeof(RadioButton);
		}
		else if (parameter.ControlType.ToLower() == "check")
		{
			Type = typeof(CheckBox);
		}
		else if (parameter.ControlType.ToLower() == "separator")
		{
			Type = typeof(Line);
		}
		else
		{
			Type = parameter.Type;
		}
		if (parameter.ControlType.ToLower() == "radio" || parameter.ControlType.ToLower() == "check")
		{
			IsBoolButton = true;
		}
		else if (parameter.ControlType.ToLower() == "separator")
		{
			IsSeparator = true;
		}
		else
		{
			IsStandard = true;
		}
	}

	public BaseProcParam(BaseProcParam item)
	{
		Argument = item.Argument;
		Unit = item.Unit;
		Group = item.Group;
		Indent = item.Indent;
		Decimals = item.Decimals;
		IsBoolButton = item.IsBoolButton;
		IsSeparator = item.IsSeparator;
		IsStandard = item.IsStandard;
		Type = item.Type;
		IsService = item.IsService;
		IsAppService = item.IsAppService;
	}
}
