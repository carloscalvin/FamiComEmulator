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
            using (BinaryWriter writer = new BinaryWriter(File.Create(filePath)))
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

                // Set the Reset Vector at the correct position (0x3FFC and 0x3FFD within PRG-ROM)
                // PRG-ROM starts at byte 16 in the file, so 0x3FFC offset is 16380
                // 16 (header) + 16380 = 16396
                int resetVectorOffset = 16 + 0x3FFC; // 16 + 16380 = 16396
                writer.Seek(resetVectorOffset, SeekOrigin.Begin);
                writer.Write((byte)0x00); // Reset Vector Low Byte
                writer.Write((byte)0x80); // Reset Vector High Byte
            }
        }
    }
}
