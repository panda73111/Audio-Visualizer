using Love;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using System;
using System.Collections.Generic;
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
    class FreqVisualizerMathNet : VisualizerWindow
    {
        private WaveBuffer buffer;

        private float size = 10;

        private Complex[] values;

        private double count = 64;

        public override void Load()
        {
            WindowTitle = "Frequency Visualizer";
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

            // fft
            values = new Complex[len];
            for (int i = 0; i < len; i++)
                values[i] = new Complex(buffer.FloatBuffer[i], 0.0);
            Fourier.Forward(values, FourierOptions.Default);
        }

        private void DrawVis(int i, float size, double value)
        {
            value *= WindowHeight / 2;

            value += 1;
            value /= 3;

            for (float l = 0; l < value; l++)
            {
                float u = l / WindowHeight;
                Graphics.SetColor(u, 1 - u, 0);
                Graphics.Line(i * size, WindowHeight - l, (i + 1) * size, WindowHeight - l);
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

            size = WindowWidth / 64;

            for (int i = 0; i < count; i++)
            {
                DrawVis(i, size, values[i].Magnitude);
            }
        }
    }
}
