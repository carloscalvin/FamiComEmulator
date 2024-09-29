namespace FamiComEmulator.Components
{
    public class Ppu2c02 : IPpu2c02
    {
        #region public properties

        // PPU Registers
        public byte Control { get; set; }
        public byte Mask { get; set; }
        public byte Status { get; private set; }
        public byte OamAddress { get; set; }
        public byte OamData { get; set; }
        public byte Scroll { get; set; }
        public byte PpuAddress { get; set; }
        public byte PpuData { get; set; }
        public int PpuX { get; private set; }
        public int PpuY { get; private set; }

        #endregion

        #region private properties

        // Reference to central bus
        private ICentralBus _bus;

        // PPU Memory
        private byte[] _patternTables = new byte[0x2000]; // 8KB
        private byte[] _nameTables = new byte[0x1000];    // 4KB
        private byte[] _palettes = new byte[0x20];        // 32 bytes
        private byte[] _oam = new byte[0x100];            // Object Attribute Memory

        // Internal state variables
        private int _cycle;
        private int _scanline;
        private bool _vblank;
        private bool _nmiOccurred;

        #endregion

        #region public methods

        public void AddCentralBus(CentralBus bus)
        {
            _bus = bus;
        }

        public void Clock()
        {
            _cycle++;
            PpuX = _cycle;
            if (_cycle >= 341)
            {
                _cycle = 0;
                _scanline++;
                if (_scanline >= 262)
                {
                    _scanline = 0;
                }
                if (_scanline == 241 && _cycle == 1)
                {
                    Status |= 0x80;
                    _vblank = true;
                    _bus.Cpu.Nmi();
                }

                if (_scanline == 261 && _cycle == 1)
                {
                    Status &= 0x7F;
                    _vblank = false;
                }

                PpuY = _scanline;
            }

            if (_scanline >= 0 && _scanline < 240)
            {
                RenderPixel();
            }
        }

        public void Reset()
        {
            Control = 0;
            Mask = 0;
            Status = 0;
            OamAddress = 0;
            OamData = 0;
            Scroll = 0;
            PpuAddress = 0;
            PpuData = 0;

            _cycle = 21;
            _scanline = 0;
            PpuX = _cycle;
            PpuY = _scanline;
            _vblank = false;
            _nmiOccurred = false;

            Array.Clear(_patternTables, 0, _patternTables.Length);
            Array.Clear(_nameTables, 0, _nameTables.Length);
            Array.Clear(_palettes, 0, _palettes.Length);
            Array.Clear(_oam, 0, _oam.Length);
        }

        public void WriteRegister(byte register, byte data)
        {
            switch (register)
            {
                case 0x00: // PPUCTRL
                    Control = data;
                    break;
                case 0x01: // PPUMASK
                    Mask = data;
                    break;
                case 0x03: // OAMADDR
                    OamAddress = data;
                    break;
                case 0x04: // OAMDATA
                    OamData = data;
                    break;
                case 0x05: // PPUSCROLL
                    Scroll = data;
                    break;
                case 0x06: // PPUADDR
                    PpuAddress = (byte)((data << 8) | (PpuAddress & 0x00FF));
                    break;
                case 0x07: // PPUDATA
                    WritePpuMemory(PpuAddress, data);
                    PpuAddress++;
                    break;
                default:
                    break;
            }
        }

        public byte ReadRegister(byte register)
        {
            switch (register)
            {
                case 0x02: // PPUSTATUS
                    byte status = Status;
                    Status &= 0x7F; // Clear VBlank flag after reading
                    return status;
                case 0x04: // OAMDATA
                    return OamData;
                case 0x07: // PPUDATA
                    return ReadPpuMemory(PpuAddress);
                default:
                    return 0;
            }
        }

        #endregion

        #region private methods

        private void WritePpuMemory(ushort address, byte data)
        {
            // Map PPU address space
            if (address <= 0x1FFF)
            {
                _patternTables[address & 0x1FFF] = data;
            }
            else if (address <= 0x2FFF)
            {
                _nameTables[address & 0x0FFF] = data;
            }
            else if (address <= 0x3EFF)
            {
                _palettes[address & 0x1F] = data;
            }
            // Mirroring of palette data
            else if (address <= 0x3FFF)
            {
                _palettes[address & 0x1F] = data;
            }
        }

        private byte ReadPpuMemory(ushort address)
        {
            if (address <= 0x1FFF)
            {
                return _patternTables[address & 0x1FFF];
            }
            else if (address <= 0x2FFF)
            {
                return _nameTables[address & 0x0FFF];
            }
            else if (address <= 0x3EFF)
            {
                return _palettes[address & 0x1F];
            }
            else if (address <= 0x3FFF)
            {
                return _palettes[address & 0x1F];
            }

            return 0x00;
        }

        private void RenderPixel()
        {
        }

        #endregion
    }
}
