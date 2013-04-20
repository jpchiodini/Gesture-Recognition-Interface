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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TwitterHome twitterHome;
        private TwitterFeed twitterFeed;
        private Photobooth photobooth;
        private TwitterEngine twitEngine;

        private int AppState; // State variable for the application
        /*
         * 0: Twitter Home
         * 1: Feed
         * 2: Photo App
         */

        public MainWindow()
        {
            InitializeComponent();

            twitEngine = new TwitterEngine();

            twitEngine.initializeTwitterEngine(); // initialize and authorize twitter credentials

            twitterHome = new TwitterHome(); // twitter home screen
            twitterFeed = new TwitterFeed(twitEngine.getTwitterContext()); // twitter feed user control

            photobooth = new Photobooth();

            //Initialize the Kinect gesture engine
            GestureEngine gestEngine = new GestureEngine();

            gestEngine.reset();
            gestEngine.init();

            //adds an event handler to the event that a new gesture is done
            gestEngine.GestureChanged += gestEngine_GestureChanged;

            AppState = 0; // initialize state

            this.pageTransitionControl.ShowPage(twitterHome);
        }

        void gestEngine_GestureChanged(int newGestureID)
        {
            if (AppState == 0)
            {
                if (newGestureID == 0) // RS
                    twitterHome.moveRight();
                if (newGestureID == 1) // LS
                    twitterHome.moveLeft();
                if (newGestureID == 2 || newGestureID == 3) // RP or LP
                {
                    if (twitterHome.TwitterHomeElementFlow.SelectedIndex == 0)
                    {
                        twitterFeed = new TwitterFeed(twitEngine.getTwitterContext()); // update twitter feed
                        pageTransitionControl.ShowPage(twitterFeed);
                        AppState = 1;
                        return;
                    }
                    if (twitterHome.TwitterHomeElementFlow.SelectedIndex == 1)
                    {
                        pageTransitionControl.ShowPage(photobooth);
                        AppState = 2;
                        return;
                    }
                }
            }

            if (AppState == 1)
            {
                if (newGestureID == 0) //RS
                    twitterFeed.moveUpTwitterFeed();
                if (newGestureID == 1) //LS
                    twitterFeed.moveDownTwitterFeed();
                if (newGestureID == 4 || newGestureID == 5) // RB or LB
                {
                    pageTransitionControl.ShowPage(twitterHome);
                    AppState = 0;
                    return;
                }
            }

            if (AppState == 2)
            {
                if (newGestureID == 2 || newGestureID == 3) // RP or LP
                {
                    photobooth.tweetSnapShot(twitEngine.getTwitterContext()); // take a snapshot and tweet it
                    
                    twitterFeed = new TwitterFeed(twitEngine.getTwitterContext()); // update twitter feed
                    
                    pageTransitionControl.ShowPage(twitterFeed);
                    AppState = 1;
                    return;
                }
                if (newGestureID == 4 || newGestureID == 5) // RB or LB
                {
                    pageTransitionControl.ShowPage(twitterHome);
                    AppState = 0;
                    return;
                }
            }
        }
    }
}
