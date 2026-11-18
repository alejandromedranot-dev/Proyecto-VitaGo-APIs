namespace MiWebAPI.Models
{
    public class MedicoResumen
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Especialidad { get; set; }
        public int? AniosExperiencia { get; set; }
        public decimal? Rating { get; set; }
        public int TotalResenas { get; set; }
        public decimal PrecioConsulta { get; set; }
        public bool ConsultaOnline { get; set; }
        public bool Verificado { get; set; }
        public string? FotoUrl { get; set; }
        public string? Consultorio { get; set; }
        public string? Ciudad { get; set; }
    }
}