using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INET.Services.DTO
{
    public class AccountRenderingViewDTO
    {
        public int AccountingRenderingID { get; set; }

        public int ImprovementPlanId { get; set; }
        public string Number { get; set; }

        public string DictumNumber { get; set; }

        public string ExpenditureObjectType { get; set; }
        public decimal AprovedAmount { get; set; }
        public decimal RejectedAmount { get; set; }
        public string Comments { get; set; }
        public DateTime Date { get; set; }
    }
}
