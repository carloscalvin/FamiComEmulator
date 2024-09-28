using FamiComEmulator.Components;

namespace FamiComEmulator.Tests.Components
{
    public class CartridgeTests : IDisposable
    {
        private readonly string _validRomPath;
        private readonly string _invalidRomPath;
        private readonly string _unsupportedMapperRomPath;
        private readonly string _realRomPath;

        public CartridgeTests()
        {
            // Create temporary ROM files for testing
            _validRomPath = Path.GetTempFileName();
            _invalidRomPath = Path.GetTempFileName();
            _unsupportedMapperRomPath = Path.GetTempFileName();
            _realRomPath = Path.Combine("TestRoms", "nestest.nes");

            if (!File.Exists(_realRomPath))
            {
                throw new FileNotFoundException($"Real ROM file not found at path: {_realRomPath}");
            }

            // Create a valid ROM for Mapper 0 (NROM) with 1 PRG-ROM and 1 CHR-ROM
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(_validRomPath)))
            {
                // Magic Number "NES\x1A"
                writer.Write(new byte[] { 0x4E, 0x45, 0x53, 0x1A });
                writer.Write((byte)1); // Number of PRG-ROM banks
                writer.Write((byte)1); // Number of CHR-ROM banks
                writer.Write((byte)0x00); // Mapper1
                writer.Write((byte)0x00); // Mapper2
                writer.Write((byte)0);    // Number of PRG-RAM
                writer.Write((byte)0x00); // AdditionalFlags1
                writer.Write((byte)0x00); // AdditionalFlags2
                writer.Write(new byte[5]); // Reserved

                // PRG-ROM data (16KB)
                writer.Write(new byte[16384]);

                // CHR-ROM data (8KB)
                writer.Write(new byte[8192]);
            }

            // Create an invalid ROM (Incorrect Magic Number)
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(_invalidRomPath)))
            {
                writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }); // Incorrect Magic Number
                writer.Write((byte)1); // Number of PRG-ROM banks
                writer.Write((byte)1); // Number of CHR-ROM banks
                writer.Write((byte)0x00); // Mapper1
                writer.Write((byte)0x00); // Mapper2
                writer.Write((byte)0);    // Number of PRG-RAM
                writer.Write((byte)0x00); // AdditionalFlags1
                writer.Write((byte)0x00); // AdditionalFlags2
                writer.Write(new byte[5]); // Reserved

                // PRG-ROM data (16KB)
                writer.Write(new byte[16384]);

                // CHR-ROM data (8KB)
                writer.Write(new byte[8192]);
            }

            // Create a ROM with an unsupported mapper (Mapper 1)
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(_unsupportedMapperRomPath)))
            {
                writer.Write(new byte[] { 0x4E, 0x45, 0x53, 0x1A }); // Magic Number "NES\x1A"
                writer.Write((byte)2); // Number of PRG-ROM banks
                writer.Write((byte)1); // Number of CHR-ROM banks
                writer.Write((byte)0x10); // Mapper1 (bit 4 set for Mapper 1)
                writer.Write((byte)0x00); // Mapper2
                writer.Write((byte)0);    // Number of PRG-RAM
                writer.Write((byte)0x00); // AdditionalFlags1
                writer.Write((byte)0x00); // AdditionalFlags2
                writer.Write(new byte[5]); // Reserved

                // PRG-ROM data (32KB)
                writer.Write(new byte[32768]);

                // CHR-ROM data (8KB)
                writer.Write(new byte[8192]);
            }
        }

        [Fact]
        public void Cartridge_LoadsValidRomSuccessfully()
        {
            // Act
            Cartridge cartridge = new Cartridge(_validRomPath);

            // Assert
            Assert.NotNull(cartridge);

            // Verify that PRG-ROM was loaded correctly by reading from address 0x8000
            byte data = 0x00;
            bool readSuccess = cartridge.Read(0x8000, ref data);
            Assert.True(readSuccess);
            Assert.Equal(0x00, data); // Since the PRG-ROM was initialized with 0x00

            // Verify that CHR-ROM was loaded correctly by reading from address 0x2000 (not implemented yet)
            // This part can be expanded once CHR-ROM handling is implemented
        }

        [Fact]
        public void Cartridge_ThrowsExceptionForInvalidMagicNumber()
        {
            // Act & Assert
            Assert.Throws<InvalidDataException>(() => new Cartridge(_invalidRomPath));
        }

        [Fact]
        public void Cartridge_ThrowsExceptionForUnsupportedMapper()
        {
            // Act & Assert
            Assert.Throws<NotSupportedException>(() => new Cartridge(_unsupportedMapperRomPath));
        }

        [Fact]
        public void Cartridge_ReadReturnsCorrectData()
        {
            // Arrange
            Cartridge cartridge = new Cartridge(_validRomPath);
            byte expectedData = 0x00; // Since PRG-ROM was initialized with 0x00
            ushort address = 0x8000;

            // Act
            byte actualData = 0xFF;
            bool success = cartridge.Read(address, ref actualData);

            // Assert
            Assert.True(success);
            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public void Cartridge_LoadsRealRomSuccessfully()
        {
            // Arrange
            Cartridge cartridge = new Cartridge(_realRomPath);

            // Act & Assert
            Assert.NotNull(cartridge);

            // Verify that PRG-ROM was loaded correctly by reading from specific addresses
            // The exact values depend on the 'nestest.nes' ROM content
            // For demonstration, we'll read a few addresses and ensure they can be read without exceptions

            ushort[] testAddresses = { 0x8000, 0xC000, 0xFFFF };
            foreach (var address in testAddresses)
            {
                byte data = 0x00;
                bool readSuccess = cartridge.Read(address, ref data);
                Assert.True(readSuccess, $"Failed to read from address {address:X4}");
            }
        }

        public void Dispose()
        {
            // Delete temporary ROM files
            if (File.Exists(_validRomPath))
                File.Delete(_validRomPath);
            if (File.Exists(_invalidRomPath))
                File.Delete(_invalidRomPath);
            if (File.Exists(_unsupportedMapperRomPath))
                File.Delete(_unsupportedMapperRomPath);
        }
    }
}
