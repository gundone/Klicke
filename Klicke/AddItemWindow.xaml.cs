using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using CommandProcessor;
using HookManagerNS;
using InputSimulator;
using InputSimulator.Struct;
using KeyInterceptorNS;
using Cursor = System.Windows.Input.Cursor;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Panel = System.Windows.Controls.Panel;
using Window = InputSimulator.Window;

namespace Klicke
{
	/// <summary>
	/// Interaction logic for AddItemWindow.xaml
	/// </summary>
	public partial class AddItemWindow : System.Windows.Window
	{
		
		private int _wheel = 0;
		private MainWindow Parent;
		
		public AddItemWindow(MainWindow parent)
		{
			//Cursor cur = new Cursor(@"C:\Users\bahbk\source\repos\Klicke\Klicke\bin\Debug\cur1.cur");
			//Cursor = cur;
			Parent = parent;
			InitializeComponent();
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void button2_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{

		}
		private void MouseEventsStartStop(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.F2)
			{
				HookManager.MouseMove += MouseMoveHook;
				HookManager.MouseDown += MouseDownHook;
				HookManager.MouseUp   += MouseUpHook;
				HookManager.MouseWheel += MouseWheelHook;
			}

			if (e.KeyData == Keys.F3)
			{
				KeyInterceptor.ClearAll();
				HookManager.ClearAll();
				KeyInterceptor.KeyDown += MouseEventsStartStop;
			}
		}

		private void MouseWheelHook(object sender, MouseEventArgs e)
		{
			_wheel += e.Delta;
			TextBoxWheel.Text = _wheel.ToString();
			System.Drawing.Point click = new System.Drawing.Point(e.X, e.Y);
			
			var action = $"MouseAction: X={click.X};Y={click.Y};Button={e.Button};Event=Scroll;Clicks={e.Clicks};Wheel={e.Delta}";
			Parent.AddAction(action);
		}

		private void MouseMoveHook(object sender, MouseEventArgs e)
		{
			System.Drawing.Point click = new System.Drawing.Point(e.X, e.Y);
			if (_wndBound != null)
			{
				IntPtr hWndBound = IntPtr.Zero;
				hWndBound = string.IsNullOrWhiteSpace(_wndBound.Item2) 
					? Window.SearchWindow(_wndBound.Item1, null).FirstOrDefault() 
					: Window.SearchWindow(_wndBound.Item1, _wndBound.Item2).FirstOrDefault();
				if (hWndBound != IntPtr.Zero)
				{
					Window.ScreenToClient(hWndBound, ref click);
				}
			}
			TextBoxX.Text = click.X.ToString();
			TextBoxY.Text = click.Y.ToString();
		}

		private void MouseDownHook(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			System.Drawing.Point click = new System.Drawing.Point(e.X, e.Y);
			var attr = "";
			if (_wndBound != null)
			{
				IntPtr hWndBound = IntPtr.Zero;
				hWndBound = string.IsNullOrWhiteSpace(_wndBound.Item2) 
					? Window.SearchWindow(_wndBound.Item1, null).FirstOrDefault() 
					: Window.SearchWindow(_wndBound.Item1, _wndBound.Item2).FirstOrDefault();
				if (hWndBound != IntPtr.Zero)
				{
					Window.ScreenToClient(hWndBound, ref click);
					attr = $"WndClass={_wndBound.Item1};WndTitle={_wndBound.Item2};";
				}
			}
			var action = $"MouseAction: {attr}X={click.X};Y={click.Y};Button={e.Button};Event=Down;Clicks={e.Clicks};Wheel={e.Delta}";
			Parent.AddAction(action);
		}

