namespace FamiComEmulator.Tests
{
    public static class TestRomCreator
    {
        /// <summary>
        /// Creates a ROM with LDA #$42 instruction.
        /// </summary>
        public static void CreateLdaTestRom(string filePath)
        {
            // Instructions:
            // LDA #$42 (0xA9 0x42)
            // NOP (0xEA)
            // BRK (0x00) - to stop execution
            byte[] prgRomData = new byte[]
            {
                0xA9, 0x42, // LDA #$42
                0xEA,       // NOP
                0x00        // BRK
            };

            RomBuilder.CreateTestRom(filePath, prgRomData);
        }

        /// <summary>
        /// Creates a ROM with LDA #$10 and ADC #$05 instructions.
        /// </summary>
        public static void CreateAdcTestRom(string filePath)
        {
            // Instructions:
            // LDA #$10 (0xA9 0x10)
            // ADC #$05 (0x69 0x05)
            // NOP (0xEA)
            // BRK (0x00) - to stop execution
            byte[] prgRomData = new byte[]
            {
                0xA9, 0x10, // LDA #$10
                0x69, 0x05, // ADC #$05
                0xEA,       // NOP
                0x00        // BRK
            };

            RomBuilder.CreateTestRom(filePath, prgRomData);
        }
    }
}
