using INET.Core.Constants;
using INET.Core.Enums;
using INET.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Utils.Helpers.Permissions
{
    static public class ImprovementPlanPermissions
    {
        /// <summary>
        /// Inidica si el usuario tiene acceso y/o visibilidad de un plan
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public static bool HasAccess(Data.ImprovementPlan plan)
        {
            var result = false;
            if (SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.READ))
            {
                // si tiene lectura, tiene que tener
                // acceso a todas las provincias o a la del plan
                // acceso a todos los ejes o al del plan
                // acceso a todos los niveles o al del plan
                result = (SessionHelper.CurrentUserProfile.Provinces.Count == 0 || SessionHelper.HasProvince(plan.CUE.Substring(0, 2))) && (SessionHelper.GetFields().Count == 0 || SessionHelper.HasField(plan.FieldId))
                    && (plan.InstitutionLevelInt.HasValue == false || (SessionHelper.CurrentUserProfile.InstitutionLevels.Count == 0 || SessionHelper.HasInstitutionLevel(plan.InstitutionLevelInt.Value)));
            }
            return result;
        }

        /// <summary>
        /// Inidica si el usuario tiene acceso y/o visibilidad de un plan
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public static bool HasAccess(int id)
        {
            return HasAccess(GetPlan(id));
        }

        /// <summary>
        /// Puede cambiar el estado del elemento
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public static bool CanChangeStatus(Data.ImprovementPlan plan)
        {
            var result = false;
            var ifEval = (!SessionHelper.HasRole(RoleConstants.EVALUADOR) || plan.EvaluatorUserId == SessionHelper.CurrentUserId);
            if (SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.CHANGE_STATUS, plan.StatusId) && HasAccess(plan) && ifEval)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Si se puede actualizar de forma masiva solicitados
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="status_id"></param>
        /// <param name="evaluator_id"></param>
        /// <returns></returns>
        public static bool CanBulkAprove(int status_id, int evaluator_id)
        {
            var cond1 = status_id == (int)StageStatusEnum.EnEvaluacion && SessionHelper.HasPermission(PermissionConstants.Solicitudes.EDIT, (int)StageStatusEnum.EnEvaluacion);
            var cond2 = (!SessionHelper.HasRole(RoleConstants.EVALUADOR) || evaluator_id == SessionHelper.CurrentUserId);
            return cond1 && cond2;
        }

        #region Helper methods
        /// <summary>
        /// Helper method
        /// </summary>
        static private Data.ImprovementPlan GetPlan(int id)
        {
            Data.ImprovementPlan result;
            var context = new INETContext();
            result = context.ImprovementPlans.Where(x => x.Id == id).FirstOrDefault();
            return result;
        }
        #endregion

        /// <summary>
        /// Proxy implementation to external permissions
        /// </summary>
        static public class Solicitudes
        {
            public static bool CanCreate(int planId)
            {
                var plan = ImprovementPlanPermissions.GetPlan(planId);
                return SolicitudePermissions.CanCreate(plan);
            }
        }

        /// <summary>
        /// Proxy implementation to external permissions
        /// </summary>
        static public class Incidences
        {
            static public bool CanCreate(int id)
            {
                return CanCreate(ImprovementPlanPermissions.GetPlan(id));
            }

            static public bool CanCreate(Data.ImprovementPlan plan)
            {
                var cond1 = SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.INCIDENTS_WRITE);
                var cond2 = ImprovementPlanPermissions.CanChangeStatus(plan) || (plan.StatusId == (int)StageStatusEnum.EnEvaluacion && plan.EvaluatorUserId == SessionHelper.CurrentUserId);
                return cond1 && cond2;
            }
        }

        /// <summary>
        /// Proxy implementation to external permissions
        /// </summary>
        static public class Comments
        {
            static public bool CanCreate(int id)
            {
                return CanCreate(ImprovementPlanPermissions.GetPlan(id));
            }

            public static bool CanCreate(Data.ImprovementPlan plan)
            {
                var cond1 = SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.COMMENTS_WRITE);
                var cond2 = ImprovementPlanPermissions.CanChangeStatus(plan) || (plan.StatusId == (int)StageStatusEnum.EnEvaluacion && plan.EvaluatorUserId == SessionHelper.CurrentUserId);
                return cond1 && cond2;
            }
        }

        /// <summary>
        /// Proxy implementation to external permissions
        /// </summary>
        static public class Dictums
        {
            public static bool CanCreate(int planId)
            {
                var plan = ImprovementPlanPermissions.GetPlan(planId);
                return DictumPermissions.CanCreate(plan);
            }
        }

    }
}
