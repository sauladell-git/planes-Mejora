using INET.Core.Constants;
using INET.Core.Enums;
using INET.Data;
using INET.Services.DTO;
using INET.Utils.Helpers;
using INET.Utils.Helpers.Permissions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace INET.Services
{
    /// <summary>
    /// Servicio encargado del manejo de documentos
    /// </summary>
    public class DocumentService : BusinessService
    {
        private readonly TemplateService _templateService;
        private readonly SolicitudeService _solicitudeService;
        private readonly UserProfileService _userProfileService;

        public DocumentService(INETContext context, TemplateService templateService, SolicitudeService solicitudeService, UserProfileService userProfileService)
        {
            Context = context;
            _templateService = templateService;
            _solicitudeService = solicitudeService;
            _userProfileService = userProfileService;
        }

        /// <summary>
        /// Get a document by id
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public Document Get(int documentId)
        {
            return Context.Documents.Where(x => x.Id == documentId).FirstOrDefault();
        }

        /// <summary>
        /// Lista todos los documentos de un plan de mejora
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora del cual se desea listar los documentos</param>
        /// <returns>Una lista con los documentos de un plan de mejora</returns>
        public IList<Document> List(int improvementPlanId)
        {
            var resos = Context.Documents.Include("Status")
                                    .Include("Template")
                                    .OfType<Resolution>()
                                    .Where(x => x.Dictums.Any(y => y.ImprovementPlanId.Value == improvementPlanId));
            var dictums = Context.Documents.Include("Status")
                                    .Include("Template")
                                    .Where(x => x.ImprovementPlanId.Value == improvementPlanId);

          
             return dictums.Union(resos).ToList();
        }

        #region Dictums

        /// <summary>
        /// Salva un dictamen
        /// </summary>
        /// <param name="dto">DTO del dictamen a salvar</param>
        /// <param name="application_path">Path base de la aplicación, viene del request que lo consume para encontrar los css</param>
        /// <returns>True en caso de que se haya salvado el dictamen. False en caso contrario.</returns>
        public bool Save(DictumDTO dto, string application_path)
        {
            var result = false;
            var status = (DictumStatusEnum)dto.StatusId;

            var original_status = dto.Id > 0 ? GetDictum(dto.Id).StatusId : dto.StatusId;

            switch (status)
            {
                case DictumStatusEnum.Borrador:
                case DictumStatusEnum.PendienteAprobacion:
                    if (original_status == (int)DictumStatusEnum.Emitido)
                    {
                        UndoEmitDictum(dto.Id, dto);
                    }
                    result = SaveDictum(dto);
                    break;

                case DictumStatusEnum.Emitido:
                    if (original_status == (int)DictumStatusEnum.Firmado)
                    {
                        result = UndoSignDictum(dto.Id);
                    }
                    else if (original_status != (int)status)
                    {
                        result = EmitDictum(dto.Id, dto.Body, application_path);
                    }
                    else
                    {
                        result = SaveDictum(dto);
                    }
                    break;
                case DictumStatusEnum.Firmado:
                    if (original_status == DictumStatusEnum.Borrador.ToInt())
                    {
                        dto.StatusId = DictumStatusEnum.Borrador.ToInt();
                        result = SaveDictum(dto);
                    }
                    break;
                default:
                    //result = SetEligibility(dto.Id, dto.StatusId, dto.ExternalNumber, dto.ExternalEntity);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Guarda un dictamen
        /// </summary>
        /// <param name="dto">DTO que contiene el dictamen a guardar</param>
        /// <returns>True en caso de que se haya salvado el dictamen. False en caso contrario.</returns>
        private bool SaveDictum(DictumDTO dto)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == dto.ImprovementPlanId).First();

            var dictum = (dto.Id == 0)
                                ? new Dictum()
                                : GetDictum(dto.Id);

            if (dto.StatusId == DictumStatusEnum.Borrador.ToInt())
            {
                dictum.TemplateId = dto.TemplateId;
                dictum.FileNumber = dto.FileNumber;
                dictum.Ammount = dto.Ammount;
                dictum.Balance = dto.Balance;

                // Agrego las solicitudes seleccionadas
                foreach (var solicitudeId in dto.SolicitudesIds)
                    dictum.Solicitudes.Add(Context.Solicitudes.Where(x => x.Id == solicitudeId).First());

                // Bloqueo las solicitudes para que no se puedan utilizar en otro dictamen
                foreach (var solicitude in dictum.Solicitudes)
                    solicitude.Locked = true;
            }

            var setNumber = false;
            if (dto.Id == 0)
            {
                setNumber = true;
                dictum.Locked = false;
                dictum.CreationDate = DateTime.Now;
                dictum.CreationUserId = SessionHelper.CurrentUserId;
                dictum.StatusId = DictumStatusEnum.Borrador.ToInt();
                plan.Documents.Add(dictum);
            }
            else
            {
                dictum.StatusId = dto.StatusId;
                dictum.Body = dto.Body;
            }

            BeginAuditLog();
            Context.SaveChanges();
            if (setNumber == true)
            {
                dictum.DictumNumber = dictum.Number.ToString() + "/" + _solicitudeService.GetActiveSchoolYear().Cycle;
                Context.SaveChanges();
            }
            EndAuditLog(dictum.ImprovementPlanId.Value, dictum.DictumNumber);
            return true;
        }

        /// <summary>
        /// Elimina un dictamen
        /// </summary>
        /// <param name="documentId">Id del dictamen que se desea eliminar</param>        
        /// <returns>True en caso de que se haya eliminado el dictamen. False en caso contrario.</returns>
        public bool DeleteDictum(int documentId)
        {
            var dictum = Context.Documents.OfType<Dictum>().Where(x => x.Id == documentId).First();
            if (!DictumPermissions.CanDelete(dictum))
                return false;

            try
            {
                var identifier = dictum.DictumNumber;

                foreach (var solicitude in dictum.Solicitudes)
                    solicitude.Locked = false;

                dictum.Solicitudes.Clear();
                Context.Documents.Remove(dictum);
                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(dictum.ImprovementPlanId.HasValue ? dictum.ImprovementPlanId.Value : 0, identifier);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Lista todos los estados posibles que puede tener un dictamen
        /// </summary>
        /// <returns>Una lista con todos los estados que puede tener un dictamen</returns>        
        public IList<Status> ListStatusForDictum()
        {
            return Context.Status.Where(x => x.StatusCriterionId == (int)StatusCriterionEnum.Dictum
                        || x.StatusCriterionId == (int)StatusCriterionEnum.Elegibility).ToList();
        }

        /// <summary>
        /// Obtiene un dictamen
        /// </summary>
        /// <param name="dictumId">Id del dictamen que se desea obtener</param>
        /// <returns>El dictamenn correspondiente al id pasado como parámetro</returns>
        public Dictum GetDictum(int dictumId, UserProfile user_profile = null)
        {
            var query = Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId);
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Pasa el estado de un dictamen a emitido
        /// </summary>
        /// <param name="dictumId">Id del dictamen que se desea emitir</param>        
        /// <param name="content">HTML con el contenido corregido del dictamen</param>
        /// <param name="application_path">Path base de la aplicación, viene del request que lo consume para encontrar los css</param>
        /// <returns>True en caso de que se haya emitido el examen. False en caso de que haya ocurrido un error.</returns>
        private bool EmitDictum(int dictumId, string content, string application_path)
        {
            try
            {
                var dictum = Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId).First();

                // Verifico si el usuario puede emitir el dictamen
                if (!DictumPermissions.CanEmit(dictum.StatusId))
                    return false;

                if (dictum.Solicitudes.Any(x => string.IsNullOrEmpty(x.FileNumber)))
                    return false;

                dictum.UserBody = content;
                var body = dictum.Template.Content.Replace("[CUERPO]", content); // Reemplazo el cuerpo en el template solo al emitir
                dictum.Body = _templateService.Parse(body, dictum, dictum.DocumentVariables.ToList());
                dictum.StatusId = DictumStatusEnum.Emitido.ToInt();
                dictum.EmissionDate = DateTime.Now;

                var fileName = DateTime.Now.Ticks + ".pdf";
                var bytes = _templateService.GetContentPDF(dictum.Body, application_path + "/content/tinyMCE-custom.css", dictum.Id, dictum.Template.Id);
                _templateService.SaveDocument(bytes, dictum.Id, ".pdf");

                // Actualizo el total aprobado
                decimal approvedTotal = 0;
                var activeSchoolYear = _solicitudeService.GetActiveSchoolYear();
                foreach (var solicitude in dictum.Solicitudes)
                {
                    approvedTotal += solicitude.ApprovedTotal.HasValue ? solicitude.ApprovedTotal.Value : 0;
                    solicitude.SchoolYearId = activeSchoolYear.Id;
                }
                dictum.DictumNumber = dictum.Number.ToString() + "/" + activeSchoolYear.Cycle;

                dictum.Ammount = approvedTotal;

                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(dictum.ImprovementPlanId.Value, dictum.DictumNumber);
                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }

        private bool UndoEmitDictum(int dictumId, DictumDTO dto)
        {
            try
            {
                var dictum = Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId).First();

                // puede deshacer la emisión y el dictámen no fue usado en una resolución aprobada
                if (!DictumPermissions.CanUndoEmit(dictum))
                    return false;

                dictum.Body = dictum.UserBody;
                dto.Body = dictum.UserBody;
                dictum.StatusId = DictumStatusEnum.PendienteAprobacion.ToInt();
                dictum.EmissionDate = null;
                try
                {
                    File.Delete(Path.Combine(ConfigurationManager.AppSettings.Get("PDF.SaveDocsPath"), dictum.PDF));
                }
                catch (Exception ex) { }
                dictum.PDF = null;

                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(dictum.ImprovementPlanId.Value, dictum.DictumNumber);
                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Firma un dictamen
        /// </summary>
        /// <param name="dictumId">Id del dictamen que se desea firmar</param>
        /// <param name="changeStatus">Apply status change or only setup date</param>
        /// <returns>True en caso de que se haya firmado el dictamen. False en caso contrario.</returns>
        public bool SignDictum(int dictumId, string signDate, bool changeStatus = true)
        {
            try
            {
                var dictum = Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId).First();

                //if (!DictumPermissions.CanSign(dictum.StatusId))
                //    return false;

                if (changeStatus == true)
                {
                    dictum.StatusId = DictumStatusEnum.Firmado.ToInt();
                }

                dictum.SignatureDate = ParseDateFromString(signDate);

                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(dictum.ImprovementPlanId.Value, dictum.DictumNumber);

                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Save signed dictum document
        /// </summary>
        /// <param name="dictumId"></param>
        /// <param name="signedDocument"></param>
        /// <param name="applicationPath"></param>
        /// <returns></returns>
        public bool SaveSignedDictumFile(int dictumId, HttpPostedFileBase signedDocument)
        {
            try
            {
                var dictum = Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId).First();

                // Salvo el archivo en el disco
                var filePath = ConfigurationManager.AppSettings.Get("PDF.SaveDocsPath") + "\\" + ConfigConstants.SIGNED_DICTUMS_PATH + "\\";
                var fileName = dictumId + "-" + signedDocument.FileName;
                if (!filePath.EndsWith("\\"))
                    filePath = filePath + "\\";

                // Creo un directorio único para los anexo
                if (!Directory.Exists(filePath))
                {
                    var di = new DirectoryInfo(filePath);
                    di.Create();
                }

                filePath = String.Concat(filePath, fileName);

                // Guardo el archivo en el disco
                signedDocument.SaveAs(string.Concat(filePath));

                // Guardo datos en el dictames
                dictum.SignedDocument = fileName;

                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(dictum.ImprovementPlanId.Value, dictum.DictumNumber);

                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Save signed dictum document
        /// </summary>
        /// <param name="dictumId"></param>
        /// <param name="signedDocument"></param>
        /// <param name="applicationPath"></param>
        /// <returns></returns>
        public bool SaveAnnexDictumFile(int dictumId, HttpPostedFileBase signedDocument)
        {
            try
            {  
                var dictum = Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId).First();

                // Salvo el archivo en el disco
                var filePath = ConfigurationManager.AppSettings.Get("PDF.SaveDocsPath") + "\\" + ConfigConstants.ANNEX_DICTUMS_PATH + "\\";
                var fileName = dictumId + "-" + signedDocument.FileName;
                if (!filePath.EndsWith("\\"))
                    filePath = filePath + "\\";

                // Creo un directorio único para los anexo
                if (!Directory.Exists(filePath))
                {
                    var di = new DirectoryInfo(filePath);
                    di.Create();
                }

                filePath = String.Concat(filePath, fileName);

                // Guardo el archivo en el disco
                signedDocument.SaveAs(string.Concat(filePath));

                // Guardo datos en el dictames
                dictum.AnnexDocument = fileName;

                //BeginAuditLog();
                Context.SaveChanges();
               // EndAuditLog(dictum.ImprovementPlanId.Value, dictum.DictumNumber);

                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }


        public bool UndoSignDictum(int dictumId)
        {
            try
            {
                var dictum = Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId).First();

                // puede deshacer la emisión y el dictámen no fue usado en una resolución aprobada
                if (!DictumPermissions.CanUndoSign(dictum))
                    return false;

                dictum.StatusId = DictumStatusEnum.Emitido.ToInt();
                dictum.SignatureDate = null;

                try
                {
                    var fileName = dictumId + "-" + dictum.SignedDocument;
                    File.Delete(ConfigurationManager.AppSettings.Get("PDF.SaveDocsPath") + "\\" + ConfigConstants.SIGNED_DICTUMS_PATH + "\\" + fileName);
                }
                catch (Exception ex) {
                    
                }

                dictum.SignedDocument = null;

                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(dictum.ImprovementPlanId.Value, dictum.DictumNumber);
                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Setea la elegibilidad de un dictamen
        /// </summary>
        /// <param name="dictumId">Id del dictamen</param>
        /// <param name="statusId">Estado de elegibilidad que se desea setear</param>
        /// <param name="externalNumber">Numero externo</param>
        /// <param name="externalEntity">Entidad Externa</param>
        /// <returns>True en caso de que se haya seteado la elegibilidad. False en caso comntrario.</returns>
        private bool SetEligibility(int dictumId, int statusId, string externalNumber, string externalEntity)
        {
            try
            {
                var dictum = Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId).First();
                dictum.StatusId = statusId;
                dictum.EligibilityExternalNumber = externalNumber.Trim();
                dictum.ElegibilityExternalEntity = externalEntity.Trim();

                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(dictum.ImprovementPlanId.Value, dictum.DictumNumber);
                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Anula un dictamen
        /// </summary>
        /// <param name="dictumId">Id del dictamen que se desea anular</param>
        /// <returns>True en caso de que se haya anulado el dictamen. False en caso contrario.</returns>
        public bool BlockDictum(int dictumId)
        {
            var dictum = Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId).FirstOrDefault();
            if (dictum != null && DictumPermissions.CanBlock(dictum))
            {
                dictum.StatusId = DictumStatusEnum.Anulado.ToInt();
                BeginAuditLog();
                foreach (var solicitude in dictum.Solicitudes)
                    solicitude.Locked = false;
                Context.SaveChanges();
                EndAuditLog(dictum.ImprovementPlanId.Value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Busca dictámenes de acuerdo a los filtros seleccionados
        /// </summary>
        /// <param name="dto">DTO con los filtros a aplicar</param>
        /// <returns>Una colección dictámenes que cumple con los filtros requeridos</returns>
        public DictumResultDTO SearchDictums(DataTableDTO dto)
        {
            var pre_query = from dictum in Context.Set<Dictum>()
                            select new
                            {
                                dictum,
                                plan = (from plan in Context.ImprovementPlans where plan.Id == dictum.ImprovementPlanId select plan).FirstOrDefault(),
                                userProfile = (from userProfile in Context.UserProfiles where userProfile.UserId == dictum.CreationUserId select userProfile).FirstOrDefault(),
                                status = (from status in Context.Status where status.Id == dictum.StatusId select status).FirstOrDefault(),
                                resolutions = dictum.Resolutions
                            };

            var totalRecords = pre_query.Count();

            if (dto.iCustomSearch_StatusId.HasValue)
            {
                pre_query = pre_query.Where(x => x.dictum.StatusId == dto.iCustomSearch_StatusId.Value);
            }
            else if (dto.statusIds.Count > 0)
            {
                pre_query = pre_query.Where(x => dto.statusIds.Contains(x.dictum.StatusId));
            }

            if (!string.IsNullOrWhiteSpace(dto.sCustomSearch_CUE))
                pre_query = pre_query.Where(x => x.plan.CUE.Contains(dto.sCustomSearch_CUE.Trim()));

            if (!string.IsNullOrEmpty(dto.sCustomSearch_Dictum))
                pre_query = pre_query.Where(x =>
                    x.dictum.DictumNumber.Contains(dto.sCustomSearch_Dictum.Trim())
                    );

            if (!string.IsNullOrEmpty(dto.sCustomSearch_Resolution))
            {
                pre_query = pre_query.Where(x => x.dictum.Resolutions.Where(y => y.ResolutionNumber.Contains(dto.sCustomSearch_Resolution.Trim())).Any());
            }

            if (!string.IsNullOrEmpty(dto.sCustomSearch_ResolutionId))
            {
                pre_query = pre_query.Where(x => x.dictum.Resolutions.Where(y => SqlFunctions.StringConvert((double)y.Id).Contains(dto.sCustomSearch_ResolutionId.Trim())).Any());
            }

            if (dto.iCustomSearch_FieldId.HasValue)
                pre_query = pre_query.Where(x => x.plan.FieldId == dto.iCustomSearch_FieldId.Value);
            else if (dto.fieldsIds.Count > 0)
            {
                pre_query = pre_query.Where(x => dto.fieldsIds.Contains(x.plan.FieldId));
            }

            if (dto.levelsIds.Count > 0)
            {
                pre_query = pre_query.Where(x => x.plan.InstitutionLevelInt.HasValue && dto.levelsIds.Contains(x.plan.InstitutionLevelInt.Value));
            }

            if (!string.IsNullOrWhiteSpace(dto.sCustomSearch_Identifier))
                pre_query = pre_query.Where(x => x.plan.Identifier.Contains(dto.sCustomSearch_Identifier.Trim()));

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

            switch (dto.iSortCol_0)
            {
                case 1:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.dictum.Id) : pre_query.OrderByDescending(x => x.dictum.Id);
                    break;
                case 2:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.plan.Identifier) : pre_query.OrderByDescending(x => x.plan.Identifier);
                    break;
                case 3:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.dictum.CreationDate) : pre_query.OrderByDescending(x => x.dictum.CreationDate);
                    break;
                case 4:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.userProfile.Name + " " + x.userProfile.LastName) : pre_query.OrderByDescending(x => x.userProfile.Name + " " + x.userProfile.LastName);
                    break;
                case 6:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.status.Description) : pre_query.OrderByDescending(x => x.status.Description);
                    break;
                case 7:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.dictum.Ammount) : pre_query.OrderByDescending(x => x.dictum.Ammount);
                    break;
                default:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.dictum.Id) : pre_query.OrderByDescending(x => x.dictum.Id);
                    break;
            }

            // Armo el resultado
            var result = new DictumResultDTO();
            result.Echo = dto.sEcho;
            result.TotalRecords = totalRecords;
            result.TotalDisplayRecords = (dto.FilterResults) ? pre_query.Count() : totalRecords;
            result.FilteredDictums = pre_query.Skip(dto.iDisplayStart).Take(dto.iDisplayLength).Select(x => new DictumListItem
            {
                Dictum = x.dictum,
                ImprovementPlanIdentifier = x.plan.Identifier,
                UserName = x.userProfile.Name + " " + x.userProfile.LastName,
                StatusDescription = x.status.Description,
                Resolutions = x.resolutions
            }).ToList();
            return result;

        }

        #endregion

        #region Resolutions

        /// <summary>
        /// Guarda una Disposiciones
        /// </summary>
        /// <param name="dto">DTO de la Disposicion a guardar</param>
        /// <param name="application_path">Path base de la aplicación, viene del request que lo consume para encontrar los css</param>
        /// <returns>True en caso de que se haya guardado la Disposicion. False en caso contrario.</returns>
        public bool Save(ResolutionDTO dto, string application_path)
        {
            var result = false;
            var status = (ResolutionStatusEnum)dto.StatusId;
            switch (status)
            {
                case ResolutionStatusEnum.Borrador:
                case ResolutionStatusEnum.PendienteAprobacion:
                    result = SaveResolution(dto);
                    break;
                case ResolutionStatusEnum.Emitido:
                    result = EmitResolution(dto.Id, dto.Body, dto.Variables, application_path);
                    break;

                case ResolutionStatusEnum.Protocolizado:
                    //result = ProtocolizeResolution(dto.Id, dto.ResolutionNumber, ParseDateFromString(dto.ShipDate));
                    break;
                default:
                    //result = SetShipDate(dto.Id, ParseDateFromString(dto.ShipDate));
                    break;
            }

            return result;
        }

        /// <summary>
        /// Guarda una Disposiciones
        /// </summary>
        /// <param name="dto">DTO de la Disposición a guardar</param>
        /// <returns>True en caso de que se haya guardado la Disposición. False en caso contrario.</returns>
        private bool SaveResolution(ResolutionDTO dto)
        {
            //var plan = Context.ImprovementPlans.Where(x => x.Id == dto.ImprovementPlanId).First();
            var plans = new List<int>();

            var resolution = (dto.Id == 0)
                                ? new Resolution()
                                : Context.Documents.OfType<Resolution>().Where(x => x.Id == dto.Id).First();
            List<int> dictumFields;
            List<int> dictumLevels;
            if (resolution.Id > 0)
            {
                dictumFields = resolution.Dictums.Select(x => x.ImprovementPlan.FieldId).ToList();
                dictumLevels = resolution.Dictums.Where(x => x.ImprovementPlan.InstitutionLevelInt.HasValue).Select(x => x.ImprovementPlan.InstitutionLevelInt.Value).ToList();
            }
            else
            {
                dictumFields = Context.Documents.OfType<Dictum>().Where(x => dto.DictumsIds.Contains(x.Id)).Select(x => x.ImprovementPlan.FieldId).ToList();
                dictumLevels = Context.Documents.OfType<Dictum>().Where(x => dto.DictumsIds.Contains(x.Id)).Where(x => x.ImprovementPlan.InstitutionLevelInt.HasValue).Select(x => x.ImprovementPlan.InstitutionLevelInt.Value).ToList();
            }

            if (dto.StatusId == ResolutionStatusEnum.Borrador.ToInt())
            {
                resolution.TemplateId = dto.TemplateId;
                //resolution.ImprovementPlanId = dto.ImprovementPlanId;

                // Agrego los dictámenes seleccionados
                foreach (var dictumId in dto.DictumsIds)
                    resolution.Dictums.Add(Context.Documents.OfType<Dictum>().Where(x => x.Id == dictumId).First());

                // Bloqueo todos los dictamenes incluidos en la Disposición para que no se puedan incluir en otros dictámenes
                foreach (var dictum in resolution.Dictums)
                {
                    dictum.Locked = true;
                    plans.Add(dictum.ImprovementPlanId.Value);
                }
            }

            if (dto.Id == 0)
            {
                resolution.CreationDate = DateTime.Now;
                resolution.CreationUserId = SessionHelper.CurrentUserId;
                resolution.StatusId = ResolutionStatusEnum.Borrador.ToInt();
                //plan.Documents.Add(resolution);
            }
            else
            {
                resolution.StatusId = dto.StatusId;
                resolution.Body = dto.Body;
            }

            var documentVariables = resolution.DocumentVariables.ToList();
            foreach (var dv in documentVariables)
                Context.DocumentVariables.Remove(dv);

            foreach (var dv in dto.Variables)
                if (!string.IsNullOrWhiteSpace(dv.VariableText))
                    resolution.DocumentVariables.Add(new DocumentVariable() { TemplateVariableId = dv.TemplateVariableId, Text = dv.VariableText });

            if (resolution.Id == 0) Context.Documents.Add(resolution);

            BeginAuditLog();
            Context.SaveChanges();
            foreach (var planId in plans)
            {
                EndAuditLog(planId, resolution.Number.ToString());
            }
            return true;
        }

        /// <summary>
        /// Obtiene una Disposición
        /// </summary>
        /// <param name="resolutionId">Id de la Disposición que se desea obtener</param>
        /// <returns>La Disposición correspondiente al id pasado como parámetro</returns>
        public Resolution GetResolution(int resolutionId, UserProfile user_profile = null)
        {
            var query = Context.Documents.OfType<Resolution>().Where(x => x.Id == resolutionId);
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Pasa el estado de una Disposición a emitido
        /// </summary>
        /// <param name="resolutionId">Id de la disposición que se desea emitir</param>        
        /// <param name="content">HTML con el contenido corregido de la disposición</param>
        /// <param name="documentVariables">Variables del documento a remplazar</param>
        /// <param name="application_path">Path base de la aplicación, viene del request que lo consume para encontrar los css</param>
        /// <returns>True en caso de que se haya emitido la disposición. False en caso de que haya ocurrido un error.</returns>
        private bool EmitResolution(int resolutionId, string content, List<DocumentVariableDTO> documentVariables, string application_path)
        {
            try
            {
                var resolution = Context.Documents.OfType<Resolution>().Where(x => x.Id == resolutionId).First();

                if (resolution.Dictums.SelectMany(x => x.Solicitudes).Any(x => string.IsNullOrEmpty(x.FileNumber)))
                    return false;

                // Actualizo el total
                resolution.AmountExecuted = resolution.Dictums.Sum(x => x.Ammount);

                //// Actualizo las variables del documento
                var variables = resolution.DocumentVariables.ToList();
                foreach (var dv in variables)
                    Context.DocumentVariables.Remove(dv);

                foreach (var dv in documentVariables)
                    resolution.DocumentVariables.Add(new DocumentVariable() { TemplateVariableId = dv.TemplateVariableId, Text = dv.VariableText });

                var body = resolution.Template.Content.Replace("[CUERPO]", content); // Reemplazo el cuerpo en el template solo al emitir                
                resolution.Body = _templateService.Parse(body, resolution, resolution.DocumentVariables.ToList());
                resolution.StatusId = ResolutionStatusEnum.Emitido.ToInt();
                resolution.EmissionDate = DateTime.Now;

                var bytes = _templateService.GetContentDocx(resolution.Body, resolution.Id, resolution.Template.Id, "");
                _templateService.SaveDocument(bytes, resolution.Id, ".docx");

                /// Guardamos anexo docx
                var anexoDocxData = _templateService.GenerateResolutionAnexDocx(resolution);
                var anexoBytes = _templateService.GetContentDocx(anexoDocxData, resolution.Id, resolution.Template.Id, "");
                _templateService.SaveAnnex(anexoBytes, resolution.Id, "_anexo.docx");

                /// Guardamos anexo xlsx
                var anexoXlsxData = _templateService.GenerateResolutionAnexXlsx(resolution);
                _templateService.SaveAnnexExtra(anexoXlsxData, resolution.Id, "_anexo.xlsx");


                BeginAuditLog();
                Context.SaveChanges();
                var plans = resolution.Dictums.Select(x => x.ImprovementPlanId.Value).ToList();
                foreach (var planId in plans)
                {
                    EndAuditLog(planId, resolution.Number.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Firma una disposición
        /// </summary>
        /// <param name="resolutionId">Id de la resolucion que se desea firmar</param>
        /// <param name="annex">Anexo del ministerio</param>
        /// <returns>True en caso de que se haya firmado la resolucion. False en caso contrario.</returns>
        public bool SignResolution(int resolutionId, string signDate, string annexSignDate, string resolutionNumber, HttpPostedFileBase signedDocument, HttpPostedFileBase annex)
        {
            try
            {
                var resolution = Context.Documents.OfType<Resolution>().Where(x => x.Id == resolutionId).First();

                var isSigned = resolution.StatusId == ResolutionStatusEnum.Firmado.ToInt();

                // Verifico si el usuario puede firmar la disposición
                if (!ResolutionPermissions.CanSign(resolution))
                    return false;

                if (annex?.ContentLength > 0)
                {
                    // Salvo el archivo en el disco
                    var filePath = ConfigurationManager.AppSettings.Get("SaveAnnexPath");
                    if (!filePath.EndsWith("\\"))
                        filePath = filePath + "\\";

                    // Creo un directorio único para el anexo
                    var directoryName = DateTime.Now.Ticks;
                    filePath = filePath + directoryName;
                    var di = new DirectoryInfo(filePath);
                    di.Create();

                    // Guardo el archivo en el disco
                    annex.SaveAs(string.Concat(filePath, "\\", annex.FileName));
                    resolution.Annex = string.Concat("/", directoryName, "/", annex.FileName);
                }

                if (signedDocument?.ContentLength > 0)
                {
                    // Salvo el archivo en el disco
                    var filePathSigned = ConfigurationManager.AppSettings.Get("PDF.SaveDocsPath") + "\\" + ConfigConstants.SIGNED_RESOLUTIONS_PATH + "\\";
                    var fileName = resolution.Id + "-" + signedDocument.FileName;
                    if (!filePathSigned.EndsWith("\\"))
                        filePathSigned = filePathSigned + "\\";

                    // Creo un directorio único para los anexo
                    if (!Directory.Exists(filePathSigned))
                    {
                        var dir = new DirectoryInfo(filePathSigned);
                        dir.Create();
                    }

                    // Guardo el archivo en el disco
                    signedDocument.SaveAs(String.Concat(filePathSigned, fileName));
                    resolution.SignedDocument = fileName;
                }

                // Guardo datos en la disposición
                if (!isSigned)
                {
                    resolution.StatusId = ResolutionStatusEnum.Firmado.ToInt();
                }
                resolution.ResolutionNumber = resolutionNumber;
                resolution.SignatureDate = ParseDateFromString(signDate);
                resolution.AnnexSignatureDate = ParseDateFromString(annexSignDate);
                

                BeginAuditLog();
                Context.SaveChanges();
                if (!isSigned)
                {
                    var plans = resolution.Dictums.Select(x => x.ImprovementPlanId.Value).ToList();
                    foreach (var planId in plans)
                    {
                        EndAuditLog(planId, resolution.Number.ToString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }

        ///// <summary>
        ///// Pasa una disposición al estado protocolizado</summary>
        ///// <param name="resolutionId">Id de la disposición</param>
        ///// <param name="resolutionNumber">Número de disposición brindado por el ministerio</param>
        ///// <param name="shipDate">Fecha de envío</param>
        ///// <returns>True en caso de que se haya pasado al estado protocolizado. False en caso contrario.</returns>
        //private bool ProtocolizeResolution(int resolutionId, string resolutionNumber, DateTime? shipDate)
        //{
        //    try
        //    {
        //        var resolution = Context.Documents.OfType<Resolution>().Where(x => x.Id == resolutionId).First();

        //        // Verifico si el usuario puede protocolizar la disposición
        //        if (!RoleHelper.DocumentPermissions.ResolutionPermissions.CanUpdate(resolution.StatusId, resolution.Dictums.Select(x => x.ImprovementPlan.FieldId).ToList(), resolution.Dictums.Where(x => x.ImprovementPlan.InstitutionLevelInt.HasValue).Select(x => x.ImprovementPlan.InstitutionLevelInt.Value).ToList())
        //            || string.IsNullOrWhiteSpace(resolutionNumber))
        //            return false;

        //        resolution.ProtocolizedDate = DateTime.Now;
        //        resolution.ResolutionNumber = resolutionNumber;
        //        resolution.StatusId = ResolutionStatusEnum.Protocolizado.ToInt();

        //        if (shipDate.HasValue)
        //            resolution.ShipDate = shipDate.Value;

        //        BeginAuditLog();
        //        Context.SaveChanges();
        //        var plans = resolution.Dictums.Select(x => x.ImprovementPlanId.Value).ToList();
        //        foreach (var planId in plans)
        //        {
        //            EndAuditLog(planId, resolution.Number.ToString());
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        FileSystemLog.Error(ex);
        //        return false;
        //    }
        //}

        /// <summary>
        /// Permite guardar la fecha de envío
        /// </summary>
        /// <param name="resolutionId">Id de la disposición.</param>
        /// <param name="shipDate">Fecha de envío.</param>
        /// <returns>True en caso de que se pueda setear la fecha de envío. False en caso contrario.</returns>
        //private bool SetShipDate(int resolutionId, DateTime? shipDate)
        //{
        //    try
        //    {
        //        var resolution = Context.Documents.OfType<Resolution>().Where(x => x.Id == resolutionId).First();

        //        if (!RoleHelper.DocumentPermissions.ResolutionPermissions.CanSetShipDate(
        //            resolution.StatusId, 
        //            resolution.Dictums.Select(x => x.ImprovementPlan.FieldId).ToList(),
        //            resolution.Dictums.Where(x=>x.ImprovementPlan.InstitutionLevelInt.HasValue).Select(x => x.ImprovementPlan.InstitutionLevelInt.Value).ToList()
        //            ))
        //            return false;

        //        if (shipDate.HasValue)
        //        {
        //            resolution.ShipDate = shipDate.Value;
        //            BeginAuditLog();
        //            Context.SaveChanges();
        //            var plans = resolution.Dictums.Select(x => x.ImprovementPlanId.Value).ToList();
        //            foreach (var planId in plans)
        //            {
        //                EndAuditLog(planId, resolution.Number.ToString());
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        FileSystemLog.Error(ex);
        //        return false;
        //    }
        //}

        /// <summary>
        /// Elimina una disposición
        /// </summary>
        /// <param name="resolution">disposicion a eliminar</param>        
        /// <returns>True en caso de que se haya eliminado la disposición. False en caso contrario.</returns>
        public bool DeleteResolution(Resolution resolution)
        {
            try
            {
                var identifier = resolution.Number.ToString();

                foreach (var dictum in resolution.Dictums)
                    dictum.Locked = false;

                resolution.Dictums.Clear();

                var documentVariables = resolution.DocumentVariables.ToList();
                foreach (var dv in documentVariables)
                    Context.DocumentVariables.Remove(dv);

                Context.Documents.Remove(resolution);
                BeginAuditLog();
                Context.SaveChanges();
                var plans = resolution.Dictums.Select(x => x.ImprovementPlanId.Value).ToList();
                foreach (var planId in plans)
                {
                    EndAuditLog(planId, resolution.Number.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                FileSystemLog.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Anula una disposición
        /// </summary>
        /// <param name="resolution">disposición que se desea anular</param>
        /// <returns>True en caso de que se haya anulado la disposición. False en caso contrario.</returns>
        public bool BlockResolution(Resolution resolution)
        {
            resolution.StatusId = ResolutionStatusEnum.Anulado.ToInt();
            foreach (var dictum in resolution.Dictums)
                dictum.Locked = false;

            BeginAuditLog();
            Context.SaveChanges();
            var plans = resolution.Dictums.Select(x => x.ImprovementPlanId.Value).ToList();
            foreach (var planId in plans)
            {
                EndAuditLog(planId, resolution.Number.ToString());
            }
            return true;
        }

        /// <summary>
        /// Lista los dictamenes que se pueden incluir en una disposición (que se encuentren firmados y no hayan sido utilizados)
        /// </summary>
        /// <returns>Una lista con los los dictamenes que pueden ser incluidos en una disposición</returns>
        public IEnumerable<Dictum> ListDictumsForResolution()
        {
            return Context.Documents.OfType<Dictum>()
                    .Where(x => x.Locked == false
                        && (x.StatusId == (int)DictumStatusEnum.Firmado || (int)x.StatusId == (int)EligibilityStatusEnum.Aprobado));
        }

        public IEnumerable<Dictum> ListDictums(int improvementPlanId)
        {
            return Context.Documents.OfType<Dictum>()
                    .Where(x => x.ImprovementPlanId == improvementPlanId).ToList();

        }
        /// <summary>
        /// Lista los dictamenes que se pueden incluir en una disposición (que se encuentren firmados y no hayan sido utilizados)
        /// </summary>
        /// <returns>Una lista con los los dictamenes que pueden ser incluidos en una disposición</returns>
        public IList<Dictum> ListDictumsForResolutionByTemplate(int templateId)
        {
            var templateFields = Context.Templates.Find(templateId).Fields.Select(x => x.Id).ToList();
            return ListDictumsForResolution().Where(x => templateFields.Any(i => i == x.ImprovementPlan.FieldId)).ToList();
        }

        /// <summary>
        /// Lista los dictamenes que se pueden incluir en una disposición (que se encuentren firmados y no hayan sido utilizados)
        /// </summary>
        /// <param name="improvemetPlanId">Id del plan de mejora que contiene los dictamenes</param>
        /// <param name="fileNumber">Numero de Expediente</param>
        /// <returns>Una lista con los los dictamenes del plan de mejora que pueden ser incluidos en una disposición</returns>
        public IList<Dictum> ListDictumsForResolution(int improvemetPlanId, string fileNumber)
        {
            return ListDictumsForResolution()
                    .Where(x => x.ImprovementPlanId.Value == improvemetPlanId && x.FileNumber == fileNumber).ToList();
        }

        /// <summary>
        /// Lista todos los estados posibles que puede tener una disposición
        /// </summary>
        /// <returns>Una lista con todos los estados que puede tener una resolucion</returns>
        public IList<Status> ListStatusForResolution()
        {
            return Context.Status.Where(x => x.StatusCriterionId == (int)StatusCriterionEnum.Resolution).ToList();
        }

        /// <summary>
        /// Busca resoluciones de acuerdo a los filtros seleccionados
        /// </summary>
        /// <param name="dto">DTO con los filtros a aplicar</param>
        /// <returns>Una colección de resoluciones que cumple con los filtros requeridos</returns>
        public ResolutionResultDTO SearchResolutions(DataTableDTO dto)
        {
            var pre_query = from resolution in Context.Set<Resolution>()
                            select new
                            {
                                resolution,
                                plans = (from plan in resolution.Dictums.Select(x => x.ImprovementPlan) select plan),
                                userProfile = (from userProfile in Context.UserProfiles where userProfile.UserId == resolution.CreationUserId select userProfile).FirstOrDefault(),
                                status = (from status in Context.Status where status.Id == resolution.StatusId select status).FirstOrDefault()
                            };

            var totalRecords = pre_query.Count();

            if (dto.iCustomSearch_StatusId.HasValue)
            {
                pre_query = pre_query.Where(x => x.resolution.StatusId == dto.iCustomSearch_StatusId.Value);
            }
            else if (dto.statusIds.Count > 0)
            {
                pre_query = pre_query.Where(x => dto.statusIds.Contains(x.resolution.StatusId));
            }

            if (!string.IsNullOrWhiteSpace(dto.sCustomSearch_CUE))
                pre_query = pre_query.Where(x => x.plans.Any(y => y.CUE.Contains(dto.sCustomSearch_CUE.Trim())));

            if (!string.IsNullOrEmpty(dto.sCustomSearch_Dictum))
                pre_query = pre_query.Where(x => x.resolution.Dictums.Where(y =>
                    y.DictumNumber.Contains(dto.sCustomSearch_Dictum.Trim())
                    ).Any());

            if (!string.IsNullOrEmpty(dto.sCustomSearch_Resolution))
            {
                pre_query = pre_query.Where(x => x.resolution.ResolutionNumber.Contains(dto.sCustomSearch_Resolution));
            }

            if (dto.iCustomSearch_FieldId.HasValue)
                pre_query = pre_query.Where(x => x.plans.Any(y => y.FieldId == dto.iCustomSearch_FieldId.Value));
            else if (dto.fieldsIds.Count > 0)
            {
                pre_query = pre_query.Where(x => x.plans.Where(y => dto.fieldsIds.Contains(y.FieldId)).Any());
            }

            if (dto.levelsIds.Count > 0)
            {
                pre_query = pre_query.Where(x => x.plans.Where(y => y.InstitutionLevelInt.HasValue && dto.levelsIds.Contains(y.InstitutionLevelInt.Value)).Any());
            }

            if (!string.IsNullOrWhiteSpace(dto.sCustomSearch_ResolutionId))
                pre_query = pre_query.Where(x => SqlFunctions.StringConvert((double)x.resolution.Id).Contains(dto.sCustomSearch_ResolutionId.Trim()));

            if (dto.provNumbers.Count > 0)
            {
                pre_query = pre_query.Where(x => x.plans.Where(y => dto.provNumbers.Contains(y.CUE.Substring(0, 2))).Any());
            }
            else if (dto.iCustomSearch_ProvinceId.HasValue)
            {
                var province = Context.Provinces.Where(x => x.Id == dto.iCustomSearch_ProvinceId.Value).FirstOrDefault();
                string province_number = province != null ? province.Number : "9999"; // if is a non province no result must return
                pre_query = pre_query.Where(x => x.plans.Any(y => y.CUE.StartsWith(province_number)));
            }

            if (!string.IsNullOrWhiteSpace(dto.sCustomSearch_Identifier))
                pre_query = pre_query.Where(x => x.plans.Any(y => y.Identifier.Contains(dto.sCustomSearch_Identifier.Trim())));

            switch (dto.iSortCol_0)
            {
                case 1:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.resolution.Id) : pre_query.OrderByDescending(x => x.resolution.Id);
                    break;
                case 2:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.resolution.ResolutionNumber) : pre_query.OrderByDescending(x => x.resolution.ResolutionNumber);
                    break;
                case 3:
                    break;
                case 4:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.resolution.CreationDate) : pre_query.OrderByDescending(x => x.resolution.CreationDate);
                    break;
                case 5:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.userProfile.Name + " " + x.userProfile.LastName) : pre_query.OrderByDescending(x => x.userProfile.Name + " " + x.userProfile.LastName);
                    break;
                case 6:
                    //pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.plan.Identifier) : pre_query.OrderByDescending(x => x.plan.Identifier);
                    break;
                case 7:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.status.Description) : pre_query.OrderByDescending(x => x.status.Description);
                    break;
                case 8:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.resolution.AmountExecuted) : pre_query.OrderByDescending(x => x.resolution.AmountExecuted);
                    break;
                default:
                    pre_query = pre_query.OrderByDescending(x => x.resolution.Id);
                    break;
            }

            // Armo el resultado
            var result = new ResolutionResultDTO();
            result.Echo = dto.sEcho;
            result.TotalRecords = totalRecords;
            result.TotalDisplayRecords = (dto.FilterResults) ? pre_query.Count() : totalRecords;
            result.FilteredResolutions = pre_query.Skip(dto.iDisplayStart).Take(dto.iDisplayLength).Select(x => new ResolutionListItem
            {
                Resolution = x.resolution,
                ImprovementPlanIdentifiers = x.plans.Select(y => y.Identifier).ToList(),
                UserName = x.userProfile.Name + " " + x.userProfile.LastName,
                StatusDescription = x.status.Description
            }).ToList();
            return result;
        }

        #endregion
    }
}