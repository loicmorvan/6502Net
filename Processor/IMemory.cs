namespace Processor
{
    public interface IMemory
    {
        Bus<byte> DataBus { get; set; }
        Bus<Address> AddressBus { get; set; }
        Bus<bool> RwBus { get; set; }

        void Cycle();
    }
}