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
            this.SizeSelectBox_Y.SelectedIndex = 5;
            if (!DecalLoaded)
            {
                this.GenerateButton.Enabled = false;
            }
            initializeWaitGraphic();
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

        //CONSIDER: break this to a class?
        private string lastSourceFilename = "";
        bool DecalLoaded = false;
        Tuple<float, float> lastWidthHeight = new Tuple<float, float>(0, 0);
        private Bitmap SourceBitmap;
        private float SourceAspect;
        private Bitmap lastGenerated;
        private bool Rendered = false;

        private void initializeWaitGraphic()
        {
            int boxDim = 32;
            WaitBox.SizeMode = PictureBoxSizeMode.StretchImage;
            WaitBox.Size = new Size(boxDim, boxDim);
            centerWaitGraphic();
            WaitBox.Enabled = false;
            WaitBox.Visible = false;
        }
        private void centerWaitGraphic()
        {
            WaitBox.Location = new System.Drawing.Point((int)(decalPreview.Width / 2 - WaitBox.Width / 2), (int)(decalPreview.Top + (decalPreview.Height / 2 - WaitBox.Height / 2)));
        }

        //todo: Make graphic ACTUALLY transparent. Will require a custom class.
        //REF: http://www.codeproject.com/Articles/25048/How-to-Use-Transparent-Images-and-Labels-in-Window

        delegate void waitGraphicEnableDelegate(bool enable);

        private void waitGraphicEnable(bool enable)
        {
            if (this.WaitBox.InvokeRequired)
            {
                waitGraphicEnableDelegate d = new waitGraphicEnableDelegate(waitGraphicEnable);
                this.Invoke(d, new object[] { enable });
            }
            else
            {
                this.WaitBox.Enabled = enable;
                this.WaitBox.Visible = enable;
            }
        }

        private void ButtonLoad_onClick(object sender, EventArgs e)//load button
        {
            //Todo: Add ability to load image file formats also
            //OpenFileDialogue.Filter = "SVG Files (*.svg)|*.svg|Image files (*.png, *.tiff, *.tif)|*.png;.tiff;.tif|All files (*.*)|*.*";
            //This may be a significant undertaking
            //Most of these routines are built with an SVG being assumed.

            OpenFileDialogue.Filter = "SVG Files (*.svg)|*.svg|All files (*.*)|*.*";
            if (OpenFileDialogue.ShowDialog() != DialogResult.OK) return;

                lastSourceFilename = OpenFileDialogue.FileName;
            
            
            //to minimize disk IO this is only opened once here and when it needs to be rerendered for actual field generation. 
            SvgDocument d = SvgDocument.Open(OpenFileDialogue.FileName);
            lastWidthHeight = new Tuple<float, float>(d.Width.Value, d.Height.Value);
            DecalLoaded = true;
            this.GenerateButton.Enabled = true;
            SourceAspect = lastWidthHeight.Item1 / lastWidthHeight.Item2;
            int sourceHeight = 4096;
            int sourceWidth = (int)(4096*(1.0f/SourceAspect));
            
            //rendered once to memory. That file in memory is downsampled for the preview image as the window is resized.
            SourceBitmap = SignedDistanceFieldRenderer.RenderSvgToBitmapWithMaximumSize(lastSourceFilename, sourceWidth, sourceHeight);
            decalPreview.Image = BitmapHelper.ResizeBitmapToFit(SourceBitmap, decalPreview.Width, decalPreview.Height, SourceAspect);
     
        }

     

        //TODO: put this in the renderer class where it fits better. Will need to convert lastWidthHeight to a variable passed in.
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

        private void GenerateButton_onClick(object sender, EventArgs e)//generate button
        {
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;
            progressBar1.Value = progressBar1.Minimum;

            if (!string.IsNullOrEmpty(lastSourceFilename))
            {


                //reWork this so that the source image is rendered as some multiple of the target image size.

                //change scale factor to a tuple to indicate x and y sizing.
                Thread gThread, iThread;
                int scalefactor = this.SizeSelectBox_X.SelectedIndex;
                int scaleFactorH = this.SizeSelectBox_Y.SelectedIndex;
                int spreadFactor = (int)this.SpreadSelector.Value;
                float aspect = lastWidthHeight.Item1 / lastWidthHeight.Item2;
                //separate render thread. This is the real time sink for this routime.
                gThread = new Thread(() =>
                {
                    Debug.Print("Generating distance field from " + lastSourceFilename.ToString());
                    lastGenerated = SignedDistanceFieldRenderer.RenderSvgToDistanceFieldToBMP(lastSourceFilename, scalefactor, spreadFactor, (int)(4096 * aspect), 4096);
                    decalPreview.Image = lastGenerated;
                    Debug.Print("Distance field generated.");
                    Rendered = true;
                });
                gThread.Start();

                //thread that shows the spinning "working" graphic. while rendering in progress. Hides it when render completes.
                iThread = new Thread(() =>
                {
                    waitGraphicEnable(true);
                    gThread.Join();
                    waitGraphicEnable(false);
                    
                });
                iThread.Start();
            }
        }


        #endregion

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (!Rendered)
            {
                if (DecalLoaded)
                {
                    //decalPreview.Image = RenderSVGToFit(lastSourceFilename, decalPreview.Width, decalPreview.Height);
                    decalPreview.Image = BitmapHelper.ResizeBitmapToFit(SourceBitmap, decalPreview.Width, decalPreview.Height, (lastWidthHeight.Item1 / lastWidthHeight.Item2));                }
            }
            else
            {
                decalPreview.Image = lastGenerated;
            }
            centerWaitGraphic();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            //save distance field image.
            svgSaveFile.Filter = "PNG Files (*.png)|*.png|All files (*.*)|*.*";
            if (svgSaveFile.ShowDialog() != DialogResult.OK) return;
            lastGenerated.Save(svgSaveFile.FileName, ImageFormat.Png);
        }

  
    }

    
}
