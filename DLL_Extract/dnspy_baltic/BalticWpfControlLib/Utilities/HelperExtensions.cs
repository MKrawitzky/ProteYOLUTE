using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace BalticWpfControlLib.Utilities
{
	// Token: 0x0200004B RID: 75
	public static class HelperExtensions
	{
		// Token: 0x0600041A RID: 1050 RVA: 0x000191A4 File Offset: 0x000173A4
		public static bool? ShowDialog(this Window window, IWin32OwnerWindow ownerWindow)
		{
			new WindowInteropHelper(window).Owner = ((ownerWindow != null) ? ownerWindow.Handle : IntPtr.Zero);
			if (ownerWindow != null && ownerWindow.IsMaximized.GetValueOrDefault())
			{
				window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			}
			return window.ShowDialog();
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x000191EC File Offset: 0x000173EC
		public static IWin32OwnerWindow GetActiveWindow()
		{
			return HelperExtensions.GetOwnerWindow(HelperExtensions.Native.GetActiveWindow());
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x000191F8 File Offset: 0x000173F8
		public static IWin32OwnerWindow GetOwnerWindow(IntPtr handle)
		{
			if (handle != IntPtr.Zero)
			{
				Control control = Control.FromChildHandle(handle);
				if (control != null)
				{
					return new HelperExtensions.FromControl(control);
				}
			}
			return new HelperExtensions.FromHandle(handle);
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x00019233 File Offset: 0x00017433
		public static IWin32OwnerWindow GetOwnerWindow(this Control control)
		{
			return new HelperExtensions.FromControl(control);
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00019240 File Offset: 0x00017440
		public static IWin32OwnerWindow GetOwnerWindow(this Process process)
		{
			return HelperExtensions.GetOwnerWindow(process.MainWindowHandle);
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x00019250 File Offset: 0x00017450
		internal static void HideMinimizeMinimizeButton(this Window window)
		{
			IntPtr handle = new WindowInteropHelper(window).Handle;
			int currentStyle = HelperExtensions.Native.GetWindowLong(handle, -16);
			HelperExtensions.Native.SetWindowLong(handle, -16, currentStyle & -131073);
		}

		// Token: 0x02000112 RID: 274
		private static class Native
		{
			// Token: 0x060007EA RID: 2026
			[DllImport("user32.dll", ExactSpelling = true)]
			public static extern IntPtr GetAncestor(IntPtr hwnd, HelperExtensions.Native.GetAncestorFlags flags);

			// Token: 0x060007EB RID: 2027
			[DllImport("user32.dll", ExactSpelling = true)]
			public static extern IntPtr GetActiveWindow();

			// Token: 0x060007EC RID: 2028
			[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
			public static extern bool GetWindowPlacement(IntPtr hWnd, ref HelperExtensions.Native.WINDOWPLACEMENT lpwndpl);

			// Token: 0x060007ED RID: 2029
			[DllImport("user32.dll")]
			public static extern int GetWindowLong(IntPtr hWnd, int index);

			// Token: 0x060007EE RID: 2030
			[DllImport("user32.dll")]
			public static extern int SetWindowLong(IntPtr hWnd, int index, int value);

			// Token: 0x04000435 RID: 1077
			public const int GWL_STYLE = -16;

			// Token: 0x04000436 RID: 1078
			public const int WS_MAXIMIZEBOX = 65536;

			// Token: 0x04000437 RID: 1079
			public const int WS_MINIMIZEBOX = 131072;

			// Token: 0x0200013C RID: 316
			public enum GetAncestorFlags
			{
				// Token: 0x040009D9 RID: 2521
				GetParent = 1,
				// Token: 0x040009DA RID: 2522
				GetRoot,
				// Token: 0x040009DB RID: 2523
				GetRootOwner
			}

			// Token: 0x0200013D RID: 317
			public enum ShowState
			{
				// Token: 0x040009DD RID: 2525
				SW_HIDE,
				// Token: 0x040009DE RID: 2526
				SW_SHOWNORMAL,
				// Token: 0x040009DF RID: 2527
				SW_NORMAL = 1,
				// Token: 0x040009E0 RID: 2528
				SW_SHOWMINIMIZED,
				// Token: 0x040009E1 RID: 2529
				SW_SHOWMAXIMIZED,
				// Token: 0x040009E2 RID: 2530
				SW_MAXIMIZE = 3,
				// Token: 0x040009E3 RID: 2531
				SW_SHOWNOACTIVATE,
				// Token: 0x040009E4 RID: 2532
				SW_SHOW,
				// Token: 0x040009E5 RID: 2533
				SW_MINIMIZE,
				// Token: 0x040009E6 RID: 2534
				SW_SHOWMINNOACTIVE,
				// Token: 0x040009E7 RID: 2535
				SW_SHOWNA,
				// Token: 0x040009E8 RID: 2536
				SW_RESTORE,
				// Token: 0x040009E9 RID: 2537
				SW_SHOWDEFAULT,
				// Token: 0x040009EA RID: 2538
				SW_FORCEMINIMIZE,
				// Token: 0x040009EB RID: 2539
				SW_MAX = 11
			}

			// Token: 0x0200013E RID: 318
			[Serializable]
			public struct RECT
			{
				// Token: 0x060008B1 RID: 2225 RVA: 0x000472E8 File Offset: 0x000454E8
				public RECT(int left, int top, int right, int bottom)
				{
					this.Left = left;
					this.Top = top;
					this.Right = right;
					this.Bottom = bottom;
				}

				// Token: 0x040009EC RID: 2540
				public int Left;

				// Token: 0x040009ED RID: 2541
				public int Top;

				// Token: 0x040009EE RID: 2542
				public int Right;

				// Token: 0x040009EF RID: 2543
				public int Bottom;
			}

			// Token: 0x0200013F RID: 319
			[Serializable]
			public struct POINT
			{
				// Token: 0x060008B2 RID: 2226 RVA: 0x00047307 File Offset: 0x00045507
				public POINT(int x, int y)
				{
					this.X = x;
					this.Y = y;
				}

				// Token: 0x040009F0 RID: 2544
				public int X;

				// Token: 0x040009F1 RID: 2545
				public int Y;
			}

			// Token: 0x02000140 RID: 320
			[Serializable]
			public struct WINDOWPLACEMENT
			{
				// Token: 0x1700019D RID: 413
				// (get) Token: 0x060008B3 RID: 2227 RVA: 0x00047318 File Offset: 0x00045518
				public static HelperExtensions.Native.WINDOWPLACEMENT Default
				{
					get
					{
						HelperExtensions.Native.WINDOWPLACEMENT result = default(HelperExtensions.Native.WINDOWPLACEMENT);
						result.Length = Marshal.SizeOf<HelperExtensions.Native.WINDOWPLACEMENT>(result);
						return result;
					}
				}

				// Token: 0x040009F2 RID: 2546
				public int Length;

				// Token: 0x040009F3 RID: 2547
				public int Flags;

				// Token: 0x040009F4 RID: 2548
				public HelperExtensions.Native.ShowState ShowCmd;

				// Token: 0x040009F5 RID: 2549
				public HelperExtensions.Native.POINT MinPosition;

				// Token: 0x040009F6 RID: 2550
				public HelperExtensions.Native.POINT MaxPosition;

				// Token: 0x040009F7 RID: 2551
				public HelperExtensions.Native.RECT NormalPosition;
			}
		}

		// Token: 0x02000113 RID: 275
		private struct FromControl : IWin32OwnerWindow, global::System.Windows.Interop.IWin32Window
		{
			// Token: 0x1700017A RID: 378
			// (get) Token: 0x060007EF RID: 2031 RVA: 0x0003E2A4 File Offset: 0x0003C4A4
			public IntPtr Handle
			{
				get
				{
					Form form = this._control.FindForm();
					if (form != null)
					{
						return form.Handle;
					}
					return HelperExtensions.Native.GetAncestor(this._control.Handle, HelperExtensions.Native.GetAncestorFlags.GetRoot);
				}
			}

			// Token: 0x1700017B RID: 379
			// (get) Token: 0x060007F0 RID: 2032 RVA: 0x0003E2D8 File Offset: 0x0003C4D8
			public bool? IsMaximized
			{
				get
				{
					Form form = this._control.FindForm();
					if (form != null)
					{
						return new bool?(form.WindowState == FormWindowState.Maximized);
					}
					return null;
				}
				set { }
			}

			// Token: 0x060007F1 RID: 2033 RVA: 0x0003E30C File Offset: 0x0003C50C
			public FromControl(Control control)
			{
				this._control = control;
			}

			// Token: 0x04000438 RID: 1080
			private Control _control;
		}

		// Token: 0x02000114 RID: 276
		private struct FromHandle : IWin32OwnerWindow, global::System.Windows.Interop.IWin32Window
		{
			// Token: 0x1700017C RID: 380
			// (get) Token: 0x060007F2 RID: 2034 RVA: 0x0003E318 File Offset: 0x0003C518
			public bool? IsMaximized
			{
				get
				{
					HelperExtensions.Native.WINDOWPLACEMENT wp = HelperExtensions.Native.WINDOWPLACEMENT.Default;
					if (!HelperExtensions.Native.GetWindowPlacement(this.Handle, ref wp))
					{
						return null;
					}
					if (wp.ShowCmd == HelperExtensions.Native.ShowState.SW_SHOWMAXIMIZED)
					{
						return new bool?(true);
					}
					return new bool?(false);
				}
				set { }
			}

			// Token: 0x1700017D RID: 381
			// (get) Token: 0x060007F3 RID: 2035 RVA: 0x0003E35A File Offset: 0x0003C55A
			public IntPtr Handle { get; }

			// Token: 0x060007F4 RID: 2036 RVA: 0x0003E362 File Offset: 0x0003C562
			public FromHandle(IntPtr handle)
			{
				this.Handle = handle;
			}
		}
	}
}
