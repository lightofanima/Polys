﻿using System;

namespace Polys.Video
{
    /** Represents a colour palette in CPU+GPU memory. It is a 1-D texture with 255 components. */
    public class Palette
    {
        //Array of pixels in rgba format
        byte[] colours = new byte[256 * 4];

        //The GL texture handle
        public uint colourTexture = ~0u;

        /** Sets the transparent colour of the palette. By default this is the first colour. */
        public void setTransparentColour(int index)
        {
            //Reset all to 255
            for (int i = 0; i < 256; ++i)
                colours[(i << 2) + 3] = 255;

            //Set colour at index to 0
            colours[(index << 2) + 3] = 0;

            upload();
        }

        /** Trivial constructor */
        public Palette() { }

        /** Constructs the palette from a .pal file */
        public Palette(String path)
        {
            try
            {
                if (System.IO.Path.GetExtension(path) != ".pal")
                    throw new Exception("palette must be a .pal file.");
                String[] lines = System.IO.File.ReadAllLines(path);
                if (lines.Length < 3)
                    throw new Exception("invalid .pal file: Less that 3 lines in length.");

                int numberOfColours = int.Parse(lines[2]);

                if (lines.Length - 3 < numberOfColours)
                    throw new Exception("invalid number of colour entries.");

                for (int i = 0; i < numberOfColours; ++i)
                {
                    Colour colour = new Colour(lines[i + 3]);
                    int index = i << 2;
                    colours[index] = colour.r;
                    colours[index+1] = colour.g;
                    colours[index+2] = colour.b;
                }

                setTransparentColour(0);

                upload();
            }
            catch(Exception e)
            {
                throw new Exception(String.Format("Error loading palette \"{0}\": {1}", path, e.Message));
            }
        }

        /** Construct the palette from an Imaging.ColorPalette object */
        public Palette(System.Drawing.Imaging.ColorPalette p)
        {
            for (int i = 0; i < p.Entries.Length; ++i)
            {
                Colour colour = new Colour(p.Entries[i]);
                int index = i * 4;
                colours[index] = colour.r;
                colours[index + 1] = colour.g;
                colours[index + 2] = colour.b;
            }
            setTransparentColour(0);
            upload();
        }

        /** Returns the colour at a specific index */
        public Colour this[int key]
        {
            get
            {
                int index = key << 2;
                return new Colour(colours[index], colours[index+1], colours[index+2], colours[index+3]);
            }
            set
            {
                int index = key << 2;
                colours[index] = value.r;
                colours[index + 1] = value.g;
                colours[index + 2] = value.b;
                colours[index + 3] = value.a;
            }
        }

        /** Uploads the palette into GPU memory */
        void upload()
        {
            if(colourTexture==~0u)
                colourTexture = OpenGL.Gl.GenTexture();

            bind();
            OpenGL.Gl.TexImage1D(OpenGL.TextureTarget.Texture1D, 0, OpenGL.PixelInternalFormat.Rgba8, 256, 0, OpenGL.PixelFormat.Rgba, 
                OpenGL.PixelType.UnsignedByte, System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(colours, 0));
            OpenGL.Gl.TexParameteri(OpenGL.TextureTarget.Texture1D, OpenGL.TextureParameterName.TextureMagFilter, OpenGL.TextureParameter.Nearest);
            OpenGL.Gl.TexParameteri(OpenGL.TextureTarget.Texture1D, OpenGL.TextureParameterName.TextureMinFilter, OpenGL.TextureParameter.Nearest);
        }

        /** Binds the palette as the current 1D texture */
        public void bind()
        {
            OpenGL.Gl.BindTexture(OpenGL.TextureTarget.Texture1D, colourTexture);
        }
    }
}
