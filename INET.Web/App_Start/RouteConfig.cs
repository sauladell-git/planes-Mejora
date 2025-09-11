using System.Web.Mvc;
using System.Web.Routing;

namespace INET.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Home", "bienvenido", new { controller = "Home", action = "Index" });
            routes.MapRoute("List", "planes-de-mejora", new { controller = "Plan", action = "List" });
            routes.MapRoute("Details", "detalle-del-plan", new { controller = "Plan", action = "Details" });
            routes.MapRoute("UpdateSolicitudes", "actualizar-solicitados", new { controller = "Plan", action = "UpdateSolicitudes" });
            routes.MapRoute("Editor", "plan-de-mejora", new { controller = "Plan", action = "Editor" });
            routes.MapRoute("Incidences", "incidencias", new { controller = "Incidence", action = "List" });
            routes.MapRoute("AdminIndex", "administracion", new { controller = "Admin", action = "Index" });
            routes.MapRoute("Users", "usuarios", new { controller = "Admin", action = "Users" });
            routes.MapRoute("Roles", "roles", new { controller = "Admin", action = "Roles" });
            routes.MapRoute("Budgets", "partida-presupuestaria", new { controller = "Admin", action = "Budgets" });
            routes.MapRoute("Budgets_216", "partida-presupuestaria-216", new { controller = "Admin", action = "Budgets216" });
            routes.MapRoute("ForgotPassword", "recuperar-contraseña", new { controller = "Account", action = "ForgotPassword" });
            routes.MapRoute("ChangePassword", "cambiar-contraseña", new { controller = "Account", action = "ChangePassword" });
            routes.MapRoute("ResetPassword", "resetear-contraseña", new { controller = "Account", action = "ResetPassword" });
            routes.MapRoute("ListTemplates", "plantillas-de-documentos", new { controller = "Admin", action = "TemplatesList" });
            routes.MapRoute("TemplateEditor", "editar-plantilla", new { controller = "Admin", action = "TemplateEditor" });
            routes.MapRoute("Dictums", "dictamenes", new { controller = "Dictum", action = "List" });
            routes.MapRoute("Resolutions", "resoluciones", new { controller = "Resolution", action = "List" });
            routes.MapRoute("Reports", "reportes", new { controller = "Report", action = "Index" });
           
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}