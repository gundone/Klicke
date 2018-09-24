using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

		public MainWindow()
		{
			InitializeComponent();
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
			}
		}

		private void ActionListBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			Serial.Clear();
			Serial.AddRange(Regex.Split(ActionListBox.Text, @"\r?\n|\r"));
		}

		private async void StartButton_Click(object sender, RoutedEventArgs e)
		{
			ActionListBox.IsEnabled = false;
			await Task.Run(()=>{
				foreach (var t in Serial)
				{
					Executor.Run(t)?.ConfigureAwait(false);
				}
			});
			ActionListBox.IsEnabled = true;
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
			}
		}
	}
}
