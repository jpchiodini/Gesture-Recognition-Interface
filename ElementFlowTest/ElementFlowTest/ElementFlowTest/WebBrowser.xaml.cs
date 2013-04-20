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

namespace ElementFlowTest
{
    /// <summary>
    /// Interaction logic for WebBrowser.xaml
    /// </summary>
    public partial class WebBrowser : UserControl
    {
        public WebBrowser()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }

        public void setBrowserURL(String newURL)
        {
            Uri u = new Uri(newURL);
            browserObject.Source = u;
        }

        public void scrollUp()
        {
            mshtml.HTMLDocument htmlDoc = browserObject.Document as mshtml.HTMLDocument;
            if (htmlDoc != null) htmlDoc.parentWindow.scrollBy(0, -200);
        }
        public void scrollDown()
        {
            mshtml.HTMLDocument htmlDoc = browserObject.Document as mshtml.HTMLDocument;
            if (htmlDoc != null) htmlDoc.parentWindow.scrollBy(0, 200);
        }
    }
}
