using System;
using System.Linq;
using System.Web;
using EntityFramework.Audit;
using INET.Core.Interfaces;
using INET.Data;
using log4net;
using INET.Services.DTO;
using System.Collections.Generic;
using INET.Utils.Helpers;
using INET.Core.Enums;
using ClosedXML.Excel;
using System.Net;

namespace INET.Services
{
    /// <summary>
    /// Servicio base del que heredan todos los servicios
    /// </summary>
    public class BusinessService
    {
        /// <summary>
        /// Propiedad para acceder al DataContex de forma sencilla desde cualquier servicio.         
        /// </summary>
        protected INETContext Context { get; set; }

        /// <summary>
        /// Propiedad para acceder al Log4Net y poder escribir en un archivo de texto desde cualquier parte del disco
        /// </summary>
        protected static readonly ILog FileSystemLog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Obtiene la fecha en base a un string
        /// </summary>
        /// <param name="date">Fecha como string de formulario</param>
        /// <returns>La fecha en base a un string</returns>
        public DateTime? ParseDateFromString(string _date)
        {
            DateTime? date = null;
            if (!string.IsNullOrWhiteSpace(_date))
            {
                date = Convert.ToDateTime(_date);
            }

            return date;
        }

        /// <summary>
        /// Obtiene un ciclo lectivo por ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>SchoolYear</returns>
        public SchoolYear GetShoolYear(int id)
        {
            return Context.SchoolYears.Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Obtiene el Id de un ciclo lectivo
        /// </summary>
        /// <param name="schoolYear">Descripción del ciclo lectivo</param>
        /// <returns>El id del ciclo lectivo correspondiente</returns>
        public int GetSchoolYearId(string schoolYear)
        {
            return Context.SchoolYears.Where(x => x.Cycle.Equals(schoolYear)).FirstOrDefault().Id;
        }

        /// <summary>
        /// Obtiene el Id de la línea en base a su descripción
        /// </summary>
        /// <param name="field">Descripción del eje</param>
        /// <param name="line">Descripción de la línea</param>
        /// <returns>El id correspondiente a la descripción de la línea</returns>
        public int GetLineId(int field, string line)
        {
            line = line.Substring(0, 1);
            return Context.Lines.Where(x => x.Code.Equals(line) && x.Field.StatusId == (int) AxisStatusEnum.Vigente && x.FieldId == field).FirstOrDefault().Id;
        }

        /// <summary>
        /// Obtiene  línea 22  en base a su descripción
        /// </summary>

        /// <param name="text">texto xls </param>
        /// <returns>El registro linea </returns>
        public Line GetLineId(string text)
        {

            return Context.Lines.Where(x => x.title_xls.Equals(text)).FirstOrDefault();
        }


        /// <summary>
        /// Obtiene  línea 22  en base a su descripción
        /// </summary>

        /// <param name="text">texto xls </param>
        /// <returns>El registro linea 22</returns>
        public Lines_22 GetLine22( string text)
        {
            
            return Context.Lines_22.Where(x => x.title_xls.Equals(text)).FirstOrDefault();
        }



        /// <summary>
        /// Obtiene Lineas Por Plan
        /// </summary>

        /// <param name="planId">Id del plan</param>
        /// <returns>El id  del plan</returns>
        public Line GetLines(int planId)
        {
            int lineId= Context.ImprovementPlans.Where(x => x.Id == planId).FirstOrDefault().Solicitudes.FirstOrDefault().LineId.Value;
            return Context.Lines.Where(v => v.Id == lineId).FirstOrDefault();
        }

        /// <summary>
        /// Obtiene el id de la unidad de medida. Si no existe, lo agrega a la base.
        /// </summary>
        /// <param name="measurementUnit">Descripción de la unidad de medida</param>
        /// <returns>El id de la unidad de medida perteneciente a la descripción del mismo</returns>
        /// <remarks>Cabe destacar que como los valores se levantan de un combo, en caso de que no exista la unidad de medida se agrega a la base</remarks>
        public int GetMeasurementUnitId(string measurementUnit)
        {
            measurementUnit = StringHelper.CamelCase(measurementUnit);
            var mu = Context.MeasurementUnits.Where(x => x.Description == measurementUnit).FirstOrDefault();
            if (mu == null)
            {
                mu = new MeasurementUnit() { Description = measurementUnit };
                Context.MeasurementUnits.Add(mu);
                Context.SaveChanges();
            }

            return mu.Id;
        }

        /// <summary>
        /// Obtiene el nombre de un ID de tipo de institución desde la tabla de planes, prioridad al último actualizado
        /// </summary>
        /// <param name="institutionLevelId"></param>
        /// <returns>Nombre del tipo de plan</returns>
        public string GetInstitutionLevelName(int institutionLevelId)
        {
            var refPlan = Context.ImprovementPlans.Where(x => x.InstitutionLevelInt == (int)institutionLevelId).OrderByDescending(x => x.Id);
            return refPlan.Count() > 0 ? refPlan.First().InstitutionLevel : "Todos";
        }

        /// <summary>
        /// Obtiene un listado de todos los valores de tipo de institución cargados en planes
        /// </summary>
        /// <returns>Lista de tipos de institución</returns>
        public List<InstitutionLevel> ListLocalInstitutionLevels()
        {
            var levels = from ll in Context.ImprovementPlans
                         join li in Context.InstitutionLevels on ll.InstitutionLevelInt equals li.Code
                         group li by li into grp
                         select grp.Key;
            return levels.ToList();
        }

        /// <summary>
        /// Obtiene un listado de todos los valores de tipo de institución
        /// </summary>
        /// <returns>Lista de tipos de institución</returns>
        public List<InstitutionLevel> ListInstitutionLevels()
        {
            var levels = from l in Context.InstitutionLevels select l;
            return levels.ToList();
        }

        /// <summary>
        /// Lista los diferentes tipos de gastos
        /// </summary>
        /// <returns>Una coleccion con todos los tipos de gastos</returns>
        public IList<ExpenditureType> ListExpenditureTypes()
        {
            return Context.ExpenditureTypes.OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Lista los diferentes tipos de gastos
        /// </summary>
        /// <returns>Una coleccion con todos los tipos de gastos</returns>
        public IList<ExpenditureObjectType> ListExpenditureObjectTypes(int? ExpenditureId)
        {
            if (ExpenditureId==null)
                 return Context.ExpenditureObjectTypes.OrderBy(x => x.Description).ToList();
            else if ( ExpenditureId== (int) ExpenditureTypeEnum.GastoCapital)
                return Context.ExpenditureObjectTypes.Where(x=>x.Id==1).ToList();
            else
                return Context.ExpenditureObjectTypes.OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Lista todas las unidades de medida
        /// </summary>
        /// <returns>Una colección con todas las unidades de medida</returns>
        public IList<MeasurementUnit> ListMeasurementUnits()
        {
            return Context.MeasurementUnits.OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Obtiene un tipo de plan por ID
        /// </summary>
        /// <param name="planTypeId"></param>
        /// <returns>ImprovementPlansType</returns>
        public ImprovementPlansType GetImprovementPlanType(int planTypeId)
        {
            return Context.ImprovementPlansTypes.Where(x => x.Id == planTypeId).FirstOrDefault();
        }

        /// <summary>
        /// Lista los tipos de planes de mejora
        /// </summary>
        /// <returns>Una lista con los diferentes planes de mejora</returns>
        public IList<ImprovementPlansType> ListImprovementPlansTypes()
        {
            return Context.ImprovementPlansTypes.ToList();
        }

        /// <summary>
        /// Lista los tipos de solicitados
        /// </summary>
        /// <returns>Una coleccion con los tipos de solicitados</returns>
        public IList<SolicitudeType> ListSolicitudesTypes()
        {
            return Context.SolicitudeTypes.OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Obtiene una provincia por ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Province</returns>
        public Province GetProvince(int id)
        {
            return Context.Provinces.Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Obtiene una provincia a través de su COD NUM de CUE
        /// </summary>
        /// <param name="provNumber">COD NUM de la provincia</param>
        /// <returns>La provincia</returns>
        public Province GetProvinceByNumber(string provNumber)
        {
            return Context.Provinces.Where(x => x.Number == provNumber).FirstOrDefault();
        }

        /// <summary>
        /// Lista todas las provincias
        /// </summary>
        /// <returns>Una lista con todas las provincias</returns>
        public List<Province> ListProvinces()
        {
            // Devuelvo las provincias ordenadas alfabéticamente con CABA en primer lugar
            var cud = Context.Provinces.Where(x => x.Code == "CUD").First();
            var provinces = Context.Provinces.ToList().Except(new[] { cud }).OrderBy(x => x.Name).ToList();
            provinces.Insert(0, cud);
            return provinces;
        }

        /// <summary>
        /// Lista los ejes
        /// </summary>
        /// <returns>Una colección con los ejes</returns>
        public List<Field> ListFields()
        {
            return Context.Fields.Where(x => x.StatusId != (int) AxisStatusEnum.Archivado).OrderBy(x => x.Description).ToList();
        }


        /// <summary>
        /// Lista los sub-ejes
        /// </summary>
        /// /// <param name="fieldId">Eje!</param>
        /// <returns>Una colección con los Sub-ejes</returns>
        public List<SubField> ListSubFields(int fieldId)
        {
            return Context.SubFields.Where(x => x.StatusId != (int)AxisStatusEnum.Archivado && x.FieldId==fieldId).OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Lista los sub-ejes
        /// </summary>
        /// <returns>Una colección con los Sub-ejes</returns>
        public List<SubField> ListSubFields()
        {
            return Context.SubFields.Where(x => x.StatusId != (int)AxisStatusEnum.Archivado).OrderBy(x => x.Description).ToList();
        }
        /// <summary>
        /// Lista los ejes
        /// </summary>
        /// <param name="userName">Nombre de usuario</param>
        /// <returns>Una colección con los ejes habilitados para el usuario</returns>
        public IList<Field> ListFields(string userName)
        {
            return Context.UserProfiles.Where(x => x.UserName == userName).First().Fields.OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Lista las lineas de un solicitado en base al eje del plan de mejora
        /// </summary>
        /// <param name="fieldId">Id del eje del cual se desean obtener las lineas</param>
        /// <returns>Un colección con las lineas pertenecientes al eje seleccionado</returns>
        public IList<Line> ListLinesByField(int fieldId)
        {
            return Context.Lines.Where(x => x.FieldId == fieldId).OrderBy(x => x.Description).ToList();
        }


        /// <summary>
        /// Lista las lineas de un solicitado en base al sub-eje del plan de mejora
        /// </summary>
        /// <param name="subfieldId">Id del sub-eje del cual se desean obtener las lineas</param>
        /// <returns>Un colección con las lineas pertenecientes al eje seleccionado</returns>
        public IList<Lines_22> ListLinesBySubField(int subfieldId)
        {
            return Context.Lines_22.Where(x => x.SubFieldId == subfieldId).OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Lista las lineas plan 22
        /// </summary>

        /// <returns>Un colección con las lineas</returns>
        public IList<Lines_22> ListLines22()
        {



            var data = Context.Lines_22.ToList();
         
            
            foreach (Lines_22 l in data)
            {
                l.Description= l.SubField.Code + "."+ l.Code + "." + l.Description;

            }

            return data;
        }


        /// <summary>
        /// obtiene por una linea_22 los correspodiente eje, linea, subeje
        /// </summary>
        /// <param name="line22Id">Id  Linea 22</param>
        /// <returns>Dto linea 22</returns>
        public Line22DTO getLine22DTO(int line22Id)
        {
            Line22DTO result = new Line22DTO();
            var line_22 = Context.Lines_22.Where(x => x.Id == line22Id).FirstOrDefault();

            result.SubFieldId = line_22.SubFieldId;
            result.FieldId = line_22.SubField.FieldId;
            result.LineId = line_22.LineId;

           

            var sub_eje = Context.Lines_22.Where(x => x.Id == line22Id).Select(v => v.SubField).OrderBy(x => x.Description).ToList(); ;
            var eje = sub_eje.Select(x => x.Field).OrderBy(x => x.Description).ToList();
            var linea = Context.Lines_22.Where(x => x.Id == line22Id).Select(x => x.Line).ToList();




            result.Lines = linea;
            result.Id = line22Id;
          
            result.SubFields = sub_eje;
            result.Fields = eje;
            return result;
        }




        /// <summary>
        /// Lista las lineas de un solicitado en base al plan de mejora
        /// </summary>
        /// <param name="plan">Plan de mejora</param>
        /// <returns>Un colección con las lineas pertenecientes al eje seleccionado</returns>
        public IList<Line> ListLinesForPlan(ImprovementPlan plan)
        {
            //SIA 07-10 , si ya tiene una linea cargada el plan , respetarlo
            //var solicitudes= Context.Solicitudes.Where(x => x.ImprovementPlanId == plan.Id).ToList();
            // if (solicitudes.Count==0)
            // {
            //         if (plan.Dependence != null && plan.Dependence.Contains("Nacional"))
            //         {
            //             var _data = System.Configuration.ConfigurationManager.AppSettings["national_plan_field_line"];
            //             var _config = _data.Split(',').Select(x => x.Trim());
            //             return Context.Lines.Where(x => x.FieldId == plan.FieldId && _config.Contains(x.Field.Code + "/" + x.Code)).Select(x => x).ToList();
            //         } else
            //         {
            //             return Context.Lines.Where(x => x.FieldId == plan.FieldId).OrderBy(x => x.Description).ToList();
            //             }
            // } else
            // {
            //     List<Line> list = new List<Line>();
            //     list.Add(solicitudes.First().Line);
            //     return list;


            // }
            //SIA 18-11 se depreca el codigo anterior 

            if (plan.Dependence != null && plan.Dependence.Contains("Nacional"))
            {
                var _data = System.Configuration.ConfigurationManager.AppSettings["national_plan_field_line"];
                var _config = _data.Split(',').Select(x => x.Trim());
                return Context.Lines.Where(x => x.FieldId == plan.FieldId && _config.Contains(x.Field.Code + "/" + x.Code)).Select(x => x).ToList();
            }
            else
            {
                return Context.Lines.Where(x => x.FieldId == plan.FieldId).OrderBy(x => x.Description).ToList();
            }

        }

        /// <summary>
        /// Lista los posibles días de un mes
        /// </summary>
        /// <returns>Una lista de KeyValuePair con los posibles días del mes</returns>
        public List<KeyValuePair<string, string>> ListDays()
        {
            var days = new List<KeyValuePair<string, string>>();
            for (int i = 1; i <= 31; i++)
                days.Add(new KeyValuePair<string, string>(i.ToString().PadLeft(2, '0'), i.ToString().PadLeft(2, '0')));

            return days;
        }

        /// <summary>
        /// Lista los posibles meses de un año
        /// </summary>
        /// <returns>Una lista de KeyValuePair con los posibles meses del año</returns>
        public List<KeyValuePair<string, string>> ListMonths()
        {
            var months = new List<KeyValuePair<string, string>>();
            for (int i = 1; i <= 12; i++)
                months.Add(new KeyValuePair<string, string>(i.ToString().PadLeft(2, '0'), i.ToString().PadLeft(2, '0')));

            return months;
        }

        /// <summary>
        /// Lista los ciclos lectivos
        /// </summary>
        /// <returns>Una coleccion con los ciclos lectivos existentes</returns>
        public IList<SchoolYear> ListSchoolYears()
        {
            return Context.SchoolYears.OrderBy(x => x.Cycle).ToList();
        }

        /// <summary>
        /// Lista el ciclo lectivo activo
        /// </summary>
        /// <returns>Un ciclos lectivos</returns>
        public SchoolYear GetActiveSchoolYear()
        {
            return Context.SchoolYears.Where(x => x.Active == true).OrderBy(x => x.Cycle).FirstOrDefault();
        }

        /// <summary>
        /// Lista los posibles años
        /// </summary>
        /// <returns>Una lista de KeyValuePair con los posibles años</returns>
        public List<KeyValuePair<string, string>> ListYears()
        {
            var years = new List<KeyValuePair<string, string>>();
            var schoolYears = ListSchoolYears();
            foreach (var schoolYear in schoolYears)
                years.Add(new KeyValuePair<string, string>(schoolYear.Cycle, schoolYear.Cycle));

            return years;
        }


       

        /// <summary>
        /// Obtiene datos de la institución a traves de un Web Service del ministerio
        /// </summary>
        /// <param name="cue">CUE de la institución de la cual se desea obtener info</param>
        /// <returns>Devuelve un institutionDTO con la info de la institución perteneciente al CUE.</returns>
        protected InstitutionDTO GetInstitutionData(string cue)
        {
            var dto = new InstitutionDTO();

            try
            {
                if (cue.Length < 9) //caba o Bs As con 0!
                    cue = "0" + cue;
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);


            
                
                var service = new wsINET.ConsultaRegistro();
                service.Credentials= System.Net.CredentialCache.DefaultCredentials;
                var data = service.obtenerDatosInstitucion(cue.Trim());

                if (data.cue == "0")
                    return null;

                if (data.cue.Length < 9) //caba o Bs As con 0!
                    data.cue = "0" + data.cue;

                dto.CUE = data.cue;
                dto.Name = data.tipo;
                dto.Name = data.nombre;
                dto.Province = data.provincia;
                dto.Department = data.departamento;
                dto.Locality = data.localidad;
                dto.Orientation = data.orientacion;
                dto.Type = data.claseinstitucion;
                dto.Active = data.activo;
                dto.Ambit = data.ambito;
                dto.Specializations = data.especializaciones;
                dto.Level = data.claseinstitucion;
                dto.LevelInt = data.claseinstit;
                dto.Dependence = data.dependencia;


            } catch (Exception ex)
            {
                return null;
               // throw new Exception(ex.Message);

            }
            return dto;
        }

        public static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Lista las especializaciones
        /// </summary>
        /// <param name="serviceSpecializations">Listado de especializaciones que devuelve el servicio</param>
        /// <returns>Una colección de KeyValuePair formateada en base a las especializaciones que devuelve el WebService</returns>
        public List<KeyValuePair<string, string>> ListSpecializations(string serviceSpecializations)
        {
            serviceSpecializations += "|TODOS - Todos";
            if (string.IsNullOrEmpty(serviceSpecializations))
                return new List<KeyValuePair<string, string>>();
            return serviceSpecializations.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(x => new KeyValuePair<string, string>(x.Trim(), x.Trim()))
                                            .OrderBy(x => x.Value)
                                            .ToList();
        }

        private AuditLogger Logger { get; set; }

        /// <summary>
        /// Obtiene la descripcion en castellano de una entidad del sistema
        /// </summary>
        /// <param name="entityName">Entidad de la cual se desea obtener su descripción</param>        
        /// <returns>El nombre en castellano de la entidad correspondiente</returns>
        /// <remarks>Esto es práctico para armar los mensajes de auditoría</remarks>        
        public string GetEntityDescription(string entityName)
        {
            var entityDescription = "";
            switch (entityName)
            {
                case "ImprovementPlan":
                    entityDescription = "Plan de Mejora";
                    break;

                case "Solicitude":
                    entityDescription = "Solicitado";
                    break;

                case "Dictum":
                    entityDescription = "Dictamen";
                    break;

                case "Resolution":
                    entityDescription = "Disposiciones";
                    break;
            }

            return entityDescription;
        }

        /// <summary>
        /// Lista todos los eje por tipo de plantilla
        /// </summary>
        /// <param name="templateTypeId">Id del tipo de plantilla</param>
        /// <returns>Una lista con los eje correspondientes a un determinado tipo de plantilla</returns>
        public IList<TemplateTypeField> ListTemplateTypeFields(int templateTypeId)
        {
            return Context.TemplateTypeFields.Where(x => x.TemplateTypeId == templateTypeId).OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Lista todos los bloques de acción por plantilla
        /// </summary>
        /// <param name="templateTypeId">Id del tipo de plantilla</param>
        /// <returns>Una lista con los bloques de acción correspondientes a un determinado tipo de plantilla</returns>
        public IList<TemplateTypeBlock> ListTemplateTypeBlocks(int templateTypeId)
        {
            return Context.TemplateTypeBlocks.Where(x => x.TemplateTypeId == templateTypeId).OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Lista todas las variables por plantilla
        /// </summary>
        /// <param name="templateId">Id de la plantilla</param>
        /// <returns>Una lista con todas las variables correspondientes a un determinado tipo de plantilla</returns>
        public IList<TemplateVariable> ListTemplateVariables(int templateId)
        {
            return Context.TemplateVariables.Where(x => x.TemplateId == templateId).OrderBy(x => x.Variable).ToList();
        }

        /// <summary>
        /// Lista los tipos de plantillas
        /// </summary>
        /// <returns>Una lista con los diferentes tipos de plantillas</returns>
        public IList<TemplateType> ListTemplateTypes()
        {
            return Context.TemplateTypes.OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Comienza el monitoreo para realizar un log de auditoría
        /// </summary>
        protected void BeginAuditLog()
        {
            Logger = new AuditLogger(Context, GetAuditConfifuration());
        }


        /// <summary>
        /// Finaliza el monitoreo para realizar un log de auditoría
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora asociado a la auditoría</param>
        protected void EndAuditLog(int improvementPlanId)
        {
            EndAuditLog(improvementPlanId, null);
        }

        /// <summary>
        /// Finaliza el monitoreo para realizar un log de auditoría
        /// <param name="improvementPlanId">Id del plan de mejora asociado a la auditoría</param>
        /// <param name="additionalData">Información adicional que desea loguearse</param>
        /// </summary>
        protected void EndAuditLog(int improvementPlanId, string additionalData)
        {
            foreach (var entity in Logger.LastLog.Entities)
            {
                foreach (var property in entity.Properties)
                {
                    // La librería no discrimina bien las propiedades heredadas de una super clase. Por eso, si por ejemplo 
                    // es un documento, dicrimino aparte las propiedades de la clase base que no quiero auditar.
                    if (entity.EntityType.BaseType != null && ExcludedBaseProperty(entity.EntityType.BaseType, property.Name))
                        continue;

                    // Grabo la auditoría
                    if (entity.Current as IAuditable != null)
                    {
                        
                        var audit = new Audit();
                        audit.Action = entity.Action.ToString();
                        audit.EntityId = ((IAuditable)entity.Current).Id;
                        audit.EntityName = entity.EntityType.Name;
                        audit.PropertyName = property.Name;
                        audit.UserId = SessionHelper.CurrentUserId;
                        audit.TimeStamp = Logger.LastLog.Date;
                        audit.OriginalValue = Convert.ToString(property.Original);
                        audit.AdditionalData = additionalData;
                        audit.CurrentValue = (property.Current != null) ? property.Current.ToString() : null;
                        audit.ImprovementPlanId = improvementPlanId; // Asocio la auditoría al plan de mejora                        
                        try
                        {
                            Context.Audits.Add(audit);
                            Context.SaveChanges();
                        }
                        catch (Exception ex) {
                            throw ex;
                        }
                        
                    }
                }
            }
        }
        
        /// <summary>
        /// Obtiene la configuración a aplicar en la auditoría
        /// </summary>
        /// <returns>Un objeto AuditConfiguration con la configuracion de que entidades y propiedades auditar</returns>
        /// <remarks>La idea es dejar como no auditable la propiedad que quiero auditar. Por ejemplo, no le seteo no Auditable 
        /// al estado, pero si al statusId. De esa forma, al modificarse el estado, lo audito en base a su descripción.
        /// </remarks>
        private AuditConfiguration GetAuditConfifuration()
        {
            var auditConfiguration = AuditConfiguration.Default;

            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;

            // Customizo la auditoría del plan de mejora (solo audito el cambio de estado)
            auditConfiguration.IsAuditable<ImprovementPlan>()
                .NotAudited(x => x.Comments)
                .NotAudited(x => x.CUE)                
                .NotAudited(x => x.FieldId)
                .NotAudited(x => x.Field)
                .NotAudited(x => x.Id)
                .NotAudited(x => x.Identifier)
                .NotAudited(x => x.ImprovementPlans1)
                .NotAudited(x => x.ImprovementPlanTypeId)
                .NotAudited(x => x.ImprovementPlansType)
                .NotAudited(x => x.Incidences)
                .NotAudited(x => x.Parent)
                .NotAudited(x => x.ReceptionDate)
                .NotAudited(x => x.SchoolYearId)
                .NotAudited(x => x.SchoolYear)
                .NotAudited(x => x.Solicitudes)
                .NotAudited(x => x.Summary)
                .NotAudited(x => x.UserProfile)
                .NotAudited(x => x.Attachment)
                .NotAudited(x => x.AttachmentAdd)     
                .NotAudited(x => x.Documentation)
                .NotAudited(x => x.StatusId);          

            // Customizo la auditoría de los solicitados (solo audito el cambio de estado)
            auditConfiguration.IsAuditable<Solicitude>()
                .NotAudited(x => x.ApprovedAmount)
                .NotAudited(x => x.ApprovedPriceUnit)
                .NotAudited(x => x.ApprovedTotal)
                .NotAudited(x => x.Comments)
                .NotAudited(x => x.CUE)
                .NotAudited(x => x.Details)
                .NotAudited(x => x.Dictums)
                .NotAudited(x => x.ExpenditureType)
                .NotAudited(x => x.ExpenditureTypeId)
                .NotAudited(x => x.FileNumber)
                .NotAudited(x => x.Id)
                .NotAudited(x => x.ImprovementPlan)
                .NotAudited(x => x.ImprovementPlanId)
                .NotAudited(x => x.Incidences)
                .NotAudited(x => x.Line)
                .NotAudited(x => x.LineId)
                .NotAudited(x => x.MeasurementUnit)
                .NotAudited(x => x.MeasurementUnitId)
                .NotAudited(x => x.ReassignedId)
                .NotAudited(x => x.RequestedAmount)
                .NotAudited(x => x.RequestedPriceUnit)
                .NotAudited(x => x.RequestedTotal)
                .NotAudited(x => x.SchoolYear)
                .NotAudited(x => x.SchoolYearId)
                .NotAudited(x => x.SolicitudeRelationship)
                .NotAudited(x => x.ReassignedId)
                .NotAudited(x => x.Reasigned)
                .NotAudited(x => x.ReassignedRequestedTotal)
                .NotAudited(x => x.ReassignedGrantedTotal)
                .NotAudited(x => x.SolicitudeType)
                .NotAudited(x => x.SolicitudeTypeId)
                .NotAudited(x => x.Specialization)
                .NotAudited(x => x.Locked)
                .NotAudited(x => x.StatusId);

            // Customizo la auditoría de los dictámenes
            // Las propiedades heredadas de la clase documento no las discrimina correctamente
            // Se quitan con el método ExcludedBaseProperty
            auditConfiguration.IsAuditable<Dictum>()
                .NotAudited(x => x.Ammount)
                .NotAudited(x => x.Balance)
                .NotAudited(x => x.Body)
                .NotAudited(x => x.CreationDate)
                .NotAudited(x => x.CreationUser)
                .NotAudited(x => x.CreationUserId)
                .NotAudited(x => x.DocumentVariables)
                .NotAudited(x => x.ElegibilityExternalEntity)
                .NotAudited(x => x.EligibilityExternalNumber)
                .NotAudited(x => x.EligibilityRequired)
                .NotAudited(x => x.EligibilityStatusId)
                .NotAudited(x => x.EmissionDate)
                .NotAudited(x => x.FileNumber)
                .NotAudited(x => x.Id)
                .NotAudited(x => x.ImprovementPlan)
                .NotAudited(x => x.ImprovementPlanId)
                .NotAudited(x => x.Locked)
                .NotAudited(x => x.Number)
                .NotAudited(x => x.PDF)
                .NotAudited(x => x.Resolutions)
                .NotAudited(x => x.SignatureDate)
                .NotAudited(x => x.Solicitudes)
                .NotAudited(x => x.StatusId)
                .NotAudited(x => x.Template)
                .NotAudited(x => x.TemplateId);

            // Customizo la auditoría de las Disposiciones
            // Las propiedades heredadas de la clase documento no las discrimina correctamente
            // Se quitan con el método ExcludedBaseProperty
            auditConfiguration.IsAuditable<Resolution>()
                .NotAudited(x => x.AmountExecuted)
                .NotAudited(x => x.Annex)
                .NotAudited(x => x.Body)
                .NotAudited(x => x.CreationDate)
                .NotAudited(x => x.CreationUser)
                .NotAudited(x => x.CreationUserId)
                .NotAudited(x => x.Dictums)
                .NotAudited(x => x.DocumentVariables)
                .NotAudited(x => x.EmissionDate)
                .NotAudited(x => x.Id)
                .NotAudited(x => x.ImprovementPlan)
                .NotAudited(x => x.ImprovementPlanId)
                .NotAudited(x => x.Number)
                .NotAudited(x => x.PDF)
                .NotAudited(x => x.ProtocolizedDate)
                .NotAudited(x => x.ResolutionNumber)
                .NotAudited(x => x.ShipDate)
                .NotAudited(x => x.SignatureDate)
                .NotAudited(x => x.StatusId)
                .NotAudited(x => x.Template)
                .NotAudited(x => x.TemplateId);
            
            // Evito que se audite el FileNumber
            auditConfiguration.IsAuditable<FileNumber>()
                .NotAudited(x => x.Id)
                .NotAudited(x => x.LastFileNumber);

            // Seteo que el status se audite como su Descripción
            auditConfiguration.IsAuditable<Status>().DisplayMember(t => t.Description);

            return auditConfiguration;
        }

        /// <summary>
        /// Determina si se debe considerar alguna propiedad (perteneciente a una super clase) como no auditable.
        /// </summary>
        /// <param name="propertyName">Propiedad que se desea saber si se desea auditar</param>
        /// <returns>True en caso de que se desee excluir la propiedad de las propiedades a editar. False en caso contrario.</returns>
        private bool ExcludedBaseProperty(Type type, string propertyName)
        {
            var inst = Activator.CreateInstance(type);
            var properties = inst.GetType().GetProperties();
            return propertyName != "Status" && properties.Select(x => x.Name).Contains(propertyName);
        }

        static public class ReportHelper
        {
            /// <summary>
            /// Aplica formato para el titulo del reporte
            /// </summary>
            /// <param name="hRow"></param>
            static public void ApplyHeaderTitleFormatToRow(IXLRow hRow)
            {
                hRow.Height = 22;
                var hStyle = hRow.Style;
                hStyle.Font.Bold = true;
                hStyle.Font.FontSize = 16;
                hStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                hStyle.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                hStyle.NumberFormat.Format = "";
            }
        }
    }    
}
