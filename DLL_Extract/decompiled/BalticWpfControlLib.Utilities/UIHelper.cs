using System;
using System.Drawing;
using System.Windows.Forms;

namespace BalticWpfControlLib.Utilities;

internal static class UIHelper
{
	public static Rectangle CalcOnScreenBounds(Rectangle rect)
	{
		Rectangle workingArea = Screen.FromRectangle(rect).WorkingArea;
		return CalcOnScreenBounds(rect, workingArea);
	}

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
		return CalcOnScreenBounds(rect);
	}
}
