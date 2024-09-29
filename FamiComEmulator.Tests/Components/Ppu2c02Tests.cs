using FamiComEmulator.Components;
using FamiComEmulator.Tests.TestRoms;

namespace FamiComEmulator.Tests.Components
{
    public class Ppu2c02Tests
    {
        // Paths for test ROMs
        private readonly string _nestestRomPath;
        private readonly string _nestestLogPath;

        public Ppu2c02Tests()
        {
            // Define paths for the test ROMs
            _nestestRomPath = Path.Combine("TestRoms", "nestest.nes");
            _nestestLogPath = Path.Combine("TestRoms", "nestest.log");

            // Ensure the TestRoms directory exists
            Directory.CreateDirectory("TestRoms");

            // Assume 'nestest.nes' is already present in TestRoms directory
            if (!File.Exists(_nestestRomPath))
            {
                throw new FileNotFoundException($"Test ROM file not found at path: {_nestestRomPath}");
            }
        }

        [Fact]
        public void TestPpuStateDuringNestest()
        {
            // Arrange
            ICpu6502 cpu6502 = new Cpu6502();
            IPpu2c02 ppu2c02 = new Ppu2c02();
            ICentralBus bus = new CentralBus(cpu6502, ppu2c02);
            bus.AddCartridge(new Cartridge(_nestestRomPath));

            // Parse the nestest log
            List<CpuPpuState> expectedStates = NestestParser.ParseLog(_nestestLogPath);

            // Reset the bus, cpu and ppu
            bus.Reset();

            // Hardcode the Program Counter to 0xC000 to enter the automated test mode
            cpu6502.ProgramCounter = 0xC000;
            cpu6502.Cycle = 7;
            ppu2c02.PpuX = 21;
            ppu2c02.PpuY = 0;
            cpu6502.Clock();

            // Act & Assert
            for (int i = 0; i < expectedStates.Count-1 ; i++)
            {
                while (cpu6502.ProgramCounter != expectedStates[i + 1].Address)
                {
                    bus.Clock();
                }

                int ppuX = ppu2c02.PpuX;
                int ppuY = ppu2c02.PpuY;

                // Assert PPU state
                Assert.Equal(expectedStates[i].PpuX, ppuX);
                Assert.Equal(expectedStates[i].PpuY, ppuY);
            }
        }
    }
}
