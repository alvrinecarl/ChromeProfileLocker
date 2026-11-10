using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace ChromeProfileLocker
{
    public partial class Form1 : Form
    {
        private ListBox lstProfiles;
        private Label lblStatus;
        private List<ChromeProfile> chromeProfiles =
            new List<ChromeProfile>();

        private string chromeUserData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Google", "Chrome", "User Data"
        );

        public Form1()
        {
            InitializeComponent();

            CreateInterface();
            LoadChromeProfiles();
        }

        private void CreateInterface()
        {
            this.Text = "Chrome Profile Locker";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            Label title = new Label();
            title.Text = "CHROME PROFILE LOCKER";
            title.Font = new Font("Arial", 16, FontStyle.Bold);
            title.Location = new Point(30, 20);
            title.Size = new Size(420, 35);
            this.Controls.Add(title);

            lstProfiles = new ListBox();
            lstProfiles.Location = new Point(30, 75);
            lstProfiles.Size = new Size(420, 180);
            lstProfiles.Font = new Font("Arial", 12);
            this.Controls.Add(lstProfiles);

            Button btnUnlock = new Button();
            btnUnlock.Text = "Unlock Profile";
            btnUnlock.Location = new Point(30, 275);
            btnUnlock.Size = new Size(190, 40);
            btnUnlock.Click += BtnUnlock_Click;
            this.Controls.Add(btnUnlock);

            Button btnSettings = new Button();
            btnSettings.Text = "Settings";
            btnSettings.Location = new Point(260, 275);
            btnSettings.Size = new Size(190, 40);
            btnSettings.Click += BtnSettings_Click;
            this.Controls.Add(btnSettings);

            lblStatus = new Label();
            lblStatus.Text = "Loading profiles...";
            lblStatus.Location = new Point(30, 330);
            lblStatus.Size = new Size(420, 25);
            this.Controls.Add(lblStatus);
        }

        private void LoadChromeProfiles()
        {
            lstProfiles.Items.Clear();
            chromeProfiles.Clear();

            string localState = Path.Combine(chromeUserData, "Local State");

            if (!File.Exists(localState))
            {
                lblStatus.Text = "Chrome profile information not found.";
                return;
            }

            try
            {
                JavaScriptSerializer serializer =
                    new JavaScriptSerializer();

                string json = File.ReadAllText(localState);

                Dictionary<string, object> root =
                    serializer.DeserializeObject(json)
                    as Dictionary<string, object>;

                Dictionary<string, object> profile =
                    root["profile"] as Dictionary<string, object>;

                Dictionary<string, object> infoCache =
                    profile["info_cache"] as Dictionary<string, object>;

                foreach (KeyValuePair<string, object> item in infoCache)
                {
                    Dictionary<string, object> details =
                        item.Value as Dictionary<string, object>;

                    string profileName = item.Key;

                    if (details != null && details.ContainsKey("name"))
                    {
                        profileName = details["name"].ToString();
                    }

                    ChromeProfile chromeProfile = new ChromeProfile();
                    chromeProfile.Name = profileName;
                    chromeProfile.Directory = item.Key;

                    chromeProfiles.Add(chromeProfile);
                    lstProfiles.Items.Add(chromeProfile);
                }

                if (lstProfiles.Items.Count > 0)
                {
                    lstProfiles.SelectedIndex = 0;
                    lblStatus.Text =
                        chromeProfiles.Count + " Chrome profiles found.";
                }
                else
                {
                    lblStatus.Text = "No Chrome profiles found.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Unable to load Chrome profiles.";

                MessageBox.Show(
                    ex.Message,
                    "Profile Loading Error"
                );
            }
        }

        private void BtnUnlock_Click(object sender, EventArgs e)
        {
            ChromeProfile selectedProfile =
                lstProfiles.SelectedItem as ChromeProfile;

            if (selectedProfile == null)
            {
                MessageBox.Show("Please select a Chrome profile.");
                return;
            }

            ProfilePassword savedPassword =
                PasswordStore.Get(selectedProfile.Directory);

            if (savedPassword == null)
            {
                MessageBox.Show(
                    "This profile has no password configured.\n" +
                    "Open Settings and create one first."
                );
                return;
            }

            if (savedPassword.RequiresPassword)
            {
                PasswordDialog passwordDialog =
                    new PasswordDialog(
                        "Unlock Chrome Profile",
                        "Enter your profile password:",
                        true
                    );

                DialogResult result = passwordDialog.ShowDialog(this);

                if (result == DialogResult.Retry)
                {
                    OpenSettingsWithPIN(selectedProfile);
                    return;
                }

                if (result != DialogResult.OK)
                {
                    return;
                }

                if (!PasswordStore.Verify(
                    selectedProfile.Directory,
                    passwordDialog.EnteredPassword))
                {
                    MessageBox.Show(
                        "Incorrect password.",
                        "Access Denied"
                    );
                    return;
                }
            }

            LaunchChrome(selectedProfile);
        }

        private void LaunchChrome(ChromeProfile profile)
        {
            string chromePath =
                @"C:\Program Files\Google\Chrome\Application\chrome.exe";

            if (!File.Exists(chromePath))
            {
                MessageBox.Show("Chrome.exe could not be found.");
                return;
            }

            try
            {
                ProcessStartInfo startInfo =
                    new ProcessStartInfo();

                startInfo.FileName = chromePath;

                startInfo.Arguments =
                    "--user-data-dir=\"" + chromeUserData +
                    "\" --profile-directory=\"" + profile.Directory + "\"";

                startInfo.UseShellExecute = true;

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Chrome Launch Error"
                );
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            OpenSettingsWithPIN(null);
        }

        private void OpenSettingsWithPIN(ChromeProfile selectedProfile)
        {
            // First-time setup
            if (!PasswordStore.HasSettingsPin())
            {
                PasswordDialog createPin = new PasswordDialog(
                    "Create Settings PIN",
                    "Create your new Settings PIN:",
                    false
                );

                if (createPin.ShowDialog(this) != DialogResult.OK)
                    return;

                PasswordDialog confirmPin = new PasswordDialog(
                    "Confirm Settings PIN",
                    "Enter your new PIN again:",
                    false
                );

                if (confirmPin.ShowDialog(this) != DialogResult.OK)
                    return;

                if (createPin.EnteredPassword != confirmPin.EnteredPassword)
                {
                    MessageBox.Show("PINs do not match.");
                    return;
                }

                PasswordStore.SetSettingsPin(createPin.EnteredPassword);

                MessageBox.Show("Settings PIN created successfully.");
            }
            else
            {
                PasswordDialog pinDialog = new PasswordDialog(
                    "Settings Security",
                    "Enter your Settings PIN:",
                    false
                );

                if (pinDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (!PasswordStore.VerifySettingsPin(pinDialog.EnteredPassword))
                {
                    MessageBox.Show("Incorrect Settings PIN.", "Access Denied");
                    return;
                }
            }

            SettingsForm settings = new SettingsForm(
                chromeProfiles,
                selectedProfile
            );

            settings.ShowDialog(this);
        }
    }

    public class ChromeProfile
    {
        public string Name { get; set; }
        public string Directory { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}