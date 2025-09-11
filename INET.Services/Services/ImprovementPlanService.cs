using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using INET.Core.Enums;
using INET.Core.Constants;
using INET.Data;
using INET.Services.DTO;
using INET.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web.Security;
using INET.Utils.Helpers.Permissions;
using log4net;
namespace INET.Services
{
    /// <summary>
    /// Servicio de Planes de Mejora
    /// </summary>
    public class ImprovementPlanService : BusinessService
    {
        SolicitudeService _solicitudeService;
        FileNumberService _fileNumberService;
        IncidenceService _incidenceService;
        UserProfileService _userProfileService;
        ImportService _importService;
        DocumentService _documentService;
        TemplateService _templateService;
        AccountingService _accountingService;
        protected static readonly ILog FileSystemLog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ImprovementPlanService(INETContext context, SolicitudeService solicitudeService, FileNumberService fileNumberService, IncidenceService incidenceService, UserProfileService userProfileService, ImportService importService, DocumentService documentService, TemplateService templateService)
        {
            Context = context;
            _solicitudeService = solicitudeService;
            _fileNumberService = fileNumberService;
            _incidenceService = incidenceService;
            _userProfileService = userProfileService;
            _importService = importService;
            _documentService = documentService;
            _templateService = templateService;
            _accountingService = new AccountingService(Context);
        }

        /// <summary>
        /// Do comments count for plan
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public int countCommentsFor(int planId)
        {
            return Context.Comments.Where(x => x.ImprovementPlans.Where(y => y.Id == planId).Any() && x.Solicitudes.Count() == 0).Count();
        }

        /// <summary>
        /// Do incidences count for plan
        /// </summary>
        /// <param name="planId"></param>
        /// <returns>Int</returns>
        public int countIncidencesFor(int planId)
        {
            return Context.Incidences.Where(x => x.ImprovementPlanId == planId).Count();
        }

        /// <summary>
        /// Get general final solicitudes status
        /// </summary>
        /// <param name="solicitudesCount"></param>
        /// <param name="solicitudesApproved"></param>
        /// <param name="solicitudesRejected"></param>
        /// <returns>String</returns>
        public string CalculateOverallSolicitudesStatus(int solicitudesCount, int solicitudesApproved, int solicitudesRejected)
        {
            if (solicitudesRejected > 0 && solicitudesRejected == solicitudesCount)
                return "Rechazo";

            if (solicitudesApproved > 0 && solicitudesApproved < solicitudesCount)
                return "Aprobación Parcial";

            if (solicitudesApproved > 0 && solicitudesApproved == solicitudesCount)
                return "Aprobación Total";

            if (solicitudesCount > 0 && solicitudesApproved == 0 && solicitudesRejected == 0)
                return "Aprobación Pendiente";

            return "-";
        }

        public ImprovementPlan GetById(int id)
        {
            return Context.ImprovementPlans
                .Include("Incidences").Include("Incidences.IncidenceType").Include("Incidences.UserProfile").Include("Incidences.Comments")
                .Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Lista los estados del plan de acuerdo al perfil del usuario actual
        /// </summary>
        /// <returns>Una lista con los estados del plan</returns>
        public IList<Status> ListStatusForPlan()
        {
            return Context.Status.Where(x => x.StatusCriterionId == (int)StatusCriterionEnum.Stage).ToList();
        }

        /// <summary>
        /// Lista los estados del plan de acuerdo al perfil del usuario actual y al estado del plan
        /// </summary>
        /// <param name="currentStatusId">Estado en el que se encuentra el plan</param>
        /// <returns>Una lista con los estados del plan</returns>
        public IList<Status> ListStatusForPlan(int currentStatusId)
        {
            IList<Status> list = new List<Status>();

            switch (currentStatusId)
            {
                case ((int)StageStatusEnum.EnCargaProvincia):
                    list = Context.Status.Where(x => x.Id == (int)StageStatusEnum.EnCargaProvincia
                            || x.Id == (int)StageStatusEnum.AElevarProvincia).ToList();
                    break;

                case ((int)StageStatusEnum.AElevarProvincia):
                    list = Context.Status.Where(x => x.Id == (int)StageStatusEnum.EnCargaProvincia
                        || x.Id == (int)StageStatusEnum.AElevarProvincia
                        || x.Id == (int)StageStatusEnum.EnComisionRecepcionProvincia).ToList();
                    break;
                case ((int)StageStatusEnum.EnComisionRecepcionProvincia):
                    list = Context.Status.Where(x => x.Id == (int)StageStatusEnum.AElevarProvincia
                        || x.Id == (int)StageStatusEnum.EnComisionRecepcionProvincia
                        || x.Id == (int)StageStatusEnum.EnIngreso).ToList();
                    break;
                case ((int)StageStatusEnum.EnIngreso):
                    list = Context.Status.Where(x => x.Id == (int)StageStatusEnum.EnComisionRecepcionProvincia
                        || x.Id == (int)StageStatusEnum.EnIngreso
                        || x.Id == (int)StageStatusEnum.EnEvaluacion).ToList();
                    break;
                case ((int)StageStatusEnum.EnEvaluacion):
                    list = Context.Status.Where(x => x.Id == (int)StageStatusEnum.EnIngreso
                            || x.Id == (int)StageStatusEnum.EnEvaluacion
                            || x.Id == (int)StageStatusEnum.EnAdministracion).ToList();
                    break;
                case ((int)StageStatusEnum.EnAdministracion):
                    list = Context.Status.Where(x => x.Id == (int)StageStatusEnum.EnEvaluacion
                            || x.Id == (int)StageStatusEnum.EnAdministracion
                            || x.Id == (int)StageStatusEnum.Anulado
                            || x.Id == (int)StageStatusEnum.Cerrado).ToList();
                    break;
                case ((int)StageStatusEnum.Cerrado):
                    list = Context.Status.Where(x => x.Id == (int)StageStatusEnum.EnAdministracion
                            || x.Id == (int)StageStatusEnum.Cerrado).ToList();
                    break;
                case ((int)StageStatusEnum.Anulado):
                    list = Context.Status.Where(x => x.Id == (int)StageStatusEnum.EnAdministracion
                        || x.Id == (int)StageStatusEnum.Anulado).ToList();
                    break;
            }

            return list;
        }

        /// <summary>
        /// Cambia el estado de un plan de mejora
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora del cual se desea cambiar el estado</param>        
        /// <param name="statusId">Id del estado</param>
        /// <returns>True en caso de que se haya modificado el estado. False en caso contrario</returns>
        public bool ChangeStatus(int improvementPlanId, int statusId)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).FirstOrDefault();

            if (plan != null)
            {

                if (!ListStatusForPlan(plan.StatusId).Any(x => x.Id == statusId))
                {
                    return false;
                }

                // Valido que todos los solicitados tengan línea para poder pasarlo a Evaluación, medida de seguridad.
                if ((StageStatusEnum)plan.StatusId == StageStatusEnum.EnIngreso)
                {
                    // all correct, set to field date
                    if (plan.Solicitudes.Where(x => x.LineId == null).Any() || plan.Solicitudes.Count < 0)
                        return false;
                }

                // Valido que tenga completos los numeros de expediente para pasar a evaluacion
                if ((StageStatusEnum)statusId == StageStatusEnum.EnEvaluacion)
                {
                    // all correct, set to field date
                    if (!plan.FieldDate.HasValue)
                        plan.FieldDate = DateTime.Now;
                }

                // Cambio el estado
                BeginAuditLog();
                plan.StatusId = statusId;
                Context.SaveChanges();
                EndAuditLog(plan.Id);

                return true;
            }

            return false;
        }

