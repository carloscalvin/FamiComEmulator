namespace FamiComEmulator.Components
{
    public class Cartridge : ICartridge
    {
        private byte _numPRGROM = 0;

        private byte[] _PRGROMMemory;

        struct NesHeader
        {
            public byte[] Magic;
            public byte NumPRGROM;
            public byte NumCHRROM;
            public byte Mapper1;
            public byte Mapper2;
            public byte NumPRGRAM;
            public byte AdditionalFlags1;
            public byte AdditionalFlags2;
            public byte[] Reserved;
        }

        public Cartridge(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                NesHeader header = new NesHeader
                {
                    Magic = reader.ReadBytes(4),
                    NumPRGROM = reader.ReadByte(),
                    NumCHRROM = reader.ReadByte(),
                    Mapper1 = reader.ReadByte(),
                    Mapper2 = reader.ReadByte(),
                    NumPRGRAM = reader.ReadByte(),
                    AdditionalFlags1 = reader.ReadByte(),
                    AdditionalFlags2 = reader.ReadByte(),
                    Reserved = reader.ReadBytes(5)
                };

                if ((header.Mapper1 & 0x04) != 0)
                {
                    reader.BaseStream.Seek(512, SeekOrigin.Current);
                }

                _numPRGROM = header.NumPRGROM;
                _PRGROMMemory = reader.ReadBytes(_numPRGROM * 16384);
            }
        }

        public bool Read(ushort address, ref byte data)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                data = _PRGROMMemory[address & (_numPRGROM > 1 ? 0x7FFF : 0x3FFF)];

                return true;
            }
            else
                return false;
        }

        public bool Write(ushort address, byte data)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                _PRGROMMemory[address & (_numPRGROM > 1 ? 0x7FFF : 0x3FFF)] = data;

                return true;
            }
            else
                return false;
        }
    }
}
