using INET.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    /// <summary>
    /// Clase que encapsula los parámetros del plugin DataTable
    /// </summary>
    public class DataTableDTO
    {
        /// <summary>
        /// Número de secuencia enviado en cada Request por el DataTable, el mismo valor debe devolverse en el Response
        /// </summary>       
        public string sEcho { get; set; }

        /// <summary>
        /// Texto usado para filtrar    
        /// </summary>
        public string sSearch { get; set; }

        /// <summary>
        /// Número de registros que deberían mostrarse en la tabla
        /// </summary>
        public int iDisplayLength { get; set; }

        /// <summary>
        ///  Primer registro que debería mostrarse (usado para paginar)
        /// </summary>
        public int iDisplayStart { get; set; }

        /// <summary>
        /// Número de columnas en la tabla
        /// </summary>
        public int iColumns { get; set; }

        /// <summary>
        /// Número de columnas que se utilizan para ordenar
        /// </summary>
        public int iSortingCols { get; set; }

        /// <summary>
        /// Lista del nombre de las columnas separadas por comas
        /// </summary>
        public string sColumns { get; set; }

        /// <summary>
        /// Indica el orden en el que se desea ordenar. Puede ser "asc" o "desc"
        /// </summary>
        public string sSortDir_0 { get; set; }

        /// <summary>
        /// Indica el índice de la columna por la cual se desea ordenar
        /// </summary>
        public int iSortCol_0 { get; set; }
        
        #region Custom Search

        /// <summary>
        /// Identificador del plan
        /// </summary>
        public string sCustomSearch_Identifier { get; set; }

        /// <summary>
        /// Texto usado para filtrar    
        /// </summary>
        public string sCustomSearch_Summary { get; set; }

        /// <summary>
        /// Nro. Expediente de los solicitados
        /// </summary>
        public string sCustomSearch_FileNumber { get; set; }

        /// <summary>
        /// Nro. dictamen
        /// </summary>
        public string sCustomSearch_Dictum { get; set; }

        /// <summary>
        /// ID dictamen
        /// </summary>
        public string sCustomSearch_DictumId { get; set; }

        /// <summary>
        /// Nro. resolucion
        /// </summary>
        public string sCustomSearch_Resolution { get; set; }

        /// <summary>
        /// ID resolucion
        /// </summary>
        public string sCustomSearch_ResolutionId { get; set; }

        /// <summary>
        /// ID provincia
        /// </summary>
        public int? iCustomSearch_ProvinceId { get; set; }

        /// <summary>
        /// Numeros de provincias
        /// </summary>
        public List<String> provNumbers { get; set; }
        
        /// <summary>
        /// CUE del plan
        /// </summary>
        public string sCustomSearch_CUE { get; set; }        

        /// <summary>
        /// Ciclo Lectivo del plan
        /// </summary>
        public int? iCustomSearch_SchoolYearId { get; set; }

        /// <summary>
        /// Evaluador del plan
        /// </summary>
        public int? iCustomSearch_EvaluatorId { get; set; }

        /// <summary>
        /// Tipo de plan
        /// </summary>        
        public int? iCustomSearch_ImprovementPlanId { get; set; }

        public string sCustomSearch_Articulator { get; set; }

        public string sCustomSearch_ArticulatorExp { get; set; }
        /// <summary>
        /// Tipo de plan
        /// </summary>        
        public int? iCustomSearch_ImprovementPlanTypeId { get; set; }
        
        /// <summary>
        /// Fecha de emisión
        /// </summary>
        public DateTime? dCustomSearch_ReceptionDate { get; set; }

        /// <summary>
        /// Id del eje
        /// </summary>
        public int? iCustomSearch_FieldId { get; set; }

        /// <summary>
        /// ID de ejes
        /// </summary>
        public List<int> fieldsIds { get; set; }

        /// <summary>
        /// ID de ejes
        /// </summary>
        public List<int> levelsIds { get; set; }

        /// <summary>
        /// Id del estado
        /// </summary>
        public int? iCustomSearch_StatusId { get; set; }

        /// <summary>
        /// ID de estados
        /// </summary>
        public List<int> statusIds { get; set; }

        /// <summary>
        /// Contructor
        /// </summary>
        public DataTableDTO() {
            provNumbers = new List<String>();
            fieldsIds = new List<int>();
            levelsIds = new List<int>();
            statusIds = new List<int>();
        }

        /// <summary>
        /// Determina si se deben filtrar o no los resultados
        /// </summary>
        public bool FilterResults
        {
            get 
            {
                return !string.IsNullOrWhiteSpace(sCustomSearch_Identifier)
                        || !string.IsNullOrWhiteSpace(sCustomSearch_Summary)
                        || !string.IsNullOrWhiteSpace(sCustomSearch_FileNumber)
                        || !string.IsNullOrWhiteSpace(sCustomSearch_Dictum)
                        || !string.IsNullOrWhiteSpace(sCustomSearch_DictumId)
                        || !string.IsNullOrWhiteSpace(sCustomSearch_Resolution)
                        || !string.IsNullOrWhiteSpace(sCustomSearch_ResolutionId)
                        || !string.IsNullOrWhiteSpace(sCustomSearch_CUE)
                        || !string.IsNullOrWhiteSpace(sCustomSearch_Articulator)
                        || iCustomSearch_SchoolYearId.HasValue
                        || iCustomSearch_EvaluatorId.HasValue
                        || iCustomSearch_ImprovementPlanTypeId.HasValue
                        || dCustomSearch_ReceptionDate.HasValue
                        || iCustomSearch_FieldId.HasValue
                        || iCustomSearch_StatusId.HasValue
                        || iCustomSearch_ProvinceId.HasValue
                        || provNumbers.Count > 0
                        || fieldsIds.Count > 0
                        || levelsIds.Count > 0
                        || statusIds.Count > 0;
            }
        }

        #endregion
    }

    /// <summary>
    /// Dummy para listados de planes
    /// </summary>
    public class ImprovementPlanListItem
    {
        public ImprovementPlan Plan { get; set; }
        public string ImprovementPlansTypeDescription { get; set; }
        public string SchoolYearDescription { get; set; }
        public string FieldCode { get; set; }
        public string FieldDescription { get; set; }
        public string UserName { get; set; }
        public string StatusDescription { get; set; }
        public IQueryable FileNumbers { get; set; }
        public int CommentsCount { get; set; }
        public int IncidencesCount { get; set; }
        public int SolicitudesCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    /// <summary>
    /// Clase que encapsula el resultado de una consulta ServerSide del JQuery DataTable
    /// </summary>
    public class ImprovementPlanResultDTO
    {
        /// <summary>
        /// Número de secuencia enviado en cada Request por el DataTable, el mismo valor debe devolverse en el Response
        /// </summary>
        public string Echo { get; set; }

        /// <summary>
        /// Total de registros que devuelve la consulta sin ningún filtro
        /// </summary>
        public int TotalRecords { get; set; }
        
        /// <summary>
        /// Total de registros con algún filtro aplicado
        /// </summary>
        public int TotalDisplayRecords { get; set; }

        /// <summary>
        /// Lista de planes a mostrar
        /// </summary>
        public IList<ImprovementPlanListItem> FilteredImprovementPlans { get; set; }
    }

    /// <summary>
    /// Clase que encapsula el resultado de una consulta ServerSide del JQuery DataTable
    /// </summary>
    public class SolicitudeResultDTO
    {
        /// <summary>
        /// Número de secuencia enviado en cada Request por el DataTable, el mismo valor debe devolverse en el Response
        /// </summary>
        public string Echo { get; set; }

        /// <summary>
        /// Total de registros que devuelve la consulta sin ningún filtro
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Total de registros con algún filtro aplicado
        /// </summary>
        public int TotalDisplayRecords { get; set; }

        /// <summary>
        /// Lista de planes a mostrar
        /// </summary>
        public IList<Solicitude> FilteredSolicitudes { get; set; }
    }

    /// <summary>
    /// Dummy para listados de dictamenes
    /// </summary>
    public class DictumListItem
    {
        public Dictum Dictum { get; set; }
        public string ImprovementPlanIdentifier { set; get; }
        public string UserName { set; get; }
        public string StatusDescription { set; get; }
        public ICollection<Resolution> Resolutions { get; set; }
    }

    /// <summary>
    /// Clase que encapsula el resultado de una consulta ServerSide del JQuery DataTable
    /// </summary>
    public class DictumResultDTO
    {
        /// <summary>
        /// Número de secuencia enviado en cada Request por el DataTable, el mismo valor debe devolverse en el Response
        /// </summary>
        public string Echo { get; set; }

        /// <summary>
        /// Total de registros que devuelve la consulta sin ningún filtro
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Total de registros con algún filtro aplicado
        /// </summary>
        public int TotalDisplayRecords { get; set; }

        /// <summary>
        /// Lista de planes a mostrar
        /// </summary>
        public IList<DictumListItem> FilteredDictums { get; set; }
    }

    /// <summary>
    /// Dummy para listados de resoluciones
    /// </summary>
    public class ResolutionListItem
    {
        public Resolution Resolution{ get; set; }
        public List<String> ImprovementPlanIdentifiers { set; get; }
        public string UserName { set; get; }
        public string StatusDescription { set; get; }
        public int FieldId { set; get; }
        public string FieldDescription { set; get; }
    }

    /// <summary>
    /// Clase que encapsula el resultado de una consulta ServerSide del JQuery DataTable
    /// </summary>
    public class ResolutionResultDTO
    {
        /// <summary>
        /// Número de secuencia enviado en cada Request por el DataTable, el mismo valor debe devolverse en el Response
        /// </summary>
        public string Echo { get; set; }

        /// <summary>
        /// Total de registros que devuelve la consulta sin ningún filtro
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Total de registros con algún filtro aplicado
        /// </summary>
        public int TotalDisplayRecords { get; set; }

        /// <summary>
        /// Lista de planes a mostrar
        /// </summary>
        public IList<ResolutionListItem> FilteredResolutions { get; set; }
    }

}