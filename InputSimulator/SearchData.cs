using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputSimulator
{
	internal class SearchData
	{
		// You can put any vars in here...
		public string Wndclass;
		public string Title;
		public readonly List<IntPtr> HWnd = new List<IntPtr>();
	}
}
