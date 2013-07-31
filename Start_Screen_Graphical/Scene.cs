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
using FullSkeletonSKD1;
using System.Threading;
using System.IO;


namespace Start_Screen_Graphical
{
    /// <Scene Class Description>
    /// This Class Renders over the blank form created using Vertex,Index, and Const Buffers
    /// Matrix Multiplications allow the cube to take a location relative to the camera on screen as well as 
    /// Rotate and Scale
    /// </summary>
    public class Scene
    {
        //STRUCT DEFINITIONS.

        //Holds vertices along with normals and textcoords.
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
        //Struct defining Constant Buffer info for cube.
        struct CBUFFER
        {
            public Matrix Final;
            public Matrix Rotatation;
            public Vector4 LightVector;
            public Vector4 LightColor;
            public Vector4 AmbientColor;
        }

        //VIEW MATRICES

        //cube
        Matrix mTranslate, mScale, myLook, matProjection, mRotate1;
        //screen
        Matrix sTranslate, sScale, smyLook, smatProjection, sRotate1;
        //world matrices
        Matrix matFinal, screenFinal;
        //Rotation matrices
        //cube
        Matrix mRotateX, mRotateY, mRotateZ;
        //screen
        Matrix sRotateX;


        Device device;

        InputLayout layout;
        ShaderSignature inputSignature;
        //textures
        ShaderResourceView skeletonSRV,SRVscreen, SRVAlert1, SRVAlert2, SRV, SRV1, SRV2, SRV3, SRV4, SRV5;
        //texture list for cube
        List<ShaderResourceView> SRVlist = new List<ShaderResourceView>();
        //texture list for alert
        List<ShaderResourceView> SRVAlertlist = new List<ShaderResourceView>();

        PixelShader pixelShader;
        VertexShader vertexShader;
        Buffer Cube, Screen, Alert;
        Buffer constantBuffer;
        Buffer indexBuffer;
        Buffer screenIndexBuffer;
        Buffer alertIndexBuffer;
        CBUFFER cbuffer, cbuffer1;
        BlendState enabled;

        //objects
        SubScreen_Handler SSH;
        //SkeletonImage skImg;
        private Form_Utilities form_utilities = new Form_Utilities();
        GestureEngine gestEngine;
        Bitmap bmp;
        //MemoryStream stream = new MemoryStream()
        //event handler
        public event PauseEvent press_pause;
        public event ChangeFace change_face;

        //VARIABLES
        public static float Time = 0.785f;
        public static float timeCap = 0.785f;
        public static float Zoom = 0.0f;
        public static float Cap = 0.0f;
        public static float Cap1 = 0.59f;
        public static float width = 0.35f;
        public static float width1;
        public bool zoom = true;
<<<<<<< HEAD
        public bool screened = false;
        public bool can_swipe = true;
        public bool signed_in = false;
        bool lock_screen = false;
        bool initial_lock = false;
=======
        //moing screen for demo purposes
        public bool screened = false;

        //##############
        //Other Variables
        //##############
>>>>>>> 4b3e4cb880c4384ca3451a07175b67692ddbcc22
        int currentFace = 0;
        //Default gesture
        int currentGesture = 10;
        public float before_zoom = 0.785f;
        //Current index of face
        float face_index = 0;
        float face_pos = 0;
        public int state = 0;

        /// <summary>
        /// Default constructor for scene initialization
        /// </summary>
        /// <param name="device"></param>
        /// <param name="form"></param>
        /// <param name="context"></param>
        public Scene(Device device, Form form, DeviceContext context)
        {
            //Get the input for a new gesture
            gestEngine = new GestureEngine();
            //Initialize the subscreen handler
            SSH = new SubScreen_Handler(gestEngine);
            //initialize new skeleton image detection
            //skImg = new SkeletonImage();

            gestEngine.reset();
            gestEngine.init();
            gestEngine.GestureChanged += new Start_Screen_Graphical.GestureEngine.NewGestureEventHandler(gestEngine_GestureChanged);
            this.device = device;
            initTextures();
            Initialize(form, context);
            CreateCube();
            createScreen();
            createAlert();
            CreateConstBuffer(context);
            CubeIndexBuffer();
            ScreenIndexBuffer();
            AlertIndexBuffer();
            BuildShaderInputLayout();
        }

