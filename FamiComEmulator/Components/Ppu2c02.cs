using System.Drawing;

namespace FamiComEmulator.Components
{
    public class Ppu2c02 : IPpu2c02
    {
        #region public properties

        // PPU Registers
        public byte Control { get; set; }
        public byte Mask { get; set; }
        public byte Status { get; set; }
        public byte OamAddress { get; set; }
        public byte OamData { get; set; }
        public byte ScrollX { get; private set; }
        public byte ScrollY { get; private set; }
        public ushort PpuAddress { get; set; }
        public byte PpuData { get; set; }
        public int PpuX { get; set; }
        public int PpuY { get; set; }

        #endregion

        #region private properties

        private bool _grayscale;
        private LoopyRegister _vramAddress;
        private LoopyRegister _tramAddress;
        private IPpuRenderer _sprScreen;
        private bool _renderBackground;
        private bool _renderSprites;
        private int _fineX;

        private SpriteAttributeEntry[] _spriteScanline = new SpriteAttributeEntry[8];
        private int _spriteCount;
        private bool _spriteZeroHitPossible;
        private bool _spriteZeroBeingRendered;

        // Reference to central bus
        private ICentralBus _bus;

        // PPU Memory
        private byte[][] _patternTables = new byte[2][] { new byte[0x1000], new byte[0x1000] };
        private byte[][] _nameTables = new byte[2][]
        {
            new byte[0x0400], // Name Table 0
            new byte[0x0400]  // Name Table 1
        };
        private byte[] _palettes = new byte[0x20];        // 32 bytes
        internal byte[] _oam = new byte[0x100];            // Object Attribute Memory

        // Internal state variables
        private int _cycle;
        private int _scanline;
        private bool _vblank;
        private bool _nmiOccurred;
        private byte _writeToggle = 0;
        private bool _ppuAddrToggle = false;
        private byte _ppuDataBuffer = 0;

        // Background shifters
        internal ushort _bgShifterPatternLo;
        internal ushort _bgShifterPatternHi;
        internal ushort _bgShifterAttribLo;
        internal ushort _bgShifterAttribHi;

        // Sprite shifters
        private ushort[] _spriteShiftersPatternLo = new ushort[8];
        private ushort[] _spriteShiftersPatternHi = new ushort[8];

        #endregion

        #region constructor

        public Ppu2c02(IPpuRenderer sprScreen)
        {
            _sprScreen = sprScreen;
            _vramAddress = new LoopyRegister();
            _tramAddress = new LoopyRegister();
        }

        #endregion

        #region palette

