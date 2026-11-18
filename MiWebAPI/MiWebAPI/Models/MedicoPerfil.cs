namespace MiWebAPI.Models
{
    public class MedicoPerfil
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Especialidad { get; set; }
        public string? Bio { get; set; }
        public int AniosExperiencia { get; set; }
        public int NumPacientes { get; set; }
        public string? Idiomas { get; set; }
        public decimal PrecioConsulta { get; set; }
        public bool ConsultaOnline { get; set; }
        public bool Verificado { get; set; }
        public decimal? Rating { get; set; }
        public int TotalResenas { get; set; }
        public string? Telefono { get; set; }
        public string? FotoUrl { get; set; }

        public List<string> Especialidades { get; set; } = new();
        public List<ConsultorioPublico> Consultorios { get; set; } = new();
        public List<HorarioDia> Horarios { get; set; } = new();
        public List<ResenaPublica> Resenas { get; set; } = new();
    }

    public class ConsultorioPublico
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Estado { get; set; }
        public string? Cp { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }

    public class HorarioDia
    {
        private static readonly string[] Dias =
            { "", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

        public byte DiaSemana { get; set; }                       // 1 = lunes ... 7 = domingo
        public string DiaNombre => DiaSemana is >= 1 and <= 7 ? Dias[DiaSemana] : "";
        public string HoraInicio { get; set; } = string.Empty;    // "09:00"
        public string HoraFin { get; set; } = string.Empty;       // "17:00"
    }

    public class ResenaPublica
    {
        public string? Autor { get; set; }       // "Luis P."
        public int Rating { get; set; }
        public string? Comentario { get; set; }
        public DateTime Fecha { get; set; }
    }
}