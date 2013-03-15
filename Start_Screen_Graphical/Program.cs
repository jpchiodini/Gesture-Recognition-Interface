////##########################################
////Senior Design Start Screen Alpha
////Author: John Chiodini
////Email: chiodini@bu.edu
////###########################################

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Buffer = SlimDX.Direct3D11.Buffer;
using Resource = SlimDX.Direct3D11.Resource;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Events;

//main rendering loop
namespace Start_Screen_Graphical
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //Console.ReadLine();
            var form = new RenderForm ("Transparency");
            
            start_screen mycube = new start_screen(form);
            {                
                MessagePump.Run(form, mycube.RenderFrame);
                {
                    
                }
            }
        }
    }
}



