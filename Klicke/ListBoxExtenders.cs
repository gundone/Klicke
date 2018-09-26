using System;
using System.Windows;
using System.Windows.Controls;

namespace Klicke
{
	public static class ScrollCurrentItemIntoViewBehavior
	{
		public static readonly DependencyProperty AutoScrollToCurrentItemProperty =
			DependencyProperty.RegisterAttached("AutoScrollToCurrentItem",
				typeof(bool), typeof(ScrollCurrentItemIntoViewBehavior),
				new UIPropertyMetadata(default(bool), OnAutoScrollToCurrentItemChanged));

		public static bool GetAutoScrollToCurrentItem(DependencyObject obj)
		{
			return (bool)obj.GetValue(AutoScrollToCurrentItemProperty);
		}

		public static void OnAutoScrollToCurrentItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			if (!(obj is ListBox listBox)) return;

			var newValue = (bool)e.NewValue;
			if (newValue)
				listBox.SelectionChanged += ListBoxSelectionChanged;
			else
				listBox.SelectionChanged -= ListBoxSelectionChanged;
		}

		public static void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!(sender is ListBox listBox) || listBox.SelectedItem == null) return;

			listBox.Items.MoveCurrentTo(listBox.SelectedItem);
			listBox.ScrollIntoView(listBox.SelectedItem);
		}

		public static void SetAutoScrollToCurrentItem(DependencyObject obj, bool value)
		{
			obj.SetValue(AutoScrollToCurrentItemProperty, value);
		}
	}
}