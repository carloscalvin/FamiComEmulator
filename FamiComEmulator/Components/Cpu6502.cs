using static FamiComEmulator.Components.ICpu6502;

namespace FamiComEmulator.Components
{
    internal class Cpu6502 : ICpu6502
    {
        #region public properties

        public byte Accumulator { get; set; }

        public byte XRegister { get; set; }

        public byte YRegister { get; set; }

        public byte StackPointer { get; set; }

        public ushort ProgramCounter { get; set; }

        public Flags6502 Status { get; set; }

        #endregion

        #region private properties

        private Instruction[,] _instructions { get; set; } = new Instruction[16, 16];

        private CentralBus _bus = new CentralBus();

        private ushort _address_abs = 0x0000;

        private ushort _address_rel = 0x0000;

        #endregion

        #region constructor

        public Cpu6502()
        {
            _instructions[0, 0] = new Instruction { Name = "BRK", OperationCode = BRK, AddressingMode = IMM, Cycles = 7 };
            _instructions[0, 1] = new Instruction { Name = "ORA", OperationCode = ORA, AddressingMode = IZX, Cycles = 6 };
            _instructions[0, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[0, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[0, 4] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 3 };
            _instructions[0, 5] = new Instruction { Name = "ORA", OperationCode = ORA, AddressingMode = ZP0, Cycles = 3 };
            _instructions[0, 6] = new Instruction { Name = "ASL", OperationCode = ASL, AddressingMode = ZP0, Cycles = 5 };
            _instructions[0, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[0, 8] = new Instruction { Name = "PHP", OperationCode = PHP, AddressingMode = IMP, Cycles = 3 };
            _instructions[0, 9] = new Instruction { Name = "ORA", OperationCode = ORA, AddressingMode = IMM, Cycles = 2 };
            _instructions[0, 10] = new Instruction { Name = "ASL", OperationCode = ASL, AddressingMode = IMP, Cycles = 2 };
            _instructions[0, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[0, 12] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[0, 13] = new Instruction { Name = "ORA", OperationCode = ORA, AddressingMode = ABS, Cycles = 4 };
            _instructions[0, 14] = new Instruction { Name = "ASL", OperationCode = ASL, AddressingMode = ABS, Cycles = 6 };
            _instructions[0, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[1, 0] = new Instruction { Name = "BPL", OperationCode = BPL, AddressingMode = REL, Cycles = 2 };
            _instructions[1, 1] = new Instruction { Name = "ORA", OperationCode = ORA, AddressingMode = IZY, Cycles = 5 };
            _instructions[1, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[1, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[1, 4] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[1, 5] = new Instruction { Name = "ORA", OperationCode = ORA, AddressingMode = ZPX, Cycles = 4 };
            _instructions[1, 6] = new Instruction { Name = "ASL", OperationCode = ASL, AddressingMode = ZPX, Cycles = 6 };
            _instructions[1, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[1, 8] = new Instruction { Name = "CLC", OperationCode = CLC, AddressingMode = IMP, Cycles = 2 };
            _instructions[1, 9] = new Instruction { Name = "ORA", OperationCode = ORA, AddressingMode = ABY, Cycles = 4 };
            _instructions[1, 10] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[1, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[1, 12] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[1, 13] = new Instruction { Name = "ORA", OperationCode = ORA, AddressingMode = ABX, Cycles = 4 };
            _instructions[1, 14] = new Instruction { Name = "ASL", OperationCode = ASL, AddressingMode = ABX, Cycles = 7 };
            _instructions[1, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[2, 0] = new Instruction { Name = "JSR", OperationCode = JSR, AddressingMode = ABS, Cycles = 6 };
            _instructions[2, 1] = new Instruction { Name = "AND", OperationCode = AND, AddressingMode = IZX, Cycles = 6 };
            _instructions[2, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[2, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[2, 4] = new Instruction { Name = "BIT", OperationCode = BIT, AddressingMode = ZP0, Cycles = 3 };
            _instructions[2, 5] = new Instruction { Name = "AND", OperationCode = AND, AddressingMode = ZP0, Cycles = 3 };
            _instructions[2, 6] = new Instruction { Name = "ROL", OperationCode = ROL, AddressingMode = ZP0, Cycles = 5 };
            _instructions[2, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[2, 8] = new Instruction { Name = "PLP", OperationCode = PLP, AddressingMode = IMP, Cycles = 4 };
            _instructions[2, 9] = new Instruction { Name = "AND", OperationCode = AND, AddressingMode = IMM, Cycles = 2 };
            _instructions[2, 10] = new Instruction { Name = "ROL", OperationCode = ROL, AddressingMode = IMP, Cycles = 2 };
            _instructions[2, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[2, 12] = new Instruction { Name = "BIT", OperationCode = BIT, AddressingMode = ABS, Cycles = 4 };
            _instructions[2, 13] = new Instruction { Name = "AND", OperationCode = AND, AddressingMode = ABS, Cycles = 4 };
            _instructions[2, 14] = new Instruction { Name = "ROL", OperationCode = ROL, AddressingMode = ABS, Cycles = 6 };
            _instructions[2, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[3, 0] = new Instruction { Name = "BMI", OperationCode = BMI, AddressingMode = REL, Cycles = 2 };
            _instructions[3, 1] = new Instruction { Name = "AND", OperationCode = AND, AddressingMode = IZY, Cycles = 5 };
            _instructions[3, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[3, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[3, 4] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[3, 5] = new Instruction { Name = "AND", OperationCode = AND, AddressingMode = ZPX, Cycles = 4 };
            _instructions[3, 6] = new Instruction { Name = "ROL", OperationCode = ROL, AddressingMode = ZPX, Cycles = 6 };
            _instructions[3, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[3, 8] = new Instruction { Name = "SEC", OperationCode = SEC, AddressingMode = IMP, Cycles = 2 };
            _instructions[3, 9] = new Instruction { Name = "AND", OperationCode = AND, AddressingMode = ABY, Cycles = 4 };
            _instructions[3, 10] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[3, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[3, 12] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[3, 13] = new Instruction { Name = "AND", OperationCode = AND, AddressingMode = ABX, Cycles = 4 };
            _instructions[3, 14] = new Instruction { Name = "ROL", OperationCode = ROL, AddressingMode = ABX, Cycles = 7 };
            _instructions[3, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[4, 0] = new Instruction { Name = "RTI", OperationCode = RTI, AddressingMode = IMP, Cycles = 6 };
            _instructions[4, 1] = new Instruction { Name = "EOR", OperationCode = EOR, AddressingMode = IZX, Cycles = 6 };
            _instructions[4, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[4, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[4, 4] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 3 };
            _instructions[4, 5] = new Instruction { Name = "EOR", OperationCode = EOR, AddressingMode = ZP0, Cycles = 3 };
            _instructions[4, 6] = new Instruction { Name = "LSR", OperationCode = LSR, AddressingMode = ZP0, Cycles = 5 };
            _instructions[4, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[4, 8] = new Instruction { Name = "PHA", OperationCode = PHA, AddressingMode = IMP, Cycles = 3 };
            _instructions[4, 9] = new Instruction { Name = "EOR", OperationCode = EOR, AddressingMode = IMM, Cycles = 2 };
            _instructions[4, 10] = new Instruction { Name = "LSR", OperationCode = LSR, AddressingMode = IMP, Cycles = 2 };
            _instructions[4, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[4, 12] = new Instruction { Name = "JMP", OperationCode = JMP, AddressingMode = ABS, Cycles = 3 };
            _instructions[4, 13] = new Instruction { Name = "EOR", OperationCode = EOR, AddressingMode = ABS, Cycles = 4 };
            _instructions[4, 14] = new Instruction { Name = "LSR", OperationCode = LSR, AddressingMode = ABS, Cycles = 6 };
            _instructions[4, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[5, 0] = new Instruction { Name = "BVC", OperationCode = BVC, AddressingMode = REL, Cycles = 2 };
            _instructions[5, 1] = new Instruction { Name = "EOR", OperationCode = EOR, AddressingMode = IZY, Cycles = 5 };
            _instructions[5, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[5, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[5, 4] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[5, 5] = new Instruction { Name = "EOR", OperationCode = EOR, AddressingMode = ZPX, Cycles = 4 };
            _instructions[5, 6] = new Instruction { Name = "LSR", OperationCode = LSR, AddressingMode = ZPX, Cycles = 6 };
            _instructions[5, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[5, 8] = new Instruction { Name = "CLI", OperationCode = CLI, AddressingMode = IMP, Cycles = 2 };
            _instructions[5, 9] = new Instruction { Name = "EOR", OperationCode = EOR, AddressingMode = ABY, Cycles = 4 };
            _instructions[5, 10] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[5, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[5, 12] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[5, 13] = new Instruction { Name = "EOR", OperationCode = EOR, AddressingMode = ABX, Cycles = 4 };
            _instructions[5, 14] = new Instruction { Name = "LSR", OperationCode = LSR, AddressingMode = ABX, Cycles = 7 };
            _instructions[5, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[6, 0] = new Instruction { Name = "RTS", OperationCode = RTS, AddressingMode = IMP, Cycles = 6 };
            _instructions[6, 1] = new Instruction { Name = "ADC", OperationCode = ADC, AddressingMode = IZX, Cycles = 6 };
            _instructions[6, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[6, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[6, 4] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 3 };
            _instructions[6, 5] = new Instruction { Name = "ADC", OperationCode = ADC, AddressingMode = ZP0, Cycles = 3 };
            _instructions[6, 6] = new Instruction { Name = "ROR", OperationCode = ROR, AddressingMode = ZP0, Cycles = 5 };
            _instructions[6, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[6, 8] = new Instruction { Name = "PLA", OperationCode = PLA, AddressingMode = IMP, Cycles = 4 };
            _instructions[6, 9] = new Instruction { Name = "ADC", OperationCode = ADC, AddressingMode = IMM, Cycles = 2 };
            _instructions[6, 10] = new Instruction { Name = "ROR", OperationCode = ROR, AddressingMode = IMP, Cycles = 2 };
            _instructions[6, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[6, 12] = new Instruction { Name = "JMP", OperationCode = JMP, AddressingMode = IND, Cycles = 5 };
            _instructions[6, 13] = new Instruction { Name = "ADC", OperationCode = ADC, AddressingMode = ABS, Cycles = 4 };
            _instructions[6, 14] = new Instruction { Name = "ROR", OperationCode = ROR, AddressingMode = ABS, Cycles = 6 };
            _instructions[6, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[7, 0] = new Instruction { Name = "BVS", OperationCode = BVS, AddressingMode = REL, Cycles = 2 };
            _instructions[7, 1] = new Instruction { Name = "ADC", OperationCode = ADC, AddressingMode = IZY, Cycles = 5 };
            _instructions[7, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[7, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[7, 4] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[7, 5] = new Instruction { Name = "ADC", OperationCode = ADC, AddressingMode = ZPX, Cycles = 4 };
            _instructions[7, 6] = new Instruction { Name = "ROR", OperationCode = ROR, AddressingMode = ZPX, Cycles = 6 };
            _instructions[7, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[7, 8] = new Instruction { Name = "SEI", OperationCode = SEI, AddressingMode = IMP, Cycles = 2 };
            _instructions[7, 9] = new Instruction { Name = "ADC", OperationCode = ADC, AddressingMode = ABY, Cycles = 4 };
            _instructions[7, 10] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[7, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[7, 12] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[7, 13] = new Instruction { Name = "ADC", OperationCode = ADC, AddressingMode = ABX, Cycles = 4 };
            _instructions[7, 14] = new Instruction { Name = "ROR", OperationCode = ROR, AddressingMode = ABX, Cycles = 7 };
            _instructions[7, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[8, 0] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[8, 1] = new Instruction { Name = "STA", OperationCode = STA, AddressingMode = IZX, Cycles = 6 };
            _instructions[8, 2] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[8, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[8, 4] = new Instruction { Name = "STY", OperationCode = STY, AddressingMode = ZP0, Cycles = 3 };
            _instructions[8, 5] = new Instruction { Name = "STA", OperationCode = STA, AddressingMode = ZP0, Cycles = 3 };
            _instructions[8, 6] = new Instruction { Name = "STX", OperationCode = STX, AddressingMode = ZP0, Cycles = 3 };
            _instructions[8, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 3 };
            _instructions[8, 8] = new Instruction { Name = "DEY", OperationCode = DEY, AddressingMode = IMP, Cycles = 2 };
            _instructions[8, 9] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[8, 10] = new Instruction { Name = "TXA", OperationCode = TXA, AddressingMode = IMP, Cycles = 2 };
            _instructions[8, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[8, 12] = new Instruction { Name = "STY", OperationCode = STY, AddressingMode = ABS, Cycles = 4 };
            _instructions[8, 13] = new Instruction { Name = "STA", OperationCode = STA, AddressingMode = ABS, Cycles = 4 };
            _instructions[8, 14] = new Instruction { Name = "STX", OperationCode = STX, AddressingMode = ABS, Cycles = 4 };
            _instructions[8, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 4 };
            _instructions[9, 0] = new Instruction { Name = "BCC", OperationCode = BCC, AddressingMode = REL, Cycles = 2 };
            _instructions[9, 1] = new Instruction { Name = "STA", OperationCode = STA, AddressingMode = IZY, Cycles = 6 };
            _instructions[9, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[9, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[9, 4] = new Instruction { Name = "STY", OperationCode = STY, AddressingMode = ZPX, Cycles = 4 };
            _instructions[9, 5] = new Instruction { Name = "STA", OperationCode = STA, AddressingMode = ZPX, Cycles = 4 };
            _instructions[9, 6] = new Instruction { Name = "STX", OperationCode = STX, AddressingMode = ZPY, Cycles = 4 };
            _instructions[9, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 4 };
            _instructions[9, 8] = new Instruction { Name = "TYA", OperationCode = TYA, AddressingMode = IMP, Cycles = 2 };
            _instructions[9, 9] = new Instruction { Name = "STA", OperationCode = STA, AddressingMode = ABY, Cycles = 5 };
            _instructions[9, 10] = new Instruction { Name = "TXS", OperationCode = TXS, AddressingMode = IMP, Cycles = 2 };
            _instructions[9, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[9, 12] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 5 };
            _instructions[9, 13] = new Instruction { Name = "STA", OperationCode = STA, AddressingMode = ABX, Cycles = 5 };
            _instructions[9, 14] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[9, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[10, 0] = new Instruction { Name = "LDY", OperationCode = LDY, AddressingMode = IMM, Cycles = 2 };
            _instructions[10, 1] = new Instruction { Name = "LDA", OperationCode = LDA, AddressingMode = IZX, Cycles = 6 };
            _instructions[10, 2] = new Instruction { Name = "LDX", OperationCode = LDX, AddressingMode = IMM, Cycles = 2 };
            _instructions[10, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[10, 4] = new Instruction { Name = "LDY", OperationCode = LDY, AddressingMode = ZP0, Cycles = 3 };
            _instructions[10, 5] = new Instruction { Name = "LDA", OperationCode = LDA, AddressingMode = ZP0, Cycles = 3 };
            _instructions[10, 6] = new Instruction { Name = "LDX", OperationCode = LDX, AddressingMode = ZP0, Cycles = 3 };
            _instructions[10, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 3 };
            _instructions[10, 8] = new Instruction { Name = "TAY", OperationCode = TAY, AddressingMode = IMP, Cycles = 2 };
            _instructions[10, 9] = new Instruction { Name = "LDA", OperationCode = LDA, AddressingMode = IMM, Cycles = 2 };
            _instructions[10, 10] = new Instruction { Name = "TAX", OperationCode = TAX, AddressingMode = IMP, Cycles = 2 };
            _instructions[10, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[10, 12] = new Instruction { Name = "LDY", OperationCode = LDY, AddressingMode = ABS, Cycles = 4 };
            _instructions[10, 13] = new Instruction { Name = "LDA", OperationCode = LDA, AddressingMode = ABS, Cycles = 4 };
            _instructions[10, 14] = new Instruction { Name = "LDX", OperationCode = LDX, AddressingMode = ABS, Cycles = 4 };
            _instructions[10, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 4 };
            _instructions[11, 0] = new Instruction { Name = "BCS", OperationCode = BCS, AddressingMode = REL, Cycles = 2 };
            _instructions[11, 1] = new Instruction { Name = "LDA", OperationCode = LDA, AddressingMode = IZY, Cycles = 5 };
            _instructions[11, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[11, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[11, 4] = new Instruction { Name = "LDY", OperationCode = LDY, AddressingMode = ZPX, Cycles = 4 };
            _instructions[11, 5] = new Instruction { Name = "LDA", OperationCode = LDA, AddressingMode = ZPX, Cycles = 4 };
            _instructions[11, 6] = new Instruction { Name = "LDX", OperationCode = LDX, AddressingMode = ZPY, Cycles = 4 };
            _instructions[11, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 4 };
            _instructions[11, 8] = new Instruction { Name = "CLV", OperationCode = CLV, AddressingMode = IMP, Cycles = 2 };
            _instructions[11, 9] = new Instruction { Name = "LDA", OperationCode = LDA, AddressingMode = ABY, Cycles = 4 };
            _instructions[11, 10] = new Instruction { Name = "TSX", OperationCode = TSX, AddressingMode = IMP, Cycles = 2 };
            _instructions[11, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 4 };
            _instructions[11, 12] = new Instruction { Name = "LDY", OperationCode = LDY, AddressingMode = ABX, Cycles = 4 };
            _instructions[11, 13] = new Instruction { Name = "LDA", OperationCode = LDA, AddressingMode = ABX, Cycles = 4 };
            _instructions[11, 14] = new Instruction { Name = "LDX", OperationCode = LDX, AddressingMode = ABY, Cycles = 4 };
            _instructions[11, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 4 };
            _instructions[12, 0] = new Instruction { Name = "CPY", OperationCode = CPY, AddressingMode = IMM, Cycles = 2 };
            _instructions[12, 1] = new Instruction { Name = "CMP", OperationCode = CMP, AddressingMode = IZX, Cycles = 6 };
            _instructions[12, 2] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[12, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[12, 4] = new Instruction { Name = "CPY", OperationCode = CPY, AddressingMode = ZP0, Cycles = 3 };
            _instructions[12, 5] = new Instruction { Name = "CMP", OperationCode = CMP, AddressingMode = ZP0, Cycles = 3 };
            _instructions[12, 6] = new Instruction { Name = "DEC", OperationCode = DEC, AddressingMode = ZP0, Cycles = 5 };
            _instructions[12, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[12, 8] = new Instruction { Name = "INY", OperationCode = INY, AddressingMode = IMP, Cycles = 2 };
            _instructions[12, 9] = new Instruction { Name = "CMP", OperationCode = CMP, AddressingMode = IMM, Cycles = 2 };
            _instructions[12, 10] = new Instruction { Name = "DEX", OperationCode = DEX, AddressingMode = IMP, Cycles = 2 };
            _instructions[12, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[12, 12] = new Instruction { Name = "CPY", OperationCode = CPY, AddressingMode = ABS, Cycles = 4 };
            _instructions[12, 13] = new Instruction { Name = "CMP", OperationCode = CMP, AddressingMode = ABS, Cycles = 4 };
            _instructions[12, 14] = new Instruction { Name = "DEC", OperationCode = DEC, AddressingMode = ABS, Cycles = 6 };
            _instructions[12, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[13, 0] = new Instruction { Name = "BNE", OperationCode = BNE, AddressingMode = REL, Cycles = 2 };
            _instructions[13, 1] = new Instruction { Name = "CMP", OperationCode = CMP, AddressingMode = IZY, Cycles = 5 };
            _instructions[13, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[13, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[13, 4] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[13, 5] = new Instruction { Name = "CMP", OperationCode = CMP, AddressingMode = ZPX, Cycles = 4 };
            _instructions[13, 6] = new Instruction { Name = "DEC", OperationCode = DEC, AddressingMode = ZPX, Cycles = 6 };
            _instructions[13, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[13, 8] = new Instruction { Name = "CLD", OperationCode = CLD, AddressingMode = IMP, Cycles = 2 };
            _instructions[13, 9] = new Instruction { Name = "CMP", OperationCode = CMP, AddressingMode = ABY, Cycles = 4 };
            _instructions[13, 10] = new Instruction { Name = "NOP", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[13, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[13, 12] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[13, 13] = new Instruction { Name = "CMP", OperationCode = CMP, AddressingMode = ABX, Cycles = 4 };
            _instructions[13, 14] = new Instruction { Name = "DEC", OperationCode = DEC, AddressingMode = ABX, Cycles = 7 };
            _instructions[13, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[14, 0] = new Instruction { Name = "CPX", OperationCode = CPX, AddressingMode = IMM, Cycles = 2 };
            _instructions[14, 1] = new Instruction { Name = "SBC", OperationCode = SBC, AddressingMode = IZX, Cycles = 6 };
            _instructions[14, 2] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[14, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[14, 4] = new Instruction { Name = "CPX", OperationCode = CPX, AddressingMode = ZP0, Cycles = 3 };
            _instructions[14, 5] = new Instruction { Name = "SBC", OperationCode = SBC, AddressingMode = ZP0, Cycles = 3 };
            _instructions[14, 6] = new Instruction { Name = "INC", OperationCode = INC, AddressingMode = ZP0, Cycles = 5 };
            _instructions[14, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 5 };
            _instructions[14, 8] = new Instruction { Name = "INX", OperationCode = INX, AddressingMode = IMP, Cycles = 2 };
            _instructions[14, 9] = new Instruction { Name = "SBC", OperationCode = SBC, AddressingMode = IMM, Cycles = 2 };
            _instructions[14, 10] = new Instruction { Name = "NOP", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[14, 11] = new Instruction { Name = "UNDEFINED", OperationCode = SBC, AddressingMode = IMP, Cycles = 2 };
            _instructions[14, 12] = new Instruction { Name = "CPX", OperationCode = CPX, AddressingMode = ABS, Cycles = 4 };
            _instructions[14, 13] = new Instruction { Name = "SBC", OperationCode = SBC, AddressingMode = ABS, Cycles = 4 };
            _instructions[14, 14] = new Instruction { Name = "INC", OperationCode = INC, AddressingMode = ABS, Cycles = 6 };
            _instructions[14, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[15, 0] = new Instruction { Name = "BEQ", OperationCode = BEQ, AddressingMode = REL, Cycles = 2 };
            _instructions[15, 1] = new Instruction { Name = "SBC", OperationCode = SBC, AddressingMode = IZY, Cycles = 5 };
            _instructions[15, 2] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 2 };
            _instructions[15, 3] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 8 };
            _instructions[15, 4] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[15, 5] = new Instruction { Name = "SBC", OperationCode = SBC, AddressingMode = ZPX, Cycles = 4 };
            _instructions[15, 6] = new Instruction { Name = "INC", OperationCode = INC, AddressingMode = ZPX, Cycles = 6 };
            _instructions[15, 7] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 6 };
            _instructions[15, 8] = new Instruction { Name = "SED", OperationCode = SED, AddressingMode = IMP, Cycles = 2 };
            _instructions[15, 9] = new Instruction { Name = "SBC", OperationCode = SBC, AddressingMode = ABY, Cycles = 4 };
            _instructions[15, 10] = new Instruction { Name = "NOP", OperationCode = NOP, AddressingMode = IMP, Cycles = 2 };
            _instructions[15, 11] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
            _instructions[15, 12] = new Instruction { Name = "UNDEFINED", OperationCode = NOP, AddressingMode = IMP, Cycles = 4 };
            _instructions[15, 13] = new Instruction { Name = "SBC", OperationCode = SBC, AddressingMode = ABX, Cycles = 4 };
            _instructions[15, 14] = new Instruction { Name = "INC", OperationCode = INC, AddressingMode = ABX, Cycles = 7 };
            _instructions[15, 15] = new Instruction { Name = "UNDEFINED", OperationCode = UndefinedOperation, AddressingMode = IMP, Cycles = 7 };
        }

        #endregion

        #region public methods

        public void AddCentralBus(CentralBus bus)
        {
            _bus = bus;
        }

        public void Write(ushort address, byte data)
        {
            _bus.Write(address, data);
        }

        public byte Read(ushort address)
        {
            return _bus.Read(address);
        }

        #endregion

        #region private methods

        private int GetFlag(Flags6502 flag)
        {
            return Status.HasFlag(flag) ? 1 : 0;
        }

        private void SetFlag(Flags6502 flag, bool overflow)
        {
            if (overflow)
                Status |= flag;
            else
                Status &= ~flag;
        }

        private Instruction GetInstructionByPosition(int row, int column)
        {
            return _instructions[row, column];
        }

        #endregion

        #region addressing modes

        private byte IMP()
        {
            return 0;
        }

        private byte IMM()
        {
            _address_abs = ProgramCounter++;
            return 0;
        }

        private byte ZP0()
        {
            _address_abs = Read(ProgramCounter);
            ProgramCounter++;
            _address_abs &= 0x00FF;

            return 0;
        }

        private byte ZPX()
        {
            _address_abs = (ushort)(Read(ProgramCounter) + XRegister);
            ProgramCounter++;
            _address_abs &= 0x00FF;

            return 0;
        }

        private byte ZPY()
        {
            _address_abs = (ushort)(Read(ProgramCounter) + YRegister);
            ProgramCounter++;
            _address_abs &= 0x00FF;

            return 0;
        }

        private byte REL()
        {
            _address_rel = Read(ProgramCounter);
            ProgramCounter++;
            if ((_address_rel & 0x80) != 0)
                _address_rel |= 0xFF00;

            return 0;
        }

        private byte ABS()
        {
            ushort lowByte = Read(ProgramCounter);
            ProgramCounter++;
            ushort highByte = Read(ProgramCounter);
            ProgramCounter++;

            _address_abs = (ushort)((highByte << 8) | lowByte);

            return 0;
        }

        private byte ABX()
        {
            ushort lowByte = Read(ProgramCounter);
            ProgramCounter++;
            ushort highByte = Read(ProgramCounter);
            ProgramCounter++;

            _address_abs = (ushort)((highByte << 8) | lowByte);

            _address_abs += XRegister;

            if ((_address_abs & 0xFF00) != (highByte << 8))
                return 1;
            else
                return 0;
        }

        private byte ABY()
        {
            ushort lowByte = Read(ProgramCounter);
            ProgramCounter++;
            ushort highByte = Read(ProgramCounter);
            ProgramCounter++;

            _address_abs = (ushort)((highByte << 8) | lowByte);

            _address_abs += YRegister;

            if ((_address_abs & 0xFF00) != (highByte << 8))
                return 1;
            else
                return 0;
        }

        private byte IND()
        {
            ushort lowPointer = Read(ProgramCounter);
            ProgramCounter++;
            ushort highPointer = Read(ProgramCounter);
            ProgramCounter++;

            ushort pointer = (ushort)((highPointer << 8) | lowPointer);

            if (lowPointer == 0x00FF)
            {
                _address_abs = (ushort)(Read((ushort)((pointer & 0xFF00) << 8)) | Read((ushort)(pointer + 0)));
            }
            else
            {
                _address_abs = (ushort)(Read((ushort)((pointer + 1) << 8)) | Read((ushort)(pointer + 0)));
            }

            return 0;
        }

        private byte IZX()
        {
            ushort basePointer = Read(ProgramCounter);
            ProgramCounter++;

            ushort lowByte = Read((ushort)((ushort)(basePointer + XRegister) & 0x00FF));
            ushort highByte = Read((ushort)((ushort)(basePointer + XRegister + 1) & 0x00FF));

            _address_abs = (ushort)((highByte << 8) | lowByte);

            return 0;
        }

        private byte IZY()
        {
            ushort basePointer = Read(ProgramCounter);
            ProgramCounter++;

            ushort lowByte = Read((ushort)(basePointer & 0x00FF));
            ushort highByte = Read((ushort)((basePointer + 1) & 0x00FF));

            _address_abs = (ushort)((highByte << 8) | lowByte);
            _address_abs += YRegister;

            if ((_address_abs & 0xFF00) != (highByte << 8))
                return 1;
            else
                return 0;
        }

        #endregion

        #region operation codes

        private byte BRK()
        {
            return 0;
        }
        private byte SEC()
        {
            return 0;
        }

        private byte RTI()
        {
            return 0;
        }

        private byte PHA()
        {
            return 0;
        }

        private byte CLI()
        {
            return 0;
        }

        private byte BVC()
        {
            return 0;
        }

        private byte EOR()
        {
            return 0;
        }

        private byte RTS()
        {
            return 0;
        }

        private byte PLA()
        {
            return 0;
        }

        private byte SEI()
        {
            return 0;
        }

        private byte BMI()
        {
            return 0;
        }

        private byte PLP()
        {
            return 0;
        }

        private byte JMP()
        {
            return 0;
        }

        private byte BVS()
        {
            return 0;
        }

        private byte ADC()
        {
            return 0;
        }

        private byte ROR()
        {
            return 0;
        }

        private byte DEY()
        {
            return 0;
        }

        private byte TXA()
        {
            return 0;
        }

        private byte BCC()
        {
            return 0;
        }

        private byte STY()
        {
            return 0;
        }

        private byte STX()
        {
            return 0;
        }

        private byte ROL()
        {
            return 0;
        }

        private byte BIT()
        {
            return 0;
        }

        private byte AND()
        {
            return 0;
        }

        private byte BPL()
        {
            return 0;
        }

        private byte JSR()
        {
            return 0;
        }

        private byte CLC()
        {
            return 0;
        }

        private byte PHP()
        {
            return 0;
        }

        private byte TYA()
        {
            return 0;
        }

        private byte TXS()
        {
            return 0;
        }

        private byte STA()
        {
            return 0;
        }

        private byte TAY()
        {
            return 0;
        }

        private byte TAX()
        {
            return 0;
        }

        private byte BCS()
        {
            return 0;
        }

        private byte CLV()
        {
            return 0;
        }

        private byte TSX()
        {
            return 0;
        }

        private byte LDA()
        {
            return 0;
        }

        private byte LSR()
        {
            return 0;
        }

        private byte ASL()
        {
            return 0;
        }

        private byte LDX()
        {
            return 0;
        }

        private byte INY()
        {
            return 0;
        }

        private byte DEX()
        {
            return 0;
        }

        private byte CPY()
        {
            return 0;
        }

        private byte CLD()
        {
            return 0;
        }

        private byte BNE()
        {
            return 0;
        }

        private byte DEC()
        {
            return 0;
        }

        private byte INX()
        {
            return 0;
        }

        private byte CMP()
        {
            return 0;
        }

        private byte CPX()
        {
            return 0;
        }

        private byte SED()
        {
            return 0;
        }

        private byte BEQ()
        {
            return 0;
        }

        private byte NOP()
        {
            return 0;
        }

        private byte INC()
        {
            return 0;
        }

        private byte SBC()
        {
            return 0;
        }

        private byte LDY()
        {
            return 0;
        }

        private byte ORA()
        {
            return 0;
        }

        private byte UndefinedOperation()
        {
            return 0;
        }

        #endregion
    }
}
