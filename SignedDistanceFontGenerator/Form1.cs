using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Drawing2D;
using Svg;

/// TODO:
///  - Output .font file (JSON format) with:
///     - UVs for rendering
///     - Kerning and other relevant glyph information
///  - Adjust fitting algorithm using GetTextMetrics?

namespace SignedDistanceFontGenerator
{
    public partial class Form1 : Form
    {
        Dictionary<int, Bitmap> dict = new Dictionary<int,Bitmap>();
        int counter = 0;

        public Form1()
        {
            InitializeComponent();
        }

        Bitmap RenderSvgToDistanceField(string filename)
        {
            uint[] buffer;
            int width = 4096;
            int height = 4096;
            SvgDocument d = SvgDocument.Open(@"c:\emoticons.svg");
            float aspect = d.Width.Value / d.Height.Value;
            d.Width = new SvgUnit(SvgUnitType.Pixel, 4096.0f * aspect);
            d.Height = new SvgUnit(SvgUnitType.Pixel, 4096.0f);
            Bitmap svg = d.Draw();
            Bitmap bmp = BitmapHelper.CreateNewManagedBitmap(width, height, out buffer);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(svg, 0, 0);
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = x + y * width;

                    uint r = buffer[idx] & 0xff;
                    uint g = (buffer[idx] & 0xff00) >> 8;
                    uint b = (buffer[idx] & 0xff0000) >> 16;
                    uint a = (buffer[idx] & 0xff000000) >> 24;

                    uint gray = (r + g + b) / 3;

                    if (a == 0) // assume background
                    {
                        buffer[idx] = 0xffffffff;
                    }
                    else if (gray < 127)
                    {
                        buffer[idx] = 0xff000000;
                    }
                    else
                    {
                        buffer[idx] = 0xffffffff;
                    }
                }
            }

