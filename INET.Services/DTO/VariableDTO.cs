using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class DocumentVariableDTO
    {
        public int TemplateVariableId { get; set; }
        public string VariableText { get; set; }
    }

    public class TemplateVariableDTO
    { 
        public int TemplateVariableId { get; set; }
        public string VariableName { get; set; }
    }
}
