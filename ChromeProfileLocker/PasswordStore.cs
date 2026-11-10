using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

namespace ChromeProfileLocker
{
    public class ProfilePassword
    {
        public bool RequiresPassword { get; set; }
        public string Salt { get; set; }
        public string PasswordHash { get; set; }
    }

    public static class PasswordStore
    {
        private static readonly string Folder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ChromeProfileLocker"
            );

        private static readonly string FilePath =
            Path.Combine(Folder, "passwords.json");

        private static Dictionary<string, ProfilePassword> Load()
        {
            if (!File.Exists(FilePath))
            {
                return new Dictionary<string, ProfilePassword>();
            }

            try
            {
                JavaScriptSerializer serializer =
                    new JavaScriptSerializer();

                string json = File.ReadAllText(FilePath);

                Dictionary<string, ProfilePassword> data =
                    serializer.Deserialize<
                        Dictionary<string, ProfilePassword>>(json);

                if (data == null)
                {
                    return new Dictionary<string, ProfilePassword>();
                }

                return data;
            }
            catch
            {
                return new Dictionary<string, ProfilePassword>();
            }
        }

        private static void Save(
            Dictionary<string, ProfilePassword> data)
        {
            Directory.CreateDirectory(Folder);

            JavaScriptSerializer serializer =
                new JavaScriptSerializer();

            string json = serializer.Serialize(data);

            File.WriteAllText(FilePath, json);
        }

        public static ProfilePassword Get(string profileDirectory)
        {
            Dictionary<string, ProfilePassword> data = Load();

            if (data.ContainsKey(profileDirectory))
            {
                return data[profileDirectory];
            }

            return null;
        }

        public static void SetPassword(
            string profileDirectory,
            string password)
        {
            byte[] salt = new byte[16];

            using (RandomNumberGenerator rng =
                RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string saltText = Convert.ToBase64String(salt);

            string hash = CreateHash(password, salt);

            Dictionary<string, ProfilePassword> data = Load();

            data[profileDirectory] = new ProfilePassword
            {
                RequiresPassword = true,
                Salt = saltText,
                PasswordHash = hash
            };

            Save(data);
        }

        public static void SetNoPassword(string profileDirectory)
        {
            Dictionary<string, ProfilePassword> data = Load();

            data[profileDirectory] = new ProfilePassword
            {
                RequiresPassword = false,
                Salt = "",
                PasswordHash = ""
            };

            Save(data);
        }

        public static void Remove(string profileDirectory)
        {
            Dictionary<string, ProfilePassword> data = Load();

            if (data.ContainsKey(profileDirectory))
            {
                data.Remove(profileDirectory);
                Save(data);
            }
        }

        public static bool Verify(
            string profileDirectory,
            string password)
        {
            ProfilePassword saved = Get(profileDirectory);

            if (saved == null || !saved.RequiresPassword)
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(saved.Salt);

                string enteredHash = CreateHash(password, salt);

                return SlowEquals(
                    enteredHash,
                    saved.PasswordHash
                );
            }
            catch
            {
                return false;
            }
        }

        private static string CreateHash(
            string password,
            byte[] salt)
        {
            using (Rfc2898DeriveBytes derive =
                new Rfc2898DeriveBytes(password, salt, 100000))
            {
                byte[] hash = derive.GetBytes(32);

                return Convert.ToBase64String(hash);
            }
        }

        private static bool SlowEquals(string a, string b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            byte[] first = Convert.FromBase64String(a);
            byte[] second = Convert.FromBase64String(b);

            if (first.Length != second.Length)
            {
                return false;
            }

            int difference = 0;

            for (int i = 0; i < first.Length; i++)
            {
                difference |= first[i] ^ second[i];
            }

            return difference == 0;
        }
        public static bool HasSettingsPin()
        {
            return Get("APP_SETTINGS_PIN") != null;
        }

        public static void SetSettingsPin(string pin)
        {
            SetPassword("APP_SETTINGS_PIN", pin);
        }

        public static bool VerifySettingsPin(string pin)
        {
            return Verify("APP_SETTINGS_PIN", pin);
        }
    }
}