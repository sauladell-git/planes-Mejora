using INET.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INET.Services.DTO
{
    public class AccountRenderingDTO
    {
        INET.Services.BusinessService bs = new INET.Services.BusinessService();
        public int? id { get; set; }
        public int improvementPlanId { get; set; }

        public int dictumId { get; set; }

        public int expenditureObjectTypeId { get; set; }

        public decimal aprovedAmount { get; set; }

        public decimal rejectedAmount { get; set; }

        public string comment { get; set; }
        public string  number { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime date { get; set; }

        public IList<ExpenditureObjectType> ExpedureObjectTypes { get; set; }
        public IList<Dictum> Dictums { get; set; }


        [Required]
        [DataType(DataType.Text)]
        public string DateDay { get; set; }

        [Required]
        public string DateMonth { get; set; }

        [Required]
        public string DateYear { get; set; }

        public List<KeyValuePair<string, string>> Days { get; set; }
        public List<KeyValuePair<string, string>> Months { get; set; }
        public List<KeyValuePair<string, string>> Years { get; set; }

        [Required(ErrorMessage = "El campo {0} no es una fecha válida")]
        public DateTime? Date
        {
            get
            {
                DateTime receptionDate;
                DateTime.TryParseExact(String.Format("{0}-{1}-{2}", DateYear, DateMonth, DateDay), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out receptionDate);
                if (receptionDate != DateTime.MinValue)
                    return receptionDate;

                return null;
            }
        }
        public AccountRenderingDTO()
        {
            this.Days = new List<KeyValuePair<string, string>>();
            this.Months = new List<KeyValuePair<string, string>>();
            this.Years = new List<KeyValuePair<string, string>>();
            // SIA 09-11-21 default del dia 
            this.DateDay = DateTime.Now.Day.ToString("00");
            this.DateMonth = DateTime.Now.Month.ToString("00");
            this.DateYear = DateTime.Now.Year.ToString();

            // ReceptionDate Combos
            this.Days.AddRange(bs.ListDays());
            this.Months.AddRange(bs.ListMonths());
            rejectedAmount = 0;
            comment = "";
            

        }
    }

    
}
