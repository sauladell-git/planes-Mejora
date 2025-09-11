using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class ImprovementPlanListDTO
    {
        public ImprovementPlanListDTO()
        {
            Filters = new FiltersDTO();
        }

        public FiltersDTO Filters { get; set; }
        public List<ImprovementPlan> ImprovementPlans { get; set; }
    }
}
