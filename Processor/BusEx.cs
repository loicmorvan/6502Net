namespace Processor
{
    public static class BusEx
    {
        public static void Signal(this Bus<bool> bus)
        {
            bus.Value = !bus.Value;
            bus.Value = !bus.Value;
        }
    }
}