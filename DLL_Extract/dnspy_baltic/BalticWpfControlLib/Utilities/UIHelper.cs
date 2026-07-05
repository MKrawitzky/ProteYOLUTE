using System;
using System.Drawing;
using System.Windows.Forms;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x02000055 RID: 85
	internal static class UIHelper
	{
		// Token: 0x0600048A RID: 1162 RVA: 0x0001A6B4 File Offset: 0x000188B4
		public static Rectangle CalcOnScreenBounds(Rectangle rect)
		{
			Rectangle workingArea = Screen.FromRectangle(rect).WorkingArea;
			return UIHelper.CalcOnScreenBounds(rect, workingArea);
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x0001A6D4 File Offset: 0x000188D4
		public static Rectangle CalcOnScreenBounds(Rectangle rect, Rectangle workingArea)
		{
			if (rect.Width > workingArea.Width)
			{
				rect.Width = workingArea.Width;
			}
			if (rect.Height > workingArea.Height)
			{
				rect.Height = workingArea.Height;
			}
			if (rect.X < workingArea.Left)
			{
				rect.X = workingArea.Left;
			}
			if (rect.Y < workingArea.Top)
			{
				rect.Y = workingArea.Top;
			}
			if (rect.Right >= workingArea.Right)
			{
				rect.X = Math.Max(workingArea.Left, workingArea.Right - rect.Width);
			}
			if (rect.Bottom >= workingArea.Bottom)
			{
				rect.Y = Math.Max(workingArea.Top, workingArea.Bottom - rect.Height);
			}
			return rect;
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x0001A7C0 File Offset: 0x000189C0
		public static Rectangle CalcOnScreenBounds(Rectangle rect, Size minimumSize)
		{
			if (rect.Width < minimumSize.Width)
			{
				rect.Width = minimumSize.Width;
			}
			if (rect.Height < minimumSize.Height)
			{
				rect.Height = minimumSize.Height;
			}
			return UIHelper.CalcOnScreenBounds(rect);
		}
	}
}
