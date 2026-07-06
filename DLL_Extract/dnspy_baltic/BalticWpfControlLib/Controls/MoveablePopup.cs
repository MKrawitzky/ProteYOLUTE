// ---------------------------------------------------------------------------
// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.
// https://github.com/MKrawitzky/ProteYOLUTE
// ---------------------------------------------------------------------------

﻿using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BalticWpfControlLib.Controls
{
	// Token: 0x02000088 RID: 136
	public class MoveablePopup : Popup
	{
		// Token: 0x0600065B RID: 1627 RVA: 0x0003B654 File Offset: 0x00039854
		protected override void OnInitialized(EventArgs e)
		{
			FrameworkElement frameworkElement = base.Child as FrameworkElement;
			frameworkElement.MouseLeftButtonDown += this.Child_MouseLeftButtonDown;
			frameworkElement.MouseLeftButtonUp += this.Child_MouseLeftButtonUp;
			frameworkElement.MouseMove += this.Child_MouseMove;
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x0003B6A1 File Offset: 0x000398A1
		private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			UIElement uielement = sender as FrameworkElement;
			this._initialMousePosition = e.GetPosition(null);
			uielement.CaptureMouse();
			this._isDragging = true;
			e.Handled = true;
		}

		// Token: 0x0600065D RID: 1629 RVA: 0x0003B6CC File Offset: 0x000398CC
		private void Child_MouseMove(object sender, MouseEventArgs e)
		{
			if (this._isDragging)
			{
				Point currentPoint = e.GetPosition(null);
				base.HorizontalOffset += currentPoint.X - this._initialMousePosition.X;
				base.VerticalOffset += currentPoint.Y - this._initialMousePosition.Y;
			}
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x0003B729 File Offset: 0x00039929
		private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (this._isDragging)
			{
				(sender as FrameworkElement).ReleaseMouseCapture();
				this._isDragging = false;
				e.Handled = true;
			}
		}

		// Token: 0x0400035D RID: 861
		private Point _initialMousePosition;

		// Token: 0x0400035E RID: 862
		private bool _isDragging;
	}
}
