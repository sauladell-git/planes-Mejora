using INET.Core.Enums;
using INET.Services;
using INET.Services.DTO;
using INET.Utils.Helpers;
using Resources;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace INET.Web.Controllers
{
    //[InitializeSimpleMembership]
    public class AccountController : BaseController
    {
        UserProfileService _userProfileService;
        public AccountController(UserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (SessionHelper.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && _userProfileService.IsActiveUser(model.UserName) && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form            
            AddMessageResult(INETResources.Login_InvalidLogin, MessageResultTypeEnum.Error, true);
            CheckMessageResult();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LocalPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                bool changePasswordSucceeded = _userProfileService.ChangePassword(SessionHelper.CurrentUserProfile.UserName, model.OldPassword, model.NewPassword);
                if (changePasswordSucceeded)
                {
                    AddMessageResult(INETResources.ChangePassword_Success, MessageResultTypeEnum.Confirmation, true);
                    return RedirectToAction("ChangePassword");
                }
                else
                {
                    AddMessageResult(INETResources.ChangePassword_Error, MessageResultTypeEnum.Error, true);
                }
            }

            CheckMessageResult();
            return View(model);
        }

        public ActionResult ChangePassword()
        {
            CheckMessageResult();
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            CheckMessageResult();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                var valid = _userProfileService.SendForgotPasswordEmail(userName);

                if (valid) AddMessageResult(INETResources.ForgotPassword_Sucess, MessageResultTypeEnum.Confirmation);
                else AddMessageResult(INETResources.ForgotPassword_Error, MessageResultTypeEnum.Error, true);

                return RedirectToAction("ForgotPassword");
            }

            AddMessageResult(INETResources.InvalidModel, MessageResultTypeEnum.Error, true);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string userName, string token)
        {
            CheckMessageResult();
            return View(new ResetPasswordModel() { UserName = userName, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var passwordChanged = _userProfileService.ResetPassword(model);
                if (passwordChanged)
                    return RedirectToAction("Index", "Home");
            }

            AddMessageResult(INETResources.ResetPassword_Error, MessageResultTypeEnum.Error, true);
            return View();
        }
    }
}
