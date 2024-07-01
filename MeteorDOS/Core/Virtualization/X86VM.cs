using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Virtualization
{
    public class X86VM
    {
        private Stack<object> stack = new Stack<object>();
        private uint PC = 0;
        private uint CPUMode = 16;
        private bool Running = false;
        private Dictionary<string, object> Regs8 = new Dictionary<string, object>()
        {
            {"al", 0},
            {"bl", 0},
            {"cl", 0},
            {"dl", 0},
            {"ah", 0},
            {"ch", 0},
            {"dh", 0},
            {"bh", 0},
        };
        private Dictionary<string, object> Regs16 = new Dictionary<string, object>()
        {
            {"ax", 0},
            {"bx", 0},
            {"cx", 0},
            {"dx", 0},
            {"si", 0},
            {"di", 0},
            {"bp", 0},
            {"sp", 0},
            {"ip", 0},
            {"flags", 0},
        };
        private Dictionary<string, object> Regs32 = new Dictionary<string, object>() 
        {
            {"eax", 0},
            {"ebx", 0},
            {"ecx", 0},
            {"edx", 0},
            {"esi", 0},
            {"edi", 0},
            {"ebp", 0},
            {"esp", 0},
            {"eip", 0},
            {"eflags", 0},
        };
        private Dictionary<string, object> Regs64 = new Dictionary<string, object>() 
        {
            {"rax", 0},
            {"rbx", 0},
            {"rcx", 0},
            {"rdx", 0},
            {"rsi", 0},
            {"rdi", 0},
            {"rbp", 0},
            {"rsp", 0},
            {"rip", 0},
            {"rflags", 0},
        };
        private Dictionary<string, object> SegmentRegs = new Dictionary<string, object>()
        {
            {"cs", 0},
            {"ss", 0},
            {"ds", 0},
            {"es", 0},
            {"fs", 0},
            {"gs", 0},
        };
        private Dictionary<string, object> SystemRegs = new Dictionary<string, object>()
        {
            {"cr0", 0},
            {"cr1", 0},
            {"cr2", 0},
            {"cr3", 0},
            {"cr4", 0},
            {"dr0", 0},
            {"dr1", 0},
            {"dr2", 0},
            {"dr3", 0},
            {"dr4", 0},
        };
        public void Push(object value)
        {
            if (!Running) return;
            stack.Push(value);
        }
        public void Pop(string reg) 
        {
            if (!Running) return;
            object value = stack.Pop();
            if (Regs8.ContainsKey(reg)) 
            {
                Regs8[reg] = value;
            }
            else if (Regs16.ContainsKey(reg))
            {
                Regs16[reg] = value;
            }
            else if (Regs32.ContainsKey(reg))
            {
                if (CPUMode < 32)
                {
                    Console.WriteLine($"{PC}: 32bit Register not available, This can be because register is not supported by current CPU mode, CPU Mode: {CPUMode}bit");
                    Running = false;
                    return;
                }
                Regs32[reg] = value;
            }
            else if (Regs64.ContainsKey(reg))
            {
                if (CPUMode < 64)
                {
                    Console.WriteLine($"{PC}: 64bit Register not available, This can be because register is not supported by current CPU mode, CPU Mode: {CPUMode}bit");
                    Running = false;
                    return;
                }
                Regs64[reg] = value;
            }
            else
            {
                Console.WriteLine($"{PC}: Invalid register");
            }
        }
        public void MoveR(string reg1, string reg2)
        {
            if (!Running) return;
            if (CPUMode == 16)
            {
                if (Regs8.ContainsKey(reg1))
                {
                    if (Regs8.ContainsKey(reg2))
                    {
                        Regs8[reg2] = reg1;
                    }
                    else if (Regs16.ContainsKey(reg2))
                    {
                        Regs16[reg2] = reg1;
                    }
                    else if (Regs32.ContainsKey(reg2))
                    {
                        Console.WriteLine($"{PC}: 32bit Register not available, This can be because register is not supported by current CPU mode, CPU Mode: {CPUMode}bit");
                        Running = false;
                    }
                    else if (Regs64.ContainsKey(reg2))
                    {
                        Console.WriteLine($"{PC}: 64bit Register not available, This can be because register is not supported by current CPU mode, CPU Mode: {CPUMode}bit");
                        Running = false;
                    }
                }
                else if (Regs16.ContainsKey(reg1))
                {
                    if (Regs8.ContainsKey(reg2))
                    {
                        Regs8[reg2] = reg1;
                    }
                    else if (Regs16.ContainsKey(reg2))
                    {
                        Regs16[reg2] = reg1;
                    }
                    else if (Regs32.ContainsKey(reg2))
                    {
                        Console.WriteLine($"{PC}: 32bit Register not available, This can be because register is not supported by current CPU mode, CPU Mode: {CPUMode}bit");
                        Running = false;
                    }
                    else if (Regs64.ContainsKey(reg2))
                    {
                        Console.WriteLine($"{PC}: 64bit Register not available, This can be because register is not supported by current CPU mode, CPU Mode: {CPUMode}bit");
                        Running = false;
                    }
                }
                else
                {
                    Console.WriteLine($"{PC}: Invalid register");
                    Running = false;
                }
                return;
            }
            else if (CPUMode == 32)
            {
                if (Regs8.ContainsKey(reg1))
                {
                    if (Regs8.ContainsKey(reg2))
                    {
                        Regs8[reg2] = reg1;
                    }
                    else if (Regs16.ContainsKey(reg2))
                    {
                        Regs16[reg2] = reg1;
                    }
                    else if (Regs32.ContainsKey(reg2))
                    {
                        Regs32[reg2] = reg1;
                    }
                }
                else if (Regs16.ContainsKey(reg1))
                {
                    if (Regs8.ContainsKey(reg2))
                    {
                        Regs8[reg2] = reg1;
                    }
                    else if (Regs16.ContainsKey(reg2))
                    {
                        Regs16[reg2] = reg1;
                    }
                    else if (Regs32.ContainsKey(reg2))
                    {
                        Regs32[reg2] = reg1;
                    }
                }
                else if (Regs32.ContainsKey(reg1))
                {
                    if (Regs8.ContainsKey(reg2))
                    {
                        Regs8[reg2] = reg1;
                    }
                    else if (Regs16.ContainsKey(reg2))
                    {
                        Regs16[reg2] = reg1;
                    }
                    else if (Regs32.ContainsKey(reg2))
                    {
                        Regs32[reg2] = reg1;
                    }
                }
                else
                {
                    Console.WriteLine($"{PC}: Invalid register");
                    Running = false;
                }
                return;
            }
            else if (CPUMode == 64)
            {
                if (Regs8.ContainsKey(reg1))
                {
                    if (Regs8.ContainsKey(reg2))
                    {
                        Regs8[reg2] = reg1;
                    }
                    else if (Regs16.ContainsKey(reg2))
                    {
                        Regs16[reg2] = reg1;
                    }
                    else if (Regs32.ContainsKey(reg2))
                    {
                        Regs32[reg2] = reg1;
                    }
                    else if (Regs64.ContainsKey(reg2))
                    {
                        Regs64[reg2] = reg1;
                    }
                }
                else if (Regs16.ContainsKey(reg1))
                {
                    if (Regs8.ContainsKey(reg2))
                    {
                        Regs8[reg2] = reg1;
                    }
                    else if (Regs16.ContainsKey(reg2))
                    {
                        Regs16[reg2] = reg1;
                    }
                    else if (Regs32.ContainsKey(reg2))
                    {
                        Regs32[reg2] = reg1;
                    }
                    else if (Regs64.ContainsKey(reg2))
                    {
                        Regs64[reg2] = reg1;
                    }
                }
                else if (Regs32.ContainsKey(reg1))
                {
                    if (Regs8.ContainsKey(reg2))
                    {
                        Regs8[reg2] = reg1;
                    }
                    else if (Regs16.ContainsKey(reg2))
                    {
                        Regs16[reg2] = reg1;
                    }
                    else if (Regs32.ContainsKey(reg2))
                    {
                        Regs32[reg2] = reg1;
                    }
                    else if (Regs64.ContainsKey(reg2))
                    {
                        Regs64[reg2] = reg1;
                    }
                }
                else if (Regs64.ContainsKey(reg1))
                {
                    if (Regs8.ContainsKey(reg2))
                    {
                        Regs8[reg2] = reg1;
                    }
                    else if (Regs16.ContainsKey(reg2))
                    {
                        Regs16[reg2] = reg1;
                    }
                    else if (Regs32.ContainsKey(reg2))
                    {
                        Regs32[reg2] = reg1;
                    }
                    else if (Regs64.ContainsKey(reg2))
                    {
                        Regs64[reg2] = reg1;
                    }
                }
                else
                {
                    Console.WriteLine($"{PC}: Invalid register");
                    Running = false;
                }
                return;
            }
        }
        public void Halt()
        {
            Running = false;
        }
        public void Advance()
        {
            if (!Running) return;
            PC++;
        }
    }
}