        private readonly Color[] NesPalette = new Color[64]
        {
            // 0x00 - 0x0F
            Color.FromArgb(84, 84, 84),    // 0x00
            Color.FromArgb(0, 30, 116),    // 0x01
            Color.FromArgb(8, 16, 144),     // 0x02
            Color.FromArgb(48, 0, 136),     // 0x03
            Color.FromArgb(68, 0, 100),     // 0x04
            Color.FromArgb(92, 0, 48),      // 0x05
            Color.FromArgb(84, 4, 0),       // 0x06
            Color.FromArgb(60, 24, 0),      // 0x07
            Color.FromArgb(32, 42, 0),      // 0x08
            Color.FromArgb(8, 58, 0),       // 0x09
            Color.FromArgb(0, 64, 0),       // 0x0A
            Color.FromArgb(0, 60, 0),       // 0x0B
            Color.FromArgb(0, 50, 60),      // 0x0C
            Color.FromArgb(0, 0, 0),        // 0x0D
            Color.FromArgb(0, 0, 0),        // 0x0E
            Color.FromArgb(0, 0, 0),        // 0x0F

            // 0x10 - 0x1F
            Color.FromArgb(152, 150, 152),  // 0x10
            Color.FromArgb(8, 76, 196),      // 0x11
            Color.FromArgb(48, 50, 236),     // 0x12
            Color.FromArgb(92, 30, 228),     // 0x13
            Color.FromArgb(136, 20, 176),    // 0x14
            Color.FromArgb(160, 20, 100),    // 0x15
            Color.FromArgb(152, 34, 32),     // 0x16
            Color.FromArgb(120, 60, 0),      // 0x17
            Color.FromArgb(84, 90, 0),       // 0x18
            Color.FromArgb(40, 114, 0),      // 0x19
            Color.FromArgb(8, 124, 0),       // 0x1A
            Color.FromArgb(0, 118, 40),      // 0x1B
            Color.FromArgb(0, 102, 120),     // 0x1C
            Color.FromArgb(0, 0, 0),         // 0x1D
            Color.FromArgb(0, 0, 0),         // 0x1E
            Color.FromArgb(0, 0, 0),         // 0x1F

            // 0x20 - 0x2F
            Color.FromArgb(236, 238, 236),  // 0x20
            Color.FromArgb(76, 154, 236),   // 0x21
            Color.FromArgb(120, 124, 236),  // 0x22
            Color.FromArgb(176, 98, 236),   // 0x23
            Color.FromArgb(228, 84, 236),   // 0x24
            Color.FromArgb(236, 88, 180),   // 0x25
            Color.FromArgb(236, 106, 100),  // 0x26
            Color.FromArgb(212, 136, 32),   // 0x27
            Color.FromArgb(160, 170, 0),    // 0x28
            Color.FromArgb(116, 196, 0),    // 0x29
            Color.FromArgb(76, 208, 32),    // 0x2A
            Color.FromArgb(56, 204, 108),   // 0x2B
            Color.FromArgb(56, 180, 204),   // 0x2C
            Color.FromArgb(60, 60, 60),     // 0x2D
            Color.FromArgb(0, 0, 0),        // 0x2E
            Color.FromArgb(0, 0, 0),        // 0x2F

            // 0x30 - 0x3F
            Color.FromArgb(236, 238, 236),  // 0x30
            Color.FromArgb(168, 204, 236),  // 0x31
            Color.FromArgb(188, 188, 236),  // 0x32
            Color.FromArgb(212, 178, 236),  // 0x33
            Color.FromArgb(236, 174, 236),  // 0x34
            Color.FromArgb(236, 174, 212),  // 0x35
            Color.FromArgb(236, 180, 176),  // 0x36
            Color.FromArgb(228, 196, 144),  // 0x37
            Color.FromArgb(204, 210, 120),  // 0x38
            Color.FromArgb(180, 222, 120),  // 0x39
            Color.FromArgb(168, 226, 144),  // 0x3A
            Color.FromArgb(152, 226, 180),  // 0x3B
            Color.FromArgb(160, 214, 228),  // 0x3C
            Color.FromArgb(160, 162, 160),  // 0x3D
            Color.FromArgb(0, 0, 0),        // 0x3E
            Color.FromArgb(0, 0, 0)         // 0x3F
        };

        #endregion

        #region public methods

        public void AddCentralBus(CentralBus bus)
        {
            _bus = bus;
        }

        public void Clock()
        {
            PpuX++;
            _cycle = PpuX;
            _scanline = PpuY;

            if (_scanline == 241 && _cycle == 1)
            {
                Status |= (byte)PpuStatusFlags.VerticalBlank;
                _vblank = true;
                if ((Control & (byte)PpuControlFlags.EnableNmi) != 0)
                {
                    _bus.Cpu.Nmi();
                    _nmiOccurred = true;
                }
            }

            if (_scanline == 261 && _cycle == 1)
            {
                Status &= 0x7F; // Clear VBlank flag
                _vblank = false;
            }

            if (_cycle >= 341)
            {
                _cycle = PpuX = 0;
                _scanline++;
                PpuY++;

                if (_scanline >= 262)
                {
                    _scanline = 0;
                    PpuY = 0;
                }

                if (_scanline >= 0 && _scanline < 240)
                {
                    EvaluateSprites();
                    //LoadBackgroundShifters();
                    //LoadSpriteShifters();
                }
            }

            if (_scanline >= 0 && _scanline < 240)
            {
                if (_cycle >= 1 && _cycle <= 256)
                {
                    UpdateShifters();
                    ComposePixel();
                }
            }
        }

