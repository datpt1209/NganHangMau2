using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Group data by BloodGroup
            groupedData = bloodBags
                .GroupBy(b => b.BloodGroup)
                .ToDictionary(g => g.Key, g => g.Select(b => new SubReportData
                {
                    BloodGroup = b.BloodGroup,
                    Id = b.Id,
                    ExpiryDate = b.ExpiryDate,
                    ProductionDate = b.ProductionDate,
                    BloodProductType = b.BloodProductType,
                    EnteredBy = b.EnteredBy
                }).ToList());

            // Create a list of unique blood groups for the main report
            var reportSource = groupedData.Keys.Select(bloodGroup => new { BloodGroup = bloodGroup }).ToList();
            reportViewer.LocalReport.ReportEmbeddedResource = "NganHangMau2.BloodStorageReport.rdlc";

            ReportDataSource rds = new ReportDataSource();
            rds.Name = "DataSet1";
            rds.Value = reportSource;
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.LocalReport.DataSources.Add(rds);

            // Add SubreportProcessing event handler
            reportViewer.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);


            // Set display mode to Print Preview
            reportViewer.SetDisplayMode(DisplayMode.PrintLayout);


            reportViewer.RefreshReport();
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
