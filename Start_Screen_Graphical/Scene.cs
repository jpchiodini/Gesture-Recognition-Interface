//##########################################
//Senior Design Start Screen Alpha
//Author: John Chiodini
//Email: chiodini@bu.edu
//###########################################

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
using My_Form_Utilities;

//need to figure out how to set up multiple constant buffers...
namespace Start_Screen_Graphical
{
    /// <Scene Class Description>
    /// This Class Renders over the blank form created using Vertex,Index, and Const Buffers
    /// Matrix Multiplications allow the cube to take a location relative to the camera on screen as well as 
    /// Rotate and Scale
    /// </summary>
    public class Scene
    {
        /**-------------------------------
         * Struct Definitions
         *-----------------------------*/
        //holds vertices along with normals and textcoords
        struct myVertex
        {
            public myVertex(Vector3 pos, Vector3 normal, Vector2 uv)
            {
                Pos = pos;
                Normal = normal;
                UV = uv;
            }
            Vector3 Pos;
            Vector3 Normal;
            Vector2 UV;
        };
        //Struct defining Constant Buffer info for cube
        struct CBUFFER
        {
            public Matrix Final;
            public Matrix Rotatation;
            public Vector4 LightVector;
            public Vector4 LightColor;
            public Vector4 AmbientColor;
        }
        /*######################
         *  Initializations
         *######################*/
        //View matrices
        //cube
        Matrix IdMat, mTranslate, mScale, myLook, matProjection, mRotate1;
        //screen
        Matrix sTranslate, sScale, smyLook, smatProjection, sRotate1;
        //world matrices
        Matrix matFinal, screenFinal;
        //Rotation matrices
        //cube
        Matrix mRotateX, mRotateY, mRotateZ;
        //screen
        Matrix sRotateX, sRotateY, sRotateZ;

        Device device;
        Buffer Cube, Screen;
        InputLayout layout;
        ShaderSignature inputSignature;
        ShaderResourceView SRVscreen, SRV, SRV1, SRV2, SRV3, SRV4, SRV5;
        List<ShaderResourceView> SRVlist = new List<ShaderResourceView>();
        PixelShader pixelShader;
        VertexShader vertexShader;
        Buffer constantBuffer, constantBuffer1;
        Buffer indexBuffer;
        Buffer screenIndexBuffer;
        CBUFFER cbuffer, cbuffer1;
        BlendState enabled;

        //event handler
        public event PauseEvent press_pause;
        public event ChangeFace change_face;

        //#######################
        //offsets
        //#######################
        public static float Time = 0.785f;
        public static float Zoom = 0.0f;
        public static float Cap = 0.0f;
        public static float Cap1 = 0.0f;
        public static float width = 0.35f;
        public static float width1;
        public bool zoom = true;

        //##############
        //Other Variables
        //##############
        int currentFace = 0;
        private Form_Utilities form_utilities = new Form_Utilities();


        public Scene(Device device, Form form, DeviceContext context)
        {
            this.device = device;
            initTextures();
            Initialize(form, context);
            CreateCube();
            createScreen();
            CreateConstBuffer(context);
            CubeIndexBuffer();
            ScreenIndexBuffer();
            BuildShaderInputLayout();
        }

