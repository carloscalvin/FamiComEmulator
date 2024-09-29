namespace FamiComEmulator.Components
{
    public interface IPpu2c02
    {
        // PPU Registers
        byte Control { get; set; }
        byte Mask { get; set; }
        byte Status { get; }
        byte OamAddress { get; set; }
        byte OamData { get; set; }
        byte Scroll { get; set; }
        byte PpuAddress { get; set; }
        byte PpuData { get; set; }
        int PpuX { get; }
        int PpuY { get; }

        // Methods
        void Clock();
        void Reset();
        void WriteRegister(byte register, byte data);
        byte ReadRegister(byte register);
        void AddCentralBus(CentralBus bus);
    }
}
