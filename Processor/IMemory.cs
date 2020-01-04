namespace Processor
{
    public interface IMemory
    {
        byte this[int address] { get; set; }
    }
}