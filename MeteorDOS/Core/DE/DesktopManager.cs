using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core.Memory;
using Cosmos.System;
using GrapeGL.Graphics;
using GrapeGL.Hardware.GPU;
using GrapeGL.Hardware.GPU.VMWare;
using MeteorDOS.Core.Processing;

namespace MeteorDOS.Core.DE
{
    public static class DesktopManager
    {
        public static SVGAIICanvas screen;
        public static Canvas cursoridle;
        public static void Init(ushort width, ushort height)
        {
            screen = new SVGAIICanvas(width, height);
            MouseManager.ScreenWidth = width;
            MouseManager.ScreenHeight = height;
            cursoridle = Image.FromBitmap(Resources.CursorIdleBytes);
        }
        public static void Run()
        {
            while (true)
            {
                TweenLogin();
                screen.Clear(Color.DeepGray);
                screen.DrawFilledRectangle(100, 100, (ushort)(screen.Width - 200), (ushort)(screen.Height - 200), 10, Color.White);
                screen.DrawRectangle(100, 100, (ushort)(screen.Width - 200), (ushort)(screen.Height - 200), 10, Color.LightGray);
                screen.DrawImage((int)MouseManager.X, (int)MouseManager.Y, cursoridle);
                screen.Update();
                Heap.Collect();
            }
        }
        public static void TweenLogin()
        {
            for (int i = 0; i < 100; i++)
            {
                screen.Clear(Color.DeepGray);
                screen.DrawFilledRectangle(i, 100, (ushort)(screen.Width - 200), (ushort)(screen.Height - 200), 10, Color.White);
                screen.DrawRectangle(i, 100, (ushort)(screen.Width - 200), (ushort)(screen.Height - 200), 10, Color.LightGray);
                screen.Update();
                Heap.Collect();
            }
        }
    }
}
