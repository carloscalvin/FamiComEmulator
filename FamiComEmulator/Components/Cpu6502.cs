using static FamiComEmulator.Components.ICpu6502;

namespace FamiComEmulator.Components
{
    internal class Cpu6502 : ICpu6502
    {
        public byte Accumulator { get; set; }

        public byte XRegister { get; set; }

        public byte YRegister { get; set; }

        public byte StackPointer { get; set; }

        public short ProgramCounter { get; set; }

        public Flags6502 Status { get; set; }

        public byte[,] Instructions { get; set; } = new byte[16, 16];


        private CentralBus _bus;

        public void AddCentralBus(CentralBus bus)
        {
            _bus = bus;
        }

        public void Write(short address, byte data)
        {
            _bus.Write(address, data);
        }

        public byte Read(short address)
        {
            return _bus.Read(address);
        }

        private int GetFlag(Flags6502 flag)
        {
            return Status.HasFlag(flag) ? 1 : 0;
        }

        void SetFlag(Flags6502 flag, bool overflow)
        {
            if (overflow)
                Status |= flag;
            else
                Status &= ~flag;
        }
    }
}
