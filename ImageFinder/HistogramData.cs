namespace SlavApp.ImageFinder
{
    public class HistogramData
    {
        public HistogramData()
        {
        }

        public HistogramData(double[][] data)
        {
            this.X = data[0];
            this.Y = data[1];
        }

        public HistogramData(double[] data1, double[] data2)
        {
            this.X = data1;
            this.Y = data2;
        }

        public double[] X { get; set; }

        public double[] Y { get; set; }
    }
}