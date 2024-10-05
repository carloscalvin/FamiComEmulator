namespace FamiComEmulator.Components
{
    public interface IPpu2c02
    {
        // PPU Registers
        byte Control { get; set; }
        byte Mask { get; set; }
        byte Status { get; set; }
        byte OamAddress { get; set; }
        byte OamData { get; set; }
        byte ScrollX { get; }
        byte ScrollY { get; }
        ushort PpuAddress { get; set; }
        byte PpuData { get; set; }
        int PpuX { get; set; }
        int PpuY { get; set; }

        // Methods
        void Clock();
        void Reset();
        void WriteRegister(byte register, byte data);
        byte ReadRegister(byte register);
        void PerformOamDma(byte page);
        void AddCentralBus(CentralBus bus);
    }
}
