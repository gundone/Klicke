using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InputSimulator
{
	public class Window
	{
		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto,ExactSpelling=true)]
		public static extern IntPtr SetFocus(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindow(IntPtr hWnd);

		[DllImport("User32.dll")]
		public static extern IntPtr GetTopWindow(IntPtr hWnd);

		[DllImport("User32.dll")]
		public static extern IntPtr GetNextWindow(IntPtr hWnd, uint wCmd);

		[DllImport("user32.dll", SetLastError = false)]
		static extern IntPtr GetDesktopWindow();
		
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref SearchData data);
		
		[DllImport("user32.dll")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

		[DllImport("user32.dll")]
		public static extern IntPtr WindowFromPoint(POINT Point);


		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName,int nMaxCount);

		// more here: http://www.pinvoke.net/default.aspx/user32.showwindow

		[DllImport("user32.dll", EntryPoint = "FindWindow")]
		public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

		private const UInt32 WM_CLOSE          = 0x0010;

		public static void CloseWindow(IntPtr hwnd) {
			SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
		}

		public static List<IntPtr> SearchWindow(string wndclass, string title)
		{
           
			SearchData sd = new SearchData
			{
				Wndclass = wndclass,
				Title = title 
			};

			EnumWindows(EnumProc, ref sd);
#if !DEBUG
                if(resize)
                    sd.hWnd.ForEach(x => WinApi.ResizeWindowSoft(x));
#endif
			return sd.hWnd;
		}

		private static bool EnumProc(IntPtr hWnd, ref SearchData data)
		{
			// Check classname and title 
			// This is different from FindWindow() in that the code below allows partial matches
			StringBuilder sb = new StringBuilder(1024);
			GetClassName(hWnd, sb, sb.Capacity);
			//wndClasses.Add(sb.ToString());
			if (data != null && 
			    sb.ToString() == data.Wndclass 
			    && !string.IsNullOrWhiteSpace(data.Title))
			{
				sb = new StringBuilder(1024);
				GetWindowText(hWnd, sb, sb.Capacity);
				if (sb.ToString().Contains(data.Title))
				{
					data.hWnd.Add(hWnd);
					//return false;    // Found the wnd, halt enumeration
				}
			}
			return true;
		}



		private delegate bool EnumWindowsProc(IntPtr hWnd, ref SearchData data);

		private class SearchData
		{
			// You can put any vars in here...
			public string Wndclass;
			public string Title;
			public List<IntPtr> hWnd = new List<IntPtr>();
		}


		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}

			public static implicit operator System.Drawing.Point(POINT p)
			{
				return new System.Drawing.Point(p.X, p.Y);
			}

			public static implicit operator POINT(System.Drawing.Point p)
			{
				return new POINT(p.X, p.Y);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

	}

	public enum SysCommands
	{
		SC_SIZE = 0xF000,
		SC_MOVE = 0xF010,
		SC_MINIMIZE = 0xF020,
		SC_MAXIMIZE = 0xF030,
		SC_NEXTWINDOW = 0xF040,
		SC_PREVWINDOW = 0xF050,
		SC_CLOSE = 0xF060,
		SC_VSCROLL = 0xF070,
		SC_HSCROLL = 0xF080,
		SC_MOUSEMENU = 0xF090,
		SC_KEYMENU = 0xF100,
		SC_ARRANGE = 0xF110,
		SC_RESTORE = 0xF120,
		SC_TASKLIST = 0xF130,
		SC_SCREENSAVE = 0xF140,
		SC_HOTKEY = 0xF150,
		//#if(WINVER >= 0x0400) //Win95
		SC_DEFAULT = 0xF160,
		SC_MONITORPOWER = 0xF170,
		SC_CONTEXTHELP = 0xF180,
		SC_SEPARATOR = 0xF00F,
		//#endif /* WINVER >= 0x0400 */

		//#if(WINVER >= 0x0600) //Vista
		SCF_ISSECURE = 0x00000001,
		//#endif /* WINVER >= 0x0600 */

		/*
		 * Obsolete names
		 */
		SC_ICON = SC_MINIMIZE,
		SC_ZOOM = SC_MAXIMIZE
	}
	public enum ShowWindowCommands
	{
		/// <summary>
		/// Hides the window and activates another window.
		/// </summary>
		Hide = 0,
		/// <summary>
		/// Activates and displays a window. If the window is minimized or 
		/// maximized, the system restores it to its original size and position.
		/// An application should specify this flag when displaying the window 
		/// for the first time.
		/// </summary>
		Normal = 1,
		/// <summary>
		/// Activates the window and displays it as a minimized window.
		/// </summary>
		ShowMinimized = 2,
		/// <summary>
		/// Maximizes the specified window.
		/// </summary>
		Maximize = 3, // is this the right value?
		/// <summary>
		/// Activates the window and displays it as a maximized window.
		/// </summary>       
		ShowMaximized = 3,
		/// <summary>
		/// Displays a window in its most recent size and position. This value 
		/// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
		/// the window is not activated.
		/// </summary>
		ShowNoActivate = 4,
		/// <summary>
		/// Activates the window and displays it in its current size and position. 
		/// </summary>
		Show = 5,
		/// <summary>
		/// Minimizes the specified window and activates the next top-level 
		/// window in the Z order.
		/// </summary>
		Minimize = 6,
		/// <summary>
		/// Displays the window as a minimized window. This value is similar to
		/// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
		/// window is not activated.
		/// </summary>
		ShowMinNoActive = 7,
		/// <summary>
		/// Displays the window in its current size and position. This value is 
		/// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
		/// window is not activated.
		/// </summary>
		ShowNA = 8,
		/// <summary>
		/// Activates and displays the window. If the window is minimized or 
		/// maximized, the system restores it to its original size and position. 
		/// An application should specify this flag when restoring a minimized window.
		/// </summary>
		Restore = 9,
		/// <summary>
		/// Sets the show state based on the SW_* value specified in the 
		/// STARTUPINFO structure passed to the CreateProcess function by the 
		/// program that started the application.
		/// </summary>
		ShowDefault = 10,
		/// <summary>
		///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
		/// that owns the window is not responding. This flag should only be 
		/// used when minimizing windows from a different thread.
		/// </summary>
		ForceMinimize = 11
	}
	
}
