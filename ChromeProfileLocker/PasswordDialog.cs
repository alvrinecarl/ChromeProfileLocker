using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChromeProfileLocker
{
    public class PasswordDialog : Form
    {
        private TextBox txtPassword;
        private Button btnUnlock;
        private Button btnForgot;

        public string EnteredPassword
        {
            get { return txtPassword.Text; }
        }

        public PasswordDialog(
            string title,
            string message,
            bool showForgotButton)
        {
            this.Text = title;
            this.Size = new Size(380, 190);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblMessage = new Label();
            lblMessage.Text = message;
            lblMessage.Location = new Point(20, 20);
            lblMessage.AutoSize = true;
            this.Controls.Add(lblMessage);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(20, 55);
            txtPassword.Size = new Size(320, 25);
            txtPassword.UseSystemPasswordChar = true;
            this.Controls.Add(txtPassword);

            btnUnlock = new Button();
            btnUnlock.Text = "Continue";
            btnUnlock.Location = new Point(20, 100);
            btnUnlock.Size = new Size(100, 30);
            btnUnlock.Click += delegate
            {
                if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Please enter a password or PIN.");
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(btnUnlock);

            if (showForgotButton)
            {
                btnForgot = new Button();
                btnForgot.Text = "Forgot Password?";
                btnForgot.Location = new Point(145, 100);
                btnForgot.Size = new Size(150, 30);
                btnForgot.Click += delegate
                {
                    this.DialogResult = DialogResult.Retry;
                    this.Close();
                };
                this.Controls.Add(btnForgot);
            }

            this.AcceptButton = btnUnlock;
        }
    }
}