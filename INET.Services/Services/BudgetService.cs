using ClosedXML.Excel;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using INET.Core.Enums;
using INET.Data;
using INET.Services.DTO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Caching;

namespace INET.Services
{
    /// <summary>
    /// Servicio encargado de gestionar el presupuesto
    /// </summary>
    public class BudgetService : BusinessService
    {
        private readonly SolicitudeService _solicitudeService;

        public BudgetService(INETContext context, SolicitudeService solicitudeService)
        {
            Context = context;
            Context.Database.CommandTimeout = 120;
            _solicitudeService = solicitudeService;
        }

        /// <summary>
        /// Lista el presupuesto correspondiente a un determinado ciclo lectivo
        /// </summary>
        /// <param name="schoolYearId">Ciclo lectivo del que se desea listar el presupuesto</param>
        /// <returns>Un listado de TreeDTO para poder armas el árbol del presupuesto correspondiente a ese ciclo lectivo</returns>
        public IList<TreeDTO> ListBudget(int schoolYearId)
        {
            var lst = new List<TreeDTO>();
            var provinces = Context.Provinces.ToList().OrderBy(x => x.Name);
            var fields = Context.Fields.ToList().OrderBy(x => x.Code);
            foreach (var province in provinces)
            {
                var node = new TreeDTO(0, province.Name);
                foreach (var field in fields)
                {

                    var budgets = Context.Budgets.Where(x => x.SchoolYearId == schoolYearId && x.ProvinceId == province.Id && x.FieldId == field.Id).ToList().OrderBy(x => x.Line.Code);
                    if (budgets.Count() > 0)
                    {
                        var subNode = new TreeDTO(0, field.Code + " - " + field.Description);

                        foreach (var budget in budgets)
                            subNode.children.Add(new TreeDTO(budget.Id, budget.Line.Code + " - " + budget.Line.Description, budget.Ammount));

                        node.children.Add(subNode);
                    }

                }
                lst.Add(node);
            }

            return lst;
        }

        /// <summary>
        /// Lista el presupuesto correspondiente a un determinado ciclo lectivo
        /// </summary>
        /// <param name="schoolYearId">Ciclo lectivo del que se desea listar el presupuesto</param>
        /// <returns>Un listado de TreeDTO para poder armas el árbol del presupuesto correspondiente a ese ciclo lectivo</returns>
        public IList<TreeDTO> ListBudget216(int schoolYearId)
        {
            var lst = new List<TreeDTO>();
            var provinces = Context.Provinces.ToList().OrderBy(x => x.Name);
            var fields = Context.SubFields.ToList().OrderBy(x => x.Code);
            foreach (var province in provinces)
            {
                var node = new TreeDTO(0, province.Name);
                foreach (var field in fields)
                {

                    var budgets = Context.Budgets_216.Where(x => x.SchoolYearId == schoolYearId && x.ProvinceId == province.Id && x.FieldId == field.Id).ToList().OrderBy(x => x.Lines_22.Code);
                    if (budgets.Count() > 0)
                    {
                        var subNode = new TreeDTO(0, field.Code + " - " + field.Description);

                        foreach (var budget in budgets)
                            subNode.children.Add(new TreeDTO(budget.Id, budget.Lines_22.Code + " - " + budget.Lines_22.Description, budget.Ammount));

                        node.children.Add(subNode);
                    }

                }
                lst.Add(node);
            }

            return lst;
        }



        /// <summary>
        /// Guarda un presupuesto
        /// </summary>
        /// <param name="budgetId">Id del presupuesto que se desea guardar</param>
        /// <param name="ammount">Monto asignado al presupuesto</param>
        public void Save(int budgetId, decimal ammount)
        {
            var budget = Context.Budgets.Where(x => x.Id == budgetId).First();
            budget.Ammount = ammount;
            Context.SaveChanges();
        }


        /// <summary>
        /// Guarda un presupuesto
        /// </summary>
        /// <param name="budgetId">Id del presupuesto que se desea guardar</param>
        /// <param name="ammount">Monto asignado al presupuesto</param>
        public void Save216(int budgetId, decimal ammount)
        {
            var budget = Context.Budgets_216.Where(x => x.Id == budgetId).First();
            budget.Ammount = ammount;
            Context.SaveChanges();
        }

        /// <summary>
        /// Toma Presupuesto por ciclo  y provincia 
        /// </summary>
        /// <param name="SchoolYearId">Id del presupuesto que se desea guardar</param>
        /// <param name="ProvinceId">Monto asignado al presupuesto</param>
        public decimal GetBudget(int schoolYearId, int ProvinceId)
        {
            var budget = Context.Budgets.Where(x => (x.SchoolYearId == schoolYearId&& x.ProvinceId==ProvinceId)).Sum(g=>g.Ammount);
            if (budget != null)
                return budget.Value;
            else return 0;
           
        }

