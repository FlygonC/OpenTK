using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTK
{
    public class AttributeInfo
    {
        public String name = "";
        public int address = -1;
        public int size = 0;
        public ActiveAttribType type;
    }

    public class UniformInfo
    {
        public String name = "";
        public int address = -1;
        public int size = 0;
        public ActiveUniformType type;
    }

    class ShaderProgram
    {
        //Variables
        public int ProgramID = -1;
        public int VShaderID = -1;
        public int FShaderID = -1;
        public int AttributeCount = 0;
        public int UniformCount = 0;

        public Dictionary<String, AttributeInfo> Attributes = new Dictionary<string, AttributeInfo>();
        public Dictionary<String, UniformInfo> Uniforms = new Dictionary<string, UniformInfo>();
        public Dictionary<String, uint> Buffers = new Dictionary<string, uint>();
        //Constructor
        public ShaderProgram()
        {
            //Creates the Program
            ProgramID = GL.CreateProgram();
        }
        //Loading Shaders...
        private void loadShader(String code, ShaderType type, out int address)
        {
            //Creates the shader
            address = GL.CreateShader(type);
            //Inputs Source
            GL.ShaderSource(address, code);
            //Compile
            GL.CompileShader(address);
            //Attach to Program
            GL.AttachShader(ProgramID, address);
            //Info log
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }
        //Public access for loading Shaders
        //Load source from string
        public void LoadShaderFromString(String code, ShaderType type)
        {
            if (type == ShaderType.VertexShader)
            {
                loadShader(code, type, out VShaderID);
            }
            else if (type == ShaderType.FragmentShader)
            {
                loadShader(code, type, out FShaderID);
            }
        }
        //Load source from file
        public void LoadShaderFromFile(String filename, ShaderType type)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                if (type == ShaderType.VertexShader)
                {
                    loadShader(sr.ReadToEnd(), type, out VShaderID);
                }
                else if (type == ShaderType.FragmentShader)
                {
                    loadShader(sr.ReadToEnd(), type, out FShaderID);
                }
            }
        }
        //Link Program
        public void Link()
        {
            //Compile Program + log info
            GL.LinkProgram(ProgramID);
            Console.WriteLine(GL.GetProgramInfoLog(ProgramID));
            //Get how many Attributes and Uniforms are in the Program(AttributCount and UniformCount)
            GL.GetProgram(ProgramID, ProgramParameter.ActiveAttributes, out AttributeCount);
            GL.GetProgram(ProgramID, ProgramParameter.ActiveUniforms, out UniformCount);

            //Analyze and add each attribute to the Attribute Dictionary(Attributes)
            for (int i = 0; i < AttributeCount; i++)
            {
                //Create "info" to buffer the Attribute information
                AttributeInfo info = new AttributeInfo();
                int length = 0;
                //Buffer for the Variable name of the Attribute
                StringBuilder name = new StringBuilder();
                //MAGIC!
                GL.GetActiveAttrib(ProgramID, i, 256, out length, out info.size, out info.type, name);
                //Move data from info into Dictionary
                info.name = name.ToString();
                info.address = GL.GetAttribLocation(ProgramID, info.name);
                Attributes.Add(name.ToString(), info);
            }
            //Repeat for Uniforms
            for (int i = 0; i < UniformCount; i++)
            {
                UniformInfo info = new UniformInfo();
                int length = 0;

                StringBuilder name = new StringBuilder();

                GL.GetActiveUniform(ProgramID, i, 256, out length, out info.size, out info.type, name);

                info.name = name.ToString();
                Uniforms.Add(name.ToString(), info);
                info.address = GL.GetUniformLocation(ProgramID, info.name);
            }
        }

        //Generate VertexBuffers and Uniform buffers
        public void GenBuffers()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                //Generates buffer
                uint buffer = 0;//Temporary storage for reference
                GL.GenBuffers(1, out buffer);
                //Add buffer to Dictionary
                Buffers.Add(Attributes.Values.ElementAt(i).name, buffer);
            }

            for (int i = 0; i < Uniforms.Count; i++)
            {
                uint buffer = 0;
                GL.GenBuffers(1, out buffer);

                Buffers.Add(Uniforms.Values.ElementAt(i).name, buffer);
            }
        }

        //Enable Attributes
        public void EnableVertexAttribArrays()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                GL.EnableVertexAttribArray(Attributes.Values.ElementAt(i).address);
            }
        }
        //Disable Attributes
        public void DisableVertexAttribArrays()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                GL.DisableVertexAttribArray(Attributes.Values.ElementAt(i).address);
            }
        }

        //Get Attributes by name
        public int GetAttribute(string name)
        {
            if (Attributes.ContainsKey(name))
            {
                return Attributes[name].address;
            }
            else
            {
                return -1;
            }
        }
        //Get Uniforms by name
        public int GetUniform(string name)
        {
            if (Uniforms.ContainsKey(name))
            {
                return Uniforms[name].address;
            }
            else
            {
                return -1;
            }
        }
        //Get Buffers by name
        public uint GetBuffer(string name)
        {
            if (Buffers.ContainsKey(name))
            {
                return Buffers[name];
            }
            else
            {
                return 0;
            }
        }

        //New Constructor to create full Shader Program + Buffers with one call
        public ShaderProgram(String vshader, String fshader, bool fromFile = false)
        {
            ProgramID = GL.CreateProgram();

            if (fromFile)
            {
                LoadShaderFromFile(vshader, ShaderType.VertexShader);
                LoadShaderFromFile(fshader, ShaderType.FragmentShader);
            }
            else
            {
                LoadShaderFromString(vshader, ShaderType.VertexShader);
                LoadShaderFromString(fshader, ShaderType.FragmentShader);
            }

            Link();
            GenBuffers();
        }
    }
}
