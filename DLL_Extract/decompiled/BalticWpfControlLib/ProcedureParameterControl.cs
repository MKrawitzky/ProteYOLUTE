// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class ProcedureParameterControl : UserControl, INotifyPropertyChanged, IComponentConnector, IStyleConnector
{
	private readonly List<ProcParam> _parameters = new List<ProcParam>();

	private bool _hasError;

	internal ContainerTemplatableItemsControl ParameterItems;

	private bool _contentLoaded;

	public bool IsAppServiceMode { get; set; }

	public bool IsValid
	{
		get
		{
			return _hasError;
		}
		set
		{
			SetField(ref _hasError, value, "IsValid");
		}
	}

	public event EventHandler ArgumentValueUpdated;

	public event PropertyChangedEventHandler PropertyChanged;

	public ProcedureParameterControl()
		: this(isAppServiceMode: false)
	{
	}

	public ProcedureParameterControl(bool isAppServiceMode)
	{
		InitializeComponent();
		IsAppServiceMode = isAppServiceMode;
	}

	public void SetParameters(IEnumerable<ProcedureParameter> procedureParameters, IEnumerable<ChildProcedureParameter> childProcedureParameters, string imagePath, ProcedureArguments valuePresets = null, ChildProcedureArguments childValuePresets = null)
	{
		_parameters.Clear();
		ChildProcedureParameter[] source = childProcedureParameters.ToArray();
		foreach (ProcedureParameter param in procedureParameters)
		{
			ObservableCollection<ChildProcParam> observableCollection = new ObservableCollection<ChildProcParam>();
			foreach (ChildProcedureParameter item3 in source.Where((ChildProcedureParameter x) => x.Header == param.Name))
			{
				ProcedureArgument procedureArgument = item3.CreateArgument();
				ChildProcParam item = new ChildProcParam(item3.Header, new ProcedureParameter(item3.Name, item3.ControlType, item3.Type, procedureArgument.Value, item3.Unit, item3.IsService, IsAppServiceMode, item3.ToolTipText, item3.ToolTipImageName, item3.Indent, item3.Group), item3.ToolTipImageName, IsAppServiceMode);
				observableCollection.Add(item);
			}
			ProcParam item2 = ((valuePresets != null && valuePresets.Contains(param.Name)) ? new ProcParam(param, valuePresets[param.Name], imagePath, IsAppServiceMode, observableCollection) : new ProcParam(param, imagePath, IsAppServiceMode, observableCollection));
			_parameters.Add(item2);
		}
		ParameterItems.ItemsSource = _parameters;
	}

	public (ProcedureArguments, ChildProcedureArguments) GetParameters()
	{
		ProcedureArguments procedureArguments = new ProcedureArguments();
		ChildProcedureArguments childProcedureArguments = new ChildProcedureArguments();
		if (ParameterItems.ItemsSource is List<ProcParam> list)
		{
			foreach (ProcParam item in list)
			{
				procedureArguments.Add(new ProcedureArgument(item.Name, item.Value, item.Unit, item.IsService, IsAppServiceMode));
				foreach (ChildProcParam childProcParam in item.ChildProcParams)
				{
					childProcedureArguments.Add(new ChildProcedureArgument(childProcParam.Header, childProcParam.Name, childProcParam.Value, childProcParam.Unit, childProcParam.IsService, IsAppServiceMode));
				}
			}
		}
		return (procedureArguments, childProcedureArguments);
	}

	public void ClearErrors()
	{
		foreach (ProcParam parameter in _parameters)
		{
			parameter.ErrorMessage = null;
		}
		IsValid = true;
	}

	public void SetError(string parameter, string error)
	{
		ProcParam procParam = _parameters.FirstOrDefault((ProcParam pp) => pp.Name.Equals(parameter));
		if (procParam != null)
		{
			procParam.ErrorMessage = error;
			IsValid = false;
		}
	}

	public ProcedureArguments CreateArguments()
	{
		ProcedureArguments procedureArguments = new ProcedureArguments();
		foreach (ProcParam parameter in _parameters)
		{
			procedureArguments.Add(parameter.Argument);
		}
		return procedureArguments;
	}

	public static ChildProcedureArguments CreateChildArguments()
	{
		return new ChildProcedureArguments();
	}

	private void ParameterItems_SourceUpdated(object sender, DataTransferEventArgs e)
	{
		this.ArgumentValueUpdated?.Invoke(this, EventArgs.Empty);
	}

	private void RadioButton_Loaded(object sender, RoutedEventArgs e)
	{
		RadioButton rButton = sender as RadioButton;
		if (rButton != null)
		{
			ProcParam procParam = _parameters.FirstOrDefault((ProcParam x) => x.Name == (string)rButton.Content);
			if (procParam != null)
			{
				rButton.GroupName = procParam.Group;
			}
		}
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/procedureparametercontrol.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 1)
		{
			ParameterItems = (ContainerTemplatableItemsControl)target;
		}
		else
		{
			_contentLoaded = true;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 2)
		{
			((RadioButton)target).Loaded += RadioButton_Loaded;
		}
	}
}
