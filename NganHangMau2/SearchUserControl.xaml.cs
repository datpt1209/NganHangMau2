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
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchUserControl : UserControl
    {

        private List<BloodBag> bloodBags = new List<BloodBag>();
        string currentUserName = UserManager.Instance.CurrentUserName;
        string connectionString = AppConfig.GetConnectionString();
        public SearchUserControl()
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
            cmbStatus.SelectedIndex = -1;
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
                cmbStatus.ItemsSource = bloodData.Statuses;
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
                            BloodBag searchBag = SearchBloodBagById(bloodBag.Id);
                            bloodBags.Add(searchBag);
                            UpdateBloodBagList();
                            ClearTxtId(textBox);
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // Lấy các giá trị từ các điều kiện tìm kiếm
            string id = txtId.Text;
            string aboBloodType = cmbABOBloodType.Text;
            string rhesusBloodType = cmbRhesusBloodType.Text;
            string bloodProductType = cmbBloodProductType.Text;
            string volume = cmbVolume.Text;
            DateTime? productionDate = dtpProductionDate.SelectedDate;
            DateTime? expiryDate = dtpExpiryDate.SelectedDate;
            DateTime? inputDate = dtinputDate.SelectedDate;
            string status = cmbStatus.Text;

            // Thực hiện truy vấn cơ sở dữ liệu với các điều kiện tìm kiếm
            var results = SearchBloodBags(id, aboBloodType, rhesusBloodType, bloodProductType, volume, productionDate, expiryDate, inputDate, status);

            // Cập nhật DataGrid với kết quả tìm kiếm
            bloodBags.Clear();
            bloodBags = results;
            dgvBloodBags.ItemsSource = results;
        }

        private BloodBag SearchBloodBagById(string id)
        {
            BloodBag result = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM BloodBags WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result = new BloodBag
                                {
                                    Id = reader["Id"].ToString(),
                                    BloodGroup = reader["BloodGroup"].ToString(),
                                    ProductionDate = Convert.ToDateTime(reader["ProductionDate"]),
                                    ExpiryDate = Convert.ToDateTime(reader["ExpiryDate"]),
                                    BloodProductType = reader["BloodProductType"].ToString(),
                                    Volume = reader["Volume"].ToString(),
                                    EnteredBy = reader["EnteredBy"].ToString(),
                                    EnteredDate = Convert.ToDateTime(reader["EnteredDate"]),
                                    Status = reader["Status"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"An error occurred while searching for the blood bag: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }

            return result;
        }


        private List<BloodBag> SearchBloodBags(string id, string aboBloodType, string rhesusBloodType, string bloodProductType, string volume, DateTime? productionDate, DateTime? expiryDate, DateTime? inputDate, string status)
        {
            List<BloodBag> results = new List<BloodBag>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    StringBuilder queryBuilder = new StringBuilder("SELECT * FROM BloodBags WHERE 1=1");

                    if (!string.IsNullOrEmpty(id))
                    {
                        queryBuilder.Append(" AND Id LIKE @Id");
                    }
                    if (!string.IsNullOrEmpty(aboBloodType))
                    {
                        queryBuilder.Append(" AND ABO_Group = @ABO_Group");
                    }

                    if (!string.IsNullOrEmpty(rhesusBloodType))
                    {
                        queryBuilder.Append(" AND Rhesus_Group = @Rhesus_Group");
                    }

                    if (!string.IsNullOrEmpty(bloodProductType))
                    {
                        queryBuilder.Append(" AND BloodProductType LIKE @BloodProductType");
                    }
                    if (!string.IsNullOrEmpty(volume))
                    {
                        queryBuilder.Append(" AND Volume = @Volume");
                    }
                    if (productionDate.HasValue)
                    {
                        queryBuilder.Append(" AND CONVERT(date, ProductionDate) >= @ProductionDate");
                    }
                    if (expiryDate.HasValue)
                    {
                        queryBuilder.Append(" AND CONVERT(date, ExpiryDate) <= @ExpiryDate");
                    }
                    if (inputDate.HasValue)
                    {
                        queryBuilder.Append(" AND EnteredDate = @EnteredDate");
                    }
                    if (!string.IsNullOrEmpty(status))
                    {
                        queryBuilder.Append(" AND Status = @Status");
                    }

                    using (SqlCommand command = new SqlCommand(queryBuilder.ToString(), connection))
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            command.Parameters.AddWithValue("@Id", "%" + id + "%");
                        }
                        if (!string.IsNullOrEmpty(aboBloodType))
                        {

                            command.Parameters.AddWithValue("@ABO_Group", aboBloodType);

                        }
                        if (!string.IsNullOrEmpty(rhesusBloodType))

                        {
                                string convert_Rhesus = rhesusBloodType == "Dương tính" ? "+" : "-";
                                command.Parameters.AddWithValue("@Rhesus_Group", convert_Rhesus);
                        }
                      
                        if (!string.IsNullOrEmpty(bloodProductType))
                        {
                            command.Parameters.AddWithValue("@BloodProductType", "%" + bloodProductType + "%");
                        }
                        if (!string.IsNullOrEmpty(volume))
                        {
                            command.Parameters.AddWithValue("@Volume", volume);
                        }
                        if (productionDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@ProductionDate", productionDate.Value.Date);
                        }
                        if (expiryDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@ExpiryDate", expiryDate.Value.Date);
                        }
                        if (inputDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@EnteredDate", inputDate.Value.Date);
                        }
                        if (!string.IsNullOrEmpty(status))

                        {
                            string convert_Status = status == "Chưa xuất" ? "Available" : "Exported";
                            command.Parameters.AddWithValue("@Status", convert_Status);
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BloodBag bloodBag = new BloodBag
                                {
                                    Id = reader["Id"].ToString(),
                                    BloodGroup = reader["BloodGroup"].ToString(),
                                    ProductionDate = Convert.ToDateTime(reader["ProductionDate"]),
                                    ExpiryDate = Convert.ToDateTime(reader["ExpiryDate"]),
                                    BloodProductType = reader["BloodProductType"].ToString(),
                                    Volume = reader["Volume"].ToString(),
                                    EnteredBy = reader["EnteredBy"].ToString(),
                                    EnteredDate = Convert.ToDateTime(reader["EnteredDate"]),
                                    Status = reader["Status"].ToString()
                                };
                                results.Add(bloodBag);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"An error occurred while searching blood bags: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }

            return results;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
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
    }
}
