// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace BalticWpfControlLib.Utilities;

public class CircularDisplayBuffer
{
	private readonly Queue<double> _queue;

	private readonly int _size;

	public double Average
	{
		get
		{
			double num = 0.0;
			if (_queue.Count == 0)
			{
				return 0.0;
			}
			for (int i = 0; i < _queue.Count; i++)
			{
				num += _queue.ElementAt(i);
			}
			return num / (double)_queue.Count;
		}
	}

	public int nItems => _queue.Count;

	public CircularDisplayBuffer(int size)
	{
		_queue = new Queue<double>(size);
		_size = size;
	}

	public void Add(double obj)
	{
		if (_queue.Count == _size)
		{
			_queue.Dequeue();
		}
		_queue.Enqueue(obj);
	}
}
