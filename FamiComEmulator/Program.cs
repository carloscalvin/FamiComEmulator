namespace FamiComEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create the PPU renderer (assuming it's a form-based renderer)
            IPpuRenderer renderer = new PpuRenderer(3);

            // Start the renderer in a separate thread
            Thread rendererThread = new Thread(() =>
            {
                renderer.Run(); // Run the rendering loop
            });
            rendererThread.SetApartmentState(ApartmentState.STA); // Required for Windows Forms
            rendererThread.Start();

            // Wait for the renderer thread to exit (i.e., when the user closes the window)
            rendererThread.Join();
        }
    }
}
