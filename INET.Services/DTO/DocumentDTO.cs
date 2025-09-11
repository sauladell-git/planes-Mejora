using INET.Core.Enums;
using INET.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public abstract class BaseDocumentDTO
    {
        public int Id { get; set; }
        public int ImprovementPlanId { get; set; }
        public string Number { get; set; }
        public string Body { get; set; }        

        [Required]
        public int StatusId { get; set; }

        public int OriginalStatusId { get; set; }

        public IList<Status> Status { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public int TemplateTypeId { get; set; }

        public List<Template> Templates { get; set; }
        public List<TemplateType> TemplatesTypes { get; set; }
    }

    public class DocumentDTO
    {
        public DocumentDTO()
        {
            this.DictumDTO = new DictumDTO();
            this.ResolutionDTO = new ResolutionDTO();
        }

        public DictumDTO DictumDTO { get; set; }
        public ResolutionDTO ResolutionDTO { get; set; }
    }

    public class DictumDTO : BaseDocumentDTO
    {
        public DictumDTO()
        {
            this.SolicitudesIds = new List<int>();
            this.Templates = new List<Template>();
            this.TemplatesTypes = new List<TemplateType>();
            this.FileNumbers = new List<KeyValuePair<string, string>>();            
        }
        
        public string FileNumber { get; set; }
        public decimal Ammount { get; set; }
        public decimal Balance { get; set; }  
        public string ExternalNumber { get; set; }        
        public string ExternalEntity { get; set; }
        public string SignDate { get; set; }

        public string SignedDocument { get; set; }

        public string AnnexDocument { get; set; }
        public List<int> SolicitudesIds { get; set; }        
        public List<KeyValuePair<string, string>> FileNumbers { get; set; }        
    }

    public class ResolutionDTO : BaseDocumentDTO
    {
        public ResolutionDTO()
        {
            this.DictumsIds = new List<int>();
            this.Templates = new List<Template>();
            this.TemplatesTypes = new List<TemplateType>();
            this.FileNumbers = new List<KeyValuePair<string, string>>();
            this.Variables = new List<DocumentVariableDTO>();
        }

        [Required]
        public string FileNumber { get; set; }
        public string ResolutionNumber { get; set; }
        public string ShipDate { get; set; }
        public string SignatureDate { get; set; }
        public string AnnexSignatureDate { get; set; }

        public decimal AmmountExecuted { get; set; }
        public List<int> DictumsIds { get; set; }
        public List<KeyValuePair<string, string>> FileNumbers { get; set; }
        public List<DocumentVariableDTO> Variables { get; set; }
    }
}