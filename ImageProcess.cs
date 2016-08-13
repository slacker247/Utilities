using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class ImageProcess
    {
        // http://stats.stackexchange.com/questions/39037/how-does-neural-network-recognise-images
        public static Bitmap convertToGreyScale(Bitmap bmp)
        {
            int rgb;
            Color c;
            Bitmap img = (Bitmap)bmp.Clone();

            for (int i = 0; i < img.Height * img.Width; i++)
            {
                int y = i/img.Width;
                int x = i - img.Width * y;
                c = img.GetPixel(x, y);
                rgb = (int)((c.R + c.G + c.B) / 3);
                img.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
            }
            return img;
        }

        public static Bitmap scaleImage(Bitmap image, int percent)
        {
            double perc = (double)percent / 100d;
            return scaleImage(image, (int)(image.Width * perc), (int)(image.Height * perc));
        }

        public static Bitmap scaleImage(Bitmap image, int maxWidth, int maxHeight)
        {
            double ratioX = (double)maxWidth / (double)image.Width;
            double ratioY = (double)maxHeight / (double)image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap newImage = stretchImage(image, newWidth, newHeight);

            return newImage;
        }

        public static Bitmap stretchImage(Bitmap image, int width, int height)
        {
            Bitmap newImage = new Bitmap(width, height);

            Graphics graphics = Graphics.FromImage(newImage);
            graphics.DrawImage(image, 0, 0, width, height);
            graphics.Flush();
            graphics.Dispose();

            return newImage;
        }

        public static void saveImage(byte[] bytes, String fileName)
        {
            Image img = null;
            using (var ms = new MemoryStream(bytes))
            {
                img = Image.FromStream(ms);
                img.Save(fileName);
            }
        }

        public static long rgb2kelvin(Color color)
        {
            return 0;
        }
    }
}
