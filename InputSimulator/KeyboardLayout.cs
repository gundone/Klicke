using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace InputSimulator
{
	public sealed class KeyboardLayout
	{
		[DllImport("user32.dll",
			CallingConvention = CallingConvention.StdCall,
			CharSet = CharSet.Unicode,
			EntryPoint = "LoadKeyboardLayout",
			SetLastError = true,
			ThrowOnUnmappableChar = false)]
		static extern uint LoadKeyboardLayout(
			StringBuilder pwszKlid,
			uint flags);

		[DllImport("user32.dll",
			CallingConvention = CallingConvention.StdCall,
			CharSet = CharSet.Unicode,
			EntryPoint = "GetKeyboardLayout",
			SetLastError = true,
			ThrowOnUnmappableChar = false)]
		static extern uint GetKeyboardLayout(
			uint idThread);

		[DllImport("user32.dll",
			CallingConvention = CallingConvention.StdCall,
			CharSet = CharSet.Unicode,
			EntryPoint = "ActivateKeyboardLayout",
			SetLastError = true,
			ThrowOnUnmappableChar = false)]
		static extern uint ActivateKeyboardLayout(
			uint hkl,
			uint flags);

		static class KeyboardLayoutFlags
		{
			// ReSharper disable once InconsistentNaming
			public const uint KLF_ACTIVATE = 0x00000001;
			// ReSharper disable once InconsistentNaming
			public const uint KLF_SETFORPROCESS = 0x00000100;
		}

		private readonly uint _hkl;

		private KeyboardLayout(CultureInfo cultureInfo)
		{
			string layoutName = cultureInfo.LCID.ToString("x8");

			var pwszKlid = new StringBuilder(layoutName);
			_hkl = LoadKeyboardLayout(pwszKlid, KeyboardLayoutFlags.KLF_ACTIVATE);
		}

		private KeyboardLayout(uint hkl)
		{
			_hkl = hkl;
		}

		public static bool operator ==(KeyboardLayout k1, KeyboardLayout k2)
		{
			return k1?.Handle == k2?.Handle;
		}

		public static bool operator !=(KeyboardLayout k1, KeyboardLayout k2)
		{
			return k1?.Handle != k2?.Handle;
		}

		public uint Handle => _hkl;

		public static KeyboardLayout GetCurrent()
		{
			uint hkl = GetKeyboardLayout((uint)Thread.CurrentThread.ManagedThreadId);
			return new KeyboardLayout(hkl);
		}

		public static KeyboardLayout Load(CultureInfo culture)
		{
			return new KeyboardLayout(culture);
		}

		public void Activate()
		{
			ActivateKeyboardLayout(_hkl, KeyboardLayoutFlags.KLF_ACTIVATE);
		}
	}
}
