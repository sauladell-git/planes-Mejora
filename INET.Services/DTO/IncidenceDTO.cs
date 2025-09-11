using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class IncidenceDTO
    {
        public int? SolicitudeId { get; set; }
        
        public IList<KeyValuePair<int, string>> Status { get; set; }

        [Required]
        public int IncidenceTypeId { get; set; }
        public IList<IncidenceType> IncidencesTypes { get; set; }

        [Required]
        public string Details { get; set; }

        [Required]
        public int ImprovementPlanId { get; set; }

        public int Id { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string StatusDescription { get; set; }
        public string UserFullName { get; set; }
        public string Subject { get; set; }
        public string DateString { get; set; }
        public bool IsSolicitudeIncidence { get; set; }
        public List<CommentDTO> Comments { get; set; }
    }
}
