namespace FamiComEmulator.Components
{
    internal interface ICentralBus
    {
        Cpu6502 Cpu { get; set; }

        Ram Ram { get; set; }

        void Write(short address, byte data);

        byte Read(short address);
    }
}