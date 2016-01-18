using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ConvertToTcx;

namespace ConvertToTcxWinform
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAddFile_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    Title = "Find LeMond .csv files",
                    Filter = "Supported Files (*.csv;*.3dp;*.csvx)|*.csv;*.3dp;*.csvx|LeMond Files (*.csv)|*.csv|CompuTrainer (*.3dp)|*.3dp|XTrainer (*.csvx)|*.csvx",
                    FilterIndex = 1
                };

            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                lstFiles.Items.Add(dialog.FileName);
            }

        }

        private void btnCreateTcx_Click(object sender, EventArgs e)
        {
            if (lstFiles.Items.Count == 0)
            {
                MessageBox.Show("You must add a file before you can create a .tcx file");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Save Tcx As",
                Filter = "tcx files (*.tcx)|*.tcx|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                FileName = Path.GetFileNameWithoutExtension((string)lstFiles.Items[0])
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var streams = new List<SourcedStream>();
                try
                {
                    using (TextWriter textWriter = new StreamWriter(dialog.FileName))
                    {
                        foreach (string item in lstFiles.Items)
                        {
                            var path = item;
                            streams.Add(new SourcedStream { Stream = new FileStream(path, FileMode.Open), Source = path });
                        }
                        new Converter().WriteTcxFile(streams, textWriter);
                    }
                    MessageBox.Show(string.Format("File '{0}' was created successfully", dialog.FileName), "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, String.Format("Error creating the TCX file: \r\n{0}\r\n\r\nDetails:\r\n{1}", ex.Message, ex.ToString()), "Error creating TCX file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    foreach (var sourcedStream in streams)
                    {
                        IDisposable disposable = sourcedStream.Stream as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            lstFiles.Items.Clear();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
