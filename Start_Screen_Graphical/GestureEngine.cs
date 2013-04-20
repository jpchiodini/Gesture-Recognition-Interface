using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.IO;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Threading;
using Events;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Start_Screen_Graphical
{
    public class GestureEngine
    {

        // GESTURE HANDLER

        private static int curr_id; //current gesture id

        public delegate void NewGestureEventHandler(int newGestureID);
        public event NewGestureEventHandler GestureChanged;
        public Bitmap bmp;
        

        //public event SendAlert sendAlert;

        public int CurrentID
        {
            get { return GestureEngine.curr_id; }
            set
            {
                GestureEngine.curr_id = value;
                if (this.GestureChanged != null)
                    this.GestureChanged(value);
            }
            
        }

        public void reset()
        {
            frameindex = 0;
            Mean_Init();
            Dist_Init();
        }

        static int VectDimen = 13;
        static int CovDimen = 91;
        static int FPSegment = 30;
        float[,] DistData = new float[VectDimen, FPSegment];
        float[] MeanDist = new float[VectDimen];
        int frameindex = 31;
        float[, ,] lib = new float[8, LibrarySize, CovDimen];
        float[] NN_Result = new float[8];
        
        
        public string[] action = { 
                                     "Right Swing", //0
                                     "Left Swing",  //1
                                     "Right Push",  //2
                                     "Left Push",   //3
                                     "Right Back",  //4
                                     "Left Back",   //5
                                     "Open",        //6
                                     "Close",       //7
                                     "NR" };        //8
        string result;
        int[] DecisionArray = new int[6];
        public int Decision;
        int currentD = 0;
        int cooldown = 0;
        static int GCD = 10;

        public bool isReady = true;

        static int LibrarySize = 100;
        KinectSensor kinectsensor;
        const int skeletonCount = 6;
        Skeleton[] allskeletons = new Skeleton[skeletonCount];

        //initial Kinect and skeleton stream
        public void init()
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                kinectsensor = KinectSensor.KinectSensors[0];
                if (kinectsensor.Status == KinectStatus.Connected)
                {
                    kinectsensor.SkeletonStream.Enable();
                    kinectsensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); //addded
                    kinectsensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);    //added        
                    kinectsensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectsensor_SkeletonFrameReady);
                    kinectsensor.AllFramesReady += FramesReady;
                    kinectsensor.Start();
                }
            }

            fetchLib();
            Dist_Init();
            Mean_Init();

        }

        public Bitmap getImage()
        {
            return bmp;
        }

        //returns the latest picture!!!!!
        void FramesReady(object sender, AllFramesReadyEventArgs e)
        {
            ColorImageFrame VFrame = e.OpenColorImageFrame();
            if (VFrame == null) return;
            byte[] pixelS = new byte[VFrame.PixelDataLength];
            bmp = ImageToBitmap(VFrame);


            SkeletonFrame SFrame = e.OpenSkeletonFrame();
            if (SFrame == null) return;

            Graphics g = Graphics.FromImage(bmp);
            Skeleton[] Skeletons = new Skeleton[SFrame.SkeletonArrayLength];
            SFrame.CopySkeletonDataTo(Skeletons);

            foreach (Skeleton S in Skeletons)
            {
                if (S.TrackingState == SkeletonTrackingState.Tracked)
                {
                    //body
                    DrawBone(JointType.Head, JointType.ShoulderCenter, S, g);
                    DrawBone(JointType.ShoulderCenter, JointType.Spine, S, g);
                    DrawBone(JointType.Spine, JointType.HipCenter, S, g);
                    //left leg
                    DrawBone(JointType.HipCenter, JointType.HipLeft, S, g);
                    DrawBone(JointType.HipLeft, JointType.KneeLeft, S, g);
                    DrawBone(JointType.KneeLeft, JointType.AnkleLeft, S, g);
                    DrawBone(JointType.AnkleLeft, JointType.FootLeft, S, g);
                    //Right Leg
                    DrawBone(JointType.HipCenter, JointType.HipRight, S, g);
                    DrawBone(JointType.HipRight, JointType.KneeRight, S, g);
                    DrawBone(JointType.KneeRight, JointType.AnkleRight, S, g);
                    DrawBone(JointType.AnkleRight, JointType.FootRight, S, g);
                    //Left Arm
                    DrawBone(JointType.ShoulderCenter, JointType.ShoulderLeft, S, g);
                    DrawBone(JointType.ShoulderLeft, JointType.ElbowLeft, S, g);
                    DrawBone(JointType.ElbowLeft, JointType.WristLeft, S, g);
                    DrawBone(JointType.WristLeft, JointType.HandLeft, S, g);
                    //Right Arm
                    DrawBone(JointType.ShoulderCenter, JointType.ShoulderRight, S, g);
                    DrawBone(JointType.ShoulderRight, JointType.ElbowRight, S, g);
                    DrawBone(JointType.ElbowRight, JointType.WristRight, S, g);
                    DrawBone(JointType.WristRight, JointType.HandRight, S, g);

                }
            }
        }

        void DrawBone(JointType j1, JointType j2, Skeleton S, Graphics g)
        {
            Point p1 = GetJoint(j1, S);
            Point p2 = GetJoint(j2, S);
            g.DrawLine(Pens.Red, p1, p2);
        }

        Point GetJoint(JointType j, Skeleton S)
        {
            SkeletonPoint Sloc = S.Joints[j].Position;
            ColorImagePoint Cloc = kinectsensor.MapSkeletonPointToColor(Sloc, ColorImageFormat.RgbResolution640x480Fps30);
            return new Point(Cloc.X, Cloc.Y);
        }

        Bitmap ImageToBitmap(ColorImageFrame Image)
        {
            byte[] pixelData =
                     new byte[Image.PixelDataLength];
            Image.CopyPixelDataTo(pixelData);
            Bitmap bmap = new Bitmap(
                   Image.Width,
                   Image.Height,
                   PixelFormat.Format32bppRgb);
            BitmapData bmapS = bmap.LockBits(
              new Rectangle(0, 0,
                         Image.Width, Image.Height),
              ImageLockMode.WriteOnly,
              bmap.PixelFormat);
            IntPtr ptr = bmapS.Scan0;
            Marshal.Copy(pixelData, 0, ptr,
                       Image.PixelDataLength);
            bmap.UnlockBits(bmapS);
            return bmap;
        }

        public void Dist_Init()
        {
            for (int j = 0; j < VectDimen; j++)
            {
                for (int f = 0; f < FPSegment; f++)
                {
                    DistData[j, f] = 0;
                }
            }
        }

        public void fetchLib()
        {
            using (TextReader fetch = File.OpenText("Library/RS.txt"))
            {
                for (int s = 0; s < LibrarySize; s++)
                {
                    string text = fetch.ReadLine();
                    string[] temp = text.Split(' ');
                    for (int i = 0; i < CovDimen; i++)
                    {
                        lib[0, s, i] = float.Parse(temp[i]);
                    }
                }
            }
            using (TextReader fetch = File.OpenText("Library/LS.txt"))
            {
                for (int s = 0; s < LibrarySize; s++)
                {
                    string text = fetch.ReadLine();
                    string[] temp = text.Split(' ');
                    for (int i = 0; i < CovDimen; i++)
                    {
                        lib[1, s, i] = float.Parse(temp[i]);
                    }
                }
            }
            using (TextReader fetch = File.OpenText("Library/RP.txt"))
            {
                for (int s = 0; s < LibrarySize; s++)
                {
                    string text = fetch.ReadLine();
                    string[] temp = text.Split(' ');
                    for (int i = 0; i < CovDimen; i++)
                    {
                        lib[2, s, i] = float.Parse(temp[i]);
                    }
                }
            }
            using (TextReader fetch = File.OpenText("Library/LP.txt"))
            {
                for (int s = 0; s < LibrarySize; s++)
                {
                    string text = fetch.ReadLine();
                    string[] temp = text.Split(' ');
                    for (int i = 0; i < CovDimen; i++)
                    {
                        lib[3, s, i] = float.Parse(temp[i]);
                    }
                }
            }
            using (TextReader fetch = File.OpenText("Library/RB.txt"))
            {
                for (int s = 0; s < LibrarySize; s++)
                {
                    string text = fetch.ReadLine();
                    string[] temp = text.Split(' ');
                    for (int i = 0; i < CovDimen; i++)
                    {
                        lib[4, s, i] = float.Parse(temp[i]);
                    }
                }
            }
            using (TextReader fetch = File.OpenText("Library/LB.txt"))
            {
                for (int s = 0; s < LibrarySize; s++)
                {
                    string text = fetch.ReadLine();
                    string[] temp = text.Split(' ');
                    for (int i = 0; i < CovDimen; i++)
                    {
                        lib[5, s, i] = float.Parse(temp[i]);
                    }
                }
            }
            using (TextReader fetch = File.OpenText("Library/OP.txt"))
            {
                for (int s = 0; s < LibrarySize; s++)
                {
                    string text = fetch.ReadLine();
                    string[] temp = text.Split(' ');
                    for (int i = 0; i < CovDimen; i++)
                    {
                        lib[6, s, i] = float.Parse(temp[i]);
                    }
                }
            }
            using (TextReader fetch = File.OpenText("Library/CL.txt"))
            {
                for (int s = 0; s < LibrarySize; s++)
                {
                    string text = fetch.ReadLine();
                    string[] temp = text.Split(' ');
                    for (int i = 0; i < CovDimen; i++)
                    {
                        lib[7, s, i] = float.Parse(temp[i]);
                    }
                }
            }
        }

        public int Comparison(MathNet.Numerics.LinearAlgebra.Generic.Matrix<float> LogCov)
        {
            int Decision = -1;
            for (int a = 0; a < 8; a++)
            {
                NN_Result[a] = 100000;
                for (int s = 0; s < LibrarySize; s++)
                {
                    float distance = 0;
                    int e = 0;
                    for (int i = 0; i < VectDimen; i++)
                    {
                        for (int j = i; j < VectDimen; j++)
                        {
                            if (j == i)
                            {
                                distance += (float)Math.Pow(LogCov[i, j] - lib[a, s, e], 2);
                            }
                            else
                            {
                                distance += (float)(2 * Math.Pow(LogCov[i, j] - lib[a, s, e], 2));
                            }
                            e++;
                        }
                    }
                    if (distance < NN_Result[a])
                    {
                        NN_Result[a] = distance;
                    }
                }
            }
            float Shortest = NN_Result[0];
            result = action[0];
            Decision = 0;

            if (NN_Result[1] < Shortest)
            {
                Shortest = NN_Result[1];
                result = action[1];
                Decision = 1;
            }

            if (NN_Result[2] < Shortest)
            {
                Shortest = NN_Result[2];
                result = action[2];
                Decision = 2;
            }

            if (NN_Result[3] < Shortest)
            {
                Shortest = NN_Result[3];
                result = action[3];
                Decision = 3;
            }

            if (NN_Result[4] < Shortest)
            {
                Shortest = NN_Result[4];
                result = action[4];
                Decision = 4;
            }

            if (NN_Result[5] < Shortest)
            {
                Shortest = NN_Result[5];
                result = action[5];
                Decision = 5;
            }

            if (NN_Result[6] < Shortest)
            {
                Shortest = NN_Result[6];
                result = action[6];
                Decision = 6;
            }
            if (NN_Result[7] < Shortest)
            {
                Shortest = NN_Result[7];
                result = action[7];
                Decision = 7;
            }
            if (Shortest > 150)
            {
                result = action[8] + " " + Shortest.ToString() + "\n(" + result + ")";
                Decision = 8;
            }

            if (DistData[4, 29] - DistData[4, 0] + DistData[10, 29] - DistData[10, 0] > 1.0f)
            {
                result = "Idle";
                Decision = -1;
            }
            return Decision;
        }

        public static void tryEvd(DenseMatrix Cov)
        {
            DenseMatrix copy = Cov;
            var copyevd = copy.Evd();
        }

        public MathNet.Numerics.LinearAlgebra.Generic.Matrix<float> ToCovMat(int VectDimen, int FPSegment, float[,] DistData, float[] MeanDist)
        {
            DenseMatrix Cov = new DenseMatrix(VectDimen);
            for (int i = 0; i < VectDimen; i++)
            {
                for (int j = i; j < VectDimen; j++)
                {
                    for (int t = 0; t < FPSegment; t++)
                    {
                        Cov[i, j] += DistData[i, t] * DistData[j, t];
                    }
                    Cov[i, j] -= MeanDist[i] * MeanDist[j] * FPSegment;
                    Cov[i, j] /= FPSegment - 1;
                }
            }
            for (int i = 0; i < VectDimen; i++)
            {
                for (int j = i; j < VectDimen; j++)
                {
                    Cov[j, i] = Cov[i, j];
                }
            }

            MathNet.Numerics.LinearAlgebra.Single.Factorization.Evd evd;
            MathNet.Numerics.LinearAlgebra.Generic.Matrix<float> LogD;

            var thread = new Thread(() => tryEvd(Cov));
            thread.Start();

            if (!thread.Join(3000))
            {
                LogD = Cov;
                LogD.Clear();
                return LogD;
            }

            evd = Cov.Evd();
            LogD = evd.D();

            for (int i = 0; i < VectDimen; i++)
            {
                LogD[i, i] = (float)Math.Log(Math.Abs((double)LogD[i, i]));

            }
            MathNet.Numerics.LinearAlgebra.Generic.Matrix<float> LogCov;
            LogCov = evd.EigenVectors() * LogD * evd.EigenVectors().Transpose();

            return LogCov;
        }

        public void Mean_Init()
        {
            for (int i = 0; i < VectDimen; i++)
            {
                MeanDist[i] = 0;
            }
        }

        float[] ToCoor(Joint joint)
        {
            DepthImagePoint depth = kinectsensor.MapSkeletonPointToDepth(joint.Position, DepthImageFormat.Resolution640x480Fps30);
            
            return new float[] { depth.X, depth.Y, depth.Depth };
        }

        public Skeleton GetFirstSkeleton(SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }
                skeletonFrameData.CopySkeletonDataTo(allskeletons);

                Skeleton first = (from s in allskeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();
                return first;
            }
        }

        public void kinectsensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton skeleton = GetFirstSkeleton(e);

            

            if (skeleton == null)
            {
                return;
            }
            if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {

                float[] Neck = ToCoor(skeleton.Joints[JointType.ShoulderCenter]);
                float[] Center = ToCoor(skeleton.Joints[JointType.Spine]);
                float[] RElbow = ToCoor(skeleton.Joints[JointType.ElbowRight]);
                float[] RHand = ToCoor(skeleton.Joints[JointType.HandRight]);
                float[] LElbow = ToCoor(skeleton.Joints[JointType.ElbowLeft]);
                float[] LHand = ToCoor(skeleton.Joints[JointType.HandLeft]);

                float[][] MetaCoor = { LElbow, LHand, RElbow, RHand };

                if (frameindex < 30)
                {
                    float Hfactor = (float)Math.Sqrt(Math.Pow(Neck[0] - Center[0], 2) + Math.Pow(Neck[1] - Center[1], 2) + Math.Pow(Neck[2] - Center[2], 2));
                    for (int i = 0; i < MetaCoor.Length; i++)
                    {
                        DistData[3 * i, frameindex] = (MetaCoor[i][0] - Center[0]) / Hfactor;
                        DistData[3 * i + 1, frameindex] = (MetaCoor[i][1] - Center[1]) / Hfactor;
                        DistData[3 * i + 2, frameindex] = (MetaCoor[i][2] - Center[2]) / Hfactor;
                    }

                    DistData[VectDimen - 1, frameindex] = (float)frameindex;
                    for (int i = 0; i < VectDimen; i++)
                    {
                        MeanDist[i] += DistData[i, frameindex];
                    }

                    ++frameindex;

                    if (cooldown > 0)
                    {
                        cooldown--;

                        if (cooldown == 0)
                        {
                            //Output ready signal
                            //Console.WriteLine("READY");
                        }
                        frameindex = 0;
                    }

                    if (frameindex == 5)
                    {
                        float distance = 0;

                        for (int i = 0; i < 4; i++)
                        {
                            distance += (float)Math.Sqrt(Math.Pow(DistData[3 * i, 4] - DistData[3 * i, 0], 2) + Math.Pow(DistData[3 * i + 1, 4] - DistData[3 * i + 1, 0], 2)
                                        + Math.Pow(DistData[3 * i + 2, 4] - DistData[3 * i + 2, 0], 2));
                        }
                        if (distance < 1.0f)
                        {
                            frameindex = 0;
                            Mean_Init();
                        }
                    }
                    if (frameindex == FPSegment)
                    {
                        currentD = (frameindex / 5) - 1;
                        for (int i = 0; i < VectDimen; i++)
                        {
                            MeanDist[i] /= FPSegment;
                        }
                        MathNet.Numerics.LinearAlgebra.Generic.Matrix<float> performed = ToCovMat(13, 30, DistData, MeanDist);

                        Decision = Comparison(performed);

                        //Output Gesture
                        toCube(Decision);

                        frameindex = 0;
                        Mean_Init();

                        cooldown = GCD;

                        //Output WAIT
                        //Console.WriteLine("WAIT");
                    }
                }
            }
        }

        void toCube(int decisionID)
        {
            this.CurrentID = decisionID;
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
            }
        }
    
    }
}
