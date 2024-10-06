namespace FamiComEmulator.Components
{
    /// <summary>
    /// Represents the central bus in the NES emulator, handling communication between CPU, PPU, RAM, and Cartridge.
    /// </summary>
    public class CentralBus : ICentralBus
    {
        private byte[] _controllerState = new byte[2];

        /// <summary>
        /// Initializes a new instance of the <see cref="CentralBus"/> class with the specified CPU and PPU.
        /// </summary>
        /// <param name="cpu">The CPU component to connect to the bus.</param>
        /// <param name="ppu">The PPU component to connect to the bus.</param>
        public CentralBus(ICpu6502 cpu, IPpu2c02 ppu)
        {
            Cpu = cpu;
            Ppu = ppu;
            Ram = new Ram();
            Cpu.AddCentralBus(this);
            Ppu.AddCentralBus(this);
            Controller = new byte[2];
        }

        /// <summary>
        /// Gets the CPU connected to the central bus.
        /// </summary>
        public ICpu6502 Cpu { get; }

        /// <summary>
        /// Gets the PPU connected to the central bus.
        /// </summary>
        public IPpu2c02 Ppu { get; }

        /// <summary>
        /// Gets the Cartridge connected to the central bus.
        /// </summary>
        public ICartridge Cartridge { get; private set; }

        /// <summary>
        /// Gets the system RAM connected to the central bus.
        /// </summary>
        public Ram Ram { get; }

        public byte[] Controller { get; internal set; }

        /// <summary>
        /// Adds a cartridge to the central bus.
        /// </summary>
        /// <param name="cartridge">The cartridge to add.</param>
        public void AddCartridge(ICartridge cartridge)
        {
            Cartridge = cartridge;
        }

        /// <summary>
        /// Advances the emulator by one clock cycle.
        /// </summary>
        public void Clock()
        {
            Cpu?.Clock();
            for (int i = 0; i < 3; i++)
            {
                Ppu?.Clock();
            }
        }

        /// <summary>
        /// Reads a byte from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from (0x0000 - 0xFFFF).</param>
        /// <returns>The byte value stored at the specified address.</returns>
        public byte Read(ushort address)
        {
            byte data = 0x00;

            if (address >= 0x0000 && address <= 0x1FFF)
            {
                // Handle internal RAM (0x0000 - 0x1FFF), mirrored every 2KB
                data = Ram.Read(address);
                return data;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                // Handle PPU registers (0x2000 - 0x3FFF), mirrored every 8 bytes
                data = Ppu.ReadRegister((byte)(address & 0x0007));
                return data;
            }
            else if (address == 0x4014)
            {
                // OAM DMA access (typically write-only, reading returns 0)
                data = 0x00;
                return data;
            }
            else if (address >= 0x4016 && address <= 0x4017)
            {
                data = (byte)((_controllerState[address & 0x0001] & 0x80) > 0 ? 0x01 : 0x00);
                _controllerState[address & 0x0001] <<= 1;

                return data;
            }
            else if (address >= 0x4020 && address <= 0xFFFF)
            {
                // Handle cartridge space (0x4020 - 0xFFFF)
                if (Cartridge != null && Cartridge.Read(address, ref data))
                {
                    return data;
                }
            }

            // Default return value if address is not handled
            return data;
        }

        /// <summary>
        /// Writes a byte to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to (0x0000 - 0xFFFF).</param>
        /// <param name="data">The byte value to write to memory.</param>
        public void Write(ushort address, byte data)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                // Handle internal RAM (0x0000 - 0x1FFF), mirrored every 2KB
                Ram.Write(address, data);
                return;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                // Handle PPU registers (0x2000 - 0x3FFF), mirrored every 8 bytes
                Ppu.WriteRegister((byte)(address & 0x0007), data);
                return;
            }
            else if (address == 0x4014)
            {
                // Handle OAM DMA access
                Ppu.PerformOamDma(data);
                return;
            }
            else if (address >= 0x4016 && address <= 0x4017)
            {
                _controllerState[address & 0x0001] = Controller[address & 0x0001];
                return;
            }
            else if (address >= 0x4020 && address <= 0xFFFF)
            {
                // Handle cartridge space (0x4020 - 0xFFFF)
                if (Cartridge != null && Cartridge.Write(address, data))
                {
                    return;
                }
            }

            // No operation if address is not handled
        }

        /// <summary>
        /// Resets the central bus and connected components.
        /// </summary>
        public void Reset()
        {
            Cpu?.Reset();
            Ppu?.Reset();
        }
    }
}
