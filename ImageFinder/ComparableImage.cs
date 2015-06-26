namespace EyeOpen.Imaging
{
    using Newtonsoft.Json;
    using System;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Represents an image and its RGB projections.
    /// </summary>
    public class ComparableImage
    {
        private readonly FileInfo file;
        private readonly RgbProjections projections;

        public ComparableImage(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (!file.Exists)
            {
                throw new FileNotFoundException();
            }

            this.file = file;

            using (var orgBtm = new Bitmap(file.FullName))
            using (var bitmap = ImageUtility.ResizeBitmap(orgBtm, 100, 100))
            {
                projections = new RgbProjections(ImageUtility.GetRgbProjections(bitmap));
            }
        }

        public ComparableImage(FileInfo file, double[][] values)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (!file.Exists)
            {
                throw new FileNotFoundException();
            }

            this.file = file;

            projections = new RgbProjections(values);
        }

        public ComparableImage(FileInfo file, byte[] data) : this(file, FromByteArray(data))
        {
        }

        public FileInfo File
        {
            get
            {
                return file;
            }
        }

        public RgbProjections Projections
        {
            get
            {
                return projections;
            }
        }

        /// <summary>
        /// Calculate the similarity to another image.
        /// </summary>
        /// <param name="compare">The image to compare with.</param>
        /// <returns>Return a value from 0 to 1 that is the similarity.</returns>
        public double CalculateSimilarity(ComparableImage compare)
        {
            return projections.CalculateSimilarity(compare.projections);
        }

        public override string ToString()
        {
            return file.Name;
        }

        private static double[][] FromByteArray(byte[] data)
        {
            var str = System.Text.Encoding.ASCII.GetString(data);
            var r = JsonConvert.DeserializeObject<double[][]>(str);
            return r;
        }

        public byte[] ToByteArray()
        {
            var array = new [] { this.Projections.HorizontalProjection, this.Projections.VerticalProjection };
            return System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(array));
        }
    }
}
