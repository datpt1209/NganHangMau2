using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NganHangMau2
{
    /// <summary>
    /// Interaction logic for InputUserControl.xaml
    /// </summary>
    public partial class InputUserControl : UserControl
    {
        private List<BloodBag> bloodBags = new List<BloodBag>();
        string currentUserName = UserManager.Instance.CurrentUserName;
        string connectionString = AppConfig.GetConnectionString();
        private ToastNotificationService _tn;
        public InputUserControl()
        {
            InitializeComponent();
            LoadBloodData();
            _tn = new ToastNotificationService();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtId.Text) || txtId.Text == "Mã túi máu")
            {
                MessageBox.Show("Vui lòng nhập mã túi máu");
                return;
            }
            if (cmbABOBloodType.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn nhóm máu ABO");
                return;
            }
            if (cmbRhesusBloodType.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn nhóm máu Rhesus");
                return;
            }
            if (cmbBloodProductType.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn loại sản phẩm máu");
                return;
            }
            if (cmbVolume.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn thể tích túi máu");
                return;
            }

            string bloodBagId = txtId.Text;
            if (bloodBags.Any(b => b.Id == bloodBagId))
            {
                MessageBox.Show("Mã túi máu đã tồn tại trong danh sách");
                return;
            }

            string aboBloodType = cmbABOBloodType.SelectedItem as string ?? string.Empty;
            string rhesusBloodType = cmbRhesusBloodType.SelectedItem as string ?? string.Empty;
            string bloodGroup = aboBloodType + (rhesusBloodType == "Dương tính" ? " +" : " -");
            string bloodProductType = cmbBloodProductType.SelectedItem as string ?? string.Empty;
            string volume = cmbVolume.SelectedItem as string ?? string.Empty;
            string storageTemperature = "2-6°C";

            if (bloodProductType == "Khối hồng cầu")
            {
                bloodProductType = $"KHỐI HỒNG CẦU TỪ {volume} MÁU TOÀN PHẦN";
            }
            if (bloodProductType == "Huyết tương tươi đông lạnh")
            {
                bloodProductType = $"HUYẾT TƯƠNG TƯƠI ĐÔNG LẠNH {volume}";
                storageTemperature = "-30°C";
            }
            if (bloodProductType == "Tiểu cầu đậm đặc")
            {
                bloodProductType = $"TIỂU CẦU ĐẬM ĐẶC {volume}";
                storageTemperature = "20-25°C lắc liên tục";
            }

            BloodBag bloodBag = new BloodBag
            {
                Id = txtId.Text,
                BloodGroup = bloodGroup,
                ABO_Group = aboBloodType,
                Rhesus_Group = rhesusBloodType == "Dương tính" ? "+" : "-",
                Volume = volume,
                VolumeNum = int.Parse(volume.Substring(0,3)),
                ProductionDate = dtpProductionDate.SelectedDate ?? DateTime.Now,
                ExpiryDate = dtpExpiryDate.SelectedDate ?? DateTime.Now,
                BloodProductType = bloodProductType,
                EnteredBy = currentUserName, 
                EnteredDate = DateTime.Now.Date,
                StorageTemperature = storageTemperature,
                Status = "Available"
            };

            bloodBags.Add(bloodBag);
            UpdateBloodBagList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveBloodBagsToDatabase();
        }

        private void btnConfigureDatabase_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow();
            configWindow.ShowDialog();
        }
        private void SaveBloodBagsToDatabase()
        {
            if (bloodBags.Count == 0)
            {
                MessageBox.Show("Không có túi máu nào để lưu");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (var bloodBag in bloodBags.ToList())
                            {
                                string query = "INSERT INTO BloodBags (Id, BloodGroup, ABO_Group, Rhesus_Group, ProductionDate, ExpiryDate, BloodProductType, Volume, VolumeNum, StorageTemperature, EnteredBy, EnteredDate, Status) " +
                                               "VALUES (@Id, @BloodGroup, @ABO_Group, @Rhesus_Group, @ProductionDate, @ExpiryDate, @BloodProductType, @Volume, @VolumeNum, @StorageTemperature, @EnteredBy, @EnteredDate, @Status)";

                                using (SqlCommand command = new SqlCommand(query, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Id", bloodBag.Id);
                                    command.Parameters.AddWithValue("@BloodGroup", bloodBag.BloodGroup);
                                    command.Parameters.AddWithValue("@ABO_Group", bloodBag.ABO_Group);
                                    command.Parameters.AddWithValue("@Rhesus_Group", bloodBag.Rhesus_Group);
                                    command.Parameters.AddWithValue("@ProductionDate", bloodBag.ProductionDate);
                                    command.Parameters.AddWithValue("@ExpiryDate", bloodBag.ExpiryDate);
                                    command.Parameters.AddWithValue("@BloodProductType", bloodBag.BloodProductType);
                                    command.Parameters.AddWithValue("@Volume", bloodBag.Volume);
                                    command.Parameters.AddWithValue("@VolumeNum", bloodBag.VolumeNum);
                                    command.Parameters.AddWithValue("@StorageTemperature", bloodBag.StorageTemperature);
                                    command.Parameters.AddWithValue("@EnteredBy", bloodBag.EnteredBy);
                                    command.Parameters.AddWithValue("@EnteredDate", bloodBag.EnteredDate);
                                    command.Parameters.AddWithValue("@Status", bloodBag.Status);

                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            ToastNotificationService.ShowSuccess("Blood bags saved successfully!");
                            PrintBloodBagReport();

                            // Reset the list and clear the text boxes
                            UpdateBloodBagList();
                            ClearInputFields();
                        }
                        catch (SqlException ex)
                        {
                            if (transaction.Connection != null)
                            {
                                transaction.Rollback();
                            }
                            LogError(ex); // Log detailed error information
                            MessageBox.Show($"An error occurred while saving blood bags to the database: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            if (transaction.Connection != null)
                            {
                                transaction.Rollback();
                            }
                            LogError(ex); // Log detailed error information
                            MessageBox.Show($"An unexpected error occurred: {ex.Message}");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                LogError(ex); // Log detailed error information
                MessageBox.Show($"An error occurred while connecting to the database: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogError(ex); // Log detailed error information
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
        }
        private void LogError(Exception ex)
        {
            // Log the error details to a file or other logging mechanism
            string logFilePath = "error_log.txt";
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {ex.ToString()}{Environment.NewLine}");
        }

        private void ClearInputFields()
        {
            bloodBags.Clear();
            txtId.Clear();
            cmbABOBloodType.SelectedIndex = -1;
            cmbRhesusBloodType.SelectedIndex = -1;
            cmbBloodProductType.SelectedIndex = -1;
            cmbVolume.SelectedIndex = -1;
            dtpProductionDate.SelectedDate = null;
            dtpExpiryDate.SelectedDate = null;
        }
        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            // Scan button logic
        }

        private BloodBag ParseBloodBag(string data)
        {
            BloodBag bloodBag = new BloodBag();

            // Tách các thông tin từ chuỗi data
            string[] parts = data.Split('|');

            if (parts.Length >= 8)
            {
                bloodBag.Id = parts[0].Trim();
                bloodBag.BloodGroup = parts[1].Trim();
                bloodBag.ABO_Group = parts[1].Trim().Split(' ').Length > 1 ? (parts[1].Trim().Split(' ')[0]) : string.Empty;
                bloodBag.Rhesus_Group = parts[1].Trim().Split(' ').Length > 1 ? (parts[1].Trim().Split(' ')[1]) : string.Empty;
                bloodBag.ProductionDate = DateTime.Parse(parts[3]);
                bloodBag.ExpiryDate = DateTime.Parse(parts[4]);
                bloodBag.EnteredBy = currentUserName;
                bloodBag.BloodProductType = parts[6].Trim();
                bloodBag.Volume = ExtractVolume(parts[6].Trim());
                bloodBag.VolumeNum = int.Parse(bloodBag.Volume.Substring(0, 3));
                bloodBag.StorageTemperature = parts[7].Trim();
                bloodBag.Status = "Available";
                bloodBag.EnteredDate = DateTime.Now.Date;
            }

            return bloodBag;
        }

        private string ExtractVolume(string bloodProductType)
        {
            // Biểu thức chính quy để tìm thể tích (số theo sau bởi "mL" hoặc "ml")
            var match = System.Text.RegularExpressions.Regex.Match(bloodProductType, @"(\d+)\s?ml", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value + "ml";
            }
            return string.Empty;
        }

        private void UpdateBloodBagList()
        {
            dgvBloodBags.ItemsSource = null;
            dgvBloodBags.ItemsSource = bloodBags;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintBloodBagReport();
        }

        private void PrintBloodBagReport()
        {
            if (bloodBags.Count == 0)
            {
                MessageBox.Show("Không có túi máu nào để in");
                return;
            }
            var reportWindow = new ReportWindow(bloodBags);
            reportWindow.Show();
        }

        private void LoadBloodData()
        {
            string jsonFilePath = "BloodData.json";
            if (File.Exists(jsonFilePath))
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                var bloodData = JsonSerializer.Deserialize<BloodData>(jsonData);

                cmbABOBloodType.ItemsSource = bloodData.ABOBloodTypes;
                cmbRhesusBloodType.ItemsSource = bloodData.RhesusBloodTypes;
                cmbBloodProductType.ItemsSource = bloodData.BloodProductTypes;
                cmbVolume.ItemsSource = bloodData.Volumes;
            }
            else
            {
                MessageBox.Show("BloodData.json file not found.");
            }
        }

        private void txtId_PreviewKeyDown(object sender, KeyEventArgs e)
       {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtId.Text) || txtId.Text.Length <= 18)
                {
                    ClearTxtId(txtId);
                    return;
                }
                else
                {
                    e.Handled = true; // Prevent the tab key from moving focus to the next control
                    TextBox textBox = sender as TextBox;
                    if (textBox != null)
                    {
                        string scannedData = textBox.Text.Trim();

                        // Remove tab characters from the scanned data
                        scannedData = scannedData.Replace("\t", "");

                        try
                        {
                            string decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(scannedData));
                            BloodBag bloodBag = ParseBloodBag(decodedData);

                            if (bloodBags.Any(b => b.Id == bloodBag.Id))
                            {
                                MessageBox.Show("Mã túi máu đã tồn tại trong danh sách");
                                // Clear the TextBox and set focus back to it
                                ClearTxtId(textBox);
                                return;
                            }
                            bloodBags.Add(bloodBag);
                            UpdateBloodBagList();
                        }
                        catch (System.FormatException ex)
                        {
                            MessageBox.Show("Invalid QR code data: " + ex.Message);
                        }

                        ClearTxtId(textBox);
                    }
                }
            }
        }

        private static void ClearTxtId(TextBox textBox)
        {
            // Clear the TextBox and set focus back to it
            textBox.Clear();
            textBox.Focus();
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearInputFields();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtId.Focus();
        }
    }
}