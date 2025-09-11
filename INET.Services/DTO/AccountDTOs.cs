using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace INET.Services.DTO
{
    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6)]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 6)]
        [DataType(DataType.Password)]        
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6)]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "Usuario:")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6)]
        [Display(Name = "Contraseña:")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required]        
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6)]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "La contraseña y la nueva contraseña no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
