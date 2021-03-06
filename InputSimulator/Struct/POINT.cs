﻿using System.Runtime.InteropServices;

namespace InputSimulator.Struct
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct POINT
	{
		public readonly int X;
		public readonly int Y;

		public POINT(int x, int y)
		{
			X = x;
			Y = y;
		}

		public POINT(System.Drawing.Point pt)
		{
			X = pt.X;
			Y = pt.Y;
		}

		public static implicit operator System.Drawing.Point(POINT p)
		{
			return new System.Drawing.Point(p.X, p.Y);
		}

		public static implicit operator POINT(System.Drawing.Point p)
		{
			return new POINT(p.X, p.Y);
		}
	}
}