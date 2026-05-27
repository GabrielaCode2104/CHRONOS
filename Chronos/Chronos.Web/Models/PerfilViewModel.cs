using System.ComponentModel.DataAnnotations;

namespace Chronos.Web.Models
{
    public class PerfilViewModel
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Carrera { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }

        public string PreguntaSecreta { get; set; } = string.Empty;

        public string RespuestaSecreta { get; set; } = string.Empty;
    }

    public class CambiarPasswordViewModel
    {
        [Required]
        public string PasswordActual { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string PasswordNueva { get; set; } = string.Empty;

        [Required, Compare("PasswordNueva", ErrorMessage = "Las contraseñas no coinciden")]
        public string PasswordConfirmar { get; set; } = string.Empty;
    }

    public class RecuperarPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PreguntaSecreta { get; set; } = string.Empty;

        [Required]
        public string RespuestaSecreta { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string PasswordNueva { get; set; } = string.Empty;

        [Required, Compare("PasswordNueva", ErrorMessage = "Las contraseñas no coinciden")]
        public string PasswordConfirmar { get; set; } = string.Empty;
    }
}