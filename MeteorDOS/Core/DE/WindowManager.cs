using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.DE
{
    public class Form
    {
        public string Name;
        public uint X;
        public uint Y;
        public uint Width;
        public uint Height;
        public Bitmap Container;
        public Form(string name, uint x, uint y, uint width, uint height)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Container = new Bitmap(width, height, ColorDepth.ColorDepth32);
        }
        public void DrawPixel(int x, int y, Color color)
        {
            (byte a, byte r, byte g, byte b) clr = ExtractARGB(color.ToArgb());
            for (int i = 0; i < Container.Width; i++) 
            {
                if (i == x) 
                {
                    for (int j = 0; j < Container.Height; j++) 
                    {
                        if (j == y)
                        {
                            Container.RawData[(y * Container.Width + x) * 32] = clr.b;
                            Container.RawData[(y * Container.Width + x) * 32 + 1] = clr.g;
                            Container.RawData[(y * Container.Width + x) * 32 + 2] = clr.r;
                            Container.RawData[(y * Container.Width + x) * 32 + 3] = clr.a;
                        }
                    }
                }
            }
        }
        public void DrawPixel(int x, int y, byte a, byte r, byte g, byte b)
        {
            for (int i = 0; i < Container.Width; i++)
            {
                if (i == x)
                {
                    for (int j = 0; j < Container.Height; j++)
                    {
                        if (j == y)
                        {
                            Container.RawData[(y * Container.Width + x) * 32] = b;
                            Container.RawData[(y * Container.Width + x) * 32 + 1] = g;
                            Container.RawData[(y * Container.Width + x) * 32 + 2] = r;
                            Container.RawData[(y * Container.Width + x) * 32 + 3] = a;
                        }
                    }
                }
            }
        }
        public void DrawRectangle(int x, int y, int width, int height, Color color)
        {
            for (int i = 0; i < Container.Width; x++)
            {
                if (i == x)
            }
        }
        (byte a, byte r, byte g, byte b) ExtractARGB(int packedColor)
        {
            byte a = (byte)((packedColor >> 24) & 0xFF);
            byte r = (byte)((packedColor >> 16) & 0xFF);
            byte g = (byte)((packedColor >> 8) & 0xFF);
            byte b = (byte)(packedColor & 0xFF);
            return (a, r, g, b);
        }
    }
    public class WindowManager
    {
    }
}
