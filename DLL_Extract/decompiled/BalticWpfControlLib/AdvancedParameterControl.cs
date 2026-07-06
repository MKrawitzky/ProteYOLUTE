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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using BalticWpfControlLib.Utilities;
using Bruker.Lc.Baltic;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class AdvancedParameterControl : UserControl, IComponentConnector
{
	public delegate void ModificationUpdate(bool isModified);

	public delegate void ValidationUpdate(bool isValid);

	public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register("HasError", typeof(bool), typeof(AdvancedParameterControl), new PropertyMetadata(false));

	private readonly ProcedureArguments _valuePresets = new ProcedureArguments();

	private readonly List<AdvChildProcParam> _childParameters = new List<AdvChildProcParam>();

	private readonly ChildProcedureArguments _childValuePresets = new ChildProcedureArguments();

	private readonly BindableBalticMethod _method;

	private readonly ExperimentInfo _experiment;

	private readonly BalticInstrumentFacade _instrument;

	private readonly bool _isAppServiceMode;

	internal ContainerTemplatableItemsControl ParameterItems;

	private bool _contentLoaded;

	[Description("Gets if there's an error registered for any parameter.")]
	public bool HasError => (bool)GetValue(HasErrorProperty);

	public ObservableCollection<AdvProcParam> Parameters { get; } = new ObservableCollection<AdvProcParam>();


	public event ModificationUpdate ModificationUpdateEvent;

	public event ValidationUpdate ValidationUpdateEvent;

	public event EventHandler ArgumentValueUpdated;

	public AdvancedParameterControl()
	{
		InitializeComponent();
		base.DataContext = this;
	}

	public AdvancedParameterControl(BindableBalticMethod method, BalticInstrumentFacade instrument, ExperimentInfo experiment, bool isAppServiceMode)
	{
		InitializeComponent();
		_method = method;
		_instrument = instrument;
		_experiment = experiment;
		_isAppServiceMode = isAppServiceMode;
		base.DataContext = this;
	}

	public void SetParameters(IEnumerable<ProcedureParameter> procedureParameters, ProcedureArguments procArgs, IEnumerable<ChildProcedureParameter> childProcedureParameters, ChildProcedureArguments childProcArgs, ProcedureArguments valuePresets, ChildProcedureArguments childValuePresets)
	{
		Parameters.Clear();
		_childParameters.Clear();
		_valuePresets.Clear();
		_childValuePresets.Clear();
		foreach (ProcedureArgument valuePreset in valuePresets)
		{
			_valuePresets.Add(new ProcedureArgument(valuePreset));
		}
		foreach (ChildProcedureArgument childValuePreset in childValuePresets)
		{
			_childValuePresets.Add(new ChildProcedureArgument(childValuePreset));
		}
		foreach (ChildProcedureParameter param in childProcedureParameters)
		{
			AdvChildProcParam advChildProcParam = null;
			if (childValuePresets.FirstOrDefault((ChildProcedureArgument x) => x.Header == param.Header && x.ProcArg.Name == param.Name) != null)
			{
				ChildProcedureArgument childProcedureArgument = childProcArgs.Find((ChildProcedureArgument x) => x.Header == param.Header && x.ProcArg.Name == param.Name);
				if (childProcedureArgument != null)
				{
					advChildProcParam = new AdvChildProcParam(param, childProcedureArgument, _isAppServiceMode);
				}
			}
			if (advChildProcParam == null)
			{
				advChildProcParam = new AdvChildProcParam(param, _isAppServiceMode);
			}
			_childParameters.Add(advChildProcParam);
		}
		foreach (ProcedureParameter procedureParameter in procedureParameters)
		{
			AdvProcParam advProcParam = (valuePresets.Contains(procedureParameter.Name) ? new AdvProcParam(procedureParameter, procArgs[procedureParameter.Name], _method, _experiment, _instrument, _childParameters, _isAppServiceMode) : new AdvProcParam(procedureParameter, _method, _experiment, _instrument, _childParameters, _isAppServiceMode));
			advProcParam.ValidationUpdateEvent += Pp_ValidationUpdateEvent;
			advProcParam.ModificationUpdateEvent += Pp_ModificationUpdateEvent;
			Parameters.Add(advProcParam);
		}
	}

	private void Pp_ModificationUpdateEvent(bool isModified)
	{
		this.ModificationUpdateEvent?.Invoke(isModified);
	}

	private void Pp_ValidationUpdateEvent(bool isValid)
	{
		this.ValidationUpdateEvent?.Invoke(isValid);
	}

	public void ResetParameterValues()
	{
		foreach (ProcedureArgument param in _valuePresets)
		{
			AdvProcParam advProcParam = Parameters.SingleOrDefault((AdvProcParam x) => x.Name == param.Name);
			if (advProcParam != null)
			{
				advProcParam.Value = param.Value;
			}
		}
	}

	public void ClearErrors()
	{
		foreach (AdvProcParam parameter in Parameters)
		{
			parameter.ErrorMessage = null;
			parameter.IsChildError = false;
		}
		SetValue(HasErrorProperty, false);
	}

	public void SetError(string header, string parameter, string error)
	{
		AdvProcParam advProcParam = Parameters.FirstOrDefault((AdvProcParam pp) => pp.Name.Equals(parameter));
		if (advProcParam != null)
		{
			advProcParam.ErrorMessage = error;
			advProcParam.IsChildError = true;
			SetValue(HasErrorProperty, true);
			return;
		}
		advProcParam = Parameters.FirstOrDefault((AdvProcParam pp) => pp.Name.Equals(header));
		if (advProcParam != null)
		{
			advProcParam.ErrorMessage = error;
			advProcParam.IsChildError = true;
			SetValue(HasErrorProperty, true);
		}
	}

	public ProcedureArguments CreateArguments()
	{
		ProcedureArguments procedureArguments = new ProcedureArguments();
		foreach (AdvProcParam parameter in Parameters)
		{
			procedureArguments.Add(parameter.Argument);
		}
		return procedureArguments;
	}

	private void ParameterItems_SourceUpdated(object sender, DataTransferEventArgs e)
	{
		this.ArgumentValueUpdated?.Invoke(this, EventArgs.Empty);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "9.0.7.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/advancedparametercontrol.xaml", UriKind.Relative);
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
}
