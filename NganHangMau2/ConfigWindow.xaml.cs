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
            try
            {
               
                string connectionString = ConfigHelper.GetConnectionString("BloodBankDB");
                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                txtServerName.Text = builder.DataSource;
                txtDatabaseName.Text = builder.InitialCatalog;
                txtUserId.Text = builder.UserID;
                txtPassword.Password = builder.Password;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string serverName = txtServerName.Text;
            string databaseName = txtDatabaseName.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Password;

            string connectionString = $"Server={serverName};Database={databaseName};User Id={userId};Password={password};";
            ConfigHelper.SaveConnectionString("BloodBankDB", connectionString);
            ConfigHelper.EncryptConfigSection("connectionStrings");
            MessageBox.Show("Configuration saved and encrypted successfully!");
            this.Close();
        }
    }
}
