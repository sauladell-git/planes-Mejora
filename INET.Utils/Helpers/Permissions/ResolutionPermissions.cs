using INET.Data;
using INET.Core.Constants;
using INET.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Utils.Helpers.Permissions
{
    public static class ResolutionPermissions
    {

        static public bool CanEdit(Resolution reso)
        {
            var fieldsIds = reso.Dictums.Select(x => x.ImprovementPlan.FieldId).ToList();
            var levelsIds = reso.Dictums.Where(x => x.ImprovementPlan.InstitutionLevelInt.HasValue).Select(x => x.ImprovementPlan.InstitutionLevelInt.Value).ToList();
            var provincesNumbers = reso.Dictums.Select(x => x.ImprovementPlan.CUE.Substring(0, 2)).ToList();
            return CanEdit(reso.StatusId, fieldsIds, levelsIds, provincesNumbers);
        }

        static public bool CanEdit(int statusId, IList<int> improvementPlanFieldsId, IList<int> institutionLevelId, IList<string> provincesNumbers)
        {
            var userFields = SessionHelper.GetFields();
            var userInstitutionLevels = SessionHelper.GetInstitutionLevels();
            var userProvinces = SessionHelper.CurrentUserProfile.Provinces;

            var permittedStatuses = new List<int>() { (int)ResolutionStatusEnum.Borrador, (int)ResolutionStatusEnum.PendienteAprobacion };

            if (SessionHelper.HasPermission(PermissionConstants.Resolutions.SIGN))
            {
                permittedStatuses.Add((int)ResolutionStatusEnum.Emitido);
            }

            if (SessionHelper.HasRole(RoleConstants.ADMIN))
            {
                permittedStatuses.Add((int)ResolutionStatusEnum.Firmado);
            }

            var cond0 = permittedStatuses.Contains(statusId);
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Resolutions.EDIT);
            var cond2 = userFields.Count == 0 || (improvementPlanFieldsId.Count > 0 && !improvementPlanFieldsId.Except(userFields.Select(x => x.Id)).Any());
            var cond3 = userInstitutionLevels.Count == 0 || (institutionLevelId.Count > 0 && !institutionLevelId.Except(userInstitutionLevels.Select(x => x.Id)).Any());
            var cond4 = userProvinces.Count == 0 || (provincesNumbers.Count > 0 && !provincesNumbers.Except(userProvinces.Select(x => x.Number)).Any());
            return cond0 && cond1 && cond2 && cond3 && cond4;
        }

        static public bool CanDelete(Resolution reso)
        {
            var fieldsIds = reso.Dictums.Select(x => x.ImprovementPlan.FieldId).ToList();
            var levelsIds = reso.Dictums.Where(x => x.ImprovementPlan.InstitutionLevelInt.HasValue).Select(x => x.ImprovementPlan.InstitutionLevelInt.Value).ToList();
            var provincesNumbers = reso.Dictums.Select(x => x.ImprovementPlan.CUE.Substring(0, 2)).ToList();
            return CanDelete(reso.StatusId, fieldsIds, levelsIds, provincesNumbers);
        }

        static public bool CanDelete(int statusId, IList<int> improvementPlanFieldsId, IList<int> institutionLevelId, IList<string> provincesNumbers)
        {
            var userFields = SessionHelper.GetFields();
            var userInstitutionLevels = SessionHelper.GetInstitutionLevels();
            var userProvinces = SessionHelper.CurrentUserProfile.Provinces;

            var permittedStatuses = new int[] { (int)ResolutionStatusEnum.Borrador, (int)ResolutionStatusEnum.PendienteAprobacion };

            var cond0 = permittedStatuses.Contains(statusId);
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Resolutions.DELETE);
            var cond2 = userFields.Count == 0 || (improvementPlanFieldsId.Count > 0 && !improvementPlanFieldsId.Except(userFields.Select(x => x.Id)).Any());
            var cond3 = userInstitutionLevels.Count == 0 || (institutionLevelId.Count > 0 && !institutionLevelId.Except(userInstitutionLevels.Select(x => x.Id)).Any());
            var cond4 = userProvinces.Count == 0 || (provincesNumbers.Count > 0 && !provincesNumbers.Except(userProvinces.Select(x => x.Number)).Any());
            return cond0 && cond1 && cond2 && cond3 && cond4;
        }

        static public bool CanBlock(Resolution reso)
        {
            var fieldsIds = reso.Dictums.Select(x => x.ImprovementPlan.FieldId).ToList();
            var levelsIds = reso.Dictums.Where(x => x.ImprovementPlan.InstitutionLevelInt.HasValue).Select(x => x.ImprovementPlan.InstitutionLevelInt.Value).ToList();
            var provincesNumbers = reso.Dictums.Select(x => x.ImprovementPlan.CUE.Substring(0, 2)).ToList();
            return CanBlock(reso.StatusId, fieldsIds, levelsIds, provincesNumbers);
        }

        static public bool CanBlock(int statusId, IList<int> improvementPlanFieldsId, IList<int> institutionLevelId, IList<string> provincesNumbers)
        {
            var userFields = SessionHelper.GetFields();
            var userInstitutionLevels = SessionHelper.GetInstitutionLevels();
            var userProvinces = SessionHelper.CurrentUserProfile.Provinces;

            var permittedStatuses = new int[] { (int)ResolutionStatusEnum.Emitido };

            var cond0 = permittedStatuses.Contains(statusId);
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Resolutions.BLOCK);
            var cond2 = userFields.Count == 0 || (improvementPlanFieldsId.Count > 0 && !improvementPlanFieldsId.Except(userFields.Select(x => x.Id)).Any());
            var cond3 = userInstitutionLevels.Count == 0 || (institutionLevelId.Count > 0 && !institutionLevelId.Except(userInstitutionLevels.Select(x => x.Id)).Any());
            var cond4 = userProvinces.Count == 0 || (provincesNumbers.Count > 0 && !provincesNumbers.Except(userProvinces.Select(x => x.Number)).Any());
            return cond0 && cond1 && cond2 && cond3 && cond4;
        }

        static public bool CanEmit(Resolution reso)
        {
            var fieldsIds = reso.Dictums.Select(x => x.ImprovementPlan.FieldId).ToList();
            var levelsIds = reso.Dictums.Where(x => x.ImprovementPlan.InstitutionLevelInt.HasValue).Select(x => x.ImprovementPlan.InstitutionLevelInt.Value).ToList();
            var provincesNumbers = reso.Dictums.Select(x => x.ImprovementPlan.CUE.Substring(0, 2)).ToList();

            var userFields = SessionHelper.GetFields();
            var userInstitutionLevels = SessionHelper.GetInstitutionLevels();
            var userProvinces = SessionHelper.CurrentUserProfile.Provinces;

            var permittedStatuses = new int[] { (int)ResolutionStatusEnum.PendienteAprobacion };

            var cond0 = permittedStatuses.Contains(reso.StatusId);
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Resolutions.EMIT);
            var cond2 = userFields.Count == 0 || (fieldsIds.Count > 0 && !fieldsIds.Except(userFields.Select(x => x.Id)).Any());
            var cond3 = userInstitutionLevels.Count == 0 || (levelsIds.Count > 0 && !levelsIds.Except(userInstitutionLevels.Select(x => x.Id)).Any());
            var cond4 = userProvinces.Count == 0 || (provincesNumbers.Count > 0 && !provincesNumbers.Except(userProvinces.Select(x => x.Number)).Any());
            return cond0 && cond1 && cond2 && cond3 && cond4;
        }

        static public bool CanSign(Resolution reso)
        {
            var fieldsIds = reso.Dictums.Select(x => x.ImprovementPlan.FieldId).ToList();
            var levelsIds = reso.Dictums.Where(x => x.ImprovementPlan.InstitutionLevelInt.HasValue).Select(x => x.ImprovementPlan.InstitutionLevelInt.Value).ToList();
            var provincesNumbers = reso.Dictums.Select(x => x.ImprovementPlan.CUE.Substring(0, 2)).ToList();

            var userFields = SessionHelper.GetFields();
            var userInstitutionLevels = SessionHelper.GetInstitutionLevels();
            var userProvinces = SessionHelper.CurrentUserProfile.Provinces;

            var permittedStatuses = new List<int>();
            permittedStatuses.Add((int)ResolutionStatusEnum.Emitido);

            if (SessionHelper.HasRole(RoleConstants.ADMIN))
            {
                permittedStatuses.Add((int)ResolutionStatusEnum.Firmado);
            }

            var cond0 = permittedStatuses.Contains(reso.StatusId);
            var cond1 = SessionHelper.HasPermission(PermissionConstants.Resolutions.SIGN);
            var cond2 = userFields.Count == 0 || (fieldsIds.Count > 0 && !fieldsIds.Except(userFields.Select(x => x.Id)).Any());
            var cond3 = userInstitutionLevels.Count == 0 || (levelsIds.Count > 0 && !levelsIds.Except(userInstitutionLevels.Select(x => x.Id)).Any());
            var cond4 = userProvinces.Count == 0 || (provincesNumbers.Count > 0 && !provincesNumbers.Except(userProvinces.Select(x => x.Number)).Any());
            return cond0 && cond1 && cond2 && cond3 && cond4;
        }

        #region Helpers
        /// <summary>
        /// Helper method
        /// </summary>
        static private Data.Resolution GetResolution(int id)
        {
            Data.Resolution result;
            var context = new INETContext();
            result = context.Documents.Include("ImprovementPlan").OfType<Resolution>().Where(x => x.Id == id).FirstOrDefault();
            return result;
        }



        /// <summary>
        /// List available statuses for a solicitude in a given status
        /// </summary>
        /// <param name="resolutionId"></param>
        /// <returns></returns>
        public static IEnumerable<Status> ListAvailableStatuses(int resolutionId)
        {
            var context = new INETContext();
            if (resolutionId == 0)
                return context.Status.Where(x => x.Id == (int)ResolutionStatusEnum.Borrador).ToList();

            Resolution resolution = GetResolution(resolutionId);

            var isAdmin = SessionHelper.HasRole(RoleConstants.ADMIN);
            var canEmit = SessionHelper.HasPermission(PermissionConstants.Resolutions.EMIT);
            var canSign = SessionHelper.HasPermission(PermissionConstants.Resolutions.SIGN);
            //var canBack = SessionHelper.HasPermission(PermissionConstants.Resolutions.SIGN_BACK);
            switch ((int)resolution.StatusId)
            {
                case (int)ResolutionStatusEnum.Borrador:
                    return context.Status.Where(x => x.Id == (int)ResolutionStatusEnum.Borrador || x.Id == (int)ResolutionStatusEnum.PendienteAprobacion || (isAdmin && canSign && x.Id == (int)ResolutionStatusEnum.Firmado)).ToList();
                case (int)ResolutionStatusEnum.PendienteAprobacion:
                    return context.Status.Where(x => x.Id == (int)ResolutionStatusEnum.Borrador || x.Id == (int)ResolutionStatusEnum.PendienteAprobacion || (canEmit && x.Id == (int)ResolutionStatusEnum.Emitido)).ToList();
                case (int)ResolutionStatusEnum.Emitido:
                    return context.Status.Where(x => x.Id == (int)ResolutionStatusEnum.Emitido || x.Id == (int)ResolutionStatusEnum.Borrador || x.Id == (int)ResolutionStatusEnum.PendienteAprobacion || (canSign && x.Id == (int)ResolutionStatusEnum.Firmado)).ToList();
                case (int)ResolutionStatusEnum.Firmado:
                    return context.Status.Where(x => (isAdmin  && x.Id == (int)ResolutionStatusEnum.Borrador) || (isAdmin&& x.Id == (int)ResolutionStatusEnum.PendienteAprobacion) || (isAdmin && canSign && x.Id == (int)ResolutionStatusEnum.Firmado)).ToList();
                    

              default:
                    return new List<Status>();
            }
        }

        #endregion
    }
}
