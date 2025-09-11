using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class ListSolicitudesDTO
    {
        public int SolicitudeId { get; set; }
        public string CUE { get; set; }
        public string Details { get; set; }
        public string SchoolYear { get; set; }
        public string StatusDescription { get; set; }
        public string Line { get; set; }
        public string ExpenditureType { get; set; }
        public string Specialization { get; set; }
        public string SolicitudeType { get; set; }
        public string Reassigned { get; set; }
        public string MeasurementUnit { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal RequestedPriceUnit { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal ApprovedPriceUnit { get; set; }
        public decimal Requested { get; set; }
        public decimal Approved { get; set; }
    }
}
