using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class CueSolicitudesReportDTO
    {
        public CueSolicitudesReportDTO()
        {
        }

        public string Provincia { get; set; }
        public DateTime? FechaDeIngreso { get; set; }
        public string TipoDePlan { get; set; }
        public string CUE { get; set; }

        public string CicloLectivo { get; set; }

        public string Axis { get; set; }
        public string Lines { get; set; }
        public string Summary { get; set; }
        public string Level { get; set; }
        public string Institucion { get; set; }
        public string Departamento { get; set; }
        public string Localidad { get; set; }
        public string CodPlan { get; set; }
        public string Estado { get; set; }
        public string Details { get; set; }
        public decimal? TotalSolicitado { get; set; }


        public decimal? totalSolicitado_Bienes_Servicios { get; set; }

        public decimal? totalSolicitado_Viaticos { get; set; }

        public decimal? totalSolicitado_RRHH { get; set; }
        public string Expedientes { get; set; }
        public DateTime? FechaIngresoCampo { get; set; }
        public string FechaDictamenes { get; set; }
        public string NroDictamenes { get; set; }

        public string TipoGasto { get; set; }

        public decimal? totalDictaminado { get; set; }
        public decimal? totalAprobado { get; set; }

        public decimal? totalAprobado_Bienes_Servicios { get; set; }

        public decimal? totalAprobado_Viaticos { get; set; }

        public decimal? totalAprobado_RRHH { get; set; }
        public decimal? totalAprobadoCapital { get; set; }

        public decimal? totalAprobadoCorriente { get; set; }

        //public decimal? totalDesestimado { get; set; }
        public decimal? totalAnulado { get; set; }
        public decimal? totalRechazado { get; set; }
        public decimal? totalElegible { get; set; }
        public string Resoluciones { get; set; }
        public decimal? totalSinDictamen { get; set; }
        public decimal? totalSinResolucion { get; set; }
        public int Id { get; set; }

        public string     evaluatorName { get; set; }

        public string FechaResoluciones { get; set; }

        public string FechaAnexoResoluciones { get; set; }

        public int IdPlan { get; set; }

        public string Nombre { get; set; }

        public string Gestion { get; set; }

        public string Modalidad { get; set; }

        public string Clase { get; set; }
    }
}
