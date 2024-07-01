using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Processing
{
    public class Process
    {
        public string Name { get; }
        public uint PID { get; }
        public Action Run { get; }
        public Process(string name, uint pid, Action runfunc) 
        {
            Name = name;
            PID = pid;
            Run = runfunc;
        }
    }
    public class ProcessManager
    {
        // Process Handling and execution implementation
        public static List<Process> Processes = new List<Process>();
        public static uint CurrentID = 0;
        // Implement AddProcess with unique pid generation
        public static void AddProcess(string name, Action RunFunction)
        {
            Process process = new Process(name, CurrentID, RunFunction);
            CurrentID++;
        }
        public static void Yield()
        {
            for (int i = Processes.Count - 1; i >= 0; i--)
            {
                Processes[i].Run();
                CurrentID--;
            }
        }
    }
}
