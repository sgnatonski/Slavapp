namespace ImageFinder
{
    [ProtoBuf.ProtoContract]
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

        [ProtoBuf.ProtoMember(1, IsRequired = true)]
        public double[] X { get; set; }

        [ProtoBuf.ProtoMember(2, IsRequired = true)]
        public double[] Y { get; set; }
    }
}