using INET.Core.Constants;
using INET.Core.Enums;
using INET.Core.Models;
using INET.Data;
using INET.Services;
using INET.Services.DTO;
using INET.Utils.Helpers;
using INET.Utils.Helpers.Permissions;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INET.Web.Controllers
{
    public class PlanController : BaseController
    {
        private readonly ImprovementPlanService _improvementPlanService;
        private readonly SolicitudeService _solicitudeService;
        private readonly IncidenceService _incidenceService;
        private readonly UserProfileService _userProfileService;
        private readonly TemplateService _templateService;
        private readonly DocumentService _documentService;
        private readonly AccountingService _accountingService;
      
        public PlanController(ImprovementPlanService improvementPlanService, SolicitudeService solicitudeService, IncidenceService incidenceService, UserProfileService userProfileService, TemplateService templateService, DocumentService documentService, AccountingService accountingService)
        {
            _improvementPlanService = improvementPlanService;
            _solicitudeService = solicitudeService;
            _incidenceService = incidenceService;
            _userProfileService = userProfileService;
            _templateService = templateService;
            _documentService = documentService;
            _accountingService = accountingService;
        }

        #region List

        public ActionResult ListPaginated(DataTableDTO dto)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ));

            dto.iDisplayLength = 20;
        
            // Obtengo la info de la grilla paginada
            var userProfile = SessionHelper.CurrentUserProfile;
            if (userProfile.Provinces.Count > 0)
            {
                foreach (INET.Data.Province prov in userProfile.Provinces)
                {
                    dto.provNumbers.Add(prov.Number);
                }
            }

            if (userProfile.Fields.Count > 0)
            {
                foreach (INET.Data.Field field in userProfile.Fields)
                {
                    dto.fieldsIds.Add(field.Id);
                }
            }

            if (userProfile.InstitutionLevels.Count > 0)
            {
                foreach (INET.Data.InstitutionLevel level in userProfile.InstitutionLevels)
                {
                    dto.levelsIds.Add(level.Code);
                }
            }
            var result = _improvementPlanService.ListImprovementPlans(dto);
            try
            {
                 result = _improvementPlanService.ListImprovementPlans(dto);

            } catch (Exception ex)
            {
                throw new Exception(ex.Message + "-" + ex.InnerException);
            }

            
          

       try { 
            // Devuelvo el resultado
            return Json(new
            {
                sEcho = result.Echo,
                iTotalRecords = result.TotalRecords,
                iTotalDisplayRecords = result.TotalDisplayRecords,
                aaData = result.FilteredImprovementPlans.Select(x => new Dictionary<string, object>()
                {   { "DT_RowId", x.Plan.Id.ToString() },
                    { "0", RenderPartialToString(PartialView("_PlanActions", x.Plan), ControllerContext) },
                    //{ "1",  " " },
                      { "1", x.Plan.Summary.Length==0? ( _improvementPlanService.SubField_Line(x.Plan)) : StringHelper.Cut(x.Plan.Summary,80) },
                    { "2",  x.ImprovementPlansTypeDescription },
                      //{ "3", x.Plan.CUE },
                        { "3", x.Plan.Articulator==null?(x.Plan.Parent!=null?x.Plan.Parent.Articulator:null):x.Plan.Articulator },
                    { "4", x.Plan.CUE },
                    //{ "4", _improvementPlanService.ListFileNumbers(x.Plan).Aggregate("", (c, t) => c + (String.IsNullOrEmpty(c) ? "" : ", ") + t.Value) },
                    { "5", x.FileNumbers },
                    { "6", x.SchoolYearDescription },
                    { "7", x.FieldCode },
                    { "8", x.SolicitudesCount },
                    { "9", (x.UserName != null) ? x.UserName : "-" },
                    { "10", x.CommentsCount },
                    { "11", x.IncidencesCount },
                    { "12", x.StatusDescription },
                    { "13", _improvementPlanService.CalculateOverallSolicitudesStatus(x.SolicitudesCount, x.ApprovedCount, x.RejectedCount) }
                }).ToArray()
            }, JsonRequestBehavior.AllowGet);
            } catch (Exception ex)
            {  
                    throw new Exception(ex.ToString() + "- " + ex.InnerException + "-" + ex.StackTrace);

            }
        }

        public ActionResult SearchSolicitudes(DataTableDTO dto)
        {
            var plan = _improvementPlanService.GetById(dto.iCustomSearch_ImprovementPlanId ?? 0);
            this.HasAccess(ImprovementPlanPermissions.HasAccess(plan));

            if (ModelState.IsValid)
            {
                dto.iDisplayLength = 20;

                var includeCueCol = plan.ImprovementPlanTypeId == ImprovementPlanTypeEnum.Jurisdiccional.ToInt();
                SolicitudeResultDTO result = _solicitudeService.Search(dto, includeCueCol);

                var values = new List<Dictionary<string, object>>();

                var data = result.FilteredSolicitudes.Select(x => buildItems(x, includeCueCol)).ToArray();


                // Devuelvo el resultado
                return Json(new
                {
                    sEcho = result.Echo,
                    iTotalRecords = result.TotalRecords,
                    iTotalDisplayRecords = result.TotalDisplayRecords,
                    aaData = data
                }, JsonRequestBehavior.AllowGet);
            }

            return Json("");
        }

        Dictionary<string, object> buildItems(Solicitude x, bool includeCue)
        {
            var item = new Dictionary<string, object>();
            var i = 0;

            item.Add("DT_RowId", x.Id.ToString());
            item.Add((i++).ToString(), x.Id > 0 ? RenderPartialToString(PartialView("_SolicitudeActions", x), ControllerContext) : "");
            item.Add((i++).ToString(), x.Id > 0 ? "<input type=\"checkbox\" class=\"selectedSolicitudes\" name=\"selectedSolicitudes\" value=\"" + x.Id + "\" />" : "");
            item.Add((i++).ToString(), x.Id);
            //if (includeCue)
            //{
            //    item.Add((i++).ToString(), x.CUE);
            //    item.Add((i++).ToString(), x.Management);
            //}

            // SIA 02-06-22 include siempre CUE COL
            item.Add((i++).ToString(), x.CUE);
            item.Add((i++).ToString(), x.Management);

            item.Add((i++).ToString(), x.ExpenditureType.Description);
            if (x.ExpenditureObjectTypeId !=null)
               item.Add((i++).ToString(), x.ExpenditureObjectType.Description);
            else
                item.Add((i++).ToString(), "");

            item.Add((i++).ToString(), x.LineId.HasValue ? x.Line.Code : null);
            item.Add((i++).ToString(), x.FileNumber);
            item.Add((i++).ToString(), x.Status.Description);
            item.Add((i++).ToString(), x.Details);
            item.Add((i++).ToString(), x.ReassignedId.HasValue ? x.ReassignedId.ToString() : "Original");
            item.Add((i++).ToString(), x.Id > 0 ? RenderPartialToString(PartialView("_SolicitudeComments", x), ControllerContext) : "");
            item.Add((i++).ToString(), x.Id > 0 ? RenderPartialToString(PartialView("_SolicitudeIncidents", x), ControllerContext) : "");
            item.Add((i++).ToString(), x.RequestedTotal.ToString("C"));
            item.Add((i++).ToString(), x.ApprovedTotal.HasValue ? x.ApprovedTotal.Value.ToString("C") : "0");

            return item;
        }



        public ActionResult List()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ));

            CheckMessageResult();
            var model = new ImprovementPlanListDTO();
            model.Filters = LoadSearchFilters();
            model.ImprovementPlans = new List<ImprovementPlan>();
            return View(model);
        }

        public ActionResult ExportImprovementPlansToExcel()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ));

            var stream = _improvementPlanService.ExportImprovementPlansToExcel();
            return File(stream, "application/vnd.ms-excel", "Lista_Planes_de_Mejora_" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".xls");
        }

        #endregion

        #region Improvement Plan Editor

        public ActionResult Editor(int? id)
        {
            // Valido que el usuario pueda crear o editar
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] { PermissionConstants.ImprovementPlan.CREATE, PermissionConstants.ImprovementPlan.EDIT }));

            ImprovementPlanEditorDTO model = null;

            if (TempData["ParentId"] != null)
            {
                var parentId = Convert.ToInt32(TempData["ParentId"]);
                model = _improvementPlanService.GetImprovementPlanForEdition(parentId);
                model.Id = 0;
                model.DocumentationURL = null;
                model.AttachmentURL = null;
                model.ParentId = parentId;
                TempData["ParentId"] = null;
            }
            else if (id.HasValue)
            {
                model = _improvementPlanService.GetImprovementPlanForEdition(id.Value);
            }

            // si está editando tiene que tener acceso al plan
            if (model != null && model.Id > 0 && (!ImprovementPlanPermissions.HasAccess(model.Id) || !SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.EDIT, model.StatusId)))
            {
                return RedirectToAction("List");
            }

            return View(Load(model));
        }

        public ActionResult Copy(int id)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.CREATE));
            TempData["ParentId"] = id;
            return RedirectToAction("Editor");
        }

        [HttpPost]
        public ActionResult Editor(ImprovementPlanEditorDTO dto, HttpPostedFileBase _documentation, HttpPostedFileBase _attachment, HttpPostedFileBase _attachmentAdd)
        {
            // Valido que el usuario pueda crear planes
            if (dto.Id == 0 && !SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.CREATE))
                return RedirectToAction("List");
            else if (dto.Id > 0 && ImprovementPlanPermissions.HasAccess(dto.Id))
            {
                var _improvementPlan = _improvementPlanService.GetImprovementPlanForEdition(dto.Id);
                if (!SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.EDIT, _improvementPlan.StatusId))
                {
                    return RedirectToAction("List");
                }
            }

            var doSave = true;

            // solo puede crear de sus provincias y de sus ejes, si tiene algunos
            if (SessionHelper.CurrentUserProfile.Provinces.Count > 0 && !SessionHelper.CurrentUserProfile.Provinces.Any(x => x.Number == dto.CUE.Substring(0, 2)))
            {
                ViewBag.MessageResult = new MessageResult() { Text = INETResources.ImprovementPlan_InvalidUserProvince, Type = MessageResultTypeEnum.Attention };
                doSave = false;
            }

            // solo puede crear de sus provincias y de sus ejes, si tiene algunos
            /*if (doSave && SessionHelper.GetFields().Count > 0 && !SessionHelper.HasField(dto.FieldId))*/ //SIA 15-07-2022 - con line22, el fieldId Deprecado!
            if (doSave && SessionHelper.GetFields().Count == 0 )
            {
                ViewBag.MessageResult = new MessageResult() { Text = INETResources.ImprovementPlan_InvalidUserField, Type = MessageResultTypeEnum.Attention };
                doSave = false;
            }

            // el cue debe ser real y valido para el tipo de plan y el campo para seleccionado debe ser válido
            if (doSave && !_improvementPlanService.IsValidCUE(dto.CUE, dto.ImprovementPlanTypeId))
            {
                ViewBag.MessageResult = new MessageResult() { Text = INETResources.CUE_NotValidCUE, Type = MessageResultTypeEnum.Attention };
                doSave = false;
            }

            // el eje debe ser válido si es un plan de tipo institucional y un cue de dependencia nacional
            if (doSave && !_improvementPlanService.IsValidCueField(dto.CUE, dto.ImprovementPlanTypeId, dto.FieldId))
            {
                ViewBag.MessageResult = new MessageResult() { Text = INETResources.CUE_FieldNotValid, Type = MessageResultTypeEnum.Attention };
                doSave = false;
            }

            // no se puede cambiar el CUE a un plan existente
            if (doSave && _improvementPlanService.IsDifferentCUE(dto.Id, dto.CUE))
            {
                ViewBag.MessageResult = new MessageResult() { Text = INETResources.CUE_CannotChangeCUE, Type = MessageResultTypeEnum.Attention };
                doSave = false;
            }

            // no se puede cambiar el tipo de plan a un plan creado
            if (doSave && !_improvementPlanService.CanChangeImprovementPlanType(dto.Id, dto.ImprovementPlanTypeId))
            {
                ViewBag.MessageResult = new MessageResult() { Text = INETResources.ImprovementPlan_CantChangeType, Type = MessageResultTypeEnum.Attention };
                doSave = false;
            }

            // debe tener la documentacion necesaria
            if (doSave && !_improvementPlanService.HasDocumentation(dto.Id) && _documentation == null)
            {
                ViewBag.MessageResult = new MessageResult() { Text = INETResources.ImprovementPlan_DocumentationRequired, Type = MessageResultTypeEnum.Attention };
                doSave = false;
            }

            ModelState.Remove("ArticulatorExp");
            ModelState.Remove("ArticulatorLineId");
            ModelState.Remove("Summary");

            if (ModelState.IsValid && doSave)
            {
                var plan = _improvementPlanService.Save(dto);

                if (_documentation != null)
                {
                    _improvementPlanService.AddDocumentationToPlan(plan, _documentation);
                }

                if (_attachment != null)
                {
                    _improvementPlanService.AddAttachmentToPlan(plan, _attachment);
                }
                if (_attachmentAdd != null)
                {
                    _improvementPlanService.AddAttachmentExtraToPlan(plan, _attachmentAdd);
                }

                AddMessageResult(String.Format(INETResources.ImprovementPlan_Save, plan.Identifier), MessageResultTypeEnum.Confirmation, true);

                return RedirectToAction("Details", new { id = plan.Id });
            }

            return View(Load(dto));
        }

        #endregion

        #region Improvement Plan Details

        public ActionResult Details(int id, int? tab)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ));

            try
            {
                if (!ImprovementPlanPermissions.HasAccess(id))
                {
                    AddMessageResult(INETResources.ImprovementPlan_NoPermissions, MessageResultTypeEnum.Attention);
                    return RedirectToAction("List");
                }

                CheckMessageResult();
                var planDTO = _improvementPlanService.GetImprovementPlanForVisualization(id);

                return View(planDTO);
            }
            catch (Exception ex)
            {
                

                AddMessageResult(ex.Message + "-" +  ex.InnerException.ToString(), MessageResultTypeEnum.Attention);
                return RedirectToAction("List");
            }
        }


        [HttpPost]
        public ActionResult BlockPlan(int improvementPlanId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.BLOCK, plan.StatusId) && ImprovementPlanPermissions.HasAccess(plan));

            if (_improvementPlanService.Block(improvementPlanId))
            {
                AddMessageResult(INETResources.ImprovementPlan_Block_Success, MessageResultTypeEnum.Confirmation, true);
                return Json(new { errorMessage = "" });
            }

            return Json(new { errorMessage = INETResources.ImprovementPlan_Block_Error });
        }

        [HttpPost]
        public ActionResult SetEvaluator(int improvementPlanId, int? evaluatorId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);
            this.HasAccess(plan.StatusId == (int)StageStatusEnum.EnEvaluacion && SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.ASSIGN_EVALUATOR) && ImprovementPlanPermissions.HasAccess(plan));

            return Json(_improvementPlanService.SetEvaluator(plan, evaluatorId));
        }

        [HttpPost]
        public ActionResult SaveFileNumbers(int improvementPlanId, List<string> fileNumbers)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Solicitudes.FILE_NUMBER) && ImprovementPlanPermissions.HasAccess(plan));

            var lst = new List<FileNumberDTO>();

            foreach (var fn in fileNumbers)
                lst.Add(new FileNumberDTO() { LineId = Convert.ToInt32(fn.Split('|')[0]), FileNumber = fn.Split('|')[1] });

            return Json(_improvementPlanService.SaveFileNumbers(improvementPlanId, lst));
        }

        public ActionResult ExportImprovementPlansToPDF(int improvementPlanId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ) && ImprovementPlanPermissions.HasAccess(plan));

            var stream = _improvementPlanService.ExportImprovementPlansToPDF(improvementPlanId);
            return File(stream, "application/pdf", "Plan_de_Mejora_" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".pdf");
        }

        public ActionResult ListUserTimeLine(int improvementPlanId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ) && ImprovementPlanPermissions.HasAccess(plan));

            return Json(_improvementPlanService.ListTimeLineHistory(improvementPlanId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeStatus(int improvementPlanId, int statusId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);
            this.HasAccess(ImprovementPlanPermissions.CanChangeStatus(plan));

            if (plan != null)
            {
                var statusChanged = _improvementPlanService.ChangeStatus(improvementPlanId, statusId);
                if (statusChanged)
                    AddMessageResult(INETResources.ChangeStatus_Success, MessageResultTypeEnum.Confirmation, true);
                else
                    AddMessageResult(INETResources.ChangeStatus_Error, MessageResultTypeEnum.Error, true);
                return Json(statusChanged);
            }
            else
            {
                AddMessageResult(INETResources.ChangeStatusNoSolicitudes_Error, MessageResultTypeEnum.Error, true);
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult DeletePlan(int improvementPlanId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);

            if (plan.Id > 0)
            {
                this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.DELETE, plan.StatusId) && ImprovementPlanPermissions.HasAccess(plan));

                _improvementPlanService.Delete(plan.Id);
                AddMessageResult(String.Format(INETResources.ImprovementPlan_Delete, plan.Identifier), MessageResultTypeEnum.Confirmation, true);
                return Json(new { messageError = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { messageError = string.Format(INETResources.ImprovementPlan_Delete_Error, "") }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult ListLinesByField(int FieldId)
        {

            var data = _improvementPlanService.ListLinesByField(FieldId).ToList();

            List<KeyValuePair<int, string>> lista = new List<KeyValuePair<int, string>>();

            foreach ( Line  l in data)
            {
                lista.Add(new KeyValuePair<int, string>(l.Id, l.Description));

            }

            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListSubField(int FieldId)
        {
            var data = _improvementPlanService.ListSubFields(FieldId);
            List<KeyValuePair<int, string>> lista = new List<KeyValuePair<int, string>>();

            foreach (SubField l in data)
            {
                lista.Add(new KeyValuePair<int, string>(l.Id, l.Description));

            }



            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListLinesBySubField(int SubFieldId)
        {
            List<KeyValuePair<int, string>> lista = new List<KeyValuePair<int, string>>();
            var data = _improvementPlanService.ListLinesBySubField(SubFieldId);
            foreach (Lines_22 l in data)
            {
                lista.Add(new KeyValuePair<int, string>(l.Id, l.Description));

            }

            return Json(lista, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ListLines22()
        {
            List<KeyValuePair<int, string>> lista = new List<KeyValuePair<int, string>>();
            var data = _improvementPlanService.ListLines22();
            foreach (Lines_22 l in data)
            {
                lista.Add(new KeyValuePair<int, string>(l.Id, l.SubField.Code+ l.Code+ " " + l.Description));
            
            }

            return Json(lista, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Line22DTO(int Line22Id)
        {
            List<KeyValuePair<int, string>> lista = new List<KeyValuePair<int, string>>();
            var data = _improvementPlanService.getLine22DTO(Line22Id);
            var json = new Line22DTOjson(data);



            return Json(json, JsonRequestBehavior.AllowGet);
        }



        #endregion

        #region Incidences

        [HttpPost]
        public ActionResult ChangeIncidenceStatus(int improvementPlanId, int incidenceId, int? solicitudeId, bool returnAll)
        {
            var incidence = _incidenceService.Get(incidenceId);
            if (IncidencePermissions.CanChangeStatus(incidence))
            {
                _incidenceService.Close(incidence.Id);
                var incidences = _improvementPlanService.GetById(improvementPlanId).Incidences;

                if (!returnAll)
                    return PartialView("_IncidencesList", _incidenceService.ListForUser(SessionHelper.CurrentUserId));

                if (solicitudeId.HasValue)
                    return PartialView("_Incidences", incidences.Where(x => x.SolicitudeId == solicitudeId.Value).OrderBy(x => x.Id).ToList());

                return PartialView("_Incidences", incidences.Where(x => !x.SolicitudeId.HasValue).OrderBy(x => x.Id).ToList());
            }

            return Json("");
        }

        [HttpPost]
        public ActionResult SaveIncidence(IncidenceDTO incidenceDTO)
        {
            var plan = _improvementPlanService.GetById(incidenceDTO.ImprovementPlanId);

            if (incidenceDTO.SolicitudeId.HasValue)
            {
                var sol = _solicitudeService.Get(incidenceDTO.SolicitudeId.Value);
                this.HasAccess(SolicitudePermissions.Incidences.CanCreate(sol.StatusId, plan.StatusId, plan.EvaluatorUserId.HasValue ? plan.EvaluatorUserId.Value : 0));
            }
            else
            {
                this.HasAccess(ImprovementPlanPermissions.Incidences.CanCreate(plan));
            }

            if (ModelState.IsValid)
            {
                incidenceDTO.UserId = SessionHelper.CurrentUserId;
                if (!_incidenceService.SaveIncidence(incidenceDTO, plan.Id))
                {
                    throw new Exception(INETResources.Incidences_Save_Error.ToString());
                }
            }

            if (incidenceDTO.IsSolicitudeIncidence)
                return Json(new { success = true });

            var plan_incidences = _incidenceService.ListForPlan(plan.Id);
            return PartialView("_Incidences", plan_incidences.ToList());
        }

        [HttpPost]
        public ActionResult SaveIncidenceComment(int improvementPlanId, int incidenceId, int? solicitudeId, string text, bool returnAll)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.INCIDENTS_WRITE));

            _incidenceService.SaveComment(incidenceId, SessionHelper.CurrentUserProfile.UserName, text);
            var incidences = _improvementPlanService.GetById(improvementPlanId).Incidences;

            if (!returnAll)
                return PartialView("_Incidences", incidences.Where(x => x.Id == incidenceId).ToList());

            if (solicitudeId.HasValue)
                return PartialView("_Incidences", incidences.Where(x => x.SolicitudeId == solicitudeId.Value).OrderBy(x => x.Id).ToList());

            return PartialView("_Incidences", incidences.Where(x => !x.SolicitudeId.HasValue).OrderBy(x => x.Id).ToList());
        }

        public ActionResult ListImprovementPlanIncidences(int improvementPlanId)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.INCIDENTS_READ));
            return PartialView("_Incidences", _improvementPlanService.GetById(improvementPlanId).Incidences.Where(x => !x.SolicitudeId.HasValue).OrderBy(x => x.Id).ToList());
        }

        public ActionResult GetIncidence(int incidenceId)
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] { PermissionConstants.ImprovementPlan.INCIDENTS_READ, PermissionConstants.Solicitudes.INCIDENTS_READ }));

            var lst = new List<Incidence>();
            var incidence = _incidenceService.Get(incidenceId);
            if (incidence != null)
                lst.Add(incidence);

            return PartialView("_Incidences", lst);
        }

        #endregion
        #region LineasRelacionadas
        [HttpPost]
        public ActionResult SaveRelatedLine(int ImprovementPlanId, int Line22Id)
        {
            try
            {
                var plan = _improvementPlanService.GetById(ImprovementPlanId);



                if (ModelState.IsValid)
                {

                    if (!_improvementPlanService.SaveRelatedLine(Line22Id, plan.Id))
                    {
                        throw new Exception(INETResources.ImprovementPlan_Save.ToString());
                    }
                }

                //List<RelatedLines_22> l = new List<RelatedLines_22>();
                //RelatedLines_22 r = new RelatedLines_22();
               
                //r.ImprovementPlanId = r.Id;
                //r.Id = 1;
                //r.Line22Id = Line22Id;
                //l.Add(r);

                var rl = _improvementPlanService.ListRelatedLines(plan.Id);
                return PartialView("_RelatedLines", rl);
            } catch (Exception ex)
            {
                return null;

            }
        }

        #endregion
        #region Comments
        [HttpPost]
        public ActionResult SaveImprovementPlanComment(int improvementPlanId, string text)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);

            this.HasAccess(ImprovementPlanPermissions.Incidences.CanCreate(plan));

            _improvementPlanService.SaveComment(improvementPlanId, SessionHelper.CurrentUserId, text);
            return PartialView("_Comments", _improvementPlanService.GetById(improvementPlanId).Comments.OrderBy(x => x.Id).ToList());

        }

        #endregion

        #region Solicitudes

        [HttpPost]
        public ActionResult UpdateSolicitudes(int planId, String action, int[] selectedSolicitudes)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(planId));
            int success = 0;
            var actionName = "actualizaron";
            if (selectedSolicitudes == null)
            {
                var solicitudes = _solicitudeService.List(planId);

                foreach (Solicitude s in solicitudes)
                {
                    if (action.Equals("approve-all"))
                    {
                        actionName = "aprobaron";
                        if (_solicitudeService.Approve(s.Id)) success++;
                    }
                    else if (action.Equals("reject-all"))
                    {
                        actionName = "rechazaron";
                        if (_solicitudeService.Reject(s.Id)) success++;

                    }

                }



                AddMessageResult(string.Format(INETResources.ImprovementPlan_SolicitudeActionResult, actionName, success, solicitudes.Count), success > 0 ? MessageResultTypeEnum.Confirmation : MessageResultTypeEnum.Attention, true);
            } else
            {


           
            if (selectedSolicitudes.Count() < 1)
            {
                AddMessageResult(INETResources.ImprovementPlan_NoSolicitudesSelected, MessageResultTypeEnum.Error, true);
            }
            else
            {
                   

                 
                        foreach (int solicitudeId in selectedSolicitudes)
                        {
                            if (action.Equals("process"))
                            {
                                actionName = "marcaron en proceso";
                                if (_solicitudeService.InProcess(solicitudeId)) success++;
                            }
                            else if (action.Equals("approve"))
                            {
                                actionName = "aprobaron";
                                if (_solicitudeService.Approve(solicitudeId)) success++;
                            }
                            else if (action.Equals("reject"))
                            {
                                actionName = "rechazaron";
                                if (_solicitudeService.Reject(solicitudeId)) success++;
                            }
                            else if (action.Equals("delete"))
                            {
                                actionName = "eliminaron";
                                if (_solicitudeService.CanDeleteSolicitude(solicitudeId))
                                {
                                    _solicitudeService.Delete(solicitudeId);
                                    success++;
                                }
                            }
                        }
                        AddMessageResult(string.Format(INETResources.ImprovementPlan_SolicitudeActionResult, actionName, success, selectedSolicitudes.Count()), success > 0 ? MessageResultTypeEnum.Confirmation : MessageResultTypeEnum.Attention, true);

                    


         
              }
            }

            return RedirectToAction("Details", new { id = planId, tabId = 2 });
        }

        [HttpPost]
        public ActionResult SaveSolicitude(SolicitudeDTO solicitudeDTO)
        {

            this.HasAccess(ImprovementPlanPermissions.HasAccess(solicitudeDTO.ImprovementPlanId));

            if (solicitudeDTO.SolicitudeId == 0)
            {
                // está creando
                if (!ImprovementPlanPermissions.Solicitudes.CanCreate(solicitudeDTO.ImprovementPlanId))
                {
                    throw new ApplicationException("El usuario no posee permisos para crear un solicitado");
                }
            }
            else
            {
                // está editando
                var solicitude = _solicitudeService.Get(solicitudeDTO.SolicitudeId);
                if (!SolicitudePermissions.CanEdit(solicitude))
                {
                    throw new ApplicationException("El usuario no posee permisos para editar el solicitado");
                }
            }

            if (_solicitudeService.Save(solicitudeDTO))
            {
                return Json(new { success = true });
                //return PartialView("_Solicitudes", _solicitudeService.List(solicitudeDTO.ImprovementPlanId));
            }

            return Json("");
        }

        [HttpPost]
        public ActionResult DeleteSolicitude(int improvementPlanId, int solicitudeId)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId));

            if (_solicitudeService.CanDeleteSolicitude(solicitudeId))
            {
                _solicitudeService.Delete(solicitudeId);
                return Json(new { success = true });
            }

            return Json("");
        }

        [HttpPost]
        public ActionResult BlockSolicitude(int improvementPlanId, int solicitudeId)
        {
            var s = _solicitudeService.Get(solicitudeId);
            this.HasAccess(SolicitudePermissions.CanBlock(s) && ImprovementPlanPermissions.HasAccess(s.ImprovementPlan));

            if (_solicitudeService.Block(solicitudeId))
                return Json(new { success = true });

            return Json("");
        }

        [HttpPost]
        public ActionResult SaveSolicitudeComment(int improvementPlanId, bool includeInDictum, int solicitudeId, string text)
        {
            var solicitude = _solicitudeService.Get(solicitudeId);
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId) && SolicitudePermissions.Comments.CanCreate(solicitude.StatusId, solicitude.ImprovementPlan.StatusId, solicitude.ImprovementPlan.EvaluatorUserId ?? 0));

            var plan = _improvementPlanService.GetById(improvementPlanId);
            _solicitudeService.SaveComment(solicitudeId, includeInDictum, SessionHelper.CurrentUserProfile.UserName, text);
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ImportSolicitudes(HttpPostedFileBase file, int planId)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(planId) && ImprovementPlanPermissions.Solicitudes.CanCreate(planId));

            if (ModelState.IsValid)
            {
                var messageResult = _solicitudeService.ImportSolicitudes(file.InputStream, planId, System.IO.Path.GetExtension(file.FileName));
                if (string.IsNullOrEmpty(messageResult))
                {
                    AddMessageResult(INETResources.Import_Solicitudes_Success, MessageResultTypeEnum.Confirmation, true);
                }
                else
                {
                    AddMessageResult(string.Format(INETResources.Import_Solicitudes_Error, messageResult), MessageResultTypeEnum.Error, true);
                }
            }

            return RedirectToAction("Details", new { id = planId });
        }

        [HttpPost]
        public ActionResult ImportRelatedPlanesSolicitudes(HttpPostedFileBase file, int planId)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(planId) && ImprovementPlanPermissions.Solicitudes.CanCreate(planId));

            if (ModelState.IsValid)
            {
                var messageResult = _solicitudeService.ImportRelatedPlanesSolicitudes(file.InputStream, planId, System.IO.Path.GetExtension(file.FileName));
                if (string.IsNullOrEmpty(messageResult))
                {
                    AddMessageResult(INETResources.Import_Solicitudes_Success, MessageResultTypeEnum.Confirmation, true);
                }
                else
                {
                    AddMessageResult(string.Format(INETResources.Import_Solicitudes_Error, messageResult), MessageResultTypeEnum.Error, true);
                }
            }

            return RedirectToAction("Details", new { id = planId });
        }

        [HttpPost]
        public ActionResult ImportSolicitudesApprovals(HttpPostedFileBase file_update, int planId)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(planId));

            if (ModelState.IsValid)
            {
                var messageResult = _solicitudeService.ImportSolicitudesApprovals(file_update.InputStream, planId, System.IO.Path.GetExtension(file_update.FileName));
                if (string.IsNullOrEmpty(messageResult))
                {
                    AddMessageResult(INETResources.Import_Solicitudes_Success, MessageResultTypeEnum.Confirmation, true);
                }
                else
                {
                    AddMessageResult(string.Format(INETResources.Import_Solicitudes_Error, messageResult), MessageResultTypeEnum.Error, true);
                }
            }

            return RedirectToAction("Details", new { id = planId });
        }

        public ActionResult ExportSolicitudeToPDF(int solicitudeId)
        {
            var solicitude = _solicitudeService.Get(solicitudeId);
            this.HasAccess(ImprovementPlanPermissions.HasAccess(solicitude.ImprovementPlan));

            var stream = _solicitudeService.ExportSolicitudeToPDF(solicitudeId);
            return File(stream, "application/pdf", "Solicitud_" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".pdf");
        }

        public ActionResult ExportSolicitudes(int improvementPlanId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);

            this.HasAccess(ImprovementPlanPermissions.HasAccess(plan));

            if (SessionHelper.HasRole(INET.Core.Constants.RoleConstants.OPERADOR_PROVINCIAL) && plan.StatusId != StageStatusEnum.EnCargaProvincia.ToInt())
            {
                var stream = _solicitudeService.ExportSolicitudesToPDF(improvementPlanId);
                return File(stream, "application/pdf", "ListSolicitudes_" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".pdf");
            }
            else
            {
                var stream = _solicitudeService.ExportSolicitudesToExcel(improvementPlanId);
                return File(stream, "application/vnd.ms-excel", "ListSolicitudes_" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".xlsx");
            }
        }

        public ActionResult ListSolicitudeComments(int solicitudeId)
        {
            var solicitude = _solicitudeService.Get(solicitudeId);
            this.HasAccess(ImprovementPlanPermissions.HasAccess(solicitude.ImprovementPlan) && SessionHelper.HasPermission(PermissionConstants.Solicitudes.COMMENTS_READ));

            var comments = _solicitudeService.Get(solicitudeId).Comments
                                .OrderByDescending(x => x.Date)
                                .Select(x =>
                                        new
                                        {
                                            User = x.UserProfile.FullName,
                                            IncludeInDictum = x.IncludeInDictum,
                                            CommentDate = x.Date.ToString("dd/MM/yyyy"),
                                            Text = x.Text
                                        });

            return Json(new { comments }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListSolicitudeIncidences(int solicitudeId)
        {
            var solicitude = _solicitudeService.Get(solicitudeId);
            this.HasAccess(ImprovementPlanPermissions.HasAccess(solicitude.ImprovementPlan) && SessionHelper.HasPermission(PermissionConstants.Solicitudes.INCIDENTS_READ));

            return PartialView("_Incidences", _solicitudeService.Get(solicitudeId).Incidences.OrderBy(x => x.Id).ToList());
        }

        public ActionResult ListSolicitudeSpecializationsByCue(int improvementPlanId, string cue)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId));

            return Json(_solicitudeService.ListSpecializationsByCue(improvementPlanId, cue), JsonRequestBehavior.AllowGet);
        }


        public ActionResult ListSolicitudeManagementByCue(int improvementPlanId, string cue)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId));

            return Json(_solicitudeService.ListManagementByCue(improvementPlanId, cue), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadSolicitudeCombos(int improvementPlanId, int improvementPlanStatusId, int solicitudeId)
        {
            var solicitude = _solicitudeService.Get(solicitudeId);
            if (solicitude == null || !ImprovementPlanPermissions.HasAccess(improvementPlanId))
            {
                this.HasAccess(false);
            }

            var statuses = SolicitudePermissions.ListAvailableStatuses(solicitude.StatusId, improvementPlanStatusId);
            if (!statuses.Where(x => x.Id == solicitude.StatusId).Any())
            {
                var _statuses = new List<Status>();
                _statuses.Add(solicitude.Status);
                statuses = _statuses;
            }

            return Json(new
            {
                Specializations = _solicitudeService.ListSpecializations(improvementPlanId, solicitudeId),
                Status = statuses.Select(x => new { Id = x.Id, Description = x.Description }).ToList()
            }, JsonRequestBehavior.AllowGet);
        }



   


        public ActionResult LoadLineasCombos(int improvementPlanId)
        {
            List<Line> lista = new List<Line>();
           var line =  _solicitudeService.GetLines(improvementPlanId);
            lista.Add(line);
            return Json(new
            {
                Lines = lista

            }, JsonRequestBehavior.AllowGet) ;
        }



        public ActionResult GetSolicitude(int solicitudeId)
        {
            var solicitude = _solicitudeService.Get(solicitudeId);
            this.HasAccess(ImprovementPlanPermissions.HasAccess(solicitude.ImprovementPlan));

            return Json(new
            {
                Id = solicitude.Id,
                ApprovedAmount = solicitude.ApprovedAmount,
                ApprovedPriceUnit = solicitude.ApprovedPriceUnit,
                ApprovedTotal = solicitude.ApprovedTotal,
                AvailableTotal = solicitude.AvailableTotal,
                CUE = solicitude.CUE,
                Details = solicitude.Details,
                ExpenditureTypeId = solicitude.ExpenditureTypeId,
                ExpenditureObjectTypeId =solicitude.ExpenditureObjectTypeId,
                FileNumber = solicitude.FileNumber,
                ImprovementPlanId = solicitude.ImprovementPlanId,
                Level = solicitude.Level,
                LineId = solicitude.LineId,
                Locked = solicitude.Locked,
                MeasurementUnitId = solicitude.MeasurementUnitId,
                ReassignedGrantedTotal = solicitude.ReassignedGrantedTotal,
                ReassignedId = solicitude.ReassignedId,
                ReassignedRequestedTotal = solicitude.ReassignedRequestedTotal,
                RequestedAmount = solicitude.RequestedAmount,
                RequestedPriceUnit = solicitude.RequestedPriceUnit,
                RequestedTotal = solicitude.RequestedTotal,
                SchoolYearId = solicitude.SchoolYearId,
                SolicitudeTypeId = solicitude.SolicitudeTypeId,
                Specialization = solicitude.Specialization,
                SpecializationCode = solicitude.SpecializationCode,
                StatusId = solicitude.StatusId,
                CommentsCount = solicitude.Comments.Count,
                IncidencesCount = solicitude.Incidences.Count,
                disabled = solicitude.StatusId == SolicitudeStatusEnum.Anulado.ToInt(),
                canEdit = SolicitudePermissions.CanEdit(solicitude)
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListSolicitudesToReassign(string cue, int improvementPlanId)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId));

            return Json(_solicitudeService.ListSolicitudesToReassign(cue, improvementPlanId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFileNumberForSolicitude(int improvementPlanId, int lineId)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId));

            return Json(new { FileNumber = _solicitudeService.GetFileNumberForSolicitude(improvementPlanId, lineId) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFileNumberForSolicitude_22(int improvementPlanId)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId));

            return Json(new { FileNumber = _solicitudeService.GetFileNumberForSolicitude(improvementPlanId) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListExpenditureObjectTypes(int improvementPlanId,int expenditureTypeId)
        {
            List<KeyValuePair<int, string>> Objects = new List<KeyValuePair<int, string>>();
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId));
            var  _data=  _solicitudeService.ListExpenditureObjectTypes(expenditureTypeId);

           
            Objects = new List<KeyValuePair<int, string>>();
       

            foreach (ExpenditureObjectType ot in _data)
            {
                Objects.Add(new KeyValuePair<int, string>(ot.Id, ot.Description));

            }

            return Json(Objects, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListExpenditureObjectTypesBySol(int solicitudeId)
        {
            List<KeyValuePair<int, string>> Objects = new List<KeyValuePair<int, string>>();
            var solicitude = _solicitudeService.Get(solicitudeId);
            this.HasAccess(ImprovementPlanPermissions.HasAccess(solicitude.ImprovementPlanId));
            

            var _data = _solicitudeService.ListExpenditureObjectTypes(solicitude.ExpenditureObjectTypeId);


            Objects = new List<KeyValuePair<int, string>>();


            foreach (ExpenditureObjectType ot in _data)
            {
                Objects.Add(new KeyValuePair<int, string>(ot.Id, ot.Description));

            }

            return Json(Objects, JsonRequestBehavior.AllowGet);
        }


        #endregion
        public ActionResult AccountDictumAmount(int DictumId)
        {
            var dictum =_documentService.GetDictum(DictumId);

            return Json(new { amount =_accountingService.DictumAmount(dictum) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveAccount(int? id, int improvementPlanId, decimal? aprovedAmount,decimal? rejectedAmount,string comments, int day,int month,int? year, int? expenditureObjectTypeId, string number, int? dictumId)
        {
            if (aprovedAmount == 0)
                throw new  Exception("Debe ingresar monto!");
            if (day == 0)
                throw new Exception("Debe ingresar dia!");
            if (month == 0)
                throw new Exception("Debe ingresar mes!");
            if (year == 0 || year is null)
                throw new Exception("Debe ingresar año!");
            if (expenditureObjectTypeId is null)
                throw new Exception("Debe ingresar Objeto de Gasto!");
            if (expenditureObjectTypeId==0)
                throw new Exception("Debe ingresar Objeto de Gasto!");
            if (number.Length== 0)
                throw new Exception("Ingrese Numero de Rendicion!");
            if (dictumId==0)
                throw new Exception("Ingrese Dictamen!");
           
           decimal dictumAmount= _accountingService.DictumAmount(_documentService.GetDictum(dictumId.Value));
            if (aprovedAmount> dictumAmount)
                throw new Exception("El monto aprobado Excede al monto del Dictamen!");
            DateTime date = new DateTime(year.Value, month, day);
            _accountingService.Save(id, improvementPlanId,aprovedAmount.Value, rejectedAmount, comments,date,expenditureObjectTypeId.Value,number,dictumId.Value);
           var AC= _accountingService.List(improvementPlanId);

            return Json(AC, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteAccount(int AccountId)
        {
           
           var result= _accountingService.Delete(AccountId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccount(int Id)
        {
            var account = _accountingService.Get(Id);
            //this.HasAccess(ImprovementPlanPermissions.HasAccess(solicitude.ImprovementPlan));

            return Json(new
            {
                id = account.id,
                improvementPlanId = account.improvementPlanId,
                aprovedAmount = account.aprovedAmount,
                comment= account.comment,
                DateDay =account.DateDay,
                DateMonth =account.DateMonth,
                DateYear   =account.DateYear,
                dictumId=account.dictumId,
                expenditureObjectTypeId=account.expenditureObjectTypeId,
                number =account.number,
                rejectedAmount=account.rejectedAmount
               
            }, JsonRequestBehavior.AllowGet);
        }

        #region Summary

        public ActionResult UpdateSummary(int improvementPlanId)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId));

            return PartialView("_Summary", _improvementPlanService.GetById(improvementPlanId).Solicitudes.ToList());
        }



        #endregion

        #region Documents

        public ActionResult ListTemplates(int templateTypeId)
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] {
                PermissionConstants.Dictums.CREATE,
                PermissionConstants.Dictums.EDIT,
                PermissionConstants.Resolutions.CREATE,
                PermissionConstants.Resolutions.EDIT,
                PermissionConstants.ImprovementPlan.READ
            }));
            return Json(_templateService.ListByTemplateType(templateTypeId)
                        .Select(x => new { Id = x.Id, Name = x.Name })
                        .ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListTemplatesForCreation(int templateTypeId, int planId)
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] {
                PermissionConstants.Dictums.CREATE,
                PermissionConstants.Dictums.EDIT,
                PermissionConstants.Resolutions.CREATE,
                PermissionConstants.Resolutions.EDIT
            }) && ImprovementPlanPermissions.HasAccess(planId));
            return Json(_templateService.ListByTemplateTypeForPlan(templateTypeId, planId)
                        .Select(x => new { Id = x.Id, Name = x.Name })
                        .ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Preview(int documentId, int templateId, string htmlContent, List<DocumentVariableDTO> variables)
        {
            var document = _documentService.Get(documentId);
            if (document.ImprovementPlanId.HasValue)
            {
                this.HasAccess(ImprovementPlanPermissions.HasAccess(document.ImprovementPlan));

                var fileName = string.Format("preview_{0}.pdf", DateTime.Now.Ticks);
                var preview = _templateService.Preview(documentId, htmlContent, variables ?? new List<DocumentVariableDTO>(), this.HttpContext.Request.ApplicationPath, "pdf");
                return File(preview.ToArray(), "application/pdf", fileName);

            }
            else
            {
                var reso = _documentService.GetResolution(documentId);
                this.HasAccess(ResolutionPermissions.CanEmit(reso));

                var fileName = string.Format("preview_{0}.docx", DateTime.Now.Ticks);
                var preview = _templateService.Preview(documentId, htmlContent, variables ?? new List<DocumentVariableDTO>(), this.HttpContext.Request.ApplicationPath, "docx");
                return File(preview.ToArray(), "application/docx", fileName);

            }
        }

        #region Dictums

        public ActionResult LoadDictumCombos(int dictumId)
        {
            var statuses = DictumPermissions.ListAvailableStatuses(dictumId);

            if (dictumId > 0)
            {
                var dictum = _documentService.GetDictum(dictumId);
                this.HasAccess(ImprovementPlanPermissions.HasAccess(dictum.ImprovementPlan));
                if (!statuses.Where(x => x.Id == dictum.StatusId).Any())
                {
                    var _statuses = new List<Status>();
                    _statuses.Add(dictum.Status);
                    statuses = _statuses;
                }
            }

            return Json(new
            {
                Statuses = statuses.Select(x => new { Id = x.Id, Description = x.Description }).ToList(),
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDictumEditData(int dictumId)
        {
            var dictum = _documentService.GetDictum(dictumId);
            this.HasAccess(ImprovementPlanPermissions.HasAccess(dictum.ImprovementPlan));
            try
            {
                return Json(
                        new
                        {
                            DictumId = dictum.Id,
                            TemplateTypeId = dictum.Template.TemplateTypeId,
                            TemplateId = dictum.TemplateId,
                            FileNumbers = _improvementPlanService.ListFileNumbers(dictum.ImprovementPlan),
                            FileNumber = (dictum.Solicitudes.Count > 0) ? dictum.Solicitudes.First().FileNumber : "",
                            StatusId = dictum.StatusId,
                            ExternalNumber = dictum.EligibilityExternalNumber,
                            ExternalEntity = dictum.ElegibilityExternalEntity,
                            SignDate = (dictum.SignatureDate.HasValue) ? dictum.SignatureDate.Value.ToString("dd/MM/yyyy") : "",
                            Body = dictum.Body,
                            SignedDocument = dictum.SignedDocument,
                            AnnexDocument = dictum.AnnexDocument,
                            Solicitudes = dictum.Solicitudes.Select
                            (x => new
                            {
                                Id = x.Id,
                                Line = x.Line.Code,
                                Details = x.Details,
                                Status = x.Status.Description,
                                RequestedTotal = x.RequestedTotal,
                                ApprovedTotal = x.ApprovedTotal
                            }).ToList()
                        }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public ActionResult GetDictumEmitData(int dictumId)
        {
            // TODO: Validar que el usuario tenga permisos sobre ese dictamen
            var dictum = _documentService.GetDictum(dictumId);
            var template = _templateService.Get(dictum.TemplateId);  // TODO: Devolver ya parseado?

            return Json(new
            {
                FileNumber = dictum.FileNumber,
                StatusId = dictum.StatusId,
                Content = template.Content
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteDictum(int improvementPlanId, int documentId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);

            this.HasAccess(ImprovementPlanPermissions.HasAccess(plan) && DictumPermissions.CanDelete(documentId));

            if (_documentService.DeleteDictum(documentId))
            {
                List<Document> documents;
                if (SessionHelper.HasAnyRole(new string[] { RoleConstants.OPERADOR_PROVINCIAL, RoleConstants.REFERENTE_JURISDICCIONAL }))
                {
                    documents = _documentService.List(improvementPlanId).Where(x => x.StatusId == DictumStatusEnum.Firmado.ToInt()).ToList();
                }
                else
                {
                    documents = _documentService.List(improvementPlanId).OfType<Dictum>().ToList<Document>();
                }

                return PartialView("_DocumentsList", documents);
            }

            return Json("");
        }

        [HttpPost]
        public ActionResult BlockDictum(int improvementPlanId, int dictumId)
        {
            var plan = _improvementPlanService.GetById(improvementPlanId);

            this.HasAccess(ImprovementPlanPermissions.HasAccess(plan) && DictumPermissions.CanBlock(dictumId));

            if (_documentService.BlockDictum(dictumId))
            {
                List<Document> documents;
                if (SessionHelper.HasAnyRole(new string[] { RoleConstants.OPERADOR_PROVINCIAL, RoleConstants.REFERENTE_JURISDICCIONAL }))
                {
                    documents = _documentService.List(improvementPlanId).Where(x => x.StatusId == DictumStatusEnum.Firmado.ToInt()).ToList();
                }
                else
                {
                    documents = _documentService.List(improvementPlanId).OfType<Dictum>().ToList<Document>();
                }

                return PartialView("_DocumentsList", documents);
            }

            return Json("");
        }

        [HttpPost]
        public ActionResult SaveDictum(DictumDTO dto,HttpPostedFileBase annexDocument)
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

            var result = _documentService.Save(dto, this.HttpContext.Request.ApplicationPath);
            if (result)
            {
                return PartialView("_DocumentsList", GetDocuments(plan));
            }

            return Json("");
        }

        [HttpPost]
        public ActionResult SaveDictumAnnex(DictumDTO dto,int planId,int dictumId, HttpPostedFileBase annexDocument)
        {
            if (annexDocument == null)
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

        /// <summary>
        /// List Plan documentos for display
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        private List<Document> GetDocuments(ImprovementPlan plan)
        {
            List<Document> documents;
            if (SessionHelper.HasAnyRole(new string[] { RoleConstants.OPERADOR_PROVINCIAL, RoleConstants.REFERENTE_JURISDICCIONAL }))
            {
                documents = _documentService.List(plan.Id).Where(x => x.StatusId == DictumStatusEnum.Firmado.ToInt()).ToList();
            }
            else
            {
                documents = _documentService.List(plan.Id).OfType<Dictum>().ToList<Document>();
            }
            return documents;
        }

        public ActionResult SignDictum(string SignDate, HttpPostedFileBase signedDocument, int planId, int dictumId, int statusId)
        {
            if (planId > 0 && dictumId > 0)
            {
                var plan = _improvementPlanService.GetById(planId);
                var dictum = _documentService.GetDictum(dictumId);

                this.HasAccess(ImprovementPlanPermissions.HasAccess(plan));

                bool sign = false;
                bool save = false;
                if (DictumPermissions.CanSign(dictum.StatusId) && statusId == (int)DictumStatusEnum.Firmado)
                {
                    sign = _documentService.SignDictum(dictum.Id, SignDate, true);
                    if (sign)
                    {
                        save = _documentService.SaveSignedDictumFile(dictum.Id, signedDocument);
                    }
                }
                else if (SessionHelper.HasRole(RoleConstants.ADMIN))
                {
                    sign = _documentService.SignDictum(dictum.Id, SignDate, false);
                    if (Request.Files.Count > 0)
                    {
                        _documentService.SaveSignedDictumFile(dictum.Id, signedDocument);
                    }
                    save = true;
                }
                if (save)
                {
                    return PartialView("_DocumentsList", GetDocuments(plan));
                }
                else
                {
                    //_documentService.UndoSignDictum(dictumId);
                }

            }
            return Json("");
        }

        public ActionResult ListSolicitudesForSelection(int improvementPlanId, int templateTypeId, string fileNumber)
        {
            this.HasAccess(ImprovementPlanPermissions.HasAccess(improvementPlanId));

            return PartialView("_SolicitudesSelector", _solicitudeService.List(improvementPlanId, templateTypeId, fileNumber));
        }

        #endregion

        #region Resolution

        //public ActionResult SignResolution(int resolutionId, int resolutionPlanId, HttpPostedFileBase file)
        //{
        //    if (ModelState.IsValid && file != null)
        //    {
        //        //if (_documentService.SignResolution(resolutionId, file))
        //        //{
        //        //    AddMessageResult(INETResources.Resolution_Save_Success, MessageResultTypeEnum.Confirmation, true);
        //        //}
        //        //else
        //        //{
        //        //    AddMessageResult(INETResources.Resolution_Save_Error, MessageResultTypeEnum.Error, true);
        //        //}
        //    }
        //    else
        //    {
        //        AddMessageResult(INETResources.Resolution_Save_Error, MessageResultTypeEnum.Error, true);
        //    }

        //    return RedirectToAction("Details", new { id = resolutionPlanId, tabId = 1 });
        //}

        //public ActionResult ListDictumsForSelection(int improvementPlanId, int templateTypeId, string fileNumber)
        //{
        //    return PartialView("_DictumsSelector", _documentService.ListDictumsForResolution(improvementPlanId, fileNumber));
        //}

        //[HttpPost]
        //public ActionResult SaveResolution(ResolutionDTO dto)
        //{
        //    if (dto.Id == 0)
        //    {
        //        this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Resolutions.CREATE));
        //    } else
        //    {
        //        this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Resolutions.CREATE));
        //    }
        //    var result = _documentService.Save(dto, this.HttpContext.Request.ApplicationPath);
        //    if (result)
        //        return PartialView("_DocumentsList", _documentService.List(dto.ImprovementPlanId));

        //    return Json("");
        //}



        //[HttpPost]
        //public ActionResult DeleteResolution(int improvementPlanId, int documentId)
        //{
        //    if (_documentService.DeleteResolution(documentId))
        //        return PartialView("_DocumentsList", _documentService.List(improvementPlanId));

        //    return Json("");
        //}



        //[HttpPost]
        //public ActionResult BlockResolution(int improvementPlanId, int resolutionId)
        //{
        //    if (_documentService.BlockResolution(resolutionId))
        //        return PartialView("_DocumentsList", _documentService.List(improvementPlanId));

        //    return Json("");
        //}

        //#endregion
        #endregion
        #endregion

        #region Logic Methods

        /// <summary>
        /// Inicializa el DTO de carga para editar un plan
        /// </summary>
        /// <param name="model">DTO a cargar. En caso de cargar la página por primera vez, deberá ser nulo.</param>
        /// <returns>El DTO inicializado para la vista de edición de un plan</returns>
        private ImprovementPlanEditorDTO Load(ImprovementPlanEditorDTO model)
        {
            if (model == null)
                model = new ImprovementPlanEditorDTO();

            model.SchoolYears = _solicitudeService.ListSchoolYears();

            if (SessionHelper.CurrentUserProfile.Fields.Count == 0)
            {
                model.Fields = _improvementPlanService.ListFields().Where(x => x.StatusId == (int)AxisStatusEnum.Vigente).ToList();
                //model.SubFields = _improvementPlanService.ListFields().Where(x => x.StatusId == (int)AxisStatusEnum.Vigente).ToList();
            }
            else
            {
                model.Fields = SessionHelper.GetFields().Where(x => x.StatusId == (int)AxisStatusEnum.Vigente).ToList();
            }


            model.ImprovementPlansTypes = _improvementPlanService.ListImprovementPlansTypes();
            if (SessionHelper.HasRole(RoleConstants.OPERADOR_PROVINCIAL))
            {
                model.ImprovementPlansTypes = model.ImprovementPlansTypes.Where(x => x.Id != (int)ImprovementPlanTypeEnum.Nacional).ToList();
            }

            // ReceptionDate Combos
            model.Days.AddRange(_improvementPlanService.ListDays());
            model.Months.AddRange(_improvementPlanService.ListMonths());
            model.Years.AddRange(_improvementPlanService.ListYears());

            if (model.SchoolYearId == 0) model.SchoolYearId = _solicitudeService.GetActiveSchoolYear().Id;

            //Lineas22
            model.Lines22 = _solicitudeService.ListLines22();
            model.SubFields = _solicitudeService.ListSubFields();
            if (model.Line22Id!=null)
            {
                var dto22 = _solicitudeService.getLine22DTO(model.Line22Id.Value);
                model.Lines = dto22.Lines;
                model.SubFields = dto22.SubFields;

            }
            // Lineas, SubEjes



            return model;
        }

        /// <summary>
        /// Carga los filtros de búsqueda
        /// </summary>
        /// <returns>Un listado de FiltersDTO con los filtros a buscar</returns>
        private FiltersDTO LoadSearchFilters()
        {
            var filters = new FiltersDTO();

            filters.SchoolYears = _solicitudeService.ListSchoolYears();

            // solo de la misma provincia
            var provNumbers = new List<String>();
            var userProfile = SessionHelper.CurrentUserProfile;
            if (userProfile.Provinces.Count > 0)
            {
                foreach (INET.Data.Province prov in userProfile.Provinces)
                {
                    provNumbers.Add(prov.Number);
                }
            }
            filters.Evaluators = _userProfileService.ListEvaluatorsUsers(provNumbers);

            filters.ImprovementPlansTypes = _improvementPlanService.ListImprovementPlansTypes();
            filters.Fields = SessionHelper.GetFields();
            filters.Status = _improvementPlanService.ListStatusForPlan();
            filters.Days.AddRange(_improvementPlanService.ListDays());
            filters.Months.AddRange(_improvementPlanService.ListMonths());
            filters.Years.AddRange(_improvementPlanService.ListYears());

            return filters;
        }

        #endregion
    }
}
