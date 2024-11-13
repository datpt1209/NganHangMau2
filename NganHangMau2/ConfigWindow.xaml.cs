using Microsoft.Data.SqlClient;
using System;
using System.Configuration;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
                AppConfig.ReloadSetting();
                txtUserId.Text = AppConfig.Username;
                txtPassword.Password = AppConfig.Password;
                txtDatabase.Text = AppConfig.Database;
                txtServer.Text = AppConfig.Server;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Configuration config = System.Configuration.
                    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["Server"].Value = txtServer.Text;
                config.AppSettings.Settings["Database"].Value = txtDatabase.Text;
                config.AppSettings.Settings["Username"].Value = txtUserId.Text;

                var encryptedPassword = AesEncryption.Encrypt(txtPassword.Password);
                config.AppSettings.Settings["Password"].Value = encryptedPassword;
                config.Save(ConfigurationSaveMode.Full);
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");

                MessageBox.Show("Configuration saved successfully.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnTestConection_Click(object sender, RoutedEventArgs e)
        {
            string server = txtServer.Text;
            string database = txtDatabase.Text;
            string userId = txtUserId.Text;
            string password = txtPassword.Password;

            string connectionString = AppConfig.BuildConnectionString(server, database, userId, password);
            if (IsDatabaseConnectionSuccessful(connectionString))
            {
                MessageBox.Show("Connection successful.");
            }
            else
            {
                MessageBox.Show("Connection failed.");
            }
        }
        public static bool IsDatabaseConnectionSuccessful(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true; // Connection successful
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"SQL Exception: {ex.Message}");
                return false; // Connection failed
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}");
                return false; // Connection failed
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
