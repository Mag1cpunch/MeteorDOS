using MeteorDOS.Core.Packages;
using MeteorDOS.Core.Processing;
using MeteorDOS.Core.Processing.CommandManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeteorDOS.Core
{
    public unsafe class DOSShell
    {
        public static CommandExecutionStatus CMDStatus = CommandExecutionStatus.None;
        public static void InitShell()
        {
            CDCommand cd = new CDCommand();
            RebootCommand rebootCommand = new RebootCommand();
            ShutdownCommand shutdownCommand = new ShutdownCommand();
            ClearCommand clearCommand = new ClearCommand();
            LogoutCommand logoutCommand = new LogoutCommand();
            MakeUserCommand makeUserCommand = new MakeUserCommand();
            RemoveUserCommand removeUserCommand = new RemoveUserCommand();
            LSCommand LSCommand = new LSCommand();
            EchoCommand echoCommand = new EchoCommand();
            Commands.RegisterCommand(cd);
            Commands.RegisterCommand(rebootCommand);
            Commands.RegisterCommand(shutdownCommand);
            Commands.RegisterCommand(clearCommand);
            Commands.RegisterCommand(makeUserCommand);
            Commands.RegisterCommand(logoutCommand);
            Commands.RegisterCommand(removeUserCommand);
            Commands.RegisterCommand(LSCommand);
            Commands.RegisterCommand(echoCommand);
        }
        public static void KeyHandler()
        {
            while (true)
            {
                if (Cosmos.System.KeyboardManager.TryReadKey(out var key))
                {
                    if (Cosmos.System.KeyboardManager.ControlPressed)
                    {
                        if (key.Key == Cosmos.System.ConsoleKeyEx.C)
                        {
                            if (!Commands.IsRunning) continue;
                            Commands.CanRun = false;
                            while (CMDStatus != CommandExecutionStatus.Aborted) { };
                            CMDStatus = CommandExecutionStatus.None;
                            Commands.CanRun = true;
                        }
                    }
                }
            }
        }
        public static void Run()
        {
            //Thread* thread1 = Threading.CreateThread(Write1);
            //Thread* thread2 = Threading.CreateThread(Write2);
            //Threading.StartThread(thread1);
            //Threading.StartThread(thread2);
            //while (true) { }
            //Cosmos.System.Thread keyhandler = new Cosmos.System.Thread(KeyHandler);
            //keyhandler.Start();
            while (true)
            {
                try
                {
                    if (Environment.CurrentDirectory == @"0:\")
                    {
                        Console.Write(Environment.CurrentDirectory + $"{UserManager.CurrentUser}>> ");
                    }
                    else
                    {
                        Console.Write(Environment.CurrentDirectory + @"\" + $"{UserManager.CurrentUser}>> ");
                    }
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }
                    CMDStatus = Commands.ExecuteCommand(input);
                }
                catch (Exception ex) 
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An error has occurred: {ex.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }
    }
}
