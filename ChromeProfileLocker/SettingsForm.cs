using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ChromeProfileLocker
{
    public partial class SettingsForm : Form
    {
        private ComboBox cmbProfiles;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private CheckBox chkNoPassword;

        public SettingsForm(
            List<ChromeProfile> profiles,   
            ChromeProfile selectedProfile)
        {
            this.Text = "Chrome Profile Settings";


            this.Text = "Chrome Profile Settings";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblProfile = new Label();
            lblProfile.Text = "Select Chrome Profile:";
            lblProfile.Location = new Point(25, 20);
            lblProfile.AutoSize = true;
            this.Controls.Add(lblProfile);

            cmbProfiles = new ComboBox();
            cmbProfiles.Location = new Point(25, 45);
            cmbProfiles.Size = new Size(380, 30);
            cmbProfiles.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProfiles.DataSource = profiles;
            this.Controls.Add(cmbProfiles);

            if (selectedProfile != null)
            {
                cmbProfiles.SelectedItem = selectedProfile;
            }

            Label lblPassword = new Label();
            lblPassword.Text = "New Password:";
            lblPassword.Location = new Point(25, 90);
            lblPassword.AutoSize = true;
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(25, 115);
            txtPassword.Size = new Size(380, 25);
            txtPassword.UseSystemPasswordChar = true;
            this.Controls.Add(txtPassword);

            Label lblConfirm = new Label();
            lblConfirm.Text = "Confirm Password:";
            lblConfirm.Location = new Point(25, 150);
            lblConfirm.AutoSize = true;
            this.Controls.Add(lblConfirm);

            txtConfirmPassword = new TextBox();
            txtConfirmPassword.Location = new Point(25, 175);
            txtConfirmPassword.Size = new Size(380, 25);
            txtConfirmPassword.UseSystemPasswordChar = true;
            this.Controls.Add(txtConfirmPassword);

            chkNoPassword = new CheckBox();
            chkNoPassword.Text = "This profile does not require a password";
            chkNoPassword.Location = new Point(25, 215);
            chkNoPassword.AutoSize = true;
            chkNoPassword.CheckedChanged += ChkNoPassword_CheckedChanged;
            this.Controls.Add(chkNoPassword);

            Button btnSave = new Button();
            btnSave.Text = "Save";
            btnSave.Location = new Point(25, 255);
            btnSave.Size = new Size(110, 35);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            Button btnClear = new Button();
            btnClear.Text = "Clear Password";
            btnClear.Location = new Point(155, 255);
            btnClear.Size = new Size(125, 35);
            btnClear.Click += BtnClear_Click;
            this.Controls.Add(btnClear);

            Button btnChangePIN = new Button();
            btnChangePIN.Text = "Change Settings PIN";
            btnChangePIN.Location = new Point(25, 305);
            btnChangePIN.Size = new Size(380, 35);
            btnChangePIN.Click += BtnChangePIN_Click;
            this.Controls.Add(btnChangePIN);

            Button btnClose = new Button();
            btnClose.Text = "Close";
            btnClose.Location = new Point(295, 255);
            btnClose.Size = new Size(110, 35);
            btnClose.Click += delegate
            {
                this.Close();
            };
            this.Controls.Add(btnClose);
        }

        private void ChkNoPassword_CheckedChanged(
            object sender, EventArgs e)
        {
            txtPassword.Enabled = !chkNoPassword.Checked;
            txtConfirmPassword.Enabled = !chkNoPassword.Checked;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            ChromeProfile profile =
                cmbProfiles.SelectedItem as ChromeProfile;

            if (profile == null)
            {
                MessageBox.Show("Please select a profile.");
                return;
            }

            if (chkNoPassword.Checked)
            {
                PasswordStore.SetNoPassword(profile.Directory);

                MessageBox.Show("Settings saved.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter a password.");
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            PasswordStore.SetPassword(
                profile.Directory,
                txtPassword.Text
            );

            txtPassword.Clear();
            txtConfirmPassword.Clear();

            MessageBox.Show("Password saved successfully.");
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ChromeProfile profile =
                cmbProfiles.SelectedItem as ChromeProfile;

            if (profile == null)
            {
                MessageBox.Show("Please select a profile.");
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Remove the saved password for this profile?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (answer == DialogResult.Yes)
            {
                PasswordStore.Remove(profile.Directory);

                txtPassword.Clear();
                txtConfirmPassword.Clear();
                chkNoPassword.Checked = false;

                MessageBox.Show(
                    "Saved password removed. You must configure this profile again."
                );
            }
        }

        private void BtnChangePIN_Click(object sender, EventArgs e)
        {
            PasswordDialog currentPin = new PasswordDialog(
                "Verify Current PIN",
                "Enter your current Settings PIN:",
                false
            );

            if (currentPin.ShowDialog(this) != DialogResult.OK)
                return;

            if (!PasswordStore.VerifySettingsPin(currentPin.EnteredPassword))
            {
                MessageBox.Show("Incorrect current PIN.");
                return;
            }

            PasswordDialog newPin = new PasswordDialog(
                "New Settings PIN",
                "Enter your new Settings PIN:",
                false
            );

            if (newPin.ShowDialog(this) != DialogResult.OK)
                return;

            PasswordDialog confirmPin = new PasswordDialog(
                "Confirm New PIN",
                "Enter your new PIN again:",
                false
            );

            if (confirmPin.ShowDialog(this) != DialogResult.OK)
                return;

            if (newPin.EnteredPassword != confirmPin.EnteredPassword)
            {
                MessageBox.Show("PINs do not match.");
                return;
            }

            PasswordStore.SetSettingsPin(newPin.EnteredPassword);

            MessageBox.Show("Settings PIN changed successfully!");
        }
    }
}