using Love;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using System.Numerics;

namespace AudioVisualizer
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
        private WaveBuffer buffer;

        private static int size = 10;

        private Complex[] values;

        private readonly int count = 150;

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
            buffer = new WaveBuffer(e.Buffer); // save the buffer in the class variable

            int len = buffer.FloatBuffer.Length / 8;

            // FFT
            values = new Complex[len];
            for (int i = 0; i < len; i++)
                values[i] = new Complex(buffer.FloatBuffer[i], 0.0);
            Fourier.Forward(values, FourierOptions.Default);
        }

        private void DrawVis(int i, float size, double value)
        {
            int windowHeight = Graphics.GetHeight();
            value *= windowHeight / 2;

            value /= 3;

            for (float l = 0; l < value; l++)
            {
                float u = l / windowHeight;
                Graphics.SetColor(u, 1 - u, 0);
                Graphics.Line(i * size, windowHeight - l, (i + 1) * size, windowHeight - l);
            }
        }

        public override void Draw()
        {
            Graphics.SetColor(1, 1, 1);
            if (buffer == null)
            {
                Graphics.Print("No buffer available");
                return;
            }

            size = Graphics.GetWidth() / count;

            for (int i = 0; i < count; i++)
            {
                DrawVis(i, size, values[i].Magnitude);
            }
        }

        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            if (key == KeyConstant.F) Window.SetFullscreen(!Window.GetFullscreen());
            if (key == KeyConstant.Escape) Event.Quit();
        }
    }
}
