using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BalticWpfControlLib;

public class AdvParameterTypeTemplateSelector : DataTemplateSelector
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
		}
	};

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (container is FrameworkElement frameworkElement && item is AdvProcParam advProcParam)
		{
			if (_templateMap.TryGetValue(advProcParam.Type, out var value))
			{
				return frameworkElement.FindResource(value) as DataTemplate;
			}
			return frameworkElement.FindResource("defaultParamTemplate") as DataTemplate;
		}
		return null;
	}
}