        public void Reset()
        {
            Control = 0;
            Mask = 0;
            Status = 0;
            OamAddress = 0;
            OamData = 0;
            ScrollX = 0;
            ScrollY = 0;
            PpuAddress = 0;
            PpuData = 0;

            _cycle = 0;
            _scanline = 0;
            PpuX = _cycle;
            PpuY = _scanline;
            _vblank = false;
            _nmiOccurred = false;

            for (int i = 0; i < _patternTables.Length; i++)
            {
                if (_patternTables[i] != null)
                {
                    Array.Clear(_patternTables[i], 0, _patternTables[i].Length);
                }
            }

            for (int i = 0; i < _nameTables.Length; i++)
            {
                if (_nameTables[i] != null)
                {
                    Array.Clear(_nameTables[i], 0, _nameTables[i].Length);
                }
            }

            Array.Clear(_palettes, 0, _palettes.Length);
            Array.Clear(_oam, 0, _oam.Length);

            _vramAddress.Reset();
            _tramAddress.Reset();

            _bgShifterPatternLo = 0;
            _bgShifterPatternHi = 0;
            _bgShifterAttribLo = 0;
            _bgShifterAttribHi = 0;

            Array.Clear(_spriteShiftersPatternLo, 0, _spriteShiftersPatternLo.Length);
            Array.Clear(_spriteShiftersPatternHi, 0, _spriteShiftersPatternHi.Length);
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
                    UpdateRenderFlags();
                    UpdateGrayscaleFlag();
                    break;
                case 0x03: // OAMADDR
                    OamAddress = data;
                    break;
                case 0x04: // OAMDATA
                    OamData = data;
                    break;
                case 0x05: // PPUSCROLL
                    if (_writeToggle == 0)
                    {
                        ScrollX = data;
                        _writeToggle = 1;
                    }
                    else
                    {
                        ScrollY = data;
                        _writeToggle = 0;
                    }
                    break;
                case 0x06: // PPUADDR
                    if (!_ppuAddrToggle)
                    {
                        PpuAddress = (ushort)((data << 8) | (PpuAddress & 0x00FF));
                        _ppuAddrToggle = true;
                    }
                    else
                    {
                        PpuAddress = (ushort)((PpuAddress & 0xFF00) | data);
                        _ppuAddrToggle = false;
                    }
                    break;
                case 0x07: // PPUDATA
                    WritePpuMemory(PpuAddress, data);
                    IncrementPpuAddress();
                    break;
                case 0x14:
                    PerformOamDma(data);
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
                    _ppuAddrToggle = false; // Reset toggle
                    _writeToggle = 0; // Reset scroll toggle
                    return status;
                case 0x04: // OAMDATA
                    return OamData;
                case 0x07: // PPUDATA
                    byte data;
                    if (PpuAddress <= 0x3EFF)
                    {
                        data = ReadPpuMemory(PpuAddress);
                    }
                    else
                    {
                        data = _palettes[PpuAddress & 0x1F];
                    }

                    byte bufferedData = _ppuDataBuffer;
                    _ppuDataBuffer = data;
                    IncrementPpuAddress();
                    return bufferedData;
                default:
                    return 0;
            }
        }

        public void PerformOamDma(byte page)
        {
            ushort address = (ushort)(page << 8);
            for (int i = 0; i < 256; i++)
            {
                byte data = _bus.Read(address++);
                _oam[i] = data;
            }
        }

        #endregion

        #region private methods

