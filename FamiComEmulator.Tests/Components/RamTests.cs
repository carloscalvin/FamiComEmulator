using FamiComEmulator.Components;

namespace FamiComEmulator.Tests.Components
{
    public class RamTests
    {
        private readonly Ram _ram;

        public RamTests()
        {
            _ram = new Ram();
        }

        [Fact]
        public void Ram_Initializes_ClearedMemory()
        {
            // Arrange & Act
            for (int address = 0x0000; address <= 0x1FFF; address++)
            {
                byte data = _ram.Read((ushort)address);
                // Assert
                Assert.Equal(0x00, data);
            }
        }

        [Fact]
        public void Ram_WriteAndRead_SingleAddress()
        {
            // Arrange
            ushort address = 0x07FF;
            byte value = 0xAB;

            // Act
            _ram.Write(address, value);
            byte readValue = _ram.Read(address);

            // Assert
            Assert.Equal(value, readValue);
        }

        [Fact]
        public void Ram_WriteAndRead_MirroredAddresses()
        {
            // Arrange
            ushort baseAddress = 0x0000;
            ushort mirroredAddress = 0x0800; // Mirrored of 0x0000
            byte value = 0xCD;

            // Act
            _ram.Write(baseAddress, value);
            byte readValue = _ram.Read(mirroredAddress);

            // Assert
            Assert.Equal(value, readValue);
        }

        [Fact]
        public void Ram_WriteAndRead_MaxAddress()
        {
            // Arrange
            ushort address = 0x1FFF;
            byte value = 0xEF;

            // Act
            _ram.Write(address, value);
            byte readValue = _ram.Read(address);

            // Assert
            Assert.Equal(value, readValue);
        }

        [Fact]
        public void Ram_WriteAndRead_EveryTwoKB()
        {
            // Arrange
            ushort[] testAddresses = { 0x0000, 0x07FF, 0x0800, 0x0FFF, 0x1000, 0x17FF, 0x1800, 0x1FFF };
            byte[] testValues = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            // Act
            for (int i = 0; i < testAddresses.Length; i++)
            {
                _ram.Write(testAddresses[i], testValues[i]);
            }

            // Assert
            Assert.Equal(0x07, _ram.Read(0x0000));
            Assert.Equal(0x07, _ram.Read(0x0800));
            Assert.Equal(0x07, _ram.Read(0x1000));
            Assert.Equal(0x07, _ram.Read(0x1800));
            Assert.Equal(0x08, _ram.Read(0x07FF));
            Assert.Equal(0x08, _ram.Read(0x0FFF));
            Assert.Equal(0x08, _ram.Read(0x17FF));
            Assert.Equal(0x08, _ram.Read(0x1FFF));
        }

        [Fact]
        public void Ram_Mirroring_WorksCorrectly()
        {
            // Arrange
            ushort baseAddress = 0x0000;
            ushort mirroredAddress = 0x0800; // Mirrored address of 0x0000
            byte value = 0xAA;

            // Act
            _ram.Write(baseAddress, value);
            byte readMirrored = _ram.Read(mirroredAddress);

            // Assert
            Assert.Equal(value, readMirrored);
        }
    }
}
