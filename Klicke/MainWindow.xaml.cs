using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using CommandProcessor;
using Application = System.Windows.Application;
//using DialogResult =  System.Windows.Forms.DialogResult;

namespace Klicke
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		private List<object> Record = new List<object>();
		private List<string> Serial = new List<string>();

		OptionsWindow optWnd = new OptionsWindow();

		public MainWindow()
		{
			InitializeComponent();
			if (Properties.Settings.Default.OpenLastScript 
			    && File.Exists(Properties.Settings.Default.LastOpenedScript))
			{
				ActionListBox.Text = string.Join("\r\n", File.ReadAllLines(Properties.Settings.Default.LastOpenedScript));
			}
		}

		private void ExitMenuItemClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void LoadMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			OpenFileDialog od = new OpenFileDialog
			{
				Filter = @"(*.log)|*.log|(All Files *.*)|*.*"
			};
			if (od.ShowDialog() == System.Windows.Forms.DialogResult.OK && od.CheckFileExists)
			{
				ActionListBox.Text = string.Join("\r\n", File.ReadAllLines(od.FileName));
				Properties.Settings.Default.LastOpenedScript = od.FileName;
			}
		}

		private void AddButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (!IsWindowOpen<AddItemWindow>("AddAction"))
			{
				AddItemWindow addWnd = new AddItemWindow(this);
				addWnd.Show();
			}

		}

		private static bool IsWindowOpen<T>(string name = "") where T : Window
		{
			return string.IsNullOrEmpty(name)
				? Application.Current.Windows.OfType<T>().Any()
				: Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
		}

		public void AddAction(string action)
		{
			//Serial.Add(action);
			//ActionListBox.Clear();
			if (Properties.Settings.Default.AddActionsToCursor)
			{
				var index = ActionListBox.CaretIndex;
				ActionListBox.Text = ActionListBox.Text.Insert(index, action + Environment.NewLine);
				ActionListBox.CaretIndex = index + (action + Environment.NewLine).Length;
			}
				
			else
				ActionListBox.Text += action + Environment.NewLine;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (IsWindowOpen<AddItemWindow>("AddAction"))
			{
				Application
					.Current
					.Windows
					.OfType<AddItemWindow>()
					.First(w => w.Name.Equals("AddAction"))
					.Close();
				Application
					.Current
					.Windows
					.OfType<AddItemWindow>()
					.First(w => w.Name.Equals("Options"))
					.Close();
			}
		}

		private void ActionListBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			Serial.Clear();
			Serial.AddRange(Regex.Split(ActionListBox.Text, @"\r?\n|\r"));
		}

		CancellationTokenSource _tokenSource2 = new CancellationTokenSource();
		
		

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			ActionListBox.IsEnabled = false;
			ActionListBox.Visibility = Visibility.Hidden;
			ActionList.Items.Clear();
			foreach (var s in Serial)
			{
				ListBoxItem itm = new ListBoxItem();
				itm.Content = s;
				ActionList.Items.Add(itm);
			}
			ActionList.Visibility = Visibility.Visible;
			int i = 0;
			_tokenSource2 = new CancellationTokenSource();
			CancellationToken ct =  _tokenSource2.Token;

			var t = Task.Run(() =>
			{
				try
				{


					foreach (var line in Serial)
					{
						ActionList.Dispatcher.Invoke((Action) (() => { ActionList.SelectedIndex = i++; }));
						if (ct.IsCancellationRequested)
						{
							// Clean up here, then...
							break;
						}

						Executor.Run(line)?.ConfigureAwait(false);
					}
				}
				catch (OperationCanceledException oexc)
				{
					//ignored
				}
			}, ct);
		}

	
		private void SaveMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sd = new SaveFileDialog
			{
				Filter = @"(*.log)|*.log|(All Files *.*)|*.*"
			};
			if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				File.WriteAllLines(sd.FileName, Serial);
				Properties.Settings.Default.LastOpenedScript = sd.FileName;
			}
		}

		private void SetOptions(object sender, RoutedEventArgs e)
		{
			optWnd.ShowDialog();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			//base.OnClosed(e);
			Application.Current.Shutdown();
		}

		private void Image_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{

		}

		private void StopButtonClick(object sender, RoutedEventArgs e)
		{
			_tokenSource2.Cancel();
			ActionList.Visibility = Visibility.Hidden;
			ActionListBox.Visibility = Visibility.Visible;
			ActionListBox.IsEnabled = true;
		}
	}
}
