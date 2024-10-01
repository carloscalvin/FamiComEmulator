using static FamiComEmulator.Components.Ppu2c02;

namespace FamiComEmulator.Components
{
    /// <summary>
    /// Represents a NES Cartridge, handling PRG-ROM and CHR-ROM data.
    /// </summary>
    public class Cartridge : ICartridge
    {
        public Mirror Mirror { get; private set; }
        private readonly byte _numPRGROM;
        private readonly byte _numCHRROM;
        private readonly byte _mapperNumber;
        private readonly byte _numPRGRAM;
        private byte[] _PRGROMMemory;
        private byte[] _CHRROMMemory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cartridge"/> class by loading a NES ROM file.
        /// </summary>
        /// <param name="filePath">The path to the NES ROM file.</param>
        /// <exception cref="InvalidDataException">Thrown when the ROM file is invalid or corrupted.</exception>
        public Cartridge(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                NesHeader header = ReadHeader(reader);

                // Validate Magic Number
                if (!header.IsValidMagic())
                {
                    throw new InvalidDataException("Invalid NES ROM file: Incorrect magic number.");
                }

                _numPRGROM = header.NumPRGROM;
                _numCHRROM = header.NumCHRROM;
                _numPRGRAM = header.NumPRGRAM;
                _mapperNumber = header.GetMapperNumber();

                // Handle Mapper Specific Loading (Simplified for Mapper 0 - NROM)
                if (_mapperNumber == 0)
                {
                    LoadNROM(reader, header);
                }
                else
                {
                    throw new NotSupportedException($"Mapper {_mapperNumber} is not supported yet.");
                }
            }
        }

        /// <summary>
        /// Reads the NES header from the ROM file.
        /// </summary>
        /// <param name="reader">The binary reader for the ROM file.</param>
        /// <returns>A <see cref="NesHeader"/> instance containing header data.</returns>
        private NesHeader ReadHeader(BinaryReader reader)
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

            // Check if trainer is present
            bool hasTrainer = (header.Mapper1 & 0x04) != 0;
            if (hasTrainer)
            {
                reader.BaseStream.Seek(512, SeekOrigin.Current);
            }

            return header;
        }

        /// <summary>
        /// Loads NROM-specific data from the ROM file.
        /// </summary>
        /// <param name="reader">The binary reader for the ROM file.</param>
        /// <param name="header">The NES header data.</param>
        private void LoadNROM(BinaryReader reader, NesHeader header)
        {
            // Load PRG-ROM
            _PRGROMMemory = reader.ReadBytes(_numPRGROM * 16384);
            if (_PRGROMMemory.Length != _numPRGROM * 16384)
            {
                throw new InvalidDataException("Invalid NES ROM file: Incomplete PRG-ROM data.");
            }

            // Load CHR-ROM if present
            if (_numCHRROM > 0)
            {
                _CHRROMMemory = reader.ReadBytes(_numCHRROM * 8192);
                if (_CHRROMMemory.Length != _numCHRROM * 8192)
                {
                    throw new InvalidDataException("Invalid NES ROM file: Incomplete CHR-ROM data.");
                }
            }
            else
            {
                // Some cartridges use CHR-RAM instead of CHR-ROM
                _CHRROMMemory = Array.Empty<byte>();
            }

            bool horizontal = (header.Mapper1 & 0x01) != 0;
            bool vertical = (header.Mapper1 & 0x02) != 0;

            if (horizontal && !vertical)
                Mirror = Mirror.Horizontal;
            else if (vertical && !horizontal)
                Mirror = Mirror.Vertical;
            else
                Mirror = Mirror.Horizontal;
        }

        /// <summary>
        /// Reads a byte from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from (0x8000 - 0xFFFF).</param>
        /// <param name="data">The byte value read from memory.</param>
        /// <returns>True if the read was successful; otherwise, false.</returns>
        public bool Read(ushort address, ref byte data)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                int bank = 0;
                ushort offset = (ushort)(address - 0x8000);

                if (_numPRGROM == 1)
                {
                    // Single 16KB bank is mirrored to 0x8000-0xFFFF
                    offset &= 0x3FFF;
                }
                else
                {
                    // Multiple banks (assuming 32KB for Mapper 0)
                    bank = (address >= 0xC000) ? 1 : 0;
                    offset = (ushort)(address - (bank == 0 ? 0x8000 : 0xC000));
                }

                data = _PRGROMMemory[(bank * 16384) + offset];
                return true;
            }
            else if (address >= 0x0000 && address <= 0x1FFF)
            {
                /*data = _CHRROMMemory[address & 0x1FFF];
                return true;*/
            }

            return false;
        }

        /// <summary>
        /// Writes a byte to the specified memory address. Not supported for PRG-ROM.
        /// </summary>
        /// <param name="address">The memory address to write to (0x8000 - 0xFFFF).</param>
        /// <param name="data">The byte value to write to memory.</param>
        /// <returns>Always false as PRG-ROM is read-only.</returns>
        public bool Write(ushort address, byte data)
        {
            // PRG-ROM is typically read-only. Handle PRG-RAM if supported by the mapper.
            // Currently, only Mapper 0 (NROM) is supported, which does not have PRG-RAM.
            return false;
        }

        /// <summary>
        /// Represents the NES ROM header structure.
        /// </summary>
        private struct NesHeader
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

            /// <summary>
            /// Checks if the magic number matches "NES\x1A".
            /// </summary>
            /// <returns>True if valid; otherwise, false.</returns>
            public bool IsValidMagic()
            {
                return Magic.Length == 4 &&
                       Magic[0] == 0x4E && // 'N'
                       Magic[1] == 0x45 && // 'E'
                       Magic[2] == 0x53 && // 'S'
                       Magic[3] == 0x1A;   // Control-Z
            }

            /// <summary>
            /// Calculates the mapper number from Mapper1 and Mapper2 fields.
            /// </summary>
            /// <returns>The mapper number.</returns>
            public byte GetMapperNumber()
            {
                return (byte)(((Mapper2 >> 4) << 4) | (Mapper1 >> 4));
            }
        }
    }
}