        private byte ReadPpuMemory(ushort address)
        {
            byte data = 0x00;
            address &= 0x3FFF;

            if (_bus.Cartridge.Read(address, ref data))
            {
            }
            else if (address >= 0x0000 && address <= 0x1FFF)
            {
                data = _patternTables[(address & 0x1000) >> 12][address & 0x0FFF];
            }
            else if (address >= 0x2000 && address <= 0x3EFF)
            {
                address &= 0x0FFF;

                switch (_bus.Cartridge.Mirror)
                {
                    case Mirror.Vertical:
                        if (address < 0x0400)
                            data = _nameTables[0][address & 0x03FF];
                        else if (address < 0x0800)
                            data = _nameTables[1][address & 0x03FF];
                        else if (address < 0x0C00)
                            data = _nameTables[0][address & 0x03FF];
                        else
                            data = _nameTables[1][address & 0x03FF];
                        break;
                    case Mirror.Horizontal:
                        if (address < 0x0800)
                            data = _nameTables[0][address & 0x03FF];
                        else
                            data = _nameTables[1][address & 0x03FF];
                        break;
                    case Mirror.FourScreen:
                        break;
                    default:
                        break;
                }
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                address &= 0x001F;
                if (address == 0x0010) address = 0x0000;
                if (address == 0x0014) address = 0x0004;
                if (address == 0x0018) address = 0x0008;
                if (address == 0x001C) address = 0x000C;
                data = (byte)(_palettes[address] & (_grayscale ? 0x30 : 0x3F));
            }

            return data;
        }

        private void WritePpuMemory(ushort address, byte data)
        {
            address &= 0x3FFF;

            if (_bus.Cartridge.Write(address, data))
            {
            }
            else if (address >= 0x0000 && address <= 0x1FFF)
            {
                _patternTables[(address & 0x1000) >> 12][address & 0x0FFF] = data;
            }
            else if (address >= 0x2000 && address <= 0x3EFF)
            {
                address &= 0x0FFF;

                switch (_bus.Cartridge.Mirror)
                {
                    case Mirror.Vertical:
                        if (address < 0x0400)
                            _nameTables[0][address & 0x03FF] = data;
                        else if (address < 0x0800)
                            _nameTables[1][address & 0x03FF] = data;
                        else if (address < 0x0C00)
                            _nameTables[0][address & 0x03FF] = data;
                        else
                            _nameTables[1][address & 0x03FF] = data;
                        break;
                    case Mirror.Horizontal:
                        if (address < 0x0800)
                            _nameTables[0][address & 0x03FF] = data;
                        else
                            _nameTables[1][address & 0x03FF] = data;
                        break;
                    case Mirror.FourScreen:
                        break;
                    default:
                        break;
                }
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                address &= 0x001F;
                if (address == 0x0010) address = 0x0000;
                if (address == 0x0014) address = 0x0004;
                if (address == 0x0018) address = 0x0008;
                if (address == 0x001C) address = 0x000C;
                _palettes[address] = data;
            }
        }

        private byte GetNameTable(ushort address)
        {
            switch (_bus.Cartridge.Mirror)
            {
                case Mirror.Vertical:
                    if (address < 0x0400)
                        return _nameTables[0][address & 0x03FF];
                    else if (address < 0x0800)
                        return _nameTables[1][address & 0x03FF];
                    else if (address < 0x0C00)
                        return _nameTables[0][address & 0x03FF];
                    else
                        return _nameTables[1][address & 0x03FF];
                case Mirror.Horizontal:
                    if (address < 0x0800)
                        return _nameTables[0][address & 0x03FF];
                    else
                        return _nameTables[1][address & 0x03FF];
                case Mirror.FourScreen:
                    return 0;
                default:
                    return 0;
            }
        }

        private void RenderBackground()
        {
            ushort bitMask = (ushort)(0x8000 >> _fineX);

            bool bgBit0 = (_bgShifterPatternLo & bitMask) != 0;
            bool bgBit1 = (_bgShifterPatternHi & bitMask) != 0;
            byte bgPixel = (byte)((bgBit1 ? 1 : 0) << 1 | (bgBit0 ? 1 : 0));

            bool bgPal0 = (_bgShifterAttribLo & bitMask) != 0;
            bool bgPal1 = (_bgShifterAttribHi & bitMask) != 0;
            byte bgPalette = (byte)((bgPal1 ? 1 : 0) << 1 | (bgPal0 ? 1 : 0));

            Color bgColor = GetColourFromPaletteRam(bgPalette, bgPixel);

            _sprScreen.SetPixel(_cycle - 1, _scanline, bgColor);
        }

