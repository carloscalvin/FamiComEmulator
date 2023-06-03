namespace FamiComEmulator.Components
{
    internal interface ICpu6502
    {
        byte Accumulator { get; set; }

        short ProgramCounter { get; set; }

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

        void AddCentralBus(CentralBus bus);

        void Write(short address, byte data);

        byte Read(short address);
    }
}
