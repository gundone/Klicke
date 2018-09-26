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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Klicke
{
	/// <summary>
	/// Interaction logic for RichTextBoxE.xaml
	/// </summary>
	public partial class RichTextBoxE 
	{
		public RichTextBoxE()
		{
			InitializeComponent();
			DefaultStyleKeyProperty.OverrideMetadata(typeof(RichTextBoxE), new FrameworkPropertyMetadata(typeof(RichTextBoxE)));
		}
		
		public static readonly DependencyProperty TextProperty = 
			DependencyProperty.Register(
				"Text", 
				typeof(string),
				typeof(RichTextBoxE),
				new FrameworkPropertyMetadata(
					string.Empty, 
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
					new PropertyChangedCallback(ValuePropertyChanged)));

		public static readonly RoutedEvent FuckYouChangedEvent = 
			EventManager.RegisterRoutedEvent(
				"YouFucked",
				RoutingStrategy.Bubble, 
				typeof(TextChangedEventHandler), 
				typeof(RichTextBoxE));

		public event TextChangedEventHandler YouFucked
		{
			add => AddHandler(FuckYouChangedEvent, value);
			remove => RemoveHandler(FuckYouChangedEvent, value);
		}

		public static readonly RoutedEvent TapEvent = EventManager.RegisterRoutedEvent(
			"Tap", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RichTextBoxE));

		// Provide CLR accessors for the event
		public event RoutedEventHandler Tap
		{
			add { AddHandler(TapEvent, value); } 
			remove { RemoveHandler(TapEvent, value); }
		}


		private static void ValuePropertyChanged(
			DependencyObject d,
			DependencyPropertyChangedEventArgs e)
		{
			RichTextBoxE x = (RichTextBoxE)d;
			x.Text = (string)e.NewValue;
		}


		public string Text
		{
			get => new TextRange(Document.ContentStart, Document.ContentEnd).Text;
			set
			{
				Document.Blocks.Clear();
				Document.Blocks.Add(new Paragraph(new Run(value)));
			}
		}

		public int CaretIndex
		{
			get => CaretPosition.DocumentStart.GetOffsetToPosition(CaretPosition);
			set => CaretPosition = CaretPosition.DocumentStart.GetPositionAtOffset(value);
		}

	}
}
