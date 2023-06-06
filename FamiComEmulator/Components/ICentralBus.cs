namespace FamiComEmulator.Components
{
    public interface ICentralBus
    {
        ICpu6502 Cpu { get; set; }

        Ram Ram { get; set; }

        void Clock();

        byte Read(ushort address);

        void Reset();

        void Write(ushort address, byte data);
    }
}
