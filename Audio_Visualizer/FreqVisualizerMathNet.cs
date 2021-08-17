using Love;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using System.Numerics;

namespace Audio_Visualizer
{
    enum SmoothType
    {
        horizontal,
        vertical,
        both
    }

    /*
     * Visualizer using frequencies
     */
    class FreqVisualizerMathNet : Scene
    {
        private WaveBuffer _buffer;

        private Complex[] _values;
        private VolumeLevel[] _levels;

        private const int Count = 150;

        private enum VolumeLevel { Low, Middle, High };

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

        private void DrawVis(int i, double value)
        {
            int windowHeight = Graphics.GetHeight();
            float barWidth = Graphics.GetWidth() / Count + 0.5f;
            value *= windowHeight / 2;

            value /= 3;

            for (float l = 0; l < value; l++)
            {
                float u = l / windowHeight;
                Graphics.SetColor(u, 1 - u, 0);
                Graphics.Line(i * barWidth, windowHeight - l, (i + 1) * barWidth, windowHeight - l);
            }
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
                DrawVis(i, _values[i].Magnitude);
            }
        }

        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            if (key == KeyConstant.F) Window.SetFullscreen(!Window.GetFullscreen());
            if (key == KeyConstant.Escape) Event.Quit();
        }
    }
}
