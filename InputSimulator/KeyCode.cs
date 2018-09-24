using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InputSimulator
{
	public enum KeyCode 
	{
		_0 = 0x30,
		_1 = 0x31,
		_2 = 0x32,
		_3 = 0x33,
		_4 = 0x34,
		_5 = 0x35,
		_6 = 0x36,
		_7 = 0x37,
		_8 = 0x38,
		_9 = 0x39,
		Enter = 13,
		//comma = 0xBC,
		dec     = Keys.Decimal,
		comma   = Keys.Oemcomma,
		dot     = Keys.OemPeriod,
		F5      = Keys.F5,
		F6      = Keys.F6,
		F7      = Keys.F7,
		Del     = Keys.Delete,
		Bcp     = Keys.Back,
		Ctrl    = Keys.ControlKey,
		Left    = Keys.Left,
		Right   = Keys.Right,
		Up      = Keys.Up,
		Down    = Keys.Down,
		quest   = Keys.OemQuestion,
		Esc     = Keys.Escape,
		Tab     = Keys.Tab,
		Alt     = Keys.Alt
	}
}
