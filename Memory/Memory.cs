using BreadBoard;
using System;
using System.Reactive.Linq;

namespace Memory
{
    public class Memory : IMemory
    {
        private readonly byte[] _memory;
        private const int _maxCapacity = ushort.MaxValue + 1;

        public Memory()
        {
            _memory = new byte[_maxCapacity];

            ReadyBus.ValueChanged.Where(v => !v).Subscribe(_ => Cycle());
        }

        public Bus<byte> DataBus { get; set; } = new Bus<byte>();
        public Bus<Address> AddressBus { get; set; } = new Bus<Address>();
        public Bus<bool> RwBus { get; set; } = new Bus<bool>();
        public Bus<bool> ReadyBus { get; set; } = new Bus<bool>();

        /// <summary>
        /// Loads a program into the processors memory
        /// </summary>
        /// <param name="offset">The offset in memory when loading the program.</param>
        /// <param name="program">The program to be loaded</param>
        /// <param name="initialProgramCounter">The initial PC value, this is the entry point of the program</param>
        public static IMemory LoadProgram(Address offset, byte[] program, Address initialProgramCounter)
        {
            if (offset + program.Length > _maxCapacity)
            {
                throw new ArgumentOutOfRangeException();
            }

            var memory = new Memory();

            for (var i = 0; i < program.Length; i++)
            {
                memory._memory[(ushort)(i + offset)] = program[i];
            }

            //Write the initialProgram Counter to the reset vector
            memory._memory[0xFFFC] = initialProgramCounter.GetLowBits();
            memory._memory[0xFFFD] = initialProgramCounter.GetHighBits();

            return memory;
        }

        private void Cycle()
        {
            if (RwBus.Value)
            {
                DataBus.Value = _memory[AddressBus.Value];
            }
            else
            {
                _memory[AddressBus.Value] = DataBus.Value;
            }
        }
    }
}
