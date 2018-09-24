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
using HookManagerNS;
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
			}
		}

		private void MouseWheelHook(object sender, MouseEventArgs e)
		{
			_wheel += e.Delta;
			TextBoxWheel.Text = _wheel.ToString();
			var action = $"MouseAction: X={e.X};Y={e.Y};Button={e.Button};Event=Scroll;Clicks={e.Clicks};Wheel={e.Delta}";
			Parent.AddAction(action);
		}

		private void MouseMoveHook(object sender, MouseEventArgs e)
		{
			TextBoxX.Text = e.X.ToString();
			TextBoxY.Text = e.Y.ToString();
		}

		private void MouseDownHook(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			var action = $"MouseAction: X={e.X};Y={e.Y};Button={e.Button};Event=Down;Clicks={e.Clicks};Wheel={e.Delta}";
			Parent.AddAction(action);
		}

		private void MouseUpHook(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			var action = $"MouseAction: X={e.X};Y={e.Y};Button={e.Button};Event=Up;Clicks={e.Clicks};Wheel={e.Delta}";
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
			}
		}

		private void KeyInterceptorOnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			var action = $"KeyboardAction: KeyUp={keyEventArgs.KeyCode}";
			Parent.AddAction(action);
		}

		private void KeyInterceptorOnKeyDown(object sender, KeyEventArgs keyEventArgs)
		{
			var action = $"KeyboardAction: KeyDown={keyEventArgs.KeyCode}";
			Parent.AddAction(action);
		}

		private void WindowActionSelect(object sender, MouseEventArgs e)
		{
			var action = $"WaitForWindow: WndClass={textBoxWndClass.Text};WndTitle={textBoxWndTitle.Text};Maximize={MaximizeWndCheckBox.IsChecked};Close={CloseWndCheckBox.IsChecked}";
			Parent.AddAction(action);
		}

		private void WindowSearchStartStop(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.F2)
			{
				HookManager.MouseMove += WindowSearch;
				HookManager.MouseDown += WindowActionSelect;
			}
			else if (e.KeyData == Keys.F3)
			{
				HookManager.ClearAll();
				KeyInterceptor.ClearAll();
			}
		}

		private void WindowSearch(object sender, MouseEventArgs e)
		{
			var hWnd = Window.WindowFromPoint(new Window.POINT(e.X, e.Y));
			StringBuilder ClassName = new StringBuilder(256);
			StringBuilder Title = new StringBuilder(256);
			//Get the window class name
			var nRet = Window.GetClassName(hWnd, ClassName, ClassName.Capacity);
			var nx = Window.GetWindowText(hWnd, Title, Title.Capacity);
			if(nRet != 0)
			{
				textBoxWndClass.Text = ClassName.ToString();
			}

			if (nx != 0)
			{
				textBoxWndTitle.Text = Title.ToString();
			}
		}

		private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			KeyInterceptor.ClearAll();
			HookManager.ClearAll();
			
			if(MouseTab.IsSelected)
			{
				KeyInterceptor.KeyDown += MouseEventsStartStop;
			}
			else if (KeyboardTab.IsSelected)
			{
				KeyInterceptor.KeyDown += KeyboardEventsStartStop;
			}
			else if (WaitForWindowTab?.IsSelected ?? false)
			{
				KeyInterceptor.KeyDown += WindowSearchStartStop;
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
	}
}
