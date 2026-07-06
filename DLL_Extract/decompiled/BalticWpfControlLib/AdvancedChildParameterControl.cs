// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Bruker.Lc.Business;

namespace BalticWpfControlLib;

public class AdvancedChildParameterControl : UserControl, IComponentConnector
{
	public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register("HasError", typeof(bool), typeof(AdvancedChildParameterControl), new PropertyMetadata(false));

	private readonly List<AdvChildProcParam> _childParameters = new List<AdvChildProcParam>();

	private readonly ChildProcedureArguments _valuePresets = new ChildProcedureArguments();

	private readonly bool _isAppServiceMode;

	internal ContainerTemplatableItemsControl ParameterItems;

	private bool _contentLoaded;

	public bool HasError => (bool)GetValue(HasErrorProperty);

	public event EventHandler ArgumentValueUpdated;

	public AdvancedChildParameterControl(bool isAppServiceMode)
	{
		InitializeComponent();
		_isAppServiceMode = isAppServiceMode;
	}

	public bool Exists(string name)
	{
		return _childParameters.FirstOrDefault((AdvChildProcParam p) => p.Name == name) != null;
	}

	public void SetParameters(IEnumerable<ChildProcedureParameter> childProcedureParameters, ChildProcedureArguments procArgs, ChildProcedureArguments valuePresets)
	{
		_childParameters.Clear();
		_valuePresets.Clear();
		foreach (ChildProcedureArgument valuePreset in valuePresets)
		{
			_valuePresets.Add(new ChildProcedureArgument(valuePreset));
		}
		foreach (ChildProcedureParameter param in childProcedureParameters)
		{
			AdvChildProcParam advChildProcParam = null;
			if (valuePresets.Find((ChildProcedureArgument x) => x.Header == param.Header && x.ProcArg.Name == param.Name) != null)
			{
				ChildProcedureArgument childProcedureArgument = procArgs.Find((ChildProcedureArgument x) => x.Header == param.Header && x.ProcArg.Name == param.Name);
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
		ParameterItems.ItemsSource = _childParameters;
	}

	public void ResetParameterValues()
	{
		foreach (ChildProcedureArgument param in _valuePresets)
		{
			AdvChildProcParam advChildProcParam = _childParameters.Find((AdvChildProcParam x) => x.Header == param.Header && x.Name == param.ProcArg.Name);
			if (advChildProcParam != null)
			{
				advChildProcParam.Value = param.ProcArg.Value;
			}
		}
	}

	public void ClearErrors()
	{
		foreach (AdvChildProcParam childParameter in _childParameters)
		{
			childParameter.ErrorMessage = null;
		}
		SetValue(HasErrorProperty, false);
	}

	public void SetError(string parameter, string error)
	{
		AdvChildProcParam advChildProcParam = _childParameters.FirstOrDefault((AdvChildProcParam pp) => pp.Name.Equals(parameter));
		if (advChildProcParam != null)
		{
			advChildProcParam.ErrorMessage = error;
			SetValue(HasErrorProperty, true);
		}
	}

	public ChildProcedureArguments CreateChildArguments()
	{
		ChildProcedureArguments childProcedureArguments = new ChildProcedureArguments();
		foreach (AdvChildProcParam childParameter in _childParameters)
		{
			childProcedureArguments.Add(childParameter.ChildArgument);
		}
		return childProcedureArguments;
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
			Uri resourceLocator = new Uri("/BalticWpfControlLib;V3.0.0.0;component/advancedchildparametercontrol.xaml", UriKind.Relative);
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
