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
    }
}
