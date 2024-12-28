using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms.Integration;

namespace NganHangMau2
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        private List<BloodBag> bloodBags;
        private Dictionary<string, List<SubReportData>> groupedData;

        public ReportWindow(List<BloodBag> bloodBags)
        {
            InitializeComponent();
            this.bloodBags = bloodBags;
            this.Loaded += Window_Loaded;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the first WindowsFormsHost
            ReportViewer reportViewer1 = new ReportViewer();
            reportViewer1.LocalReport.ReportEmbeddedResource = "NganHangMau2.BloodStorageReport2.rdlc";
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", bloodBags));
            reportViewer1.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer1.RefreshReport();
            windowsFormsHost1.Child = reportViewer1;

            // Initialize the second WindowsFormsHost
            ReportViewer reportViewer2 = new ReportViewer();
            reportViewer2.LocalReport.ReportEmbeddedResource = "NganHangMau2.BloodStorage_BloodGroup.rdlc";
            reportViewer2.LocalReport.DataSources.Clear();
            reportViewer2.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", bloodBags));
            reportViewer2.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer2.RefreshReport();
            windowsFormsHost2.Child = reportViewer2;

            radioButton1.IsChecked = true;
        }

        void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            // Provide data for the subreport
            var bloodGroup = e.Parameters["BloodGroup"].Values[0];
            if (groupedData.TryGetValue(bloodGroup, out var subReportData))
            {
                e.DataSources.Add(new ReportDataSource("SubReportDataSet", subReportData));
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (radioButton1.IsChecked == true)
            {
                if (windowsFormsHost1 != null)
                {
                    windowsFormsHost1.Visibility = Visibility.Visible;
                }
                if (windowsFormsHost2 != null)
                {
                    windowsFormsHost2.Visibility = Visibility.Collapsed;
                }
            }
            else if (radioButton2.IsChecked == true)
            {
                if (windowsFormsHost1 != null)
                {
                    windowsFormsHost1.Visibility = Visibility.Collapsed;
                }
                if (windowsFormsHost2 != null)
                {
                    windowsFormsHost2.Visibility = Visibility.Visible;
                }
            }
        }
    }

    public class SubReportData
    {
        public string Id { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public DateTime ProductionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BloodProductType { get; set; } = string.Empty;
        public string EnteredBy { get; set; } = string.Empty;
    }

}
