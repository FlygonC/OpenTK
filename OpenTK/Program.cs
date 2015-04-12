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
            //Shaders
            int pgmID, vsID, fsID;
            //Shader Attributes
            int attribute_vcol;
            int attribute_vpos;
            int uniform_mview;
            //Finally Vertex Buffers
            int vbo_position;
            int vbo_color;
            int vbo_mview;
            //Buffer data
            Vector3[] vertdata;
            Vector3[] coldata;
            Matrix4[] mviewdata;

            //Custom Shader loading from file
            void loadShader(String filename, ShaderType type, int program, out int address)
            {
                address = GL.CreateShader(type);
                using (StreamReader sr = new StreamReader(filename))
                {
                    GL.ShaderSource(address, sr.ReadToEnd());
                }
                GL.CompileShader(address);
                GL.AttachShader(program, address);
                Console.WriteLine(GL.GetShaderInfoLog(address));
            }
            //creating a shader program
            //Initialize program for shaders and program
            void InitProgram()
            {
                pgmID = GL.CreateProgram();

                loadShader("vs.glsl", ShaderType.VertexShader, pgmID, out vsID);
                loadShader("fs.glsl", ShaderType.FragmentShader, pgmID, out fsID);

                GL.LinkProgram(pgmID);
                Console.WriteLine(GL.GetProgramInfoLog(pgmID));

                attribute_vpos = GL.GetAttribLocation(pgmID, "vPosition");
                attribute_vcol = GL.GetAttribLocation(pgmID, "vColor");
                uniform_mview = GL.GetUniformLocation(pgmID, "modelview");

                if (attribute_vpos == -1 || attribute_vcol == -1 || uniform_mview == -1)
                {
                    Console.WriteLine("Error binding attributes");
                }

                GL.GenBuffers(1, out vbo_position);
                GL.GenBuffers(1, out vbo_color);
                GL.GenBuffers(1, out vbo_mview);
            }
           


            //By overriding the functions of GameWindow, I can take control and make it work to my specifications
            //OnLoad is called when the context or window is first opened, I use this to change the title of the window and also the clear color
            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                InitProgram();
                //create buffer data
                vertdata = new Vector3[] { 
                    new Vector3(-0.8f, -0.8f, 0f),
                    new Vector3( 0.8f, -0.8f, 0f),
                    new Vector3( 0f,  0.8f, 0f)
                };
                coldata = new Vector3[] { 
                    new Vector3(1f, 0f, 0f),
                    new Vector3( 0f, 0f, 1f),
                    new Vector3( 0f,  1f, 0f)
                };
                mviewdata = new Matrix4[]{
                    Matrix4.Identity
                };

                //other window stuff
                Title = "Hello OpenTK!";

                GL.ClearColor(Color.Black);
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {//OnUpdateFrame is called every Frame
                base.OnUpdateFrame(e);
                
                //Buffer handeling
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0);
                //uniform matrix
                GL.UniformMatrix4(uniform_mview, false, ref mviewdata[0]);
                //Enable Program
                GL.UseProgram(pgmID);
                //Clear buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            //OnRenderFrame is when the window contents are drawn, ie GL data is loaded and swapbuffers is called
            protected override void OnRenderFrame(FrameEventArgs e)
            {//OnRenderFrame is called every Refresh
                base.OnRenderFrame(e);

                //drawing with vertex buffers
                GL.Viewport(0, 0, Width, Height);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //GL.Enable(EnableCap.DepthTest);

                GL.EnableVertexAttribArray(attribute_vpos);
                GL.EnableVertexAttribArray(attribute_vcol);
 
                GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
 
                GL.DisableVertexAttribArray(attribute_vpos);
                GL.DisableVertexAttribArray(attribute_vcol);
 
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

                GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);

                GL.MatrixMode(MatrixMode.Projection);

                GL.LoadMatrix(ref projection);
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