using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Start_Screen_Graphical;
using System.Diagnostics;


namespace cameraTest_1
{
    public partial class Form1 : Form
    {
        //initialize gesture engine
        public int currentGesture;
        GestureEngine gestEng;
        private bool gesture_lock = false;

        public Form1(GestureEngine gestEng)
        {
            //initialize
            this.gestEng = gestEng;
            gestEng.GestureChanged += new Start_Screen_Graphical.GestureEngine.NewGestureEventHandler(gestEngine_GestureChanged);
            InitializeComponent();
        }
        public void pause()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {                
                if (stopwatch.ElapsedMilliseconds >= 2000)
                {
                    break;
                }
            }
        }

        private void gestEngine_GestureChanged(int newGestureID)
        {
            
            currentGesture = newGestureID;
            //temp directions
            //0 left
            //1 right
            //2 up
            //3 down
            //4 start
            //5 stop
            //6 zoom in
            //7 zoom out
            //8 exit
            //zoom
            if (gesture_lock == false)
            {
                //Stopwatch stopwatch = Stopwatch.StartNew();
                switch (currentGesture)
                {

                    //left
                    case 0:
                        gesture_lock = true;
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/com/ptz.cgi?camera=1&move=left";
                        AMC.MediaType = "mjpg";
                        AMC.Play();
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/mjpg/video.cgi";
                        AMC.Play();
                        pause();
                        gesture_lock = false;
                        break;
                    //right
                    case 1:
                        gesture_lock = true;
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/com/ptz.cgi?camera=1&move=right";
                        AMC.MediaType = "mjpg";
                        AMC.Play();
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/mjpg/video.cgi";
                        AMC.Play();
                        pause();
                        gesture_lock = false;
                        break;
                    //up
                    case 2:
                        gesture_lock = true;
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/com/ptz.cgi?camera=1&move=up";
                        AMC.MediaType = "mjpg";
                        AMC.Play();
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/mjpg/video.cgi";
                        AMC.Play();
                        pause();
                        gesture_lock = false;
                        break;
                    //down
                    case 3:
                        gesture_lock = true;
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/com/ptz.cgi?camera=1&move=down";
                        AMC.MediaType = "mjpg";
                        AMC.Play();
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/mjpg/video.cgi";
                        AMC.Play();
                        pause();
                        gesture_lock = false;
                        break;
                    //play
                    case 4:
                        gesture_lock = true;
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/mjpg/video.cgi";
                        AMC.MediaType = "mjpg";
                        AMC.Play();
                        pause();
                        break;
                    //pause
                    case 5:
                        AMC.Stop();
                        gesture_lock = false;
                        pause();
                        break;
                    //zoom in
                    case 6:
                        gesture_lock = true;
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/com/ptz.cgi?camera=1&rzoom=+1000";
                        AMC.MediaType = "mjpg";
                        AMC.Play();
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/mjpg/video.cgi";
                        AMC.Play();
                        pause();
                        gesture_lock = false;
                        break;
                    //zoom out
                    case 7:
                        gesture_lock = true;
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/com/ptz.cgi?camera=1&rzoom=-1000";
                        AMC.MediaType = "mjpg";
                        AMC.Play();
                        AMC.Stop();
                        AMC.MediaURL = "http://ptz1-iss.bu.edu/axis-cgi/mjpg/video.cgi";
                        AMC.Play();
                        pause();
                        gesture_lock = false;
                        break;
                    //close
                    case 22:
                        this.Close();
                        break;
                }
            }
        }        
    }
}
