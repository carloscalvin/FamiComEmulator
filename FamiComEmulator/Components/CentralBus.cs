namespace FamiComEmulator.Components
{
    public class CentralBus : ICentralBus
    {
        public CentralBus(ICpu6502 cpu)
        {
            Cpu = cpu;
            Ram = new Ram();
            Cpu.AddCentralBus(this);
        }

        public ICpu6502 Cpu { get; set; }

        public Ram Ram { get; set; }

        public void Clock()
        {
            Cpu?.Clock();
        }

        public byte Read(ushort address)
        {
            return Ram.Memory[address];
        }

        public void Reset()
        {
            Cpu?.Reset();
        }

        public void Write(ushort address, byte data)
        {
            Ram.Memory[address] = data;
        }
    }
}
