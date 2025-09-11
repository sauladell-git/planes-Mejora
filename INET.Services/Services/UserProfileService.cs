using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using INET.Data;
using INET.Services.DTO;
using INET.Utils.Helpers;
using WebMatrix.WebData;
using INET.Core.Constants;
using INET.Core.Enums;
using System.Transactions;

namespace INET.Services
{
    /// <summary>
    /// Servicio de Usuarios
    /// </summary>
    public class UserProfileService : BusinessService
    {
        public UserProfileService(INETContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Determina si un nombre de usuario es válido
        /// </summary>
        /// <param name="userName">Nombre de usuario a validar</param>
        /// <returns>True en caso de que sea válido y no exista ningun usuario con ese nombre. False en caso contrario.</returns>
        public bool IsValidUserName(string userName)
        {
            return !string.IsNullOrEmpty(userName) && !Context.UserProfiles.Where(x => x.UserName == userName.Trim()).Any();
        }

        /// <summary>
        /// Determina si un nombre de usuario es válido
        /// </summary>
        /// <param name="userId">Id del usuario a validar</param>
        /// <param name="userName">Nombre de usuario a validar</param>
        /// <returns>True en caso de que sea válido y no exista ningun usuario (ademas de si mismo) con ese nombre. False en caso contrario.</returns>
        public bool IsValidUserName(int userId, string userName)
        {
            var users = Context.UserProfiles.Where(x => x.UserName == userName.Trim()).ToList();
            return IsValidUserName(userName) || users.Count() == 1 && users.First().UserId == userId;
        }

        /// <summary>
        /// Guarda un usuario en la base de datos
        /// </summary>
        /// <param name="dto">DTO del usuario que se desea salvar</param>
        public void SaveUser(UserDTO dto)
        {
            UserProfile user = null;
            if (dto.UserId == 0)
            {
                WebSecurity.CreateUserAndAccount(dto.UserName.Trim(), dto.Password.Trim(), new { LastName = dto.LastName.Trim(), Name = dto.Name.Trim(), IsEnabled = true });
                user = Context.UserProfiles.Where(x => x.UserName == dto.UserName).First();
            }
            else
            {
                user = Context.UserProfiles.Where(x => x.UserId == dto.UserId).First();
                user.UserName = dto.UserName.Trim();
                user.Name = dto.Name.Trim();
                user.LastName = dto.LastName.Trim();
                user.IsEnabled = dto.IsEnabled;
            }

            user.Fields.Clear();
            foreach (var fieldId in dto.SelectedFieldsIds)
                user.Fields.Add(Context.Fields.Where(x => x.Id == fieldId).First());

            if (dto.Role == RoleConstants.ADMIN || dto.Role == RoleConstants.ADMINISTRATIVO)
            {
                dto.SelectedProvincesIds.Clear();
            }

            user.InstitutionLevels.Clear();
            foreach (var levelId in dto.SelectedLevelsIds)
                user.InstitutionLevels.Add(Context.InstitutionLevels.Where(x => x.Id == levelId).First());

            if (dto.Role == RoleConstants.ADMIN || dto.Role == RoleConstants.ADMINISTRATIVO)
            {
                dto.SelectedProvincesIds.Clear();
            }

            user.Provinces.Clear();
            foreach (var provinceId in dto.SelectedProvincesIds)
                user.Provinces.Add(Context.Provinces.Where(x => x.Id == provinceId).First());

            Context.SaveChanges();

            // Roles del usuario
            var roles = Roles.GetRolesForUser(user.UserName);
            foreach (var roleName in roles)
                Roles.RemoveUserFromRole(user.UserName, roleName);

            Roles.AddUserToRole(user.UserName, dto.Role);
        }

        /// <summary>
        /// Determina si es posible eliminar un usuario
        /// </summary>
        /// <param name="userId">Id del usuario que se desea eliminar</param>
        /// <param name="currentUserName">Usuario actualmente logueado</param>
        /// <returns>True en caso de que sea posible eliminar el usuario. False en caso contrario.</returns>
        public bool CanDeleteUser(int userId, string currentUserName)
        {
            var userComments = Context.Comments.Where(x => x.UserId == userId).Any();
            var userIncidences = Context.Incidences.Where(x => x.UserId == userId).Any();
            var userImprovementPlans = Context.ImprovementPlans.Where(x => x.EvaluatorUserId == userId).Any();
            var userName = Context.UserProfiles.Where(x => x.UserId == userId).First().UserName;

            return !userComments && !userIncidences && !userImprovementPlans && userName != currentUserName;
        }

        /// <summary>
        /// Elimina un usuario de la base de datos
        /// </summary>
        /// <param name="userId">Id el usuario que se desea eliminar</param>
        /// <param name="soft">Si es un borrado lógico o real</param>
        public void DeleteUser(int userId, bool soft)
        {
            if (soft == true)
            {
                var user = Context.UserProfiles.Where(x => x.UserId == userId).First();
                user.IsDeleted = true;
                user.IsEnabled = false;
                Context.SaveChanges();
            }
            else
            {
                DeleteUser(userId);
            }

        }

        /// <summary>
        /// Elimina un usuario de la base de datos
        /// </summary>
        /// <param name="userId">Id el usuario que se desea eliminar</param>
        public void DeleteUser(int userId)
        {
            var user = Context.UserProfiles.Where(x => x.UserId == userId).First();

            user.Fields.Clear();
            user.webpages_Roles.Clear();
            user.Provinces.Clear();

            Context.UserProfiles.Remove(user);
            Context.SaveChanges();
        }

        /// <summary>
        /// Deshabilita un usuario
        /// </summary>
        /// <param name="userId">Id del usuario a deshabilitar</param>
        public void DisableUser(int userId)
        {
            var user = Context.UserProfiles.Where(x => x.UserId == userId).First();
            user.IsEnabled = false;

            Context.SaveChanges();
        }

        /// <summary>
        /// Determina si un usuario esta activo
        /// </summary>
        /// <param name="userName">Nombre de Usuario</param>
        /// <returns>True en caso de que el usuario este activo. False en caso contrario</returns>
        public bool IsActiveUser(string userName)
        {
            var user = Context.UserProfiles.Where(x => x.UserName == userName.Trim()).FirstOrDefault();
            if (user != null)
                return user.IsEnabled && !user.IsDeleted;

            return false;
        }

        /// <summary>
        /// Lista todos los usuarios del sistema
        /// </summary>
        /// <returns></returns>
        public List<UserProfile> List()
        {
            return Context.UserProfiles.OrderBy(x => x.UserId).ThenBy(x => x.UserName).ToList();
        }

        /// <summary>
        /// Lista todos los usuarios del sistema no borrados
        /// </summary>
        /// <returns></returns>
        public List<UserProfile> ListNonDeleted()
        {
            return Context.UserProfiles.Where(x => x.IsDeleted == false).OrderBy(x => x.UserId).ThenBy(x => x.UserName).ToList();
        }

        /// <summary>
        /// Lista los posibles roles a asignar un usuario
        /// </summary>
        /// <returns>Una lista con los roles a asignar a un usuario</returns>
        public List<webpages_Roles> ListRoles()
        {
            return Context.webpages_Roles.OrderBy(x => x.RoleName).ToList();
        }

        public List<webpages_Permissions> ListPermissions()
        {
            return Context.webpages_Permissions.OrderBy(x => x.PermissionName).ToList();
        }

        public List<Status> ListStatuses()
        {
            return Context.Status.OrderBy(x => x.Description).ToList();
        }

        public List<StatusCriterion> ListStatusCriterions()
        {
            return Context.StatusCriterions.OrderBy(x => x.Description).ToList();
        }

        /// <summary>
        /// Get one role by Id
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public webpages_Roles GetRole(int roleId)
        {
            return Context.webpages_Roles.Where(x => x.RoleId == roleId).FirstOrDefault();
        }

        /// <summary>
        /// Validates Role DTO for permissions
        /// </summary>
        /// <param name="roleDTO"></param>
        /// <returns></returns>
        public bool ValidateRolePermissionsDTO(RoleDTO roleDTO)
        {
            var _return = true;

            var permissions = ListPermissions();
            var statuses = ListStatuses();

            foreach (var perm in roleDTO.SelectedPermissions)
            {
                var permission = permissions.Where(n => n.PermissionId == perm.PermissionId).FirstOrDefault();
                if (permission.PermissionId == 0 || (perm.StatusId.HasValue && !statuses.Where(n => n.Id == perm.StatusId && permission.StatusCriterions.Where(x => x.Id == n.StatusCriterionId).Any()).Any()))
                {
                    _return = false;
                    break;
                }
            }

            return _return;
        }

        /// <summary>
        /// Saves Role permissions
        /// </summary>
        /// <param name="roleDTO"></param>
        /// <returns></returns>
        public bool SaveRolePermissions(RoleDTO roleDTO)
        {
            var _return = false;
            using (var tran = new TransactionScope())
            {
                Context.webpages_RolePermissionStatuses.RemoveRange(Context.webpages_RolePermissionStatuses.Where(x => x.RoleId == roleDTO.RoleId));

                var new_ones = new List<webpages_RolePermissionStatuses>();

                foreach (var perm in roleDTO.SelectedPermissions)
                {
                    new_ones.Add(new webpages_RolePermissionStatuses() { RoleId = roleDTO.RoleId, PermissionId = perm.PermissionId, StatusId = perm.StatusId });
                }

                Context.webpages_RolePermissionStatuses.AddRange(new_ones);

                Context.SaveChanges();
                tran.Complete();

                return true;
            }
            return _return;
        }

        /// <summary>
        /// Exporta los usuarios a un archivo de excel
        /// </summary>
        /// <returns>El stream en memoria del archivo de excel generado</returns>
        public Stream ExportUsersToExcel()
        {
            var users = ListNonDeleted().Select(x => new ListUsersDTO
            {
                Id = x.UserId,
                UserName = x.UserName,
                Role = x.webpages_Roles.First().RoleName,
                FirstName = x.Name,
                LastName = x.LastName,
                Enabled = x.EnabledToString,
                FullName = x.FullName,
                CreationDate = DateTime.Now
            });

            var type = ExportFormatType.Excel;
            var reportUrl = Path.Combine(ConfigurationManager.AppSettings["ReportsPath"], "ListUsers.rpt");

            var rpt = new ReportClass { FileName = reportUrl };
            rpt.Load();
            rpt.SetDataSource(users);

            return rpt.ExportToStream(type);
        }

        /// <summary>
        /// Lista los evaluadores posibles de un plan de mejora
        /// </summary>
        /// <param name="provNumbers">Number de provincias para filtrar los usuarios a mostrar</param>
        /// <returns>Una lista con los evaluadores posibles del plan</returns>
        public IList<UserProfile> ListEvaluatorsUsers(List<String> provNumbers)
        {
            return ListEvaluatorsUsers(provNumbers, null, null);
        }

        /// <summary>
        /// Lista los evaluadores posibles de un plan de mejora
        /// </summary>
        /// <param name="provNumbers">Number de provincias para filtrar los usuarios a mostrar</param>
        /// <returns>Una lista con los evaluadores posibles del plan</returns>
        public IList<UserProfile> ListEvaluatorsUsers(List<String> provNumbers, int? fieldId, int? levelId)
        {
            var users = Context.UserProfiles.Where(y => y.IsEnabled == true && y.IsDeleted == false && y.webpages_Roles.Where(r => r.RoleName == RoleConstants.EVALUADOR).Any());
            if (provNumbers.Count > 0)
            {
                users = users.Where(x => x.Provinces.Count == 0 || x.Provinces.Where(y => provNumbers.Contains(y.Number)).Any());
            }
            if (fieldId.HasValue)
            {
                users = users.Where(x => x.Fields.Count == 0 || x.Fields.Where(y => y.Id == fieldId).Any());
            }
            if (levelId.HasValue)
            {
                users = users.Where(x => x.InstitutionLevels.Count == 0 || x.InstitutionLevels.Where(y => y.Id == levelId).Any());
            }
            return users.ToList();
        }

        /// <summary>
        /// Envía un mail de confirmación al usuario con un token para resetear la contraseña
        /// </summary>
        /// <param name="userName">Nombre de usuario (que tambien es el mail del usuario)</param>        
        /// <returns>True en caso de que se haya validado el usuario y se haya enviado el email. False en caso contrario.</returns>
        public bool SendForgotPasswordEmail(string userName)
        {
            if (Context.UserProfiles.Where(x => x.UserName == userName.Trim()).Where(x => x.IsEnabled == true && x.IsDeleted == false).Count() == 0)
                return false;

            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var token = WebSecurity.GeneratePasswordResetToken(userName);
            var resetLink = "<a href='" + urlHelper.Action("ResetPassword", "Account", new { userName = userName, token = token }, "http") + "'>" + urlHelper.Action("ResetPassword", "Account", new { userName = userName, token = token }, "http") + "</a>";
            string subject = "Cambiar Contraseña";
            string body = "Por favor, haga click en el siguiente link para poder cambiar su contraseña: <br/><br/>" + resetLink;

            MailHelper.Send(userName, subject, body);
            return true;
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="userName">Nombre de usuario del cual se desea cambiar la contraseña</param>
        /// <param name="oldPassword">Contraseña anterior</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <returns></returns>
        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            // ChangePassword a veces arroja una excepción en vez de devolver falso en algunos escenarios.
            bool changePasswordSucceeded = false;
            var user_count = Context.UserProfiles.Where(x => x.IsEnabled && x.IsDeleted == false).Count();
            if (user_count > 0)
            {
                try
                {
                    changePasswordSucceeded = WebSecurity.ChangePassword(userName, oldPassword, newPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }
            }

            return changePasswordSucceeded;
        }

        /// <summary>
        /// Resetea la contraseña de un usuario
        /// </summary>
        /// <param name="model">ResetPasswordModel con la info para resetear la contraseña</param>
        /// <returns>True en caso de que se haya modificado la contraseña. False en caso contrario.</returns>
        public bool ResetPassword(ResetPasswordModel model)
        {
            var user = Context.UserProfiles.Where(x => x.UserName == model.UserName).Where(x => x.IsEnabled == true && x.IsDeleted == false).FirstOrDefault();
            if (user != null)
            {
                var isValidToken = Context.webpages_Membership.Where(x => x.UserId == user.UserId
                                                                        && x.PasswordVerificationTokenExpirationDate > DateTime.Now
                                                                        && x.PasswordVerificationToken == model.Token).Any();

                if (isValidToken)
                {
                    WebSecurity.ResetPassword(model.Token, model.NewPassword);
                    WebSecurity.Login(model.UserName, model.NewPassword);
                    MailHelper.Send(model.UserName, "Confirmación de cambio de contraseña", "¡Felicitaciones! Usted ha modificado su contraseña con éxito.");
                    return true;
                }
            }

            return false;
        }

    }
}