        private void IncrementPpuAddress()
        {
            if ((Control & (byte)PpuControlFlags.IncrementMode) != 0)
            {
                PpuAddress += 32;
            }
            else
            {
                PpuAddress += 1;
            }
            _vramAddress.Raw = PpuAddress;
        }

        private void UpdateShifters()
        {
            if (_renderBackground)
            {
                _bgShifterPatternLo <<= 1;
                _bgShifterPatternHi <<= 1;
                _bgShifterAttribLo <<= 1;
                _bgShifterAttribHi <<= 1;
            }

            if (_renderSprites)
            {
                for (int i = 0; i < _spriteCount; i++)
                {
                    if (_spriteScanline[i].X > 0)
                    {
                        _spriteScanline[i].X--;
                    }
                    else
                    {
                        _spriteShiftersPatternLo[i] <<= 1;
                        _spriteShiftersPatternHi[i] <<= 1;
                    }
                }
            }
        }

        private void EvaluateSprites()
        {
            _spriteCount = 0;
            _spriteZeroHitPossible = false;
            _spriteZeroBeingRendered = false;

            Array.Clear(_spriteShiftersPatternLo, 0, _spriteShiftersPatternLo.Length);
            Array.Clear(_spriteShiftersPatternHi, 0, _spriteShiftersPatternHi.Length);

            for (int i = 0; i < _spriteScanline.Length; i++)
            {
                _spriteScanline[i] = new SpriteAttributeEntry { Y = 0xFF, ID = 0xFF, Attribute = 0xFF, X = 0xFF };
            }

            for (int i = 0; i < 64 && _spriteCount < 9; i++)
            {
                byte spriteY = _oam[i * 4];
                byte spriteID = _oam[i * 4 + 1];
                byte spriteAttrib = _oam[i * 4 + 2];
                byte spriteX = _oam[i * 4 + 3];
                int diff = PpuY - spriteY;
                bool spriteSize = (Control & (byte)PpuControlFlags.SpriteSize) != 0;
                int spriteHeight = spriteSize ? 16 : 8;

                if (diff >= 0 && diff < spriteHeight)
                {
                    if (_spriteCount < 8)
                    {
                        _spriteScanline[_spriteCount].Y = spriteY;
                        _spriteScanline[_spriteCount].ID = spriteID;
                        _spriteScanline[_spriteCount].Attribute = spriteAttrib;
                        _spriteScanline[_spriteCount].X = spriteX;

                        if (i == 0)
                        {
                            _spriteZeroHitPossible = true;
                        }

                        _spriteCount++;
                    }
                    else
                    {
                        Status |= (byte)PpuStatusFlags.SpriteOverflow;
                    }
                }
            }
        }

        private void LoadBackgroundShifters()
        {
            _bgShifterPatternLo = (ushort)((_bgShifterPatternLo & 0xFF00) |
                ReadPpuMemory((ushort)(0x2000 | (_vramAddress.Raw & 0x0FFF))));

            _bgShifterPatternHi = (ushort)((_bgShifterPatternHi & 0xFF00) |
                ReadPpuMemory((ushort)(0x2000 | ((_vramAddress.Raw & 0x0FFF) + 8))));

            int nametableY = _vramAddress.NametableY ? 1 : 0;
            int nametableX = _vramAddress.NametableX ? 1 : 0;

            byte attrib = ReadPpuMemory((ushort)(0x23C0 |
                (nametableY << 11) |
                (nametableX << 10) |
                ((_vramAddress.CoarseY >> 2) << 3) |
                (_vramAddress.CoarseX >> 2)));

            attrib = (byte)(((_vramAddress.CoarseY & 0x02) != 0) ? (attrib >> 4) : attrib);
            attrib = (byte)(((_vramAddress.CoarseX & 0x02) != 0) ? (attrib >> 2) : attrib);
            attrib &= 0x03;

            _bgShifterAttribLo = (ushort)((_bgShifterAttribLo & 0xFF00) | (attrib & 0x01));
            _bgShifterAttribHi = (ushort)((_bgShifterAttribHi & 0xFF00) | ((attrib & 0x02) >> 1));
        }

