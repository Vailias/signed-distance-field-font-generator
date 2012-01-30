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

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = comboBox2.Items.Count - 1;

            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                comboBox1.Items.Add(font.Name);
            }

            comboBox1.SelectedIndex = 0;
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

            saveFileDialog1.Filter = "PNG file (*.png)|*.png|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            var fnt =
             (from font in System.Drawing.FontFamily.Families
              where font.Name == comboBox1.SelectedItem.ToString()
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
                    new FontState(f, i, GetInterpolationMode(), range, saveFileDialog1.FileName)
                );
            }
        }

        private InterpolationMode GetInterpolationMode()
        {
            if (comboBox2.SelectedIndex == 0) return InterpolationMode.NearestNeighbor;
            if (comboBox2.SelectedIndex == 1) return InterpolationMode.HighQualityBilinear;
            if (comboBox2.SelectedIndex == 2) return InterpolationMode.HighQualityBicubic;

            return InterpolationMode.HighQualityBilinear;
        }

        private void DrawDistanceResults(object obj)
        {
            int w = 512;
            int h = 256;

            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

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

            pictureBox1.Invoke((MethodInvoker)delegate
            {
                pictureBox1.Image = bmp;
            });

            statusStrip1.Invoke((MethodInvoker)delegate
            {
                toolStripStatusLabel1.Text = string.Format("Ran for {0} seconds", state.s.ElapsedMilliseconds / 1000.0);
            });

            bmp.Save(state.filename, ImageFormat.Png);
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
            Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {

                g.Clear(Color.White);
                g.DrawString(c.ToString(), f, Brushes.Black, new PointF(0, 0));
            }

            Grid g1 = Grid.FromBitmap(bmp, true);
            Grid g2 = Grid.FromBitmap(bmp, false);

            g1.Generate();
            g2.Generate();

            Grid diff = Grid.FromDifference(g1, g2);
            return diff.ToBitmap(interpolation);
        }
    }

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
        public float Dist() { return (float)Math.Sqrt((dx * dx) + (dy * dy)); }
    }

    class Grid
    {
        readonly int Width;
        readonly int Height;
        Point[] grid;

        readonly Point Outside = new Point(1e6f, 1e6f);
        readonly Point Inside = new Point(0, 0);

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
            if (x >= 0 && y >= 0 && x < Width && y < Height)
                grid[x + y * Width] = p;
        }

        Point Compare(Point p, int x, int y, int offsetx, int offsety)
        {
            Point other = Get(x + offsetx, y + offsety);
            other = new Point(other.dx + offsetx, other.dy + offsety);
            return other.DistSqr() < p.DistSqr() ? other : p;
        }

        public static Grid FromBitmap(Bitmap bmp, bool neg)
        {
            Grid g = new Grid(bmp.Width, bmp.Height);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                              ImageLockMode.ReadWrite,
                              PixelFormat.Format32bppArgb);

            int width = bmp.Width;
            int height = bmp.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * data.Stride + x * 4;
                    uint color = (uint)Marshal.ReadInt32(data.Scan0, idx);
                    bool set = (color & 0xff) > 0x7f;
                    g.Put(x, y, set ^ neg ? g.Inside : g.Outside);
                }
            }

            bmp.UnlockBits(data);

            return g;
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

        public static Grid FromDifference(Grid g1, Grid g2)
        {
            Debug.Assert(g1.Width == g2.Width);
            Debug.Assert(g1.Height == g2.Height);

            Grid diff = new Grid(g1.Height, g1.Width);

            for (int y = 0; y < g1.Height; y++)
            {
                for (int x = 0; x < g1.Width; x++)
                {
                    float d = g1.grid[x + y * g1.Width].Dist() - g2.grid[x + y * g1.Width].Dist();
                    diff.Put(x, y, new Point(d, 0));
                }
            }

            return diff;
        }

        public Bitmap ToBitmap(InterpolationMode interpolation)
        {
            Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format32bppRgb);
            float spread = Math.Min(Width, Height) / 8;

            float min = -spread;
            float max =  spread;

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                              ImageLockMode.ReadWrite,
                              PixelFormat.Format32bppRgb);

            int width = bmp.Width;
            int height = bmp.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * data.Stride + x * 4;
                    float dst = grid[x + y * Width].dx;
                    dst = dst < 0
                        ? -128 * (dst - min) / min
                        : 128 + 128 * dst / max;
                    uint channel = (uint)Math.Max(0, Math.Min(255, dst));
                    uint val = (channel << 24) | (channel << 16) | (channel << 8) | channel;
                    Marshal.WriteInt32(data.Scan0, idx, (int)val);
                }
            }

            bmp.UnlockBits(data);

            bmp = ResizeBitmap(bmp, Width >> 3, Height >> 3, interpolation);

            return bmp;
        }

        private Bitmap ResizeBitmap(Bitmap bmp, int w, int h, InterpolationMode interpolation)
        {
            Bitmap result = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode     = interpolation;
                g.SmoothingMode         = SmoothingMode.HighQuality;
                g.PixelOffsetMode       = PixelOffsetMode.HighQuality;
                g.CompositingQuality    = CompositingQuality.HighQuality;

                g.DrawImage(bmp, 0, 0, w, h);
            }

            return result; ;
        }
    }

}
