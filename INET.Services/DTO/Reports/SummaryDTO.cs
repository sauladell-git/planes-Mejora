using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class SummaryDTO
    {
        public string Field { get; set; }
        public string Line { get; set; }
        public string FileNumber { get; set; }
        public decimal TotalByLine { get; set; }
    }
}
