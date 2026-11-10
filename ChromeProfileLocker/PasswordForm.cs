using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChromeProfileLocker
{
    public class PasswordForm : Form
    {
        private TextBox txtPassword;
        private Button btnUnlock;
        private Button btnCancel;

        public string EnteredPassword
        {
            get { return txtPassword.Text; }
        }

        public PasswordForm(string profileName)
        {
            Text = "Unlock Chrome Profile";
            Size = new Size(380, 190);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Label lblTitle = new Label();
            lblTitle.Text = "Enter password for " + profileName;
            lblTitle.Location = new Point(20, 20);
            lblTitle.Size = new Size(320, 25);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(20, 55);
            txtPassword.Size = new Size(320, 25);
            txtPassword.UseSystemPasswordChar = true;

            btnUnlock = new Button();
            btnUnlock.Text = "Unlock";
            btnUnlock.Location = new Point(165, 100);
            btnUnlock.Size = new Size(85, 30);
            btnUnlock.Click += BtnUnlock_Click;

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(255, 100);
            btnCancel.Size = new Size(85, 30);
            btnCancel.DialogResult = DialogResult.Cancel;

            Controls.Add(lblTitle);
            Controls.Add(txtPassword);
            Controls.Add(btnUnlock);
            Controls.Add(btnCancel);

            AcceptButton = btnUnlock;
            CancelButton = btnCancel;
        }

        private void BtnUnlock_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Please enter your password.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}