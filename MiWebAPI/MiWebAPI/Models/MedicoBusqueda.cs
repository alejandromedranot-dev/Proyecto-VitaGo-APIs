namespace MiWebAPI.Models
{
    public class MedicoBusqueda
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Especialidad { get; set; }   // LEFT JOIN: puede ser null
        public int AniosExperiencia { get; set; }
        public decimal? Rating { get; set; }        // null si aún no tiene reseñas
        public int TotalResenas { get; set; }
        public decimal PrecioConsulta { get; set; }
        public bool ConsultaOnline { get; set; }
        public bool Verificado { get; set; }
        public string? FotoUrl { get; set; }
        public string? Consultorio { get; set; }    // OUTER APPLY: puede ser null
        public string? Ciudad { get; set; }
    }
}