        private void IncrementScrollX()
        {
            if (_renderBackground || _renderSprites)
            {
                if (_vramAddress.CoarseX == 31)
                {
                    _vramAddress.CoarseX = 0;
                    _vramAddress.NametableX = !_vramAddress.NametableX;
                }
                else
                {
                    _vramAddress.CoarseX++;
                }
            }
        }

        private void IncrementScrollY()
        {
            if (_renderBackground || _renderSprites)
            {
                if (_vramAddress.FineY < 7)
                {
                    _vramAddress.FineY++;
                }
                else
                {
                    _vramAddress.FineY = 0;
                    if (_vramAddress.CoarseY == 29)
                    {
                        _vramAddress.CoarseY = 0;
                        _vramAddress.NametableY = !_vramAddress.NametableY;
                    }
                    else if (_vramAddress.CoarseY == 31)
                    {
                        _vramAddress.CoarseY = 0;
                    }
                    else
                    {
                        _vramAddress.CoarseY++;
                    }
                }
            }
        }

        private void TransferAddressX()
        {
            _vramAddress.NametableX = _tramAddress.NametableX;
            _vramAddress.CoarseX = _tramAddress.CoarseX;
        }

        private void TransferAddressY()
        {
            _vramAddress.FineY = _tramAddress.FineY;
            _vramAddress.NametableY = _tramAddress.NametableY;
            _vramAddress.CoarseY = _tramAddress.CoarseY;
        }

        private void LoadSpriteShifters()
        {
            for (int i = 0; i < _spriteCount; i++)
            {
                SpriteAttributeEntry sprite = _spriteScanline[i];

                ushort patternAddrLo = 0;
                ushort patternAddrHi = 0;

                bool spriteSize = (Control & (byte)PpuControlFlags.SpriteSize) != 0;

                if (!spriteSize)
                {
                    // 8x8 sprites
                    if ((sprite.Attribute & 0x80) == 0)
                    {
                        // Flip vertical
                        patternAddrLo = (ushort)((Control & (byte)PpuControlFlags.PatternSprite) << 12 | (sprite.ID << 4) | (PpuY - sprite.Y));
                    }
                    else
                    {
                        // No flip vertical
                        patternAddrLo = (ushort)((Control & (byte)PpuControlFlags.PatternSprite) << 12 | (sprite.ID << 4) | (7 - (PpuY - sprite.Y)));
                    }
                }
                else
                {
                    // 8x16 sprites
                    bool isTopHalf = (PpuY - sprite.Y) < 8;
                    byte spriteID = sprite.ID;

                    if ((sprite.Attribute & 0x80) != 0)
                    {
                        // Flip vertical
                        if (isTopHalf)
                        {
                            spriteID &= 0xFE;
                            patternAddrLo = (ushort)((spriteID & 0x01) << 12 | (spriteID << 4) | (7 - (PpuY - sprite.Y)));
                        }
                        else
                        {
                            spriteID |= 0x01;
                            patternAddrLo = (ushort)((spriteID & 0x01) << 12 | (spriteID << 4) | (15 - (PpuY - sprite.Y)));
                        }
                    }
                    else
                    {
                        // No flip vertical
                        if (isTopHalf)
                        {
                            spriteID &= 0xFE;
                            patternAddrLo = (ushort)((spriteID & 0x01) << 12 | (spriteID << 4) | (PpuY - sprite.Y));
                        }
                        else
                        {
                            spriteID |= 0x01;
                            patternAddrLo = (ushort)((spriteID & 0x01) << 12 | (spriteID << 4) | ((PpuY - sprite.Y) - 8));
                        }
                    }
                }

                patternAddrHi = (ushort)(patternAddrLo + 8);

                byte patternLo = ReadPpuMemory(patternAddrLo);
                byte patternHi = ReadPpuMemory(patternAddrHi);

                if ((sprite.Attribute & 0x40) != 0)
                {
                    patternLo = FlipByte(patternLo);
                    patternHi = FlipByte(patternHi);
                }

                _spriteShiftersPatternLo[i] = patternLo;
                _spriteShiftersPatternHi[i] = patternHi;
            }
        }

