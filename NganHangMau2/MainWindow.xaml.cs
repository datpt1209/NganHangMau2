using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Automation;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<BloodBag> bloodBags = new List<BloodBag>();
        private StringBuilder qrCodeData = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtEnteredBy.Text))
            {
                MessageBox.Show("Vui lòng nhập tên người nhập");
                return;
            }
            if(string.IsNullOrEmpty(txtId.Text) || txtId.Text == "Mã túi máu")
            {
                MessageBox.Show("Vui lòng nhập mã túi máu");
                return;
            }
            if(cmbABOBloodType.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn nhóm máu ABO");
                return;
            }
            if(cmbRhesusBloodType.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn nhóm máu Rhesus");
                return;
            }
            if(cmbBloodProductType.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn loại sản phẩm máu");
                return;
            }
            if(cmbVolume.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn thể tích túi máu");
                return;
            }
            string aboBloodType = (cmbABOBloodType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
            string rhesusBloodType = (cmbRhesusBloodType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
            string bloodGroup = aboBloodType + (rhesusBloodType == "Dương tính" ? "+" : "-");
            string bloodProductType = (cmbBloodProductType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
            string volume = (cmbVolume.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
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
                Id =  txtId.Text,
                BloodGroup = bloodGroup,
                ProductionDate = dtpProductionDate.SelectedDate ?? DateTime.Now,
                ExpiryDate = dtpExpiryDate.SelectedDate ?? DateTime.Now,
                BloodProductType = bloodProductType,
                EnteredBy = txtEnteredBy.Text
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

            string connectionString = ConfigHelper.GetConnectionString("BloodBankDB");

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
                EnteredBy = txtEnteredBy.Text
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
            if(bloodBags.Count == 0)
            {
                MessageBox.Show("Không có túi máu nào để in");
                return;
            }
            var reportWindow = new ReportWindow(bloodBags);
            reportWindow.Show();
        }

        private void txtId_PreviewKeyDown(object sender, KeyEventArgs e)
            {
            if (e.Key == Key.Tab)
            {
                if (string.IsNullOrEmpty(txtEnteredBy.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên người nhập");
                    return;
                }
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
                            bloodBags.Add(bloodBag);
                            UpdateBloodBagList();
                        }
                        catch (System.FormatException ex)
                        {
                            MessageBox.Show("Invalid QR code data: " + ex.Message);
                        }

                        // Clear the TextBox and set focus back to it
                        textBox.Clear();
                        textBox.Focus();
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                }
            }
        }
    }
}