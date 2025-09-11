using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Core.Constants
{
    public class TemplateConstants
    {
        public const int ResolutionDefaultTemplate = 99;
    }

    public class ConfigConstants
    {
        public const string SIGNED_DICTUMS_PATH = "dictums-signed";
        public const string ANNEX_DICTUMS_PATH = "dictums-annex";
        public const string SIGNED_RESOLUTIONS_PATH = "resolutions-signed";
    }

    public class RoleConstants
    {
        public const string COORDINADOR_EJE = "Coordinador de Eje";
        public const string EVALUADOR = "Evaluador";
        public const string ADMIN = "Admin";
        public const string ADMINISTRATIVO = "Administrativo";
        public const string AUDITOR = "Auditor";
        public const string INGRESO_MONITOREO = "Ingreso y Monitoreo";
        public const string OPERADOR_PROVINCIAL = "Operador Provincial";
        public const string REFERENTE_JURISDICCIONAL = "Referente Jurisdiccional";
        public const string DELEGADO_PROVINCIAL_INET = "Delegado Provincial INET";
    }

    public class PermissionConstants
    {
        public class Reports
        {
            // Reportes
            public const string BUDGET_BY_PROVINCE       = "REPORT_BUDGET_BY_PROVINCE";
            public const string STATEMENT_OF_ACCOUNTS    = "REPORT_STATEMENT_OF_ACCOUNTS";
            public const string IMPROVEMENT_PLANS        = "REPORT_IMPROVEMENT_PLANS";
            public const string SOLICITUDES_BY_CUE       = "REPORT_SOLICITUDES_BY_CUE";
        }

        public class Administration
        {
            // Administracion
            public const string USERS            = "ADMINISTRATION_USERS";
            public const string ROLES            = "ADMINISTRATION_ROLES";
            public const string BUDGET           = "ADMINISTRATION_BUDGET";
            public const string TEMPLATES        = "ADMINISTRATION_TEMPLATES";
            public const string ERROR_LOG        = "ADMINISTRATION_ERROR_LOG";
        }

        public class ImprovementPlan
        {
            // Planes
            public const string READ = "IMPROVEMENT_PLAN_READ";
            public const string CREATE = "IMPROVEMENT_PLAN_CREATE";
            public const string EDIT = "IMPROVEMENT_PLANS_EDIT";
            public const string DELETE = "IMPROVEMENT_PLAN_DELETE";
            public const string INCIDENTS_READ = "IMPROVEMENT_PLAN_INCIDENTS_READ";
            public const string INCIDENTS_WRITE = "IMPROVEMENT_PLAN_INCIDENTS_WRITE";
            public const string COMMENTS_READ = "IMPROVEMENT_PLAN_COMMENTS_READ";
            public const string COMMENTS_WRITE = "IMPROVEMENT_PLAN_COMMENTS_WRITE";
            public const string BLOCK = "IMPROVEMENT_PLAN_BLOCK";
            public const string CHANGE_STATUS = "IMPROVEMENT_PLAN_CHANGE_STATUS";
            public const string ASSIGN_EVALUATOR = "IMPROVEMENT_PLAN_ASSIGN_EVALUATOR";
        }

        public class Solicitudes
        {
            // Solicitados
            public const string EDIT = "SOLICITUDES_EDIT";
            public const string APPROVE = "SOLICITUDES_APPROVE";
            public const string CREATE = "SOLICITUDES_CREATE";
            public const string DELETE = "SOLICITUDES_DELETE";
            public const string BLOCK = "SOLICITUDES_BLOCK";
            public const string COMMENTS_READ = "SOLICITUDES_COMMENTS_READ";
            public const string COMMENTS_WRITE = "SOLICITUDES_COMMENTS_WRITE";
            public const string INCIDENTS_READ = "SOLICITUDES_INCIDENTS_READ";
            public const string INCIDENTS_WRITE = "SOLICITUDES_INCIDENTS_WRITE";
            public const string FILE_NUMBER = "SOLICITUDES_FILE_NUMBER";
        }

        public class Dictums
        {
            // Dictámenes
            public const string CREATE = "DICTUMS_CREATE";
            public const string DELETE = "DICTUMS_DELETE";
            public const string EDIT = "DICTUMS_EDIT";
            public const string BLOCK = "DICTUMS_BLOCK";

            public const string EMIT = "DICTUMS_EMIT";
            public const string SIGN = "DICTUMS_SIGN";
        }

        public class Resolutions
        {
            // Disposiciones
            public const string CREATE = "RESOLUTIONS_CREATE";
            public const string DELETE = "RESOLUTIONS_DELETE";
            public const string EDIT = "RESOLUTIONS_EDIT";
            public const string BLOCK = "RESOLUTIONS_BLOCK";
            public const string EMIT = "RESOLUTIONS_EMIT";
            public const string SIGN = "RESOLUTIONS_SIGN";
            public const string SIGN_BACK = "RESOLUTIONS_BACK";
        }
    }
}