        /// <summary>
        /// Applies a new texture to the cube representing last state of closed form
        /// </summary>
        /// <param name="a"></param>
        /// <param name="e"></param>
        public void getNewFace(object a, Update_Face_Event e)
        {

            //##############################################|
            //----------------------------------------------|
            //face information...                           |
            //----------------------------------------------|
            //face numbers                                  |
            //----------------------------------------------|
            //0 is red                                      |
            //1 is purple                                   |
            //2 is to face of cube                          |
            //3 is the bottom face of cube                  |   
            //4 is yellow                                   |
            //5 is orange                                   |
            //----------------------------------------------|
            //face order when spinning from beginning       |
            //----------------------------------------------|
            //purple orange red yellow                      |
            //##############################################|

            String temp = e.file_extension;
            //parse the string to take out the number...
            String ext = temp.Substring(0, (temp.Length - 1));
            String faceNum = temp.Substring(temp.Length - 1, 1);
            int index = Convert.ToInt32(faceNum);

            switch (index)
            {
                //if its the first face
                case 0:
                    //set it to the purple face
                    //mirror the extension
                    form_utilities.mirrorImage(ext);
                    SRVlist[1] = ShaderResourceView.FromFile(device, ext);
                    break;
                //if its the second face
                case 1:
                    //set it to the orange face
                    SRVlist[5] = ShaderResourceView.FromFile(device, ext);
                    break;
                //if its the third face
                case 2:
                    //set it to the red face
                    SRVlist[0] = ShaderResourceView.FromFile(device, ext);
                    break;
                //if its the fourth face
                case 3:
                    //set it to the yellow face
                    //mirror the face due to culling
                    form_utilities.mirrorImage(ext);
                    SRVlist[4] = ShaderResourceView.FromFile(device, ext);
                    break;
            }

        }

        
        private void initTextures()
        {
            //screen 
            //screen textures are mirrored in wrong, because of culling or transparency, i built a temp fix.
            String img_directory = form_utilities.getDirectory() + "\\images\\Sample Pictures";
            
            SRVscreen = ShaderResourceView.FromFile(device,img_directory + "\\metallic.jpg");
            //top and bottom
            SRV2 = ShaderResourceView.FromFile(device, img_directory + "\\green.jpg");
            SRV3 = ShaderResourceView.FromFile(device, img_directory + "\\blue.jpg");

            //side faces
            //yellow and purple are at the back so dont get rendered by back buffer
            //used mirror image to fix
            String yellow_ext = img_directory + "\\yellow.jpg";
            String purple_ext = img_directory + "\\purple.jpg";
            form_utilities.mirrorImage(yellow_ext);
            form_utilities.mirrorImage(purple_ext);

            SRV4 = ShaderResourceView.FromFile(device, yellow_ext);
            SRV1 = ShaderResourceView.FromFile(device, purple_ext);

            SRV = ShaderResourceView.FromFile(device, img_directory + "\\red.jpg");
            SRV5 = ShaderResourceView.FromFile(device, img_directory + "\\orange.jpg");

            SRVlist.Add(SRV);
            SRVlist.Add(SRV1);
            SRVlist.Add(SRV2);
            SRVlist.Add(SRV3);
            SRVlist.Add(SRV4);
            SRVlist.Add(SRV5);
        }
        //initialize the cube matrices
        public void Initialize(Form form, DeviceContext context)
        {
            /*---------------------------------
             * Constant Buffer Initialization
             *---------------------------------*/
            //Identity matrix for later use
            //Set up translation matrix.
            //Set up rotation matrix, gets updated in Draw Method
            //set up scaling
            //Set up projection matrix for camera
            //Multiply Matrices for camera            
            myLook = smyLook = Matrix.LookAtLH(new Vector3(2.0f, 1.0f, 2.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            matProjection = smatProjection = Matrix.PerspectiveFovLH((float)((float)45 * ((float)3.14 / (float)180)), (float)form.ClientSize.Width / (float)form.ClientSize.Height, 1.0f, 100.0f);
            matFinal = mRotateX * mRotate1 * myLook * matProjection;
            cbuffer.Rotatation = mRotateX;
            cbuffer.Final = matFinal;
            cbuffer.LightVector = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
            cbuffer.LightColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            cbuffer.AmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            int size = Marshal.SizeOf(typeof(CBUFFER));
            var update = new DataStream(size, true, true);
            update.Write<CBUFFER>(cbuffer);
            update.Position = 0;

            //screen matrices
            screenFinal = sRotateX * sRotate1 * smyLook * smatProjection;
            cbuffer1.Final = screenFinal;
            cbuffer1.LightVector = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
            cbuffer1.LightColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            cbuffer1.AmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            var update1 = new DataStream(size, true, true);
            update.Write<CBUFFER>(cbuffer1);
            update.Position = 0;

        }

        /// <summary>
        /// Sets up cube vertices
        /// </summary>
        /// <param name="device"></param>
        public void CreateCube()
        {

            //for transparent screen...
            int size = Marshal.SizeOf(typeof(myVertex));
            // create test vertex data, making sure to rewind the stream afterward
            DataStream vertices = new DataStream(size * 24 * 7, true, true);

            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f)));    // side 1
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f, 0.0f)));       //red
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f, 1.0f)));

            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(0.0f, 0.0f)));    // side 2
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(0.0f, 1.0f)));     //purple
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f, 1.0f)));

            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f)));    // side 3
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f)));   //green
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f)));

            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(0.0f, 0.0f)));    // side 4
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(1.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(1.0f, 0.0f)));

            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f)));    // side 5
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f)));      //yellow
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2 (1.0f, 1.0f)));

            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f)));    // side 6
            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(1.0f, 0.0f)));     //orange
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(1.0f, 1.0f)));

            vertices.Position = 0;

            //vertex buffer
            Cube = new Buffer(device, vertices, size * 24 * 7, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
        }
        public void createScreen()
        {
            //for transparent screen...
            int size = Marshal.SizeOf(typeof(myVertex));
            // create test vertex data, making sure to rewind the stream afterward
            DataStream vertices = new DataStream(size * 24, true, true);

            vertices.Write(new myVertex(new Vector3(-1.0f, 1.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f)));    // side 3
            vertices.Write(new myVertex(new Vector3(-1.0f, 1.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(1.0f, 1.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f)));
            vertices.Position = 0;

            //vertex buffer
            Screen = new Buffer(device, vertices, size * 24, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

        }
        public void CubeIndexBuffer()
        {

            int size = sizeof(uint);
            uint[] OurIndices = 
            {  
                0, 1, 2,    // side 1
                2, 1, 3, 
                4, 5, 6,    // side 2
                6, 5, 7,
                8, 9, 10,    // side 3
                10, 9, 11,
                12, 13, 14,    // side 4
                14, 13, 15,
                16, 17, 18,    // side 5
                18, 17, 19,
                20, 21, 22,    // side 6
                22, 21, 23,
                
            };
            DataStream indices = new DataStream(size * 42, true, true); //**
            indices.WriteRange(OurIndices);
            indices.Position = 0;
            indexBuffer = new Buffer(device, indices, Marshal.SizeOf(typeof(uint)) * 42, ResourceUsage.Dynamic, BindFlags.IndexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0); //*
        }
        public void ScreenIndexBuffer()
        {
            int size = sizeof(uint);
            uint[] OurIndices = 
            {  
                0, 1, 2,    // side 1
                2, 1, 3,  
            };
            DataStream indices = new DataStream(size * 6, true, true); //**
            indices.WriteRange(OurIndices);
            indices.Position = 0;
            screenIndexBuffer = new Buffer(device, indices, Marshal.SizeOf(typeof(uint)) * 6, ResourceUsage.Dynamic, BindFlags.IndexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

        }
        public void BuildShaderInputLayout()
        {

            // load and compile the vertex shader
            using (var bytecode = ShaderBytecode.CompileFromFile("C:\\transparent.fx", "VShader", "vs_4_0", ShaderFlags.None, EffectFlags.None))
            {
                inputSignature = ShaderSignature.GetInputSignature(bytecode);
                vertexShader = new VertexShader(device, bytecode);
            }
            // load and compile the pixel shader
            using (var bytecode = ShaderBytecode.CompileFromFile("C:\\transparent.fx", "PShader", "ps_4_0", ShaderFlags.None, EffectFlags.None))
                pixelShader = new PixelShader(device, bytecode);


            //Create vertex layout.
            var elements = new[] 
            {
                
                new InputElement("POSITION",0, SlimDX.DXGI.Format.R32G32B32_Float, 0,0,InputClassification.PerVertexData,0),               
                new InputElement("NORMAL", 0 ,SlimDX.DXGI.Format.R32G32B32_Float,12,0,InputClassification.PerVertexData,0), //why does zero not work here????
                new InputElement("TEXCOORD", 0 ,SlimDX.DXGI.Format.R32G32B32_Float,24,0,InputClassification.PerVertexData,0), //why does zero not work here????
                                
            };
            //config what things will be extracated from the shaders
            layout = new InputLayout(device, inputSignature, elements);

        }


        /// <summary>
        /// Creates constant buffers
        /// All buffers must be multiples of 16 bytes
        /// </summary>
        /// <param name="device"></param>
        /// <param name="context"></param>
        public void CreateConstBuffer(DeviceContext context)
        {
            context = device.ImmediateContext;
            int size = Marshal.SizeOf(typeof(CBUFFER));


            constantBuffer = new Buffer
              (
               device,
               new BufferDescription
               {
                   BindFlags = BindFlags.ConstantBuffer,
                   CpuAccessFlags = CpuAccessFlags.None,
                   OptionFlags = ResourceOptionFlags.None,
                   SizeInBytes = size,
                   Usage = ResourceUsage.Default,
                   StructureByteStride = 0
               }
              );
        }

        /// <summary>
        /// moves the camera around on the scene
        /// at this point only adjusts the height of camera on the cube
        /// </summary>
        /// <param name="height">adjusts camera height looking at cube</param>
        public void moveCamera(float height)
        {
            myLook = smyLook = Matrix.LookAtLH(new Vector3(2.0f, height, 2.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
        }

        /// <summary>
        /// Draws all objects on screen, holds drawing logic
        /// </summary>
        /// <param name="context"></param>
        /// <param name="device"></param>
        /// <param name="textpack"></param>        
        public void Draw(DeviceContext context, List<Texture2D> textpack, Form form)
        {
            // configure the Input Assembler portion of the pipeline with the vertex data
            //set shaders and buffers                        

            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);
            context.VertexShader.SetConstantBuffer(constantBuffer, 0);
            context.PixelShader.SetConstantBuffer(constantBuffer, 0);
            //#########
            //Drawing logic
            //#########
            //DEMO 1
            //cap initially starts at zero
            ////position cube, then draw  
            if (Cap < 1.5f)
            {

                positionCube(context, Time, 3.14f);
            }

            if (Cap >= 1.5f)
            {
                //test to see how cube will face up
                zoomCube(context, 0.8f);
            }
            drawCube(context, SRVlist);

            //enable blending
            context.OutputMerger.BlendState = enabled;
            InitState(device);
            //position screen, then draw

            //game logic in seperate class 
            //not yet sure how to transform from screen coordinates to world coordinates, for the moment, just using brute force
            //swipe should generate the screen "flying" away
            if (Cap >= 1.5f)
            {
                updateScreen(context, 0.0f, 0.0f, 1.333f, 0.59f, 1.0f);
                drawScreen(context, SRVscreen);

            }
            if (Cap < 1.5f)
            {
                updateScreen(context, 1.23f, 0.785f, 1.333f, 0.59f, 1.0f);
                drawScreen(context, SRVscreen);
                Cap += 0.0001f;
            }

            //set the cubes rotating position                    
            //use world coordinates to let the cube zoom to fill the screen.... have that bring up a blank form... and then exiting brings back the cube with updated screenshot pics...
            context.OutputMerger.BlendState = null;
        }
        /// <summary>
        /// Zooms the cube in based on variable time, calculates correct face to zoom
        /// </summary>
        /// <param name="context"></param>
        /// <param name="limit">how much to zoom in</param>
        public void zoomCube(DeviceContext context, float limit)
        {


            if (Zoom < limit)
            {
                //takes the current rotation value, and finds which side of the cube is being selected
                float[] cubepos = { 0.0f, 1.57f * 1.0f, 1.57f * 2.0f, 1.57f * 3.0f };
                //divide by rotation number
                //take the time and mod by four
                float temp = (Time - 0.785f) / 1.57f;
                int temp1 = (int)Math.Ceiling(temp);
                int index = temp1 % 4;
                moveCamera(0.0f);
                //position the cube
                positionCube(context, 0.0f, cubepos[index] + 0.785f, 3.14f, Zoom, 0.0f, Zoom, true);
                currentFace = index;
                Zoom += 0.001f;
            }
            //otherwise reset.. this is just for demo purposes
            else
            {
                //send a pause event.
                press_pause(this, new Pause_Form_Event(true));

                //open the selected face of the cube... will send events back to this class...
                SubScreen_Handler SSH = new SubScreen_Handler(currentFace);

                //get the command whether to pause or not
                SSH.press_pause += new PauseEvent(getState);
                SSH.change_face += new ChangeFace(getNewFace);
                SSH.RenderForm();

                //reset the original cube
                Cap = 0.0f;
                Zoom = 0.0f;
                //reset camera to initial position
                moveCamera(1.0f);
            }
        }


        /// <summary>
        /// Receives an alert when its time for the system to wake back up
        /// </summary>
        /// <param name="a"></param>
        /// <param name="e"></param>
        public void getState(object a, Pause_Form_Event e)
        {
            press_pause(this, new Pause_Form_Event(e.pause));
        }
        public void InitState(Device device)
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
        /// <summary>
        /// Updates Matrices to change the position and rotation of home screen cube
        /// </summary>
        /// <param name="context"></param>
        /// <param name="x">control x rotation</param>
        /// <param name="y">control y rotation</param>
        /// <param name="scaleX">change cube size in X</param>
        /// <param name="scaleY">change cube size in Y</param>
        /// <param name="scaleZ">change cube size in Z</param>
        private void positionCube(DeviceContext context, float y, float x)
        {

            mRotateY = Matrix.RotationY(y);
            mRotateX = Matrix.RotationX(x);

            //still have to fix lighting if x rotation is invoked...
            matFinal = mRotateX* mRotateY * myLook * matProjection;
            cbuffer.Rotatation = mRotateX* mRotateY;
            cbuffer.Final = matFinal;
            int size = Marshal.SizeOf(typeof(CBUFFER));
            var update = new DataStream(size, true, true);
            update.Write<CBUFFER>(cbuffer);
            update.Position = 0;
            //update vertices on gpu
            context.UpdateSubresource(new DataBox(0, 0, update), constantBuffer, 0);
            Time += 0.0005f;

        }
        /// <summary>
        /// more precise positioning, can also choose the mode in which the enlargement of cube can occur, used in select animation.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="scaleZ"></param>
        private void positionCube(DeviceContext context, float z, float y, float x, float scaleX, float scaleY, float scaleZ, bool value = false)
        {
            mRotateZ = Matrix.RotationZ(z);
            mRotateY = Matrix.RotationY(y);
            mRotateX = Matrix.RotationX(x);

            mScale = Matrix.Scaling(scaleX, scaleY, scaleZ);
            mTranslate = Matrix.Translation(scaleX, scaleY, scaleZ);
            //if scaling is true, then scale + translate... otw just scale            

            matFinal = mRotateX * mRotateY * mRotateZ * mTranslate * myLook * matProjection;
            //matFinal = mRotateX * myLook * matProjection;
            cbuffer.Rotatation = mRotateX*mRotateY*mRotateZ;
            cbuffer.Final = matFinal;
            int size = Marshal.SizeOf(typeof(CBUFFER));
            var update = new DataStream(size, true, true);
            update.Write<CBUFFER>(cbuffer);
            update.Position = 0;
            //update vertices on gpu
            context.UpdateSubresource(new DataBox(0, 0, update), constantBuffer, 0);
            if (value == false)
            {
                Time += 0.00005f;
            }
        }
        /// <summary>
        /// update matrices to control screen movement, controls screen size as well
        /// </summary>
        /// <param name="context"></param>
        /// <param name="x">x rotation</param>
        /// <param name="y">y rotation</param>
        /// <param name="scaleX">scale in x axis, left and right</param>
        /// <param name="scaleY">scale in y axis, coming toward camera</param>
        /// <param name="scaleZ">scale in z axis up and down</param>
        private void updateScreen(DeviceContext context, float x, float y, float scaleX, float scaleY, float scaleZ)
        {
            //axis map
            //        z            
            //        |
            //        |
            //        |
            //       / \
            //    x       y



            sRotate1 = Matrix.RotationX(x);
            sRotateX = Matrix.RotationY(y);

            sScale = Matrix.Scaling(scaleX, scaleY, scaleZ);
            sTranslate = Matrix.Translation(scaleX, scaleY, scaleZ);
            screenFinal = sScale * sRotate1 * sRotateX * myLook * matProjection;

            cbuffer1.Rotatation = sRotateX * sRotate1;
            cbuffer1.Final = screenFinal;
            int size = Marshal.SizeOf(typeof(CBUFFER));
            var update = new DataStream(size, true, true);
            update.Write<CBUFFER>(cbuffer1);
            update.Position = 0;
            //update vertices on gpu
            context.UpdateSubresource(new DataBox(0, 0, update), constantBuffer, 0);

        }
        /// <summary>
        /// update screen coordinates for rotation
        /// </summary>
        /// <param name="context"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="scaleZ"></param>
        private void updateScreen(DeviceContext context, float x, float y)
        {
            mRotate1 = Matrix.RotationX(x);
            //sRotateX = Matrix.RotationY(y);
            screenFinal = sRotateX * smyLook * smatProjection;
            cbuffer1.Rotatation = sRotateX;
            cbuffer1.Final = screenFinal;
            int size = Marshal.SizeOf(typeof(CBUFFER));
            var update = new DataStream(size, true, true);
            update.Write<CBUFFER>(cbuffer1);
            update.Position = 0;
            //update vertices on gpu
            context.UpdateSubresource(new DataBox(0, 0, update), constantBuffer, 0);
        }
        /// <summary>
        /// Draws cube using specified vertex and index buffers
        /// </summary>
        /// <param name="device"></param>
        /// <param name="context"></param>
        /// <param name="resourceView"></param>
        private void drawCube(DeviceContext context, List<ShaderResourceView> SRVlist)
        {
            int indexcount = 0;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(Cube, 32, 0));
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
            for (int ii = 0; ii < SRVlist.Count; ii++)
            {
                device.ImmediateContext.PixelShader.SetShaderResource(SRVlist[ii], 0);
                context.DrawIndexed(36, indexcount, 0);
                indexcount += 6;
            }
        }
        /// <summary>
        /// draws transparent screen using specified vertex and index buffers... later turn this and draw cube into 1
        /// </summary>
        /// <param name="device"></param>
        /// <param name="context"></param>
        /// <param name="resourceView1"></param>
        private void drawScreen(DeviceContext context, ShaderResourceView resourceView1)
        {
            device.ImmediateContext.PixelShader.SetShaderResource(resourceView1, 0);
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(Screen, 32, 0));
            context.InputAssembler.SetIndexBuffer(screenIndexBuffer, Format.R32_UInt, 0);
            context.DrawIndexed(6, 0, 0);
        }

        private void ReleaseDXResources(object sender, FormClosingEventArgs e)
        {
            constantBuffer.Dispose();
            layout.Dispose();
            inputSignature.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
        }
    }
}



