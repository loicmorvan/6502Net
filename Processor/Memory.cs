using System;

namespace Processor
{
    public class Memory : IMemory
    {
        private readonly byte[] _memory;

        public Memory()
        {
            _memory = new byte[Capacity];
        }

        public byte this[int address]
        {
            get => _memory[address];
            set => _memory[address] = value;
        }

        public const int Capacity = 0x10000;

        public void Clear()
        {
            for (var i = 0; i < Capacity; i++)
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
        public static IMemory LoadProgram(int offset, byte[] program, int initialProgramCounter)
        {
            if (offset > Capacity)
            {
                throw new InvalidOperationException("Offset '{0}' is larger than memory size '{1}'");
            }

            if (program.Length > Capacity + offset)
            {
                throw new InvalidOperationException(string.Format("Program Size '{0}' Cannot be Larger than Memory Size '{1}' plus offset '{2}'", program.Length, Capacity, offset));
            }

            var memory = new Memory();

            for (var i = 0; i < program.Length; i++)
            {
                memory[i + offset] = program[i];
            }

            var bytes = BitConverter.GetBytes(initialProgramCounter);

            //Write the initialProgram Counter to the reset vector
            memory[0xFFFC] = bytes[0];
            memory[0xFFFD] = bytes[1];

            return memory;
        }
    }
}
