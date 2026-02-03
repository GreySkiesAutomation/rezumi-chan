using System;
using System.Windows.Forms;

namespace RezumiChanGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void generateButton_Click(object sender, EventArgs e)
        {
            generateButton.Enabled = false;
            progressBar.Value = 0;
            statusLabel.Text = "Starting…";

            var progress = new Progress<RezumiChanCLI.Program.PipelineProgress>(p =>
            {
                statusLabel.Text = p.Message;
                progressBar.Value = Math.Clamp(p.Percent, 0, 100);
            });

            try
            {
                await RezumiChanCLI.Program.RunResumePipeline(
                    jobPostingTextBox.Text,
                    progress
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                statusLabel.Text = "Error";
            }
            finally
            {
                generateButton.Enabled = true;
            }
        }

    }
}