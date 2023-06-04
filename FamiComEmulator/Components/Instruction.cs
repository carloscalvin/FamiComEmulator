namespace FamiComEmulator.Components
{
    internal delegate byte AddressingModeDelegate();

    internal delegate byte OpcodeDelegate();

    internal class Instruction
    {
        public string Name { get; set; }

        public AddressingModeDelegate AddressingMode { get; set; }

        public OpcodeDelegate Opcode { get; set; }

        public int Cycles { get; set; }
    }
}
