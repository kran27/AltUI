using System.Drawing;

namespace AltUI.Extensions
{
    internal static class BitmapExtensions
    {
        internal static Bitmap SetColor(this Bitmap bitmap, Color color)
        {
            var newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (var i = 0; i < bitmap.Width; i++)
            {
                for (var j = 0; j < bitmap.Height; j++)
                {
                    var pixel = bitmap.GetPixel(i, j);
                    if (pixel.A > 0)
                        newBitmap.SetPixel(i, j, color);
                }
            }
            return newBitmap;
        }

        internal static Bitmap ChangeColor(this Bitmap bitmap, Color oldColor, Color newColor)
        {
            var newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (var i = 0; i < bitmap.Width; i++)
            {
                for (var j = 0; j < bitmap.Height; j++)
                {
                    var pixel = bitmap.GetPixel(i, j);
                    if (pixel == oldColor)
                        newBitmap.SetPixel(i, j, newColor);
                }
            }
            return newBitmap;
        }
    }
}
