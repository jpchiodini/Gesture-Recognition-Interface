using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LinqToTwitter;

namespace wpfTwitterApp
{
    class TwitterEngine
    {
        TwitterContext twitterCtx; // twitter context variable after authentication

        const string CredentialsFile = "credentials.txt";

        public void initializeTwitterEngine()
        {
            ITwitterAuthorizer auth = PerformAuthorization();

            if (auth == null)
            {
                Console.Write("\nPress any key to exit...");
                Console.ReadKey();

                return;
            }

            twitterCtx = new TwitterContext(auth);
        }

        public TwitterContext getTwitterContext()
        {
            return twitterCtx;
        }

        // From CredentialToStringAndLoadDemo
        static ITwitterAuthorizer PerformAuthorization()
        {
            InMemoryCredentials credentials;

            // validate that credentials are present
            if (!GetCredentials(out credentials))
            {
                return null;
            }

            // configure the OAuth object
            var auth = new PinAuthorizer
            {
                Credentials = credentials,
                UseCompression = true,
                GoToTwitterAuthorization = pageLink => Process.Start(pageLink),
                GetPin = () =>
                {
                    // this executes after user authorizes, which begins with the call to auth.Authorize() below.
                    Console.WriteLine("\nAfter you authorize this application, Twitter will give you a 7-digit PIN Number.\n");
                    Console.Write("Enter the PIN number here: ");
                    return Console.ReadLine();
                }
            };

            // start the authorization process (launches Twitter authorization page).
            try
            {
                auth.Authorize();
            }
            catch (WebException wex)
            {
                MessageBox.Show(
                    "Unable to authorize with Twitter right now. Please check pin number" +
                    " and ensure your credential keys are correct. Exception received: " + wex.ToString());

                return null;
            }

            File.WriteAllLines(CredentialsFile, new string[] { auth.Credentials.ToString() });

            return auth;
        }

        static bool GetCredentials(out InMemoryCredentials credentials)
        {
            credentials = new InMemoryCredentials();

            if (File.Exists(CredentialsFile))
            {
                string[] lines = File.ReadAllLines(CredentialsFile);
                if (lines != null && lines.Length > 0)
                {
                    credentials.Load(lines[0]);
                    return true;
                }
            }

            // validate that credentials are present
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["twitterConsumerKey"]) ||
                string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["twitterConsumerSecret"]))
            {
                MessageBox.Show("\nCan't Run Yet\n" +
                                  "-------------\n" +
                                  "You need to set twitterConsumerKey and twitterConsumerSecret \n" +
                                  "in App.config/appSettings.\nPlease visit http://dev.twitter.com/apps for more info.\n");
                return false;
            }

            credentials = new InMemoryCredentials
            {
                ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"],
                ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"]
            };

            return true;
        }
    }
}
