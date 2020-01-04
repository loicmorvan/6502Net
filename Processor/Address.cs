namespace Processor
{
    public readonly struct Address
    {
        private readonly byte _lowBits;
        private readonly byte _highBits;

        public Address(byte lowBits, byte highBits)
        {
            _lowBits = lowBits;
            _highBits = highBits;
        }

        public static implicit operator ushort(in Address toCast)
        {
            return (ushort)(toCast._lowBits + (toCast._highBits << 8));
        }

        public static implicit operator Address(ushort value)
        {
            return new Address((byte)(value & 0xFF), (byte)((value & 0xFF00) >> 8));
        }

        public static implicit operator Address(int value)
        {
            return (ushort)(value & 0xFFFF);
        }

        public byte GetLowBits()
        {
            return _lowBits;
        }

        public byte GetHighBits()
        {
            return _highBits;
        }
    }
}