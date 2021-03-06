﻿using System;
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
    public partial class test_form : Form
    {
        public event PauseEvent press_pause;
        public event ChangeFace change_face;
        private List<String> faces = new List<string>();
        private Form_Utilities form_utilities = new Form_Utilities();
        int side;
        int currentGesture;
        public GestureEngine gestEngine;
        
        //form gets the current side
        public test_form(int side, GestureEngine gesteng)
        {
            this.gestEngine = gesteng;          
            gestEngine.GestureChanged += new Start_Screen_Graphical.GestureEngine.NewGestureEventHandler(gestEngine_GestureChanged);


            this.side = 0;
            
            // get the list of file extensions
            this.faces = form_utilities.getFileExtensions();
            InitializeComponent();
        }


        private void gestEngine_GestureChanged(int newGestureID)
        {
            //set the current gesture equal...
            currentGesture = newGestureID;

            if (currentGesture == 4)
            {
                this.Close();
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void test_form_Load(object sender, EventArgs e)
        {

        }

    }
}
