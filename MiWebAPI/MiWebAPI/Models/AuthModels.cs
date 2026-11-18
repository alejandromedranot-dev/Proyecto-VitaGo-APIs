using System.ComponentModel.DataAnnotations;

namespace MiWebAPI.Models
{
    public class LoginRequest
    {
        [Required, EmailAddress, StringLength(254)]
        public string Correo { get; set; } = string.Empty;

        [Required, StringLength(72)]
        public string Password { get; set; } = string.Empty;
    }

    // Lo que devuelve sp_loginUsuario (uso interno, nunca se envía al cliente)
    public class UsuarioLogin
    {
        public Guid UsuarioId { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public Guid? PerfilId { get; set; }
    }

    public class LoginRespuesta
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid? PerfilId { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string Correo { get; set; } = string.Empty;
    }
}