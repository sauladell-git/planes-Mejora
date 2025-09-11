using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class LineTotalsReportDTO
    {
        public LineTotalsReportDTO()
        {
        }

        public int ImprovementPlanId { get; set; }
        public int? LineId { get; set; }
        public decimal? Inventariable { get; set; }
        public decimal? NoInventariable { get; set; }
    }
}
