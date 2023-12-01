using FamiComEmulator.Components;
using static FamiComEmulator.Components.ICpu6502;

namespace FamiComEmulator.Tests.Components
{
    public class Cpu6502Tests
    {
        private ICentralBus _bus;

        private ICpu6502 _cpu6502;

        public Cpu6502Tests()
        {
            _cpu6502 = new Cpu6502();
            _bus = new CentralBus(_cpu6502);
            _bus.AddCartridge(new Cartridge("Components/nestest.nes"));
        }

        [Fact]
        public void TestLDAInstructionExecution()
        {
            //Arrange
            _bus.Write(0xFFFC, 0x00);
            _bus.Write(0xFFFD, 0x80);
            _bus.Write(0x8000, 0xA9); // LDA immediate
            _bus.Write(0x8001, 0x42); // Value to load into Accumulator

            _bus.Reset();
            while (!_bus.Cpu.Finish())
            {
                _bus.Clock();
            }

            // Act
            for (int i = 0; i < 2; i++)
            {
                _bus.Clock();
            }

            // Assert
            Assert.Equal(0x42, _bus.Cpu.Accumulator);
            Assert.True(_bus.Cpu.GetFlag(Flags6502.Zero) == 0);
            Assert.False(_bus.Cpu.GetFlag(Flags6502.Negative) == 1);
        }

        [Fact]
        public void TestADCInstructionExecution()
        {
            // Arrange
            _bus.Write(0xFFFC, 0x00);
            _bus.Write(0xFFFD, 0x80);
            _bus.Write(0x8000, 0xA9); // LDA immediate
            _bus.Write(0x8001, 0x10); // Value to load into Accumulator

            _bus.Write(0x8002, 0x69); // ADC immediate
            _bus.Write(0x8003, 0x05); // Value to add to Accumulator

            _bus.Reset();
            while (!_bus.Cpu.Finish())
            {
                _bus.Clock();
            }

            // Act
            for (int i = 0; i < 4; i++)
            {
                _bus.Clock();
            }

            // Assert
            int carryFlag = _bus.Cpu.GetFlag(Flags6502.CarryBit);
            int zeroFlag = _bus.Cpu.GetFlag(Flags6502.Zero);
            int overflowFlag = _bus.Cpu.GetFlag(Flags6502.Overflow);
            int negativeFlag = _bus.Cpu.GetFlag(Flags6502.Negative);

            Assert.Equal(0x15, _bus.Cpu.Accumulator);
            Assert.Equal(0, carryFlag);
            Assert.Equal(0, zeroFlag);
            Assert.Equal(0, overflowFlag);
            Assert.Equal(0, negativeFlag);
        }

        [Fact]
        public void TestMultiplicationProgramExecution()
        {
            // Arrange
            _bus.Write(0xFFFC, 0x00);
            _bus.Write(0xFFFD, 0x80);
            _bus.Write(0x8000, 0xA2); // LDX #10
            _bus.Write(0x8001, 0x0A);
            _bus.Write(0x8002, 0x8E); // STX $0000
            _bus.Write(0x8003, 0x00);
            _bus.Write(0x8004, 0x00);
            _bus.Write(0x8005, 0xA2); // LDX #3
            _bus.Write(0x8006, 0x03);
            _bus.Write(0x8007, 0x8E); // STX $0001
            _bus.Write(0x8008, 0x01);
            _bus.Write(0x8009, 0x00);
            _bus.Write(0x800A, 0xAC); // LDY $0000
            _bus.Write(0x800B, 0x00);
            _bus.Write(0x800C, 0x00);
            _bus.Write(0x800D, 0xA9); // LDA #0
            _bus.Write(0x800E, 0x00);
            _bus.Write(0x800F, 0x18); // CLC
            _bus.Write(0x8010, 0x6D); // ADC $0001
            _bus.Write(0x8011, 0x01);
            _bus.Write(0x8012, 0x00);
            _bus.Write(0x8013, 0x88); // DEY
            _bus.Write(0x8014, 0xD0); // BNE loop
            _bus.Write(0x8015, 0xFA);
            _bus.Write(0x8016, 0x8D); // STA $0002
            _bus.Write(0x8017, 0x02);
            _bus.Write(0x8018, 0x00);
            _bus.Write(0x8019, 0xEA); // NOP
            _bus.Write(0x801A, 0xEA); // NOP
            _bus.Write(0x801B, 0xEA); // NOP

            _bus.Reset();
            while (!_bus.Cpu.Finish())
            {
                _bus.Clock();
            }

            // Act
            for (int i = 0; i < 112; i++)
            {
                _bus.Clock();
            }

            // Assert
            int xRegister = _bus.Cpu.XRegister;
            int yRegister = _bus.Cpu.YRegister;
            int accumulator = _bus.Cpu.Accumulator;
            int carryFlag = _bus.Cpu.GetFlag(Flags6502.CarryBit);
            int zeroFlag = _bus.Cpu.GetFlag(Flags6502.Zero);
            int overflowFlag = _bus.Cpu.GetFlag(Flags6502.Overflow);
            int negativeFlag = _bus.Cpu.GetFlag(Flags6502.Negative);

            Assert.Equal(3, xRegister);
            Assert.Equal(0, yRegister);
            Assert.Equal(30, accumulator);
            Assert.Equal(0, carryFlag);
            Assert.Equal(1, zeroFlag);
            Assert.Equal(0, overflowFlag);
            Assert.Equal(0, negativeFlag);

            int multiplying = _bus.Read(0x0000);
            int multiplier = _bus.Read(0x0001);
            int product = _bus.Read(0x0002);

            Assert.Equal(10, multiplying);
            Assert.Equal(3, multiplier);
            Assert.Equal(30, product);
        }

        [Fact]
        public void NesTestRom()
        {
            // Arrange
            _bus.Write(0xFFFC, 0x00);
            _bus.Write(0xFFFD, 0xC0);

            _bus.Reset();
            while (!_bus.Cpu.Finish())
            {
                _bus.Clock();
            }

            // Act
            for (int i = 0; i < 112; i++)
            {
                _bus.Clock();
            }

            // Assert
            int xRegister = _bus.Cpu.XRegister;
            int yRegister = _bus.Cpu.YRegister;
            int accumulator = _bus.Cpu.Accumulator;
            int carryFlag = _bus.Cpu.GetFlag(Flags6502.CarryBit);
            int zeroFlag = _bus.Cpu.GetFlag(Flags6502.Zero);
            int overflowFlag = _bus.Cpu.GetFlag(Flags6502.Overflow);
            int negativeFlag = _bus.Cpu.GetFlag(Flags6502.Negative);

            Assert.Equal(3, xRegister);
            Assert.Equal(0, yRegister);
            Assert.Equal(30, accumulator);
            Assert.Equal(0, carryFlag);
            Assert.Equal(1, zeroFlag);
            Assert.Equal(0, overflowFlag);
            Assert.Equal(0, negativeFlag);
        }
    }
}
