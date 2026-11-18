namespace MiWebAPI.Models
{
    public class SlotDisponible
    {
        public Guid ConsultorioId { get; set; }
        public DateTime FechaHora { get; set; }          // 2026-11-17T09:30:00
        public string Hora { get; set; } = string.Empty; // "09:30"
        public int DuracionMin { get; set; }
    }
}