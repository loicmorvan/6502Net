using System;

namespace Processor
{
    public class Memory : IMemory
    {
        private readonly byte[] _memory;
        private const int _maxCapacity = ushort.MaxValue + 1;

        public Memory()
        {
            _memory = new byte[_maxCapacity];
        }

        public byte this[in Address address]
        {
            get => _memory[address];
            set => _memory[address] = value;
        }

        public void Clear()
        {
            for (var i = 0; i < _maxCapacity; i++)
            {
                _memory[i] = 0x00;
            }
        }

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
                memory[(ushort)(i + offset)] = program[i];
            }

            //Write the initialProgram Counter to the reset vector
            memory[0xFFFC] = initialProgramCounter.GetLowBits();
            memory[0xFFFD] = initialProgramCounter.GetHighBits();

            return memory;
        }
    }
}
