using INET.Core.Constants;
using INET.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Utils.Helpers.Permissions
{
    static public class IncidencePermissions
    {

        static public bool CanComment(Data.Incidence incidence)
        {
            return CanUpdate(incidence);
        }

        static public bool CanChangeStatus(Data.Incidence incidence)
        {
            var _return = false;

            if (incidence.UserId == SessionHelper.CurrentUserId || SessionHelper.HasAnyRole(new string[] { RoleConstants.ADMIN, RoleConstants.ADMINISTRATIVO }))
            {
                _return = CanUpdate(incidence);
            }
            return _return;
        }

        #region Helper methods
        static private bool CanUpdate(Data.Incidence incidence)
        {
            var _return = false;
            if (incidence.Active)
            {
                if (incidence.SolicitudeId != null)
                {
                    if (SessionHelper.HasPermission(PermissionConstants.Solicitudes.INCIDENTS_WRITE) && SolicitudePermissions.CanEdit(incidence.Solicitude))
                    {
                        _return = true;
                    }
                }
                else
                {
                    if (SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.INCIDENTS_WRITE) && ImprovementPlanPermissions.CanChangeStatus(incidence.ImprovementPlan))
                    {
                        _return = true;
                    }
                }
            }
            return _return;
        } 
        #endregion
    }
}