		private void MouseUpHook(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			System.Drawing.Point click = new System.Drawing.Point(e.X, e.Y);
			var attr = "";
			if (_wndBound != null)
			{
				IntPtr hWndBound = IntPtr.Zero;
				hWndBound = string.IsNullOrWhiteSpace(_wndBound.Item2) 
					? Window.SearchWindow(_wndBound.Item1, null).FirstOrDefault() 
					: Window.SearchWindow(_wndBound.Item1, _wndBound.Item2).FirstOrDefault();
				if (hWndBound != IntPtr.Zero)
				{
					Window.ScreenToClient(hWndBound, ref click);
					attr = $"WndClass={_wndBound.Item1};WndTitle={_wndBound.Item2};";
				}
			}
			var action = $"MouseAction: {attr}X={click.X};Y={click.Y};Button={e.Button};Event=Up;Clicks={e.Clicks};Wheel={e.Delta}";
			Parent.AddAction(action);
		}

		private void KeyboardEventsStartStop(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.F2)
			{
				KeyInterceptor.KeyDown += KeyInterceptorOnKeyDown;
				KeyInterceptor.KeyUp   += KeyInterceptorOnKeyUp;
			}
			else if (e.KeyData == Keys.F3)
			{
				KeyInterceptor.ClearAll();
				HookManager.ClearAll();
				KeyInterceptor.KeyDown += KeyboardEventsStartStop;
			}
		}


		private Keys _lastDown;
		private void KeyInterceptorOnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			var code = keyEventArgs.KeyCode;
			if (code == Keys.F2 || code == Keys.F3) return;

