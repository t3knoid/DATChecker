using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using LoggerLib;
using DATValidatorLib;

namespace DatFixer
{
    public partial class Form1 : Form, IDatValidator
    {
        Logger Log = new Logger();

        public CompleteDelegate CompleteDelegateCallBack { get => CompleteCallback; }
        public LoggingDelegate LoggingDelegateCallback { get => LoggingCallback; }
        public StatusDelegate StatusDelegateCallBack { get => StatusCallback; }

        public Form1()
        {
            Log.Info("Starting");
            InitializeComponent();
        }
        #region Form controls
        private void Form1_Resize(object sender, EventArgs e)
        {
            btCancel.Left = (this.ClientSize.Width / 2) + 30;
            btCheck.Left = (this.ClientSize.Width / 2) - (30 + btCheck.Width);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            btCancel.Left = (this.ClientSize.Width / 2) + 30;
            btCheck.Left = (this.ClientSize.Width / 2) - (30 + btCheck.Width);
        }

        private void btBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                this.tbConcordanceFilePath.Text = openFileDialog1.FileName;
            }

        }

        private void btCheck_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                tbStatus.Clear();
                btCheck.Enabled = false;
                btBrowse.Enabled = false;
                tbConcordanceFilePath.Enabled = false;
                backgroundWorker1.RunWorkerAsync();
            }
        }
        private void btCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
            else
            {
                this.Close();
            }
        }


        #endregion

        #region DATValidator callbacks
        private void StatusCallback(string message, MessageType level)
        {            
            if (InvokeRequired) // Make sure to check that if being called from a different thread
            {
                this.Invoke(new Action(() =>
                {
                    if (tbStatus.Text.Length + message.Length > tbStatus.MaxLength) // Make sure we dont store too much text to avoid out of memory error
                    {
                        tbStatus.Clear();
                    }
                    if (level == MessageType.ERROR)
                    {
                        tbStatus.SelectionColor = Color.Red;
                    }
                    tbStatus.AppendText(message + Environment.NewLine);
                    tbStatus.SelectionColor = Color.Black;
                }));
            }
            else
            {
                if (tbStatus.Text.Length + message.Length > tbStatus.MaxLength) // Make sure we dont store too much text to avoid out of memory error
                {
                    tbStatus.Clear();
                }
                if (level == MessageType.ERROR)
                {
                    tbStatus.SelectionColor = Color.Red;
                }
                tbStatus.AppendText(message + Environment.NewLine);
                tbStatus.SelectionColor = Color.Black;
            }
            
        }

        private void LoggingCallback(string message, DATValidatorLib.MessageType level)
        {
            switch (level)
            {
                case DATValidatorLib.MessageType.ERROR:
                    LogHelper.Log(LogLevel.ERROR, message);
                    break;
                case DATValidatorLib.MessageType.WARNING:
                    LogHelper.Log(LogLevel.WARNING, message);
                    break;
                case DATValidatorLib.MessageType.INFO:
                    LogHelper.Log(LogLevel.INFO, message);
                    break;
            }

        }
        #endregion

        #region Eventhandler
        private void tbConcordanceFilePath_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(@tbConcordanceFilePath.Text))
            {
                btCheck.Enabled = true; // Checks if the specified file exists and enables convert button if it does
            }
            else
            {
                btCheck.Enabled = false; // otherwise, disable convert button
            }
        }

        private void tbConcordanceFilePath_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0)
            {
                tbConcordanceFilePath.Text = files[0];
            }

        }

        private void tbConcordanceFilePath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void tbConcordanceFilePath_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        #endregion

        #region Background worker
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            Cursor.Current = Cursors.WaitCursor;
            string currentFile = this.tbConcordanceFilePath.Text;
            string outputDir = Path.GetDirectoryName(currentFile);
            string errorFile = Path.Combine(outputDir, "error.dat");

            Log.Info("Validating " + currentFile);

            DatValidator datValidator = new DatValidator
            {
                Worker = worker,
                DatFile = currentFile,
                LoggingDelegateCallback = LoggingDelegateCallback,
                StatusDelegateCallBack = StatusDelegateCallBack
            };
            datValidator.Validate();
        }

        private void CompleteCallback(string message)
        {
            throw new NotImplementedException();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btCheck.Enabled = true;
            btBrowse.Enabled = true;
            tbConcordanceFilePath.Enabled = true;
            Cursor.Current = Cursors.Arrow;
            StatusCallback("Process complete.", MessageType.INFO);
        }
        #endregion
    }
}

