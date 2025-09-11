using INET.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace INET.Services.DTO
{
    public class TemplateDTO
    {
        public TemplateDTO()
        {
            Variables = new List<TemplateVariableDTO>();
            TemplateTypes = new List<TemplateType>();
            TemplateTypeFields = new List<TemplateTypeField>();
            TemplateVariables = new List<TemplateVariable>();
            Fields = new List<Field>();
            SelectedFieldsIds = new List<int>();
        }

        public int Id { get; set; }
        
        [Required]
        public int TemplateTypeId { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public double? marginTop { get; set; }

        [Required]
        public double? marginBottom { get; set; }

        [Required]
        public double? marginLeft { get; set; }

        [Required]
        public double? marginRight { get; set; }

        [Required]
        [AllowHtml]        
        public string Content { get; set; }

        [AllowHtml]
        public string Header { get; set; }

        public string SignatureImagePath { get; set; }

        public bool CanDeleteTemplate { get; set; }

        public string FileManagerUrl { get; set; }

        [Required]
        public IList<int> SelectedFieldsIds { get; set; }

        public List<Field> Fields { get; set; }
        public List<TemplateVariableDTO> Variables { get; set; }
        public IList<TemplateType> TemplateTypes { get; set; }
        public IList<TemplateTypeField> TemplateTypeFields { get; set; }
        public IList<TemplateVariable> TemplateVariables { get; set; }
    }
}
