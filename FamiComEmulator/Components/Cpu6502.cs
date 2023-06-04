using static FamiComEmulator.Components.ICpu6502;

namespace FamiComEmulator.Components
{
    internal class Cpu6502 : ICpu6502
    {
        public byte Accumulator { get; set; }

        public byte XRegister { get; set; }

        public byte YRegister { get; set; }

        public byte StackPointer { get; set; }

        public ushort ProgramCounter { get; set; }

        public Flags6502 Status { get; set; }

        public byte[,] Instructions { get; set; } = new byte[16, 16];


        private CentralBus _bus;

        private ushort _address_abs = 0x0000;

        private ushort _address_rel = 0x0000;

        public void AddCentralBus(CentralBus bus)
        {
            _bus = bus;
        }

        public void Write(ushort address, byte data)
        {
            _bus.Write(address, data);
        }

        public byte Read(ushort address)
        {
            return _bus.Read(address);
        }

        private int GetFlag(Flags6502 flag)
        {
            return Status.HasFlag(flag) ? 1 : 0;
        }

        private void SetFlag(Flags6502 flag, bool overflow)
        {
            if (overflow)
                Status |= flag;
            else
                Status &= ~flag;
        }

        private byte IMP()
        {
            return 0;
        }

        private byte IMM()
        {
            _address_abs = ProgramCounter++;
            return 0;
        }

        private byte ZP0()
        {
            _address_abs = Read(ProgramCounter);
            ProgramCounter++;
            _address_abs &= 0x00FF;

            return 0;
        }

        private byte ZPX()
        {
            _address_abs = (ushort)(Read(ProgramCounter) + XRegister);
            ProgramCounter++;
            _address_abs &= 0x00FF;

            return 0;
        }

        private byte ZPY()
        {
            _address_abs = (ushort)(Read(ProgramCounter) + YRegister);
            ProgramCounter++;
            _address_abs &= 0x00FF;

            return 0;
        }

        private byte REL()
        {
            _address_rel = Read(ProgramCounter);
            ProgramCounter++;
            if ((_address_rel & 0x80) != 0)
                _address_rel |= 0xFF00;

            return 0;
        }

        private byte ABS()
        {
            ushort lowByte = Read(ProgramCounter);
            ProgramCounter++;
            ushort highByte = Read(ProgramCounter);
            ProgramCounter++;

            _address_abs = (ushort)((highByte << 8) | lowByte);

            return 0;
        }

        private byte ABX()
        {
            ushort lowByte = Read(ProgramCounter);
            ProgramCounter++;
            ushort highByte = Read(ProgramCounter);
            ProgramCounter++;

            _address_abs = (ushort)((highByte << 8) | lowByte);

            _address_abs += XRegister;

            if ((_address_abs & 0xFF00) != (highByte << 8))
                return 1;
            else
                return 0;
        }

        private byte ABY()
        {
            ushort lowByte = Read(ProgramCounter);
            ProgramCounter++;
            ushort highByte = Read(ProgramCounter);
            ProgramCounter++;

            _address_abs = (ushort)((highByte << 8) | lowByte);

            _address_abs += YRegister;

            if ((_address_abs & 0xFF00) != (highByte << 8))
                return 1;
            else
                return 0;
        }

        private byte IND()
        {
            ushort lowPointer = Read(ProgramCounter);
            ProgramCounter++;
            ushort highPointer = Read(ProgramCounter);
            ProgramCounter++;

            ushort pointer = (ushort)((highPointer << 8) | lowPointer);

            if (lowPointer == 0x00FF)
            {
                _address_abs = (ushort)(Read((ushort)((pointer & 0xFF00) << 8)) | Read((ushort)(pointer + 0)));
            }
            else
            {
                _address_abs = (ushort)(Read((ushort)((pointer + 1) << 8)) | Read((ushort)(pointer + 0)));
            }

            return 0;
        }

        private byte IZX()
        {
            ushort basePointer = Read(ProgramCounter);
            ProgramCounter++;

            ushort lowByte = Read((ushort)((ushort)(basePointer + XRegister) & 0x00FF));
            ushort highByte = Read((ushort)((ushort)(basePointer + XRegister + 1) & 0x00FF));

            _address_abs = (ushort)((highByte << 8) | lowByte);

            return 0;
        }

        private byte IZY()
        {
            ushort basePointer = Read(ProgramCounter);
            ProgramCounter++;

            ushort lowByte = Read((ushort)(basePointer & 0x00FF));
            ushort highByte = Read((ushort)((basePointer + 1) & 0x00FF));

            _address_abs = (ushort)((highByte << 8) | lowByte);
            _address_abs += YRegister;

            if ((_address_abs & 0xFF00) != (highByte << 8))
                return 1;
            else
                return 0;
        }
    }
}
