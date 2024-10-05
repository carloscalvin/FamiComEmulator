namespace FamiComEmulator
{
    public class PpuRenderer : IPpuRenderer
    {
        private readonly Bitmap _bitmap;
        private PictureBox _pictureBox;
        private Form _form;

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
            _form.ClientSize = new Size(_width, _height);
            _form.FormBorderStyle = FormBorderStyle.FixedSingle;
            _form.MaximizeBox = false;

            _pictureBox = new PictureBox();
            _pictureBox.Dock = DockStyle.Fill;
            _pictureBox.Image = new Bitmap(_width, _height);
            _form.Controls.Add(_pictureBox);

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
    }
}
