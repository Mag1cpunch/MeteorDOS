using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Processing.CommandManager
{
    public interface ICommand
    {
        public string Name { get; }
        public string Usage { get; }
        public string HelpMessage { get; }
        public void Execute(string[] args);
    }
}
