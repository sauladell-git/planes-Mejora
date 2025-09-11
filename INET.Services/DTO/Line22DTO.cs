using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class Line22DTO
    {
        public Line22DTO()
        {
            Fields = new List<Field>();
            Lines  = new List<Line>();
            SubFields = new List<SubField>();
        }
        public int Id { get; set; }

        public int FieldId { get; set; }

        public int SubFieldId { get; set; }

        public int LineId { get; set; }
        public IList<Field> Fields { get; set; }
        public IList<Line>      Lines { get; set; }
        public IList<SubField> SubFields   { get; set; }
    }

    public class Line22DTOjson 
    {// mismo DTO sanitizado para json (evita referencias circulares)
        public int Id { get; set; }

        public int FieldId { get; set; }

        public int SubFieldId { get; set; }

        public int LineId { get; set; }
       public  List<KeyValuePair<int, string>> Fields { get; set; }
       public  List<KeyValuePair<int, string>> Lines { get; set; }
       public   List<KeyValuePair<int, string>> SubFields { get; set; }

        public Line22DTOjson(Line22DTO dto)
        {
             Fields = new List<KeyValuePair<int, string>>();
             Lines = new List<KeyValuePair<int, string>>();
             SubFields = new List<KeyValuePair<int, string>>();

            Id = dto.Id;
            FieldId = dto.FieldId;
            LineId = dto.LineId;
            SubFieldId = dto.SubFieldId;

            foreach (Field l in dto.Fields)
            {
                Fields.Add(new KeyValuePair<int, string>(l.Id, l.Description));

            }

            foreach (Line l in dto.Lines)
            {
                Lines.Add(new KeyValuePair<int, string>(l.Id, l.Description));

            }

            foreach (SubField l in dto.SubFields)
            {
                SubFields.Add(new KeyValuePair<int, string>(l.Id, l.Description));

            }


        }
      
    }
}
