namespace FamiComEmulator.Components
{
    internal class CentralBus : ICentralBus
    {
        public CentralBus()
        {
            Cpu.AddCentralBus(this);
        }

        public Cpu6502 Cpu { get; set; } = new Cpu6502();

        public Ram Ram { get; set; } = new Ram();

        public byte Read(short address)
        {
            return Ram.Memory[address];
        }

        public void Write(short address, byte data)
        {
            Ram.Memory[address] = data;
        }
    }
}
