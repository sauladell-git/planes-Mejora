using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class DictumListDTO
    {
        public DictumListDTO()
        {
            Filters = new FiltersDTO();
        }

        public FiltersDTO Filters { get; set; }
        public List<Dictum> Dictums { get; set; }
        public DictumDTO DictumDTO { get; set; }
    }
}
