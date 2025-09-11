using INET.Core.Constants;
using INET.Core.Enums;
using INET.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace INET.Utils.Helpers
{
    /// <summary>
    /// Helper encargado de gestionar y centralizar todas las tareas de roles y permisos
    /// </summary>
    public static class RoleHelperOld
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
        /// Id del usuario actualmente logueado
        /// </summary>
        public static int CurrentUserId
        {
            get
            {
                if (RoleHelperOld.CurrentUserProfile != null)
                    return CurrentUserProfile.UserId;

                return 0;
            }
        }

        /// <summary>
        /// Usuario actualmente logueado
        /// </summary>
        public static UserProfile CurrentUserProfile
        {
            get
            {
                if (RoleHelperOld.IsAuthenticated)
                {
                    var context = new INETContext();
                    return context.UserProfiles.Where(x => x.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault();
                }
                return null;
            }
        }

        /// <summary>
        /// Obtiene los ejes que el usuario puede ver
        /// </summary>
        /// <returns></returns>
        public static List<Field> GetUserFields()
        {
            List<Field> list = new List<Field>();
            if (RoleHelperOld.IsAuthenticated)
            {
                if (RoleHelperOld.CurrentUserHasRole(new string[] { RoleConstants.ADMIN, RoleConstants.ADMINISTRATIVO }))
                {
                    var context = new INETContext();
                    list = context.Fields.Where(x => x.StatusId != (int)AxisStatusEnum.Archivado).ToList();
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
        public static List<InstitutionLevel> GetUserInstitutionLevels()
        {
            List<InstitutionLevel> list = new List<InstitutionLevel>();
            if (RoleHelperOld.IsAuthenticated)
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
        public static bool CurrentUserHasField(int fieldId)
        {
            bool has = false;
            has = RoleHelperOld.GetUserFields().Where(x => x.Id == fieldId).Any();
            return has;
        }

        /// <summary>
        /// Determina si el usuario actual tiene un determinado nivel institucional habilitado
        /// </summary>
        /// <param name="levelId">Id del nivel que se desea chequear</param>
        /// <returns>True en caso de que posea el nivel. False en caso contrario</returns>
        public static bool CurrentUserHasInstitutionLevel(int levelId)
        {
            bool has = false;
            has = RoleHelperOld.GetUserInstitutionLevels().Where(x => x.Code == levelId).Any();
            return has;
        }

        /// <summary>
        /// Determina si el usuario actual tiene una determinada provincia relacionada
        /// </summary>
        /// <param name="provId">Id de la provincia que se desea chequear</param>
        /// <returns>True o False en caso contrario</returns>
        public static bool CurrentUserHasProvince(int provId)
        {
            bool has = false;
            has = RoleHelperOld.CurrentUserProfile.Provinces.Where(x => x.Id == provId).Any();
            return has;
        }

        /// <summary>
        /// Determina si el usuario actual tiene una determinada provincia relacionada
        /// </summary>
        /// <param name="provNumber">Numero de la provincia que se desea chequear</param>
        /// <returns>True o False en caso contrario</returns>
        public static bool CurrentUserHasProvince(string provNumber)
        {
            bool has = false;
            has = RoleHelperOld.CurrentUserProfile.Provinces.Where(x => x.Number == provNumber).Any();
            return has;
        }

        /// <summary>
        /// Check if the user has any of the given roles in a list
        /// </summary>
        /// <param name="roleList"></param>
        /// <returns>True o False en caso contrario</returns>
        public static bool CurrentUserHasRole(string[] roleList)
        {
            return RoleHelperOld.CurrentUserProfile.webpages_Roles.Any(x => roleList.Contains(x.RoleName));
        }

        /// <summary>
        /// Check if the user has a given role
        /// </summary>
        /// <param name="role"></param>
        /// <returns>True o False en caso contrario</returns>
        public static bool CurrentUserHasRole(string role)
        {
            return RoleHelperOld.CurrentUserProfile.webpages_Roles.Any(x => x.RoleName == role);
        }

        /// <summary>
        /// Ge Main user CurrentUserRole
        /// </summary>
        public static string CurrentUserRole
        {
            get
            {
                return Roles.GetRolesForUser().FirstOrDefault();
            }
        }

        /// <summary>
        /// Permisos para los reportes existentes
        /// </summary>
        public static class ReportPermissions
        {
            /// <summary>
            /// Si puede generar alguno de los reportes existentes
            /// </summary>
            /// <returns></returns>
            public static bool CanRead()
            {
                return CanGenerateBudgetByProvinceReport() || CanGenerateImprovementPlanReport() || CanGenerateStatementOfAccountReport();
            }

            /// <summary>
            /// Si puede generar reporte de Ejecución presupuestaria de una provincia
            /// </summary>
            /// <returns></returns>
            public static bool CanGenerateBudgetByProvinceReport()
            {
                return true;
            }

            /// <summary>
            /// Si puede generar reporte de planes
            /// </summary>
            /// <returns></returns>
            public static bool CanGenerateImprovementPlanReport()
            {
                return true;
            }

            /// <summary>
            /// Si puede generar reporte de solicitados de un CUE
            /// </summary>
            /// <returns></returns>
            public static bool CanGenerateCUESolicitudesReport()
            {
                return true;
            }

            /// <summary>
            /// Si puede generar estado de cuenta de las provincias
            /// </summary>
            /// <returns></returns>
            public static bool CanGenerateStatementOfAccountReport()
            {
                return RoleHelperOld.CurrentUserHasRole(new string[] { RoleConstants.ADMIN, RoleConstants.ADMINISTRATIVO, RoleConstants.AUDITOR, RoleConstants.COORDINADOR_EJE, RoleConstants.DELEGADO_PROVINCIAL_INET, RoleConstants.INGRESO_MONITOREO });
            }
        }

        /// <summary>
        /// Establece los permisos relacionados al plan de mejora
        /// </summary>
        public static class ImprovementPlanPermissions
        {

            /// <summary>
            /// Determina si se puede visualizar un plan de mejora
            /// </summary>
            /// <param name="PlanId">ID del plan</param>
            /// <returns>True en caso de que se pueda visualizar. False en caso contrario.</returns>
            public static bool CanRead(int PlanId)
            {
                var result = false;
                using (var context = new INETContext())
                {
                    var plan = context.ImprovementPlans.Where(x => x.Id == PlanId).FirstOrDefault();
                    if (plan.Id > 0)
                    {
                        result = CanRead(plan);
                    }
                }
                return result;
            }

            /// <summary>
            /// Determina si se puede visualizar un plan de mejora
            /// </summary>
            /// <param name="plan">Plan de mejora</param>
            /// <returns>True en caso de que se pueda visualizar. False en caso contrario.</returns>
            public static bool CanRead(ImprovementPlan plan)
            {
                var result = false;
                if (RoleHelperOld.CurrentUserRole == RoleConstants.ADMIN || RoleHelperOld.CurrentUserRole == RoleConstants.ADMINISTRATIVO)
                {
                    result = true;
                }
                else
                {
                    result = (RoleHelperOld.CurrentUserProfile.Provinces.Count == 0 || RoleHelperOld.CurrentUserHasProvince(plan.CUE.Substring(0, 2))) && (RoleHelperOld.GetUserFields().Count == 0 || RoleHelperOld.CurrentUserHasField(plan.FieldId))
                        && (plan.InstitutionLevelInt.HasValue == false || (RoleHelperOld.CurrentUserProfile.InstitutionLevels.Count == 0 || RoleHelperOld.CurrentUserHasInstitutionLevel(plan.InstitutionLevelInt.Value)));
                }
                return result;
            }

            /// <summary>
            /// Determina si el usuario puede crear un plan de mejora
            /// </summary>
            /// <param name="user_roles">Roles del usuario a validar</param>
            /// <returns>True en caso de que pueda crear un plan de mejora. False en caso contrario.</returns>
            public static bool CanCreate()
            {
                string[] test = { RoleConstants.ADMIN, RoleConstants.INGRESO_MONITOREO, RoleConstants.OPERADOR_PROVINCIAL, RoleConstants.DELEGADO_PROVINCIAL_INET };
                return test.Contains(Roles.GetRolesForUser().First());
            }

            /// <summary>
            /// Determina si se puede actualizar un plan de mejora
            /// </summary>
            /// <param name="improvementPlanStatusId">Estado actual del plan del mejora</param>
            /// <returns>True en caso de que sea posible modificar el plan. False en caso contrario.</returns>
            public static bool CanUpdate(int improvementPlanStatusId)
            {
                if (RoleHelperOld.CurrentUserRole == RoleConstants.ADMIN)
                {
                    return improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt() || improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt();
                }
                else if (RoleHelperOld.CurrentUserRole == RoleConstants.INGRESO_MONITOREO)
                {
                    return improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt();
                }
                else if (RoleHelperOld.CurrentUserRole == RoleConstants.OPERADOR_PROVINCIAL)
                {
                    return improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt();
                }
                else if (RoleHelperOld.CurrentUserRole == RoleConstants.DELEGADO_PROVINCIAL_INET)
                {
                    return improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt();
                }
                return false;
            }

            /// <summary>
            /// Determina si se puede borrar un plan de mejora. 
            /// </summary>
            /// <param name="improvementPlanStatusId">Estado actual del plan de mejora</param>
            /// <returns>True en caso de que sea posible eliminarlo. False en caso contrario.</returns>
            public static bool CanDelete(int improvementPlanStatusId)
            {
                if (RoleHelperOld.CurrentUserRole == RoleConstants.ADMIN)
                {
                    return improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt() || improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt();
                }
                else if (RoleHelperOld.CurrentUserRole == RoleConstants.INGRESO_MONITOREO)
                {
                    return improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt();
                }
                else if (RoleHelperOld.CurrentUserRole == RoleConstants.OPERADOR_PROVINCIAL)
                {
                    return improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt();
                }
                else if (RoleHelperOld.CurrentUserRole == RoleConstants.DELEGADO_PROVINCIAL_INET)
                {
                    return improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt();
                }

                return false;
            }

            /// <summary>
            /// Determina si se puede cambiar el estado de un plan de mejora
            /// </summary>
            /// <param name="planId"></param>
            /// <returns></returns>
            public static bool CanChangeStatus(int planId)
            {
                var result = false;
                using (var context = new INETContext())
                {
                    var plan = context.ImprovementPlans.Where(x => x.Id == planId).FirstOrDefault();
                    if (plan.Id > 0)
                    {
                        result = CanChangeStatus(plan);
                    }
                }
                return result;
            }

            /// <summary>
            /// Determina si se puede cambiar el estado de un plan de mejora
            /// </summary>
            /// <param name="plan">Plan de mejora</param>
            /// <returns>True en caso de que se pueda cambiar el estado del plan. False en caso contrario.</returns>
            public static bool CanChangeStatus(ImprovementPlan plan)
            {
                if (!RoleHelperOld.ImprovementPlanPermissions.CanRead(plan.Id))
                    return false;

                switch (RoleHelperOld.CurrentUserRole)
                {
                    case RoleConstants.ADMIN:
                        return true;

                    case RoleConstants.INGRESO_MONITOREO:
                        return plan.StatusId == StageStatusEnum.EnIngreso.ToInt()
                            || plan.StatusId == StageStatusEnum.EnCaratulizacion.ToInt()
                            || plan.StatusId == StageStatusEnum.EnComisionRecepcionProvincia.ToInt();

                    case RoleConstants.COORDINADOR_EJE:
                        return plan.StatusId == StageStatusEnum.EnEvaluacion.ToInt();

                    case RoleConstants.ADMINISTRATIVO:
                        return plan.StatusId == StageStatusEnum.EnAdministracion.ToInt();

                    case RoleConstants.OPERADOR_PROVINCIAL:
                        return plan.StatusId == StageStatusEnum.EnCargaProvincia.ToInt();

                    case RoleConstants.REFERENTE_JURISDICCIONAL:
                        return plan.StatusId == StageStatusEnum.AElevarProvincia.ToInt();

                    case RoleConstants.DELEGADO_PROVINCIAL_INET:
                        var statuses = new int[] {
                            StageStatusEnum.EnComisionRecepcionProvincia.ToInt(),
                            StageStatusEnum.EnCargaProvincia.ToInt()
                        };
                        return statuses.Any(x => x == plan.StatusId);

                    default:
                        return false;
                }
            }

            /// <summary>
            /// Determina si se puede setear el evaluador de un plan de mejora
            /// </summary>
            /// <param name="improvementPlanStatusId">Estado del plan de mejora</param>
            /// <param name="fieldId">Id del eje</param>
            /// <param name="userFields">Ejes asignados al usuario actual</param>
            /// <param name="levelInt"></param>
            /// <param name="userLevels"></param>
            /// <returns>True en caso de que el usuario pueda asignar un evaluador. False en caso contrario.</returns>
            public static bool CanSetEvaluator(int improvementPlanStatusId, int fieldId, List<Field> userFields, int? levelInt, List<InstitutionLevel> userLevels)
            {
                if (improvementPlanStatusId == StageStatusEnum.Anulado.ToInt() || improvementPlanStatusId == (int)StageStatusEnum.Cerrado.ToInt()) return false;
                switch (RoleHelperOld.CurrentUserRole)
                {
                    case RoleConstants.ADMIN:
                        return true;
                    case RoleConstants.COORDINADOR_EJE:
                        return improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt() && userFields.Where(x => x.Id == fieldId).Any() &&
                            (levelInt.HasValue == false || userLevels.Where(x => x.Code == levelInt.Value).Any());
                    case RoleConstants.DELEGADO_PROVINCIAL_INET:
                        return false;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// Determina si se puede anular un plan de mejora
            /// </summary>
            /// <param name="improvementPlanStatusId">Estado actual del plan de mejora</param>
            /// <param name="fieldId">Eje asignado al plan</param>
            /// <param name="evaluatorUser">Evaluador asignado al plan</param>
            /// <param name="userFields">Eje asignados al usuario actual</param>
            /// <returns>True en caso de que el usuario pueda anular el plan. False en caso contrario.</returns>
            public static bool CanBlock(int improvementPlanStatusId, int fieldId, UserProfile evaluatorUser, IList<Field> userFields, int? levelInt, List<InstitutionLevel> userLevels)
            {
                var role = Roles.GetRolesForUser().FirstOrDefault();
                switch (role)
                {
                    case RoleConstants.ADMIN:
                        return improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt()
                            || improvementPlanStatusId == StageStatusEnum.EnCaratulizacion.ToInt()
                            || improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt()
                            || improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt();
                    case RoleConstants.ADMINISTRATIVO:
                        return improvementPlanStatusId == StageStatusEnum.EnAdministracion.ToInt();

                    default:
                        return false;
                }
            }

        }

        /// <summary>
        /// Establece los permisos relacionados a los solicitados
        /// </summary>
        public static class SolicitudePermissions
        {
            /// <summary>
            /// Determina si se puede visualizar un solicitado
            /// </summary>
            /// <param name="solicitudeStatusId">Id del solicitado</param>
            /// <returns>True en caso de que se pueda visualizar el solicitado</returns>
            public static bool CanRead(int solicitudeStatusId)
            {
                if (Roles.IsUserInRole(RoleConstants.ADMIN) || Roles.IsUserInRole(RoleConstants.ADMINISTRATIVO) || Roles.IsUserInRole(RoleConstants.AUDITOR) || Roles.IsUserInRole(RoleConstants.INGRESO_MONITOREO)
                    || Roles.IsUserInRole(RoleConstants.EVALUADOR) || Roles.IsUserInRole(RoleConstants.COORDINADOR_EJE))
                    return true;

                return false;
            }

            /// <summary>
            /// Determina si se puede crear un solicitado
            /// </summary>
            /// <param name="improvementPlanStatusId">Id del estado actual del plan de mejora</param>
            /// <param name="improvementFieldId">Id del eje del plan</param>
            /// <param name="evaluatorUser">Evaluador del plan de mejora</param>            
            /// <returns>True en caso de que sea posible crear un solicitado. False en caso contrario.</returns>
            public static bool CanCreate(int improvementPlanStatusId, int improvementFieldId, UserProfile evaluatorUser, int? institutionLevelId)
            {
                var role = Roles.GetRolesForUser().First();
                int[] statuses;
                switch (role)
                {
                    case RoleConstants.ADMIN:
                        statuses = new int[] { StageStatusEnum.EnIngreso.ToInt(), StageStatusEnum.EnEvaluacion.ToInt(), StageStatusEnum.EnCargaProvincia.ToInt() };
                        return statuses.Any(x => x == improvementPlanStatusId);

                    case RoleConstants.INGRESO_MONITOREO:
                        return improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt();

                    case RoleConstants.EVALUADOR:
                        return improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt()
                                && evaluatorUser != null && evaluatorUser.UserName == RoleHelperOld.CurrentUserProfile.UserName;

                    case RoleConstants.COORDINADOR_EJE:
                        return improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt() && CurrentUserHasField(improvementFieldId) && (!institutionLevelId.HasValue || CurrentUserHasInstitutionLevel(institutionLevelId.Value));

                    case RoleConstants.OPERADOR_PROVINCIAL:
                        return improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt() && CurrentUserHasField(improvementFieldId) && (!institutionLevelId.HasValue || CurrentUserHasInstitutionLevel(institutionLevelId.Value));

                    case RoleConstants.DELEGADO_PROVINCIAL_INET:
                        statuses = new int[] { StageStatusEnum.EnCargaProvincia.ToInt(), StageStatusEnum.EnEvaluacion.ToInt() };
                        return statuses.Any(x => x == improvementPlanStatusId) && CurrentUserHasField(improvementFieldId) && (!institutionLevelId.HasValue || RoleHelperOld.CurrentUserHasInstitutionLevel(institutionLevelId.Value));

                    default:
                        return false;
                }
            }

            /// <summary>
            /// Determina si se puede actualizar un solicitado, agregando validación de uso del solicitado
            /// </summary>
            /// <param name="solicitudeStatusId">Id del estado del solicitado</param>
            /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
            /// <param name="improvementPlanFieldId">Id del eje del plan</param>
            /// <param name="evaluatorUser">Evaluador del plan</param>
            /// <param name="solicitude">El Solicitado</param>
            /// <returns>True en caso de que se pueda editar el solicitado. False en caso contrario.</returns>
            public static bool CanUpdate(int solicitudeStatusId, int improvementPlanStatusId, int improvementPlanFieldId, UserProfile evaluatorUser, Solicitude solicitude, int? institutionLevelId)
            {
                var validationStatus = true;
                var role = Roles.GetRolesForUser().First();
                switch (role)
                {
                    case RoleConstants.ADMIN:
                        break;
                    case RoleConstants.OPERADOR_PROVINCIAL:
                        break;
                    case RoleConstants.INGRESO_MONITOREO:
                        break;
                    case RoleConstants.EVALUADOR:
                        break;
                    case RoleConstants.COORDINADOR_EJE:
                        if (solicitude.StatusId == SolicitudeStatusEnum.Aprobado.ToInt())
                        {
                            int inDictums = solicitude.Dictums.Where(x => x.StatusId != DictumStatusEnum.Anulado.ToInt()).Count();
                            validationStatus = inDictums == 0;
                        }
                        break;
                    case RoleConstants.DELEGADO_PROVINCIAL_INET:
                        if (solicitude.StatusId == SolicitudeStatusEnum.Aprobado.ToInt())
                        {
                            int inDictums = solicitude.Dictums.Where(x => x.StatusId != DictumStatusEnum.Anulado.ToInt()).Count();
                            validationStatus = inDictums == 0;
                        }
                        break;
                }
                return validationStatus && RoleHelperOld.SolicitudePermissions.CanUpdate(solicitudeStatusId, improvementPlanStatusId, improvementPlanFieldId, evaluatorUser, institutionLevelId);
            }


            /// <summary>
            /// Determina si se puede actualizar un solicitado
            /// </summary>
            /// <param name="solicitudeStatusId">Id del estado del solicitado</param>
            /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
            /// <param name="improvementPlanFieldId">Id del eje del plan</param>
            /// <param name="evaluatorUser">Evaluador del plan</param>
            /// <returns>True en caso de que se pueda editar el solicitado. False en caso contrario.</returns>
            public static bool CanUpdate(int solicitudeStatusId, int improvementPlanStatusId, int improvementPlanFieldId, UserProfile evaluatorUser, int? institutionLevelId)
            {
                var validPlanStatus = false;
                var validSolicitudeStatus = solicitudeStatusId == SolicitudeStatusEnum.EnProceso.ToInt() || solicitudeStatusId == SolicitudeStatusEnum.Pendiente.ToInt();
                var role = Roles.GetRolesForUser().First();
                switch (role)
                {
                    case RoleConstants.ADMIN:
                        validPlanStatus = improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt()
                            || improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt()
                            || improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt();
                        return validPlanStatus && validSolicitudeStatus;

                    case RoleConstants.OPERADOR_PROVINCIAL:
                        return improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt() && solicitudeStatusId == SolicitudeStatusEnum.Pendiente.ToInt();

                    case RoleConstants.DELEGADO_PROVINCIAL_INET:
                        validPlanStatus = improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt();
                        return validPlanStatus && validSolicitudeStatus && CurrentUserHasField(improvementPlanFieldId) && (!institutionLevelId.HasValue || CurrentUserHasInstitutionLevel(institutionLevelId.Value));

                    case RoleConstants.INGRESO_MONITOREO:
                        validPlanStatus = improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt();
                        return validPlanStatus && validSolicitudeStatus;

                    case RoleConstants.EVALUADOR:
                        validPlanStatus = improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt();
                        return validPlanStatus && validSolicitudeStatus && evaluatorUser != null && evaluatorUser.UserName == RoleHelperOld.CurrentUserProfile.UserName;

                    case RoleConstants.COORDINADOR_EJE:
                        validPlanStatus = improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt();
                        return validPlanStatus && validSolicitudeStatus && CurrentUserHasField(improvementPlanFieldId) && (!institutionLevelId.HasValue || CurrentUserHasInstitutionLevel(institutionLevelId.Value));

                    default:
                        return false;
                }
            }

            /// <summary>
            /// Determina si se puede eliminar un solicitado
            /// </summary>
            /// <param name="solicitudeStatusId">Id del estado del solicitado</param>
            /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
            /// <returns>True en caso de que se pueda eliminar un solicitado. False en caso contrario.</returns>
            public static bool CanDelete(int solicitudeStatusId, int improvementPlanStatusId)
            {
                if (Roles.IsUserInRole(RoleConstants.ADMIN))
                    return (improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt() || improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt()) && solicitudeStatusId == SolicitudeStatusEnum.Pendiente.ToInt();
                else if (Roles.IsUserInRole(RoleConstants.INGRESO_MONITOREO))
                    return improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt() && solicitudeStatusId == SolicitudeStatusEnum.Pendiente.ToInt();
                else if (Roles.IsUserInRole(RoleConstants.OPERADOR_PROVINCIAL))
                    return solicitudeStatusId == SolicitudeStatusEnum.Pendiente.ToInt() && improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt();
                else if (Roles.IsUserInRole(RoleConstants.DELEGADO_PROVINCIAL_INET))
                    return solicitudeStatusId == SolicitudeStatusEnum.Pendiente.ToInt() && improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt();

                return false;
            }

            /// <summary>
            /// Determina si es posible importar un plan
            /// </summary>
            /// <param name="improvementPlanId">Id del plan de mejora al cual se desean agregar los solicitados</param>
            /// <param name="improvemnentPlanStatusId">Estado en el que se encuentra el plan que se desea importar</param>
            /// <param name="improvementPlanFieldId">Id del eje del plan de mejora</param>
            /// <param name="evaluatorUser">Evaluador del plan</param>
            /// <param name="institutionLevelId"></param>
            /// <returns>True en caso de que se pueda importar solicitudes. False en caso contrario.</returns>
            public static bool CanImportSolicitudes(int improvementPlanId, int improvemnentPlanStatusId, int improvementPlanFieldId, UserProfile evaluatorUser, int? institutionLevelId)
            {
                return improvementPlanId > 0 && SolicitudePermissions.CanCreate(improvemnentPlanStatusId, improvementPlanFieldId, evaluatorUser, institutionLevelId);
            }

            /// <summary>
            /// Determina si se puede anular un solicitado
            /// </summary>
            /// <param name="solicitudeStatusId">Id del estado actual del solicitado</param>
            /// <param name="improvementPlanStatusId">Id del estado actual del plan de mejora</param>
            /// <param name="improvementPlanFieldId">Id del eje del plan de mejora</param>
            /// <param name="evaluatorUser">Usuario evaluador</param>
            /// <param name="institutionLevelId"></param>
            /// <returns>True en caso de que se pueda anular el solicitado. False en caso contrario.</returns>
            public static bool CanBlock(int solicitudeStatusId, int improvementPlanStatusId, int improvementPlanFieldId, UserProfile evaluatorUser, int? institutionLevelId)
            {
                return CanUpdate(solicitudeStatusId, improvementPlanStatusId, improvementPlanFieldId, evaluatorUser, institutionLevelId) && !Roles.IsUserInRole(RoleConstants.OPERADOR_PROVINCIAL);
            }

            /// <summary>
            /// Determina si se puede agregar un comentario al solicitado
            /// </summary>
            /// <param name="solicitudeStatusId">Id del estado del solicitado</param>
            /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
            /// <param name="improvementFieldId">Id del eje del plan</param>
            /// <param name="evaluatorUser">Usuario Evaluador</param>
            /// <param name="institutionLevelId"></param>
            /// <returns>True en caso de que se pueda agregar un comentario. False en caso contrario</returns>
            public static bool CanAddComment(int solicitudeStatusId, int improvementPlanStatusId, int improvementFieldId, UserProfile evaluatorUser, int? institutionLevelId)
            {
                return CanUpdate(solicitudeStatusId, improvementPlanStatusId, improvementFieldId, evaluatorUser, institutionLevelId);
            }

            /// <summary>
            /// Determina si se puede agregar una incidencia al solicitado
            /// </summary>
            /// <param name="solicitudeStatusId">Id del estado del solicitado</param>
            /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
            /// <param name="improvementPlanFieldId">Id del eje del plan</param>
            /// <param name="evaluatorUser">Usuario Evaluador</param>
            /// <param name="institutionLevelId"></param>
            /// <returns>True en caso de que se pueda agregar una incidencia al solicitado. False en caso contrario</returns>
            public static bool CanAddIncidence(int solicitudeStatusId, int improvementPlanStatusId, int improvementPlanFieldId, UserProfile evaluatorUser, int? institutionLevelId)
            {
                return CanUpdate(solicitudeStatusId, improvementPlanStatusId, improvementPlanFieldId, evaluatorUser, institutionLevelId);
            }
        }

        /// <summary>
        /// Establece los permisos relacionados a los comentarios
        /// </summary>
        public static class CommentPermissions
        {
            /// <summary>
            /// Si se puede comentar una incidencia
            /// </summary>
            /// <param name="incidence"></param>
            /// <returns></returns>
            public static bool CanCreate(Incidence incidence)
            {
                return incidence.Active == true && RoleHelperOld.IncidencePermissions.CanCreate(incidence.ImprovementPlan);
            }

            /// <summary>
            /// Determina si se puede crear un comentario
            /// </summary>
            /// <param name="improvementPlanStatusId">Id del estado del plan de mejora</param>
            /// <param name="improvementPlanFieldId">Id del eje del plan de mejora</param>
            /// <param name="evaluatorUser">Evaluador del plan de mejora</param>
            /// <param name="institutionLevelId"></param>
            /// <returns>True en caso de que se pueda crear un comentario. False en caso contrario.</returns>
            public static bool CanCreate(int improvementPlanStatusId, int improvementPlanFieldId, UserProfile evaluatorUser, int? institutionLevelId)
            {
                var role = Roles.GetRolesForUser().First();
                switch (role)
                {
                    case RoleConstants.ADMIN:
                        return improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt() || improvementPlanStatusId == StageStatusEnum.EnCaratulizacion.ToInt()
                                                            || improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt()
                                                            || improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt();

                    case RoleConstants.INGRESO_MONITOREO:
                        return improvementPlanStatusId == StageStatusEnum.EnIngreso.ToInt() || improvementPlanStatusId == StageStatusEnum.EnCaratulizacion.ToInt();

                    case RoleConstants.EVALUADOR:
                        return improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt() && evaluatorUser != null
                                && evaluatorUser.UserName == RoleHelperOld.CurrentUserProfile.UserName;

                    case RoleConstants.COORDINADOR_EJE:
                        return improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt() && CurrentUserHasField(improvementPlanFieldId) && (institutionLevelId.HasValue == false || CurrentUserHasInstitutionLevel(institutionLevelId.Value));

                    case RoleConstants.OPERADOR_PROVINCIAL:
                        return improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt() && CurrentUserHasField(improvementPlanFieldId) && (institutionLevelId.HasValue == false || CurrentUserHasInstitutionLevel(institutionLevelId.Value));

                    case RoleConstants.DELEGADO_PROVINCIAL_INET:
                        var result = improvementPlanStatusId == StageStatusEnum.EnCargaProvincia.ToInt() || improvementPlanStatusId == StageStatusEnum.EnEvaluacion.ToInt() || improvementPlanStatusId == StageStatusEnum.EnComisionRecepcionProvincia.ToInt();
                        return result && CurrentUserHasField(improvementPlanFieldId) && (institutionLevelId.HasValue == false || CurrentUserHasInstitutionLevel(institutionLevelId.Value));

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Establece los permisos relacionados a las incidencias
        /// </summary>
        public static class IncidencePermissions
        {

            /// <summary>
            /// Determina si se puede crear una incidencia
            /// </summary>
            /// <param name="plan">Plan de mejora a comentar</param>
            /// <returns>True en caso de que se pueda crear una nueva incidencia. False en caso contrario.</returns>
            public static bool CanCreate(ImprovementPlan plan)
            {
                bool hasLevel = plan.InstitutionLevelInt.HasValue ? RoleHelperOld.CurrentUserHasInstitutionLevel(plan.InstitutionLevelInt.Value) : true;

                switch (RoleHelperOld.CurrentUserRole)
                {
                    case RoleConstants.ADMINISTRATIVO:
                        return plan.StatusId == StageStatusEnum.EnAdministracion.ToInt();

                    case RoleConstants.ADMIN:
                        return true;

                    case RoleConstants.INGRESO_MONITOREO:
                        return plan.StatusId == StageStatusEnum.EnIngreso.ToInt() || plan.StatusId == StageStatusEnum.EnCaratulizacion.ToInt();

                    case RoleConstants.EVALUADOR:
                        return plan.StatusId == StageStatusEnum.EnEvaluacion.ToInt() && plan.EvaluatorUserId == RoleHelperOld.CurrentUserId && RoleHelperOld.CurrentUserHasField(plan.FieldId) && hasLevel;

                    case RoleConstants.COORDINADOR_EJE:
                        return plan.StatusId == StageStatusEnum.EnEvaluacion.ToInt() && RoleHelperOld.CurrentUserHasField(plan.FieldId) && hasLevel;

                    case RoleConstants.OPERADOR_PROVINCIAL:
                        return plan.StatusId == StageStatusEnum.EnCargaProvincia.ToInt() && RoleHelperOld.CurrentUserHasField(plan.FieldId) && hasLevel;

                    case RoleConstants.REFERENTE_JURISDICCIONAL:
                        return plan.StatusId == StageStatusEnum.AElevarProvincia.ToInt() && RoleHelperOld.CurrentUserHasField(plan.FieldId) && hasLevel;

                    case RoleConstants.DELEGADO_PROVINCIAL_INET:
                        var statusValid = plan.StatusId == StageStatusEnum.EnComisionRecepcionProvincia.ToInt() || plan.StatusId == StageStatusEnum.EnCargaProvincia.ToInt() || plan.StatusId == StageStatusEnum.EnEvaluacion.ToInt();
                        return statusValid && RoleHelperOld.CurrentUserHasField(plan.FieldId) && hasLevel;

                    default:
                        return false;
                }
            }

            /// <summary>
            /// Determina si el usuario tiene permisos para cambiar el estado de una incidencia
            /// </summary>
            /// <param name="incidence">Incidencia</param>
            /// <returns>True en caso de que se pueda.</returns>
            public static bool CanChangeStatus(Incidence incidence)
            {
                if (incidence.UserId == RoleHelperOld.CurrentUserId || RoleHelperOld.CurrentUserHasRole(new string[] { RoleConstants.ADMIN, RoleConstants.ADMINISTRATIVO }))
                {
                    return RoleHelperOld.IncidencePermissions.CanCreate(incidence.ImprovementPlan);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Establece los permisos relacionados a los documentos
        /// </summary>
        public static class DocumentPermissions
        {
            /// <summary>
            /// Establece los permisos relacionados a los dictamenes
            /// </summary>
            public static class DictumPermissions
            {
                /// <summary>
                /// Determina si es posible crear dictámenes
                /// </summary>
                /// <param name="improvementPLanStatusId">Id del Plan de mejora para el cual se desea crear el dictamen</param>
                /// <param name="evaluatorUser">Evaluador del plan de mejora</param>
                /// <returns>True en caso de que se pueda crear el dictamen. False en caso contrario.</returns>
                public static bool CanCreate(int improvementPLanStatusId, UserProfile evaluatorUser)
                {
                    if (improvementPLanStatusId == StageStatusEnum.Anulado.ToInt() || improvementPLanStatusId == (int)StageStatusEnum.Cerrado.ToInt()) return false;

                    return Roles.IsUserInRole(RoleConstants.ADMIN) || (Roles.IsUserInRole(RoleConstants.EVALUADOR) && improvementPLanStatusId == StageStatusEnum.EnEvaluacion.ToInt()
                            && evaluatorUser != null && evaluatorUser.UserName == RoleHelperOld.CurrentUserProfile.UserName);
                }

                /// <summary>
                /// Determina si se puede editar un dictamen
                /// </summary>
                /// <param name="dictumStatusId">Id del estado del dictamen</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan de mejora</param>
                /// <param name="evaluatorUser">Usuario evaluador del plan de mejora</param>                
                /// <returns>True en caso de que se pueda editar. False en caso contrario.</returns>
                public static bool CanUpdate(int dictumStatusId, int improvementPlanFieldId, UserProfile evaluatorUser, int? institutionLevelId)
                {
                    switch (RoleHelperOld.CurrentUserRole)
                    {
                        case RoleConstants.EVALUADOR:
                            return (dictumStatusId == DictumStatusEnum.Borrador.ToInt() || dictumStatusId == DictumStatusEnum.PendienteAprobacion.ToInt())
                                    && evaluatorUser != null && evaluatorUser.UserName == RoleHelperOld.CurrentUserProfile.UserName;

                        case RoleConstants.COORDINADOR_EJE:
                            return (dictumStatusId == DictumStatusEnum.PendienteAprobacion.ToInt() || dictumStatusId == DictumStatusEnum.Borrador.ToInt())
                                    && CurrentUserHasField(improvementPlanFieldId) && (institutionLevelId.HasValue == false || CurrentUserHasInstitutionLevel(institutionLevelId.Value));

                        case RoleConstants.ADMINISTRATIVO:
                            return dictumStatusId == DictumStatusEnum.Emitido.ToInt();

                        case RoleConstants.ADMIN:
                            return dictumStatusId == DictumStatusEnum.Borrador.ToInt() || dictumStatusId == DictumStatusEnum.PendienteAprobacion.ToInt()
                                || dictumStatusId == DictumStatusEnum.Emitido.ToInt() || dictumStatusId == EligibilityStatusEnum.Pendiente.ToInt();

                        default:
                            return false;
                    }
                }

                /// <summary>
                /// Determina si es posible anular un dictamen
                /// </summary>
                /// <param name="dictumStatusId">id del estado del dictamen</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan de mejora</param>
                /// <returns>True en caso de que se pueda anular un dictamen. False en caso contrario.</returns>
                public static bool CanBlock(int dictumStatusId, int improvementPlanFieldId, int? institutionLevelId)
                {
                    return ((RoleHelperOld.CurrentUserRole == RoleConstants.COORDINADOR_EJE && CurrentUserHasField(improvementPlanFieldId) && (institutionLevelId.HasValue == false || CurrentUserHasInstitutionLevel(institutionLevelId.Value)))
                            || RoleHelperOld.CurrentUserRole == RoleConstants.ADMIN) && dictumStatusId == DictumStatusEnum.PendienteAprobacion.ToInt();
                }

                /// <summary>
                /// Determina si se puede o no eliminar un dictamen
                /// </summary>
                /// <param name="dictumStatusId">Id del dictamen que se desea eliminar</param>
                /// <param name="evaluatorUser">Evaluador del plan de mejora</param>
                /// <returns>True en caso de que se pueda eliminar el dictamen. False en caso contrario.</returns>
                public static bool CanDelete(int dictumStatusId, UserProfile evaluatorUser)
                {
                    return ((RoleHelperOld.CurrentUserRole == RoleConstants.EVALUADOR && evaluatorUser != null && evaluatorUser.UserName == RoleHelperOld.CurrentUserProfile.UserName)
                                || RoleHelperOld.CurrentUserRole == RoleConstants.ADMIN) && dictumStatusId == DictumStatusEnum.Borrador.ToInt();
                }

                /// <summary>
                /// Determina si se puede o no emitir un dictamen
                /// </summary>
                /// <param name="dictumStatusId">Id del estado del dictamen que se desea emitir</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>
                /// <returns>True en caso de que se pueda emitir el dictamen. False en caso contrario.</returns>
                public static bool CanEmit(int dictumStatusId, int improvementPlanFieldId, int? institutionLevelId)
                {
                    return ((RoleHelperOld.CurrentUserRole == RoleConstants.COORDINADOR_EJE && CurrentUserHasField(improvementPlanFieldId) && (institutionLevelId.HasValue == false || CurrentUserHasInstitutionLevel(institutionLevelId.Value))) || RoleHelperOld.CurrentUserRole == RoleConstants.ADMIN) && dictumStatusId == DictumStatusEnum.PendienteAprobacion.ToInt();
                }

                /// <summary>
                /// Determina si se puede o no firmar un dictamen
                /// </summary>
                /// <param name="dictumStatusId">Id del Estado del dictamen</param>
                /// <returns>True en caso de que se pueda firmar. False en caso contrario.</returns>
                public static bool CanSign(int dictumStatusId)
                {
                    return (RoleHelperOld.CurrentUserRole == RoleConstants.ADMIN || RoleHelperOld.CurrentUserRole == RoleConstants.ADMINISTRATIVO) && dictumStatusId == DictumStatusEnum.Emitido.ToInt();
                }
            }

            /// <summary>
            /// Establece los permisos relacionados a las resoluciones
            /// </summary>
            public static class ResolutionPermissions
            {

                /// <summary>
                /// Si el usuario puede crear resoluciones
                /// </summary>
                /// <returns></returns>
                public static bool CanCreate()
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO);
                }

                /// <summary>
                /// Determina si es posible crear una disposición para un plan y eje
                /// </summary>
                /// <param name="improvementPLanStatusId">Id del Plan de mejora para el cual se desea crear la disposición</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>
                /// <returns>True en caso de que se pueda crear la disposición. False en caso contrario.</returns>
                public static bool CanCreate(int improvementPLanStatusId, int improvementPlanFieldId, int? institutionLevelId)
                {
                    if (improvementPLanStatusId == StageStatusEnum.Anulado.ToInt() || improvementPLanStatusId == (int)StageStatusEnum.Cerrado.ToInt()) return false;

                    return CanCreate()
                                && (improvementPLanStatusId == StageStatusEnum.EnEvaluacion.ToInt() || improvementPLanStatusId == StageStatusEnum.EnAdministracion.ToInt())
                                && CurrentUserHasField(improvementPlanFieldId)
                                && (institutionLevelId.HasValue == false || CurrentUserHasInstitutionLevel(institutionLevelId.Value));
                }

                /// <summary>
                /// Determina si se puede editar una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del estado de la disposición</param>         
                /// <returns>True en caso de que se pueda editar. False en caso contrario.</returns>
                public static bool CanUpdate(int resolutionStatusId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    switch (role)
                    {
                        case RoleConstants.ADMIN:
                        case RoleConstants.ADMINISTRATIVO:
                            return (resolutionStatusId == ResolutionStatusEnum.Borrador.ToInt()
                                || resolutionStatusId == ResolutionStatusEnum.PendienteAprobacion.ToInt()
                                || resolutionStatusId == ResolutionStatusEnum.Emitido.ToInt()
                                || resolutionStatusId == ResolutionStatusEnum.Firmado.ToInt()
                                || resolutionStatusId == ResolutionStatusEnum.Protocolizado.ToInt());
                        default:
                            return false;
                    }
                }

                /// <summary>
                /// Determina si se puede editar una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del estado de la disposición</param>
                /// <param name="improvementPlanFieldsId">Id del eje del plan</param>                
                /// <returns>True en caso de que se pueda editar. False en caso contrario.</returns>
                public static bool CanUpdate(int resolutionStatusId, IList<int> improvementPlanFieldsId, IList<int> institutionLevelId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    switch (role)
                    {
                        case RoleConstants.ADMIN:
                        case RoleConstants.ADMINISTRATIVO:
                            return CanUpdate(resolutionStatusId) && GetUserFields().Select(x => x.Id).Except(improvementPlanFieldsId).Count() == 0
                                && (institutionLevelId.Count == 0 || GetUserInstitutionLevels().Select(x => x.Code).Except(institutionLevelId).Count() == 0);
                        default:
                            return false;
                    }
                }

                /// <summary>
                /// Determina si se puede editar una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del estado de la disposición</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>                
                /// <returns>True en caso de que se pueda editar. False en caso contrario.</returns>
                public static bool CanUpdate(int resolutionStatusId, int improvementPlanFieldId, int? institutionLevelId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    switch (role)
                    {
                        case RoleConstants.ADMIN:
                        case RoleConstants.ADMINISTRATIVO:
                            return CanUpdate(resolutionStatusId) && CurrentUserHasField(improvementPlanFieldId) && (institutionLevelId.HasValue == false || CurrentUserHasInstitutionLevel(institutionLevelId.Value));
                        default:
                            return false;
                    }
                }

                /// <summary>
                /// Determina si se puede o no emitir una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del estado de la disposición que se desea emitir</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>
                /// <returns>True en caso de que se pueda emitir la disposición. False en caso contrario.</returns>
                public static bool CanEmit(int resolutionStatusId, IList<int> improvementPlanFieldsId, IList<int> institutionLevelId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && resolutionStatusId == ResolutionStatusEnum.PendienteAprobacion.ToInt()
                            && CanUpdate(resolutionStatusId) && GetUserFields().Select(x => x.Id).Except(improvementPlanFieldsId).Count() == 0
                                && (institutionLevelId.Count == 0 || GetUserInstitutionLevels().Select(x => x.Code).Except(institutionLevelId).Count() == 0); ;
                }

                /// <summary>
                /// Determina si se puede o no emitir una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del estado de la disposición que se desea emitir</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>
                /// <returns>True en caso de que se pueda emitir la disposición. False en caso contrario.</returns>
                public static bool CanEmit(int resolutionStatusId, int improvementPlanFieldId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && resolutionStatusId == ResolutionStatusEnum.PendienteAprobacion.ToInt()
                            && CurrentUserHasField(improvementPlanFieldId);
                }

                /// <summary>
                /// Determina si se puede o no firmar una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del Estado de la disposición</param>
                /// <returns>True en caso de que se pueda firmar. False en caso contrario.</returns>
                public static bool CanSign(int resolutionStatusId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && resolutionStatusId == ResolutionStatusEnum.Emitido.ToInt();
                }

                /// <summary>
                /// Determina si se puede o no firmar una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del Estado de la disposición</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>
                /// <returns>True en caso de que se pueda firmar. False en caso contrario.</returns>
                public static bool CanSign(int resolutionStatusId, int improvementPlanFieldId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && resolutionStatusId == ResolutionStatusEnum.Emitido.ToInt();
                }

                /// <summary>
                /// Determina si es posible o no eliminar una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del estado de la disposición que se desea eliminar</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>
                /// <returns>True en caso de que sea posible eliminar la disposición. False en caso contrario.</returns>
                public static bool CanDelete(int resolutionStatusId, IList<int> improvementPlanFieldsId, IList<int> institutionLevelId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && CanUpdate(resolutionStatusId) && GetUserFields().Select(x => x.Id).Except(improvementPlanFieldsId).Count() == 0
                                && (institutionLevelId.Count == 0 || GetUserInstitutionLevels().Select(x => x.Code).Except(institutionLevelId).Count() == 0)
                            && resolutionStatusId == ResolutionStatusEnum.Borrador.ToInt();
                }

                /// <summary>
                /// Determina si es posible o no eliminar una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del estado de la disposición que se desea eliminar</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>
                /// <returns>True en caso de que sea posible eliminar la disposición. False en caso contrario.</returns>
                public static bool CanDelete(int resolutionStatusId, int improvementPlanFieldId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && CurrentUserHasField(improvementPlanFieldId)
                            && resolutionStatusId == ResolutionStatusEnum.Borrador.ToInt();
                }

                /// <summary>
                /// Determina si es posible anular una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del estado de la disposición</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>
                /// <returns>True en caso de que se pueda anular la disposición. False en caso contrario.</returns>
                public static bool CanBlock(int resolutionStatusId, IList<int> improvementPlanFieldsId, IList<int> institutionLevelId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && CanUpdate(resolutionStatusId) && GetUserFields().Select(x => x.Id).Except(improvementPlanFieldsId).Count() == 0
                                && (institutionLevelId.Count == 0 || GetUserInstitutionLevels().Select(x => x.Code).Except(institutionLevelId).Count() == 0)
                            && resolutionStatusId == ResolutionStatusEnum.Emitido.ToInt();
                }

                /// <summary>
                /// Determina si es posible anular una disposición
                /// </summary>
                /// <param name="resolutionStatusId">Id del estado de la disposición</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan</param>
                /// <returns>True en caso de que se pueda anular la disposición. False en caso contrario.</returns>
                public static bool CanBlock(int resolutionStatusId, int improvementPlanFieldId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && CurrentUserHasField(improvementPlanFieldId)
                            && resolutionStatusId == ResolutionStatusEnum.Emitido.ToInt();
                }

                /// <summary>
                /// Determina si es posible setear la fecha de envío
                /// </summary>
                /// <param name="resolutionStatusId">Id del estdo actual de la disposición</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan de mejora</param>
                /// <returns>True en caso de que se pueda setear la fecha de envío. False en caso contrario.</returns>
                public static bool CanSetShipDate(int resolutionStatusId, IList<int> improvementPlanFieldsId, IList<int> institutionLevelId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && CanUpdate(resolutionStatusId) && GetUserFields().Select(x => x.Id).Except(improvementPlanFieldsId).Count() == 0
                                && (institutionLevelId.Count == 0 || GetUserInstitutionLevels().Select(x => x.Code).Except(institutionLevelId).Count() == 0)
                            && (resolutionStatusId == ResolutionStatusEnum.Firmado.ToInt() || resolutionStatusId == ResolutionStatusEnum.Protocolizado.ToInt());
                }

                /// <summary>
                /// Determina si es posible setear la fecha de envío
                /// </summary>
                /// <param name="resolutionStatusId">Id del estdo actual de la disposición</param>
                /// <param name="improvementPlanFieldId">Id del eje del plan de mejora</param>
                /// <returns>True en caso de que se pueda setear la fecha de envío. False en caso contrario.</returns>
                public static bool CanSetShipDate(int resolutionStatusId, int improvementPlanFieldId)
                {
                    var role = RoleHelperOld.CurrentUserRole;
                    return (role == RoleConstants.ADMIN || role == RoleConstants.ADMINISTRATIVO)
                            && CurrentUserHasField(improvementPlanFieldId)
                            && (resolutionStatusId == ResolutionStatusEnum.Firmado.ToInt() || resolutionStatusId == ResolutionStatusEnum.Protocolizado.ToInt());
                }
            }
        }

    }
}
