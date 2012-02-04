using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SignedDistanceFontGenerator
{
    struct Point
    {
        public float dx;
        public float dy;

        public Point(float dx_, float dy_)
        {
            dx = dx_;
            dy = dy_;
        }

        public float DistSqr() { return (dx * dx) + (dy * dy); }
        public float Dist() { return (float)Math.Sqrt(DistSqr()); }
    }

    class Grid
    {
        readonly int Width;
        readonly int Height;
        Point[] grid;

        static Point Outside = new Point(1e6f, 1e6f);
        static Point Inside = new Point(0, 0);

        private Grid(int width, int height)
        {
            Width = width;
            Height = height;
            grid = new Point[width * height];
        }

        Point Get(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
                return grid[x + y * Width];
            else
                return Outside;
        }

        void Put(int x, int y, Point p)
        {
            grid[x + y * Width] = p;
        }

        Point Compare(Point p, int x, int y, int offsetx, int offsety)
        {
            Point other = Get(x + offsetx, y + offsety);
            other = new Point(other.dx + offsetx, other.dy + offsety);
            return other.DistSqr() < p.DistSqr() ? other : p;
        }

        public static void FromBitmap(out Grid g1, out Grid g2, int width, int height, uint[] buffer)
        {
            g1 = new Grid(width, height);
            g2 = new Grid(width, height);

            Point inside = Inside;
            Point outside = Outside;

            int idx = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    uint color = buffer[idx];
                    bool set = (color & 0xff) > 0x7f;

                    if (set)
                    {
                        g2.Put(x, y, inside);
                        g1.Put(x, y, outside);
                    }
                    else
                    {
                        g1.Put(x, y, inside);
                        g2.Put(x, y, outside);
                    }

                    idx++;
                }
            }
        }

        public void Generate()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Point p = Get(x, y);
                    p = Compare(p, x, y, -1, 0);
                    p = Compare(p, x, y, 0, -1);
                    p = Compare(p, x, y, -1, -1);
                    p = Compare(p, x, y, 1, -1);
                    Put(x, y, p);
                }

                for (int x = Width - 1; x >= 0; x--)
                {
                    Point p = Get(x, y);
                    p = Compare(p, x, y, 1, 0);
                    Put(x, y, p);
                }
            }

            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = Width - 1; x >= 0; x--)
                {
                    Point p = Get(x, y);
                    p = Compare(p, x, y, 1, 0);
                    p = Compare(p, x, y, 0, 1);
                    p = Compare(p, x, y, -1, 1);
                    p = Compare(p, x, y, 1, 1);
                    Put(x, y, p);
                }

                for (int x = 0; x < Width; x++)
                {
                    Point p = Get(x, y);
                    p = Compare(p, x, y, -1, 0);
                    Put(x, y, p);
                }
            }
        }

        public static Bitmap ToBitmap(Grid g1, Grid g2, int scalefactor, InterpolationMode interpolation)
        {
            uint[] buffer;
            Bitmap bmp = BitmapHelper.CreateNewManagedBitmap(g1.Width, g1.Height, out buffer);
            float spread = Math.Min(g1.Width, g1.Height) / (1 << scalefactor);

            float min = -spread;
            float max = spread;

            int width = bmp.Width;
            int height = bmp.Height;

            int idx = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dst = g1.grid[idx].Dist() - g2.grid[idx].Dist();

                    dst = dst < 0
                        ? -128 * (dst - min) / min
                        : 128 + 128 * dst / max;

                    uint channel = (uint)Math.Max(0, Math.Min(255, dst));
                    uint val = (channel << 24) | (channel << 16) | (channel << 8) | channel;

                    buffer[idx] = val;
                    idx++;
                }
            }

            bmp = ResizeBitmap(bmp, g1.Width >> scalefactor, g1.Height >> scalefactor, interpolation);

            return bmp;
        }

        private static Bitmap ResizeBitmap(Bitmap bmp, int w, int h, InterpolationMode interpolation)
        {
            Bitmap result = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = interpolation;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(bmp, 0, 0, w, h);
            }

            return result; ;
        }
    }
}
