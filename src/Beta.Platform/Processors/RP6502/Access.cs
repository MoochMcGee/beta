namespace Beta.Platform.Processors.RP6502
{
    public delegate void Access<TAddress, TData>(TAddress address, ref TData data)
        where TAddress : struct
        where TData : struct;
}
