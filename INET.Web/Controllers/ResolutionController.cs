using INET.Core.Constants;
using INET.Core.Enums;
using INET.Data;
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

    public class ResolutionController : BaseController
    {
        private readonly DocumentService _documentService;
        private readonly TemplateService _templateService;
        private readonly UserProfileService _userProfileService;

        public ResolutionController(DocumentService documentService, TemplateService templateService, UserProfileService userProfileService)
        {
            _documentService = documentService;
            _templateService = templateService;
            _userProfileService = userProfileService;
        }

        public ActionResult List()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ));

            var model = new ResolutionListDTO();
            model.Filters = LoadSearchFilters();
            model.Resolutions = new List<Data.Resolution>();
            model.ResolutionDTO = LoadResolutionDTO();
            CheckMessageResult();
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveResolution(ResolutionDTO dto)
        {
            if (dto.Id == 0)
            {
                this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Resolutions.CREATE));
            }
            else
            {
                var reso = _documentService.GetResolution(dto.Id);
                this.HasAccess(ResolutionPermissions.CanEdit(reso));
                if (dto.StatusId == (int)ResolutionStatusEnum.Emitido && dto.StatusId != reso.StatusId)
                {
                    this.HasAccess(ResolutionPermissions.CanEmit(reso));
                }
                else if (dto.StatusId == (int)ResolutionStatusEnum.Firmado && dto.StatusId != reso.StatusId)
                {
                    this.HasAccess(ResolutionPermissions.CanSign(reso));
                }
            }
            var result = _documentService.Save(dto, HttpContext.Request.ApplicationPath);
            if (result)
                return Json("{return: true}");

            return Json("");
        }

        public ActionResult GetResolutionEditData(int resolutionId)
        {
            var resolution = _documentService.GetResolution(resolutionId, SessionHelper.CurrentUserProfile);
            return Json(
                    new
                    {
                        ResolutionId = resolution.Id,
                        TemplateTypeId = resolution.Template.TemplateTypeId,
                        Templates = _templateService.ListByTemplateType(resolution.Template.TemplateTypeId).Select(x => new { Id = x.Id, Name = x.Name }).ToList(),
                        TemplateId = resolution.TemplateId,
                        //FileNumbers = _improvementPlanService.ListFileNumbers(resolution.ImprovementPlan),
                        //FileNumber = resolution.Dictums.First().FileNumber,
                        ResolutionNumber = resolution.ResolutionNumber,
                        StatusId = resolution.StatusId,
                        Body = resolution.Body,
                        ShipDate = (resolution.ShipDate.HasValue) ? resolution.ShipDate.Value.ToString("dd/MM/yyyy") : "",
                        SignatureDate = (resolution.SignatureDate.HasValue) ? resolution.SignatureDate.Value.ToString("dd/MM/yyyy") : "",
                        AnnexSignatureDate = (resolution.AnnexSignatureDate.HasValue) ? resolution.AnnexSignatureDate.Value.ToString("dd/MM/yyyy") : "",
                        SignedDocument = resolution.SignedDocument,
                        Annex = resolution.Annex,
                        TemplateVariables = resolution.Template.TemplateVariables.Select(x => new { Id = x.Id, Variable = x.Variable }),
                        DocumentVariables = resolution.DocumentVariables.Select(x => new { TemplateVariableId = x.TemplateVariableId, Text = x.Text }),
                        Dictums = resolution.Dictums.Select
                        (
                        x => new
                        {
                            Id = x.Id,
                            Identifier = x.DictumNumber,
                            Status = x.Status.Description,
                            SignatureDate = (x.SignatureDate.HasValue) ? x.SignatureDate.Value.ToString("dd/MM/yyyy") : "",
                            Ammount = x.Ammount
                        }).ToList()
                    }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListDictumsForResolutionSelection(int templateId)
        {
            return PartialView("_DictumsSelector", _documentService.ListDictumsForResolutionByTemplate(templateId));
        }

        [HttpPost]
        public ActionResult DeleteResolution(int documentId)
        {
            var reso = _documentService.GetResolution(documentId);
            this.HasAccess(ResolutionPermissions.CanDelete(reso));

            if (_documentService.DeleteResolution(reso))
                return Json("{return: true}");

            return Json("");
        }

        [HttpPost]
        public ActionResult BlockResolution(int resolutionId)
        {
            var reso = _documentService.GetResolution(resolutionId);
            this.HasAccess(ResolutionPermissions.CanBlock(reso));

            if (_documentService.BlockResolution(reso))
                return Json("{return: true}");

            return Json("");

        }

        public ActionResult SignResolution(int resolutionId, int resolutionPlanId, string SignatureDate, HttpPostedFileBase fileSigned, string ResolutionNumber, string AnnexSignatureDate, HttpPostedFileBase file)
        {

            if (_documentService.SignResolution(resolutionId, SignatureDate, AnnexSignatureDate, ResolutionNumber, fileSigned, file))
            {
                return Json("{return: true}");
            }

            return Json("");
            //return RedirectToAction("List");
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
                            (dto.iCustomSearch_StatusId.HasValue
                                && dto.iCustomSearch_StatusId.Value != ResolutionStatusEnum.Firmado.ToInt()
                                && dto.iCustomSearch_StatusId.Value != ResolutionStatusEnum.Protocolizado.ToInt()
                            )
                        )
                    {
                        dto.iCustomSearch_StatusId = null;
                        dto.statusIds.Add(ResolutionStatusEnum.Firmado.ToInt());
                        dto.statusIds.Add(ResolutionStatusEnum.Protocolizado.ToInt());
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

                ResolutionResultDTO result = _documentService.SearchResolutions(dto);

                // Devuelvo el resultado
                return Json(new
                {
                    sEcho = result.Echo,
                    iTotalRecords = result.TotalRecords,
                    iTotalDisplayRecords = result.TotalDisplayRecords,
                    aaData = result.FilteredResolutions.Select(x => new Dictionary<string, object>()
                    {   { "DT_RowId", x.Resolution.Id },
                        { "0", RenderPartialToString(PartialView("_ResolutionActions", x.Resolution), ControllerContext) },
                        { "1", x.Resolution.Id },
                        { "2", x.Resolution.ResolutionNumber },
                        { "3", String.Join(" - ", x.Resolution.Dictums.Select(y => y.DictumNumber).ToArray()) },
                        { "4", x.Resolution.CreationDate.ToString("dd/MM/yyyy") },
                        { "5", x.UserName },
                        { "6", String.Join(", ", x.ImprovementPlanIdentifiers) },
                        { "7", x.StatusDescription },
                        { "8", x.Resolution.AmountExecuted.HasValue ? x.Resolution.AmountExecuted.Value.ToString("C") : "0" }
                    }).ToArray()
                }, JsonRequestBehavior.AllowGet);
            }
            return Json("");
        }

        public ActionResult LoadStatusCombo(int resolutionId)
        {
            var statuses = ResolutionPermissions.ListAvailableStatuses(resolutionId);

            if (resolutionId > 0)
            {
                var reso = _documentService.GetResolution(resolutionId);
                if (!statuses.Where(x => x.Id == reso.StatusId).Any())
                {
                    var _statuses = new List<Status>();
                    _statuses.Add(reso.Status);
                    statuses = _statuses;
                }
            }

            return Json(new
            {
                Statuses = statuses.Select(x => new { Id = x.Id, Description = x.Description }).ToList(),
            }, JsonRequestBehavior.AllowGet);
        }

        #region Logic Methods

        /// <summary>
        /// Carga los filtros de búsqueda
        /// </summary>
        /// <returns>Un listado de FiltersDTO con los filtros a buscar</returns>
        private FiltersDTO LoadSearchFilters()
        {
            var filters = new FiltersDTO();

            filters.Fields = SessionHelper.GetFields();

            filters.Status = _documentService.ListStatusForResolution();
            if (SessionHelper.HasAnyRole(new string[] { RoleConstants.OPERADOR_PROVINCIAL, RoleConstants.REFERENTE_JURISDICCIONAL }))
            {
                filters.Status = filters.Status.Where(x => x.Id == (int)ResolutionStatusEnum.Firmado || x.Id == (int)ResolutionStatusEnum.Protocolizado).ToList();
            }

            return filters;
        }

        /// <summary>
        /// Carga un resolutionDTO con los valores correspondientes para los combos 
        /// del cuadro de dialogo
        /// </summary>
        /// <returns>Un objeto ResolutionDTO con los datos cargados</returns>
        private ResolutionDTO LoadResolutionDTO()
        {
            var dto = new ResolutionDTO();
            dto.TemplatesTypes = _templateService.ListTemplateTypesForResolution();
            dto.Status = _documentService.ListStatusForResolution();

            return dto;
        }

        #endregion
    }
}
