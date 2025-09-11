using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INET.Core.Constants;
using INET.Core.Enums;
using INET.Data;

namespace INET.Utils.Helpers.Permissions
{
    static public class DictumPermissions
    {
        public static bool CanCreate(ImprovementPlan plan)
        {
            var result = false;
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Dictums.CREATE, plan.StatusId);
            var cond2 = (!SessionHelper.HasRole(RoleConstants.EVALUADOR) || plan.EvaluatorUserId == SessionHelper.CurrentUserId);
            if (cond1 && cond2)
            {
                result = true;
            }
            return result;
        }

        public static bool CanEdit(int dictumId)
        {
            return CanEdit(GetDictum(dictumId));
        }

        public static bool CanEdit(Dictum dictum)
        {
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Dictums.EDIT, dictum.ImprovementPlan.StatusId);
            var cond2 = (!SessionHelper.HasRole(RoleConstants.EVALUADOR) || dictum.ImprovementPlan.EvaluatorUserId == SessionHelper.CurrentUserId);
            var cond3 = ListAvailableStatuses(dictum.Id).Where(x => x.Id == dictum.StatusId).Any() && (dictum.StatusId != (int)DictumStatusEnum.Firmado || CanUndoSign(dictum));
            return cond1 && cond2 && cond3;
        }

        public static bool CanDelete(int dictumId)
        {
            return CanDelete(GetDictum(dictumId));
        }

        public static bool CanDelete(Dictum dictum)
        {
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Dictums.DELETE, dictum.ImprovementPlan.StatusId);
            var cond2 = dictum.StatusId != (int)DictumStatusEnum.Emitido && dictum.StatusId != (int)DictumStatusEnum.Firmado;
            var cond3 = dictum.Locked == false;
            return cond1 && cond2 && cond3;
        }

        public static bool CanBlock(int dictumId)
        {
            return CanBlock(GetDictum(dictumId));
        }

        public static bool CanBlock(Dictum dictum)
        {
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Dictums.BLOCK, dictum.ImprovementPlan.StatusId);
            var cond2 = (!SessionHelper.HasRole(RoleConstants.EVALUADOR) || dictum.ImprovementPlan.EvaluatorUserId == SessionHelper.CurrentUserId);
            var cond3 = dictum.StatusId == (int)DictumStatusEnum.Emitido || dictum.StatusId == (int)DictumStatusEnum.Firmado;
            var cond4 = dictum.Locked == false;
            return cond1 && cond2 && cond3 && cond4;
        }

        public static bool CanEmit(int statusId)
        {
            return SessionHelper.HasPermission(PermissionConstants.Dictums.EMIT) && statusId == (int)DictumStatusEnum.PendienteAprobacion;
        }

        public static bool CanUndoEmit(Dictum dictum)
        {
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Dictums.EMIT);
            var cond2 = SessionHelper.HasRole(RoleConstants.ADMIN);
            var cond3 = dictum.StatusId == (int)DictumStatusEnum.Emitido;
            var cond4 = dictum.Locked == false;
            return cond1 && cond2 && cond3 && cond4;
        }

        public static bool CanSign(int statusId)
        {
            return SessionHelper.HasPermission(PermissionConstants.Dictums.SIGN) && statusId == (int)DictumStatusEnum.Emitido;
        }

        public static bool CanUndoSign(Dictum dictum)
        {
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Dictums.SIGN);
            var cond2 = SessionHelper.HasRole(RoleConstants.ADMIN);
            var cond3 = dictum.StatusId == (int)DictumStatusEnum.Firmado;
            var cond4 = dictum.Locked == false;
            return cond1 && cond2 && cond3 && cond4;
        }

        #region Helpers
        /// <summary>
        /// Helper method
        /// </summary>
        static private Data.Dictum GetDictum(int id)
        {
            Data.Dictum result;
            var context = new INETContext();
            result = context.Documents.Include("ImprovementPlan").OfType<Dictum>().Where(x => x.Id == id).FirstOrDefault();
            return result;
        }



        /// <summary>
        /// List available statuses for a solicitude in a given status
        /// </summary>
        /// <param name="dictumId"></param>
        /// <returns></returns>
        public static IEnumerable<Status> ListAvailableStatuses(int dictumId)
        {
            var context = new INETContext();
            if (dictumId == 0)
            {
                return context.Status.Where(x => x.Id == (int)DictumStatusEnum.Borrador).ToList();
            }

            Dictum dictum = GetDictum(dictumId);
            var planStatus = (StageStatusEnum)dictum.ImprovementPlan.StatusId;

            var isAdmin = SessionHelper.HasRole(RoleConstants.ADMIN);
            var canEmit = SessionHelper.HasPermission(PermissionConstants.Dictums.EMIT);
            var canSign = SessionHelper.HasPermission(PermissionConstants.Dictums.SIGN);
            var canUndoEmit = CanUndoEmit(dictum);
            var canUndoSign = CanUndoSign(dictum);

            switch (planStatus)
            {
                case StageStatusEnum.EnEvaluacion:

                    switch ((int)dictum.StatusId)
                    {
                        case (int)DictumStatusEnum.Borrador:
                            return context.Status.Where(x => x.Id == (int)DictumStatusEnum.Borrador || x.Id == (int)DictumStatusEnum.PendienteAprobacion || (isAdmin && canSign && x.Id == (int)DictumStatusEnum.Firmado)).ToList();
                        case (int)DictumStatusEnum.PendienteAprobacion:
                            return context.Status.Where(x => x.Id == (int)DictumStatusEnum.Borrador || x.Id == (int)DictumStatusEnum.PendienteAprobacion || (canEmit && x.Id == (int)DictumStatusEnum.Emitido)).ToList();
                        case (int)DictumStatusEnum.Emitido:
                            return context.Status.Where(x =>
                            (canSign && (x.Id == (int)DictumStatusEnum.Emitido || x.Id == (int)DictumStatusEnum.Firmado))
                            || (canUndoEmit && x.Id == (int)DictumStatusEnum.PendienteAprobacion)
                            ).ToList();
                        case (int)DictumStatusEnum.Firmado:
                            return context.Status.Where(x => canUndoSign && (x.Id == (int)DictumStatusEnum.Emitido || x.Id == (int)DictumStatusEnum.Firmado)).ToList();
                        default:
                            return new List<Status>();
                    }

                case StageStatusEnum.EnAdministracion:

                    switch ((int)dictum.StatusId)
                    {
                        case (int)DictumStatusEnum.Borrador:
                            return context.Status.Where(x => x.Id == (int)DictumStatusEnum.Borrador || x.Id == (int)DictumStatusEnum.PendienteAprobacion || (isAdmin && canSign && x.Id == (int)DictumStatusEnum.Firmado)).ToList();
                        case (int)DictumStatusEnum.PendienteAprobacion:
                            return context.Status.Where(x => x.Id == (int)DictumStatusEnum.Borrador || x.Id == (int)DictumStatusEnum.PendienteAprobacion || (canEmit && x.Id == (int)DictumStatusEnum.Emitido)).ToList();
                        case (int)DictumStatusEnum.Emitido:
                            return context.Status.Where(x => canSign && (x.Id == (int)DictumStatusEnum.Emitido || x.Id == (int)DictumStatusEnum.Firmado)).ToList();
                        case (int)DictumStatusEnum.Firmado:
                            return context.Status.Where(x => canUndoSign && (x.Id == (int)DictumStatusEnum.Emitido || x.Id == (int)DictumStatusEnum.Firmado)).ToList();
                        default:
                            return new List<Status>();
                    }

                default:
                    return new List<Status>();
            }
        }

        #endregion
    }
}
