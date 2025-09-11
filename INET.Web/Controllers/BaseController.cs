using INET.Core.Enums;
using INET.Core.Models;
using Resources;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace INET.Web.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {

        /// <summary>
        /// Verifica si se ha pasado un mensaje desde alguna otra vista para pasarla a la vista actual
        /// </summary>
        public void CheckMessageResult()
        {
            if (TempData["MessageResult"] != null)
            {
                ViewBag.MessageResult = TempData["MessageResult"];
                TempData["MessageResult"] = null;
            }
        }

        /// <summary>
        /// Agrega un mensaje para retornar a la página
        /// </summary>
        /// <param name="text">Texto del mensaje</param>
        /// <param name="type">Tipo de Mensaje</param>
        public void AddMessageResult(string text, MessageResultTypeEnum type)
        {
            AddMessageResult(text, type, false);
        }

        /// <summary>
        /// Agrega un mensaje para retornar a la página
        /// </summary>
        /// <param name="text">Texto del mensaje</param>
        /// <param name="type">Tipo del mensaje</param>
        /// <param name="fadeOut">Determina si el mensaje de desvanecerá pasado unos segundos</param>
        public void AddMessageResult(string text, MessageResultTypeEnum type, bool fadeOut)
        {
            TempData["MessageResult"] = new MessageResult() { Text = text, Type = type, FadeOut = fadeOut };
        }

        /// <summary>
        /// Render view to string
        /// </summary>
        /// <param name="partialView"></param>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        public string RenderPartialToString(PartialViewResult partialView, ControllerContext controllerContext)
        {
            using (var sw = new StringWriter())
            {
                partialView.View = ViewEngines.Engines.FindPartialView(controllerContext, partialView.ViewName).View;
                var vc = new ViewContext(controllerContext, partialView.View, partialView.ViewData, partialView.TempData, sw);
                partialView.View.Render(vc, sw);
                var partialViewString = sw.GetStringBuilder().ToString();
                return partialViewString;
            }
        }

        protected ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Redirects to forbiden page if user has no access
        /// </summary>
        /// <param name="status"></param>
        protected void HasAccess(bool status)
        {
            if (!status)
            {
                throw new HttpException(401, INETResources.Forbiden);
            }
        }

    }
}
