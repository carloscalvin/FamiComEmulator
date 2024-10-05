using FamiComEmulator.Components;
using FamiComEmulator;

public class PpuRenderer : IPpuRenderer
{
    private readonly Bitmap _bitmap;
    private PictureBox _pictureBox;
    private Form _form;
    private Button _loadRomButton;
    private ICentralBus _bus;

    private readonly int _width;
    private readonly int _height;

    public PpuRenderer(int width, int height)
    {
        _width = width;
        _height = height;
        _bitmap = new Bitmap(_width, _height);
    }

    public void SetPixel(int x, int y, Color color)
    {
        lock (_bitmap)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                _bitmap.SetPixel(x, y, color);
            }
        }
    }

    public void Clear()
    {
        lock (_bitmap)
        {
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.Clear(Color.Black);
            }
        }
    }

    public void Run()
    {
        _form = new Form();
        _form.Text = "NES Emulator";
        _form.ClientSize = new Size(_width, _height + 50); // Extra space for the button
        _form.FormBorderStyle = FormBorderStyle.FixedSingle;
        _form.MaximizeBox = false;

        _pictureBox = new PictureBox();
        _pictureBox.Dock = DockStyle.Top;
        _pictureBox.Height = _height;
        _pictureBox.Image = new Bitmap(_width, _height);
        _form.Controls.Add(_pictureBox);

        // Add Load ROM button
        _loadRomButton = new Button();
        _loadRomButton.Text = "Load ROM";
        _loadRomButton.Dock = DockStyle.Bottom;
        _loadRomButton.Height = 40;
        _loadRomButton.Click += LoadRomButton_Click;
        _form.Controls.Add(_loadRomButton);

        // Start a timer to refresh the picture box periodically
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        timer.Interval = 16; // Approximately 60fps
        timer.Tick += (sender, e) =>
        {
            lock (_bitmap)
            {
                using (Graphics g = Graphics.FromImage(_pictureBox.Image))
                {
                    g.DrawImageUnscaled(_bitmap, 0, 0);
                }
                _pictureBox.Invalidate();
            }
        };
        timer.Start();

        Application.Run(_form);
    }

    private void LoadRomButton_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "NES ROM Files (*.nes)|*.nes|All Files (*.*)|*.*";
            openFileDialog.Title = "Select NES ROM";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string romPath = openFileDialog.FileName;

                // Load the selected ROM and initialize the emulation
                LoadRomAndStartEmulation(romPath);
            }
        }
    }

    private void LoadRomAndStartEmulation(string romPath)
    {
        if (!System.IO.File.Exists(romPath))
        {
            MessageBox.Show("ROM file not found at path: " + romPath);
            return;
        }

        // Create the CPU and PPU
        ICpu6502 cpu = new Cpu6502();
        IPpu2c02 ppu = new Ppu2c02(this);

        // Create the central bus
        _bus = new CentralBus(cpu, ppu);

        // Load a cartridge
        ICartridge cartridge = new Cartridge(romPath);
        _bus.AddCartridge(cartridge);

        // Reset the system
        _bus.Reset();

        // Start the emulation loop in a separate thread
        Thread emulationThread = new Thread(() =>
        {
            while (true)
            {
                _bus.Clock(); // Clock the CPU and PPU for each cycle
            }
        });
        emulationThread.Start();
    }
}
