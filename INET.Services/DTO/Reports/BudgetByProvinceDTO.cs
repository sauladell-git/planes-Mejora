using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class BudgetByProvinceDTO
    {
        public string SchoolYear { get; set; }
        public string BudgetYear { get; set; }
        public string Province { get; set; }
    }

    public class BudgetByProvinceDetailsDTO
    {
        public int ImprovementPlanType { get; set; }
        public int CategoryId { get; set; }
        public int SchoolYearId { get; set; }
        public int FieldId { get; set; }
        public string Field { get; set; }
        public string Line { get; set; }
        public int LineId { get; set; }
        public string LineCode { get; set; }
        public int ProvinceId { get; set; }
        public string ProvinceCode { get; set; }
        public string ProvinceNumber { get; set; }
        public decimal TotalPlanned { get; set; }
        public decimal TotalRejected { get; set; }
        public decimal TotalRequested { get; set; }
        public decimal TotalInResolutions { get; set; }
        public decimal TotalDismissed { get; set; }
    }

}
