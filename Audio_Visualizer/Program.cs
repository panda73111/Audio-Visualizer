using Love;

namespace Audio_Visualizer
{
    class Program
    {
        static void Main()
        {
            BootConfig config = new BootConfig() { WindowResizable = true };
            Boot.Run(new FreqVisualizerMathNet(), config);
        }
    }
}