        /// <summary>
        /// Gets new gesture ID from Kinect via Gesture Engine
        /// </summary>
        /// <param name="newGestureID"></param>
        private void gestEngine_GestureChanged(int newGestureID)
        {
            if (lock_screen == false && initial_lock == false)
            {
                //Updates Global current gesture.
                currentGesture = newGestureID;

                if (currentGesture == 0 || currentGesture == 1)
                {
                    lock_screen = true;
                }

                if (currentGesture == 0)
                {
                    timeCap += 1.57f;
                }

                if (currentGesture == 1)
                {
                    timeCap -= 1.57f;
                }

                if (currentGesture == 2)
                {
                    lock_screen = true;
                }
            }

        }

        /// <summary>
        /// Applies a new texture to the cube representing last state of closed form.
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

            //parse the string to take out the number...
            String temp = e.file_extension;
            String ext = temp.Substring(0, (temp.Length - 1));
            String faceNum = temp.Substring(temp.Length - 1, 1);
            int index = Convert.ToInt32(faceNum);

            switch (index)
            {
                //first face
                case 0:
                    //set it to the purple face
                    //mirror the extension
                    form_utilities.mirrorImage(ext);
                    SRVlist[1] = ShaderResourceView.FromFile(device, ext);
                    break;
                //second face
                case 1:
                    //set it to the orange face
                    SRVlist[5] = ShaderResourceView.FromFile(device, ext);
                    break;
                //third face
                case 2:
                    //set it to the red face
                    SRVlist[0] = ShaderResourceView.FromFile(device, ext);
                    break;
                //fourth face
                case 3:
                    //set it to the yellow face
                    //mirror the face due to culling
                    form_utilities.mirrorImage(ext);
                    SRVlist[4] = ShaderResourceView.FromFile(device, ext);
                    break;
            }

        }

