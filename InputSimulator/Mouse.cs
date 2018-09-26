using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InputSimulator
{
    public static class Mouse
    {
	    [DllImport("user32.dll")]
	    static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

	    [DllImport("user32.dll")]
	    internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,  int cbSize);
	    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
	    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int cButtons, UIntPtr dwExtraInfo);
	    private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
	    private const uint MOUSEEVENTF_LEFTUP = 0x04;
	    private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
	    private const uint MOUSEEVENTF_WHEEL = 0x0800;
	    private const uint MOUSEEVENTF_RIGHTUP = 0x10;
	    static readonly Random Random = new Random();

		
	    public static void Down(bool right = false)
	    {
		    var down = right ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_LEFTDOWN;
		    mouse_event(down, 0, 0, 0, UIntPtr.Zero);
	    }

	    public static void Up(bool right = false)
	    {
		    var up = right ? MOUSEEVENTF_RIGHTUP : MOUSEEVENTF_LEFTUP;
		    mouse_event(up, 0, 0, 0, UIntPtr.Zero);
	    }

	    public static void Click(bool right = false)
	    {
		    var down = MOUSEEVENTF_LEFTDOWN;
		    var up   = MOUSEEVENTF_LEFTUP;
		    if (right)
		    {
			    down = MOUSEEVENTF_RIGHTDOWN;
			    up = MOUSEEVENTF_RIGHTUP;
		    }
		    mouse_event(down | up, 0, 0, 0, UIntPtr.Zero); // left mouse button down
	    }

	    public static void DoubleClick(bool right = false)
	    {
		    Click(right);
		    Thread.Sleep(Random.Next(78, 121));
		    Click(right);
	    }

	    public static void Wheel(int val)
	    {
		    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, val, UIntPtr.Zero);
	    }

	    public static void LinearSmoothMove(Point newPosition, TimeSpan duration)
	    {
		    Point start = Cursor.Position;

		    var rnd = new Random();
		    // Find the vector between start and targetPoint
		    double deltaX = newPosition.X - start.X;
		    double deltaY = newPosition.Y - start.Y;

		    // start a timer
		    var stopwatch = new Stopwatch();
		    stopwatch.Start();
		    double timeFraction;
		    do
		    {
			    var v = rnd.Next(0, 1000);
			    Trace.WriteLine(stopwatch.Elapsed.Ticks + ", " + v + ", " + (double)(stopwatch.Elapsed.Ticks + v) / duration.Ticks);
			    timeFraction = (double)(stopwatch.Elapsed.Ticks + v * 5) / duration.Ticks;
			    if (timeFraction > 1.0)
				    timeFraction = 1.0;
			    var addX = rnd.Next(0, 1);
			    var addY = rnd.Next(0, 1);
			    var curPoint = new Point(start.X + (int)(timeFraction * deltaX) + addX,
				    start.Y + (int)(timeFraction * deltaY) + addY);
			    Cursor.Position = curPoint;
			    Thread.Sleep(5);
		    } 
		    while (timeFraction < 1.0);
		    Cursor.Position = newPosition;
	    }

	    public static void DownToWindow(IntPtr wndHandle , Point clientPoint)
	    {
		    var oldPos = Cursor.Position;

		    /// get screen coordinates
		    ClientToScreen(wndHandle, ref clientPoint);

		    /// set cursor on coords, and press mouse
		    Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

		    var inputMouseDown = new INPUT();
		    inputMouseDown.Type = 0; /// input type mouse
		    inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down


		    var inputs = new INPUT[] { inputMouseDown};
		    SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

		    /// return mouse 
		    Cursor.Position = oldPos;
	    }

	    public static void UpToWindow(IntPtr wndHandle , Point clientPoint)
	    {
		    var oldPos = Cursor.Position;

		    /// get screen coordinates
		    ClientToScreen(wndHandle, ref clientPoint);

		    /// set cursor on coords, and press mouse
		    Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

		    var inputMouseUp = new INPUT();
		    inputMouseUp.Type = 0; /// input type mouse
		    inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up

		    var inputs = new INPUT[] { inputMouseUp };
		    SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

		    /// return mouse 
		    Cursor.Position = oldPos;
	    }

	    public static void ClickToWindow(IntPtr wndHandle , Point clientPoint)
	    {
			DownToWindow(wndHandle, clientPoint);
			UpToWindow(wndHandle, clientPoint);
		}

#pragma warning disable 649
	    internal struct INPUT
	    {
		    public UInt32 Type;
		    public MOUSEKEYBDHARDWAREINPUT Data;
	    }

	    [StructLayout(LayoutKind.Explicit)]
	    internal struct MOUSEKEYBDHARDWAREINPUT
	    {
		    [FieldOffset(0)]
		    public MOUSEINPUT Mouse;
	    }

	    internal struct MOUSEINPUT
	    {
		    public Int32 X;
		    public Int32 Y;
		    public UInt32 MouseData;
		    public UInt32 Flags;
		    public UInt32 Time;
		    public IntPtr ExtraInfo;
	    }

#pragma warning restore 649
    }
}
