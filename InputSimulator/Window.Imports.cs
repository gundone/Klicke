using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using InputSimulator.Struct;

namespace InputSimulator
{
	public static partial class Window
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
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumChildWindows(IntPtr hwndParent, EnumChildWindowProc lpEnumFunc, IntPtr lParam);

		
		[DllImport("user32.dll")]
		private static extern int GetWindowText(this IntPtr hWnd, StringBuilder text, int count);

		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

		[DllImport("user32.dll")]
		private static extern IntPtr WindowFromPoint(POINT Point);

		[DllImport("user32.dll")]
		public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

		[DllImport ("user32.dll")]
		public static extern bool ScreenToClient (IntPtr hWnd, ref Point lpPoint);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName,int nMaxCount);
		
		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


		// more here: http://www.pinvoke.net/default.aspx/user32.showwindow

		[DllImport("user32.dll", EntryPoint = "FindWindow")]
		public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, WM msg, int wParam, StringBuilder lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
		public static extern IntPtr DeleteDC(IntPtr hDc);

		[DllImport("user32.dll", EntryPoint = "ReleaseDC")]
		public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);

		[DllImport("user32.dll")]
		private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

		[DllImport("user32.dll")]
		static extern bool RedrawWindow(IntPtr hWnd, IntPtr  lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);
	}
}
