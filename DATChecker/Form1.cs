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
using static DATValidatorLib.DatValidator;

namespace DatFixer
{
    public partial class Form1 : Form
    {
        Logger Log = new Logger();

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
                
            };
            datValidator.OnValidateStatus += OnValidateStatus;
            datValidator.OnValidateError += OnValidateError;
            datValidator.OnValidateComplete += OnValidateComplete;
            datValidator.Validate();
        }

        private void OnValidateComplete(object sender, ValidationCompleteEventArgs e)
        {
            if (InvokeRequired) // Make sure to check that if being called from a different thread
            {
                this.Invoke(new Action(() =>
                {
                    if (tbStatus.Text.Length + e.Message.Length > tbStatus.MaxLength) // Make sure we dont store too much text to avoid out of memory error
                    {
                        tbStatus.Clear();
                    }
                }));
            }
            else
            {
                if (tbStatus.Text.Length + e.Message.Length > tbStatus.MaxLength) // Make sure we dont store too much text to avoid out of memory error
                {
                    tbStatus.Clear();
                }
            }

            switch (e.Status)
            {
                case ValidationStatus.FAILED:
                    {
                        if (InvokeRequired) // Make sure to check that if being called from a different thread
                        {
                            this.Invoke(new Action(() =>
                            {
                                tbStatus.SelectionColor = Color.Red;
                                tbStatus.AppendText(e.Message + Environment.NewLine);
                                tbStatus.AppendText("Error detected." + Environment.NewLine);
                                tbStatus.SelectionColor = Color.Black;
                            }));
                        }
                        else
                        {
                            tbStatus.SelectionColor = Color.Red;
                            tbStatus.AppendText(e.Message + Environment.NewLine);
                            tbStatus.SelectionColor = Color.Black;
                        }

                        break;
                    }
                case ValidationStatus.CANCELLED:
                    {
                        if (InvokeRequired) // Make sure to check that if being called from a different thread
                        {
                            this.Invoke(new Action(() =>
                            {
                                tbStatus.SelectionColor = Color.Yellow;
                                tbStatus.AppendText(e.Message + Environment.NewLine);
                                tbStatus.AppendText("Validation cancelled." + Environment.NewLine);
                                tbStatus.SelectionColor = Color.Black;
                            }));
                        }
                        else
                        {
                            tbStatus.SelectionColor = Color.Yellow;
                            tbStatus.AppendText(e.Message + Environment.NewLine);
                            tbStatus.SelectionColor = Color.Black;
                        }
                        break;
                    }
                case ValidationStatus.PASSED:
                    {
                        if (InvokeRequired) // Make sure to check that if being called from a different thread
                        {
                            this.Invoke(new Action(() =>
                            {
                                tbStatus.SelectionColor = Color.Green;
                                tbStatus.AppendText(e.Message + Environment.NewLine);
                                tbStatus.AppendText("File is OK." + Environment.NewLine);
                                tbStatus.SelectionColor = Color.Black;
                            }));
                        }
                        else
                        {
                            tbStatus.SelectionColor = Color.Green;
                            tbStatus.AppendText(e.Message + Environment.NewLine);
                            tbStatus.SelectionColor = Color.Black;
                        }
                        break;
                    }

            }

        }
        private void OnValidateError(object sender, ValidationErrorEventArgs e)
        {
            if (InvokeRequired) // Make sure to check that if being called from a different thread
            {
                this.Invoke(new Action(() =>
                {
                    if (tbStatus.Text.Length + e.Message.Length > tbStatus.MaxLength) // Make sure we dont store too much text to avoid out of memory error
                    {
                        tbStatus.Clear();
                    }                   
                    tbStatus.SelectionColor = Color.Red;
                    tbStatus.AppendText(e.Message + Environment.NewLine);
                    if (e.Ex != null)
                    {
                        tbStatus.AppendText(e.Ex.StackTrace + Environment.NewLine);
                    }
                    tbStatus.SelectionColor = Color.Black;
                }));
            }
            else
            {
                if (tbStatus.Text.Length + e.Message.Length > tbStatus.MaxLength) // Make sure we dont store too much text to avoid out of memory error
                {
                    tbStatus.Clear();
                }
                tbStatus.SelectionColor = Color.Red;
                tbStatus.AppendText(e.Message + Environment.NewLine);
                if (e.Ex != null)
                {
                    tbStatus.AppendText(e.Ex.StackTrace + Environment.NewLine);
                }
                tbStatus.SelectionColor = Color.Black;
            }

        }

        private void OnValidateStatus(object sender, ValidationStatusEventArgs e)
        {
            if (InvokeRequired) // Make sure to check that if being called from a different thread
            {
                this.Invoke(new Action(() =>
                {
                    if (tbStatus.Text.Length + e.Message.Length > tbStatus.MaxLength) // Make sure we dont store too much text to avoid out of memory error
                    {
                        tbStatus.Clear();
                    }
                }));
            }
            else
            {
                if (tbStatus.Text.Length + e.Message.Length > tbStatus.MaxLength) // Make sure we dont store too much text to avoid out of memory error
                {
                    tbStatus.Clear();
                }
            }

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btCheck.Enabled = true;
            btBrowse.Enabled = true;
            tbConcordanceFilePath.Enabled = true;
            Cursor.Current = Cursors.Arrow;
        }
        #endregion
    }
}

