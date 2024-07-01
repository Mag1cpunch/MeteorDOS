using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorDOS.Core.Processing.Threading
{
    public static unsafe class APIC
    {
        private const uint LAPIC_BASE_ADDRESS = 0xFEE00000;

        // Local APIC Registers (offsets)
        private const uint LAPIC_EOI = 0x00B0;
        private const uint LAPIC_SVR = 0x00F0;
        private const uint LAPIC_TPR = 0x0080;
        private const uint LAPIC_TIMER = 0x0320;
        private const uint LAPIC_TIMER_INITIAL_COUNT = 0x0380;
        private const uint LAPIC_TIMER_CURRENT_COUNT = 0x0390;
        private const uint LAPIC_TIMER_DIVIDE_CONFIG = 0x03E0;

        // Access LAPIC registers using memory-mapped I/O
        private static uint* lapicBase = (uint*)LAPIC_BASE_ADDRESS;

        public static void Initialize()
        {
            // Enable Local APIC
            WriteRegister(LAPIC_SVR, 0x100 | 32); // Set Spurious Interrupt Vector to 32 and enable APIC

            // Set Local APIC Timer to periodic mode with vector 32 (IRQ 32)
            WriteRegister(LAPIC_TIMER, 0x20000 | 32);

            // Set Timer Initial Count
            WriteRegister(LAPIC_TIMER_INITIAL_COUNT, 1000000);

            // Set Timer Divide Configuration (divide by 16)
            WriteRegister(LAPIC_TIMER_DIVIDE_CONFIG, 0x3);
        }

        public static void WriteRegister(uint offset, uint value)
        {
            *(lapicBase + offset / 4) = value;
        }

        public static uint ReadRegister(uint offset)
        {
            return *(lapicBase + offset / 4);
        }

        public static void EOI()
        {
            WriteRegister(LAPIC_EOI, 0);
        }
    }
}
