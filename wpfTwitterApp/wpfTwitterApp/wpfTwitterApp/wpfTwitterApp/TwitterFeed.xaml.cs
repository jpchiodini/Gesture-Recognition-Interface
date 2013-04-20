using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Net;
using System.Diagnostics;
using System.IO;
using LinqToTwitter;
using System.Windows.Threading;

namespace wpfTwitterApp
{
    /// <summary>
    /// Interaction logic for TwitterFeed.xaml
    /// </summary>
    public partial class TwitterFeed : UserControl
    {
        List<Tweet> twitterFeedList; // List of Tweet objects

        TwitterContext twitterCtx;

        public TwitterFeed()
        {
            InitializeComponent();
        }

        public TwitterFeed(TwitterContext currCtx)
        {
            InitializeComponent();

            twitterCtx = currCtx;
            updateTweetList();
        }

        public void setTwitterContext(TwitterContext currCtx)
        {
            twitterCtx = currCtx;
            
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            
        }

        public void updateText()
        {
            int selected = TwitterElementFlow.SelectedIndex; //current selection

            string currName = twitterFeedList[selected].Name;
            string currText = twitterFeedList[selected].TweetText;
            string currTime = twitterFeedList[selected].DateTime;

            selectedTweetText.Text = currName + "\n\n" + currText + "\n\n" + currTime;
        }

        private void TwitterElementFlow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateText();
        }

        public void updateTweetList()
        {
            twitterFeedList = new List<Tweet>();

            var tweets =
                from tweet in twitterCtx.Status
                where tweet.Type == StatusType.Home
                select tweet;

            Console.WriteLine("\nTweets:\n");

            List<Status> tweetList = tweets.ToList();

            foreach (Status tweet in tweetList)
            {
                // Create new Tweet and add them into the feed list
                Tweet curr = new Tweet();
                curr.TweetText = tweet.Text;
                curr.Name = tweet.User.Name;
                curr.ImageLink = tweet.User.ProfileImageUrl;
                curr.DateTime = tweet.CreatedAt.ToLocalTime().ToString();

                Console.WriteLine(curr.DateTime);
                Console.WriteLine(curr.TweetText);
                Console.WriteLine(curr.Name);
                Console.WriteLine(curr.ImageLink);
                Console.WriteLine();

                twitterFeedList.Add(curr);
            }
            twitterFeedList.Reverse();

            TwitterElementFlow.ItemsSource = twitterFeedList;
            updateText();
        }

        // Move Up and Down twitter feed
        public void moveUpTwitterFeed()
        {
            TwitterElementFlow.SelectedIndex++;
        }

        public void moveDownTwitterFeed()
        {
            TwitterElementFlow.SelectedIndex--;
        }

    }
}
