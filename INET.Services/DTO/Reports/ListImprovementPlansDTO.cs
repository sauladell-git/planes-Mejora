using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class ListImprovementPlansDTO
    {
        public string Identifier { get; set; }
        public string ImprovementPlanType { get; set; }
        public string CUE { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string SchoolYear { get; set; }
        public string Field { get; set; }
        public int SolicitudesCount { get; set; }
        public string Evaluator { get; set; }
        public int CommentsCount { get; set; }
        public int IncidencesCount { get; set; }
        public string Status { get; set; }
    }
}
