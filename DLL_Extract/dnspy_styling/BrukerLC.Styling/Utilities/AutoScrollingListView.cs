using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrukerLC.Styling.Utilities
{
	// Token: 0x02000004 RID: 4
	public class AutoScrollingListView : ListView
	{
		// Token: 0x06000003 RID: 3 RVA: 0x00002068 File Offset: 0x00000268
		protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
			base.OnItemsSourceChanged(oldValue, newValue);
			if (oldValue is INotifyCollectionChanged)
			{
				(oldValue as INotifyCollectionChanged).CollectionChanged -= this.ItemsCollectionChanged;
			}
			if (!(newValue is INotifyCollectionChanged))
			{
				return;
			}
			(newValue as INotifyCollectionChanged).CollectionChanged += this.ItemsCollectionChanged;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020BC File Offset: 0x000002BC
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this._scrollViewer = AutoScrollingListView.RecursiveVisualChildFinder<ScrollViewer>(this) as ScrollViewer;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000020D8 File Offset: 0x000002D8
		private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this._scrollViewer == null)
			{
				return;
			}
			if (!this._scrollViewer.VerticalOffset.Equals(this._scrollViewer.ScrollableHeight))
			{
				return;
			}
			base.UpdateLayout();
			this._scrollViewer.ScrollToBottom();
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002120 File Offset: 0x00000320
		private static DependencyObject RecursiveVisualChildFinder<T>(DependencyObject rootObject)
		{
			DependencyObject child = VisualTreeHelper.GetChild(rootObject, 0);
			if (child == null)
			{
				return null;
			}
			if (!(child.GetType() == typeof(T)))
			{
				return AutoScrollingListView.RecursiveVisualChildFinder<T>(child);
			}
			return child;
		}

		// Token: 0x04000002 RID: 2
		private ScrollViewer _scrollViewer;
	}
}
