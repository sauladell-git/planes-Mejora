using INET.Core.Constants;
using INET.Core.Enums;
using INET.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Utils.Helpers.Permissions
{
    static public class SolicitudePermissions
    {

        public static bool CanCreate(ImprovementPlan plan)
        {
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Solicitudes.CREATE, plan.StatusId);
            var cond2 = (!SessionHelper.HasRole(RoleConstants.EVALUADOR) || plan.EvaluatorUserId == SessionHelper.CurrentUserId);
            return cond1 && cond2;
        }

        public static bool CanEdit(Data.Solicitude s)
        {
            return CanEdit(s.StatusId, s.ImprovementPlan.StatusId, s.ImprovementPlan.EvaluatorUserId ?? 0, s.Locked);
        }

        public static bool CanEdit(int solicitudeStatusId, int planStatusId, int evaluatorId, bool locked)
        {
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Solicitudes.EDIT, planStatusId);
            var cond2 = (!SessionHelper.HasRole(RoleConstants.EVALUADOR) || evaluatorId == SessionHelper.CurrentUserId);
            var cond3 = ListAvailableStatuses(solicitudeStatusId, planStatusId).Where(x=> x.Id == solicitudeStatusId).Any();
            var cond4 = solicitudeStatusId != (int)SolicitudeStatusEnum.Aprobado || SessionHelper.HasPermission(PermissionConstants.Solicitudes.APPROVE);
            var cond5 = locked == false;
            return cond1 && cond4 && cond2 && cond3 && cond5;
        }

        public static bool CanBlock(Data.Solicitude s)
        {
            return CanBlock(s.ImprovementPlan.EvaluatorUserId ?? 0, s.ImprovementPlan.StatusId, s.Locked);
        }

        public static bool CanBlock(int planEvaluatorId, int planStatusId, bool locked)
        {
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Solicitudes.BLOCK, planStatusId);
            var cond2 = (!SessionHelper.HasRole(RoleConstants.EVALUADOR) || planEvaluatorId == SessionHelper.CurrentUserId);
            var cond3 = locked == false;
            return cond1 && cond2 && cond3;
        }

        public static bool CanDelete(Data.Solicitude s)
        {
            return CanDelete(s.StatusId, s.ImprovementPlan.StatusId, s.Locked);
        }

        public static bool CanDelete(int solicitudeStatusId, int planStatusId, bool locked)
        {
            var cond1 = solicitudeStatusId == (int)SolicitudeStatusEnum.Pendiente;
            var cond2 = SessionHelper.HasPermission(PermissionConstants.Solicitudes.DELETE, planStatusId);
            var cond3 = locked == false;
            return cond1 && cond2 && cond3;
        }

        #region Internal helpers
        /// <summary>
        /// Helper method
        /// </summary>
        static private Data.Solicitude GetSolicitude(int id)
        {
            Data.Solicitude result;
            var context = new INETContext();
            result = context.Solicitudes.Where(x => x.Id == id).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// List available statuses for a solicitude in a given status
        /// </summary>
        /// <param name="solicitudeId"></param>
        /// <param name="improvementPlanStatusId"></param>
        /// <returns></returns>
        public static IEnumerable<Status> ListAvailableStatuses(int solicitudeStatusId, int improvementPlanStatusId)
        {
            var context = new INETContext();
            if (solicitudeStatusId == 0)
                return context.Status.Where(x => x.Id == (int)SolicitudeStatusEnum.Pendiente).ToList();

            var planStatus = (StageStatusEnum) improvementPlanStatusId;

            var canApprove = SessionHelper.HasPermission(PermissionConstants.Solicitudes.APPROVE);

            switch (planStatus)
            {

                case StageStatusEnum.EnCargaProvincia:
                    return context.Status.Where(x => x.Id == (int)SolicitudeStatusEnum.Pendiente).ToList();

                case StageStatusEnum.EnIngreso:
                    return context.Status.Where(x => x.Id == (int)SolicitudeStatusEnum.Pendiente).ToList();

                case StageStatusEnum.EnEvaluacion:

                    switch ((SolicitudeStatusEnum)solicitudeStatusId)
                    {
                        case SolicitudeStatusEnum.Pendiente:
                            return context.Status.Where(x => x.Id == (int)SolicitudeStatusEnum.EnProceso || (canApprove && x.Id == (int)SolicitudeStatusEnum.Rechazado)
                                                || (canApprove && x.Id == (int)SolicitudeStatusEnum.Aprobado)
                                                || x.Id == (int)SolicitudeStatusEnum.Pendiente).ToList();
                        case SolicitudeStatusEnum.EnProceso:
                            return context.Status.Where(x => x.Id == (int)SolicitudeStatusEnum.EnProceso || (canApprove && x.Id == (int)SolicitudeStatusEnum.Rechazado)
                                                || (canApprove && x.Id == (int)SolicitudeStatusEnum.Aprobado) || (canApprove && x.Id == (int)SolicitudeStatusEnum.Pendiente)).ToList();
                        case SolicitudeStatusEnum.Rechazado:
                            return context.Status.Where(x => x.Id == (int)SolicitudeStatusEnum.EnProceso || x.Id == (int)SolicitudeStatusEnum.Rechazado).ToList();
                        case SolicitudeStatusEnum.Aprobado:
                            return context.Status.Where(x => (canApprove && x.Id == (int)SolicitudeStatusEnum.EnProceso) ||   x.Id == (int)SolicitudeStatusEnum.Aprobado || (canApprove && x.Id == (int)SolicitudeStatusEnum.Pendiente)  ).ToList();

                        default:
                            return new List<Status>();
                    }

                default:
                    return new List<Status>();
            }
        }
        #endregion

        #region Proxy to external permissions
        /// <summary>
        /// Proxy implementation to external permissions
        /// </summary>
        static public class Incidences
        {
            static public bool CanCreate(int solicitudeStatusId, int planStatusId, int evaluatorId)
            {
                bool cond1 = SessionHelper.HasPermission(PermissionConstants.Solicitudes.INCIDENTS_WRITE);
                bool cond2 = SolicitudePermissions.CanEdit(solicitudeStatusId, planStatusId, evaluatorId, false);
                return cond1 && cond2;
            }
        }

        /// <summary>
        /// Proxy implementation to external permissions
        /// </summary>
        static public class Comments
        {
            
            public static bool CanCreate(int solicitudeStatusId, int planStatusId, int evaluatorId)
            {
                bool cond1 = SessionHelper.HasPermission(PermissionConstants.Solicitudes.COMMENTS_WRITE);
                bool cond2 = SolicitudePermissions.CanEdit(solicitudeStatusId, planStatusId, evaluatorId, false);
                return cond1 && cond2;
            }

        }
        #endregion

    }
}
