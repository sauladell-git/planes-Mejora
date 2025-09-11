using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class SolicitudeReportDTO
    {
        public string Identifier { get; set; }
        public string CUE { get; set; }
        public string Details { get; set; }
        public string Line { get; set; }
        public string ExpenditureType { get; set; }
        public string MeasurementUnit { get; set; }
        public string SchoolYear { get; set; }
        public string SolicitudeType { get; set; }
        public string Status { get; set; }
        public string Specialization { get; set; }
        public string RequestedAmount { get; set; }
        public string RequestedPriceUnit { get; set; }
        public string RequestedTotal { get; set; }
        public string ApprovedAmount { get; set; }
        public string ApprovedPriceUnit { get; set; }
        public string ApprovedTotal { get; set; }        
    }
}