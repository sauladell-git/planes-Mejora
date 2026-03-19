using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class ImprovementPlanReportDTO
    {
        public ImprovementPlanReportDTO()
        {
        }
        public string Cycle     { get; set; }
        public string Provincia { get; set; }
        public DateTime? FechaDeIngreso { get; set; }
        public string TipoDePlan { get; set; }
        public string Axis { get; set; }
        public string Lines { get; set; }
        public string Summary { get; set; }
        public string CUE { get; set; }
        public string InstitutionLevel { get; set; }
        public string Institucion { get; set; }
        public string Departamento { get; set; }
        public string Localidad { get; set; }
        public string CodPlan { get; set; }
        public string Estado { get; set; }
        public decimal? TotalSolicitado { get; set; }
        public decimal? TotalSolicitadoCapital { get; set; }
        public decimal? TotalSolicitadoCorriente { get; set; }
        public string Expedientes { get; set; }
        public DateTime? FechaIngresoCampo { get; set; }
        public string evaluatorName { get; set; }
        public string FechaDictamenes { get; set; }
        public string NroDictamenes { get; set; }
        public decimal? totalDictaminado { get; set; }
        public decimal? totalDictaminadoCapital { get; set; }
        public decimal? totalDictaminadoCorriente { get; set; }
        public decimal? totalAprobado { get; set; }

        public decimal? totalAprobadoCapital { get; set; }
        public decimal? totalAprobadoCorriente { get; set; }

        public decimal? totalDesestimado { get; set; }
        public decimal? totalAnulado { get; set; }
        public decimal? totalRechazado { get; set; }
        public decimal? totalElegible { get; set; }
        public string FechaResoluciones { get; set; }
        public string FechaAnexoResoluciones { get; set; }
        public string Resoluciones { get; set; }
        public decimal? totalSinDictamen { get; set; }
        public decimal? totalSinResolucion { get; set; }
        public int Id { get; set; }

        public string Articulador { get; set; }

        public decimal? totalSolicitadoRRHH { get; set; }
        public decimal? totalSolicitadoBienesServicios { get; set; }

        public decimal? totalSolicitadoViaticos { get; set; }

        public decimal? totalAprobadoRRHH { get; set; }
        public decimal? totalAprobadoBienesServicios { get; set; }

        public decimal? totalAprobadoViaticos { get; set; }

        public decimal? totalRendidoBys { get; set; }

        public decimal? totalRendidoPyv { get; set; }

        public decimal? totalRendidoRrhh { get; set; }

        public decimal? totalRendido { get; set; }

        public string ultimaFechaRendido { get; set; } 
    }
}
