using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.DTOs.UsersDto
{
    public class AdminResetPasswordDto
    {
        [Required(ErrorMessage = "El Id del usuario es obligatorio")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,100}$", ErrorMessage =
            "La contraseña debe contener mayúsculas, minúsculas, números y símbolos.")]
        public string NewPassword { get; set; }
    }
}
