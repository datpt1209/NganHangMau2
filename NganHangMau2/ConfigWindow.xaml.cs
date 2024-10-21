using System;
using System.Configuration;
using System.Windows;

namespace NganHangMau2
{
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            DecryptConfigSection("connectionStrings");
            string connectionString = ConfigurationManager.ConnectionStrings["BloodBankDB"].ConnectionString;
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            txtServerName.Text = builder.DataSource;
            txtDatabaseName.Text = builder.InitialCatalog;
            txtUserId.Text = builder.UserID;
            txtPassword.Password = builder.Password;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string serverName = txtServerName.Text;
            string databaseName = txtDatabaseName.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Password;

            string connectionString = $"Server={serverName};Database={databaseName};User Id={userId};Password={password};";
            SaveConnectionString("BloodBankDB", connectionString);
            EncryptConfigSection("connectionStrings");
            MessageBox.Show("Configuration saved and encrypted successfully!");
            this.Close();
        }

        private void SaveConnectionString(string name, string connectionString)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringsSection section = config.ConnectionStrings;

            if (section.ConnectionStrings[name] != null)
            {
                section.ConnectionStrings[name].ConnectionString = connectionString;
            }
            else
            {
                section.ConnectionStrings.Add(new ConnectionStringSettings(name, connectionString));
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        private void DecryptConfigSection(string sectionKey)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection section = config.GetSection(sectionKey);

            if (section != null && section.SectionInformation.IsProtected)
            {
                section.SectionInformation.UnprotectSection();
                config.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection(sectionKey);
            }
        }

        private void EncryptConfigSection(string sectionKey)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection section = config.GetSection(sectionKey);

            if (section != null && !section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                config.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection(sectionKey);
            }
        }
    }
}
