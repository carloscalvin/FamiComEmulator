using static FamiComEmulator.Components.Ppu2c02;

namespace FamiComEmulator.Components
{
    public interface ICartridge
    {
        bool Read(ushort address, ref byte data);

        bool Write(ushort address, byte data);

        Mirror Mirror { get; }
    }
}
