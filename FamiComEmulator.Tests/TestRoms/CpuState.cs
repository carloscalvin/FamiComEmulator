namespace FamiComEmulator.Tests.TestRoms
{
    public class CpuState
    {
        public ushort Address { get; set; }
        public string Opcode { get; set; }
        public string Instruction { get; set; }
        public byte Accumulator { get; set; }
        public byte XRegister { get; set; }
        public byte YRegister { get; set; }
        public byte Status { get; set; }
        public byte StackPointer { get; set; }
        public int PpuX { get; set; }
        public int PpuY { get; set; }
        public int Cycle { get; set; }
    }
}
