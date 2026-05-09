using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.UsersDto
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(5, ErrorMessage = "La clave debe ser mayor a 4 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,100}$", ErrorMessage =
            "La contraseña debe contener mayúsculas, minúsculas, números y símbolos.")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