        public void AddAttachmentToPlan(ImprovementPlan plan, System.Web.HttpPostedFileBase attachment)
        {
            BeginAuditLog();
            // Salvo el archivo en el disco
            var _filePath = ConfigurationManager.AppSettings.Get("SaveAnnexPath");
            if (!_filePath.EndsWith("\\"))
                _filePath = _filePath + "\\";

            // Creo un directorio único para el anexo
            var directoryName = DateTime.Now.Ticks;
            var filePath = _filePath + directoryName;
            var di = new DirectoryInfo(filePath);
            var filename = "AT-" + attachment.FileName;
            di.Create();

            // Guardo el archivo en el disco
            attachment.SaveAs(string.Concat(filePath, "\\", filename));

            // Eliminar documento anterior
            if (plan.Attachment != null)
            {
                var parts = plan.Attachment.Split('/');
                var old_attachment = _filePath + parts[1];
                if (System.IO.Directory.Exists(old_attachment)) System.IO.Directory.Delete(old_attachment, true);
            }

            plan.Attachment = string.Concat(directoryName, "/", filename);

            Context.SaveChanges();
            EndAuditLog(plan.Id, "Se modificó el adjunto del plan");
        }

        public void AddAttachmentExtraToPlan(ImprovementPlan plan, System.Web.HttpPostedFileBase attachment)
        {
            BeginAuditLog();
            // Salvo el archivo en el disco
            var _filePath = ConfigurationManager.AppSettings.Get("SaveAnnexPath");
            if (!_filePath.EndsWith("\\"))
                _filePath = _filePath + "\\";

            // Creo un directorio único para el anexo
            var directoryName = DateTime.Now.Ticks;
            var filePath = _filePath + directoryName;
            var di = new DirectoryInfo(filePath);
            var filename = "AT2-" + attachment.FileName;
            di.Create();

            // Guardo el archivo en el disco
            attachment.SaveAs(string.Concat(filePath, "\\", filename));

            // Eliminar documento anterior
            if (plan.AttachmentAdd != null)
            {
                var parts = plan.AttachmentAdd.Split('/');
                var old_attachment = _filePath + parts[1];
                if (System.IO.Directory.Exists(old_attachment)) System.IO.Directory.Delete(old_attachment, true);
            }

            plan.AttachmentAdd = string.Concat(directoryName, "/", filename);

            Context.SaveChanges();
            EndAuditLog(plan.Id, "Se modificó el adjunto extra  del plan");
        }

        public void AddDocumentationToPlan(ImprovementPlan plan, System.Web.HttpPostedFileBase attachment)
        {
            BeginAuditLog();
            // Salvo el archivo en el disco
            var _filePath = ConfigurationManager.AppSettings.Get("SaveAnnexPath");
            if (!_filePath.EndsWith("\\"))
                _filePath = _filePath + "\\";

            // Creo un directorio único para el anexo
            var directoryName = DateTime.Now.Ticks;
            var filePath = _filePath + directoryName;
            var di = new DirectoryInfo(filePath);
            var filename = "JP-" + attachment.FileName;
            di.Create();

            // Guardo el archivo en el disco
            attachment.SaveAs(string.Concat(filePath, "\\", filename));

            // Eliminar documento anterior
            if (plan.Documentation != null)
            {
                var parts = plan.Documentation.Split('/');
                var old_attachment = _filePath + parts[1];
                if (System.IO.Directory.Exists(old_attachment)) System.IO.Directory.Delete(old_attachment, true);
            }

            plan.Documentation = string.Concat(directoryName, "/", filename);

            Context.SaveChanges();
            EndAuditLog(plan.Id, "Se modificó la documentación pedagógica del plan");
        }

        /// <summary>
        /// Find if a given plan ID has Documentation allready added
        /// </summary>
        /// <param name="plan_id"></param>
        /// <returns></returns>
        public bool HasDocumentation(int plan_id)
        {
            return plan_id == 0 ? false : Context.ImprovementPlans.Where(x => x.Id == plan_id && x.Documentation != null).Count() > 0;
        }

        /// <summary>
        /// Guarda los detalles y el resumen de un plan de mejora
        /// </summary>
        /// <param name="dto">DTO del plan a guardar</param>
        /// <returns>El plan de mejora guardado</returns>
        public ImprovementPlan Save(ImprovementPlanEditorDTO dto)
        {
            BeginAuditLog();

            var plan = (dto.Id == 0)
                ? new ImprovementPlan()
                : Context.ImprovementPlans.Where(x => x.Id == dto.Id).First();

            // Valido que el usuario pueda crear un plan de mejora
            if (dto.Id == 0 && !SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.CREATE))
                throw new ApplicationException("El usuario no posee permisos para crear un plan de mejora");

