using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Windows;


namespace NganHangMau2
{
    public class AppConfig
    {
        public static string Username { get; set; } = "";
        public static string Password { get; set; } = "";
        public static string Server { get; set; } = "";
        public static string Database { get; set; } = "";
        public static string Entropy { get; set; } = "";
        public static string PasswordIn64 { get; set; } = "";

        public static void ReloadSetting()
        {
            var config = System.Configuration.ConfigurationManager.AppSettings;
            Username = config["Username"] ?? "";
            PasswordIn64 = config["Password"] ?? "";
            var entropyIn64 = config["Entropy"] ?? "";
            Server = config["Server"] ?? "";
            Database = config["Database"] ?? "";

            if (PasswordIn64.Length != 0)
            {
                try
                {
                    Password = AesEncryption.Decrypt(PasswordIn64);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error decrypting password: {ex.Message}");
                }
            }
        }
        public static void Save()
        {
            Configuration config = System.Configuration.
                        ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Username"].Value = Username;
            config.AppSettings.Settings["Password"].Value = PasswordIn64;
            config.AppSettings.Settings["Entropy"].Value = Entropy;
            config.Save(ConfigurationSaveMode.Full);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }
        public static string GetConnectionString()
        {
            ReloadSetting();
            string connectionString =  BuildConnectionString(Server, Database, Username, Password);
            return connectionString;

        }
        public static string BuildConnectionString(string server, string database, string username, string password)
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder();
            builder.DataSource = server;
            builder.InitialCatalog = database;
            builder.TrustServerCertificate = true;
            builder.UserID = username;
            builder.Password = password;

            string connectionString = builder.ConnectionString;
            return connectionString;
        }
    }
}
