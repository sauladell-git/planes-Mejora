using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class TreeDTO
    {
        public TreeDTO()
        {
            this.children = new List<TreeDTO>();
        }

        public TreeDTO(int id, string label)
        {
            this.id = id;
            this.label = label;
            this.children = new List<TreeDTO>();
        }

        public TreeDTO(int id, string label, decimal? ammount)
        {
            this.id = id;
            this.label = label;
            this.ammount = ammount;
            this.children = new List<TreeDTO>();        
        }

        public int id { get; set; }        
        public string label { get; set; }
        public decimal? ammount { get; set; }
        public IList<TreeDTO> children { get; set; }
    }
}
