using System.ComponentModel.DataAnnotations;

namespace MiWebAPI.Models
{
    public class PacienteRegistro
    {
        [Required, EmailAddress, StringLength(254)]
        public string Correo { get; set; } = string.Empty;

        [Required, StringLength(72, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Apellidos { get; set; } = string.Empty;
    }
}