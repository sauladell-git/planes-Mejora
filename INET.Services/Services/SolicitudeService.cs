using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using INET.Core.Enums;
using INET.Data;
using INET.Services.DTO;
using INET.Utils.Helpers;
using System.Web.Security;
using INET.Core.Constants;
using INET.Utils.Helpers.Permissions;
using System.Data.Entity.SqlServer;
using ClosedXML.Excel;

namespace INET.Services
{
    /// <summary>
    /// Servicio de Solicitudes
    /// </summary>
    public class SolicitudeService : BusinessService
    {
        private readonly ImportService _importService;
        private ImprovementPlanService _improvementPLanService;

        public SolicitudeService(INETContext context, ImportService importService)
        {
            Context = context;
            _importService = importService;
        }

        /// <summary>
        /// Do solicitudes count for plan
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public int countSolicitudesFor(int planId)
        {
            return Context.Solicitudes.Where(x => x.ImprovementPlanId == planId).Count();
        }

        /// <summary>
        /// Obtiene un solicitado
        /// </summary>
        /// <param name="id">Id del solicitado a obtener</param>
        /// <returns>El solicitado que corresponde al id pasado como parámetro.</returns>
        public Solicitude Get(int id)
        {
            return Context.Solicitudes.Include("ImprovementPlan").Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Lista las especializaciones
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <param name="solicitudeId">Id del solicitado</param>
        /// <returns>Una colección de KeyValuePair formateada en base a las especializaciones que devuelve el WebService</returns>
        public List<KeyValuePair<string, string>> ListSpecializations(int improvementPlanId, int solicitudeId)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
            var cue = (plan.ImprovementPlanTypeId == ImprovementPlanTypeEnum.Institucional.ToInt())
                            ? plan.CUE
                            : Context.Solicitudes.Where(x => x.Id == solicitudeId).First().CUE;

            return ListSpecializationsByCue(improvementPlanId, cue);
        }

        /// <summary>
        /// Lista la Gestion
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <param name="solicitudeId">Id del solicitado</param>
        /// <returns>Devuelve el Ambito de Gestion  que devuelve el WebService</returns>
        public string ListManagement(int improvementPlanId, int solicitudeId)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
            var cue = (plan.ImprovementPlanTypeId == ImprovementPlanTypeEnum.Institucional.ToInt())
                            ? plan.CUE
                            : Context.Solicitudes.Where(x => x.Id == solicitudeId).First().CUE;

            return ListManagementByCue(improvementPlanId, cue);
        }

        /// <summary>
        /// Lista las especializaciones correspondientes a un determinado CUE
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <param name="cue">CUE del cual se desean listar las especializaciones</param>
        /// <returns>Una colección de KeyValuePair formateada en base a las especializaciones que devuelve el WebService para ese CUE</returns>
        public List<KeyValuePair<string, string>> ListSpecializationsByCue(int improvementPlanId, string cue)
        {
            if (IsValidCUE(improvementPlanId, cue))
            {
                var service = new wsINET.ConsultaRegistro();
                var data = service.obtenerDatosInstitucion(cue.Trim());
                if (data != null)
                    return ListSpecializations(data.especializaciones);
                else
                    return ListSpecializations("");
            }
            // SIA devuelve siempre "Todos" cuando no encuentra Cue Valido
            return ListSpecializations("");
            return new List<KeyValuePair<string, string>>();
        }


        /// <summary>
        /// Lista  la gestion de  un determinado CUE
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <param name="cue">CUE del cual se desean listar las especializaciones</param>
        /// <returns>Un String que deuvelve la gestion de ese CUE</returns>
        public string ListManagementByCue(int improvementPlanId, string cue)
        {
            if (IsValidCUE(improvementPlanId, cue))
            {
                var service = new wsINET.ConsultaRegistro();
                var data = service.obtenerDatosInstitucion(cue.Trim());
                if (data != null)
                    return data.ambito.Substring(0, 1);
                else
                    return "";
            }
            return "";
            // SIA devuelve siempre "Todos" cuando no encuentra Cue Valido
            //   return ListSpecializations("");

        }

