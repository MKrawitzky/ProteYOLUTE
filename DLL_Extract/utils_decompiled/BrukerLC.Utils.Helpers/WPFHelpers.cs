using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace BrukerLC.Utils.Helpers;

public static class WPFHelpers
{
	public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
	{
		return (from t in assembly.GetTypes()
			where string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)
			select t).ToArray();
	}

	public static T FindAncestor<T>(DependencyObject dependencyObject) where T : class
	{
		DependencyObject dependencyObject2 = dependencyObject;
		do
		{
			dependencyObject2 = VisualTreeHelper.GetParent(dependencyObject2);
		}
		while (dependencyObject2 != null && !(dependencyObject2 is T));
		return dependencyObject2 as T;
	}
}
