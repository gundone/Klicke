using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace KeyInterceptorNS
{
	public partial class KeyInterceptor
	{
		private static int s_KeyboardHookHandle;
		private static LowLevelKeyboardProc s_KeaboardDelegate;

		private static void EnsureSubscribedToGlobalKeyboardEvents()
		{
			// install Mouse hook only if it is not installed and must be installed
			if (s_KeyboardHookHandle == 0)
			{
				//See comment of this field. To avoid GC to clean it up.
				s_KeaboardDelegate = HookCallback;
				//install hook
				using (Process curProcess = Process.GetCurrentProcess())
				using (ProcessModule curModule = curProcess.MainModule)
				{
					var mar = GetModuleHandle(curModule.ModuleName);
					//var mar = LoadLibrary("user32.dll");
					s_KeyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, s_KeaboardDelegate, mar, 0);
					//If SetWindowsHookEx fails.
					if (s_KeyboardHookHandle == 0)
					{
						//Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
						int errorCode = Marshal.GetLastWin32Error();
						//do cleanup

						//Initializes and throws a new instance of the Win32Exception class with the specified error. 
						throw new Win32Exception(errorCode);
					}
				}
			}
		}
		
		private static void TryUnsubscribeFromGlobalKeyboardEvents()
		{
			//if no subsribers are registered unsubsribe from hook
			if (s_KeyDown == null &&
			    s_KeyUp == null)
			{
				ForceUnsunscribeFromGlobalKeyboardEvents();
			}
		}


		private static void ForceUnsunscribeFromGlobalKeyboardEvents()
		{
			if (s_KeyboardHookHandle != 0)
			{
				//uninstall hook
				int result = UnhookWindowsHookEx(s_KeyboardHookHandle);
				//reset invalid handle
				s_KeyboardHookHandle = 0;
				//Free up for GC
				s_KeaboardDelegate = null;
				//if failed and exception must be thrown
				if (result == 0)
				{
					//Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
					int errorCode = Marshal.GetLastWin32Error();
					//Initializes and throws a new instance of the Win32Exception class with the specified error. 
					throw new Win32Exception(errorCode);
				}
			}
		}

		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				int vkCode = Marshal.ReadInt32(lParam);
				KeyEventArgs e = new KeyEventArgs((Keys) vkCode);
				switch ((int)wParam)
				{
					case WM_KEYDOWN:
						s_KeyDown?.Invoke(null, e);
						break;
					case WM_KEYUP:
						s_KeyUp?.Invoke(null, e);
						break;
				}
			}

			return CallNextHookEx(s_KeyboardHookHandle, nCode, wParam, lParam);
		}

		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern IntPtr LoadLibrary(string lpFileName);

		public static void ClearAll()
		{
			ForceUnsunscribeFromGlobalKeyboardEvents();
			s_KeyDown = null;
			s_KeyUp = null;
		}
	}
}