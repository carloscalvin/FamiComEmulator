using FamiComEmulator.Components;
using static FamiComEmulator.Components.ICpu6502;

namespace FamiComEmulator.Tests.Components
{
    public class Cpu6502Tests
    {
        // Paths for test ROMs
        private readonly string _ldaRomPath;
        private readonly string _adcRomPath;
        private readonly string _nestestRomPath;
        private readonly string _multiplicationRomPath;

        public Cpu6502Tests()
        {
            // Define paths for the test ROMs
            _ldaRomPath = Path.Combine("TestRoms", "test_lda.nes");
            _adcRomPath = Path.Combine("TestRoms", "test_adc.nes");
            _nestestRomPath = Path.Combine("TestRoms", "nestest.nes");
            _multiplicationRomPath = Path.Combine("TestRoms", "test_multiplication.nes");

            // Ensure the TestRoms directory exists
            Directory.CreateDirectory("TestRoms");

            // Create test ROMs
            TestRomCreator.CreateLdaTestRom(_ldaRomPath);
            TestRomCreator.CreateAdcTestRom(_adcRomPath);
            TestRomCreator.CreateMultiplicationTestRom(_multiplicationRomPath);

            // Assume 'nestest.nes' is already present in TestRoms directory
            if (!File.Exists(_nestestRomPath))
            {
                throw new FileNotFoundException($"Real ROM file not found at path: {_nestestRomPath}");
            }
        }

        [Fact]
        public void TestLDAInstructionExecution()
        {
            // Arrange
            ICpu6502 cpu6502 = new Cpu6502();
            ICentralBus bus = new CentralBus(cpu6502);
            bus.AddCartridge(new Cartridge(_ldaRomPath));
            bus.Reset();
            while (!bus.Cpu.Finish())
            {
                bus.Clock();
            }

            // Act
            for (int i = 0; i < 4; i++)
            {
                bus.Clock();
            }

            // Assert
            Assert.Equal(0x42, cpu6502.Accumulator);
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.Zero));
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.Negative));
        }

        [Fact]
        public void TestADCInstructionExecution()
        {
            // Arrange
            ICpu6502 cpu6502 = new Cpu6502();
            ICentralBus bus = new CentralBus(cpu6502);
            bus.AddCartridge(new Cartridge(_adcRomPath));
            bus.Reset();
            while (!bus.Cpu.Finish())
            {
                bus.Clock();
            }

            // Act
            for (int i = 0; i < 4; i++)
            {
                bus.Clock();
            }

            // Assert
            Assert.Equal(0x15, cpu6502.Accumulator); // 0x10 + 0x05 = 0x15
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.CarryBit));
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.Zero));
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.Overflow));
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.Negative));
        }

        [Fact]
        public void TestMultiplicationProgramExecution()
        {
            // Arrange
            ICpu6502 cpu6502 = new Cpu6502();
            ICentralBus bus = new CentralBus(cpu6502);
            bus.AddCartridge(new Cartridge(_multiplicationRomPath));
            bus.Reset();
            while (!bus.Cpu.Finish())
            {
                bus.Clock();
            }

            // Act
            for (int i = 0; i < 112; i++)
            {
                bus.Clock();
            }

            // Assert
            // After multiplication, the product should be stored at $0002
            // In our multiplication program, 10 * 3 = 30 (0x1E)
            byte product = bus.Read(0x0002);

            Assert.Equal(30, product); // 0x1E in hexadecimal
            Assert.Equal(3, cpu6502.XRegister); // X should be 3 (multiplier)
            Assert.Equal(0, cpu6502.YRegister); // Y should be 0 after loop
            Assert.Equal(30, cpu6502.Accumulator); // A should be 30 after additions
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.CarryBit));
            Assert.Equal(1, cpu6502.GetFlag(Flags6502.Zero));
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.Overflow));
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.Negative)); // Negative flag not set because A is positive
        }

        [Fact]
        public void NesTestRom()
        {
            // Arrange
            ICpu6502 cpu6502 = new Cpu6502();
            ICentralBus bus = new CentralBus(cpu6502);
            bus.AddCartridge(new Cartridge(_nestestRomPath));
            bus.Reset();

            // Act
            while (!cpu6502.Finish())
            {
                bus.Clock();
            }

            // Asserts based on the expected state after running 'nestest.nes'
            // These values should be adjusted according to 'nestest.nes' specifications
            Assert.Equal(3, cpu6502.XRegister);
            Assert.Equal(0, cpu6502.YRegister);
            Assert.Equal(30, cpu6502.Accumulator);
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.CarryBit));
            Assert.Equal(1, cpu6502.GetFlag(Flags6502.Zero));
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.Overflow));
            Assert.Equal(0, cpu6502.GetFlag(Flags6502.Negative));
        }
    }
}
