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
using Microsoft.Kinect;
using LinqToTwitter;
using System.IO;
using System.Globalization;

namespace wpfTwitterApp
{
    /// <summary>
    /// Interaction logic for Photobooth.xaml
    /// </summary>
    public partial class Photobooth : UserControl
    {
        KinectSensor kinectsensor; // Active Kinect sensor

        private WriteableBitmap colorBitmap; // Bitmap that will hold color information
        private byte[] colorPixels; // Intermediate storage for the color data received from the camera
        

        public Photobooth()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {

            kinectsensor = KinectSensor.KinectSensors[0];

            // Turn on the color stream to receive color frames
            this.kinectsensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            // Allocate space to put the pixels we'll receive
            this.colorPixels = new byte[this.kinectsensor.ColorStream.FramePixelDataLength];

            // This is the bitmap we'll display on-screen
            this.colorBitmap = new WriteableBitmap(this.kinectsensor.ColorStream.FrameWidth, this.kinectsensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            kinectsensor.ColorFrameReady += kinectsensor_ColorFrameReady;
        }

        void kinectsensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
            this.KinectStream.Source = this.colorBitmap;
        }

        // take a snapshot on push gesture, and tweet it
        public void tweetSnapShot(TwitterContext ctx)
        {
            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);
            string saveDirectory = @"..\..\Snapshots";
            string filename = "KinectSnapshot-" + time + ".png";
            string path = System.IO.Path.Combine(saveDirectory, filename);

            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Saving Snapshot Failed");
            }

            var mediaItems =
            new List<Media>
            {
                new Media
                {
                    Data = Utilities.GetFileBytes(path),
                    FileName = filename,
                    ContentType = MediaContentType.Png
                }
            };
            // tweeting with media takes the picture file and the status message
            var tweet = ctx.TweetWithMedia("Tweeting with media! #KineXionZ",false,mediaItems);
        }
    }
}
