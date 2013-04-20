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
    /// Interaction logic for Faculty.xaml
    /// </summary>
    public partial class Faculty : UserControl
    {

        private List<FacultyMember> FMList;

        public Faculty()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
            selectedFacultyLabel.Content = FMList[FacultyElementFlow.SelectedIndex].Name;
        }

        public void loadXmlDoc()
        {
            FMList = new List<FacultyMember>();

            String xmlSource = "http://tinyurl.com/btrbbke";

            XmlDocument facultyXmlDoc = new XmlDocument();
            facultyXmlDoc.Load(xmlSource);

            XmlNodeList title_list = facultyXmlDoc.GetElementsByTagName("title");
            XmlNodeList link_list = facultyXmlDoc.GetElementsByTagName("link");
            XmlNodeList d_list = facultyXmlDoc.GetElementsByTagName("description");

            for (int i = 1; i < d_list.Count; i++)
            {
                String dshiz = d_list[i].InnerText;

                XmlDocument newDoc = new XmlDocument();
                newDoc.LoadXml(dshiz);

                XmlNodeList img_list = newDoc.GetElementsByTagName("img");
                String imglink = img_list[0].Attributes["src"].Value;

                String title = title_list[i].InnerText;
                String link = link_list[i].InnerText;

                FacultyMember fm = new FacultyMember();
                fm.Name = title;
                fm.BrowserLink = link;
                fm.ImageLink = imglink;

                FMList.Add(fm);
            }
            FacultyElementFlow.ItemsSource = FMList;
        }

        public String getFacultyURL()
        {
            return FMList[FacultyElementFlow.SelectedIndex].BrowserLink;
        }

        public void moveRight()
        {
            int currIndex = FacultyElementFlow.SelectedIndex;
            int numItems = FacultyElementFlow.Items.Count;

            if ((currIndex + 1) < numItems)
            {
                FacultyElementFlow.SelectedIndex = currIndex + 1;
                selectedFacultyLabel.Content = FMList[FacultyElementFlow.SelectedIndex].Name;
            }
        }

        public void moveLeft()
        {
            int currIndex = FacultyElementFlow.SelectedIndex;
            if ((currIndex - 1) >= 0)
            {
                FacultyElementFlow.SelectedIndex = currIndex - 1;
                selectedFacultyLabel.Content = FMList[FacultyElementFlow.SelectedIndex].Name;
            }
        }

    }
}
