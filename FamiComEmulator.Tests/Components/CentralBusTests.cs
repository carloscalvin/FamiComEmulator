using FamiComEmulator.Components;

namespace FamiComEmulator.Tests.Components
{
    public class CentralBusTests
    {
        private readonly CentralBus _bus;

        public CentralBusTests()
        {
            _bus = new CentralBus(new Cpu6502(), new Ppu2c02(new PpuRenderer()), new Apu());
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

        // Test for writing and reading controller 1 status
        [Fact]
        public void CentralBus_WriteAndRead_Controller1()
        {
            // Arrange
            ushort address = 0x4016;
            _bus.Controller[0] = 0x80; // A Button pressed (bit 7)

            // Act
            _bus.Write(address, 0x01);  // Simulate reset write
            byte readValue = _bus.Read(address); // Read controller state

            // Assert
            Assert.Equal(0x01, readValue);  // A button should be active
        }

        // Test for writing and reading multiple button presses on controller 1
        [Fact]
        public void CentralBus_WriteAndRead_Controller1_MultipleReads()
        {
            // Arrange
            ushort address = 0x4016;
            _bus.Controller[0] = 0xC0; // A and B Buttons pressed (bits 7 and 6)

            // Act
            _bus.Write(address, 0x01);  // Simulate reset write
            byte readA = _bus.Read(address); // Read A button
            byte readB = _bus.Read(address); // Read B button

            // Assert
            Assert.Equal(0x01, readA);  // A button should be active
            Assert.Equal(0x01, readB);  // B button should be active
        }

        // Test for writing and reading controller 2 status
        [Fact]
        public void CentralBus_WriteAndRead_Controller2()
        {
            // Arrange
            ushort address = 0x4017;
            _bus.Controller[1] = 0x80; // A Button pressed (bit 7)

            // Act
            _bus.Write(address, 0x01);  // Simulate reset write
            byte readValue = _bus.Read(address); // Read controller state

            // Assert
            Assert.Equal(0x01, readValue);  // A button should be active
        }

        // Test for writing and reading multiple button presses on controller 2
        [Fact]
        public void CentralBus_WriteAndRead_Controller2_MultipleReads()
        {
            // Arrange
            ushort address = 0x4017;
            _bus.Controller[1] = 0xC0; // A and B Buttons pressed (bits 7 and 6)

            // Act
            _bus.Write(address, 0x01);  // Simulate reset write
            byte readA = _bus.Read(address); // Read A button
            byte readB = _bus.Read(address); // Read B button

            // Assert
            Assert.Equal(0x01, readA);  // A button should be active
            Assert.Equal(0x01, readB);  // B button should be active
        }
    }
}
