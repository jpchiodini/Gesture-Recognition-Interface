using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Drawing;

namespace My_Form_Utilities
{
    class Form_Utilities
    {
        public List<String> getFileExtensions()
        {
            List<String> faces = new List<string>();

            int end = 0;
            String directory = System.IO.Directory.GetCurrentDirectory();

            end = directory.IndexOf("Start_Screen_Graphical\\Start_Screen_Graphical");
            String final_dir = directory.Substring(0, end) + "Start_Screen_Graphical\\images\\Screenshots";

            String face1 = final_dir + "\\face1.jpg";
            String face2 = final_dir + "\\face2.jpg";
            String face3 = final_dir + "\\face3.jpg";
            String face4 = final_dir + "\\face4.jpg";
            faces.Add(face1);
            faces.Add(face2);
            faces.Add(face3);
            faces.Add(face4);

            return faces;
        }

        //returns the root directory that user can dump files or folders into.
        //Will not change file contents even if project directory is changed
        public String getDirectory()
        {
            int end = 0;
            String directory = System.IO.Directory.GetCurrentDirectory();
            // directory = directory.Substring(2, directory.Length - 2);
            end = directory.IndexOf("Start_Screen_Graphical\\Start_Screen_Graphical");

            String final_dir = directory.Substring(0, end) + "Start_Screen_Graphical";
            return final_dir;
        }

        /// <summary>
        /// takes a screen shot of a form and return the file extension that it was saved to
        /// </summary>
        /// <param name="file_extension"></param>
        /// <param name="f"></param>
        public String screenCapture_Winform(string file_extension,Form f)
        {
            
            Rectangle bounds = f.Bounds;
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }
                bitmap.Save(file_extension, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            return file_extension;

        }

        /// <summary>
        /// will allow the user to take a screenshot of a wpf form and return the file extension it was saved to
        /// </summary>
        /// <returns></returns>
        public String screenCapture_WPF()
        { return null; }


        public void mirrorImage(String file_ext)
        {
            var bmp = Bitmap.FromFile(file_ext);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bmp.Save(file_ext);
        }


        public static float nfmod(float a, float b)
        {
            return a - b * (float)Math.Floor(a / b);
        }


    }
}
