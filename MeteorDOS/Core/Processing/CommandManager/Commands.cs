using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Processing.CommandManager
{
    public enum CommandExecutionStatus
    {
        Success,
        Failed,
        Aborted,
        None
    }
    public class Commands
    {
        private static List<ICommand> commands = new List<ICommand>();
        public static bool CanRun = true;
        public static bool IsRunning = false;
        public static void RegisterCommand(ICommand command)
        {
            if (CommandExists(command.Name)) 
            {
                throw new Exception($"Command already exists: {command.Name}");
            }
            commands.Add(command);
        }
        public static void UnRegisterCommand(string name)
        {
            ICommand command = GetCommand(name);
            if (command == null)
            {
                throw new Exception($"Command not found: {name}");
            }
            commands.Remove(command);
        }
        public static bool CommandExists(string name)
        {
            foreach (var cmd in commands)
            {
                if (cmd.Name == name)
                {
                    return true;
                }
            }
            return false;
        }
        public static ICommand GetCommand(string name)
        {
            foreach (var cmd in commands)
            {
                if (cmd.Name == name)
                {
                    return cmd;
                }
            }
            return null;
        }
        public static CommandExecutionStatus ExecuteCommand(string command)
        {
            IsRunning = true;
            if (string.IsNullOrWhiteSpace(command))
            {
                Console.WriteLine("Command cannot be null or empty.");
                return CommandExecutionStatus.None;
            }

            string[] args = GetCommandArgs(command);

            string commandName = args[0];
            ICommand cmd = GetCommand(commandName);
            if (cmd == null)
            {
                Console.WriteLine($"Command not found: {commandName}");
                return CommandExecutionStatus.None;
            }

            CommandExecutionStatus status = cmd.Execute(args);
            IsRunning = false;
            return status;
        }
        public static string[] GetCommandArgs(string command)
        {
            return command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static int GetArgsCount(string command)
        {
            return GetCommandArgs(command).Length;
        }
        public static int FirstIndexOfArg(string command, string argname)
        {
            string[] args = GetCommandArgs(command);
            for (int i = 0; i < args.Length; i++) 
            {
                if (args[i] == argname) return i;
            }
            return -1;
        }
        public static int[] IndexesOfArg(string command, string argname)
        {
            string[] args = GetCommandArgs(command);
            List<int> indexes = new List<int>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == argname) indexes.Add(i);
            }
            return indexes.ToArray();
        }
        public static int GetArgCount(string command, string argname)
        {
            uint count = 0;
            foreach (string arg in GetCommandArgs(command))
            {
                if (arg == argname) count++;
            }
            return (int)count;
        }
        public static string GetNextArg(string command, string argname)
        {
            int index = FirstIndexOfArg(command, argname);
            if (index < GetArgsCount(command) - 1)
            {
                return GetCommandArgs(command)[index + 1];
            }
            return string.Empty;
        }
    }
}
