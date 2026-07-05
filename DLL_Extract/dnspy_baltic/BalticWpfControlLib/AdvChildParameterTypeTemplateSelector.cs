using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BalticWpfControlLib
{
	// Token: 0x02000009 RID: 9
	public class AdvChildParameterTypeTemplateSelector : DataTemplateSelector
	{
		// Token: 0x06000032 RID: 50 RVA: 0x00002910 File Offset: 0x00000B10
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;
			if (element != null)
			{
				AdvChildProcParam param = item as AdvChildProcParam;
				if (param != null)
				{
					string res;
					if (this.templateMap.TryGetValue(param.Type, out res))
					{
						element.FindResource(res);
						return element.FindResource(res) as DataTemplate;
					}
					return element.FindResource("defaultParamTemplate") as DataTemplate;
				}
			}
			return null;
		}

		// Token: 0x04000017 RID: 23
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
	}
}
