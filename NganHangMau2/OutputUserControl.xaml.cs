using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
        string currentUserName = UserManager.Instance.CurrentUserName;
        string connectionString = AppConfig.GetConnectionString();
        private ToastNotificationService _tn;
        BloodBag currentBloodBag = null;

        public OutputUserControl()
        {
            InitializeComponent();
            _tn = new ToastNotificationService();
            Unloaded += OnUnload;
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            _tn.OnUnloaded();
        }

        private void txtId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtId.Text) || txtId.Text.Length <= 18)
                {
                    return;
                }

                e.Handled = true; // Prevent the tab key from moving focus to the next control
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    string scannedData = textBox.Text.Trim();
                    scannedData = scannedData.Replace("\t", ""); // Remove tab characters from the scanned data

                    try
                    {
                        string decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(scannedData));
                        BloodBag bloodBag = ParseBloodBag(decodedData);
                        var bloodBagFromDb = SearchBloodBagById(bloodBag.Id);
                        if (bloodBagFromDb != null && bloodBagFromDb.Status == "Available")
                        {
                            currentBloodBag = bloodBagFromDb;
                            DisplayBloodBagInfo(bloodBagFromDb);
                        }
                        else
                        {
                            MessageBox.Show("Blood bag not found or not available.");
                        }

                        ClearTxtId(textBox);
                    }
                    catch (FormatException ex)
                    {
                        MessageBox.Show("Invalid QR code data: " + ex.Message);
                    }

                    ClearTxtId(textBox);
                }
            }
        }

        private void DisplayBloodBagInfo(BloodBag bloodBag)
        {
            // Show blood bag info card
            bloodBagInfoCard.Visibility = Visibility.Visible;

            // Populate blood bag info
            lblBloodGroup.Text = bloodBag.BloodGroup;
            lblBloodProductType.Text = bloodBag.BloodProductType;
            lblProductionDate.Text = bloodBag.ProductionDate.ToString("dd/MM/yyyy");
            lblExpiryDate.Text = bloodBag.ExpiryDate.ToString("dd/MM/yyyy");
            lblEnteredDate.Text = bloodBag.EnteredDate.ToString("dd/MM/yyyy");
            lblEnteredBy.Text = bloodBag.EnteredBy;

            if (bloodBag.Status == "Exported")
            {
               
                txtId.Clear();
                txtId.Focus();
                lblExportedBy.Text = bloodBag.ExportedBy;
                lblExportedDate.Text = bloodBag.ExportedDate.ToString("dd/MM/yyyy");
                txtPatientId.Text = bloodBag.ExportedTo;
                stExportedBy.Visibility = Visibility.Visible;
                stExportedDate.Visibility = Visibility.Visible;

                // Disable editing
                txtPatientId.IsEnabled = false;
                btnSave.Visibility = Visibility.Hidden;

                ToastNotificationService.ShowInformation("This blood bag has already been exported.");
            }
            else
            {
                // Enable editing
                txtPatientId.IsEnabled = true;
                txtPatientId.Clear();
                btnSave.Visibility = Visibility.Visible;
                stExportedBy.Visibility = Visibility.Hidden;
                stExportedDate.Visibility = Visibility.Hidden;
                // Set focus to txtPatientId
                txtPatientId.Focus();

            }

            // Show patient info card
            patientInfoCard.Visibility = Visibility.Visible;
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
                                var bloodBag = new BloodBag
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

                                if (bloodBag.Status == "Exported")
                                {
                                    reader.Close();
                                    string exportQuery = "SELECT * FROM ExportedBloodBags WHERE BloodBagID = @Id";
                                    using (SqlCommand exportCommand = new SqlCommand(exportQuery, connection))
                                    {
                                        exportCommand.Parameters.AddWithValue("@Id", bloodBagId);
                                        using (SqlDataReader exportReader = exportCommand.ExecuteReader())
                                        {
                                            if (exportReader.Read())
                                            {
                                                bloodBag.ExportedBy = exportReader["ExportedBy"].ToString();
                                                bloodBag.ExportedDate = Convert.ToDateTime(exportReader["ExportedDate"]);
                                                bloodBag.ExportedTo = exportReader["ExportedTo"].ToString();
                                            }
                                        }
                                    }
                                }

                                return bloodBag;
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

        private void ExportBloodBag(string bloodBagId, string patientName, DateTime exportedDate, string exportedBy)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("ExportBloodBag", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@IdBloodBag", bloodBagId);
                        command.Parameters.AddWithValue("@PatientName", patientName);
                        command.Parameters.AddWithValue("@ExportedDate", exportedDate);
                        command.Parameters.AddWithValue("@ExportedBy", exportedBy);

                        command.ExecuteNonQuery();
                    }
                    ToastNotificationService.ShowSuccess("Blood bag exported successfully!");
                    ClearTxtId(txtId);
                    txtId.Clear();
                    txtId.Focus();
                    lblExportedBy.Text = exportedBy;
                    lblExportedDate.Text = exportedDate.ToString("dd/MM/yyyy");
                    txtPatientId.Text = patientName;
                    stExportedBy.Visibility = Visibility.Visible;
                    stExportedDate.Visibility = Visibility.Visible;

                    // Disable editing
                    txtPatientId.IsEnabled = false;
                    btnSave.Visibility = Visibility.Hidden;


                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"An error occurred while exporting blood bag: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
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

            currentBloodBag = SearchBloodBagById(bloodBagId);

            if (currentBloodBag != null)
            {
                DisplayBloodBagInfo(currentBloodBag);
            }
            else
            {
                MessageBox.Show("Blood bag not found.");
                txtId.Clear();
                txtId.Focus();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (currentBloodBag != null)
            {
                ExportBloodBag(currentBloodBag.Id, txtPatientId.Text, DateTime.Now, currentUserName);
                currentBloodBag = null;
            }
            else
            {
                MessageBox.Show("No blood bag selected for export.");
            }
        }
    }
}
