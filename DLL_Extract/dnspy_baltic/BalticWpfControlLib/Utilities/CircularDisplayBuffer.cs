using System;
using System.Collections.Generic;
using System.Linq;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000048 RID: 72
	public class CircularDisplayBuffer
	{
		// Token: 0x060003F1 RID: 1009 RVA: 0x00018CAE File Offset: 0x00016EAE
		public CircularDisplayBuffer(int size)
		{
			this._queue = new Queue<double>(size);
			this._size = size;
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x00018CC9 File Offset: 0x00016EC9
		public void Add(double obj)
		{
			if (this._queue.Count == this._size)
			{
				this._queue.Dequeue();
			}
			this._queue.Enqueue(obj);
		}

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060003F3 RID: 1011 RVA: 0x00018CF8 File Offset: 0x00016EF8
		public double Average
		{
			get
			{
				double average = 0.0;
				if (this._queue.Count == 0)
				{
					return 0.0;
				}
				for (int i = 0; i < this._queue.Count; i++)
				{
					average += this._queue.ElementAt(i);
				}
				return average / (double)this._queue.Count;
			}
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x060003F4 RID: 1012 RVA: 0x00018D59 File Offset: 0x00016F59
		public int nItems
		{
			get
			{
				return this._queue.Count;
			}
		}

		// Token: 0x04000260 RID: 608
		private readonly Queue<double> _queue;

		// Token: 0x04000261 RID: 609
		private readonly int _size;
	}
}
