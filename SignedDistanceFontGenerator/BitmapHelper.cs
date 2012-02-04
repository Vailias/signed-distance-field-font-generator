using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

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
    }
}