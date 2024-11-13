using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        public InputUserControl()
        {
            InitializeComponent();
            LoadBloodData();
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
            string bloodGroup = aboBloodType + (rhesusBloodType == "Dương tính" ? "+" : "-");
            string bloodProductType = cmbBloodProductType.SelectedItem as string ?? string.Empty;
            string volume = cmbVolume.SelectedItem as string ?? string.Empty;

            if (bloodProductType == "Hồng cầu lắng")
            {
                bloodProductType = $"KHỐI HỒNG CẦU TỪ {volume} mL MÁU TOÀN PHẦN";
            }
            if (bloodProductType == "PFC")
            {
                bloodProductType = $"HUYẾT TƯƠNG TƯƠI ĐÔNG LẠNH {volume} mL";
            }
            if (bloodProductType == "Tiểu cầu")
            {
                bloodProductType = $"TIỂU CẦU ĐẬM ĐẶC {volume} mL";
            }

            BloodBag bloodBag = new BloodBag
            {
                Id = txtId.Text,
                BloodGroup = bloodGroup,
                ProductionDate = dtpProductionDate.SelectedDate ?? DateTime.Now,
                ExpiryDate = dtpExpiryDate.SelectedDate ?? DateTime.Now,
                BloodProductType = bloodProductType,
                EnteredBy = currentUserName
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

                    foreach (var bloodBag in bloodBags)
                    {
                        string query = "INSERT INTO BloodBags (Id, BloodGroup, ProductionDate, ExpiryDate, BloodProductType, EnteredBy, EnteredDate) " +
                                       "VALUES (@Id, @BloodGroup, @ProductionDate, @ExpiryDate, @BloodProductType, @EnteredBy, @EnteredDate)";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Id", bloodBag.Id);
                            command.Parameters.AddWithValue("@BloodGroup", bloodBag.BloodGroup);
                            command.Parameters.AddWithValue("@ProductionDate", bloodBag.ProductionDate);
                            command.Parameters.AddWithValue("@ExpiryDate", bloodBag.ExpiryDate);
                            command.Parameters.AddWithValue("@BloodProductType", bloodBag.BloodProductType);
                            command.Parameters.AddWithValue("@EnteredBy", bloodBag.EnteredBy);
                            command.Parameters.AddWithValue("@EnteredDate", bloodBag.EnteredDate);

                            command.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Blood bags saved successfully!");
                PrintBloodBagReport();


                // Reset the list and clear the text boxes
                bloodBags.Clear();
                UpdateBloodBagList();
                ClearInputFields();

            }
            catch (SqlException ex)
            {
                MessageBox.Show($"An error occurred while saving blood bags to the database: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
        }

        private void ClearInputFields()
        {
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
            var parts = data.Split('|');
            return new BloodBag
            {
                Id = parts[0],
                BloodGroup = parts[1],
                ProductionDate = DateTime.Parse(parts[3]),
                ExpiryDate = DateTime.Parse(parts[4]),
                BloodProductType = parts[6],
                EnteredBy = currentUserName
            };
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
            if (e.Key == Key.Tab)
            {
                if (string.IsNullOrEmpty(txtId.Text) || txtId.Text.Length <= 18)
                {
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
    }
}