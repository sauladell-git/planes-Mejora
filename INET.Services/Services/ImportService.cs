using Excel;
using INET.Core.Enums;
using INET.Data;
using INET.Services.DTO;
using INET.Utils.Helpers;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace INET.Services
{
    /// <summary>
    /// Servicio encargado de importar los archivos excels como planes de mejora
    /// </summary>
    public class ImportService : BusinessService
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private FileNumberService _fileNumberService;
        public bool flag_cue_vacio = false;
        public ImportService(INETContext context, FileNumberService fileNumberService)
        {
            Context = context;
            _fileNumberService = fileNumberService;
        }

        /// <summary>
        /// Importa documentos excels como planes de mejora
        /// </summary>
        /// <param name="inputPath">Path de la carpeta en la que se encuentran los archivos excels a importar</param>        
        /// <returns>Un objeto ImportResultDTO con el resultado de la importación</returns>
        public ImportResultDTO Import(string inputPath)
        {
            var di = new DirectoryInfo(inputPath);
            var log = LogManager.GetLogger("LogToImportFile");
            var logEvaluators = LogManager.GetLogger("LogEvaluatorsToFile");
            var errors = 0;
            var processed = 0;

            // Inicializo los logs
            log.Info("====== Comenzando importación de datos ======");
            logEvaluators.Info("====== Comenzando logueo de evaluadores ======");

            foreach (var file in di.GetFiles())
            {
                try
                {
                    // Evito procesar los archivos temporales
                    if (file.Name.Length > 2 && file.Name.Substring(0, 2) == "~$")
                        continue;

                    Console.WriteLine(String.Concat("Procesando ", file.Name));

                    var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read);
                    var excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                    excelReader.IsFirstRowAsColumnNames = true;
                    var result = excelReader.AsDataSet();

                    try
                    {
                        // Obtengo los datos del Plan de Mejora
                        var rowSheet1 = result.Tables[0].Rows[0];

                        var plan = new ImprovementPlan();
                        plan.Id = di.GetFiles().ToList().IndexOf(file);
                        plan.ImprovementPlanTypeId = Context.ImprovementPlansTypes.AsEnumerable().Where(x => x.Description == rowSheet1[0].ToString()).First().Id;
                        plan.SchoolYearId = GetSchoolYearId(rowSheet1[1].ToString());
                        plan.CUE = GetImprovementPlanCUE(plan.ImprovementPlanTypeId, rowSheet1[2].ToString(), result);
                        plan.ReceptionDate = Convert.ToDateTime(rowSheet1[3].ToString());
                        plan.Summary = rowSheet1[4].ToString();
                        plan.FieldId = GetFieldId(rowSheet1[6].ToString());
                        plan.StatusId = StageStatusEnum.EnIngreso.ToInt();
                        plan.Identifier = String.Format("{0}-{1}-{2}-{3}", DateTime.Now.Year, plan.FieldId, plan.CUE, _fileNumberService.LastFileNumber());

                        // Obtengo del excel los datos de los solicitados asociados al plan
                        var solicitudes = ReadSolicitudes(result.Tables[1].Rows,plan.Line_22_Id.Value,plan.SchoolYearId, plan.StatusId);
                        
                        foreach (var solicitude in solicitudes)
                            plan.Solicitudes.Add(solicitude);

                        excelReader.Close();
                        Context.ImprovementPlans.Add(plan);
                        Context.SaveChanges();
                        processed++;
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        if (!excelReader.IsClosed)
                            excelReader.Close();

                        stream.Close();
                        log.Error(String.Format("Ocurrió un error procesando el archivo {0}", file.Name), ex);
                    }
                }
                catch (Exception ex)
                {
                    errors++;
                    log.Error(String.Format("Ocurrió un error procesando el archivo {0}", file.Name), ex);
                }
            }

            // Finalizo los logs
            log.Info(string.Format("Se procesaron {0} archivo(s) con {1} error(es)", processed, errors));
            log.Info("Importación de datos finalizada");
            logEvaluators.Info("Logueo de evaluadores finalizado");

            return new ImportResultDTO() { Errors = errors, Processed = processed };
        }

        /// <summary>
        /// Importa las solicitudes de un archivo Excel, Basado en Resu 2022
        /// </summary>
        /// <param name="stream">Stream del archivo a importar</param>
        /// <param name="line22Id">Id del eje</param>
        /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
        /// <param name="extension"></param>
        /// <param name="schoolYearId"></param>
        /// <returns>Un listado con una colección de solicitados correspondientes a los registros del Excel</returns>
        public List<Solicitude> ImportSolicitudes(Stream stream, int line22Id,int schoolYearId, int improvementPlanStatusId, string extension)
        {
            var lst = new List<Solicitude>();
           
            IExcelDataReader excelReader;
            if (extension.Equals(".xlsx"))
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            else
            {
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            excelReader.IsFirstRowAsColumnNames = true;
            var result = excelReader.AsDataSet();


            var dataSheetQuery = result.Tables.OfType<DataTable>().Where(x => x.TableName == "DETALLE POR LÍNEA DE FINANCIAMI");
            //if (improvementPlanStatusId == (int)StageStatusEnum.EnIngreso && dataSheetQuery.Any())
            if (dataSheetQuery.Any())
            {

                lst.AddRange(ReadSolicitudes(dataSheetQuery.FirstOrDefault().Rows, line22Id, schoolYearId, improvementPlanStatusId));

               
            }
         

            excelReader.Close();

            return lst;
        }

        /// <summary>
        /// Importa las solicitudes de un archivo Excel, Basado en Resu 2022
        /// </summary>
        /// <param name="stream">Stream del archivo a importar</param>
        /// <param name="line22Id">Id del eje</param>
        /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
        /// <param name="extension"></param>
        /// <param name="schoolYearId"></param>
        /// <returns>Un listado con una colección de solicitados correspondientes a los registros del Excel</returns>
        public ImportCompleteDTO ImportSolicitudes(Stream stream, int line22Id, int schoolYearId, ImprovementPlan plan, string extension)
        {
            var lst = new List<Solicitude>();
            var hijos = new List<Solicitude>();
            ImportCompleteDTO DTO = new ImportCompleteDTO();
            IExcelDataReader excelReader;
            if (extension.Equals(".xlsx"))
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            else
            {
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            excelReader.IsFirstRowAsColumnNames = true;
            var result = excelReader.AsDataSet();


            var dataSheetQuery = result.Tables.OfType<DataTable>().Where(x => x.TableName == "DETALLE POR LÍNEA DE FINANCIAMI");
            //if (improvementPlanStatusId == (int)StageStatusEnum.EnIngreso && dataSheetQuery.Any())
            if (dataSheetQuery.Any())
            {

                lst.AddRange(ReadSolicitudes(dataSheetQuery.FirstOrDefault().Rows, line22Id, schoolYearId, true));
                hijos.AddRange(this.GetRelatedLines(dataSheetQuery.FirstOrDefault().Rows, plan, schoolYearId));


            }


            excelReader.Close();

            DTO.solicitados_plan = lst;
            DTO.solicitados_hijos = hijos;
            return DTO;
            
        }

        /// <summary>
        /// Importa las solicitudes de un archivo Excel, Genera las lineas Res 2022 relacionadas
        /// </summary>
        /// <param name="stream">Stream del archivo a importar</param>
        /// <param name="ImprovementPlanId">Id del Plan</param>
        /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
        /// <param name="extension"></param>
        /// <param name="schoolYearId"></param>
        /// <returns>Un listado con una colección de RelatedPLans correspondientes a los registros del Excel</returns>
        public List<Solicitude> GetRelatedLines(Stream stream, ImprovementPlan plan, int schoolYearId,  string extension)
        {
            var lst = new List<RelatedLines_22>();
            var solicitados = new List<Solicitude>();
            IExcelDataReader excelReader;
            if (extension.Equals(".xlsx"))
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            else
            {
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            excelReader.IsFirstRowAsColumnNames = true;
            var result = excelReader.AsDataSet();


            var dataSheetQuery = result.Tables.OfType<DataTable>().Where(x => x.TableName == "DETALLE POR LÍNEA DE FINANCIAMI");
            //if (improvementPlanStatusId == (int)StageStatusEnum.EnIngreso && dataSheetQuery.Any())
            if (dataSheetQuery.Any())
            {

                lst.AddRange(ReadRelatedLines(dataSheetQuery.FirstOrDefault().Rows, plan));
                
            }

            Context.RelatedLines_22.AddRange(lst);
            Context.SaveChanges();
            //obtenemos todos los solicitados!
            foreach(RelatedLines_22 r in lst)
            {
                solicitados.AddRange(ReadSolicitudes(dataSheetQuery.FirstOrDefault().Rows,r.Line22Id.Value, schoolYearId,1));
            }


            excelReader.Close();

            return solicitados;
        }


        /// <summary>
        /// Importa las solicitudes de un archivo Excel, Genera las lineas Res 2022 relacionadas
        /// </summary>
        /// <param name="datashett"> DataShet de xls ya abierto  a importar</param>
        /// <param name="ImprovementPlanId">Id del Plan</param>
        /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
        /// <param name="extension"></param>
        /// <param name="schoolYearId"></param>
        /// <returns>Un listado con una colección de RelatedPLans correspondientes a los registros del Excel</returns>
        public List<Solicitude>  GetRelatedLines(DataRowCollection sheets, ImprovementPlan plan, int schoolYearId )
        {
            var lst = new List<RelatedLines_22>();
            var solicitados = new List<Solicitude>();
            IExcelDataReader excelReader;
          


          
            if (sheets.Count>1)
            {

                lst.AddRange(ReadRelatedLines(sheets, plan));

            }

            Context.RelatedLines_22.AddRange(lst);
            Context.SaveChanges();
            //obtenemos todos los solicitados!
            foreach (RelatedLines_22 r in lst)
            {
                solicitados.AddRange(ReadSolicitudes(sheets, r.Line22Id.Value, schoolYearId, true));
            }




            return solicitados;
        }



        /// <summary>
        /// Importa las solicitudes de un archivo Excel, Basado en Resu 2016!
        /// </summary>
        /// <param name="stream">Stream del archivo a importar</param>
        /// <param name="FieldId">Id del eje</param>
        /// <param name="LineId">Id de la linea </param>
        /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
        /// <param name="extension"></param>
        /// <param name="schoolYearId"></param>
        /// <returns>Un listado con una colección de solicitados correspondientes a los registros del Excel</returns>
        public List<Solicitude> ImportSolicitudes(Stream stream, int FieldId, int LineId, int schoolYearId, int improvementPlanStatusId, string extension)
        {
            var lst = new List<Solicitude>();
            IExcelDataReader excelReader;
            if (extension.Equals(".xlsx"))
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            else
            {
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            excelReader.IsFirstRowAsColumnNames = true;
            var result = excelReader.AsDataSet();


            var dataSheetQuery = result.Tables.OfType<DataTable>().Where(x => x.TableName == "DETALLE POR LÍNEA DE FINANCIAMI");
            if (improvementPlanStatusId == (int)StageStatusEnum.EnIngreso && dataSheetQuery.Any())
            {

                lst.AddRange(ReadSolicitudes(dataSheetQuery.FirstOrDefault().Rows,FieldId, LineId, schoolYearId, improvementPlanStatusId));
            }


            excelReader.Close();

            return lst;
        }



        /// <summary>
        /// Lee las solicitudes de un excel de plan enviado por jurisdicción (formato por resolución INET) y lo convierte en una lista de solicitados
        /// </summary>
        /// <param name="solicitudesRows">Colección de filas del excel que contiene los solicitados</param>
        /// <param name="fieldId">Id del eje del plan de mejora</param>
        /// <returns>Una lista de solicitados perteneciente a los registros del Excel</returns>
        private List<Solicitude> ReadSolicitudesJurisdictionalSheet(DataRowCollection solicitudesRows, string fieldId, int improvementPlanStatusId, int schoolYeadId)
        {
            var lst = new List<Solicitude>();
            
            foreach (DataRow row in solicitudesRows)
            {
                // evitar las primeras 6 filas
                if (solicitudesRows.IndexOf(row) < 6)
                    continue;
                int n;
                // la primer columna debe ser no vacía y tener 9 posiciones para ser un cue
                if (string.IsNullOrEmpty(row[0].ToString()) || row[0].ToString().Length != 9 || row[0].ToString().Split().Where(x => int.TryParse("123", out n) == false).Any())
                {
                    flag_cue_vacio = true;// si hay cues vacios avisar 
                    break;
                }
                var solicitude = new Solicitude();
                solicitude.Id = solicitudesRows.IndexOf(row);
                solicitude.CUE = row[0].ToString();
                solicitude.Details = row[3].ToString();
                solicitude.SchoolYearId = schoolYeadId;
                solicitude.LineId = 30;
                //solicitude.LineId = GetLineId(Int32.Parse(fieldId), row[3].ToString());
                solicitude.Specialization = "TODOS - Todos";
                //solicitude.Level = row[6].ToString();
                solicitude.RequestedAmount = Convert.ToDecimal(row[7].ToString());
                solicitude.MeasurementUnitId = GetMeasurementUnitId(row[6].ToString());
                solicitude.RequestedPriceUnit = Convert.ToDecimal(row[8].ToString().Replace("$", ""));
                solicitude.StatusId = SolicitudeStatusEnum.Pendiente.ToInt();
                solicitude.ExpenditureTypeId = GetExpenditureTypeId(row[4].ToString());
                solicitude.FileNumber = "";
                solicitude.SolicitudeTypeId = SolicitudeTypeEnum.Original.ToInt();

                // Valido que los ids calculados sean válidos
                //if (solicitude.LineId == 0)
                //    throw new ApplicationException("La línea del solicitado no es válida. Asegúrese que corresponde al eje del plan.");

                if (solicitude.SchoolYearId == 0)
                    throw new ApplicationException("El ciclo lectivo no es válido.");
                
                if (solicitude.SchoolYearId < GetActiveSchoolYear().Id)
                    throw new ApplicationException("El ciclo lectivo no puede ser anterior al ciclo lectivo actual.");

                lst.Add(solicitude);
            }

            return lst;
        }

        /// <summary>
        /// Lee las solicitudes de un excel y lo convierte en una lista de solicitados
        /// </summary>
        /// <param name="solicitudesRows">Colección de filas del excel que contiene los solicitados</param>
        /// <param name="subfieldId">Id de la linea 22 del plan de mejora</param>
        /// <returns>Una lista de solicitados perteneciente a los registros del Excel</returns>
        private List<Solicitude> ReadSolicitudes(DataRowCollection solicitudesRows, int line22Id,int schoolYearId, int improvementPlanStatusId)
        {
            var lst = new List<Solicitude>();
            int i = 0;

            foreach (DataRow row in solicitudesRows)
            {
                if (i++ < 8)
                {
                    // skip headers
                    continue;
                }
                if (string.IsNullOrEmpty(row[0].ToString()))
                    break;

                var solicitude = new Solicitude();
                solicitude.Id = solicitudesRows.IndexOf(row);
                solicitude.CUE = row[0].ToString();
                solicitude.Details = row[9].ToString();
                solicitude.SchoolYearId = schoolYearId;
               // solicitude.LineId = GetLineId(Int32.Parse(fieldId), row[3].ToString());
                solicitude.Specialization = "TODOS - Todos";

                var Line22 =  GetLine22( row[7].ToString());
                if (Line22!=null)
                {
                    //if (Line22.Id == line22Id)
                  /*  {*/   if (row[11].ToString()=="" || row[12].ToString()=="")
                            throw new ApplicationException("El archivo tiene columnas de precio vacias!");
                        solicitude.LineId = Line22.LineId;
                        solicitude.Field_Id = Line22.SubField.Field.Id;
                        solicitude.SubField_Id = Line22.SubFieldId;
                        solicitude.Line_22_Id = Line22.Id;
                        solicitude.RequestedAmount = Convert.ToDecimal(row[11].ToString());
                        solicitude.RequestedPriceUnit = Convert.ToDecimal(row[12].ToString().Replace("$", ""));

                        solicitude.MeasurementUnitId = GetMeasurementUnitId(row[10].ToString());
                      
                        solicitude.StatusId = SolicitudeStatusEnum.Pendiente.ToInt();
                        solicitude.ExpenditureTypeId = GetExpenditureTypeId(row[14].ToString());
                        solicitude.ExpenditureObjectTypeId = GetExpenditureObjectTypeId(row[15].ToString());
                        solicitude.FileNumber = "";
                        solicitude.SolicitudeTypeId = SolicitudeTypeEnum.Original.ToInt();
                       if (Line22.Id==line22Id)
                           lst.Add(solicitude);
                    //}

                    // solicitude.Level = row[6].ToString();
           

                //// Valido que los ids calculados sean válidos
                //if (solicitude.LineId == 0)
                //    throw new ApplicationException("La línea del solicitado no es válida. Asegúrese que corresponde al eje del plan.");

                //if (solicitude.SchoolYearId == 0)
                //    throw new ApplicationException("El ciclo lectivo no es válido.");

                //if (solicitude.SchoolYearId < Context.SchoolYears.Where(x => x.Active.HasValue && x.Active.Value == true).FirstOrDefault().Id)
                //    throw new ApplicationException("El ciclo lectivo no puede ser anterior al ciclo lectivo actual.");

                
                }
            }

        
            if (lst.Count==0)
                throw new ApplicationException("No hay Solicitudes que coincidan con la linea res 22 seteada del plan!");

            return lst;
        }


        private List<Solicitude> ReadSolicitudes(DataRowCollection solicitudesRows, int line22Id, int schoolYearId, bool padre)
        {
            var lst = new List<Solicitude>();
            int i = 0;

            foreach (DataRow row in solicitudesRows)
            {
                if (i++ < 8)
                {
                    // skip headers
                    continue;
                }
                if (string.IsNullOrEmpty(row[0].ToString()))
                    break;

                var solicitude = new Solicitude();
                solicitude.Id = solicitudesRows.IndexOf(row);
                solicitude.CUE = row[0].ToString();
                solicitude.Details = row[9].ToString();
                solicitude.SchoolYearId = schoolYearId;
                // solicitude.LineId = GetLineId(Int32.Parse(fieldId), row[3].ToString());
                solicitude.Specialization = "TODOS - Todos";

                var Line22 = GetLine22(row[7].ToString());
                if (Line22 != null)
                {
                    //if (Line22.Id == line22Id)
                    /*  {*/
                    if (row[11].ToString() == "" || row[12].ToString() == "")
                        throw new ApplicationException("El archivo tiene columnas de precio vacias!");
                    solicitude.LineId = Line22.LineId;
                    solicitude.Field_Id = Line22.SubField.Field.Id;
                    solicitude.SubField_Id = Line22.SubFieldId;
                    solicitude.Line_22_Id = Line22.Id;
                    solicitude.RequestedAmount = Convert.ToDecimal(row[11].ToString());
                    solicitude.RequestedPriceUnit = Convert.ToDecimal(row[12].ToString().Replace("$", ""));

                    solicitude.MeasurementUnitId = GetMeasurementUnitId(row[10].ToString());

                    solicitude.StatusId = SolicitudeStatusEnum.Pendiente.ToInt();
                    solicitude.ExpenditureTypeId = GetExpenditureTypeId(row[14].ToString());
                    solicitude.ExpenditureObjectTypeId = GetExpenditureObjectTypeId(row[15].ToString());
                    solicitude.FileNumber = "";
                    solicitude.SolicitudeTypeId = SolicitudeTypeEnum.Original.ToInt();
                    if (!padre)
                        lst.Add(solicitude);
                    else if (padre && Line22.Id == line22Id)
                        lst.Add(solicitude);
                    //}

                    // solicitude.Level = row[6].ToString();


                    //// Valido que los ids calculados sean válidos
                    //if (solicitude.LineId == 0)
                    //    throw new ApplicationException("La línea del solicitado no es válida. Asegúrese que corresponde al eje del plan.");

                    //if (solicitude.SchoolYearId == 0)
                    //    throw new ApplicationException("El ciclo lectivo no es válido.");

                    //if (solicitude.SchoolYearId < Context.SchoolYears.Where(x => x.Active.HasValue && x.Active.Value == true).FirstOrDefault().Id)
                    //    throw new ApplicationException("El ciclo lectivo no puede ser anterior al ciclo lectivo actual.");


                }
            }


            //if (lst.Count == 0)
            //    throw new ApplicationException("No hay Solicitudes que coincidan con la linea res 22 seteada del plan!");

            return lst;
        }

        /// Lee las solicitudes de un excel y lo convierte en una lista de Lineas Relacionadas
        /// </summary>
        /// <param name="solicitudesRows">Colección de filas del excel que contiene los solicitados</param>
        /// <param name="subfieldId">Id de la linea 22 del plan de mejora</param>
        /// <returns>Una lista de solicitados perteneciente a los registros del Excel</returns>
        private List<RelatedLines_22> ReadRelatedLines(DataRowCollection solicitudesRows, ImprovementPlan plan)
        {
            var lst = new List<RelatedLines_22>();
            int i = 0;
       
            foreach (DataRow row in solicitudesRows)
            {
                if (i++ < 8)
                {
                    // skip headers
                    continue;
                }
                if (string.IsNullOrEmpty(row[0].ToString()))
                    break;

               var cue = row[0].ToString();

                var Line22 = GetLine22(row[7].ToString());


                if (Line22 != null)
                {
                   
                        if (row[11].ToString() == "" || row[12].ToString() == "")
                            throw new ApplicationException("El archivo tiene columnas de precio vacias!");

                    //if (Line22.Id == plan.Line_22_Id.Value)
                    //    continue;

                    var r = new RelatedLines_22();
                    if (lst.Where(x=>x.Line22Id==Line22.Id).FirstOrDefault() ==null   )
                    {
                        r.ImprovementPlanId = plan.Id;
                        r.Line22Id = Line22.Id;
                        r.CUE      = cue;
                        if (Line22.Id == plan.Line_22_Id.Value)
                            r.ImprovementPlanId_Link = plan.Id;
                        lst.Add(r);

                    }
               
                 

                }
            }


            if (lst.Count == 0)
                throw new ApplicationException("No hay Solicitudes que coincidan con la lineas res 22!");

            return lst;
        }



        /// <summary>
        /// Lee las solicitudes de un excel y lo convierte en una lista de solicitados
        /// </summary>
        /// <param name="solicitudesRows">Colección de filas del excel que contiene los solicitados</param>
        /// <param name="FieldId">Id de la linea 22 del plan de mejora</param>
        /// <param name="LineId">Id de la linea 22 del plan de mejora</param>
        /// <returns>Una lista de solicitados perteneciente a los registros del Excel</returns>
        private List<Solicitude> ReadSolicitudes(DataRowCollection solicitudesRows,int FieldId, int lineId, int schoolYearId, int improvementPlanStatusId)
        {
            var lst = new List<Solicitude>();
            int i = 0;

            foreach (DataRow row in solicitudesRows)
            {
                if (i++ < 8)
                {
                    // skip headers
                    continue;
                }
                if (string.IsNullOrEmpty(row[0].ToString()))
                    break;

                var solicitude = new Solicitude();
                solicitude.Id = solicitudesRows.IndexOf(row);
                solicitude.CUE = row[0].ToString();
                solicitude.Details = row[9].ToString();
                solicitude.SchoolYearId = schoolYearId;
                // solicitude.LineId = GetLineId(Int32.Parse(fieldId), row[3].ToString());
                solicitude.Specialization = "TODOS - Todos";

                var Line = GetLineId(row[8].ToString());
                if (Line != null)
                {
                    if (Line.Id == lineId)
                    {
                        if (row[11].ToString() == "" || row[12].ToString() == "")
                            throw new ApplicationException("El archivo tiene columnas de precio vacias!");
                        solicitude.LineId = Line.Id;
                        solicitude.Field_Id = Line.FieldId;
                        solicitude.SubField_Id = null;
                        solicitude.Line_22_Id = null;
                        solicitude.RequestedAmount = Convert.ToDecimal(row[11].ToString());
                        solicitude.RequestedPriceUnit = Convert.ToDecimal(row[12].ToString().Replace("$", ""));

                        solicitude.MeasurementUnitId = GetMeasurementUnitId(row[10].ToString());

                        solicitude.StatusId = SolicitudeStatusEnum.Pendiente.ToInt();
                        solicitude.ExpenditureTypeId = GetExpenditureTypeId(row[14].ToString());
                        solicitude.ExpenditureObjectTypeId = GetExpenditureObjectTypeId(row[15].ToString());
                        solicitude.FileNumber = "";
                        solicitude.SolicitudeTypeId = SolicitudeTypeEnum.Original.ToInt();
                        lst.Add(solicitude);
                    }

                   


                }
            }


            if (lst.Count == 0)
                throw new ApplicationException("No hay Solicitudes que coincidan con la linea seteada del plan!");

            return lst;
        }



        /// <summary>
        /// Importa las solicitudes a actualizar de un archivo Excel
        /// </summary>
        /// <param name="stream">Stream del archivo a importar</param>
        /// <param name="fieldId">Id del eje</param>
        /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
        /// <returns>Un listado con una colección de solicitados correspondientes a los registros del Excel</returns>
        public List<Solicitude> ImportSolicitudesApprovals(Stream stream, int fieldId, int improvementPlanStatusId, string extension)
        {
            var lst = new List<Solicitude>();
            IExcelDataReader excelReader;
            if (extension.Equals(".xlsx"))
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            else
            {
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }

            excelReader.IsFirstRowAsColumnNames = true;
            var result = excelReader.AsDataSet();

            lst.AddRange(ReadSolicitudesApprovals(result.Tables[0].Rows, fieldId.ToString(), improvementPlanStatusId));
            excelReader.Close();

            return lst;
        }

        /// <summary>
        /// Lee las solicitudes de un excel y lo convierte en una lista de solicitados para actualizar
        /// </summary>
        /// <param name="solicitudesRows">Colección de filas del excel que contiene los solicitados</param>
        /// <param name="fieldId">Id del eje del plan de mejora</param>
        /// <returns>Una lista de solicitados perteneciente a los registros del Excel</returns>
        private List<Solicitude> ReadSolicitudesApprovals(DataRowCollection solicitudesRows, string fieldId, int improvementPlanStatusId)
        {
            var lst = new List<Solicitude>();
            int i = 0;
            foreach (DataRow row in solicitudesRows)
            {
                // ignore first two rows
                if (i++ < 3)
                {
                    continue;
                }
                else
                {
                    if (string.IsNullOrEmpty(row[0].ToString()))
                        break;

                    if (row[10].ToString().Length > 0 && row[11].ToString().Length > 0)
                    {
                        var solicitude = new Solicitude();
                        solicitude.Id = Int32.Parse(row[0].ToString());
                        solicitude.ApprovedAmount = Int16.Parse(row[10].ToString());
                        solicitude.ApprovedPriceUnit = Decimal.Parse(row[11].ToString());
                        // Valido que los ids calculados sean válidos
                        if (solicitude.Id == 0)
                            throw new ApplicationException($"No se recibió un ID de solicitado para modificar (Fila {i}).");
                        lst.Add(solicitude);
                    }
                }
            }

            return lst;
        }

        /// <summary>
        /// Obtiene el CUE a asignarle a un plan de mejora
        /// </summary>
        /// <param name="improvementPlanType">Id del tipo de plan de mejora</param>
        /// <param name="province">Descripción de la provincia correspondiente al plan</param>
        /// <param name="dataSet">DataSet correspondiente al archivo Excel</param>
        /// <returns>Un string con el CUE a asignar al plan de mejora</returns>
        private string GetImprovementPlanCUE(int improvementPlanType, string province, DataSet dataSet)
        {
            var type = (ImprovementPlanTypeEnum)improvementPlanType;

            switch (type)
            {
                case ImprovementPlanTypeEnum.Nacional:
                    return "000000000";

                case ImprovementPlanTypeEnum.Jurisdiccional:
                    return Context.Provinces.Where(x => x.Name == province).First().Number.PadRight(7, '0');

                case ImprovementPlanTypeEnum.Institucional:
                    return dataSet.Tables[0].Rows[0][0].ToString().Trim();

                default:
                    return null;
            }
        }

        /// <summary>
        /// Obtiene el Id del eje
        /// </summary>
        /// <param name="field">Descripción del eje</param>
        /// <returns>Obtiene el Id del eje en base a la descripción del eje</returns>
        private int GetFieldId(string field)
        {
            switch (field)
            {
                case "Igualdad":
                    return 1;

                case "Formación":
                    return 2;

                case "Entornos":
                    return 3;

                case "Piso":
                    return 4;

                case "Infraestructura":
                    return 5;

                case "Red":
                    return 6;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Loguea el evaluador ingresado
        /// </summary>
        /// <param name="fileName">Archivo excel que tiene asignado el evaluador</param>
        /// <param name="identifier">Identificador del plan de mejora</param>
        /// <param name="fieldId">Id del eje</param>
        /// <param name="evaluator">Evaluador asignado</param>
        /// <remarks>Hay un error en el Excel y se asigna los evaluadores </remarks>
        public void LogEvaluator(string fileName, string identifier, int fieldId, string evaluator)
        {
            if (!string.IsNullOrEmpty(evaluator))
            {
                var log = LogManager.GetLogger("LogEvaluatorsToFile");
                log.Info(evaluator.ToUpper());
                log.Info(String.Concat("\t Archivo:\t\t", fileName));
                log.Info(String.Concat("\t Id del Plan:\t", identifier));
                log.Info(String.Concat("\t Id del Eje:\t", fieldId));
            }
        }

        /// <summary>
        /// Devuelve el id del tipo de gasto 
        /// </summary>
        /// <param name="expenditureType">Descripción del tipo de gasto</param>
        /// <returns></returns>
        private int? GetExpenditureTypeId(string expenditureType)
        {
            switch (expenditureType)
            {
                case "Gasto Corriente":
                case "Corriente":
                    return 1;

                case "Gasto de Capital":
                case "Capital":
                    return 2;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Devuelve el id del objeto de gasto 
        /// </summary>
        /// <param name="expenditureType">Descripción del objeto de gasto</param>
        /// <returns></returns>
        private int? GetExpenditureObjectTypeId(string expenditureObjectType)
        {
            switch (expenditureObjectType)
            {
                case "Bienes_y_servicios":
                    return 1;
                case "Pasajes_y_viáticos":
                    return 2;
             
               
                case "RRHH":
                    return 3;

                default:
                    return null;
            }
        }

    }
}
