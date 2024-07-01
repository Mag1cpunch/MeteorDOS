using MeteorDOS.Core.Packages;
using MeteorDOS.Core.Processing;
using MeteorDOS.Core.Processing.CommandManager;
using MeteorDOS.Core.Processing.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core
{
    public unsafe class DOSShell
    {
        public static void InitShell()
        {
            CDCommand cd = new CDCommand();
            RebootCommand rebootCommand = new RebootCommand();
            ShutdownCommand shutdownCommand = new ShutdownCommand();
            ClearCommand clearCommand = new ClearCommand();
            LogoutCommand logoutCommand = new LogoutCommand();
            MakeUserCommand makeUserCommand = new MakeUserCommand();
            RemoveUserCommand removeUserCommand = new RemoveUserCommand();
            Commands.RegisterCommand(cd);
            Commands.RegisterCommand(rebootCommand);
            Commands.RegisterCommand(shutdownCommand);
            Commands.RegisterCommand(clearCommand);
            Commands.RegisterCommand(makeUserCommand);
            Commands.RegisterCommand(logoutCommand);
            Commands.RegisterCommand(removeUserCommand);
        }
        public static void Run()
        {
            //Thread* thread1 = Threading.CreateThread(Write1);
            //Thread* thread2 = Threading.CreateThread(Write2);
            //Threading.StartThread(thread1);
            //Threading.StartThread(thread2);
            //while (true) { }
            while (true)
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
                Commands.ExecuteCommand(input);
            }
        }
    }
}
