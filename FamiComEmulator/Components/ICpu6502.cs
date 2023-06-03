namespace FamiComEmulator.Components
{
    internal interface ICpu6502
    {
        byte Accumulator { get; set; }

        byte[,] Instructions { get; set; }

        short ProgramCounter { get; set; }

        byte StackPointer { get; set; }

        byte XRegister { get; set; }

        byte YRegister { get; set; }

        void AddCentralBus(CentralBus bus);

        void Write(short address, byte data);

        byte Read(short address);
    }
}
