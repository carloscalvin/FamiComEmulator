namespace FamiComEmulator.Tests
{
    /// <summary>
    /// Helper class to create specific test ROMs.
    /// </summary>
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

        /// <summary>
        /// Creates a ROM with a simple multiplication program.
        /// </summary>
        public static void CreateMultiplicationTestRom(string filePath)
        {
            // Multiplication Program:
            // ; Load X with 10 (multiplicand)
            // LDX #$0A       ; 0xA2 0x0A
            // STX $0000      ; 0x8E 0x00 0x00
            // ; Load X with 3 (multiplier)
            // LDX #$03       ; 0xA2 0x03
            // STX $0001      ; 0x8E 0x01 0x00
            // ; Load Y with multiplicand from $0000
            // LDY $0000      ; 0xAC 0x00 0x00
            // LDA #$00       ; 0xA9 0x00
            // CLC            ; 0x18
            // Loop:
            // ADC $0001      ; 0x6D 0x01 0x00
            // DEY            ; 0x88
            // BNE Loop       ; 0xD0 0xFA (branch back by 6 bytes)
            // STA $0002      ; 0x8D 0x02 0x00
            // NOP            ; 0xEA
            // NOP            ; 0xEA
            // NOP            ; 0xEA
            byte[] prgRomData = new byte[]
            {
                // LDX #$0A
                0xA2, 0x0A,       // 0x8000: A2 0A

                // STX $0000
                0x8E, 0x00, 0x00, // 0x8002: 8E 00 00

                // LDX #$03
                0xA2, 0x03,       // 0x8005: A2 03

                // STX $0001
                0x8E, 0x01, 0x00, // 0x8007: 8E 01 00

                // LDY $0000
                0xAC, 0x00, 0x00, // 0x800A: AC 00 00

                // LDA #$00
                0xA9, 0x00,       // 0x800D: A9 00

                // CLC
                0x18,             // 0x800F: 18

                // ADC $0001
                0x6D, 0x01, 0x00, // 0x8010: 6D 01 00

                // DEY
                0x88,             // 0x8013: 88

                // BNE Loop (branch back by 6 bytes to LDY)
                0xD0, 0xFA,       // 0x8014: D0 FA

                // STA $0002
                0x8D, 0x02, 0x00, // 0x8016: 8D 02 00

                // NOP
                0xEA,             // 0x8019: EA

                // NOP
                0xEA,             // 0x801A: EA

                // NOP
                0xEA              // 0x801B: EA
            };

            RomBuilder.CreateTestRom(filePath, prgRomData);
        }
    }
}
