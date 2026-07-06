// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

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