        /// <summary>
        /// Graba un solicitado de un plan de mejora
        /// </summary>
        /// <param name="dto">DTO del Solicitado a agregar</param>
        public bool Save(SolicitudeDTO dto)
        {
            var result = false;

            BeginAuditLog();

            var solicitude = (dto.SolicitudeId == 0)
                                ? new Solicitude()
                                : Context.Solicitudes.Where(x => x.Id == dto.SolicitudeId).First();

            var plan = Context.ImprovementPlans.Where(x => x.Id == dto.ImprovementPlanId).First();

            int inDictums = solicitude.Dictums.Where(x => x.StatusId != DictumStatusEnum.Anulado.ToInt()).Count();
            if (inDictums != 0)
            {
                throw new ApplicationException("El solicitado se encuentra ya en documentos, no puede ser actualizado.");
            }

            var availableStatusCount = SolicitudePermissions.ListAvailableStatuses(solicitude.StatusId, plan.StatusId)
                                            .Where(x => x.Id == dto.StatusId);
            if (availableStatusCount.Count() < 1)
                throw new ApplicationException("El estado indicado no se encuentra disponible.");

            if (!string.IsNullOrWhiteSpace(dto.FileNumber) && (Context.Solicitudes.Where(x => x.ImprovementPlanId != plan.Id && x.FileNumber == dto.FileNumber).Any()
                ))
                throw new ApplicationException("No es posible asignar ese numero de expediente debido a que ya sido utilizado.");

            if (!IsValidCUE(dto.ImprovementPlanId, dto.CUE))
                throw new ApplicationException("El CUE indicado no es válildo para el tipo de plan indicado.");

            if (!IsValidCUELine(dto.ImprovementPlanId, dto.CUE, dto.LineId))
            {
                throw new ApplicationException("La línea indicada no es válilda para el tipo de plan indicado.");
            }

            if (solicitude.Id == 0)
            {
                solicitude.SchoolYearId = plan.SchoolYearId;
                //Eje-Linea-SubEje-SubLinea del plan 
                solicitude.Line_22_Id = plan.Line_22_Id;
                solicitude.Field_Id = plan.FieldId;
                solicitude.SubField_Id = plan.SubFieldId;
                solicitude.LineId = plan.LineId;
            }

            var prevStatus = solicitude.StatusId;
            solicitude.StatusId = dto.StatusId;


            if (prevStatus != (int)SolicitudeStatusEnum.EnProceso || prevStatus != (int)SolicitudeStatusEnum.Pendiente)
            {
                solicitude.CUE = dto.CUE.Trim();
                solicitude.Management = dto.Management;
                solicitude.Details = dto.Details.Trim();
                solicitude.LineId = dto.LineId;
                //Eje-Linea-SubEje-SubLinea del plan  (para updates de solicitados anteriores a los cambios...)
                solicitude.Field_Id = plan.FieldId;
                solicitude.SubField_Id = plan.SubFieldId;
                solicitude.Line_22_Id = plan.Line_22_Id;
                solicitude.LineId = plan.LineId;
                //if (dto.Line_22_Id != 0)
                //{
                //    var dto_22 = this.getLine22DTO(dto.Line_22_Id);
                //    solicitude.Field_Id = dto_22.FieldId;
                //    solicitude.SubField_Id = dto_22.SubFieldId;
                //    solicitude.Line_22_Id = dto.Line_22_Id;
                //}
                solicitude.MeasurementUnitId = dto.MeasurementUnitId;
                solicitude.ExpenditureTypeId = dto.ExpenditureTypeId;
                if (dto.ExpenditureObjectTypeId != 0)
                    solicitude.ExpenditureObjectTypeId = dto.ExpenditureObjectTypeId;
                else
                    solicitude.ExpenditureObjectTypeId = null;

                solicitude.Specialization = dto.Specialization;
                solicitude.SolicitudeTypeId = dto.SolicitudeTypeId;
                solicitude.FileNumber = String.IsNullOrWhiteSpace(dto.FileNumber) || String.IsNullOrEmpty(dto.FileNumber) ? null : dto.FileNumber;

                solicitude.RequestedAmount = dto.RequestedAmount.Value;
                solicitude.RequestedPriceUnit = Math.Round(Convert.ToDecimal(dto.RequestedPriceUnit), 2);

                solicitude.Locked = false;
                solicitude.Management = "";
                // si tiene CUE y no es jurisdiccional
                if (!String.IsNullOrEmpty(solicitude.CUE) && !solicitude.CUE.Substring(2).Equals("0000000"))
                {
                    InstitutionDTO institution = GetInstitutionData(solicitude.CUE);
                    if (institution != null)
                    {
                        solicitude.Level = institution.Level;
                        solicitude.Management = institution.Ambit.Substring(0, 1);
                    }


                }

                if (!string.IsNullOrWhiteSpace(dto.Reassigned) && dto.ReassignedId.HasValue)
                {
                    CalculateReassignedAmmount(solicitude, dto.ReassignedId.Value);
                }


                if (prevStatus != solicitude.StatusId && (prevStatus == (int)SolicitudeStatusEnum.Aprobado)) // Si viene de cambiar un aprobado, borrar los numeros de aprobado!
                {
                    solicitude.ApprovedAmount = null;
                    solicitude.ApprovedPriceUnit = null;


                }
            }



            if (dto.SolicitudeId == 0)
                plan.Solicitudes.Add(solicitude);

            if (solicitude.StatusId == (int)SolicitudeStatusEnum.Aprobado && prevStatus != (int)SolicitudeStatusEnum.Aprobado)
            {
                solicitude.ApprovedAmount = dto.ApprovedAmount;
                solicitude.ApprovedPriceUnit = Math.Round(Convert.ToDecimal(dto.ApprovedPriceUnit), 2);
            }

            try
            {
                Context.SaveChanges();
                EndAuditLog(dto.ImprovementPlanId);

                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Elimina un solicitado
        /// </summary>
        /// <param name="id">Id del solicitado que se desea eliminar</param>        
        public void Delete(int id)
        {
            var solicitude = Context.Solicitudes.Where(x => x.Id == id).First();

            foreach (var incidene in solicitude.Incidences)
                Context.Comments.RemoveRange(incidene.Comments);

            // Si el solicitado estaba tomando fondos de otro solicitado, entonces los liberio
            ReleaseReassignedAmmount(solicitude);

            Context.Incidences.RemoveRange(solicitude.Incidences);
            Context.Comments.RemoveRange(solicitude.Comments);
            Context.Solicitudes.Remove(solicitude);

            BeginAuditLog();
            Context.SaveChanges();
            EndAuditLog(solicitude.ImprovementPlanId);
        }

        /// <summary>
        /// Libera el monto reasignado de otro solicitado
        /// </summary>
        /// <param name="solicitude">Solicitado del cual se desea liberar el monto reasignado (en caso de que tuviera)</param>
        private void ReleaseReassignedAmmount(Solicitude solicitude)
        {
            if (solicitude.ReassignedId.HasValue)
            {
                var reassignedSolicitude = Context.Solicitudes.Where(x => x.Id == solicitude.ReassignedId).First();
                reassignedSolicitude.ReassignedGrantedTotal -= solicitude.ReassignedRequestedTotal;
            }
        }

        /// <summary>
        /// Calcula el monto reasignado de un solicitado
        /// </summary>
        /// <param name="solicitude">Solicitado</param>
        private void CalculateReassignedAmmount(Solicitude solicitude, int reassignedId)
        {
            // Valido que ese solicitado del cual reasigno los fondos, tenga fondos suficientes
            var reassignedSolicitude = Context.Solicitudes.Where(x => x.Id == reassignedId).First();
            var reassignedAmmount = (solicitude.ApprovedTotal.HasValue) ? solicitude.ApprovedTotal.Value : solicitude.RequestedTotal;

            // Si el usuario cambia el solicitado del cual está reasignando, debo dejar los montons de reasignación como estaban originalmente                
            if (solicitude.ReassignedId.HasValue && solicitude.ReassignedId.Value != reassignedId)
            {
                var originalReassignedSolicitude = Context.Solicitudes.Where(x => x.Id == solicitude.ReassignedId).First();
                originalReassignedSolicitude.ReassignedGrantedTotal -= solicitude.ReassignedRequestedTotal;
                solicitude.ReassignedRequestedTotal = 0;
            }

            if (reassignedSolicitude.AvailableTotal < solicitude.RequestedTotal)
                throw new ApplicationException("El total disponible del cual se desea reasignar el solicitado es menor al monto solicitado");

            if (solicitude.ApprovedTotal.HasValue && reassignedSolicitude.AvailableTotal < solicitude.ApprovedTotal.Value)
                throw new ApplicationException("El total disponible del cual se desea reasignar el solicitado es menor al monto aprobado");

            // Asigno solicitado del cual deseo tomar los fondos
            solicitude.ReassignedId = reassignedId;
            // Registro en el solicitado cuanto fue lo que pedí para reasignar
            solicitude.ReassignedRequestedTotal += reassignedAmmount;
            // Registro en el solicitado del cual estoy reasignando el dinero, cuanto se ha tomado del mismo
            reassignedSolicitude.ReassignedGrantedTotal += reassignedAmmount;
        }

        /// <summary>
        /// Anula un solicitado
        /// </summary>
        /// <param name="id">Id del solicitado que se desea anular</param>     
        /// <returns>True en caso de que se haya anulado el solicitado. False en caso contrario.</returns>
        public bool Block(int id)
        {
            var solicitude = Context.Solicitudes.Where(x => x.Id == id).First();
            BeginAuditLog();
            solicitude.StatusId = SolicitudeStatusEnum.Anulado.ToInt();
            // Si la solicitud estaba tomando fondos de otro solicitado, entonces los libero al haber sido anulada
            ReleaseReassignedAmmount(solicitude);
            Context.SaveChanges();
            EndAuditLog(solicitude.ImprovementPlanId);
            return true;
        }

        /// <summary>
        /// Rechaza un solicitado
        /// </summary>
        /// <param name="id">Id del solicitado que se desea rechazar</param>     
        /// <returns>True en caso de que se haya rechazado el solicitado. False en caso contrario.</returns>
        public bool Reject(int id)
        {
            var solicitude = Context.Solicitudes.Where(x => x.Id == id).First();

            var availableStatusCount = SolicitudePermissions.ListAvailableStatuses(solicitude.StatusId, solicitude.ImprovementPlan.StatusId)
                                            .Where(x => x.Id == SolicitudeStatusEnum.EnProceso.ToInt()).Any();

            if (
                solicitude.CUE.Trim().Equals("")
                || solicitude.FileNumber.Trim().Equals("")
                || solicitude.SchoolYearId <= 0
                || solicitude.Details.Trim().Equals("")
                || solicitude.LineId <= 0
                || solicitude.ExpenditureTypeId <= 0
                || solicitude.Specialization.Trim().Equals("")
                || solicitude.SpecializationCode.Trim().Equals("")
                || solicitude.SolicitudeTypeId <= 0
                || solicitude.MeasurementUnitId <= 0
                || solicitude.RequestedAmount <= 0
                || solicitude.RequestedPriceUnit <= 0
                || solicitude.RequestedTotal <= 1
                )
            {

                return false;
            }
            else if (SolicitudePermissions.CanEdit(solicitude) && availableStatusCount)
            {
                BeginAuditLog();
                solicitude.StatusId = SolicitudeStatusEnum.Rechazado.ToInt();
                solicitude.ApprovedAmount = null;
                solicitude.ApprovedPriceUnit = null;

                // Si la solicitud estaba tomando fondos de otro solicitado, entonces los libero al haber sido anulada
                ReleaseReassignedAmmount(solicitude);
                Context.SaveChanges();
                EndAuditLog(solicitude.ImprovementPlanId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Actualiza a estado en proceso un solicitado
        /// </summary>
        /// <param name="id">Id del solicitado que se desea aprobar</param>     
        /// <returns>True en caso de que se haya cambiado el estado del solicitado. False en caso contrario.</returns>
        public bool InProcess(int id)
        {
            var soli = Context.Solicitudes.Where(x => x.Id == id).FirstOrDefault();
            var availableStatusCount = SolicitudePermissions.ListAvailableStatuses(soli.StatusId, soli.ImprovementPlan.StatusId)
                                            .Where(x => x.Id == SolicitudeStatusEnum.EnProceso.ToInt()).Any();
            if (soli.Id != 0 && SolicitudePermissions.CanEdit(soli) && availableStatusCount)
            {
                soli.StatusId = SolicitudeStatusEnum.EnProceso.ToInt();
                if (Context.SaveChanges() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// Aprueba un solicitado
        /// </summary>
        /// <param name="id">Id del solicitado que se desea aprobar</param>     
        /// <returns>True en caso de que se haya aprobado el solicitado. False en caso contrario.</returns>
        public bool Approve(int id)
        {
            var solicitude = Context.Solicitudes.Where(x => x.Id == id).FirstOrDefault();
            if (!solicitude.ApprovedAmount.HasValue) solicitude.ApprovedAmount = solicitude.RequestedAmount;
            if (!solicitude.ApprovedPriceUnit.HasValue) solicitude.ApprovedPriceUnit = solicitude.RequestedPriceUnit;

            var availableStatusCount = SolicitudePermissions.ListAvailableStatuses(solicitude.StatusId, solicitude.ImprovementPlan.StatusId)
                                            .Where(x => x.Id == SolicitudeStatusEnum.EnProceso.ToInt()).Any();

            if (solicitude.CUE.Trim() == ""
                || solicitude.SchoolYearId <= 0
                || solicitude.Details.Trim() == ""
                || solicitude.LineId <= 0
                || (!solicitude.ExpenditureTypeId.HasValue || solicitude.ExpenditureTypeId.Value <= 0)
                || (solicitude.Specialization == null || solicitude.Specialization.Trim().Equals(""))
                || (solicitude.SpecializationCode == null || solicitude.SpecializationCode.Trim().Equals(""))
                || (!solicitude.SolicitudeTypeId.HasValue || solicitude.SolicitudeTypeId.Value <= 0)
                || solicitude.MeasurementUnitId <= 0
                || solicitude.RequestedAmount <= 0
                || (!solicitude.RequestedPriceUnit.HasValue || solicitude.RequestedPriceUnit.Value <= 0)
                || solicitude.RequestedTotal <= 1
                || (!solicitude.ApprovedAmount.HasValue || solicitude.ApprovedAmount.Value <= 0)
                || (!solicitude.ApprovedPriceUnit.HasValue || solicitude.ApprovedPriceUnit.Value <= 0)
                )
            {
                return false;
            }
            else if (SolicitudePermissions.CanEdit(solicitude) && availableStatusCount)
            {
                BeginAuditLog();
                solicitude.StatusId = SolicitudeStatusEnum.Aprobado.ToInt();
                Context.SaveChanges();
                EndAuditLog(solicitude.ImprovementPlanId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Lista los solicitados de un plan de mejora
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <returns>Una lista con los solicitados pertenecientes a ese plan de mejora</returns>
        public IList<Solicitude> List(int improvementPlanId)
        {
            return Context.ImprovementPlans
                .Include("Solicitudes.Line")
                .Include("Solicitudes.Status")
                .Include("Solicitudes.ExpenditureType")
                .Include("Solicitudes.SolicitudeType")
                .Where(x => x.Id == improvementPlanId).First().Solicitudes.OrderByDescending(x => x.Id).ToList();
        }

        /// <summary>
        /// Busca solicitados de acuerdo a los filtros seleccionados
        /// </summary>
        /// <param name="dto">DTO con los filtros a aplicar</param>
        /// <returns>Una colección solicitados que cumple con los filtros requeridos</returns>
        public SolicitudeResultDTO Search(DataTableDTO dto, bool includeCueCol)
        {
            var pre_query = from solicitude in Context.Solicitudes
                            select new
                            {
                                sol = solicitude,
                                RequestedTotal = solicitude.RequestedAmount > 0 && solicitude.RequestedPriceUnit.HasValue ? Math.Round((solicitude.RequestedAmount.Value * solicitude.RequestedPriceUnit.Value), 2) : 0,
                                ApprovedTotal = solicitude.ApprovedAmount.HasValue && solicitude.ApprovedPriceUnit.HasValue ? Math.Round((solicitude.ApprovedAmount.Value * solicitude.ApprovedPriceUnit.Value), 2) : 0,
                            };

            if (dto.iCustomSearch_ImprovementPlanId.HasValue && dto.iCustomSearch_ImprovementPlanId.Value > 0)
            {
                pre_query = pre_query.Where(x => x.sol.ImprovementPlanId == dto.iCustomSearch_ImprovementPlanId.Value);
            }

            if (!String.IsNullOrEmpty(dto.sSearch))
            {
                pre_query = pre_query.Where(x => x.sol.Details.Contains(dto.sSearch.Trim())
                || SqlFunctions.StringConvert((double)x.sol.Id).Contains(dto.sSearch.Trim())
                || x.sol.FileNumber.Contains(dto.sSearch.Trim())
                || x.sol.Status.Description.Contains(dto.sSearch.Trim())
                );
            }

            var totalRecords = pre_query.Count();

            var iSortCol_0 = !includeCueCol && dto.iSortCol_0 > 2 ? dto.iSortCol_0 + 1 : dto.iSortCol_0;

            switch (iSortCol_0)
            {
                case 2:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.Id) : pre_query.OrderByDescending(x => x.sol.Id);
                    break;
                case 3:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.CUE) : pre_query.OrderByDescending(x => x.sol.CUE);
                    break;
                case 4:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.Line.Description) : pre_query.OrderByDescending(x => x.sol.Line.Description);
                    break;
                case 5:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.FileNumber) : pre_query.OrderByDescending(x => x.sol.FileNumber);
                    break;
                case 6:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.Status.Description) : pre_query.OrderByDescending(x => x.sol.Status.Description);
                    break;
                case 7:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.Details) : pre_query.OrderByDescending(x => x.sol.Details);
                    break;
                case 8:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.ReassignedId) : pre_query.OrderByDescending(x => x.sol.ReassignedId);
                    break;
                case 9:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.Comments.Count) : pre_query.OrderByDescending(x => x.sol.Comments.Count);
                    break;
                case 10:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.Incidences.Count) : pre_query.OrderByDescending(x => x.sol.Incidences.Count);
                    break;
                case 11:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.RequestedTotal) : pre_query.OrderByDescending(x => x.RequestedTotal);
                    break;
                case 12:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.ApprovedTotal) : pre_query.OrderByDescending(x => x.ApprovedTotal);
                    break;
                default:
                    pre_query = dto.sSortDir_0 == "asc" ? pre_query.OrderBy(x => x.sol.Id) : pre_query.OrderByDescending(x => x.sol.Id);
                    break;
            }

            // Armo el resultado
            var result = new SolicitudeResultDTO();
            result.Echo = dto.sEcho;
            result.TotalRecords = totalRecords;
            result.TotalDisplayRecords = (dto.FilterResults) ? pre_query.Count() : totalRecords;
            result.FilteredSolicitudes = pre_query.Select(x => x.sol).Skip(dto.iDisplayStart).Take(dto.iDisplayLength).ToList();
            return result;

        }

