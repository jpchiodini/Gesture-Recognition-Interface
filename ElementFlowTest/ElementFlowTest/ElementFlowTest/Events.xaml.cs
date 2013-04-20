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
using System.Xml;

namespace ElementFlowTest
{
    /// <summary>
    /// Interaction logic for Events.xaml
    /// </summary>
    public partial class Events : UserControl
    {
        public Events()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }

        public String getEventsURL()
        {
            int selectedIndex = listItems.SelectedIndex + 1;
            Console.WriteLine(selectedIndex);

            String xmlSource = "http://tinyurl.com/b7juaff";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlSource);

            XmlNodeList links = xmlDoc.GetElementsByTagName("link");

            String url = links[selectedIndex].InnerText;

            return url;
        }

        public void moveDownList()
        {
            int currIndex = listItems.SelectedIndex;
            int numItems = listItems.Items.Count;

            if ((currIndex + 1) < numItems)
            {
                listItems.SelectedIndex = currIndex + 1;
            }
        }

        public void moveUpList()
        {
            int currIndex = listItems.SelectedIndex;
            if ((currIndex - 1) >= 0)
            {
                listItems.SelectedIndex = currIndex - 1;
            }
        }

    }
}
