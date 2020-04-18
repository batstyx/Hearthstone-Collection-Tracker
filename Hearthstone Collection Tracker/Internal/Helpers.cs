using System;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal
{
    public static class Helpers
    {
        public static int Clamp(this int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static string GetValidFileName(this string fileName)
        {
            StringBuilder initialString = new StringBuilder(fileName);
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                initialString = initialString.Replace(c, '_');
            }
            return initialString.ToString();
        }

        public static double GetAvgHue(this Bitmap bmp, double saturationThreshold = 0.05)
        {
            var totalHue = 0.0f;
            var validPixels = 0;
            for (var i = 0; i < bmp.Width; i++)
            {
                for (var j = 0; j < bmp.Height; j++)
                {
                    var pixel = bmp.GetPixel(i, j);

                    //ignore sparkle
                    if (pixel.GetSaturation() > saturationThreshold)
                    {
                        totalHue += pixel.GetHue();
                        validPixels++;
                    }
                }
            }

            return totalHue / validPixels;
        }

        public static double GetAvgBrightness(this Bitmap bmp, double saturationThreshold = 0.05)
        {
            var totalBrightness = 0.0f;
            var validPixels = 0;
            for (var i = 0; i < bmp.Width; i++)
            {
                for (var j = 0; j < bmp.Height; j++)
                {
                    var pixel = bmp.GetPixel(i, j);

                    //ignore sparkle
                    if (pixel.GetSaturation() > saturationThreshold)
                    {
                        totalBrightness += pixel.GetBrightness();
                        validPixels++;
                    }
                }
            }

            return totalBrightness / validPixels;
        }
    }
}
