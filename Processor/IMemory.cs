namespace Processor
{
    public interface IMemory
    {
        byte this[in Address address] { get; set; }
    }
}