        /// <summary>
        /// Set up Shader Resource Views for initial rendering.
        /// </summary>
        private void initTextures()
        {

            //screen textures are mirrored in wrong, because of culling or transparency
            String img_directory = form_utilities.getDirectory() + "\\images\\Sample Pictures";

            //SRVscreen = ShaderResourceView.FromStream(device,)
            SRVscreen = ShaderResourceView.FromFile(device, img_directory + "\\metallic.jpg");

            //alert            
            SRVAlert1 = ShaderResourceView.FromFile(device, img_directory + "\\stop.png");
            SRVAlert2 = ShaderResourceView.FromFile(device, img_directory + "\\go.jpg");
            //skeleton
            skeletonSRV = ShaderResourceView.FromFile(device, img_directory + "\\black.jpg");

            //top and bottom
            SRV2 = ShaderResourceView.FromFile(device, img_directory + "\\green.jpg");
            SRV3 = ShaderResourceView.FromFile(device, img_directory + "\\blue.jpg");

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

            //add to cube faces
            SRVlist.Add(SRV);
            SRVlist.Add(SRV1);
            SRVlist.Add(SRV2);
            SRVlist.Add(SRV3);
            SRVlist.Add(SRV4);
            SRVlist.Add(SRV5);

            //add to alert faces
            SRVAlertlist.Add(SRVAlert1);
            SRVAlertlist.Add(SRVAlert2);
        }
        /// <summary>
        /// Initialize all rotation Matrices
        /// </summary>
        /// <param name="form"></param>
        /// <param name="context"></param>
        public void Initialize(Form form, DeviceContext context)
        {

            //Set up translation matrix.
            myLook = smyLook = Matrix.LookAtLH(new Vector3(2.0f, 1.0f, 2.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            //Set up projection matrix for camera
            matProjection = smatProjection = Matrix.PerspectiveFovLH((float)((float)45 * ((float)3.14 / (float)180)), (float)form.ClientSize.Width / (float)form.ClientSize.Height, 1.0f, 100.0f);
            //Multiply Matrices for camera
            matFinal = mRotateX * mRotate1 * myLook * matProjection;
            //Set up rotation matrix, gets updated in Draw Method
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
        /// Set up cube vertices.
        /// </summary>
        /// <param name="device"></param>
        public void CreateCube()
        {

            int size = Marshal.SizeOf(typeof(myVertex));
            // create test vertex data, making sure to rewind the stream afterward
            DataStream vertices = new DataStream(size * 24 * 7, true, true);
            // Side 1 //Red
            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0f, 0.0f, 1.0f), new Vector2(1.0f, 1.0f)));
            //Side 2, purple.
            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(0.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.0f, 0.0f, -1.0f), new Vector2(1.0f, 1.0f)));
            //Side 3, green.
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f)));
            //Side 4, ?
            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(0.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(1.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.0f, -1.0f, 0.0f), new Vector2(1.0f, 0.0f)));
            //Side 5, yellow
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1.0f, 1.0f)));
            //Side 6, orange
            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector2(1.0f, 1.0f)));
            vertices.Position = 0;

            //Vertex Buffer.
            Cube = new Buffer(device, vertices, size * 24 * 7, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
        }
        /// <summary>
        /// Set up Transparent Screen
        /// </summary>
        public void createScreen()
        {

            int size = Marshal.SizeOf(typeof(myVertex));
            // create test vertex data, making sure to rewind the stream afterward
            DataStream vertices = new DataStream(size * 24, true, true);

            vertices.Write(new myVertex(new Vector3(-1.0f, 1.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f)));    // side 3
            vertices.Write(new myVertex(new Vector3(-1.0f, 1.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(1.0f, 1.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f)));
            vertices.Position = 0;

            //Vertex Buffer
            Screen = new Buffer(device, vertices, size * 24, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

        }
        /// <summary>
        ///  Create an alert square on the top right to guide user gestures.
        /// </summary>
        public void createAlert()
        {

            //for green or red cube
            int size = Marshal.SizeOf(typeof(myVertex));
            // create test vertex data, making sure to rewind the stream afterward
            DataStream vertices = new DataStream(size * 24, true, true);

            vertices.Write(new myVertex(new Vector3(-1.0f, 1.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f)));    // side 3
            vertices.Write(new myVertex(new Vector3(-1.0f, 1.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f)));
            vertices.Write(new myVertex(new Vector3(1.0f, 1.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f)));
            vertices.Write(new myVertex(new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f)));
            vertices.Position = 0;

            //Vertex Buffer
            Alert = new Buffer(device, vertices, size * 24, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

        }
        /// <summary>
        /// Set up Cube Index Buffer.
        /// </summary>
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
            DataStream indices = new DataStream(size * 42, true, true);
            indices.WriteRange(OurIndices);
            indices.Position = 0;
            indexBuffer = new Buffer(device, indices, Marshal.SizeOf(typeof(uint)) * 42, ResourceUsage.Dynamic, BindFlags.IndexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0); //*
        }
        /// <summary>
        /// Set up Screen Index Buffer
        /// </summary>
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

        /// <summary>
        /// Set up Alert Index Buffer
        /// </summary>
        public void AlertIndexBuffer()
        {
            int size = sizeof(uint);
            uint[] OurIndices = 
            {  
                0, 1, 2,    // side 1
                2, 1, 3,  
            };
            DataStream indices = new DataStream(size * 6, true, true);
            indices.WriteRange(OurIndices);
            indices.Position = 0;
            alertIndexBuffer = new Buffer(device, indices, Marshal.SizeOf(typeof(uint)) * 6, ResourceUsage.Dynamic, BindFlags.IndexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

        }

        /// <summary>
        /// Compile shader code and add it with Textures.
        /// </summary>
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
                new InputElement("NORMAL", 0 ,SlimDX.DXGI.Format.R32G32B32_Float,12,0,InputClassification.PerVertexData,0), 
                new InputElement("TEXCOORD", 0 ,SlimDX.DXGI.Format.R32G32B32_Float,24,0,InputClassification.PerVertexData,0),                                
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
            //this.bmp = skImg.getImage();
            this.bmp = gestEngine.getImage();
            if (bmp != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    // Save image to stream.
                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    stream.Position = 0;
                    SRVscreen = ShaderResourceView.FromStream(device, stream, (int)stream.Length);
                }
            }
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);
            context.VertexShader.SetConstantBuffer(constantBuffer, 0);
            context.PixelShader.SetConstantBuffer(constantBuffer, 0);

            //BEGIN DRAWING LOGIC

            //if there is no inputted gesture stay as is
            if (currentGesture == 10 || signed_in == false)
            {
                positionCube(context, 1.0f, 3.14f);
            }
            if (currentGesture == 11 && signed_in == true)
            {
                positionCube(context, face_pos, 3.14f);
            }

            //if right gesture
            if (currentGesture == 0 && signed_in == true)
            {
<<<<<<< HEAD
                screened = true;
                //position alerts                    
                drawAlert(context, SRVAlertlist);
                positionCube(context, Time, 3.14f);

            }

            //if left gesture
            if (currentGesture == 1 && signed_in == true)
            {
                screened = true;
                //position alerts                    
                drawAlert(context, SRVAlertlist);
                positionCube(context, Time, 3.14f);
=======
                if (Cap1 > -2.0f && screened == false)
                {
                    positionCube(context, 1.0f, 3.14f);
                }
                else
                {
                    screened = true;
                    positionCube(context, Time, 3.14f);
                }
>>>>>>> 4b3e4cb880c4384ca3451a07175b67692ddbcc22
            }

            //if zoomed is called
            if (currentGesture == 2 && signed_in == true)
            {
                
                //test to see how cube will face up
                zoomCube(context, 0.8f);
            }
            //draw cube
            drawCube(context, SRVlist);

            //enable blending
            context.OutputMerger.BlendState = enabled;
            InitState(device);

            //position screen, then draw
            //Caps control when screen should dissapear.
            if (currentGesture == 3 && Cap < 0.5f)
            {
                if (screened == false)
                {
<<<<<<< HEAD
                    signed_in = true;
                    //make the screen shrink
                    initial_lock = true;
                    drawAlert(context, SRVAlertlist);
                    updateScreen(context, 1.23f, 0.785f, 1.333f, Cap1, 1.0f);
                    drawScreen(context, SRVscreen);
                    positionCube(context, 1.0f, 3.14f);
                }
                Cap += 0.005f;
                Cap1 -= 0.05f;
=======
                    //shift screen to start with
                    updateScreen(context, 0.0f, 0.0f, 1.333f, 0.59f, 1.0f);
                    drawScreen(context, SRVscreen);
                }

