
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

namespace Start_Screen_Graphical
{
    /// <cube Description>
    /// Cube creates the swap chain and sets up the blank form for rendering
    /// </summary>
    public class start_screen
    {

        /*#####################
         *  Initializations
         *#####################*/
        SlimDX.Direct3D11.Device device;
        SwapChain swapChain;
        Scene scene = null;
        RenderTargetView renderTarget;
        BlendState enabled;
        List<Texture2D> textpack = new List<Texture2D>();
        Form form;
        bool pause = false;

        /// <summary>
        /// Calls the initializers
        /// </summary>
        /// <param name="form"></param>
        public start_screen(Form form)
        {
            this.form = form;
            InitializeD3D();
            initializeScene(form);

        }
        /// <summary>
        /// Capture textures and call scene class
        /// </summary>
        /// <param name="form"></param>
        public void initializeScene(Form form)
        {
            DeviceContext context = device.ImmediateContext;
            scene = new Scene(device, form, context);
            scene.press_pause += new PauseEvent(pauseScreen);
        }
        /// <summary>
        /// sends an event when to pause the rendering loop
        /// </summary>
        /// <param name="a"></param>
        /// <param name="e"></param>

        public void pauseScreen(object a, Pause_Form_Event e)
        {
            pause = e.pause;
        }

        public void InitializeD3D()
        {

            var description = new SwapChainDescription()
            {
                BufferCount = 2,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = form.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };

            //Create swap chain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, description, out this.device, out swapChain);
            // create a view of our render target, which is the backbuffer of the swap chain we just created
            // setting a viewport is required if you want to actually see anything
            var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0);
            renderTarget = new RenderTargetView(device, resource);
            var context = device.ImmediateContext;
            var viewport = new Viewport(0.0f, 0.0f, form.ClientSize.Width, form.ClientSize.Height);
            context.OutputMerger.SetTargets(renderTarget);
            context.Rasterizer.SetViewports(viewport);
        }
        public void RenderFrame()
        {


            if (pause == false)
            {
                DeviceContext mycontext = device.ImmediateContext;
                mycontext.ClearRenderTargetView(renderTarget, new Color4(1.0f, 1.0f, 1.0f));
                scene.Draw(device.ImmediateContext, textpack, form);
                mycontext.OutputMerger.BlendFactor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
                swapChain.Present(0, PresentFlags.None);
            }

        }
        public void InitState()
        {
            BlendStateDescription blendStateDesc = new BlendStateDescription()
            {
                IndependentBlendEnable = false,
                AlphaToCoverageEnable = false,
            };

            blendStateDesc.RenderTargets[0].BlendEnable = true; //turn on blending?
            blendStateDesc.RenderTargets[0].BlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTargets[0].SourceBlend = BlendOption.SourceAlpha;
            blendStateDesc.RenderTargets[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendStateDesc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
            blendStateDesc.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
            blendStateDesc.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
            blendStateDesc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            enabled = BlendState.FromDescription(device, blendStateDesc);
        }
        private void ReleaseDXResources(object sender, FormClosingEventArgs e)
        {
            renderTarget.Dispose();
            swapChain.Dispose();
            device.Dispose();
        }
    }
}
