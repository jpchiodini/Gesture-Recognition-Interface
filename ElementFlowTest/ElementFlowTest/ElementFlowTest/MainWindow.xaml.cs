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
using FluidKit.Controls;
using WpfPageTransitions;

namespace ElementFlowTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StartScreen startScreen;
        private NewsPage newsPage;
        private Faculty faculty;
        private WebBrowser browser;
        private Events events;

        public MainWindow()
        {
            InitializeComponent();
            
            // Create instances of UserControls
            startScreen = new StartScreen();
            newsPage = new NewsPage();
            faculty = new Faculty();
            events = new Events();
            browser = new WebBrowser();

            faculty.loadXmlDoc();

            // Show the startScreen 
            pageTransitionControl.ShowPage(startScreen);

            //pageTransitionControl.ShowPage(faculty);

            //Initialize the Kinect gesture engine
            GestureEngine gestEngine = new GestureEngine();

            gestEngine.reset();
            gestEngine.init();

            //adds an event handler to the event that a new gesture is done
            gestEngine.GestureChanged += new ElementFlowTest.GestureEngine.NewGestureEventHandler(gestEngine_GestureChanged);
        }

        private void gestEngine_GestureChanged(int newGestureID)
        {
            // Psuedo state machine depending on the gesture, current state, and last state, generates the correct action

            if (newGestureID == 0) //RS
            {
                if (startScreen.IsFocused)
                {
                    startScreen.incrementSelected();
                }
                if (newsPage.IsFocused)
                {
                    newsPage.moveUpList();
                }
                if (faculty.IsFocused)
                {
                    faculty.moveRight();
                }
                if (events.IsFocused)
                {
                    events.moveUpList();
                }
                if (browser.IsFocused)
                {
                    browser.scrollUp();
                }
            }


            if (newGestureID == 1) //LS
            {
                if (startScreen.IsFocused)
                {
                    startScreen.decrementSelected();
                }
                if (newsPage.IsFocused)
                {
                    newsPage.moveDownList();
                }
                if (faculty.IsFocused)
                {
                    faculty.moveLeft();
                }
                if (events.IsFocused)
                {
                    events.moveDownList();
                }
                if (browser.IsFocused)
                {
                    browser.scrollDown();
                }
            }

            if (newGestureID == 2 || newGestureID == 3) //RP or LP
            {
                if (startScreen.IsFocused)
                {
                    if (startScreen.getCurrIndex() == 0)
                    {
                        browser.setBrowserURL("http://www.bu.edu/thebus/live-view/");
                        pageTransitionControl.ShowPage(browser);
                    }
                    if (startScreen.getCurrIndex() == 1)
                    {
                        pageTransitionControl.ShowPage(events);
                    }
                    if (startScreen.getCurrIndex() == 2)
                    {
                        pageTransitionControl.ShowPage(faculty);
                    }
                    if (startScreen.getCurrIndex() == 3)
                    {
                        pageTransitionControl.ShowPage(newsPage);
                    }
                }
                if (newsPage.IsFocused)
                {
                    browser.setBrowserURL(newsPage.getLinkToSelected());
                    pageTransitionControl.ShowPage(browser);
                }
                if (faculty.IsFocused)
                {
                    browser.setBrowserURL(faculty.getFacultyURL());
                    pageTransitionControl.ShowPage(browser);
                }
                if (events.IsFocused)
                {
                    browser.setBrowserURL(events.getEventsURL());
                    pageTransitionControl.ShowPage(browser);
                }
            }


            if (newGestureID == 4 || newGestureID == 5) //Close
            {
                pageTransitionControl.ShowPage(startScreen);
            }
            
        }
    }
}
