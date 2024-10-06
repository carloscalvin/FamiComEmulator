using System.Drawing;
using FamiComEmulator.Components;
using FamiComEmulator.Tests.TestRoms;
using Moq;

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
            Cpu6502 cpu6502 = new Cpu6502();
            Ppu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            ICentralBus bus = new CentralBus(cpu6502, ppu2c02);
            bus.AddCartridge(new Cartridge(_nestestRomPath));

            // Parse the nestest log
            List<CpuPpuState> expectedStates = NestestParser.ParseLog(_nestestLogPath);

            // Reset the bus, cpu and ppu
            bus.Reset();

            // Hardcode the Program Counter to 0xC000 to enter the automated test mode
            cpu6502.ProgramCounter = 0xC000;
            cpu6502.Cycle = 7;
            ppu2c02.PpuX = ppu2c02._cycle = 21;
            ppu2c02.PpuY = ppu2c02._scanline = 0;
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

        [Fact]
        public void Reset_ShouldInitializeRegistersAndState()
        {
            // Arrange
            IPpu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            ppu2c02.Control = 0xFF;
            ppu2c02.Mask = 0xFF;
            ppu2c02.OamAddress = 0xFF;
            ppu2c02.OamData = 0xFF;
            ppu2c02.PpuAddress = 0xFFFF;
            ppu2c02.PpuData = 0xFF;
            ppu2c02.PpuX = 100;
            ppu2c02.PpuY = 100;

            // Act
            ppu2c02.Reset();

            // Assert
            Assert.Equal(0x00, ppu2c02.Control);
            Assert.Equal(0x00, ppu2c02.Mask);
            Assert.Equal(0x00, ppu2c02.Status);
            Assert.Equal(0x00, ppu2c02.OamData);
            Assert.Equal(0x0000, ppu2c02.PpuAddress);
            Assert.Equal(0x00, ppu2c02.PpuData);
            Assert.Equal(0, ppu2c02.PpuX);
            Assert.Equal(0, ppu2c02.PpuY);
        }

        [Fact]
        public void WriteRegister_ShouldUpdateControlRegister()
        {
            // Arrange
            IPpu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            byte register = 0x00; // PPUCTRL
            byte data = 0x80;

            // Act
            ppu2c02.WriteRegister(register, data);

            // Assert
            Assert.Equal(data, ppu2c02.Control);
        }

        [Fact]
        public void ReadRegister_ShouldReturnStatusAndClearVBlank()
        {
            // Arrange
            IPpu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            byte register = 0x02; // PPUSTATUS
            ppu2c02.Status = 0x80; // VerticalBlank flag set

            // Act
            byte status = ppu2c02.ReadRegister(register);

            // Assert
            Assert.Equal(0x80, status);
            Assert.Equal(0x00, ppu2c02.Status); // VBlank flag cleared
        }

        [Fact]
        public void WriteRegister_ShouldHandlePPUScrollToggle()
        {
            // Arrange
            IPpu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            byte register = 0x05; // PPUSCROLL
            byte scrollX = 0x10;
            byte scrollY = 0x20;

            // Act
            ppu2c02.WriteRegister(register, scrollX); // First write (ScrollX)
            ppu2c02.WriteRegister(register, scrollY); // Second write (ScrollY)

            // Assert
            Assert.Equal(scrollX, ppu2c02.ScrollX);
            Assert.Equal(scrollY, ppu2c02.ScrollY);
        }

        [Fact]
        public void ReadRegister_ShouldBufferPPUData()
        {
            // Arrange
            byte expectedData = 0xAB;
            ushort ppuAddress = 0x2000;
            byte register = 0x07; // PPUDATA
            ICpu6502 cpu6502 = new Cpu6502();
            IPpu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            // Create a mock CentralBus and set up the PPU
            var bus = new CentralBus(cpu6502, ppu2c02);

            // Mock the Cartridge to return expectedData when reading from ppuAddress
            var mockCartridge = new Mock<ICartridge>();
            mockCartridge.Setup(c => c.Read(It.IsAny<ushort>(), ref It.Ref<byte>.IsAny))
                .Callback((ushort address, ref byte data) =>
                {
                    if (address == (ppuAddress & 0x3FFF))
                    {
                        data = expectedData;
                    }
                })
                .Returns(true);
            bus.AddCartridge(mockCartridge.Object);

            // Set PpuAddress to the target address
            ppu2c02.WriteRegister(0x06, (byte)((ppuAddress >> 8) & 0xFF)); // High byte
            ppu2c02.WriteRegister(0x06, (byte)(ppuAddress & 0xFF));        // Low byte

            // Act
            byte firstRead = ppu2c02.ReadRegister(register); // Should return bufferedData (initially 0)
            byte secondRead = ppu2c02.ReadRegister(register); // Should return expectedData

            // Assert
            Assert.Equal(0x00, firstRead);
            Assert.Equal(expectedData, secondRead);
        }

        [Fact]
        public void Clock_ShouldIncrementCycleAndScanline()
        {
            // Arrange
            Mock<ICartridge> mockCartridge = new Mock<ICartridge>();
            mockCartridge.Setup(c => c.Read(It.IsAny<ushort>(), ref It.Ref<byte>.IsAny))
                         .Returns(false);
            mockCartridge.Setup(c => c.Write(It.IsAny<ushort>(), It.IsAny<byte>()))
                         .Returns(false);
            ICpu6502 cpu6502 = new Cpu6502();
            Ppu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            CentralBus bus = new CentralBus(cpu6502, ppu2c02);
            bus.AddCartridge(mockCartridge.Object);
            int initialCycle = ppu2c02.PpuX;
            int initialScanline = ppu2c02.PpuY;

            // Act
            ppu2c02.Clock();

            // Assert
            Assert.Equal(initialCycle + 1, ppu2c02.PpuX);
            Assert.Equal(initialScanline, ppu2c02.PpuY);
        }

        [Fact]
        public void Clock_ShouldTriggerNmi()
        {
            // Arrange
            Mock<ICpu6502> mockCpu6502 = new Mock<ICpu6502>();
            Ppu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            ICentralBus bus = new CentralBus(mockCpu6502.Object, ppu2c02);
            Mock<ICartridge> mockCartridge = new Mock<ICartridge>();
            mockCartridge.Setup(c => c.Read(It.IsAny<ushort>(), ref It.Ref<byte>.IsAny))
                         .Returns(false);
            mockCartridge.Setup(c => c.Write(It.IsAny<ushort>(), It.IsAny<byte>()))
                         .Returns(false);
            ICpu6502 cpu6502 = new Cpu6502();
            bus.AddCartridge(mockCartridge.Object);
            ppu2c02.PpuY = ppu2c02._scanline = 240;
            ppu2c02.PpuX = ppu2c02._cycle = 0;
            ppu2c02.Control = 0x80; // Enable NMI

            // Act
            for (int i = 0; i < 341 * 22; i++) // Advance to scanline 241
            {
                ppu2c02.Clock();
            }

            // Assert
            mockCpu6502.Verify(c => c.Nmi(), Times.Once);
        }

        [Fact]
        public void Clock_ShouldClearVBlankOnPreRenderScanline()
        {
            // Arrange
            Ppu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            ppu2c02.Status |= (byte)Ppu2c02.PpuStatusFlags.VerticalBlank;
            ppu2c02._cycle = 0;
            ppu2c02._scanline = -1; // Pre-render scanline

            // Act
            ppu2c02.Clock(); // Advance to cycle 1 of scanline -1

            // Assert
            Assert.False((ppu2c02.Status & (byte)Ppu2c02.PpuStatusFlags.VerticalBlank) != 0);
        }

        [Fact]
        public void Clock_ShouldSetVBlankOnScanline241()
        {
            // Arrange
            Ppu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            ppu2c02._cycle = 0;
            ppu2c02._scanline = 241;

            // Act
            ppu2c02.Clock(); // Advances to cycle 1 of scanline 241

            // Assert
            Assert.True((ppu2c02.Status & (byte)Ppu2c02.PpuStatusFlags.VerticalBlank) != 0);
        }

        [Fact]
        public void PerformOamDma_ShouldTransfer256BytesFromMemory()
        {
            // Arrange
            Mock<ICartridge> mockCartridge = new Mock<ICartridge>();
            mockCartridge.Setup(c => c.Read(It.IsAny<ushort>(), ref It.Ref<byte>.IsAny))
                         .Returns(false);
            mockCartridge.Setup(c => c.Write(It.IsAny<ushort>(), It.IsAny<byte>()))
                         .Returns(false);
            ICpu6502 cpu6502 = new Cpu6502();
            Ppu2c02 ppu2c02 = new Ppu2c02(new PpuRenderer());
            CentralBus bus = new CentralBus(cpu6502, ppu2c02);
            bus.AddCartridge(mockCartridge.Object);
            byte page = 0x02;
            ushort startAddress = (ushort)(page << 8);
            byte[] dmaData = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                dmaData[i] = (byte)i;
                bus.Ram.Write((ushort)(startAddress + i), dmaData[i]);
            }

            // Act
            bus.Write(0x4014, page);

            // Assert
            Assert.NotNull(ppu2c02._oam);
            for (int i = 0; i < 256; i++)
            {
                Assert.Equal(dmaData[i], ppu2c02._oam[i]);
            }
        }

        [Fact]
        public void ComposePixel_ShouldRenderBackgroundPixelCorrectly()
        {
            // Arrange
            var _mockRenderer = new Mock<IPpuRenderer>();
            var ppu2c02 = new Ppu2c02(_mockRenderer.Object);

            // Set Mask to render background and update render flags
            ppu2c02.Mask |= (byte)Ppu2c02.PpuMaskFlags.RenderBackground;
            ppu2c02.UpdateRenderFlags(); // Ensure _renderBackground is updated

            // Set fine X scroll to 0
            ppu2c02._fineX = 0;

            // Set cycle and scanline
            ppu2c02._cycle = 1; // So that _cycle - 1 = 0
            ppu2c02._scanline = 0;

            // Set background shifters to produce bgPixel = 1 and bgPalette = 1
            ppu2c02._bgShifterPatternLo = 0x8000; // Binary: 1000 0000 0000 0000
            ppu2c02._bgShifterPatternHi = 0x0000;
            ppu2c02._bgShifterAttribLo = 0x8000;
            ppu2c02._bgShifterAttribHi = 0x0000;

            int expectedCycle = 1;
            int expectedScanline = 0;

            // Act
            ppu2c02.ComposePixel();

            // Assert
            // The pixel should be based on bgPixel = 1 and bgPalette = 1
            Color expectedColor = ppu2c02.GetColourFromPaletteRam(1, 1);
            _mockRenderer.Verify(r => r.SetPixel(expectedCycle - 1, expectedScanline, expectedColor), Times.Once);
        }

        [Fact]
        public void IncrementScrollX_ShouldIncrementCoarseX()
        {
            // Arrange
            var ppu = new Ppu2c02(new PpuRenderer());
            ppu._vramAddress.CoarseX = 5;
            ppu._vramAddress.NametableX = false;
            ppu._renderBackground = true;

            // Act
            ppu.IncrementScrollX();

            // Assert
            Assert.Equal(6, ppu._vramAddress.CoarseX);
            Assert.False(ppu._vramAddress.NametableX);
        }

        [Fact]
        public void IncrementScrollY_ShouldIncrementFineY()
        {
            // Arrange
            var ppu = new Ppu2c02(new PpuRenderer());
            ppu._vramAddress.FineY = 5;
            ppu._vramAddress.CoarseY = 10;
            ppu._vramAddress.NametableY = false;
            ppu._renderBackground = true;

            // Act
            ppu.IncrementScrollY();

            // Assert
            Assert.Equal(6, ppu._vramAddress.FineY);
            Assert.Equal(10, ppu._vramAddress.CoarseY);
            Assert.False(ppu._vramAddress.NametableY);
        }

        [Fact]
        public void TransferAddressX_ShouldCopyHorizontalScroll()
        {
            // Arrange
            var ppu = new Ppu2c02(new PpuRenderer());
            ppu._tramAddress.CoarseX = 10;
            ppu._tramAddress.NametableX = true;
            ppu._vramAddress.CoarseX = 5;
            ppu._vramAddress.NametableX = false;
            ppu._renderBackground = true;

            // Act
            ppu.TransferAddressX();

            // Assert
            Assert.Equal(10, ppu._vramAddress.CoarseX);
            Assert.True(ppu._vramAddress.NametableX);
        }

        [Fact]
        public void TransferAddressY_ShouldCopyVerticalScroll()
        {
            // Arrange
            var ppu = new Ppu2c02(new PpuRenderer());
            ppu._tramAddress.FineY = 3;
            ppu._tramAddress.CoarseY = 12;
            ppu._tramAddress.NametableY = true;
            ppu._vramAddress.FineY = 5;
            ppu._vramAddress.CoarseY = 8;
            ppu._vramAddress.NametableY = false;
            ppu._renderBackground = true;

            // Act
            ppu.TransferAddressY();

            // Assert
            Assert.Equal(3, ppu._vramAddress.FineY);
            Assert.Equal(12, ppu._vramAddress.CoarseY);
            Assert.True(ppu._vramAddress.NametableY);
        }

        [Fact]
        public void ComposePixel_ShouldSetSpriteZeroHitFlag()
        {
            // Arrange
            var ppu = new Ppu2c02(new PpuRenderer());
            ppu.Mask = (byte)(Ppu2c02.PpuMaskFlags.RenderBackground | Ppu2c02.PpuMaskFlags.RenderSprites);
            ppu.UpdateRenderFlags();

            ppu._spriteZeroHitPossible = true;
            ppu._spriteZeroBeingRendered = true;
            ppu._cycle = 10;
            ppu._scanline = 20;

            ppu._bgShifterPatternLo = 0x8000;
            ppu._bgShifterPatternHi = 0x0000;
            ppu._bgShifterAttribLo = 0x8000;
            ppu._bgShifterAttribHi = 0x0000;
            ppu._fineX = 0;

            ppu._spriteShiftersPatternLo[0] = 0x80;
            ppu._spriteShiftersPatternHi[0] = 0x00;
            ppu._spriteScanline[0].Attribute = 0x00;
            ppu._spriteScanline[0].X = 0;
            ppu._spriteCount = 1;

            // Act
            ppu.ComposePixel();

            // Assert
            Assert.True((ppu.Status & (byte)Ppu2c02.PpuStatusFlags.SpriteZeroHit) != 0);
        }
    }
}
