using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for OutputUserControl.xaml
    /// </summary>
    public partial class OutputUserControl : UserControl
    {
        private List<BloodBag> bloodBagsToExport = new List<BloodBag>();
        string currentUserName = UserManager.Instance.CurrentUserName;
        string connectionString = AppConfig.GetConnectionString();
        public OutputUserControl()
        {
            InitializeComponent();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            foreach (var bloodBag in bloodBagsToExport)
            {
                UpdateBloodBagStatus(bloodBag.Id, "Exported");
            }

            MessageBox.Show("Blood bags exported successfully!");
            bloodBagsToExport.Clear();
            UpdateBloodBagList();
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
                            autoExport(bloodBag.Id);
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

        private BloodBag SearchBloodBagById(string bloodBagId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM BloodBags WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", bloodBagId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new BloodBag
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
                MessageBox.Show($"An error occurred while searching blood bag: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
            return null;
        }


        private void UpdateBloodBagStatus(string bloodBagId, string newStatus)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "UPDATE BloodBags SET Status = @Status WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", bloodBagId);
                        command.Parameters.AddWithValue("@Status", newStatus);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"An error occurred while updating blood bag status: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
        }

        private void UpdateBloodBagList()
        {
            dgvBloodBags.ItemsSource = null;
            dgvBloodBags.ItemsSource = bloodBagsToExport;
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
        private static void ClearTxtId(TextBox textBox)
        {
            // Clear the TextBox and set focus back to it
            textBox.Clear();
            textBox.Focus();
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string bloodBagId = txtId.Text;

            if (string.IsNullOrEmpty(bloodBagId))
            {
                MessageBox.Show("Please enter a blood bag ID.");
                return;
            }

            addExportList(bloodBagId);
        }

        private void addExportList(string bloodBagId)
        {
            var bloodBag = SearchBloodBagById(bloodBagId);

            if (bloodBag != null)
            {
                if (bloodBag.Status != "Exported")
                {
                    bloodBagsToExport.Add(bloodBag);
                    UpdateBloodBagList();
                }
                else
                {
                    MessageBox.Show("Blood bag has already been exported.");
                }
            }
            else
            {
                MessageBox.Show("Blood bag not found.");
            }
        }
        private void autoExport(string bloodBagId)
        {
            var bloodBag = SearchBloodBagById(bloodBagId);

            if (bloodBag != null)
            {
                if (bloodBag.Status != "Exported")
                {
                    UpdateBloodBagStatus(bloodBag.Id, "Exported");
                }
                else
                {
                    MessageBox.Show("Blood bag has already been exported.");
                }
            }
            else
            {
                MessageBox.Show("Blood bag not found.");
            }
        }
    }
}
