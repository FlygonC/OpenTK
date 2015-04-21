using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Example
{
    class Program
    {
        //GameWindow is a Class implemented by OpenTK that works in place of GLFW or the like
        class Game : GameWindow
        {
            //ShaderProgram class Dictionary
            Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
            string activeShader = "default";
            //Element Buffer
            int ibo_elements;
            //Buffer data
            Vector3[] vertdata;
            Vector3[] coldata;
            Matrix4[] mviewdata;
            int[] indicedata;//Index Buffer

            float time = 0.0f;

            //Custom Shader loading from file
            /*void loadShader(String filename, ShaderType type, int program, out int address)
            {
                address = GL.CreateShader(type);
                using (StreamReader sr = new StreamReader(filename))
                {
                    GL.ShaderSource(address, sr.ReadToEnd());
                }
                GL.CompileShader(address);
                GL.AttachShader(program, address);
                Console.WriteLine(GL.GetShaderInfoLog(address));
            }*/
            //creating a shader program
            //Initialize program for shaders
            void InitProgram()
            {
                GL.GenBuffers(1, out ibo_elements);

                shaders.Add("default", new ShaderProgram("Shaders/vs.glsl", "Shaders/fs.glsl", true));
            }
            
            //By overriding the functions of GameWindow, I can take control and make it work to my specifications
            //OnLoad is called when the context or window is first opened, I use this to change the title of the window and also the clear color
            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                InitProgram();
                //create buffer data
                vertdata = new Vector3[] { 
                    new Vector3(-0.8f, -0.8f,  -0.8f),
                    new Vector3(0.8f, -0.8f,  -0.8f),
                    new Vector3(0.8f, 0.8f,  -0.8f),
                    new Vector3(-0.8f, 0.8f,  -0.8f),
                    new Vector3(-0.8f, -0.8f,  0.8f),
                    new Vector3(0.8f, -0.8f,  0.8f),
                    new Vector3(0.8f, 0.8f,  0.8f),
                    new Vector3(-0.8f, 0.8f,  0.8f),
                };
                coldata = new Vector3[] { 
                    new Vector3(1f, 0f, 0f),
                    new Vector3( 0f, 0f, 1f), 
                    new Vector3( 1f,  0f, 0f),
                    new Vector3(0f, 0f, 1f),
                    new Vector3( 0f, 0f, 1f), 
                    new Vector3( 1f,  0f, 0f),
                    new Vector3(0f, 0f, 1f),
                    new Vector3( 1f, 0f, 0f)
                }; 
                mviewdata = new Matrix4[]{
                    Matrix4.Identity
                };
                indicedata = new int[]{
                    //front
                    0, 7, 3,
                    0, 4, 7,
                    //back
                    1, 2, 6,
                    6, 5, 1,
                    //left
                    0, 2, 1,
                    0, 3, 2,
                    //right
                    4, 5, 6,
                    6, 7, 4,
                    //top
                    2, 3, 6,
                    6, 3, 7,
                    //bottom
                    0, 1, 5,
                    0, 5, 4
                };

                //other window stuff
                Title = "Hello OpenTK!";
                GL.ClearColor(Color.Magenta);
                GL.PointSize(5f);
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {//OnUpdateFrame is called every Frame
                base.OnUpdateFrame(e);

                time += (float)e.Time;
                
                //Buffer handeling

                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

                if (shaders[activeShader].GetAttribute("vColor") != -1)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                    GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
                }
                
               

                //uniform matrix
                mviewdata[0] = Matrix4.CreateRotationY(1f * time) * 
                    Matrix4.CreateRotationX(0.5f * time) * 
                    Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f) * 
                    Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f);
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref mviewdata[0]);
                //Index Buffer
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);
                //Enable Program
                GL.UseProgram(shaders[activeShader].ProgramID);
                //Clear buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            //OnRenderFrame is when the window contents are drawn, ie GL data is loaded and swapbuffers is called
            protected override void OnRenderFrame(FrameEventArgs e)
            {//OnRenderFrame is called every Refresh
                base.OnRenderFrame(e);

                GL.Viewport(0, 0, Width, Height);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.Enable(EnableCap.DepthTest);

                //GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);
                //drawing with ShaderProgram
                shaders[activeShader].EnableVertexAttribArrays();

                GL.DrawElements(BeginMode.Triangles, indicedata.Length, DrawElementsType.UnsignedInt, 0);

                shaders[activeShader].DisableVertexAttribArrays();
 
                GL.Flush();

                //Drawing with GL, doing it like this is called "Immediate Mode" and is "incorrect"
                /*GL.Begin(PrimitiveType.Triangles);

                GL.Color3(1.0f, 0.0f, 0.0f);
                GL.Vertex3(-1.0f, -1.0f, 4.0f);

                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(1.0f, -1.0f, 4.0f);

                GL.Color3(0.0f, 0.0f, 1.0f);
                GL.Vertex3(0.0f, 1.0f, 4.0f);

                GL.End();*/

                SwapBuffers();
            }
            //OnResize is called whenever the window is resized by the user, I use this function to change the projection to stay fitted(though I think I would rather have it stretch, here it recenters)
            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);

                //GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

                //Matrix4 projection = Matrix4.CreateOrthographic(ClientSize.Width, ClientSize.Height, 0, 10);

                //GL.MatrixMode(MatrixMode.Projection);

                //GL.LoadMatrix(ref projection);
            }
        }

        public static void Main()
        {
            using (Game game = new Game())
            {
                //The GameWindow has its own loop, Run starts this loop with parameters for framerate and refresh rate
                game.Run(30, 30);
            }
        }
    }
}