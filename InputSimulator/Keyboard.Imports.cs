using System;
using System.Runtime.InteropServices;
using System.Text;

namespace InputSimulator
{
	public static partial class Keyboard
	{
		[DllImport("user32.dll")]
		private static extern void keybd_event(byte bVk, byte bScan, KeyEventFlags dwFlags, UIntPtr dwExtraInfo);

		[DllImport("user32.dll")]
		private static extern uint MapVirtualKey(uint uCode, MapVirtualKeyMapTypes uMapType);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "LoadKeyboardLayout", SetLastError = true, ThrowOnUnmappableChar = false)]
		private static extern uint LoadKeyboardLayout(StringBuilder pwszKlid, uint flags);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "GetKeyboardLayout", SetLastError = true, ThrowOnUnmappableChar = false)]
		private static extern uint GetKeyboardLayout(uint idThread);

		[DllImport("user32.dll", 
			CallingConvention = CallingConvention.StdCall, 
			CharSet = CharSet.Unicode, 
			EntryPoint = "ActivateKeyboardLayout", 
			SetLastError = true, 
			ThrowOnUnmappableChar = false)]
		private static extern uint ActivateKeyboardLayout(uint hkl,
			KeyboardLayoutFlags Flags);
	}
}