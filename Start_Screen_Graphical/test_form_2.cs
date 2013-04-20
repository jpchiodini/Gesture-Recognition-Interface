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
    public partial class test_form_2 : Form
    {
        public event PauseEvent press_pause;
        public event ChangeFace change_face;
        private List<String> faces = new List<string>();
        private Form_Utilities form_utilities = new Form_Utilities();
        int side;
        int currentGesture;
        public GestureEngine gestEngine;
        public bool gesture_lock = false;

        public test_form_2(int side, GestureEngine gesteng)
        {
            this.gestEngine = gesteng;

            //gestEngine.reset();
           // gestEngine.init();
            gestEngine.GestureChanged += new Start_Screen_Graphical.GestureEngine.NewGestureEventHandler(gestEngine_GestureChanged);
            this.side = side;

            // get the list of file extensions
            this.faces = form_utilities.getFileExtensions();

            InitializeComponent();
        }

        private void gestEngine_GestureChanged(int newGestureID)
        {
            //set the current gesture equal...           
            if (gesture_lock == false)
            {
                //set the current gesture equal...
                currentGesture = newGestureID;




                //push
                if (currentGesture == 4)
                {
                    press_pause(this, new Pause_Form_Event(false));

                    String file_ext = form_utilities.screenCapture_Winform(faces[side], this);
                    //save it...            
                    change_face(this, new Update_Face_Event(file_ext));
                    this.Close();
                    gesture_lock = true;
                }

            }

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
