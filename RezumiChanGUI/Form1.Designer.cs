using System.Windows.Forms;

namespace RezumiChanGUI
{
    partial class Form1
    {
        private TextBox jobPostingTextBox;
        private Button generateButton;
        private Label statusLabel;
        private ProgressBar progressBar;


        private void InitializeComponent()
        {
            this.jobPostingTextBox = new TextBox();
            this.generateButton = new Button();
            this.statusLabel = new Label();

            // 
            // jobPostingTextBox
            // 
            this.jobPostingTextBox.Multiline = true;
            this.jobPostingTextBox.ScrollBars = ScrollBars.Vertical;
            this.jobPostingTextBox.Dock = DockStyle.Fill;
            this.jobPostingTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);

            // 
            // generateButton
            // 
            this.generateButton.Text = "Generate Resume";
            this.generateButton.Dock = DockStyle.Bottom;
            this.generateButton.Height = 40;
            this.generateButton.Click += generateButton_Click;
            
            //
            // progress bar
            //
            
            this.progressBar = new ProgressBar();
            this.progressBar.Dock = DockStyle.Bottom;
            this.progressBar.Height = 16;
            this.progressBar.Minimum = 0;
            this.progressBar.Maximum = 100;
            this.progressBar.Value = 0;

            this.Controls.Add(this.progressBar);


            // 
            // statusLabel
            // 
            this.statusLabel.Text = "Ready";
            this.statusLabel.Dock = DockStyle.Bottom;
            this.statusLabel.Height = 20;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // 
            // Form1
            // 
            this.Text = "RezumiChan";
            this.Width = 900;
            this.Height = 700;

            this.Controls.Add(this.jobPostingTextBox);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.generateButton);
        }
    }
}