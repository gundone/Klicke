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
    }
}
