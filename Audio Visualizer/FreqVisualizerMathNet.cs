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

        private static int vertical_smoothness = 3;
        private static int horizontal_smoothness = 1;
        private float size = 10;

        private static SmoothType smoothType = SmoothType.both;

        private List<Complex[]> smooth = new List<Complex[]>();

        private Complex[] values;

        private double pre_value = 0;

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

            // shift array
            if (smoothType == SmoothType.vertical || smoothType == SmoothType.both)
            {
                smooth.Add(values);
                if (smooth.Count > vertical_smoothness)
                    smooth.RemoveAt(0);
            }
        }
        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            base.KeyPressed(key, scancode, isRepeat);

            switch (key)
            {
                case KeyConstant.Right:
                    horizontal_smoothness++;
                    break;
                case KeyConstant.Left:
                    if (horizontal_smoothness > 1)
                        horizontal_smoothness--;
                    break;
                case KeyConstant.Down:
                    if (vertical_smoothness > 1)
                    {
                        vertical_smoothness--;
                        for (int i = 0; i < smooth.Count; i++)
                            smooth.RemoveAt(i);
                    }
                    break;
                case KeyConstant.Up:
                    vertical_smoothness++;
                    for (int i = 0; i < smooth.Count; i++)
                        smooth.RemoveAt(i);
                    break;
                case KeyConstant.H:
                    smoothType = SmoothType.horizontal;
                    break;
                case KeyConstant.V:
                    smoothType = SmoothType.vertical;
                    break;
                case KeyConstant.B:
                    smoothType = SmoothType.both;
                    break;
            }
        }

        public double vSmooth(int i, Complex[][] s)
        {
            double value = 0;

            for (int v = 0; v < s.Length; v++)
                value += Math.Abs(s[v] != null ? s[v][i].Magnitude : 0.0);

            return value / s.Length;
        }

        public double MovingAverage(Complex[] v, int i)
        {
            double value = 0;

            for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, 64); h++)
                value += v[h].Magnitude;

            return value / ((horizontal_smoothness + 1) * 2);
        }

        public double BothSmooth(int i)
        {
            var s = smooth.ToArray();

            double value = 0;

            for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, 64); h++)
                value += vSmooth(h, s);

            return value / ((horizontal_smoothness + 1) * 2);
        }

        public double hSmooth(int i)
        {
            if (i > 1)
            {
                double value = values[i].Magnitude;

                for (int h = i - horizontal_smoothness; h <= i + horizontal_smoothness; h++)
                    value += values[h].Magnitude;

                return value / ((horizontal_smoothness + 1) * 2);
            }

            return 0;
        }

        private void DrawVis(int i, double c, float size, double value)
        {
            float pre_x = 0, pre_y = 0, x = 0, y = 0;
            value *= WindowHeight / 2;

            value += BothSmooth(i - 1) + BothSmooth(i + 1);
            value /= 3;

            for (float l = 0; l < value; l++)
            {
                float u = l / WindowHeight;
                Graphics.SetColor(u, 1 - u, 0);
                Graphics.Line(i * size, WindowHeight - l, (i + 1) * size, WindowHeight - l);
            }
            pre_value = value;
        }

        public override void Draw()
        {
            Graphics.SetColor(1, 1, 1);
            if (buffer == null)
            {
                Graphics.Print("No buffer available");
                return;
            }

            Graphics.Print("FPS:" + Timer.GetFPS() + "\n1-8: visualizer mode\nLeft/right arrows: horizontal smoothness strength\n Current: " + horizontal_smoothness + "\nUp/down arrows: vertical smoothness strength\n Current: " + vertical_smoothness, 0, 0);

            size = WindowWidth / 64;

            if (smoothType == SmoothType.vertical)
            {
                var s = smooth.ToArray();
                // vertical smoothness
                for (int i = 0; i < count; i++)
                {
                    double value = 0;
                    for (int v = 0; v < s.Length; v++)
                        value += Math.Abs(s[v] != null ? s[v][i].Magnitude : 0.0);
                    value /= s.Length;

                    DrawVis(i, count, size, value);
                }
            }
            else if (smoothType == SmoothType.horizontal)
            {
                for (int i = 0; i < count; i++)
                {
                    double value = 0;
                    for (int h = Math.Max(i - horizontal_smoothness, 0); h < Math.Min(i + horizontal_smoothness, 64); h++)
                        value += values[h].Magnitude;
                    value /= ((horizontal_smoothness + 1) * 2);

                    DrawVis(i, count, size, value);
                }
            }
            else if (smoothType == SmoothType.both)
            {
                for (int i = 0; i < count; i++)
                {
                    double value = BothSmooth(i);
                    DrawVis(i, count, size, value);
                }
            }
        }
    }
}
