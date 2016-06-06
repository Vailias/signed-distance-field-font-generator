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
            this.SizeSelectBox_X.SelectedIndex = 5;
        }

        Win32.TEXTMETRIC GetTextMetrics(Font f)
        {
            Win32.TEXTMETRIC t;
            IntPtr hdc = Win32.GetDC(Handle);
            Win32.SelectObject(hdc, f.ToHfont());
            Win32.GetTextMetrics(hdc, out t);
            return t;
        }
        #region FontForm
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

            progressBar1.Maximum = range;
            progressBar1.Step = 1;
            progressBar1.Value = progressBar1.Minimum;

            fontSaveFile.Filter = "PNG file (*.png)|*.png|All files (*.*)|*.*";
            if (fontSaveFile.ShowDialog() != DialogResult.OK) return;

            var fnt =
             (from font in System.Drawing.FontFamily.Families
              where font.Name == fontList.SelectedItem.ToString()
              select font).First();

            Font f = FindFontWithDimensions(fnt, 256, 256, 16);

            for (byte i = start; i <= end; i++)
            {
                char c = SignedDistanceFieldRenderer.AsciiToChar(i);

                ThreadPool.QueueUserWorkItem(
                    new WaitCallback(RenderDistanceFieldForAsciiChar),
                    new FontState(f, c, GetInterpolationMode(), range, fontSaveFile.FileName)
                );
            }
        }

        private static Font FindFontWithDimensions(FontFamily fnt, int width, int height, int border)
        {
            Font f;

            float emSize = width; // M-width
            while (true)
            {
                f = new Font(fnt, emSize, GraphicsUnit.Pixel);
                if (f.Height < height - 32) break;
                emSize--;
            }

            return f;
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
            //Todo: Revise this to be more freeform
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
                state.glyph, state.font, 256, 256, state.interpolation);

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
        #endregion
        #region Decal Functions

        private string lastSvgFilename = "";
        bool DecalLoaded = false;
        Tuple<float, float> lastWidthHeight = new Tuple<float, float>(0, 0);
        private void button3_Click(object sender, EventArgs e)//load button
        {
            //Todo: Add ability to load image file formats also
            svgOpenFile.Filter = "SVG Files (*.svg)|*.svg|All files (*.*)|*.*";
            if (svgOpenFile.ShowDialog() != DialogResult.OK) return;

            lastSvgFilename = svgOpenFile.FileName;
            //to minimize disk IO this is only opened once here and when it needs to be rerendered. 
            SvgDocument d = SvgDocument.Open(svgOpenFile.FileName);
            lastWidthHeight = new Tuple<float, float>(d.Width.Value, d.Height.Value);
            DecalLoaded = true;

            decalPreview.Image = RenderSVGToFit(svgOpenFile.FileName, decalPreview.Width, decalPreview.Height);
           
        }

        private Bitmap RenderSVGToFit(string FileName, int width, int height) 
        {
            
            float documentAspect = lastWidthHeight.Item1 / lastWidthHeight.Item2;
            int renderheight, renderwidth;
                if ((documentAspect <= 1.0f)) 
                {
                    renderheight = height;
                    renderwidth = (int)(height * documentAspect);

                }
                else {
                    renderheight = (int)(width * (1.0f/ documentAspect));
                    renderwidth = width;
                }

            return SignedDistanceFieldRenderer.RenderSvgToBitmapWithMaximumSize(FileName, renderwidth, renderheight);
         }

        private Tuple<int,int> GetRenderToFitSizes(int width, int height)
        {
            //there's probably a more elegant way to do this, but it works.
            int renderheight, renderwidth;
            float displayAspect = width / height;
            if (displayAspect > 1) //width greater than height
            {
                renderheight = height;
                renderwidth = (int)(width * (1.0f / displayAspect));

            }
            else {
                renderheight = (int)(height * displayAspect);
                renderwidth = width;
            }
            return new Tuple<int, int>(renderwidth, renderheight);
        }

        private void button2_Click(object sender, EventArgs e)//generate button
        {
            if (!string.IsNullOrEmpty(lastSvgFilename))
            {
                svgSaveFile.Filter = "PNG Files (*.png)|*.png|All files (*.*)|*.*";
                if (svgSaveFile.ShowDialog() != DialogResult.OK) return;
                int scalefactor = this.SizeSelectBox_X.SelectedIndex;
                int spreadFactor = (int)this.SpreadSelector.Value;
                SignedDistanceFieldRenderer.RenderSvgToDistanceFieldToFile(lastSvgFilename, svgSaveFile.FileName, scalefactor, spreadFactor);
                
            }
        }


        #endregion

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (DecalLoaded)
            {
                decalPreview.Image = RenderSVGToFit(lastSvgFilename, decalPreview.Width, decalPreview.Height);
            }
        }
    }

    
}
