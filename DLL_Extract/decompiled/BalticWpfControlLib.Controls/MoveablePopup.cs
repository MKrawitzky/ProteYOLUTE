using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BalticWpfControlLib.Controls;

public class MoveablePopup : Popup
{
	private Point _initialMousePosition;

	private bool _isDragging;

	protected override void OnInitialized(EventArgs e)
	{
		FrameworkElement obj = base.Child as FrameworkElement;
		obj.MouseLeftButtonDown += Child_MouseLeftButtonDown;
		obj.MouseLeftButtonUp += Child_MouseLeftButtonUp;
		obj.MouseMove += Child_MouseMove;
	}

	private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		FrameworkElement obj = sender as FrameworkElement;
		_initialMousePosition = e.GetPosition(null);
		obj.CaptureMouse();
		_isDragging = true;
		e.Handled = true;
	}

	private void Child_MouseMove(object sender, MouseEventArgs e)
	{
		if (_isDragging)
		{
			Point position = e.GetPosition(null);
			base.HorizontalOffset += position.X - _initialMousePosition.X;
			base.VerticalOffset += position.Y - _initialMousePosition.Y;
		}
	}

	private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (_isDragging)
		{
			(sender as FrameworkElement).ReleaseMouseCapture();
			_isDragging = false;
			e.Handled = true;
		}
	}
}
