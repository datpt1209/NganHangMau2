using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NganHangMau2
{
    public class BloodBag
    {
        public string Id { get; set; }
        public string BloodGroup { get; set; }
        public string ABO_Group { get; set; }
        public string Rhesus_Group { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BloodProductType { get; set; }
        public string Volume { get; set; }
        public int VolumeNum { get; set; }
        public string StorageTemperature { get; set; }
        public string EnteredBy { get; set; } = string.Empty;
        public DateTime EnteredDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = string.Empty;
        public string ExportedBy { get; set; } = string.Empty;
        public DateTime ExportedDate { get; set; } = DateTime.Now;
        public string ExportedTo { get; set; } = string.Empty;


    }
}