			_lastDown = Keys.Zoom;
			var action = $"KeyboardAction: KeyUp={keyEventArgs.KeyCode}";
			Parent.AddAction(action);
		}

		private void KeyInterceptorOnKeyDown(object sender, KeyEventArgs keyEventArgs)
		{
			var code = keyEventArgs.KeyCode;
			if (code == Keys.F2 || code == Keys.F3 || _lastDown == code) return;

			_lastDown = code;
			var action = $"KeyboardAction: KeyDown={code}";
			Parent.AddAction(action);

		}

		Tuple<string,string> _wndBound = null;

		private void WindowActionSelect(object sender, MouseEventArgs e)
		{

			var action = $"WaitForWindow: WndClass={TextBoxWndClass.Text};" +
			             $"WndTitle={TextBoxWndTitle.Text};" +
			             $"Maximize={MaximizeWndCheckBox.IsChecked};" +
			             $"Close={CloseWndCheckBox.IsChecked};" +
			             $"Bind={BindWindow.IsChecked};" +
			             $"Unbind={UnbindWindow.IsChecked}";
			if (BindWindow.IsChecked ?? false)
			{
				_wndBound = new Tuple<string, string>(TextBoxWndClass.Text, TextBoxWndTitle.Text);
			}
			if (UnbindWindow.IsChecked ?? false)
			{
				_wndBound = null;
			}
			Parent.AddAction(action);
			_lastWnd.RefreshWindow();
			HookManager.MouseMove -= WindowSearch;
			HookManager.ClearAll();
			KeyInterceptor.ClearAll();
		}

		IntPtr _lastWnd = IntPtr.Zero;
		private void WindowSearch(object sender, MouseEventArgs e)
		{
			_lastWnd.RefreshWindow();
			var hWnd = Window.WindowFromPoint(e.X, e.Y);
			hWnd.RefreshWindow();
			StringBuilder ClassName = new StringBuilder(256);
			StringBuilder Title = new StringBuilder(256);
			//Get the window class name
			var nRet = Window.GetClassName(hWnd, ClassName, ClassName.Capacity);
			var nx   = hWnd.GetWindowText();
			if(nRet != 0)
			{
				hWnd.HighlightClientFrame();
				TextBoxWndClass.Text = ClassName.ToString();
				
			}

			if (nx.Length != 0)
			{
				TextBoxWndTitle.Text = Title.ToString();
			}
			_lastWnd = hWnd;
		}

		private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			KeyInterceptor.ClearAll();
			HookManager.ClearAll();
			
			if(MouseTab.IsSelected)
			{
				if (_wndBound != null)
				{
					WndBound.Text = $"WndClass={_wndBound.Item1};" +
					                   $"WndTitle={_wndBound.Item2};";
				}
				else
				{
					WndBound.Text = "";
				}
				KeyInterceptor.KeyDown += MouseEventsStartStop;
			}
			else if (KeyboardTab.IsSelected)
			{
				KeyInterceptor.KeyDown += KeyboardEventsStartStop;
			}
			else if (WaitForWindowTab?.IsSelected ?? false)
			{
				//KeyInterceptor.KeyDown += WindowSearchStartStop;
			}
		}

		private void AddAction_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			
		}

		private void OpenProgramAddAction(object sender, RoutedEventArgs e)
		{
			var action = $"OpenProgram: Path={ProgramPath.Text}";
			if (!string.IsNullOrWhiteSpace(ProgramAgrs.Text))
				action += $";Args={ProgramAgrs.Text}";
			Parent.AddAction(action);
		}

		private void SleepAddAction(object sender, RoutedEventArgs e)
		{
			var action = $"Sleep: Time={SleepTime.Text}";
			Parent.AddAction(action);
		}

		private void OpenFileDialog(object sender, MouseButtonEventArgs e)
		{
			OpenFileDialog od = new OpenFileDialog
			{
				Filter = @"(*.exe)|*.exe|(All Files *.*)|*.*"
			};
			if (od.ShowDialog() == System.Windows.Forms.DialogResult.OK && od.CheckFileExists)
			{
				ProgramPath.Text = od.FileName;
			}
		}

		private void AddLocateTextAction(object sender, RoutedEventArgs e)
		{
			var action = $"LocateText: Text={locateText.Text}";
			if (!string.IsNullOrWhiteSpace(LangTextBox.Text))
				action += $";Lang={LangTextBox.Text}";
			if (!string.IsNullOrWhiteSpace(MinHeightTextBox.Text))
				action += $";MinHeight={MinHeightTextBox.Text}";
			if (!string.IsNullOrWhiteSpace(MaxHeightTextBox.Text))
				action += $";MaxHeight={MaxHeightTextBox.Text}";
			if (!string.IsNullOrWhiteSpace(MinWidthTextBox.Text))
				action += $";MinWidth={MinWidthTextBox.Text}";
			Parent.AddAction(action);
		}

		private void SetKbdLayoutBtnClick(object sender, RoutedEventArgs e)
		{
			var action = $"SetKeyboardLayout: Lang={(KbdLayout.SelectedItem  as ListBoxItem)?.Content}";
			Parent.AddAction(action);
		}

		private void BindWindow_OnClick(object sender, RoutedEventArgs e)
		{
			
		}

		private void UnbindWndClick(object sender, RoutedEventArgs e)
		{
			_wndBound = null;
			WndBound.Text = "";
		}

		private void WndBound_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var command = Command.Parse($@"BindWindow: {WndBound.Text}");
			var cls = command.Attr.ContainsKey("WndClass") ? command.Attr["WndClass"]: "";
			var ttl = command.Attr.ContainsKey("WndTitle") ? command.Attr["WndTitle"]: "";
			_wndBound = !string.IsNullOrWhiteSpace(cls) 
				? new Tuple<string, string>(cls, ttl) 
				: null;
		}

		private void OnTop_OnClick(object sender, RoutedEventArgs e)
		{
			Topmost = OnTop.IsChecked ?? false;
		}

		private void StartWndSearching_OnClick(object sender, RoutedEventArgs e)
		{
			HookManager.MouseMove += WindowSearch;
			HookManager.MouseUp   += WindowActionSelect;
		}

		private void ScriptPath_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			OpenFileDialog od = new OpenFileDialog
			{
				Filter = @"(*.log)|*.log|(All Files *.*)|*.*"
			};
			if (od.ShowDialog() == System.Windows.Forms.DialogResult.OK && od.CheckFileExists)
			{
				ScriptPath.Text = od.FileName;
			}
		}

		private void OpenScriptAddAction(object sender, RoutedEventArgs e)
		{
			var action = $"Execute: Script={ScriptPath.Text}";
			Parent.AddAction(action);
		}
	}
}
