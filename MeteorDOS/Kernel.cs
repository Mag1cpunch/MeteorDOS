using Cosmos.System.FileSystem.VFS;
using Cosmos.System.FileSystem;
using MeteorDOS.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using MeteorDOS.Core.Processing;
using MeteorDOS.Core.Processing.Threading;

namespace MeteorDOS
{
    public class Kernel : Sys.Kernel
    {

        //MOFS fs;
        public bool FirstTime = false;
        CosmosVFS fs;
        protected override void BeforeRun()
        {
            fs = new CosmosVFS();
            VFSManager.RegisterVFS(fs);
        }

        protected override void Run()
        {
            if (FirstTime == false)
            {
                Environment.CurrentDirectory = @"0:\";
                Console.WriteLine("Verifing Filesystem...");
                if (!VFSManager.DirectoryExists(@"0:\Core"))
                {
                    VFSManager.CreateDirectory(@"0:\Core");
                }
                if (!VFSManager.DirectoryExists(@"0:\AppFilesX86"))
                {
                    VFSManager.CreateDirectory(@"0:\AppFilesX86");
                }
                if (!VFSManager.DirectoryExists(@"0:\AppFilesX64"))
                {
                    VFSManager.CreateDirectory(@"0:\AppFilesX64");
                }
                if (!VFSManager.DirectoryExists(@"0:\Users"))
                {
                    VFSManager.CreateDirectory(@"0:\Users");
                }
                if (!VFSManager.DirectoryExists(@"0:\Users\root"))
                {
                    VFSManager.CreateDirectory(@"0:\Users\root");
                }
                if (!VFSManager.DirectoryExists(@"0:\Users\root\AppData"))
                {
                    VFSManager.CreateDirectory(@"0:\Users\root\AppData");
                }
                if (!VFSManager.DirectoryExists(@"0:\Users\root\Downloads"))
                {
                    VFSManager.CreateDirectory(@"0:\Users\root\Downloads");
                }
                if (!VFSManager.DirectoryExists(@"0:\Users\root\Pictures"))
                {
                    VFSManager.CreateDirectory(@"0:\Users\root\Pictures");
                }
                if (!VFSManager.DirectoryExists(@"0:\Users\root\Videos"))
                {
                    VFSManager.CreateDirectory(@"0:\Users\root\Videos");
                }
                if (!VFSManager.DirectoryExists(@"0:\Users\root\Documents"))
                {
                    VFSManager.CreateDirectory(@"0:\Users\root\Documents");
                }
                Console.WriteLine("Done!");
                //RepairDisk();
                DOSShell.InitShell();
                FirstTime = true;
            }
            if (!UserManager.UserExists("root"))
            {
                UserManager.CreateUser("root", "root");
            }
            //foreach (BlockDevice device in BlockDevice.Devices)
            //{
            //    Disk disk = new Disk(device);
            //    if (disk.Type == BlockDeviceType.HardDrive)
            //    {
            //        byte[] bsdata = new byte[508];
            //        MOFS.FormatDisk(device, bsdata, 8);
            //        fs = new MOFS(device);
            //        break;
            //    }
            //}
            //byte[] data = new byte[512];
            //byte[] text = Encoding.UTF8.GetBytes("Hello, World!");
            //for (int i = 0; i < text.Length; i++) 
            //{
            //    data[i] = text[i];
            //}
            //fs.WriteSector(1, data);
            //byte[] text2 = fs.ReadSector(1);
            //Console.WriteLine(Encoding.UTF8.GetString(text2));
            //while (true) 
            //{
            //    Console.WriteLine();
            //    Console.Write("[ Press any key to shutdown ]");
            //    Console.ReadLine();
            //    Sys.Power.Shutdown();
            //}
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[[ ------------------------------------------ ]]");
            Console.Write("[[ "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write("MeteorDOS, Version: 1.0, Status: Pre-Alpha"); Console.ForegroundColor = ConsoleColor.Green; Console.Write(" ]]");
            Console.WriteLine();
            Console.WriteLine("[[ ------------------------------------------ ]]");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            UserManager.LoginPrompt();
            DOSShell.Run();
        }
        public void RepairDisk()
        {
            Disk disk = VFSManager.GetDisks()[0];
            disk.Clear();
            disk.CreatePartition((int)(disk.Size / 1024 / 1024));
            disk.FormatPartition(0, "FAT32");
        }
    }
}
