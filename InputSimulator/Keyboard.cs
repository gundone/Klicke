using System;
using System.Globalization;
using System.Windows.Forms;

namespace InputSimulator
{
	public partial class Keyboard
	{
		public static void PressKey(KeyCode keyCode)
		{
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC), KeyEventFlags.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC), KeyEventFlags.KEYEVENTF_KEYUP, UIntPtr.Zero);
		}

		public static void PressKey(Keys keyCode)
		{
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC), KeyEventFlags.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC), KeyEventFlags.KEYEVENTF_KEYUP, UIntPtr.Zero);
		}


		public static void KeyDown(Keys keyCode)
		{
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC), KeyEventFlags.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
			
		}

		public static void KeyUp(Keys keyCode)
		{
			keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC), KeyEventFlags.KEYEVENTF_KEYUP, UIntPtr.Zero);
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
