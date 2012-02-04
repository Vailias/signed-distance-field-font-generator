using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
        Dictionary<char, Bitmap> dict = new Dictionary<char, Bitmap>();
        int counter = 0;

        public Form1()
        {
            InitializeComponent();
            ThreadPool.SetMaxThreads(Environment.ProcessorCount, 1);
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
            public char glyph;
            public int range;
            public Stopwatch s = new Stopwatch();
            public string filename;

            public FontState(Font f, char g, InterpolationMode i, int r, string file)
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
            byte start = 0x20;
            byte end = 0x7e;
            int range = end - start;

            progressBar1.Maximum = end - start;
            progressBar1.Step = 1;
            progressBar1.Value = progressBar1.Minimum;

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

            for (byte i = start; i <= end; i++)
            {
                char c = SignedDistanceFieldRenderer.AsciiToChar(i);

                ThreadPool.QueueUserWorkItem(
                    new WaitCallback(RenderDistanceFieldForAsciiChar),
                    new FontState(f, c, GetInterpolationMode(), range, fontSaveFile.FileName)
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

            FontState state = (FontState)obj;
            Bitmap bmp;

            lock (dict)
            {
                bmp = SignedDistanceFieldRenderer.CreateTextureAtlasFromCachedBitmaps(dict, w, h);
            }

            bmp.Save(state.filename, ImageFormat.Png);
            state.s.Stop();

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

            Bitmap bmp = SignedDistanceFieldRenderer.RenderDistanceFieldForChar(
                state.glyph, state.font, state.interpolation);

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

        private void button3_Click(object sender, EventArgs e)
        {
            svgOpenFile.Filter = "SVG Files (*.svg)|*.svg|All files (*.*)|*.*";
            if (svgOpenFile.ShowDialog() != DialogResult.OK) return;

            lastSvgFilename = svgOpenFile.FileName;
            decalPreview.Image = SignedDistanceFieldRenderer.RenderSvgToBitmapWithMaximumSize(
                svgOpenFile.FileName, decalPreview.Width, decalPreview.Height);
        }

        private string lastSvgFilename = "";

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastSvgFilename))
            {
                svgSaveFile.Filter = "PNG Files (*.png)|*.png|All files (*.*)|*.*";
                if (svgSaveFile.ShowDialog() != DialogResult.OK) return;
                SignedDistanceFieldRenderer.RenderSvgToDistanceFieldToFile(lastSvgFilename, svgSaveFile.FileName);
            }
        }
    }

    
}
