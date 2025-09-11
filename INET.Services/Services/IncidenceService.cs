using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using INET.Data;
using INET.Services.DTO;
using System.Web.Security;
using INET.Utils.Helpers;
using INET.Core.Constants;
using INET.Utils.Helpers.Permissions;

namespace INET.Services
{
    /// <summary>
    /// Servicio de Incidencias
    /// </summary>
    public class IncidenceService : BusinessService
    {
        UserProfileService _userProfileService;

        public IncidenceService(INETContext context, UserProfileService userProfileService)
        {
            Context = context;
            _userProfileService = userProfileService;
        }

        /// <summary>
        /// Lista los Status correspondientes a una incidencia
        /// </summary>
        /// <returns>Una lista de KeyValuePair con los estados posibles que puede tener una incidencia</returns>
        public IList<KeyValuePair<int, string>> ListStatus()
        {
            var lst = new List<KeyValuePair<int, string>>();
            lst.Add(new KeyValuePair<int, string>(1, "Abierto"));
            lst.Add(new KeyValuePair<int, string>(0, "Cerrado"));

            return lst;
        }

        /// <summary>
        /// Guarda una incidencia
        /// </summary>
        /// <param name="dto">DTO de la incidencia a salvar</param>
        public bool SaveIncidence(IncidenceDTO dto, int planId)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == planId).FirstOrDefault();

            if (plan.Id > 0)
            {
                try
                {
                    var incidence = new Incidence();

                    incidence.Active = true;
                    incidence.Date = DateTime.Now;
                    incidence.Details = dto.Details;
                    incidence.UserId = dto.UserId;
                    incidence.IncidentTypeId = dto.IncidenceTypeId;

                    if (dto.SolicitudeId.HasValue)
                        incidence.SolicitudeId = dto.SolicitudeId.Value;

                    plan.Incidences.Add(incidence);

                    Context.SaveChanges();

                } catch (Exception e)
                {
                    return false;
                }

                return true;
            } else
            {
                return false;
            }
        }

        /// <summary>
        /// Cambia el estado de una incidencia
        /// </summary>
        /// <param name="incidenceId">Id incidencia</param>
        public void Close(int incidenceId)
        {
            var incidence = Context.Incidences.Where(x => x.Id == incidenceId).First();
            incidence.Active = false;
            Context.SaveChanges();
        }

        /// <summary>
        /// Salva un comentario de una Incidencia
        /// </summary>
        /// <param name="incidenceId">Id de la incidencia en la cual se desea salvar el comentario</param>
        /// <param name="userName">Nombre de usuario del usuario que generó el comentario</param>
        /// <param name="text">Texto del comentario</param>
        public void SaveComment(int incidenceId, string userName, string text)
        {
            var incidence = Context.Incidences.Where(x => x.Id == incidenceId).First();
            if (incidence != null)
            {
                if (IncidencePermissions.CanComment(incidence))
                {
                    var comment = new Comment();
                    comment.UserId = Context.UserProfiles.Where(x => x.UserName == userName).First().UserId;
                    comment.Text = text;
                    comment.Date = DateTime.Now;

                    incidence.Comments.Add(comment);

                    Context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Obtiene una incidencia
        /// </summary>
        /// <param name="incidenceId">Id de la incidencia</param>
        /// <returns>La incidencia correspondiente al id pasado como parámetro</returns>
        public Incidence Get(int incidenceId)
        {
            var enable_solicitudes = SessionHelper.HasPermission(PermissionConstants.Solicitudes.INCIDENTS_READ);
            var enable_plans = SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.INCIDENTS_READ);
            IEnumerable<Incidence> incidence = new List<Incidence>();
            if (enable_plans)
            {
                incidence = Context.Incidences.Where(x => x.Id == incidenceId && (enable_solicitudes || (!enable_solicitudes && x.SolicitudeId == null))).OrderBy(x => x.Date);
            }
            else
            {
                if (enable_solicitudes)
                {
                    incidence = Context.Incidences.Where(x => x.Id == incidenceId && x.SolicitudeId != null).OrderBy(x => x.Date);
                }
            }
            return incidence.First();
        }

        /// <summary>
        /// Lista todas las incidencias de un usuario
        /// </summary>
        /// <param name="userId">ID de usuario del usuario del que se desean listar las incidencias</param>
        /// <returns>Un listado con todas las incidencias del usuario</returns>
        public IList<Incidence> ListForUser(int userId)
        {
            var enable_solicitudes = SessionHelper.HasPermission(PermissionConstants.Solicitudes.INCIDENTS_READ);
            var enable_plans = SessionHelper.HasPermission(PermissionConstants.ImprovementPlan.INCIDENTS_READ);

            var user_provinces = SessionHelper.CurrentUserProfile.Provinces.Select(y => y.Number);
            var user_fields = SessionHelper.GetFields().Select(x => x.Id);
            var user_levels = SessionHelper.GetInstitutionLevels().Select(x=> x.Id);

            // solo de planes de provincia, eje y/o nivel que el usuario puede ver
            var incidences = Context.Incidences.Include("IncidenceType").Include("UserProfile")
                .Where(x => x.UserId == userId && (user_provinces.Count() == 0 || user_provinces.Contains(x.ImprovementPlan.CUE.Substring(0, 2))) && (user_fields.Count() == 0 || user_fields.Contains(x.ImprovementPlan.FieldId))
                    && (x.ImprovementPlan.InstitutionLevelInt.HasValue == false || (user_levels.Count() == 0 || user_levels.Contains(x.ImprovementPlan.InstitutionLevelInt.Value))));
            if (enable_plans)
            {
                incidences = incidences.Where(x => enable_solicitudes || (!enable_solicitudes && x.SolicitudeId == null));
            }
            else
            {
                if (enable_solicitudes)
                {
                    incidences = incidences.Where(x => x.SolicitudeId != null);
                }
            }
            return incidences.OrderBy(x => x.Date).ToList();
        }

        public IList<Incidence> ListForPlan(int planId)
        {
            var incidences = Context.Incidences.Include("IncidenceType").Include("UserProfile").Include("Comments")
                .Where(x => x.ImprovementPlanId == planId && x.SolicitudeId == null);
            return incidences.ToList();
        }
    }
}