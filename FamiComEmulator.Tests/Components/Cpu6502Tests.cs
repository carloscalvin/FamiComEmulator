using FamiComEmulator.Components;
using FamiComEmulator.Tests.TestRoms;
using static FamiComEmulator.Components.ICpu6502;

namespace FamiComEmulator.Tests.Components
{
    public class Cpu6502Tests
    {
        // Paths for test ROMs
        private readonly string _ldaRomPath;
        private readonly string _adcRomPath;
        private readonly string _nestestRomPath;
        private readonly string _nestestLogPath;
        private readonly string _multiplicationRomPath;

        public Cpu6502Tests()
        {
            // Define paths for the test ROMs
            _ldaRomPath = Path.Combine("TestRoms", "test_lda.nes");
            _adcRomPath = Path.Combine("TestRoms", "test_adc.nes");
            _nestestRomPath = Path.Combine("TestRoms", "nestest.nes");
            _nestestLogPath = Path.Combine("TestRoms", "nestest.log");
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
            IPpu2c02 ppu = new Ppu2c02(new PpuRenderer());
            IApu apu = new Apu();
            ICentralBus bus = new CentralBus(cpu6502, ppu, apu);
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
            IPpu2c02 ppu = new Ppu2c02(new PpuRenderer());
            IApu apu = new Apu();
            ICentralBus bus = new CentralBus(cpu6502, ppu, apu);
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
            IPpu2c02 ppu = new Ppu2c02(new PpuRenderer());
            IApu apu = new Apu();
            ICentralBus bus = new CentralBus(cpu6502, ppu, apu);
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
        public void TestNestest()
        {
            // Arrange
            ICpu6502 cpu6502 = new Cpu6502();
            IPpu2c02 ppu = new Ppu2c02(new PpuRenderer());
            IApu apu = new Apu();
            ICentralBus bus = new CentralBus(cpu6502, ppu, apu);
            bus.AddCartridge(new Cartridge(_nestestRomPath));

            // Parse the nestest log
            List<CpuPpuState> expectedStates = NestestParser.ParseLog(_nestestLogPath);

            // Reset the bus and CPU
            bus.Reset();

            // Hardcode the Program Counter to 0xC000 to enter the automated test mode
            cpu6502.ProgramCounter = 0xC000;
            cpu6502.Cycle = 7;

            // Act & Assert
            foreach (var expectedState in expectedStates)
            {
                while (cpu6502.ProgramCounter != expectedState.Address)
                {
                    cpu6502.Clock();
                }

                ushort pc = cpu6502.ProgramCounter;
                byte a = cpu6502.Accumulator;
                byte x = cpu6502.XRegister;
                byte y = cpu6502.YRegister;
                byte status = (byte)cpu6502.Status;
                byte sp = cpu6502.StackPointer;
                int cycle = cpu6502.Cycle;

                Assert.Equal(expectedState.Address, pc);
                Assert.Equal(expectedState.Accumulator, a);
                Assert.Equal(expectedState.XRegister, x);
                Assert.Equal(expectedState.YRegister, y);
                Assert.Equal(expectedState.Status, status);
                Assert.Equal(expectedState.StackPointer, sp);
                Assert.Equal(expectedState.Cycle, cycle);
            }
        }
    }
}
