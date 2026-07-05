using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrukerLC.Styling.Utilities;

public class AutoScrollingListView : ListView
{
	private ScrollViewer _scrollViewer;

	protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
	{
		base.OnItemsSourceChanged(oldValue, newValue);
		if (oldValue is INotifyCollectionChanged)
		{
			(oldValue as INotifyCollectionChanged).CollectionChanged -= ItemsCollectionChanged;
		}
		if (newValue is INotifyCollectionChanged)
		{
			(newValue as INotifyCollectionChanged).CollectionChanged += ItemsCollectionChanged;
		}
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		_scrollViewer = RecursiveVisualChildFinder<ScrollViewer>(this) as ScrollViewer;
	}

	private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (_scrollViewer != null && _scrollViewer.VerticalOffset.Equals(_scrollViewer.ScrollableHeight))
		{
			UpdateLayout();
			_scrollViewer.ScrollToBottom();
		}
	}

	private static DependencyObject RecursiveVisualChildFinder<T>(DependencyObject rootObject)
	{
		DependencyObject child = VisualTreeHelper.GetChild(rootObject, 0);
		if (child == null)
		{
			return null;
		}
		if (!(child.GetType() == typeof(T)))
		{
			return RecursiveVisualChildFinder<T>(child);
		}
		return child;
	}
}
