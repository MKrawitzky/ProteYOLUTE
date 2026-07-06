// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using BalticClassLib;

namespace BalticWpfControlLib.Utilities;

public class BalticGradientList : ObservableCollection<BalticGradientItem>
{
	public IEnumerable<BalticMethod.GradientItem> ToGradientList()
	{
		List<BalticMethod.GradientItem> list = new List<BalticMethod.GradientItem>();
		using IEnumerator<BalticGradientItem> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			BalticGradientItem current = enumerator.Current;
			list.Add(new BalticMethod.GradientItem(current.Time * 60.0, current.Flow / 1000.0, current.Composition / 100.0));
		}
		return list;
	}
}
