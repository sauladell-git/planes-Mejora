using INET.Core.Constants;
using INET.Services;
using INET.Utils.Helpers;
using System.Web.Mvc;

namespace INET.Web.Controllers
{
    public class IncidenceController : BaseController
    {
        IncidenceService _incidenceService;

        public IncidenceController(IncidenceService incidenceService)
        {
            _incidenceService = incidenceService;
        }

        public ActionResult List()
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] { PermissionConstants.ImprovementPlan.INCIDENTS_READ, PermissionConstants.Solicitudes.INCIDENTS_READ }));
            return View(_incidenceService.ListForUser(SessionHelper.CurrentUserId));
        }

    }
}
