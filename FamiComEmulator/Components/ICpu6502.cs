namespace FamiComEmulator.Components
{
    public interface ICpu6502
    {
        byte Accumulator { get; set; }

        ushort ProgramCounter { get; set; }

        byte StackPointer { get; set; }

        byte XRegister { get; set; }

        byte YRegister { get; set; }

        Flags6502 Status { get; set; }

        [Flags]
        enum Flags6502
        {
            CarryBit = (1 << 0),
            Zero = (1 << 1),
            DisableInterrupts = (1 << 2),
            DecimalMode = (1 << 3),
            Break = (1 << 4),
            Unused = (1 << 5),
            Overflow = (1 << 6),
            Negative = (1 << 7),
        };

        int GetFlag(Flags6502 flag);

        void Reset();

        void Irq(); 

        void Nmi();

        void Clock();

        void AddCentralBus(CentralBus bus);

        void Write(ushort address, byte data);

        byte Read(ushort address);

        bool Finish();
    }
}
