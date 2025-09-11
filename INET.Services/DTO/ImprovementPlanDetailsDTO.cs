using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using INET.Data;

namespace INET.Services.DTO
{
    public class ImprovementPlanDetailsDTO
    {
        public ImprovementPlanDetailsDTO()
        {
            this.Institution = new InstitutionDTO();
            this.SolicitudeDTO = new SolicitudeDTO();
            this.IncidenceDTO = new IncidenceDTO();
            this.DocumentDTO = new DocumentDTO();
            this.Incidences = new List<Incidence>();
            this.Solicitudes = new List<Solicitude>();
            this.ImprovementPlanComments = new List<Comment>();
            this.Documents = new List<Document>();
            this.Lines_22 = new List<Lines_22>();
            this.RelatedLines_Lines_22 = new List<Lines_22>();
            this.AccountRenderings = new List<AccountRenderingViewDTO>();
            this.AccountRenderingDTO = new AccountRenderingDTO();
        }

        public int Id { get; set; }
        public int? InstitutionLevelInt { get; set; }
        public string AttachmentURL { get; set; }

        public string AttachmentAddURL { get; set; }
        public string DocumentationURL { get; set; }
        public int ProvinceId { get; set; }
        public int SchoolYearId { get; set; }
        public int StatusId { get; set; }
        public int FieldId { get; set; }
        public int EvaluatorId { get; set; }
        public string Identifier { get; set; }
        public string ImprovementPlanType { get; set; }

        public string Articulator { get; set; }

        public string ArticulatorExp { get; set; }
        public string CUE { get; set; }
        public UserProfile Evaluator { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string SchoolYear { get; set; }

        public string Pronafe { get; set; }
        public string Field { get; set; }

        public string SubField { get; set; }

        public string Line { get; set; }

        public int? LineId { get; set; }
        public List<Lines_22> Lines_22 {get;set;}
        public string Line22 { get; set; }


        public int? Line22Id { get; set; }

        public string Line22_Xls_Sheet { get; set; }

        public string Xls_Col { get; set; }
        public string User { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
        public string ParentIdentifier { get; set; }        
        public int? ParentId { get; set; }
        public InstitutionDTO Institution { get; set; }
        public SolicitudeDTO SolicitudeDTO { get; set; }
        public IncidenceDTO IncidenceDTO { get; set; }
        public DocumentDTO DocumentDTO { get; set; }
        public IList<Incidence> Incidences { get; set; }        
        public List<Solicitude> Solicitudes { get; set; }
        public List<Comment> ImprovementPlanComments { get; set; }
        public List<Field> UserFields { get; set; }
        public IList<UserProfile> Evaluators { get; set; }
        public IList<Status> AvailableStatus { get; set; }
        public List<Document> Documents { get; set; }

        public int RelatedLineId { get; set; }
        public List<RelatedLines_22> RelatedLines { get; set; }

        public List<Lines_22> RelatedLines_Lines_22 { get; set; }

       public List<AccountRenderingViewDTO> AccountRenderings { get; set; }

        public AccountRenderingDTO AccountRenderingDTO { get; set; }
    }
}
