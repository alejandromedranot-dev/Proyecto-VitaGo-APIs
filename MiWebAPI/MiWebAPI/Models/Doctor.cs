namespace MiWebAPI.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Especialidad { get; set; }
        public string? Correo { get; set; }
        public int Telefono { get; set; }
    }
}
