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
		public static IntPtr WindowFromPoint(int x, int y)
		{
			return WindowFromPoint(new Point(x, y));
		}

		public static IntPtr WindowFromPoint(Point pt)
		{
			return WindowFromPoint(new POINT(pt));
		}

		public static void CloseWindow(IntPtr hwnd) {
			SendMessage(hwnd, WM.CLOSE, IntPtr.Zero, IntPtr.Zero);
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
			return sd.HWnd;
		}

		public static List<IntPtr> GetChildWindows(this IntPtr parent, bool recursive = false)
		{
			List<IntPtr> result = new List<IntPtr>();
			GCHandle listHandle = GCHandle.Alloc(result);
			try
			{
				EnumChildWindowProc childProc = new EnumChildWindowProc(EnumWindow);
				EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
			}
			finally
			{
				if (listHandle.IsAllocated)
					listHandle.Free();
			}
			if(!recursive)
				return result;
			var children = result.SelectMany(x => x.GetChildWindows(true)).ToList();
			if (children.Count == 0)
				return result;
			result.AddRange(children);
			var hs = new HashSet<IntPtr>(result);
			return hs.ToList();
		}

		public delegate bool EnumChildWindowProc(IntPtr hWnd, IntPtr parameter);

		/// <summary>
		/// Callback method to be used when enumerating windows.
		/// </summary>
		/// <param name="handle">Handle of the next window</param>
		/// <param name="pointer">Pointer to a GCHandle that holds a reference to the list to fill</param>
		/// <returns>True to continue the enumeration, false to bail</returns>
		private static bool EnumWindow(IntPtr handle, IntPtr pointer)
		{
			GCHandle gch = GCHandle.FromIntPtr(pointer);
			if (!(gch.Target is List<IntPtr> list))
			{
				throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
			}
			list.Add(handle);
			//  You can modify this to check to see if you want to cancel the operation, then return a null here
			return true;
		}

		public static string GetWindowText(this IntPtr hWnd)
		{
			IntPtr Handle = Marshal.AllocHGlobal(100);
			int NumText = (int)SendMessage(hWnd, WM.GETTEXT, (IntPtr)50, Handle);
			string Text = Marshal.PtrToStringUni(Handle);
			IntPtr ChildHandle = FindWindowEx(hWnd, IntPtr.Zero, "Edit", null);
			//// Send The WM_GETTEXT Message
			if (ChildHandle != IntPtr.Zero)
			{
				IntPtr hndl = Marshal.AllocHGlobal(200);
				SendMessage(ChildHandle, WM.GETTEXT, (IntPtr)200, hndl);
				Text = Marshal.PtrToStringUni(hndl);
			}
			
			
			return Text;
		}

		public static string GetWindowClass(this IntPtr hWnd)
		{
			StringBuilder sb = new StringBuilder(256);
			//hWnd.GetWindowText(sb, sb.Capacity);
			//if(sb.Length == 0 || string.IsNullOrWhiteSpace(sb.ToString()))
			GetClassName(hWnd, sb, sb.Capacity);
			return sb.ToString();
		}

		public static void RefreshWindow(this IntPtr hWnd)
		{
			RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero,  RedrawWindowFlags.Frame | RedrawWindowFlags.UpdateNow | RedrawWindowFlags.Invalidate);
		}

		public static void DrawRectangle(this IntPtr wndHandle, Rectangle rect)
		{
			IntPtr desktopPtr = GetDC(IntPtr.Zero);
			Graphics g = Graphics.FromHdc(desktopPtr);
			Point lpPoint = rect.Location;
			ClientToScreen(wndHandle, ref lpPoint);
			var rctgl = new Rectangle(lpPoint, rect.Size);
			Pen b = new Pen(Color.DeepSkyBlue);
			g.DrawRectangle(b, rctgl);
			g.Dispose();
			ReleaseDC(IntPtr.Zero, desktopPtr);
		}	

		public static void HighlightWindowFrame(this IntPtr wndHandle)
		{
			IntPtr desktopPtr = GetDC(IntPtr.Zero);
			Graphics g = Graphics.FromHdc(desktopPtr);
			GetWindowRect(wndHandle, out var rect);
			Pen b = new Pen(Color.DeepSkyBlue);
			g.DrawRectangle(b, rect.AsRectangle);
			g.Dispose();
			ReleaseDC(IntPtr.Zero, desktopPtr);
		}



		public static void HighlightClientFrame(this IntPtr wndHandle)
		{
			IntPtr desktopPtr = GetDC(IntPtr.Zero);
			Graphics g = Graphics.FromHdc(desktopPtr);
			GetClientRect(wndHandle, out var client);

			Point clientLocation = client.Location;
			ClientToScreen(wndHandle, ref clientLocation);
			client.Location = clientLocation;
			Pen b = new Pen(Color.DeepSkyBlue);

			g.DrawRectangle(b, client.AsRectangle);
			g.Dispose();
			ReleaseDC(IntPtr.Zero, desktopPtr);
			DeleteDC(desktopPtr);
		}

		private delegate bool EnumWindowsProc(IntPtr hWnd, ref SearchData data);

		private static bool EnumProc(IntPtr hWnd, ref SearchData data)
		{
			// Check classname and title 
			// This is different from FindWindow() in that the code below allows partial matches
			StringBuilder sb = new StringBuilder(1024);
			GetClassName(hWnd, sb, sb.Capacity);
			//wndClasses.Add(sb.ToString());
			if (data != null && 
			    sb.ToString() == data.Wndclass)
			{
				sb = new StringBuilder(1024);
				GetWindowText(hWnd, sb, sb.Capacity);
				if (!string.IsNullOrWhiteSpace(data.Title) && sb.ToString().Contains(data.Title))
				{
					data.HWnd.Add(hWnd);
					//return false;    // Found the wnd, halt enumeration
				}
				else if (data.Title == null)
				{
					data.HWnd.Add(hWnd);
				}
			}
			return true;
		}

	

		

	}
}