        /// <summary>
        /// Exporta a un Excel el presupuesto por una determinada provincia, version original "horizontal"
        /// </summary>
        /// <param name="schoolYearId">Ciclo Lectivo de los planes a incluir</param>
        /// <param name="budgetYearId">Ciclo Lectivo de los montos ejecutados a incluir</param>
        /// <param name="provinces">provincias</param>
        /// <param name="InstitutionLevelId">Nivel de la institución</param>
        /// <param name="dependenceNational">Dependencia nacional</param>
        /// <returns>Un stream correspondiente al archivo excel generado para el reporte</returns>
        public Stream GenerateBudgetByProvinceReport_V1(int schoolYearId, int budgetYearId, List<Province> provinces, int InstitutionLevelId, bool dependenceNational = false)
        {
            // datos generales reporte
            var schoolYear = Context.SchoolYears.Where(x => x.Id == schoolYearId).First();
            string budgetYearName = "";
            if (budgetYearId > 0)
            {
                var budgetYear = Context.SchoolYears.Where(x => x.Id == budgetYearId).First();
                budgetYearName = budgetYear.Cycle;
            }
            else
            {
                budgetYearName = "Todas";
            }

            MemoryStream stream = new MemoryStream();
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = wb.Worksheets.Add("Ejecución Presupuestaria");

                // Titulo planilla
                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(1));
                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(2));
                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(3));
                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(4));

                ws.Cell(1, 2).Value = "CICLO LECTIVO";
                ws.Cell(1, 4).Value = schoolYear.Cycle;
                ws.Cell(2, 2).Value = "PARTIDA";
                ws.Cell(2, 4).Value = budgetYearName;
                ws.Cell(3, 2).Value = "TIPO DE INSTITUCION";
                ws.Cell(3, 4).Value = GetInstitutionLevelName(InstitutionLevelId);

                if (dependenceNational)
                {
                    ws.Cell(4, 2).Value = "DEPENDENCIA";
                    ws.Cell(4, 4).Value = "NACIONAL";
                }

                // Encabezados
                var table_heads = new List<String>();
                var table_heads_groups = new List<String>();
                table_heads.Add("COD.");
                ws.Column(table_heads.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                table_heads.Add("EJE");
                table_heads.Add("COD.");
                ws.Column(table_heads.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                table_heads.Add("LINEA");

                // lineas
                var showOnlyActiveFields = Int16.Parse(schoolYear.Cycle.Trim()) > 2016;
                var lines = from line in Context.Lines
                            join field in Context.Fields on line.FieldId equals field.Id
                            where (showOnlyActiveFields && field.StatusId == (int)AxisStatusEnum.Vigente) || showOnlyActiveFields == false
                            orderby field.Code, field.Id, line.Code, line.Description, line.Id
                            select new
                            {
                                fieldStatus = field.StatusId,
                                fieldCode = field.Code,
                                fieldDescription = field.Description,
                                lineCode = line.Code,
                                lineDescription = line.Description,
                                lineId = line.Id
                            };

                ws.Cell(7, 1).InsertData(lines.Select(x => new { fieldCode = x.fieldCode, fieldDescription = x.fieldDescription, lineCode = x.lineCode, lineDescription = x.lineDescription }).AsEnumerable());

                var lineIds = lines.Select(x => x.lineId).ToList();

                // detalle por lineas reporte
                var color = false;
                var total_cell_offset = 0;
                // encabezado totales
                if (provinces.Count > 1)
                {
                    total_cell_offset = table_heads.Count + 1;
                    color = GenerateBudgetByProvinceReportBlock(ws, table_heads, "TOTAL", color, dependenceNational);
                }

                // columnas totales
                var totalPlannedCols = new List<string>();
                var totalRequestedCols = new List<string>();
                var totalInDictumsCols = new List<string>();
                var totalInResolutionsCols = new List<string>();

                //cada provincia
                foreach (var prov in provinces)
                {
                    // encabezados
                    var prov_cell_offset = table_heads.Count + 1;
                    color = GenerateBudgetByProvinceReportBlock(ws, table_heads, prov.Name.ToUpper(), color, dependenceNational);
                    //seba
                    // data
                    var rows = from line in Context.Lines
                               join field in Context.Fields on line.FieldId equals field.Id
                               where lineIds.Contains(line.Id)
                               orderby field.Code, field.Id, line.Code, line.Description, line.Id
                               select new
                               {
                                   line = line,
                                   totalPlanned = dependenceNational == false ? Context.Budgets.Where(x => x.LineId == line.Id && x.SchoolYearId == schoolYearId && x.ProvinceId == prov.Id).Sum(x => x.Ammount) ?? 0 : 0,
                                   totalRequested = Context.ImprovementPlans.Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                   totalInDictums = Context.ImprovementPlans.Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.StatusId == (int)DictumStatusEnum.Emitido || y.StatusId == (int)DictumStatusEnum.Firmado).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions = Context.ImprovementPlans.Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0
                               };

                    IEnumerable<dynamic> _rows;
                    if (dependenceNational)
                    {
                        _rows = rows.Select(x => new { totalRequested = x.totalRequested, totalInDictums = x.totalInDictums, totalInResolutions = x.totalInResolutions });
                        //_rows = rows.Select(x => new { totalRequested = x.line.Id, totalInDictums = x.totalInDictums, totalInResolutions = x.totalInResolutions });
                    }
                    else
                    {
                        _rows = rows.Select(x => new { totalPlanned = x.totalPlanned, totalRequested = x.totalRequested, totalInDictums = x.totalInDictums, totalInResolutions = x.totalInResolutions });
                        //_rows = rows.Select(x => new { totalPlanned = x.line.Id, totalRequested = x.totalRequested, totalInDictums = x.totalInDictums, totalInResolutions = x.totalInResolutions });
                    }
                    ws.Cell(7, prov_cell_offset).InsertData(
                        _rows
                    );
                    totalPlannedCols.Add(ws.Column(prov_cell_offset).ColumnLetter());
                    totalRequestedCols.Add(ws.Column(prov_cell_offset + 1).ColumnLetter());
                    totalInDictumsCols.Add(ws.Column(prov_cell_offset + 2).ColumnLetter());
                    totalInResolutionsCols.Add(ws.Column(prov_cell_offset + 3).ColumnLetter());
                }

                // data totales
                if (provinces.Count > 1)
                {
                    var lrCount = 7;
                    foreach (var line in lines)
                    {
                        if (dependenceNational == false)
                        {
                            ws.Cell(lrCount, total_cell_offset).FormulaA1 = String.Format("=SUM({0})", totalPlannedCols.Aggregate((a, b) => (!String.IsNullOrEmpty(a) ? a + (a.Contains(",") ? "" : lrCount.ToString()) + "," : "") + b + lrCount.ToString()));
                        }
                        ws.Cell(lrCount, total_cell_offset + 1 - (dependenceNational ? 1 : 0)).FormulaA1 = String.Format("=SUM({0})", totalRequestedCols.Aggregate((a, b) => (!String.IsNullOrEmpty(a) ? a + (a.Contains(",") ? "" : lrCount.ToString()) + "," : "") + b + lrCount.ToString()));
                        ws.Cell(lrCount, total_cell_offset + 2 - (dependenceNational ? 1 : 0)).FormulaA1 = String.Format("=SUM({0})", totalInDictumsCols.Aggregate((a, b) => (!String.IsNullOrEmpty(a) ? a + (a.Contains(",") ? "" : lrCount.ToString()) + "," : "") + b + lrCount.ToString()));
                        ws.Cell(lrCount, total_cell_offset + 3 - (dependenceNational ? 1 : 0)).FormulaA1 = String.Format("=SUM({0})", totalInResolutionsCols.Aggregate((a, b) => (!String.IsNullOrEmpty(a) ? a + (a.Contains(",") ? "" : lrCount.ToString()) + "," : "") + b + lrCount.ToString()));
                        // delete all fields no vigentes with no data
                        var test = ws.Cell(lrCount, total_cell_offset + 1 - (dependenceNational ? 1 : 0)).Value;
                        if (line.fieldStatus != (int)AxisStatusEnum.Vigente
                            && ws.Cell(lrCount, total_cell_offset + 1 - (dependenceNational ? 1 : 0)).Value.ToString() == "0"
                            && ws.Cell(lrCount, total_cell_offset + 2 - (dependenceNational ? 1 : 0)).Value.ToString() == "0"
                            && ws.Cell(lrCount, total_cell_offset + 3 - (dependenceNational ? 1 : 0)).Value.ToString() == "0"
                            )
                        {
                            ws.Row(lrCount).Delete();
                        }
                        else
                        {
                            lrCount++;
                        }
                    }
                }

                ws.Row(6).Style.Font.FontSize = 12;
                ws.Row(6).Height = 22;
                var heads_row = new List<String[]>();
                heads_row.Add(table_heads.ToArray());
                var heads_row_range = ws.Cell(6, 1).InsertData(heads_row);
                heads_row_range.Style.Font.Bold = true;
                heads_row_range.Style.Font.FontColor = XLColor.White;
                heads_row_range.Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;

                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
            }
            return stream;
        }


        /// <summary>
        /// Exporta a un Excel el presupuesto por una determinada provincia, version original "horizontal"
        /// </summary>
        /// <param name="schoolYearId">Ciclo Lectivo de los planes a incluir</param>
        /// <param name="budgetYearId">Ciclo Lectivo de los montos ejecutados a incluir</param>
        /// <param name="provinces">provincias</param>
        /// <param name="InstitutionLevelId">Nivel de la institución</param>
        /// <param name="dependenceNational">Dependencia nacional</param>
        /// <returns>Un stream correspondiente al archivo excel generado para el reporte</returns>
        public Stream GenerateBudgetByProvinceReport_V1_22(int schoolYearId, int budgetYearId, List<Province> provinces, int InstitutionLevelId, bool dependenceNational = false)
        {
            // datos generales reporte
            var schoolYear = Context.SchoolYears.Where(x => x.Id == schoolYearId).First();
            string budgetYearName = "";
            if (budgetYearId > 0)
            {
                var budgetYear = Context.SchoolYears.Where(x => x.Id == budgetYearId).First();
                budgetYearName = budgetYear.Cycle;
            }
            else
            {
                budgetYearName = "Todas";
            }

            MemoryStream stream = new MemoryStream();
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = wb.Worksheets.Add("Ejecución Presupuestaria");

                // Titulo planilla
                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(1));
                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(2));
                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(3));
                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(4));

                ws.Cell(1, 2).Value = "CICLO LECTIVO";
                ws.Cell(1, 4).Value = schoolYear.Cycle;
                ws.Cell(2, 2).Value = "PARTIDA";
                ws.Cell(2, 4).Value = budgetYearName;
                ws.Cell(3, 2).Value = "TIPO DE INSTITUCION";
                ws.Cell(3, 4).Value = GetInstitutionLevelName(InstitutionLevelId);

                if (dependenceNational)
                {
                    ws.Cell(4, 2).Value = "DEPENDENCIA";
                    ws.Cell(4, 4).Value = "NACIONAL";
                }

                // Encabezados
                var table_heads = new List<String>();
                var table_heads_groups = new List<String>();
                table_heads.Add("COD.");
                ws.Column(table_heads.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                table_heads.Add("EJE");
                table_heads.Add("COD.");
                ws.Column(table_heads.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                table_heads.Add("LINEA");

                // lineas
                var showOnlyActiveFields = Int16.Parse(schoolYear.Cycle.Trim()) > 2016;
                var lines = from line in Context.Lines_22
                            join field in Context.SubFields on line.SubFieldId equals field.Id
                            //where
                            //(showOnlyActiveFields && field.StatusId == (int)AxisStatusEnum.Vigente) || showOnlyActiveFields == false
                            orderby field.Code, field.Id, line.Code, line.Description, line.Id
                            select new
                            {
                                fieldStatus = field.StatusId,
                                fieldCode = field.Code,
                                fieldDescription = field.Description,
                                lineCode = line.Code,
                                lineDescription = line.Description,
                                lineId = line.Id
                            };

                ws.Cell(7, 1).InsertData(lines.Select(x => new { fieldCode = x.fieldCode, fieldDescription = x.fieldDescription, lineCode = x.lineCode, lineDescription = x.lineDescription }).AsEnumerable());

                var lineIds = lines.Select(x => x.lineId).ToList();

                // detalle por lineas reporte
                var color = false;
                var total_cell_offset = 0;
                // encabezado totales
                if (provinces.Count > 1)
                {
                    total_cell_offset = table_heads.Count + 1;
                    color = GenerateBudgetByProvinceReportBlock(ws, table_heads, "TOTAL", color, dependenceNational);
                }

                // columnas totales
                var totalPlannedCols = new List<string>();
                var totalRequestedCols = new List<string>();
                var totalInDictumsCols = new List<string>();
                var totalInResolutionsCols = new List<string>();

                //cada provincia
                foreach (var prov in provinces)
                {
                    // encabezados
                    var prov_cell_offset = table_heads.Count + 1;
                    color = GenerateBudgetByProvinceReportBlock(ws, table_heads, prov.Name.ToUpper(), color, dependenceNational);
                    //seba
                    // data
                    var rows = from line in Context.Lines_22
                               join field in Context.SubFields on line.SubFieldId equals field.Id
                               //where lineIds.Contains(line.Id)
                               orderby field.Code, field.Id, line.Code, line.Description, line.Id
                               select new
                               {
                                   line = line,
                                   totalPlanned = dependenceNational == false ? Context.Budgets.Where(x => x.LineId == line.Id && x.SchoolYearId == schoolYearId && x.ProvinceId == prov.Id).Sum(x => x.Ammount) ?? 0 : 0,
                                   totalRequested = Context.ImprovementPlans.Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                   totalInDictums = Context.ImprovementPlans.Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.StatusId == (int)DictumStatusEnum.Emitido || y.StatusId == (int)DictumStatusEnum.Firmado).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions = Context.ImprovementPlans.Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0
                               };

                    IEnumerable<dynamic> _rows;
                    if (dependenceNational)
                    {
                        _rows = rows.Select(x => new { totalRequested = x.totalRequested, totalInDictums = x.totalInDictums, totalInResolutions = x.totalInResolutions });
                        //_rows = rows.Select(x => new { totalRequested = x.line.Id, totalInDictums = x.totalInDictums, totalInResolutions = x.totalInResolutions });
                    }
                    else
                    {
                        _rows = rows.Select(x => new { totalPlanned = x.totalPlanned, totalRequested = x.totalRequested, totalInDictums = x.totalInDictums, totalInResolutions = x.totalInResolutions });
                        //_rows = rows.Select(x => new { totalPlanned = x.line.Id, totalRequested = x.totalRequested, totalInDictums = x.totalInDictums, totalInResolutions = x.totalInResolutions });
                    }
                    ws.Cell(7, prov_cell_offset).InsertData(
                        _rows
                    );
                    totalPlannedCols.Add(ws.Column(prov_cell_offset).ColumnLetter());
                    totalRequestedCols.Add(ws.Column(prov_cell_offset + 1).ColumnLetter());
                    totalInDictumsCols.Add(ws.Column(prov_cell_offset + 2).ColumnLetter());
                    totalInResolutionsCols.Add(ws.Column(prov_cell_offset + 3).ColumnLetter());
                }

                // data totales
                if (provinces.Count > 1)
                {
                    var lrCount = 7;
                    foreach (var line in lines)
                    {
                        if (dependenceNational == false)
                        {
                            ws.Cell(lrCount, total_cell_offset).FormulaA1 = String.Format("=SUM({0})", totalPlannedCols.Aggregate((a, b) => (!String.IsNullOrEmpty(a) ? a + (a.Contains(",") ? "" : lrCount.ToString()) + "," : "") + b + lrCount.ToString()));
                        }
                        ws.Cell(lrCount, total_cell_offset + 1 - (dependenceNational ? 1 : 0)).FormulaA1 = String.Format("=SUM({0})", totalRequestedCols.Aggregate((a, b) => (!String.IsNullOrEmpty(a) ? a + (a.Contains(",") ? "" : lrCount.ToString()) + "," : "") + b + lrCount.ToString()));
                        ws.Cell(lrCount, total_cell_offset + 2 - (dependenceNational ? 1 : 0)).FormulaA1 = String.Format("=SUM({0})", totalInDictumsCols.Aggregate((a, b) => (!String.IsNullOrEmpty(a) ? a + (a.Contains(",") ? "" : lrCount.ToString()) + "," : "") + b + lrCount.ToString()));
                        ws.Cell(lrCount, total_cell_offset + 3 - (dependenceNational ? 1 : 0)).FormulaA1 = String.Format("=SUM({0})", totalInResolutionsCols.Aggregate((a, b) => (!String.IsNullOrEmpty(a) ? a + (a.Contains(",") ? "" : lrCount.ToString()) + "," : "") + b + lrCount.ToString()));
                        // delete all fields no vigentes with no data
                        var test = ws.Cell(lrCount, total_cell_offset + 1 - (dependenceNational ? 1 : 0)).Value;
                        if (line.fieldStatus != (int)AxisStatusEnum.Vigente
                            && ws.Cell(lrCount, total_cell_offset + 1 - (dependenceNational ? 1 : 0)).Value.ToString() == "0"
                            && ws.Cell(lrCount, total_cell_offset + 2 - (dependenceNational ? 1 : 0)).Value.ToString() == "0"
                            && ws.Cell(lrCount, total_cell_offset + 3 - (dependenceNational ? 1 : 0)).Value.ToString() == "0"
                            )
                        {
                            ws.Row(lrCount).Delete();
                        }
                        else
                        {
                            lrCount++;
                        }
                    }
                }

                ws.Row(6).Style.Font.FontSize = 12;
                ws.Row(6).Height = 22;
                var heads_row = new List<String[]>();
                heads_row.Add(table_heads.ToArray());
                var heads_row_range = ws.Cell(6, 1).InsertData(heads_row);
                heads_row_range.Style.Font.Bold = true;
                heads_row_range.Style.Font.FontColor = XLColor.White;
                heads_row_range.Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;

                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
            }
            return stream;
        }
        /// <summary>
        /// Agrega un bloque de datos
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="table_heads"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="dependenceNational"></param>
        /// <returns></returns>
        private static bool GenerateBudgetByProvinceReportBlock(IXLWorksheet ws, List<string> table_heads, String text, bool color, bool dependenceNational)
        {
            var prov_cell_offset = table_heads.Count + 1;
            ws.Cell(4, prov_cell_offset).Value = text;

            if (dependenceNational == false)
            {
                table_heads.Add("PLANIFICADO");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
            }
            table_heads.Add("SOLICITADO");
            ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
            table_heads.Add("DICTAMINADO");
            ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
            table_heads.Add("APROBADO");
            ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

            var prov_cell_range = ws.Range(4, prov_cell_offset, 4, table_heads.Count);
            prov_cell_range.Style.Font.Bold = true;
            prov_cell_range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            prov_cell_range.Style.Font.FontColor = XLColor.White;
            prov_cell_range.Style.Fill.BackgroundColor = (color = !color) == false ? XLColor.BlueGray : XLColor.LightCoral;
            prov_cell_range.Merge();
            return color;
        }

        /// <summary>
        /// Exporta a un Excel el presupuesto por una determinada provincia, Version 2021 "Vertical"
        /// </summary>
        /// <param name="schoolYearId">Ciclo Lectivo de los planes a incluir</param>
        /// <param name="budgetYearId">Ciclo Lectivo de los montos ejecutados a incluir</param>
        /// <param name="provinces">provincias</param>
        /// <param name="InstitutionLevelId">Nivel de la institución</param>
        /// <param name="dependenceNational">Dependencia nacional</param>
        /// <returns>Un stream correspondiente al archivo excel generado para el reporte</returns>


        public Stream GenerateBudgetByProvinceReport_V2(int schoolYearId, int budgetYearId, List<Province> provinces, int InstitutionLevelId, bool dependenceNational = false)
        {
            // datos generales reporte
            var schoolYear = Context.SchoolYears.Where(x => x.Id == schoolYearId).First();
            string budgetYearName = "";
            if (budgetYearId > 0)
            {
                var budgetYear = Context.SchoolYears.Where(x => x.Id == budgetYearId).First();
                budgetYearName = budgetYear.Cycle;
            }
            else
            {
                budgetYearName = "Todas";
            }

            MemoryStream stream = new MemoryStream();
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = wb.Worksheets.Add("Ejecución Presupuestaria");

                // Titulo planilla
                //ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(1));
                ws.Row(1).Style.Font.FontSize = 12;
                ws.Row(1).Height = 22;
                ws.Row(1).Style.Font.Bold = true;
                ws.Row(1).Style.Font.FontColor = XLColor.White;
                ws.Row(1).Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;
                ws.Column(5).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(6).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(7).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(8).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(9).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(10).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(11).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(12).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(13).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(14).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(15).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(16).Style.NumberFormat.Format = "$ #,##0.00";
                //Encabezados
                ws.Cell(1, 1).Value = "CICLO LECTIVO";
                ws.Cell(1, 2).Value = "JURISDICCION";
                ws.Cell(1, 3).Value = "EJE";
                ws.Cell(1, 4).Value = "LINEA";
                ws.Cell(1, 5).Value = "PLANIFICADO";
                ws.Cell(1, 6).Value = "SOLICITADO";
                ws.Cell(1, 7).Value = "SOLICITADO BYS";
                ws.Cell(1, 8).Value = "SOLICITADO VIATICO";
                ws.Cell(1, 9).Value = "SOLICITADO RRHH";
                ws.Cell(1, 10).Value = "DICTAMINADO";
                ws.Cell(1, 11).Value = "APROBADO";
                ws.Cell(1, 12).Value = "APROBADO CAPITAL";
                ws.Cell(1, 13).Value = "APROBADO CORRIENTE";
                ws.Cell(1, 14).Value = "APROBADO VIATICO";
                ws.Cell(1, 15).Value = "APROBADO BIENES Y SERVICIOS";
                ws.Cell(1, 16).Value = "APROBADO RRHH";
                // Calculo Lineas
                // lineas
                var showOnlyActiveFields = Int16.Parse(schoolYear.Cycle.Trim()) > 2016;
                var lines = from line in Context.Lines
                            join field in Context.Fields on line.FieldId equals field.Id
                            where (showOnlyActiveFields && field.StatusId == (int)AxisStatusEnum.Vigente) || showOnlyActiveFields == false
                            orderby field.Code, field.Id, line.Code, line.Description, line.Id
                            select new
                            {
                                fieldStatus = field.StatusId,
                                fieldCode = field.Code,
                                fieldDescription = field.Description,
                                lineCode = line.Code,
                                lineDescription = line.Description,
                                lineId = line.Id
                            };
                var lineIds = lines.Select(x => x.lineId).ToList();
                // Recorrer 

                int I = 2;

                foreach (var prov in provinces)
                { 
                  
                        // data
                        var rows = from line in Context.Lines
                                   join field in Context.Fields on line.FieldId equals field.Id
                                   where lineIds.Contains(line.Id)
                                   orderby field.Code, field.Id, line.Code, line.Description, line.Id
                                   select new
                                   {
                                      
                                       Line = line.Code,
                                      
                                       Field     = field.Code,
                                       totalPlanned = dependenceNational == false ? Context.Budgets.Where(x => x.LineId == line.Id && x.SchoolYearId == schoolYearId && x.ProvinceId == prov.Id).Sum(x => x.Ammount) ?? 0 : 0,
                                       totalRequested = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                       totalRequested_B = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId== 1).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                       totalRequested_V = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 2).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                       totalRequested_R = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 3).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,

                                       totalInDictums = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.StatusId == (int)DictumStatusEnum.Emitido || y.StatusId == (int)DictumStatusEnum.Firmado).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                       totalInResolutions = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                       totalInResolutions_corriente = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureTypeId== (int)ExpenditureTypeEnum.GastoCorriente && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                       totalInResolutions_capital = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCapital && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                       totalInResolutions_B = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 1 && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                       totalInResolutions_V = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 2 && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                       totalInResolutions_R = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 3 && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0

                                      

                                   };


                    foreach (var r in rows.ToList())
                    {
                        ws.Cell(I, 1).Value = schoolYear.Cycle;
                        ws.Cell(I, 2).Value = prov.Name;
                        ws.Cell(I, 3).Value = r.Field;
                        ws.Cell(I, 4).Value = r.Line;
                        ws.Cell(I, 5).Value = r.totalPlanned;
                   
                        ws.Cell(I, 6).Value = r.totalRequested;
                        ws.Cell(I, 7).Value = r.totalRequested_B;
                        ws.Cell(I, 8).Value = r.totalRequested_V;
                        ws.Cell(I, 9).Value = r.totalRequested_R;
                       
                        ws.Cell(I, 10).Value = r.totalInDictums;
                        ws.Cell(I, 11).Value = r.totalInResolutions;
                        ws.Cell(I, 12).Value = r.totalInResolutions_capital;
                        ws.Cell(I, 13).Value = r.totalInResolutions_corriente;
                        ws.Cell(I, 14).Value = r.totalInResolutions_B;
                        ws.Cell(I, 15).Value = r.totalInResolutions_V;
                        ws.Cell(I, 16).Value = r.totalInResolutions_R;

                        I = I+1;
                    }

                    var rows_pronafe = from line in Context.Lines
                                       join field in Context.Fields on line.FieldId equals field.Id
                                       //  where lineIds.Contains(line.Id)
                                       orderby field.Code, field.Id, line.Code, line.Description, line.Id
                                       select new
                                       {

                                           Line = line.Code,

                                           Field = field.Code,
                                           totalPlanned = dependenceNational == false ? Context.Budgets_216.Where(x => x.LineId == line.Id && x.SchoolYearId == schoolYearId && x.ProvinceId == prov.Id).Sum(x => x.Ammount) ?? 0 : 0,
                                           totalRequested = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId.Value == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                           totalRequested_B = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId.Value == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 1).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                           totalRequested_V = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId.Value == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 2).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                           totalRequested_R = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId.Value == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 3).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,

                                           totalInDictums = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.StatusId == (int)DictumStatusEnum.Emitido || y.StatusId == (int)DictumStatusEnum.Firmado).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                           totalInResolutions = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                           totalInResolutions_corriente = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCorriente && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                           totalInResolutions_capital = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCapital && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                           totalInResolutions_B = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 1 && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                           totalInResolutions_V = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 2 && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                           totalInResolutions_R = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 3 && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0



                                       };

                    var pronafe = rows_pronafe.Where(x => x.totalRequested > 0).ToList();
                    foreach (var r in pronafe.ToList())
                    {
                        ws.Cell(I, 1).Value = schoolYear.Cycle;
                        ws.Cell(I, 2).Value = prov.Name + "-PRONAFE";
                        ws.Cell(I, 3).Value = r.Field;
                        ws.Cell(I, 4).Value = r.Line;
                        ws.Cell(I, 5).Value = r.totalPlanned;

                        ws.Cell(I, 6).Value = r.totalRequested;
                        ws.Cell(I, 7).Value = r.totalRequested_B;
                        ws.Cell(I, 8).Value = r.totalRequested_V;
                        ws.Cell(I, 9).Value = r.totalRequested_R;

                        ws.Cell(I, 10).Value = r.totalInDictums;
                        ws.Cell(I, 11).Value = r.totalInResolutions;
                        ws.Cell(I, 12).Value = r.totalInResolutions_capital;
                        ws.Cell(I, 13).Value = r.totalInResolutions_corriente;
                        ws.Cell(I, 14).Value = r.totalInResolutions_B;
                        ws.Cell(I, 15).Value = r.totalInResolutions_V;
                        ws.Cell(I, 16).Value = r.totalInResolutions_R;

                        I = I + 1;
                    }


                }

               
                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
            }
            return stream;
        }

        public Stream GenerateBudgetByProvinceReport_V2_22(int schoolYearId, int budgetYearId, List<Province> provinces, int InstitutionLevelId, bool dependenceNational = false)
        {
            // datos generales reporte
            var schoolYear = Context.SchoolYears.Where(x => x.Id == schoolYearId).First();
            string budgetYearName = "";
            if (budgetYearId > 0)
            {
                var budgetYear = Context.SchoolYears.Where(x => x.Id == budgetYearId).First();
                budgetYearName = budgetYear.Cycle;
            }
            else
            {
                budgetYearName = "Todas";
            }

            MemoryStream stream = new MemoryStream();
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = wb.Worksheets.Add("Ejecución Presupuestaria");

                // Titulo planilla
                //ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(1));
                ws.Row(1).Style.Font.FontSize = 12;
                ws.Row(1).Height = 22;
                ws.Row(1).Style.Font.Bold = true;
                ws.Row(1).Style.Font.FontColor = XLColor.White;
                ws.Row(1).Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;
                ws.Column(5).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(6).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(7).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(8).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(9).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(10).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(11).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(12).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(13).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(14).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(15).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Column(16).Style.NumberFormat.Format = "$ #,##0.00";
                //Encabezados
                ws.Cell(1, 1).Value = "CICLO LECTIVO";
                ws.Cell(1, 2).Value = "JURISDICCION";
                ws.Cell(1, 3).Value = "EJE";
                ws.Cell(1, 4).Value = "LINEA";
                ws.Cell(1, 5).Value = "PLANIFICADO";
                ws.Cell(1, 6).Value = "SOLICITADO";
                ws.Cell(1, 7).Value = "SOLICITADO BYS";
                ws.Cell(1, 8).Value = "SOLICITADO VIATICO";
                ws.Cell(1, 9).Value = "SOLICITADO RRHH";
                ws.Cell(1, 10).Value = "DICTAMINADO";
                ws.Cell(1, 11).Value = "APROBADO";
                ws.Cell(1, 12).Value = "APROBADO CAPITAL";
                ws.Cell(1, 13).Value = "APROBADO CORRIENTE";
                ws.Cell(1, 14).Value = "APROBADO VIATICO";
                ws.Cell(1, 15).Value = "APROBADO BIENES Y SERVICIOS";
                ws.Cell(1, 16).Value = "APROBADO RRHH";
                // Calculo Lineas
                // lineas
                var showOnlyActiveFields = Int16.Parse(schoolYear.Cycle.Trim()) > 2016;
                //var lines = from line in Context.Lines_22
                //            join field in Context.SubFields on line.SubFieldId equals field.Id
                //            where (showOnlyActiveFields && field.StatusId == (int)AxisStatusEnum.Vigente) || showOnlyActiveFields == false
                //            orderby field.Code, field.Id, line.Code, line.Description, line.Id
                //            select new
                //            {
                //                fieldStatus = field.StatusId,
                //                fieldCode = field.Code,
                //                fieldDescription = field.Description,
                //                lineCode = line.Code,
                //                lineDescription = line.Description,
                //                lineId = line.Id
                //            };
                //var lineIds = lines.Select(x => x.lineId).ToList();
                // Recorrer 

                int I = 2;

                foreach (var prov in provinces)
                {

                    // data - No Pronafe!
                    var rows = from line in Context.Lines_22
                               join field in Context.SubFields on line.SubFieldId equals field.Id
                             //  where lineIds.Contains(line.Id)
                               orderby field.Code, field.Id, line.Code, line.Description, line.Id
                               select new
                               {

                                   Line = line.Code,

                                   Field = field.Code,
                                   totalPlanned = dependenceNational == false ? Context.Budgets_216.Where(x => x.LineId == line.Id && x.SchoolYearId == schoolYearId && x.ProvinceId == prov.Id).Sum(x => x.Ammount) ?? 0 : 0,
                                   totalRequested = Context.ImprovementPlans.Where(x=>(x.Pronafe.Value!=true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.Line_22_Id == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                   totalRequested_B = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.Line_22_Id == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 1).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                   totalRequested_V = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.Line_22_Id == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 2).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                   totalRequested_R = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.Line_22_Id == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 3).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,

                                   totalInDictums = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.Line_22_Id == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.StatusId == (int)DictumStatusEnum.Emitido || y.StatusId == (int)DictumStatusEnum.Firmado).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.Line_22_Id == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_corriente = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCorriente && x.Line_22_Id == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_capital = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCapital && x.Line_22_Id == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_B = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 1 && x.Line_22_Id == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_V = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 2 && x.Line_22_Id == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_R = Context.ImprovementPlans.Where(x => (x.Pronafe.Value != true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 3 && x.Line_22_Id == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0



                               };





                    foreach (var r in rows.ToList())
                    {
                        ws.Cell(I, 1).Value = schoolYear.Cycle;
                        ws.Cell(I, 2).Value = prov.Name;
                        ws.Cell(I, 3).Value = r.Field;
                        ws.Cell(I, 4).Value = r.Line;
                        ws.Cell(I, 5).Value = r.totalPlanned;

                        ws.Cell(I, 6).Value = r.totalRequested;
                        ws.Cell(I, 7).Value = r.totalRequested_B;
                        ws.Cell(I, 8).Value = r.totalRequested_V;
                        ws.Cell(I, 9).Value = r.totalRequested_R;

                        ws.Cell(I, 10).Value = r.totalInDictums;
                        ws.Cell(I, 11).Value = r.totalInResolutions;
                        ws.Cell(I, 12).Value = r.totalInResolutions_capital;
                        ws.Cell(I, 13).Value = r.totalInResolutions_corriente;
                        ws.Cell(I, 14).Value = r.totalInResolutions_B;
                        ws.Cell(I, 15).Value = r.totalInResolutions_V;
                        ws.Cell(I, 16).Value = r.totalInResolutions_R;

                        I = I + 1;
                    }

                    var rows_pronafe = from line in Context.Lines
                               join field in Context.Fields on line.FieldId equals field.Id
                               //  where lineIds.Contains(line.Id)
                               orderby field.Code, field.Id, line.Code, line.Description, line.Id
                               select new
                               {

                                   Line = line.Code,

                                   Field = field.Code,
                                   totalPlanned = dependenceNational == false ? Context.Budgets_216.Where(x => x.LineId == line.Id && x.SchoolYearId == schoolYearId && x.ProvinceId == prov.Id).Sum(x => x.Ammount) ?? 0 : 0,
                                   totalRequested = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId.Value == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                   totalRequested_B = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId.Value == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 1).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                   totalRequested_V = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId.Value == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 2).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,
                                   totalRequested_R = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId.Value == line.Id && x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == 3).Sum(x => x.RequestedAmount * x.RequestedPriceUnit) ?? 0,

                                   totalInDictums = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.StatusId == (int)DictumStatusEnum.Emitido || y.StatusId == (int)DictumStatusEnum.Firmado).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_corriente = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCorriente && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_capital = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCapital && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_B = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 1 && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_V = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 2 && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0,
                                   totalInResolutions_R = Context.ImprovementPlans.Where(x => (x.Pronafe.Value == true)).Where(x => x.CUE.Substring(0, 2) == prov.Number && x.SchoolYearId == schoolYearId && ((dependenceNational == false && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence))) || (dependenceNational == true && x.Dependence.Contains("Nacional") == true)) && (InstitutionLevelId == 0 || (InstitutionLevelId > 0 && x.InstitutionLevelInt == InstitutionLevelId)) && x.StatusId != (int)StageStatusEnum.Anulado).SelectMany(x => x.Solicitudes).Where(x => x.ExpenditureObjectTypeId == 3 && x.LineId == line.Id && ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit) ?? 0



                               };

                  var pronafe=  rows_pronafe.Where(x => x.totalRequested > 0).ToList();
                   // I = I + 1;
                    ws.Cell(I, 1).Value = schoolYear.Cycle;
                    ws.Cell(I, 2).Value = prov.Name;
                    ws.Cell(I, 3).Value ="PRONAFE";
                    ws.Cell(I, 4).Value = "P";
                    ws.Cell(I, 5).Value =  pronafe.Sum(x=>x.totalPlanned);

                    ws.Cell(I, 6).Value = pronafe.Sum(x => x.totalRequested);
                    ws.Cell(I, 7).Value = pronafe.Sum(x => x.totalRequested_B);
                    ws.Cell(I, 8).Value = pronafe.Sum(x => x.totalRequested_V);
                    ws.Cell(I, 9).Value = pronafe.Sum(x => x.totalRequested_R);

                    ws.Cell(I, 10).Value = pronafe.Sum(x => x.totalInDictums);
                    ws.Cell(I, 11).Value = pronafe.Sum(x => x.totalInResolutions);
                    ws.Cell(I, 12).Value = pronafe.Sum(x => x.totalInResolutions_capital);
                    ws.Cell(I, 13).Value = pronafe.Sum(x => x.totalInResolutions_corriente);
                    ws.Cell(I, 14).Value = pronafe.Sum(x => x.totalInResolutions_B);
                    ws.Cell(I, 15).Value = pronafe.Sum(x => x.totalInResolutions_V);
                    ws.Cell(I, 16).Value = pronafe.Sum(x => x.totalInResolutions_R);
                }


                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
            }
            return stream;
        }


        /// <summary>
        /// Exporta a un Excel el presupuesto por una determinada provincia
        /// </summary>
        /// <param name="schoolYearId">Ciclo Lectivo de los planes a incluir</param>
        /// <param name="budgetYearId">Ciclo Lectivo de los montos ejecutados a incluir</param>
        /// <param name="provNumbers">Lista de provincias</param>
        /// <returns>Un stream correspondiente al archivo excel generado para el reporte</returns>
        public Stream GenerateStatementsOfAccountsReport(int schoolYearId, int budgetYearId, IList<String> provNumbers)
        {
            var schoolYear = Context.SchoolYears.Where(x => x.Id == schoolYearId).First();
            var budgetYearName = "";
            if (budgetYearId > 0)
            {
                var budgetYear = Context.SchoolYears.Where(x => x.Id == budgetYearId).First();
                budgetYearName = budgetYear.Cycle;
            }
            else
            {
                budgetYearName = "Todas";
            }

            MemoryStream stream = new MemoryStream();
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = wb.Worksheets.Add("Estado cuenta provincias");

                // Encabezados
                var table_heads = new List<String>();
                table_heads.Add("Ciclo");
                table_heads.Add("Provincia");
                table_heads.Add("Total Planificado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado Bienes y Servicios");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado Viaticos");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado RRHH");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                //table_heads.Add("Elegibilidad");
                //ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                //table_heads.Add("Total Evaluado");
                //ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Dictaminado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                //table_heads.Add("En Disposiciones");
                //ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                //table_heads.Add("Rechazado");
                //ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                //table_heads.Add("Desestimado");
                //ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                //table_heads.Add("Pendiente");
                //ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Aprobado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
               
                table_heads.Add("Total Aprobado Capital");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Aprobado Corriente");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Aprobado Bienes y Servicios");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Aprobado Viaticos");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Aprobado RRHH");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Row(1).Style.Font.FontSize = 12;
                ws.Row(1).Height = 22;
                var heads_row = new List<String[]>();
                heads_row.Add(table_heads.ToArray());
                var heads_row_range = ws.Cell(1, 1).InsertData(heads_row);
                heads_row_range.Style.Font.Bold = true;
                heads_row_range.Style.Font.FontColor = XLColor.White;
                heads_row_range.Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;

                // INFO
                var lst = new List<StatementOfAccountDTO>();
                var provinces = Context.Provinces.Where(x => provNumbers.Count > 0 ? provNumbers.Contains(x.Number) : true).Select(y => new
                {
                    province = y,
                    totals = Context.ImprovementPlans.Where(x => x.CUE.StartsWith(y.Number) && x.SchoolYearId == schoolYearId && x.StatusId != (int)StageStatusEnum.Anulado && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence)))
                        .SelectMany(x => x.Solicitudes)
                        .GroupBy(x => true)
                        .Select(grp => new
                        {
                            TotalPending = grp.Sum(x => x.StatusId == (int)SolicitudeStatusEnum.Pendiente || x.StatusId == (int)SolicitudeStatusEnum.EnProceso ? x.RequestedAmount * x.RequestedPriceUnit : 0),
                            TotalElegible = grp.Sum(x => x.StatusId == (int)SolicitudeStatusEnum.Elegible ? x.RequestedAmount * x.RequestedPriceUnit : 0),
                            TotalRequested = grp.Sum(x => x.StatusId != (int)SolicitudeStatusEnum.Anulado ? x.RequestedAmount * x.RequestedPriceUnit : 0),
                            TotalRequested_B = grp.Sum(x => (x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId== (int)ExpenditureObjectTypeEnum.Bienes_Servicios) ? x.RequestedAmount * x.RequestedPriceUnit : 0),
                            TotalRequested_V = grp.Sum(x => (x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == (int)ExpenditureObjectTypeEnum.Viaticos) ? x.RequestedAmount * x.RequestedPriceUnit : 0),
                            TotalRequested_R = grp.Sum(x => (x.StatusId != (int)SolicitudeStatusEnum.Anulado && x.ExpenditureObjectTypeId == (int)ExpenditureObjectTypeEnum.RRHH) ? x.RequestedAmount * x.RequestedPriceUnit : 0),
                            TotalInDictum = grp.Where(x => x.Dictums.Where(j => j.StatusId == (int)DictumStatusEnum.Emitido || j.StatusId == (int)DictumStatusEnum.Firmado).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit),
                            // TotalApproved = grp.Sum(x => x.StatusId == (int)SolicitudeStatusEnum.Aprobado ? x.ApprovedAmount * x.ApprovedPriceUnit : 0),
                            TotalApproved = grp.Where(x => x.Dictums.Where(j => j.Resolutions.Where(g=>g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit),
                            TotalApproved_Capital = grp.Where(x => (x.Dictums.Where(j => j.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any())&& x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCapital).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit),
                            TotalApproved_Ordinary = grp.Where(x => (x.Dictums.Where(j => j.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()) && x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCorriente).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit),
                            TotalApproved_B = grp.Where(x => (x.Dictums.Where(j => j.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()) && x.ExpenditureObjectTypeId==(int)ExpenditureObjectTypeEnum.Bienes_Servicios ).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit),
                            TotalApproved_V = grp.Where(x => (x.Dictums.Where(j => j.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()) && x.ExpenditureObjectTypeId == (int)ExpenditureObjectTypeEnum.Viaticos).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit),
                            TotalApproved_R = grp.Where(x => (x.Dictums.Where(j => j.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any()) && x.ExpenditureObjectTypeId == (int)ExpenditureObjectTypeEnum.RRHH).Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit),


                            // TotalApproved_Capital  = grp.Sum(x => x.StatusId == (int)SolicitudeStatusEnum.Aprobado && x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCapital ? x.ApprovedAmount * x.ApprovedPriceUnit : 0),
                            // TotalApproved_Ordinary = grp.Sum(x => x.StatusId == (int)SolicitudeStatusEnum.Aprobado && x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCorriente ? x.ApprovedAmount * x.ApprovedPriceUnit : 0),
                            Rejected = grp.Sum(x => x.StatusId == (int)SolicitudeStatusEnum.Rechazado ? x.RequestedAmount * x.RequestedPriceUnit : 0),
                            Dismissed = grp.Sum(x => x.StatusId == (int)SolicitudeStatusEnum.Aprobado ? (x.RequestedAmount * x.RequestedPriceUnit) - (x.ApprovedAmount * x.ApprovedPriceUnit) : 0),
                            
                        })
                });

                foreach (var data in provinces)
                {
                    var dto = new StatementOfAccountDTO();
                    dto.BudgetYear = budgetYearName;
                    dto.SchoolYear = schoolYear.Cycle;
                    dto.Province = data.province.Name;
                    dto.ProvinceId = data.province.Id;
                    var extra = data.totals.ToList();
                    if (extra.Any())
                    {
                        dto.TotalInDictum = extra.FirstOrDefault().TotalInDictum ?? 0;
                        dto.TotalApproved = extra.FirstOrDefault().TotalApproved ?? 0;
                        dto.TotalElegible = extra.FirstOrDefault().TotalElegible ?? 0;
                        dto.TotalPending = extra.FirstOrDefault().TotalPending ?? 0;
                        dto.TotalRequested = extra.FirstOrDefault().TotalRequested ?? 0;
                        dto.Rejected = extra.FirstOrDefault().Rejected ?? 0;
                        dto.Dismissed = extra.FirstOrDefault().Dismissed ?? 0;
                        dto.TotalApproved_Capital = extra.FirstOrDefault().TotalApproved_Capital ?? 0;
                        dto.TotalApproved_Ordinary = extra.FirstOrDefault().TotalApproved_Ordinary ?? 0;
                        dto.TotalPlanned = this.GetBudget(schoolYear.Id, data.province.Id);
                        dto.TotalRequested_B = extra.FirstOrDefault().TotalRequested_B ?? 0;
                        dto.TotalRequested_V = extra.FirstOrDefault().TotalRequested_V ?? 0;
                        dto.TotalRequested_R = extra.FirstOrDefault().TotalRequested_R ?? 0;
                        dto.TotalApproved_B = extra.FirstOrDefault().TotalApproved_B ?? 0;
                        dto.TotalApproved_V = extra.FirstOrDefault().TotalApproved_V ?? 0;
                        dto.TotalApproved_R = extra.FirstOrDefault().TotalApproved_R ?? 0;
                    }

                    lst.Add(dto);
                }

                //var inResolutions = Context.Provinces.Where(x => provNumbers.Count > 0 ? provNumbers.Contains(x.Number) : true).Select(p => new
                //{
                //    province = p,
                //    totals = Context.ImprovementPlans.Where(x => x.CUE.StartsWith(p.Number) && x.SchoolYearId == schoolYearId && x.StatusId != (int)StageStatusEnum.Anulado && (x.Dependence.Contains("Nacional") == false || String.IsNullOrEmpty(x.Dependence)))
                //        .SelectMany(x => x.Solicitudes)
                //        .Where(x => ((budgetYearId > 0) ? x.SchoolYearId == budgetYearId : true) && x.Dictums.Where(y => y.Resolutions.Where(g => g.StatusId == (int)ResolutionStatusEnum.Emitido || g.StatusId == (int)ResolutionStatusEnum.Firmado || g.StatusId == (int)ResolutionStatusEnum.Protocolizado).Any()).Any())
                //        .GroupBy(x => true)
                //        .Select(grp => new
                //        {
                //            TotalInResolution = grp.Sum(x => x.ApprovedAmount * x.ApprovedPriceUnit),
                //            //Dismissed = grp.Sum(x => (x.RequestedAmount * x.RequestedPriceUnit) - (x.ApprovedAmount * x.ApprovedPriceUnit))
                //        })
                //});
                //foreach (var data in inResolutions)
                //{
                //    var _dto = lst.Where(x => x.ProvinceId == data.province.Id).FirstOrDefault();
                //    if (_dto.ProvinceId > 0)
                //    {
                //        var extra = data.totals.ToList();
                //        if (extra.Any())
                //        {
                //            _dto.TotalInResolution = extra.FirstOrDefault().TotalInResolution ?? 0;
                //            //_dto.Dismissed = extra.FirstOrDefault().Dismissed ?? 0;
                //        }

                //    }
                //}

                //var budgets = Context.Provinces.Where(x => provNumbers.Count > 0 ? provNumbers.Contains(x.Number) : true).Select(p => new
                //{
                //    province = p,
                //    totalPlanned = p.Budgets.Where(x => x.SchoolYearId == schoolYearId).Sum(x => x.Ammount) ?? 0
                //});
                //foreach (var data in budgets)
                //{
                //    var _dto = lst.Where(x => x.ProvinceId == data.province.Id).FirstOrDefault();
                //    if (_dto.ProvinceId > 0)
                //    {
                //        _dto.TotalPlanned = data.totalPlanned;
                //    }
                //}
                ws.Cell(2, 1).InsertData(lst.Select(x => x.SchoolYear));
                ws.Cell(2, 2).InsertData(lst.Select(x => x.Province));
                ws.Cell(2, 3).InsertData(lst.Select(x => x.TotalPlanned));
                ws.Cell(2, 4).InsertData(lst.Select(x => x.TotalRequested));
                ws.Cell(2, 5).InsertData(lst.Select(x => x.TotalRequested_B));
                ws.Cell(2, 6).InsertData(lst.Select(x => x.TotalRequested_V));
                ws.Cell(2, 7).InsertData(lst.Select(x => x.TotalRequested_R));
                //   ws.Cell(4, 4).InsertData(lst.Select(x => x.TotalElegible));

                ws.Cell(2, 8).InsertData(lst.Select(x => x.TotalInDictum));
                ws.Cell(2, 9).InsertData(lst.Select(x => x.TotalApproved));
                ws.Cell(2, 10).InsertData(lst.Select(x => x.TotalApproved_Capital));
                ws.Cell(2, 11).InsertData(lst.Select(x => x.TotalApproved_Ordinary));
                ws.Cell(2, 12).InsertData(lst.Select(x => x.TotalApproved_B));
                ws.Cell(2, 13).InsertData(lst.Select(x => x.TotalApproved_V));
                ws.Cell(2, 14).InsertData(lst.Select(x => x.TotalApproved_R));
                //ws.Cell(4, 7).InsertData(lst.Select(x => x.TotalInResolution));
                //ws.Cell(4, 8).InsertData(lst.Select(x => x.Rejected + x.Dismissed));
                //ws.Cell(4, 9).InsertData(lst.Select(x => x.Dismissed));
                //ws.Cell(4, 9).InsertData(lst.Select(x => x.TotalPending));

                //// Titulo planilla
                //ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(1));
                //ws.Cell(1, 1).Value = "CICLO LECTIVO";
                //ws.Cell(1, 2).Value = schoolYear.Cycle;
                //ws.Cell(1, 3).Value = "PARTIDA";
                //ws.Cell(1, 4).Value = budgetYearName;

                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
            }
            return stream;
        }

        #region Improvement Plans By Budget DataSource

        /// <summary>
        /// Exporta a un excel los planes de mejora de un determinado ciclo lectivo y categoría
        /// </summary>
        /// <param name="schoolYearId">Ciclo Lectivo de los planes a incluir</param>
        /// <param name="budgetYearId">Ciclo Lectivo de los montos ejecutados a incluir</param>
        /// <param name="planTypeIds">Tipos de plan a incluir</param>
        /// <param name="provNumbers">Provincias a incluir en el reporte</param>
        /// <param name="dependenceNational">Si es de dependencias nacionales</param>
        /// <param name="CUE">CUE de institucion a filtrar</param>
        /// <param name="abreviated">Reporte completo con detalle de líneas o solo totales</param>
        /// <returns>Un stream correspondiente al archivo excel generado para el reporte</returns>
        public Stream GenerateImprovementPlansReport(int schoolYearId, int budgetYearId, string planTypeIds, List<string> provNumbers, bool dependenceNational = false, string CUE = null, bool abreviated = false)
        {


            var planTypes = String.Join(",", planTypeIds.Split(',').Select(x => Int32.Parse(x)));

            // Obtengo DataSource del reporte principal
            var schoolYear = "";
            var budgetYearName = "";
            if (budgetYearId > 0)
            {
                var budgetYear = Context.SchoolYears.Where(x => x.Id == budgetYearId).First();
                budgetYearName = budgetYear.Cycle;
            }
            else
            {
                budgetYearName = "Todas";
            }

            if (schoolYearId>0)
            {
                schoolYear= Context.SchoolYears.Where(x => x.Id == schoolYearId).First().Cycle;
            } else
            {
                schoolYear = "Todos";
            }
            

            MemoryStream stream = new MemoryStream();
            try
            {
                using (var wb = new XLWorkbook(XLEventTracking.Disabled))
                {
                    var ws = wb.Worksheets.Add("Reporte Planes");

                    //// Titulo planilla
                    ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(1));
                    ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(2));

                    ws.Cell(1, 1).Value = "CICLO LECTIVO";
                    ws.Cell(1, 2).Value = schoolYear;
                    ws.Cell(1, 3).Value = "PARTIDA";
                    ws.Cell(1, 4).Value = budgetYearName;
                    if (dependenceNational)
                    {
                        ws.Cell(2, 1).Value = "DEPENDENCIA";
                        ws.Cell(2, 2).Value = "NACIONAL";
                    }

                    // Encabezados
                    var table_heads = new List<String>();

                    table_heads.Add("Provincia");
                    table_heads.Add("Nº De Expedientes");
                    table_heads.Add("Tipo de Plan");
                    table_heads.Add("EJE");
                    table_heads.Add("Linea/s");
                    table_heads.Add("Descripción");
                    table_heads.Add("Fecha Ingreso");
                    table_heads.Add("Fecha Ing. a Eje");
                    table_heads.Add("Evaluador");
                    table_heads.Add("Total Solicitado");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    table_heads.Add("Total Solicitado Inventariable");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    table_heads.Add("Total Solicitado No Inventariable");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    table_heads.Add("Nº Dictámenes");
                    table_heads.Add("Total Dictaminado");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    table_heads.Add("Fecha Firma Dictámenes");
                    table_heads.Add("Total Dictaminado Inventariable");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    table_heads.Add("Total Dictaminado No Inventariable");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    //table_heads.Add("Desestimado");
                    //ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    table_heads.Add("Anulado");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    table_heads.Add("Rechazo");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    table_heads.Add("Total Aprobado");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                    table_heads.Add("Fecha Firma Disposiciones");
                    table_heads.Add("Fecha Firma Anexos");
                    table_heads.Add("Disposiciones");
                    table_heads.Add("CUE");
                    table_heads.Add("Nivel");
                    table_heads.Add("Inst./Juris.");
                    table_heads.Add("Departamento");
                    table_heads.Add("Localidad");
                    table_heads.Add("Código de Plan");
                    table_heads.Add("Estado del Plan");
                    table_heads.Add("Total Rendido Bys");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                    table_heads.Add("Total Rendido Viaticos");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                    table_heads.Add("Total Rendido RRHH");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                    table_heads.Add("Total Rendido ");
                    ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";


                    table_heads.Add("ID Sistema");

                    var heads_row = new List<String[]>();
                    heads_row.Add(table_heads.ToArray());
                    var heads_row_range = ws.Cell(4, 1).InsertData(heads_row.ToArray());

                    // Formato encabezados
                    ws.Row(4).Style.Font.FontSize = 12;
                    ws.Row(4).Height = 22;
                    ws.SheetView.Freeze(4, 0);
                    heads_row_range.Style.Font.Bold = true;
                    heads_row_range.Style.Font.FontColor = XLColor.White;
                    heads_row_range.Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;

                    // CONTENIDO
                    string sql = @"SELECT prov.NAME as Provincia
	,p.ReceptionDate as FechaDeIngreso
	,TipoDePlan = (SELECT pt.Description FROM ImprovementPlansTypes AS pt WHERE pt.Id = p.ImprovementPlanTypeId)
    ,Axis = (select f.Code from Fields as f where f.Id = p.FieldId)
    ,Lines =  STUFF((SELECT ', ' + lc.CODE FROM Solicitudes AS lc_sol INNER JOIN Lines AS lc ON lc.Id = lc_sol.LineId WHERE lc_sol.ImprovementPlanId = p.Id GROUP BY lc.CODE FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
    ,p.Summary
	,p.CUE as CUE
    ,p.InstitutionLevel as InstitutionLevel
	,p.InstitutionName as Institucion
	,p.Department as Departamento
	,p.Location as Localidad
	,p.Identifier as CodPlan
	,Estado = (SELECT ps.Description FROM STATUS AS ps WHERE ps.Id = p.StatusId)
	,subp.totalSolicitado as TotalSolicitado
    ,subp.totalSolicitadoInvetariable as TotalSolicitadoInventariable
    ,subp.totalSolicitadoNoInvetariable as TotalSolicitadoNoInventariable
	,Expedientes = STUFF((SELECT ', ' + solFN.FileNumber FROM (SELECT DISTINCT (solFN_.FileNumber) AS FileNumber FROM Solicitudes AS solFN_ WHERE solFN_.ImprovementPlanId = p.Id) AS solFN FOR XML PATH(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	,p.FieldDate as FechaIngresoCampo
    , CONCAT(usu.Name, ' ', usu.LastName) as evaluatorName
	,FechaDictamenes = STUFF((SELECT ', ' + CONVERT(VARCHAR, dctSD.SignatureDate, 103) FROM (SELECT DISTINCT (dctSD_.SignatureDate) AS SignatureDate FROM Dictums AS dctSD_D INNER JOIN Documents AS dctSD_ ON dctSD_D.Id = dctSD_.Id AND dctSD_.ImprovementPlanId = p.Id ) AS dctSD FOR XML PATH(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	,NroDictamenes = STUFF((SELECT ', ' + dctNum_.DictumNumber FROM Dictums AS dctNum_ INNER JOIN Documents AS dctNum_D ON dctNum_D.Id = dctNum_.Id WHERE dctNum_D.ImprovementPlanId = p.Id FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	,subp.totalDictaminado
    ,subp.totalDictaminadoInventariable as totalDictaminadoInventariable
    ,subp.totalDictaminadoNoInventariable as totalDictaminadoNoInventariable
    ,subp.totalAprobado
	,subp.totalDesestimado
	,subp.totalAnulado
	,subp.totalRechazado
	,subp.totalElegible
    ,FechaResoluciones = STUFF((SELECT ', ' + CONVERT(VARCHAR, rd_rdd.SignatureDate, 103) FROM Resolutions as rd_r INNER JOIN Documents as rd_rdd ON rd_rdd.Id = rd_r.Id INNER JOIN ResolutionDictums AS rd_rd ON rd_r.Id = rd_rd.ResolutionId inner join Documents as rd_dd on rd_dd.Id = rd_rd.DictumId and rd_dd.ImprovementPlanId = p.Id where rd_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
    ,FechaAnexoResoluciones = STUFF((SELECT ', ' + CONVERT(VARCHAR, rad_r.AnnexSignatureDate, 103) FROM Resolutions as rad_r INNER JOIN Documents as rad_rdd ON rad_rdd.Id = rad_r.Id INNER JOIN ResolutionDictums AS rad_rd ON rad_r.Id = rad_rd.ResolutionId inner join Documents as rad_dd on rad_dd.Id = rad_rd.DictumId and rad_dd.ImprovementPlanId = p.Id where rad_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	,Resoluciones = STUFF((SELECT ', ' + rn_r.ResolutionNumber FROM Resolutions as rn_r INNER JOIN Documents as rn_rdd ON rn_rdd.Id = rn_r.Id INNER JOIN ResolutionDictums AS rn_rd ON rn_r.Id = rn_rd.ResolutionId inner join Documents as rn_dd on rn_dd.Id = rn_rd.DictumId and rn_dd.ImprovementPlanId = p.Id where rn_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
    ,subp.totalSinDictamen
	,subp.totalSinResolucion
    ,subp.totalRendidoBys
    ,subp.totalRendidoPyv
    ,subp.totalRendidoRrhh
    ,(subp.totalRendidoBys + subp.totalRendidoPyv +subp.totalRendidoRrhh) as TotalRendido
	,p.Id
FROM (
	SELECT prep.Id
		,totalAprobado = ISNULL(prep.totalAprobado, 0)
		,totalDesestimado = ISNULL(prep.totalDesestimado, 0)
		,totalDictaminado = ISNULL(prep.totalDictaminado, 0)
        ,totalDictaminadoInventariable = ISNULL(prep.totalDictaminadoInventariable, 0)
        ,totalDictaminadoNoInventariable = ISNULL(prep.totalDictaminadoNoInventariable, 0)
        ,totalSinDictamen = ISNULL(prep.totalSinDictamen, 0)
		,totalSinResolucion = ISNULL(prep.totalSinResolucion, 0)
		,totalSolicitado = SUM(sol.RequestedAmount * sol.RequestedPriceUnit)
        ,totalSolicitadoInvetariable = sum(CASE WHEN sol.ExpenditureTypeId = @inventariable THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
        ,totalSolicitadoNoInvetariable = sum(CASE WHEN sol.ExpenditureTypeId = @noInventariable THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
		,totalAnulado = sum(CASE WHEN sol.StatusId = @solicitudeStatusAnulado THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
		,totalRechazado = sum(CASE WHEN sol.StatusId = @solicitudeStatusRechazado THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
		,totalElegible = sum(CASE WHEN sol.StatusId = @solicitudeStatusElegible THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
	    ,totalRendidoBys =ISNULL(prep.totalRendidoBys, 0)
        ,totalRendidoPyv =ISNULL(prep.totalRendidoPyv, 0)
        ,totalRendidoRrhh =ISNULL(prep.totalRendidoRrhh, 0)
        
     FROM (
		SELECT p1.id
            ,totalAprobado = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
			,totalDesestimado = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN (sol1.RequestedAmount * sol1.RequestedPriceUnit) - sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
            ,totalDictaminado = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
            ,totalDictaminadoInventariable = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and sol1.ExpenditureTypeId = @inventariable THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
            ,totalDictaminadoNoInventariable = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and sol1.ExpenditureTypeId = @noInventariable THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
			,totalSinDictamen = sum(CASE WHEN dt1.Id IS NULL AND sol1.StatusId = @solicitudeStatusAprobado THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
			,totalSinResolucion = sum(CASE WHEN dt1.Id IS NOT NULL AND d1.StatusId NOT IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
            ,totalRendidoBys    = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and  ac.AccountingRenderingID is not null and ac.ExpenditureObjectTypeID=1 THEN ac.aprovedAmount ELSE 0 END)
		    ,totalRendidoPyv    = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and  ac.AccountingRenderingID is not null and ac.ExpenditureObjectTypeID=2 THEN ac.aprovedAmount ELSE 0 END)
            ,totalRendidoRrhh    = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and  ac.AccountingRenderingID is not null and ac.ExpenditureObjectTypeID=3 THEN ac.aprovedAmount ELSE 0 END)
        FROM ImprovementPlans AS p1
		LEFT JOIN Solicitudes AS sol1 ON sol1.ImprovementPlanId = p1.Id
		LEFT JOIN Dictums_Solicitudes AS ds1 ON ds1.SolicitudeId = sol1.Id
		LEFT JOIN Documents AS dt1 ON ds1.DictumId = dt1.Id
		LEFT JOIN ResolutionDictums AS rs1 ON rs1.DictumId = ds1.DictumId
		LEFT JOIN Documents AS d1 ON d1.id = rs1.ResolutionId
        LEFT JOIN Resolutions as r on d1.Id = r.Id
        LEFT JOIN AccountRendering ac on ac.DictumId=dt1.Id
        WHERE (p1.SchoolYearId = @schoolYearId or @schoolYearId=0)  AND p1.ImprovementPlanTypeId IN ({0}) AND SUBSTRING(p1.CUE, 1, 2) IN ({1}) AND (ISNULL(@CUE, 0) = 0 OR p1.CUE = @CUE)
        AND ((@dependenceNational = 0 AND (NOT (p1.Dependence LIKE '%Nacional%') OR p1.Dependence is null)) OR (@dependenceNational = 1 AND p1.Dependence LIKE '%Nacional%'))
		GROUP BY p1.Id
		) AS prep
	LEFT JOIN Solicitudes AS sol ON sol.ImprovementPlanId = prep.Id 
	GROUP BY prep.Id
		,prep.totalAprobado
		,prep.totalDesestimado
        ,prep.totalDictaminado
        ,prep.totalDictaminadoInventariable
        ,prep.totalDictaminadoNoInventariable
		,prep.totalSinDictamen
		,prep.totalSinResolucion
        ,prep.totalRendidoBys
        ,prep.totalRendidoPyv
        ,prep.totalRendidoRrhh
	) AS subp
INNER JOIN ImprovementPlans AS p ON p.id = subp.Id
LEFT JOIN Provinces AS prov ON prov.Number = SUBSTRING(p.CUE, 1, 2)
left join UserProfile as usu on usu.UserId = p.EvaluatorUserId
ORDER BY p.id DESC
OPTION(RECOMPILE)";

                    sql = String.Format(sql, planTypes, string.Join(",", provNumbers.Select(x => "'" + x + "'")));
                    var plans = Context.Database.SqlQuery<ImprovementPlanReportDTO>(sql, new object[] {
                        new SqlParameter("@schoolYearId", schoolYearId),
                        new SqlParameter("@dependenceNational", dependenceNational ? 1 : 0),
                        new SqlParameter("@budgetYearId", budgetYearId),
                        new SqlParameter("@CUE", CUE ?? ""),
                        new SqlParameter("@solicitudeStatusAprobado", (int)SolicitudeStatusEnum.Aprobado),
                        new SqlParameter("@solicitudeStatusAnulado", (int)SolicitudeStatusEnum.Anulado),
                        new SqlParameter("@solicitudeStatusRechazado", (int)SolicitudeStatusEnum.Rechazado),
                        new SqlParameter("@solicitudeStatusElegible", (int)SolicitudeStatusEnum.Elegible),
                        new SqlParameter("@dictumEmitido", (int)DictumStatusEnum.Emitido),
                        new SqlParameter("@dictumSigned", (int)DictumStatusEnum.Firmado),
                        new SqlParameter("@resoEmitidoStatus", (int)ResolutionStatusEnum.Emitido),
                        new SqlParameter("@resoFirmadoStatus", (int)ResolutionStatusEnum.Firmado),
                        new SqlParameter("@resoProtocolizadoStatus", (int)ResolutionStatusEnum.Protocolizado),
                        new SqlParameter("@inventariable", (int)ExpenditureTypeEnum.GastoCapital),
                        new SqlParameter("@noInventariable", (int)ExpenditureTypeEnum.GastoCorriente)
                }).ToList();

                    ws.Cell(5, 1).InsertData(plans.Select(i => new
                    {
                        i.Provincia,
                        i.Expedientes,
                        i.TipoDePlan,
                        i.Axis,
                        i.Lines,
                        i.Summary,
                        i.FechaDeIngreso,
                        i.FechaIngresoCampo,
                        i.evaluatorName,
                        i.TotalSolicitado,
                        i.TotalSolicitadoCapital,
                        i.TotalSolicitadoCorriente,
                        i.NroDictamenes,
                        i.totalDictaminado,
                        i.FechaDictamenes,
                        i.totalDictaminadoCapital,
                        i.totalDictaminadoCorriente,
                        i.totalAnulado,
                        Rechazado = i.totalRechazado + i.totalDesestimado,
                        i.totalAprobado,
                        i.FechaResoluciones,
                        i.FechaAnexoResoluciones,
                        i.Resoluciones,
                        i.CUE,
                        i.InstitutionLevel,
                        i.Institucion,
                        i.Departamento,
                        i.Localidad,
                        i.CodPlan,
                        i.Estado,
                        i.totalRendidoBys,
                        i.totalRendidoPyv,
                        i.totalRendidoRrhh,
                        i.totalRendido,
                        i.Id
                    }).AsEnumerable());

                    if (abreviated == false)
                    {
                        // Totales de Lineas
                        var fieldSql = @"SELECT p1.Id AS ImprovementPlanId
                	,sol1.LineId AS LineId
                	,Inventariable = ISNULL((select sum(sol2.RequestedAmount * sol2.RequestedPriceUnit) from Solicitudes as sol2 where sol2.ImprovementPlanId = p1.Id and sol2.ExpenditureTypeId = @inventariable and sol2.LineId = sol1.LineId and sol2.StatusId <> @solicitudeStatusAnulado), 0)
                	,NoInventariable = ISNULL((select sum(sol3.RequestedAmount * sol3.RequestedPriceUnit) from Solicitudes as sol3 where sol3.ImprovementPlanId = p1.Id and sol3.ExpenditureTypeId = @noInventariable and sol3.LineId = sol1.LineId and sol3.StatusId <> @solicitudeStatusAnulado), 0)
                FROM ImprovementPlans AS p1
                LEFT JOIN Solicitudes AS sol1 ON sol1.ImprovementPlanId = p1.Id
                WHERE (p1.SchoolYearId = @schoolYearId or @schoolYearId=0)
                	AND p1.ImprovementPlanTypeId IN ({0}) 
                    AND SUBSTRING(p1.CUE, 1, 2) IN ({1})
                    AND (ISNULL(@CUE, 0) = 0 OR p1.CUE = @CUE)
                    AND ((@dependenceNational = 0 AND (NOT (p1.Dependence LIKE '%Nacional%') OR p1.Dependence is null)) OR (@dependenceNational = 1 AND p1.Dependence LIKE '%Nacional%'))
                GROUP BY p1.Id
                	,sol1.LineId
                ORDER BY p1.Id
                	,sol1.LineId
                OPTION(RECOMPILE)";

                        fieldSql = String.Format(fieldSql, planTypes, string.Join(",", provNumbers.Select(x => "'" + x + "'")));
                        var lineTotals = Context.Database.SqlQuery<LineTotalsReportDTO>(fieldSql, new object[] {
                    new SqlParameter("@schoolYearId", (int)schoolYearId),
                    new SqlParameter("@dependenceNational", dependenceNational ? 1 : 0),
                    new SqlParameter("@solicitudeStatusAnulado", (int)SolicitudeStatusEnum.Anulado),
                    new SqlParameter("@inventariable", (int)ExpenditureTypeEnum.GastoCapital),
                    new SqlParameter("@noInventariable", (int)ExpenditureTypeEnum.GastoCorriente),
                    new SqlParameter("@CUE", CUE ?? ""),
                }).ToList();
                        var linesIds = lineTotals.Select(x => x.LineId).Where(x => x.HasValue).Distinct().ToList();
                        var fieldsList = from fields in (from lines in Context.Lines where linesIds.Contains(lines.Id) orderby lines.Field.Code ascending select lines.Field).Distinct() select new { field = fields, lines = fields.Lines };

                        // Armado de grilla de totales de linea en mismo orden que los planes
                        var totalsByLine = new Dictionary<int, List<dynamic>>();

                        foreach (var lineId in linesIds)
                        {
                            var _column = new List<dynamic>();
                            foreach (var row in plans)
                            {
                                _column.Add(lineTotals.Where(x => x.ImprovementPlanId == row.Id && x.LineId == lineId).Select(y => y).FirstOrDefault());
                            }
                            totalsByLine.Add(lineId.Value, _column);
                        }

                        // Agregado columnas de ejes y lineas
                        var flag = true;
                        var flag2 = true;
                        var lastInserted = 17;
                        ws.Row(2).Height = 22;
                        ws.Row(3).Height = 22;
                        foreach (var row in fieldsList)
                        {
                            int _linesCount = row.lines.Count();
                            var _fh = ws.Column(lastInserted).InsertColumnsAfter(_linesCount * 2);
                            _fh.Style.NumberFormat.Format = "$ #,##0.00";
                            ws.Range(5, _fh.First().ColumnNumber(), plans.Count() + 4, _fh.Last().ColumnNumber()).Value = 0; // todos a cero por defecto
                            lastInserted = _fh.Last().ColumnNumber();
                            _fh.First().Cell(2).Value = row.field.Code + " - " + row.field.Description;
                            var _fRange = ws.Range(2, _fh.First().ColumnNumber(), 2, lastInserted).Merge();
                            _fh.First().Cell(2).Style.Fill.BackgroundColor = flag == true ? XLColor.BlueGray : XLColor.LightCoral;
                            _fh.First().Cell(2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            flag = !flag;
                            _fRange.Style.Font.Bold = true;
                            _fRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            int lCount = 0;
                            foreach (var line in row.lines)
                            {
                                var lCol = _fh.ElementAt(lCount);
                                lCol.Cell(3).Value = line.Code;
                                var _lRange = ws.Range(3, lCol.ColumnNumber(), 3, lCol.ColumnNumber() + 1).Merge();
                                _lRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                _lRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                _lRange.Style.Fill.BackgroundColor = flag2 == true ? XLColor.LightGray : XLColor.PeachPuff;
                                _lRange.Style.Font.Bold = true;
                                flag2 = !flag2;
                                lCol.Cell(4).Value = "Inventariable";
                                lCol.Cell(4).Style.Fill.BackgroundColor = XLColor.Green;
                                _fh.ElementAt(lCount + 1).Cell(4).Value = "No Inventariable";
                                _fh.ElementAt(lCount + 1).Cell(4).Style.Fill.BackgroundColor = XLColor.Orange;

                                if (totalsByLine.ContainsKey(line.Id))
                                {
                                    _fh.ElementAt(lCount).Cell(5).InsertData(totalsByLine[line.Id].Select(x => x != null ? x.Inventariable : 0).AsEnumerable());
                                }

                                if (totalsByLine.ContainsKey(line.Id))
                                {
                                    _fh.ElementAt(lCount + 1).Cell(5).InsertData(totalsByLine[line.Id].Select(x => x != null ? x.NoInventariable : 0).AsEnumerable());
                                }

                                lCount = lCount + 2;
                            }
                        }
                    }

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(stream);
                    stream.Position = 0;
                }
            } catch (Exception ex)
            {

            }
            return stream;
            
        }



        /// <summary>
        /// Exporta a un excel los planes de mejora de un determinado ciclo lectivo y categoría, abreviado!
        /// </summary>
        /// <param name="schoolYearId">Ciclo Lectivo de los planes a incluir</param>
        /// <param name="budgetYearId">Ciclo Lectivo de los montos ejecutados a incluir</param>
        /// <param name="planTypeIds">Tipos de plan a incluir</param>
        /// <param name="provNumbers">Provincias a incluir en el reporte</param>
        /// <param name="dependenceNational">Si es de dependencias nacionales</param>
        /// <param name="CUE">CUE de institucion a filtrar</param>
        
        /// <returns>Un stream correspondiente al archivo excel generado para el reporte</returns>
        public Stream GenerateImprovementPlansReport(int schoolYearId, int budgetYearId, string planTypeIds, List<string> provNumbers, bool dependenceNational = false, string CUE = null)

        { 
            var planTypes = String.Join(",", planTypeIds.Split(',').Select(x => Int32.Parse(x)));

            // Obtengo DataSource del reporte principal
            var schoolYear = Context.SchoolYears.Where(x => x.Id == schoolYearId).First();
            var budgetYearName = "";
            if (budgetYearId > 0)
            {
                var budgetYear = Context.SchoolYears.Where(x => x.Id == budgetYearId).First();
                budgetYearName = budgetYear.Cycle;
            }
            else
            {
                budgetYearName = "Todas";
            }

            MemoryStream stream = new MemoryStream();
            try { 
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = wb.Worksheets.Add("Reporte Planes");

                

                // Encabezados
                var table_heads = new List<String>();
                table_heads.Add("ID Sistema");
                table_heads.Add("Plan Articulador");
                table_heads.Add("Ciclo Lectivo");
                table_heads.Add("Jurisdiccion");
                table_heads.Add("Tipo de Plan");
                table_heads.Add("EJE");
                table_heads.Add("Linea/s");
                table_heads.Add("Descripción");
                table_heads.Add("CUE");
                table_heads.Add("Nº De Expedientes");
                table_heads.Add("Fecha Ingreso");
                table_heads.Add("Evaluador");
                table_heads.Add("Fecha Ing. a Eje");
                table_heads.Add("Total Solicitado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado Gastos de Capital");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado Gastos corrientes");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Total Solicitado Bienes y Servicios");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado RRHH");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado Viaticos");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Nº Dictámenes");
                table_heads.Add("Fecha Firma Dictámenes");

                table_heads.Add("Total Dictaminado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
          
                table_heads.Add("Total Dictaminado Gastos de Capital");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Dictaminado Gastos Corrientes");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Disposiciones");
                table_heads.Add("Fecha Firma Disposiciones");
                table_heads.Add("Fecha Firma Anexos");
                table_heads.Add("Total Aprobado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Aprobado Gastos de Capital");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Aprobado Gastos Corrientes");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Aprobado Solicitado Bienes y Servicios");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Aprobado Solicitado RRHH");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Aprobado Solicitado Viaticos");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                //table_heads.Add("Desestimado");
                //ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Anulado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Rechazo");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Total Rendido Bys");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Total Rendido Viaticos");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Total Rendido RRHH");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Total Rendido ");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";



                //table_heads.Add("Nivel");
                //table_heads.Add("Inst./Juris.");
                //table_heads.Add("Departamento");
                //table_heads.Add("Localidad");
                //table_heads.Add("Código de Plan");
                //table_heads.Add("Estado del Plan");
                //table_heads.Add("ID Sistema");

                var heads_row = new List<String[]>();
                heads_row.Add(table_heads.ToArray());
                var heads_row_range = ws.Cell(1, 1).InsertData(heads_row.ToArray());

                // Formato encabezados
                ws.Row(1).Style.Font.FontSize = 12;
                ws.Row(1).Height = 22;
                ws.SheetView.Freeze(1, 0);
                heads_row_range.Style.Font.Bold = true;
                heads_row_range.Style.Font.FontColor = XLColor.White;
                heads_row_range.Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;

                    string sql = @"
;WITH TotalesSolicitados AS (
    SELECT 
        ImprovementPlanId,
        SUM(RequestedAmount * RequestedPriceUnit) as totalSolicitado,
        SUM(CASE WHEN ExpenditureTypeId = @inventariable THEN RequestedAmount * RequestedPriceUnit ELSE 0 END) as totalSolicitadoCapital,
        SUM(CASE WHEN ExpenditureTypeId = @noInventariable THEN RequestedAmount * RequestedPriceUnit ELSE 0 END) as totalSolicitadoCorriente,
        SUM(CASE WHEN StatusId = @solicitudeStatusAnulado THEN RequestedAmount * RequestedPriceUnit ELSE 0 END) as totalAnulado,
        SUM(CASE WHEN StatusId = @solicitudeStatusRechazado THEN RequestedAmount * RequestedPriceUnit ELSE 0 END) as totalRechazado,
        SUM(CASE WHEN StatusId = @solicitudeStatusElegible THEN RequestedAmount * RequestedPriceUnit ELSE 0 END) as totalElegible,
        SUM(CASE WHEN ExpenditureObjectTypeId = 1 THEN RequestedAmount * RequestedPriceUnit ELSE 0 END) as totalSolicitadoBienesServicios,
        SUM(CASE WHEN ExpenditureObjectTypeId = 2 THEN RequestedAmount * RequestedPriceUnit ELSE 0 END) as totalSolicitadoViaticos,
        SUM(CASE WHEN ExpenditureObjectTypeId = 3 THEN RequestedAmount * RequestedPriceUnit ELSE 0 END) as totalSolicitadoRRHH
    FROM Solicitudes
    GROUP BY ImprovementPlanId
),
TotalesAprobadosDictaminados AS (
    SELECT 
        sol1.ImprovementPlanId,
        SUM(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as totalAprobado,
        SUM(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND (((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND sol1.ExpenditureTypeId = @inventariable) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as TotalAprobadoCapital,
        SUM(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND (((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND sol1.ExpenditureTypeId = @noinventariable) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as TotalAprobadoCorriente,
        SUM(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as totalDictaminado,
        SUM(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and sol1.ExpenditureTypeId = @inventariable THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as totalDictaminadoCapital,
        SUM(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and sol1.ExpenditureTypeId = @noInventariable THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as totalDictaminadoCorriente,
        SUM(CASE WHEN dt1.Id IS NULL AND sol1.StatusId = @solicitudeStatusAprobado THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as totalSinDictamen,
        SUM(CASE WHEN dt1.Id IS NOT NULL AND d1.StatusId NOT IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as totalSinResolucion,
        SUM(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN (sol1.RequestedAmount * sol1.RequestedPriceUnit) - (sol1.ApprovedAmount * sol1.ApprovedPriceUnit) ELSE 0 END) as totalDesestimado,
        SUM(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND (((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND sol1.ExpenditureTypeId = @noinventariable AND sol1.ExpenditureObjectTypeId=1) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as totalAprobadoBienesServicios,
        SUM(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND (((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND sol1.ExpenditureTypeId = @noinventariable AND sol1.ExpenditureObjectTypeId=2) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as totalAprobadoViaticos,
        SUM(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND (((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND sol1.ExpenditureTypeId = @noinventariable AND sol1.ExpenditureObjectTypeId=3) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END) as totalAprobadoRRHH
    FROM Solicitudes AS sol1
    LEFT JOIN Dictums_Solicitudes AS ds1 ON ds1.SolicitudeId = sol1.Id
    LEFT JOIN Documents AS dt1 ON ds1.DictumId = dt1.Id
    LEFT JOIN ResolutionDictums AS rs1 ON rs1.DictumId = ds1.DictumId
    LEFT JOIN Documents AS d1 ON d1.id = rs1.ResolutionId
    GROUP BY sol1.ImprovementPlanId
),
TotalesRendidos AS (
    SELECT 
        doc.ImprovementPlanId,
        SUM(CASE WHEN ac.ExpenditureObjectTypeID = 1 THEN ac.aprovedAmount ELSE 0 END) as totalRendidoBys,
        SUM(CASE WHEN ac.ExpenditureObjectTypeID = 2 THEN ac.aprovedAmount ELSE 0 END) as totalRendidoPyv,
        SUM(CASE WHEN ac.ExpenditureObjectTypeID = 3 THEN ac.aprovedAmount ELSE 0 END) as totalRendidoRrhh
    FROM AccountRendering ac
    INNER JOIN Documents doc ON ac.DictumId = doc.Id
    GROUP BY doc.ImprovementPlanId
)
SELECT 
    p.Id,
    Articulador = isNull(p.Articulator,(select isnull(articulator,'') from improvementplans as Art where Art.id=p.ParentId) ),
    prov.NAME as Provincia,
    p.ReceptionDate as FechaDeIngreso,
    TipoDePlan = (SELECT pt.Description FROM ImprovementPlansTypes AS pt WHERE pt.Id = p.ImprovementPlanTypeId),
    Axis = (select f.Code from Fields as f where f.Id = p.FieldId),
    Lines = STUFF((SELECT ', ' + lc.CODE FROM Solicitudes AS lc_sol INNER JOIN Lines AS lc ON lc.Id = lc_sol.LineId WHERE lc_sol.ImprovementPlanId = p.Id GROUP BY lc.CODE FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
    Summary = (Case len(p.Summary) When 0 then (select S.Code+'.' + L.Code from Lines_22 L inner join SubFields S on L.SubFieldId=S.Id and L.id=P.Line_22_Id ) else P.Summary End),
    p.FieldDate as FechaIngresoCampo,
    p.CUE,
    p.InstitutionName as Institucion,
    p.Identifier as CodPlan,
    ts.totalSolicitado,
    ts.totalSolicitadoCapital,
    ts.totalSolicitadoCorriente,
    ts.totalSolicitadoBienesServicios,
    ts.totalSolicitadoViaticos,
    ts.totalSolicitadoRRHH,
    ta.totalDictaminado,
    ta.totalDictaminadoCapital,
    ta.totalDictaminadoCorriente,
    ta.totalAprobado,
    ta.TotalAprobadoCapital,
    ta.TotalAprobadoCorriente,
    ta.totalDesestimado,
    ts.totalAnulado,
    ts.totalRechazado,
    ts.totalElegible,
    ta.totalSinDictamen,
    ta.totalSinResolucion,
    ta.totalAprobadoBienesServicios,
    ta.totalAprobadoViaticos,
    ta.totalAprobadoRRHH,
    FechaResoluciones = STUFF((SELECT ', ' + CONVERT(VARCHAR, rd_rdd.SignatureDate, 103) FROM Resolutions as rd_r INNER JOIN Documents as rd_rdd ON rd_rdd.Id = rd_r.Id INNER JOIN ResolutionDictums AS rd_rd ON rd_r.Id = rd_rd.ResolutionId inner join Documents as rd_dd on rd_dd.Id = rd_rd.DictumId and rd_dd.ImprovementPlanId = p.Id where rd_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
    FechaAnexoResoluciones = STUFF((SELECT ', ' + CONVERT(VARCHAR, rad_r.AnnexSignatureDate, 103) FROM Resolutions as rad_r INNER JOIN Documents as rad_rdd ON rad_rdd.Id = rad_r.Id INNER JOIN ResolutionDictums AS rad_rd ON rad_r.Id = rad_rd.ResolutionId inner join Documents as rad_dd on rad_dd.Id = rad_rd.DictumId and rad_dd.ImprovementPlanId = p.Id where rad_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
    Resoluciones = STUFF((SELECT ', ' + rn_r.ResolutionNumber FROM Resolutions as rn_r INNER JOIN Documents as rn_rdd ON rn_rdd.Id = rn_r.Id INNER JOIN ResolutionDictums AS rn_rd ON rn_r.Id = rn_rd.ResolutionId inner join Documents as rn_dd on rn_dd.Id = rn_rd.DictumId and rn_dd.ImprovementPlanId = p.Id where rn_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
    ISNULL(tr.totalRendidoBys, 0) as totalRendidoBys,
    ISNULL(tr.totalRendidoPyv, 0) as totalRendidoPyv,
    ISNULL(tr.totalRendidoRrhh, 0) as totalRendidoRrhh,
    FechaDictamenes = STUFF((SELECT ', ' + CONVERT(VARCHAR, dctSD.SignatureDate, 103) FROM (SELECT DISTINCT (dctSD_.SignatureDate) AS SignatureDate FROM Dictums AS dctSD_D INNER JOIN Documents AS dctSD_ ON dctSD_D.Id = dctSD_.Id AND dctSD_.ImprovementPlanId = p.Id ) AS dctSD FOR XML PATH(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
    NroDictamenes = STUFF((SELECT ', ' + dctNum_.DictumNumber FROM Dictums AS dctNum_ INNER JOIN Documents AS dctNum_D ON dctNum_D.Id = dctNum_.Id WHERE dctNum_D.ImprovementPlanId = p.Id FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
    TotalRendido = (ISNULL(tr.totalRendidoBys, 0) + ISNULL(tr.totalRendidoPyv, 0) + ISNULL(tr.totalRendidoRrhh, 0)),
    Expedientes = STUFF((SELECT ', ' + solFN.FileNumber FROM (SELECT DISTINCT FileNumber FROM Solicitudes WHERE ImprovementPlanId = p.Id) AS solFN FOR XML PATH(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
    evaluatorName = usu.Name + ' ' + usu.LastName
FROM ImprovementPlans p
LEFT JOIN TotalesSolicitados ts ON p.Id = ts.ImprovementPlanId
LEFT JOIN TotalesAprobadosDictaminados ta ON p.Id = ta.ImprovementPlanId
LEFT JOIN TotalesRendidos tr ON p.Id = tr.ImprovementPlanId
LEFT JOIN Provinces prov ON prov.Number = SUBSTRING(p.CUE, 1, 2)
LEFT JOIN UserProfile usu ON usu.UserId = p.EvaluatorUserId
WHERE p.SchoolYearId = @schoolYearId 
  AND p.ImprovementPlanTypeId IN ({0})
  AND SUBSTRING(p.CUE, 1, 2) IN ({1})
ORDER BY p.id DESC
OPTION(RECOMPILE)";

                    // CONTENIDO
                    //                    string sql = @"SELECT p.Id Id,
                    //    isNull(p.Articulator,(select isnull(articulator,'')
                    //	 from improvementplans as Art where Art.id=p.ParentId) )as Articulador
                    //   , prov.NAME as Provincia
                    //	,p.ReceptionDate as FechaDeIngreso
                    //	,TipoDePlan = (SELECT pt.Description FROM ImprovementPlansTypes AS pt WHERE pt.Id = p.ImprovementPlanTypeId)
                    //    ,Axis = (select f.Code from Fields as f where f.Id = p.FieldId)
                    //    ,Lines =  STUFF((SELECT ', ' + lc.CODE FROM Solicitudes AS lc_sol INNER JOIN Lines AS lc ON lc.Id = lc_sol.LineId WHERE lc_sol.ImprovementPlanId = p.Id GROUP BY lc.CODE FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    //    ,(Case len(P.Summary) When 0 then (select S.Code+'.' + L.Code from Lines_22 L inner join SubFields S on L.SubFieldId=S.Id  and L.id=P.Line_22_Id )  else  P.Summary End) as Summary
                    //	,p.CUE as CUE
                    //    ,p.InstitutionLevel as InstitutionLevel
                    //	,p.InstitutionName as Institucion
                    //	,p.Department as Departamento
                    //	,p.Location as Localidad
                    //	,p.Identifier as CodPlan
                    //	,Estado = (SELECT ps.Description FROM STATUS AS ps WHERE ps.Id = p.StatusId)
                    //	,subp.totalSolicitado as TotalSolicitado
                    //    ,subp.totalSolicitadoCapital as TotalSolicitadoCapital
                    //    ,subp.totalSolicitadoCorriente as TotalSolicitadoCorriente
                    //	,Expedientes = STUFF((SELECT ', ' + solFN.FileNumber FROM (SELECT DISTINCT (solFN_.FileNumber) AS FileNumber FROM Solicitudes AS solFN_ WHERE solFN_.ImprovementPlanId = p.Id) AS solFN FOR XML PATH(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    //	,p.FieldDate as FechaIngresoCampo
                    //    , CONCAT(usu.Name, ' ', usu.LastName) as evaluatorName
                    //	,FechaDictamenes = STUFF((SELECT ', ' + CONVERT(VARCHAR, dctSD.SignatureDate, 103) FROM (SELECT DISTINCT (dctSD_.SignatureDate) AS SignatureDate FROM Dictums AS dctSD_D INNER JOIN Documents AS dctSD_ ON dctSD_D.Id = dctSD_.Id AND dctSD_.ImprovementPlanId = p.Id ) AS dctSD FOR XML PATH(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    //	,NroDictamenes = STUFF((SELECT ', ' + dctNum_.DictumNumber FROM Dictums AS dctNum_ INNER JOIN Documents AS dctNum_D ON dctNum_D.Id = dctNum_.Id WHERE dctNum_D.ImprovementPlanId = p.Id FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    //	,subp.totalDictaminado
                    //    ,subp.totalDictaminadoCapital as totalDictaminadoCapital
                    //    ,subp.totalDictaminadoCorriente as totalDictaminadoCorriente
                    //    ,subp.totalAprobado
                    //    ,subp.TotalAprobadoCapital
                    //    ,subp.TotalAprobadoCorriente
                    //	,subp.totalDesestimado
                    //	,subp.totalAnulado
                    //	,subp.totalRechazado
                    //	,subp.totalElegible
                    //    ,FechaResoluciones = STUFF((SELECT ', ' + CONVERT(VARCHAR, rd_rdd.SignatureDate, 103) FROM Resolutions as rd_r INNER JOIN Documents as rd_rdd ON rd_rdd.Id = rd_r.Id INNER JOIN ResolutionDictums AS rd_rd ON rd_r.Id = rd_rd.ResolutionId inner join Documents as rd_dd on rd_dd.Id = rd_rd.DictumId and rd_dd.ImprovementPlanId = p.Id where rd_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    //    ,FechaAnexoResoluciones = STUFF((SELECT ', ' + CONVERT(VARCHAR, rad_r.AnnexSignatureDate, 103) FROM Resolutions as rad_r INNER JOIN Documents as rad_rdd ON rad_rdd.Id = rad_r.Id INNER JOIN ResolutionDictums AS rad_rd ON rad_r.Id = rad_rd.ResolutionId inner join Documents as rad_dd on rad_dd.Id = rad_rd.DictumId and rad_dd.ImprovementPlanId = p.Id where rad_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    //	,Resoluciones = STUFF((SELECT ', ' + rn_r.ResolutionNumber FROM Resolutions as rn_r INNER JOIN Documents as rn_rdd ON rn_rdd.Id = rn_r.Id INNER JOIN ResolutionDictums AS rn_rd ON rn_r.Id = rn_rd.ResolutionId inner join Documents as rn_dd on rn_dd.Id = rn_rd.DictumId and rn_dd.ImprovementPlanId = p.Id where rn_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    //    ,subp.totalSinDictamen
                    //	,subp.totalSinResolucion
                    //	,p.Id
                    //    ,subp.totalSolicitadoBienesServicios
                    //    ,subp.totalSolicitadoViaticos
                    //    ,subp.totalSolicitadoRRHH
                    //    ,subp.totalAprobadoBienesServicios
                    //    ,subp.totalAprobadoViaticos
                    //    ,subp.totalAprobadoRRHH
                    //    ,subp.totalRendidoBys
                    //    ,subp.totalRendidoPyv
                    //    ,subp.totalRendidoRrhh
                    //    ,(subp.totalRendidoBys + subp.totalRendidoPyv +subp.totalRendidoRrhh) as TotalRendido
                    //FROM (
                    //	SELECT prep.Id
                    //		,totalAprobado = ISNULL(prep.totalAprobado, 0)
                    //        ,totalAprobadoCapital = ISNULL(prep.totalAprobadoCapital, 0)
                    //        ,totalAprobadoCorriente = ISNULL(prep.totalAprobadoCorriente, 0)
                    //		,totalDesestimado = ISNULL(prep.totalDesestimado, 0)
                    //		,totalDictaminado = ISNULL(prep.totalDictaminado, 0)
                    //        ,totalDictaminadoCapital = ISNULL(prep.totalDictaminadoCapital, 0)
                    //        ,totalDictaminadoCorriente = ISNULL(prep.totalDictaminadoCorriente, 0)
                    //        ,totalSinDictamen = ISNULL(prep.totalSinDictamen, 0)
                    //		,totalSinResolucion = ISNULL(prep.totalSinResolucion, 0)
                    //		,totalSolicitado = SUM(sol.RequestedAmount * sol.RequestedPriceUnit)
                    //        ,totalSolicitadoCapital = sum(CASE WHEN sol.ExpenditureTypeId = @inventariable THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
                    //        ,totalSolicitadoCorriente = sum(CASE WHEN sol.ExpenditureTypeId = @noInventariable THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)

                    //		,totalAnulado = sum(CASE WHEN sol.StatusId = @solicitudeStatusAnulado THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
                    //		,totalRechazado = sum(CASE WHEN sol.StatusId = @solicitudeStatusRechazado THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
                    //		,totalElegible = sum(CASE WHEN sol.StatusId = @solicitudeStatusElegible THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
                    //        ,totalSolicitadoBienesServicios=  sum(CASE WHEN  sol.ExpenditureObjectTypeId=1 THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
                    //        ,totalSolicitadoViaticos=  sum(CASE WHEN  sol.ExpenditureObjectTypeId=2 THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
                    //        ,totalSolicitadoRRHH=  sum(CASE WHEN  sol.ExpenditureObjectTypeId=3 THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
                    //        ,totalAprobadoBienesServicios=ISNULL(prep.totalAprobadoBienesServicios, 0)
                    //        ,totalAprobadoViaticos=ISNULL(prep.totalAprobadoViaticos, 0)
                    //        ,totalAprobadoRRHH=ISNULL(prep.totalAprobadoRRHH, 0)
                    //        ,totalRendidoBys =ISNULL(prep.totalRendidoBys, 0)
                    //        ,totalRendidoPyv =ISNULL(prep.totalRendidoPyv, 0)
                    //        ,totalRendidoRrhh =ISNULL(prep.totalRendidoRrhh, 0)

                    //	FROM (
                    //		SELECT p1.id
                    //            ,totalAprobado          = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //			,totalAprobadoCapital   = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ( ( (@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND  sol1.ExpenditureTypeId = @inventariable) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //            ,totalAprobadoCorriente = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND  ( ( (@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND  sol1.ExpenditureTypeId = @noinventariable)  THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //            ,totalAprobadoBienesServicios =  sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND  ( ( (@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND  sol1.ExpenditureTypeId = @noinventariable AND sol1.ExpenditureObjectTypeId=1)  THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //            ,totalAprobadoViaticos =  sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND  ( ( (@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND  sol1.ExpenditureTypeId = @noinventariable AND sol1.ExpenditureObjectTypeId=2)  THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //            ,totalAprobadoRRHH =  sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND  ( ( (@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) AND  sol1.ExpenditureTypeId = @noinventariable AND sol1.ExpenditureObjectTypeId=3)  THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //            ,totalDesestimado = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol1.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN (sol1.RequestedAmount * sol1.RequestedPriceUnit) - sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //            ,totalDictaminado = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //            ,totalDictaminadoCapital = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and sol1.ExpenditureTypeId = @inventariable THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //            ,totalDictaminadoCorriente = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and sol1.ExpenditureTypeId = @noInventariable THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //			,totalSinDictamen = sum(CASE WHEN dt1.Id IS NULL AND sol1.StatusId = @solicitudeStatusAprobado THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //			,totalSinResolucion = sum(CASE WHEN dt1.Id IS NOT NULL AND d1.StatusId NOT IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) THEN sol1.ApprovedAmount * sol1.ApprovedPriceUnit ELSE 0 END)
                    //            ,totalRendidoBys    = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and  ac.AccountingRenderingID is not null and ac.ExpenditureObjectTypeID=1 THEN ac.aprovedAmount ELSE 0 END)
                    //		    ,totalRendidoPyv    = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and  ac.AccountingRenderingID is not null and ac.ExpenditureObjectTypeID=2 THEN ac.aprovedAmount ELSE 0 END)
                    //            ,totalRendidoRrhh    = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) and  ac.AccountingRenderingID is not null and ac.ExpenditureObjectTypeID=3 THEN ac.aprovedAmount ELSE 0 END)	
                    //FROM ImprovementPlans AS p1
                    //		LEFT JOIN Solicitudes AS sol1 ON sol1.ImprovementPlanId = p1.Id
                    //		LEFT JOIN Dictums_Solicitudes AS ds1 ON ds1.SolicitudeId = sol1.Id
                    //		LEFT JOIN Documents AS dt1 ON ds1.DictumId = dt1.Id
                    //		LEFT JOIN ResolutionDictums AS rs1 ON rs1.DictumId = ds1.DictumId
                    //		LEFT JOIN Documents AS d1 ON d1.id = rs1.ResolutionId
                    //        LEFT JOIN Resolutions as r on d1.Id = r.Id
                    //        LEFT JOIN AccountRendering ac on ac.DictumId=dt1.Id
                    //        WHERE p1.SchoolYearId = @schoolYearId AND p1.ImprovementPlanTypeId IN ({0}) AND SUBSTRING(p1.CUE, 1, 2) IN ({1}) AND (ISNULL(@CUE, 0) = 0 OR p1.CUE = @CUE)
                    //        AND ((@dependenceNational = 0 AND (NOT (p1.Dependence LIKE '%Nacional%') OR p1.Dependence is null)) OR (@dependenceNational = 1 AND p1.Dependence LIKE '%Nacional%'))
                    //		GROUP BY p1.Id
                    //		) AS prep
                    //	LEFT JOIN Solicitudes AS sol ON sol.ImprovementPlanId = prep.Id 
                    //	GROUP BY prep.Id
                    //		,prep.totalAprobado
                    //        ,prep.totalAprobadoCapital
                    //        ,prep.totalAprobadoCorriente
                    //		,prep.totalDesestimado
                    //        ,prep.totalDictaminado
                    //        ,prep.totalDictaminadoCapital
                    //        ,prep.totalDictaminadoCorriente
                    //		,prep.totalSinDictamen
                    //		,prep.totalSinResolucion
                    //        ,prep.totalAprobadoBienesServicios
                    //        ,prep.totalAprobadoViaticos
                    //        ,prep.totalAprobadoRRHH
                    //        ,prep.totalRendidoBys
                    //        ,prep.totalRendidoPyv
                    //        ,prep.TotalRendidoRrhh

                    //	) AS subp
                    //INNER JOIN ImprovementPlans AS p ON p.id = subp.Id
                    //LEFT JOIN Provinces AS prov ON prov.Number = SUBSTRING(p.CUE, 1, 2)
                    //left join UserProfile as usu on usu.UserId = p.EvaluatorUserId
                    //ORDER BY p.id DESC
                    //OPTION(RECOMPILE)";



                    sql = String.Format(sql, planTypes, string.Join(",", provNumbers.Select(x => "'" + x + "'")));
            
                    var plans = Context.Database.SqlQuery<ImprovementPlanReportDTO>(sql, new object[] {
                        new SqlParameter("@schoolYearId", schoolYearId),
                        new SqlParameter("@dependenceNational", dependenceNational ? 1 : 0),
                        new SqlParameter("@budgetYearId", budgetYearId),
                        new SqlParameter("@CUE", CUE ?? ""),
                        new SqlParameter("@solicitudeStatusAprobado", (int)SolicitudeStatusEnum.Aprobado),
                        new SqlParameter("@solicitudeStatusAnulado", (int)SolicitudeStatusEnum.Anulado),
                        new SqlParameter("@solicitudeStatusRechazado", (int)SolicitudeStatusEnum.Rechazado),
                        new SqlParameter("@solicitudeStatusElegible", (int)SolicitudeStatusEnum.Elegible),
                        new SqlParameter("@dictumEmitido", (int)DictumStatusEnum.Emitido),
                        new SqlParameter("@dictumSigned", (int)DictumStatusEnum.Firmado),
                        new SqlParameter("@resoEmitidoStatus", (int)ResolutionStatusEnum.Emitido),
                        new SqlParameter("@resoFirmadoStatus", (int)ResolutionStatusEnum.Firmado),
                        new SqlParameter("@resoProtocolizadoStatus", (int)ResolutionStatusEnum.Protocolizado),
                        new SqlParameter("@inventariable", (int)ExpenditureTypeEnum.GastoCapital),
                        new SqlParameter("@noInventariable", (int)ExpenditureTypeEnum.GastoCorriente)
                }).ToList();

                ws.Cell(2, 1).InsertData(plans.Select(i => new
                {
                    i.Id,
                    i.Articulador,
                    schoolYear.Cycle,
                    i.Provincia,
                    i.TipoDePlan,
                    i.Axis,
                    i.Lines,
                    i.Summary,
                    i.CUE,
                    i.Expedientes,
                    i.FechaDeIngreso,
                    i.evaluatorName,
                    i.FechaIngresoCampo,
                    i.TotalSolicitado,
                    i.TotalSolicitadoCapital,
                    i.TotalSolicitadoCorriente,
                    i.totalSolicitadoBienesServicios,
                    i.totalSolicitadoRRHH,
                    i.totalSolicitadoViaticos,
                    i.NroDictamenes,
                    i.FechaDictamenes,
                    i.totalDictaminado,
                    i.totalDictaminadoCapital,
                    i.totalDictaminadoCorriente,
                    i.Resoluciones,
                    i.FechaResoluciones,
                    i.FechaAnexoResoluciones,
                    i.totalAprobado,
                    i.totalAprobadoCapital,
                    i.totalAprobadoCorriente,
                    i.totalAprobadoBienesServicios,
                    i.totalAprobadoRRHH,
                    i.totalAprobadoViaticos,
                    i.totalAnulado,
                    Rechazado = i.totalRechazado + i.totalDesestimado,
                    i.totalRendidoBys,
                    i.totalRendidoPyv,
                    i.totalRendidoRrhh,
                    i.totalRendido

                  
                }).AsEnumerable());

               

                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
            }
            } catch (Exception ex)
            {

            }
            return stream;
        }

        #endregion

        /// <summary>
        /// Exporta a un excel los planes de mejora de un determinado ciclo lectivo y categoría
        /// </summary>
        /// <param name="CUE">CUE de institucion a filtrar</param>
        /// <param name="schoolYearId">Ciclo Lectivo de los planes a incluir</param>
        /// <param name="budgetYearId">Ciclo Lectivo de los montos ejecutados a incluir</param>
        /// <returns>Un stream correspondiente al archivo excel generado para el reporte</returns>
        public Stream GenerateCUESolicitudesReport(string CUE, int schoolYearId, int budgetYearId, List<string> provNumbers)
        {
            // Obtengo DataSource del reporte principal
            var  schollYearName = "";
            if (schoolYearId > 0)
            {
                var schoolYear = Context.SchoolYears.Where(x => x.Id == schoolYearId).First();
                schollYearName = schoolYear.Cycle;
            }
            else
                schollYearName = "Todos";

           var budgetYearName = "";
            if (budgetYearId > 0)
            {
                var budgetYear = Context.SchoolYears.Where(x => x.Id == budgetYearId).First();
                budgetYearName = budgetYear.Cycle;
            }
            else
            {
                budgetYearName = "Todas";
            }
           
           
            MemoryStream stream = new MemoryStream();
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = wb.Worksheets.Add("Reporte Solicitados de CUE");
                // Titulo planilla
                //ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(1));

                //ws.Cell(1, 1).Value = "CICLO LECTIVO";
                //ws.Cell(1, 2).Value = schoolYear.Cycle;
                //ws.Cell(1, 3).Value = "PARTIDA";
                //ws.Cell(1, 4).Value = budgetYearName;

                // Encabezados
                var table_heads = new List<String>();
                table_heads.Add("ID Plan");
                table_heads.Add("ID Solicitado");
                table_heads.Add("Fecha Ingreso");
                table_heads.Add("CUE");
                table_heads.Add("Ciclo Lectivo");
                table_heads.Add("Jurisdiccion");
                table_heads.Add("Tipo de Plan");
                table_heads.Add("EJE");
                table_heads.Add("Linea/s");
                table_heads.Add("Descripción");
                table_heads.Add("Evaluador");

                table_heads.Add("Fecha Ing. a Eje");
                table_heads.Add("Detalle Solicitado");
                table_heads.Add("Total Solicitado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Total Solicitado Bienes/Servicios");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado Pasajes y Viaticos");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Solicitado RRHH");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Nº Dictámenes");
                table_heads.Add("Total Dictaminado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Fecha Firma Dictámenes");
                table_heads.Add("Exp");

                table_heads.Add("Total Aprobado");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Total Aprobado Bienes/Servicios");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Aprobado Pasajes y Viaticos");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Aprobado RRHH");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Disposiciones");

                table_heads.Add("Fecha Firma Disposiciones");
                table_heads.Add("Fecha Firma Anexos");
                table_heads.Add("Tipo de Gasto");
                table_heads.Add("Total Aprobado Capital");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Total Aprobado Corriente");

                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Rendido Bienes y Servicios");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Rendido RRHH");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Rendido Viaticos");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("TOTAL RENDIDO");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                table_heads.Add("Rendido Gastos de Capital");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("REndido Gastos Corrientes");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("Nombre");
                table_heads.Add("Gestion");
                table_heads.Add("Modalidad");
                table_heads.Add("Clase");



                var heads_row = new List<String[]>();
                heads_row.Add(table_heads.ToArray());
                var heads_row_range = ws.Cell(1, 1).InsertData(heads_row.ToArray());

                // Formato encabezados
                ws.Row(1).Style.Font.FontSize = 12;
                ws.Row(1).Height = 22;
                ws.SheetView.Freeze(1, 0);
                heads_row_range.Style.Font.Bold = true;
                heads_row_range.Style.Font.FontColor = XLColor.White;
                heads_row_range.Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;

                // CONTENIDO
                string sql = @"SELECT p.Id IdPlan,subp.Id as Id, prov.NAME as Provincia
	,p.ReceptionDate as FechaDeIngreso
	,TipoDePlan = (SELECT pt.Description FROM ImprovementPlansTypes AS pt WHERE pt.Id = p.ImprovementPlanTypeId)
    ,Axis = (select f.Code from Fields as f where f.Id = p.FieldId)
    ,Lines =  STUFF((SELECT ', ' + lc.CODE FROM Solicitudes AS lc_sol INNER JOIN Lines AS lc ON lc.Id = lc_sol.LineId WHERE lc_sol.ImprovementPlanId = p.Id GROUP BY lc.CODE FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
    ,p.Summary
    ,p.ReceptionDate as FechaDeIngreso
	,subp.CUE as CUE
    ,subp.Level as Level
	,p.InstitutionName as Institucion
	,p.Department as Departamento
	,p.Location as Localidad
	,p.Identifier as CodPlan
    ,TipoGasto=(select f.Description from ExpenditureTypes f where id=subp.ExpenditureTypeId)
	,Estado = (SELECT ps.Description FROM STATUS AS ps WHERE ps.Id = p.StatusId)
    ,subp.Details as Details
	,subp.totalSolicitado as TotalSolicitado
	,Expedientes = subp.FileNumber
	,p.FieldDate as FechaIngresoCampo
	,FechaDictamenes = STUFF((SELECT ', ' + CONVERT(VARCHAR, dctSD.SignatureDate, 103) FROM (SELECT DISTINCT (dctSD_DD.SignatureDate) AS SignatureDate FROM Dictums AS dctSD_D INNER JOIN Dictums_Solicitudes AS dctSD_ ON dctSD_D.Id = dctSD_.DictumId AND dctSD_.SolicitudeId = subp.Id INNER JOIN Documents AS dctSD_DD ON dctSD_DD.Id = dctSD_D.Id ) AS dctSD FOR XML PATH(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	,NroDictamenes = STUFF((SELECT ', ' + dctNum_.DictumNumber FROM Dictums AS dctNum_ INNER JOIN Dictums_Solicitudes AS dctNum_D ON dctNum_D.DictumId = dctNum_.Id AND dctNum_D.SolicitudeId = subp.Id FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	,subp.totalDictaminado
    ,subp.totalAprobado
    ,subp.totalAprobadoCapital
    ,subp.totalAprobadoCorriente
    ,FechaResoluciones = STUFF((SELECT ', ' + CONVERT(VARCHAR, rd_rdd.SignatureDate, 103) FROM Resolutions as rd_r INNER JOIN Documents as rd_rdd ON rd_rdd.Id = rd_r.Id INNER JOIN ResolutionDictums AS rd_rd ON rd_r.Id = rd_rd.ResolutionId inner join Documents as rd_dd on rd_dd.Id = rd_rd.DictumId and rd_dd.ImprovementPlanId = p.Id where rd_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
    ,FechaAnexoResoluciones = STUFF((SELECT ', ' + CONVERT(VARCHAR, rad_r.AnnexSignatureDate, 103) FROM Resolutions as rad_r INNER JOIN Documents as rad_rdd ON rad_rdd.Id = rad_r.Id INNER JOIN ResolutionDictums AS rad_rd ON rad_r.Id = rad_rd.ResolutionId inner join Documents as rad_dd on rad_dd.Id = rad_rd.DictumId and rad_dd.ImprovementPlanId = p.Id where rad_rdd.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) FOR XML PATH('') ,TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
    , CONCAT(usu.Name, ' ', usu.LastName) as evaluatorName
	,subp.totalAnulado
	,(subp.totalRechazado + subp.totalDesestimado) as totalRechazado
	,subp.totalElegible
	,Resoluciones = STUFF((SELECT ', ' + resNum_.ResolutionNumber FROM Resolutions AS resNum_ 
        INNER JOIN ResolutionDictums AS resNum_D ON resNum_D.ResolutionId = resNum_.Id 
        INNER JOIN Dictums_Solicitudes AS dctNum_RD ON dctNum_RD.DictumId = resNum_D.DictumId AND dctNum_RD.SolicitudeId = subp.Id
        FOR XML PATH(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
    ,subp.totalSinDictamen
	,subp.totalSinResolucion
	,subp.Id as Id
    ,subp.totalSolicitadoByS as totalSolicitado_Bienes_Servicios
    ,subp.totalSolicitadoPyV as totalSolicitado_Viaticos
    ,subp.totalSolicitadoRRHH as totalSolicitado_RRHH
    ,subp.totalAprobadoByS as totalAprobado_Bienes_Servicios
    ,subp.TotalAprobadoPyV as totalAprobado_Viaticos
    ,subp.TotalAprobadoRRHH as totalAprobado_RRHH
    ,y.Cycle as CicloLectivo
    , '' as Nombre
    , '' as Gestion
    , '' as Modalidad
    , '' as Clase
FROM (
	SELECT sol.Id as Id
        ,sol.FileNumber as FileNumber
        ,sol.Details as Details
        ,sol.ImprovementPlanId as ImprovementPlanId
        ,sol.CUE as CUE
        ,sol.Level as Level
        ,sol.ExpenditureTypeId as ExpenditureTypeId
		,totalAprobado          = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
        ,totalAprobadoCapital   = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND (sol.ExpenditureTypeId=2 AND (sol.SchoolYearId = @budgetYearId   OR @budgetYearId = 0 )) THEN sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
        ,totalAprobadoCorriente = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND (sol.ExpenditureTypeId=1 AND (sol.SchoolYearId = @budgetYearId   OR @budgetYearId = 0 )) THEN sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
        ,totalAprobadoByS =         sum(CASE WHEN sol.ExpenditureObjectTypeId=1 AND d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
        ,totalAprobadoPyV =         sum(CASE WHEN sol.ExpenditureObjectTypeId=2 AND d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
        ,totalAprobadoRRHH =         sum(CASE WHEN sol.ExpenditureObjectTypeId=3 AND d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
        ,totalDesestimado       = sum(CASE WHEN d1.StatusId IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) AND ((@budgetYearId > 0 AND sol.SchoolYearId = @budgetYearId) OR @budgetYearId = 0) THEN (sol.RequestedAmount * sol.RequestedPriceUnit) - sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
        ,totalDictaminado       = sum(CASE WHEN dt1.Id IS NOT NULL AND (dt1.StatusId = @dictumEmitido OR dt1.StatusId = @dictumSigned) THEN sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
        ,totalSinDictamen       = sum(CASE WHEN dt1.Id IS NULL AND sol.StatusId = @solicitudeStatusAprobado THEN sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
        ,totalSinResolucion = sum(CASE WHEN dt1.Id IS NOT NULL AND d1.StatusId NOT IN (@resoEmitidoStatus,@resoFirmadoStatus,@resoProtocolizadoStatus) THEN sol.ApprovedAmount * sol.ApprovedPriceUnit ELSE 0 END)
		,totalSolicitado = SUM(sol.RequestedAmount * sol.RequestedPriceUnit)
        ,totalSolicitadoByS=SUM( CASE WHEN sol.ExpenditureObjectTypeId=1 THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
	    ,totalSolicitadoPyV=SUM( CASE WHEN sol.ExpenditureObjectTypeId=2 THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
        ,totalSolicitadoRRHH=SUM( CASE WHEN sol.ExpenditureObjectTypeId=3 THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)

        ,totalAnulado = sum(CASE WHEN sol.StatusId = @solicitudeStatusAnulado THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
		,totalRechazado = sum(CASE WHEN sol.StatusId = @solicitudeStatusRechazado THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
		,totalElegible = sum(CASE WHEN sol.StatusId = @solicitudeStatusElegible THEN sol.RequestedAmount * sol.RequestedPriceUnit ELSE 0 END)
	FROM Solicitudes AS sol
        LEFT JOIN Dictums_Solicitudes AS ds1 ON ds1.SolicitudeId = sol.Id
	    LEFT JOIN Documents AS dt1 ON ds1.DictumId = dt1.Id
	    LEFT JOIN ResolutionDictums AS rs1 ON rs1.DictumId = ds1.DictumId
	    LEFT JOIN Documents AS d1 ON d1.id = rs1.ResolutionId
         
    WHERE ( sol.CUE = @CUE    OR @CUE='0' ) AND ( sol.SchoolYearId = @schoolYearId or  @schoolYearId=0 )
	GROUP BY sol.Id, sol.FileNumber, sol.Details, sol.ImprovementPlanId, sol.CUE, sol.Level,sol.ExpenditureTypeId
	) AS subp
INNER JOIN ImprovementPlans AS p ON p.id = subp.ImprovementPlanId
INNER JOIN SchoolYears as y on y.id=p.SchoolYearId
LEFT JOIN Provinces AS prov ON prov.Number = SUBSTRING(p.CUE, 1, 2)
LEFT JOIN UserProfile as usu on usu.UserId = p.EvaluatorUserId
WHERE SUBSTRING(subp.CUE, 1, 2) IN ({0}) 
ORDER BY subp.id DESC

OPTION(RECOMPILE)";

                sql = String.Format(sql,string.Join(",", provNumbers.Select(x => "'" + x + "'")));

                var plans = Context.Database.SqlQuery<CueSolicitudesReportDTO>(sql, new object[] {
                        new SqlParameter("@schoolYearId", schoolYearId),
                        new SqlParameter("@budgetYearId", budgetYearId),
                        new SqlParameter("@CUE", CUE),
                        new SqlParameter("@solicitudeStatusAprobado", (int)SolicitudeStatusEnum.Aprobado),
                        new SqlParameter("@solicitudeStatusAnulado", (int)SolicitudeStatusEnum.Anulado),
                        new SqlParameter("@solicitudeStatusRechazado", (int)SolicitudeStatusEnum.Rechazado),
                        new SqlParameter("@solicitudeStatusElegible", (int)SolicitudeStatusEnum.Elegible),
                        new SqlParameter("@dictumEmitido", (int)DictumStatusEnum.Emitido),
                        new SqlParameter("@dictumSigned", (int)DictumStatusEnum.Firmado),
                        new SqlParameter("@resoEmitidoStatus", (int)ResolutionStatusEnum.Emitido),
                        new SqlParameter("@resoFirmadoStatus", (int)ResolutionStatusEnum.Firmado),
                        new SqlParameter("@resoProtocolizadoStatus", (int)ResolutionStatusEnum.Protocolizado),
                        new SqlParameter("@inventariable", (int)ExpenditureTypeEnum.GastoCapital),
                        new SqlParameter("@noInventariable", (int)ExpenditureTypeEnum.GastoCorriente)
                }).ToList();



                ws.Cell(2, 1).InsertData(plans.Select(i => new
                {
                    i.IdPlan,
                    i.Id,
                    i.FechaDeIngreso,
                    i.CUE,
                    i.CicloLectivo,
                    i.Provincia,
                    i.TipoDePlan,
                    i.Axis,
                    i.Lines,
                     i.Summary,
                   
                    i.evaluatorName,
                    i.FechaIngresoCampo,
                    i.Details,
                    i.TotalSolicitado,
                    i.totalSolicitado_Bienes_Servicios,
                    i.totalSolicitado_Viaticos,
                    i.totalSolicitado_RRHH,
                    i.NroDictamenes,
                    i.totalDictaminado,
                  
                    i.FechaDictamenes,
                    i.Expedientes,
                    i.totalAprobado,
                    i.totalAprobado_Bienes_Servicios,
                    i.totalAprobado_Viaticos,
                    i.totalAprobado_RRHH,
                    i.Resoluciones,
                    i.FechaResoluciones,
                    i.FechaAnexoResoluciones,
                    i.TipoGasto,
                    i.totalAprobadoCapital,
                    i.totalAprobadoCorriente,
                    S = AccountImprovementPlanAmount(i.IdPlan,1),
                    V = AccountImprovementPlanAmount(i.IdPlan, 2),
                    R = AccountImprovementPlanAmount(i.IdPlan, 3),
                    T = AccountImprovementPlanAmount(i.IdPlan, 0),
                    RC=AccountImprovementPlanAmount(i.IdPlan, 1),
                    RCC=AccountImprovementPlanAmount(i.IdPlan, 2) + AccountImprovementPlanAmount(i.IdPlan, 3),
                    i.Nombre,
                    i.Gestion,
                    i.Modalidad,
                    i.Clase
                  

                }).AsEnumerable());


              
               
                // SIA 01-10-2021 ocultamos todo
                //// Totales de Lineas
                //var fieldSql = @"SELECT sol1.Id AS ImprovementPlanId
                //	,sol1.LineId AS LineId
                //	,Inventariable = CASE WHEN sol1.ExpenditureTypeId = @inventariable and sol1.StatusId <> @solicitudeStatusAnulado THEN sol1.RequestedAmount * sol1.RequestedPriceUnit ELSE 0 END
                //    ,NoInventariable = CASE WHEN sol1.ExpenditureTypeId = @noInventariable and sol1.StatusId <> @solicitudeStatusAnulado THEN sol1.RequestedAmount * sol1.RequestedPriceUnit ELSE 0 END
                //FROM Solicitudes AS sol1
                //    INNER JOIN ImprovementPlans as p1 ON p1.Id = sol1.ImprovementPlanId
                //WHERE p1.SchoolYearId = @schoolYearId AND sol1.CUE = @CUE
                //ORDER BY sol1.Id, sol1.LineId
                //OPTION(RECOMPILE)";

                //var lineTotals = Context.Database.SqlQuery<LineTotalsReportDTO>(fieldSql, new object[] {
                //    new SqlParameter("@schoolYearId", (int)schoolYearId),
                //    new SqlParameter("@solicitudeStatusAnulado", (int)SolicitudeStatusEnum.Anulado),
                //    new SqlParameter("@inventariable", (int)ExpenditureTypeEnum.GastoCapital),
                //    new SqlParameter("@noInventariable", (int)ExpenditureTypeEnum.GastoCorriente),
                //    new SqlParameter("@CUE", CUE ?? ""),
                //}).ToList();
                //var linesIds = lineTotals.Select(x => x.LineId).Where(x => x.HasValue).Distinct().ToList();
                //var fieldsList = from fields in (from lines in Context.Lines where linesIds.Contains(lines.Id) orderby lines.Field.Code ascending select lines.Field).Distinct() select new { field = fields, lines = fields.Lines };

                //// Armado de grilla de totales de linea en mismo orden que los planes
                //var totalsByLine = new Dictionary<int, List<dynamic>>();

                //foreach (var lineId in linesIds)
                //{
                //    var _column = new List<dynamic>();
                //    foreach (var row in plans)
                //    {
                //        _column.Add(lineTotals.Where(x => x.ImprovementPlanId == row.Id && x.LineId == lineId).Select(y => y).FirstOrDefault());
                //    }
                //    totalsByLine.Add(lineId.Value, _column);
                //}

                //// Agregado columnas de ejes y lineas
                //var flag = true;
                //var flag2 = true;
                //var lastInserted = 14;
                //ws.Row(2).Height = 22;
                //ws.Row(3).Height = 22;
                //foreach (var row in fieldsList)
                //{
                //    int _linesCount = row.lines.Count();
                //    var _fh = ws.Column(lastInserted).InsertColumnsAfter(_linesCount * 2);
                //    _fh.Style.NumberFormat.Format = "$ #,##0.00";
                //    ws.Range(5, _fh.First().ColumnNumber(), plans.Count() + 4, _fh.Last().ColumnNumber()).Value = 0; // todos a cero por defecto
                //    lastInserted = _fh.Last().ColumnNumber();
                //    _fh.First().Cell(2).Value = row.field.Code + " - " + row.field.Description;
                //    var _fRange = ws.Range(2, _fh.First().ColumnNumber(), 2, lastInserted).Merge();
                //    _fh.First().Cell(2).Style.Fill.BackgroundColor = flag == true ? XLColor.BlueGray : XLColor.LightCoral;
                //    _fh.First().Cell(2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                //    flag = !flag;
                //    _fRange.Style.Font.Bold = true;
                //    _fRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //    int lCount = 0;
                //    foreach (var line in row.lines)
                //    {
                //        var lCol = _fh.ElementAt(lCount);
                //        lCol.Cell(3).Value = line.Code;
                //        var _lRange = ws.Range(3, lCol.ColumnNumber(), 3, lCol.ColumnNumber() + 1).Merge();
                //        _lRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                //        _lRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                //        _lRange.Style.Fill.BackgroundColor = flag2 == true ? XLColor.LightGray : XLColor.PeachPuff;
                //        _lRange.Style.Font.Bold = true;
                //        flag2 = !flag2;
                //        lCol.Cell(4).Value = "Inventariable";
                //        lCol.Cell(4).Style.Fill.BackgroundColor = XLColor.Green;
                //        _fh.ElementAt(lCount + 1).Cell(4).Value = "No Inventariable";
                //        _fh.ElementAt(lCount + 1).Cell(4).Style.Fill.BackgroundColor = XLColor.Orange;

                //        if (totalsByLine.ContainsKey(line.Id))
                //        {
                //            _fh.ElementAt(lCount).Cell(5).InsertData(totalsByLine[line.Id].Select(x => x != null ? x.Inventariable : 0).AsEnumerable());
                //        }

                //        if (totalsByLine.ContainsKey(line.Id))
                //        {
                //            _fh.ElementAt(lCount + 1).Cell(5).InsertData(totalsByLine[line.Id].Select(x => x != null ? x.NoInventariable : 0).AsEnumerable());
                //        }

                //        lCount = lCount + 2;
                //    }
                //}

                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
            }

            return stream;
        }
        public decimal AccountImprovementPlanAmount(int id_plan, int idTipo)
        {
            var accountRendering = (from D in Context.AccountRenderings
                                    where D.ImprovementPlanId == id_plan
                                    && (D.ExpenditureObjectTypeId == idTipo || idTipo == 0)
                                    select D).ToList();
            if (accountRendering.Count > 0)
            {
                return accountRendering.Sum(x => x.AprovedAmount);
            }
            else
            {
                return 0;
            }
        }
        public Stream generateAccountRenderingReport(int schoolYearId)
        {


            // datos generales reporte
            //var schoolYear = Context.SchoolYears.Where(x => x.Id == schoolYearId).First();
            //string budgetYearName = "";
            //if (budgetYearId > 0)
            //{
            //    var budgetYear = Context.SchoolYears.Where(x => x.Id == budgetYearId).First();
            //    budgetYearName = budgetYear.Cycle;
            //}
            //else
            //{
            //    budgetYearName = "Todas";
            //}

            MemoryStream stream = new MemoryStream();
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = wb.Worksheets.Add("Rendiciones");

                // Titulo planilla
                //ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(1));
                ws.Row(1).Style.Font.FontSize = 12;
                ws.Row(1).Height = 22;
                ws.Row(1).Style.Font.Bold = true;
                ws.Row(1).Style.Font.FontColor = XLColor.White;
                ws.Row(1).Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;
              
                ws.Column(12).Style.NumberFormat.Format = "$ #,##0.00";
                //Encabezados
                ws.Cell(1, 1).Value = "ID PLAN";
                ws.Cell(1, 2).Value = "ID RENDICION";
                ws.Cell(1, 3).Value = "FECHA";
                ws.Cell(1, 4).Value = "CUE-PLAN";
                ws.Cell(1, 5).Value = "CICLO";

                ws.Cell(1, 6).Value = "TIPO PLAN";
                ws.Cell(1, 7).Value = "EJE";
                ws.Cell(1, 8).Value = "LINEA";
                ws.Cell(1, 9).Value = "DICTAMEN";
                ws.Cell(1, 10).Value = "RENDIDO_BYS";
                ws.Cell(1, 11).Value = "RENDIDO_RRHH";
                ws.Cell(1, 12).Value = "RENDIDO_VIATICO";
               
              
                // Calculo Lineas
                // lineas
               
                int I = 2;


try

                {
                    List<AccountRendering> rows = new List<AccountRendering>() ;

                    if (schoolYearId == 0)
                    {
                        rows = (from ac in Context.AccountRenderings
                                select ac).ToList();
                    }
                    else
                    {
                        rows = (from ac in Context.AccountRenderings
                                from y in Context.SchoolYears
                                where ac.Date.Year.ToString() == y.Cycle
                                && y.Id == schoolYearId
                                select ac).ToList();
                    }






                    foreach (var r in rows.ToList())
                    {
                       ws.Cell(I, 1).Value = r.ImprovementPlanId;
                       ws.Cell(I, 2).Value = r.Number;
                       ws.Cell(I, 3).Value = r.Date.ToShortDateString();
                       ws.Cell(I, 4).Value = r.ImprovementPlan.CUE;
                       ws.Cell(I, 5).Value = r.ImprovementPlan.SchoolYear.Description;
                       ws.Cell(I, 6).Value = r.ImprovementPlan.ImprovementPlansType.Description;
                       ws.Cell(I, 7).Value = r.ImprovementPlan.Field!= null? r.ImprovementPlan.Field.Description:" ";
                       ws.Cell(I, 8).Value = r.ImprovementPlan.Line!= null ?r.ImprovementPlan.Line.Description :" ";
                       ws.Cell(I, 9).Value = r.Dictum.Number.ToString();
                       if (r.ExpenditureObjectTypeId==1)
                       {
                        ws.Cell(I, 10).Value = r.AprovedAmount;
                        ws.Cell(I, 11).Value = 0;
                        ws.Cell(I, 12).Value = 0;
                       } else if (r.ExpenditureObjectTypeId == 2)
                       {
                        ws.Cell(I, 10).Value = 0;
                        ws.Cell(I, 11).Value = r.AprovedAmount;
                        ws.Cell(I, 12).Value = 0;
                        } else if (r.ExpenditureObjectTypeId == 3)
                       {
                        ws.Cell(I, 10).Value = 0;
                        ws.Cell(I, 11).Value = 0;
                        ws.Cell(I, 12).Value = r.AprovedAmount;
                        }

                     

                        I = I + 1;
                    }

                   

                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
              

                } catch (Exception ex)
                { }
                }
            return stream;
           
           

           

        }








    }
}