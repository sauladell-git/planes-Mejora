using INET.Core.Constants;
using INET.Data;
using INET.Services;
using INET.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INET.Web.Controllers
{

    public class ReportController : BaseController
    {
        private readonly BudgetService _budgetService;
        private readonly UserProfileService _userProfileService;
        private readonly SolicitudeService _solicitudeService;

        public ReportController(BudgetService budgetService, UserProfileService userProfileService, SolicitudeService solicitudeService)
        {
            _budgetService = budgetService;
            _userProfileService = userProfileService;
            _solicitudeService = solicitudeService;
        }

        protected override void OnResultExecuting(ResultExecutingContext context)
        {
            CheckAndHandleFileResult(context);

            base.OnResultExecuting(context);
        }

        private const string FILE_DOWNLOAD_COOKIE_NAME = "fileDownload";

        /// <summary>
        /// If the current response is a FileResult (an MVC base class for files) then write a
        /// cookie to inform jquery.fileDownload that a successful file download has occured
        /// </summary>
        /// <param name="context"></param>
        private void CheckAndHandleFileResult(ResultExecutingContext context)
        {
            if (context.Result is FileResult)
                //jquery.fileDownload uses this cookie to determine that a file download has completed successfully
                Response.SetCookie(new HttpCookie(FILE_DOWNLOAD_COOKIE_NAME, "true") { Path = "/" });
            else
                //ensure that the cookie is removed in case someone did a file download without using jquery.fileDownload
                if (Request.Cookies[FILE_DOWNLOAD_COOKIE_NAME] != null)
                Response.Cookies[FILE_DOWNLOAD_COOKIE_NAME].Expires = DateTime.Now.AddYears(-1);
        }

        public ActionResult Index()
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] {
                PermissionConstants.Reports.BUDGET_BY_PROVINCE,
                PermissionConstants.Reports.IMPROVEMENT_PLANS,
                PermissionConstants.Reports.SOLICITUDES_BY_CUE,
                PermissionConstants.Reports.STATEMENT_OF_ACCOUNTS
            }));

            var user = SessionHelper.CurrentUserProfile;
            // Si el usuario tiene alguna provincia relacionada solo ve las relacionadas
            if (user.Provinces.Count > 0)
            {
                ViewBag.Provinces = user.Provinces;
            }
            else
            {
                ViewBag.Provinces = _budgetService.ListProvinces();
            }

            ViewBag.PlanTypes = _budgetService.ListImprovementPlansTypes();
            ViewBag.SchoolYears = _solicitudeService.ListSchoolYears();
            ViewBag.InstitutionLevels = _budgetService.ListLocalInstitutionLevels();

            return View();
        }

        #region REPORTS

        public ActionResult BudgetByProvince(int schoolYearId, int budgetYearId, int provinceId, int InstitutionLevelId, bool dependenceNational = false, int reportType=1, int lines=1)
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] {
                PermissionConstants.Reports.BUDGET_BY_PROVINCE
            }));

            var provs = SessionHelper.CurrentUserProfile.Provinces;
            if (provs.Count == 0) provs = _budgetService.ListProvinces();
            if (provinceId > 0)
            {
                provs = provs.Where(x => x.Id == provinceId).ToList();
            }
            System.IO.Stream stream;
            if (reportType == 1) // Vertical
            {
                if (lines == 1)
                    stream = _budgetService.GenerateBudgetByProvinceReport_V2(schoolYearId, budgetYearId, provs.ToList(), InstitutionLevelId, dependenceNational);
                else
                    stream = _budgetService.GenerateBudgetByProvinceReport_V2_22(schoolYearId, budgetYearId, provs.ToList(), InstitutionLevelId, dependenceNational);

            }
            else // horizontal, el "original"
            {
                if (lines == 1)
                    {
                    stream = _budgetService.GenerateBudgetByProvinceReport_V2(schoolYearId, budgetYearId, provs.ToList(), InstitutionLevelId, dependenceNational);

                    }
                    else
                     {
                    stream = _budgetService.GenerateBudgetByProvinceReport_V1(schoolYearId, budgetYearId, provs.ToList(), InstitutionLevelId, dependenceNational);

                    }
            }
            var schoolYear = _budgetService.GetShoolYear(schoolYearId);
            var budgetYear = budgetYearId > 0 ? _budgetService.GetShoolYear(budgetYearId).Cycle : "Todas";
            var provinceName = provs.Count > 1 ? "Todas" : provs.FirstOrDefault().Name;

            return File(stream, "application/vnd.ms-excel", "Global_Jurisdcciones_Eje_Linea_" + provinceName + "-Ciclo_" + schoolYear.Cycle + (dependenceNational ? "-Dep_Nacional" : "") + "-Partida_" + budgetYear + "-" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".xlsx");
        }

        public ActionResult BudgetByProvince216(int schoolYearId, int budgetYearId, int provinceId, int InstitutionLevelId, bool dependenceNational = false, int reportType = 1)
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] {
                PermissionConstants.Reports.BUDGET_BY_PROVINCE
            }));

            var provs = SessionHelper.CurrentUserProfile.Provinces;
            if (provs.Count == 0) provs = _budgetService.ListProvinces();
            if (provinceId > 0)
            {
                provs = provs.Where(x => x.Id == provinceId).ToList();
            }
            System.IO.Stream stream;
            if (reportType == 1) // Vertical
                stream = _budgetService.GenerateBudgetByProvinceReport_V2(schoolYearId, budgetYearId, provs.ToList(), InstitutionLevelId, dependenceNational);
            else // horizontal, el "original"
                stream = _budgetService.GenerateBudgetByProvinceReport_V1(schoolYearId, budgetYearId, provs.ToList(), InstitutionLevelId, dependenceNational);
            var schoolYear = _budgetService.GetShoolYear(schoolYearId);
            var budgetYear = budgetYearId > 0 ? _budgetService.GetShoolYear(budgetYearId).Cycle : "Todas";
            var provinceName = provs.Count > 1 ? "Todas" : provs.FirstOrDefault().Name;

            return File(stream, "application/vnd.ms-excel", "Global_Jurisdcciones_Eje_Linea_216_" + provinceName + "-Ciclo_" + schoolYear.Cycle + (dependenceNational ? "-Dep_Nacional" : "") + "-Partida_" + budgetYear + "-" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".xlsx");
        }

        public ActionResult StatementsOfAccounts(int schoolYearId, int budgetYearId)
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] {
                PermissionConstants.Reports.STATEMENT_OF_ACCOUNTS
            }));

            // Si el usuario tiene alguna provincia relacionada solo ve las relacionadas
            var provs = SessionHelper.CurrentUserProfile.Provinces;
            List<String> provNumbers = new List<String>();
            foreach (Province prov in provs)
            {
                provNumbers.Add(prov.Number);
            }
            var stream = _budgetService.GenerateStatementsOfAccountsReport(schoolYearId, budgetYearId, provNumbers);
            var schoolYear = _budgetService.GetShoolYear(schoolYearId);
            var budgetYear = budgetYearId > 0 ? _budgetService.GetShoolYear(budgetYearId).Cycle : "Todas";
            return File(stream, "application/vnd.ms-excel", "Global_Jurisdcciones-Ciclo_" + schoolYear.Cycle + "-Partida_" + budgetYear + "-" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".xlsx");
        }


        public ActionResult ImprovementPlansByBudget(int schoolYearId, int budgetYearId, string planTypeId, int provinceId, bool dependenceNational = false, string CUE = null, bool abreviated=false)
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] {
                PermissionConstants.Reports.IMPROVEMENT_PLANS
            }));

            var provs = SessionHelper.CurrentUserProfile.Provinces;
            if (provs.Count == 0) provs = _budgetService.ListProvinces();
            if (provinceId > 0)
            {
                provs = provs.Where(x => x.Id == provinceId).ToList();
            }
            System.IO.Stream stream=null;
            try { 
     
            if (abreviated)
                stream = _budgetService.GenerateImprovementPlansReport(schoolYearId, budgetYearId, planTypeId, provs.Select(x => x.Number).ToList(), dependenceNational, CUE);
            else
                 stream = _budgetService.GenerateImprovementPlansReport(schoolYearId, budgetYearId, planTypeId, provs.Select(x => x.Number).ToList(), dependenceNational, CUE, abreviated);
            } catch (Exception ex)
            {

            }
            var schoolYear = _budgetService.GetShoolYear(schoolYearId);
            var budgetYear = budgetYearId > 0 ? _budgetService.GetShoolYear(budgetYearId).Cycle : "Todas";
            return File(stream, "application/vnd.ms-excel", "PM_" + schoolYear.Cycle + (dependenceNational ? "-Dep_Nacional" : "") + "-Partida_" + budgetYear + "-" + String.Format("{0:yyyy_MM_dd}", DateTime.Now) + ".xlsx");
        }

   

        public ActionResult CUESolicitudesReport(string CUE, int schoolYearId, int budgetYearId)
        {
            this.HasAccess(SessionHelper.HasAnyPermission(new string[] {
                PermissionConstants.Reports.SOLICITUDES_BY_CUE
            }));
            
            if (CUE.Length == 0)
                CUE = "0";
                System.IO.Stream stream = null;
            try
            {
                var user = SessionHelper.CurrentUserProfile;
                List<Province> provs = new List<Province>();

                // Si el usuario tiene alguna provincia relacionada solo ve las relacionadas
                if (user.Provinces.Count > 0)
                {
                    provs = user.Provinces.ToList();
                }
                else
                {
                    provs = _budgetService.ListProvinces();
                }

                stream = _budgetService.GenerateCUESolicitudesReport(CUE, schoolYearId, budgetYearId, provs.Select(x => x.Number).ToList());

            }
            catch (Exception ex)
            {

            }

            var schoolYear = schoolYearId>0? _budgetService.GetShoolYear(schoolYearId).Cycle:"Todas";

            var budgetYear = budgetYearId > 0 ? _budgetService.GetShoolYear(budgetYearId).Cycle : "Todas";
            return File(stream, "application/vnd.ms-excel", "Solicitados_CUE-" + schoolYear + "-"+budgetYear+ ".xlsx");
        }

        public ActionResult AccountRenderingReport(int schoolYearId)
        {
            System.IO.Stream stream = null;
            stream = _budgetService.generateAccountRenderingReport(schoolYearId);
            return File(stream, "application/vnd.ms-excel", "PM_RENDICIONES.xlsx");

        }
        #endregion

    }
}
