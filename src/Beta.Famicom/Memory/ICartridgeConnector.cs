namespace Beta.Famicom.Memory
{
    public interface ICartridgeConnector
    {
        bool Ciram(ushort address);

        int CiramA10(ushort address);

        void R2A03Read(ushort address, ref byte data);

        void R2A03Write(ushort address, ref byte data);

        void R2C02Read(ushort address, ref byte data);

        void R2C02Write(ushort address, ref byte data);
    }
}
