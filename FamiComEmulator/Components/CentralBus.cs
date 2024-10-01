namespace FamiComEmulator.Components
{
    /// <summary>
    /// Represents the central bus in the NES emulator, handling communication between CPU, RAM, and Cartridge.
    /// </summary>
    public class CentralBus : ICentralBus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CentralBus"/> class with the specified CPU.
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

            // Handle PPU registers (0x2000 - 0x2007)
            if (address >= 0x2000 && address <= 0x2007)
            {
                data = Ppu.ReadRegister((byte)address);
                return data;
            }

            // Attempt to read from the cartridge first
            if (Cartridge != null && Cartridge.Read(address, ref data))
            {
                return data;
            }

            // Handle RAM addresses (0x0000 - 0x1FFF)
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                data = Ram.Read(address);
                return data;
            }

            // Handle PPU memory reads (0x0000 - 0x3FFF)
            if (address >= 0x0000 && address <= 0x3FFF)
            {
                data = Ppu.ReadPpuMemory((ushort)(address & 0x3FFF));
                return data;
            }

            // TODO: Handle other memory ranges (IO registers, etc.)

            return data;
        }

        /// <summary>
        /// Writes a byte to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to (0x0000 - 0xFFFF).</param>
        /// <param name="data">The byte value to write to memory.</param>
        public void Write(ushort address, byte data)
        {
            if (address == 0x4014)
            {
                // DMA de OAM
                Ppu.PerformOamDma(data);
                return;
            }
            // Handle PPU registers (0x2000 - 0x2007)
            if (address >= 0x2000 && address <= 0x2007)
            {
                Ppu.WriteRegister((byte)address, data);
                return;
            }

            // Attempt to write to the cartridge first
            if (Cartridge != null && Cartridge.Write(address, data))
            {
                return;
            }

            // Handle RAM addresses (0x0000 - 0x1FFF)
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                Ram.Write(address, data);
                return;
            }

            // Handle PPU memory writes (0x0000 - 0x3FFF)
            if (address >= 0x0000 && address <= 0x3FFF)
            {
                Ppu.WritePpuMemory((ushort)(address & 0x3FFF), data);
                return;
            }

            // TODO: Handle other memory ranges (IO registers, etc.)
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