        private byte FlipByte(byte b)
        {
            b = (byte)((b & 0xF0) >> 4 | (b & 0x0F) << 4);
            b = (byte)((b & 0xCC) >> 2 | (b & 0x33) << 2);
            b = (byte)((b & 0xAA) >> 1 | (b & 0x55) << 1);
            return b;
        }

        internal void ComposePixel()
        {
            byte bgPixel = 0;
            byte bgPalette = 0;
            byte fgPixel = 0;
            byte fgPalette = 0;
            bool fgPriority = false;

            if (_renderBackground)
            {
                ushort bitMask = (ushort)(0x8000 >> _fineX);
                bool bgBit0 = (_bgShifterPatternLo & bitMask) != 0;
                bool bgBit1 = (_bgShifterPatternHi & bitMask) != 0;
                bgPixel = (byte)((bgBit1 ? 1 : 0) << 1 | (bgBit0 ? 1 : 0));

                bool bgPal0 = (_bgShifterAttribLo & bitMask) != 0;
                bool bgPal1 = (_bgShifterAttribHi & bitMask) != 0;
                bgPalette = (byte)((bgPal1 ? 1 : 0) << 1 | (bgPal0 ? 1 : 0));
            }

            if (_renderSprites)
            {
                for (int i = 0; i < _spriteCount; i++)
                {
                    if (_spriteScanline[i].X == 0)
                    {
                        bool fgBit0 = (_spriteShiftersPatternLo[i] & 0x80) != 0;
                        bool fgBit1 = (_spriteShiftersPatternHi[i] & 0x80) != 0;
                        fgPixel = (byte)((fgBit1 ? 1 : 0) << 1 | (fgBit0 ? 1 : 0));

                        fgPalette = (byte)((_spriteScanline[i].Attribute & 0x03) + 0x04);
                        fgPriority = (_spriteScanline[i].Attribute & 0x20) == 0;

                        if (fgPixel != 0)
                        {
                            if (i == 0)
                            {
                                _spriteZeroBeingRendered = true;
                            }
                            break;
                        }
                    }
                }
            }

            byte finalPixel = 0;
            byte finalPalette = 0;

            if (bgPixel == 0 && fgPixel == 0)
            {
                finalPixel = 0x00;
                finalPalette = 0x00;
            }
            else if (bgPixel == 0 && fgPixel > 0)
            {
                finalPixel = fgPixel;
                finalPalette = fgPalette;
            }
            else if (bgPixel > 0 && fgPixel == 0)
            {
                finalPixel = bgPixel;
                finalPalette = bgPalette;
            }
            else if (bgPixel > 0 && fgPixel > 0)
            {
                if (fgPriority)
                {
                    finalPixel = fgPixel;
                    finalPalette = fgPalette;
                }
                else
                {
                    finalPixel = bgPixel;
                    finalPalette = bgPalette;
                }

                // Sprite Zero Hit Detection
                if (_spriteZeroHitPossible && _spriteZeroBeingRendered)
                {
                    if ((_renderBackground && _renderSprites))
                    {
                        if ((_cycle >= 9 && _cycle < 258) || (_cycle >= 1 && _cycle < 258))
                        {
                            Status |= (byte)PpuStatusFlags.SpriteZeroHit;
                        }
                    }
                }
            }

            Color color = GetColourFromPaletteRam(finalPalette, finalPixel);
            _sprScreen.SetPixel(_cycle - 1, _scanline, color);
        }

