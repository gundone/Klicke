using System;

namespace InputSimulator
{
	[Flags]
    internal enum KeyboardLayoutFlags
    {
		/// <summary>
		/// If the specified input locale identifier is not already loaded, the function loads and activates the input locale identifier for the current thread
		/// </summary>
		KLF_ACTIVATE = 0x00000001,
		/// <summary>
		/// Substitutes the specified input locale identifier with another locale preferred by the user. The system starts with this flag set, and it is recommended that your application always use this flag
		/// </summary>
		KLF_SUBSTITUTE_OK = 0x00000002,
		/// <summary>
		/// If this bit is set, the system's circular list of loaded locale identifiers is reordered by moving the locale identifier to the head of the list. If this bit is not set, the list is rotated without a change of order
		/// </summary>
		KLF_REORDER = 0x00000008,
		/// <summary>
		/// If the new input locale identifier has the same language identifier as a current input locale identifier, the new input locale identifier replaces the current one as the input locale identifier for that language
		/// </summary>
		KLF_REPLACELANG = 0x00000010,
		/// <summary>
		/// Prevents a ShellProc hook procedure from receiving an HSHELL_LANGUAGE hook code when the new input locale identifier is loaded. This value is typically used when an application loads multiple input locale identifiers one after another
		/// </summary>
		KLF_NOTELLSHELL = 0x00000080,
		/// <summary>
		/// Activates the specified locale identifier for the entire process
		/// </summary>
		KLF_SETFORPROCESS = 0x00000100,
		/// <summary>
		/// This is used with KLF_RESET.
		/// </summary>
		KLF_SHIFTLOCK = 0x00010000,
		/// <summary>
		/// If set but KLF_SHIFTLOCK is not set, the Caps Lock state is turned off by pressing the Caps Lock key again. If set and KLF_SHIFTLOCK is also set, the Caps Lock state is turned off by pressing either SHIFT key
		/// </summary>
		KLF_RESET = 0x40000000,
    }
}