using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComputerVision;
using InputSimulator;

namespace CommandProcessor
{
    public class Executor
    {

	    private IntPtr _currentWnd = IntPtr.Zero;

	    public static Task Run(string cmdStr)
	    {
		    var cmd = Command.Parse(cmdStr);
		    var executor = new Executor();
		    Type thisType = executor.GetType();
		    MethodInfo method = thisType.GetMethod(cmd.Name);
		    if (method != null)
		    {
			    //if (method.GetCustomAttributes(typeof(AsyncStateMachineAttribute), false).Length > 0)
			    //{
					Task t = (Task)method.Invoke(executor, new object[]{ new string[]{cmdStr}});
				    return t;
			 //   }
				//else 
				//	method.Invoke(executor, new object[]{ new string[]{cmdStr}});
		    }
			return Task.Delay(200);
	    }

	    public void MouseAction(object[] args) //int x, int y, string btn, int clicks, int wheel 
	    {

		    var command = Command.Parse((string)args[0]);
			
		    var x      = int.Parse(command.Attr["X"]);
		    var y      = int.Parse(command.Attr["Y"]);
		    var btn    = command.Attr["Button"];
		    var clicks = int.Parse(command.Attr["Clicks"]);
		    var wheel  = int.Parse(command.Attr["Wheel"]);

		    Cursor.Position = new Point(x, y);
		    //Mouse.LinearSmoothMove(new Point(x, y), TimeSpan.FromMilliseconds(200));
		    if (wheel != 0)
		    {
			    Mouse.Wheel(wheel);
			    return;
		    }
		    switch (clicks)
		    {
				case 1:
					Mouse.Click(btn == "Right");
					break;
				case 2: 
					Mouse.DoubleClick(btn == "Right");
					break;
		    }

		    return;
	    }

	    public void KeyboardAction(object[] args)
	    {
		    var command = Command.Parse((string)args[0]);
		    var action = command.Attr.First().Key;
		    var key = (Keys)Enum.Parse(typeof(Keys), command.Attr.First().Value);
		    switch (action)
		    {
				case "KeyUp":
					Keyboard.KeyUp(key);
					break;
				case "KeyDown":
					Keyboard.KeyDown(key);
					break;
				default:
					throw new ArgumentException($"Undefined KeyboardAction {command}");
		    }

		    Thread.Sleep(100);
	    }

	    public void WaitForWindow(object[] args) //(string wndClass, string wndTitle, int checkSec = 1)
	    {
		    var command = Command.Parse((string)args[0]);
		    var wndClass = command.Attr["WndClass"];
		    var wndTitle = command.Attr["WndTitle"];
			var max      = command.Attr.ContainsKey("Maximize") && bool.Parse(command.Attr["Maximize"]);
		    var close    = command.Attr.ContainsKey("Close") && bool.Parse(command.Attr["Close"]);
		    var checkSec = 1;
			Stopwatch t = new Stopwatch();
			t.Start();
		    IntPtr hWnd = IntPtr.Zero;
		    do
		    {
			    Thread.Sleep(checkSec * 1000);
			    hWnd = Window.SearchWindow(wndClass, wndTitle).FirstOrDefault();
		    } while (hWnd == IntPtr.Zero);
			Thread.Sleep(100);
		    if (close)
		    {
				Window.CloseWindow(hWnd);
				return;
		    }
			if(max)
				Window.ShowWindow(hWnd, ShowWindowCommands.Maximize);
		   
		
			Window.SetForegroundWindow(hWnd);
		    Window.SetFocus(hWnd);
	    }

	    public void OpenProgram(object[] args)
	    {
		    var command = Command.Parse((string)args[0]);
		    var path = command.Attr["Path"];

		    var args1 = command.Attr.ContainsKey("Args") ? command.Attr["Args"] : "";
		    Process.Start(path, args1);
	    }


		/// <summary>
		/// LocateSpecificText(this IntPtr wndHandle, string searchText, string lang = "en", int minHeight = 7, int maxHeight = 30, int minWidth = 40)
		/// </summary>
		/// <returns></returns>
		public List<Rectangle> LocateText(object[] args)
	    {
		    var command   = Command.Parse((string)args[0]);
		    var text      = command.Attr["Text"];
		    var lang      = command.Attr.ContainsKey("Lang")      ? command.Attr["Lang"] : "en";
		    var minHeight = command.Attr.ContainsKey("MinHeight") ? int.Parse(command.Attr["MinHeight"]) : 7; 
		    var maxHeight = command.Attr.ContainsKey("MaxHeight") ? int.Parse(command.Attr["MaxHeight"]) : 30;
		    var minWidth  = command.Attr.ContainsKey("MinWidth")  ? int.Parse(command.Attr["MinWidth"]) : 40;
		    return _currentWnd.LocateSpecificText(text, lang, minHeight, maxHeight, minWidth);
	    }

	    public void Sleep(object[] args)
	    {
		    var command = Command.Parse((string)args[0]);
		    var ms = command.Attr["Time"];
			Thread.Sleep(TimeSpan.FromMilliseconds(int.Parse(ms)));
	    }

	    public void SetKeyboardLayout(object[] args)
		{
			var command   = Command.Parse((string)args[0]);
			var lang      = command.Attr["Lang"];
			var culture   = CultureInfo.CreateSpecificCulture(lang);
			var k1        = KeyboardLayout.GetCurrent();
			var k2 = KeyboardLayout.Load(culture);
			if (k1 == k2) return;
			Keyboard.KeyDown(Keys.LMenu);
			Keyboard.KeyDown(Keys.LShiftKey);
			Keyboard.KeyUp(Keys.LShiftKey);
			Keyboard.KeyUp(Keys.LMenu);
		}


    }


}
