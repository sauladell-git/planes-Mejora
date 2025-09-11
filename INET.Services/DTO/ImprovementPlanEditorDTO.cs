using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using INET.Data;
using System.Web;

namespace INET.Services.DTO
{
    public class ImprovementPlanEditorDTO
    {

        public ImprovementPlanEditorDTO()
        {
            this.Days = new List<KeyValuePair<string, string>>();
            this.Months = new List<KeyValuePair<string, string>>();
            this.Years = new List<KeyValuePair<string, string>>();
            // SIA 09-11-21 default del dia 
            this.ReceptionDateDay = DateTime.Now.Day.ToString("00");
            this.ReceptionDateMonth = DateTime.Now.Month.ToString("00");
            this.ReceptionDateYear = DateTime.Now.Year.ToString();
            Lines = new List<Line>();
            Lines22 = new List<Lines_22>();
            Lines22_ArticulatorLine =new List<Lines_22>();
            SubFields = new List<SubField>();
            //SIA 18-02-2023 pronafe false defecto!
            Pronafe = false;
            AccountRenderings = new List<AccountRendering>();

        }


        

        public int Id { get; set; }        
        public string Identifier { get; set; }
        public string AttachmentURL { get; set; }
        public string DocumentationURL { get; set; }

        public string AttachmentAddURL { get; set; }

        [Required]        
        public string CUE { get; set; }

        [Required]
        public string Summary { get; set; }        

        [Required]
        [DataType(DataType.Text)]
        public string ReceptionDateDay { get; set; }        

        [Required]
        public string ReceptionDateMonth { get; set; }
        
        [Required]
        public string ReceptionDateYear { get; set; }

        public List<KeyValuePair<string, string>> Days { get; set; }
        public List<KeyValuePair<string, string>> Months { get; set; }
        public List<KeyValuePair<string, string>> Years { get; set; }

        [Required(ErrorMessage="El campo {0} no es una fecha válida")]
        public DateTime? ReceptionDate
        {
            get 
            { 
                DateTime receptionDate;
                DateTime.TryParseExact(String.Format("{0}-{1}-{2}", ReceptionDateYear, ReceptionDateMonth, ReceptionDateDay), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out receptionDate);
                if (receptionDate != DateTime.MinValue)
                    return receptionDate;

                return null;
            }
        }

        [Required]        
        public int SchoolYearId { get; set; }
        public IList<SchoolYear> SchoolYears { get; set; }

        [Required]        
        public int ImprovementPlanTypeId { get; set; }
        public IList<ImprovementPlansType> ImprovementPlansTypes { get; set; }

        [Required]
        public int FieldId { get; set; }
        public IList<Field> Fields { get; set; }

        public int LineId { get; set; }


        public IList<Line> Lines { get; set; }

        public int? SubField      { get; set; }
        
        public IList<SubField> SubFields { get; set; }


        public int? Line22Id { get; set; }


        public IList<Lines_22> Lines22 { get; set; }

        public int StatusId { get; set; }        



        public int? ParentId { get; set; }

        public string Articulator { get; set; }

        public string ArticulatorExp { get; set; }

        public bool Pronafe { get; set; }
        public HttpPostedFileBase SolicitudesImportFile { get; set; }

        public int? ArticulatorLineId { get; set; }

        public IList<Lines_22> Lines22_ArticulatorLine { get; set; }
        public int? ArticulatorImprovementPlanId { get; set; }

        public IList<AccountRendering> AccountRenderings { get; set; }
    }
}
