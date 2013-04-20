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

namespace wpfTwitterApp
{
    /// <summary>
    /// Interaction logic for TwitterHome.xaml
    /// </summary>
    public partial class TwitterHome : UserControl
    {
        private String[] twitterOptions = {
                                              "Twitter Feed",   // 0
                                              "Photobooth"      // 1
                                          };

        public TwitterHome()
        {
            InitializeComponent();
            updateSelected();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }

        private void TwitterHomeElementFlow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateSelected();
        }

        private void updateSelected()
        {
            TwitterHomeSelectedText.Content = twitterOptions[TwitterHomeElementFlow.SelectedIndex];
        }

        public void moveRight()
        {
            TwitterHomeElementFlow.SelectedIndex++;
        }

        public void moveLeft()
        {
            TwitterHomeElementFlow.SelectedIndex--;
        }
    }
}
