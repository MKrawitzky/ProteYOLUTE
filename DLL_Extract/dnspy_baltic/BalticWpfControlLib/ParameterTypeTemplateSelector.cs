using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace BalticWpfControlLib
{
	// Token: 0x02000034 RID: 52
	public class ParameterTypeTemplateSelector : DataTemplateSelector
	{
		// Token: 0x06000312 RID: 786 RVA: 0x00014520 File Offset: 0x00012720
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;
			if (element != null)
			{
				ProcParam param = item as ProcParam;
				if (param != null)
				{
					string res;
					if (this._templateMap.TryGetValue(param.Type, out res))
					{
						return element.FindResource(res) as DataTemplate;
					}
					return element.FindResource("defaultParamTemplate") as DataTemplate;
				}
			}
			if (element != null)
			{
				ChildProcParam procParam = item as ChildProcParam;
				if (procParam != null)
				{
					string res2;
					if (this._templateMap.TryGetValue(procParam.Type, out res2))
					{
						return element.FindResource(res2) as DataTemplate;
					}
					return element.FindResource("defaultParamTemplate") as DataTemplate;
				}
			}
			return null;
		}

		// Token: 0x040001DB RID: 475
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
	}
}
