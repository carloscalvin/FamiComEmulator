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
        }

        [Fact]
        public void TestLDAInstructionExecution()
        {
            //Arrange
            _bus.Write(0xFFFC, 0x00);
            _bus.Write(0xFFFD, 0x20);
            _bus.Write(0x2000, 0xA9); // LDA immediate
            _bus.Write(0x2001, 0x42); // Value to load into Accumulator

            _bus.Cpu.Reset();
            while (!_bus.Cpu.Finish())
            {
                _bus.Cpu.Clock();
            }

            // Act
            for (int i = 0; i < 2; i++)
            {
                _bus.Cpu.Clock();
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
            _bus.Write(0xFFFD, 0x20);
            _bus.Write(0x2000, 0xA9); // LDA immediate
            _bus.Write(0x2001, 0x10); // Value to load into Accumulator

            _bus.Write(0x2002, 0x69); // ADC immediate
            _bus.Write(0x2003, 0x05); // Value to add to Accumulator

            _bus.Cpu.Reset();
            while (!_bus.Cpu.Finish())
            {
                _bus.Cpu.Clock();
            }

            // Act
            for (int i = 0; i < 4; i++)
            {
                _bus.Cpu.Clock();
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
    }
}