>>>>>>> 4b3e4cb880c4384ca3451a07175b67692ddbcc22
            }
            else if (Cap < 0.5f)
            {
<<<<<<< HEAD
                updateScreen(context, 1.23f, 0.785f, 1.333f, 0.59f, 1.0f);
                drawScreen(context, SRVscreen);
            }
            else
            {
                initial_lock = false;
                screened = true;
=======
                if (screened == false)
                {
                    updateScreen(context, 1.23f, 0.785f, 1.333f, Cap1, 1.0f);
                    drawScreen(context, SRVscreen);
                }
                Cap += 0.0001f;
                Cap1 -= 0.001f;
>>>>>>> 4b3e4cb880c4384ca3451a07175b67692ddbcc22
            }

            //set the cubes rotating position                    
            //use world coordinates to let the cube zoom to fill the screen.... have that bring up a blank form... and then exiting brings back the cube with updated screenshot pics...
            context.OutputMerger.BlendState = null;

            //draw skeleton stream on top???
            drawSkeletonStream(context, skeletonSRV);

        }

        /// <summary>
        /// Zooms the cube in based on variable time, calculates correct face to zoom
        /// </summary>
        /// <param name="context"></param>
        /// <param name="limit">how much to zoom in</param>
        public void zoomCube(DeviceContext context, float limit)
        {

            //float index1 = 0;
            float[] cubepos = { 0.0f, 1.57f * 1.0f, 1.57f * 2.0f, 1.57f * 3.0f };

            //reversed index positions for backwards cube
            float[] cubepos_neg = { 1.57f * 3.0f, 1.57f * 2.0f, 1.57f * 1.0f, 0.0f };

            if (Zoom < limit)
            {
                //takes the current rotation value, and finds which side of the cube is being selected
                float temp = (Time) / 1.57f;

                if (Time >= 0.785f)
                {
                    //round down
                    temp = (float)Math.Floor(temp);
                    //index1 = Form_Utilities.nfmod(temp, 4.0f);
                    face_index = temp % 4;
                    //reset world camera
                    moveCamera(0.0f);
                    positionCube(context, 0.0f, cubepos[(int)face_index] + 0.785f, 3.14f, Zoom, 0.0f, Zoom, true);
                    currentFace = (int)face_index;
                    face_pos = cubepos[(int)face_index] + 0.785f;
                }
                //if the cube is turning in negative  
                else if (Time < 0)
                {
                    //round down
                    temp = (float)Math.Floor(temp);
                    face_index = Form_Utilities.nfmod(temp, 4.0f);
                    //Reset world camera.
                    moveCamera(0.0f);
                    positionCube(context, 0.0f, cubepos[(int)face_index] + 0.785f, 3.14f, Zoom, 0.0f, Zoom, true);
                    currentFace = (int)face_index;
                    face_pos = cubepos[(int)face_index] + 0.785f;
                }
                Zoom += 0.01f;
            }
            //Reset cube to rest on the face that was just picked.
            else
            {
                //send a pause event.
                press_pause(this, new Pause_Form_Event(true));

                //open the selected face of the cube... will send events back to this class...
                // SubScreen_Handler SSH = new SubScreen_Handler(currentFace, gestEngine);
                SSH.setScreen(currentFace);

                //get the command whether to pause or not
                SSH.press_pause += new PauseEvent(getState);
                SSH.change_face += new ChangeFace(getNewFace);
                SSH.RenderForm();

                //reset the original cube                
                currentGesture = 11;
                Zoom = 0.0f;

                //set everything to the index position, and rotate from there                
                //before_zoom = cubepos[(int)face_index] + 0.785f;

                timeCap = Time;
                //reset camera to initial position
                moveCamera(1.0f);
                lock_screen = false;
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
            matFinal = mRotateX * mRotateY * myLook * matProjection;
            cbuffer.Rotatation = mRotateX * mRotateY;
            cbuffer.Final = matFinal;
            int size = Marshal.SizeOf(typeof(CBUFFER));
            var update = new DataStream(size, true, true);
            update.Write<CBUFFER>(cbuffer);
            update.Position = 0;
            //update vertices on gpu
            if (currentGesture == 0)
            {
                if (Time < timeCap)
                {
                    Time += 0.05f;
                }
                else
                {
                    lock_screen = false;
                    currentGesture = 12;
                }
            }
            if (currentGesture == 1)
            {
                if (Time > timeCap)
                {
                    Time -= 0.05f;
                }
                else
                {
                    lock_screen = false;
                    currentGesture = 12;
                }
            }
            context.UpdateSubresource(new DataBox(0, 0, update), constantBuffer, 0);
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
            cbuffer.Rotatation = mRotateX * mRotateY * mRotateZ;
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
            //     x     y

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
            //Update vertices on GPU.
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
        /// Alows for positioning of the Alert object
        /// (now also includes skeleton stream object)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="x">X rotation</param>
        /// <param name="y">Y rotation</param>
        /// <param name="scaleX">scale in X</param>
        /// <param name="scaleY">scale in Y</param>
        /// <param name="scaleZ">scale in Z</param>
        /// <param name="transX">translate object in X</param>
        /// <param name="transY">translate object in Y</param>
        /// <param name="transZ">translate object in Z</param>
        private void updateAlert(DeviceContext context, float x, float y, float scaleX, float scaleY, float scaleZ, float transX, float transY, float transZ)
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
            sTranslate = Matrix.Translation(transX, transY, transZ);
            screenFinal = sScale * sTranslate * sRotate1 * sRotateX * myLook * matProjection;
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
        public void drawAlert(DeviceContext context, List<ShaderResourceView> resourceView1)
        {
            //position the alert in the window first.. it will be fixed anyways
            updateAlert(context, 1.23f, 0.785f, 1.333f, -3.0f, 1.0f, -3.4f, -3.5f, -2.9f);

            if (can_swipe)
            {
                device.ImmediateContext.PixelShader.SetShaderResource(resourceView1[0], 0);
            }
            //stop
            if (!can_swipe)
            {
                device.ImmediateContext.PixelShader.SetShaderResource(resourceView1[1], 0);
            }
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(Alert, 32, 0));
            context.InputAssembler.SetIndexBuffer(alertIndexBuffer, Format.R32_UInt, 0);
            context.DrawIndexed(6, 0, 0);
        }
        /// <summary>
        /// Draws the Skeleton Stream
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resourceView1"></param>
        public void drawSkeletonStream(DeviceContext context, ShaderResourceView skeletonSRV)
        {
            //position the alert in the window first.. it will be fixed anyways
            updateAlert(context, 1.23f, 0.785f, 1.333f, -3.0f, 1.3f, -3.7f, -3.5f, 2.5f);
            device.ImmediateContext.PixelShader.SetShaderResource(skeletonSRV, 0);
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(Alert, 32, 0));
            context.InputAssembler.SetIndexBuffer(alertIndexBuffer, Format.R32_UInt, 0);
            context.DrawIndexed(6, 0, 0);
        }



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

