namespace FamiComEmulator.Components
{
    public class CentralBus : ICentralBus
    {
        private ICartridge? _cartridge;

        public CentralBus(ICpu6502 cpu)
        {
            Cpu = cpu;
            Ram = new Ram();
            Cpu.AddCentralBus(this);
        }

        public ICpu6502 Cpu { get; set; }

        public Ram Ram { get; set; }

        public void AddCartridge(ICartridge cartridge)
        {
            _cartridge = cartridge;
        }

        public void Clock()
        {
            Cpu?.Clock();
        }
        public byte Read(ushort address)
        {
            byte data = 0x00;
            if (_cartridge == null || !_cartridge.Read(address, ref data))
            {
                if (address >= 0x0000 && address <= 0x1FF)
                {
                    data = Ram.Memory[address];
                }
            }

            return data;
        }

        public void Reset()
        {
            Cpu?.Reset();
        }

        public void Write(ushort address, byte data)
        {
            if(_cartridge == null || !_cartridge.Write(address, data))
            {
                if(address >= 0x0000 && address <= 0x1FF)
                {
                    Ram.Memory[address] = data;
                }
            }
        }
    }
}
