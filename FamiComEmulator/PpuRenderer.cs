using System.Drawing;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FamiComEmulator
{
    public class PpuRenderer : IPpuRenderer, IDisposable
    {
        private readonly string _title;
        private readonly int _width;
        private readonly int _height;
        private readonly byte[] _pixelBuffer;
        private GameWindow _window;

        // OpenGL handles
        private int _shaderProgram;
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _elementBufferObject;
        private int _textureId;

        // Vertex data for a full-screen quad
        private readonly float[] _vertices =
        {
            // positions    // texCoords
            -1.0f,  1.0f,    0.0f, 1.0f, // Top-left
             1.0f,  1.0f,    1.0f, 1.0f, // Top-right
             1.0f, -1.0f,    1.0f, 0.0f, // Bottom-right
            -1.0f, -1.0f,    0.0f, 0.0f  // Bottom-left
        };

        private readonly uint[] _indices =
        {
            0, 1, 2,
            2, 3, 0
        };

        // Lock object for thread-safe operations
        private readonly object _lock = new object();

        // Flag to signal the window to close
        private bool _shouldClose = false;

        public PpuRenderer(int width, int height, string title = "FamiCom Emulator")
        {
            _width = width;
            _height = height;
            _pixelBuffer = new byte[width * height * 4]; // RGBA
            _title = title;
        }

        /// <summary>
        /// Starts the rendering loop on the main thread.
        /// This method blocks until the window is closed.
        /// </summary>
        public void Run()
        {
            // Configure window settings
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(_width, _height),
                Title = _title,
                Flags = ContextFlags.ForwardCompatible,
                // Specify OpenGL profile and version
                APIVersion = new Version(4, 5),
                Profile = ContextProfile.Core
            };

            _window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);
            _window.Load += OnLoad;
            _window.RenderFrame += OnRenderFrame;
            _window.Resize += OnResize;
            _window.Closing += OnClosing;
            _window.Run();
        }

        private void OnLoad()
        {
            // Compile and link shaders
            _shaderProgram = CreateShaderProgram(vertexShaderSource, fragmentShaderSource);

            // Configure VAO, VBO, and EBO
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // VBO
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // EBO
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // Attribute locations
            int vertexLocation = GL.GetAttribLocation(_shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            int texCoordLocation = GL.GetAttribLocation(_shaderProgram, "aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            // Create and configure texture
            _textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0,
                          PixelFormat.Rgba, PixelType.UnsignedByte, _pixelBuffer);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Unbind to prevent accidental modifications
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Set background color
            GL.ClearColor(Color.Black);
        }

        private void OnResize(ResizeEventArgs args)
        {
            GL.Viewport(0, 0, args.Width, args.Height);
        }

        private void OnRenderFrame(FrameEventArgs args)
        {
            // Clear the screen
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Update the texture with the pixel buffer
            lock (_lock)
            {
                GL.BindTexture(TextureTarget.Texture2D, _textureId);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, _width, _height,
                                PixelFormat.Rgba, PixelType.UnsignedByte, _pixelBuffer);
            }

            // Draw the quad
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            // Unbind
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Swap buffers
            _window.SwapBuffers();
        }

        private void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _shouldClose = true;
            Dispose();
        }

        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return;

            int index = (y * _width + x) * 4;
            lock (_lock)
            {
                _pixelBuffer[index + 0] = color.B; // Blue
                _pixelBuffer[index + 1] = color.G; // Green
                _pixelBuffer[index + 2] = color.R; // Red
                _pixelBuffer[index + 3] = color.A; // Alpha
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_pixelBuffer, 0, _pixelBuffer.Length);
            }
        }

        private int CreateShaderProgram(string vertexSource, string fragmentSource)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompilation(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader);

            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            CheckProgramLinking(shaderProgram);

            // Shaders are linked into the program and can be deleted
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        private void CheckShaderCompilation(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling shader: {infoLog}");
            }
        }

        private void CheckProgramLinking(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Error linking shader program: {infoLog}");
            }
        }

        // Basic shaders for rendering the texture
        private readonly string vertexShaderSource = @"
            #version 450 core
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec2 aTexCoord;

            out vec2 TexCoord;

            void main()
            {
                gl_Position = vec4(aPosition, 0.0, 1.0);
                TexCoord = aTexCoord;
            }
        ";

        private readonly string fragmentShaderSource = @"
            #version 450 core
            out vec4 FragColor;

            in vec2 TexCoord;

            uniform sampler2D screenTexture;

            void main()
            {
                FragColor = texture(screenTexture, TexCoord);
            }
        ";

        public void Dispose()
        {
            // Clean up OpenGL resources
            GL.DeleteTexture(_textureId);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteProgram(_shaderProgram);

            _window.Close();
            _window.Dispose();
        }
    }
}
