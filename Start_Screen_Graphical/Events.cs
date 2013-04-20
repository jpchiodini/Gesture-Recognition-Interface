using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Events
{
    public delegate void PauseEvent(object sender, Pause_Form_Event e);
    public delegate void ChangeFace(object sender, Update_Face_Event e);
    public delegate void SendAlert(object sender, Alert_Event e);

    
    /// <summary>
    /// //sends events to pause and unpause the rendering loop
    /// </summary>
    public class Pause_Form_Event: EventArgs
    {
        
        private bool pse;

        public bool pause
        { get { return pse; } }

        public Pause_Form_Event(bool pse)
        {
            this.pse = pse;
        }

    }

    /// <summary>
    /// updates the cubes faces sending links to SRVs
    /// </summary>
    public class Update_Face_Event : EventArgs
    {
        //returns filename for strings
        private String f_ext;

        public String file_extension
        { get { return f_ext; } }

        public Update_Face_Event(String f_ext)
        {
            this.f_ext = f_ext;
        }
    }

    //generates an alert for the user to wait or proceed with swiping
    public class Alert_Event : EventArgs
    {
        //returns filename for strings
        private bool alert;

        public bool Alert
        { get { return alert; } }

        public Alert_Event(bool alert)
        {
            this.alert = alert;
        }
    }


}
