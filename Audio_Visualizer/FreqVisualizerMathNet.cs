using System;
using Love;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using System.IO.Ports;
using System.Linq;
using System.Numerics;

namespace Audio_Visualizer
{
    /*
     * Visualizer using frequencies
     */
    internal class FreqVisualizerMathNet : Scene
    {
        private WaveBuffer _buffer;

        private Complex[] _values;

        private const int Count = 211;

        private const int BaudRate = 2000000;

        private enum VolumeLevel : byte { Low, Middle, High }

        private SerialPort _port;
        private static readonly byte[] Buffer = new byte[Count];

        public override void Load()
        {
            Window.SetTitle("Frequency Visualizer");
            base.Load();

            // start audio capture
            var capture = new WasapiLoopbackCapture();

            capture.DataAvailable += DataAvailable;

            capture.RecordingStopped += (s, a) =>
            {
                capture.Dispose();
            };

            string[] ports = SerialPort.GetPortNames();
            if (ports.Any())
            {
                _port = new SerialPort(ports.Last(), BaudRate, Parity.Even);
                _port.Open();
            }

            capture.StartRecording();
        }

        public void DataAvailable(object sender, WaveInEventArgs e)
        {
            _buffer = new WaveBuffer(e.Buffer); // save the buffer in the class variable

            int len = _buffer.FloatBuffer.Length / 8;

            // FFT
            _values = new Complex[len];
            for (int i = 0; i < len; i++)
                _values[i] = new Complex(_buffer.FloatBuffer[i], 0.0);
            Fourier.Forward(_values, FourierOptions.Default);
        }

        private static void DrawVis(int i, double value, SerialPort port = null)
        {
            int windowHeight = Graphics.GetHeight();
            double middleLevelMinimum = windowHeight / 50d;
            double highLevelMinimum = 25 * middleLevelMinimum;
            float barWidth = Graphics.GetWidth() / (Count + 0.5f);
            value *= windowHeight / 2d;

            value /= 3;

            for (float l = 2 * barWidth; l < value; l++)
            {
                float u = l / windowHeight;
                Graphics.SetColor(u, 1 - u, 0);
                Graphics.Line(i * barWidth, windowHeight - l, (i + 1) * barWidth, windowHeight - l);
            }

            VolumeLevel level = (byte)VolumeLevel.Low;
            if (value > highLevelMinimum)
                level = VolumeLevel.High;
            else if (value > middleLevelMinimum)
                level = VolumeLevel.Middle;

            if (port != null)
            {
                Buffer[i] = (byte)level;
                port.Write(Buffer, 0, Count);
            }

            switch (level)
            {
                case VolumeLevel.Low:
                    Graphics.SetColor(Color.Green);
                    break;
                case VolumeLevel.Middle:
                    Graphics.SetColor(Color.Yellow);
                    break;
                case VolumeLevel.High:
                    Graphics.SetColor(Color.Red);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Graphics.Circle(DrawMode.Fill, new Love.Vector2((i + 0.5f) * barWidth, windowHeight - barWidth), barWidth / 2f);
        }

        public override void Draw()
        {
            Graphics.SetColor(1, 1, 1);
            if (_buffer == null)
            {
                Graphics.Print("No buffer available");
                return;
            }

            for (int i = 0; i < Count; i++)
            {
                DrawVis(i, _values[i].Magnitude, _port);
            }
        }

        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            if (key == KeyConstant.F) Window.SetFullscreen(!Window.GetFullscreen());
            if (key == KeyConstant.Escape) Event.Quit();
        }
    }
}
