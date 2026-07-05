using System.Windows;
using System.Windows.Controls;

namespace BalticWpfControlLib;

public class ContainerTemplatableItemsControl : ItemsControl
{
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new ContentControl();
	}

	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return false;
	}
}
