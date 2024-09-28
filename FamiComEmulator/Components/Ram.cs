namespace FamiComEmulator.Components
{
    /// <summary>
    /// Represents the system RAM for the NES emulator.
    /// Handles 2KB of internal RAM with mirroring up to 0x1FFF.
    /// </summary>
    public class Ram
    {
        private const int RamSize = 0x0800; // 2KB
        private readonly byte[] _memory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ram"/> class and clears the memory.
        /// </summary>
        public Ram()
        {
            _memory = new byte[RamSize];
            Array.Clear(_memory, 0, _memory.Length);
        }

        /// <summary>
        /// Reads a byte from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from (0x0000 - 0x1FFF).</param>
        /// <returns>The byte value stored at the specified address.</returns>
        public byte Read(ushort address)
        {
            // NES RAM is mirrored every 2KB up to 0x1FFF
            ushort mirroredAddress = (ushort)(address % RamSize);
            return _memory[mirroredAddress];
        }

        /// <summary>
        /// Writes a byte to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to (0x0000 - 0x1FFF).</param>
        /// <param name="data">The byte value to write to memory.</param>
        public void Write(ushort address, byte data)
        {
            // NES RAM is mirrored every 2KB up to 0x1FFF
            ushort mirroredAddress = (ushort)(address % RamSize);
            _memory[mirroredAddress] = data;
        }
    }
}
