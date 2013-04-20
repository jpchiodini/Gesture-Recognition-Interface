using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElementFlowTest
{
	/// <summary>
	/// Interaction logic for StartScreen.xaml
	/// </summary>
	public partial class StartScreen : UserControl
	{
		String[] itemNames = {
                                 "Boston University Shuttle",   //0
                                 "Upcoming Events",             //1
                                 "ECE Faculty",                 //2
                                 "ECE News"                     //3
                             };                   
		
		public StartScreen()
		{
			this.InitializeComponent();
			selectedLabel.Content = itemNames[_elementFlow.SelectedIndex];
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }

		public void incrementSelected()
		{
			int newIndex = ++_elementFlow.SelectedIndex;
			if (newIndex < 4)
			{
				selectedLabel.Content = itemNames[newIndex];
			}
		}
		public void decrementSelected()
		{
			int newIndex = --_elementFlow.SelectedIndex;
			if (newIndex >= 0)
			{
				selectedLabel.Content = itemNames[newIndex];
			}
		}
        public int getCurrIndex()
        {
            return _elementFlow.SelectedIndex;
        }
	}
}