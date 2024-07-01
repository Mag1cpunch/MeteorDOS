using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using System;
using System.Collections.Generic;
using static Cosmos.Core.INTs;

namespace MeteorDOS.Core.Processing.Threading
{
    public unsafe struct Thread
    {
        public int id;
        public uint* stack_ptr;
        public Threading.thread_entry_point entry_point;
        public uint* stack_base;
    }

    public unsafe class Threading
    {
        private const int MaxThreads = 10; // Adjust as needed
        private static Thread[] threads = new Thread[MaxThreads];
        private static int threadCount = 0;

        private static uint StackSizePerThread = 1024 * 16; // 16 KB per thread stack

        public delegate void* thread_entry_point();

        //public static void Initialize()
        //{
        //    // Initialize Local APIC
        //    APIC.Initialize();

        //    // Register the interrupt handler for the timer
        //    INTs.SetIrqHandler(32, TimerInterruptHandler);

        //    // Enable interrupts
        //    CPU.EnableInterrupts();
        //}

        public static int CreateThread(thread_entry_point EntryPoint)
        {
            if (threadCount >= MaxThreads)
            {
                throw new Exception("Exceeded maximum thread count.");
            }

            uint* stack = (uint*)Heap.Alloc(StackSizePerThread);
            if (stack == null) throw new Exception("Failed to allocate stack for thread.");

            Thread thread = new Thread
            {
                id = Random.Shared.Next(),
                stack_ptr = stack + (StackSizePerThread / 4) - 1,
                entry_point = EntryPoint,
                stack_base = stack
            };

            threads[threadCount] = thread;
            int threadIndex = threadCount;
            threadCount++;

            return threadIndex;
        }

        //private static int GenerateUniqueId()
        //{
        //    int id = (int)(RTC.Second * 1000 + RTC.Millisecond);
        //    while (threads.Exists(t => t.id == id))
        //    {
        //        id = (int)(RTC.Second * 1000 + RTC.Millisecond) + new Random().Next(1000);
        //    }
        //    return id;
        //}

        //private static void TimerInterruptHandler(ref IRQContext aContext)
        //{
        //    // Disable interrupts
        //    CPU.DisableInterrupts();

        //    // Save the current thread context
        //    // Note: Actual implementation will require saving CPU registers

        //    // Switch to the next thread
        //    currentThread = (currentThread + 1) % threads.Count;

        //    // Load the next thread context
        //    // Note: Actual implementation will require restoring CPU registers

        //    // Acknowledge the interrupt
        //    APIC.EOI();

        //    // Enable interrupts
        //    CPU.EnableInterrupts();
        //}

        public static void StartThreading()
        {
            // Implement thread execution logic
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].entry_point();
            }
        }

        public static void DisposeThread(int threadIndex)
        {
            // Implement thread disposal logic
            if (threadIndex >= 0 && threadIndex < threadCount)
            {
                Thread thread = threads[threadIndex];
                if (thread.stack_base != null)
                {
                    Heap.Free(thread.stack_base);
                    // Reset thread structure if needed
                    thread.id = 0;
                    thread.stack_ptr = null;
                    thread.entry_point = null;
                    thread.stack_base = null;
                }
            }
        }
    }
}
