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
    private int _scale;

    private InputHandler _inputHandler;
    private IAudioProcessor _audioProcessor;

    public PpuRenderer(int scale = 1)
    {
        _width = 256;
        _height = 240;
        _scale = scale;
        _bitmap = new Bitmap(_width * _scale, _height * _scale);
    }

    public void SetPixel(int x, int y, Color color)
    {
        lock (_bitmap)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                for (int i = 0; i < _scale; i++)
                {
                    for (int j = 0; j < _scale; j++)
                    {
                        int scaledX = x * _scale + i;
                        int scaledY = y * _scale + j;
                        _bitmap.SetPixel(scaledX, scaledY, color);
                    }
                }
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
        _form.ClientSize = new Size(_width * _scale, _height * _scale + 50); // Extra space for the button
        _form.FormBorderStyle = FormBorderStyle.FixedSingle;
        _form.MaximizeBox = false;

        _pictureBox = new PictureBox();
        _pictureBox.Dock = DockStyle.Top;
        _pictureBox.Height = _height * _scale;
        _pictureBox.Image = new Bitmap(_width * _scale, _height * _scale);
        _form.Controls.Add(_pictureBox);

        // Add Load ROM button
        _loadRomButton = new Button();
        _loadRomButton.Text = "Load ROM";
        _loadRomButton.Dock = DockStyle.Bottom;
        _loadRomButton.Height = 40;
        _loadRomButton.Click += LoadRomButton_Click;
        _form.Controls.Add(_loadRomButton);

        // Register key events
        _form.KeyDown += Form_KeyDown;
        _form.KeyUp += Form_KeyUp;
        _form.KeyPreview = true; // Important to receive key events

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

        // Create the CPU, PPU and APU
        ICpu6502 cpu = new Cpu6502();
        IPpu2c02 ppu = new Ppu2c02(this);
        IApu apu = new Apu();

        // Create the central bus
        _bus = new CentralBus(cpu, ppu, apu);

        // Load a cartridge
        ICartridge cartridge = new Cartridge(romPath);
        _bus.AddCartridge(cartridge);

        // Reset the system
        _bus.Reset();

        // Initialize InputHandler with bus
        _inputHandler = new InputHandler(_bus);

        // Initialize the audio processor
        _audioProcessor = new AudioProcessor();

        // Start the emulation loop in a separate thread
        Thread emulationThread = new Thread(() =>
        {
            while (true)
            {
                _bus.Clock(); // Clock the CPU, PPU, and APU for each cycle

                // Process the current sample from the APU
                _audioProcessor.ProcessSample(apu.CurrentSample);
            }
        });
        emulationThread.IsBackground = true;
        emulationThread.Start();
    }

    private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        _inputHandler?.KeyDown(e.KeyCode);
    }

    private void Form_KeyUp(object sender, KeyEventArgs e)
    {
        _inputHandler?.KeyUp(e.KeyCode);
    }
}