            return CreateDistanceField(InterpolationMode.HighQualityBicubic, width, height, 5, buffer);
        }

        Win32.TEXTMETRIC GetTextMetrics(Font f)
        {
            Win32.TEXTMETRIC t;
            IntPtr hdc = Win32.GetDC(Handle);
            Win32.SelectObject(hdc, f.ToHfont());
            Win32.GetTextMetrics(hdc, out t);
            return t;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fontFilterMethod.SelectedIndex = fontFilterMethod.Items.Count - 1;

            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                fontList.Items.Add(font.Name);
            }

            fontList.SelectedIndex = 0;
        }

        class FontState
        {
            public Font font;
            public InterpolationMode interpolation;
            public int glyph;
            public int range;
            public Stopwatch s = new Stopwatch();
            public string filename;

            public FontState(Font f, int g, InterpolationMode i, int r, string file)
            {
                font = f;
                glyph = g;
                interpolation = i;
                range = r;
                filename = file;
                s.Start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // ascii printable range
            counter = 0;
            int start = 0x20;
            int end = 0x7e;
            int range = end - start;

            progressBar1.Maximum = end - start;
            progressBar1.Step = 1;
            progressBar1.Value = progressBar1.Minimum;

            ThreadPool.SetMaxThreads(Environment.ProcessorCount, 1);

            fontSaveFile.Filter = "PNG file (*.png)|*.png|All files (*.*)|*.*";
            if (fontSaveFile.ShowDialog() != DialogResult.OK) return;

            var fnt =
             (from font in System.Drawing.FontFamily.Families
              where font.Name == fontList.SelectedItem.ToString()
              select font).First();

            // find a font-height that won't leak outside the image
            Font f;
            float emSize = 256; // M-width
            while (true)
            {
                f = new Font(fnt, emSize, GraphicsUnit.Pixel);
                if (f.Height < 256 - 32) break;
                emSize--;
            }

            for (int i = start; i <= end; i++)
            {
                ThreadPool.QueueUserWorkItem(
                    new WaitCallback(RenderDistanceFieldForAsciiChar),
                    new FontState(f, i, GetInterpolationMode(), range, fontSaveFile.FileName)
                );
            }
        }

        private InterpolationMode GetInterpolationMode()
        {
            if (fontFilterMethod.SelectedIndex == 0) return InterpolationMode.NearestNeighbor;
            if (fontFilterMethod.SelectedIndex == 1) return InterpolationMode.HighQualityBilinear;
            if (fontFilterMethod.SelectedIndex == 2) return InterpolationMode.HighQualityBicubic;

            return InterpolationMode.HighQualityBilinear;
        }

        private void DrawDistanceResults(object obj)
        {
            int w = 512;
            int h = 256;

            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                lock (dict)
                {
                    foreach(KeyValuePair<int, Bitmap> k in dict)
                    {
                        int idx = (k.Key - 0x20);
                        int x = idx % (w / k.Value.Width);
                        int y = idx / (w / k.Value.Width);

                        g.DrawImage(k.Value, x * k.Value.Width, y * k.Value.Height, k.Value.Width, k.Value.Height);
                    }
                }
            }

            FontState state = (FontState)obj;
            state.s.Stop();

            bmp.Save(state.filename, ImageFormat.Png);

            fontPreview.Invoke((MethodInvoker)delegate
            {
                fontPreview.Image = new Bitmap(bmp);
            });

            statusStrip1.Invoke((MethodInvoker)delegate
            {
                toolStripStatusLabel1.Text = string.Format("Ran for {0} seconds", state.s.ElapsedMilliseconds / 1000.0);
            });
        }

        private void RenderDistanceFieldForAsciiChar(object obj)
        {
            FontState state = (FontState)obj;

            // convert ascii to unicode
            byte[] input = { (byte)(int)state.glyph };
            byte[] output = Encoding.Convert(Encoding.ASCII, Encoding.Unicode, input);
            char[] c = new char[Encoding.Unicode.GetCharCount(output, 0, output.Length)];
            Encoding.Unicode.GetChars(output, 0, output.Length, c, 0);
            Debug.Assert(c.Length == 1); // one char in should be one out.

            Bitmap bmp = RenderDistanceFieldForChar(c[0], state.font, state.interpolation);

            lock (dict)
            {
                dict[state.glyph] = bmp;
            }

            statusStrip1.Invoke((MethodInvoker)delegate
            {
                progressBar1.PerformStep();
            });

            if (Interlocked.Increment(ref counter) > state.range)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DrawDistanceResults), obj);
            }
        }

        private Bitmap RenderDistanceFieldForChar(char c, Font f, InterpolationMode interpolation)
        {
            int width = 256;
            int height = 256;
            uint[] buffer;

            Bitmap bmp = BitmapHelper.CreateNewManagedBitmap(width, height, out buffer);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawString(c.ToString(), f, Brushes.Black, new PointF(0, 0));
            }

            return CreateDistanceField(interpolation, width, height, 3, buffer);
        }

        private static Bitmap CreateDistanceField(InterpolationMode interpolation, int width, int height, int scale, uint[] buffer)
        {
            Grid g1, g2;
            Grid.FromBitmap(out g1, out g2, width, height, buffer);

            g1.Generate();
            g2.Generate();

            return Grid.ToBitmap(g1, g2, scale, interpolation);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            svgOpenFile.Filter = "SVG Files (*.svg)|*.svg|All files (*.*)|*.*";
            if (svgOpenFile.ShowDialog() != DialogResult.OK) return;

            lastSvgFilename = svgOpenFile.FileName;

            SvgDocument svg = SvgDocument.Open(svgOpenFile.FileName);
            float aspect = svg.Width.Value / svg.Height.Value;
            svg.Width = new SvgUnit(SvgUnitType.Pixel, decalPreview.Width * aspect);
            svg.Height = new SvgUnit(SvgUnitType.Pixel, decalPreview.Height);
            Bitmap bmp = svg.Draw();
            decalPreview.Image = bmp;
        }

        private string lastSvgFilename = "";

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastSvgFilename))
            {
                svgSaveFile.Filter = "PNG Files (*.png)|*.png|All files (*.*)|*.*";
                if (svgSaveFile.ShowDialog() != DialogResult.OK) return;
                RenderSvgToDistanceField(lastSvgFilename).Save(svgSaveFile.FileName);
            }
        }
    }

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

    [StructLayout(LayoutKind.Explicit, Size=8)]
    struct Point
    {
        [FieldOffset(0)]
        public float dx;

        [FieldOffset(4)]
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
                    p = Compare(p, x, y, -1,  0);
                    p = Compare(p, x, y,  0, -1);
                    p = Compare(p, x, y, -1, -1);
                    p = Compare(p, x, y,  1, -1);
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
                    p = Compare(p, x, y,  1, 0);
                    p = Compare(p, x, y,  0, 1);
                    p = Compare(p, x, y, -1, 1);
                    p = Compare(p, x, y,  1, 1);
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
            float max =  spread;

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

    class Win32
    {
        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GLYPHMETRICS
        {
            public int gmBlackBoxX;
            public int gmBlackBoxY;
            [MarshalAs(UnmanagedType.Struct)]
            public POINT gmptGlyphOrigin;
            public short gmCellIncX;
            public short gmCellIncY;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FIXED
        {

            public short fract;
            public short value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MAT2
        {
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED eM11;
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED eM12;
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED eM21;
            [MarshalAs(UnmanagedType.Struct)]
            public FIXED eM22;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC lptm);

        [DllImport("gdi32.dll")]
        static extern uint GetGlyphOutline(IntPtr hdc, uint uChar, uint uFormat,
           out GLYPHMETRICS lpgm, uint cbBuffer, IntPtr lpvBuffer, ref MAT2 lpmat2);
    }

}
