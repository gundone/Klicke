using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InputSimulator
{
	public class Keyboard
	{
		
		[DllImport("user32.dll")]
		static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

		[DllImport("user32.dll")]
		static extern uint MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "LoadKeyboardLayout", SetLastError = true, ThrowOnUnmappableChar = false)]
		static extern uint LoadKeyboardLayout(StringBuilder pwszKlid, uint flags);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetKeyboardLayout", SetLastError = true, ThrowOnUnmappableChar = false)]
		static extern uint GetKeyboardLayout(uint idThread);

		[DllImport("user32.dll", 
			CallingConvention = CallingConvention.StdCall, 
			CharSet = CharSet.Unicode, 
			EntryPoint = "ActivateKeyboardLayout", 
			SetLastError = true, 
			ThrowOnUnmappableChar = false)]
		static extern uint ActivateKeyboardLayout(
			uint hkl,
			uint Flags);

		static class KeyboardLayoutFlags
		{
			public const uint KLF_ACTIVATE = 0x00000001;
			public const uint KLF_SETFORPROCESS = 0x00000100;
		}

		const uint MAPVK_VK_TO_VSC = 0x00;
		const uint MAPVK_VSC_TO_VK = 0x01;
		const uint MAPVK_VK_TO_CHAR = 0x02;
		const uint MAPVK_VSC_TO_VK_EX = 0x03;
		const uint MAPVK_VK_TO_VSC_EX = 0x04;
		
		const int KEYEVENTF_EXTENDEDKEY = 0x1;
		const int KEYEVENTF_KEYUP = 0x2;
		
		public static void PressKey(KeyCode keyCode)
		{
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC), 0, UIntPtr.Zero);
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC), KEYEVENTF_KEYUP, UIntPtr.Zero);
		}

		public static void PressKey(Keys keyCode)
		{
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC), 0, UIntPtr.Zero);
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC), KEYEVENTF_KEYUP, UIntPtr.Zero);
		}


		public static void KeyDown(Keys keyCode)
		{
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC), 0, UIntPtr.Zero);
			
		}

		public static void KeyUp(Keys keyCode)
		{
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC), KEYEVENTF_KEYUP, UIntPtr.Zero);
		}


	

		public static CultureInfo GetCurrentKeyboardLayout()
		{
			try
			{
				IntPtr foregroundWindow = Window.GetForegroundWindow();
				uint foregroundProcess  = Window.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
				uint keyboardLayout = GetKeyboardLayout(foregroundProcess) & 0xFFFF;
				return new CultureInfo((int) keyboardLayout);
			}
			catch (Exception)
			{
				return new CultureInfo(1033); // Assume English if something went wrong.
			}
		}
	}
}