            // Valido que el usuario pueda editar un plan de mejora
            if (dto.Id > 0 && (!SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.EDIT, plan.StatusId) || !ImprovementPlanPermissions.HasAccess(plan)))
                throw new ApplicationException("El usuario no posee permisos para editar un plan de mejora");

            var fileNumber = _fileNumberService.LastFileNumber();

            plan.CUE = dto.CUE;
            plan.Articulator = dto.Articulator;
            if (dto.ArticulatorExp==null)
                plan.ArticulatorExp ="";
                    else
                plan.ArticulatorExp = dto.ArticulatorExp ;
            if (dto.Summary == null)
                plan.Summary = "";
            else
                plan.Summary = dto.Summary;
            plan.ReceptionDate = dto.ReceptionDate.Value;
            if (plan.Id == 0)
            {
                plan.SchoolYearId = dto.SchoolYearId;
                if (SessionHelper.HasAnyRole(new string[] { RoleConstants.OPERADOR_PROVINCIAL, RoleConstants.DELEGADO_PROVINCIAL_INET, RoleConstants.REFERENTE_JURISDICCIONAL }))
                {
                    plan.StatusId = StageStatusEnum.EnCargaProvincia.ToInt();
                }
                else
                {
                    plan.StatusId = StageStatusEnum.EnIngreso.ToInt();
                }
            }

            plan.ImprovementPlanTypeId = dto.ImprovementPlanTypeId;
            plan.FieldId = dto.FieldId;
            //Datos Eje y suub Ejes
            plan.LineId = dto.LineId;
            plan.Pronafe = dto.Pronafe;

            if (dto.Pronafe==false)
            {    plan.SubFieldId = dto.SubField;
                 plan.Line_22_Id = dto.Line22Id;
            } 
            if (plan.Id == 0)
            {
                plan.Identifier = String.Format("{0}-{1}-{2}-{3}", DateTime.Now.Year, plan.FieldId, plan.CUE, fileNumber);
                Context.ImprovementPlans.Add(plan);
            }

            // Importo solicitados del Excel
            if (dto.SolicitudesImportFile != null)
            {
                if (dto.SolicitudesImportFile.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    throw new Exception("El archivo a importar debe ser un excel válido");

                var importedSolicitudes= new List<Solicitude>();
                if (plan.Pronafe.Value==true) //pronafe busca la linea  2016
                    importedSolicitudes = _importService.ImportSolicitudes(dto.SolicitudesImportFile.InputStream,plan.FieldId, plan.LineId.Value, plan.SchoolYearId, plan.StatusId, System.IO.Path.GetExtension(dto.SolicitudesImportFile.FileName));
                else
                     importedSolicitudes= _importService.ImportSolicitudes(dto.SolicitudesImportFile.InputStream, plan.Line_22_Id.Value, plan.SchoolYearId,plan.StatusId, System.IO.Path.GetExtension(dto.SolicitudesImportFile.FileName));
                foreach (var impSolicitude in importedSolicitudes)
                    plan.Solicitudes.Add(impSolicitude);
            }

            // Determino si es una copia de otro plan
            if (dto.ParentId.HasValue)
            {
                var parent = Context.ImprovementPlans.Where(x => x.Id == dto.ParentId).First();
                var solicitudes = parent.Solicitudes.Where(x => x.StatusId != SolicitudeStatusEnum.Anulado.ToInt()).ToList();

                plan.ParentId = dto.ParentId.Value;

                foreach (var solicitude in solicitudes)
                {
                    var s = new Solicitude();
                    s.Id = solicitudes.IndexOf(solicitude);
                    s.CUE = solicitude.CUE;
                    s.SchoolYearId = solicitude.SchoolYearId;
                    s.Details = solicitude.Details;
                    s.LineId = solicitude.LineId;
                    s.ExpenditureTypeId = solicitude.ExpenditureTypeId;
                    s.Specialization = solicitude.Specialization;
                    s.MeasurementUnitId = solicitude.MeasurementUnitId;
                    s.StatusId = SolicitudeStatusEnum.Pendiente.ToInt();
                    s.RequestedAmount = solicitude.RequestedAmount;
                    s.RequestedPriceUnit = solicitude.RequestedPriceUnit;
                    s.FileNumber = "";

                    plan.Solicitudes.Add(s);
                }
            }



            Context.SaveChanges();
            EndAuditLog(plan.Id);

            return plan;
        }

        /// <summary>
        /// Lista todos los planes de mejora
        /// </summary>
        /// <returns>Una lista con todos los planes de mejora</returns>
        public IList<ImprovementPlan> ListImprovementPlans()
        {
            return Context.ImprovementPlans.ToList();
        }

        /// <summary>
        /// Lista los planes de mejoras paginados
        /// </summary>
        /// <param name="dto">DataTableDTO que representa el JQueryDataTable con la info de la tabla enviada por el cliente.</param>        
        /// <returns>Un objeto ImprovementPlanResultDTO que encapsula la respuesta con los resultados paginados</returns>
        public ImprovementPlanResultDTO ListImprovementPlans(DataTableDTO dto)
        {
            log4net.Config.XmlConfigurator.Configure();
            FileSystemLog.Info("paginated!");
            // Filtro los resultados si corresponde
            var pre_query = from plan in Context.ImprovementPlans
                            select new
                            {
                                plan,
                                year = (from year in Context.SchoolYears where year.Id == plan.SchoolYearId select year).FirstOrDefault(),
                                planType = (from planType in Context.ImprovementPlansTypes where planType.Id == plan.ImprovementPlanTypeId select planType).FirstOrDefault(),
                                field = (from _field in Context.Fields where _field.Id == plan.FieldId select _field).FirstOrDefault(),
                                status = (from _status in Context.Status where _status.Id == plan.StatusId select _status).FirstOrDefault(),
                                userProfile = (from _userProfile in Context.UserProfiles where _userProfile.UserId == plan.EvaluatorUserId select _userProfile).FirstOrDefault(),
                                incidencesCount = (from inci in Context.Incidences where inci.ImprovementPlanId == plan.Id && inci.SolicitudeId == null select inci).Count(),
                                commentsCount = plan.Comments.Count(),
                                solicitudesCount = (from solis in Context.Solicitudes where solis.ImprovementPlanId == plan.Id select solis).Count(),
                                approvedCount = (from solis in Context.Solicitudes where solis.ImprovementPlanId == plan.Id && solis.StatusId == (int)SolicitudeStatusEnum.Aprobado select solis).Count(),
                                rejectedCount = (from solis in Context.Solicitudes where solis.ImprovementPlanId == plan.Id && solis.StatusId == (int)SolicitudeStatusEnum.Rechazado select solis).Count(),
                                fileNumbers = (from solis in Context.Solicitudes where solis.ImprovementPlanId == plan.Id orderby solis.FileNumber select solis.FileNumber).Distinct()
                            };

            var totalRecords = pre_query.Count();
            if (dto.FilterResults)
            {
                if (!string.IsNullOrWhiteSpace(dto.sCustomSearch_Identifier))
                    pre_query = pre_query.Where(x => x.plan.Identifier.Contains(dto.sCustomSearch_Identifier.Trim()));

                if (!string.IsNullOrWhiteSpace(dto.sCustomSearch_Summary))
                    pre_query = pre_query.Where(x => x.plan.Summary.Contains(dto.sCustomSearch_Summary.Trim()));

                if (!string.IsNullOrEmpty(dto.sCustomSearch_FileNumber))
                {
                    //var fileNumberPlans = Context.Solicitudes.Where(x => x.FileNumber.Contains(dto.sCustomSearch_FileNumber.Trim())).Select(x => x.ImprovementPlanId).ToArray();
                    //pre_query = pre_query.Where(x => fileNumberPlans.Contains(x.plan.Id));

                    pre_query = pre_query.Where(x => Context.Solicitudes.Where(fn => fn.ImprovementPlanId == x.plan.Id && fn.FileNumber.Contains(dto.sCustomSearch_FileNumber.Trim())).Any());
                }

                if (!string.IsNullOrWhiteSpace(dto.sCustomSearch_CUE))
                    pre_query = pre_query.Where(x => x.plan.CUE.Contains(dto.sCustomSearch_CUE.Trim()));

                if (dto.iCustomSearch_SchoolYearId.HasValue)
                    pre_query = pre_query.Where(x => x.plan.SchoolYearId == dto.iCustomSearch_SchoolYearId.Value);

                if (dto.iCustomSearch_ImprovementPlanTypeId.HasValue)
                    pre_query = pre_query.Where(x => x.plan.ImprovementPlanTypeId == dto.iCustomSearch_ImprovementPlanTypeId.Value);

                if (dto.dCustomSearch_ReceptionDate.HasValue)
                    pre_query = pre_query.Where(x => x.plan.ReceptionDate == dto.dCustomSearch_ReceptionDate);

                if (dto.iCustomSearch_FieldId.HasValue)
                {
                    pre_query = pre_query.Where(x => x.plan.FieldId == dto.iCustomSearch_FieldId.Value);
                }
                else if (dto.fieldsIds.Count > 0)
                {
                    pre_query = pre_query.Where(x => dto.fieldsIds.Contains(x.plan.FieldId));
                }

                //Sia 09-03-22 filtro
                if (dto.sCustomSearch_Articulator!=null)
                {
                    pre_query = pre_query.Where(x => x.plan.Articulator.Contains(dto.sCustomSearch_Articulator));
                }
                if (dto.sCustomSearch_ArticulatorExp != null)
                {
                    pre_query = pre_query.Where(x => x.plan.ArticulatorExp.Contains(dto.sCustomSearch_ArticulatorExp));
                }


                if (dto.levelsIds.Count > 0)
                {
                    pre_query = pre_query.Where(x => x.plan.InstitutionLevelInt.HasValue && dto.levelsIds.Contains(x.plan.InstitutionLevelInt.Value));
                }

                if (dto.iCustomSearch_StatusId.HasValue)
                    pre_query = pre_query.Where(x => x.plan.StatusId == dto.iCustomSearch_StatusId.Value);

                if (dto.provNumbers.Count > 0)
                {
                    pre_query = pre_query.Where(x => dto.provNumbers.Contains(x.plan.CUE.Substring(0, 2)));
                }
                else if (dto.iCustomSearch_ProvinceId.HasValue)
                {
                    var province = Context.Provinces.Where(x => x.Id == dto.iCustomSearch_ProvinceId.Value).FirstOrDefault();
                    string province_number = province != null ? province.Number : "9999"; // if is a non province no result must return
                    pre_query = pre_query.Where(x => x.plan.CUE.StartsWith(province_number));
                }

                if (dto.iCustomSearch_EvaluatorId.HasValue)
                    pre_query = (dto.iCustomSearch_EvaluatorId.Value == -1)
                                ? pre_query.Where(x => x.plan.EvaluatorUserId == null)
                                : pre_query.Where(x => x.plan.EvaluatorUserId == dto.iCustomSearch_EvaluatorId.Value);
                if (dto.sCustomSearch_Dictum != null) // filtro por dictamen
                {
                    pre_query = pre_query.Where(x => x.plan.Solicitudes.Any(v=>v.Dictums.Any(b=> b.DictumNumber.Contains(dto.sCustomSearch_Dictum) )));


                }
            }

            // Establezco el orden dependiendo la columna
            // Función para ordenar los resultados

            switch (dto.iSortCol_0)
            {
                case 1:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.plan.Summary) : pre_query.OrderByDescending(x => x.plan.Summary);
                    break;
                case 2:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.planType.Description) : pre_query.OrderByDescending(x => x.planType.Description);
                    break;
                case 3:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.plan.CUE) : pre_query.OrderByDescending(x => x.plan.CUE);
                    break;
                //case 4:
                //    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.fileNumbers.FirstOrDefault()) : pre_query.OrderByDescending(x => x.fileNumbers.FirstOrDefault());
                //    break;
                case 5:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.year.Cycle) : pre_query.OrderByDescending(x => x.year.Cycle);
                    break;
                case 6:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.field.Code) : pre_query.OrderByDescending(x => x.field.Code);
                    break;
                case 7:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.solicitudesCount) : pre_query.OrderByDescending(x => x.solicitudesCount);
                    break;
                case 8:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.userProfile.Name + " " + x.userProfile.LastName) : pre_query.OrderByDescending(x => x.userProfile.Name + " " + x.userProfile.LastName);
                    break;
                case 9:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.commentsCount) : pre_query.OrderByDescending(x => x.commentsCount);
                    break;
                case 10:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.incidencesCount) : pre_query.OrderByDescending(x => x.incidencesCount);
                    break;
                case 11:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.status.Description) : pre_query.OrderByDescending(x => x.status.Description);
                    break;
                //        case 11: return c.StatusDescription;
                default:
                    pre_query = pre_query.OrderByDescending(x => x.plan.Id);
                    break;
            }

            // Armo el resultado
            var result = new ImprovementPlanResultDTO();
            result.Echo = dto.sEcho;
            result.TotalRecords = totalRecords;
            result.TotalDisplayRecords = (dto.FilterResults) ? pre_query.Count() : totalRecords;
            try { 
            result.FilteredImprovementPlans = pre_query.Select(x => new ImprovementPlanListItem
            {
                Plan = x.plan,
                ImprovementPlansTypeDescription = x.planType.Description,
                SchoolYearDescription = x.year.Description,
                FieldCode = x.field.Code,
                FieldDescription = x.field.Description,
                UserName = x.userProfile.Name + " " + x.userProfile.LastName,
                StatusDescription = x.status.Description,
                IncidencesCount = x.incidencesCount,
                CommentsCount = x.commentsCount,
                SolicitudesCount = x.solicitudesCount,
                ApprovedCount = x.approvedCount,
                RejectedCount = x.rejectedCount,
                FileNumbers = x.fileNumbers
            }).Skip(dto.iDisplayStart).Take(dto.iDisplayLength).ToList();
            return result;
            } catch (Exception ex)
            {
               ex.InnerException.Message.ToString();
                FileSystemLog.Error(ex);
                    throw new Exception("LOG!:" + ex.InnerException.Message.ToString()  );
                return null;
            }
        }



        public string  SubField_Line(ImprovementPlan p)
        {
            string sf = "";
            string l_22 = "";

            var Line = Context.Lines_22.Where(x => x.Id == p.Line_22_Id).FirstOrDefault(); 
            var SubField = Context.SubFields.Where(x => x.Id == p.SubFieldId).FirstOrDefault();
            if (Line != null)
                l_22 = Line.Code;
            if (SubField != null)
                sf = SubField.Code;

            return sf + "." + l_22;
        }
        /// <summary>
        /// Obtiene un plan de mejora para la edición
        /// </summary>
        /// <param name="id">Id del plan de mejora</param>
        /// <returns>Una dto con los datos del plan de mejora a editar</returns>
        public ImprovementPlanEditorDTO GetImprovementPlanForEdition(int id)
        {
            return Context.ImprovementPlans.Where(x => x.Id == id)
                                            .ToList()
                                            .Select(x => new ImprovementPlanEditorDTO()
                                            {
                                                Id = x.Id,
                                                Identifier = x.Identifier,
                                                CUE = x.CUE,
                                                FieldId = x.FieldId,
                                                ImprovementPlanTypeId = x.ImprovementPlanTypeId,
                                                ReceptionDateDay = x.ReceptionDate.ToString("dd"),
                                                ReceptionDateMonth = x.ReceptionDate.ToString("MM"),
                                                ReceptionDateYear = x.ReceptionDate.ToString("yyyy"),
                                                SchoolYearId = x.SchoolYearId,
                                                StatusId = x.StatusId,
                                                Summary = x.Summary,
                                                AttachmentURL = x.Attachment,
                                                DocumentationURL = x.Documentation,
                                                AttachmentAddURL = x.AttachmentAdd,
                                                Articulator = x.Articulator,
                                                ArticulatorExp =x.ArticulatorExp,
                                                SubField= x.SubFieldId,
                                                Line22Id =x.Line_22_Id,
                                                Pronafe  = x.Pronafe==null?false:x.Pronafe.Value
                                            }).First();
        }

        /// <summary>
        /// Lista el historial de un plan de mejora
        /// </summary>
        /// <param name="improvementPlanId">El id del plan de mejora del cual se desea listar el historial</param>
        /// <returns>El historial del plan de mejora correspondiente</returns>
        public List<string> ListTimeLineHistory(int improvementPlanId)
        {
            var timeLine = new List<string>();

            // Obtengo el historial del plan
            var audits = Context.Audits.Where(x => x.ImprovementPlanId == improvementPlanId).OrderByDescending(x => x.TimeStamp).ToList();
            foreach (var audit in audits)
            {
                var message = "";
                switch (audit.Action)
                {
                    case "Added":
                        message = GetCreateLogMessage(audit);
                        break;

                    case "Modified":
                        message = GetUpdateLogMessage(audit);
                        break;

                    case "Deleted":
                        message = GetDeleteLogMessage(audit);
                        break;
                }

                timeLine.Add(message);
            }

            return timeLine;
        }

        /// <summary>
        /// Obtiene el mensaje del historial para una entidad creada
        /// </summary>
        /// <param name="audit">Auditoría</param>
        /// <returns>El mensaje de la auditoría agregada</returns>
        private string GetCreateLogMessage(Audit audit)
        {
            if (audit.PropertyName == "EvaluatorUserId")
            {
                var userId = Convert.ToInt32(audit.CurrentValue);
                var userName = Context.UserProfiles.Where(x => x.UserId == userId).First().FullName;
                return string.Format("{0} - {1} - Agregó como evaluador al usuario '{2}'",
                                        audit.TimeStamp.ToString("dd/MM/yyyy"),
                                        audit.UserProfile.FullName,
                                        userName);
            }

            var description = "";
            switch (audit.EntityName)
            {
                case "Dictum":
                    description = string.Concat("el ", GetEntityDescription(audit.EntityName), " ", audit.AdditionalData);
                    break;
                case "Resolution":
                    description = string.Concat("la ", GetEntityDescription(audit.EntityName), " ", audit.AdditionalData);
                    break;
                case "Solicitude":
                    description = string.Concat("el ", GetEntityDescription(audit.EntityName), " ", audit.EntityId);
                    break;
                default:
                    description = string.Concat("un nuevo ", GetEntityDescription(audit.EntityName));
                    break;
            }

            return string.Format("{0} - {1} - Agregó {2} con estado '{3}'",
                                    audit.TimeStamp.ToString("dd/MM/yyyy"),
                                    audit.UserProfile.FullName,
                                    description,
                                    audit.CurrentValue);
        }

        /// <summary>
        /// Obtiene el mensaje del historial para una entidad actualizada
        /// </summary>
        /// <param name="audit">Auditoría</param>
        /// <returns>El mensaje de la auditoría modificada</returns>
        private string GetUpdateLogMessage(Audit audit)
        {
            if (audit.PropertyName == "EvaluatorUserId")
            {
                var userId = Convert.ToInt32(audit.CurrentValue);
                if (userId > 0)
                {


                    var userName = Context.UserProfiles.Where(x => x.UserId == userId).First().FullName;

                    return String.Format("{0} - {1} - Seleccionó como evaluador al usuario '{2}'",
                                                      audit.TimeStamp.ToString("dd/MM/yyyy"),
                                                      audit.UserProfile.FullName,
                                                  userName);
                } else {
                    return String.Format("{0} - {1} - Dejó el plan sin evaluador asignado",
                                                      audit.TimeStamp.ToString("dd/MM/yyyy"),
                                                      audit.UserProfile.FullName);
                }
        }

        var identifier = (audit.EntityName == "Dictum" || audit.EntityName == "Resolution")
                            ? audit.AdditionalData
                            : audit.EntityId.ToString();

            if (audit.PropertyName == "Documentation")
            {
                return String.Format("{0} - {1} - Modificó el XLS  del {2} número {3} de '{4}' a '{5}'",
                                                  audit.TimeStamp.ToString("dd/MM/yyyy"),
                                                  audit.UserProfile.FullName,
                                                  GetEntityDescription(audit.EntityName),
                                                  identifier,
                                                  audit.OriginalValue,
                                                  audit.CurrentValue);
            }

            if (audit.PropertyName == "Attachment")
            {
                return String.Format("{0} - {1} - Modificó la Información adicional del {2} número {3} de '{4}' a '{5}'",
                                                  audit.TimeStamp.ToString("dd/MM/yyyy"),
                                                  audit.UserProfile.FullName,
                                                  GetEntityDescription(audit.EntityName),
                                                  identifier,
                                                  audit.OriginalValue,
                                                  audit.CurrentValue);
            }

            return String.Format("{0} - {1} - Modificó el estado del {2} número {3} de '{4}' a '{5}'",
                                                  audit.TimeStamp.ToString("dd/MM/yyyy"),
                                                  audit.UserProfile.FullName,
                                                  GetEntityDescription(audit.EntityName),
                                                  identifier,
                                                  audit.OriginalValue,
                                                  audit.CurrentValue);
        }

        /// <summary>
        /// Obtiene el mensaje del historial para una entidad eliminada
        /// </summary>
        /// <param name="audit">Auditoría</param>
        /// <returns>El mensaje de la auditoría eliminada</returns>
        private string GetDeleteLogMessage(Audit audit)
{
    if (audit.PropertyName == "EvaluatorUserId")
    {
        var userId = Convert.ToInt32(audit.CurrentValue);
        var userName = Context.UserProfiles.Where(x => x.UserId == userId).First().FullName;
        string.Format("{0} - {1} - Eliminó como evaluador al usuario {2}",
                                       audit.TimeStamp.ToString("dd/MM/yyyy"),
                                       audit.UserProfile.FullName,
                                       userName);
    }

    var identifier = (audit.EntityName == "Dictum" || audit.EntityName == "Resolution")
                        ? audit.AdditionalData
                        : audit.EntityId.ToString();

    return string.Format("{0} - {1} - Eliminó el {2} número {3}",
                                    audit.TimeStamp.ToString("dd/MM/yyyy"),
                                    audit.UserProfile.FullName,
                                    GetEntityDescription(audit.EntityName),
                                    identifier);
}

