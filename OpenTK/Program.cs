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

namespace OpenTK
{
    class Program
    {
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