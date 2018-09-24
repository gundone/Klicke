using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Input;
using KeyEventHandler = System.Windows.Forms.KeyEventHandler;

namespace KeyInterceptorNS
{
	public partial class KeyInterceptor
	{
		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private static IntPtr _hookID = IntPtr.Zero;


		private static event KeyEventHandler s_KeyDown;
		private static event KeyEventHandler s_KeyUp;

		public static event KeyEventHandler KeyDown
		{
			add
			{
				EnsureSubscribedToGlobalKeyboardEvents();
				s_KeyDown += value;
			}
			remove
			{
				s_KeyDown -= value;
				TryUnsubscribeFromGlobalKeyboardEvents();
			}
		}
		
		public static event KeyEventHandler KeyUp
		{
			add
			{
				EnsureSubscribedToGlobalKeyboardEvents();
				s_KeyUp += value;
			}
			remove
			{
				s_KeyUp -= value;
				TryUnsubscribeFromGlobalKeyboardEvents();
			}
		}

		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		private static extern int UnhookWindowsHookEx(int hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr CallNextHookEx(int hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);
	}

}
