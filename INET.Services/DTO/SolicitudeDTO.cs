using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using INET.Data;

namespace INET.Services.DTO
{
    public class SolicitudeDTO
    {
        public SolicitudeDTO()
        {
            this.Status = new List<Status>();
            this.SolicitudesTypes = new List<SolicitudeType>();
            this.MeasurementUnits = new List<MeasurementUnit>();
            this.ExpedureTypes = new List<ExpenditureType>();
            this.ExpedureObjectTypes = new List<ExpenditureObjectType>();
            this.Specializations = new List<KeyValuePair<string, string>>();
            this.Lines_22 = new List<Lines_22>();
        }

        [Required]
        public string CUE { get; set; }

        public string Management { get; set; }
        [Required]
        public string Details { get; set; }

        public int? ReassignedId { get; set; }

        public string Reassigned { get; set; }
        
        [Required]
        public decimal? RequestedAmount { get; set; }
        
        [Required]
        public decimal? RequestedPriceUnit { get; set; }
        
        public decimal? RequestedTotal
        {
            get
            {
                if (RequestedAmount.HasValue && RequestedPriceUnit.HasValue)
                    return Math.Round(RequestedAmount.Value * RequestedPriceUnit.Value, 2);
                
                return null;
            }
        }
        
        public decimal? ApprovedAmount { get; set; }
        
        public decimal? ApprovedPriceUnit { get; set; }
        
        public decimal? ApprovedTotal
        {
            get
            {
                if (ApprovedAmount.HasValue && ApprovedPriceUnit.HasValue)
                    return Math.Round(ApprovedAmount.Value * ApprovedPriceUnit.Value, 2);

                return null;
            }
        }
        
        public int SolicitudeId { get; set; }
        
        public int ImprovementPlanId { get; set; }

        public string FileNumber { get; set; }

        [Required]
        public int SchoolYearId { get; set; }
        public IList<SchoolYear> SchoolYears { get; set; }

        [Required]
        public int LineId { get; set; }
        public IList<Line> Lines { get; set; }

        public int FieldId { get; set; }

        public int SubFieldId { get; set; }

        public int Line_22_Id { get; set; }
        public IList<Lines_22> Lines_22 { get; set; }

        public int? SolicitudeTypeId { get; set; }
        public IList<SolicitudeType> SolicitudesTypes { get; set; }

        public int MeasurementUnitId { get; set; }       
        public IList<MeasurementUnit> MeasurementUnits { get; set; }

        public int ExpenditureTypeId { get; set; }        
        public IList<ExpenditureType> ExpedureTypes { get; set; }

        public int ExpenditureObjectTypeId { get; set; }
        public IList<ExpenditureObjectType> ExpedureObjectTypes { get; set; }

        public List<KeyValuePair<string, string>> Specializations { get; set; }

        [Required]
        public int StatusId { get; set; }
        public IList<Status> Status { get; set; }

        public string SchoolYear { get; set; }
        public string Line { get; set; }
        public string ExpenditureType { get; set; }
        public string Specialization { get; set; }
        public string SolicitudeType { get; set; }
        public string StatusDescription { get; set; }
        public string MeasurementUnit { get; set; }
    }
}