/// <summary>
/// Devuelve si hay un expediente repetido
/// </summary>
/// <param name="fileNumbers">Lista de números de expediente a guardar</param>
/// <returns>True en caso de que no haya repetidos</returns>
public bool FileNumbersNotRepeated(List<string> fileNumbers)
{
    var duplicateItems = fileNumbers.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

    if (duplicateItems.Count == 0)
        return true;
    else
        return false;
}

/// <summary>
/// Salva una lista de Nros de Expedientes (fileNumnbers)
/// </summary>
/// <param name="improvementPlanId">Id del plan de mejora</param>
/// <param name="fileNumbers">Lista de números de expediente a guardar</param>
/// <returns>True en caso de que se haya salvado la lista completa. False en caso contrario</returns>
public bool SaveFileNumbers(int improvementPlanId, IList<FileNumberDTO> fileNumbers)
{
    try
    {
        // Determino si todos los números de expediente a asignar son válidos
        if (!IsValidFileNumber(improvementPlanId, fileNumbers.Select(x => x.FileNumber.Trim()).ToArray()))
            return false;

        using (var tran = new TransactionScope())
        {
            var plan = GetById(improvementPlanId);

            if (plan.StatusId == (int)StageStatusEnum.EnEvaluacion)
            {
                foreach (var fileNumber in fileNumbers)
                {
                    var solicitudes = plan.Solicitudes.Where(x => x.LineId == fileNumber.LineId);
                    foreach (var solicitude in solicitudes)
                    {
                        if (solicitude.StatusId != SolicitudeStatusEnum.Anulado.ToInt())
                            solicitude.FileNumber = fileNumber.FileNumber;
                    }
                    var dictums = solicitudes.SelectMany(x => x.Dictums);
                    foreach (var dictum in dictums)
                    {
                        dictum.FileNumber = fileNumber.FileNumber;
                    }
                }
                Context.SaveChanges();
                tran.Complete();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    catch (Exception)
    {
        return false;
    }
}

/// <summary>
/// Determina si un número de expediente es válido
/// </summary>
/// <param name="improvementPlanId">Id del plan de mejora</param>
/// <param name="fileNumbers">Números de expedientes</param>
/// <returns></returns>
private bool IsValidFileNumber(int improvementPlanId, IEnumerable<string> fileNumbers)
{
    //return Context.Solicitudes.Where(x => x.ImprovementPlanId != improvementPlanId && fileNumbers.Contains(x.FileNumber)).Count() == 0 && FileNumbersNotRepeated(fileNumbers.ToList());
    return Context.Solicitudes.Where(x => x.ImprovementPlanId != improvementPlanId && fileNumbers.Contains(x.FileNumber)).Count() == 0;
}

/// <summary>
/// Obtiene los detalles de un plan de mejora para poder visualizarlo
/// </summary>
/// <param name="id">Id del plan de mejora del cual se desean obtener los detalles</param>
/// <returns>Un ImprovementPlanDetailsDTO con los detalles del plan de mejora solicitado</returns>
public ImprovementPlanDetailsDTO GetImprovementPlanForVisualization(int id)
{
    var plan = Context.ImprovementPlans.Include("Solicitudes").Include("Incidences").Include("Comments").Where(x => x.Id == id).First();

    ImprovementPlanDetailsDTO dto = new ImprovementPlanDetailsDTO();
    dto.Id = plan.Id;
    dto.AttachmentURL = plan.Attachment;
    dto.DocumentationURL = plan.Documentation;
    dto.AttachmentAddURL = plan.AttachmentAdd;
    dto.Identifier = plan.Identifier;
    dto.ImprovementPlanType = plan.ImprovementPlansType.Description;
    dto.ReceptionDate = plan.ReceptionDate;
    dto.SchoolYearId = plan.SchoolYear.Id;
    dto.SchoolYear = plan.SchoolYear.Description;
    dto.Solicitudes = plan.Solicitudes.OrderByDescending(x => x.Id).ToList();
    dto.StatusId = plan.StatusId;
    dto.Status = plan.Status.Description;
    dto.Summary = plan.Summary;
    dto.User = (plan.UserProfile != null) ? plan.UserProfile.FullName : "-";
    dto.CUE = plan.CUE;
    dto.ProvinceId = GetProvinceByNumber(plan.CUE.Substring(0, 2)).Id;
    dto.FieldId = plan.Field.Id;
    dto.Field = plan.Field.Description;
    if (plan.Pronafe==null)
            {
                dto.Pronafe = "NO";
            }
    else
            { if (plan.Pronafe == true)
                    dto.Pronafe = "SI";
                else
                    dto.Pronafe = "NO";
            }


            if (plan.SubFieldId != null)
       {
                dto.SubField = plan.SubField.Description;
       }

            if (plan.LineId != null)
            {
                dto.Line = plan.Line.Description;
                dto.LineId = plan.LineId;
            }
        
            if (plan.Lines_22 != null)
            {
                dto.Line22 =plan.Lines_22.Description;
                dto.Line22_Xls_Sheet = plan.Lines_22.title_xls;
                dto.Xls_Col = "H";
                dto.Line22Id = plan.Line_22_Id;
            } else
            {
                dto.Line22_Xls_Sheet = plan.Line!=null? plan.Line.title_xls: "";
                dto.Xls_Col = "I";

            }

            dto.ParentIdentifier = (plan.ParentId.HasValue) ? plan.Parent.Identifier : "Ninguno";
            dto.ParentId = plan.ParentId;
    dto.Articulator = plan.Articulator;
    dto.ArticulatorExp = plan.ArticulatorExp;
    if (!String.IsNullOrEmpty(plan.CUE))
    {
        dto.Institution = GetInstitutionData(plan.CUE);
        if (dto.Institution != null)
        {
            plan.InstitutionName = dto.Institution.Name;
            plan.Department = dto.Institution.Department;
            plan.Location = dto.Institution.Locality;
            if (!String.IsNullOrEmpty(dto.Institution.Level))
            {
                plan.InstitutionLevel = dto.Institution.Level;
            }
            if (!String.IsNullOrEmpty(dto.Institution.LevelInt))
            {
                plan.InstitutionLevelInt = Int32.Parse(dto.Institution.LevelInt);
            }
            if (!String.IsNullOrEmpty(dto.Institution.Dependence))
            {
                plan.Dependence = dto.Institution.Dependence;
            }
            Context.SaveChanges();
        }
    }
    dto.InstitutionLevelInt = plan.InstitutionLevelInt;
    dto.ImprovementPlanComments = plan.Comments.ToList();
    dto.Incidences = plan.Incidences.Where(x => !x.SolicitudeId.HasValue).ToList();

    // solo de la misma provincia, mismo eje y mismo nivel
    var provNumbers = new List<String>();
    provNumbers.Add(plan.CUE.Substring(0, 2));
    dto.Evaluators = _userProfileService.ListEvaluatorsUsers(provNumbers, plan.FieldId, plan.InstitutionLevelInt);
    dto.EvaluatorId = (plan.EvaluatorUserId.HasValue) ? plan.EvaluatorUserId.Value : 0;
    dto.Evaluator = plan.UserProfile;
    if (SessionHelper.HasAnyRole(new string[] { RoleConstants.OPERADOR_PROVINCIAL, RoleConstants.REFERENTE_JURISDICCIONAL }))
    {
        dto.Documents = _documentService.List(plan.Id).Where(x => x.StatusId == DictumStatusEnum.Firmado.ToInt()).ToList();
    }
    else
    {
                // dto.Documents = _documentService.List(plan.Id).OfType<Dictum>().ToList<Document>();
                dto.Documents = _documentService.List(plan.Id).ToList<Document>();
    }

    // Combos Solicitados
    dto.SolicitudeDTO.SchoolYears = _solicitudeService.ListSchoolYears();
    dto.SolicitudeDTO.Lines = _solicitudeService.ListLinesByField(plan.FieldId);

    //dto.SolicitudeDTO.Lines = _solicitudeService.ListLinesForPlan(plan);
            if (plan.LineId != null)
                dto.SolicitudeDTO.LineId = plan.LineId.Value; 
    if (plan.SubFieldId!=null)
      {
                dto.SolicitudeDTO.SubFieldId = plan.SubFieldId.Value;
                dto.SolicitudeDTO.Lines_22 = this.ListLinesBySubField(plan.SubFieldId.Value);
                dto.SolicitudeDTO.Line_22_Id = plan.Line_22_Id.Value;

      } else
            {
                dto.SolicitudeDTO.SubFieldId = 0;

                dto.SolicitudeDTO.Line_22_Id = 0;

            }
    dto.SolicitudeDTO.ExpedureTypes = _solicitudeService.ListExpenditureTypes();
            dto.SolicitudeDTO.ExpedureObjectTypes = _solicitudeService.ListExpenditureObjectTypes(null);

    dto.SolicitudeDTO.SolicitudesTypes = _solicitudeService.ListSolicitudesTypes();
    dto.SolicitudeDTO.MeasurementUnits = _solicitudeService.ListMeasurementUnits();

    // Cargo las especializaciones del plan solo si es un plan nacional.
    if (plan.ImprovementPlanTypeId == ImprovementPlanTypeEnum.Institucional.ToInt() && dto.Institution != null)
        dto.SolicitudeDTO.Specializations = _solicitudeService.ListSpecializations(dto.Institution.Specializations);

    // Combos Incidencias
    dto.IncidenceDTO.Status = _incidenceService.ListStatus();
    dto.IncidenceDTO.IncidencesTypes = Context.IncidenceTypes.OrderBy(x => x.Description).ToList();

    // Combos de Dictamenes
    dto.DocumentDTO.DictumDTO.TemplatesTypes = _templateService.ListTemplateTypesForDictums().Where(x => x.Eligibility == false).ToList();
    dto.DocumentDTO.DictumDTO.Status = _documentService.ListStatusForDictum();
    dto.DocumentDTO.DictumDTO.FileNumbers = ListFileNumbers(plan);

    // Combos de Resoluciones
    dto.DocumentDTO.ResolutionDTO.TemplatesTypes = _templateService.ListTemplateTypesForResolution();
    dto.DocumentDTO.ResolutionDTO.FileNumbers = ListFileNumbers(plan);
    dto.DocumentDTO.ResolutionDTO.Status = _documentService.ListStatusForResolution();

    // Estados posibles que puede tomar el plan según el perfil y el estado del plan
    dto.AvailableStatus = ListStatusForPlan(dto.StatusId);

    dto.RelatedLines = ListRelatedLines(plan.Id).ToList();
            dto.RelatedLines_Lines_22 = ListLines22().ToList();
            dto.AccountRenderings = _accountingService.List(plan.Id).ToList();
            dto.AccountRenderingDTO = new AccountRenderingDTO();
            dto.AccountRenderingDTO.Dictums = _documentService.ListDictums(plan.Id).ToList().Where(x=>x.StatusId==(int) DictumStatusEnum.Firmado ).ToList();
            dto.AccountRenderingDTO.ExpedureObjectTypes = _documentService.ListExpenditureObjectTypes(null);
            dto.AccountRenderingDTO.Years = _documentService.ListYears();
            return dto;
}

/// <summary>
/// Lista todos los números de expedientes de un plan de Mejora
/// </summary>
/// <param name="plan">Plan de mejora que contiene los solicitados con los distintos números de expedientes</param>
/// <returns>Un listado con los números de expediente que contiene el plan</returns>
public List<KeyValuePair<string, string>> ListFileNumbers(ImprovementPlan plan)
{
    return plan.Solicitudes.Where(x => !string.IsNullOrEmpty(x.FileNumber))
                            .Select(x => x.FileNumber)
                            .Distinct()
                            .Select(x => new KeyValuePair<string, string>(x, x))
                            .OrderBy(x => x.Value).ToList();
}

/// <summary>
/// Salva un comentario de un plan de mejora
/// </summary>
/// <param name="improvementPlanId">Id del plan de mejora en el cual se desea salvar el comentario</param>
/// <param name="userName">Id del usuario que generó el comentario</param>
/// <param name="text">Texto del comentario a guardar</param>
/// <returns>Devuelve el comentario guardado</returns>
public Comment SaveComment(int improvementPlanId, int userId, string text)
{
    var comment = new Comment();
    comment.UserId = userId;
    comment.Text = text;
    comment.Date = DateTime.Now;

    var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
    plan.Comments.Add(comment);

    Context.SaveChanges();

    return comment;
}

/// <summary>
/// Elimina un plan
/// </summary>
/// <param name="id">Id del plan que se desea eliminar</param>        
public void Delete(int id)
{
    var plan = Context.ImprovementPlans.Where(x => x.Id == id).First();

    // Verifico si existen planes que tienen el plan actual como padre. 
    var childrenPlans = Context.ImprovementPlans.Where(x => x.ParentId == id).ToList();
    var Related = Context.RelatedLines_22.Where(x => x.ImprovementPlanId_Link == id && x.ImprovementPlanId!=id);
    var Related_Childs = Context.RelatedLines_22.Where(x =>  x.ImprovementPlanId == id);
            // Si existen planes hijos con este plan como padre, los dejo como nulos
            foreach (var childrenPlan in childrenPlans)
        childrenPlan.ParentId = null;

    foreach (var incidene in plan.Incidences)
        Context.Comments.RemoveRange(incidene.Comments);

    foreach (var solicitude in plan.Solicitudes)
    {
        Context.Comments.RemoveRange(solicitude.Comments);
        Context.Incidences.RemoveRange(solicitude.Incidences);
    }

    Context.Comments.RemoveRange(plan.Comments);
    Context.Incidences.RemoveRange(plan.Incidences);
    Context.Solicitudes.RemoveRange(plan.Solicitudes);
    Context.RelatedLines_22.RemoveRange(Related);
    Context.RelatedLines_22.RemoveRange(Related_Childs);

    Context.ImprovementPlans.Remove(plan);
    
    BeginAuditLog();
    Context.SaveChanges();
    EndAuditLog(plan.Id);
}

/// <summary>
/// Anula un plan
/// </summary>
/// <param name="id">Id del plan que se desea anular</param>   
/// <returns>True en caso de que se haya anulado el plan. False en caso contrario.</returns>
public bool Block(int id)
{

    var plan = Context.ImprovementPlans.Where(x => x.Id == id).First();

    plan.StatusId = StageStatusEnum.Anulado.ToInt();

    foreach (var solicitude in plan.Solicitudes)
        _solicitudeService.Block(solicitude.Id);

    BeginAuditLog();
    Context.SaveChanges();
    EndAuditLog(plan.Id);
    return true;

}

/// <summary>
/// Determina si un CUE existe para determinar si es válido
/// </summary>
/// <param name="cue">CUE a validar</param>
/// <param name="improvementPlanTypeId"></param>
/// <returns>True en caso de que exista y sea válido. False en caso contrario.</returns>
public bool IsValidCUE(string cue, int improvementPlanTypeId)
{
    var type = (ImprovementPlanTypeEnum)improvementPlanTypeId;
    var _return = false;
    switch (type)
    {
        case ImprovementPlanTypeEnum.Nacional:
            // Valido ya que son todos ceros impuestos por js
            _return = cue == "000000000";
            break;
        case ImprovementPlanTypeEnum.Jurisdiccional:
            // Valido que los dos primeros numeros sean 2 numeros (ya que debería corresponder a una provincia) seguido de 7 ceros.
            if (cue.Length == 9)
            {
                var sub1 = cue.Substring(0, 2);
                var sub2 = cue.Substring(2, cue.Length - 2);
                var validProvinceCode = Context.Provinces.Where(x => x.Number == sub1).Any();
                _return = ((StringHelper.IsNumber(sub1) && sub1 != "00") && (sub2 == "0000000") && validProvinceCode);
            }
            else
            {
                _return = false;
            }

            break;

        case ImprovementPlanTypeEnum.Institucional:
            // Valido que el servicio lo retome como válido
            _return = cue.Length == 9 && GetInstitutionData(cue) != null;
            break;

        default:
            _return = false;
            break;
    }

    return _return;
}

/// <summary>
/// Determina si un CUE existe para determinar si es válido
/// </summary>
/// <param name="cue">CUE a validar</param>
/// <param name="improvementPlanTypeId"></param>
/// <param name="lineId"></param>
/// <returns>True en caso de que exista y sea válido. False en caso contrario.</returns>
public bool IsValidCueField(string cue, int improvementPlanTypeId, int improvementPlanLineId)
{
    var type = (ImprovementPlanTypeEnum)improvementPlanTypeId;
    var _return = false;
    switch (type)
    {
        case ImprovementPlanTypeEnum.Nacional:
            _return = true;
            break;
        case ImprovementPlanTypeEnum.Jurisdiccional:
            _return = true;
            break;

        case ImprovementPlanTypeEnum.Institucional:
            // validamos que si es un CUE de dependencia nacional sea de los ejes permitidos
            var _plan_data = GetInstitutionData(cue);
            _return = !_plan_data.Dependence.Contains("Nacional") || (_plan_data.Dependence.Contains("Nacional") && GetValidFieldsForNationalDependentPlans().Contains(improvementPlanLineId));

            break;

        default:
            _return = false;
            break;
    }

    return _return;
}

/// <summary>
/// Lista todos los planes
/// </summary>
/// <returns></returns>
public List<ImprovementPlan> List()
{
    return Context.ImprovementPlans.OrderBy(x => x.Id).ThenBy(x => x.CUE).ToList();
}

/// <summary>
/// Determina si es posible cambiar el tipo de plan
/// </summary>
/// <param name="improvementPlanId">Id del plan</param>
/// <param name="improvementPlanTypeId">Id del tipo de plan</param>
/// <returns>True en caso de que se pueda cambiar el tipo de plan. False en caso contrario.</returns>
public bool CanChangeImprovementPlanType(int improvementPlanId, int improvementPlanTypeId)
{
    if (improvementPlanId > 0)
    {
        var plan = GetById(improvementPlanId);
        if (plan.ImprovementPlanTypeId != improvementPlanTypeId && plan.Solicitudes.Count > 0)
            return false;
    }

    return true;
}

/// <summary>
/// Determina si cambio el CUE
/// </summary>
public bool IsDifferentCUE(int improvementPlanId, string CUE)
{
    if (improvementPlanId > 0)
    {
        var plan = GetById(improvementPlanId);
        if (plan.CUE != CUE.Trim() && plan.Solicitudes.Count > 0)
            return true;
    }

    return false;
}

/// <summary>
/// Setea un evaluador para un determinado plan de mejora
/// </summary>
/// <param name="improvementPlanId">Id del plan de mejora</param>
/// <param name="evaluatorId">Id del evaluador</param>
/// <returns>True en caso de que se haya seteado el evaluador. False en caso contrario</returns>
public bool SetEvaluator(ImprovementPlan plan, int? evaluatorId)
{
    if (evaluatorId.HasValue)
    {
        var user = Context.UserProfiles.Where(x => x.UserId == evaluatorId).First();
        if (user != null && user.UserId > 0)
        {
            // debe ser un usuario evaluador con permiso para esa provincia, con permiso para ese eje y para ese nivel
            if (Roles.IsUserInRole(user.UserName, RoleConstants.EVALUADOR)
                && (user.Fields.Count == 0 || user.Provinces.Where(x => x.Number == plan.CUE.Substring(0, 2)).Any())
                && (user.Provinces.Count == 0 || user.Fields.Where(x => x.Id == plan.FieldId).Any())
                && (plan.InstitutionLevelInt == null || (user.InstitutionLevels.Count == 0 || user.InstitutionLevels.Where(x => x.Id == plan.FieldId).Any()))
                )
            {
                BeginAuditLog();
                plan.EvaluatorUserId = evaluatorId;
                Context.SaveChanges();
                EndAuditLog(plan.Id);
                return true;
            }
        }
    }
    else
    {
        BeginAuditLog();
        plan.EvaluatorUserId = null;
        Context.SaveChanges();
        EndAuditLog(plan.Id);
        return true;
    }
    return false;
}

/// <summary>
/// Exporta la lista de planes a un excel
/// </summary>
/// <returns>El stream en memoria del archivo de excel generado</returns>
public Stream ExportImprovementPlansToExcel()
{
    var dto = new DataTableDTO();
    dto.iDisplayStart = 0;
    dto.iDisplayLength = 100000000;
    var result = ListImprovementPlans(dto);
    var plans = result.FilteredImprovementPlans.Select(x => new ListImprovementPlansDTO
    {
        Identifier = x.Plan.Identifier,
        ImprovementPlanType = x.ImprovementPlansTypeDescription,
        CUE = x.Plan.CUE,
        ReceptionDate = x.Plan.ReceptionDate,
        SchoolYear = x.SchoolYearDescription,
        Field = x.FieldDescription,
        SolicitudesCount = x.Plan.Solicitudes.Count,
        Evaluator = x.UserName,
        CommentsCount = x.Plan.Comments.Count,
        IncidencesCount = x.Plan.Incidences.Count,
        Status = x.StatusDescription
    });

    var type = ExportFormatType.Excel;
    var reportUrl = Path.Combine(ConfigurationManager.AppSettings["ReportsPath"], "ListImprovementPlans.rpt");

    var rpt = new ReportClass { FileName = reportUrl };
    rpt.Load();
    rpt.SetDataSource(plans);

    return rpt.ExportToStream(type);
}

/// <summary>
/// Exporta los detalles de un plan
/// </summary>
/// <param name="improvementPlanId">Id del plan de mejora del cual se quiere generar el PDF</param>
/// <returns>El stream en memoria del archivo de pdf generado</returns>
public Stream ExportImprovementPlansToPDF(int improvementPlanId)
{
    var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).Select(x => new ImprovementPlanDetailsDTO
    {
        Identifier = x.Identifier,
        ImprovementPlanType = x.ImprovementPlansType.Description,
        CUE = x.CUE,
        ReceptionDate = x.ReceptionDate,
        SchoolYear = x.SchoolYear.Description,
        Field = x.Field.Description,
        User = (x.UserProfile != null) ? x.UserProfile.Name + " " + x.UserProfile.LastName : "N/D",
        Status = x.Status.Description,
        Summary = x.Summary,
        Solicitudes = x.Solicitudes.ToList()
    });

    var solicitudesSummary = new List<SummaryDTO>();
    var solicitudesByLine = plan.First().Solicitudes.GroupBy(x => x.Line).ToList();
    foreach (var sbl in solicitudesByLine)
    {
        // Verifico si para la línea actual hay al menos un solicitado habilitado
        var validLine = sbl.Where(x => x.StatusId != SolicitudeStatusEnum.Anulado.ToInt()).Any();
        if (validLine)
        {
            // Calculo el total por línea excluyendo los solicitados anulados
            decimal totalByLine = 0;
            foreach (var s in sbl)
            {
                if (s.StatusId != SolicitudeStatusEnum.Anulado.ToInt())
                    totalByLine += s.RequestedTotal;
            }
            var solicitude = sbl.Where(x => x.StatusId != SolicitudeStatusEnum.Anulado.ToInt()).First();
            solicitudesSummary.Add(new SummaryDTO()
            {
                Field = solicitude.ImprovementPlan.Field.Description,
                Line = solicitude.Line.Description,
                FileNumber = solicitude.FileNumber,
                TotalByLine = totalByLine
            });
        }
    }

    var type = ExportFormatType.PortableDocFormat;
    var reportUrl = Path.Combine(ConfigurationManager.AppSettings["ReportsPath"], "ImprovementPlan.rpt");

    var rpt = new ReportClass { FileName = reportUrl };
    rpt.Load();
    rpt.SetDataSource(plan);
    rpt.Subreports[0].SetDataSource(solicitudesSummary);

    return rpt.ExportToStream(type);
}

/// <summary>
/// Listado de los campos habilitados para planes de dependencia nacional
/// </summary>
/// <returns></returns>
public List<int> GetValidFieldsForNationalDependentPlans()
{
    var data = System.Configuration.ConfigurationManager.AppSettings["national_plan_field_line"];
    var _config = data.Split(',').Select(x => x.Trim());
    var _field_codes = _config.Select(x => x.Split('/').First()).ToList();
    return Context.Fields.Where(x => _field_codes.Contains(x.Code)).Select(x => x.Id).ToList();
}

        public bool SaveRelatedLine(int line22Id, int planId)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == planId).FirstOrDefault();

            if (plan.Id > 0)
            {
                try
                {
                    var rp = new RelatedLines_22();
                    rp.ImprovementPlanId = planId;
                    rp.Line22Id = line22Id;


                    plan.RelatedLines_22.Add(rp);

                    Context.SaveChanges();

                }
                catch (Exception e)
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public IList<RelatedLines_22> ListRelatedLines(int planId)
        {
            var rl = new List<RelatedLines_22>();

            // primero, todas las lineas si es "padre"
            var p = Context.ImprovementPlans.Where(x => x.Id == planId).FirstOrDefault();
            if (p.ParentId == null) // es padre!
            { rl = Context.RelatedLines_22.Where(x => (x.ImprovementPlanId == planId)).ToList();
             // rl.AddRange(Context.RelatedLines_22.Where(x => (x.i == planId)).ToList();)
            }
            else
            {
                rl.AddRange( Context.RelatedLines_22.Where(x => (x.ImprovementPlanId == p.ParentId)).ToList());
            }


            foreach (RelatedLines_22 r in rl)
            {
                if (r.Lines_22 == null)
                {
                    r.Lines_22 = Context.Lines_22.Where(x => x.Id == r.Line22Id).FirstOrDefault();

                }
            }


            //luego todas las lineas del lote !
            //var p = Context.ImprovementPlans.Where(x => x.ParentId == planId).ToList();
            //foreach (ImprovementPlan pl in p)
            //{
            //    var rl_2 = Context.RelatedLines_22.Where(x => x.ImprovementPlanId == pl.Id);
            //    foreach (RelatedLines_22 r in rl_2)
            //    {
            //        if (r.Lines_22 == null)
            //        {
            //            r.Lines_22 = Context.Lines_22.Where(x => x.Id == r.Line22Id).FirstOrDefault();

            //        }
            //    }

            //    rl.ToList().AddRange(rl_2.ToList());
            //}

            return rl.ToList();
        }

        public void GenerateRelatedPlans(ImprovementPlan plan)
        {
            // generamos copias de los planes


            foreach (RelatedLines_22 r in plan.RelatedLines_22)
            {
                var np = new ImprovementPlan();
                np.ImprovementPlanTypeId = plan.ImprovementPlanTypeId;
                var Line22 = this.getLine22DTO(r.Line22Id.Value);
                var ol = Context.Lines_22.Where(x => x.Id == r.Line22Id.Value).FirstOrDefault();
                string summary = Line22.SubFields.FirstOrDefault().Code + ol.Code;
                np.Line_22_Id = r.Line22Id;
                np.SubFieldId = Line22.SubFieldId;
                np.FieldId = Line22.FieldId;
                np.LineId = Line22.LineId;
                np.ParentId = plan.Id;
                np.SchoolYearId = plan.SchoolYearId;
                np.CUE = r.CUE;
                np.Summary = summary;
                np.FieldDate = plan.FieldDate;
                np.Attachment = plan.Attachment;
                np.AttachmentAdd = plan.AttachmentAdd;
                np.Documentation = np.Documentation;
                np.Identifier = String.Format("{0}-{1}-{2}-{3}", DateTime.Now.Year, np.FieldId, np.CUE, summary);
                Context.ImprovementPlans.Add(plan);

            }

            Context.SaveChanges();

        }
    }

}


