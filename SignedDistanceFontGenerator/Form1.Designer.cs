namespace SignedDistanceFontGenerator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fontList = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.fontPreview = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.progressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.fontFilterMethod = new System.Windows.Forms.ComboBox();
            this.fontSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.SpreadSelector = new System.Windows.Forms.NumericUpDown();
            this.SpreadLabel = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.SizeSelectBox_Y = new System.Windows.Forms.ComboBox();
            this.SizeSelectBox_X = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label_Scale = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.GenerateButton = new System.Windows.Forms.Button();
            this.decalPreview = new System.Windows.Forms.PictureBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.svgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.svgSaveFile = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.fontPreview)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SpreadSelector)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.decalPreview)).BeginInit();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fontList
            // 
            this.fontList.FormattingEnabled = true;
            this.fontList.Location = new System.Drawing.Point(6, 6);
            this.fontList.Name = "fontList";
            this.fontList.Size = new System.Drawing.Size(233, 21);
            this.fontList.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(443, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Generate";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // fontPreview
            // 
            this.fontPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fontPreview.Location = new System.Drawing.Point(6, 34);
            this.fontPreview.Name = "fontPreview";
            this.fontPreview.Size = new System.Drawing.Size(512, 256);
            this.fontPreview.TabIndex = 4;
            this.fontPreview.TabStop = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 544);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(592, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // progressBar1
            // 
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // fontFilterMethod
            // 
            this.fontFilterMethod.FormattingEnabled = true;
            this.fontFilterMethod.Items.AddRange(new object[] {
            "Nearest Neighbor",
            "Bilinear",
            "Bicubic"});
            this.fontFilterMethod.Location = new System.Drawing.Point(353, 5);
            this.fontFilterMethod.Name = "fontFilterMethod";
            this.fontFilterMethod.Size = new System.Drawing.Size(84, 21);
            this.fontFilterMethod.TabIndex = 7;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(592, 566);
            this.tabControl1.TabIndex = 8;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel1);
            this.tabPage2.Location = new System.Drawing.Point(4, 23);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(584, 539);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Decal";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.decalPreview, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(578, 533);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.SaveButton);
            this.panel1.Controls.Add(this.GenerateButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(572, 52);
            this.panel1.TabIndex = 10;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.SpreadSelector);
            this.panel3.Controls.Add(this.SpreadLabel);
            this.panel3.Location = new System.Drawing.Point(93, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(151, 48);
            this.panel3.TabIndex = 10;
            // 
            // SpreadSelector
            // 
            this.SpreadSelector.Location = new System.Drawing.Point(91, 24);
            this.SpreadSelector.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.SpreadSelector.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.SpreadSelector.Name = "SpreadSelector";
            this.SpreadSelector.Size = new System.Drawing.Size(43, 20);
            this.SpreadSelector.TabIndex = 7;
            this.SpreadSelector.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // SpreadLabel
            // 
            this.SpreadLabel.AutoSize = true;
            this.SpreadLabel.Location = new System.Drawing.Point(11, 28);
            this.SpreadLabel.Name = "SpreadLabel";
            this.SpreadLabel.Size = new System.Drawing.Size(74, 13);
            this.SpreadLabel.TabIndex = 8;
            this.SpreadLabel.Text = "Spread Factor";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.SizeSelectBox_Y);
            this.panel2.Controls.Add(this.SizeSelectBox_X);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.label_Scale);
            this.panel2.Location = new System.Drawing.Point(244, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(231, 58);
            this.panel2.TabIndex = 9;
            // 
            // panel4
            // 
            this.panel4.Location = new System.Drawing.Point(237, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(84, 48);
            this.panel4.TabIndex = 7;
            // 
            // SizeSelectBox_Y
            // 
            this.SizeSelectBox_Y.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SizeSelectBox_Y.DropDownWidth = 64;
            this.SizeSelectBox_Y.FormattingEnabled = true;
            this.SizeSelectBox_Y.Items.AddRange(new object[] {
            "4096",
            "2048",
            "1024",
            "512",
            "256",
            "128",
            "64"});
            this.SizeSelectBox_Y.Location = new System.Drawing.Point(149, 26);
            this.SizeSelectBox_Y.Name = "SizeSelectBox_Y";
            this.SizeSelectBox_Y.Size = new System.Drawing.Size(67, 21);
            this.SizeSelectBox_Y.TabIndex = 6;
            // 
            // SizeSelectBox_X
            // 
            this.SizeSelectBox_X.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SizeSelectBox_X.DropDownWidth = 64;
            this.SizeSelectBox_X.FormattingEnabled = true;
            this.SizeSelectBox_X.Items.AddRange(new object[] {
            "4096",
            "2048",
            "1024",
            "512",
            "256",
            "128",
            "64"});
            this.SizeSelectBox_X.Location = new System.Drawing.Point(76, 26);
            this.SizeSelectBox_X.Name = "SizeSelectBox_X";
            this.SizeSelectBox_X.Size = new System.Drawing.Size(67, 21);
            this.SizeSelectBox_X.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AccessibleRole = System.Windows.Forms.AccessibleRole.ScrollBar;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(108, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Width";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AccessibleRole = System.Windows.Forms.AccessibleRole.ScrollBar;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(178, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Height";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_Scale
            // 
            this.label_Scale.AutoSize = true;
            this.label_Scale.Location = new System.Drawing.Point(13, 30);
            this.label_Scale.Name = "label_Scale";
            this.label_Scale.Size = new System.Drawing.Size(62, 13);
            this.label_Scale.TabIndex = 5;
            this.label_Scale.Text = "Output Size";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(0, 24);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 25);
            this.button3.TabIndex = 2;
            this.button3.Text = "Open";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.Location = new System.Drawing.Point(493, 27);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 0;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // GenerateButton
            // 
            this.GenerateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GenerateButton.Location = new System.Drawing.Point(493, 3);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = new System.Drawing.Size(75, 23);
            this.GenerateButton.TabIndex = 0;
            this.GenerateButton.Text = "Generate";
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // decalPreview
            // 
            this.decalPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.decalPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.decalPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.decalPreview.Location = new System.Drawing.Point(3, 61);
            this.decalPreview.Margin = new System.Windows.Forms.Padding(3, 3, 3, 25);
            this.decalPreview.Name = "decalPreview";
            this.decalPreview.Size = new System.Drawing.Size(572, 447);
            this.decalPreview.TabIndex = 3;
            this.decalPreview.TabStop = false;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.fontPreview);
            this.tabPage1.Controls.Add(this.fontFilterMethod);
            this.tabPage1.Controls.Add(this.fontList);
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Location = new System.Drawing.Point(4, 23);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(584, 539);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Font";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 566);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "Form1";
            this.Text = "Signed distance field generator tool";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.fontPreview)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SpreadSelector)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.decalPreview)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox fontList;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox fontPreview;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar progressBar1;
        private System.Windows.Forms.ComboBox fontFilterMethod;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.SaveFileDialog fontSaveFile;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.PictureBox decalPreview;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button GenerateButton;
        private System.Windows.Forms.OpenFileDialog svgOpenFile;
        private System.Windows.Forms.SaveFileDialog svgSaveFile;
        private System.Windows.Forms.ComboBox SizeSelectBox_X;
        private System.Windows.Forms.Label label_Scale;
        private System.Windows.Forms.Label SpreadLabel;
        private System.Windows.Forms.NumericUpDown SpreadSelector;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox SizeSelectBox_Y;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
    }
}