        private void UpdateRenderFlags()
        {
            _renderBackground = (Mask & (byte)PpuMaskFlags.RenderBackground) != 0;
            _renderSprites = (Mask & (byte)PpuMaskFlags.RenderSprites) != 0;
        }

        private void UpdateGrayscaleFlag()
        {
            _grayscale = (Mask & (byte)PpuMaskFlags.Grayscale) != 0;
        }

        internal Color GetColourFromPaletteRam(byte palette, byte pixel)
        {
            if (palette >= 8)
            {
                palette = 0;
            }
            int paletteIndex = palette * 4 + pixel;
            paletteIndex &= 0x1F;

            if (paletteIndex >= NesPalette.Length)
            {
                return Color.Black;
            }

            Color color = NesPalette[paletteIndex];

            if (_grayscale)
            {
                int gray = (color.R + color.G + color.B) / 3;
                return Color.FromArgb(gray, gray, gray);
            }

            if ((Mask & (byte)PpuMaskFlags.EnhanceRed) != 0)
            {
                color = Color.FromArgb(Math.Min(color.R + 64, 255), color.G, color.B);
            }
            if ((Mask & (byte)PpuMaskFlags.EnhanceGreen) != 0)
            {
                color = Color.FromArgb(color.R, Math.Min(color.G + 64, 255), color.B);
            }
            if ((Mask & (byte)PpuMaskFlags.EnhanceBlue) != 0)
            {
                color = Color.FromArgb(color.R, color.G, Math.Min(color.B + 64, 255));
            }

            return color;
        }

        #endregion

        #region flags and structs

        [Flags]
        public enum PpuStatusFlags : byte
        {
            VerticalBlank = 0x80,
            SpriteZeroHit = 0x40,
            SpriteOverflow = 0x20,
            Unused = 0x1F
        }

        [Flags]
        public enum PpuMaskFlags : byte
        {
            Grayscale = 0x01,
            RenderBackgroundLeft = 0x02,
            RenderSpritesLeft = 0x04,
            RenderBackground = 0x08,
            RenderSprites = 0x10,
            EnhanceRed = 0x20,
            EnhanceGreen = 0x40,
            EnhanceBlue = 0x80
        }

        [Flags]
        public enum PpuControlFlags : byte
        {
            NametableX = 0x01,
            NametableY = 0x02,
            IncrementMode = 0x04,
            PatternSprite = 0x08,
            PatternBackground = 0x10,
            SpriteSize = 0x20,
            SlaveMode = 0x40,
            EnableNmi = 0x80
        }

        public struct LoopyRegister
        {
            public ushort Raw;

            public int CoarseX
            {
                get => (Raw >> 0) & 0x1F;
                set => Raw = (ushort)((Raw & ~0x001F) | (value & 0x001F));
            }

            public int CoarseY
            {
                get => (Raw >> 5) & 0x1F;
                set => Raw = (ushort)((Raw & ~0x03E0) | ((value & 0x1F) << 5));
            }

            public bool NametableX
            {
                get => (Raw & 0x0400) != 0;
                set => Raw = (ushort)(value ? (Raw | 0x0400) : (Raw & ~0x0400));
            }

            public bool NametableY
            {
                get => (Raw & 0x0800) != 0;
                set => Raw = (ushort)(value ? (Raw | 0x0800) : (Raw & ~0x0800));
            }

            public int FineY
            {
                get => (Raw >> 12) & 0x07;
                set => Raw = (ushort)((Raw & ~0x7000) | ((value & 0x07) << 12));
            }

            public void Reset()
            {
                Raw = 0;
            }
        }

        public struct SpriteAttributeEntry
        {
            public byte Y;           // Y position of sprite
            public byte ID;          // ID of tile from pattern memory
            public byte Attribute;   // Flags define how sprite should be rendered
            public byte X;           // X position of sprite
        }

        public enum Mirror
        {
            Horizontal,
            Vertical,
            SingleScreenLower,
            SingleScreenUpper,
            FourScreen
        }

        #endregion
    }
}
