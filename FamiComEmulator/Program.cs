using FamiComEmulator.Components;

namespace FamiComEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please provide the path to the NES ROM file:");
            string romPath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(romPath))
            {
                Console.WriteLine("No ROM file path was provided.");
                return;
            }

            // Check if the file exists
            if (!System.IO.File.Exists(romPath))
            {
                Console.WriteLine($"ROM file not found at path: {romPath}");
                return;
            }

            // Create the PPU renderer (assuming it's a form-based renderer)
            IPpuRenderer renderer = new PpuRenderer(256, 240);

            // Create the CPU and PPU
            ICpu6502 cpu = new Cpu6502();
            IPpu2c02 ppu = new Ppu2c02(renderer);

            // Create the central bus
            ICentralBus bus = new CentralBus(cpu, ppu);

            // Load a cartridge
            ICartridge cartridge = new Cartridge(romPath);
            bus.AddCartridge(cartridge);

            // Reset the system
            bus.Reset();

            // Start the renderer in a separate thread
            Thread rendererThread = new Thread(() =>
            {
                renderer.Run(); // Run the rendering loop
            });
            rendererThread.SetApartmentState(ApartmentState.STA); // Required for Windows Forms
            rendererThread.Start();

            // Start the emulation loop in a separate thread
            Thread emulationThread = new Thread(() =>
            {
                while (true)
                {
                    bus.Clock(); // Clock the CPU and PPU for each cycle

                    // Sleep to simulate real-time emulation speed (optional)
                    Thread.Sleep(1);
                }
            });
            emulationThread.Start();

            // Wait for the renderer thread to exit (i.e., when the user closes the window)
            rendererThread.Join();
        }
    }
}
