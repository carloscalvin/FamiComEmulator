namespace FamiComEmulator.Components
{
    internal interface ICentralBus
    {
        Cpu6502 Cpu { get; set; }

        Ram Ram { get; set; }

        void Write(ushort address, byte data);

        byte Read(ushort address);
    }
}