namespace FamiComEmulator.Components
{
    internal class Cpu6502 : ICpu6502
    {
        public byte Accumulator { get; set; }

        public byte XRegister { get; set; }

        public byte YRegister { get; set; }

        public byte StackPointer { get; set; }

        public short ProgramCounter { get; set; }

        public byte[,] Instructions { get; set; } = new byte[16, 16];

        public struct StatusRegister
        {
            public bool Z;
            public bool C;
            public bool I;
        }

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

        private CentralBus _bus;
    }
}
