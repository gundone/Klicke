using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using CommandProcessor;
using Application = System.Windows.Application;
using InputSimulator;
using Window = System.Windows.Window;

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
		public static readonly RoutedCommand Save = new RoutedCommand();

		public static readonly RoutedCommand SaveAs = new RoutedCommand();

		public static readonly RoutedCommand Open = new RoutedCommand();

		private void QuickSave(object sender, ExecutedRoutedEventArgs e)
		{
			var fileName = Properties.Settings.Default.LastOpenedScript;
			File.WriteAllText(fileName, ActionTextBox.Text);
			Properties.Settings.Default.LastOpenedScript = fileName;
			Properties.Settings.Default.Save();
		}

		public MainWindow()
		{
			InitializeComponent();
			Save.InputGestures.Add( new KeyGesture(Key.S , ModifierKeys.Control ));
			SaveAs.InputGestures.Add( new KeyGesture(Key.S , ModifierKeys.Control | ModifierKeys.Shift ));
			Open.InputGestures.Add( new KeyGesture(Key.O, ModifierKeys.Control));
			//ActionTextBox.Document.PageWidth = 1000;
			if (Properties.Settings.Default.OpenLastScript 
			    && File.Exists(Properties.Settings.Default.LastOpenedScript))
			{
				//ActionTextBox.Document.Blocks.Clear();
				//ActionTextBox.Document.Blocks.Add(new Paragraph(new Run( string.Join("\r\n", File.ReadAllLines(Properties.Settings.Default.LastOpenedScript)))));
				ActionTextBox.Text = File.ReadAllText(Properties.Settings.Default.LastOpenedScript);
				Title = "Klicke - " + Properties.Settings.Default.LastOpenedScript;
			}
		}

		private void ExitMenuItemClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void LoadMenuItem_OnClick(object sender, ExecutedRoutedEventArgs e)
		{
			OpenFileDialog od = new OpenFileDialog
			{
				Filter = @"(*.log)|*.log|(All Files *.*)|*.*"
			};
			if (od.ShowDialog() == System.Windows.Forms.DialogResult.OK && od.CheckFileExists)
			{
				ActionTextBox.Text = string.Join("\r\n", File.ReadAllLines(od.FileName));
				Properties.Settings.Default.LastOpenedScript = od.FileName;
				Title = "Klicke - " + od.FileName;
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
			//ActionTextBox.Clear();
			if (Properties.Settings.Default.AddActionsToCursor)
			{
				var index = ActionTextBox.CaretIndex;
				ActionTextBox.Text = ActionTextBox.Text.Insert(index, action + Environment.NewLine);
				ActionTextBox.CaretIndex = index + (action + Environment.NewLine).Length;
			}
				
			else
				ActionTextBox.Text += action + Environment.NewLine;
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

			if (IsWindowOpen<AddItemWindow>("Options"))
			{
				Application
					.Current
					.Windows
					.OfType<AddItemWindow>()
					.First(w => w.Name.Equals("Options"))
					.Close();
			}
		}

		List<Tag> m_tags = new List<Tag>();
		private List<Tag> m_comments = new List<Tag>();
		


		public static bool IsKnownTag(string tag)
		{
			string[] specialWords = { "WaitForWindow", "Sleep", "MouseAction", "KeyboardAction", "OpenProgram" };              
			var tags = new List<string>(specialWords);     
			// We also want to know all possible delimiters so adding this stuff.     
			
			return tags.Exists(delegate(string s) { return s.ToLower().Equals(tag.ToLower()); });
		}

		public void FindCommentedStrings(Run theRun)
		{
			var lines = Regex.Split(theRun.Text, "\r\n|\n");
			var index = 0;
			foreach (var line in lines)
			{
				if (line.Length >0 && line[0] == '#')
				{
					Tag t = new Tag();
					var sIndex = theRun.Text.IndexOf(line, index, StringComparison.Ordinal);
					var eIndex = sIndex + line.Length;
					t.StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex, LogicalDirection.Forward);
					t.EndPosition   = theRun.ContentStart.GetPositionAtOffset(eIndex + 1, LogicalDirection.Backward);
					t.Word = line;
					m_comments.Add(t);
					index = eIndex;
				}
			}
		}


		private static bool GetSpecials(char i)
		{
			char[] chrs = {
				'.',
				')',
				'(',
				'[',
				']',
				'>',
				'<',
				':',
				';',
				'\n',
				'\t',
				'\r',
				'='
			};
			var specials = new List<char>(chrs);
			foreach (var item in specials)
			{
				if (item.Equals(i))
				{
					return true;
				}
			}
			return false;
		}
		internal void CheckWordsInRun(Run theRun) //do not hightlight keywords in this method
		{
			//How, let's go through our text and save all tags we have to save.               
			int sIndex = 0;
			int eIndex = 0;
			var text = ActionTextBox.Text;
			for (int i = 0; i < text.Length; i++)
			{
				if (Char.IsWhiteSpace(text[i]) | GetSpecials(text[i]))
				{
					if (i > 0 && !(Char.IsWhiteSpace(text[i - 1]) | GetSpecials(text[i - 1])))
					{
						eIndex = i - 1;
						string word = text.Substring(sIndex, eIndex - sIndex + 1);
						Tag t = new Tag
						{
							StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex, LogicalDirection.Forward),
							EndPosition   = theRun.ContentStart.GetPositionAtOffset(eIndex + 1, LogicalDirection.Backward),
							Word = word
						};

						if (!(t.StartPosition is null || t.EndPosition is null) && IsKnownTag(word))
						{
							m_tags.Add(t);
						}
					}
					sIndex = i + 1;
				}
			}
			//How this works. But wait. If the word is last word in my text I'll never hightlight it, due I'm looking for separators. Let's add some fix for this case
			string lastWord = text.Substring(sIndex, text.Length - sIndex);
			if (IsKnownTag(lastWord))
			{
				Tag t = new Tag();
				t.StartPosition = theRun.ContentStart.GetPositionAtOffset(sIndex, LogicalDirection.Forward);
				t.EndPosition = theRun.ContentStart.GetPositionAtOffset(text.Length, LogicalDirection.Backward); //fix 1
				t.Word = lastWord;
				m_tags.Add(t);
			}
		}

		private int _runs = 0;
		private void ActionListBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//if (ActionTextBox.Document == null)
			//	return;
			
			//ActionTextBox.TextChanged -= ActionListBox_TextChanged;
			
			//m_tags.Clear();
			//m_comments.Clear();

			////first clear all the formats
			//TextRange documentRange = new TextRange(ActionTextBox.Document.ContentStart, ActionTextBox.Document.ContentEnd);
			//documentRange.ClearAllProperties();
			////text = documentRange.Text; //fix 2
			//if (true)
			//{
			//	//Now let's create navigator to go though the text, find all the keywords but do not hightlight
			//	TextPointer navigator = ActionTextBox.Document.ContentStart;
				
				
			//	while (navigator?.CompareTo(ActionTextBox.Document.ContentEnd) < 0)
			//	{
			//		TextPointerContext context = navigator.GetPointerContext(LogicalDirection.Backward);
			//		if (context == TextPointerContext.ElementStart && navigator.Parent is Run)
			//		{
			//			var text = ((Run)navigator.Parent).Text; //fix 2
			//			if (text != "")
			//			{	
			//				CheckWordsInRun((Run)navigator.Parent);
			//				_runs++;
			//			}
							
			//		}
			//		navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
			//	}

			//	//only after all keywords are found, then we highlight them
			//	for (int i = 0; i < m_comments.Count; i++)
			//	{
			//		try
			//		{
			//			TextRange range = new TextRange(m_comments[i].StartPosition, m_comments[i].EndPosition);
			//			range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Tomato));
			//			range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
			//		}
			//		catch { }
			//	}
			//	for (int i = 0; i < m_tags.Count; i++)
			//	{
			//		try
			//		{
			//			TextRange range = new TextRange(m_tags[i].StartPosition, m_tags[i].EndPosition);
			//			range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.DarkCyan));
			//			range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
			//		}
			//		catch { }
			//	}
			//}
			
			//ActionTextBox.TextChanged += ActionListBox_TextChanged;
			Serial.Clear();

			var lines = Regex.Split(ActionTextBox.Text, @"\r?\n|\r")
				.Where(line =>line.Length > 0 && line[0] != '#');
			Serial.AddRange(lines);
		}

		CancellationTokenSource _tokenSource2 = new CancellationTokenSource();
		
		

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			ActionTextBox.IsEnabled = false;
			ActionTextBox.Visibility = Visibility.Hidden;
			ActionList.Items.Clear();
			foreach (var s in Serial)
			{
				ListBoxItem itm = new ListBoxItem {Content = s};
				ActionList.Items.Add(itm);
			}
			ActionList.Visibility = Visibility.Visible;
			int i = 0;
			_tokenSource2 = new CancellationTokenSource();
			CancellationToken ct =  _tokenSource2.Token;
			Executor.Report += (s, args) =>
			{
				Dispatcher.Invoke(() =>
				{
					ExecutorMessage.Visibility = string.IsNullOrWhiteSpace(args.Message) ? Visibility.Hidden : Visibility.Visible;
					ExecutorMessage.Text = args.Message;
				});
			}; 
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
			}, ct).ContinueWith(_ =>
			{
				this.Dispatcher.Invoke(
				(Action) (() =>
				{
					ActionList.Visibility = Visibility.Hidden;
					ActionTextBox.Visibility = Visibility.Visible;
					ActionTextBox.IsEnabled = true;
				}));
			}, ct);
		}

		private void StepButton_Click(object sender, RoutedEventArgs e)
		{
			ActionTextBox.IsEnabled = false;
			ActionTextBox.Visibility = Visibility.Hidden;
			ActionList.Items.Clear();


			var linesToRun = Regex.Split(ActionTextBox.SelectedText, "\r\n|\n");
			foreach (var s in linesToRun)
			{
				ListBoxItem itm = new ListBoxItem {Content = s};
				ActionList.Items.Add(itm);
			}
			ActionList.Visibility = Visibility.Visible;
			int i = 0;
			_tokenSource2 = new CancellationTokenSource();
			CancellationToken ct =  _tokenSource2.Token;
			Executor.Report += (s, args) =>
			{
				Dispatcher.Invoke(() =>
				{
					ExecutorMessage.Visibility = string.IsNullOrWhiteSpace(args.Message) ? Visibility.Hidden : Visibility.Visible;
					ExecutorMessage.Text = args.Message;
				});
			}; 
			var t = Task.Run(() =>
			{
				try
				{
					foreach (var line in linesToRun)
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
			}, ct).ContinueWith(_ =>
			{
				this.Dispatcher.Invoke(
					(Action) (() =>
					{
						ActionList.Visibility = Visibility.Hidden;
						ActionTextBox.Visibility = Visibility.Visible;
						ActionTextBox.IsEnabled = true;
						StepButton.IsEnabled = false;
					}));
			}, ct);
		}


	
		private void SaveAsMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sd = new SaveFileDialog
			{
				Filter = @"(*.log)|*.log|(All Files *.*)|*.*"
			};
			if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				File.WriteAllText(sd.FileName, ActionTextBox.Text);
				Properties.Settings.Default.LastOpenedScript = sd.FileName;
				Properties.Settings.Default.Save();
				Title = "Klicke - " + sd.FileName;
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
			Executor.StopAll();
			_tokenSource2.Cancel();
			ActionList.Visibility = Visibility.Hidden;
			ActionTextBox.Visibility = Visibility.Visible;
			ActionTextBox.IsEnabled = true;
			
		}

		private void ActionTextBox_OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			StepButton.IsEnabled = !string.IsNullOrWhiteSpace(ActionTextBox.SelectedText);
		}


		private void SaveMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			Save.Execute(this, null);
		}

		private void Button_OnClick(object sender, RoutedEventArgs e)
		{
			var xpymep = InputSimulator.Window.FindWindow("Notepad", null);
			var process = Process.GetProcessesByName("xpymep").FirstOrDefault();
			if (process != null)
			{
				var threads = process.Threads;
				ActionTextBox.Text = threads.Count.ToString();
			}
			
		}
	}
}
