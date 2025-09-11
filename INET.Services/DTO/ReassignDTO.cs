using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class ReassignDTO
    {
        public int Id { get; set; }
        public string Details { get; set; }
        public string Resolution { get; set; }
        public decimal RequestedTotal { get; set; }
        public decimal ApprovedTotal { get; set; }
        public decimal ReassignedTotal { get; set; }
        public decimal AvailableTotal { get; set; }

        // TODO: Formatear directamente en JavaScript. No pueden ser de solo lectura por el JSONSerializer no las serializa
        public string RequestedTotalAsCurrency { get; set; }        
        public string ApprovedTotalAsCurrency { get; set; }        
        public string ReassignedTotalAsCurrency { get; set; }
        public string AvailableTotalAsCurrency { get; set; }
    }
}
