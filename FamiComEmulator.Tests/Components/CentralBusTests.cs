using FamiComEmulator.Components;

namespace FamiComEmulator.Tests.Components
{
    public class CentralBusTests
    {
        private readonly CentralBus _bus;
        private readonly ICpu6502 _cpu6502;

        public CentralBusTests()
        {
            _cpu6502 = new Cpu6502();
            _bus = new CentralBus(_cpu6502);
        }

        [Fact]
        public void CentralBus_ReadAndWrite_RamAccess()
        {
            // Arrange
            ushort address = 0x0000;
            byte value = 0xAA;

            // Act
            _bus.Write(address, value);
            byte readValue = _bus.Read(address);

            // Assert
            Assert.Equal(value, readValue);
        }

        [Fact]
        public void CentralBus_ReadAndWrite_MirroredRamAccess()
        {
            // Arrange
            ushort baseAddress = 0x0000;
            ushort mirroredAddress = 0x0800; // Mirrored address
            byte value = 0xBB;

            // Act
            _bus.Write(baseAddress, value);
            byte readValue = _bus.Read(mirroredAddress);

            // Assert
            Assert.Equal(value, readValue);
        }

        [Fact]
        public void CentralBus_ReadAndWrite_MaxRamAddress()
        {
            // Arrange
            ushort address = 0x1FFF;
            byte value = 0xCC;

            // Act
            _bus.Write(address, value);
            byte readValue = _bus.Read(address);

            // Assert
            Assert.Equal(value, readValue);
        }

        [Fact]
        public void CentralBus_ReadOutsideRam_DoesNotAffectRam()
        {
            // Arrange
            ushort nonRamAddress = 0x2000; // Assuming no mapper or other hardware is connected
            byte value = 0xDD;

            // Act
            _bus.Write(nonRamAddress, value);
            byte readValue = _bus.Read(nonRamAddress);

            // Assert
            // Since no mapper or hardware is connected, read should return 0x00
            Assert.Equal(0x00, readValue);
        }

        [Fact]
        public void CentralBus_WriteAndRead_MultipleMirroredAddresses()
        {
            // Arrange
            ushort[] testAddresses = { 0x0000, 0x07FF, 0x0800, 0x0FFF, 0x1000, 0x17FF, 0x1800, 0x1FFF };
            byte[] testValues = { 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80 };

            // Act
            for (int i = 0; i < testAddresses.Length; i++)
            {
                _bus.Write(testAddresses[i], testValues[i]);
            }

            // Assert
            Assert.Equal(0x70, _bus.Read(0x0000));
            Assert.Equal(0x80, _bus.Read(0x07FF));
            Assert.Equal(_bus.Read(0x0000), _bus.Read(0x0800));
            Assert.Equal(_bus.Read(0x07FF), _bus.Read(0x0FFF));
            Assert.Equal(_bus.Read(0x0000), _bus.Read(0x1000));
            Assert.Equal(_bus.Read(0x07FF), _bus.Read(0x17FF));
            Assert.Equal(_bus.Read(0x0000), _bus.Read(0x1800));
            Assert.Equal(_bus.Read(0x07FF), _bus.Read(0x1FFF));
        }

    }
}
