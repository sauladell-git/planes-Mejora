using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INET.Data;

namespace INET.Services.DTO
{
  public  class ImportCompleteDTO
    { public List<Solicitude> solicitados_plan { get; set; }
        public List<Solicitude> solicitados_hijos { get; set; }
        public ImportCompleteDTO()
        {
            solicitados_hijos = new List<Solicitude>();
            solicitados_plan = new List<Solicitude>();

        }
    }
}
