using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NganHangMau2
{
    public class BloodBag
    {
        public string Id { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public DateTime ProductionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BloodProductType { get; set; } = string.Empty;
        public string EnteredBy { get; set; } = string.Empty;
        public DateTime EnteredDate { get; set; } = DateTime.Now;

    }
}
