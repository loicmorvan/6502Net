using System;

namespace Processor
{
    public class Memory
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

        public int Capacity => 0x10000;

        /// <summary>
        /// Loads a program into the processors memory
        /// </summary>
        /// <param name="offset">The offset in memory when loading the program.</param>
        /// <param name="program">The program to be loaded</param>
        /// <param name="initialProgramCounter">The initial PC value, this is the entry point of the program</param>
        public void LoadProgram(int offset, byte[] program, int initialProgramCounter)
        {
            LoadProgram(offset, program);

            var bytes = BitConverter.GetBytes(initialProgramCounter);

            //Write the initialProgram Counter to the reset vector
            _memory[0xFFFC] = bytes[0];
            _memory[0xFFFD] = bytes[1];
        }

        /// <summary>
        /// Loads a program into the processors memory
        /// </summary>
        /// <param name="offset">The offset in memory when loading the program.</param>
        /// <param name="program">The program to be loaded</param>
        public void LoadProgram(int offset, byte[] program)
        {
            if (offset > Capacity)
            {
                throw new InvalidOperationException("Offset '{0}' is larger than memory size '{1}'");
            }

            if (program.Length > Capacity + offset)
            {
                throw new InvalidOperationException(string.Format("Program Size '{0}' Cannot be Larger than Memory Size '{1}' plus offset '{2}'", program.Length, Capacity, offset));
            }

            for (var i = 0; i < program.Length; i++)
            {
                _memory[i + offset] = program[i];
            }
        }
    }
}
