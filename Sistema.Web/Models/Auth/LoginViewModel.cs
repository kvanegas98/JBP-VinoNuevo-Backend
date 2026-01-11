using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }
    }

    public class LoginResponseViewModel
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public int RolId { get; set; }
        public string RolNombre { get; set; }
        public string Token { get; set; }
        public string Expiracion { get; set; }
    }
}
