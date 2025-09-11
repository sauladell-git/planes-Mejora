using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class FiltersDTO
    {
        public FiltersDTO()
        {
            this.SchoolYears = new List<SchoolYear>();
            this.Evaluators = new List<UserProfile>();
            this.ImprovementPlansTypes = new List<ImprovementPlansType>();
            this.Fields = new List<Field>();
            this.Status = new List<Status>();
            this.Days = new List<KeyValuePair<string,string>>();
            this.Months = new List<KeyValuePair<string, string>>();
            this.Years = new List<KeyValuePair<string, string>>();
        }

        public string Identifier { get; set; }
        public string Summary { get; set; }
        public string Number { get; set; }
        public string CUE { get; set; }
        public int? ProvinceId { get; set; }
        public int? SchoolYearId { get; set; }
        public int? EvaluatorId { get; set; }
        public int? ImprovementPlanTypeId { get; set; }
        public int? FieldId { get; set; }
        public int? StatusId { get; set; }
        public string FileNumber { get; set; }
        public string ReceptionDateDay { get; set; }
        public string ReceptionDateMonth { get; set; }
        public string ReceptionDateYear { get; set; }

        public string Dictum { get; set; }
        public string Resolution { get; set; }
        public int? DictumId { get; set; }
        public int? ResolutionId { get; set; }
        public string ImprovementPlanIdentifier { get; set; }

        public string Articulator { get; set; }

        public string ArticulatorExp { get; set; }
        public DateTime? ReceptionDate
        {
            get 
            {
                DateTime receptionDate;
                if (!string.IsNullOrEmpty(ReceptionDateDay) && !string.IsNullOrEmpty(ReceptionDateMonth) && !string.IsNullOrEmpty(ReceptionDateYear))
                {
                    DateTime.TryParseExact(String.Format("{0}-{1}-{2}", ReceptionDateYear, ReceptionDateMonth, ReceptionDateDay), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out receptionDate);
                    if (receptionDate != DateTime.MinValue)
                        return receptionDate;
                }

                return null;
            }
        }

        public IList<SchoolYear> SchoolYears { get; set; }
        public IList<UserProfile> Evaluators { get; set; }
        public IList<ImprovementPlansType> ImprovementPlansTypes { get; set; }
        public IList<Field> Fields { get; set; }
        public IList<Status> Status { get; set; }

        public List<KeyValuePair<string, string>> Days { get; set; }
        public List<KeyValuePair<string, string>> Months { get; set; }
        public List<KeyValuePair<string, string>> Years { get; set; }
    }
}
