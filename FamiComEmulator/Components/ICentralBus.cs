namespace FamiComEmulator.Components
{
    public interface ICentralBus
    {
        ICpu6502 Cpu { get; }
        Ram Ram { get; }
        IPpu2c02 Ppu { get; }
        ICartridge Cartridge { get; }
        void AddCartridge(ICartridge cartridge);
        void Clock();
        byte Read(ushort address);
        void Reset();
        void Write(ushort address, byte data);
    }
}
