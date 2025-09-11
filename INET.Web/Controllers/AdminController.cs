using INET.Core.Constants;
using INET.Core.Enums;
using INET.Core.Models;
using INET.Data;
using INET.Services;
using INET.Services.DTO;
using INET.Utils.Helpers;
using Resources;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace INET.Web.Controllers
{
    public class AdminController : BaseController
    {
        UserProfileService _userProfileService;
        BudgetService _budgetService;
        TemplateService _templateService;
        SolicitudeService _solicitudeService;

        public AdminController(UserProfileService userProfileService, BudgetService budgetService, TemplateService templateService, SolicitudeService solicitudeService)
        {
            _userProfileService = userProfileService;
            _budgetService = budgetService;
            _templateService = templateService;
            _solicitudeService = solicitudeService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] {
                PermissionConstants.Administration.USERS,
                PermissionConstants.Administration.BUDGET,
                PermissionConstants.Administration.TEMPLATES,
                PermissionConstants.Administration.ERROR_LOG
            }));

            CheckMessageResult();
            return View(Load());
        }

        [HttpGet]
        public ActionResult Roles()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.ROLES));

            CheckMessageResult();
            var roles = new RoleDTO();
            roles.Roles = _userProfileService.ListRoles().ToList();
            //roles.Roles = _userProfileService.ListRoles().ToList();
            return View(roles);
        }

        [HttpGet]
        public ActionResult RoleEditor(int id)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.ROLES));

            CheckMessageResult();

            var roleDTO = LoadRole(id);
            return View(roleDTO);
        }

        [HttpPost]
        public ActionResult RoleEditor(RoleDTO roleDTO)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.ROLES));
            var doSave = false;
            var result_msg = INETResources.Roles_Error;

            CheckMessageResult();

            // Do validations
            var exists = _userProfileService.GetRole(roleDTO.RoleId);
            if (exists != null)
            {
                if (_userProfileService.ValidateRolePermissionsDTO(roleDTO))
                {
                    doSave = true;
                }
                else
                {
                    result_msg = INETResources.Roles_Invalid;
                }
            }
            else
            {
                result_msg = INETResources.Roles_Invalid;
            }

            if (doSave)
            {
                // Do Save actions
                if (_userProfileService.SaveRolePermissions(roleDTO))
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = INETResources.Roles_Save_Success });
                    }
                    else
                    {
                        AddMessageResult(INETResources.Roles_Save_Success, MessageResultTypeEnum.Confirmation, true);
                        return RedirectToAction("RoleEditor", new { id = roleDTO.RoleId });
                    }
                }
                else
                {
                    result_msg = INETResources.Roles_Error;
                }
            }

            // Show error message if not saved
            roleDTO = LoadRole(roleDTO);
            if (Request.IsAjaxRequest())
            {
                return Json(new { error = result_msg });
            }
            else
            {
                ViewBag.MessageResult = new MessageResult() { Text = result_msg, Type = MessageResultTypeEnum.Error };
                return View(roleDTO);
            }
        }

        [HttpGet]
        public ActionResult Users()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.USERS));
            CheckMessageResult();
            return View(Load());
        }

        [HttpGet]
        public ActionResult Budgets()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.BUDGET));

            return View(_solicitudeService.ListSchoolYears());
        }

        [HttpGet]
        public ActionResult Budgets_216()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.BUDGET));

            return View(_solicitudeService.ListSchoolYears());
        }

        [HttpPost]
        public ActionResult SaveUser(UserDTO dto)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.USERS));

            // Quito validacion del password en caso de que sea una edición
            if (dto.UserId > 0)
            {
                var password = ModelState["Password"];
                password.Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                if (_userProfileService.IsValidUserName(dto.UserId, dto.UserName))
                {
                    _userProfileService.SaveUser(dto);
                    AddMessageResult(INETResources.Users_Save_Success, MessageResultTypeEnum.Confirmation, true);
                    return Json(new { messageError = "" });
                }
                else
                {
                    return Json(new { messageError = String.Format(INETResources.Users_UserAlreadyExist, dto.UserName) });
                }
            }

            return Json(new { messageError = String.Format(INETResources.Users_Error, dto.UserName) });
        }

        public ActionResult DeleteUser(int userId)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.USERS));

            if (ModelState.IsValid)
            {
                _userProfileService.DeleteUser(userId, true);
                AddMessageResult(INETResources.Users_Delete_Success, MessageResultTypeEnum.Confirmation, true);
                return Json(new { messageError = "" });
            }

            return Json(new { messageError = INETResources.InvalidModel });
        }


        [HttpPost]
        public ActionResult DisableUser(int userId)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.USERS));

            _userProfileService.DisableUser(userId);
            return Json(new { messageError = "" });
        }

        public ActionResult ExportUsersToExcel()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.USERS));

            var stream = _userProfileService.ExportUsersToExcel();
            return File(stream, "application/vnd.ms-excel", "Listado_de_Usuarios_" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".xls");
        }

        public ActionResult ListBudgets(int schoolYearId)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.BUDGET));

            return Json(_budgetService.ListBudget(schoolYearId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListBudgets216(int schoolYearId)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.BUDGET));

            return Json(_budgetService.ListBudget216(schoolYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveBudget(int budgetId, decimal? ammount)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.BUDGET));

            if (ammount.HasValue)
                _budgetService.Save(budgetId, ammount.Value);
            return Json("");
        }

        [HttpPost]
        public ActionResult SaveBudget216(int budgetId, decimal? ammount)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.BUDGET));

            if (ammount.HasValue)
                _budgetService.Save216(budgetId, ammount.Value);
            return Json("");
        }

        [HttpGet]
        public ActionResult ListTemplateTypeFieldsAndBlocks(int templateTypeId)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.TEMPLATES));

            var templateTypeFields = _templateService.ListTemplateTypeFields(templateTypeId)
                .Select(x => new { Field = x.Field, Description = x.Description }).ToList();

            var templateTypeBlocks = _templateService.ListTemplateTypeBlocks(templateTypeId)
                .Select(x => new { Block = x.Block, Description = x.Description }).ToList();

            return Json(new { Fields = templateTypeFields, Blocks = templateTypeBlocks }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult TemplatesList()
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.TEMPLATES));

            CheckMessageResult();
            return View(_templateService.List());
        }

        [HttpGet]
        public ActionResult TemplateEditor(int? id)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.TEMPLATES));

            return View(LoadTemplate(id));
        }

        [HttpPost]
        public ActionResult TemplateEditor(TemplateDTO model)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.TEMPLATES));

            if (ModelState.IsValid)
            {
                if (_templateService.IsValid(model.TemplateTypeId, model.Content))
                {
                    _templateService.Save(model);
                    AddMessageResult(INETResources.Templates_Save_Success, MessageResultTypeEnum.Confirmation, true);
                    return RedirectToAction("TemplatesList");
                }
                else
                {
                    AddMessageResult(INETResources.Templates_NotValid, MessageResultTypeEnum.Confirmation, true);
                }
            }
            else
            {
                AddMessageResult(INETResources.Template_Error, MessageResultTypeEnum.Error, true);
            }

            CheckMessageResult();
            return View(LoadTemplate(model.Id));
        }


        public ActionResult DeleteTemplate(int templateId)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.TEMPLATES));

            if (_templateService.DeleteTemplate(templateId))
            {
                AddMessageResult(INETResources.Templates_Delete_Success, MessageResultTypeEnum.Confirmation, true);
                return Json(new { messageError = "" });
            }

            return Json(new { messageError = INETResources.Template_Delete_Error });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Preview(string htmlContent, string htmlHeader, int prevTemplateId, string htmlFormat)
        {
            this.HasAccess(SessionHelper.HasPermission(PermissionConstants.Administration.TEMPLATES));

            var fileName = "Plantilla_" + DateTime.Now.Ticks;
            byte[] preview;
            string extension;
            string mimeType;
            switch (htmlFormat)
            {
                case "docx":
                    extension = ".docx";
                    mimeType = "application/docx";
                    preview = _templateService.GetContentDocx(htmlContent, null, prevTemplateId, htmlHeader);
                    break;
                case "pdf":
                default:
                    extension = ".pdf";
                    mimeType = "application/pdf";
                    preview = _templateService.GetContentPDF(htmlContent, this.HttpContext.Request.ApplicationPath + "/content/tinyMCE-custom.css", null, prevTemplateId, htmlHeader);
                    break;
            }
            return File(preview, mimeType, fileName + extension);
        }

        #region Logic Methods

        private UserDTO Load()
        {
            var model = new UserDTO();
            model.Fields = _userProfileService.ListFields();
            model.Roles = _userProfileService.ListRoles();
            model.Levels = _userProfileService.ListInstitutionLevels();
            model.Provinces = _userProfileService.ListProvinces();
            model.Users = _userProfileService.ListNonDeleted();

            model.CurrentUser = SessionHelper.CurrentUserProfile.UserName;

            return model;
        }

        private TemplateDTO LoadTemplate(int? id)
        {
            var model = new TemplateDTO();
            model.TemplateTypes = _templateService.ListTemplateTypes();
            model.TemplateTypeFields = _templateService.ListTemplateTypeFields(model.TemplateTypeId);
            model.CanDeleteTemplate = _templateService.CanDeleteTemplate(model.TemplateTypeId);
            model.Fields = _templateService.ListFields();

            var siteUrl = ConfigurationManager.AppSettings.Get("SiteUrl");
            if (!siteUrl.EndsWith("/"))
                siteUrl = siteUrl + "/";

            model.FileManagerUrl = siteUrl + "FileManager";

            if (id.HasValue && id.Value > 0)
            {
                var template = _templateService.Get(id.Value);
                model.Id = template.Id;
                model.Name = template.Name;
                model.Active = template.Active;
                model.Content = template.Content;
                model.Header = template.Header;
                model.TemplateTypeId = template.TemplateTypeId;
                model.TemplateVariables = template.TemplateVariables.ToList();
                model.marginTop = template.marginTop;
                model.marginBottom = template.marginBottom;
                model.marginLeft = template.marginLeft;
                model.marginRight = template.marginRight;
                model.SignatureImagePath = template.SignatureImagePath;
                foreach (Field field in template.Fields.ToList())
                {
                    model.SelectedFieldsIds.Add(field.Id);
                }
            }

            return model;
        }

        /// <summary>
        /// Load a given role by id
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        private RoleDTO LoadRole(int roleId)
        {
            var role = new RoleDTO();
            role.RoleId = roleId;
            return LoadRole(role);
        }

        /// <summary>
        /// Load a given role by id
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        private RoleDTO LoadRole(RoleDTO role)
        {
            var _role = _userProfileService.GetRole(role.RoleId);
            if (_role.RoleId > 0)
            {
                role.RoleId = _role.RoleId;
                role.RoleName = _role.RoleName;

                if (!role.SelectedPermissions.Any())
                {
                    var existentPermissions = _role.webpages_RolePermissionStatuses.ToList();
                    foreach (var permission in existentPermissions)
                    {
                        role.SelectedPermissions.Add(new RolePermissionStatusesDTO() { PermissionId = permission.PermissionId, StatusId = permission.StatusId ?? 0 });
                    }
                }

                role.Permissions = _userProfileService.ListPermissions();
                role.StatusCriterions = _userProfileService.ListStatusCriterions();
            }
            return role;
        }

        #endregion
    }
}