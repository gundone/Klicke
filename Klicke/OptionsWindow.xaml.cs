using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Klicke
{
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow : Window
	{
		public OptionsWindow()
		{
			InitializeComponent();
			AddActionsToCursor.IsChecked = Properties.Settings.Default.AddActionsToCursor;
			OpenLastScript.IsChecked     = Properties.Settings.Default.OpenLastScript;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Hide();
			e.Cancel = true;
			Properties.Settings.Default.Save();
		}

		private void OnAddActionsCheckboxSettingChanged(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.AddActionsToCursor = AddActionsToCursor.IsChecked ?? false;
			Properties.Settings.Default.Save();
		}

		private void OpenLastScriptChecked(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.OpenLastScript = OpenLastScript.IsChecked ?? false;
			Properties.Settings.Default.Save();
		}
	}
}
