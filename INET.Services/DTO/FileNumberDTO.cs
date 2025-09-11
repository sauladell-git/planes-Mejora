using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class Test
    {
        public List<FileNumberDTO> List { get; set; }
    }

    public class FileNumberDTO
    {
        public int LineId { get; set; }
        public string FileNumber { get; set; }
    }
}
