using System;
using System.Windows;
using System.Windows.Media;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000057 RID: 87
	public static class WPFHelper
	{
		// Token: 0x06000490 RID: 1168 RVA: 0x0001A830 File Offset: 0x00018A30
		public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				if (child != null)
				{
					T dependencyObject = child as T;
					if (dependencyObject != null)
					{
						return dependencyObject;
					}
				}
				T childOfChild = WPFHelper.FindVisualChild<T>(child);
				if (childOfChild != null)
				{
					return childOfChild;
				}
			}
			return default(T);
		}
	}
}