        /// <summary>
        /// Lista los solicitados de un plan de mejora de actuerdo al tipo de dictamen
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <param name="templateTypeId">Id del tipo de plantilla</param>
        /// <returns>Una lista con los solicitados pertenecientes al plan de mejora que pueden aplicarse a un tipo dictamen determinado</returns>
        public IList<Solicitude> List(int improvementPlanId, int templateTypeId, string fileNumber)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
            switch (templateTypeId)
            {
                case (int)TemplateTypeEnum.Dictamen:
                    return plan.Solicitudes.Where(x => (x.StatusId == (int)SolicitudeStatusEnum.Aprobado || x.StatusId == SolicitudeStatusEnum.Rechazado.ToInt())
                                                        && (!String.IsNullOrEmpty(fileNumber) ? x.FileNumber == fileNumber : true) && !x.Locked).OrderBy(x => x.StatusId).ToList();

                case (int)TemplateTypeEnum.DictamenDeEligibilidad:
                    return plan.Solicitudes.Where(x => x.StatusId == (int)SolicitudeStatusEnum.Elegible && (!String.IsNullOrEmpty(fileNumber) ? x.FileNumber == fileNumber : true) && !x.Locked).ToList();

                default:
                    return new List<Solicitude>();
            }
        }

        /// <summary>
        /// Determina si se puede o no borrar un solicitado dependiendo si tiene o no incidencias abiertas asociadas
        /// y si el estado del solicitado no sea aprobado.
        /// </summary>
        /// <param name="id">Id del solicitado que se desea borrar</param>
        /// <returns>True en caso de que se pueda borrar el solicitado. False en caso contrario.</returns>
        public bool CanDeleteSolicitude(int id)
        {
            var solicitude = Context.Solicitudes.Where(x => x.Id == id).First();

            if (!SolicitudePermissions.CanDelete(solicitude))
                return false;

            var incidences = Context.Incidences.Where(x => x.SolicitudeId == id).ToList();
            bool openInc = false;
            foreach (Incidence inc in incidences)
            {
                if (inc.Active)
                    openInc = true;
            }

            if ((solicitude.StatusId == SolicitudeStatusEnum.Aprobado.ToInt()) || openInc)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Salva un comentario de un Solicitado
        /// </summary>
        /// <param name="solicitudeId">Id del solicitado del cual se desea salvar un comentario</param>
        /// <param name="includeInDictum">Determina si el comentario se debe incluir en el dictamen como observación</param>
        /// <param name="userName">Nombre del usuario que generó el comentario</param>        
        /// <param name="text">Texto del comentario</param>
        public void SaveComment(int solicitudeId, bool includeInDictum, string userName, string text)
        {
            var comment = new Comment();
            comment.UserId = Context.UserProfiles.Where(x => x.UserName == userName).First().UserId;
            comment.IncludeInDictum = includeInDictum;
            comment.Text = text;
            comment.Date = DateTime.Now;

            var solicitude = Context.Solicitudes.Where(x => x.Id == solicitudeId).First();
            solicitude.Comments.Add(comment);

            Context.SaveChanges();
        }

        /// <summary>
        /// Determina si es válido un cue para un solicitado
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <param name="cue">CUE del solicitado</param>
        /// <returns>True en caso de que sea válido asignar ese cue a ese solicitado. False en caso contrario.</returns>
        public bool IsValidCUE(int improvementPlanId, string cue)
        {  
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
            var type = (ImprovementPlanTypeEnum)plan.ImprovementPlanTypeId;
            if (cue.Length == 8)
                cue = "0" + cue;
            var _cue_data = GetInstitutionData(cue);

            switch (type)
            {
                case ImprovementPlanTypeEnum.Nacional:
                    // El cue puede ser institucional y no puede ser de dependencia nacional
                    if (_cue_data.Dependence == null) return false;
                    return IsValidCUE(cue) && !_cue_data.Dependence.Contains("Nacional");

                case ImprovementPlanTypeEnum.Jurisdiccional:
                    // El cue debe ser el del plan o de la misma provincia y debe ser válido y no puede ser de dependencia nacional
                    //SIA cambios, no conectado con RFIETP
                    // if (_cue_data.Dependence == null) return false;
                    // return plan.CUE.Trim() == cue.Trim() || (cue.Length == 9 && cue.Trim().Substring(0, 2) == plan.CUE.Substring(0, 2) && IsValidCUE(cue)) && !_cue_data.Dependence.Contains("Nacional");
                    //SIA cambios, no conectado con RFIETP
                    return plan.CUE.Trim() == cue.Trim() || (cue.Length == 9 && cue.Trim().Substring(0, 2) == plan.CUE.Substring(0, 2));
                case ImprovementPlanTypeEnum.Institucional:
                    // El cue debe ser el mismo que el del plan (que debería ser válido)
                    return cue.Trim() == plan.CUE && IsValidCUE(cue);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determina si es válido un cue para un solicitado
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <param name="cue">CUE del solicitado</param>
        /// <param name="solicitudeLineId"></param>
        /// <returns>True en caso de que sea válido asignar ese cue a ese solicitado. False en caso contrario.</returns>
        public bool IsValidCUELine(int improvementPlanId, string cue, int solicitudeLineId)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
            var type = (ImprovementPlanTypeEnum)plan.ImprovementPlanTypeId;

            switch (type)
            {
                case ImprovementPlanTypeEnum.Nacional:
                    return true;
                case ImprovementPlanTypeEnum.Jurisdiccional:
                    return true;
                case ImprovementPlanTypeEnum.Institucional:
                    // Si el cue es de dependencia nacional la linea debe ser la correcta
                    var _cue_data = GetInstitutionData(cue);
                    var _lines = GetValidLinesForNationalDependentPlans();
                    return (!_cue_data.Dependence.Contains("Nacional") || (_cue_data.Dependence.Contains("Nacional") && _lines.Contains(solicitudeLineId)));
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determina si un cue es válido de acuerdo a lo que devuelve el servicio
        /// </summary>
        /// <param name="cue">CUE a validar</param>
        /// <returns>True en caso de que sea válido. False en caso contrario.</returns>
        private bool IsValidCUE(string cue)
        {
            var service = new wsINET.ConsultaRegistro();
            if (!string.IsNullOrEmpty(cue) && cue.Length == 9)
            {
                try
                {
                    var data = service.obtenerDatosInstitucion(cue.Trim());
                    return data.cue != "0";
                }
                catch (Exception)
                {
                }
            }

            return false;
        }

        /// <summary>
        /// Obtiene el número de expediente para un solicitado determinado de un plan de mejora
        /// </summary>
        /// <param name="improvementPlanId">Id del Plan de Mejora</param>
        /// <param name="lineId">Id de la línea</param>
        /// <returns>El número de expediente correspondiente a la línea de ese plan de mejora</returns>
        public string GetFileNumberForSolicitude(int improvementPlanId, int lineId)
        {
            var solicitudes = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId)
                                                        .SelectMany(x => x.Solicitudes)
                                                        .Where(x => x.LineId == lineId && !string.IsNullOrEmpty(x.FileNumber))
                                                        .ToList();
            if (solicitudes.Count > 0)
                return solicitudes.First().FileNumber;

            return null;
        }


        /// <summary>
        /// Obtiene el número de expediente para un solicitado determinado de un plan de mejora, solo por id ya que 2022 solo hay una linea por plan
        /// </summary>
        /// <param name="improvementPlanId">Id del Plan de Mejora</param>
        /// <param name="lineId">Id de la línea</param>
        /// <returns>El número de expediente correspondiente a la línea de ese plan de mejora</returns>
        public string GetFileNumberForSolicitude(int improvementPlanId)
        {
            var solicitudes = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId)
                                                        .SelectMany(x => x.Solicitudes)
                                                        .Where(x => !string.IsNullOrEmpty(x.FileNumber))
                                                        .ToList();
            if (solicitudes.Count > 0)
                return solicitudes.First().FileNumber;

            return null;
        }


        /// <summary>
        /// Lista todos los solicitados que pueden reasignarse
        /// </summary>
        /// <param name="improvementPlanId">Id del plan de mejora en el cual se desean reasignar los solicitados</param>
        /// <param name="cue">CUE del solicitado</param>
        /// <returns>Un listado con todos los solicitados que se pueden reasignar</returns>
        public IList<ReassignDTO> ListSolicitudesToReassign(string cue, int improvementPlanId)
        {
            var lst = new List<ReassignDTO>();
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
            var validSolicitudes = plan.Documents.OfType<Resolution>()
                                    .Where(x => x.StatusId == (int)ResolutionStatusEnum.Firmado || x.StatusId == (int)ResolutionStatusEnum.Protocolizado)
                                    .SelectMany(x => x.Dictums)
                                    .SelectMany(x => x.Solicitudes)
                                    .ToList();

            // Recorro todos los solicitados incluidos en resoluciones firmadas
            foreach (var solicitude in validSolicitudes)
            {
                // Verifico que todavía pueda reasignar plata de ese solicitado                
                if (solicitude.ReassignedGrantedTotal < solicitude.ApprovedTotal)
                {
                    // Obtengo las resoluciones en las que se usó ese solicitado
                    var resolutionIdentifier = "";
                    var resolutions = solicitude.Dictums.SelectMany(x => x.Resolutions).ToList();
                    foreach (var resolution in resolutions)
                        resolutionIdentifier = (resolutions.Count == 0)
                                                ? resolution.Number.ToString()
                                                : (resolutions.Last() != resolution) ? resolution.Number.ToString() + ", " : resolution.Number.ToString();

                    // Lo agrego a la lista
                    var reasigned = solicitude.ReassignedGrantedTotal.HasValue ? solicitude.ReassignedGrantedTotal.Value : 0;
                    lst.Add(
                        new ReassignDTO()
                        {
                            Id = solicitude.Id,
                            Details = solicitude.Details,
                            RequestedTotal = solicitude.RequestedTotal,
                            ApprovedTotal = solicitude.ApprovedTotal.Value,
                            Resolution = resolutionIdentifier,
                            ReassignedTotal = solicitude.ReassignedGrantedTotal.HasValue ? solicitude.ReassignedGrantedTotal.Value : 0,
                            AvailableTotal = solicitude.AvailableTotal,
                            RequestedTotalAsCurrency = solicitude.RequestedTotal.ToString("C"),
                            ApprovedTotalAsCurrency = solicitude.ApprovedTotal.Value.ToString("C"),
                            ReassignedTotalAsCurrency = reasigned.ToString("C"),
                            AvailableTotalAsCurrency = solicitude.AvailableTotal.ToString("C")
                        });
                }
            }

            return lst;
        }

        /// <summary>
        /// Importa una lista de solicitados a un plan de mejora
        /// </summary>
        /// <param name="stream">Archivo con los solicitados que se desean importar</param>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <returns>Una lista con los errores encontrados</returns>
        public string ImportSolicitudes(Stream stream, int improvementPlanId, string extension)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();

            try
            {
                var solicitudes = new List<Solicitude>();
                if (plan.Pronafe==null) //false!
                {
                 solicitudes = _importService.ImportSolicitudes(stream, plan.Line_22_Id.Value, plan.SchoolYearId, plan.StatusId, extension);

                }
                else
                {

              
                if (plan.Pronafe.Value == true)
                    solicitudes = _importService.ImportSolicitudes(stream, plan.FieldId, plan.LineId.Value, plan.SchoolYearId, plan.StatusId, extension);
                else
                    solicitudes = _importService.ImportSolicitudes(stream, plan.Line_22_Id.Value, plan.SchoolYearId, plan.StatusId, extension);
                }
                var fileNumbers = new Dictionary<int, string>();
                foreach (var solicitude in solicitudes)
                {
                    if (IsValidCUE(plan.Id, solicitude.CUE))
                    {
                        // recuperar nro de expediente de solicitados existentes
                        solicitude.FileNumber = "";
                        if (solicitude.LineId.HasValue)
                        {
                            if (!fileNumbers.ContainsKey(solicitude.LineId.Value))
                            {
                                var fileNumber = Context.Solicitudes.Where(x => x.LineId == solicitude.LineId && x.ImprovementPlanId == improvementPlanId && x.FileNumber != "").Select(x => x.FileNumber).FirstOrDefault();
                                fileNumbers.Add(solicitude.LineId.Value, fileNumber);
                                solicitude.FileNumber = fileNumber;
                            }
                            else
                            {
                                solicitude.FileNumber = fileNumbers.Where(x => x.Key == solicitude.LineId).FirstOrDefault().Value;
                            }
                        }
                        plan.Solicitudes.Add(solicitude);
                    }
                }

                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(plan.Id);
                if (_importService.flag_cue_vacio)
                { throw new Exception("Archivo importado pero existe la posibilidad de  errores"); }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }


        /// <summary>
        /// Importa una lista de solicitados a un plan de mejora, trae tambien los relacionados en un DTO!
        /// </summary>
        /// <param name="stream">Archivo con los solicitados que se desean importar</param>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <returns>Una lista con los errores encontrados</returns>
        public ImportCompleteDTO ImportSolicitudes_V2(Stream stream, int improvementPlanId, string extension)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
            ImportCompleteDTO import_dto = new ImportCompleteDTO();

            try
            {
                var solicitudes = new List<Solicitude>();
                if (plan.Pronafe.Value == true)
                    solicitudes = _importService.ImportSolicitudes(stream, plan.FieldId, plan.LineId.Value, plan.SchoolYearId, plan.StatusId, extension);
                else
                {
                    import_dto = _importService.ImportSolicitudes(stream, plan.Line_22_Id.Value, plan.SchoolYearId, plan, extension);
                    


                }
                var fileNumbers = new Dictionary<int, string>();
                foreach (var solicitude in import_dto.solicitados_plan)
                {   
                    if ( IsValidCUE(plan.Id, solicitude.CUE) || solicitude.CUE.Substring(solicitude.CUE.Length - 3)=="000"  )
                    {
                        // recuperar nro de expediente de solicitados existentes
                        solicitude.FileNumber = "";
                        if (solicitude.LineId.HasValue)
                        {
                            if (!fileNumbers.ContainsKey(solicitude.LineId.Value))
                            {
                                var fileNumber = Context.Solicitudes.Where(x => x.LineId == solicitude.LineId && x.ImprovementPlanId == improvementPlanId && x.FileNumber != "").Select(x => x.FileNumber).FirstOrDefault();
                                fileNumbers.Add(solicitude.LineId.Value, fileNumber);
                                solicitude.FileNumber = fileNumber;
                            }
                            else
                            {
                                solicitude.FileNumber = fileNumbers.Where(x => x.Key == solicitude.LineId).FirstOrDefault().Value;
                            }
                        }
                        plan.Solicitudes.Add(solicitude);
                    }
                }

                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(plan.Id);
                if (_importService.flag_cue_vacio)
                { throw new Exception("Archivo importado pero existe la posibilidad de  errores"); }
            }
            catch (Exception ex)
            {
               throw new Exception( "Error Importacion : "+ ex.Message);
            }

            return import_dto;
        }



        /// <summary>
        /// Importa una lista de solicitados a un plan de mejora , devuelve los solicitados de los planes y las lineas relacionadas para post-proceso
        /// </summary>
        /// <param name="stream">Archivo con los solicitados que se desean importar</param>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <returns>Una lista con los errores encontrados</returns>
        public string ImportSolicitudes_Completo(Stream stream,  int improvementPlanId, string extension)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();

            try
            {
                var solicitudes = new List<Solicitude>();
                if (plan.Pronafe.Value == true)
                    solicitudes = _importService.ImportSolicitudes(stream, plan.FieldId, plan.LineId.Value, plan.SchoolYearId, plan.StatusId, extension);
                else
                    solicitudes = _importService.ImportSolicitudes(stream, plan.Line_22_Id.Value, plan.SchoolYearId, plan.StatusId, extension);

                var fileNumbers = new Dictionary<int, string>();
                foreach (var solicitude in solicitudes.Where(x=>x.Line_22_Id==plan.Line_22_Id)) //solicitudes misma linea plan
                {
                    if (IsValidCUE(plan.Id, solicitude.CUE))
                    {
                        // recuperar nro de expediente de solicitados existentes
                        solicitude.FileNumber = "";
                        if (solicitude.LineId.HasValue)
                        {
                            if (!fileNumbers.ContainsKey(solicitude.LineId.Value))
                            {
                                var fileNumber = Context.Solicitudes.Where(x => x.LineId == solicitude.LineId && x.ImprovementPlanId == improvementPlanId && x.FileNumber != "").Select(x => x.FileNumber).FirstOrDefault();
                                fileNumbers.Add(solicitude.LineId.Value, fileNumber);
                                solicitude.FileNumber = fileNumber;
                            }
                            else
                            {
                                solicitude.FileNumber = fileNumbers.Where(x => x.Key == solicitude.LineId).FirstOrDefault().Value;
                            }
                        }
                        plan.Solicitudes.Add(solicitude);
                    }
                }
               
                //Grabar lineas del xls!

                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(plan.Id);
                if (_importService.flag_cue_vacio)
                { throw new Exception("Archivo importado pero existe la posibilidad de  errores"); }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }


        /// <summary>
        /// Importa una lista de solicitados a un plan de mejora, genera un plan de mejora por cada linea 
        /// </summary>
        /// <param name="stream">Archivo con los solicitados que se desean importar</param>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <returns>Una lista con los errores encontrados</returns>
        public string ImportRelatedPlanesSolicitudes(Stream stream, int improvementPlanId, string extension)
        {

            // primero importa lineas del plan principal y genera las lineas asociadas al plan!!
            Stream stream_related = stream;
            ImportCompleteDTO dto = new ImportCompleteDTO();
            try { dto= ImportSolicitudes_V2(stream, improvementPlanId, extension); }
            catch (Exception ex)
            {
                { throw new Exception("Error importando lineas del plan princial" + ex.Message); }

            }


            

            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
            Stream stream_bk = stream;
            try
            {
          
                //PASO 2  - Genera PLanes
                // PASO 3 - Importa solicitados en cada plan
               // var solicitudes = new List<Solicitude>();
              //  solicitudes = _importService.GetRelatedLines(stream_related, plan, plan.SchoolYearId, extension);
                //genera lineas relacionadas en base al xls 

                // creamos los planes 
                this.GenerateRelatedPlans(plan);
                //recargamos el plan
                plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();
                //Grabamos Solicitudes
                try { 
                    foreach (RelatedLines_22 r in plan.RelatedLines_22.Where(x=>x.Line22Id!=plan.Line_22_Id).ToList())
                    {

                        var p = Context.ImprovementPlans.Where(x => x.ParentId == improvementPlanId).ToList();
                        var plan_relacionado= p.Where(x => x.Line_22_Id == r.Line22Id).First();
                        //solicitudes = _importService.ImportSolicitudes(stream_bk,r.Line22Id.Value, plan.SchoolYearId, plan.StatusId, extension);

                        var fileNumbers = new Dictionary<int, string>();

                        var solicitudes_plan = dto.solicitados_hijos.Where(x => x.Line_22_Id == r.Line22Id.Value).ToList();
                        foreach (var solicitude in solicitudes_plan)
                        {
                            solicitude.ImprovementPlanId = plan_relacionado.Id;

                            if (IsValidCUE(plan_relacionado.Id, solicitude.CUE) || solicitude.CUE.Substring(solicitude.CUE.Length - 3) == "000")
                            {
                                // recuperar nro de expediente de solicitados existentes
                                solicitude.FileNumber = "";
                                if (solicitude.LineId.HasValue)
                                {
                                    if (!fileNumbers.ContainsKey(solicitude.LineId.Value))
                                    {
                                        var fileNumber = Context.Solicitudes.Where(x => x.LineId == solicitude.LineId && x.ImprovementPlanId == improvementPlanId && x.FileNumber != "").Select(x => x.FileNumber).FirstOrDefault();
                                        fileNumbers.Add(solicitude.LineId.Value, fileNumber);
                                        solicitude.FileNumber = fileNumber;
                                    }
                                    else
                                    {
                                        solicitude.FileNumber = fileNumbers.Where(x => x.Key == solicitude.LineId).FirstOrDefault().Value;
                                    }
                                }
                                plan_relacionado.Solicitudes.Add(solicitude);
                            }
                        }


                    }
                } catch (Exception ex)
                {
                    throw new Exception("Se crearon los planes asociados, pero existe posibilidad de errores al insert solicitados:" + ex.Message);
                }






                BeginAuditLog();
                Context.SaveChanges();
                EndAuditLog(plan.Id);
                if (_importService.flag_cue_vacio)
                { throw new Exception("Archivo importado pero existe la posibilidad de  errores"); }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        /// <summary>
        /// Importa una lista de solicitados actualizados a un plan de mejora
        /// </summary>
        /// <param name="stream">Archivo con los solicitados que se desean actualizar</param>
        /// <param name="improvementPlanId">Id del plan de mejora</param>
        /// <returns>Una lista con los errores encontrados</returns>
        public string ImportSolicitudesApprovals(Stream stream, int improvementPlanId, string extension)
        {
            var plan = Context.ImprovementPlans.Where(x => x.Id == improvementPlanId).First();

            //try
            //{
            BeginAuditLog();
            var solicitudes = _importService.ImportSolicitudesApprovals(stream, plan.FieldId, plan.StatusId, extension);
            var changed = new List<int>();
            foreach (var solicitude in solicitudes)
            {
                // solo detectamos cambios a los montos y cantidades aprobadas, si no ignoramos
                if (solicitude.ApprovedAmount <= 0 || solicitude.ApprovedPriceUnit <= 0)
                    continue;

                var original = plan.Solicitudes.Where(x => x.Id == solicitude.Id).FirstOrDefault();
                // update data
                if (original != null && original.Id > 0)
                {
                    if (SolicitudePermissions.CanEdit(original) && (original.StatusId == SolicitudeStatusEnum.EnProceso.ToInt() || original.StatusId == SolicitudeStatusEnum.Pendiente.ToInt()))
                    {
                        original.ApprovedPriceUnit = solicitude.ApprovedPriceUnit;
                        original.ApprovedAmount = solicitude.ApprovedAmount;
                        changed.Add(original.Id);
                    }
                    else
                    {
                        return "No se puede editar el solicitado indicado. El usuario no tiene permisos o el solicitado se encuentra en un estado que no permite ser modificado.";
                    }
                }
                else
                {
                    return "No se encontró el solicitado en el plan indicado.";
                }
            }
            Context.SaveChanges();
            EndAuditLog(plan.Id);

            foreach (var _solId in changed)
            {
                Approve(_solId);
            }

            //}
            //catch (Exception ex)
            //{
            //    return ex.Message;
            //}

            return null;
        }

        /// <summary>
        /// Exporta la lista de planes a un excel
        /// </summary>
        /// <returns>El stream en memoria del archivo de excel generado</returns>
        public Stream ExportSolicitudesToExcel(int improvementPlanId)
        {
            MemoryStream stream = new MemoryStream();
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var plan = Context.ImprovementPlans.Include("Field").Where(x => x.Id == improvementPlanId).FirstOrDefault();
                var ws = wb.Worksheets.Add("Solicitados");

                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(1));
                ReportHelper.ApplyHeaderTitleFormatToRow(ws.Row(2));

                ws.Cell(1, 1).Value = "PLAN";
                ws.Cell(1, 2).Value = plan.Identifier;

                ws.Cell(2, 1).Value = "EJE";
                ws.Cell(2, 2).Value = plan.Field.Code;

                // Encabezados
                var pre_table_heads = new List<String>();
                pre_table_heads.Add("SOLICITADO");
                pre_table_heads.Add("");
                pre_table_heads.Add("APROBADO");
                pre_table_heads.Add("");
                pre_table_heads.Add("");

                var table_heads = new List<String>();
                table_heads.Add("ID");
                table_heads.Add("CUE");
                table_heads.Add("EJE");
                table_heads.Add("LINEA");
                table_heads.Add("EXPEDIENTE");
                table_heads.Add("ESTADO");
                table_heads.Add("DETALLE");
                table_heads.Add("REASIGNADO");
                table_heads.Add("CANTIDAD");
                table_heads.Add("PRECIO UNITARIO");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("CANTIDAD");
                table_heads.Add("PRECIO UNITARIO");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";
                table_heads.Add("PRECIO TOTAL");
                ws.Column(table_heads.Count).Style.NumberFormat.Format = "$ #,##0.00";

                var pre_header_rows = new List<String[]>();
                pre_header_rows.Add(pre_table_heads.ToArray());

                var header_rows = new List<String[]>();
                header_rows.Add(table_heads.ToArray());

                var first_heads_row_range = ws.Cell(3, 9).InsertData(pre_header_rows);
                var heads_row_range = ws.Cell(4, 1).InsertData(header_rows);

                // Formato encabezados
                ws.Row(3).Style.Font.FontSize = 12;
                ws.Row(3).Height = 22;
                ws.Row(4).Style.Font.FontSize = 12;
                ws.Row(4).Height = 22;
                ws.SheetView.Freeze(4, 0);

                first_heads_row_range.Style.Font.Bold = true;
                first_heads_row_range.Style.Font.FontColor = XLColor.White;
                first_heads_row_range.Style.Fill.BackgroundColor = XLColor.AntiqueBrass;
                first_heads_row_range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("I3:J3").Merge();
                ws.Range("K3:M3").Merge();
                ws.Range("K3:M3").Style.Fill.BackgroundColor = XLColor.BlueGray;

                heads_row_range.Style.Font.Bold = true;
                heads_row_range.Style.Font.FontColor = XLColor.White;
                heads_row_range.Style.Fill.BackgroundColor = XLColor.Aurometalsaurus;

                // Agregar datos
                var solicitudes = Context.Solicitudes.Include("Line").Include("Line.Field").Where(x => x.ImprovementPlanId == plan.Id).OrderBy(x => x.Id).Select(x => new
                {
                    x.Id,
                    x.CUE,
                    FieldCode = x.LineId.HasValue ? x.Line.Field.Code : null,
                    LineCode = x.LineId.HasValue ? x.Line.Code : null,
                    x.FileNumber,
                    x.Status.Description,
                    x.Details,
                    reasigned = x.ReassignedId.HasValue ? SqlFunctions.StringConvert((decimal)x.ReassignedId) : "Orginal",
                    x.RequestedAmount,
                    x.RequestedPriceUnit,
                    x.ApprovedAmount,
                    x.ApprovedPriceUnit,
                    approvedTotal = 0
                });

                ws.Cell(5, 1).InsertData(solicitudes.AsEnumerable());

                for (int i = 0; i < solicitudes.Count(); i++)
                {
                    var offset = i + 5;
                    ws.Cell($"M{offset}").FormulaA1 = $"K{offset}*L{offset}";
                }


                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
            }
            return stream;

        }

        /// <summary>
        /// Exporta la lista de planes a un excel
        /// </summary>
        /// <returns>El stream en memoria del archivo de excel generado</returns>
        public Stream ExportSolicitudesToPDF(int improvementPlanId)
        {
            return ExportSolicitudes(improvementPlanId, ExportFormatType.PortableDocFormat);
        }

        /// <summary>
        /// Exporta la lista de planes a un excel
        /// </summary>
        /// <param name="improvementPlanId"></param>
        /// <param name="format">ExportFormatType.Excel || ExportFormatType.PortableDocFormat</param>
        /// <returns>El stream en memoria del archivo de excel generado</returns>
        private Stream ExportSolicitudes(int improvementPlanId, ExportFormatType format)
        {
            var solicitude = List(improvementPlanId).Select(x => new
            {
                CUE = x.CUE,
                Details = x.Details,
                SolicitudeId = x.Id,
                ImprovementPlanId = x.ImprovementPlanId,
                SchoolYear = x.SchoolYear.Description,
                Line = x.Line.Description,
                SolicitudeType = (x.SolicitudeType != null) ? x.SolicitudeType.Description : "",
                MeasurementUnit = (x.MeasurementUnit != null) ? x.MeasurementUnit.Description : "",
                ExpenditureType = (x.ExpenditureType != null) ? x.ExpenditureType.Description : "",
                StatusDescription = x.Status.Description,
                Specialization = x.Specialization,
                RequestedAmount = x.RequestedAmount,
                RequestedPriceUnit = x.RequestedPriceUnit.HasValue ? x.RequestedPriceUnit.Value : 0,
                Requested = Math.Round((x.RequestedAmount.HasValue ? x.RequestedAmount.Value : 0) * (x.RequestedPriceUnit.HasValue ? x.RequestedPriceUnit.Value : 0), 2),
                ApprovedAmount = x.ApprovedAmount.HasValue ? x.ApprovedAmount.Value : 0,
                ApprovedPriceUnit = x.ApprovedPriceUnit.HasValue ? x.ApprovedPriceUnit.Value : 0,
                Approved = (x.ApprovedTotal.HasValue) ? x.ApprovedTotal.Value : 0
            });

            var type = format;
            var reportUrl = Path.Combine(ConfigurationManager.AppSettings["ReportsPath"], "ListSolicitudes.rpt");

            var rpt = new ReportClass { FileName = reportUrl };
            rpt.Load();
            rpt.SetDataSource(solicitude);

            return rpt.ExportToStream(type);
        }

        /// <summary>
        /// Exporta los detalles de una solicitud
        /// </summary>
        /// <param name="improvementPlanId">Id de la solicitud de la cual se quiere generar el PDF</param>
        /// <returns>El stream en memoria del archivo del pdf generado</returns>
        public Stream ExportSolicitudeToPDF(int solicitudeId)
        {
            var solicitud = Context.Solicitudes.Where(x => x.Id == solicitudeId).ToList().Select(x => new
            {
                CUE = x.CUE,
                Details = x.Details,
                SolicitudeId = x.Id,
                ImprovementPlanId = x.ImprovementPlan.Identifier,
                SchoolYear = x.SchoolYear.Description,
                Line = x.Line.Description,
                SolicitudeType = (x.SolicitudeType != null) ? x.SolicitudeType.Description : "",
                MeasurementUnit = (x.MeasurementUnit != null) ? x.MeasurementUnit.Description : "",
                ExpenditureType = (x.ExpenditureType != null) ? x.ExpenditureType.Description : "",
                StatusDescription = x.Status.Description,
                Specialization = x.Specialization,
                RequestedAmountRep = x.RequestedAmount,
                RequestedPriceUnitRep = x.RequestedPriceUnit.HasValue ? x.RequestedPriceUnit.Value : 0,
                RequestedTotalRep = Math.Round(x.RequestedAmount.Value * (x.RequestedPriceUnit.HasValue ? x.RequestedPriceUnit.Value : 0), 2),
                ApprovedAmountRep = (x.ApprovedAmount.HasValue) ? x.ApprovedAmount.Value : 0,
                ApprovedPriceUnitRep = (x.ApprovedPriceUnit.HasValue) ? x.ApprovedPriceUnit.Value : 0,
                ApprovedTotalRep = (x.ApprovedTotal.HasValue) ? x.ApprovedTotal.Value : 0
            });

            var type = ExportFormatType.PortableDocFormat;
            var reportUrl = Path.Combine(ConfigurationManager.AppSettings["ReportsPath"], "Solicitude.rpt");

            var rpt = new ReportClass { FileName = reportUrl };
            rpt.Load();
            rpt.SetDataSource(solicitud);

            return rpt.ExportToStream(type);
        }

        /// <summary>
        /// Listado de las líneas habilitadas para planes de dependencia nacional
        /// </summary>
        /// <returns></returns>
        public List<int> GetValidLinesForNationalDependentPlans()
        {
            var _data = System.Configuration.ConfigurationManager.AppSettings["national_plan_field_line"];
            var _config = _data.Split(',').Select(x => x.Trim());
            return Context.Lines.Where(x => _config.Contains(x.Field.Code + "/" + x.Code)).Select(x => x.Id).ToList();
        }

        public void GenerateRelatedPlans(ImprovementPlan plan)
        {
            // generamos copias de los planes


            foreach (RelatedLines_22 r in plan.RelatedLines_22.Where(x=>x.Line22Id!=plan.Line_22_Id))
            {
                var np = new ImprovementPlan();
                np.ImprovementPlanTypeId = plan.ImprovementPlanTypeId;
                var Line22 = this.getLine22DTO(r.Line22Id.Value);
                var ol = Context.Lines_22.Where(x => x.Id == r.Line22Id.Value).FirstOrDefault();
                var sf = Context.SubFields.Where(x => x.Id == ol.SubFieldId).FirstOrDefault();

                string summary =sf.Code +"."+ ol.Code;
                np.Line_22_Id = r.Line22Id;
                np.SubFieldId = Line22.SubFieldId;
                np.FieldId = Line22.FieldId;
                np.LineId = Line22.LineId;
                np.ParentId = plan.Id;
                np.SchoolYearId = plan.SchoolYearId;
                if (r.CUE.Length<9)
                    np.CUE ="0"+ r.CUE;
                else
                    np.CUE = r.CUE;
                np.Summary = summary;
                np.ReceptionDate = plan.ReceptionDate;
                np.FieldDate = plan.FieldDate;
                np.Attachment = plan.Attachment;
                np.AttachmentAdd = plan.AttachmentAdd;
                np.Documentation = plan.Documentation;
                np.Identifier = String.Format("{0}-{1}-{2}-{3}", DateTime.Now.Year, np.FieldId, np.CUE, summary);
                np.StatusId = plan.StatusId;
                Context.ImprovementPlans.Add(np);

            }

            Context.SaveChanges(); // primera grabada, dsp actualizamos 

            foreach (RelatedLines_22 r in plan.RelatedLines_22)
            {
                var p = Context.ImprovementPlans.Where(x => x.ParentId == plan.Id && x.Line_22_Id == r.Line22Id).FirstOrDefault();

                if (p!=null)
                {
                    r.ImprovementPlanId_Link = p.Id;
                }
            }
            Context.SaveChanges();
        }
    }
}