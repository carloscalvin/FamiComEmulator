namespace FamiComEmulator.Tests
{
    /// <summary>
    /// Helper class to create NES test ROMs with specific instructions.
    /// </summary>
    public static class RomBuilder
    {
        /// <summary>
        /// Creates a minimal NES ROM with specified PRG-ROM data.
        /// </summary>
        /// <param name="filePath">Path where the ROM will be saved.</param>
        /// <param name="prgRomData">Byte array representing the PRG-ROM instructions.</param>
        public static void CreateTestRom(string filePath, byte[] prgRomData)
        {
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(filePath)))
            {
                // Write NES Header
                writer.Write(new byte[] { 0x4E, 0x45, 0x53, 0x1A }); // Magic Number "NES\x1A"
                writer.Write((byte)1); // Number of PRG-ROM banks (16KB each)
                writer.Write((byte)0); // Number of CHR-ROM banks (0 for simplicity)
                writer.Write((byte)0x00); // Flags 6 - Mapper and mirroring
                writer.Write((byte)0x00); // Flags 7 - Mapper, VS/Playchoice, NES 2.0
                writer.Write((byte)0);    // Number of PRG-RAM banks
                writer.Write((byte)0x00); // Flags 8 - TV system
                writer.Write((byte)0x00); // Flags 9 - TV system, PRG-RAM presence
                writer.Write(new byte[5]); // Reserved bytes

                // Write PRG-ROM data
                writer.Write(prgRomData);

                // Pad the PRG-ROM to 16KB if necessary
                int prgRomSize = 16384; // 16KB
                if (prgRomData.Length < prgRomSize)
                {
                    writer.Write(new byte[prgRomSize - prgRomData.Length]);
                }

                // Set the Reset Vector at the end of PRG-ROM to point to 0x8000
                // Calculate the offset where to write the reset vector
                long prgRomEndOffset = 16 + prgRomSize; // 16 bytes header + 16KB PRG-ROM
                writer.Seek((int)(prgRomEndOffset - 2), SeekOrigin.Begin);
                writer.Write((byte)0x00); // Reset Vector Low Byte
                writer.Write((byte)0x80); // Reset Vector High Byte
            }
        }
    }
}
