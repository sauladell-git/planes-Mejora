using INET.Core.Constants;
using INET.Core.Enums;
using INET.Services;
using INET.Services.DTO;
using INET.Utils.Helpers;
using INET.Utils.Helpers.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INET.Web.Controllers
{

    public class DictumController : BaseController
    {
        private readonly DocumentService _documentService;
        private readonly TemplateService _templateService;
        private readonly UserProfileService _userProfileService;
        private readonly ImprovementPlanService _improvementPlanService;

        public DictumController(DocumentService documentService, TemplateService templateService, UserProfileService userProfileService, ImprovementPlanService improvementPlanService)
        {
            _documentService = documentService;
            _templateService = templateService;
            _userProfileService = userProfileService;
            _improvementPlanService = improvementPlanService;
        }

        public ActionResult List()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ));

            var model = new DictumListDTO();
            model.Filters = LoadSearchFilters();
            model.Dictums = new List<Data.Dictum>();
            model.DictumDTO = LoadDictumDTO();

            return View(model);
        }

        public ActionResult Search(DataTableDTO dto)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ));
            if (ModelState.IsValid)
            {
                dto.iDisplayLength = 20;

                if (SessionHelper.HasAnyRole(new string[] { RoleConstants.OPERADOR_PROVINCIAL, RoleConstants.REFERENTE_JURISDICCIONAL }))
                {
                    if (!dto.iCustomSearch_StatusId.HasValue ||
                        (dto.iCustomSearch_StatusId.HasValue && dto.iCustomSearch_StatusId.Value != DictumStatusEnum.Firmado.ToInt()))
                    {
                        dto.iCustomSearch_StatusId = null;
                        dto.statusIds.Add(DictumStatusEnum.Firmado.ToInt());
                    }
                }

                // filtramos por Provincia del usuario
                var userProfile = SessionHelper.CurrentUserProfile;
                if (userProfile.Provinces.Count > 0)
                {
                    foreach (INET.Data.Province prov in userProfile.Provinces)
                    {
                        dto.provNumbers.Add(prov.Number);
                    }
                }

                // filtramos por eje del usuario
                if (userProfile.Fields.Count > 0)
                {
                    foreach (INET.Data.Field field in userProfile.Fields)
                    {
                        dto.fieldsIds.Add(field.Id);
                    }
                }

                // filtramos por nivel del usuario
                if (userProfile.InstitutionLevels.Count > 0)
                {
                    foreach (INET.Data.InstitutionLevel level in userProfile.InstitutionLevels)
                    {
                        dto.levelsIds.Add(level.Code);
                    }
                }

                DictumResultDTO result = _documentService.SearchDictums(dto);

                // Devuelvo el resultado
                return Json(new
                {
                    sEcho = result.Echo,
                    iTotalRecords = result.TotalRecords,
                    iTotalDisplayRecords = result.TotalDisplayRecords,
                    aaData = result.FilteredDictums.Select(x => new Dictionary<string, object>()
                    {   { "DT_RowId", x.Dictum.Id.ToString() },
                        { "0", x.Dictum != null ? RenderPartialToString(PartialView("_DictumActions", x.Dictum), ControllerContext) : "" },
                        { "1", x.Dictum.DictumNumber },
                        { "2", x.ImprovementPlanIdentifier },
                        { "3", x.Dictum.CreationDate.ToString("dd/MM/yyyy") },
                        { "4", x.UserName },
                        { "5", x.Resolutions.ToList().Aggregate("", (c, t) => c + (String.IsNullOrEmpty(c) ? "" : ", ") + t.ResolutionNumber) },
                        { "6", x.StatusDescription },
                        { "7", x.Dictum.Ammount.HasValue ? x.Dictum.Ammount.Value.ToString("C") : "0" }
                    }).ToArray()
                }, JsonRequestBehavior.AllowGet);
            }

            return Json("");
        }

        public ActionResult DeleteDictum(int improvementPlanId, int documentId)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Dictums.DELETE));

            if (_documentService.DeleteDictum(documentId))
                return Json("{result: true}");

            return Json("");
        }

        public ActionResult BlockDictum(int improvementPlanId, int dictumId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);

            this.HasAccess(ImprovementPlanPermissions.HasAccess(plan) && DictumPermissions.CanBlock(dictumId));

            if (_documentService.BlockDictum(dictumId))
                return Json("{result: true}");

            return Json("");
        }
        [HttpPost]
        public ActionResult SaveDictumAnnex(DictumDTO dto, int planId, int dictumId, HttpPostedFileBase annexDocument)
        {
            if (annexDocument==null)
                return Json("{result: false}");
            var plan = _improvementPlanService.GetById(planId);

            if (dictumId == 0)
            {
                this.HasAccess(ImprovementPlanPermissions.HasAccess(plan) && DictumPermissions.CanCreate(plan));
            }
            else
            {
                this.HasAccess(ImprovementPlanPermissions.HasAccess(plan) && DictumPermissions.CanEdit(dictumId));
            }

            var result = _documentService.SaveAnnexDictumFile(dictumId, annexDocument);
            if (result)
            {
                return Json("{result: true}");
            }

            return Json("");
        }
        [HttpPost]
        public ActionResult SaveDictum(DictumDTO dto, HttpPostedFileBase annexDocument)
        {
            var plan = _improvementPlanService.GetById(dto.ImprovementPlanId);
            if (dto.Id == 0)
            {
                this.HasAccess(ImprovementPlanPermissions.HasAccess(plan) && DictumPermissions.CanCreate(plan));
            }
            else
            {
                this.HasAccess(ImprovementPlanPermissions.HasAccess(plan) && DictumPermissions.CanEdit(dto.Id));
            }

            var result = _documentService.Save(dto, HttpContext.Request.ApplicationPath);
            if (result)
                return Json("{result: true}");

            return Json("");
        }

        #region Logic Methods

        /// <summary>
        /// Carga los filtros de búsqueda
        /// </summary>
        /// <returns>Un listado de FiltersDTO con los filtros a buscar</returns>
        private FiltersDTO LoadSearchFilters()
        {
            var filters = new FiltersDTO();

            filters.Status = _documentService.ListStatusForDictum().Where(x => x.Id != (int)EligibilityStatusEnum.Aprobado && x.Id != (int)EligibilityStatusEnum.Pendiente && x.Id != (int)EligibilityStatusEnum.Rechazado).ToList();

            if (SessionHelper.HasAnyRole(new string[] { RoleConstants.OPERADOR_PROVINCIAL, RoleConstants.REFERENTE_JURISDICCIONAL }))
            {
                filters.Status = filters.Status.Where(x => x.Id == (int)DictumStatusEnum.Firmado).ToList();
            }
            filters.Fields = SessionHelper.GetFields();

            return filters;
        }

        /// <summary>
        /// Carga un dictumDTO con los valores correspondientes para los combos 
        /// del cuadro de dialogo
        /// </summary>
        /// <returns>Un objeto DictumDTO con los datos cargados</returns>
        private DictumDTO LoadDictumDTO()
        {
            // Combos de Dictamenes
            var dto = new DictumDTO();
            dto.TemplatesTypes = _templateService.ListTemplateTypesForDictums();
            dto.Status = _documentService.ListStatusForDictum().Where(x => x.Id != (int)EligibilityStatusEnum.Aprobado && x.Id != (int)EligibilityStatusEnum.Pendiente && x.Id != (int)EligibilityStatusEnum.Rechazado).ToList();
            //dto.FileNumbers = ListFileNumbers(plan);

            return dto;
        }

        #endregion
    }
}