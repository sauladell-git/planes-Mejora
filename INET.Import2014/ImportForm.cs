using INET.Data;
using INET.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace INET.Import2014
{
    public partial class ImportForm : Form
    {
        public const int LOG_ERROR = 1;
        public const int LOG_INFO = 2;
        public const int LOG_ALL = 0;
        public int log_level = LOG_ERROR;

        public ImportForm()
        {
            InitializeComponent();
            ApplyDefaultValues();
        }

        private void ApplyDefaultValues()
        {
            this.logLevelBox.SelectedIndex = 0;
            this.lineStartTxt.Value = Properties.Settings.Default.fromLine;
            this.linesToProcTxt.Value = Properties.Settings.Default.lineCount;
            if (string.IsNullOrEmpty(Properties.Settings.Default.xlsPath))
            {
                this.xlsBrowserTxt.Text = Properties.Resources.ResourceManager.GetString("xlsBrowserTxt");
            }
            else
            {
                this.xlsBrowserTxt.Text = Properties.Settings.Default.xlsPath;
                this.xlsBrowser.FileName = this.xlsBrowserTxt.Text;
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.anexPath))
            {
                this.anexBrowserTxt.Text = Properties.Resources.ResourceManager.GetString("anexBrowserTxt");
            }
            else
            {
                this.anexBrowserTxt.Text = Properties.Settings.Default.anexPath;
                this.anexBrowser.SelectedPath = this.anexBrowserTxt.Text;
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.dictumsPath))
            {
                this.dictumsBrowserTxt.Text = Properties.Resources.ResourceManager.GetString("dictumsBrowserTxt");
            }
            else
            {
                this.dictumsBrowserTxt.Text = Properties.Settings.Default.dictumsPath;
                this.dictumsBrowser.SelectedPath = this.dictumsBrowserTxt.Text;
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.resolutionsPath))
            {
                this.resolutionsBrowserTxt.Text = Properties.Resources.ResourceManager.GetString("resolutionsBrowserTxt");
            }
            else
            {
                this.resolutionsBrowserTxt.Text = Properties.Settings.Default.resolutionsPath;
                this.resolutionsBrowser.SelectedPath = this.resolutionsBrowserTxt.Text;
            }
        }

        private void xlsBrowserBtn_Click(object sender, EventArgs e)
        {
            this.xlsBrowser.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            this.xlsBrowser.ShowDialog();
        }

        private void xlsBrowser_FileOk(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.xlsPath = this.xlsBrowser.FileName.ToString();
            this.xlsBrowserTxt.Text = Properties.Settings.Default.xlsPath;
        }

        private void anexBrowserBtn_Click(object sender, EventArgs e)
        {
            if (this.anexBrowser.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.anexPath = this.anexBrowser.SelectedPath.ToString();
                this.anexBrowserTxt.Text = Properties.Settings.Default.anexPath;
            }
        }

        private void dictumsBrowserBtn_Click(object sender, EventArgs e)
        {
            if (this.dictumsBrowser.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.dictumsPath = this.dictumsBrowser.SelectedPath.ToString();
                this.dictumsBrowserTxt.Text = Properties.Settings.Default.dictumsPath;
            }
        }

        private void resolutionsBrowserBtn_Click(object sender, EventArgs e)
        {
            if (this.resolutionsBrowser.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.resolutionsPath = this.resolutionsBrowser.SelectedPath.ToString();
                this.resolutionsBrowserTxt.Text = Properties.Settings.Default.resolutionsPath;
            }
        }

        public void log(string txt, int level)
        {
            if (level == LOG_ERROR) txt = "ERROR: " + txt;
            if (level == LOG_ALL) txt = " > " + txt;

            if (level >= log_level)
            {
                this.logTxt.AppendText(txt + "\r\n");
                using (StreamWriter outputFile = new StreamWriter(@"log.txt", true))
                {
                    outputFile.WriteLine(txt);
                }
            }
        }

        protected void truncateLog()
        {
            File.WriteAllText(@"log.txt", String.Empty);
        }

        private void goBtn_Click(object sender, EventArgs e)
        {
            this.logTxt.Text = "";
            truncateLog();
            var _import = new Import();
            _import.log = new LogDelegate(this.log);
            if (string.IsNullOrEmpty(this.xlsBrowser.FileName) || string.IsNullOrEmpty(this.dictumsBrowser.SelectedPath) || string.IsNullOrEmpty(this.resolutionsBrowser.SelectedPath) || string.IsNullOrEmpty(this.anexBrowser.SelectedPath))
            {
                this.log("ERROR: Debes seleccionar todos los recursos y archivos para continuar.", LOG_INFO);
                this.log("ERROR: Debes indicar el nivel de logueo para comenzar.", LOG_INFO);
            }
            else
            {
                Properties.Settings.Default.fromLine = (int)this.lineStartTxt.Value;
                Properties.Settings.Default.lineCount = (int)this.linesToProcTxt.Value;

                this.log_level = this.logLevelBox.SelectedItem.ToString() == "INFO" ? LOG_INFO : this.logLevelBox.SelectedItem.ToString() == "ERROR" ? LOG_ERROR : LOG_ALL;
                this.goBtn.Enabled = false;
                //try
                //{
                _import.Process(this.xlsBrowser.FileName, this.dictumsBrowser.SelectedPath, this.resolutionsBrowser.SelectedPath, this.anexBrowser.SelectedPath, (int)this.lineStartTxt.Value, (int)this.linesToProcTxt.Value);
                //}
                //catch (Exception ex)
                //{
                //    this.log(ex.ToString(), LOG_ALL);
                //}
            }
            this.goBtn.Enabled = true;
            this.log("FIN", LOG_INFO);
        }

    }
}
