using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class StatementOfAccountDTO
    {
        public string SchoolYear { get; set; }
        public string BudgetYear { get; set; }
        public string Province { get; set; }
        public int ProvinceId { get; set; }
        public decimal TotalRequested { get; set; }

        public decimal TotalRequested_B { get; set; }
        public decimal TotalRequested_V { get; set; }
        public decimal TotalRequested_R { get; set; }
        public decimal TotalInDictum { get; set; }
        public decimal TotalApproved { get; set; }
        public decimal TotalElegible { get; set; }
        public decimal TotalInResolution { get; set; }
        public decimal TotalPlanned { get; set; }
        public decimal Rejected { get; set; }
        public decimal Dismissed { get; set; }
        public decimal Reassigned { get; set; }
        public decimal TotalPending { get; set; }
        // 23-09-21 campos agregados por SIA
        public decimal TotalApproved_Capital { get; set; }
        public decimal TotalApproved_Ordinary { get; set; }

        public decimal TotalApproved_B { get; set; }
        public decimal TotalApproved_V { get; set; }
        public decimal TotalApproved_R { get; set; }
    }
}