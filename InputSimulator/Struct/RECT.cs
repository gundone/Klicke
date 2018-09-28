using System.Drawing;
using System.Runtime.InteropServices;
// ReSharper disable All

namespace InputSimulator.Struct
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public Size Size
		{
			get =>new Size(Right-Left, Bottom-Top);
			set
			{
				Right = value.Width + Left;
				Bottom = value.Height + Top;
			}
		}

		public Point Location
		{
			get =>new Point(Left, Top);
			set
			{
				var w = Size.Width;
				var h = Size.Height;
				Left = value.X;
				Top = value.Y;
				Right = Left + w;
				Bottom = Top + h;
			}
		} 

		public Rectangle AsRectangle => new Rectangle(Location, Size);
	}
}