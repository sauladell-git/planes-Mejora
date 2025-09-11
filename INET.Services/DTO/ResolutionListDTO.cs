using INET.Data;
using System.Collections.Generic;

namespace INET.Services.DTO
{
    public class ResolutionListDTO
    {
        public ResolutionListDTO()
        {
            Filters = new FiltersDTO();
        }

        public FiltersDTO Filters { get; set; }
        public List<Resolution> Resolutions { get; set; }
        public ResolutionDTO ResolutionDTO { get; set; }
    }
}
