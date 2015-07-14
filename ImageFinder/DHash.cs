using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.ImageFinder
{
    public class DHash
    {
        public ulong Compute(string filename)
        {
            var bits = new BitArray(64);
            var img = ToGrayscaleAndScale(Image.FromFile(filename), 9, 8);
            for(var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    var p1 = img.GetPixel(x, y);
                    var p2 = img.GetPixel(x + 1, y);
                    bool diff = (p1.R + p1.G + p1.B) < (p2.R + p2.G + p2.B);
                    bits.Set(y * 8 + x, diff);
                }
            }

            ulong[] array = new ulong[1];
            bits.CopyTo(array, 0);
            return array[0];
        }

        static Bitmap ToGrayscaleAndScale(Image imgPhoto, int width, int height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)width / (float)sourceWidth);
            nPercentH = ((float)height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((height - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Red);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            ColorMatrix colorMatrix = new ColorMatrix(new float[][] 
            {
                new float[] {.3f, .3f, .3f, 0, 0},
                new float[] {.59f, .59f, .59f, 0, 0},
                new float[] {.11f, .11f, .11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            grPhoto.DrawImage(imgPhoto, new Rectangle(destX, destY, destWidth, destHeight), sourceX, sourceY, sourceWidth, sourceHeight, GraphicsUnit.Pixel, attributes);

            grPhoto.Dispose();
            return bmPhoto;
        }
    }
}
