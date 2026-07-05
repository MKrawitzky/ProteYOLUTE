using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BalticWpfControlLib
{
	// Token: 0x0200000F RID: 15
	public class AdvParameterTypeTemplateSelector : DataTemplateSelector
	{
		// Token: 0x0600007C RID: 124 RVA: 0x00003808 File Offset: 0x00001A08
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;
			if (element != null)
			{
				AdvProcParam param = item as AdvProcParam;
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
			return null;
		}

		// Token: 0x0400003A RID: 58
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
	}
}
