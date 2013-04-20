using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Events;
using Chomp;
using cameraTest_1;


namespace Start_Screen_Graphical
{
    class SubScreen_Handler
    {
        private int screen ;        
        public event PauseEvent press_pause;
        public event ChangeFace change_face;
        public GestureEngine gesteng;

        public SubScreen_Handler(GestureEngine gesteng)
        {
            this.gesteng = gesteng;
            //this.screen = screen;
        }

        public void setScreen(int screen)
        {
            this.screen = screen;
        }

        /// <summary>
        /// chooses which subscreen to show
        /// </summary>
        public void RenderForm()
        {
            //these forms are for demo purposes only.
            //these statements should initialize the actual forms that will correspond
            //to the faces of the cube
            switch (screen)
            {
                    
                    //purple screen
                case 0:
                    //test_form testform = new test_form(screen,gesteng);
                    //initialize events
                    //testform.press_pause += new PauseEvent(pauseScreen);
                    //testform.change_face += new ChangeFace(changeFace);
                    //frmScreen formScreen = new frmScreen();
                    //formScreen.ShowDialog();
                    //testform.ShowDialog();
                  
                    var wpfWindow = new ElementFlowTest.MainWindow();
                    wpfWindow.Show();
                    break;
                case 1:
                    //test_form_2 testform_2 = new test_form_2(screen,gesteng);
                    //initialize events
                    //testform_2.press_pause += new PauseEvent(pauseScreen);
                   // testform_2.change_face += new ChangeFace(changeFace);            
                    //testform_2.ShowDialog();
                    var wpfWindow1 = new wpfTwitterApp.MainWindow();
                    wpfWindow1.Show();
                    break;
                case 2:
                    //test_form_3 testform_3 = new test_form_3(screen,gesteng);
                    //initialize events
                    //testform_3.press_pause += new PauseEvent(pauseScreen);
                    //testform_3.change_face += new ChangeFace(changeFace);            
                    //testform_3.ShowDialog();
                    cameraTest_1.Form1 cameraForm = new cameraTest_1.Form1(gesteng);
                    cameraForm.ShowDialog();
                    
                    break;
                case 3:
                    test_form_4 testform_4 = new test_form_4(screen,gesteng);
                    //initialize events
                    testform_4.press_pause += new PauseEvent(pauseScreen);
                    testform_4.change_face += new ChangeFace(changeFace);            
                    testform_4.ShowDialog();                    
                    break;            
            }
            
        }
        //relay pause command from application close, unpause
        public void pauseScreen(object a, Pause_Form_Event e)
        {
            press_pause(this, new Pause_Form_Event(false));            
        }
        //relay the extension of the changed face file
        public void changeFace(object a, Update_Face_Event e)
        {
            //the face number is appended at the end
            String file_ext = e.file_extension +screen.ToString();
            change_face(this, new Update_Face_Event(file_ext));
        }

    }
}
