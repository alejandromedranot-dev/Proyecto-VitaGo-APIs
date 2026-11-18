namespace MiWebAPI.Models
{
    public class RegistroPacienteRequest
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Correo { get; set; }
        public string? Password { get; set; }
        // Agrega aquí los demás campos que pida tu procedimiento almacenado de registro
    }
}