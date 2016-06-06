using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace SignedDistanceFontGenerator
{
    class BitmapHelper
    {
        public static Bitmap CreateNewManagedBitmap(int width, int height, out uint[] buffer)
        {
            buffer = new uint[width * height];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
            Bitmap bmp = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, pointer);
            return bmp;
        }

        public static Bitmap ResizeBitmap(Bitmap bmp, int w, int h, InterpolationMode interpolation)
        {
            Bitmap result = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
                g.InterpolationMode = interpolation;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(bmp, 0, 0, w, h);
            }

            return result; ;
        }
    }
}