// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BalticWpfControlLib;

public class AdvChildParameterTypeTemplateSelector : DataTemplateSelector
{
	private readonly Dictionary<Type, string> templateMap = new Dictionary<Type, string>
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
		}
	};

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (container is FrameworkElement frameworkElement && item is AdvChildProcParam advChildProcParam)
		{
			if (templateMap.TryGetValue(advChildProcParam.Type, out var value))
			{
				frameworkElement.FindResource(value);
				return frameworkElement.FindResource(value) as DataTemplate;
			}
			return frameworkElement.FindResource("defaultParamTemplate") as DataTemplate;
		}
		return null;
	}
}
