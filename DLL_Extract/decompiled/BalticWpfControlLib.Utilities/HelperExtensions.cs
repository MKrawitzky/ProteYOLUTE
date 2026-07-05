using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace BalticWpfControlLib.Utilities;

public static class HelperExtensions
{
	private static class Native
	{
		public enum GetAncestorFlags
		{
			GetParent = 1,
			GetRoot,
			GetRootOwner
		}

		public enum ShowState
		{
			SW_HIDE = 0,
			SW_SHOWNORMAL = 1,
			SW_NORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_SHOWMAXIMIZED = 3,
			SW_MAXIMIZE = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
			SW_FORCEMINIMIZE = 11,
			SW_MAX = 11
		}

		[Serializable]
		public struct RECT
		{
			public int Left;

			public int Top;

			public int Right;

			public int Bottom;

			public RECT(int left, int top, int right, int bottom)
			{
				Left = left;
				Top = top;
				Right = right;
				Bottom = bottom;
			}
		}

		[Serializable]
		public struct POINT
		{
			public int X;

			public int Y;

			public POINT(int x, int y)
			{
				X = x;
				Y = y;
			}
		}

		[Serializable]
		public struct WINDOWPLACEMENT
		{
			public int Length;

			public int Flags;

			public ShowState ShowCmd;

			public POINT MinPosition;

			public POINT MaxPosition;

			public RECT NormalPosition;

			public static WINDOWPLACEMENT Default
			{
				get
				{
					WINDOWPLACEMENT wINDOWPLACEMENT = default(WINDOWPLACEMENT);
					wINDOWPLACEMENT.Length = Marshal.SizeOf(wINDOWPLACEMENT);
					return wINDOWPLACEMENT;
				}
			}
		}

		public const int GWL_STYLE = -16;

		public const int WS_MAXIMIZEBOX = 65536;

		public const int WS_MINIMIZEBOX = 131072;

		[DllImport("user32.dll", ExactSpelling = true)]
		public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

		[DllImport("user32.dll", ExactSpelling = true)]
		public static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hWnd, int index);

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int index, int value);
	}

	private struct FromControl : IWin32OwnerWindow, System.Windows.Interop.IWin32Window
	{
		private Control _control;

		public IntPtr Handle => _control.FindForm()?.Handle ?? Native.GetAncestor(_control.Handle, Native.GetAncestorFlags.GetRoot);

		public bool? IsMaximized
		{
			get
			{
				Form form = _control.FindForm();
				if (form != null)
				{
					return form.WindowState == FormWindowState.Maximized;
				}
				return null;
			}
		}

		public FromControl(Control control)
		{
			_control = control;
		}
	}

	private struct FromHandle : IWin32OwnerWindow, System.Windows.Interop.IWin32Window
	{
		public bool? IsMaximized
		{
			get
			{
				Native.WINDOWPLACEMENT lpwndpl = Native.WINDOWPLACEMENT.Default;
				if (Native.GetWindowPlacement(Handle, ref lpwndpl))
				{
					if (lpwndpl.ShowCmd == Native.ShowState.SW_SHOWMAXIMIZED)
					{
						return true;
					}
					return false;
				}
				return null;
			}
		}

		public IntPtr Handle { get; }

		public FromHandle(IntPtr handle)
		{
			Handle = handle;
		}
	}

	public static bool? ShowDialog(this Window window, IWin32OwnerWindow ownerWindow)
	{
		new WindowInteropHelper(window).Owner = ownerWindow?.Handle ?? IntPtr.Zero;
		if (ownerWindow != null && ownerWindow.IsMaximized.GetValueOrDefault())
		{
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}
		return window.ShowDialog();
	}

	public static IWin32OwnerWindow GetActiveWindow()
	{
		return GetOwnerWindow(Native.GetActiveWindow());
	}

	public static IWin32OwnerWindow GetOwnerWindow(IntPtr handle)
	{
		if (handle != IntPtr.Zero)
		{
			Control control = Control.FromChildHandle(handle);
			if (control != null)
			{
				return new FromControl(control);
			}
		}
		return new FromHandle(handle);
	}

	public static IWin32OwnerWindow GetOwnerWindow(this Control control)
	{
		return new FromControl(control);
	}

	public static IWin32OwnerWindow GetOwnerWindow(this Process process)
	{
		return GetOwnerWindow(process.MainWindowHandle);
	}

	internal static void HideMinimizeMinimizeButton(this Window window)
	{
		IntPtr handle = new WindowInteropHelper(window).Handle;
		int windowLong = Native.GetWindowLong(handle, -16);
		Native.SetWindowLong(handle, -16, windowLong & -131073);
	}
}
