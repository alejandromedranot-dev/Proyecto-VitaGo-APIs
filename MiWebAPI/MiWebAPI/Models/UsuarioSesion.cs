namespace MiWebAPI.Models
{
    public class UsuarioSesion
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Rol { get; set; }
        public Guid? PerfilId { get; set; }
    }
}