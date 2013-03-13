using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Events;

namespace Start_Screen_Graphical
{
    public partial class test_form : Form
    {
        public event PauseEvent press_pause;
        public event ChangeFace change_face;
        private List<String> faces = new List<string>();
        int side;
        
        public test_form(int side)
        {
            this.side = side - 1;
            String directory = System.IO.Directory.GetCurrentDirectory();
            String face1 = "C:\\Users\\jpchiodini\\Documents\\Visual Studio 2010\\Projects\\Senior_Design\\Start_Screen_Graphical\\images\\Screenshots\\face1.jpg";
            String face2 = "C:\\Users\\jpchiodini\\Documents\\Visual Studio 2010\\Projects\\Senior_Design\\Start_Screen_Graphical\\images\\Screenshots\\face2.jpg";
            String face3 = "C:\\Users\\jpchiodini\\Documents\\Visual Studio 2010\\Projects\\Senior_Design\\Start_Screen_Graphical\\images\\Screenshots\\face3.jpg";
            String face4 = "C:\\Users\\jpchiodini\\Documents\\Visual Studio 2010\\Projects\\Senior_Design\\Start_Screen_Graphical\\images\\Screenshots\\face4.jpg";
            faces.Add(face1);
            faces.Add(face2);
            faces.Add(face3);
            faces.Add(face4);

            InitializeComponent();
        }        

        private void button1_Click(object sender, EventArgs e)
        {
            
            //when you click pause on the button it goes back to the cube...
            press_pause(this, new Pause_Form_Event(false));
            //take screen shot...
            //save it...
            String file_ext = ScreenCapture();
            change_face(this, new Update_Face_Event(file_ext));
            this.Close();
        }

        private String ScreenCapture()
        {

            Rectangle bounds = this.Bounds;
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }
                bitmap.Save(faces[side], System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            return faces[side];
        }



    }
}
