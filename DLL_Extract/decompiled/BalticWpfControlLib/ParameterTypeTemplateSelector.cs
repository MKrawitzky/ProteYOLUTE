// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace BalticWpfControlLib;

public class ParameterTypeTemplateSelector : DataTemplateSelector
{
	private readonly Dictionary<Type, string> _templateMap = new Dictionary<Type, string>
	{
		{
			typeof(int),
			"intParamTemplate"
		},
		{
			typeof(bool),
			"boolParamTemplate"
		},
		{
			typeof(double),
			"doubleParamTemplate"
		},
		{
			typeof(string),
			"stringParamTemplate"
		},
		{
			typeof(RadioButton),
			"radioParamTemplate"
		},
		{
			typeof(CheckBox),
			"checkParamTemplate"
		},
		{
			typeof(Line),
			"separatorParamTemplate"
		},
		{
			typeof(object),
			"customParamTemplate"
		}
	};

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		FrameworkElement frameworkElement = container as FrameworkElement;
		if (frameworkElement != null && item is ProcParam procParam)
		{
			if (_templateMap.TryGetValue(procParam.Type, out var value))
			{
				return frameworkElement.FindResource(value) as DataTemplate;
			}
			return frameworkElement.FindResource("defaultParamTemplate") as DataTemplate;
		}
		if (frameworkElement != null && item is ChildProcParam childProcParam)
		{
			if (_templateMap.TryGetValue(childProcParam.Type, out var value2))
			{
				return frameworkElement.FindResource(value2) as DataTemplate;
			}
			return frameworkElement.FindResource("defaultParamTemplate") as DataTemplate;
		}
		return null;
	}
}
