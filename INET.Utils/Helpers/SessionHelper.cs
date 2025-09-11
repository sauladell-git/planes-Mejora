using INET.Core.Constants;
using INET.Core.Enums;
using INET.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace INET.Utils.Helpers
{
    public static class SessionHelper
    {
        /// <summary>
        /// Si el usuario se encuentra autenticado
        /// </summary>
        public static bool IsAuthenticated
        {
            get
            {
                return HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }

        /// <summary>
        /// Usuario actualmente logueado
        /// </summary>
        public static UserProfile CurrentUserProfile
        {
            get
            {
                if (SessionHelper.IsAuthenticated)
                {
                    var context = new INETContext();
                    return context.UserProfiles.Where(x => x.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault();
                }
                return null;
            }
        }

        /// <summary>
        /// Id del usuario actualmente logueado
        /// </summary>
        public static int CurrentUserId
        {
            get
            {
                if (SessionHelper.CurrentUserProfile != null)
                    return CurrentUserProfile.UserId;

                return 0;
            }
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        public static List<string> CurrentUserRoles
        {
            get
            {
                if (SessionHelper.IsAuthenticated)
                {
                    return Roles.GetRolesForUser().ToList();
                }
                else
                {
                    return new List<string>();
                }

            }
        }

        /// <summary>
        /// Get user permissions
        /// </summary>
        public static List<webpages_RolePermissionStatuses> CurrentUserPermissions
        {
            get
            {
                if (SessionHelper.IsAuthenticated)
                {
                    var context = new INETContext();
                    var roles = CurrentUserRoles;
                    return context.UserProfiles.Where(x => x.UserId == CurrentUserId).SelectMany(x => x.webpages_Roles).SelectMany(x => x.webpages_RolePermissionStatuses).ToList();
                }
                else
                {
                    return new List<webpages_RolePermissionStatuses>();
                }

            }
        }

        public static bool HasPermission(string permission)
        {
            var permissions = CurrentUserPermissions;
            return permissions.Where(x => x.webpages_Permissions.PermissionCode == permission).Any();
        }

        /// <summary>
        /// Chek user credencials for a given permission
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="status_id"></param>
        /// <returns></returns>
        public static bool HasPermission(string permission, int status_id)
        {
            var permissions = CurrentUserPermissions;
            return permissions.Where(x => x.webpages_Permissions.PermissionCode == permission && x.StatusId == status_id).Any();
        }

        /// <summary>
        /// Chek user credencials for at list one of the given permission
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static bool HasAnyPermission(string[] _permissions)
        {
            var permissions = CurrentUserPermissions;
            return permissions.Where(x => _permissions.Contains(x.webpages_Permissions.PermissionCode)).Any();
        }

        /// <summary>
        /// Chek user credencials for a given role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool HasRole(string role)
        {
            var roles = CurrentUserRoles;
            return roles.Contains(role);
        }

        /// <summary>
        /// Chek user credencials for at list one of the given roles
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static bool HasAnyRole(string[] roles)
        {
            var _roles = CurrentUserRoles;
            return _roles.Intersect(roles).Any();
        }

        /// <summary>
        /// Obtiene los ejes que el usuario puede ver
        /// </summary>
        /// <returns></returns>
        public static List<Field> GetFields()
        {
            List<Field> list = new List<Field>();
            if (SessionHelper.IsAuthenticated)
            {
                if (SessionHelper.HasAnyRole(new string[] { RoleConstants.ADMIN, RoleConstants.ADMINISTRATIVO }))
                {
                    var context = new INETContext();
                    list = context.Fields.ToList();
                }
                else
                {
                    list = CurrentUserProfile.Fields.ToList();
                }
            }
            return list;
        }

        /// <summary>
        /// Obtiene los niveles que el usuario puede ver
        /// </summary>
        /// <returns></returns>
        public static List<InstitutionLevel> GetInstitutionLevels()
        {
            List<InstitutionLevel> list = new List<InstitutionLevel>();
            if (SessionHelper.IsAuthenticated)
            {
                list = CurrentUserProfile.InstitutionLevels.ToList();
            }
            return list;
        }

        /// <summary>
        /// Determina si el usuario actual tiene un determinado eje habilitado
        /// </summary>
        /// <param name="fieldId">Id del eje que se desea chequear</param>
        /// <returns>True en caso de que posea el eje. False en caso contrario</returns>
        public static bool HasField(int fieldId)
        {
            bool has = false;
            has = SessionHelper.GetFields().Where(x => x.Id == fieldId).Any();
            return has;
        }

        /// <summary>
        /// Determina si el usuario actual tiene un determinado nivel institucional habilitado
        /// </summary>
        /// <param name="levelId">Id del nivel que se desea chequear</param>
        /// <returns>True en caso de que posea el nivel. False en caso contrario</returns>
        public static bool HasInstitutionLevel(int levelId)
        {
            bool has = false;
            has = SessionHelper.GetInstitutionLevels().Where(x => x.Code == levelId).Any();
            return has;
        }

        /// <summary>
        /// Determina si el usuario actual tiene una determinada provincia relacionada
        /// </summary>
        /// <param name="provId">Id de la provincia que se desea chequear</param>
        /// <returns>True o False en caso contrario</returns>
        public static bool HasProvince(int provId)
        {
            bool has = false;
            has = SessionHelper.CurrentUserProfile.Provinces.Where(x => x.Id == provId).Any();
            return has;
        }

        /// <summary>
        /// Determina si el usuario actual tiene una determinada provincia relacionada
        /// </summary>
        /// <param name="provNumber">Numero de la provincia que se desea chequear</param>
        /// <returns>True o False en caso contrario</returns>
        public static bool HasProvince(string provNumber)
        {
            bool has = false;
            has = SessionHelper.CurrentUserProfile.Provinces.Where(x => x.Number == provNumber).Any();
            return has;
        }

        /// <summary>
        /// Lista los estados de proceso plan que puede ver el usuario
        /// </summary>
        /// <returns>Una lista con los estados del plan</returns>
        public static IList<Status> GetVisiblePlanStageStatuses()
        {
            var permissions = CurrentUserPermissions;
            return permissions.Where(x => x.StatusId != null && x.Status.StatusCriterionId == (int)StatusCriterionEnum.Stage).GroupBy(x => x.StatusId).Select(x => x.First().Status).ToList();
        }

        /// <summary>
        /// Idicates where a given status should be ignored for a permission on permission construccion
        /// Excluded explitly to avoid breaking business logic and process configuration
        /// </summary>
        /// <param name="permissionCode"></param>
        /// <param name="statusId"></param>
        /// <returns></returns>
        public static bool IgnorePermissionStatus(string permissionCode, int statusId)
        {
            int[] ignored_statuses = new int[] { };

            switch (permissionCode)
            {
                case PermissionConstants.ImprovementPlan.BLOCK:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                    };
                    break;
                case PermissionConstants.ImprovementPlan.DELETE:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.EnAdministracion
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                    };
                    break;
                case PermissionConstants.ImprovementPlan.EDIT:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.EnEvaluacion
                        , (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.EnAdministracion
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                    };
                    break;
                case PermissionConstants.Solicitudes.BLOCK:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.EnAdministracion
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                    };
                    break;
                case PermissionConstants.Solicitudes.EDIT:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.EnAdministracion
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                    };
                    break;
                case PermissionConstants.Solicitudes.CREATE:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.EnAdministracion
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                    };
                    break;
                case PermissionConstants.Solicitudes.DELETE:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.EnAdministracion
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                    };
                    break;
                case PermissionConstants.Dictums.CREATE:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnCargaProvincia
                        , (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                        , (int) StageStatusEnum.EnIngreso
                    };
                    break;
                case PermissionConstants.Dictums.EDIT:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnCargaProvincia
                        , (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                        , (int) StageStatusEnum.EnIngreso
                    };
                    break;
                case PermissionConstants.Dictums.BLOCK:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnCargaProvincia
                        , (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                        , (int) StageStatusEnum.EnIngreso
                    };
                    break;
                case PermissionConstants.Dictums.DELETE:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnCargaProvincia
                        , (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                        , (int) StageStatusEnum.EnIngreso
                    };
                    break;
                case PermissionConstants.Resolutions.CREATE:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnCargaProvincia
                        , (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                        , (int) StageStatusEnum.EnIngreso
                    };
                    break;
                case PermissionConstants.Resolutions.EDIT:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnCargaProvincia
                        , (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                        , (int) StageStatusEnum.EnIngreso
                    };
                    break;
                case PermissionConstants.Resolutions.BLOCK:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnCargaProvincia
                        , (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                        , (int) StageStatusEnum.EnIngreso
                    };
                    break;
                case PermissionConstants.Resolutions.DELETE:
                    ignored_statuses = new int[] {
                        (int) StageStatusEnum.Cerrado
                        , (int) StageStatusEnum.Anulado
                        , (int) StageStatusEnum.AElevarProvincia
                        , (int) StageStatusEnum.EnCargaProvincia
                        , (int) StageStatusEnum.EnCaratulizacion
                        , (int) StageStatusEnum.EnComisionRecepcionProvincia
                        , (int) StageStatusEnum.EnIngreso
                    };
                    break;
            }

            return ignored_statuses.Contains(statusId);
        }
    }
}
