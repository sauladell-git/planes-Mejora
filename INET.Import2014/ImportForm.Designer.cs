namespace INET.Import2014
{
    partial class ImportForm
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
            Properties.Settings.Default.Save();
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
            this.xlsBrowserTxt = new System.Windows.Forms.TextBox();
            this.anexBrowserTxt = new System.Windows.Forms.TextBox();
            this.dictumsBrowserTxt = new System.Windows.Forms.TextBox();
            this.resolutionsBrowserTxt = new System.Windows.Forms.TextBox();
            this.xlsBrowserBtn = new System.Windows.Forms.Button();
            this.anexBrowserBtn = new System.Windows.Forms.Button();
            this.dictumsBrowserBtn = new System.Windows.Forms.Button();
            this.resolutionsBrowserBtn = new System.Windows.Forms.Button();
            this.anexBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.dictumsBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.resolutionsBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.xlsBrowser = new System.Windows.Forms.OpenFileDialog();
            this.logTxt = new System.Windows.Forms.TextBox();
            this.goBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lineStartTxt = new System.Windows.Forms.NumericUpDown();
            this.logLevelBox = new System.Windows.Forms.ListBox();
            this.linesToProcTxt = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.lineStartTxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.linesToProcTxt)).BeginInit();
            this.SuspendLayout();
            // 
            // xlsBrowserTxt
            // 
            this.xlsBrowserTxt.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.xlsBrowserTxt.Location = new System.Drawing.Point(16, 15);
            this.xlsBrowserTxt.Margin = new System.Windows.Forms.Padding(4);
            this.xlsBrowserTxt.Name = "xlsBrowserTxt";
            this.xlsBrowserTxt.ReadOnly = true;
            this.xlsBrowserTxt.Size = new System.Drawing.Size(789, 22);
            this.xlsBrowserTxt.TabIndex = 0;
            // 
            // anexBrowserTxt
            // 
            this.anexBrowserTxt.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.anexBrowserTxt.Location = new System.Drawing.Point(16, 50);
            this.anexBrowserTxt.Margin = new System.Windows.Forms.Padding(4);
            this.anexBrowserTxt.Name = "anexBrowserTxt";
            this.anexBrowserTxt.ReadOnly = true;
            this.anexBrowserTxt.Size = new System.Drawing.Size(789, 22);
            this.anexBrowserTxt.TabIndex = 1;
            // 
            // dictumsBrowserTxt
            // 
            this.dictumsBrowserTxt.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.dictumsBrowserTxt.Location = new System.Drawing.Point(16, 86);
            this.dictumsBrowserTxt.Margin = new System.Windows.Forms.Padding(4);
            this.dictumsBrowserTxt.Name = "dictumsBrowserTxt";
            this.dictumsBrowserTxt.ReadOnly = true;
            this.dictumsBrowserTxt.Size = new System.Drawing.Size(789, 22);
            this.dictumsBrowserTxt.TabIndex = 2;
            // 
            // resolutionsBrowserTxt
            // 
            this.resolutionsBrowserTxt.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.resolutionsBrowserTxt.Location = new System.Drawing.Point(16, 122);
            this.resolutionsBrowserTxt.Margin = new System.Windows.Forms.Padding(4);
            this.resolutionsBrowserTxt.Name = "resolutionsBrowserTxt";
            this.resolutionsBrowserTxt.ReadOnly = true;
            this.resolutionsBrowserTxt.Size = new System.Drawing.Size(789, 22);
            this.resolutionsBrowserTxt.TabIndex = 3;
            // 
            // xlsBrowserBtn
            // 
            this.xlsBrowserBtn.Location = new System.Drawing.Point(815, 12);
            this.xlsBrowserBtn.Margin = new System.Windows.Forms.Padding(4);
            this.xlsBrowserBtn.Name = "xlsBrowserBtn";
            this.xlsBrowserBtn.Size = new System.Drawing.Size(185, 28);
            this.xlsBrowserBtn.TabIndex = 4;
            this.xlsBrowserBtn.Text = "Seleccionar";
            this.xlsBrowserBtn.UseVisualStyleBackColor = true;
            this.xlsBrowserBtn.Click += new System.EventHandler(this.xlsBrowserBtn_Click);
            // 
            // anexBrowserBtn
            // 
            this.anexBrowserBtn.Location = new System.Drawing.Point(815, 48);
            this.anexBrowserBtn.Margin = new System.Windows.Forms.Padding(4);
            this.anexBrowserBtn.Name = "anexBrowserBtn";
            this.anexBrowserBtn.Size = new System.Drawing.Size(185, 28);
            this.anexBrowserBtn.TabIndex = 5;
            this.anexBrowserBtn.Text = "Seleccionar";
            this.anexBrowserBtn.UseVisualStyleBackColor = true;
            this.anexBrowserBtn.Click += new System.EventHandler(this.anexBrowserBtn_Click);
            // 
            // dictumsBrowserBtn
            // 
            this.dictumsBrowserBtn.Location = new System.Drawing.Point(815, 84);
            this.dictumsBrowserBtn.Margin = new System.Windows.Forms.Padding(4);
            this.dictumsBrowserBtn.Name = "dictumsBrowserBtn";
            this.dictumsBrowserBtn.Size = new System.Drawing.Size(185, 28);
            this.dictumsBrowserBtn.TabIndex = 6;
            this.dictumsBrowserBtn.Text = "Seleccionar";
            this.dictumsBrowserBtn.UseVisualStyleBackColor = true;
            this.dictumsBrowserBtn.Click += new System.EventHandler(this.dictumsBrowserBtn_Click);
            // 
            // resolutionsBrowserBtn
            // 
            this.resolutionsBrowserBtn.Location = new System.Drawing.Point(815, 119);
            this.resolutionsBrowserBtn.Margin = new System.Windows.Forms.Padding(4);
            this.resolutionsBrowserBtn.Name = "resolutionsBrowserBtn";
            this.resolutionsBrowserBtn.Size = new System.Drawing.Size(185, 28);
            this.resolutionsBrowserBtn.TabIndex = 7;
            this.resolutionsBrowserBtn.Text = "Seleccionar";
            this.resolutionsBrowserBtn.UseVisualStyleBackColor = true;
            this.resolutionsBrowserBtn.Click += new System.EventHandler(this.resolutionsBrowserBtn_Click);
            // 
            // anexBrowser
            // 
            this.anexBrowser.Description = this.anexBrowserBtn.Text;
            // 
            // dictumsBrowser
            // 
            this.dictumsBrowser.Description = this.dictumsBrowserBtn.Text;
            // 
            // resolutionsBrowser
            // 
            this.resolutionsBrowser.Description = this.resolutionsBrowserBtn.Text;
            // 
            // xlsBrowser
            // 
            this.xlsBrowser.InitialDirectory = "%DESKTOP%";
            this.xlsBrowser.Title = this.xlsBrowserBtn.Text;
            this.xlsBrowser.FileOk += new System.ComponentModel.CancelEventHandler(this.xlsBrowser_FileOk);
            // 
            // logTxt
            // 
            this.logTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logTxt.Location = new System.Drawing.Point(16, 217);
            this.logTxt.Multiline = true;
            this.logTxt.Name = "logTxt";
            this.logTxt.ReadOnly = true;
            this.logTxt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTxt.Size = new System.Drawing.Size(984, 603);
            this.logTxt.TabIndex = 8;
            // 
            // goBtn
            // 
            this.goBtn.Location = new System.Drawing.Point(509, 156);
            this.goBtn.Margin = new System.Windows.Forms.Padding(4);
            this.goBtn.Name = "goBtn";
            this.goBtn.Size = new System.Drawing.Size(491, 53);
            this.goBtn.TabIndex = 9;
            this.goBtn.Text = "Importar >>";
            this.goBtn.UseVisualStyleBackColor = true;
            this.goBtn.Click += new System.EventHandler(this.goBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 159);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 17);
            this.label1.TabIndex = 15;
            this.label1.Text = "Log level:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(231, 159);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 17);
            this.label2.TabIndex = 16;
            this.label2.Text = "Comenzar en fila:";
            // 
            // lineStartTxt
            // 
            this.lineStartTxt.Location = new System.Drawing.Point(357, 159);
            this.lineStartTxt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.lineStartTxt.Name = "lineStartTxt";
            this.lineStartTxt.Size = new System.Drawing.Size(120, 22);
            this.lineStartTxt.TabIndex = 17;
            // 
            // logLevelBox
            // 
            this.logLevelBox.FormattingEnabled = true;
            this.logLevelBox.ItemHeight = 16;
            this.logLevelBox.Items.AddRange(new object[] {
            "INFO",
            "ERROR",
            "ALL"});
            this.logLevelBox.Location = new System.Drawing.Point(89, 159);
            this.logLevelBox.Name = "logLevelBox";
            this.logLevelBox.Size = new System.Drawing.Size(135, 52);
            this.logLevelBox.TabIndex = 18;
            // 
            // linesToProcTxt
            // 
            this.linesToProcTxt.Location = new System.Drawing.Point(357, 187);
            this.linesToProcTxt.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.linesToProcTxt.Name = "linesToProcTxt";
            this.linesToProcTxt.Size = new System.Drawing.Size(120, 22);
            this.linesToProcTxt.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(231, 187);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 17);
            this.label3.TabIndex = 19;
            this.label3.Text = "# filas a procesar";
            // 
            // ImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1017, 832);
            this.Controls.Add(this.linesToProcTxt);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.logLevelBox);
            this.Controls.Add(this.lineStartTxt);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.goBtn);
            this.Controls.Add(this.logTxt);
            this.Controls.Add(this.resolutionsBrowserBtn);
            this.Controls.Add(this.dictumsBrowserBtn);
            this.Controls.Add(this.anexBrowserBtn);
            this.Controls.Add(this.xlsBrowserBtn);
            this.Controls.Add(this.resolutionsBrowserTxt);
            this.Controls.Add(this.dictumsBrowserTxt);
            this.Controls.Add(this.anexBrowserTxt);
            this.Controls.Add(this.xlsBrowserTxt);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ImportForm";
            this.Text = "Importación Planes 2014";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.lineStartTxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.linesToProcTxt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox xlsBrowserTxt;
        private System.Windows.Forms.TextBox anexBrowserTxt;
        private System.Windows.Forms.TextBox dictumsBrowserTxt;
        private System.Windows.Forms.TextBox resolutionsBrowserTxt;
        private System.Windows.Forms.Button xlsBrowserBtn;
        private System.Windows.Forms.Button anexBrowserBtn;
        private System.Windows.Forms.Button dictumsBrowserBtn;
        private System.Windows.Forms.Button resolutionsBrowserBtn;
        private System.Windows.Forms.FolderBrowserDialog anexBrowser;
        private System.Windows.Forms.FolderBrowserDialog dictumsBrowser;
        private System.Windows.Forms.FolderBrowserDialog resolutionsBrowser;
        private System.Windows.Forms.OpenFileDialog xlsBrowser;
        private System.Windows.Forms.TextBox logTxt;
        private System.Windows.Forms.Button goBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown lineStartTxt;
        private System.Windows.Forms.ListBox logLevelBox;
        private System.Windows.Forms.NumericUpDown linesToProcTxt;
        private System.Windows.Forms.Label label3;
    }
}

