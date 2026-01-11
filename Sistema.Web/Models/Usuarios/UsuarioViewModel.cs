using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Usuarios
{
    public class UsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public int RolId { get; set; }
        public string RolNombre { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearUsuarioViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        public int RolId { get; set; }
    }

    public class ActualizarUsuarioViewModel
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        public int RolId { get; set; }
    }

    public class CambiarPasswordViewModel
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string PasswordActual { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string PasswordNueva { get; set; }
    }

    public class RolViewModel
    {
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
        public int TotalUsuarios { get; set; }
    }

    public class CrearRolViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(200)]
        public string Descripcion { get; set; }
    }

    public class ActualizarRolViewModel
    {
        [Required]
        public int RolId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(200)]
        public string Descripcion { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string PasswordNueva { get; set; }
    }
}
