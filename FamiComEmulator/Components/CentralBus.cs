namespace FamiComEmulator.Components
{
    /// <summary>
    /// Represents the central bus in the NES emulator, handling communication between CPU, RAM, and Cartridge.
    /// </summary>
    public class CentralBus : ICentralBus
    {
        private ICartridge? _cartridge;

        /// <summary>
        /// Initializes a new instance of the <see cref="CentralBus"/> class with the specified CPU.
        /// </summary>
        /// <param name="cpu">The CPU component to connect to the bus.</param>
        public CentralBus(ICpu6502 cpu)
        {
            Cpu = cpu;
            Ram = new Ram();
            Cpu.AddCentralBus(this);
        }

        /// <summary>
        /// Gets the CPU connected to the central bus.
        /// </summary>
        public ICpu6502 Cpu { get; }

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
            _cartridge = cartridge;
        }

        /// <summary>
        /// Advances the emulator by one clock cycle.
        /// </summary>
        public void Clock()
        {
            Cpu?.Clock();
        }

        /// <summary>
        /// Reads a byte from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from (0x0000 - 0xFFFF).</param>
        /// <returns>The byte value stored at the specified address.</returns>
        public byte Read(ushort address)
        {
            byte data = 0x00;

            // Attempt to read from the cartridge first
            if (_cartridge != null && _cartridge.Read(address, ref data))
            {
                return data;
            }

            // Handle RAM addresses (0x0000 - 0x1FFF)
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                data = Ram.Read(address);
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
            // Attempt to write to the cartridge first
            if (_cartridge != null && _cartridge.Write(address, data))
            {
                return;
            }

            // Handle RAM addresses (0x0000 - 0x1FFF)
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                Ram.Write(address, data);
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
        }
    }
}
