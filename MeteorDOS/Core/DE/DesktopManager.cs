using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.Core.Memory;
using MeteorDOS.Core.Processing;

namespace MeteorDOS.Core.DE
{
    public static class DesktopManager
    {
        public static SVGAIICanvas screen;
        public static Bitmap cursoridle;
        public static void Init(ushort width, ushort height)
        {
            screen = new SVGAIICanvas(new Mode(width, height, ColorDepth.ColorDepth32));
            MouseManager.ScreenWidth = width;
            MouseManager.ScreenHeight = height;
            cursoridle = new Bitmap(Resources.CursorIdleBytes);
        }
        public static void Run()
        {
            while (true)
            {
                screen.Clear(Color.DarkGray);
                screen.DrawFilledRectangle(Color.White, 100, 100, (ushort)(screen.Mode.Width - 200), (ushort)(screen.Mode.Height - 200));
                screen.DrawRectangle(Color.LightGray, 100, 100, (ushort)(screen.Mode.Width - 200), (ushort)(screen.Mode.Height - 200));
                screen.DrawImageAlpha(cursoridle, (int)MouseManager.X, (int)MouseManager.Y);
                screen.Display();
                Heap.Collect();
            }
        }
    }
}
