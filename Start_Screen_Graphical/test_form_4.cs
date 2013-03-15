using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Events;
using My_Form_Utilities;

namespace Start_Screen_Graphical
{
    public partial class test_form_4 : Form
    {
        public event PauseEvent press_pause;
        public event ChangeFace change_face;
        private List<String> faces = new List<string>();
        private Form_Utilities form_utilities = new Form_Utilities();
        int side;

        public test_form_4(int side)
        {
            this.side = 0;

            // get the list of file extensions
            this.faces = form_utilities.getFileExtensions();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //when you click pause on the button it goes back to the cube...
            press_pause(this, new Pause_Form_Event(false));

            String file_ext = form_utilities.screenCapture_Winform(faces[side], this);
            //save it...            
            change_face(this, new Update_Face_Event(file_ext));
            this.Close();

        }
    }
}
